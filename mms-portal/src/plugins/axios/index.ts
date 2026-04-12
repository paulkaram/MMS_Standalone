import axios, { type AxiosInstance, type AxiosError, type InternalAxiosRequestConfig, type AxiosResponse } from 'axios'
import { useUserStore } from '@/stores/user'

// Retry configuration
const RETRY_CONFIG = {
  maxRetries: 3,
  retryDelay: 1000, // Base delay in ms
  retryableStatuses: [408, 429, 500, 502, 503, 504], // Transient errors
  retryableMethods: ['get', 'head', 'options', 'put', 'delete'] // Idempotent methods
}

// Create axios instance with default config
const mainApiAxios: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_MAIN_API || '/api/',
  timeout: 60000, // 60 seconds - reasonable timeout
  headers: {
    'Accept': 'application/json; charset=utf-8',
    'Content-Type': 'application/json',
    'Cache-Control': 'no-cache',
    'Pragma': 'no-cache',
    'Expires': '-1'
  }
})

// Extend config type to include retry metadata
interface RetryConfig extends InternalAxiosRequestConfig {
  _retry?: boolean
  _retryCount?: number
  _startTime?: number
}

// Calculate delay with exponential backoff and jitter
const getRetryDelay = (retryCount: number): number => {
  const baseDelay = RETRY_CONFIG.retryDelay * Math.pow(2, retryCount)
  const jitter = baseDelay * 0.1 * Math.random() // Add 10% jitter
  return Math.min(baseDelay + jitter, 30000) // Max 30 seconds
}

// Check if request should be retried
const shouldRetry = (error: AxiosError, config: RetryConfig): boolean => {
  const retryCount = config._retryCount || 0
  if (retryCount >= RETRY_CONFIG.maxRetries) return false

  const method = config.method?.toLowerCase() || ''
  if (!RETRY_CONFIG.retryableMethods.includes(method)) return false

  // Network errors
  if (!error.response) return true

  // Retryable status codes
  return RETRY_CONFIG.retryableStatuses.includes(error.response.status)
}

// Sleep utility for retry delay
const sleep = (ms: number): Promise<void> => new Promise(resolve => setTimeout(resolve, ms))

// Track token refresh state to prevent race conditions
let isRefreshing = false
let failedRequestsQueue: Array<{
  resolve: (token: string) => void
  reject: (error: unknown) => void
}> = []

// Process queued requests after token refresh
const processQueue = (error: unknown, token: string | null = null) => {
  failedRequestsQueue.forEach(prom => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token!)
    }
  })
  failedRequestsQueue = []
}

// Auth endpoints that should not include the Authorization header
const authEndpoints = [
  'auth',
  'auth/login-options',
  'auth/verify',
  'auth/2fa',
  'auth/refresh-token'
]

// Check if the URL is an auth endpoint
const isAuthEndpoint = (url: string | undefined): boolean => {
  if (!url) return false
  const normalizedUrl = url.replace(/^\/+/, '').toLowerCase()
  return authEndpoints.some(endpoint =>
    normalizedUrl === endpoint || normalizedUrl.startsWith(endpoint + '?')
  )
}

// Request interceptor to add auth token and timing
mainApiAxios.interceptors.request.use(
  (config: RetryConfig) => {
    const userStore = useUserStore()

    // Add request timing for telemetry
    config._startTime = Date.now()

    // Only add Authorization header for non-auth endpoints
    if (!isAuthEndpoint(config.url)) {
      const token = userStore.token
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`
      }
    }

    // Add language headers (consistent with Case Portal / IAM Portal)
    const language = userStore.language
    if (config.headers) {
      config.headers['Accept-Language'] = language === 'ar' ? 'ar-SA,ar;q=1.0' : 'en-US,en;q=1.0'
      config.headers['X-Language'] = language
      config.headers['Content-Language'] = language
    }

    return config
  },
  (error: AxiosError) => {
    return Promise.reject(error)
  }
)

// Response interceptor for error handling and token refresh
mainApiAxios.interceptors.response.use(
  (response: AxiosResponse) => {
    // Return raw response for blob and arraybuffer types (file downloads/binary data)
    if (response.config.responseType === 'blob' || response.config.responseType === 'arraybuffer') {
      return response
    }
    // Extract data from response automatically
    return response.data
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }
    const userStore = useUserStore()

    // Handle 403 Forbidden (Access Denied) - do NOT refresh or logout
    if (error.response?.status === 403) {
      return Promise.reject(error)
    }

    // Handle 401 Unauthorized
    if (error.response?.status === 401 && !originalRequest._retry) {
      // Skip refresh token handling for auth endpoints
      if (isAuthEndpoint(originalRequest.url)) {
        return Promise.reject(error)
      }

      if (isRefreshing) {
        // Queue this request while refresh is in progress
        return new Promise((resolve, reject) => {
          failedRequestsQueue.push({ resolve, reject })
        }).then(token => {
          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${token}`
          }
          return mainApiAxios(originalRequest)
        }).catch(err => {
          return Promise.reject(err)
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      const refreshToken = userStore.refreshToken

      if (!refreshToken) {
        console.warn('No refresh token available, redirecting to login')
        userStore.removeUserSession()
        window.location.href = '/login'
        return Promise.reject(error)
      }

      try {
        // Try to refresh the token - send as { refreshToken } matching old system
        const response = await axios.post(
          `${import.meta.env.VITE_MAIN_API || '/api/'}auth/refresh-token`,
          { refreshToken },
          {
            headers: {
              'Content-Type': 'application/json'
            }
          }
        )

        // API returns { data: { token }, success, message } wrapper
        const newToken = response.data.data?.token || response.data.token || response.data.accessToken
        const newRefreshToken = response.data.data?.refreshToken || response.data.refreshToken

        if (newToken) {
          userStore.updateToken(newToken, newRefreshToken)

          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${newToken}`
          }

          processQueue(null, newToken)
          return mainApiAxios(originalRequest)
        } else {
          console.error('No token in refresh response:', response.data)
          throw new Error('No token in refresh response')
        }
      } catch (refreshError: any) {
        console.error('Token refresh failed:', refreshError?.response?.data || refreshError)
        processQueue(refreshError, null)
        userStore.removeUserSession()
        window.location.href = '/login'
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    // Retry logic for transient failures (not 401)
    if (error.response?.status !== 401) {
      const config = originalRequest as RetryConfig
      if (shouldRetry(error, config)) {
        config._retryCount = (config._retryCount || 0) + 1
        const delay = getRetryDelay(config._retryCount - 1)
        await sleep(delay)
        return mainApiAxios(config)
      }
    }

    return Promise.reject(error)
  }
)

// Export as both default and named for compatibility
export { mainApiAxios }
export default mainApiAxios

// Export a function to create a custom instance if needed
export function createApiInstance(baseURL: string): AxiosInstance {
  const instance = axios.create({
    baseURL,
    timeout: 30000,
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    }
  })

  // Add the same interceptors
  instance.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
      const userStore = useUserStore()
      const token = userStore.token

      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`
      }

      return config
    },
    (error: AxiosError) => {
      return Promise.reject(error)
    }
  )

  return instance
}

import rawAxios from 'axios'
import { mainApiAxios as axios } from '@/plugins/axios'
import { useUserStore } from '@/stores/user'

export interface LoginResponse {
  token: string
  refreshToken: string
  user: {
    id: string
    username: string
    fullName: string
    email: string
    language: string
    profilePictureUrl: string | null
    hasProfilePicture: boolean
    roles: string[]
    permissions: string[]
  }
  requiresTwoFA?: boolean
  twoFAMethod?: string
  // Extended response properties from API
  statusCode?: number
  userInfo?: {
    id?: string
    userId?: string
    method?: string
  }
  data?: {
    token?: string
    accessToken?: string
    refreshToken?: string
    RefreshToken?: string
    user?: any
    userInfo?: any
  }
}

export interface VerificationRequest {
  userId: string
  method: string
  code?: string
}

export interface LoginOptions {
  allowUsernamePassword: boolean
  allowSSO: boolean
  ssoUrl?: string
}

const AuthService = {
  /**
   * Login with username and password
   */
  login(username: string, password: string): Promise<LoginResponse> {
    return axios.post('auth', { username, password })
  },

  /**
   * Request 2FA verification code
   */
  requestVerificationCode(request: VerificationRequest): Promise<{ success: boolean; message?: string }> {
    return axios.post('auth/verify', request)
  },

  /**
   * Check/verify 2FA code
   */
  checkVerificationCode(request: VerificationRequest): Promise<LoginResponse> {
    return axios.post('auth/2fa', request)
  },

  /**
   * Get available login options
   */
  getLoginOptions(): Promise<LoginOptions> {
    return axios.get('auth/login-options')
  },

  /**
   * Refresh access token
   */
  refreshToken(refreshToken: string): Promise<{ token: string; refreshToken: string }> {
    return axios.post('auth/refresh-token', { refreshToken })
  },

  /**
   * Update user language preference (persists to MMS database).
   * Uses raw axios to bypass the 401-interceptor — prevents a race condition
   * where the interceptor clears the session during the reload window.
   */
  updateLanguage(language: string): Promise<any> {
    const userStore = useUserStore()
    const token = userStore.token
    const baseUrl = import.meta.env.VITE_MAIN_API || '/api/'
    return rawAxios.post(`${baseUrl}users/language`, JSON.stringify(language), {
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {})
      }
    }).catch(() => {})
  },

  /**
   * Logout user
   */
  logout(): Promise<void> {
    return axios.post('users/logout')
  }
}

export default AuthService

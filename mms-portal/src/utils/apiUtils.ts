/**
 * API Utilities for enterprise-grade request handling
 */

/**
 * Creates an AbortController with automatic cleanup
 * Use this for cancellable requests like search/autocomplete
 */
export function createAbortController(): {
  controller: AbortController
  signal: AbortSignal
  abort: () => void
} {
  const controller = new AbortController()
  return {
    controller,
    signal: controller.signal,
    abort: () => controller.abort()
  }
}

/**
 * Manages a single cancellable request - automatically cancels previous request
 * when a new one is made. Useful for search-as-you-type functionality.
 */
export class CancellableRequest {
  private controller: AbortController | null = null

  /**
   * Get a signal for the current request, cancelling any previous request
   */
  getSignal(): AbortSignal {
    // Cancel previous request if exists
    if (this.controller) {
      this.controller.abort()
    }
    this.controller = new AbortController()
    return this.controller.signal
  }

  /**
   * Cancel the current request
   */
  cancel(): void {
    if (this.controller) {
      this.controller.abort()
      this.controller = null
    }
  }

  /**
   * Check if request was aborted (for error handling)
   */
  static isAbortError(error: unknown): boolean {
    return error instanceof Error && error.name === 'AbortError'
  }
}

/**
 * Debounce utility for search inputs
 * @param fn - Function to debounce
 * @param delay - Delay in milliseconds
 */
export function debounce<T extends (...args: Parameters<T>) => ReturnType<T>>(
  fn: T,
  delay: number
): (...args: Parameters<T>) => void {
  let timeoutId: ReturnType<typeof setTimeout> | null = null

  return (...args: Parameters<T>) => {
    if (timeoutId) {
      clearTimeout(timeoutId)
    }
    timeoutId = setTimeout(() => {
      fn(...args)
    }, delay)
  }
}

/**
 * Throttle utility for rate limiting
 * @param fn - Function to throttle
 * @param limit - Minimum time between calls in milliseconds
 */
export function throttle<T extends (...args: Parameters<T>) => ReturnType<T>>(
  fn: T,
  limit: number
): (...args: Parameters<T>) => void {
  let inThrottle = false

  return (...args: Parameters<T>) => {
    if (!inThrottle) {
      fn(...args)
      inThrottle = true
      setTimeout(() => {
        inThrottle = false
      }, limit)
    }
  }
}

/**
 * Normalize API errors for consistent handling
 */
export interface NormalizedError {
  message: string
  status?: number
  code?: string
  details?: Record<string, unknown>
}

export function normalizeError(error: unknown): NormalizedError {
  if (error instanceof Error) {
    // Axios error
    const axiosError = error as { response?: { status?: number; data?: { message?: string } } }
    if (axiosError.response) {
      return {
        message: axiosError.response.data?.message || error.message,
        status: axiosError.response.status,
        code: `HTTP_${axiosError.response.status}`
      }
    }
    // Network error
    if (error.name === 'AbortError') {
      return {
        message: 'Request was cancelled',
        code: 'ABORT_ERROR'
      }
    }
    return {
      message: error.message,
      code: 'UNKNOWN_ERROR'
    }
  }
  return {
    message: 'An unknown error occurred',
    code: 'UNKNOWN_ERROR'
  }
}

import { defineStore } from 'pinia'

// Cookie name must match what the PDF viewer expects
const SESSION_COOKIE_NAME = 'mms-session'

// Helper to set cookie (for PDF viewer compatibility)
function setSessionCookie(token: string | null) {
  if (token) {
    const cookieValue = JSON.stringify({ user: { token } })
    // Set cookie with path=/ so it's accessible everywhere
    document.cookie = `${SESSION_COOKIE_NAME}=${encodeURIComponent(cookieValue)}; path=/; SameSite=Lax`
  } else {
    // Remove cookie
    document.cookie = `${SESSION_COOKIE_NAME}=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT`
  }
}

export interface User {
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

export interface TwoFAData {
  userId: string
  method: string
  verified: boolean
}

export interface UserState {
  user: User | null
  token: string | null
  refreshToken: string | null
  TwoFA: TwoFAData | null
  defaultLanguage: string
}

// Migrate old Pinia persist blob to unified keys
;(() => {
  try {
    const old = localStorage.getItem('mms-user-session')
    if (old) {
      const parsed = JSON.parse(old)
      // The IAM token is now the primary token (from Phase 1 backend change)
      const token = parsed.token
      if (token && !localStorage.getItem('pickOne_token')) {
        localStorage.setItem('pickOne_token', token)
      }
      if (parsed.refreshToken && !localStorage.getItem('pickOne_refresh_token')) {
        localStorage.setItem('pickOne_refresh_token', parsed.refreshToken)
      }
      if (parsed.user && !localStorage.getItem('pickOne_user')) {
        localStorage.setItem('pickOne_user', JSON.stringify(parsed.user))
      }
    }
  } catch { /* ignore migration errors */ }
})()

export const useUserStore = defineStore('user', {
  state: (): UserState => ({
    user: null,
    token: null,
    refreshToken: null,
    TwoFA: null,
    defaultLanguage: import.meta.env.VITE_DEFAULT_LANGUAGE || 'ar'
  }),

  getters: {
    isAuthenticated: (state): boolean => {
      return Boolean(state.token && state.user)
    },

    loggedInUser: (state): User | null => {
      return state.user
    },

    language: (state): string => {
      return state.defaultLanguage
    },

    isRtl: (state): boolean => {
      return state.defaultLanguage === 'ar'
    },

    hasPermission: (state) => (permission: string): boolean => {
      return state.user?.permissions?.includes(permission) ?? false
    },

    hasRole: (state) => (role: string): boolean => {
      return state.user?.roles?.includes(role) ?? false
    },

    profilePicture: (state): string | null => {
      return state.user?.profilePictureUrl ?? null
    },

    twoFAUserId: (state): string => {
      return state.TwoFA?.userId ?? ''
    },

    twoFAMethod: (state): string => {
      return state.TwoFA?.method ?? 'sms'
    }
  },

  actions: {
    setUserSession(payload: { user: User; token: string; refreshToken?: string; iamToken?: string }) {
      const { user, token, refreshToken } = payload
      if (user && token) {
        this.token = token
        if (refreshToken) {
          this.refreshToken = refreshToken
        }
        this.user = user
        // Sync to unified localStorage keys
        localStorage.setItem('pickOne_token', token)
        if (refreshToken) localStorage.setItem('pickOne_refresh_token', refreshToken)
        localStorage.setItem('pickOne_user', JSON.stringify(user))
        // Sync to cookie for PDF viewer compatibility
        setSessionCookie(token)
      }
    },

    setUser2FA(data: TwoFAData) {
      this.TwoFA = data
    },

    removeUser2FA() {
      this.TwoFA = null
    },

    setLanguage(language: string) {
      this.defaultLanguage = language === 'ar' ? 'ar' : 'en'
    },

    setProfilePictureUrl(url: string) {
      if (this.user) {
        // Don't add timestamp if URL already has one
        if (url.includes('?t=') || url.includes('&t=')) {
          this.user.profilePictureUrl = url
        } else {
          const timestamp = Date.now()
          this.user.profilePictureUrl = `${url}?t=${timestamp}`
        }
      }
    },

    setHasProfilePicture(hasProfile: boolean) {
      if (this.user) {
        this.user.hasProfilePicture = hasProfile
      }
    },

    updateToken(token: string, refreshToken?: string) {
      this.token = token
      if (refreshToken) {
        this.refreshToken = refreshToken
      }
      // Sync to cookie for PDF viewer compatibility
      setSessionCookie(token)
    },

    updateRefreshToken(refreshToken: string) {
      this.refreshToken = refreshToken
    },

    removeUserSession() {
      this.token = null
      this.refreshToken = null
      this.user = null
      this.TwoFA = null
      // Clear unified keys
      localStorage.removeItem('pickOne_token')
      localStorage.removeItem('pickOne_refresh_token')
      localStorage.removeItem('pickOne_user')
      localStorage.removeItem('mms-user-session')
      // Remove cookie
      setSessionCookie(null)
    },

    setToken(token: string) {
      this.token = token
      // Sync to cookie for PDF viewer compatibility
      setSessionCookie(token)
    },

    clearTwoFAState() {
      this.TwoFA = null
    },

    // Initialize cookie from stored token (for page refreshes)
    initSessionCookie() {
      if (this.token) {
        setSessionCookie(this.token)
      }
    },

    $reset() {
      this.user = null
      this.token = null
      this.refreshToken = null
      this.TwoFA = null
      this.defaultLanguage = import.meta.env.VITE_DEFAULT_LANGUAGE || 'ar'
      localStorage.removeItem('pickOne_token')
      localStorage.removeItem('pickOne_refresh_token')
      localStorage.removeItem('pickOne_user')
      localStorage.removeItem('mms-user-session')
    }
  },

  persist: {
    key: 'mms-user-session',
    storage: localStorage,
    paths: ['user', 'token', 'refreshToken', 'defaultLanguage']
  }
})

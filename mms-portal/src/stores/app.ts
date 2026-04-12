import { defineStore } from 'pinia'

export interface ThemeOptions {
  primaryColor: string
  secondaryColor: string
  navigationColor: string
  logoUrl?: string
}

export interface AppState {
  translationFailed: boolean
  translationLoading: boolean
  translationsLoaded: boolean
  themeOptions: ThemeOptions | null
  sidebarCollapsed: boolean
  sidebarMobile: boolean
}

export const useAppStore = defineStore('app', {
  state: (): AppState => ({
    translationFailed: false,
    translationLoading: false,
    translationsLoaded: false,
    themeOptions: null,
    sidebarCollapsed: false,
    sidebarMobile: false
  }),

  getters: {
    isTranslationFailed: (state): boolean => {
      return state.translationFailed
    },

    isTranslationLoading: (state): boolean => {
      return state.translationLoading
    },

    isReady: (state): boolean => {
      return state.translationsLoaded && !state.translationFailed
    },

    theme: (state): ThemeOptions | null => {
      return state.themeOptions
    },

    primaryColor: (state): string => {
      return state.themeOptions?.primaryColor || '#520773'
    },

    secondaryColor: (state): string => {
      return state.themeOptions?.secondaryColor || '#C05A7C'
    },

    navigationColor: (state): string => {
      return state.themeOptions?.navigationColor || '#F2F2F2'
    },

    isSidebarCollapsed: (state): boolean => {
      return state.sidebarCollapsed
    },

    isSidebarMobileOpen: (state): boolean => {
      return state.sidebarMobile
    }
  },

  actions: {
    setTranslationFailed(status: boolean) {
      this.translationFailed = status
    },

    setTranslationLoading(status: boolean) {
      this.translationLoading = status
    },

    setTranslationsLoaded(status: boolean) {
      this.translationsLoaded = status
    },

    setThemeOptions(options: ThemeOptions) {
      this.themeOptions = options
      // Apply theme to CSS variables
      this.applyTheme(options)
    },

    applyTheme(options: ThemeOptions) {
      const root = document.documentElement
      if (options.primaryColor) {
        root.style.setProperty('--color-primary', options.primaryColor)
      }
      if (options.secondaryColor) {
        root.style.setProperty('--color-secondary', options.secondaryColor)
      }
      if (options.navigationColor) {
        root.style.setProperty('--color-navigation', options.navigationColor)
      }
    },

    toggleSidebar() {
      this.sidebarCollapsed = !this.sidebarCollapsed
    },

    setSidebarCollapsed(collapsed: boolean) {
      this.sidebarCollapsed = collapsed
    },

    toggleMobileSidebar() {
      this.sidebarMobile = !this.sidebarMobile
    },

    setMobileSidebarOpen(open: boolean) {
      this.sidebarMobile = open
    },

    closeMobileSidebar() {
      this.sidebarMobile = false
    },

    async fetchThemeOptions() {
      try {
        // This will be replaced with actual API call
        // const data = await SettingsService.listThemeColors()
        // this.setThemeOptions(data)
      } catch (error) {
        console.error('Failed to fetch theme options:', error)
      }
    },

    $reset() {
      this.translationFailed = false
      this.translationLoading = false
      this.translationsLoaded = false
      this.themeOptions = null
      this.sidebarCollapsed = false
      this.sidebarMobile = false
    }
  },

  persist: {
    key: 'mms-app-state',
    storage: localStorage,
    paths: ['sidebarCollapsed', 'themeOptions']
  }
})

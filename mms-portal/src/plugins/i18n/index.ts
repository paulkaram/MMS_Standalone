import { createI18n } from 'vue-i18n'
import { useAppStore } from '@/stores/app'
import { useUserStore } from '@/stores/user'

// Type for translation messages
type MessageSchema = Record<string, string>

// Default Arabic translations for development
const defaultArTranslations: MessageSchema = {
  ApplicationName: 'نظام إدارة الاجتماعات',
  Login: 'تسجيل الدخول',
  Logout: 'تسجيل الخروج',
  Username: 'اسم المستخدم',
  Password: 'كلمة المرور',
  EnterUsername: 'أدخل اسم المستخدم',
  EnterPassword: 'أدخل كلمة المرور',
  RememberMe: 'تذكرني',
  ForgotPassword: 'نسيت كلمة المرور؟',
  LoginWithSSO: 'الدخول بالهوية الموحدة',
  Or: 'أو',
  Welcome: 'مرحباً',
  LoggingIn: 'جاري الدخول...',
  SignIn: 'دخول',
  EnterCredentials: 'أدخل بياناتك للوصول إلى النظام',
  OrganizationNameAr: 'القوات الخاصة للأمن البيئي',
  OrganizationNameEn: 'Special Forces for Environmental Security',
  SystemNameAr: 'نظام إدارة الاجتماعات',
  SystemNameEn: 'Meeting Management System',
  UsernameRequired: 'اسم المستخدم مطلوب',
  PasswordRequired: 'كلمة المرور مطلوبة',
  AccountLocked: 'الحساب مقفل. يرجى التواصل مع المسؤول.',
  ServerError: 'خطأ في استجابة الخادم',
  InvalidCredentials: 'اسم المستخدم أو كلمة المرور غير صحيحة',
  CopyrightSFES: 'SFES - وزارة الداخلية',
  Home: 'الرئيسية',
  Dashboard: 'لوحة المعلومات',
  Meetings: 'الاجتماعات',
  MyMeetings: 'اجتماعاتي',
  AddMeeting: 'إضافة اجتماع',
  DraftMeetings: 'مسودات الاجتماعات',
  Tasks: 'المهام',
  Recommendations: 'التوصيات',
  Chat: 'المحادثات',
  Reports: 'التقارير',
  Administration: 'الإدارة',
  Settings: 'الإعدادات',
  Profile: 'الملف الشخصي',
  CouncilsAndCommittees: 'المجالس واللجان',
  Roles: 'الأدوار',
  Dictionary: 'القاموس',
  SystemSettings: 'إعدادات النظام',
  AuditTrail: 'سجل التدقيق',
  PageNotFound: 'الصفحة غير موجودة',
  GoHome: 'الرئيسية',
  GoBack: 'رجوع',
  Save: 'حفظ',
  Cancel: 'إلغاء',
  Delete: 'حذف',
  Edit: 'تعديل',
  Add: 'إضافة',
  Search: 'بحث',
  Loading: 'جاري التحميل...',
  NoData: 'لا توجد بيانات',
  Confirm: 'تأكيد',
  Yes: 'نعم',
  No: 'لا',
  JustNow: 'الآن',
  MinutesShort: 'د',
  HoursShort: 'س',
  DaysShort: 'ي'
}

// Default English translations
const defaultEnTranslations: MessageSchema = {
  ApplicationName: 'Meeting Management System',
  Login: 'Login',
  Logout: 'Logout',
  Username: 'Username',
  Password: 'Password',
  EnterUsername: 'Enter username',
  EnterPassword: 'Enter password',
  RememberMe: 'Remember me',
  ForgotPassword: 'Forgot password?',
  LoginWithSSO: 'Login with SSO',
  Or: 'Or',
  Welcome: 'Welcome',
  LoggingIn: 'Logging in...',
  SignIn: 'Sign In',
  EnterCredentials: 'Enter your credentials to access the system',
  OrganizationNameAr: 'القوات الخاصة للأمن البيئي',
  OrganizationNameEn: 'Special Forces for Environmental Security',
  SystemNameAr: 'نظام إدارة الاجتماعات',
  SystemNameEn: 'Meeting Management System',
  UsernameRequired: 'Username is required',
  PasswordRequired: 'Password is required',
  AccountLocked: 'Account is locked. Please contact the administrator.',
  ServerError: 'Server response error',
  InvalidCredentials: 'Invalid username or password',
  CopyrightSFES: 'SFES - Ministry of Interior',
  Home: 'Home',
  Dashboard: 'Dashboard',
  Meetings: 'Meetings',
  MyMeetings: 'My Meetings',
  AddMeeting: 'Add Meeting',
  DraftMeetings: 'Draft Meetings',
  Tasks: 'Tasks',
  Recommendations: 'Recommendations',
  Chat: 'Chat',
  Reports: 'Reports',
  Administration: 'Administration',
  Settings: 'Settings',
  Profile: 'Profile',
  CouncilsAndCommittees: 'Councils & Committees',
  Roles: 'Roles',
  Dictionary: 'Dictionary',
  SystemSettings: 'System Settings',
  AuditTrail: 'Audit Trail',
  PageNotFound: 'Page Not Found',
  GoHome: 'Home',
  GoBack: 'Go Back',
  Save: 'Save',
  Cancel: 'Cancel',
  Delete: 'Delete',
  Edit: 'Edit',
  Add: 'Add',
  Search: 'Search',
  Loading: 'Loading...',
  NoData: 'No data',
  Confirm: 'Confirm',
  Yes: 'Yes',
  No: 'No',
  JustNow: 'Just now',
  MinutesShort: 'min',
  HoursShort: 'hr',
  DaysShort: 'd'
}

// Create i18n instance with composition API mode
const i18n = createI18n<[MessageSchema], 'ar' | 'en'>({
  legacy: false, // Use Composition API mode
  globalInjection: true, // Enable $t() in templates
  locale: 'ar', // Default locale
  fallbackLocale: 'ar',
  messages: {
    ar: defaultArTranslations,
    en: defaultEnTranslations
  },
  silentTranslationWarn: true,
  silentFallbackWarn: true,
  missingWarn: false,
  fallbackWarn: false
})

// Track if translations are loaded from API
let apiTranslationsLoaded = false
let loadingPromise: Promise<void> | null = null

/**
 * Sync i18n locale from the user store's language preference.
 * Called after translations load and on every navigation.
 */
function syncLocaleFromStore(): void {
  try {
    const userStore = useUserStore()
    const lang = userStore.language as 'ar' | 'en'
    setLocale(lang)

    // Update document title in the current language
    const appName = i18n.global.t('ApplicationName')
    if (appName && appName !== 'ApplicationName') {
      document.title = appName
    }
  } catch {
    // Store not ready yet (pre-login), keep default
  }
}

/**
 * Load translations from the API
 * Waits for API translations to load before marking as ready
 */
export async function loadTranslation(): Promise<void> {
  // Always sync locale with store (handles language switch + reload)
  syncLocaleFromStore()

  // If API translations already loaded, just ensure locale is correct
  if (apiTranslationsLoaded) {
    syncLocaleFromStore()
    return
  }

  // If already loading, wait for existing promise
  if (loadingPromise) {
    await loadingPromise
    syncLocaleFromStore()
    return
  }

  // Start loading - MUST wait for API translations before allowing navigation
  loadingPromise = loadFromApi()
  await loadingPromise
  syncLocaleFromStore()
}

/**
 * Load translations from API (same as old system)
 */
async function loadFromApi(): Promise<void> {
  try {
    // Set loading state
    const appStore = useAppStore()
    appStore.setTranslationLoading(true)

    // Import TranslationsService dynamically to avoid circular dependencies
    const { default: TranslationsService } = await import('@/services/TranslationsService')

    const response: any = await TranslationsService.listTranslations()

    // Resolve translation data from response — handle multiple wrapper levels
    // After axios interceptor: response = ApiResponseDto { success, data: { ar, en }, message }
    let ar: Record<string, string> | null = null
    let en: Record<string, string> | null = null

    if (response?.data?.ar) {
      // Standard: response.data = { ar, en }
      ar = response.data.ar
      en = response.data.en
    } else if (response?.ar) {
      // Already unwrapped: response = { ar, en }
      ar = response.ar
      en = response.en
    }

    if (ar && en) {
      i18n.global.setLocaleMessage('ar', { ...defaultArTranslations, ...ar })
      i18n.global.setLocaleMessage('en', { ...defaultEnTranslations, ...en })
    }

    // Sync i18n locale with user's stored language preference
    syncLocaleFromStore()

    // Mark as loaded successfully
    apiTranslationsLoaded = true
    appStore.setTranslationsLoaded(true)
  } catch (error) {
    // API unavailable - use default translations
    console.warn('API translations unavailable, using defaults:', error)
    // Still mark as loaded with defaults so app can proceed
    apiTranslationsLoaded = true
    try {
      const appStore = useAppStore()
      appStore.setTranslationsLoaded(true)
    } catch (e) {
      // Store not ready, ignore
    }
  } finally {
    try {
      const appStore = useAppStore()
      appStore.setTranslationLoading(false)
    } catch (e) {
      // Store not ready, ignore
    }
    loadingPromise = null
  }
}

/**
 * Clear all translations (reset to defaults)
 */
export function clearTranslation(): void {
  i18n.global.setLocaleMessage('ar', defaultArTranslations)
  i18n.global.setLocaleMessage('en', defaultEnTranslations)
  apiTranslationsLoaded = false
  loadingPromise = null

  const appStore = useAppStore()
  appStore.setTranslationsLoaded(false)
}

/**
 * Check if translations are loaded from API
 */
export function isReady(): boolean {
  try {
    const appStore = useAppStore()
    return appStore.translationsLoaded
  } catch {
    return apiTranslationsLoaded
  }
}

/**
 * Set the current locale
 */
export function setLocale(locale: 'ar' | 'en'): void {
  (i18n.global.locale as any).value = locale
  document.documentElement.setAttribute('lang', locale)
  document.documentElement.setAttribute('dir', locale === 'ar' ? 'rtl' : 'ltr')
}

/**
 * Get the current locale
 */
export function getLocale(): string {
  return (i18n.global.locale as any).value
}

/**
 * Translate a key with optional parameters
 */
export function t(key: string, params?: Record<string, unknown>): string {
  return i18n.global.t(key, params ?? {})
}

export default i18n

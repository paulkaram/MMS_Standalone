import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'

import App from './App.vue'
import router from './router'
import i18n from './plugins/i18n'
import { useUserStore } from './stores/user'

// PrimeVue
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'

// Toast notifications
import Toast, { type PluginOptions } from 'vue-toastification'
import 'vue-toastification/dist/index.css'

// Tajawal font (bundled locally — no CDN)
import '@fontsource/tajawal/200.css'
import '@fontsource/tajawal/300.css'
import '@fontsource/tajawal/400.css'
import '@fontsource/tajawal/500.css'
import '@fontsource/tajawal/700.css'
import '@fontsource/tajawal/800.css'
import '@fontsource/tajawal/900.css'

// Material Symbols icons
import 'material-symbols'

// Global styles
import './assets/css/main.css'

// Check if coming from a language transition (smooth fade-in)
const isLanguageTransition = localStorage.getItem('language_transition') === 'true'
if (isLanguageTransition) {
  localStorage.removeItem('language_transition')
}

// Create app instance
const app = createApp(App)

// Pinia with persistence
const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)
app.use(pinia)

// Initialize session cookie from stored token (for PDF viewer compatibility)
const userStore = useUserStore()
userStore.initSessionCookie()

// Router
app.use(router)

// i18n
app.use(i18n)

// PrimeVue with Aura theme
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      prefix: 'p',
      darkModeSelector: '.dark',
      cssLayer: {
        name: 'primevue',
        order: 'tailwind-base, primevue, tailwind-utilities'
      }
    }
  },
  ripple: true,
  locale: {
    // Arabic locale overrides
    startsWith: 'يبدأ بـ',
    contains: 'يحتوي على',
    notContains: 'لا يحتوي على',
    endsWith: 'ينتهي بـ',
    equals: 'يساوي',
    notEquals: 'لا يساوي',
    noFilter: 'بدون فلتر',
    lt: 'أقل من',
    lte: 'أقل من أو يساوي',
    gt: 'أكبر من',
    gte: 'أكبر من أو يساوي',
    dateIs: 'التاريخ هو',
    dateIsNot: 'التاريخ ليس',
    dateBefore: 'قبل التاريخ',
    dateAfter: 'بعد التاريخ',
    clear: 'مسح',
    apply: 'تطبيق',
    matchAll: 'مطابقة الكل',
    matchAny: 'مطابقة أي',
    addRule: 'إضافة قاعدة',
    removeRule: 'إزالة قاعدة',
    accept: 'نعم',
    reject: 'لا',
    choose: 'اختر',
    upload: 'رفع',
    cancel: 'إلغاء',
    dayNames: ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'],
    dayNamesShort: ['أحد', 'إثنين', 'ثلاثاء', 'أربعاء', 'خميس', 'جمعة', 'سبت'],
    dayNamesMin: ['ح', 'ن', 'ث', 'ر', 'خ', 'ج', 'س'],
    monthNames: ['يناير', 'فبراير', 'مارس', 'إبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'],
    monthNamesShort: ['ينا', 'فبر', 'مار', 'إبر', 'ماي', 'يون', 'يول', 'أغس', 'سبت', 'أكت', 'نوف', 'ديس'],
    today: 'اليوم',
    weekHeader: 'أسبوع',
    firstDayOfWeek: 0,
    dateFormat: 'dd/mm/yy',
    weak: 'ضعيف',
    medium: 'متوسط',
    strong: 'قوي',
    passwordPrompt: 'أدخل كلمة المرور',
    emptyFilterMessage: 'لا توجد نتائج',
    emptyMessage: 'لا توجد خيارات متاحة',
    aria: {
      trueLabel: 'نعم',
      falseLabel: 'لا',
      nullLabel: 'غير محدد',
      star: 'نجمة واحدة',
      stars: '{star} نجوم',
      selectAll: 'تحديد الكل',
      unselectAll: 'إلغاء تحديد الكل',
      close: 'إغلاق',
      previous: 'السابق',
      next: 'التالي',
      navigation: 'التنقل',
      scrollTop: 'الانتقال للأعلى',
      moveTop: 'نقل للأعلى',
      moveUp: 'تحريك للأعلى',
      moveDown: 'تحريك للأسفل',
      moveBottom: 'نقل للأسفل',
      moveToTarget: 'نقل للهدف',
      moveToSource: 'نقل للمصدر',
      moveAllToTarget: 'نقل الكل للهدف',
      moveAllToSource: 'نقل الكل للمصدر',
      pageLabel: 'صفحة {page}',
      firstPageLabel: 'الصفحة الأولى',
      lastPageLabel: 'الصفحة الأخيرة',
      nextPageLabel: 'الصفحة التالية',
      previousPageLabel: 'الصفحة السابقة',
      rowsPerPageLabel: 'صفوف لكل صفحة',
      jumpToPageDropdownLabel: 'القفز إلى قائمة الصفحات',
      jumpToPageInputLabel: 'القفز إلى إدخال الصفحة',
      selectRow: 'تحديد صف',
      unselectRow: 'إلغاء تحديد صف',
      expandRow: 'توسيع الصف',
      collapseRow: 'طي الصف',
      showFilterMenu: 'إظهار قائمة الفلتر',
      hideFilterMenu: 'إخفاء قائمة الفلتر',
      filterOperator: 'عامل الفلتر',
      filterConstraint: 'قيد الفلتر',
      editRow: 'تعديل الصف',
      saveEdit: 'حفظ التعديل',
      cancelEdit: 'إلغاء التعديل',
      listView: 'عرض القائمة',
      gridView: 'عرض الشبكة',
      slide: 'شريحة',
      slideNumber: '{slideNumber}',
      zoomImage: 'تكبير الصورة',
      zoomIn: 'تكبير',
      zoomOut: 'تصغير',
      rotateRight: 'تدوير لليمين',
      rotateLeft: 'تدوير لليسار'
    }
  }
})

// Toast configuration
const toastOptions: PluginOptions = {
  position: 'bottom-center' as const,
  timeout: 3000,
  closeOnClick: true,
  pauseOnFocusLoss: true,
  pauseOnHover: true,
  draggable: true,
  draggablePercent: 0.6,
  showCloseButtonOnHover: true,
  hideProgressBar: true,
  closeButton: false,
  icon: true,
  rtl: false,
  maxToasts: 3,
  newestOnTop: true,
  transition: 'Vue-Toastification__fade'
}
app.use(Toast, toastOptions)

// Mount app
app.mount('#app')

// Remove initial loader after mount
const loader = document.getElementById('initial-loader')
if (loader) {
  loader.style.transition = 'opacity 0.4s ease'
  loader.style.opacity = '0'
  setTimeout(() => loader.remove(), 400)
}

// Smooth fade-in after mount (if coming from language transition)
if (isLanguageTransition) {
  requestAnimationFrame(() => {
    document.body.style.transition = 'opacity 0.25s ease-in'
    document.body.style.setProperty('opacity', '1', 'important')
  })
}

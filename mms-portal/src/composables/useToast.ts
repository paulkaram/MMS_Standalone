import { useToast as useVueToastification } from 'vue-toastification'
import { useUserStore } from '@/stores/user'
import { useI18n } from 'vue-i18n'

interface ToastOptions {
  position?: string
  timeout?: number
  closeOnClick?: boolean
  pauseOnHover?: boolean
  draggable?: boolean
  showCloseButtonOnHover?: boolean
  hideProgressBar?: boolean
  closeButton?: string | boolean
  icon?: boolean
  rtl?: boolean
  [key: string]: any
}

export interface ToastInterface {
  success: (message: string, options?: ToastOptions) => void
  error: (message: string, options?: ToastOptions) => void
  warning: (message: string, options?: ToastOptions) => void
  info: (message: string, options?: ToastOptions) => void
  clear: () => void
}

function getDefaults(): ToastOptions {
  let isRtl = false
  try {
    const userStore = useUserStore()
    isRtl = userStore.language === 'ar'
  } catch {
    isRtl = document.documentElement.dir === 'rtl'
  }
  return {
    timeout: 3000,
    closeOnClick: true,
    pauseOnHover: true,
    draggable: true,
    showCloseButtonOnHover: true,
    hideProgressBar: true,
    closeButton: false,
    icon: true,
    rtl: isRtl
  }
}

export function useToast(): { toast: ToastInterface } {
  const vueToast = useVueToastification()
  let t: ((key: string) => string) | null = null
  try {
    const i18n = useI18n()
    t = i18n.t
  } catch { /* outside setup context */ }

  // Auto-translate: if message looks like a translation key (PascalCase, no spaces), translate it
  const resolve = (message: string): string => {
    if (t && message && !message.includes(' ') && /^[A-Z]/.test(message)) {
      const translated = t(message)
      return translated !== message ? translated : message
    }
    return message
  }

  const toast: ToastInterface = {
    success(message: string, options?: ToastOptions) {
      vueToast.success(resolve(message), { ...getDefaults(), ...options } as any)
    },

    error(message: string, options?: ToastOptions) {
      vueToast.error(resolve(message), { ...getDefaults(), ...options } as any)
    },

    warning(message: string, options?: ToastOptions) {
      vueToast.warning(resolve(message), { ...getDefaults(), ...options } as any)
    },

    info(message: string, options?: ToastOptions) {
      vueToast.info(resolve(message), { ...getDefaults(), ...options } as any)
    },

    clear() {
      vueToast.clear()
    }
  }

  return { toast }
}

export default useToast

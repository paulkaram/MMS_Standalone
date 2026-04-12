import { ref } from 'vue'

export type ConfirmType = 'info' | 'warning' | 'danger' | 'success'

export interface ConfirmOptions {
  title: string
  message?: string
  type?: ConfirmType
  confirmText?: string
  cancelText?: string
}

// Global state for the confirm dialog
const isOpen = ref(false)
const options = ref<ConfirmOptions>({
  title: '',
  message: '',
  type: 'info',
  confirmText: 'تأكيد',
  cancelText: 'إلغاء'
})

let resolvePromise: ((value: boolean) => void) | null = null

export function useConfirm() {
  const confirm = (opts: ConfirmOptions): Promise<boolean> => {
    options.value = {
      title: opts.title,
      message: opts.message || '',
      type: opts.type || 'info',
      confirmText: opts.confirmText || 'تأكيد',
      cancelText: opts.cancelText || 'إلغاء'
    }
    isOpen.value = true

    return new Promise((resolve) => {
      resolvePromise = resolve
    })
  }

  const handleConfirm = () => {
    isOpen.value = false
    resolvePromise?.(true)
    resolvePromise = null
  }

  const handleCancel = () => {
    isOpen.value = false
    resolvePromise?.(false)
    resolvePromise = null
  }

  return {
    isOpen,
    options,
    confirm,
    handleConfirm,
    handleCancel
  }
}

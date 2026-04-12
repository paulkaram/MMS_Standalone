import { useUserStore } from '@/stores/user'

/**
 * Get the current date locale based on user language.
 * Uses 'ar-EG' for Arabic (Gregorian calendar) and 'en-US' for English.
 * Avoids 'ar-SA' which defaults to Hijri calendar.
 */
export function getDateLocale(): string {
  try {
    const userStore = useUserStore()
    return userStore.language === 'ar' ? 'ar-EG' : 'en-US'
  } catch {
    return 'en-US'
  }
}

/**
 * Format a date string for display.
 */
export function formatDate(date: string | Date | null, options?: Intl.DateTimeFormatOptions): string {
  if (!date) return '-'
  try {
    const d = typeof date === 'string' ? new Date(date) : date
    return d.toLocaleDateString(getDateLocale(), {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      calendar: 'gregory',
      ...options
    })
  } catch {
    return String(date)
  }
}

/**
 * Format a time string for display.
 */
export function formatTime(date: string | Date | null, options?: Intl.DateTimeFormatOptions): string {
  if (!date) return '-'
  try {
    const d = typeof date === 'string' ? new Date(date) : date
    return d.toLocaleTimeString(getDateLocale(), {
      hour: '2-digit',
      minute: '2-digit',
      ...options
    })
  } catch {
    return String(date)
  }
}

/**
 * Format a date+time string for display.
 */
export function formatDateTime(date: string | Date | null): string {
  if (!date) return '-'
  try {
    const d = typeof date === 'string' ? new Date(date) : date
    return d.toLocaleString(getDateLocale(), {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      calendar: 'gregory'
    })
  } catch {
    return String(date)
  }
}

import { ref, computed, onUnmounted } from 'vue'
import { TIMER_INTERVAL_MS } from '../constants'
import type { LiveAgenda } from '../types'

/**
 * Composable for managing meeting and agenda timers
 *
 * Fixes:
 * - Memory leak: Proper interval cleanup with onUnmounted
 * - Type safety: ReturnType<typeof setInterval> instead of any
 */
export function useMeetingTimers() {
  // ═══════════════════════════════════════════════════════════════════════════
  // STATE
  // ═══════════════════════════════════════════════════════════════════════════

  const remainingTime = ref(0)
  const elapsedSeconds = ref(0)
  const isPaused = ref(false)

  // Use proper type for intervals
  let agendaTimerInterval: ReturnType<typeof setInterval> | null = null
  let elapsedTimerInterval: ReturnType<typeof setInterval> | null = null

  // ═══════════════════════════════════════════════════════════════════════════
  // COMPUTED
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Formatted meeting elapsed time (HH:MM:SS)
   */
  const meetingElapsedTime = computed(() => {
    const h = Math.floor(elapsedSeconds.value / 3600)
    const m = Math.floor((elapsedSeconds.value % 3600) / 60)
    const s = elapsedSeconds.value % 60
    return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
  })

  // ═══════════════════════════════════════════════════════════════════════════
  // HELPERS
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Format seconds to MM:SS
   */
  function formatSeconds(seconds: number): string {
    const m = Math.floor(Math.abs(seconds) / 60)
    const s = Math.abs(seconds) % 60
    return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
  }

  /**
   * Format agenda time display
   */
  function formatAgendaTime(agenda: LiveAgenda): string {
    if (agenda.elapsedSeconds !== undefined) {
      return formatSeconds(agenda.elapsedSeconds)
    }
    if (agenda.remainingSeconds !== undefined) {
      return formatSeconds(agenda.remainingSeconds)
    }
    return '00:00'
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // AGENDA TIMER METHODS
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Start the agenda countdown timer
   * @param updateAgendaCallback - Optional callback to update agenda's remainingSeconds
   * @param onTimeUp - Callback when timer reaches 0 (auto-complete agenda)
   */
  function startAgendaTimer(
    updateAgendaCallback?: (remaining: number) => void,
    onTimeUp?: () => void
  ): void {
    stopAgendaTimer()

    agendaTimerInterval = setInterval(() => {
      if (!isPaused.value) {
        remainingTime.value--
        updateAgendaCallback?.(remainingTime.value)

        if (remainingTime.value <= 0 && onTimeUp) {
          stopAgendaTimer()
          onTimeUp()
        }
      }
    }, TIMER_INTERVAL_MS)
  }

  /**
   * Stop the agenda timer
   */
  function stopAgendaTimer(): void {
    if (agendaTimerInterval !== null) {
      clearInterval(agendaTimerInterval)
      agendaTimerInterval = null
    }
  }

  /**
   * Initialize agenda timer state from agenda data
   */
  function initializeAgendaTimer(agenda: LiveAgenda): void {
    isPaused.value = agenda.paused || false
    remainingTime.value = agenda.remainingSeconds ?? (agenda.duration || 0) * 60
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // ELAPSED TIMER METHODS
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Start the meeting elapsed timer
   */
  function startElapsedTimer(): void {
    stopElapsedTimer()
    elapsedSeconds.value = 0

    elapsedTimerInterval = setInterval(() => {
      elapsedSeconds.value++
    }, TIMER_INTERVAL_MS)
  }

  /**
   * Stop the elapsed timer
   */
  function stopElapsedTimer(): void {
    if (elapsedTimerInterval !== null) {
      clearInterval(elapsedTimerInterval)
      elapsedTimerInterval = null
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // COMBINED METHODS
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Stop all timers
   */
  function stopAllTimers(): void {
    stopAgendaTimer()
    stopElapsedTimer()
  }

  /**
   * Setup live timers when meeting is already in progress
   */
  function setupLiveTimers(
    agendas: LiveAgenda[],
    onRunningAgendaFound: (index: number) => void,
    updateCallback?: (remaining: number) => void,
    onTimeUp?: () => void
  ): void {
    const runningIndex = agendas.findIndex(a => a.isRunning)

    if (runningIndex >= 0) {
      onRunningAgendaFound(runningIndex)
      const agenda = agendas[runningIndex]
      initializeAgendaTimer(agenda)
      startAgendaTimer(updateCallback, onTimeUp)
    }

    startElapsedTimer()
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // CLEANUP
  // ═══════════════════════════════════════════════════════════════════════════

  // Ensure timers are cleaned up when component unmounts
  onUnmounted(() => {
    stopAllTimers()
  })

  // ═══════════════════════════════════════════════════════════════════════════
  // RETURN
  // ═══════════════════════════════════════════════════════════════════════════

  return {
    // State
    remainingTime,
    elapsedSeconds,
    isPaused,

    // Computed
    meetingElapsedTime,

    // Helpers
    formatSeconds,
    formatAgendaTime,

    // Agenda timer methods
    startAgendaTimer,
    stopAgendaTimer,
    initializeAgendaTimer,

    // Elapsed timer methods
    startElapsedTimer,
    stopElapsedTimer,

    // Combined methods
    stopAllTimers,
    setupLiveTimers,
  }
}

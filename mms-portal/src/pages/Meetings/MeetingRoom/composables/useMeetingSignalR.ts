import { ref, onUnmounted, type Ref } from 'vue'
import { useSignalR } from '@/composables/useSignalR'
import { SignalREvents } from '../constants'
import type { LiveAgenda } from '../types'

interface SignalRHandlers {
  onMeetingStarted?: () => void
  onMeetingEnded?: () => void
  onStatusChange?: (statusId: number) => void
  onAgendaChanges?: (agendas: LiveAgenda[]) => void
  onAttendanceChange?: (meetingId: number, attendeeIds: string[]) => void
  onChatChanges?: () => void
  onAgendaNotesChanges?: (agendaId: number) => void
  onUserOnline?: (userId: string | number) => void
}

interface RegisteredHandler {
  event: string
  handler: (...args: any[]) => void
}

/**
 * Composable for managing SignalR connection for meeting room
 *
 * Fixes:
 * - Memory leak: Track registered handlers for proper cleanup
 * - Race condition: Store meetingId at mount for cleanup
 */
export function useMeetingSignalR(meetingId: Ref<string | number>) {
  const {
    connect: connectSignalR,
    disconnect: disconnectSignalR,
    on: onSignalR,
    off: offSignalR,
    invoke: invokeSignalR,
    isConnected: signalRConnected
  } = useSignalR()

  // ═══════════════════════════════════════════════════════════════════════════
  // STATE
  // ═══════════════════════════════════════════════════════════════════════════

  const onlineAttendeeIds = ref<string[]>([])
  const isSetup = ref(false)

  // Track registered handlers for proper cleanup
  const registeredHandlers: RegisteredHandler[] = []

  // Store meeting ID at setup time (route may change before cleanup)
  let storedMeetingId: string | number = ''

  // ═══════════════════════════════════════════════════════════════════════════
  // HELPER: Register handler with tracking
  // ═══════════════════════════════════════════════════════════════════════════

  function registerHandler(event: string, handler: (...args: any[]) => void): void {
    onSignalR(event, handler)
    registeredHandlers.push({ event, handler })
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // METHODS
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Setup SignalR connection and subscribe to events
   */
  async function setup(handlers: SignalRHandlers): Promise<void> {
    if (isSetup.value) return

    // Store meeting ID for cleanup
    storedMeetingId = meetingId.value

    try {
      await connectSignalR()

      if (signalRConnected.value) {
        // Join the meeting room group first
        try {
          await invokeSignalR(
            SignalREvents.CHANGE_MEETING_ATTENDANCE_STATUS,
            Number(storedMeetingId),
            true
          )
        } catch (err) {
          // Silently fail - user can still use the meeting
          console.warn('Failed to join meeting room:', err)
        }

        // Subscribe to events with tracking
        if (handlers.onMeetingStarted) {
          registerHandler(SignalREvents.MEETING_STARTED, handlers.onMeetingStarted)
        }

        if (handlers.onMeetingEnded) {
          registerHandler(SignalREvents.MEETING_ENDED, handlers.onMeetingEnded)
        }

        if (handlers.onStatusChange) {
          registerHandler(
            `${SignalREvents.MEETING_STATUS_CHANGE}${storedMeetingId}`,
            handlers.onStatusChange
          )
        }

        if (handlers.onAgendaChanges) {
          registerHandler(
            `${SignalREvents.MEETING_AGENDA_CHANGES}${storedMeetingId}`,
            handlers.onAgendaChanges
          )
        }

        if (handlers.onAttendanceChange) {
          registerHandler(
            SignalREvents.NOTIFY_MEETING_ATTENDANCE_CHANGE,
            handlers.onAttendanceChange
          )
        }

        if (handlers.onChatChanges) {
          registerHandler(
            `${SignalREvents.MEETING_CHAT_CHANGES}${storedMeetingId}`,
            handlers.onChatChanges
          )
        }

        if (handlers.onAgendaNotesChanges) {
          registerHandler(
            `${SignalREvents.MEETING_AGENDA_NOTES_CHANGES}${storedMeetingId}`,
            handlers.onAgendaNotesChanges
          )
        }

        if (handlers.onUserOnline) {
          registerHandler(SignalREvents.USER_ONLINE, handlers.onUserOnline)
        }

        isSetup.value = true
      }
    } catch (error) {
      // SignalR setup failed - meeting can still be used without real-time updates
      console.error('SignalR setup failed:', error)
    }
  }

  /**
   * Update online attendee IDs
   */
  function updateOnlineAttendees(mid: number, attendeeIds: string[]): void {
    if (mid === Number(storedMeetingId)) {
      onlineAttendeeIds.value = Array.isArray(attendeeIds) ? attendeeIds : []
    }
  }

  /**
   * Cleanup SignalR connection and handlers
   */
  async function cleanup(): Promise<void> {
    // Use stored meeting ID (route may have already changed)
    const cleanupMeetingId = storedMeetingId || meetingId.value

    if (!cleanupMeetingId) return

    // Try to notify server before disconnecting
    try {
      if (signalRConnected.value) {
        await invokeSignalR(
          SignalREvents.CHANGE_MEETING_ATTENDANCE_STATUS,
          Number(cleanupMeetingId),
          false
        )
      }
    } catch {
      // Ignore errors - connection might already be closed
    }

    // Unsubscribe all registered handlers
    for (const { event, handler } of registeredHandlers) {
      try {
        offSignalR(event, handler)
      } catch {
        // Ignore cleanup errors
      }
    }
    registeredHandlers.length = 0

    // Disconnect SignalR
    try {
      disconnectSignalR()
    } catch {
      // Ignore disconnect errors
    }

    isSetup.value = false
    onlineAttendeeIds.value = []
  }

  /**
   * Handle beforeunload event
   */
  function handleBeforeUnload(): void {
    if (signalRConnected.value) {
      try {
        invokeSignalR(
          SignalREvents.CHANGE_MEETING_ATTENDANCE_STATUS,
          Number(storedMeetingId || meetingId.value),
          false
        )
      } catch {
        // Ignore errors
      }
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // LIFECYCLE
  // ═══════════════════════════════════════════════════════════════════════════

  // Ensure cleanup on unmount
  onUnmounted(() => {
    cleanup()
  })

  // ═══════════════════════════════════════════════════════════════════════════
  // RETURN
  // ═══════════════════════════════════════════════════════════════════════════

  return {
    // State
    isConnected: signalRConnected,
    onlineAttendeeIds,
    isSetup,

    // Methods
    setup,
    cleanup,
    updateOnlineAttendees,
    handleBeforeUnload,
  }
}

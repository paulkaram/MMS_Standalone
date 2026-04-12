import { ref, shallowRef, onUnmounted } from 'vue'
import {
  HubConnectionBuilder,
  HubConnection,
  HubConnectionState,
  LogLevel
} from '@microsoft/signalr'
import { useUserStore } from '@/stores/user'

// Types
interface SignalROptions {
  hubUrl?: string
  autoReconnect?: boolean
  reconnectDelays?: number[]
  logLevel?: LogLevel
}

interface EventHandler {
  event: string
  callback: (...args: any[]) => void
}

/**
 * Composable for SignalR real-time communication
 */
export function useSignalR(options: SignalROptions = {}) {
  const {
    hubUrl = `${import.meta.env.VITE_MAIN_API}intaliohub`,
    autoReconnect = true,
    reconnectDelays = [0, 2000, 5000, 10000, 30000],
    logLevel = LogLevel.Warning
  } = options

  // State
  const connection = shallowRef<HubConnection | null>(null)
  const isConnected = ref(false)
  const isConnecting = ref(false)
  const error = ref<Error | null>(null)
  const connectionId = ref<string | null>(null)

  // Store registered event handlers for reconnection
  const eventHandlers: EventHandler[] = []

  // User store for auth token
  const userStore = useUserStore()

  /**
   * Build the SignalR connection
   */
  function buildConnection(): HubConnection {
    const builder = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => userStore.token || ''
      })
      .configureLogging(logLevel)

    if (autoReconnect) {
      builder.withAutomaticReconnect(reconnectDelays)
    }

    return builder.build()
  }

  /**
   * Connect to the SignalR hub
   */
  async function connect(): Promise<void> {
    if (connection.value?.state === HubConnectionState.Connected) {
      return
    }

    if (isConnecting.value) {
      return
    }

    isConnecting.value = true
    error.value = null

    try {
      // Build new connection if needed
      if (!connection.value || connection.value.state === HubConnectionState.Disconnected) {
        connection.value = buildConnection()
        setupConnectionEvents()
        resubscribeEvents()
      }

      await connection.value.start()
      isConnected.value = true
      connectionId.value = connection.value.connectionId

    } catch (err) {
      error.value = err as Error
      isConnected.value = false
      console.error('SignalR connection error:', err)
      throw err
    } finally {
      isConnecting.value = false
    }
  }

  /**
   * Disconnect from the SignalR hub
   */
  async function disconnect(): Promise<void> {
    if (!connection.value) return

    try {
      await connection.value.stop()
      isConnected.value = false
      connectionId.value = null
    } catch (err) {
      console.error('SignalR disconnect error:', err)
      throw err
    }
  }

  /**
   * Subscribe to a hub event
   */
  function on<T extends any[]>(event: string, callback: (...args: T) => void): void {
    // Store the handler for reconnection
    eventHandlers.push({ event, callback })

    // Subscribe if connected
    if (connection.value) {
      connection.value.on(event, callback)
    }
  }

  /**
   * Unsubscribe from a hub event
   */
  function off(event: string, callback?: (...args: any[]) => void): void {
    // Remove from stored handlers
    const index = eventHandlers.findIndex(
      h => h.event === event && (!callback || h.callback === callback)
    )
    if (index >= 0) {
      eventHandlers.splice(index, 1)
    }

    // Unsubscribe if connected
    if (connection.value) {
      if (callback) {
        connection.value.off(event, callback)
      } else {
        connection.value.off(event)
      }
    }
  }

  /**
   * Invoke a hub method
   */
  async function invoke<T = void>(method: string, ...args: any[]): Promise<T> {
    if (!connection.value || connection.value.state !== HubConnectionState.Connected) {
      throw new Error('SignalR not connected')
    }

    return connection.value.invoke<T>(method, ...args)
  }

  /**
   * Send a message to the hub (fire and forget)
   */
  async function send(method: string, ...args: any[]): Promise<void> {
    if (!connection.value || connection.value.state !== HubConnectionState.Connected) {
      throw new Error('SignalR not connected')
    }

    return connection.value.send(method, ...args)
  }

  /**
   * Setup connection event handlers
   */
  function setupConnectionEvents(): void {
    if (!connection.value) return

    connection.value.onclose((err) => {
      isConnected.value = false
      connectionId.value = null
      error.value = err || null
    })

    connection.value.onreconnecting((err) => {
      isConnected.value = false
      isConnecting.value = true
      error.value = err || null
    })

    connection.value.onreconnected((newConnectionId) => {
      isConnected.value = true
      isConnecting.value = false
      connectionId.value = newConnectionId || null
      error.value = null
    })
  }

  /**
   * Resubscribe to all events after reconnection
   */
  function resubscribeEvents(): void {
    if (!connection.value) return

    for (const handler of eventHandlers) {
      connection.value.on(handler.event, handler.callback)
    }
  }

  // Cleanup on component unmount
  onUnmounted(() => {
    disconnect()
  })

  return {
    // State
    connection,
    isConnected,
    isConnecting,
    error,
    connectionId,

    // Methods
    connect,
    disconnect,
    on,
    off,
    invoke,
    send
  }
}

/**
 * Common meeting hub events
 */
export const MeetingHubEvents = {
  // Server -> Client events
  MEETING_STARTED: 'MeetingStarted',
  MEETING_ENDED: 'MeetingEnded',
  ATTENDANCE_UPDATED: 'AttendanceUpdated',
  AGENDA_ITEM_CHANGED: 'AgendaItemChanged',
  VOTE_STARTED: 'VoteStarted',
  VOTE_ENDED: 'VoteEnded',
  VOTE_CAST: 'VoteCast',
  RECOMMENDATION_ADDED: 'RecommendationAdded',
  TASK_ASSIGNED: 'TaskAssigned',
  MESSAGE_RECEIVED: 'MessageReceived',
  USER_JOINED: 'UserJoined',
  USER_LEFT: 'UserLeft',

  // Client -> Server methods
  JOIN_MEETING: 'JoinMeeting',
  LEAVE_MEETING: 'LeaveMeeting',
  MARK_ATTENDANCE: 'MarkAttendance',
  CAST_VOTE: 'CastVote',
  SEND_MESSAGE: 'SendMessage'
} as const

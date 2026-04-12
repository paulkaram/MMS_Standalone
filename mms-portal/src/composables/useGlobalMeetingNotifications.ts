import { ref, watch, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useMeetingNotificationsStore } from '@/stores/meetingNotifications'
import { useSignalR } from '@/composables/useSignalR'
import MeetingsService from '@/services/MeetingsService'

// Meeting status enum matching backend
const MeetingStatus = {
  DRAFT: 1,
  PENDING_APPROVAL: 2,
  APPROVED: 3,
  STARTED: 4,
  FINISHED: 5,
  PENDING_INITIAL_MOM_APPROVAL: 6,
  INITIAL_MOM_APPROVED: 7,
  PENDING_FINAL_MOM_SIGN: 8,
  FINAL_MOM_SIGNED: 9,
  CANCELED: 10
} as const

// Track registered handlers to clean up properly
const registeredHandlers = new Map<string, (status: number) => void>()

export function useGlobalMeetingNotifications() {
  const userStore = useUserStore()
  const notificationsStore = useMeetingNotificationsStore()
  const router = useRouter()
  const { connect, on, off, isConnected } = useSignalR()

  const isInitialized = ref(false)
  const userMeetingIds = ref<number[]>([])

  // Fetch user's meetings that are approved or already started
  async function fetchUserMeetings() {
    try {
      // Get meetings that are approved (upcoming) or already started (in progress)
      const [approvedRes, startedRes] = await Promise.all([
        MeetingsService.searchUserMeetings(
          { onlyMyMeetings: true, statusId: MeetingStatus.APPROVED },
          1,
          50
        ),
        MeetingsService.searchUserMeetings(
          { onlyMyMeetings: true, statusId: MeetingStatus.STARTED },
          1,
          50
        )
      ])

      console.log('[GlobalMeetingNotifications] API Response - Approved:', approvedRes)
      console.log('[GlobalMeetingNotifications] API Response - Started:', startedRes)

      // Safely extract arrays from response (handle various response structures)
      // API returns { data: { items: [], totalCount } | [], success, message } or { data: [], total }
      const extractMeetings = (res: any): any[] => {
        if (!res) return []
        // Direct array in data
        if (Array.isArray(res.data)) return res.data
        // Nested in data.items
        if (res.data?.items && Array.isArray(res.data.items)) return res.data.items
        // Nested in data.data (double wrapped)
        if (res.data?.data && Array.isArray(res.data.data)) return res.data.data
        // Try direct items
        if (res.items && Array.isArray(res.items)) return res.items
        return []
      }

      const approvedMeetings = extractMeetings(approvedRes)
      const startedMeetings = extractMeetings(startedRes)

      console.log('[GlobalMeetingNotifications] Approved meetings:', approvedMeetings.length, approvedMeetings)
      console.log('[GlobalMeetingNotifications] Started meetings:', startedMeetings.length, startedMeetings)

      // Collect all meeting IDs to subscribe to
      const allMeetings = [...approvedMeetings, ...startedMeetings]
      userMeetingIds.value = allMeetings.map((m) => parseInt(m.id || m.meetingId || '0')).filter(id => id > 0)

      console.log('[GlobalMeetingNotifications] Meeting IDs to subscribe:', userMeetingIds.value)

      // Add already started meetings to the notification store
      for (const meeting of startedMeetings) {
        const meetingId = parseInt(meeting.id || meeting.meetingId || '0')
        if (meetingId > 0) {
          console.log('[GlobalMeetingNotifications] Adding started meeting to store:', meetingId, meeting.title || meeting.titleAr)
          notificationsStore.addActiveMeeting({
            id: meetingId,
            title: meeting.title || meeting.titleAr || meeting.titleEn || '',
            committeeName: meeting.committeeName || meeting.councilName,
            startedAt: new Date()
          })
        }
      }

      return allMeetings
    } catch (error) {
      console.error('[GlobalMeetingNotifications] Failed to fetch user meetings:', error)
      return []
    }
  }

  // Subscribe to meeting status changes
  function subscribeToMeetingStatusChanges() {
    for (const meetingId of userMeetingIds.value) {
      const eventName = `MeetingStatusChange${meetingId}`

      // Skip if already registered
      if (registeredHandlers.has(eventName)) {
        continue
      }

      const handler = (newStatus: number) => {
        handleMeetingStatusChange(meetingId, newStatus)
      }

      registeredHandlers.set(eventName, handler)
      on(eventName, handler)
    }
  }

  // Handle meeting status change
  function handleMeetingStatusChange(meetingId: number, newStatus: number) {
    console.log('[GlobalMeetingNotifications] Status change received - Meeting:', meetingId, 'New Status:', newStatus)
    if (newStatus === MeetingStatus.STARTED) {
      // Meeting just started - need to fetch meeting details
      console.log('[GlobalMeetingNotifications] Meeting started, fetching details...')
      fetchMeetingDetails(meetingId)
    } else if (newStatus >= MeetingStatus.FINISHED) {
      // Meeting ended or moved past started status
      console.log('[GlobalMeetingNotifications] Meeting ended, removing from notifications')
      notificationsStore.removeMeeting(meetingId)
    }
  }

  // Fetch meeting details and add to notifications
  async function fetchMeetingDetails(meetingId: number) {
    try {
      const response = await MeetingsService.searchUserMeetings(
        { onlyMyMeetings: true, meetingId },
        1,
        1
      )
      const meetings = Array.isArray(response?.data) ? response.data : []

      if (meetings.length > 0) {
        const meeting = meetings[0]
        notificationsStore.addActiveMeeting({
          id: parseInt(meeting.id || meeting.meetingId || '0'),
          title: meeting.title || meeting.titleAr || meeting.titleEn || '',
          committeeName: meeting.committeeName || meeting.councilName,
          startedAt: new Date()
        })
      }
    } catch (error) {
      console.error('[GlobalMeetingNotifications] Failed to fetch meeting details:', error)
    }
  }

  // Initialize the global notification system
  async function initialize() {
    console.log('[GlobalMeetingNotifications] Initialize called - isInitialized:', isInitialized.value, 'isAuthenticated:', userStore.isAuthenticated)
    if (isInitialized.value || !userStore.isAuthenticated) {
      return
    }

    try {
      // Fetch user's meetings
      await fetchUserMeetings()

      // Connect to SignalR
      await connect()
      console.log('[GlobalMeetingNotifications] SignalR connected:', isConnected.value)

      // Subscribe to status changes for all user meetings
      if (isConnected.value) {
        subscribeToMeetingStatusChanges()
        console.log('[GlobalMeetingNotifications] Subscribed to', userMeetingIds.value.length, 'meeting status events')
      }

      isInitialized.value = true
      console.log('[GlobalMeetingNotifications] Initialization complete')
    } catch (error) {
      console.error('[GlobalMeetingNotifications] Initialization failed:', error)
    }
  }

  // Cleanup subscriptions
  function cleanup() {
    // Unsubscribe from all handlers
    for (const [eventName, handler] of registeredHandlers) {
      off(eventName, handler)
    }
    registeredHandlers.clear()

    // Clear store
    notificationsStore.clearAll()
    userMeetingIds.value = []
    isInitialized.value = false
  }

  // Watch for authentication changes
  watch(
    () => userStore.isAuthenticated,
    (isAuthenticated) => {
      if (isAuthenticated && !isInitialized.value) {
        initialize()
      } else if (!isAuthenticated) {
        cleanup()
      }
    },
    { immediate: true }
  )

  // Watch for route changes to detect when user enters/exits meeting room
  watch(
    () => router.currentRoute.value,
    (route) => {
      if (route.name === 'meetingRoom' && route.params.id) {
        const meetingId = parseInt(route.params.id as string)
        notificationsStore.setCurrentMeetingRoom(meetingId)
      } else {
        notificationsStore.setCurrentMeetingRoom(null)
      }
    },
    { immediate: true }
  )

  // Cleanup on unmount
  onUnmounted(() => {
    // Note: We don't cleanup here because this is a global composable
    // Cleanup happens when user logs out
  })

  // Navigate to meeting room
  function joinMeeting(meetingId: number) {
    router.push({ name: 'meetingRoom', params: { id: meetingId.toString() } })
  }

  return {
    initialize,
    cleanup,
    joinMeeting,
    isInitialized
  }
}

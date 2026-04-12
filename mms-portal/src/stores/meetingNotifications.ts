import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface ActiveMeeting {
  id: number
  title: string
  committeeName?: string
  startedAt: Date
  dismissed: boolean
}

export const useMeetingNotificationsStore = defineStore('meetingNotifications', () => {
  // State
  const activeMeetings = ref<ActiveMeeting[]>([])
  const currentMeetingRoomId = ref<number | null>(null)

  // Getters
  const visibleMeetings = computed(() => {
    return activeMeetings.value.filter(
      (m) => !m.dismissed && m.id !== currentMeetingRoomId.value
    )
  })

  const hasActiveMeetings = computed(() => visibleMeetings.value.length > 0)

  // Actions
  function addActiveMeeting(meeting: Omit<ActiveMeeting, 'dismissed'>) {
    const existing = activeMeetings.value.find((m) => m.id === meeting.id)
    if (!existing) {
      activeMeetings.value.push({
        ...meeting,
        dismissed: false
      })
    }
  }

  function removeMeeting(meetingId: number) {
    const index = activeMeetings.value.findIndex((m) => m.id === meetingId)
    if (index !== -1) {
      activeMeetings.value.splice(index, 1)
    }
  }

  function dismissMeeting(meetingId: number) {
    const meeting = activeMeetings.value.find((m) => m.id === meetingId)
    if (meeting) {
      meeting.dismissed = true
    }
  }

  function undismissMeeting(meetingId: number) {
    const meeting = activeMeetings.value.find((m) => m.id === meetingId)
    if (meeting) {
      meeting.dismissed = false
    }
  }

  function setCurrentMeetingRoom(meetingId: number | null) {
    currentMeetingRoomId.value = meetingId
  }

  function clearAll() {
    activeMeetings.value = []
    currentMeetingRoomId.value = null
  }

  return {
    // State
    activeMeetings,
    currentMeetingRoomId,
    // Getters
    visibleMeetings,
    hasActiveMeetings,
    // Actions
    addActiveMeeting,
    removeMeeting,
    dismissMeeting,
    undismissMeeting,
    setCurrentMeetingRoom,
    clearAll
  }
})

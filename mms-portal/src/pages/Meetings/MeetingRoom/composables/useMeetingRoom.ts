import { ref, computed, watch, onMounted, onUnmounted, type Ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import MeetingsService from '@/services/MeetingsService'
import MeetingAgendaService from '@/services/MeetingAgendaService'
import { useToast } from '@/composables/useToast'
import { useUserStore } from '@/stores/user'
import { MeetingStatusEnum } from '@/helpers/enumerations'
import { Messages } from '../constants'
import { useMeetingTimers } from './useMeetingTimers'
import { useMeetingSignalR } from './useMeetingSignalR'
import { useMeetingChat } from './useMeetingChat'
import { useAgendaNotes } from './useAgendaNotes'
import { useAgendaRecommendations } from './useAgendaRecommendations'
import { useMeetingVoting } from './useMeetingVoting'
import { useMinutesGenerator } from './useMinutesGenerator'
import type {
  LiveMeeting,
  LiveAgenda,
  MeetingAttendee,
  MeetingAttachment,
  AgendaData
} from '../types'

interface UseMeetingRoomOptions {
  collaborationRef?: Ref<{ chatMessagesRef: HTMLElement | null } | null>
}

/**
 * Main orchestrator composable for MeetingRoom
 *
 * Coordinates all sub-composables and provides unified state management
 */
export function useMeetingRoom(_options: UseMeetingRoomOptions = {}) {
  const route = useRoute()
  const router = useRouter()
  const { toast } = useToast()
  const userStore = useUserStore()

  // ═══════════════════════════════════════════════════════════════════════════
  // CORE STATE
  // ═══════════════════════════════════════════════════════════════════════════

  const loading = ref(true)
  const loadingText = ref(Messages.LOADING_MEETING)
  const actionLoading = ref(false)

  const meeting = ref<LiveMeeting | null>(null)
  const attendees = ref<MeetingAttendee[]>([])
  const agendas = ref<LiveAgenda[]>([])
  const attachments = ref<MeetingAttachment[]>([])

  const currentAgendaIndex = ref(0)
  const currentAttachment = ref<MeetingAttachment | null>(null)
  const currentAttachmentUrl = ref('')

  // Summary state
  const meetingSummary = ref('')
  const agendaSummary = ref('')
  const allAgendasDataLoaded = ref(false)

  // UI state
  const isLightTheme = ref(true)
  const leftPanelCollapsed = ref(false)
  const rightPanelCollapsed = ref(false)
  const showConfirmStart = ref(false)
  const showConfirmEnd = ref(false)
  const showApprovalModal = ref(false)

  // ═══════════════════════════════════════════════════════════════════════════
  // COMPUTED
  // ═══════════════════════════════════════════════════════════════════════════

  const meetingId = computed(() => route.params.id as string)

  const isLive = computed(() => meeting.value?.statusId === MeetingStatusEnum.Started)
  const meetingHasEnded = computed(() => meeting.value?.statusId === MeetingStatusEnum.Finished)

  const currentAgenda = computed(() => agendas.value[currentAgendaIndex.value] || null)

  const allAgendasCompleted = computed(() => {
    if (!agendas.value || agendas.value.length === 0) return false
    return agendas.value.every(a => a.actualEndDate != null)
  })

  const canControl = computed(() => {
    if (!meeting.value) return false
    // Prefer API flag if available, otherwise fallback to creator check
    if (meeting.value.canControl !== undefined) return meeting.value.canControl
    if (meeting.value.canManage !== undefined) return meeting.value.canManage
    if (meeting.value.isOwner) return true
    // Fallback: check if user is creator (use == for type coercion)
    const userId = userStore.user?.id
    return meeting.value.createdby == userId || meeting.value.createdBy == userId
  })

  const canStartMeeting = computed(() => {
    if (!meeting.value) return false
    // Allow starting from Draft, PendingMeetingApproval (no approval required), or Approved status
    const status = meeting.value.statusId
    const canStartStatus = status === MeetingStatusEnum.Draft ||
                           status === MeetingStatusEnum.PendingMeetingApproval ||
                           status === MeetingStatusEnum.Approved

    if (!canStartStatus) return false
    return canControl.value
  })

  const canEndMeeting = computed(() => {
    if (!meeting.value) return false
    return meeting.value.statusId === MeetingStatusEnum.Started && canControl.value
  })

  const allAttachments = computed(() => {
    const meetingAtts = attachments.value || []
    const agendaAtts = agendas.value.flatMap(a => a.attachments || [])
    return [...meetingAtts, ...agendaAtts]
  })

  const presentCount = computed(() => {
    return attendees.value.filter(a => a.attended).length
  })

  const currentUserId = computed(() => String(userStore.user?.id || ''))

  // All agendas data for summary view
  const allAgendasData = computed((): AgendaData[] => {
    return agendas.value.map(agenda => ({
      id: agenda.id,
      title: agenda.title || agenda.titleAr || '',
      notes: notesComposable.allAgendasNotesMap.value[agenda.id] || [],
      recommendations: recommendationsComposable.allAgendasRecommendationsMap.value[agenda.id] || []
    }))
  })

  // ═══════════════════════════════════════════════════════════════════════════
  // CHILD COMPOSABLES
  // ═══════════════════════════════════════════════════════════════════════════

  // Timers
  const timersComposable = useMeetingTimers()

  // SignalR
  const signalRComposable = useMeetingSignalR(meetingId)

  // Chat
  const chatScrollRef = ref<HTMLElement | null>(null)
  const chatComposable = useMeetingChat({
    meetingId,
    scrollContainerRef: chatScrollRef
  })

  // Notes
  const notesComposable = useAgendaNotes({
    currentAgenda
  })

  // Recommendations
  const recommendationsComposable = useAgendaRecommendations({
    currentAgenda
  })

  // Voting
  const votingComposable = useMeetingVoting({
    currentAgenda,
    agendas,
    isLive
  })

  // Minutes Generator
  const minutesComposable = useMinutesGenerator({
    meeting,
    agendas,
    attendees,
    meetingSummary,
    allAgendasNotesMap: notesComposable.allAgendasNotesMap,
    allAgendasRecommendationsMap: recommendationsComposable.allAgendasRecommendationsMap
  })

  // ═══════════════════════════════════════════════════════════════════════════
  // HELPERS
  // ═══════════════════════════════════════════════════════════════════════════

  function toArray(data: any, paths: string[] = []): any[] {
    if (Array.isArray(data)) return data
    for (const path of paths) {
      if (data?.[path] && Array.isArray(data[path])) return data[path]
    }
    return []
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // DATA LOADING
  // ═══════════════════════════════════════════════════════════════════════════

  async function loadMeeting(): Promise<void> {
    loading.value = true
    loadingText.value = Messages.LOADING_MEETING

    try {
      const liveResponse = await MeetingsService.loadLiveMeeting(Number(meetingId.value))
      const liveData = liveResponse?.data || liveResponse

      if (liveData) {
        meeting.value = liveData

        // Load data in parallel where possible
        const loadPromises: Promise<void>[] = []

        // Attendees
        if (liveData.meetingAttendees) {
          attendees.value = toArray(liveData.meetingAttendees, [])
        } else {
          loadPromises.push(
            MeetingsService.getAttendees(meetingId.value).then(atts => {
              attendees.value = toArray(atts, ['items', 'data', 'meetingAttendees'])
            })
          )
        }

        // Agendas
        if (liveData.meetingAgendas) {
          agendas.value = toArray(liveData.meetingAgendas, [])
        } else {
          loadPromises.push(
            MeetingAgendaService.getAgendaItems(Number(meetingId.value)).then(ags => {
              agendas.value = toArray(ags, ['items', 'data', 'meetingAgendas'])
            })
          )
        }

        // Attachments
        if (liveData.attachments) {
          attachments.value = toArray(liveData.attachments, [])
        } else {
          loadPromises.push(
            MeetingsService.getAttachments(meetingId.value).then(files => {
              attachments.value = toArray(files, ['items', 'data', 'attachments'])
            })
          )
        }

        // Wait for parallel loads
        await Promise.all(loadPromises)
      }

      // Select first attachment if available
      if (allAttachments.value.length) {
        await selectAttachment(allAttachments.value[0])
      }

      // Setup live timers if meeting is in progress
      if (isLive.value) {
        timersComposable.setupLiveTimers(
          agendas.value,
          (runningIndex) => { currentAgendaIndex.value = runningIndex },
          (remaining) => {
            const ag = agendas.value[currentAgendaIndex.value]
            if (ag) ag.remainingSeconds = remaining
          },
          () => {
            const ag = agendas.value[currentAgendaIndex.value]
            if (ag && !ag.actualEndDate) completeAgenda(ag, currentAgendaIndex.value)
          }
        )
      }

      // Setup SignalR
      await setupSignalR()

      // Load chat messages
      await chatComposable.loadMessages()

      // Load notes and recommendations for the current agenda
      const currentAg = agendas.value[currentAgendaIndex.value]
      if (currentAg?.id) {
        await Promise.all([
          notesComposable.loadNotes(currentAg.id),
          recommendationsComposable.loadRecommendations(currentAg.id)
        ])
        // Check if user has already voted on this agenda
        votingComposable.findUserVote()
      }

      // Load existing minutes if meeting status >= 6 (PendingInitialMeetingMinutesApproval)
      if (meeting.value && meeting.value.statusId >= MeetingStatusEnum.PendingInitialMeetingMinutesApproval) {
        // Load minutes for viewing (all users with canViewMom)
        await minutesComposable.loadExistingMinutes()

        // Load versions and approvals only for organizers (requires permission)
        if (canControl.value) {
          await minutesComposable.loadVersions()
          // Then load approvals (requires savedMinutesAttachmentId from loadExistingMinutes)
          await minutesComposable.loadApprovals()
        }
      }
    } catch (error) {
      console.error('Error loading meeting:', error)
      toast.error(Messages.MEETING_LOAD_ERROR)
      meeting.value = null
    } finally {
      loading.value = false
    }
  }

  async function setupSignalR(): Promise<void> {
    await signalRComposable.setup({
      onMeetingStarted: () => {
        if (meeting.value) meeting.value.statusId = MeetingStatusEnum.Started
        timersComposable.startElapsedTimer()
      },
      onMeetingEnded: () => {
        if (meeting.value) meeting.value.statusId = MeetingStatusEnum.Finished
        timersComposable.stopAllTimers()
      },
      onStatusChange: (statusId: number) => {
        if (meeting.value) {
          meeting.value.statusId = statusId
          if (statusId === MeetingStatusEnum.Started) {
            timersComposable.startElapsedTimer()
          } else if (statusId === MeetingStatusEnum.Finished) {
            timersComposable.stopAllTimers()
          }
        }
      },
      onAgendaChanges: (newAgendaItems: LiveAgenda[]) => {
        if (newAgendaItems?.length > 0) {
          agendas.value = newAgendaItems
          const runningIndex = newAgendaItems.findIndex((a) => a.isRunning)
          if (runningIndex >= 0) {
            currentAgendaIndex.value = runningIndex
            const ag = newAgendaItems[runningIndex]
            timersComposable.initializeAgendaTimer(ag)

            // Start/stop timer based on agenda state
            if (ag.isRunning && !ag.paused) {
              startAgendaTimerWithAutoComplete()
            } else {
              timersComposable.stopAgendaTimer()
            }
          } else {
            timersComposable.stopAgendaTimer()
          }
          // Update user's vote status after agenda data refresh
          votingComposable.findUserVote()
        }
      },
      onAttendanceChange: (mid: number, attendeeIds: string[]) => {
        signalRComposable.updateOnlineAttendees(mid, attendeeIds)
      },
      onChatChanges: () => {
        chatComposable.loadMessages()
      },
      onAgendaNotesChanges: (agendaId: number) => {
        const currentAg = agendas.value[currentAgendaIndex.value]
        if (currentAg?.id === agendaId) {
          notesComposable.loadNotes(agendaId)
        }
      }
    })
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // MEETING ACTIONS
  // ═══════════════════════════════════════════════════════════════════════════

  function goBack(): void {
    router.back()
  }

  function confirmStartMeeting(): void {
    showConfirmStart.value = true
  }

  function confirmEndMeeting(): void {
    showConfirmEnd.value = true
  }

  async function handleStartMeeting(): Promise<void> {
    showConfirmStart.value = false
    actionLoading.value = true

    try {
      const response: any = await MeetingsService.startMeeting(meetingId.value)
      if (meeting.value) meeting.value.statusId = MeetingStatusEnum.Started

      // Update agendas from response if available
      const updatedAgendas = response?.data?.data || response?.data || response
      if (Array.isArray(updatedAgendas) && updatedAgendas.length > 0) {
        agendas.value = updatedAgendas
      }

      // Find and set the running agenda
      const runningIndex = agendas.value.findIndex((a) => a.isRunning)
      if (runningIndex >= 0) {
        currentAgendaIndex.value = runningIndex
        const ag = agendas.value[runningIndex]
        timersComposable.initializeAgendaTimer(ag)
        startAgendaTimerWithAutoComplete()
      } else if (agendas.value.length > 0) {
        currentAgendaIndex.value = 0
        const ag = agendas.value[0]
        ag.isRunning = true
        timersComposable.initializeAgendaTimer(ag)
        startAgendaTimerWithAutoComplete()
      }

      timersComposable.startElapsedTimer()
      toast.success(Messages.MEETING_STARTED)
    } catch (error) {
      console.error(error)
      toast.error(Messages.MEETING_START_FAILED)
    } finally {
      actionLoading.value = false
    }
  }

  async function handleEndMeeting(): Promise<void> {
    showConfirmEnd.value = false
    actionLoading.value = true

    try {
      await MeetingsService.endMeeting(meetingId.value)
      if (meeting.value) meeting.value.statusId = MeetingStatusEnum.Finished
      timersComposable.stopAllTimers()
      toast.success(Messages.MEETING_ENDED)
    } catch (error) {
      console.error(error)
      toast.error(Messages.MEETING_END_FAILED)
    } finally {
      actionLoading.value = false
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // AGENDA ACTIONS
  // ═══════════════════════════════════════════════════════════════════════════

  function selectAgenda(index: number): void {
    currentAgendaIndex.value = index
    const agenda = agendas.value[index]

    if (agenda?.attachments?.length) {
      selectAttachment(agenda.attachments[0])
    }

    // Load notes, recommendations and summary for the selected agenda
    if (agenda?.id) {
      notesComposable.loadNotes(agenda.id)
      recommendationsComposable.loadRecommendations(agenda.id)
      loadAgendaSummary(agenda.id)
    }
  }

  async function selectAttachment(att: MeetingAttachment): Promise<void> {
    currentAttachment.value = att
    currentAttachmentUrl.value = ''

    try {
      // Check if it's a PowerPoint file - use PDF conversion endpoint
      const fileName = att.fileName || att.name || ''
      const extension = fileName.toLowerCase().split('.').pop() || ''
      const isPowerPoint = extension === 'pptx' || extension === 'ppt'

      if (isPowerPoint) {
        // Use the PDF conversion endpoint for PowerPoint files
        currentAttachmentUrl.value = `attachments/${att.id}/as-pdf`
      } else {
        // Regular attachment - get query string
        const response: any = await MeetingsService.getAttachmentQuery(att.id)
        const queryString = response?.data?.data || response?.data || response

        if (queryString) {
          currentAttachmentUrl.value = `attachments?${queryString}`
        }
      }
    } catch (error) {
      console.error('Failed to get attachment URL:', error)
      toast.error(Messages.ATTACHMENT_LOAD_FAILED)
    }
  }

  // Helper: start agenda timer with auto-complete on time up
  function startAgendaTimerWithAutoComplete(): void {
    timersComposable.startAgendaTimer(
      (remaining) => {
        const ag = agendas.value[currentAgendaIndex.value]
        if (ag) ag.remainingSeconds = remaining
      },
      () => {
        // Timer reached 0 — auto-complete and move to next
        const ag = agendas.value[currentAgendaIndex.value]
        if (ag && !ag.actualEndDate) {
          completeAgenda(ag, currentAgendaIndex.value)
        }
      }
    )
  }

  async function toggleAgendaTimer(agenda: LiveAgenda, index: number): Promise<void> {
    currentAgendaIndex.value = index

    try {
      if (agenda.isRunning && !agenda.paused) {
        // Pause the running agenda
        const response: any = await MeetingAgendaService.pauseResumeAgenda(Number(meetingId.value))
        const updatedAgendas = response?.data?.data || response?.data || response
        if (Array.isArray(updatedAgendas)) {
          agendas.value = updatedAgendas
          const ag = agendas.value[index]
          timersComposable.isPaused.value = ag?.paused || true
        } else {
          agenda.paused = true
          timersComposable.isPaused.value = true
        }
        timersComposable.stopAgendaTimer()
      } else if (agenda.isRunning && agenda.paused) {
        // Resume the paused agenda
        const response: any = await MeetingAgendaService.pauseResumeAgenda(Number(meetingId.value))
        const updatedAgendas = response?.data?.data || response?.data || response
        if (Array.isArray(updatedAgendas)) {
          agendas.value = updatedAgendas
          const ag = agendas.value[index]
          timersComposable.isPaused.value = ag?.paused || false
          timersComposable.remainingTime.value = ag?.remainingSeconds ?? timersComposable.remainingTime.value
        } else {
          agenda.paused = false
          timersComposable.isPaused.value = false
        }
        startAgendaTimerWithAutoComplete()
      } else {
        // Start the next agenda
        const response: any = await MeetingAgendaService.startNextAgenda(Number(meetingId.value))
        const updatedAgendas = response?.data?.data || response?.data || response
        if (Array.isArray(updatedAgendas)) {
          agendas.value = updatedAgendas
          const runningIndex = agendas.value.findIndex((a) => a.isRunning)
          if (runningIndex >= 0) {
            currentAgendaIndex.value = runningIndex
            const ag = agendas.value[runningIndex]
            timersComposable.initializeAgendaTimer(ag)
          }
        } else {
          agenda.isRunning = true
          agenda.paused = false
          timersComposable.initializeAgendaTimer(agenda)
        }
        startAgendaTimerWithAutoComplete()
      }
    } catch (error) {
      console.error(error)
      toast.error(Messages.AGENDA_STATUS_FAILED)
    }
  }

  async function toggleCurrentAgendaTimer(): Promise<void> {
    const agenda = currentAgenda.value
    if (!agenda) return
    await toggleAgendaTimer(agenda, currentAgendaIndex.value)
  }

  async function completeAgenda(agenda: LiveAgenda, index: number): Promise<void> {
    try {
      const response: any = await MeetingAgendaService.startNextAgenda(Number(meetingId.value))
      const updatedAgendas = response?.data?.data || response?.data || response

      if (Array.isArray(updatedAgendas)) {
        agendas.value = updatedAgendas
        const runningIndex = agendas.value.findIndex((a) => a.isRunning)
        if (runningIndex >= 0) {
          currentAgendaIndex.value = runningIndex
          const ag = agendas.value[runningIndex]
          timersComposable.initializeAgendaTimer(ag)
          startAgendaTimerWithAutoComplete()
        } else {
          timersComposable.stopAgendaTimer()
        }
      } else {
        agenda.actualEndDate = new Date().toISOString()
        agenda.isRunning = false
        if (index < agendas.value.length - 1) {
          currentAgendaIndex.value = index + 1
          const nextAg = agendas.value[index + 1]
          nextAg.isRunning = true
          timersComposable.initializeAgendaTimer(nextAg)
          startAgendaTimerWithAutoComplete()
        } else {
          timersComposable.stopAgendaTimer()
        }
      }
      toast.success(Messages.AGENDA_COMPLETED)
    } catch (error) {
      console.error(error)
      toast.error(Messages.AGENDA_COMPLETE_FAILED)
    }
  }

  async function completeCurrentAgenda(): Promise<void> {
    const agenda = currentAgenda.value
    if (!agenda || agenda.actualEndDate) return
    await completeAgenda(agenda, currentAgendaIndex.value)
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // ATTENDANCE
  // ═══════════════════════════════════════════════════════════════════════════

  async function markAttendance(att: MeetingAttendee, present: boolean): Promise<void> {
    try {
      if (present) {
        await MeetingsService.checkInAttendee(meetingId.value, String(att.userId))
        att.attended = true
      } else {
        await MeetingsService.checkOutAttendee(meetingId.value, String(att.userId))
        att.attended = false
      }
    } catch (error) {
      console.error(error)
      toast.error(Messages.ATTENDANCE_UPDATE_FAILED)
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // SUMMARY
  // ═══════════════════════════════════════════════════════════════════════════

  async function loadMeetingSummary(): Promise<void> {
    try {
      const response: any = await MeetingsService.getMeetingSummary(Number(meetingId.value))
      if (typeof response === 'string') {
        meetingSummary.value = response
      } else if (response?.data && typeof response.data === 'string') {
        meetingSummary.value = response.data
      } else {
        meetingSummary.value = ''
      }
    } catch (error) {
      console.error('Failed to load meeting summary:', error)
      meetingSummary.value = ''
    }
  }

  async function saveMeetingSummary(summary: string): Promise<void> {
    try {
      await MeetingsService.saveMeetingSummary(Number(meetingId.value), summary)
      meetingSummary.value = summary
      toast.success(Messages.MEETING_SUMMARY_SAVED)
    } catch (error) {
      console.error('Failed to save meeting summary:', error)
      toast.error(Messages.MEETING_SUMMARY_FAILED)
    }
  }

  async function loadAgendaSummary(agendaId: number): Promise<void> {
    try {
      const response: any = await MeetingAgendaService.getAgendaSummary(agendaId)
      if (typeof response === 'string') {
        agendaSummary.value = response
      } else if (response?.data && typeof response.data === 'string') {
        agendaSummary.value = response.data
      } else {
        agendaSummary.value = ''
      }
    } catch (error) {
      console.error('Failed to load agenda summary:', error)
      agendaSummary.value = ''
    }
  }

  async function saveAgendaSummary(summary: string): Promise<void> {
    const agenda = currentAgenda.value
    if (!agenda) return

    try {
      await MeetingAgendaService.saveAgendaSummary(agenda.id, summary)
      agendaSummary.value = summary
      toast.success(Messages.AGENDA_SUMMARY_SAVED)
    } catch (error) {
      console.error('Failed to save agenda summary:', error)
      toast.error(Messages.AGENDA_SUMMARY_FAILED)
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // ALL AGENDAS DATA LOADING
  // ═══════════════════════════════════════════════════════════════════════════

  async function loadAllAgendasData(): Promise<void> {
    if (allAgendasDataLoaded.value) return

    try {
      await Promise.all([
        notesComposable.loadAllAgendasNotes(agendas.value),
        recommendationsComposable.loadAllAgendasRecommendations(agendas.value)
      ])
      allAgendasDataLoaded.value = true
    } catch (error) {
      console.error('Failed to load all agendas data:', error)
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // MINUTES
  // ═══════════════════════════════════════════════════════════════════════════

  function handleUploadMinutes(): void {
    toast.info(Messages.MINUTES_UPLOAD_COMING_SOON)
  }

  async function handleGenerateMinutes(): Promise<void> {
    // Generate minutes directly and display in the viewer
    const result = await minutesComposable.generateDirect()
    // Load versions after successful generation
    if (result.success) {
      await minutesComposable.loadVersions()
    }
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // LIFECYCLE
  // ═══════════════════════════════════════════════════════════════════════════

  async function cleanup(): Promise<void> {
    timersComposable.stopAllTimers()
    await signalRComposable.cleanup()
  }

  onMounted(() => {
    window.addEventListener('beforeunload', signalRComposable.handleBeforeUnload)
    loadMeeting()
  })

  onUnmounted(() => {
    window.removeEventListener('beforeunload', signalRComposable.handleBeforeUnload)
    cleanup()
  })

  // Watch for route changes
  watch(() => route.params.id, () => {
    loadMeeting()
  })

  // Watch for agenda changes
  watch(currentAgendaIndex, (newIndex) => {
    const agenda = agendas.value[newIndex]
    if (agenda?.id) {
      notesComposable.cancelEdit()
      notesComposable.loadNotes(agenda.id)
      recommendationsComposable.loadRecommendations(agenda.id)
      votingComposable.findUserVote()
    }
  })

  // Watch for all agendas completed
  watch(allAgendasCompleted, (completed) => {
    if (completed) {
      loadAllAgendasData()
    }
  }, { immediate: true })

  // ═══════════════════════════════════════════════════════════════════════════
  // RETURN
  // ═══════════════════════════════════════════════════════════════════════════

  return {
    // Core state
    loading,
    loadingText,
    actionLoading,
    meeting,
    attendees,
    agendas,
    attachments,
    currentAgendaIndex,
    currentAttachment,
    currentAttachmentUrl,
    meetingSummary,
    agendaSummary,

    // UI state
    isLightTheme,
    leftPanelCollapsed,
    rightPanelCollapsed,
    showConfirmStart,
    showConfirmEnd,
    showApprovalModal,

    // Computed
    meetingId,
    isLive,
    meetingHasEnded,
    currentAgenda,
    allAgendasCompleted,
    canControl,
    canStartMeeting,
    canEndMeeting,
    allAttachments,
    presentCount,
    currentUserId,
    allAgendasData,

    // Timers
    ...timersComposable,

    // SignalR
    onlineAttendeeIds: signalRComposable.onlineAttendeeIds,

    // Chat
    chatMessages: chatComposable.messages,
    chatInputText: chatComposable.inputText,
    sendChatMessage: chatComposable.sendMessage,
    chatScrollRef,

    // Notes
    agendaNotes: notesComposable.notes,
    noteText: notesComposable.noteText,
    noteIsPublic: notesComposable.noteIsPublic,
    editingNoteId: notesComposable.editingNoteId,
    visibleNotes: notesComposable.visibleNotes,
    saveOrUpdateNote: notesComposable.saveOrUpdate,
    startEditNote: notesComposable.startEdit,
    deleteAgendaNote: notesComposable.deleteNote,
    cancelEditNote: notesComposable.cancelEdit,
    loadAllAgendasNotes: () => notesComposable.loadAllAgendasNotes(agendas.value),
    allAgendasNotesMap: notesComposable.allAgendasNotesMap,

    // Recommendations
    agendaRecommendations: recommendationsComposable.recommendations,
    editingRecommendation: recommendationsComposable.editingRecommendation,
    showRecommendationModal: recommendationsComposable.showModal,
    openAddRecommendation: recommendationsComposable.openAddModal,
    editRecommendation: recommendationsComposable.openEditModal,
    deleteRecommendation: recommendationsComposable.deleteRecommendation,
    closeRecommendationModal: recommendationsComposable.closeModal,
    handleSaveRecommendation: recommendationsComposable.saveRecommendation,

    // Voting
    votingOptions: votingComposable.votingOptions,
    votingActive: votingComposable.votingActive,
    canVote: votingComposable.canVote,
    userVoteOptionId: votingComposable.userVoteOptionId,
    votingLoading: votingComposable.loading,
    submitVote: votingComposable.submitVote,
    allAgendasVotingData: votingComposable.allAgendasVotingData,
    voteTimer: votingComposable.voteTimer,
    voteStatus: votingComposable.voteStatus,
    showVotingResultsModal: votingComposable.showResultsModal,

    // Actions
    goBack,
    confirmStartMeeting,
    confirmEndMeeting,
    handleStartMeeting,
    handleEndMeeting,
    selectAgenda,
    selectAttachment,
    toggleAgendaTimer,
    toggleCurrentAgendaTimer,
    completeAgenda,
    completeCurrentAgenda,
    markAttendance,
    loadMeetingSummary,
    saveMeetingSummary,
    loadAgendaSummary,
    saveAgendaSummary,
    handleUploadMinutes,
    handleGenerateMinutes,

    // Minutes Generator
    minutesData: minutesComposable.minutesData,
    showMinutesPreviewModal: minutesComposable.showPreviewModal,
    minutesGenerating: minutesComposable.generating,
    closeMinutesPreview: minutesComposable.closePreview,
    saveMinutes: minutesComposable.saveMinutes,
    downloadMinutes: minutesComposable.downloadMinutes,
    savedMinutesUrl: minutesComposable.savedMinutesUrl,
    savedMinutesFileName: minutesComposable.savedMinutesFileName,

    // Minutes versioning
    minutesVersions: minutesComposable.versions,
    currentMinutesVersionId: minutesComposable.currentVersionId,
    loadingMinutesVersions: minutesComposable.loadingVersions,
    regeneratingMinutes: minutesComposable.regenerating,
    sendingMinutesForApproval: minutesComposable.sendingForApproval,
    regenerateMinutes: minutesComposable.regenerateMinutes,
    sendMinutesForApproval: minutesComposable.sendForApproval,
    selectMinutesVersion: minutesComposable.selectVersion,
    loadMinutesVersions: minutesComposable.loadVersions,

    // Minutes approval tracking
    minutesPendingUserIds: minutesComposable.pendingUserIds,
    minutesApprovals: minutesComposable.approvals,
    loadingMinutesApprovals: minutesComposable.loadingApprovals,
    loadMinutesPendingUsers: minutesComposable.loadPendingUsers,
    loadMinutesApprovals: minutesComposable.loadApprovals,
    clearMinutesPendingUsers: minutesComposable.clearPendingUsers,

    // Cleanup
    cleanup,
  }
}

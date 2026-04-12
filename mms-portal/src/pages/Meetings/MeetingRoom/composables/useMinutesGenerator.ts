import { ref, computed, type Ref, type ComputedRef } from 'vue'
import { useI18n } from 'vue-i18n'
import { useToast } from '@/composables/useToast'
import { useUserStore } from '@/stores/user'
import MeetingsService from '@/services/MeetingsService'
import type {
  MinutesOfMeeting,
  MinutesVersion,
  MinutesAttendee,
  MinutesAgendaItem,
  MinutesActionItem,
  MinutesStatus,
  AttendeeRole,
  GenerateMinutesRequest
} from '../types/minutes'
import type {
  LiveMeeting,
  LiveAgenda,
  MeetingAttendee,
  AgendaNote,
  AgendaRecommendation,
  UserVote,
  VotingOption
} from '../types'

interface UseMinutesGeneratorOptions {
  meeting: Ref<LiveMeeting | null>
  agendas: Ref<LiveAgenda[]>
  attendees: Ref<MeetingAttendee[]>
  meetingSummary: Ref<string>
  allAgendasNotesMap: Ref<Record<number, AgendaNote[]>>
  allAgendasRecommendationsMap: Ref<Record<number, AgendaRecommendation[]>>
}

/**
 * Composable for generating Minutes of Meeting
 */
export function useMinutesGenerator(options: UseMinutesGeneratorOptions) {
  const {
    meeting,
    agendas,
    attendees,
    meetingSummary,
    allAgendasNotesMap,
    allAgendasRecommendationsMap
  } = options

  const { toast } = useToast()
  const { t } = useI18n()
  const userStore = useUserStore()

  // ═══════════════════════════════════════════════════════════════════════════
  // STATE
  // ═══════════════════════════════════════════════════════════════════════════

  const generating = ref(false)
  const showPreviewModal = ref(false)
  const showVersionsModal = ref(false)
  const minutesData = ref<MinutesOfMeeting | null>(null)
  const versions = ref<MinutesVersion[]>([])
  const currentVersion = ref<MinutesVersion | null>(null)
  const loadingVersions = ref(false)

  // Saved minutes state for direct display
  const savedMinutesAttachmentId = ref<number | null>(null)
  const savedMinutesUrl = ref<string>('')
  const savedMinutesFileName = ref<string>('')

  // Version management state
  const currentVersionId = ref<number | null>(null)
  const sendingForApproval = ref(false)
  const regenerating = ref(false)

  // Approval tracking state
  const pendingUserIds = ref<string[]>([])
  const approvals = ref<any[]>([])
  const loadingApprovals = ref(false)

  // ═══════════════════════════════════════════════════════════════════════════
  // COMPUTED
  // ═══════════════════════════════════════════════════════════════════════════

  const canGenerate = computed(() => {
    return meeting.value !== null && agendas.value.length > 0
  })

  const hasExistingMinutes = computed(() => {
    return versions.value.length > 0
  })

  const latestVersion = computed(() => {
    if (versions.value.length === 0) return null
    return versions.value.reduce((latest, v) =>
      v.version > latest.version ? v : latest
    )
  })

  const latestApprovedVersion = computed(() => {
    const approved = versions.value.filter(v => v.status === 'approved' || v.status === 'published')
    if (approved.length === 0) return null
    return approved.reduce((latest, v) =>
      v.version > latest.version ? v : latest
    )
  })

  // ═══════════════════════════════════════════════════════════════════════════
  // DATA TRANSFORMATION
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Transform meeting data into MinutesOfMeeting structure
   */
  function buildMinutesData(): MinutesOfMeeting | null {
    const m = meeting.value
    if (!m) return null

    // Calculate duration
    const startDate = new Date(m.startTime || m.fromTime || m.start || '')
    const endDate = new Date(m.endTime || m.toTime || m.end || '')
    const durationMs = endDate.getTime() - startDate.getTime()
    const durationMinutes = Math.floor(durationMs / 60000)
    const hours = Math.floor(durationMinutes / 60)
    const minutes = durationMinutes % 60
    const durationStr = hours > 0 ? `${hours} ساعة ${minutes} دقيقة` : `${minutes} دقيقة`

    // Transform attendees
    const minutesAttendees: MinutesAttendee[] = attendees.value.map((att, index) => ({
      id: att.id || index,
      name: att.userFullName || att.fullName || att.name || att.userName || 'غير معروف',
      title: '',
      role: determineRole(att, index),
      attended: att.attended === true,
      attendedAt: att.attendedAt,
      signatureRequired: index < 2, // Chairman and secretary
      signed: false
    }))

    // Transform agenda items
    const minutesAgendaItems: MinutesAgendaItem[] = agendas.value.map((agenda, index) => {
      const notes = allAgendasNotesMap.value[agenda.id] || []
      const recommendations = allAgendasRecommendationsMap.value[agenda.id] || []
      const votingOptions = agenda.votingType?.votingOptions || agenda.votingOptions || []
      const votes = agenda.meetingUserVotes || []

      return {
        index: index + 1,
        id: agenda.id,
        title: agenda.title || agenda.titleAr || `البند ${index + 1}`,
        description: agenda.description,
        plannedDuration: agenda.duration || 0,
        actualDuration: agenda.elapsedSeconds ? Math.floor(agenda.elapsedSeconds / 60) : undefined,
        startedAt: agenda.actualStartDate,
        endedAt: agenda.actualEndDate,
        summary: '', // Will be filled from agenda summary
        discussionNotes: notes.filter(n => n.isPublic).map(n => ({
          id: n.id,
          text: n.text,
          isPublic: n.isPublic,
          authorName: n.createdByName || 'مجهول',
          createdAt: n.createdAt || ''
        })),
        hasVoting: votingOptions.length > 0,
        votingResults: votingOptions.length > 0 ? buildVotingResults(votingOptions, votes, attendees.value) : undefined,
        recommendations: recommendations.map(r => ({
          id: r.id,
          text: r.text,
          ownerName: r.ownerName,
          dueDate: r.dueDate
        })),
        attachmentsDiscussed: []
      }
    })

    // Build action items from recommendations that have owners/due dates
    const actionItems: MinutesActionItem[] = []
    agendas.value.forEach(agenda => {
      const recs = allAgendasRecommendationsMap.value[agenda.id] || []
      recs.forEach(rec => {
        if (rec.ownerId || rec.dueDate) {
          actionItems.push({
            id: rec.id,
            description: rec.text,
            assignedTo: rec.ownerName || 'غير محدد',
            assignedToId: String(rec.ownerId || ''),
            dueDate: rec.dueDate || '',
            priority: 'medium',
            status: 'pending',
            sourceAgendaId: agenda.id,
            sourceAgendaTitle: agenda.title || agenda.titleAr
          })
        }
      })
    })

    const presentCount = minutesAttendees.filter(a => a.attended).length
    const absentCount = minutesAttendees.length - presentCount

    // Determine next version number
    const nextVersion = latestVersion.value ? latestVersion.value.version + 1 : 1

    return {
      meetingId: m.id,
      meetingNumber: m.meetingNumber || m.referenceNumber || m.refNumber || `MTG-${m.id}`,
      referenceNumber: m.referenceNumber || m.refNumber || '',
      title: m.title || '',
      titleAr: m.titleAr || m.title || '',

      committeeName: m.committeeName || '',
      councilName: m.councilName || '',
      organizationName: '', // TODO: Get from organization

      date: formatDate(startDate),
      hijriDate: '', // TODO: Convert to Hijri
      startTime: formatTime(startDate),
      endTime: formatTime(endDate),
      actualDuration: durationStr,

      location: m.location || 'غير محدد',
      meetingType: 'physical',

      attendees: minutesAttendees,
      totalAttendees: minutesAttendees.length,
      presentCount,
      absentCount,
      quorumMet: presentCount >= Math.ceil(minutesAttendees.length / 2),

      agendaItems: minutesAgendaItems,
      actionItems,

      meetingSummary: meetingSummary.value || '',

      chairmanName: minutesAttendees[0]?.name || '',
      chairmanTitle: 'رئيس الاجتماع',
      secretaryName: minutesAttendees[1]?.name || '',
      secretaryTitle: 'أمين السر',

      version: nextVersion,
      versionLabel: `${nextVersion}.0`,
      status: 'draft',
      generatedAt: new Date().toISOString(),
      generatedBy: userStore.user?.fullName || ''
    }
  }

  function determineRole(att: MeetingAttendee, index: number): AttendeeRole {
    // First attendee is typically chairman, second is secretary
    if (index === 0) return 'chairman'
    if (index === 1) return 'secretary'
    return 'member'
  }

  function buildVotingResults(
    options: VotingOption[],
    votes: UserVote[],
    allAttendees: MeetingAttendee[]
  ) {
    const totalVoters = votes.length
    const optionResults = options.map(opt => {
      const optionVotes = votes.filter(v =>
        v.votingOptionId === opt.id ||
        v.vottingOptionId === opt.id ||
        v.VotingOptionId === opt.id ||
        v.VottingOptionId === opt.id
      )
      const voterNames = optionVotes.map(v => {
        const attendee = allAttendees.find(a =>
          String(a.userId) === String(v.userId) ||
          String(a.userId) === String(v.UserId)
        )
        return attendee?.userFullName || attendee?.fullName || v.userName || 'مجهول'
      })

      return {
        id: opt.id,
        name: opt.nameEn || opt.name || '',
        nameAr: opt.nameAr || opt.name || '',
        voteCount: optionVotes.length,
        percentage: totalVoters > 0 ? Math.round((optionVotes.length / totalVoters) * 100) : 0,
        voters: voterNames
      }
    })

    // Determine outcome
    const maxVotes = Math.max(...optionResults.map(o => o.voteCount))
    const winner = optionResults.find(o => o.voteCount === maxVotes)
    const outcome = winner?.nameAr || 'غير محدد'

    return {
      votingType: '',
      totalVoters,
      options: optionResults,
      outcome
    }
  }

  function formatDate(date: Date): string {
    if (isNaN(date.getTime())) return ''
    return date.toLocaleDateString('ar-EG', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    })
  }

  function formatTime(date: Date): string {
    if (isNaN(date.getTime())) return ''
    return date.toLocaleTimeString('ar-EG', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: true
    })
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // ACTIONS
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Load existing minutes if available (for when meeting status >= 6)
   */
  async function loadExistingMinutes(): Promise<boolean> {
    const m = meeting.value
    if (!m) return false

    // Skip if already loaded
    if (savedMinutesUrl.value) return true

    try {
      const response: any = await MeetingsService.getInitialMeetingMinutes(m.id)
      const attachment = response?.data || response

      if (attachment?.id) {
        // Get the attachment URL for viewing
        const urlResponse: any = await MeetingsService.getAttachmentQuery(attachment.id)
        const queryString = urlResponse?.data?.data || urlResponse?.data || urlResponse

        if (queryString) {
          savedMinutesAttachmentId.value = attachment.id
          savedMinutesUrl.value = `attachments?${queryString}`
          savedMinutesFileName.value = attachment.fileName || `محضر-اجتماع-${m.id}.pdf`
          // Also set current version ID to this attachment (latest)
          currentVersionId.value = attachment.id
          return true
        }
      }
    } catch (error) {
      // This is expected if no minutes exist yet
    }
    return false
  }

  /**
   * Generate minutes and show preview
   */
  async function generateAndPreview(): Promise<void> {
    generating.value = true

    try {
      const data = buildMinutesData()
      if (!data) {
        toast.error(t('MinutesDataIncomplete'))
        return
      }

      minutesData.value = data
      showPreviewModal.value = true
    } catch (error) {
      console.error('Failed to generate minutes:', error)
      toast.error(t('MinutesCreateFailed'))
    } finally {
      generating.value = false
    }
  }

  /**
   * Generate minutes directly and return attachment URL for display
   * This skips the preview modal and directly creates the PDF
   */
  async function generateDirect(): Promise<{ success: boolean; url?: string; fileName?: string; newStatusId?: number }> {
    const m = meeting.value
    if (!m) {
      toast.error(t('MinutesDataIncomplete'))
      return { success: false }
    }

    generating.value = true
    try {
      // Call the PDF generation API directly
      const response: any = await MeetingsService.generatePdfMinutes(m.id, {
        includePrivateNotes: false,
        includeVoterNames: true,
        language: 'ar'
      })

      if (response?.data?.success || response?.success) {
        const attachmentId = response?.data?.attachmentId || response?.attachmentId
        const fileName = response?.data?.fileName || response?.fileName || `محضر-اجتماع-${m.id}.pdf`
        const newStatusId = response?.data?.newMeetingStatusId || response?.newMeetingStatusId

        if (attachmentId) {
          // Get the attachment URL for viewing
          const urlResponse: any = await MeetingsService.getAttachmentQuery(attachmentId)
          const queryString = urlResponse?.data?.data || urlResponse?.data || urlResponse

          if (queryString) {
            const attachmentUrl = `attachments?${queryString}`
            savedMinutesAttachmentId.value = attachmentId
            savedMinutesUrl.value = attachmentUrl
            savedMinutesFileName.value = fileName

            // Update meeting status if returned
            if (newStatusId && meeting.value) {
              meeting.value.statusId = newStatusId
            }

            toast.success(t('MinutesCreatedSuccess'))
            return { success: true, url: attachmentUrl, fileName, newStatusId }
          }
        }
      }

      toast.error(response?.data?.message || response?.message || t('MinutesCreateFailed'))
      return { success: false }
    } catch (error) {
      console.error('Failed to generate minutes directly:', error)
      toast.error(t('MinutesCreateFailed'))
      return { success: false }
    } finally {
      generating.value = false
    }
  }

  /**
   * Save minutes as PDF using the new PDF generator
   */
  async function saveMinutes(): Promise<void> {
    if (!minutesData.value || !meeting.value) return

    generating.value = true
    try {
      // Call new PDF-based minutes generation API
      const response: any = await MeetingsService.generatePdfMinutes(meeting.value.id, {
        includePrivateNotes: false,
        includeVoterNames: true,
        language: 'ar'
      })

      if (response?.data?.success || response?.success) {
        toast.success(t('MinutesSavedSuccess'))
        await loadVersions()
        showPreviewModal.value = false
      } else {
        toast.error(response?.data?.message || response?.message || t('MinutesSaveFailed'))
      }
    } catch (error) {
      console.error('Failed to save minutes:', error)
      toast.error(t('MinutesSaveFailed'))
    } finally {
      generating.value = false
    }
  }

  /**
   * Download minutes PDF
   */
  async function downloadMinutes(version?: MinutesVersion): Promise<void> {
    const v = version || latestVersion.value
    if (!v) {
      toast.error(t('MinutesNoDownload'))
      return
    }

    try {
      const response: any = await MeetingsService.downloadMinutes(v.id)
      // Handle blob download
      const blob = new Blob([response], { type: 'application/pdf' })
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = v.fileName || `محضر-اجتماع-${meeting.value?.id}.pdf`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)
    } catch (error) {
      console.error('Failed to download minutes:', error)
      toast.error(t('MinutesDownloadFailed'))
    }
  }

  /**
   * Load version history from the backend
   */
  async function loadVersions(): Promise<void> {
    if (!meeting.value) return

    loadingVersions.value = true
    try {
      const response: any = await MeetingsService.getInitialMeetingMinutesVersions(meeting.value.id)
      const versionsData = response?.data || response || []

      // Map backend response to MinutesVersion format
      // Backend may return Version (capital V) or version (lowercase)
      versions.value = versionsData.map((v: any) => ({
        id: v.id || v.Id,
        version: v.version || v.Version || 1,
        status: v.status || v.Status || 'draft',
        createdAt: v.createdAt || v.createdDate || v.CreatedAt || v.CreatedDate,
        fileName: v.fileName || v.FileName,
        attachmentId: v.attachmentId || v.AttachmentId || v.id || v.Id
      }))

      // Set current version to latest if not already set, or update currentVersion object
      if (versions.value.length > 0) {
        if (!currentVersionId.value) {
          // No version selected yet, select the latest
          const latest = versions.value.reduce((a, b) => (a.version > b.version ? a : b))
          currentVersionId.value = latest.id
          currentVersion.value = latest
        } else {
          // Version already selected, find and set the currentVersion object
          const found = versions.value.find(v => v.id === currentVersionId.value)
          if (found) {
            currentVersion.value = found
          } else {
            // If not found, default to latest
            const latest = versions.value.reduce((a, b) => (a.version > b.version ? a : b))
            currentVersionId.value = latest.id
            currentVersion.value = latest
          }
        }
      }
    } catch (error) {
      console.error('Failed to load minutes versions:', error)
      versions.value = []
    } finally {
      loadingVersions.value = false
    }
  }

  /**
   * Regenerate minutes with latest recommendations/comments
   * Creates a new version
   */
  async function regenerateMinutes(): Promise<{ success: boolean; url?: string; fileName?: string }> {
    const m = meeting.value
    if (!m) {
      toast.error(t('MinutesRegenerateDataIncomplete'))
      return { success: false }
    }

    regenerating.value = true
    try {
      // Call the PDF generation API - it auto-increments version
      const response: any = await MeetingsService.generatePdfMinutes(m.id, {
        includePrivateNotes: false,
        includeVoterNames: true,
        language: 'ar'
      })

      if (response?.data?.success || response?.success) {
        const attachmentId = response?.data?.attachmentId || response?.attachmentId
        const fileName = response?.data?.fileName || response?.fileName || `محضر-اجتماع-${m.id}.pdf`

        if (attachmentId) {
          // Get the attachment URL for viewing
          const urlResponse: any = await MeetingsService.getAttachmentQuery(attachmentId)
          const queryString = urlResponse?.data?.data || urlResponse?.data || urlResponse

          if (queryString) {
            const attachmentUrl = `attachments?${queryString}`
            savedMinutesAttachmentId.value = attachmentId
            savedMinutesUrl.value = attachmentUrl
            savedMinutesFileName.value = fileName
            currentVersionId.value = attachmentId

            // Reload versions to get the new version
            await loadVersions()

            toast.success(t('MinutesRegeneratedSuccess'))
            return { success: true, url: attachmentUrl, fileName }
          }
        }
      }

      toast.error(response?.data?.message || response?.message || t('MinutesRegenerateFailed'))
      return { success: false }
    } catch (error) {
      console.error('Failed to regenerate minutes:', error)
      toast.error(t('MinutesRegenerateFailed'))
      return { success: false }
    } finally {
      regenerating.value = false
    }
  }

  /**
   * Send minutes for approval to selected users
   * @param userIds - Array of user IDs to send for approval
   * @param dueDateDays - Due date as number of days from now (0 = no due date)
   */
  async function sendForApproval(userIds: string[], dueDateDays?: number): Promise<boolean> {
    const m = meeting.value
    if (!m) {
      toast.error(t('MinutesApprovalDataIncomplete'))
      return false
    }

    if (userIds.length === 0) {
      toast.error(t('MinutesSelectAtLeastOneUser'))
      return false
    }

    sendingForApproval.value = true
    try {
      // Backend expects: MeetingMinutesTaskDto(int MeetingId, List<string> UsersIds, int DueDate)
      await MeetingsService.sendInitialMeetingMinutesForApproval(m.id, userIds, dueDateDays ?? 0)
      toast.success(t('MinutesSentForApprovalSuccess'))

      // Reload versions to update status
      await loadVersions()

      return true
    } catch (error) {
      console.error('Failed to send for approval:', error)
      toast.error(t('MinutesSendForApprovalFailed'))
      return false
    } finally {
      sendingForApproval.value = false
    }
  }

  /**
   * Select a specific version to view
   */
  async function selectVersion(versionId: number): Promise<void> {
    const version = versions.value.find(v => v.id === versionId)
    if (!version) return

    currentVersionId.value = versionId
    currentVersion.value = version

    try {
      // Get the attachment URL for the selected version
      const attachmentId = version.attachmentId || version.id
      const urlResponse: any = await MeetingsService.getAttachmentQuery(attachmentId)
      const queryString = urlResponse?.data?.data || urlResponse?.data || urlResponse

      if (queryString) {
        savedMinutesUrl.value = `attachments?${queryString}`
        savedMinutesFileName.value = version.fileName || `محضر-اجتماع-v${version.version}.pdf`
        savedMinutesAttachmentId.value = attachmentId
      }
    } catch (error) {
      console.error('Failed to load version:', error)
      toast.error(t('MinutesVersionLoadFailed'))
    }
  }

  /**
   * View a specific version
   */
  async function viewVersion(version: MinutesVersion): Promise<void> {
    currentVersion.value = version
    // TODO: Load version data and show in preview
    showPreviewModal.value = true
  }

  /**
   * Close preview modal
   */
  function closePreview(): void {
    showPreviewModal.value = false
  }

  /**
   * Close versions modal
   */
  function closeVersionsModal(): void {
    showVersionsModal.value = false
  }

  /**
   * Open versions modal
   */
  async function openVersionsModal(): Promise<void> {
    await loadVersions()
    showVersionsModal.value = true
  }

  /**
   * Load user IDs who already have pending approval tasks for current MOM version
   */
  async function loadPendingUsers(): Promise<void> {
    const m = meeting.value
    if (!m) return

    try {
      const response: any = await MeetingsService.getInitialMeetingMinutesUsers(m.id)
      const userIds = response?.data?.data || response?.data || response || []
      pendingUserIds.value = Array.isArray(userIds) ? userIds : []
    } catch (error) {
      console.error('Failed to load pending users:', error)
      pendingUserIds.value = []
    }
  }

  /**
   * Load approval statuses with comments for the current MOM attachment
   */
  async function loadApprovals(): Promise<void> {
    const attachmentId = savedMinutesAttachmentId.value
    if (!attachmentId) return

    loadingApprovals.value = true
    try {
      const response: any = await MeetingsService.getMeetingMinutesApprovals(attachmentId)
      const approvalsData = response?.data?.data || response?.data || response || []
      approvals.value = Array.isArray(approvalsData) ? approvalsData : []
    } catch (error) {
      console.error('Failed to load approvals:', error)
      approvals.value = []
    } finally {
      loadingApprovals.value = false
    }
  }

  /**
   * Clear pending users (call after regenerating new version)
   */
  function clearPendingUsers(): void {
    pendingUserIds.value = []
    approvals.value = []
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // RETURN
  // ═══════════════════════════════════════════════════════════════════════════

  return {
    // State
    generating,
    showPreviewModal,
    showVersionsModal,
    minutesData,
    versions,
    currentVersion,
    loadingVersions,

    // Saved minutes state for direct display
    savedMinutesAttachmentId,
    savedMinutesUrl,
    savedMinutesFileName,

    // Version management state
    currentVersionId,
    sendingForApproval,
    regenerating,

    // Approval tracking state
    pendingUserIds,
    approvals,
    loadingApprovals,

    // Computed
    canGenerate,
    hasExistingMinutes,
    latestVersion,
    latestApprovedVersion,

    // Actions
    loadExistingMinutes,
    generateAndPreview,
    generateDirect,
    saveMinutes,
    downloadMinutes,
    loadVersions,
    viewVersion,
    closePreview,
    closeVersionsModal,
    openVersionsModal,
    buildMinutesData,

    // Version management actions
    regenerateMinutes,
    sendForApproval,
    selectVersion,

    // Approval tracking actions
    loadPendingUsers,
    loadApprovals,
    clearPendingUsers,
  }
}

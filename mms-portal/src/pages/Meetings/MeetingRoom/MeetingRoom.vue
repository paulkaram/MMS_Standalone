<template>
  <div class="meeting-room" :class="{ light: isLightTheme }">
    <!-- LOADING STATE -->
    <MeetingRoomLoading v-if="loading" :loading-text="loadingText" />

    <!-- MAIN MEETING INTERFACE -->
    <template v-else-if="meeting">
      <!-- RIGHT PANEL (RTL): Control Panel - Attachments + Agenda -->
      <MeetingRoomControlPanel
        :attachments="combinedAttachments"
        :agendas="agendas"
        :current-attachment="currentAttachment"
        :current-agenda-index="currentAgendaIndex"
        :can-control="canControl"
        :is-live="isLive"
        :collapsed="rightPanelCollapsed"
        :active-view-type="activeViewType"
        @select-attachment="handleSelectAttachment"
        @select-agenda="selectAgenda"
        @toggle-agenda-timer="toggleAgendaTimer"
        @complete-agenda="completeAgenda"
        @toggle-collapse="rightPanelCollapsed = !rightPanelCollapsed"
      />

      <!-- CENTER PANEL: Document Viewer + Agenda Workspace -->
      <MeetingRoomViewer
        :meeting="meeting"
        :current-attachment="currentAttachment"
        :current-attachment-url="currentAttachmentUrl"
        :current-agenda="currentAgenda"
        :meeting-elapsed-time="meetingElapsedTime"
        :can-start-meeting="canStartMeeting"
        :can-end-meeting="canEndMeeting"
        :action-loading="actionLoading"
        :agenda-recommendations="agendaRecommendations"
        :agenda-notes="agendaNotes"
        :note-text="noteText"
        :note-is-public="noteIsPublic"
        :editing-note-id="editingNoteId"
        :current-user-id="currentUserId"
        :vote-timer="voteTimer"
        :vote-status="voteStatus"
        :voting-active="votingActive"
        :can-vote="canVote"
        :can-control="canControl"
        :meeting-summary="meetingSummary"
        :agenda-summary="agendaSummary"
        :is-light-theme="isLightTheme"
        :voting-options="votingOptions"
        :user-vote-option-id="userVoteOptionId"
        :voting-loading="votingLoading"
        :all-agendas-completed="allAgendasCompleted"
        :all-agendas-voting-data="allAgendasVotingData"
        :all-agendas-data="allAgendasData"
        :active-view-type="activeViewType"
        :saved-minutes-url="savedMinutesUrl"
        :saved-minutes-file-name="savedMinutesFileName"
        :minutes-generating="minutesGenerating"
        :minutes-versions="minutesVersions"
        :current-minutes-version-id="currentMinutesVersionId"
        :loading-minutes-versions="loadingMinutesVersions"
        :regenerating-minutes="regeneratingMinutes"
        :sending-minutes-for-approval="sendingMinutesForApproval"
        :creating-final-minutes="creatingFinalMinutes"
        :minutes-approvals="minutesApprovals"
        :saved-final-minutes-url="savedFinalMinutesUrl"
        :saved-final-minutes-file-name="savedFinalMinutesFileName"
        :final-minutes-generating="finalMinutesGenerating"
        :final-minutes-versions="finalMinutesVersions"
        :current-final-minutes-version-id="currentFinalMinutesVersionId"
        :loading-final-minutes-versions="loadingFinalMinutesVersions"
        :sending-final-minutes-for-approval="sendingFinalMinutesForApproval"
        :final-approvals-count="finalMinutesApprovals.length"
        :all-final-approvals-complete="allFinalApprovalsComplete"
        :approving-final-mom="approvingFinalMom"
        :can-view-mom="canViewMom"
        :is-recording="isRecording"
        :is-paused-recording="isPausedRecording"
        :recording-duration="recordingDuration"
        :is-uploading-recording="isUploadingRecording"
        :transcripts="transcripts"
        @confirm-start="confirmStartMeeting"
        @confirm-end="confirmEndMeeting"
        @go-back="goBack"
        @toggle-agenda-timer="toggleCurrentAgendaTimer"
        @complete-agenda="completeCurrentAgenda"
        @submit-vote="submitVote"
        @update:note-text="noteText = $event"
        @update:note-is-public="(val) => noteIsPublic = val"
        @save-note="saveOrUpdateNote"
        @edit-note="startEditNote"
        @delete-note="deleteAgendaNote"
        @cancel-edit="cancelEditNote"
        @open-summary-modal="loadMeetingSummary"
        @save-summary="saveMeetingSummary"
        @open-agenda-summary-modal="loadAgendaSummaryForModal"
        @save-agenda-summary="saveAgendaSummary"
        @edit-recommendation="editRecommendation"
        @delete-recommendation="deleteRecommendation"
        @open-add-recommendation="openAddRecommendation"
        @add-recommendation-for-agenda="handleAddRecommendationForAgenda"
        @add-note-for-agenda="handleAddNoteForAgenda"
        @save-note-for-agenda="handleSaveNoteForAgenda"
        @edit-note-for-agenda="handleEditNoteForAgenda"
        @toggle-theme="isLightTheme = !isLightTheme"
        @view-voting-results="showVotingResultsModal = true"
        @upload-minutes="handleUploadMinutes"
        @generate-minutes="handleGenerateMinutes"
        @regenerate-minutes="handleRegenerateMinutes"
        @open-approval-modal="handleOpenApprovalModal"
        @select-minutes-version="selectMinutesVersion"
        @create-final-minutes="showFinalConfirmModal = true"
        @view-approvals="handleViewApprovals"
        @upload-final-minutes="handleUploadFinalMinutes"
        @generate-final-minutes="handleGenerateFinalMinutes"
        @open-final-approval-modal="handleOpenFinalApprovalModal"
        @select-final-minutes-version="selectFinalMinutesVersion"
        @view-final-approvals="handleViewFinalApprovals"
        @approve-final-mom="showFinalMomApproveConfirm = true"
        @start-recording="handleStartRecording"
        @stop-recording="handleStopRecording"
        @generate-transcript-summary="handleGenerateTranscriptSummary"
        @generate-combined-summary="handleGenerateCombinedSummary"
      />

      <!-- LEFT PANEL (RTL): Collaboration - Attendees + Chat -->
      <MeetingRoomCollaboration
        ref="collaborationRef"
        :attendees="attendees"
        :chat-messages="chatMessages"
        :chat-input-text="chatInputText"
        :present-count="presentCount"
        :can-control="canControl"
        :is-live="isLive"
        :meeting-status-id="meeting?.statusId || 0"
        :online-attendee-ids="onlineAttendeeIds"
        :collapsed="leftPanelCollapsed"
        @mark-attendance="markAttendance"
        @send-message="sendChatMessage"
        @update:chat-input-text="chatInputText = $event"
        @toggle-collapse="leftPanelCollapsed = !leftPanelCollapsed"
      />
    </template>

    <!-- ERROR STATE -->
    <MeetingRoomError v-else @go-back="goBack" />

    <!-- MODALS -->
    <MeetingRoomModals
      v-model:show-confirm-start="showConfirmStart"
      v-model:show-confirm-end="showConfirmEnd"
      @start-meeting="handleStartMeeting"
      @end-meeting="handleEndMeetingWithRecording"
    />

    <!-- Add/Edit Recommendation Modal -->
    <MeetingRoomRecommendationModal
      :show="showRecommendationModal"
      :agenda-id="currentAgenda?.id || 0"
      :recommendation="editingRecommendation"
      @close="closeRecommendationModal"
      @save="handleSaveRecommendation"
    />

    <!-- Voting Results Modal -->
    <MeetingRoomVotingResultsModal
      :show="showVotingResultsModal"
      :agenda-id="currentAgenda?.id || 0"
      :agenda-title="currentAgenda?.title || ''"
      :voting-options="votingOptions"
      :meeting-user-votes="currentAgenda?.meetingUserVotes || []"
      :all-agendas-voting-data="allAgendasVotingData"
      @close="showVotingResultsModal = false"
    />

    <!-- Minutes Approval Modal (Send for Approval) -->
    <MeetingRoomApprovalModal
      :show="showApprovalModal"
      :attendees="attendees"
      :sending="sendingMinutesForApproval"
      :pending-user-ids="minutesPendingUserIds"
      @close="showApprovalModal = false"
      @send="handleSendForApproval"
    />

    <!-- Minutes Approvals Status Modal (View Approvals - Initial MOM) -->
    <MeetingRoomApprovalsModal
      :show="showApprovalsModal && !viewingFinalApprovals"
      :loading="loadingMinutesApprovals"
      :approvals="minutesApprovals"
      :title="$t('ApprovalStatusInitialMOM')"
      @close="showApprovalsModal = false"
    />

    <!-- Minutes Approvals Status Modal (View Approvals - Final MOM) -->
    <MeetingRoomApprovalsModal
      :show="showApprovalsModal && viewingFinalApprovals"
      :loading="loadingFinalMinutesApprovals"
      :approvals="finalMinutesApprovals"
      :title="$t('SignatureStatusFinalMOM')"
      @close="showApprovalsModal = false; viewingFinalApprovals = false"
    />

    <!-- Final MOM Approval Modal (Send for Signatures) -->
    <MeetingRoomApprovalModal
      :show="showFinalApprovalModal"
      :attendees="attendees"
      :sending="sendingFinalMinutesForApproval"
      :pending-user-ids="finalMinutesPendingUserIds"
      :title="$t('SendFinalMOMForSignature')"
      :submit-label="$t('SendForSignature')"
      :show-signature-option="true"
      @close="showFinalApprovalModal = false"
      @send="handleSendFinalForApproval"
    />

    <!-- Final MOM Confirmation Modal -->
    <Modal v-model="showFinalConfirmModal" :title="$t('ApproveDirectlyBtn')" icon="mdi:check-decagram" size="md">
      <p class="text-sm text-gray-600 mb-2">{{ $t('ConfirmApproveDirectly') }}</p>
      <p class="text-xs text-amber-600">{{ $t('WillSkipApprovalPhase') }}</p>
      <template #footer>
        <button class="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-50" @click="showFinalConfirmModal = false">{{ $t('Cancel') }}</button>
        <button class="px-4 py-2 text-sm font-medium text-white rounded-lg flex items-center gap-2 disabled:opacity-50" style="background:linear-gradient(135deg,#006d4b 0%,#006d4b 100%)" :disabled="creatingFinalMinutes" @click="handleCreateFinalMinutes">
          <Icon v-if="creatingFinalMinutes" icon="mdi:loading" class="w-4 h-4 animate-spin" />
          <Icon v-else icon="mdi:check-decagram" class="w-4 h-4" />
          {{ creatingFinalMinutes ? $t('ApprovingLabel') : $t('ApproveMinutes') }}
        </button>
      </template>
    </Modal>

    <!-- Final MOM Approval Confirmation Modal -->
    <Modal v-model="showFinalMomApproveConfirm" :title="$t('ApproveFinalMOM')" icon="mdi:check-decagram" size="md">
      <p class="text-sm text-gray-600 mb-2">{{ $t('ConfirmApproveFinalMOM') }}</p>
      <p class="text-xs text-amber-600">{{ $t('AfterApprovalNoEdits') }}</p>
      <template #footer>
        <button class="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-50" @click="showFinalMomApproveConfirm = false">{{ $t('Cancel') }}</button>
        <button class="px-4 py-2 text-sm font-medium text-white rounded-lg flex items-center gap-2 disabled:opacity-50" style="background:linear-gradient(135deg,#006d4b 0%,#006d4b 100%)" :disabled="approvingFinalMom" @click="confirmApproveFinalMom">
          <Icon v-if="approvingFinalMom" icon="mdi:loading" class="w-4 h-4 animate-spin" />
          <Icon v-else icon="mdi:check-decagram" class="w-4 h-4" />
          {{ approvingFinalMom ? $t('ApprovingLabel') : $t('ConfirmApproval') }}
        </button>
      </template>
    </Modal>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import Icon from '@/components/ui/Icon.vue'
import { useMeetingRoom, provideMeetingRoomContext } from './composables'
import { useMeetingRecording } from './composables/useMeetingRecording'
import { useUserStore } from '@/stores/user'
import MeetingAgendaService from '@/services/MeetingAgendaService'
import MeetingsService from '@/services/MeetingsService'
import { useToast } from '@/composables/useToast'
import { MeetingStatusEnum } from '@/helpers/enumerations'

// Child Components
import MeetingRoomLoading from './MeetingRoomLoading.vue'
import MeetingRoomError from './MeetingRoomError.vue'
import Modal from '@/components/ui/Modal.vue'
import MeetingRoomModals from './MeetingRoomModals.vue'
import MeetingRoomControlPanel from './MeetingRoomControlPanel.vue'
import MeetingRoomViewer from './MeetingRoomViewer.vue'
import MeetingRoomCollaboration from './MeetingRoomCollaboration.vue'
import MeetingRoomRecommendationModal from './MeetingRoomRecommendationModal.vue'
import MeetingRoomVotingResultsModal from './MeetingRoomVotingResultsModal.vue'
import MeetingRoomApprovalModal from './MeetingRoomApprovalModal.vue'
import MeetingRoomApprovalsModal from './MeetingRoomApprovalsModal.vue'

// Styles
import './MeetingRoom.styles.css'

// Toast for notifications
const { toast } = useToast()
const { t, locale } = useI18n()

// Local state for final MOM confirmation
const showFinalConfirmModal = ref(false)

// Local state for Final MOM approval confirmation (when all approvals complete)
const showFinalMomApproveConfirm = ref(false)

// Local state for creating final MOM (placeholder until final MOM workflow is implemented)
const creatingFinalMinutes = ref(false)

// Final MOM state
const savedFinalMinutesUrl = ref('')
const savedFinalMinutesFileName = ref('')
const finalMinutesGenerating = ref(false)
// Final MOM versioning state
const finalMinutesVersions = ref<any[]>([])
const currentFinalMinutesVersionId = ref<number | null>(null)
const loadingFinalMinutesVersions = ref(false)

// Local state for approvals modal
const showApprovalsModal = ref(false)
const viewingFinalApprovals = ref(false)

// ═══════════════════════════════════════════════════════════════════════════
// COMPOSABLE
// ═══════════════════════════════════════════════════════════════════════════

const collaborationRef = ref<InstanceType<typeof MeetingRoomCollaboration> | null>(null)

const {
  // Core state
  loading,
  loadingText,
  actionLoading,
  meeting,
  attendees,
  agendas,
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
  isLive,
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
  meetingElapsedTime,
  remainingTime,
  isPaused,

  // SignalR
  onlineAttendeeIds,

  // Chat
  chatMessages,
  chatInputText,
  sendChatMessage,

  // Notes
  agendaNotes,
  noteText,
  noteIsPublic,
  editingNoteId,
  visibleNotes,
  saveOrUpdateNote,
  startEditNote,
  deleteAgendaNote,
  cancelEditNote,
  loadAllAgendasNotes,
  allAgendasNotesMap,

  // Recommendations
  agendaRecommendations,
  editingRecommendation,
  showRecommendationModal,
  openAddRecommendation,
  editRecommendation,
  deleteRecommendation,
  closeRecommendationModal,
  handleSaveRecommendation,

  // Voting
  votingOptions,
  votingActive,
  canVote,
  userVoteOptionId,
  votingLoading,
  submitVote,
  allAgendasVotingData,
  voteTimer,
  voteStatus,
  showVotingResultsModal,

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
  minutesGenerating,
  savedMinutesUrl,
  savedMinutesFileName,

  // Minutes versioning
  minutesVersions,
  currentMinutesVersionId,
  loadingMinutesVersions,
  regeneratingMinutes,
  sendingMinutesForApproval,
  regenerateMinutes,
  sendMinutesForApproval,
  selectMinutesVersion,

  // Minutes approval tracking
  minutesPendingUserIds,
  minutesApprovals,
  loadingMinutesApprovals,
  loadMinutesPendingUsers,
  loadMinutesApprovals,
  clearMinutesPendingUsers,
} = useMeetingRoom({ collaborationRef })

// ═══════════════════════════════════════════════════════════════════════════
// AUDIO RECORDING
// ═══════════════════════════════════════════════════════════════════════════
const {
  isRecording,
  isPaused: isPausedRecording,
  recordingDuration,
  isUploading: isUploadingRecording,
  isTranscribing,
  transcripts,
  startRecording: startRec,
  stopAndUpload,
  loadTranscripts,
  generateSummary,
  generateCombinedSummary,
} = useMeetingRecording(() => Number(meeting.value?.id || 0))

// Get current user's display name for attendee attribution
const userStore = useUserStore()
const getCurrentUserName = () => {
  return userStore.user?.fullName || userStore.user?.username || currentUserId.value || ''
}

const handleStartRecording = async () => {
  const started = await startRec()
  if (started) {
    toast.success(t('RecordingStarted'))
  } else {
    toast.error(t('MicrophoneAccessDenied'))
  }
}

const handleStopRecording = async () => {
  const lang = locale.value === 'ar' ? 'ar' : 'en'
  toast.info(t('TranscribingAudio'))
  const result = await stopAndUpload(lang, getCurrentUserName())
  if (result?.status === 'Completed') {
    toast.success(t('TranscriptionCompleted'))
  } else if (result?.status === 'Failed') {
    toast.error(result.errorMessage || t('TranscriptionFailed'))
  }
}

const handleGenerateTranscriptSummary = async (transcriptId: number) => {
  toast.info(t('GeneratingSummary'))
  const result = await generateSummary(transcriptId)
  if (result?.summaryText) {
    toast.success(t('SummaryGenerated'))
  } else {
    toast.error(t('SummaryGenerationFailed'))
  }
}

const handleGenerateCombinedSummary = async () => {
  toast.info(t('GeneratingSummary'))
  const result = await generateCombinedSummary()
  if (result?.combinedSummary) {
    toast.success(t('SummaryGenerated'))
  } else {
    toast.error(t('SummaryGenerationFailed'))
  }
}

const handleEndMeetingWithRecording = async () => {
  if (isRecording.value) {
    try {
      await handleStopRecording()
    } catch (e) {
      console.error('Failed to stop recording on meeting end:', e)
    }
  }
  handleEndMeeting()
}

// ═══════════════════════════════════════════════════════════════════════════
// COMBINED ATTACHMENTS (includes MOMs)
// ═══════════════════════════════════════════════════════════════════════════

// Track which view is active: 'attachment' | 'initialMom' | 'finalMom'
const activeViewType = ref<'attachment' | 'initialMom' | 'finalMom'>('attachment')

/**
 * Check if user can view MOMs (Minutes of Meeting)
 *
 * Rules:
 * 1. Organizer (canControl) can always view MOMs
 * 2. For council/committee meetings, check member permissions via canViewMom flag from backend
 *
 * The backend should return `canViewMom` or `canViewMinutes` flag on the meeting object
 * for council/committee members with the appropriate permissions.
 */
const canViewMom = computed(() => {
  if (!meeting.value) return false

  // Organizer can always view MOMs
  if (canControl.value) return true

  // Check if backend provides explicit MOM viewing permission
  // This is used for council/committee members with MOM permissions
  const meetingData = meeting.value as any
  if (meetingData.canViewMom !== undefined) return meetingData.canViewMom
  if (meetingData.canViewMinutes !== undefined) return meetingData.canViewMinutes

  // Default: only organizer can view MOMs
  return false
})

// Combined attachments list that includes MOMs for the control panel
const combinedAttachments = computed(() => {
  const items: any[] = []

  // Add Initial MOM if available AND user has permission to view MOMs
  if (savedMinutesUrl.value && canViewMom.value) {
    items.push({
      id: 'initial-mom',
      fileName: savedMinutesFileName.value || 'المحضر المبدئي.pdf',
      name: savedMinutesFileName.value || 'المحضر المبدئي.pdf',
      isMom: true,
      momType: 'initial',
      url: savedMinutesUrl.value
    })
  }

  // Add Final MOM if available AND user has permission to view MOMs
  if (savedFinalMinutesUrl.value && canViewMom.value) {
    items.push({
      id: 'final-mom',
      fileName: savedFinalMinutesFileName.value || 'المحضر النهائي.pdf',
      name: savedFinalMinutesFileName.value || 'المحضر النهائي.pdf',
      isMom: true,
      momType: 'final',
      url: savedFinalMinutesUrl.value
    })
  }

  // Add regular attachments
  items.push(...(allAttachments.value || []))

  return items
})

// Handle attachment selection (including MOMs)
async function handleSelectAttachment(att: any) {
  if (att.isMom) {
    // Clear regular attachment selection
    currentAttachment.value = null

    // FIRST: Refresh the MOM URL to get a fresh signed token BEFORE switching view
    // This prevents 401 errors when the signed URL expires
    try {
      if (att.momType === 'final' && currentFinalMinutesVersionId.value) {
        const urlResponse: any = await MeetingsService.getAttachmentQuery(currentFinalMinutesVersionId.value)
        const queryString = urlResponse?.data?.data || urlResponse?.data || urlResponse
        if (queryString) {
          savedFinalMinutesUrl.value = `attachments?${queryString}`
        }
      } else if (att.momType === 'initial' && currentMinutesVersionId.value) {
        const urlResponse: any = await MeetingsService.getAttachmentQuery(currentMinutesVersionId.value)
        const queryString = urlResponse?.data?.data || urlResponse?.data || urlResponse
        if (queryString) {
          savedMinutesUrl.value = `attachments?${queryString}`
        }
      }
    } catch (error) {
      console.error('Failed to refresh MOM URL:', error)
      // Continue with existing URL - it may still work
    }

    // THEN: Set the active view type (this triggers the viewer to show)
    activeViewType.value = att.momType === 'final' ? 'finalMom' : 'initialMom'
  } else {
    // Regular attachment
    activeViewType.value = 'attachment'
    selectAttachment(att)
  }
}

// ═══════════════════════════════════════════════════════════════════════════
// FINAL MOM APPROVAL STATUS
// ═══════════════════════════════════════════════════════════════════════════

// TaskStatus.Approved = 2 (from MeetingsService)
const APPROVED_STATUS = 2

/**
 * Track Final MOM approval tasks
 */
const finalMinutesApprovals = ref<any[]>([])
const loadingFinalMinutesApprovals = ref(false)

/**
 * Check if all Final MOM approval tasks are completed (all approved)
 * When true, show the "Approve Final" button
 */
const allFinalApprovalsComplete = computed(() => {
  if (!finalMinutesApprovals.value || finalMinutesApprovals.value.length === 0) {
    return false
  }
  return finalMinutesApprovals.value.every(a => a.statusId === APPROVED_STATUS)
})

/**
 * Load Final MOM approval statuses (organizer only)
 */
async function loadFinalMinutesApprovals(): Promise<void> {
  // Only organizers can access approval endpoints
  if (!meeting.value || !canControl.value) {
    return
  }
  loadingFinalMinutesApprovals.value = true
  try {
    // Use the correct endpoint for Final MOM signatures (uses meetingId)
    const response: any = await MeetingsService.getFinalMeetingMinutesSignatures(meeting.value.id)
    const approvalsData = response?.data?.data || response?.data || response || []
    finalMinutesApprovals.value = Array.isArray(approvalsData) ? approvalsData : []
  } catch (error) {
    console.error('Failed to load final minutes signatures:', error)
    finalMinutesApprovals.value = []
  } finally {
    loadingFinalMinutesApprovals.value = false
  }
}

/**
 * Clear Final MOM approvals (call when new version is generated)
 */
function clearFinalMinutesApprovals(): void {
  finalMinutesApprovals.value = []
}

// ═══════════════════════════════════════════════════════════════════════════
// LOAD FINAL MOM ON PAGE LOAD
// ═══════════════════════════════════════════════════════════════════════════

/**
 * Load Final MOM when meeting status >= 8 (PendingFinalMeetingMinutesSign)
 */
async function loadExistingFinalMom(): Promise<void> {
  if (!meeting.value) return

  // Skip if already loaded or status < 8
  if (savedFinalMinutesUrl.value) return
  if (meeting.value.statusId < MeetingStatusEnum.PendingFinalMeetingMinutesSign) return

  try {
    const response: any = await MeetingsService.getFinalMeetingMinutes(meeting.value.id)
    // Backend returns ApiResponseDto<AttachmentListItemDto>: { success, data: { id, name, ... }, message }
    const attachment = response?.data?.data || response?.data

    if (attachment?.id) {
      // Get the attachment URL for viewing
      const urlResponse: any = await MeetingsService.getAttachmentQuery(attachment.id)
      const queryString = urlResponse?.data?.data || urlResponse?.data || urlResponse

      if (queryString) {
        savedFinalMinutesUrl.value = `attachments?${queryString}`
        savedFinalMinutesFileName.value = attachment.name || attachment.fileName || `محضر-نهائي-${meeting.value.id}.pdf`
        currentFinalMinutesVersionId.value = attachment.id

        // Also load versions (only for organizers)
        if (canControl.value) {
          await loadFinalMinutesVersions()
          // Load Final MOM approvals to check if all are complete
          await loadFinalMinutesApprovals()
        }
      }
    }
  } catch (error) {
    // This is expected if no final MOM exists yet
  }
}

// Watch for meeting loaded and load Final MOM if status >= 8
watch(
  () => meeting.value?.statusId,
  (statusId) => {
    if (statusId && statusId >= MeetingStatusEnum.PendingFinalMeetingMinutesSign) {
      loadExistingFinalMom()
    }
  },
  { immediate: true }
)

// Load transcripts when meeting is available
watch(
  () => meeting.value?.id,
  (id) => { if (id) loadTranscripts() },
  { immediate: true }
)

// ═══════════════════════════════════════════════════════════════════════════
// PROVIDE CONTEXT
// ═══════════════════════════════════════════════════════════════════════════

// Provide context for child components that want to use it
provideMeetingRoomContext({
  // Core meeting data
  meeting,
  agendas,
  attendees,
  attachments: computed(() => []) as any, // Use allAttachments instead

  // Current state
  currentAgenda,
  currentAgendaIndex,
  currentAttachment,
  currentAttachmentUrl,

  // Status flags
  isLive,
  meetingHasEnded: computed(() => false) as any, // Not used by children
  canControl,
  canStartMeeting,
  canEndMeeting,
  allAgendasCompleted,

  // Timers
  meetingElapsedTime,
  remainingTime,
  isPaused,

  // Notes
  agendaNotes,
  noteText,
  noteIsPublic,
  editingNoteId,
  visibleNotes,

  // Recommendations
  agendaRecommendations,
  editingRecommendation,

  // Voting
  votingOptions,
  votingActive,
  canVote,
  userVoteOptionId,
  votingLoading,

  // Summary data
  meetingSummary,
  agendaSummary,
  allAgendasVotingData,
  allAgendasData,

  // Chat
  chatMessages,
  chatInputText,

  // Attendees status
  presentCount,
  onlineAttendeeIds,

  // All attachments combined
  allAttachments,

  // Actions
  selectAgenda,
  selectAttachment,
  toggleAgendaTimer,
  completeAgenda,
  markAttendance,

  // Notes actions
  saveOrUpdateNote,
  startEditNote,
  deleteAgendaNote,
  cancelEditNote,

  // Recommendations actions
  openAddRecommendation,
  editRecommendation,
  deleteRecommendation,

  // Voting actions
  submitVote,

  // Chat actions
  sendChatMessage,

  // Summary actions
  loadMeetingSummary,
  saveMeetingSummary,
  loadAgendaSummary,
  saveAgendaSummary,

  // Current user
  currentUserId,

  // Loading states
  loading,
  actionLoading,

  // Legacy computed
  voteTimer,
  voteStatus,
})

// ═══════════════════════════════════════════════════════════════════════════
// LOCAL HELPERS
// ═══════════════════════════════════════════════════════════════════════════

function loadAgendaSummaryForModal(): void {
  const agenda = agendas.value[currentAgendaIndex.value]
  if (agenda?.id) {
    loadAgendaSummary(agenda.id)
  }
}

async function handleSendForApproval(userIds: string[], dueDateDays?: number): Promise<void> {
  const success = await sendMinutesForApproval(userIds, dueDateDays)
  if (success) {
    showApprovalModal.value = false
    // Reload pending users after sending
    await loadMinutesPendingUsers()
  }
}

async function handleOpenApprovalModal(): Promise<void> {
  // Load pending users before showing modal
  await loadMinutesPendingUsers()
  showApprovalModal.value = true
}

async function handleRegenerateMinutes(): Promise<void> {
  // Clear pending users since we're creating a new version
  clearMinutesPendingUsers()
  // Regenerate minutes (this creates a new version)
  await regenerateMinutes()
}

async function handleViewApprovals(): Promise<void> {
  // Show modal immediately (with loading state), then load data
  showApprovalsModal.value = true
  await loadMinutesApprovals()
}

async function handleCreateFinalMinutes(): Promise<void> {
  if (!meeting.value) return

  creatingFinalMinutes.value = true
  try {
    // Call API to approve Initial MOM directly (bypass approval workflow)
    const response: any = await MeetingsService.approveInitialMeetingMinutes(meeting.value.id)

    if (response?.data?.success !== false && response?.success !== false) {
      // Update meeting status to InitialMeetingMinutesApproved (7)
      meeting.value.statusId = 7
      showFinalConfirmModal.value = false
      // Switch view so the final MOM buttons (upload/generate) become visible
      activeViewType.value = 'attachment'
      toast.success(t('InitialMinutesApprovedSuccess'))
    } else {
      toast.error(response?.data?.message || response?.message || t('MinutesApproveFailed'))
    }
  } catch (error: any) {
    console.error('Failed to approve initial minutes:', error)
    toast.error(error?.response?.data?.message || t('InitialMinutesApproveFailed'))
  } finally {
    creatingFinalMinutes.value = false
  }
}

// ═══════════════════════════════════════════════════════════════════════════
// FINAL MOM HANDLERS
// ═══════════════════════════════════════════════════════════════════════════

// Hidden file input ref for Final MOM upload
const finalMinutesFileInput = ref<HTMLInputElement | null>(null)

function handleUploadFinalMinutes(): void {
  // Create and trigger file input for upload
  const input = document.createElement('input')
  input.type = 'file'
  input.accept = '.pdf'
  input.onchange = async (e) => {
    const file = (e.target as HTMLInputElement).files?.[0]
    if (!file || !meeting.value) return

    finalMinutesGenerating.value = true
    try {
      const response: any = await MeetingsService.uploadFinalMeetingMinutes(meeting.value.id, file)

      // Backend returns ApiResponseDto<AttachmentListItemDto>: { success, data: { id, name, version, ... }, message }
      const success = response?.data?.success ?? response?.success
      const attachment = response?.data?.data || response?.data

      if (success && attachment?.id) {
        const attachmentId = attachment.id
        const fileName = attachment.name || file.name

        // Get the attachment URL for viewing
        const urlResponse: any = await MeetingsService.getAttachmentQuery(attachmentId)
        const queryString = urlResponse?.data?.data || urlResponse?.data || urlResponse

        if (queryString) {
          savedFinalMinutesUrl.value = `attachments?${queryString}`
          savedFinalMinutesFileName.value = fileName
          currentFinalMinutesVersionId.value = attachmentId

          // Update meeting status if first version
          if (attachment.version === 1 && meeting.value) {
            meeting.value.statusId = MeetingStatusEnum.PendingFinalMeetingMinutesSign
          }

          // Load versions
          await loadFinalMinutesVersions()

          toast.success(t('FinalMinutesUploadedSuccess'))
        }
      } else {
        toast.error(response?.data?.message || response?.message || t('FinalMinutesUploadFailed'))
      }
    } catch (error) {
      console.error('Failed to upload final minutes:', error)
      toast.error(t('FinalMinutesUploadFailed'))
    } finally {
      finalMinutesGenerating.value = false
    }
  }
  input.click()
}

async function handleGenerateFinalMinutes(): Promise<void> {
  if (!meeting.value) return

  finalMinutesGenerating.value = true
  try {
    const response: any = await MeetingsService.generateFinalMeetingMinutes(meeting.value.id)

    // Backend returns ApiResponseDto<AttachmentListItemDto>: { success, data: { id, name, version, ... }, message }
    const success = response?.data?.success ?? response?.success
    const attachment = response?.data?.data || response?.data

    if (success && attachment?.id) {
      const attachmentId = attachment.id
      const fileName = attachment.name || `محضر-نهائي-${meeting.value.id}.pdf`

      // Get the attachment URL for viewing
      const urlResponse: any = await MeetingsService.getAttachmentQuery(attachmentId)
      const queryString = urlResponse?.data?.data || urlResponse?.data || urlResponse

      if (queryString) {
        savedFinalMinutesUrl.value = `attachments?${queryString}`
        savedFinalMinutesFileName.value = fileName
        currentFinalMinutesVersionId.value = attachmentId

        // Update meeting status if first version (backend sets status to PendingFinalMeetingMinutesSign)
        if (attachment.version === 1 && meeting.value) {
          meeting.value.statusId = MeetingStatusEnum.PendingFinalMeetingMinutesSign
        }

        // Load versions
        await loadFinalMinutesVersions()

        // Clear approvals when a new version is generated
        clearFinalMinutesApprovals()

        toast.success(t('FinalMinutesCreatedSuccess'))
      }
    } else {
      toast.error(response?.data?.message || response?.message || t('FinalMinutesCreateFailed'))
    }
  } catch (error) {
    console.error('Failed to generate final minutes:', error)
    toast.error(t('FinalMinutesCreateFailed'))
  } finally {
    finalMinutesGenerating.value = false
  }
}

// Final MOM Approval Modal state
const showFinalApprovalModal = ref(false)
const sendingFinalMinutesForApproval = ref(false)
const finalMinutesPendingUserIds = ref<number[]>([])

/**
 * Load users who already have pending signature tasks for the current Final MOM version
 */
async function loadFinalMinutesPendingUsers(): Promise<void> {
  if (!meeting.value) return

  try {
    const response: any = await MeetingsService.getFinalMeetingMinutesUsers(meeting.value.id)
    const userIds = response?.data?.data || response?.data || response || []
    finalMinutesPendingUserIds.value = Array.isArray(userIds) ? userIds : []
  } catch (error) {
    console.error('Failed to load final minutes pending users:', error)
    finalMinutesPendingUserIds.value = []
  }
}

async function handleOpenFinalApprovalModal(): Promise<void> {
  // Load Final MOM pending users before showing modal
  await loadFinalMinutesPendingUsers()
  showFinalApprovalModal.value = true
}

async function handleSendFinalForApproval(userIds: string[], dueDateDays?: number, signatureUserIds?: string[]): Promise<void> {
  if (!meeting.value) return

  sendingFinalMinutesForApproval.value = true
  try {
    const response: any = await MeetingsService.sendFinalMeetingMinutesForApproval(meeting.value.id, userIds, dueDateDays, signatureUserIds)

    if (response?.data?.success !== false && response?.success !== false) {
      toast.success(t('FinalMinutesSentForApprovalSuccess'))
      showFinalApprovalModal.value = false

      // Update meeting status if returned
      const newStatusId = response?.data?.newMeetingStatusId || response?.newMeetingStatusId
      if (newStatusId && meeting.value) {
        meeting.value.statusId = newStatusId
      }

      // Reload pending users and approvals
      await loadFinalMinutesPendingUsers()
      await loadFinalMinutesApprovals()
    } else {
      toast.error(response?.data?.message || response?.message || t('FinalMinutesSendForApprovalFailed'))
    }
  } catch (error: any) {
    console.error('Failed to send final minutes for approval:', error)
    toast.error(error?.response?.data?.message || t('FinalMinutesSendForApprovalFailed'))
  } finally {
    sendingFinalMinutesForApproval.value = false
  }
}

/**
 * Load Final MOM versions
 */
async function loadFinalMinutesVersions(): Promise<void> {
  if (!meeting.value) return

  loadingFinalMinutesVersions.value = true
  try {
    const response: any = await MeetingsService.getFinalMeetingMinutesVersions(meeting.value.id)
    const versionsData = response?.data || response || []

    finalMinutesVersions.value = versionsData.map((v: any) => ({
      id: v.id || v.Id,
      version: v.version || v.Version || 1,
      status: v.status || v.Status || 'draft',
      createdAt: v.createdAt || v.createdDate || v.CreatedAt || v.CreatedDate,
      fileName: v.fileName || v.FileName,
      attachmentId: v.attachmentId || v.AttachmentId || v.id || v.Id
    }))

    // Always set current version to the latest and update URL
    if (finalMinutesVersions.value.length > 0) {
      const latest = finalMinutesVersions.value.reduce((a: any, b: any) => (a.version > b.version ? a : b))
      currentFinalMinutesVersionId.value = latest.id

      // Also update the PDF URL to show the latest version
      try {
        const attachmentId = latest.attachmentId || latest.id
        const urlResponse: any = await MeetingsService.getAttachmentQuery(attachmentId)
        const queryString = urlResponse?.data?.data || urlResponse?.data || urlResponse
        if (queryString) {
          savedFinalMinutesUrl.value = `attachments?${queryString}`
          savedFinalMinutesFileName.value = latest.fileName || `محضر-نهائي-v${latest.version}.pdf`
        }
      } catch (urlError) {
        console.error('Failed to get latest version URL:', urlError)
      }
    }
  } catch (error) {
    console.error('Failed to load final minutes versions:', error)
    finalMinutesVersions.value = []
  } finally {
    loadingFinalMinutesVersions.value = false
  }
}

/**
 * Select a specific Final MOM version
 */
async function selectFinalMinutesVersion(versionId: number): Promise<void> {
  const version = finalMinutesVersions.value.find(v => v.id === versionId)
  if (!version) return

  currentFinalMinutesVersionId.value = versionId

  try {
    const attachmentId = version.attachmentId || version.id
    const urlResponse: any = await MeetingsService.getAttachmentQuery(attachmentId)
    const queryString = urlResponse?.data?.data || urlResponse?.data || urlResponse

    if (queryString) {
      savedFinalMinutesUrl.value = `attachments?${queryString}`
      savedFinalMinutesFileName.value = version.fileName || `محضر-نهائي-v${version.version}.pdf`
    }
  } catch (error) {
    console.error('Failed to load final minutes version:', error)
    toast.error(t('MinutesVersionLoadFailed'))
  }
}

/**
 * View Final MOM approvals
 */
async function handleViewFinalApprovals(): Promise<void> {
  viewingFinalApprovals.value = true
  showApprovalsModal.value = true
  await loadFinalMinutesApprovals()
}

/**
 * Approve Final MOM (called when all approval tasks are completed)
 */
const approvingFinalMom = ref(false)

async function handleApproveFinalMom(): Promise<void> {
  if (!meeting.value) return

  approvingFinalMom.value = true
  try {
    const response: any = await MeetingsService.approveFinalMeetingMinutes(meeting.value.id)

    if (response?.data?.success !== false && response?.success !== false) {
      toast.success(t('FinalMinutesApprovedSuccess'))

      // Update meeting status if returned
      const newStatusId = response?.data?.newMeetingStatusId || response?.newMeetingStatusId
      if (newStatusId && meeting.value) {
        meeting.value.statusId = newStatusId
      }
    } else {
      toast.error(response?.data?.message || response?.message || t('FinalMinutesApproveFailed'))
    }
  } catch (error: any) {
    console.error('Failed to approve final MOM:', error)
    toast.error(error?.response?.data?.message || t('FinalMinutesApproveFailed'))
  } finally {
    approvingFinalMom.value = false
  }
}

/**
 * Confirm and approve Final MOM (called from confirmation modal)
 */
async function confirmApproveFinalMom(): Promise<void> {
  await handleApproveFinalMom()
  showFinalMomApproveConfirm.value = false
}

function handleAddRecommendationForAgenda(agendaId: number): void {
  // Find the agenda index
  const agendaIndex = agendas.value.findIndex(a => a.id === agendaId)
  if (agendaIndex >= 0) {
    // Select the agenda first
    selectAgenda(agendaIndex)
    // Then open the add recommendation modal
    openAddRecommendation()
  }
}

function handleAddNoteForAgenda(agendaId: number): void {
  // Find the agenda index
  const agendaIndex = agendas.value.findIndex(a => a.id === agendaId)
  if (agendaIndex >= 0) {
    // Select the agenda first
    selectAgenda(agendaIndex)
    // Reset note form to add mode
    cancelEditNote()
    noteText.value = ''
    noteIsPublic.value = true
    // Focus happens automatically in the workspace
  }
}

async function handleSaveNoteForAgenda(agendaId: number, text: string, isPublic: boolean): Promise<void> {
  try {
    await MeetingAgendaService.addNote({
      meetingAgendaId: agendaId,
      text,
      isPublic
    })
    toast.success(t('NoteAddedSuccess'))
    // Reload all agendas notes to update the map
    await loadAllAgendasNotes()
  } catch (error) {
    console.error('Failed to add note:', error)
    toast.error(t('NoteAddFailed'))
  }
}

async function handleEditNoteForAgenda(noteId: number, agendaId: number, text: string, isPublic: boolean): Promise<void> {
  try {
    await MeetingAgendaService.editNote({
      id: noteId,
      meetingAgendaId: agendaId,
      text,
      isPublic
    })
    toast.success(t('NoteUpdatedSuccess'))
    // Reload all agendas notes to update the map
    await loadAllAgendasNotes()
  } catch (error) {
    console.error('Failed to edit note:', error)
    toast.error(t('NoteUpdateFailed'))
  }
}
</script>

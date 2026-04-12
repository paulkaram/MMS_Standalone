<template>
  <div class="h-screen bg-zinc-100 flex flex-col">
    <!-- Header -->
    <div class="bg-primary text-white px-4 py-3 flex items-center justify-between">
      <h1 class="text-lg font-semibold truncate" :title="meetingName">
        {{ truncate(meetingName, 120) }}
      </h1>
      <button
        type="button"
        class="p-2 hover:bg-white/10 rounded-full"
        @click="close"
      >
        <Icon icon="mdi:close" class="w-5 h-5" />
      </button>
    </div>

    <!-- Main Content -->
    <div class="flex-1 overflow-hidden p-4">
      <!-- Loading -->
      <div v-if="loading" class="h-full flex items-center justify-center">
        <div class="text-center">
          <Icon icon="mdi:loading" class="w-16 h-16 text-primary animate-spin mx-auto" />
          <p class="mt-4 text-zinc-600">{{ loadingText }}</p>
        </div>
      </div>

      <div v-else class="h-full grid grid-cols-12 gap-4">
        <!-- Agenda Sidebar -->
        <div class="col-span-2">
          <MeetingDashboardAgenda
            v-if="meetingDetails"
            :meeting-id="meeting.id"
            :meeting-owner="meetingOwner"
            :attendees="attendees"
            :status-id="meetingDetails.statusId"
            :view-mode="viewMode"
            :agenda-list="agendaItems"
            @start-next-agenda="startNextMeetingAgenda"
            @update="updateAgendaList"
          />
        </div>

        <!-- Main Area -->
        <div class="col-span-10 flex flex-col gap-4">
          <!-- Attendees Bar -->
          <MeetingDashboardAttendees
            v-if="showAttendees"
            :attendees="attendees"
            :current-attendance-list="currentAttendanceList"
          />

          <div class="flex-1 grid grid-cols-12 gap-4">
            <!-- File Viewer Area -->
            <div class="col-span-9">
              <Card class="h-full">
                <!-- No file selected -->
                <div
                  v-if="!selectedFile && !showInitialMeetingMinutes && !showFinalMeetingMinutes"
                  class="h-full flex items-center justify-center text-zinc-400"
                >
                  <div class="text-center">
                    <Icon icon="mdi:file-document-outline" class="w-20 h-20 mx-auto mb-4" />
                    <p>{{ $t('FilePlaceHolder') }}</p>
                  </div>
                </div>

                <!-- File loading -->
                <div
                  v-else-if="fileLoading"
                  class="h-full flex items-center justify-center"
                >
                  <Icon icon="mdi:loading" class="w-12 h-12 text-primary animate-spin" />
                </div>

                <!-- File viewer -->
                <div v-else-if="selectedFileData" class="h-full">
                  <FileViewer :query="selectedFileData" name="meetingFile" />
                </div>

                <!-- Error loading file -->
                <div
                  v-else-if="selectedFile && !selectedFileData"
                  class="h-full flex items-center justify-center text-zinc-500"
                >
                  <div class="text-center">
                    <Icon icon="mdi:file-alert" class="w-16 h-16 mx-auto mb-4 text-zinc-300" />
                    <p>{{ $t('ErrorLoadingFile') }}</p>
                  </div>
                </div>
              </Card>
            </div>

            <!-- Right Sidebar (Attachments + Chat) -->
            <div class="col-span-3 flex flex-col gap-4">
              <!-- Attachments -->
              <MeetingDashboardAttachments
                v-if="meetingDetails"
                :meeting-id="meeting.id"
                :agenda-items="agendaItems"
                :selected-file="selectedFile"
                :meeting-owner="meetingOwner"
                :status-id="meetingDetails.statusId"
                :view-mode="viewMode"
                @load-attachment="loadAttachment"
                @close-file="closeOpenedFile"
              />

              <!-- Chat -->
              <MeetingDashboardChat
                :meeting-id="meeting.id"
                :view-mode="viewMode"
                :expanded="chatExpanded"
                @toggle-expand="toggleChatExpand"
              />
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Footer Actions -->
    <div
      v-if="meetingDetails"
      class="bg-zinc-200 px-4 py-3 flex items-center gap-3"
    >
      <!-- Attendees Toggle -->
      <button
        type="button"
        :class="[
          'w-10 h-10 rounded-full flex items-center justify-center transition-colors',
          showAttendees ? 'bg-primary text-white' : 'bg-white text-zinc-700 hover:bg-zinc-100'
        ]"
        @click="showAttendees = !showAttendees"
      >
        <Icon icon="mdi:account-multiple" class="w-5 h-5" />
      </button>

      <!-- Attendees Status List -->
      <div class="relative">
        <button
          type="button"
          :class="[
            'w-10 h-10 rounded-full flex items-center justify-center transition-colors',
            showAttendeesStatusMenu ? 'bg-primary text-white' : 'bg-white text-zinc-700 hover:bg-zinc-100'
          ]"
          @click="showAttendeesStatusMenu = !showAttendeesStatusMenu"
        >
          <Icon icon="mdi:account-group" class="w-5 h-5" />
        </button>
        <MeetingDashboardAttendeesStatus
          v-if="showAttendeesStatusMenu"
          :meeting-id="meeting.id"
          :meeting-owner="meetingOwner"
          :attendees="attendees"
          :current-attendance-list="currentAttendanceList"
          class="absolute bottom-full mb-2 left-0 z-50"
          @close="showAttendeesStatusMenu = false"
        />
      </div>

      <!-- Info Menu -->
      <div class="relative">
        <button
          type="button"
          :class="[
            'w-10 h-10 rounded-full flex items-center justify-center transition-colors',
            showInfoMenu ? 'bg-primary text-white' : 'bg-white text-zinc-700 hover:bg-zinc-100'
          ]"
          @click="showInfoMenu = !showInfoMenu"
        >
          <Icon icon="mdi:information-variant" class="w-5 h-5" />
        </button>
        <MeetingDashboardInfo
          v-if="showInfoMenu"
          :meeting-details="meetingDetails"
          class="absolute bottom-full mb-2 left-0 z-50"
          @close="showInfoMenu = false"
        />
      </div>

      <div class="flex-1" />

      <!-- Start Meeting Button -->
      <Button
        v-if="canStartMeeting"
        variant="primary"
        icon-left="mdi:power"
        @click="startMeeting"
      >
        {{ $t('StartMeeting') }}
      </Button>

      <!-- End Meeting Button -->
      <Button
        v-if="canEndMeeting"
        variant="danger"
        :loading="endLoading"
        @click="finishMeeting"
      >
        {{ $t('EndMeeting') }}
      </Button>
    </div>

    <!-- Confirm Dialog -->
    <ConfirmModal
      v-model="confirmDialog"
      :title="confirmTitle"
      :message="confirmMessage"
      @confirm="confirmAction"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'
import Button from '@/components/ui/Button.vue'
import ConfirmModal from '@/components/ui/ConfirmModal.vue'
import MeetingDashboardAgenda from './MeetingDashboardAgenda.vue'
import MeetingDashboardAttendees from './MeetingDashboardAttendees.vue'
import MeetingDashboardAttachments from './MeetingDashboardAttachments.vue'
import MeetingDashboardChat from './MeetingDashboardChat.vue'
import MeetingDashboardAttendeesStatus from './MeetingDashboardAttendeesStatus.vue'
import MeetingDashboardInfo from './MeetingDashboardInfo.vue'
import FileViewer from '@/components/ui/FileViewer.vue'
import MeetingsService from '@/services/MeetingsService'
import AttachmentsService from '@/services/AttachmentsService'
import { useSignalR } from '@/composables/useSignalR'
import { useUserStore } from '@/stores/user'

const MeetingStatusEnum = {
  Draft: 1,
  PendingMeetingApproval: 2,
  Approved: 3,
  Started: 4,
  Finished: 5,
  PendingInitialMeetingMinutesApproval: 6,
  InitialMeetingMinutesApproved: 7,
  PendingFinalMeetingMinutesSign: 8,
  FinalMeetingMinutesSigned: 9,
  Canceled: 10
}

const props = defineProps<{
  meeting: {
    id: number
    name?: string
    title?: string
  }
  viewMode?: boolean
}>()

const emit = defineEmits(['close'])

const userStore = useUserStore()
const { connect, on, off, invoke, isConnected } = useSignalR()

// State
const loading = ref(false)
const loadingText = ref('')
const endLoading = ref(false)
const meetingName = ref('')
const meetingDetails = ref<any>(null)
const attendees = ref<any[]>([])
const agendaItems = ref<any[]>([])
const attachments = ref<any[]>([])
const meetingOwner = ref(false)
const currentAttendanceList = ref<string[]>([])

// UI State
const showAttendees = ref(true)
const showAttendeesStatusMenu = ref(false)
const showInfoMenu = ref(false)
const chatExpanded = ref(true)
const selectedFile = ref<string | null>(null)
const fileLoading = ref(false)
const selectedFileData = ref<any>(null)

// Confirm Dialog
const confirmDialog = ref(false)
const confirmTitle = ref('')
const confirmMessage = ref('')
const pendingAction = ref<(() => void) | null>(null)

// Computed
const canStartMeeting = computed(() => {
  return (
    !props.viewMode &&
    meetingOwner.value &&
    agendaItems.value.length > 0 &&
    !agendaItems.value[0].actualStartDate
  )
})

const canEndMeeting = computed(() => {
  return (
    meetingOwner.value &&
    meetingDetails.value?.statusId === MeetingStatusEnum.Started
  )
})

const showInitialMeetingMinutes = computed(() => {
  return (
    !selectedFile.value &&
    meetingOwner.value &&
    meetingDetails.value &&
    (meetingDetails.value.statusId === MeetingStatusEnum.PendingInitialMeetingMinutesApproval ||
      meetingDetails.value.statusId === MeetingStatusEnum.InitialMeetingMinutesApproved)
  )
})

const showFinalMeetingMinutes = computed(() => {
  return (
    !selectedFile.value &&
    meetingOwner.value &&
    meetingDetails.value &&
    (meetingDetails.value.statusId === MeetingStatusEnum.PendingFinalMeetingMinutesSign ||
      meetingDetails.value.statusId === MeetingStatusEnum.FinalMeetingMinutesSigned)
  )
})

// Methods
const loadMeetingDetails = async () => {
  loading.value = true
  loadingText.value = 'جاري تحميل تفاصيل الاجتماع...'

  try {
    const result = await MeetingsService.loadLiveMeeting(props.meeting.id)
    const data = result.data || result

    meetingDetails.value = data
    attachments.value = data.attachments || []
    attendees.value = data.meetingAttendees || []
    agendaItems.value = data.meetingAgendas || []
    meetingOwner.value = data.createdby === userStore.user?.id
  } catch (error) {
    console.error('Failed to load meeting details:', error)
    close()
  } finally {
    loading.value = false
  }
}

const setupSignalR = async () => {
  if (props.viewMode) return

  try {
    await connect()

    if (isConnected.value) {
      // Notify attendance
      await invoke('ChangeMeetingAttendanceStatus', props.meeting.id, true)

      // Subscribe to events
      on('NotifyMeetingAttendanceChange', handleAttendanceChange)
      on(`MeetingStatusChange${props.meeting.id}`, handleStatusChange)
      on(`MeetingAgendaChanges${props.meeting.id}`, handleAgendaChanges)
    }
  } catch (error) {
    console.error('SignalR connection failed:', error)
  }
}

const cleanupSignalR = async () => {
  if (props.viewMode) return

  try {
    if (isConnected.value) {
      await invoke('ChangeMeetingAttendanceStatus', props.meeting.id, false)
    }

    off('NotifyMeetingAttendanceChange')
    off(`MeetingStatusChange${props.meeting.id}`)
    off(`MeetingAgendaChanges${props.meeting.id}`)
  } catch (error) {
    console.error('SignalR cleanup error:', error)
  }
}

const handleAttendanceChange = (meetingId: number, attendanceIds: string[]) => {
  if (meetingId === props.meeting.id) {
    currentAttendanceList.value = attendanceIds
  }
}

const handleStatusChange = (newStatus: number) => {
  if (newStatus && meetingDetails.value) {
    meetingDetails.value.statusId = newStatus
  }
}

const handleAgendaChanges = (newAgendaItems: any[]) => {
  if (newAgendaItems?.length > 0 && newAgendaItems[0].meetingId === props.meeting.id) {
    agendaItems.value = newAgendaItems
  }
}

const startMeeting = () => {
  confirmTitle.value = 'بدء الاجتماع'
  confirmMessage.value = 'هل أنت متأكد من بدء الاجتماع؟'
  pendingAction.value = () => startNextMeetingAgenda()
  confirmDialog.value = true
}

const finishMeeting = () => {
  confirmTitle.value = 'إنهاء الاجتماع'
  confirmMessage.value = 'هل أنت متأكد من إنهاء الاجتماع؟'
  pendingAction.value = async () => {
    endLoading.value = true
    try {
      await MeetingsService.endMeeting(props.meeting.id)
    } catch (error) {
      console.error('Failed to end meeting:', error)
    } finally {
      endLoading.value = false
    }
  }
  confirmDialog.value = true
}

const confirmAction = () => {
  if (pendingAction.value) {
    pendingAction.value()
    pendingAction.value = null
  }
}

const startNextMeetingAgenda = async () => {
  loading.value = true
  try {
    await MeetingsService.nextAgenda(props.meeting.id)
  } catch (error) {
    console.error('Failed to start next agenda:', error)
  } finally {
    loading.value = false
  }
}

const updateAgendaList = (newAgendaList: any[]) => {
  agendaItems.value = newAgendaList
}

const loadAttachment = async (attachmentId: string) => {
  selectedFile.value = attachmentId
  fileLoading.value = true
  selectedFileData.value = null

  try {
    const response = await AttachmentsService.getAttachment(attachmentId)
    selectedFileData.value = response
  } catch (error) {
    console.error('Failed to load attachment:', error)
    selectedFileData.value = null
  } finally {
    fileLoading.value = false
  }
}

const closeOpenedFile = () => {
  selectedFile.value = null
  selectedFileData.value = null
}

const toggleChatExpand = () => {
  chatExpanded.value = !chatExpanded.value
}

const close = () => {
  emit('close')
}

const truncate = (text: string, length: number) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}

// Lifecycle
onMounted(async () => {
  if (props.meeting) {
    meetingName.value = props.meeting.name || props.meeting.title || ''
    await loadMeetingDetails()
    await setupSignalR()
  } else {
    close()
  }
})

onUnmounted(() => {
  cleanupSignalR()
})
</script>

<template>
  <div class="tasks-page">
      <!-- Left Panel - Tasks List -->
      <aside class="tasks-sidebar">
        <!-- Loading State -->
        <div v-if="loading" class="sidebar-loading">
          <div class="spinner"></div>
        </div>

        <!-- Tasks List -->
        <div v-else class="tasks-scroll">
          <div
            v-for="task in tasks"
            :key="task.id"
            class="task-card"
            :class="{
              'active': selectedTaskId === task.id,
              'is-delayed': task.isDelayed
            }"
            @click="showTaskData(task)"
          >
            <!-- Task Type Badge with Status Indicator -->
            <div class="task-type-wrapper">
              <div class="task-type-badge" :class="getTaskTypeClass(task.typeId)">
                <svg v-if="task.typeId === 1" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" stroke-linecap="round" stroke-linejoin="round"/>
                </svg>
                <svg v-else-if="task.typeId === 2" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" stroke-linecap="round" stroke-linejoin="round"/>
                </svg>
                <svg v-else-if="task.typeId === 3" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" stroke-linecap="round" stroke-linejoin="round"/>
                </svg>
                <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" stroke-linecap="round" stroke-linejoin="round"/>
                </svg>
              </div>
              <!-- Status Indicator -->
              <span class="status-indicator" :class="task.isDelayed ? 'delayed' : 'on-time'"></span>
            </div>

            <!-- Task Content -->
            <div class="task-content">
              <span class="task-type-label">{{ task.type }}</span>
              <h4 class="task-title">{{ task.meetingTitle }}</h4>

              <div class="task-meta">
                <span class="task-ref">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M7 20l4-16m2 16l4-16M6 9h14M4 15h14" stroke-linecap="round" stroke-linejoin="round"/>
                  </svg>
                  {{ task.meetingReference }}
                </span>

                <span v-if="task.claimed" class="task-claimed-badge">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" stroke-linecap="round" stroke-linejoin="round"/>
                    <path d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" stroke-linecap="round" stroke-linejoin="round"/>
                  </svg>
                  {{ $t('Viewed') }}
                </span>
              </div>
            </div>

            <!-- Arrow -->
            <div class="task-arrow">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M9 18l6-6-6-6" stroke-linecap="round" stroke-linejoin="round"/>
              </svg>
            </div>
          </div>

          <!-- Empty State -->
          <div v-if="tasks.length === 0" class="empty-list">
            <div class="empty-icon-wrapper">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <path d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" stroke-linecap="round" stroke-linejoin="round"/>
              </svg>
            </div>
            <h4>{{ $t('NoTasks') }}</h4>
            <p>{{ $t('AllTasksCompleted') }}</p>
          </div>
        </div>

        <!-- Pagination -->
        <footer v-if="totalPages > 1" class="sidebar-footer">
          <button :disabled="page === 1" @click="handlePageChange(page - 1)">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
              <path d="M9 18l6-6-6-6"/>
            </svg>
          </button>
          <span class="page-info">{{ page }} / {{ totalPages }}</span>
          <button :disabled="page === totalPages" @click="handlePageChange(page + 1)">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
              <path d="M15 18l-6-6 6-6"/>
            </svg>
          </button>
        </footer>
      </aside>

      <!-- Main Content - Task Details -->
    <main class="task-main">
      <!-- Loading State -->
      <div v-if="loadingDetails" class="main-loading">
        <div class="spinner large"></div>
      </div>

      <!-- Task Details -->
      <template v-else-if="showDetails && stepperItems.length > 0">
        <!-- Stepper Navigation -->
        <nav class="stepper-nav">
          <button
            v-for="(step, index) in stepperItems"
            :key="step.id"
            class="step-btn"
            :class="{
              'active': currentStep === index + 1,
              'completed': currentStep > index + 1
            }"
            @click="setCurrentStep(index + 1)"
          >
            <span class="step-indicator">
              <svg v-if="currentStep > index + 1" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
                <path d="M20 6L9 17l-5-5"/>
              </svg>
              <span v-else>{{ index + 1 }}</span>
            </span>
            <span class="step-label">{{ step.title }}</span>
          </button>
        </nav>

        <!-- Content Area -->
        <section class="content-area">
          <!-- PDF Viewer for all document tasks (including Final MOM) -->
          <div v-if="stepperItems[currentStep - 1]?.component === 'FileViewer'" class="content-scroll pdf-viewer-container">
            <div class="pdf-viewer-wrapper">
              <!-- Use iframe directly for Final MOM with signing -->
              <div v-if="isFinalMomTask && pdfBlobUrl" class="pdf-sign-viewer">
                <iframe
                  ref="signPdfIframe"
                  :src="signViewerUrl"
                  class="pdf-sign-iframe"
                />
              </div>
              <!-- Regular PDF viewer for non-signing tasks -->
              <PdfViewer
                v-else-if="documentQuery?.url"
                :src="documentQuery.url"
                :file-name="documentQuery.name || 'MeetingMinutes.pdf'"
              />
            </div>

            <!-- Saving indicator for Final MOM tasks -->
            <div v-if="isFinalMomTask && savingSignature" class="saving-indicator">
              <div class="spinner small"></div>
              <span>{{ $t('SavingSignature') }}</span>
            </div>

            <!-- Signature saved - prompt to approve -->
            <div v-else-if="isFinalMomTask && signatureSaved" class="signed-indicator success">
              <svg class="signed-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M20 6L9 17l-5-5"/>
              </svg>
              <span>{{ $t('SignatureSavedApproveToFinish') }}</span>
              <button class="unsign-btn" @click="handleUnsign" :disabled="unsigning">
                <span v-if="unsigning">{{ $t('RemovingSignature') }}</span>
                <span v-else>{{ $t('RemoveSignature') }}</span>
              </button>
            </div>

            <!-- Already Signed indicator (from previous session) -->
            <div v-else-if="isFinalMomTask && !canSign" class="signed-indicator">
              <svg class="signed-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M20 6L9 17l-5-5"/>
              </svg>
              <span>{{ $t('AlreadySigned') }}</span>
              <button class="unsign-btn" @click="handleUnsign" :disabled="unsigning">
                <span v-if="unsigning">{{ $t('RemovingSignature') }}</span>
                <span v-else>{{ $t('RemoveSignature') }}</span>
              </button>
            </div>
          </div>

          <!-- MeetingMetaData -->
          <div v-else-if="stepperItems[currentStep - 1]?.component === 'MeetingMetaData'" class="content-scroll">
            <MeetingMetaData v-model="meeting" :view-mode="true" />
          </div>

          <!-- MeetingAttendees -->
          <div v-else-if="stepperItems[currentStep - 1]?.component === 'MeetingAttendees'" class="content-scroll">
            <MeetingAttendees v-model="meeting.meetingAttendees" :meeting-id="meeting.id" :view-mode="true" />
          </div>

          <!-- MeetingAgenda -->
          <div v-else-if="stepperItems[currentStep - 1]?.component === 'MeetingAgenda'" class="content-scroll">
            <MeetingAgenda
              v-model="meeting.meetingAgendas"
              :meeting-id="meeting.id"
              :is-committee="meeting.isCommittee"
              :committee-id="meeting.committeeId"
              :view-mode="true"
            />
          </div>

          <!-- MeetingAttachments -->
          <div v-else-if="stepperItems[currentStep - 1]?.component === 'MeetingAttachments'" class="content-scroll">
            <MeetingAttachments
              v-model="meeting.attachments"
              :meeting-id="meeting.id"
              :meeting-agendas="meeting.meetingAgendas"
              :view-mode="true"
            />
          </div>

          <!-- AssociatedMeetings -->
          <div v-else-if="stepperItems[currentStep - 1]?.component === 'AssociatedMeetings'" class="content-scroll">
            <AssociatedMeetings v-model="meeting.associatedMeetings" :meeting-id="meeting.id" :view-mode="true" />
          </div>
        </section>

        <!-- Action Bar -->
        <footer class="action-bar">
          <div class="action-start">
            <button v-if="currentStep > 1" class="btn btn-outline" @click="previousStep">
              <svg class="btn-icon btn-icon-prev" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
                <path d="M15 18l-6-6 6-6"/>
              </svg>
              <span>{{ $t('Previous') }}</span>
            </button>
          </div>

          <div class="action-center">
            <!-- Regular approve button for non-Final MOM tasks -->
            <button
              v-if="stepperItems[currentStep - 1]?.hasApprove && !isFinalMomTask"
              class="btn btn-success"
              :disabled="btnLoading"
              @click="openApproveDialog(true)"
            >
              <div v-if="btnLoading" class="spinner small white"></div>
              <svg v-else class="btn-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
                <path d="M20 6L9 17l-5-5"/>
              </svg>
              <span>{{ $t('Approve') }}</span>
            </button>

            <!-- Approve button for Final MOM tasks - only shown after signature is saved -->
            <button
              v-if="isFinalMomTask && signatureSaved"
              class="btn btn-success"
              :disabled="btnLoading"
              @click="confirmApprovalDialog = true"
            >
              <div v-if="btnLoading" class="spinner small white"></div>
              <svg v-else class="btn-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
                <path d="M20 6L9 17l-5-5"/>
              </svg>
              <span>{{ $t('ApproveMinutes') }}</span>
            </button>

            <button
              v-if="stepperItems[currentStep - 1]?.hasReject && !isFinalMomTask"
              class="btn btn-danger"
              :disabled="btnLoading"
              @click="openApproveDialog(false)"
            >
              <div v-if="btnLoading" class="spinner small white"></div>
              <svg v-else class="btn-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
                <path d="M18 6L6 18M6 6l12 12"/>
              </svg>
              <span>{{ $t('Reject') }}</span>
            </button>
          </div>

          <div class="action-end">
            <button v-if="currentStep < stepperItems.length" class="btn btn-primary" @click="nextStep">
              <span>{{ $t('Next') }}</span>
              <svg class="btn-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
                <path d="M9 18l6-6-6-6"/>
              </svg>
            </button>
          </div>
        </footer>
      </template>

      <!-- Empty State -->
      <div v-else class="main-empty">
        <div class="empty-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <path d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
        <h3>{{ $t('SelectTaskToView') }}</h3>
        <p>{{ $t('ClickTaskToViewDetails') }}</p>
      </div>
    </main>

    <!-- Info Dialog -->
    <Transition name="dialog-fade">
      <div v-if="infoDialog.show" class="info-dialog-overlay" @click.self="infoDialog.show = false">
        <div class="info-dialog" :class="infoDialog.type">
          <div class="info-dialog-icon">
            <svg v-if="infoDialog.type === 'success'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10"/>
              <path d="M9 12l2 2 4-4"/>
            </svg>
            <svg v-else-if="infoDialog.type === 'error'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10"/>
              <path d="M15 9l-6 6M9 9l6 6"/>
            </svg>
            <svg v-else-if="infoDialog.type === 'warning'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/>
              <path d="M12 9v4M12 17h.01"/>
            </svg>
            <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10"/>
              <path d="M12 16v-4M12 8h.01"/>
            </svg>
          </div>
          <h3 class="info-dialog-title">{{ infoDialog.title }}</h3>
          <p class="info-dialog-message">{{ infoDialog.message }}</p>
          <button class="info-dialog-btn" :class="infoDialog.type" @click="infoDialog.show = false">
            {{ $t('OK') }}
          </button>
        </div>
      </div>
    </Transition>

    <!-- Approval Dialog -->
    <Modal v-if="noteDialog" v-model="noteDialog" :title="$t('AddNote')" size="md" no-padding>
      <div class="dialog-content">
        <label>{{ $t('Note') }}</label>
        <textarea
          v-model="note"
          rows="5"
          :placeholder="$t('AddNoteHere')"
        ></textarea>
      </div>
      <template #footer>
        <div class="dialog-actions">
          <button class="btn btn-ghost" @click="noteDialog = false">
            {{ $t('Cancel') }}
          </button>
          <button
            :class="['btn', isApproving ? 'btn-success' : 'btn-danger']"
            :disabled="btnLoading"
            @click="submitApproval"
          >
            <div v-if="btnLoading" class="spinner small white"></div>
            <template v-else>
              <svg v-if="isApproving" class="btn-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
                <path d="M20 6L9 17l-5-5"/>
              </svg>
              <svg v-else class="btn-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
                <path d="M18 6L6 18M6 6l12 12"/>
              </svg>
            </template>
            <span>{{ isApproving ? $t('Approve') : $t('Reject') }}</span>
          </button>
        </div>
      </template>
    </Modal>

    <!-- Confirmation Dialog for Final MOM Approval -->
    <Modal v-if="confirmApprovalDialog" v-model="confirmApprovalDialog" :title="$t('ConfirmApproval')" size="sm" no-padding>
      <div class="dialog-content">
        <p style="text-align: center; margin: 1rem 0;">
          {{ $t('ConfirmApproveMinutes') }}
        </p>
        <p style="text-align: center; color: var(--text-muted); font-size: 0.875rem;">
          {{ $t('ActionCannotBeUndone') }}
        </p>
      </div>
      <template #footer>
        <div class="dialog-actions">
          <button class="btn btn-ghost" @click="confirmApprovalDialog = false">
            {{ $t('Cancel') }}
          </button>
          <button
            class="btn btn-success"
            :disabled="btnLoading"
            @click="handleConfirmedApproval"
          >
            <div v-if="btnLoading" class="spinner small white"></div>
            <svg v-else class="btn-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
              <path d="M20 6L9 17l-5-5"/>
            </svg>
            <span>{{ $t('ConfirmApproval') }}</span>
          </button>
        </div>
      </template>
    </Modal>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { mainApiAxios } from '@/plugins/axios'
import Modal from '@/components/ui/Modal.vue'
import PdfViewer from '@/components/ui/PdfViewer.vue'
import MeetingMetaData from '@/components/app/meeting/MeetingMetaData.vue'
import MeetingAttendees from '@/components/app/meeting/MeetingAttendees.vue'
import MeetingAgenda from '@/components/app/meeting/MeetingAgenda.vue'
import MeetingAttachments from '@/components/app/meeting/MeetingAttachments.vue'
import AssociatedMeetings from '@/components/app/meeting/AssociatedMeetings.vue'
import MeetingTasksService from '@/services/MeetingTasksService'
import MeetingsService from '@/services/MeetingsService'
import AnnotationsService, { AnnotationType, type StampDto } from '@/services/AnnotationsService'
import type { MeetingTask } from '@/services/MeetingTasksService'

const { t, locale } = useI18n()

const TaskTypeEnum = {
  MeetingApproval: 1,
  InitialMeetingAgendaApproval: 2,
  InitialMeetingMinutesApproval: 3,
  SignFinalMeetingMinutes: 4
}

interface StepperItem {
  id: number
  title: string
  component: string
  hasApprove: boolean
  hasReject: boolean
}

// State
const tasks = ref<MeetingTask[]>([])
const totalCount = ref(0)
const totalPages = ref(0)
const page = ref(1)
const pageSize = ref(10)
const loading = ref(false)

// Computed
const delayedCount = computed(() => tasks.value.filter(t => t.isDelayed).length)

// Helpers
const getTaskTypeClass = (typeId: number) => {
  switch (typeId) {
    case 1: return 'type-approval'
    case 2: return 'type-agenda'
    case 3: return 'type-minutes'
    case 4: return 'type-signature'
    default: return 'type-default'
  }
}
const loadingDetails = ref(false)
const btnLoading = ref(false)

const selectedTaskId = ref<string | number | null>(null)
const selectedTask = ref<MeetingTask | null>(null)
const showDetails = ref(false)
const currentStep = ref(1)
const stepperItems = ref<StepperItem[]>([])

const meeting = ref<any>({
  id: null,
  title: null,
  isCommittee: false,
  committeeId: null,
  date: null,
  startTime: null,
  endTime: null,
  location: null,
  notes: null,
  meetingAttendees: [],
  meetingAgendas: [],
  associatedMeetings: [],
  attachments: []
})

const documentQuery = ref<any>(null)
const latestAttachmentId = ref<number | null>(null)
const noteDialog = ref(false)
const note = ref('')
const isApproving = ref(true)
const confirmApprovalDialog = ref(false)

// Fresh attachment params for signature submission
const attachmentParamsForSign = ref<{
  attachmentId: number
  taskId: number
  token: string
  hashValidation: string
  actions: string
} | null>(null)

// Signature state
const savingSignature = ref(false)
const unsigning = ref(false)
const canSign = ref(true)
const signatureSaved = ref(false) // Track if signature was successfully saved
const pdfBlobUrl = ref<string | null>(null)
const signPdfIframe = ref<HTMLIFrameElement | null>(null)

// Info dialog state
const infoDialog = reactive({
  show: false,
  title: '',
  message: '',
  type: 'info' as 'success' | 'error' | 'warning' | 'info'
})

const showInfoDialog = (message: string, type: 'success' | 'error' | 'warning' | 'info' = 'info', title?: string) => {
  const defaultTitles = {
    success: t('Success'),
    error: t('Error'),
    warning: t('Warning'),
    info: t('Info')
  }
  infoDialog.show = true
  infoDialog.message = message
  infoDialog.type = type
  infoDialog.title = title || defaultTitles[type]
}

// Build PDF.js viewer URL with signing enabled
const signViewerUrl = computed(() => {
  if (!pdfBlobUrl.value) return ''
  const signParam = canSign.value ? '&sign=true' : ''
  const loc = locale.value === 'ar' ? 'ar' : 'en-US'
  return `/viewer/web/viewer.html?locale=${loc}&file=${encodeURIComponent(pdfBlobUrl.value)}${signParam}`
})

// Check if current task is a Final MOM signature task
const isFinalMomTask = computed(() => {
  return selectedTask.value?.typeId === TaskTypeEnum.SignFinalMeetingMinutes
})

// Parse attachment query parameters from documentQuery URL
const attachmentParams = computed(() => {
  if (!documentQuery.value?.url) return null

  try {
    const queryString = documentQuery.value.url.replace('attachments?', '')
    const params = new URLSearchParams(queryString)
    return {
      attachmentId: parseInt(params.get('att')),
      taskId: parseInt(params.get('task')),
      token: params.get('tk'),
      hashValidation: params.get('hvd'),
      actions: params.get('act')
    }
  } catch {
    return null
  }
})

// Methods
const loadTasks = async () => {
  loading.value = true
  try {
    const response = await MeetingTasksService.listTasks(page.value, pageSize.value)
    const data = response.data || response
    tasks.value = data.data || []
    totalCount.value = data.total || 0
    totalPages.value = Math.ceil((data.total || 0) / pageSize.value)
  } catch (error) {
    console.error('Failed to load tasks:', error)
    tasks.value = []
    totalCount.value = 0
  } finally {
    loading.value = false
  }
}

const handlePageChange = (newPage: number) => {
  page.value = newPage
  loadTasks()
}

const showTaskData = async (task: MeetingTask) => {
  if (selectedTaskId.value === task.id) return

  claimTask(task)
  loadingDetails.value = true
  selectedTaskId.value = task.id
  selectedTask.value = task
  documentQuery.value = null // Reset document query
  latestAttachmentId.value = null // Reset attachment ID
  canSign.value = true // Reset sign state
  signatureSaved.value = false // Reset signature saved state
  attachmentParamsForSign.value = null // Reset attachment params

  // Cleanup previous PDF blob URL
  if (pdfBlobUrl.value) {
    URL.revokeObjectURL(pdfBlobUrl.value)
    pdfBlobUrl.value = null
  }

  try {
    // Load meeting data
    const meetingResponse = await MeetingsService.loadMeeting(task.meetingId)
    const meetingData = meetingResponse?.data || meetingResponse
    meeting.value = {
      ...meetingData,
      meetingAttendees: meetingData.meetingAttendees || [],
      meetingAgendas: meetingData.meetingAgendas || [],
      associatedMeetings: meetingData.associatedMeetings || [],
      attachments: meetingData.attachments || []
    }

    // Load the latest version of the minutes
    let latestAttachment: any = null

    if (task.typeId === TaskTypeEnum.InitialMeetingMinutesApproval) {
      // Get all versions and pick the latest
      const versionsResponse = await MeetingsService.getInitialMeetingMinutesVersions(Number(task.meetingId))
      const versions = versionsResponse?.data || versionsResponse || []
      if (versions.length > 0) {
        // Find the latest version (highest version number)
        latestAttachment = versions.reduce((latest: any, v: any) => {
          const latestVersion = latest?.version || latest?.Version || 0
          const currentVersion = v?.version || v?.Version || 0
          return currentVersion > latestVersion ? v : latest
        }, versions[0])
      }
    } else if (task.typeId === TaskTypeEnum.SignFinalMeetingMinutes) {
      // Get all versions and pick the latest
      const versionsResponse = await MeetingsService.getFinalMeetingMinutesVersions(Number(task.meetingId))
      const versions = versionsResponse?.data || versionsResponse || []
      if (versions.length > 0) {
        // Find the latest version (highest version number)
        latestAttachment = versions.reduce((latest: any, v: any) => {
          const latestVersion = latest?.version || latest?.Version || 0
          const currentVersion = v?.version || v?.Version || 0
          return currentVersion > latestVersion ? v : latest
        }, versions[0])
      }
    }

    if (latestAttachment?.id) {
      // Store the attachment ID for later use (e.g., signature modal)
      latestAttachmentId.value = latestAttachment.id

      // Get the query string for the attachment
      // For Final MOM tasks, use task-aware endpoint to get signing permissions
      let queryString: string | null = null

      if (task.typeId === TaskTypeEnum.SignFinalMeetingMinutes) {
        // Use task-aware endpoint for signing tasks
        try {
          const attachmentResponse: any = await MeetingTasksService.getTaskAttachmentQuery(task.id, latestAttachment.id)
          queryString = attachmentResponse?.data?.data || attachmentResponse?.data || attachmentResponse
        } catch (err) {
          console.warn('Task attachment endpoint failed, falling back to regular endpoint:', err)
          // Fall back to regular endpoint
          const attachmentResponse: any = await MeetingsService.getAttachmentQuery(latestAttachment.id)
          queryString = attachmentResponse?.data?.data || attachmentResponse?.data || attachmentResponse
          // Assume user can sign since the task is not completed
          canSign.value = true
        }
      } else {
        // Use regular endpoint for other tasks
        const attachmentResponse: any = await MeetingsService.getAttachmentQuery(latestAttachment.id)
        queryString = attachmentResponse?.data?.data || attachmentResponse?.data || attachmentResponse
      }

      if (queryString) {
        // Construct the URL for PdfViewer (it will prefix with /api/)
        documentQuery.value = {
          url: `attachments?${queryString}`,
          name: latestAttachment.fileName || latestAttachment.FileName || 'MeetingMinutes.pdf'
        }

        // Check if user can sign (from actions parameter)
        if (task.typeId === TaskTypeEnum.SignFinalMeetingMinutes) {
          try {
            const params = new URLSearchParams(queryString)
            const actionsBase64 = params.get('act')
            if (actionsBase64) {
              // Decode base64url - backend uses Base64UrlEncoder
              const decoded = atob(actionsBase64.replace(/-/g, '+').replace(/_/g, '/'))
              // Backend encodes Sign=true as "s=1&", RemoveSign=true as "rm=1&"
              const canSignFromBackend = decoded.includes('s=1')
              const hasAlreadySigned = decoded.includes('rm=1')
              canSign.value = canSignFromBackend
              // If user has already signed (rm=1), show signed state
              if (hasAlreadySigned) {
                signatureSaved.value = true
              }
            } else {
              // No act parameter - assume user can sign
              canSign.value = true
            }

            // Store attachment params for signature submission
            attachmentParamsForSign.value = {
              attachmentId: parseInt(params.get('att')),
              taskId: parseInt(params.get('task')),
              token: params.get('tk'),
              hashValidation: params.get('hvd'),
              actions: params.get('act')
            }

            // Load PDF as blob for the signing viewer
            const url = `attachments?${queryString}`
            const response = await mainApiAxios.get(url, { responseType: 'blob' })
            pdfBlobUrl.value = URL.createObjectURL(response.data)
          } catch (e) {
            console.warn('Failed to setup signing:', e)
            canSign.value = true
          }
        }
      }
    }

    generateStepperItems(task.typeId)
    showDetails.value = true
    currentStep.value = 1
  } catch (error) {
    console.error('Failed to load task data:', error)
    showDetails.value = false
  } finally {
    loadingDetails.value = false
  }
}

const claimTask = async (task: MeetingTask) => {
  task.claimed = true
  try {
    await MeetingTasksService.claimTask(task.id)
  } catch (error) {
    console.error('Failed to claim task:', error)
  }
}

const generateStepperItems = (typeId: number) => {
  let currentId = 1
  stepperItems.value = []

  const isMeetingMinutes = typeId === TaskTypeEnum.InitialMeetingMinutesApproval
  const isFinalMinutes = typeId === TaskTypeEnum.SignFinalMeetingMinutes

  if (isMeetingMinutes) {
    stepperItems.value.push({ id: currentId++, title: t('MeetingMinutes'), component: 'FileViewer', hasApprove: true, hasReject: true })
  } else if (isFinalMinutes) {
    stepperItems.value.push({ id: currentId++, title: t('FinalMinutes'), component: 'FileViewer', hasApprove: true, hasReject: false })
  }

  const notMeetingMinutes = !isMeetingMinutes && !isFinalMinutes

  stepperItems.value.push({ id: currentId++, title: t('MeetingInfo'), component: 'MeetingMetaData', hasApprove: notMeetingMinutes, hasReject: notMeetingMinutes })
  stepperItems.value.push({ id: currentId++, title: t('Attendees'), component: 'MeetingAttendees', hasApprove: notMeetingMinutes, hasReject: notMeetingMinutes })
  stepperItems.value.push({ id: currentId++, title: t('Agenda'), component: 'MeetingAgenda', hasApprove: notMeetingMinutes, hasReject: notMeetingMinutes })
  stepperItems.value.push({ id: currentId++, title: t('Attachments'), component: 'MeetingAttachments', hasApprove: notMeetingMinutes, hasReject: notMeetingMinutes })
  stepperItems.value.push({ id: currentId++, title: t('AssociatedMeetings'), component: 'AssociatedMeetings', hasApprove: notMeetingMinutes, hasReject: notMeetingMinutes })
}

const nextStep = () => {
  if (currentStep.value < stepperItems.value.length) currentStep.value++
}

const previousStep = () => {
  if (currentStep.value > 1) currentStep.value--
}

const setCurrentStep = (step: number) => {
  currentStep.value = step
}

const openApproveDialog = (approved: boolean) => {
  isApproving.value = approved
  note.value = ''
  noteDialog.value = true
}

const submitApproval = async () => {
  if (!selectedTaskId.value) return

  btnLoading.value = true
  try {
    let noteText = note.value || ''
    if (!isNaN(Number(noteText)) && noteText.trim() !== '') {
      noteText = '_' + noteText
    }

    const response = await MeetingTasksService.approveTask(selectedTaskId.value, isApproving.value, noteText + ' ')

    if (response.success !== false) {
      noteDialog.value = false
      note.value = ''
      showDetails.value = false
      selectedTaskId.value = null
      loadTasks()
    }
  } catch (error) {
    console.error('Failed to approve/reject task:', error)
  } finally {
    btnLoading.value = false
  }
}

// Submit approval after signing
const submitSignatureApproval = async () => {
  if (!selectedTaskId.value) return

  btnLoading.value = true
  try {
    const response = await MeetingTasksService.approveTask(selectedTaskId.value, true, t('Signed'))

    if (response.success !== false) {
      showDetails.value = false
      selectedTaskId.value = null
      loadTasks()
    }
  } catch (error) {
    console.error('Failed to approve task after signing:', error)
  } finally {
    btnLoading.value = false
  }
}

// Handle confirmed approval from confirmation dialog
const handleConfirmedApproval = async () => {
  confirmApprovalDialog.value = false
  await submitSignatureApproval()
}

// Handle signature messages from PDF viewer iframe
const handleSignatureMessage = async (event: MessageEvent) => {
  // Handle signature cancelled
  if (event.data?.type === 'signature-cancelled') {
    // Reset any signing-related state if needed
    savingSignature.value = false
    return
  }

  // Handle signature placed (save)
  if (event.data?.type !== 'signature-placed') return
  if (!attachmentParamsForSign.value || savingSignature.value) return

  savingSignature.value = true

  try {
    const { attachmentId, taskId, token, hashValidation, actions } = attachmentParamsForSign.value
    const { signatureImage, pageIndex, xPercent, yPercent, widthPercent, heightPercent } = event.data.data

    // Validate signature image exists
    if (!signatureImage) {
      console.error('[Tasks] No signature image received')
      showInfoDialog(t('SignatureImageNotReceived'), 'error', t('Error'))
      return
    }

    // Get actual PDF dimensions from the viewer data
    const { pageWidth: renderedWidth, pageHeight: renderedHeight, scale: viewerScale } = event.data.data

    // Calculate actual PDF dimensions (unscaled)
    // If viewer data is available, use it; otherwise fall back to A4
    const pdfWidth = renderedWidth && viewerScale ? renderedWidth / viewerScale : 595
    const pdfHeight = renderedHeight && viewerScale ? renderedHeight / viewerScale : 842

    // Calculate signature dimensions in PDF points
    const sigWidthPts = widthPercent * pdfWidth
    const sigHeightPts = heightPercent * pdfHeight

    // Calculate position in PDF coordinates (origin at bottom-left)
    const x = xPercent * pdfWidth
    // PDF y-axis is inverted (0 at bottom), so we need to convert
    // yPercent is measured from top in screen coords, so we flip it
    const y = pdfHeight - (yPercent * pdfHeight) - sigHeightPts

    // Clamp coordinates to valid PDF page bounds
    const clampedX = Math.max(0, Math.min(x, pdfWidth - sigWidthPts))
    const clampedY = Math.max(0, Math.min(y, pdfHeight - sigHeightPts))

    // Create signature stamp using clamped coordinates
    const signatureStamp: StampDto = {
      annotationType: AnnotationType.Signature,
      stampType: 23,
      pageIndex: pageIndex,
      rect: [clampedX, clampedY, clampedX + sigWidthPts, clampedY + sigHeightPts],
      stampData: signatureImage.replace(/^data:image\/\w+;base64,/, ''),
      rotation: 0
    }

    // Save the signature via annotations endpoint
    const response = await AnnotationsService.saveAnnotations(
      attachmentId,
      taskId,
      token,
      hashValidation,
      [signatureStamp],
      actions
    )

    // API returns { data: "token_string", success: true } or just the token string
    const newToken = typeof response === 'string' ? response : (response?.data || response)

    if (newToken && typeof newToken === 'string') {
      // Mark signature as saved - user can now approve manually
      signatureSaved.value = true
      canSign.value = false // Disable further signing

      // Reload PDF with new token to show the burned-in signature
      try {
        const newUrl = `attachments?${newToken}`
        const pdfResponse = await mainApiAxios.get(newUrl, { responseType: 'blob' })

        // Cleanup old blob URL
        if (pdfBlobUrl.value) {
          URL.revokeObjectURL(pdfBlobUrl.value)
        }

        // Create new blob URL and update
        pdfBlobUrl.value = URL.createObjectURL(pdfResponse.data)
      } catch (reloadError) {
        console.warn('[Tasks] Failed to reload PDF:', reloadError)
      }

      showInfoDialog(t('SignatureSavedSuccessfully'), 'success', t('Signed'))
    } else {
      showInfoDialog(t('SignatureSaveFailed'), 'error')
    }
  } catch (error: any) {
    console.error('Failed to save signature:', error)
    if (error?.response?.status === 409) {
      // Already signed - allow approval
      signatureSaved.value = true
      canSign.value = false
      showInfoDialog(t('AlreadySignedCanApprove'), 'warning')
    } else {
      showInfoDialog(t('SignatureError'), 'error')
    }
  } finally {
    savingSignature.value = false
  }
}

// Handle unsign request
const handleUnsign = async () => {
  if (!attachmentParamsForSign.value || unsigning.value) return

  unsigning.value = true

  try {
    const { attachmentId, taskId, token, hashValidation, actions } = attachmentParamsForSign.value

    const response = await AnnotationsService.removeSignature(
      attachmentId,
      taskId,
      token,
      hashValidation,
      actions
    )

    // API returns new token in response.data
    const newToken = typeof response === 'string' ? response : (response?.data || response)

    if (newToken && typeof newToken === 'string') {
      // Reset signature state
      signatureSaved.value = false
      canSign.value = true

      // Reload PDF without signature - add cache buster to ensure fresh load
      try {
        const cacheBuster = `&_t=${Date.now()}`
        const newUrl = `attachments?${newToken}${cacheBuster}`
        const pdfResponse = await mainApiAxios.get(newUrl, {
          responseType: 'blob',
          headers: { 'Cache-Control': 'no-cache', 'Pragma': 'no-cache' }
        })

        // Cleanup old blob URL
        if (pdfBlobUrl.value) {
          URL.revokeObjectURL(pdfBlobUrl.value)
        }

        // Create new blob URL and update - this will trigger iframe reload
        pdfBlobUrl.value = URL.createObjectURL(pdfResponse.data)

        // Update the stored token for future operations
        const params = new URLSearchParams(newToken)
        attachmentParamsForSign.value = {
          attachmentId: parseInt(params.get('att')),
          taskId: parseInt(params.get('task')),
          token: params.get('tk'),
          hashValidation: params.get('hvd'),
          actions: params.get('act')
        }
      } catch (reloadError) {
        console.warn('[Tasks] Failed to reload PDF:', reloadError)
      }

      showInfoDialog(t('SignatureRemovedSuccessfully'), 'success', t('SignatureRemoved'))
    } else {
      showInfoDialog(t('SignatureRemoveFailed'), 'error')
    }
  } catch (error: any) {
    console.error('Failed to remove signature:', error)
    showInfoDialog(t('SignatureRemoveError'), 'error')
  } finally {
    unsigning.value = false
  }
}

// Cleanup on unmount
onUnmounted(() => {
  window.removeEventListener('message', handleSignatureMessage)
  if (pdfBlobUrl.value) {
    URL.revokeObjectURL(pdfBlobUrl.value)
  }
})

onMounted(() => {
  loadTasks()
  // Listen for signature placement messages from iframe
  window.addEventListener('message', handleSignatureMessage)
})
</script>

<style scoped>
.tasks-page {
  display: flex;
  height: calc(100vh - 100px);
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid #e6eaef;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

/* ===== Sidebar ===== */
.tasks-sidebar {
  width: 340px;
  display: flex;
  flex-direction: column;
  border-inline-end: 1px solid #e6eaef;
  background: #ffffff;
}

.sidebar-loading {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Spinner */
.spinner {
  width: 24px;
  height: 24px;
  border: 3px solid #e8e8e8;
  border-top-color: var(--color-primary, #006d4b);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

.spinner.large {
  width: 32px;
  height: 32px;
  border-width: 4px;
}

.spinner.small {
  width: 16px;
  height: 16px;
  border-width: 2px;
}

.spinner.white {
  border-color: rgba(255, 255, 255, 0.3);
  border-top-color: #fff;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.tasks-scroll {
  flex: 1;
  overflow-y: auto;
  padding: 12px;
}

/* ===== Task Card ===== */
.task-card {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 14px;
  margin-bottom: 8px;
  background: #fff;
  border: 1px solid #e6eaef;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s ease;
  position: relative;
  border-inline-start: 3px solid transparent;
}

.task-card:hover {
  border-color: #d1d5db;
  border-inline-start-color: rgba(0, 109, 75, 0.4);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
  transform: translateY(-1px);
}

.task-card.active {
  border-color: #006d4b;
  border-inline-start-color: #006d4b;
  background: rgba(0, 109, 75, 0.04);
  box-shadow: 0 0 0 2px rgba(0, 109, 75, 0.1);
}

/* Task Type Wrapper with Status Indicator */
.task-type-wrapper {
  position: relative;
  flex-shrink: 0;
}

/* Task Type Badge */
.task-type-badge {
  width: 44px;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 12px;
  flex-shrink: 0;
}

.task-type-badge svg {
  width: 22px;
  height: 22px;
}

.task-type-badge.type-approval {
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
}

.task-type-badge.type-agenda {
  background: rgba(59, 130, 246, 0.1);
  color: #3b82f6;
}

.task-type-badge.type-minutes {
  background: rgba(139, 92, 246, 0.1);
  color: #8b5cf6;
}

.task-type-badge.type-signature {
  background: rgba(245, 158, 11, 0.1);
  color: #f59e0b;
}

.task-type-badge.type-default {
  background: #f1f5f9;
  color: #64748b;
}

/* Status Indicator - Small dot on corner of icon */
.status-indicator {
  position: absolute;
  top: -2px;
  right: -2px;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 2px solid #fff;
}

[dir="rtl"] .status-indicator {
  right: auto;
  left: -2px;
}

.status-indicator.on-time {
  background: #10b981;
}

.status-indicator.delayed {
  background: #ef4444;
}

/* Task Content */
.task-content {
  flex: 1;
  min-width: 0;
}

.task-type-label {
  display: block;
  font-size: 11px;
  font-weight: 600;
  color: #71717a;
  margin-bottom: 4px;
  text-transform: uppercase;
  letter-spacing: 0.02em;
}

.task-title {
  font-size: 14px;
  font-weight: 600;
  color: #27272a;
  margin: 0 0 10px;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.task-card.active .task-title {
  color: #006d4b;
}

.task-meta {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.task-ref {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 11px;
  font-weight: 600;
  padding: 4px 10px;
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
  border-radius: 6px;
}

.task-ref svg {
  width: 12px;
  height: 12px;
}

.task-claimed-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 10px;
  font-weight: 500;
  padding: 3px 8px;
  background: #f4f4f5;
  color: #71717a;
  border-radius: 5px;
}

.task-claimed-badge svg {
  width: 12px;
  height: 12px;
}

/* Task Arrow */
.task-arrow {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  color: #d4d4d8;
  transition: all 0.2s ease;
  flex-shrink: 0;
  align-self: center;
}

.task-arrow svg {
  width: 18px;
  height: 18px;
}

.task-card:hover .task-arrow {
  color: #004730;
  transform: translateX(3px);
}

[dir="rtl"] .task-arrow svg {
  transform: scaleX(-1);
}

[dir="rtl"] .task-card:hover .task-arrow {
  transform: translateX(-3px);
}

.task-card.active .task-arrow {
  color: #004730;
}

/* Empty State */
.empty-list {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 24px;
  text-align: center;
}

.empty-icon-wrapper {
  width: 72px;
  height: 72px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #f4f4f5 0%, #e4e4e7 100%);
  border-radius: 20px;
  margin-bottom: 16px;
}

.empty-icon-wrapper svg {
  width: 36px;
  height: 36px;
  color: #a1a1aa;
}

.empty-list h4 {
  font-size: 16px;
  font-weight: 600;
  color: #3f3f46;
  margin: 0 0 6px;
}

.empty-list p {
  margin: 0;
  font-size: 13px;
  color: #a1a1aa;
}

.sidebar-footer {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 10px 12px;
  border-top: 1px solid #e6eaef;
  background: #f8fafc;
}

.sidebar-footer button {
  width: 30px;
  height: 30px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f4f4f5;
  border: 1px solid #e4e4e7;
  border-radius: 10px;
  cursor: pointer;
  color: #71717a;
  transition: all 0.2s ease;
}

.sidebar-footer button svg {
  width: 18px;
  height: 18px;
}

.sidebar-footer button:hover:not(:disabled) {
  background: #006d4b;
  border-color: #006d4b;
  color: #fff;
}

.sidebar-footer button:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.page-info {
  font-size: 13px;
  font-weight: 600;
  color: #3f3f46;
  padding: 6px 14px;
  background: #f4f4f5;
  border-radius: 8px;
}

/* Main Content */
.task-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: #fff;
}

.main-loading {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}

.main-empty {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px;
  text-align: center;
  background: #f8fafc;
}

.empty-icon {
  width: 72px;
  height: 72px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 109, 75, 0.06);
  border-radius: 50%;
  color: #006d4b;
  margin-bottom: 16px;
}

.main-empty h3 {
  font-size: 16px;
  font-weight: 600;
  color: #004730;
  margin: 0 0 4px;
}

.main-empty p {
  font-size: 13px;
  color: #94a3b8;
  margin: 0;
}

.empty-icon svg {
  width: 40px;
  height: 40px;
}

.main-empty h3 {
  font-size: 16px;
  font-weight: 600;
  color: #333;
  margin: 0 0 8px;
}

.main-empty p {
  font-size: 14px;
  color: #888;
  margin: 0;
}

/* Stepper Nav */
.stepper-nav {
  display: flex;
  gap: 4px;
  padding: 10px 16px;
  background: #fff;
  border-bottom: 1px solid #e6eaef;
  overflow-x: auto;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02);
}

.step-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 14px;
  background: transparent;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s ease;
  white-space: nowrap;
  font-size: 13px;
  color: #64748b;
}

.step-btn:hover {
  background: #e6eaef;
  color: #004730;
}

.step-btn.active {
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
}

.step-btn.completed {
  background: rgba(0, 109, 75, 0.08);
  color: #007E65;
}

.step-indicator {
  width: 22px;
  height: 22px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #e2e8f0;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 700;
  color: #64748b;
}

.step-indicator svg {
  width: 14px;
  height: 14px;
}

.step-btn.active .step-indicator {
  background: rgba(255, 255, 255, 0.2);
  color: #fff;
}

.step-btn.completed .step-indicator {
  background: var(--color-primary, #006d4b);
  color: #fff;
}

.step-label {
  font-size: 13px;
  font-weight: 500;
  color: #666;
}

.step-btn.active .step-label {
  color: #fff;
}

.step-btn.completed .step-label {
  color: var(--color-primary, #006d4b);
}

/* Content Area */
.content-area {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-height: 0;
  background: #f8fafc;
}

.content-scroll {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
  min-height: 0;
}

.content-scroll.pdf-viewer-container {
  padding: 0;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

/* PDF viewer wrapper takes remaining space */
.pdf-viewer-wrapper {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
}

/* Ensure the PdfViewer component fills its wrapper */
.pdf-viewer-wrapper > * {
  flex: 1;
  min-height: 0;
}

/* PDF signing viewer (iframe) */
.pdf-sign-viewer {
  width: 100%;
  height: 100%;
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.pdf-sign-iframe {
  flex: 1;
  width: 100%;
  height: 100%;
  border: none;
}

/* Saving indicator stays at bottom with fixed height */
.content-scroll.pdf-viewer-container > .saving-indicator,
.content-scroll.pdf-viewer-container > .signed-indicator {
  flex: 0 0 auto;
}

/* Saving indicator */
.saving-indicator {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 16px 24px;
  background: rgba(0, 109, 75, 0.05);
  border-top: 1px solid rgba(0, 109, 75, 0.2);
  color: var(--color-primary, #006d4b);
  font-size: 14px;
  font-weight: 500;
}

/* Action Bar */
.action-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 20px;
  background: #ffffff;
  border-top: 1px solid #e6eaef;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.03);
}

.action-start,
.action-center,
.action-end {
  display: flex;
  align-items: center;
  gap: 10px;
}

.action-center {
  flex: 1;
  justify-content: center;
}

/* Buttons */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 9px 18px;
  font-size: 13px;
  font-weight: 500;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-icon {
  width: 16px;
  height: 16px;
  flex-shrink: 0;
}

.btn-primary {
  background: linear-gradient(135deg, #004730, #006d4b);
  color: #fff;
}

.btn-primary:hover:not(:disabled) {
  opacity: 0.9;
}

/* RTL: flip prev/next arrows */
[dir="rtl"] .btn-icon-prev { transform: scaleX(-1); }
[dir="rtl"] .action-end .btn-icon { transform: scaleX(-1); }

.btn-success {
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
}

.btn-success:hover:not(:disabled) {
  background: #006d4b;
  color: #ffffff;
}

.btn-danger {
  background: #fff;
  color: #ef4444;
  border: 1px solid #fecaca;
}

.btn-danger:hover:not(:disabled) {
  background: #fef2f2;
}

.btn-outline {
  background: #fff;
  color: #333;
  border: 1px solid #e0e0e0;
}

.btn-outline:hover:not(:disabled) {
  background: #f5f5f5;
  border-color: #d0d0d0;
}

.btn-ghost {
  background: transparent;
  color: #666;
}

.btn-ghost:hover:not(:disabled) {
  background: #f5f5f5;
  color: #333;
}

/* Dialog */
.dialog-content {
  padding: 16px;
}

.dialog-content label {
  display: block;
  font-size: 14px;
  font-weight: 500;
  color: #333;
  margin-bottom: 8px;
}

.dialog-content textarea {
  width: 100%;
  padding: 6px 8px;
  font-size: 14px;
  line-height: 1.4;
  border: 1px solid #e0e0e0;
  border-radius: 6px;
  resize: none;
  transition: all 0.15s ease;
}

.dialog-content textarea:focus {
  outline: none;
  border-color: var(--color-primary, #006d4b);
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

.dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

/* Signed Indicator */
.signed-indicator {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 16px 24px;
  background: rgba(0, 109, 75, 0.05);
  border-top: 1px solid rgba(0, 109, 75, 0.2);
  color: var(--color-primary, #006d4b);
  font-size: 14px;
  font-weight: 600;
}

.signed-indicator .signed-icon {
  width: 20px;
  height: 20px;
}

.signed-indicator.success {
  background: linear-gradient(135deg, rgba(34, 197, 94, 0.1) 0%, rgba(22, 163, 74, 0.15) 100%);
  border-top: 1px solid rgba(34, 197, 94, 0.3);
  color: #16a34a;
  animation: pulse-success 2s ease-in-out infinite;
}

@keyframes pulse-success {
  0%, 100% { background: linear-gradient(135deg, rgba(34, 197, 94, 0.1) 0%, rgba(22, 163, 74, 0.15) 100%); }
  50% { background: linear-gradient(135deg, rgba(34, 197, 94, 0.15) 0%, rgba(22, 163, 74, 0.2) 100%); }
}

/* Unsign button */
.unsign-btn {
  margin-inline-start: 16px;
  padding: 6px 12px;
  font-size: 12px;
  font-weight: 500;
  color: #dc2626;
  background: rgba(220, 38, 38, 0.1);
  border: 1px solid rgba(220, 38, 38, 0.3);
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.unsign-btn:hover:not(:disabled) {
  background: rgba(220, 38, 38, 0.2);
  border-color: rgba(220, 38, 38, 0.5);
}

.unsign-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Info Dialog */
.info-dialog-overlay {
  position: fixed;
  inset: 0;
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
}

.info-dialog {
  background: #fff;
  border-radius: 20px;
  padding: 32px 40px;
  text-align: center;
  max-width: 400px;
  width: 90%;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  animation: dialog-pop 0.3s ease-out;
}

.info-dialog-icon {
  width: 72px;
  height: 72px;
  margin: 0 auto 20px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.info-dialog.success .info-dialog-icon {
  background: linear-gradient(135deg, #22c55e 0%, #16a34a 100%);
  box-shadow: 0 8px 24px rgba(34, 197, 94, 0.35);
}

.info-dialog.error .info-dialog-icon {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  box-shadow: 0 8px 24px rgba(239, 68, 68, 0.35);
}

.info-dialog.warning .info-dialog-icon {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  box-shadow: 0 8px 24px rgba(245, 158, 11, 0.35);
}

.info-dialog.info .info-dialog-icon {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  box-shadow: 0 8px 24px rgba(59, 130, 246, 0.35);
}

.info-dialog-icon svg {
  width: 36px;
  height: 36px;
  color: #fff;
}

.info-dialog-title {
  font-size: 20px;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0 0 12px;
}

.info-dialog-message {
  font-size: 15px;
  color: #71717a;
  margin: 0 0 28px;
  line-height: 1.6;
}

.info-dialog-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 140px;
  padding: 12px 32px;
  font-size: 15px;
  font-weight: 600;
  border: none;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s ease;
  color: #fff;
}

.info-dialog-btn.success {
  background: linear-gradient(135deg, #22c55e 0%, #16a34a 100%);
  box-shadow: 0 4px 14px rgba(34, 197, 94, 0.4);
}

.info-dialog-btn.success:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(34, 197, 94, 0.5);
}

.info-dialog-btn.error {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  box-shadow: 0 4px 14px rgba(239, 68, 68, 0.4);
}

.info-dialog-btn.error:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(239, 68, 68, 0.5);
}

.info-dialog-btn.warning {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  box-shadow: 0 4px 14px rgba(245, 158, 11, 0.4);
}

.info-dialog-btn.warning:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(245, 158, 11, 0.5);
}

.info-dialog-btn.info {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  box-shadow: 0 4px 14px rgba(59, 130, 246, 0.4);
}

.info-dialog-btn.info:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(59, 130, 246, 0.5);
}

/* Dialog animation */
.dialog-fade-enter-active {
  animation: overlay-in 0.25s ease-out;
}

.dialog-fade-leave-active {
  animation: overlay-out 0.2s ease-in;
}

.dialog-fade-enter-active .info-dialog {
  animation: dialog-pop 0.3s ease-out;
}

.dialog-fade-leave-active .info-dialog {
  animation: dialog-out 0.2s ease-in;
}

@keyframes overlay-in {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes overlay-out {
  from { opacity: 1; }
  to { opacity: 0; }
}

@keyframes dialog-pop {
  0% {
    opacity: 0;
    transform: scale(0.9) translateY(20px);
  }
  100% {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}

@keyframes dialog-out {
  from {
    opacity: 1;
    transform: scale(1);
  }
  to {
    opacity: 0;
    transform: scale(0.95);
  }
}
</style>

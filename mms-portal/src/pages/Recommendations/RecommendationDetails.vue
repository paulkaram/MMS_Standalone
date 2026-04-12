<template>
  <div class="rd-page">
    <!-- Page Header -->
    <PageHeader
      :title="$t('RecommendationDetails')"
      :subtitle="recommendation?.meetingReferenceNo ? `#${recommendation.meetingReferenceNo}` : ''"
      :breadcrumbs="[{ label: $t('Recommendations'), to: '/recommendations' }]"
    >
      <template #actions>
        <button class="rd-back-btn" @click="goBack">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="rd-icon-sm"><path d="M19 12H5M12 19l-7-7 7-7" stroke-linecap="round" stroke-linejoin="round"/></svg>
          {{ $t('Back') }}
        </button>
      </template>
    </PageHeader>

    <!-- Loading -->
    <div v-if="loading" class="rd-loading">
      <div class="rd-spinner"></div>
    </div>

    <template v-else-if="recommendation">
      <div class="rd-content">

        <!-- ═══ INSIGHT + PROGRESS ROW ═══ -->
        <div class="rd-insight-row">

        <!-- ═══ INSIGHT CARD ═══ -->
        <div class="rd-card rd-insight">
          <div class="rd-insight-label">{{ $t('RecommendationInsight') }}</div>
          <p class="rd-insight-text">&ldquo;{{ recommendation.text }}&rdquo;</p>

          <div class="rd-meta-grid">
            <div class="rd-meta-item">
              <span class="rd-meta-label">{{ $t('RecommendationNo') }}</span>
              <span class="rd-meta-value">#{{ recommendation.id }}</span>
            </div>
            <div class="rd-meta-item">
              <span class="rd-meta-label">{{ $t('MeetingReference') }}</span>
              <span class="rd-ref-badge">{{ recommendation.meetingReferenceNo || '-' }}</span>
            </div>
            <div class="rd-meta-item">
              <span class="rd-meta-label">{{ $t('Owner') }}</span>
              <span class="rd-meta-value rd-with-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="rd-icon-xs"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                {{ recommendation.ownerName || '-' }}
              </span>
            </div>
            <div class="rd-meta-item">
              <span class="rd-meta-label">{{ $t('CreatedBy') }}</span>
              <span class="rd-meta-value">{{ recommendation.createdByName }}</span>
            </div>
            <div class="rd-meta-item">
              <span class="rd-meta-label">{{ $t('CreatedAt') }}</span>
              <span class="rd-meta-value rd-with-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="rd-icon-xs"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
                {{ formatDate(recommendation.createdAt) }}
              </span>
            </div>
            <div class="rd-meta-item">
              <span class="rd-meta-label">{{ $t('DueDate') }}</span>
              <span class="rd-meta-value rd-with-icon" :class="{ 'rd-overdue': isOverdue }">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="rd-icon-xs"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
                {{ formatDate(recommendation.dueDate) }}
              </span>
            </div>
          </div>
        </div>

        <!-- ═══ PROGRESS STAT ═══ -->
        <div class="rd-progress-stat">
          <div class="rd-progress-stat-label">{{ $t('ImplementationProgress') }}</div>
          <div class="rd-progress-stat-value" :style="{ color: getProgressColor(recommendation.percentage) }">{{ recommendation.percentage || 0 }}%</div>
          <div class="rd-progress-stat-track">
            <div class="rd-progress-stat-fill" :style="{ width: `${recommendation.percentage || 0}%`, backgroundColor: getProgressColor(recommendation.percentage) }"></div>
          </div>
          <div class="rd-progress-stat-status">
            <span class="mg-pill" :class="getStatusClass(recommendation.status)">{{ recommendation.statusName || recommendation.status }}</span>
          </div>
        </div>

        </div><!-- end rd-insight-row -->

        <!-- ═══ UPDATE CARD ═══ -->
        <div v-if="!viewMode && !isCompleted" class="rd-card rd-update-card">
          <div class="rd-section-head">
            <span class="rd-section-title">{{ $t('UpdateProgress') }}</span>
          </div>
          <div class="rd-update-row">
            <div class="rd-update-field">
              <CustomSelect
                v-model="editForm.statusId"
                :options="filteredStatuses"
                :label="$t('Status')"
                :placeholder="$t('ChooseStatus')"
                value-key="id"
                label-key="name"
              />
            </div>
            <div class="rd-update-field">
              <label class="rd-field-label">{{ $t('Progress') }}</label>
              <div class="rd-progress-input">
                <input type="number" v-model.number="editForm.progress" min="0" max="100" class="rd-input" @input="validateProgress" />
                <span class="rd-input-suffix">%</span>
              </div>
            </div>
            <div class="rd-update-field rd-update-action">
              <Button variant="primary" :loading="updating" :disabled="!canUpdate" @click="updateRecommendation">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="rd-icon-sm"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"/><polyline points="17 21 17 13 7 13 7 21"/><polyline points="7 3 7 8 15 8"/></svg>
                {{ $t('Update') }}
              </Button>
            </div>
          </div>
          <div class="rd-slider-wrap">
            <input type="range" v-model.number="editForm.progress" min="0" max="100" class="rd-slider" :style="{ '--progress': `${editForm.progress}%` }" />
            <div class="rd-slider-labels"><span>0%</span><span>50%</span><span>100%</span></div>
          </div>
        </div>

        <!-- ═══ COMPLETED BANNER ═══ -->
        <div v-if="isCompleted" class="rd-completed">
          <svg viewBox="0 0 24 24" fill="currentColor" class="rd-icon-md"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/></svg>
          <span>{{ $t('RecommendationCompleted') }}</span>
        </div>

        <!-- ═══ NOTES & ATTACHMENTS ROW ═══ -->
        <div class="rd-two-col">

        <!-- ═══ NOTES CARD ═══ -->
        <div class="rd-card">
          <div class="rd-section-head">
            <div class="rd-section-title-group">
              <span class="rd-section-title">{{ $t('Notes') }}</span>
              <span class="rd-count-badge">{{ notes.length }}</span>
            </div>
            <Button v-if="!viewMode && !isCompleted" variant="primary" size="sm" @click="openAddNote">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="rd-icon-sm"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
              {{ $t('Add') }}
            </Button>
          </div>

          <div v-if="notes.length === 0" class="rd-empty">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" class="rd-empty-icon"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
            <p>{{ $t('NoNotes') }}</p>
          </div>

          <div v-else class="rd-notes-list">
            <div v-for="note in notes" :key="note.id" class="rd-note">
              <div class="rd-note-head">
                <span class="rd-note-date">{{ formatDate(note.createdAt) }}</span>
                <div v-if="!viewMode && !isCompleted" class="rd-note-actions">
                  <button class="rd-icon-btn" @click="openEditNote(note)" :title="$t('Edit')">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                  </button>
                  <button class="rd-icon-btn rd-icon-btn-danger" @click="deleteNote(note.id)" :title="$t('Delete')">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
                  </button>
                </div>
              </div>
              <p class="rd-note-text">{{ note.text }}</p>
            </div>
          </div>
        </div>

        <!-- ═══ ATTACHMENTS CARD ═══ -->
        <div class="rd-card">
          <div class="rd-section-head">
            <div class="rd-section-title-group">
              <span class="rd-section-title">{{ $t('Attachments') }}</span>
              <span class="rd-count-badge">{{ attachments.length }}</span>
            </div>
            <Button v-if="!viewMode && !isCompleted" variant="primary" size="sm" @click="openAddAttachment">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="rd-icon-sm"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
              {{ $t('Add') }}
            </Button>
          </div>

          <div v-if="attachments.length === 0" class="rd-empty">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" class="rd-empty-icon"><path d="M21.44 11.05l-9.19 9.19a6 6 0 0 1-8.49-8.49l9.19-9.19a4 4 0 0 1 5.66 5.66l-9.2 9.19a2 2 0 0 1-2.83-2.83l8.49-8.48"/></svg>
            <p>{{ $t('NoAttachments') }}</p>
          </div>

          <div v-else class="rd-attach-list">
            <div v-for="attachment in attachments" :key="attachment.id" class="rd-attach">
              <div class="rd-attach-icon" :class="getFileIconClass(attachment.type)">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
              </div>
              <div class="rd-attach-info">
                <p class="rd-attach-name">{{ attachment.name }}</p>
                <p class="rd-attach-meta">{{ formatFileSize(attachment.size) }} &middot; {{ formatDate(attachment.createdDate) }}</p>
              </div>
              <div class="rd-attach-actions">
                <button class="rd-action-btn rd-action-view" @click="viewAttachment(attachment)" :title="$t('View')">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                </button>
                <button class="rd-action-btn rd-action-download" @click="downloadAttachment(attachment)" :title="$t('Download')">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
                </button>
                <button v-if="!viewMode && !isCompleted" class="rd-action-btn rd-action-delete" @click="confirmDeleteAttachment(attachment)" :title="$t('Delete')">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
                </button>
              </div>
            </div>
          </div>
        </div>

        </div><!-- end rd-two-col -->
      </div>
    </template>

    <!-- ═══ MODALS ═══ -->
    <Modal v-model="noteDialog" :title="editingNoteId ? ($t('EditNote')) : ($t('AddNote'))" size="md">
      <div class="rd-modal-form">
        <label class="rd-field-label">{{ $t('NoteText') }}</label>
        <textarea v-model="newNote" rows="4" class="rd-textarea" :placeholder="$t('EnterNote')"></textarea>
      </div>
      <template #footer>
        <Button variant="outline" @click="noteDialog = false">{{ $t('Cancel') }}</Button>
        <Button variant="primary" :loading="savingNote" :disabled="!newNote.trim()" @click="saveNote">
          {{ editingNoteId ? ($t('Save')) : ($t('Add')) }}
        </Button>
      </template>
    </Modal>

    <Modal v-model="attachmentDialog" :title="$t('AddAttachment')" size="md">
      <div class="rd-modal-form">
        <label class="rd-field-label">{{ $t('SelectFile') }}</label>
        <div class="rd-upload-area" @click="triggerFileInput" @dragover.prevent @drop.prevent="handleDrop">
          <input type="file" ref="fileInput" class="rd-hidden" @change="handleFileSelect" />
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" class="rd-upload-icon"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
          <p v-if="!selectedFile">{{ $t('ClickOrDragFile') }}</p>
          <p v-else class="rd-upload-selected">{{ selectedFile.name }}</p>
        </div>
      </div>
      <template #footer>
        <Button variant="outline" @click="attachmentDialog = false">{{ $t('Cancel') }}</Button>
        <Button variant="primary" :loading="savingAttachment" :disabled="!selectedFile" @click="addAttachment">{{ $t('Upload') }}</Button>
      </template>
    </Modal>

    <Modal v-model="deleteAttachmentDialog" :title="$t('DeleteAttachment')" variant="danger" size="sm">
      <div class="rd-confirm-delete">
        <div class="rd-warning-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
        </div>
        <p>{{ $t('ConfirmDeleteAttachment') }}</p>
        <p v-if="attachmentToDelete" class="rd-file-preview">{{ attachmentToDelete.name }}</p>
      </div>
      <template #footer>
        <Button variant="outline" @click="deleteAttachmentDialog = false">{{ $t('Cancel') }}</Button>
        <Button variant="danger" :loading="deletingAttachment" @click="deleteAttachment">{{ $t('Delete') }}</Button>
      </template>
    </Modal>

    <Modal v-model="viewerDialog" :title="viewingAttachment?.name || $t('ViewFile')" size="4xl">
      <div class="rd-viewer">
        <div v-if="loadingViewer" class="rd-viewer-loading">
          <div class="rd-spinner"></div>
          <p>{{ $t('Loading') }}...</p>
        </div>
        <DocumentViewer v-else-if="viewerQueryString" :query="viewerQueryString" :name="viewingAttachment?.name" :is-dialog="true" :is-client-side="true" />
      </div>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import PageHeader from '@/components/layout/PageHeader.vue'
import Button from '@/components/ui/Button.vue'
import Modal from '@/components/ui/Modal.vue'
import CustomSelect from '@/components/ui/CustomSelect.vue'
import DocumentViewer from '@/components/ui/DocumentViewer.vue'
import RecommendationsService from '@/services/RecommendationsService'
import { formatDate as formatDateHelper } from '@/helpers/dateFormat'
import AttachmentsService from '@/services/AttachmentsService'
import LookupsService from '@/services/LookupsService'
import { useToast } from '@/composables/useToast'
import { getRecommendationStatusColor, getStatusStyle } from '@/helpers/statusColors'
import { AgendaRecommendationStatusEnum } from '@/helpers/enumerations'
import { useUserStore } from '@/stores/user'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const { toast } = useToast()
const userStore = useUserStore()

// State
const loading = ref(false)
const updating = ref(false)
const recommendation = ref<any>(null)
const notes = ref<any[]>([])
const attachments = ref<any[]>([])
const statuses = ref<any[]>([])

const viewMode = computed(() => route.query.viewMode === 'true')
const recommendationId = computed(() => route.params.id as string)

// Edit form
const editForm = reactive({
  statusId: null as number | null,
  progress: 0
})

// Computed
const isCompleted = computed(() => {
  return recommendation.value?.statusId === AgendaRecommendationStatusEnum.Completed
})

const isOverdue = computed(() => {
  if (!recommendation.value?.dueDate) return false
  return new Date(recommendation.value.dueDate) < new Date()
})

const filteredStatuses = computed(() => {
  // Filter out Draft status
  return statuses.value.filter(s => s.id !== AgendaRecommendationStatusEnum.Draft)
})

const canUpdate = computed(() => {
  return editForm.progress !== null &&
         editForm.progress >= 0 &&
         editForm.progress <= 100 &&
         editForm.statusId !== null
})

// Note dialog
const noteDialog = ref(false)
const newNote = ref('')
const savingNote = ref(false)
const editingNoteId = ref<string | null>(null)

// Attachment dialog
const attachmentDialog = ref(false)
const selectedFile = ref<File | null>(null)
const savingAttachment = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)

// Delete attachment confirmation
const deleteAttachmentDialog = ref(false)
const attachmentToDelete = ref<any>(null)
const deletingAttachment = ref(false)

// File viewer
const viewerDialog = ref(false)
const viewingAttachment = ref<any>(null)
const viewerQueryString = ref<string>('')
const loadingViewer = ref(false)

// Clean up blob URL when dialog closes
watch(viewerDialog, (newVal) => {
  if (!newVal && viewerQueryString.value && viewerQueryString.value.startsWith('blob:')) {
    URL.revokeObjectURL(viewerQueryString.value)
    viewerQueryString.value = ''
  }
})

// Methods
const loadRecommendation = async () => {
  if (!recommendationId.value) return
  loading.value = true
  try {
    const res = await RecommendationsService.getRecommendation(recommendationId.value)
    recommendation.value = res.data || res

    // Initialize edit form
    editForm.statusId = recommendation.value.statusId
    editForm.progress = recommendation.value.percentage || 0

    await Promise.all([
      loadNotes(),
      loadAttachments(),
      loadStatuses()
    ])
  } catch (error) {
    toast.error(t('LoadRecommendationFailed'))
  } finally {
    loading.value = false
  }
}

const loadNotes = async () => {
  if (!recommendation.value?.id) return
  try {
    const res = await RecommendationsService.getNotes(String(recommendation.value.id), 1, 100)
    let notesData: any[] = []
    if (Array.isArray(res)) {
      notesData = res
    } else if (res?.data?.data && Array.isArray(res.data.data)) {
      notesData = res.data.data
    } else if (res?.data && Array.isArray(res.data)) {
      notesData = res.data
    }
    notes.value = notesData
  } catch (error) {
    notes.value = []
  }
}

const loadAttachments = async () => {
  if (!recommendation.value?.id) return
  try {
    const res = await RecommendationsService.getAttachments(String(recommendation.value.id), 1, 100)
    if (Array.isArray(res)) {
      attachments.value = res
    } else if (res?.data?.data && Array.isArray(res.data.data)) {
      attachments.value = res.data.data
    } else if (res?.data && Array.isArray(res.data)) {
      attachments.value = res.data
    } else {
      attachments.value = []
    }
  } catch (error) {
    attachments.value = []
  }
}

const loadStatuses = async () => {
  try {
    const res = await LookupsService.getRecommendationStatuses()
    statuses.value = res.data || res || []
  } catch (error) {
    // Silently fail
  }
}

const updateRecommendation = async () => {
  if (!canUpdate.value) return

  updating.value = true
  try {
    await RecommendationsService.updateRecommendation(String(recommendation.value.id), {
      id: Number(recommendation.value.id),
      progress: editForm.progress,
      statusId: editForm.statusId
    })

    toast.success(t('RecommendationUpdated'))

    // Reload data
    await loadRecommendation()

    // If completed, redirect to recommendations list
    if (editForm.statusId === AgendaRecommendationStatusEnum.Completed) {
      router.push({ name: 'recommendations' })
    }
  } catch (error) {
    toast.error(t('UpdateRecommendationFailed'))
  } finally {
    updating.value = false
  }
}

const validateProgress = () => {
  if (editForm.progress > 100) editForm.progress = 100
  if (editForm.progress < 0) editForm.progress = 0
}

const openAddNote = () => {
  newNote.value = ''
  editingNoteId.value = null
  noteDialog.value = true
}

const openEditNote = (note: any) => {
  newNote.value = note.text
  editingNoteId.value = note.id
  noteDialog.value = true
}

const saveNote = async () => {
  if (!newNote.value.trim()) return

  savingNote.value = true
  try {
    if (editingNoteId.value) {
      await RecommendationsService.editNote({
        id: editingNoteId.value,
        text: newNote.value
      })
      toast.success(t('NoteUpdated'))
    } else {
      await RecommendationsService.addNote({
        recommendationId: Number(recommendation.value.id),
        text: newNote.value
      })
      toast.success(t('NoteAdded'))
    }
    noteDialog.value = false
    newNote.value = ''
    editingNoteId.value = null
    await loadNotes()
  } catch (error) {
    toast.error(t(editingNoteId.value ? 'NoteUpdateFailed' : 'NoteAddFailed'))
  } finally {
    savingNote.value = false
  }
}

const deleteNote = async (noteId: string) => {
  try {
    await RecommendationsService.deleteNote(noteId)
    toast.success(t('NoteDeleted'))
    await loadNotes()
  } catch (error) {
    toast.error(t('NoteDeleteFailed'))
  }
}

const openAddAttachment = () => {
  selectedFile.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
  attachmentDialog.value = true
}

const triggerFileInput = () => {
  fileInput.value?.click()
}

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    selectedFile.value = target.files[0]
  }
}

const handleDrop = (event: DragEvent) => {
  if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
    selectedFile.value = event.dataTransfer.files[0]
  }
}

const addAttachment = async () => {
  if (!selectedFile.value) return

  savingAttachment.value = true
  try {
    const formData = new FormData()
    formData.append('file', selectedFile.value)
    formData.append('RecommendationId', String(recommendation.value.id))
    await RecommendationsService.addAttachment(formData)
    attachmentDialog.value = false
    selectedFile.value = null
    toast.success(t('AttachmentAdded'))
    await loadAttachments()
  } catch (error) {
    toast.error(t('AttachmentAddFailed'))
  } finally {
    savingAttachment.value = false
  }
}

const viewAttachment = async (attachment: any) => {
  viewerQueryString.value = ''
  loadingViewer.value = true
  viewingAttachment.value = attachment
  viewerDialog.value = true

  try {
    const queryResponse: any = await AttachmentsService.getAttachment(attachment.id)
    const queryString = queryResponse?.data?.data || queryResponse?.data || queryResponse

    if (queryString && typeof queryString === 'string') {
      const blobResponse: any = await AttachmentsService.download(queryString)
      const blob = blobResponse instanceof Blob ? blobResponse : (blobResponse?.data || blobResponse)

      if (blob instanceof Blob) {
        const blobUrl = URL.createObjectURL(blob)
        viewerQueryString.value = blobUrl
      } else {
        toast.error(t('FileLoadFailed'))
        viewerDialog.value = false
      }
    } else {
      toast.error(t('FileLoadFailed'))
      viewerDialog.value = false
    }
  } catch (error) {
    toast.error(t('FileLoadFailed'))
    viewerDialog.value = false
  } finally {
    loadingViewer.value = false
  }
}

const downloadAttachment = async (attachment: any) => {
  try {
    const response: any = await AttachmentsService.getAttachment(attachment.id)
    const queryString = response?.data?.data || response?.data || response

    if (queryString && typeof queryString === 'string') {
      const response: any = await AttachmentsService.download(queryString)
      const blob = response?.data || response
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = attachment.name
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)
    } else {
      toast.error(t('FileLoadFailed'))
    }
  } catch (error) {
    toast.error(t('FileLoadFailed'))
  }
}

const confirmDeleteAttachment = (attachment: any) => {
  attachmentToDelete.value = attachment
  deleteAttachmentDialog.value = true
}

const deleteAttachment = async () => {
  if (!attachmentToDelete.value) return

  deletingAttachment.value = true
  try {
    await RecommendationsService.deleteAttachment(attachmentToDelete.value.id)
    toast.success(t('AttachmentDeleted'))
    deleteAttachmentDialog.value = false
    attachmentToDelete.value = null
    await loadAttachments()
  } catch (error) {
    toast.error(t('AttachmentDeleteFailed'))
  } finally {
    deletingAttachment.value = false
  }
}

const getProgressColor = (percentage: number) => {
  if (percentage >= 100) return '#006d4b'
  if (percentage >= 50) return '#f59e0b'
  return '#3b82f6'
}

const getStatusClass = (status: string) => {
  const s = (status || '').toLowerCase()
  if (s.includes('complet') || s.includes('مكتمل')) return 'completed'
  if (s.includes('progress') || s.includes('تنفيذ')) return 'in-progress'
  if (s.includes('overdue') || s.includes('متأخر')) return 'cancelled'
  return 'draft'
}

const getStatusName = (statusId: number) => {
  const status = statuses.value.find(s => s.id === statusId)
  return status?.name || ''
}

const getProfilePictureUrl = (userId: string | number) => {
  const baseUrl = import.meta.env.VITE_API_URL || ''
  return `${baseUrl}/api/users/profile-image/${userId}`
}

const getInitials = (name: string) => {
  if (!name) return '?'
  const parts = name.trim().split(/\s+/)
  if (parts.length > 1) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.substring(0, 2).toUpperCase()
}

const formatDate = (date: string) => {
  if (!date) return '-'
  return formatDateHelper(new Date(date))
}

const formatFileSize = (bytes: number) => {
  if (!bytes) return '-'
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

const getFileIconClass = (type: string) => {
  if (!type) return ''
  const t = type.toLowerCase()
  if (t === 'pdf') return 'pdf'
  if (t === 'image' || ['jpg', 'jpeg', 'png', 'gif', 'bmp'].includes(t)) return 'image'
  if (t === 'word' || ['doc', 'docx'].includes(t)) return 'word'
  if (t === 'excel' || ['xls', 'xlsx'].includes(t)) return 'excel'
  return ''
}

const goBack = () => {
  router.back()
}

// Lifecycle
onMounted(() => {
  loadRecommendation()
})
</script>

<style scoped>
/* ═══════════════════════════════════════════ */
/* Recommendation Details — Refined Executive */
/* ═══════════════════════════════════════════ */
.rd-page { width: 100%; }
.rd-content { display: flex; flex-direction: column; gap: 20px; padding: 20px 0; }

/* ── Icons ── */
.rd-icon-xs { width: 14px; height: 14px; flex-shrink: 0; }
.rd-icon-sm { width: 16px; height: 16px; flex-shrink: 0; }
.rd-icon-md { width: 24px; height: 24px; flex-shrink: 0; }

/* ── Back Button ── */
.rd-back-btn {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 8px 16px; border-radius: 8px;
  background: #fff; border: 1px solid #e2e8f0;
  color: #475569; font-size: 13px; font-weight: 500;
  cursor: pointer; transition: all 0.15s; font-family: inherit;
}
.rd-back-btn:hover { background: #f8fafc; border-color: #cbd5e1; }

/* ── Loading ── */
.rd-loading { display: flex; justify-content: center; padding: 80px 0; }
.rd-spinner {
  width: 36px; height: 36px;
  border: 3px solid #e2e8f0; border-top-color: #006d4b;
  border-radius: 50%; animation: rd-spin 0.7s linear infinite;
}
@keyframes rd-spin { to { transform: rotate(360deg); } }

/* ── Card Base ── */
.rd-card {
  background: #fff; border: 1px solid #e2e8f0;
  border-radius: 12px; padding: 24px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.04);
}

/* ── Section Head ── */
.rd-section-head {
  display: flex; align-items: center; justify-content: space-between;
  margin-bottom: 16px;
}
.rd-section-title-group { display: flex; align-items: center; gap: 8px; }
.rd-section-title {
  font-size: 11px; font-weight: 700; color: #006d4b;
  text-transform: uppercase; letter-spacing: 0.06em;
}
.rd-count-badge {
  padding: 2px 8px; background: rgba(0, 109, 75, 0.1); color: #006d4b;
  border-radius: 10px; font-size: 11px; font-weight: 700;
}

/* ═══ INSIGHT CARD ═══ */
.rd-insight {
  border-inline-start: 4px solid #006d4b;
  padding: 28px;
}
.rd-insight-label {
  font-size: 11px; font-weight: 700; color: #006d4b;
  text-transform: uppercase; letter-spacing: 0.06em;
  margin-bottom: 10px;
}
.rd-insight-text {
  font-size: 17px; font-weight: 500; color: #0f172a;
  line-height: 1.65; margin: 0 0 24px;
}

/* ── Meta Grid ── */
.rd-meta-grid {
  display: grid; grid-template-columns: repeat(3, 1fr);
  gap: 0; border: 1px solid #f1f5f9; border-radius: 8px; overflow: hidden;
}
.rd-meta-item {
  padding: 14px 16px;
  border-bottom: 1px solid #f1f5f9;
  border-inline-end: 1px solid #f1f5f9;
}
.rd-meta-item:nth-child(3n) { border-inline-end: none; }
.rd-meta-item:nth-last-child(-n+3) { border-bottom: none; }
.rd-meta-label { display: block; font-size: 11px; color: #94a3b8; font-weight: 500; margin-bottom: 4px; }
.rd-meta-value { display: flex; align-items: center; gap: 5px; font-size: 14px; color: #0f172a; font-weight: 600; }
.rd-with-icon svg { color: #94a3b8; }
.rd-ref-badge {
  display: inline-block; padding: 3px 10px;
  background: rgba(0, 109, 75, 0.1); color: #006d4b;
  border-radius: 6px; font-size: 13px; font-weight: 700;
  font-family: 'Consolas', 'SF Mono', monospace;
}
.rd-overdue { color: #dc2626 !important; }
.rd-overdue svg { color: #dc2626 !important; }

@media (max-width: 640px) {
  .rd-meta-grid { grid-template-columns: 1fr 1fr; }
  .rd-meta-item:nth-child(2n) { border-inline-end: none; }
  .rd-meta-item:nth-child(3n) { border-inline-end: 1px solid #f1f5f9; }
}

/* ═══ INSIGHT + PROGRESS ROW ═══ */
.rd-insight-row {
  display: flex; gap: 20px; align-items: stretch;
}
.rd-insight-row .rd-insight { flex: 1; min-width: 0; }

/* ═══ PROGRESS STAT (dark card) ═══ */
.rd-progress-stat {
  width: 240px; flex-shrink: 0;
  background: #006d4b; border-radius: 12px;
  padding: 24px 20px;
  display: flex; flex-direction: column;
  align-items: center; justify-content: center;
  text-align: center; gap: 8px;
}
.rd-progress-stat-label {
  font-size: 10px; font-weight: 700; color: #64748b;
  text-transform: uppercase; letter-spacing: 0.06em;
}
.rd-progress-stat-value {
  font-size: 36px; font-weight: 800; line-height: 1;
}
.rd-progress-stat-track {
  width: 100%; height: 6px; background: #1e293b;
  border-radius: 3px; overflow: hidden;
}
.rd-progress-stat-fill {
  height: 100%; border-radius: 3px; transition: width 0.4s ease;
}
.rd-progress-stat-status { margin-top: 4px; }

@media (max-width: 768px) {
  .rd-insight-row { flex-direction: column; }
  .rd-progress-stat { width: 100%; flex-direction: row; padding: 16px 20px; }
  .rd-progress-stat-value { font-size: 24px; }
  .rd-progress-stat-track { flex: 1; }
}

/* ═══ PROGRESS ═══ */
.rd-progress-badge {
  padding: 3px 10px; background: #006d4b;
  border-radius: 6px; font-size: 13px; font-weight: 800;
}
.rd-progress-track {
  width: 100%; height: 8px; background: #e2e8f0;
  border-radius: 4px; overflow: hidden;
}
.rd-progress-fill {
  height: 100%; border-radius: 4px;
  transition: width 0.4s ease;
}

/* ═══ UPDATE CARD ═══ */
.rd-update-card { border-top: 3px solid #006d4b; }
.rd-update-row {
  display: grid; grid-template-columns: 1fr 1fr auto;
  gap: 16px; align-items: end;
}
.rd-update-field { min-width: 0; }
.rd-update-action { display: flex; align-items: end; padding-bottom: 2px; }
.rd-field-label { display: block; font-size: 13px; font-weight: 500; color: #334155; margin-bottom: 6px; }
.rd-progress-input { position: relative; }
.rd-input {
  width: 100%; padding: 9px 40px 9px 14px;
  border: 1px solid #e2e8f0; border-radius: 8px;
  font-size: 14px; color: #0f172a; background: #fff;
  outline: none; transition: border-color 0.15s; font-family: inherit;
}
[dir="rtl"] .rd-input { padding: 9px 14px 9px 40px; }
.rd-input:focus { border-color: #006d4b; box-shadow: 0 0 0 3px rgba(0,174,140,0.1); }
.rd-input-suffix {
  position: absolute; inset-inline-end: 14px; top: 50%; transform: translateY(-50%);
  color: #94a3b8; font-weight: 600; font-size: 13px;
}

/* ── Slider ── */
.rd-slider-wrap { margin-top: 16px; }
.rd-slider {
  width: 100%; height: 8px; border-radius: 4px;
  -webkit-appearance: none; appearance: none; cursor: pointer;
  background: linear-gradient(to right, #006d4b var(--progress, 0%), #e2e8f0 var(--progress, 0%));
}
[dir="rtl"] .rd-slider {
  direction: rtl;
  background: linear-gradient(to left, #006d4b var(--progress, 0%), #e2e8f0 var(--progress, 0%));
}
.rd-slider::-webkit-slider-thumb {
  -webkit-appearance: none; width: 20px; height: 20px; border-radius: 50%;
  background: #006d4b; border: 3px solid #fff;
  box-shadow: 0 2px 6px rgba(0,0,0,0.15); cursor: pointer;
}
.rd-slider::-moz-range-thumb {
  width: 20px; height: 20px; border-radius: 50%;
  background: #006d4b; border: 3px solid #fff;
  box-shadow: 0 2px 6px rgba(0,0,0,0.15); cursor: pointer;
}
.rd-slider-labels { display: flex; justify-content: space-between; margin-top: 4px; font-size: 10px; color: #94a3b8; }

@media (max-width: 640px) {
  .rd-update-row { grid-template-columns: 1fr; }
}

/* ═══ COMPLETED ═══ */
.rd-completed {
  display: flex; align-items: center; justify-content: center; gap: 10px;
  padding: 16px; background: rgba(0, 109, 75, 0.1); color: #006d4b;
  border-radius: 12px; font-weight: 700; font-size: 14px;
}

/* ═══ NOTES ═══ */
.rd-notes-list { display: flex; flex-direction: column; gap: 12px; }
.rd-note {
  padding: 14px 16px; background: #f8fafc;
  border-radius: 10px; border-inline-start: 3px solid #3b82f6;
}
.rd-note-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 8px; }
.rd-note-date { font-size: 12px; color: #94a3b8; }
.rd-note-text { font-size: 14px; color: #334155; line-height: 1.6; margin: 0; }
.rd-note-actions { display: flex; gap: 4px; }
.rd-icon-btn {
  width: 28px; height: 28px; display: flex; align-items: center; justify-content: center;
  background: transparent; border: none; border-radius: 6px;
  color: #94a3b8; cursor: pointer; transition: all 0.15s;
}
.rd-icon-btn svg { width: 14px; height: 14px; }
.rd-icon-btn:hover { background: rgba(59,130,246,0.1); color: #3b82f6; }
.rd-icon-btn-danger:hover { background: rgba(239,68,68,0.1); color: #ef4444; }

/* ═══ ATTACHMENTS ═══ */
.rd-attach-list { display: flex; flex-direction: column; gap: 10px; }
.rd-attach {
  display: flex; align-items: center; gap: 14px;
  padding: 12px 14px; background: #f8fafc;
  border-radius: 10px; border: 1px solid #f1f5f9;
}
.rd-attach-icon {
  width: 38px; height: 38px; display: flex; align-items: center; justify-content: center;
  background: rgba(59,130,246,0.08); border-radius: 8px; color: #3b82f6;
}
.rd-attach-icon svg { width: 18px; height: 18px; }
.rd-attach-icon.pdf { background: rgba(239,68,68,0.08); color: #ef4444; }
.rd-attach-icon.image { background: rgba(139,92,246,0.08); color: #8b5cf6; }
.rd-attach-icon.word { background: rgba(59,130,246,0.08); color: #3b82f6; }
.rd-attach-icon.excel { background: rgba(34,197,94,0.08); color: #22c55e; }
.rd-attach-info { flex: 1; min-width: 0; }
.rd-attach-name { font-size: 13px; font-weight: 600; color: #0f172a; margin: 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.rd-attach-meta { font-size: 11px; color: #94a3b8; margin: 2px 0 0; }
.rd-attach-actions { display: flex; gap: 4px; }
.rd-action-btn {
  width: 30px; height: 30px; display: flex; align-items: center; justify-content: center;
  background: #fff; border: 1px solid #e2e8f0; border-radius: 6px;
  cursor: pointer; transition: all 0.15s;
}
.rd-action-btn svg { width: 14px; height: 14px; }
.rd-action-view { color: #3b82f6; }
.rd-action-view:hover { background: rgba(59,130,246,0.08); border-color: rgba(59,130,246,0.3); }
.rd-action-download { color: #006d4b; }
.rd-action-download:hover { background: rgba(0,174,140,0.08); border-color: rgba(0,174,140,0.3); }
.rd-action-delete { color: #94a3b8; }
.rd-action-delete:hover { background: rgba(239,68,68,0.08); border-color: rgba(239,68,68,0.3); color: #ef4444; }

/* ═══ TWO COLUMN ═══ */
.rd-two-col { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
@media (max-width: 768px) { .rd-two-col { grid-template-columns: 1fr; } }

/* ── Button icon fix ── */
.rd-card :deep(button > span) { gap: 4px !important; }
.rd-card :deep(button > span) svg { width: 14px; height: 14px; flex-shrink: 0; }

/* ═══ EMPTY STATE ═══ */
.rd-empty { display: flex; flex-direction: column; align-items: center; padding: 40px; color: #94a3b8; }
.rd-empty-icon { width: 40px; height: 40px; margin-bottom: 10px; opacity: 0.4; }
.rd-empty p { font-size: 13px; margin: 0; }

/* ═══ MODALS ═══ */
.rd-modal-form { padding: 4px 0; }
.rd-textarea {
  width: 100%; padding: 12px; border: 1px solid #e2e8f0; border-radius: 8px;
  font-size: 14px; color: #0f172a; resize: vertical; min-height: 120px;
  outline: none; font-family: inherit;
}
.rd-textarea:focus { border-color: #006d4b; box-shadow: 0 0 0 3px rgba(0,174,140,0.1); }
.rd-upload-area {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  padding: 32px; border: 2px dashed #e2e8f0; border-radius: 10px;
  cursor: pointer; transition: all 0.2s;
}
.rd-upload-area:hover { border-color: #006d4b; background: rgba(0,174,140,0.02); }
.rd-upload-icon { width: 40px; height: 40px; color: #94a3b8; margin-bottom: 10px; }
.rd-upload-area p { font-size: 13px; color: #94a3b8; margin: 0; }
.rd-upload-selected { color: #006d4b !important; font-weight: 600; }
.rd-hidden { display: none; }
.rd-confirm-delete { display: flex; flex-direction: column; align-items: center; text-align: center; padding: 16px 0; }
.rd-warning-icon {
  width: 48px; height: 48px; display: flex; align-items: center; justify-content: center;
  background: rgba(239,68,68,0.08); border-radius: 50%; color: #ef4444; margin-bottom: 14px;
}
.rd-warning-icon svg { width: 24px; height: 24px; }
.rd-confirm-delete p { font-size: 14px; color: #475569; margin: 0 0 8px; }
.rd-file-preview {
  font-size: 13px; font-weight: 600; color: #0f172a;
  padding: 6px 14px; background: #f1f5f9; border-radius: 6px;
  max-width: 100%; word-break: break-all;
}
.rd-viewer { height: 70vh; min-height: 400px; background: #f8fafc; border-radius: 8px; overflow: hidden; }
.rd-viewer-loading {
  height: 100%; display: flex; flex-direction: column; align-items: center;
  justify-content: center; gap: 12px; color: #94a3b8;
}
.rd-viewer-loading p { font-size: 13px; margin: 0; }
</style>

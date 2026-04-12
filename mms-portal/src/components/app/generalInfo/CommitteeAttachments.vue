<template>
  <div class="attachments-section">
    <!-- Add Button -->
    <div v-if="permissionAdd" class="action-bar">
      <button class="add-btn" @click="openAddAttachmentDialog">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M12 5v14M5 12h14" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
        {{ $t('Add') }}
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading && attachments.length === 0" class="loading-state">
      <div class="spinner"></div>
    </div>

    <!-- Attachments List -->
    <div v-else-if="attachments.length > 0" class="attachments-list">
      <div
        v-for="attachment in attachments"
        :key="attachment.id"
        class="attachment-card"
        @click="viewAttachment(attachment.id)"
      >
        <!-- File Icon -->
        <div class="file-icon" :class="getFileTypeClass(attachment.name)">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
            <polyline points="14 2 14 8 20 8"/>
          </svg>
          <span class="file-ext">{{ getFileExtension(attachment.name) }}</span>
        </div>

        <!-- Content -->
        <div class="attach-content">
          <h4 class="attach-name">{{ attachment.name }}</h4>
          <div class="attach-meta">
            <span v-if="attachment.recordTypeName" class="meta-tag type-tag">
              {{ attachment.recordTypeName }}
            </span>
            <span v-if="attachment.privacyName" class="meta-tag privacy-tag">
              {{ attachment.privacyName }}
            </span>
          </div>
        </div>

        <!-- Actions -->
        <div class="attach-actions">
          <button class="view-btn" @click.stop="viewAttachment(attachment.id)">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
              <circle cx="12" cy="12" r="3"/>
            </svg>
          </button>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
        <path d="M21.44 11.05l-9.19 9.19a6 6 0 0 1-8.49-8.49l9.19-9.19a4 4 0 0 1 5.66 5.66l-9.2 9.19a2 2 0 0 1-2.83-2.83l8.49-8.48"/>
      </svg>
      <p>{{ $t('NoAttachments') }}</p>
    </div>

    <!-- Pagination -->
    <div v-if="totalCount > pageSize" class="pagination">
      <button class="page-btn" :disabled="page === 1" @click="goToPage(page - 1)">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M15 18l-6-6 6-6" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </button>
      <span class="page-info">{{ page }} / {{ totalPages }}</span>
      <button class="page-btn" :disabled="page >= totalPages" @click="goToPage(page + 1)">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M9 18l6-6-6-6" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </button>
    </div>

    <!-- View Attachment Modal -->
    <Teleport to="body">
      <div v-if="attachmentDialog" class="modal-overlay" @click.self="attachmentDialog = false">
        <div class="modal-container large">
          <div class="modal-header">
            <h3>{{ $t('Attachment') }}</h3>
            <button class="close-btn" @click="attachmentDialog = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M18 6L6 18M6 6l12 12" stroke-linecap="round" stroke-linejoin="round"/>
              </svg>
            </button>
          </div>
          <div class="modal-body">
            <div v-if="loadingDocument" class="loading-viewer">
              <div class="spinner"></div>
              <p>{{ $t('LoadingFile') }}</p>
            </div>
            <div v-else-if="documentData" class="viewer-container">
              <FileViewer :query="documentData" name="committeeAttachment" />
            </div>
            <div v-else class="error-state">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <circle cx="12" cy="12" r="10"/>
                <line x1="12" y1="8" x2="12" y2="12"/>
                <line x1="12" y1="16" x2="12.01" y2="16"/>
              </svg>
              <p>{{ $t('ErrorLoadingFile') }}</p>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn-secondary" @click="attachmentDialog = false">
              {{ $t('Close') }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Add Attachment Modal -->
    <Teleport to="body">
      <div v-if="addAttachmentDialog" class="modal-overlay" @click.self="addAttachmentDialog = false">
        <div class="modal-container">
          <div class="modal-header">
            <h3>{{ $t('AddAttachment') }}</h3>
            <button class="close-btn" @click="addAttachmentDialog = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M18 6L6 18M6 6l12 12" stroke-linecap="round" stroke-linejoin="round"/>
              </svg>
            </button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>{{ $t('SelectFile') }}</label>
              <div class="file-upload-area" @click="triggerFileInput" @dragover.prevent @drop.prevent="handleDrop">
                <input
                  type="file"
                  ref="fileInput"
                  class="hidden-input"
                  @change="handleFileSelect"
                >
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                  <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4M17 8l-5-5-5 5M12 3v12"/>
                </svg>
                <p v-if="!selectedFile">{{ $t('DragDropOrClick') }}</p>
                <p v-else class="selected-file">{{ selectedFile.name }}</p>
              </div>
            </div>
            <div class="form-group">
              <label>{{ $t('PrivacyLevel') }}</label>
              <select v-model="selectedPrivacyId" class="form-select">
                <option value="" disabled>{{ $t('SelectPrivacy') }}</option>
                <option v-for="level in privacyLevels" :key="level.id" :value="level.id">
                  {{ level.name }}
                </option>
              </select>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn-secondary" @click="addAttachmentDialog = false">
              {{ $t('Cancel') }}
            </button>
            <button
              class="btn-primary"
              :disabled="!selectedFile || !selectedPrivacyId || savingAttachment"
              @click="addAttachment"
            >
              <span v-if="savingAttachment" class="btn-spinner"></span>
              {{ savingAttachment ? ($t('Uploading')) : ($t('Upload')) }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import FileViewer from '@/components/ui/FileViewer.vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'
import AttachmentsService from '@/services/AttachmentsService'
import LookupsService from '@/services/LookupsService'

const props = defineProps<{
  committeeId: string
  permissionAdd?: boolean
}>()

const emit = defineEmits<{
  'update:count': [count: number]
}>()

// State
const loading = ref(false)
const attachments = ref<any[]>([])
const totalCount = ref(0)
const page = ref(1)
const pageSize = ref(5)

const attachmentDialog = ref(false)
const loadingDocument = ref(false)
const documentData = ref<any>(null)

const addAttachmentDialog = ref(false)
const selectedFile = ref<File | null>(null)
const selectedPrivacyId = ref<string | null>(null)
const savingAttachment = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)
const privacyLevels = ref<any[]>([])

// Computed
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

// Methods
const loadAttachments = async () => {
  loading.value = true
  try {
    const result = await CouncilCommitteesService.listCommitteeAttachmentsGeneralInfo(
      props.committeeId,
      page.value,
      pageSize.value
    )
    // API returns { success, data: { data: [], total } }
    attachments.value = result.data?.data || result.data || []
    totalCount.value = result.data?.total || result.total || 0
    emit('update:count', totalCount.value)
  } catch (error) {
    console.error('Failed to load attachments:', error)
  } finally {
    loading.value = false
  }
}

const loadPrivacyLevels = async () => {
  try {
    privacyLevels.value = await LookupsService.getPrivacyLevels()
  } catch (error) {
    console.error('Failed to load privacy levels:', error)
  }
}

const goToPage = (newPage: number) => {
  page.value = newPage
  loadAttachments()
}

const viewAttachment = async (attachmentId: string) => {
  loadingDocument.value = true
  documentData.value = null
  attachmentDialog.value = true

  try {
    const response = await AttachmentsService.getAttachment(attachmentId)
    documentData.value = response
  } catch (error) {
    console.error('Failed to load attachment:', error)
    documentData.value = null
  } finally {
    loadingDocument.value = false
  }
}

const getFileExtension = (filename: string) => {
  if (!filename) return '?'
  const parts = filename.split('.')
  return parts.length > 1 ? parts.pop()?.toUpperCase() || '?' : '?'
}

const getFileTypeClass = (filename: string) => {
  if (!filename) return 'file-default'
  const ext = filename.split('.').pop()?.toLowerCase()
  if (['pdf'].includes(ext || '')) return 'file-pdf'
  if (['doc', 'docx'].includes(ext || '')) return 'file-doc'
  if (['xls', 'xlsx'].includes(ext || '')) return 'file-excel'
  if (['ppt', 'pptx'].includes(ext || '')) return 'file-ppt'
  if (['jpg', 'jpeg', 'png', 'gif', 'svg', 'webp'].includes(ext || '')) return 'file-image'
  if (['zip', 'rar', '7z'].includes(ext || '')) return 'file-archive'
  return 'file-default'
}

const openAddAttachmentDialog = () => {
  selectedFile.value = null
  selectedPrivacyId.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
  addAttachmentDialog.value = true
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
  if (!selectedFile.value || !selectedPrivacyId.value) return

  savingAttachment.value = true
  try {
    const formData = new FormData()
    formData.append('file', selectedFile.value)

    await CouncilCommitteesService.addCommitteeAttachment(
      props.committeeId,
      formData,
      selectedPrivacyId.value
    )
    addAttachmentDialog.value = false
    await loadAttachments()
  } catch (error) {
    console.error('Failed to add attachment:', error)
  } finally {
    savingAttachment.value = false
  }
}

// Watch
watch(() => props.committeeId, () => {
  page.value = 1
  loadAttachments()
})

// Lifecycle
onMounted(() => {
  loadAttachments()
  loadPrivacyLevels()
})
</script>

<style scoped>
.attachments-section {
  /* No wrapper styling - parent card provides it */
}

.action-bar {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 16px;
}

.add-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  background: #006d4b;
  color: #fff;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.add-btn:hover {
  background: #009959;
}

.add-btn svg {
  width: 16px;
  height: 16px;
}

/* Loading */
.loading-state {
  display: flex;
  justify-content: center;
  padding: 40px;
}

.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid #e4e4e7;
  border-top-color: #006d4b;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Attachments List */
.attachments-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.attachment-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  background: #fafafa;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s;
}

.attachment-card:hover {
  background: #f4f4f5;
  transform: translateX(-4px);
}

[dir="ltr"] .attachment-card:hover {
  transform: translateX(4px);
}

/* File Icon */
.file-icon {
  position: relative;
  width: 52px;
  height: 64px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: #e4e4e7;
  border-radius: 8px;
  flex-shrink: 0;
}

.file-icon svg {
  width: 28px;
  height: 28px;
  color: #71717a;
}

.file-ext {
  position: absolute;
  bottom: 4px;
  font-size: 9px;
  font-weight: 700;
  text-transform: uppercase;
  color: #fff;
  background: #71717a;
  padding: 2px 6px;
  border-radius: 4px;
}

.file-pdf { background: rgba(239, 68, 68, 0.1); }
.file-pdf svg { color: #ef4444; }
.file-pdf .file-ext { background: #ef4444; }

.file-doc { background: rgba(59, 130, 246, 0.1); }
.file-doc svg { color: #3b82f6; }
.file-doc .file-ext { background: #3b82f6; }

.file-excel { background: rgba(34, 197, 94, 0.1); }
.file-excel svg { color: #22c55e; }
.file-excel .file-ext { background: #22c55e; }

.file-ppt { background: rgba(249, 115, 22, 0.1); }
.file-ppt svg { color: #f97316; }
.file-ppt .file-ext { background: #f97316; }

.file-image { background: rgba(168, 85, 247, 0.1); }
.file-image svg { color: #a855f7; }
.file-image .file-ext { background: #a855f7; }

.file-archive { background: rgba(234, 179, 8, 0.1); }
.file-archive svg { color: #eab308; }
.file-archive .file-ext { background: #eab308; }

/* Content */
.attach-content {
  flex: 1;
  min-width: 0;
}

.attach-name {
  font-size: 15px;
  font-weight: 600;
  color: #27272a;
  margin: 0 0 8px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.attach-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.meta-tag {
  font-size: 11px;
  padding: 3px 10px;
  border-radius: 12px;
  font-weight: 500;
}

.type-tag {
  background: rgba(59, 130, 246, 0.1);
  color: #3b82f6;
}

.privacy-tag {
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
}

/* Actions */
.attach-actions {
  flex-shrink: 0;
}

.view-btn {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fff;
  border: 1px solid #e4e4e7;
  border-radius: 8px;
  color: #71717a;
  cursor: pointer;
  transition: all 0.2s;
}

.view-btn:hover {
  background: #006d4b;
  border-color: #006d4b;
  color: #fff;
}

.view-btn svg {
  width: 18px;
  height: 18px;
}

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px;
  color: #a1a1aa;
}

.empty-state svg {
  width: 48px;
  height: 48px;
  margin-bottom: 12px;
}

.empty-state p {
  font-size: 14px;
  margin: 0;
}

/* Pagination */
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #e4e4e7;
}

.page-btn {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fff;
  border: 1px solid #e4e4e7;
  border-radius: 8px;
  color: #71717a;
  cursor: pointer;
  transition: all 0.2s;
}

.page-btn:hover:not(:disabled) {
  background: #fafafa;
  border-color: #d4d4d8;
  color: #3f3f46;
}

.page-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-btn svg {
  width: 18px;
  height: 18px;
}

[dir="rtl"] .page-btn svg {
  transform: scaleX(-1);
}

.page-info {
  font-size: 14px;
  color: #71717a;
}

/* Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
}

.modal-container {
  background: #fff;
  border-radius: 16px;
  width: 100%;
  max-width: 480px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
}

.modal-container.large {
  max-width: 900px;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid #e4e4e7;
}

.modal-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: #27272a;
  margin: 0;
}

.close-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f4f4f5;
  border: none;
  border-radius: 8px;
  color: #71717a;
  cursor: pointer;
  transition: all 0.2s;
}

.close-btn:hover {
  background: #e4e4e7;
  color: #3f3f46;
}

.close-btn svg {
  width: 18px;
  height: 18px;
}

.modal-body {
  padding: 24px;
  overflow-y: auto;
  flex: 1;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 16px 24px;
  border-top: 1px solid #e4e4e7;
}

/* Form */
.form-group {
  margin-bottom: 20px;
}

.form-group:last-child {
  margin-bottom: 0;
}

.form-group label {
  display: block;
  font-size: 14px;
  font-weight: 500;
  color: #3f3f46;
  margin-bottom: 8px;
}

.form-select {
  width: 100%;
  padding: 10px 14px;
  font-size: 14px;
  border: 1px solid #e4e4e7;
  border-radius: 8px;
  background: #fff;
  color: #27272a;
  cursor: pointer;
  transition: all 0.2s;
}

.form-select:focus {
  outline: none;
  border-color: #006d4b;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

/* File Upload */
.file-upload-area {
  border: 2px dashed #e4e4e7;
  border-radius: 12px;
  padding: 32px;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s;
}

.file-upload-area:hover {
  border-color: #006d4b;
  background: rgba(0, 109, 75, 0.02);
}

.file-upload-area svg {
  width: 40px;
  height: 40px;
  color: #a1a1aa;
  margin-bottom: 12px;
}

.file-upload-area p {
  font-size: 14px;
  color: #71717a;
  margin: 0;
}

.file-upload-area .selected-file {
  color: #006d4b;
  font-weight: 500;
}

.hidden-input {
  display: none;
}

/* Buttons */
.btn-primary,
.btn-secondary {
  padding: 10px 20px;
  font-size: 14px;
  font-weight: 500;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.btn-primary {
  background: #006d4b;
  color: #fff;
  border: none;
}

.btn-primary:hover:not(:disabled) {
  background: #009959;
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-secondary {
  background: #fff;
  color: #3f3f46;
  border: 1px solid #e4e4e7;
}

.btn-secondary:hover {
  background: #fafafa;
  border-color: #d4d4d8;
}

.btn-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

/* Viewer */
.loading-viewer {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px;
}

.loading-viewer p {
  margin-top: 16px;
  color: #71717a;
  font-size: 14px;
}

.viewer-container {
  height: 70vh;
  border-radius: 8px;
  overflow: hidden;
}

.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px;
  color: #a1a1aa;
}

.error-state svg {
  width: 48px;
  height: 48px;
  margin-bottom: 16px;
}

.error-state p {
  font-size: 14px;
  margin: 0;
}
</style>

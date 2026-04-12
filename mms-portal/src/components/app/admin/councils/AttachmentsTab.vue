<template>
  <div class="tab-panel">
    <div class="panel-header">
      <h3 class="panel-title">{{ $t('Attachments') }}</h3>
      <Button
        variant="primary"
        size="sm"
        icon-left="mdi:upload"
        @click="openAddAttachment"
      >
        {{ $t('Upload') }}
      </Button>
    </div>

    <!-- Attachments List -->
    <div class="attachments-list">
      <div
        v-for="attachment in attachments"
        :key="attachment.id"
        class="attachment-item"
      >
        <div class="file-icon" :class="getFileColorClass(attachment.name)">
          <Icon :icon="getFileIcon(attachment.name)" class="w-5 h-5" />
        </div>
        <div class="attachment-info">
          <h4 class="attachment-name">{{ attachment.name }}</h4>
          <p class="attachment-meta">{{ attachment.privacyName }}</p>
        </div>
        <div class="attachment-actions">
          <button class="action-btn view" @click="viewAttachment(attachment)" :title="$t('View')">
            <Icon icon="mdi:eye-outline" class="w-4 h-4" />
          </button>
          <button class="action-btn download" @click="downloadAttachment(attachment)" :title="$t('Download')">
            <Icon icon="mdi:download-outline" class="w-4 h-4" />
          </button>
          <button class="action-btn delete" @click="removeAttachment(attachment)" :title="$t('Delete')">
            <Icon icon="mdi:delete-outline" class="w-4 h-4" />
          </button>
        </div>
      </div>
      <div v-if="attachments.length === 0 && !loading" class="empty-state">
        <Icon icon="mdi:paperclip" class="w-16 h-16 text-zinc-200" />
        <p class="text-zinc-400">{{ $t('NoAttachments') }}</p>
      </div>
    </div>

    <!-- Add Attachment Dialog -->
    <Modal
      v-model="attachmentDialog"
      :title="$t('UploadAttachment')"
      size="md"
    >
      <form @submit.prevent="saveAttachment" class="space-y-4">
        <!-- File Upload Zone -->
        <div class="file-upload-wrapper">
          <label class="block text-sm font-medium text-zinc-700 mb-1">
            {{ $t('SelectFile') }}
          </label>
          <div v-if="selectedFile" class="file-preview">
            <div class="file-info">
              <div class="file-icon" :class="getFileColorClass(selectedFile.name)">
                <Icon :icon="getFileIcon(selectedFile.name)" class="w-5 h-5" />
              </div>
              <span class="file-name">{{ selectedFile.name }}</span>
            </div>
            <button type="button" class="file-remove" @click="clearFile" :title="$t('Remove')">
              <Icon icon="mdi:close" class="w-4 h-4" />
            </button>
          </div>
          <label v-else class="file-dropzone" :class="{ 'drag-over': isDragOver }"
            @dragover.prevent="isDragOver = true"
            @dragleave.prevent="isDragOver = false"
            @drop.prevent="handleDrop"
          >
            <input
              type="file"
              ref="fileInput"
              class="hidden"
              @change="handleFileSelect"
            />
            <Icon icon="mdi:cloud-upload-outline" class="dropzone-icon" />
            <span class="dropzone-text">{{ $t('DragFileOrClick') }}</span>
            <span class="dropzone-hint">PDF, DOC, DOCX, XLS, XLSX</span>
          </label>
        </div>

        <Select
          v-model="attachmentForm.privacyId"
          :options="privacies"
          item-text="name"
          item-value="id"
          :label="$t('PrivacyLevel')"
          required
        />
      </form>

      <template #footer>
        <Button variant="outline" @click="attachmentDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="primary" :loading="savingAttachment" :disabled="!selectedFile" @click="saveAttachment">
          {{ $t('Upload') }}
        </Button>
      </template>
    </Modal>

    <!-- Document Viewer Dialog -->
    <Modal
      v-if="viewerDialog"
      v-model="viewerDialog"
      :title="viewingAttachment?.name || $t('ViewFile')"
      size="4xl"
    >
      <div v-if="viewerLoading" class="flex items-center justify-center py-16">
        <Icon icon="mdi:loading" class="w-10 h-10 text-primary animate-spin" />
      </div>
      <div v-else-if="viewerBlobUrl" class="viewer-container">
        <iframe
          v-if="isViewerPdf"
          :src="viewerBlobUrl"
          class="w-full h-[73vh] border-0 rounded-lg"
        />
        <img
          v-else-if="isViewerImage"
          :src="viewerBlobUrl"
          :alt="viewingAttachment?.name"
          class="max-w-full max-h-[73vh] mx-auto rounded-lg"
        />
        <div v-else class="text-center py-16">
          <Icon icon="mdi:file-download" class="w-16 h-16 text-zinc-300 mx-auto mb-4" />
          <p class="text-zinc-500 mb-4">{{ $t('CannotPreview') }}</p>
          <Button variant="primary" @click="downloadAttachment(viewingAttachment)">
            <Icon icon="mdi:download" class="w-4 h-4 me-2" />
            {{ $t('Download') }}
          </Button>
        </div>
      </div>
      <div v-else class="text-center py-16">
        <Icon icon="mdi:file-alert" class="w-16 h-16 text-zinc-300 mx-auto mb-4" />
        <p class="text-zinc-500">{{ $t('ErrorLoadingFile') }}</p>
      </div>
      <template #footer>
        <Button variant="outline" @click="closeViewer">
          {{ $t('Close') }}
        </Button>
        <Button variant="primary" @click="downloadAttachment(viewingAttachment)">
          <Icon icon="mdi:download" class="w-4 h-4 me-2" />
          {{ $t('Download') }}
        </Button>
      </template>
    </Modal>

    <!-- Confirm Delete Dialog -->
    <Modal
      v-model="confirmDialog"
      :title="$t('DeleteAttachment')"
      icon="mdi:file-remove"
      variant="danger"
      size="sm"
    >
      <div class="text-center py-4">
        <Icon icon="mdi:alert-circle" class="w-16 h-16 text-error mx-auto mb-4" />
        <p class="text-zinc-600">{{ $t('ConfirmDeleteAttachment') }}</p>
      </div>
      <template #footer>
        <Button variant="outline" @click="confirmDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="danger" :loading="deleting" @click="confirmDelete">
          {{ $t('Delete') }}
        </Button>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import Select from '@/components/ui/Select.vue'
import Modal from '@/components/ui/Modal.vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'
import AttachmentsService from '@/services/AttachmentsService'
import LookupsService from '@/services/LookupsService'

// Types
interface Attachment {
  id: string
  name: string
  privacyName: string
}

// Props
const props = defineProps<{
  committeeId: string | null
}>()

// Emits
const emit = defineEmits<{
  (e: 'update:count', count: number): void
}>()

// State
const loading = ref(false)
const attachments = ref<Attachment[]>([])
const attachmentDialog = ref(false)
const savingAttachment = ref(false)
const selectedFile = ref<File | null>(null)
const attachmentForm = ref({ privacyId: '' })
const fileInput = ref<HTMLInputElement | null>(null)
const privacies = ref<any[]>([])
const isDragOver = ref(false)

// Delete confirmation
const confirmDialog = ref(false)
const attachmentToDelete = ref<Attachment | null>(null)
const deleting = ref(false)

// Viewer state
const viewerDialog = ref(false)
const viewerLoading = ref(false)
const viewingAttachment = ref<Attachment | null>(null)
const viewerBlobUrl = ref<string | null>(null)

const isViewerPdf = computed(() => {
  const name = viewingAttachment.value?.name?.toLowerCase() || ''
  return name.endsWith('.pdf')
})

const isViewerImage = computed(() => {
  const name = viewingAttachment.value?.name?.toLowerCase() || ''
  return name.endsWith('.png') || name.endsWith('.jpg') || name.endsWith('.jpeg') || name.endsWith('.gif')
})

// Methods
const loadAttachments = async () => {
  if (!props.committeeId) return
  loading.value = true
  try {
    const response = await CouncilCommitteesService.listCommitteeAttachments(props.committeeId)
    attachments.value = response.data || response || []
    emit('update:count', attachments.value.length)
  } catch (error) {
    console.error('Failed to load attachments:', error)
  } finally {
    loading.value = false
  }
}

const loadPrivacies = async () => {
  if (privacies.value.length > 0) return
  try {
    const response = await LookupsService.listPrivacies()
    privacies.value = response.data || response || []
  } catch (error) {
    console.error('Failed to load privacies:', error)
  }
}

const openAddAttachment = async () => {
  await loadPrivacies()
  attachmentForm.value = { privacyId: '' }
  selectedFile.value = null
  isDragOver.value = false
  attachmentDialog.value = true
}

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    selectedFile.value = target.files[0]
  }
}

const handleDrop = (event: DragEvent) => {
  isDragOver.value = false
  const files = event.dataTransfer?.files
  if (files && files.length > 0) {
    selectedFile.value = files[0]
  }
}

const clearFile = () => {
  selectedFile.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

const saveAttachment = async () => {
  if (!props.committeeId || !selectedFile.value || !attachmentForm.value.privacyId) return

  savingAttachment.value = true
  try {
    const formData = new FormData()
    formData.append('file', selectedFile.value)
    await CouncilCommitteesService.addCommitteeAttachment(props.committeeId, formData, attachmentForm.value.privacyId)
    attachmentDialog.value = false
    await loadAttachments()
  } catch (error) {
    console.error('Failed to upload attachment:', error)
  } finally {
    savingAttachment.value = false
  }
}

const removeAttachment = (attachment: Attachment) => {
  attachmentToDelete.value = attachment
  confirmDialog.value = true
}

const confirmDelete = async () => {
  if (!attachmentToDelete.value) return
  deleting.value = true
  try {
    await CouncilCommitteesService.deleteCommitteeAttachment(attachmentToDelete.value.id)
    confirmDialog.value = false
    await loadAttachments()
  } catch (error) {
    console.error('Failed to remove attachment:', error)
  } finally {
    deleting.value = false
  }
}

const viewAttachment = async (attachment: Attachment) => {
  viewingAttachment.value = attachment
  viewerBlobUrl.value = null
  viewerLoading.value = true
  viewerDialog.value = true

  try {
    // Get the attachment query string from API
    const response: any = await AttachmentsService.getAttachment(attachment.id)
    const queryString = response?.data?.data || response?.data || response

    if (queryString && typeof queryString === 'string') {
      // Download the file as blob - axios wraps response in .data
      const response: any = await AttachmentsService.download(queryString)
      const blob = response?.data || response
      viewerBlobUrl.value = URL.createObjectURL(blob)
    }
  } catch (error) {
    console.error('Failed to load attachment:', error)
  } finally {
    viewerLoading.value = false
  }
}

const closeViewer = () => {
  viewerDialog.value = false
  if (viewerBlobUrl.value) {
    URL.revokeObjectURL(viewerBlobUrl.value)
    viewerBlobUrl.value = null
  }
  viewingAttachment.value = null
}

const downloadAttachment = async (attachment: Attachment | null) => {
  if (!attachment) return
  try {
    // Get the attachment query string from API
    const response: any = await AttachmentsService.getAttachment(attachment.id)
    const queryString = response?.data?.data || response?.data || response

    if (queryString && typeof queryString === 'string') {
      // axios wraps response in .data
      const response: any = await AttachmentsService.download(queryString)
      const blob = response?.data || response
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = attachment.name
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      URL.revokeObjectURL(url)
    }
  } catch (error) {
    console.error('Failed to download attachment:', error)
  }
}

const getFileIcon = (filename: string): string => {
  const ext = filename?.split('.').pop()?.toLowerCase() || ''
  const icons: Record<string, string> = {
    pdf: 'mdi:file-pdf-box',
    doc: 'mdi:file-word',
    docx: 'mdi:file-word',
    xls: 'mdi:file-excel',
    xlsx: 'mdi:file-excel',
    ppt: 'mdi:file-powerpoint',
    pptx: 'mdi:file-powerpoint',
    jpg: 'mdi:file-image',
    jpeg: 'mdi:file-image',
    png: 'mdi:file-image',
    gif: 'mdi:file-image',
    zip: 'mdi:folder-zip',
    rar: 'mdi:folder-zip',
    txt: 'mdi:file-document-outline'
  }
  return icons[ext] || 'mdi:file-document-outline'
}

const getFileColorClass = (filename: string): string => {
  const ext = filename?.split('.').pop()?.toLowerCase() || ''
  if (ext === 'pdf') return 'pdf'
  if (ext === 'doc' || ext === 'docx') return 'word'
  if (ext === 'xls' || ext === 'xlsx') return 'excel'
  if (ext === 'ppt' || ext === 'pptx') return 'powerpoint'
  if (['jpg', 'jpeg', 'png', 'gif'].includes(ext)) return 'image'
  if (['zip', 'rar'].includes(ext)) return 'archive'
  return 'default'
}

// Watch for committeeId changes
watch(() => props.committeeId, () => {
  if (props.committeeId) {
    loadAttachments()
  }
}, { immediate: true })

// Expose refresh method
defineExpose({ refresh: loadAttachments })
</script>

<style scoped>
.tab-panel {
  @apply p-4;
}

.panel-header {
  @apply flex items-center justify-between mb-4;
}

.panel-title {
  @apply font-semibold text-zinc-900;
}

/* Attachments List */
.attachments-list {
  @apply space-y-2;
}

.attachment-item {
  @apply flex items-center gap-3 p-3 bg-zinc-50 rounded-xl border border-zinc-100;
  @apply hover:border-zinc-200 transition-colors;
}

.file-icon {
  @apply w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0;
}

.file-icon.pdf { @apply bg-red-100 text-red-600; }
.file-icon.word { @apply bg-blue-100 text-blue-600; }
.file-icon.excel { @apply bg-green-100 text-green-600; }
.file-icon.powerpoint { @apply bg-orange-100 text-orange-600; }
.file-icon.image { @apply bg-purple-100 text-purple-600; }
.file-icon.archive { @apply bg-amber-100 text-amber-600; }
.file-icon.default { @apply bg-zinc-100 text-zinc-600; }

.attachment-info {
  @apply flex-1 min-w-0;
}

.attachment-name {
  @apply font-medium text-zinc-900 text-sm truncate;
}

.attachment-meta {
  @apply text-xs text-zinc-500;
}

.attachment-actions {
  @apply flex items-center gap-1;
}

.action-btn {
  @apply p-2 rounded-lg transition-colors;
}

.action-btn.view { @apply text-zinc-400 hover:text-primary hover:bg-primary/10; }
.action-btn.download { @apply text-zinc-400 hover:text-blue-600 hover:bg-blue-50; }
.action-btn.delete { @apply text-zinc-400 hover:text-error hover:bg-error/10; }

/* File Upload */
.file-upload-wrapper {
  @apply w-full;
}

.file-dropzone {
  @apply flex items-center gap-3 px-4 py-3;
  @apply border border-dashed border-zinc-300 rounded-lg;
  @apply bg-zinc-50/50 cursor-pointer transition-all duration-200;
  @apply hover:border-primary hover:bg-primary/5;
}

.file-dropzone.drag-over {
  @apply border-primary bg-primary/10;
}

.dropzone-icon {
  @apply w-6 h-6 text-zinc-400 flex-shrink-0;
}

.file-dropzone:hover .dropzone-icon,
.file-dropzone.drag-over .dropzone-icon {
  @apply text-primary;
}

.dropzone-text {
  @apply text-sm text-zinc-600;
}

.dropzone-hint {
  @apply text-xs text-zinc-400 ms-auto;
}

.file-preview {
  @apply flex items-center justify-between gap-3 px-4 py-2.5;
  @apply bg-primary/5 border border-primary/20 rounded-lg;
}

.file-preview .file-info {
  @apply flex items-center gap-2 min-w-0;
}

.file-preview .file-icon {
  @apply w-8 h-8 rounded-lg;
}

.file-preview .file-name {
  @apply text-sm text-zinc-700 truncate;
}

.file-remove {
  @apply p-1 text-zinc-400 hover:text-error hover:bg-error/10 rounded transition-colors;
}

/* Viewer */
.viewer-container {
  @apply p-4;
}

/* Empty State */
.empty-state {
  @apply flex flex-col items-center justify-center py-12;
}
</style>

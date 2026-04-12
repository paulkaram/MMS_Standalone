<template>
  <div class="attachments-container">
    <!-- Upload Area -->
    <div v-if="!viewMode" class="upload-area">
      <div class="upload-grid">
        <!-- Drop Zone -->
        <div
          class="drop-zone"
          :class="{ 'dragging': isDragging, 'has-file': selectedFile }"
          @click="triggerFileInput"
          @dragover.prevent="isDragging = true"
          @dragleave.prevent="isDragging = false"
          @drop.prevent="handleDrop"
        >
          <input
            ref="fileInput"
            type="file"
            class="hidden"
            :accept="acceptedTypes"
            @change="handleFileSelect"
          >
          <template v-if="selectedFile">
            <div class="selected-file-preview">
              <div class="file-type-icon" :class="getFileColorClass(selectedFile.type)">
                <Icon :icon="getFileIcon(selectedFile.type)" class="w-6 h-6" />
              </div>
              <div class="file-info">
                <span class="file-name">{{ selectedFile.name }}</span>
                <span class="file-size">{{ formatFileSize(selectedFile.size) }}</span>
              </div>
              <button type="button" class="remove-file" @click.stop="clearFile">
                <Icon icon="mdi:close" class="w-4 h-4" />
              </button>
            </div>
          </template>
          <template v-else>
            <div class="drop-content">
              <div class="upload-icon">
                <Icon icon="mdi:cloud-upload-outline" class="w-8 h-8" />
              </div>
              <p class="drop-text">{{ $t('DropFileHere') }}</p>
              <p class="drop-hint">PDF, Word, Excel, صور (حد أقصى 25MB)</p>
            </div>
          </template>
        </div>

        <!-- Upload Options -->
        <div class="upload-options">
          <CustomSelect
            v-model="selectedPrivacyId"
            :options="privacyLevels"
            :label="$t('PrivacyLevel')"
            :placeholder="'اختر...'"
            value-key="id"
            label-key="name"
            size="small"
          />

          <CustomSelect
            v-if="agendaOptions.length > 0"
            v-model="selectedAgendaId"
            :options="agendaOptions"
            :label="$t('RelatedAgenda')"
            :placeholder="'مرفق عام'"
            value-key="id"
            label-key="title"
            clearable
            :clear-text="'مرفق عام'"
            size="small"
          />

          <button
            type="button"
            class="upload-btn"
            :disabled="!selectedFile || uploading || !meetingId"
            :title="!meetingId ? 'يجب حفظ الاجتماع أولاً' : ''"
            @click="uploadFile"
          >
            <Icon v-if="uploading" icon="mdi:loading" class="w-5 h-5 animate-spin" />
            <Icon v-else icon="mdi:upload" class="w-5 h-5" />
            <span>{{ $t('Upload') }}</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Attachments List -->
    <div class="attachments-section">
      <div class="section-header">
        <h4>{{ $t('Attachments') }}</h4>
        <span class="count-badge">{{ attachments.length }}</span>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="loading-skeleton">
        <div v-for="i in 2" :key="i" class="skeleton-item"></div>
      </div>

      <!-- Empty State -->
      <div v-else-if="attachments.length === 0" class="empty-state">
        <Icon icon="mdi:file-document-outline" class="w-10 h-10" />
        <span>{{ $t('NoAttachments') }}</span>
      </div>

      <!-- Attachments Grid -->
      <div v-else class="attachments-grid">
        <div
          v-for="attachment in attachments"
          :key="attachment.id"
          class="attachment-card"
        >
          <div class="card-content">
            <div class="file-type-icon" :class="getFileColorClass(attachment.type)">
              <Icon :icon="getFileIcon(attachment.type)" class="w-5 h-5" />
            </div>
            <div class="file-info">
              <p class="file-name" :title="attachment.name">{{ attachment.name }}</p>
              <p class="file-meta">
                <span class="file-size">{{ formatFileSize(attachment.size) }}</span>
                <span v-if="attachment.privacyName" class="privacy-badge">{{ attachment.privacyName }}</span>
                <span v-if="attachment.agendaTitle" class="agenda-badge">{{ attachment.agendaTitle }}</span>
              </p>
            </div>
          </div>
          <div class="card-actions">
            <button type="button" @click="viewAttachment(attachment)" :title="$t('View')">
              <Icon icon="mdi:eye-outline" class="w-4 h-4" />
            </button>
            <button type="button" @click="downloadAttachment(attachment)" :title="$t('Download')">
              <Icon icon="mdi:download-outline" class="w-4 h-4" />
            </button>
            <button v-if="!viewMode" type="button" class="delete-btn" @click="deleteAttachment(attachment)" :title="$t('Delete')">
              <Icon icon="mdi:delete-outline" class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- PDF Viewer Modal -->
    <Modal
      v-if="viewerDialog"
      v-model="viewerDialog"
      :title="viewerFileName"
      size="full"
      content-class="p-0"
    >
      <div v-if="viewerLoading" class="flex items-center justify-center py-12">
        <Icon icon="mdi:loading" class="w-8 h-8 text-primary animate-spin" />
      </div>
      <div v-else-if="viewerData" class="h-[85vh]">
        <PdfViewer
          :src="viewerData.url"
          :user-signature="userSignature"
          @save="onSaveAnnotations"
          @saved="onAnnotationsSaved"
          @error="onViewerError"
        />
      </div>
      <div v-else class="flex items-center justify-center py-12 text-zinc-500">
        <div class="text-center">
          <Icon icon="mdi:file-alert" class="w-12 h-12 mx-auto mb-3 text-zinc-300" />
          <p>{{ $t('ErrorLoadingFile') }}</p>
        </div>
      </div>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, nextTick } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import Modal from '@/components/ui/Modal.vue'
import PdfViewer from '@/components/ui/PdfViewer.vue'
import CustomSelect from '@/components/ui/CustomSelect.vue'
import MeetingsService from '@/services/MeetingsService'
import AttachmentsService from '@/services/AttachmentsService'
import LookupsService from '@/services/LookupsService'

export interface Attachment {
  id: string | number
  name: string
  type: string
  size: number
  recordId?: number
  recordTypeId?: number
  privacyId?: number
  privacyName?: string
  recordTypeName?: string
  agendaId?: string
  agendaTitle?: string
}

export interface AgendaItem {
  id: string | number
  title: string
}

interface Props {
  modelValue: Attachment[]
  meetingId?: number | string
  meetingAgendas?: AgendaItem[]
  viewMode?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  viewMode: false,
  meetingId: undefined,
  meetingAgendas: () => []
})

const emit = defineEmits<{
  'update:modelValue': [value: Attachment[]]
}>()

// State
const attachments = ref<Attachment[]>([...props.modelValue])
const loading = ref(false)

// Flag to prevent infinite loop between watchers
let isUpdatingFromProps = false
const uploading = ref(false)
const isDragging = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)
const selectedFile = ref<File | null>(null)
const selectedPrivacyId = ref<string | number | null>(null)
const selectedAgendaId = ref<string | number | null>(null)
const privacyLevels = ref<Array<{ id: string | number; name: string }>>([])

// Computed agenda options from props
const agendaOptions = computed(() => props.meetingAgendas || [])

// File viewer state
const viewerDialog = ref(false)
const viewerLoading = ref(false)
const viewerData = ref<{ url: string; name: string; type: string } | null>(null)
const viewerFileName = ref('')
const userSignature = ref<string | undefined>(undefined) // TODO: Load from user profile

const acceptedTypes = '.pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.png,.jpg,.jpeg,.gif,.txt,.rar,.zip'

// Watch for external changes
watch(() => props.modelValue, (newVal) => {
  if (JSON.stringify(newVal) !== JSON.stringify(attachments.value)) {
    isUpdatingFromProps = true
    attachments.value = [...newVal]
    nextTick(() => {
      isUpdatingFromProps = false
    })
  }
}, { deep: true })

// Watch for local changes and emit
watch(attachments, (newVal) => {
  if (!isUpdatingFromProps) {
    emit('update:modelValue', [...newVal])
  }
}, { deep: true })

// File methods
const triggerFileInput = () => {
  if (!selectedFile.value) {
    fileInput.value?.click()
  }
}

const handleFileSelect = (event: Event) => {
  const input = event.target as HTMLInputElement
  if (input.files?.length) {
    selectedFile.value = input.files[0]
  }
}

const handleDrop = (event: DragEvent) => {
  isDragging.value = false
  const files = event.dataTransfer?.files
  if (files?.length) {
    selectedFile.value = files[0]
  }
}

const clearFile = () => {
  selectedFile.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

const uploadFile = async () => {
  if (!selectedFile.value) return

  // Meeting must be saved first before uploading attachments
  if (!props.meetingId) {
    alert('يجب حفظ الاجتماع أولاً قبل رفع المرفقات')
    return
  }

  uploading.value = true
  try {
    const formData = new FormData()
    formData.append('file', selectedFile.value)

    const result = await MeetingsService.addAttachment(
      props.meetingId.toString(),
      formData,
      selectedPrivacyId.value,
      selectedAgendaId.value?.toString() || undefined
    )

    // API may return updated list or single item
    const resultData = result?.data || result
    if (Array.isArray(resultData)) {
      attachments.value = resultData
    } else {
      attachments.value.push({
        id: resultData.id || Date.now(),
        name: resultData.name || selectedFile.value.name,
        type: resultData.type || getFileExtension(selectedFile.value.name),
        size: resultData.size || selectedFile.value.size,
        privacyId: selectedPrivacyId.value ? Number(selectedPrivacyId.value) : undefined,
        privacyName: privacyLevels.value.find(p => String(p.id) === String(selectedPrivacyId.value))?.name,
        agendaId: selectedAgendaId.value?.toString() || undefined,
        agendaTitle: agendaOptions.value.find(a => a.id === selectedAgendaId.value)?.title
      })
    }

    // Reset
    clearFile()
    selectedAgendaId.value = null
  } catch (error) {
    console.error('Failed to upload file:', error)
  } finally {
    uploading.value = false
  }
}

const getFileExtension = (filename: string): string => {
  const ext = filename.split('.').pop()?.toLowerCase() || ''
  if (ext === 'pdf') return 'Pdf'
  if (ext === 'doc' || ext === 'docx') return 'Word'
  if (ext === 'xls' || ext === 'xlsx') return 'Excel'
  if (['png', 'jpg', 'jpeg', 'gif'].includes(ext)) return 'Image'
  if (['zip', 'rar'].includes(ext)) return 'Archive'
  return ext
}

const viewAttachment = async (attachment: Attachment) => {
  viewerFileName.value = attachment.name
  viewerLoading.value = true
  viewerData.value = null
  viewerDialog.value = true

  try {
    const response = await AttachmentsService.getAttachment(attachment.id) as any
    // Backend returns query string with tokens (tk, hvd) in response.data
    const queryString = response?.data || response
    if (queryString && typeof queryString === 'string') {
      // Build the relative URL for the PDF viewer (axios will prepend baseURL)
      // Using relative path works better with axios interceptors
      viewerData.value = {
        url: `attachments?${queryString}`,
        name: attachment.name,
        type: attachment.type
      }
    } else {
      console.error('Invalid attachment query response:', response)
      viewerData.value = null
    }
  } catch (error) {
    console.error('Failed to load attachment:', error)
    viewerData.value = null
  } finally {
    viewerLoading.value = false
  }
}

const onSaveAnnotations = async (annotations: any[]) => {
  // Annotations saved
}

const onAnnotationsSaved = (newToken?: string) => {
  // Annotations successfully burned into PDF
  // Optionally show success message
  // toast.success('تم حفظ التوقيع بنجاح')
}

const onViewerError = (err: Error) => {
  // Viewer error occurred - optionally show error message
  // toast.error('حدث خطأ في عرض الملف')
}

const downloadAttachment = async (attachment: Attachment) => {
  if (!props.meetingId) return
  try {
    const blob = await MeetingsService.downloadAttachment(props.meetingId.toString(), attachment.id.toString())
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = attachment.name
    link.click()
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('Failed to download attachment:', error)
  }
}

const deleteAttachment = async (attachment: Attachment) => {
  if (!props.meetingId) return
  try {
    await MeetingsService.deleteAttachment(props.meetingId.toString(), attachment.id.toString())
    attachments.value = attachments.value.filter(a => a.id !== attachment.id)
  } catch (error) {
    console.error('Failed to delete attachment:', error)
  }
}

const getFileIcon = (fileType: string): string => {
  const type = fileType?.toLowerCase() || ''
  if (type === 'pdf' || type.includes('pdf')) return 'mdi:file-pdf-box'
  if (type === 'word' || type === 'doc' || type === 'docx' || type.includes('word') || type.includes('doc')) return 'mdi:file-word'
  if (type === 'excel' || type === 'xls' || type === 'xlsx' || type.includes('excel') || type.includes('xls') || type.includes('sheet')) return 'mdi:file-excel'
  if (type === 'image' || type === 'png' || type === 'jpg' || type === 'jpeg' || type.includes('image')) return 'mdi:file-image'
  if (type === 'zip' || type === 'rar' || type.includes('zip') || type.includes('rar') || type.includes('compressed')) return 'mdi:folder-zip'
  return 'mdi:file-document-outline'
}

const getFileColorClass = (fileType: string): string => {
  const type = fileType?.toLowerCase() || ''
  if (type === 'pdf' || type.includes('pdf')) return 'pdf'
  if (type === 'word' || type === 'doc' || type === 'docx' || type.includes('word') || type.includes('doc')) return 'word'
  if (type === 'excel' || type === 'xls' || type === 'xlsx' || type.includes('excel') || type.includes('xls') || type.includes('sheet')) return 'excel'
  if (type === 'image' || type === 'png' || type === 'jpg' || type === 'jpeg' || type.includes('image')) return 'image'
  if (type === 'zip' || type === 'rar' || type.includes('zip') || type.includes('rar')) return 'archive'
  return 'default'
}

const formatFileSize = (bytes: number): string => {
  if (!bytes) return '-'
  const units = ['B', 'KB', 'MB', 'GB']
  let unitIndex = 0
  let size = bytes
  while (size >= 1024 && unitIndex < units.length - 1) {
    size /= 1024
    unitIndex++
  }
  return `${size.toFixed(1)} ${units[unitIndex]}`
}

const loadPrivacyLevels = async () => {
  try {
    const response = await LookupsService.getPrivacyLevels()
    // Handle different response structures and filter out null items
    const data = response?.data || response || []
    privacyLevels.value = Array.isArray(data) ? data.filter((p: any) => p && p.id) : []
    if (privacyLevels.value.length > 0 && !selectedPrivacyId.value) {
      selectedPrivacyId.value = privacyLevels.value[0].id
    }
  } catch (error) {
    console.error('Failed to load privacy levels:', error)
  }
}

const loadAttachments = async () => {
  if (!props.meetingId) return
  loading.value = true
  try {
    const response = await MeetingsService.getAttachments(props.meetingId.toString())
    const data = response?.data || response || []
    attachments.value = Array.isArray(data) ? data : []
  } catch (error) {
    console.error('Failed to load attachments:', error)
  } finally {
    loading.value = false
  }
}

// Lifecycle
onMounted(() => {
  loadPrivacyLevels()
  if (props.meetingId) {
    loadAttachments()
  }
})
</script>

<style scoped>
.attachments-container {
  @apply space-y-4;
}

/* Upload Area */
.upload-area {
  @apply bg-zinc-50/80 rounded-xl p-4 border border-zinc-100;
}

.upload-grid {
  @apply flex flex-col lg:flex-row gap-4;
}

/* Drop Zone */
.drop-zone {
  @apply flex-1 border-2 border-dashed border-zinc-300 rounded-xl p-6 cursor-pointer;
  @apply hover:border-primary/50 hover:bg-white transition-all duration-200;
  min-height: 120px;
}

.drop-zone.dragging {
  @apply border-primary bg-primary/5;
}

.drop-zone.has-file {
  @apply border-primary/30 bg-white cursor-default;
}

.drop-content {
  @apply flex flex-col items-center justify-center h-full text-center;
}

.upload-icon {
  @apply w-14 h-14 rounded-full bg-zinc-100 flex items-center justify-center text-zinc-400 mb-3;
}

.drop-text {
  @apply text-sm text-zinc-600 font-medium;
}

.drop-hint {
  @apply text-xs text-zinc-400 mt-1;
}

/* Selected File Preview */
.selected-file-preview {
  @apply flex items-center gap-3 h-full;
}

.file-type-icon {
  @apply w-12 h-12 rounded-xl flex items-center justify-center flex-shrink-0;
}

.file-type-icon.pdf { @apply bg-red-100 text-red-600; }
.file-type-icon.word { @apply bg-blue-100 text-blue-600; }
.file-type-icon.excel { @apply bg-green-100 text-green-600; }
.file-type-icon.image { @apply bg-amber-100 text-amber-600; }
.file-type-icon.archive { @apply bg-purple-100 text-purple-600; }
.file-type-icon.default { @apply bg-zinc-100 text-zinc-600; }

.file-info {
  @apply flex-1 min-w-0;
}

.file-info .file-name {
  @apply text-sm font-medium text-zinc-900 truncate;
}

.file-info .file-size {
  @apply text-xs text-zinc-500;
}

.remove-file {
  @apply w-8 h-8 rounded-full flex items-center justify-center text-zinc-400;
  @apply hover:bg-red-50 hover:text-red-500 transition-colors;
}

/* Upload Options */
.upload-options {
  @apply flex flex-col gap-3 lg:w-64;
}

.option-group {
  @apply space-y-1;
}

.option-group label {
  @apply block text-xs font-medium text-zinc-500;
}

/* Upload Button */
.upload-btn {
  @apply flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg font-medium text-sm;
  @apply bg-primary text-white;
  @apply hover:bg-primary-600 active:bg-primary-700;
  @apply disabled:opacity-50 disabled:cursor-not-allowed;
  @apply transition-all duration-200;
}

/* Attachments Section */
.attachments-section {
  @apply bg-white rounded-xl border border-zinc-100;
}

.section-header {
  @apply flex items-center gap-2 px-4 py-3 border-b border-zinc-100;
}

.section-header h4 {
  @apply text-sm font-semibold text-zinc-900;
}

.count-badge {
  @apply px-2 py-0.5 rounded-full text-xs font-medium bg-zinc-100 text-zinc-600;
}

/* Loading Skeleton */
.loading-skeleton {
  @apply p-4 space-y-3;
}

.skeleton-item {
  @apply h-16 bg-zinc-100 rounded-lg animate-pulse;
}

/* Empty State */
.empty-state {
  @apply flex flex-col items-center justify-center py-10 text-zinc-400;
}

.empty-state span {
  @apply text-sm mt-2;
}

/* Attachments Grid */
.attachments-grid {
  @apply divide-y divide-zinc-50;
}

.attachment-card {
  @apply flex items-center justify-between px-4 py-3 hover:bg-zinc-50/50 transition-colors;
}

.card-content {
  @apply flex items-center gap-3 flex-1 min-w-0;
}

.card-content .file-type-icon {
  @apply w-10 h-10 rounded-lg;
}

.card-content .file-info {
  @apply flex-1 min-w-0;
}

.card-content .file-name {
  @apply text-sm font-medium text-zinc-900 truncate;
}

.card-content .file-meta {
  @apply text-xs text-zinc-500 flex items-center gap-2 mt-0.5;
}

.privacy-badge {
  @apply px-1.5 py-0.5 rounded bg-zinc-100 text-zinc-600 text-[10px] font-medium;
}

.agenda-badge {
  @apply px-1.5 py-0.5 rounded bg-primary/10 text-primary text-[10px] font-medium;
}

/* Card Actions */
.card-actions {
  @apply flex items-center gap-1;
}

.card-actions button {
  @apply w-8 h-8 rounded-lg flex items-center justify-center text-zinc-400;
  @apply hover:bg-zinc-100 hover:text-zinc-600 transition-colors;
}

.card-actions .delete-btn:hover {
  @apply bg-red-50 text-red-500;
}
</style>

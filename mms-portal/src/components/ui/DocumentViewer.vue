<template>
  <div class="document-viewer h-full flex flex-col">
    <!-- Loading -->
    <div v-if="loading" class="flex-1 flex items-center justify-center bg-zinc-50">
      <div class="text-center">
        <div class="w-12 h-12 border-4 border-primary border-t-transparent rounded-full animate-spin mx-auto"></div>
        <p class="mt-4 text-zinc-500">{{ $t('Loading') }}</p>
      </div>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex-1 flex items-center justify-center bg-zinc-50">
      <div class="text-center">
        <div class="w-16 h-16 rounded-full bg-red-100 flex items-center justify-center mx-auto mb-4">
          <svg class="w-8 h-8 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>
        <p class="text-zinc-600 mb-4">{{ error }}</p>
        <button
          type="button"
          class="px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-600"
          @click="loadDocument"
        >
          {{ $t('Retry') }}
        </button>
      </div>
    </div>

    <!-- Unsupported (Download) -->
    <div v-else-if="toDownload" class="flex-1 flex items-center justify-center bg-zinc-50">
      <div class="text-center">
        <div class="w-16 h-16 rounded-full bg-zinc-100 flex items-center justify-center mx-auto mb-4">
          <svg class="w-8 h-8 text-zinc-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        </div>
        <p class="text-zinc-600 mb-2">{{ $t('FileIsNotSupported') }}</p>
        <button
          v-if="!hideDownloadAction"
          type="button"
          class="px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-600 flex items-center gap-2 mx-auto"
          :disabled="downloading"
          @click="download"
        >
          <svg v-if="downloading" class="w-5 h-5 animate-spin" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <svg v-else class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
          </svg>
          {{ $t('Download') }}
        </button>
      </div>
    </div>

    <!-- Document Viewer (PDF.js) -->
    <iframe
      v-else-if="documentUrl"
      ref="viewerFrame"
      :src="documentUrl"
      :name="name"
      class="flex-1 w-full border-0"
      @load="onFrameLoad"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import AttachmentsService from '@/services/AttachmentsService'
import { downloadFile } from '@/helpers/downloader'

const props = withDefaults(
  defineProps<{
    query: string | null       // Query string from backend (contains auth tokens)
    name?: string              // Iframe name
    isDialog?: boolean         // Whether viewer is in a dialog
    isClientSide?: boolean     // For blob URLs
  }>(),
  {
    query: null,
    name: 'documentViewer',
    isDialog: false,
    isClientSide: false
  }
)

const emit = defineEmits<{
  'loaded': []
  'error': [error: Error]
  'update': []
  'close': []
}>()

// State
const loading = ref(true)
const error = ref('')
const downloading = ref(false)
const hideDownloadAction = ref(false)
const viewerFrame = ref<HTMLIFrameElement | null>(null)
const isLoadedFirstTime = ref(false)

// Constants
const renderQueryString = '?norender=true'
const renderAppendQueryString = '&norender=true'

// Get viewer URL from environment
const viewerUrl = computed(() => {
  if (props.query && props.query.toLowerCase().startsWith('http')) {
    // If query is a full URL, use viewer with file= parameter
    const baseViewer = import.meta.env.VITE_VIEWER || '/viewer/web/viewer.html?locale=ar&file='
    return baseViewer.replace(/file.*/i, 'file=')
  } else if (props.isClientSide && props.query && props.query.toLowerCase().startsWith('blob')) {
    // For blob URLs, return as-is
    return props.query
  }
  return import.meta.env.VITE_VIEWER || '/viewer/web/viewer.html?locale=ar&file=http://localhost:1010/api/attachments?'
})

// Build document URL for iframe
const documentUrl = computed(() => {
  if (!props.query) return ''

  if (props.isClientSide && props.query.toLowerCase().startsWith('blob')) {
    // Wrap blob URL in PDF.js viewer for proper rendering
    return `/viewer/web/viewer.html?file=${encodeURIComponent(props.query)}`
  }

  if (!toDownload.value) {
    if (props.query.toLowerCase().startsWith('http')) {
      return `${viewerUrl.value}${props.query}`
    }
    return `${viewerUrl.value}${encodeURIComponent(props.query)}`
  }

  return ''
})

// Check if document is supported by viewer
const isSupportedByViewer = computed(() => {
  if (props.query) {
    // Blob URLs are always supported (already fetched as PDF)
    if (props.isClientSide && props.query.toLowerCase().startsWith('blob')) return true
    return !(props.query.includes(renderQueryString) || props.query.includes(renderAppendQueryString))
  }
  return false
})

// Should download instead of display
const toDownload = computed(() => {
  return !isSupportedByViewer.value && props.isDialog
})

// Should show unsupported message (used in template conditionally)
const _toNotDownload = computed(() => {
  return !isSupportedByViewer.value && !props.isDialog
})

// Methods
const loadDocument = () => {
  loading.value = true
  error.value = ''

  if (!props.query) {
    loading.value = false
    return
  }

  if (toDownload.value) {
    download()
  } else {
    // Document will load via iframe
    loading.value = false
  }
}

const onFrameLoad = () => {
  loading.value = false

  if (!isLoadedFirstTime.value) {
    isLoadedFirstTime.value = true
    emit('loaded')
    return
  }

  emit('update')
}

const download = async () => {
  if (!props.query) return

  downloading.value = true

  try {
    const response = await AttachmentsService.download(props.query)
    downloadFile(response)
    hideDownloadAction.value = true
    emit('close')
  } catch (err: any) {
    error.value = err.message || 'Failed to download file'
    emit('error', err)
  } finally {
    downloading.value = false
  }
}

// Watch for query changes
watch(() => props.query, () => {
  hideDownloadAction.value = false
  isLoadedFirstTime.value = false
  loadDocument()
})

// Lifecycle
onMounted(() => {
  loadDocument()
})
</script>

<style scoped>
.document-viewer {
  min-height: 400px;
}

iframe {
  min-height: 100%;
}
</style>

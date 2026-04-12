<template>
  <div class="pdf-viewer" ref="containerRef">
    <!-- Custom Toolbar -->
    <div class="pdf-toolbar" :class="{ 'has-actions': hasToolbarActions }">
      <!-- Right side (RTL): File info + Status -->
      <div class="toolbar-section file-info">
        <Icon icon="description" class="w-4 h-4 file-icon" />
        <span class="file-name">{{ fileName || 'PDF Document' }}</span>
        <!-- Status badge slot -->
        <slot name="status-badge"></slot>
      </div>

      <!-- Center: Extended actions slot (for version selector, action buttons) -->
      <div class="toolbar-section center-actions">
        <slot name="toolbar-actions"></slot>
      </div>

      <!-- Left side (RTL): Download -->
      <div class="toolbar-section actions">
        <button type="button" class="toolbar-btn" @click="downloadPdf" :title="$t('Download')">
          <Icon icon="download" class="w-4 h-4" />
        </button>
      </div>
    </div>

    <!-- PDF Container -->
    <div class="pdf-container">
      <!-- Loading -->
      <div v-if="loading" class="loading-overlay">
        <div class="loader"></div>
        <p>{{ $t('Loading') }}...</p>
      </div>

      <!-- Error -->
      <div v-else-if="error" class="error-state">
        <Icon icon="error" class="w-12 h-12 error-icon" />
        <p>{{ error }}</p>
        <button type="button" class="retry-btn" @click="loadPdf">
          {{ $t('Retry') }}
        </button>
      </div>

      <!-- PDF.js Viewer iframe -->
      <iframe
        v-else-if="viewerUrl"
        ref="iframeRef"
        :src="viewerUrl"
        class="pdf-iframe"
        @load="onIframeLoad"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted, computed, useSlots } from 'vue'
import { mainApiAxios } from '@/plugins/axios'
import Icon from '@/components/ui/Icon.vue'
import { useUserStore } from '@/stores/user'

const slots = useSlots()
const hasToolbarActions = computed(() => !!slots['toolbar-actions'])
const userStore = useUserStore()
const pdfLocale = computed(() => userStore.language === 'ar' ? 'ar' : 'en-US')

const props = withDefaults(
  defineProps<{
    src: string
    fileName?: string
    /** Default zoom level: 'page-width', 'page-fit', 'auto', or a number (e.g., 75 for 75%) */
    defaultZoom?: string | number
  }>(),
  {
    fileName: '',
    defaultZoom: 130
  }
)

const emit = defineEmits<{
  'error': [error: Error]
  'loaded': []
}>()

// Refs
const containerRef = ref<HTMLElement | null>(null)
const iframeRef = ref<HTMLIFrameElement | null>(null)

// State
const loading = ref(true)
const error = ref('')
const blobUrl = ref<string | null>(null)

// AbortController to cancel pending requests
let abortController: AbortController | null = null

// Build viewer URL with blob
const viewerUrl = computed(() => {
  if (!blobUrl.value) return ''
  // Use the PDF.js viewer with the blob URL and zoom level
  const zoom = props.defaultZoom || 'page-width'
  return `/viewer/web/viewer.html?locale=${pdfLocale.value}&zoom=${zoom}&file=${encodeURIComponent(blobUrl.value)}`
})

const loadPdf = async () => {
  // Cancel any pending request to prevent race conditions and 401 issues
  if (abortController) {
    abortController.abort()
  }
  abortController = new AbortController()

  loading.value = true
  error.value = ''

  // Revoke previous blob URL
  if (blobUrl.value) {
    URL.revokeObjectURL(blobUrl.value)
    blobUrl.value = null
  }

  try {
    // Prepare the URL - mainApiAxios has baseURL configured
    let url = props.src

    // Remove leading /api/ or / if present to avoid duplication
    if (url.startsWith('/api/')) {
      url = url.substring(5)
    } else if (url.startsWith('/')) {
      url = url.substring(1)
    }

    // Use mainApiAxios which has token refresh interceptor
    // The interceptor handles Authorization header and 401 token refresh
    const response = await mainApiAxios.get(url, {
      responseType: 'blob',
      headers: {
        'Accept': 'application/pdf'
      },
      signal: abortController.signal
    })

    // Create blob URL for the viewer
    blobUrl.value = URL.createObjectURL(response.data)

    loading.value = false
  } catch (err: any) {
    // Ignore abort errors - these are intentional cancellations
    if (err.name === 'AbortError' || err.name === 'CanceledError' || err.code === 'ERR_CANCELED') {
      return
    }
    console.error('[PdfViewer] Failed to load PDF:', err)
    error.value = err.message || 'فشل تحميل المستند'
    emit('error', err)
    loading.value = false
  }
}

const onIframeLoad = () => {
  emit('loaded')
}

const downloadPdf = () => {
  if (blobUrl.value) {
    const link = document.createElement('a')
    link.href = blobUrl.value
    link.download = props.fileName || 'document.pdf'
    link.click()
  }
}

// Cleanup
onUnmounted(() => {
  // Cancel any pending request
  if (abortController) {
    abortController.abort()
    abortController = null
  }
  // Revoke blob URL
  if (blobUrl.value) {
    URL.revokeObjectURL(blobUrl.value)
  }
})

// Watchers
watch(() => props.src, () => {
  if (props.src) {
    loadPdf()
  }
})

// Lifecycle
onMounted(() => {
  if (props.src) {
    loadPdf()
  }
})
</script>

<style scoped>
.pdf-viewer {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: 0;
  background: #525659;
  position: relative;
  border-radius: 8px;
  overflow: hidden;
}

/* Toolbar */
.pdf-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  background: #323639;
  border-bottom: 1px solid #444;
  z-index: 10;
  flex-shrink: 0;
  gap: 12px;
}

.toolbar-section {
  display: flex;
  align-items: center;
  gap: 6px;
}

.toolbar-section.file-info {
  flex-shrink: 0;
  min-width: 0;
  max-width: 280px;
}

.file-icon {
  color: #aaa;
  flex-shrink: 0;
}

.file-name {
  font-size: 13px;
  font-weight: 500;
  color: #eee;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.toolbar-section.center-actions {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
  min-width: 0;
}

.toolbar-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  color: #ccc;
  border-radius: 4px;
  border: none;
  background: transparent;
  cursor: pointer;
  transition: all 0.15s ease;
}

.toolbar-btn:hover {
  background: #444;
  color: #fff;
}

.toolbar-btn:active {
  background: #555;
}

/* PDF Container */
.pdf-container {
  flex: 1;
  min-height: 0;
  display: flex;
  position: relative;
  background: #525659;
}

.loading-overlay {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  gap: 16px;
}

.loader {
  width: 48px;
  height: 48px;
  border: 4px solid rgba(255, 255, 255, 0.2);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.loading-overlay p {
  color: #ccc;
  font-size: 14px;
}

.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  gap: 16px;
}

.error-icon {
  color: #ef4444;
}

.error-state p {
  color: #ccc;
  font-size: 14px;
}

.retry-btn {
  padding: 8px 16px;
  background: #803580;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
}

.retry-btn:hover {
  background: #9a4a9a;
}

/* PDF Iframe */
.pdf-iframe {
  flex: 1;
  width: 100%;
  height: 100%;
  border: none;
}
</style>

<!-- Light mode styles -->
<style>
.meeting-room.light .pdfViewerFull .pdf-viewer {
  background: #e4e4e7 !important;
}

.meeting-room.light .pdfViewerFull .pdf-toolbar {
  background: #fff !important;
  border-bottom-color: #e4e4e7 !important;
}

.meeting-room.light .pdfViewerFull .file-icon {
  color: #71717a !important;
}

.meeting-room.light .pdfViewerFull .file-name {
  color: #fff !important;
}

.meeting-room.light .pdfViewerFull .file-icon {
  color: #006d4b !important;
}

.meeting-room.light .pdfViewerFull .toolbar-btn {
  color: #52525b !important;
}

.meeting-room.light .pdfViewerFull .toolbar-btn:hover {
  background: #f4f4f5 !important;
  color: #27272a !important;
}

.meeting-room.light .pdfViewerFull .pdf-container {
  background: #e4e4e7 !important;
}

.meeting-room.light .pdfViewerFull .loading-overlay p,
.meeting-room.light .pdfViewerFull .error-state p {
  color: #71717a !important;
}

.meeting-room.light .pdfViewerFull .loader {
  border-color: rgba(128, 53, 128, 0.2) !important;
  border-top-color: #803580 !important;
}
</style>

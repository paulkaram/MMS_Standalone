<template>
  <div class="file-viewer h-full flex flex-col">
    <!-- Header -->
    <div v-if="showHeader" class="flex items-center justify-between px-4 py-2 bg-zinc-100 border-b border-zinc-200">
      <div class="flex items-center gap-2">
        <Icon :icon="fileIcon" class="w-5 h-5 text-zinc-500" />
        <span class="font-medium text-zinc-700">{{ fileName }}</span>
      </div>
      <div class="flex items-center gap-2">
        <button
          v-if="canDownload"
          type="button"
          class="p-1.5 hover:bg-zinc-200 rounded"
          @click="download"
        >
          <Icon icon="mdi:download" class="w-5 h-5 text-zinc-600" />
        </button>
        <button
          v-if="canPrint"
          type="button"
          class="p-1.5 hover:bg-zinc-200 rounded"
          @click="print"
        >
          <Icon icon="mdi:printer" class="w-5 h-5 text-zinc-600" />
        </button>
        <button
          v-if="canFullscreen"
          type="button"
          class="p-1.5 hover:bg-zinc-200 rounded"
          @click="toggleFullscreen"
        >
          <Icon :icon="isFullscreen ? 'mdi:fullscreen-exit' : 'mdi:fullscreen'" class="w-5 h-5 text-zinc-600" />
        </button>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex-1 flex items-center justify-center">
      <div class="text-center">
        <Icon icon="mdi:loading" class="w-12 h-12 text-primary animate-spin mx-auto" />
        <p class="mt-4 text-zinc-500">{{ $t('Loading') }}</p>
      </div>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex-1 flex items-center justify-center">
      <div class="text-center">
        <Icon icon="mdi:alert-circle" class="w-12 h-12 text-error mx-auto" />
        <p class="mt-4 text-zinc-600">{{ error }}</p>
        <Button v-if="canRetry" variant="outline" class="mt-4" @click="loadFile">
          {{ $t('Retry') }}
        </Button>
      </div>
    </div>

    <!-- Content -->
    <div v-else class="flex-1 overflow-auto">
      <!-- PDF Viewer -->
      <iframe
        v-if="isPdf"
        ref="pdfFrame"
        :src="fileUrl"
        class="w-full h-full border-0"
        :title="fileName"
      />

      <!-- Image Viewer -->
      <div v-else-if="isImage" class="h-full flex items-center justify-center p-4 bg-zinc-50">
        <img
          :src="fileUrl"
          :alt="fileName"
          class="max-w-full max-h-full object-contain"
        />
      </div>

      <!-- Video Viewer -->
      <div v-else-if="isVideo" class="h-full flex items-center justify-center p-4 bg-black">
        <video
          :src="fileUrl"
          controls
          class="max-w-full max-h-full"
        />
      </div>

      <!-- Audio Player -->
      <div v-else-if="isAudio" class="h-full flex items-center justify-center p-4">
        <div class="text-center">
          <Icon icon="mdi:music-note" class="w-20 h-20 text-zinc-300 mx-auto mb-4" />
          <audio :src="fileUrl" controls class="w-full max-w-md" />
        </div>
      </div>

      <!-- Office Document (via iframe or Google Docs Viewer) -->
      <iframe
        v-else-if="isOfficeDocument && officeViewerUrl"
        :src="officeViewerUrl"
        class="w-full h-full border-0"
        :title="fileName"
      />

      <!-- Unsupported Format -->
      <div v-else class="h-full flex items-center justify-center">
        <div class="text-center">
          <Icon icon="mdi:file-question" class="w-20 h-20 text-zinc-300 mx-auto" />
          <p class="mt-4 text-zinc-600">{{ $t('UnsupportedFormat') }}</p>
          <p class="text-sm text-zinc-500 mt-2">{{ fileName }}</p>
          <Button variant="primary" class="mt-4" @click="download">
            <Icon icon="mdi:download" class="w-4 h-4 mr-2" />
            {{ $t('Download') }}
          </Button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Button from './Button.vue'

interface FileQuery {
  id?: string
  url?: string
  queryString?: string  // Query string for PDF.js viewer (contains auth tokens)
  name?: string
  type?: string
  mimeType?: string
}

const props = withDefaults(
  defineProps<{
    query: FileQuery
    name?: string
    showHeader?: boolean
    canDownload?: boolean
    canPrint?: boolean
    canFullscreen?: boolean
    canRetry?: boolean
  }>(),
  {
    showHeader: true,
    canDownload: true,
    canPrint: true,
    canFullscreen: true,
    canRetry: true
  }
)

const emit = defineEmits(['loaded', 'error'])

// State
const loading = ref(true)
const error = ref('')
const isFullscreen = ref(false)
const pdfFrame = ref<HTMLIFrameElement | null>(null)
const fileUrl = ref('')

// Computed
const fileName = computed(() => props.name || props.query?.name || 'document')

const fileExtension = computed(() => {
  const name = fileName.value
  return name.split('.').pop()?.toLowerCase() || ''
})

const mimeType = computed(() => props.query?.mimeType || props.query?.type || '')

const isPdf = computed(() => {
  return fileExtension.value === 'pdf' || mimeType.value === 'application/pdf'
})

const isImage = computed(() => {
  const imageExts = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp', 'svg']
  return imageExts.includes(fileExtension.value) || mimeType.value.startsWith('image/')
})

const isVideo = computed(() => {
  const videoExts = ['mp4', 'webm', 'ogg', 'mov', 'avi']
  return videoExts.includes(fileExtension.value) || mimeType.value.startsWith('video/')
})

const isAudio = computed(() => {
  const audioExts = ['mp3', 'wav', 'ogg', 'aac', 'm4a']
  return audioExts.includes(fileExtension.value) || mimeType.value.startsWith('audio/')
})

const isOfficeDocument = computed(() => {
  const officeExts = ['doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx']
  return officeExts.includes(fileExtension.value)
})

const officeViewerUrl = computed(() => {
  if (!fileUrl.value) return ''
  // Use Microsoft Office Online viewer or Google Docs viewer
  return `https://view.officeapps.live.com/op/embed.aspx?src=${encodeURIComponent(fileUrl.value)}`
})

const fileIcon = computed(() => {
  if (isPdf.value) return 'mdi:file-pdf-box'
  if (isImage.value) return 'mdi:file-image'
  if (isVideo.value) return 'mdi:file-video'
  if (isAudio.value) return 'mdi:file-music'
  if (fileExtension.value === 'doc' || fileExtension.value === 'docx') return 'mdi:file-word'
  if (fileExtension.value === 'xls' || fileExtension.value === 'xlsx') return 'mdi:file-excel'
  if (fileExtension.value === 'ppt' || fileExtension.value === 'pptx') return 'mdi:file-powerpoint'
  return 'mdi:file-document'
})

// Methods
const loadFile = async () => {
  loading.value = true
  error.value = ''

  try {
    if (props.query?.url) {
      // URL should already contain authentication tokens from the backend
      fileUrl.value = props.query.url
    } else if (props.query?.id) {
      // For ID-based queries, fetch the authenticated URL from the backend
      const apiUrl = import.meta.env.VITE_MAIN_API || '/api/'
      fileUrl.value = `${apiUrl}attachments/${props.query.id}/view`
    } else {
      throw new Error('No file URL or ID provided')
    }

    emit('loaded')
  } catch (err: any) {
    error.value = err.message || 'Failed to load file'
    emit('error', err)
  } finally {
    loading.value = false
  }
}

const download = () => {
  if (!fileUrl.value) return

  const link = document.createElement('a')
  link.href = fileUrl.value
  link.download = fileName.value
  link.click()
}

const print = () => {
  if (pdfFrame.value) {
    pdfFrame.value.contentWindow?.print()
  } else {
    window.print()
  }
}

const toggleFullscreen = () => {
  const elem = document.documentElement
  if (!isFullscreen.value) {
    elem.requestFullscreen?.()
  } else {
    document.exitFullscreen?.()
  }
  isFullscreen.value = !isFullscreen.value
}

// Watch for query changes
watch(
  () => props.query,
  () => {
    loadFile()
  },
  { deep: true }
)

// Lifecycle
onMounted(() => {
  loadFile()
})
</script>

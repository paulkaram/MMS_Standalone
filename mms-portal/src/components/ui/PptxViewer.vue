<template>
  <div class="pptx-viewer" ref="containerRef">
    <!-- Toolbar -->
    <div class="pptx-toolbar">
      <!-- File info -->
      <div class="toolbar-section file-info">
        <Icon icon="co_present" class="w-4 h-4 file-icon" />
        <span class="file-name">{{ fileName || 'عرض تقديمي' }}</span>
      </div>

      <!-- Navigation -->
      <div class="toolbar-section navigation">
        <button
          type="button"
          class="nav-btn"
          :disabled="currentSlide <= 1"
          @click="prevSlide"
          title="الشريحة السابقة"
        >
          <Icon icon="chevron_right" class="w-4.5 h-4.5" />
        </button>
        <span class="slide-counter">
          {{ currentSlide }} / {{ totalSlides }}
        </span>
        <button
          type="button"
          class="nav-btn"
          :disabled="currentSlide >= totalSlides"
          @click="nextSlide"
          title="الشريحة التالية"
        >
          <Icon icon="chevron_left" class="w-4.5 h-4.5" />
        </button>
      </div>

      <!-- Actions -->
      <div class="toolbar-section actions">
        <button
          type="button"
          class="toolbar-btn"
          @click="toggleFullscreen"
          :title="isFullscreen ? 'إنهاء ملء الشاشة' : 'ملء الشاشة'"
        >
          <Icon v-if="!isFullscreen" icon="fullscreen" class="w-4 h-4" />
          <Icon v-else icon="fullscreen_exit" class="w-4 h-4" />
        </button>
        <button type="button" class="toolbar-btn" @click="downloadPptx" title="تحميل">
          <Icon icon="download" class="w-4 h-4" />
        </button>
      </div>
    </div>

    <!-- Slide Container -->
    <div class="pptx-container">
      <!-- Loading state -->
      <div v-if="loading" class="loading-overlay">
        <div class="loader"></div>
        <p>جاري تحميل العرض التقديمي...</p>
      </div>

      <!-- Error state -->
      <div v-else-if="error" class="error-state">
        <Icon icon="error" class="w-12 h-12 error-icon" />
        <p>{{ error }}</p>
        <button type="button" class="retry-btn" @click="loadPptx">
          إعادة المحاولة
        </button>
      </div>

      <!-- Slides Viewer -->
      <div v-else class="slides-wrapper">
        <!-- Main Slide Display -->
        <div class="slide-display" ref="slideContainerRef">
          <div
            v-if="slides.length > 0"
            class="slide-content"
            v-html="slides[currentSlide - 1]"
          ></div>
          <div v-else class="empty-slide">
            <Icon icon="co_present" class="w-16 h-16 empty-icon" />
            <p>لا توجد شرائح للعرض</p>
          </div>
        </div>

        <!-- Slide Thumbnails -->
        <div v-if="slides.length > 1" class="slide-thumbnails">
          <button
            v-for="(slide, index) in slides"
            :key="index"
            class="thumbnail"
            :class="{ active: currentSlide === index + 1 }"
            @click="goToSlide(index + 1)"
            :title="`شريحة ${index + 1}`"
          >
            <span class="thumbnail-number">{{ index + 1 }}</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { mainApiAxios } from '@/plugins/axios'
import Icon from '@/components/ui/Icon.vue'

const props = withDefaults(
  defineProps<{
    src: string
    fileName?: string
  }>(),
  {
    fileName: ''
  }
)

const emit = defineEmits<{
  error: [error: Error]
  loaded: []
  slideChange: [slideNumber: number]
}>()

// Refs
const containerRef = ref<HTMLElement | null>(null)
const slideContainerRef = ref<HTMLElement | null>(null)

// State
const loading = ref(false)
const error = ref('')
const slides = ref<string[]>([])
const currentSlide = ref(1)
const totalSlides = ref(0)
const isFullscreen = ref(false)
const blobUrl = ref<string | null>(null)

// AbortController for canceling requests
let abortController: AbortController | null = null

// Load PPTX file
const loadPptx = async () => {
  if (abortController) {
    abortController.abort()
  }
  abortController = new AbortController()

  loading.value = true
  error.value = ''
  slides.value = []
  currentSlide.value = 1
  totalSlides.value = 0

  try {
    // Prepare URL
    let url = props.src
    if (url.startsWith('/api/')) {
      url = url.substring(5)
    } else if (url.startsWith('/')) {
      url = url.substring(1)
    }

    // Fetch the PPTX file as blob
    const response = await mainApiAxios.get(url, {
      responseType: 'blob',
      signal: abortController.signal
    })

    // Store blob URL for download
    if (blobUrl.value) {
      URL.revokeObjectURL(blobUrl.value)
    }
    blobUrl.value = URL.createObjectURL(response.data)

    // Parse PPTX using pptxjs
    await parsePptx(response.data)

    loading.value = false
    emit('loaded')
  } catch (err: any) {
    if (err.name === 'AbortError' || err.name === 'CanceledError') {
      return
    }
    console.error('[PptxViewer] Failed to load PPTX:', err)
    error.value = err.message || 'فشل تحميل العرض التقديمي'
    emit('error', err)
    loading.value = false
  }
}

// Parse PPTX file using JSZip (PPTX is a ZIP archive containing XML)
const parsePptx = async (blob: Blob) => {
  try {
    // Convert blob to ArrayBuffer
    const arrayBuffer = await blob.arrayBuffer()

    // Parse using JSZip
    await renderPptxManually(arrayBuffer)

    if (slides.value.length === 0) {
      throw new Error('لم يتم العثور على شرائح في الملف')
    }
  } catch (err) {
    console.error('[PptxViewer] Parse error:', err)
    throw new Error('فشل في تحليل ملف العرض التقديمي')
  }
}

// Manual PPTX rendering fallback using JSZip
const renderPptxManually = async (arrayBuffer: ArrayBuffer): Promise<void> => {
  try {
    const JSZip = (await import('jszip')).default
    const zip = await JSZip.loadAsync(arrayBuffer)

    // PPTX is a ZIP file with XML content
    const slideFiles: string[] = []

    // Find all slide files
    zip.forEach((relativePath, file) => {
      if (relativePath.match(/ppt\/slides\/slide\d+\.xml$/)) {
        slideFiles.push(relativePath)
      }
    })

    // Sort slides by number
    slideFiles.sort((a, b) => {
      const numA = parseInt(a.match(/slide(\d+)\.xml/)?.[1] || '0')
      const numB = parseInt(b.match(/slide(\d+)\.xml/)?.[1] || '0')
      return numA - numB
    })

    // Parse each slide
    const parsedSlides: string[] = []
    for (const slidePath of slideFiles) {
      const slideXml = await zip.file(slidePath)?.async('text')
      if (slideXml) {
        const slideHtml = xmlToSimpleHtml(slideXml, slideFiles.indexOf(slidePath) + 1)
        parsedSlides.push(slideHtml)
      }
    }

    slides.value = parsedSlides
    totalSlides.value = parsedSlides.length
  } catch (err) {
    console.error('[PptxViewer] Manual parse error:', err)
    throw err
  }
}

// Convert slide XML to simple HTML (basic fallback)
const xmlToSimpleHtml = (xml: string, slideNum: number): string => {
  // Extract text content from XML
  const textMatches = xml.match(/<a:t>([^<]*)<\/a:t>/g) || []
  const texts = textMatches.map((m) => m.replace(/<\/?a:t>/g, '').trim()).filter(Boolean)

  if (texts.length === 0) {
    return `
      <div class="slide fallback-slide">
        <div class="slide-number">شريحة ${slideNum}</div>
        <div class="slide-placeholder">
          <p>محتوى الشريحة غير متوفر للعرض</p>
        </div>
      </div>
    `
  }

  return `
    <div class="slide fallback-slide">
      <div class="slide-number">شريحة ${slideNum}</div>
      <div class="slide-text-content">
        ${texts.map((t) => `<p>${t}</p>`).join('')}
      </div>
    </div>
  `
}

// Navigation
const prevSlide = () => {
  if (currentSlide.value > 1) {
    currentSlide.value--
    emit('slideChange', currentSlide.value)
  }
}

const nextSlide = () => {
  if (currentSlide.value < totalSlides.value) {
    currentSlide.value++
    emit('slideChange', currentSlide.value)
  }
}

const goToSlide = (slideNum: number) => {
  if (slideNum >= 1 && slideNum <= totalSlides.value) {
    currentSlide.value = slideNum
    emit('slideChange', currentSlide.value)
  }
}

// Fullscreen
const toggleFullscreen = async () => {
  if (!containerRef.value) return

  try {
    if (!document.fullscreenElement) {
      await containerRef.value.requestFullscreen()
      isFullscreen.value = true
    } else {
      await document.exitFullscreen()
      isFullscreen.value = false
    }
  } catch (err) {
    console.error('Fullscreen error:', err)
  }
}

// Download
const downloadPptx = () => {
  if (blobUrl.value) {
    const link = document.createElement('a')
    link.href = blobUrl.value
    link.download = props.fileName || 'presentation.pptx'
    link.click()
  }
}

// Keyboard navigation
const handleKeydown = (e: KeyboardEvent) => {
  if (e.key === 'ArrowLeft' || e.key === 'ArrowUp') {
    nextSlide() // RTL: Left arrow goes to next
  } else if (e.key === 'ArrowRight' || e.key === 'ArrowDown') {
    prevSlide() // RTL: Right arrow goes to previous
  } else if (e.key === 'Home') {
    goToSlide(1)
  } else if (e.key === 'End') {
    goToSlide(totalSlides.value)
  } else if (e.key === 'Escape' && isFullscreen.value) {
    toggleFullscreen()
  }
}

// Fullscreen change listener
const handleFullscreenChange = () => {
  isFullscreen.value = !!document.fullscreenElement
}

// Watchers
watch(
  () => props.src,
  () => {
    if (props.src) {
      loadPptx()
    }
  }
)

// Lifecycle
onMounted(() => {
  if (props.src) {
    loadPptx()
  }
  window.addEventListener('keydown', handleKeydown)
  document.addEventListener('fullscreenchange', handleFullscreenChange)
})

onUnmounted(() => {
  if (abortController) {
    abortController.abort()
    abortController = null
  }
  if (blobUrl.value) {
    URL.revokeObjectURL(blobUrl.value)
  }
  window.removeEventListener('keydown', handleKeydown)
  document.removeEventListener('fullscreenchange', handleFullscreenChange)
})
</script>

<style scoped>
.pptx-viewer {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: 0;
  background: #1a1a2e;
  position: relative;
  border-radius: 8px;
  overflow: hidden;
}

/* Toolbar */
.pptx-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  background: #16213e;
  border-bottom: 1px solid #1f3460;
  z-index: 10;
  flex-shrink: 0;
  gap: 12px;
}

.toolbar-section {
  display: flex;
  align-items: center;
  gap: 8px;
}

.file-info {
  flex: 1;
  min-width: 0;
}

.file-icon {
  color: #e94560;
  flex-shrink: 0;
}

.file-name {
  color: #eee;
  font-size: 13px;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* Navigation */
.navigation {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
  padding: 4px 8px;
}

.nav-btn {
  background: transparent;
  border: none;
  color: #aaa;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.nav-btn:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.1);
  color: white;
}

.nav-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.slide-counter {
  color: #ccc;
  font-size: 13px;
  min-width: 60px;
  text-align: center;
  font-variant-numeric: tabular-nums;
}

/* Action buttons */
.toolbar-btn {
  background: transparent;
  border: none;
  color: #aaa;
  cursor: pointer;
  padding: 6px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.toolbar-btn:hover {
  background: rgba(255, 255, 255, 0.1);
  color: white;
}

/* Container */
.pptx-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
  position: relative;
}

/* Loading */
.loading-overlay {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: #1a1a2e;
  z-index: 5;
}

.loader {
  width: 40px;
  height: 40px;
  border: 3px solid #1f3460;
  border-top-color: #e94560;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.loading-overlay p {
  margin-top: 16px;
  color: #aaa;
  font-size: 14px;
}

/* Error */
.error-state {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: #1a1a2e;
  padding: 24px;
  text-align: center;
}

.error-icon {
  color: #e94560;
  margin-bottom: 16px;
}

.error-state p {
  color: #ccc;
  margin-bottom: 16px;
}

.retry-btn {
  background: #e94560;
  color: white;
  border: none;
  padding: 8px 20px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: background 0.2s;
}

.retry-btn:hover {
  background: #d63850;
}

/* Slides Wrapper */
.slides-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

/* Main Slide Display */
.slide-display {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  min-height: 0;
  overflow: auto;
}

.slide-content {
  background: white;
  border-radius: 4px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
  max-width: 100%;
  max-height: 100%;
  overflow: hidden;
  aspect-ratio: 16 / 9;
  width: 100%;
}

/* pptxjs slide styles */
.slide-content :deep(.slide) {
  width: 100% !important;
  height: 100% !important;
  position: relative;
  background: white;
}

.slide-content :deep(.slide-number) {
  position: absolute;
  top: 8px;
  right: 8px;
  background: rgba(0, 0, 0, 0.5);
  color: white;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 12px;
}

/* Fallback slide styles */
.slide-content :deep(.fallback-slide) {
  padding: 40px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
}

.slide-content :deep(.slide-placeholder) {
  color: #666;
  text-align: center;
}

.slide-content :deep(.slide-text-content) {
  text-align: center;
  max-width: 80%;
}

.slide-content :deep(.slide-text-content p) {
  margin: 12px 0;
  font-size: 18px;
  line-height: 1.6;
  color: #333;
}

/* Empty state */
.empty-slide {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px;
  color: #666;
}

.empty-icon {
  color: #444;
  margin-bottom: 16px;
}

/* Thumbnails */
.slide-thumbnails {
  display: flex;
  gap: 8px;
  padding: 12px 16px;
  background: #0f0f23;
  border-top: 1px solid #1f3460;
  overflow-x: auto;
  flex-shrink: 0;
}

.thumbnail {
  flex-shrink: 0;
  width: 48px;
  height: 32px;
  background: #1f3460;
  border: 2px solid transparent;
  border-radius: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.thumbnail:hover {
  background: #2a4a7f;
}

.thumbnail.active {
  border-color: #e94560;
  background: #2a4a7f;
}

.thumbnail-number {
  color: #aaa;
  font-size: 12px;
  font-weight: 600;
}

.thumbnail.active .thumbnail-number {
  color: white;
}

/* Fullscreen mode */
.pptx-viewer:fullscreen {
  background: #0a0a1a;
}

.pptx-viewer:fullscreen .slide-display {
  padding: 40px;
}

.pptx-viewer:fullscreen .slide-content {
  max-width: 90vw;
  max-height: calc(100vh - 150px);
}

/* Scrollbar */
.slide-thumbnails::-webkit-scrollbar {
  height: 6px;
}

.slide-thumbnails::-webkit-scrollbar-track {
  background: #0f0f23;
}

.slide-thumbnails::-webkit-scrollbar-thumb {
  background: #1f3460;
  border-radius: 3px;
}

.slide-thumbnails::-webkit-scrollbar-thumb:hover {
  background: #2a4a7f;
}
</style>

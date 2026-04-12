<template>
  <div class="pdf-signature-viewer">
    <!-- Toolbar -->
    <div class="viewer-toolbar">
      <div class="toolbar-right">
        <Icon icon="description" class="w-4 h-4" />
        <span class="file-name">{{ fileName }}</span>
      </div>

      <div class="toolbar-center">
        <!-- Page navigation -->
        <button class="toolbar-btn" :disabled="currentPage <= 1" @click="goToPage(currentPage - 1)">
          <Icon icon="chevron_right" class="w-4.5 h-4.5" />
        </button>
        <span class="page-info">{{ currentPage }} / {{ totalPages }}</span>
        <button class="toolbar-btn" :disabled="currentPage >= totalPages" @click="goToPage(currentPage + 1)">
          <Icon icon="chevron_left" class="w-4.5 h-4.5" />
        </button>

        <div class="toolbar-divider"></div>

        <!-- Zoom -->
        <button class="toolbar-btn" @click="zoomOut" :disabled="scale <= 0.5">
          <Icon icon="zoom_out" class="w-4.5 h-4.5" />
        </button>
        <span class="zoom-info">{{ Math.round(scale * 100) }}%</span>
        <button class="toolbar-btn" @click="zoomIn" :disabled="scale >= 3">
          <Icon icon="zoom_in" class="w-4.5 h-4.5" />
        </button>
      </div>

      <div class="toolbar-left">
        <template v-if="canSign">
          <button v-if="!signatureMode" class="toolbar-btn sign-btn" @click="enterSignatureMode">
            <Icon icon="edit" class="w-4 h-4" />
            <span>{{ t('PlaceSignature') }}</span>
          </button>
          <button v-else class="toolbar-btn cancel-btn" @click="exitSignatureMode">
            <Icon icon="close" class="w-4 h-4" />
            <span>{{ t('Cancel') }}</span>
          </button>
        </template>
        <span v-else class="signed-badge">
          <Icon icon="check" class="w-4 h-4" />
          <span>{{ t('Signed') }}</span>
        </span>
      </div>
    </div>

    <!-- Signature Mode Instructions -->
    <div v-if="signatureMode && !signaturePlaced" class="signature-instructions">
      <Icon icon="arrow_selector_tool" class="w-4.5 h-4.5" />
      <span>{{ t('ClickToPlaceSignature') }}</span>
    </div>

    <!-- PDF Container -->
    <div class="pdf-container" ref="containerRef">
      <!-- Loading -->
      <div v-if="loading" class="loading-state">
        <div class="loader"></div>
        <p>جاري تحميل المستند...</p>
      </div>

      <!-- Error -->
      <div v-else-if="error" class="error-state">
        <Icon icon="error" class="w-12 h-12" />
        <p>{{ error }}</p>
        <button class="retry-btn" @click="loadPdf">إعادة المحاولة</button>
      </div>

      <!-- PDF Content -->
      <div v-else class="pdf-scroll" ref="scrollRef">
        <div class="pdf-pages">
          <div
            v-for="pageNum in totalPages"
            :key="pageNum"
            class="page-wrapper"
            :data-page="pageNum"
            :style="{ width: pageWidth + 'px' }"
            @click="handlePageClick($event, pageNum)"
          >
            <VuePdfEmbed
              :source="pdfSource"
              :page="pageNum"
              :scale="scale"
              @rendered="onPageRendered"
            />

            <!-- Signature Preview on this page -->
            <div
              v-if="signaturePlaced && signaturePosition.page === pageNum"
              class="signature-preview"
              :style="signaturePreviewStyle"
              @mousedown="startDragSignature"
            >
              <img v-if="signatureImage" :src="signatureImage" alt="Signature" />
              <div v-else class="signature-placeholder">
                <Icon icon="edit" class="w-6 h-6" />
                <span>انقر لرسم التوقيع</span>
              </div>

              <!-- Resize handles -->
              <div class="resize-handle top-left" @mousedown.stop="startResize('top-left', $event)"></div>
              <div class="resize-handle top-right" @mousedown.stop="startResize('top-right', $event)"></div>
              <div class="resize-handle bottom-left" @mousedown.stop="startResize('bottom-left', $event)"></div>
              <div class="resize-handle bottom-right" @mousedown.stop="startResize('bottom-right', $event)"></div>

              <!-- Delete button -->
              <button class="delete-signature" @click.stop="removeSignature">
                <Icon icon="close" class="w-3.5 h-3.5" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Signature Action Bar -->
    <div v-if="signaturePlaced" class="signature-action-bar">
      <button class="action-btn secondary" @click="removeSignature">
        <Icon icon="delete" class="w-4 h-4" />
        <span>{{ t('DeleteSignature') }}</span>
      </button>

      <button v-if="!signatureImage" class="action-btn primary" @click="openSignaturePad">
        <Icon icon="edit" class="w-4 h-4" />
        <span>{{ t('DrawSignature') }}</span>
      </button>

      <button v-if="signatureImage" class="action-btn success" :disabled="saving" @click="confirmSignature">
        <Icon v-if="saving" icon="progress_activity" class="w-4 h-4 spin" />
        <Icon v-else icon="check" class="w-4 h-4" />
        <span>{{ saving ? t('Saving') : t('ConfirmAndSave') }}</span>
      </button>
    </div>

    <!-- Signature Pad Modal -->
    <Teleport to="body">
      <div v-if="showSignaturePad" class="signature-modal-overlay" @click.self="showSignaturePad = false">
        <div class="signature-modal">
          <div class="modal-header">
            <h3>{{ t('DrawSignature') }}</h3>
            <button class="close-btn" @click="showSignaturePad = false">
              <Icon icon="close" class="w-5 h-5" />
            </button>
          </div>

          <div class="modal-body">
            <div class="signature-canvas-wrapper">
              <canvas
                ref="signatureCanvasRef"
                class="signature-canvas"
                @mousedown="startDrawing"
                @mousemove="draw"
                @mouseup="stopDrawing"
                @mouseleave="stopDrawing"
                @touchstart.prevent="handleTouchStart"
                @touchmove.prevent="handleTouchMove"
                @touchend.prevent="stopDrawing"
              ></canvas>
              <div v-if="!hasDrawnSignature" class="canvas-placeholder">
                <span>{{ t('DrawYourSignature') }}</span>
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button class="modal-btn clear" @click="clearSignatureCanvas" :disabled="!hasDrawnSignature">
              <Icon icon="delete" class="w-4 h-4" />
              <span>{{ t('Clear') }}</span>
            </button>
            <div class="modal-btn-group">
              <button class="modal-btn cancel" @click="showSignaturePad = false">{{ t('Cancel') }}</button>
              <button class="modal-btn confirm" @click="applySignature" :disabled="!hasDrawnSignature">
                <Icon icon="check" class="w-4 h-4" />
                <span>{{ t('Apply') }}</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useI18n } from 'vue-i18n'
import Icon from '@/components/ui/Icon.vue'
import VuePdfEmbed from 'vue-pdf-embed'
import axios from 'axios'
import { useUserStore } from '@/stores/user'

interface Props {
  src: string
  fileName?: string
  saving?: boolean
  /** Set to false if user has already signed this document */
  canSign?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  fileName: 'document.pdf',
  saving: false,
  canSign: true
})

const emit = defineEmits<{
  (e: 'signature-confirm', data: {
    signatureData: string,
    pageIndex: number,
    rect: [number, number, number, number]
  }): void
  (e: 'cancel'): void
  (e: 'loaded'): void
  (e: 'error', error: Error): void
}>()

const { t } = useI18n()
const userStore = useUserStore()

// Refs
const containerRef = ref<HTMLElement | null>(null)
const scrollRef = ref<HTMLElement | null>(null)
const signatureCanvasRef = ref<HTMLCanvasElement | null>(null)

// PDF State
const loading = ref(true)
const error = ref('')
const pdfSource = ref<{ data: Uint8Array } | null>(null)
const totalPages = ref(0)
const currentPage = ref(1)
const scale = ref(1.0)

// Store actual PDF dimensions (in PDF points, not scaled)
const pdfPageWidth = ref(595) // Actual PDF width in points
const pdfPageHeight = ref(842) // Actual PDF height in points

// Display dimensions (scaled for rendering)
const pageWidth = computed(() => pdfPageWidth.value * scale.value)

// Signature State
const signatureMode = ref(false)
const signaturePlaced = ref(false)
const signaturePosition = ref({ page: 1, x: 0, y: 0, width: 150, height: 60 })
const signatureImage = ref<string | null>(null)
const showSignaturePad = ref(false)
const hasDrawnSignature = ref(false)

// Drawing state
const isDrawing = ref(false)
let lastX = 0
let lastY = 0
let signatureCtx: CanvasRenderingContext2D | null = null

// Drag/Resize state
const isDragging = ref(false)
const isResizing = ref(false)
const resizeHandle = ref('')
const dragStart = ref({ x: 0, y: 0 })
const initialPosition = ref({ x: 0, y: 0, width: 0, height: 0 })

// Signature preview style
const signaturePreviewStyle = computed(() => {
  return {
    left: `${signaturePosition.value.x}px`,
    top: `${signaturePosition.value.y}px`,
    width: `${signaturePosition.value.width}px`,
    height: `${signaturePosition.value.height}px`
  }
})

// Load PDF
async function loadPdf() {
  loading.value = true
  error.value = ''

  try {
    const token = userStore.token
    if (!token) throw new Error('No authentication token')

    const url = props.src.startsWith('http')
      ? props.src.replace(/^https?:\/\/[^\/]+\/api\//, '/api/')
      : props.src.startsWith('/')
        ? props.src
        : `/api/${props.src}`

    const response = await axios.get(url, {
      responseType: 'arraybuffer',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Accept': 'application/pdf'
      }
    })

    const pdfData = new Uint8Array(response.data)
    pdfSource.value = { data: pdfData }

    // Get page count using pdf-lib
    const { PDFDocument } = await import('pdf-lib')
    const pdfDoc = await PDFDocument.load(pdfData)
    totalPages.value = pdfDoc.getPageCount()

    // Get first page dimensions (actual PDF points, not scaled)
    const firstPage = pdfDoc.getPage(0)
    const { width, height } = firstPage.getSize()
    pdfPageWidth.value = width
    pdfPageHeight.value = height

    loading.value = false
    emit('loaded')
  } catch (err: any) {
    console.error('[PdfSignatureViewer] Load error:', err)
    error.value = err.message || 'فشل تحميل المستند'
    emit('error', err)
    loading.value = false
  }
}

// Page rendered callback
function onPageRendered() {
  // Update page dimensions after render
}

// Navigation
function goToPage(page: number) {
  if (page < 1 || page > totalPages.value) return
  currentPage.value = page

  // Scroll to page
  const pageEl = scrollRef.value?.querySelector(`[data-page="${page}"]`)
  if (pageEl) {
    pageEl.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }
}

// Zoom - pageWidth is now computed from pdfPageWidth * scale
function zoomIn() {
  scale.value = Math.min(scale.value + 0.25, 3)
}

function zoomOut() {
  scale.value = Math.max(scale.value - 0.25, 0.5)
}

// Signature Mode
function enterSignatureMode() {
  signatureMode.value = true
}

function exitSignatureMode() {
  signatureMode.value = false
  signaturePlaced.value = false
  signatureImage.value = null
}

// Handle click on PDF page to place signature
function handlePageClick(event: MouseEvent, pageNum: number) {
  if (!signatureMode.value || signaturePlaced.value) return

  const target = event.currentTarget as HTMLElement
  const rect = target.getBoundingClientRect()

  // Calculate click position relative to page
  const x = event.clientX - rect.left
  const y = event.clientY - rect.top

  // Default signature dimensions
  const sigWidth = 150
  const sigHeight = 60

  // Center the signature on click position, but keep within page bounds
  const displayWidth = pageWidth.value
  const displayHeight = pdfPageHeight.value * scale.value

  let sigX = x - sigWidth / 2
  let sigY = y - sigHeight / 2

  // Clamp to page bounds
  sigX = Math.max(0, Math.min(sigX, displayWidth - sigWidth))
  sigY = Math.max(0, Math.min(sigY, displayHeight - sigHeight))

  signaturePosition.value = {
    page: pageNum,
    x: sigX,
    y: sigY,
    width: sigWidth,
    height: sigHeight
  }

  signaturePlaced.value = true
}

// Remove signature
function removeSignature() {
  signaturePlaced.value = false
  signatureImage.value = null
  signaturePosition.value = { page: 1, x: 0, y: 0, width: 150, height: 60 }
}

// Drag signature
function startDragSignature(event: MouseEvent) {
  if (isResizing.value) return

  isDragging.value = true
  dragStart.value = { x: event.clientX, y: event.clientY }
  initialPosition.value = { ...signaturePosition.value }

  document.addEventListener('mousemove', onDrag)
  document.addEventListener('mouseup', stopDrag)
}

function onDrag(event: MouseEvent) {
  if (!isDragging.value) return

  const dx = event.clientX - dragStart.value.x
  const dy = event.clientY - dragStart.value.y

  // Calculate new position
  let newX = initialPosition.value.x + dx
  let newY = initialPosition.value.y + dy

  // Clamp to page bounds
  const displayWidth = pageWidth.value
  const displayHeight = pdfPageHeight.value * scale.value
  const sigWidth = signaturePosition.value.width
  const sigHeight = signaturePosition.value.height

  newX = Math.max(0, Math.min(newX, displayWidth - sigWidth))
  newY = Math.max(0, Math.min(newY, displayHeight - sigHeight))

  signaturePosition.value.x = newX
  signaturePosition.value.y = newY
}

function stopDrag() {
  isDragging.value = false
  document.removeEventListener('mousemove', onDrag)
  document.removeEventListener('mouseup', stopDrag)
}

// Resize signature
function startResize(handle: string, event: MouseEvent) {
  isResizing.value = true
  resizeHandle.value = handle
  dragStart.value = { x: event.clientX, y: event.clientY }
  initialPosition.value = { ...signaturePosition.value }

  document.addEventListener('mousemove', onResize)
  document.addEventListener('mouseup', stopResize)
}

function onResize(event: MouseEvent) {
  if (!isResizing.value) return

  const dx = event.clientX - dragStart.value.x
  const dy = event.clientY - dragStart.value.y

  const minSize = 50

  switch (resizeHandle.value) {
    case 'bottom-right':
      signaturePosition.value.width = Math.max(minSize, initialPosition.value.width + dx)
      signaturePosition.value.height = Math.max(minSize, initialPosition.value.height + dy)
      break
    case 'bottom-left':
      signaturePosition.value.x = initialPosition.value.x + dx
      signaturePosition.value.width = Math.max(minSize, initialPosition.value.width - dx)
      signaturePosition.value.height = Math.max(minSize, initialPosition.value.height + dy)
      break
    case 'top-right':
      signaturePosition.value.y = initialPosition.value.y + dy
      signaturePosition.value.width = Math.max(minSize, initialPosition.value.width + dx)
      signaturePosition.value.height = Math.max(minSize, initialPosition.value.height - dy)
      break
    case 'top-left':
      signaturePosition.value.x = initialPosition.value.x + dx
      signaturePosition.value.y = initialPosition.value.y + dy
      signaturePosition.value.width = Math.max(minSize, initialPosition.value.width - dx)
      signaturePosition.value.height = Math.max(minSize, initialPosition.value.height - dy)
      break
  }
}

function stopResize() {
  isResizing.value = false
  resizeHandle.value = ''
  document.removeEventListener('mousemove', onResize)
  document.removeEventListener('mouseup', stopResize)
}

// Signature Pad
function openSignaturePad() {
  showSignaturePad.value = true
  nextTick(() => initSignatureCanvas())
}

function initSignatureCanvas() {
  const canvas = signatureCanvasRef.value
  if (!canvas) return

  canvas.width = 500
  canvas.height = 200

  signatureCtx = canvas.getContext('2d')
  if (signatureCtx) {
    signatureCtx.strokeStyle = '#000'
    signatureCtx.lineWidth = 2
    signatureCtx.lineCap = 'round'
    signatureCtx.lineJoin = 'round'
  }

  hasDrawnSignature.value = false
}

function startDrawing(e: MouseEvent) {
  if (!signatureCtx) return
  isDrawing.value = true
  const rect = signatureCanvasRef.value!.getBoundingClientRect()
  lastX = e.clientX - rect.left
  lastY = e.clientY - rect.top
}

function draw(e: MouseEvent) {
  if (!isDrawing.value || !signatureCtx) return

  const rect = signatureCanvasRef.value!.getBoundingClientRect()
  const x = e.clientX - rect.left
  const y = e.clientY - rect.top

  signatureCtx.beginPath()
  signatureCtx.moveTo(lastX, lastY)
  signatureCtx.lineTo(x, y)
  signatureCtx.stroke()

  lastX = x
  lastY = y
  hasDrawnSignature.value = true
}

function stopDrawing() {
  isDrawing.value = false
}

function handleTouchStart(e: TouchEvent) {
  if (!signatureCtx || e.touches.length !== 1) return
  isDrawing.value = true
  const rect = signatureCanvasRef.value!.getBoundingClientRect()
  lastX = e.touches[0].clientX - rect.left
  lastY = e.touches[0].clientY - rect.top
}

function handleTouchMove(e: TouchEvent) {
  if (!isDrawing.value || !signatureCtx || e.touches.length !== 1) return

  const rect = signatureCanvasRef.value!.getBoundingClientRect()
  const x = e.touches[0].clientX - rect.left
  const y = e.touches[0].clientY - rect.top

  signatureCtx.beginPath()
  signatureCtx.moveTo(lastX, lastY)
  signatureCtx.lineTo(x, y)
  signatureCtx.stroke()

  lastX = x
  lastY = y
  hasDrawnSignature.value = true
}

function clearSignatureCanvas() {
  if (!signatureCtx || !signatureCanvasRef.value) return
  signatureCtx.clearRect(0, 0, signatureCanvasRef.value.width, signatureCanvasRef.value.height)
  hasDrawnSignature.value = false
}

function applySignature() {
  if (!signatureCanvasRef.value || !hasDrawnSignature.value) return

  signatureImage.value = signatureCanvasRef.value.toDataURL('image/png')
  showSignaturePad.value = false
}

// Confirm and save signature
function confirmSignature() {
  if (!signatureImage.value) return

  // Convert screen coordinates to PDF coordinates
  // PDF coordinate system: (0,0) at bottom-left, Y increases upward
  // Screen coordinate system: (0,0) at top-left, Y increases downward
  //
  // The signature position is in screen pixels at current scale
  // We need to convert to PDF points (unscaled)

  const sigX = signaturePosition.value.x / scale.value
  const sigY = signaturePosition.value.y / scale.value
  const sigWidth = signaturePosition.value.width / scale.value
  const sigHeight = signaturePosition.value.height / scale.value

  // Use actual PDF page height for coordinate conversion
  const actualPageHeight = pdfPageHeight.value

  // PDF Y coordinate: flip from top-left to bottom-left origin
  // For a signature at screen Y position, its bottom edge in PDF coords is:
  // pdfY1 = pageHeight - (screenY + signatureHeight)
  // Its top edge in PDF coords is:
  // pdfY2 = pageHeight - screenY
  const pdfX1 = Math.max(0, sigX)
  const pdfY1 = Math.max(0, actualPageHeight - sigY - sigHeight)
  const pdfX2 = Math.min(pdfPageWidth.value, sigX + sigWidth)
  const pdfY2 = Math.min(actualPageHeight, actualPageHeight - sigY)

  emit('signature-confirm', {
    signatureData: signatureImage.value,
    pageIndex: signaturePosition.value.page - 1, // 0-indexed
    rect: [pdfX1, pdfY1, pdfX2, pdfY2]
  })
}

// Lifecycle
onMounted(() => {
  if (props.src) {
    loadPdf()
  }
})

watch(() => props.src, () => {
  if (props.src) {
    loadPdf()
  }
})
</script>

<style scoped>
.pdf-signature-viewer {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: #1a1a1a;
  border-radius: 8px;
  overflow: hidden;
}

/* Toolbar */
.viewer-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px;
  background: #2a2a2a;
  border-bottom: 1px solid #333;
  flex-shrink: 0;
}

.toolbar-right, .toolbar-center, .toolbar-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.toolbar-right {
  color: #ccc;
}

.file-name {
  font-size: 13px;
  font-weight: 500;
  color: #eee;
}

.toolbar-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 10px;
  background: transparent;
  border: none;
  border-radius: 6px;
  color: #aaa;
  font-size: 13px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.toolbar-btn:hover:not(:disabled) {
  background: #3a3a3a;
  color: #fff;
}

.toolbar-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.toolbar-btn.sign-btn {
  background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%);
  color: #fff;
}

.toolbar-btn.sign-btn:hover {
  background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);
}

.toolbar-btn.cancel-btn {
  background: #ef4444;
  color: #fff;
}

.toolbar-btn.cancel-btn:hover {
  background: #dc2626;
}

.signed-badge {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  background: rgba(34, 197, 94, 0.15);
  color: #22c55e;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
}

.page-info, .zoom-info {
  font-size: 12px;
  color: #888;
  min-width: 60px;
  text-align: center;
}

.toolbar-divider {
  width: 1px;
  height: 20px;
  background: #444;
  margin: 0 8px;
}

/* Signature Instructions */
.signature-instructions {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px;
  background: linear-gradient(135deg, rgba(99, 102, 241, 0.15) 0%, rgba(139, 92, 246, 0.15) 100%);
  color: #a5b4fc;
  font-size: 14px;
  border-bottom: 1px solid rgba(99, 102, 241, 0.3);
}

/* PDF Container */
.pdf-container {
  flex: 1;
  min-height: 0;
  display: flex;
  position: relative;
}

.loading-state, .error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: 100%;
  gap: 16px;
  color: #888;
}

.loader {
  width: 40px;
  height: 40px;
  border: 3px solid #333;
  border-top-color: #6366f1;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.retry-btn {
  padding: 8px 16px;
  background: #6366f1;
  color: #fff;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.pdf-scroll {
  flex: 1;
  overflow: auto;
  padding: 20px;
}

.pdf-pages {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;
}

.page-wrapper {
  position: relative;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.4);
  background: #fff;
}

.page-wrapper :deep(canvas) {
  display: block;
}

/* Signature Preview */
.signature-preview {
  position: absolute;
  border: 2px dashed #6366f1;
  background: rgba(99, 102, 241, 0.1);
  cursor: move;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.signature-preview img {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
}

.signature-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  color: #6366f1;
  font-size: 11px;
}

.resize-handle {
  position: absolute;
  width: 12px;
  height: 12px;
  background: #6366f1;
  border: 2px solid #fff;
  border-radius: 2px;
}

.resize-handle.top-left { top: -6px; left: -6px; cursor: nwse-resize; }
.resize-handle.top-right { top: -6px; right: -6px; cursor: nesw-resize; }
.resize-handle.bottom-left { bottom: -6px; left: -6px; cursor: nesw-resize; }
.resize-handle.bottom-right { bottom: -6px; right: -6px; cursor: nwse-resize; }

.delete-signature {
  position: absolute;
  top: -10px;
  right: -10px;
  width: 22px;
  height: 22px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #ef4444;
  color: #fff;
  border: 2px solid #fff;
  border-radius: 50%;
  cursor: pointer;
  transition: transform 0.15s ease;
}

.delete-signature:hover {
  transform: scale(1.1);
}

/* Signature Action Bar */
.signature-action-bar {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 12px 16px;
  background: #2a2a2a;
  border-top: 1px solid #333;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.action-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.action-btn.secondary {
  background: #3a3a3a;
  color: #ccc;
}

.action-btn.secondary:hover:not(:disabled) {
  background: #444;
  color: #fff;
}

.action-btn.primary {
  background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%);
  color: #fff;
}

.action-btn.primary:hover:not(:disabled) {
  background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);
}

.action-btn.success {
  background: #22c55e;
  color: #fff;
}

.action-btn.success:hover:not(:disabled) {
  background: #16a34a;
}

.spin {
  animation: spin 1s linear infinite;
}

/* Signature Modal */
.signature-modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
}

.signature-modal {
  background: #fff;
  border-radius: 16px;
  width: 90%;
  max-width: 560px;
  overflow: hidden;
  box-shadow: 0 25px 50px rgba(0, 0, 0, 0.3);
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid #e4e4e7;
}

.modal-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: #27272a;
  margin: 0;
}

.close-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  background: transparent;
  border: none;
  border-radius: 8px;
  color: #71717a;
  cursor: pointer;
}

.close-btn:hover {
  background: #f4f4f5;
  color: #27272a;
}

.modal-body {
  padding: 20px;
}

.signature-canvas-wrapper {
  position: relative;
  background: #fafbfc;
  border: 2px dashed #d4d4d8;
  border-radius: 12px;
  overflow: hidden;
}

.signature-canvas {
  display: block;
  width: 100%;
  height: 200px;
  cursor: crosshair;
  touch-action: none;
}

.canvas-placeholder {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #a1a1aa;
  font-size: 14px;
  pointer-events: none;
}

.modal-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  background: #fafafa;
  border-top: 1px solid #e4e4e7;
}

.modal-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 10px 16px;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.modal-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.modal-btn.clear {
  background: #fff;
  color: #71717a;
  border: 1px solid #d4d4d8;
}

.modal-btn.clear:hover:not(:disabled) {
  background: #f4f4f5;
  color: #ef4444;
}

.modal-btn-group {
  display: flex;
  gap: 8px;
}

.modal-btn.cancel {
  background: transparent;
  color: #71717a;
}

.modal-btn.cancel:hover {
  background: #f4f4f5;
}

.modal-btn.confirm {
  background: #6366f1;
  color: #fff;
}

.modal-btn.confirm:hover:not(:disabled) {
  background: #4f46e5;
}
</style>

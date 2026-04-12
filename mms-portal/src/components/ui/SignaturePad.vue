<template>
  <div class="signature-pad-container">
    <div class="signature-header">
      <h3>{{ title || t('Signature') }}</h3>
      <p class="signature-hint">{{ t('DrawYourSignature') }}</p>
    </div>

    <div class="signature-canvas-wrapper" :class="{ 'has-signature': hasSignature }">
      <canvas
        ref="canvasRef"
        class="signature-canvas"
        @mousedown="startDrawing"
        @mousemove="draw"
        @mouseup="stopDrawing"
        @mouseleave="stopDrawing"
        @touchstart.prevent="handleTouchStart"
        @touchmove.prevent="handleTouchMove"
        @touchend.prevent="stopDrawing"
      ></canvas>

      <div v-if="!hasSignature" class="signature-placeholder">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <path d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
        <span>{{ t('SignHere') }}</span>
      </div>
    </div>

    <div class="signature-actions">
      <button type="button" class="btn-clear" @click="clearSignature" :disabled="!hasSignature">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
        <span>{{ t('Clear') }}</span>
      </button>

      <div class="btn-group-main">
        <button type="button" class="btn-cancel" @click="$emit('cancel')">
          {{ t('Cancel') }}
        </button>
        <button
          type="button"
          class="btn-confirm"
          @click="confirmSignature"
          :disabled="!hasSignature || saving"
        >
          <div v-if="saving" class="spinner"></div>
          <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M5 13l4 4L19 7" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
          <span>{{ saving ? t('Saving') : t('ConfirmSignature') }}</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

interface Props {
  title?: string
  width?: number
  height?: number
  lineWidth?: number
  lineColor?: string
  saving?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  title: '',
  width: 500,
  height: 200,
  lineWidth: 2,
  lineColor: '#000000',
  saving: false
})

const emit = defineEmits<{
  (e: 'confirm', data: string): void
  (e: 'cancel'): void
}>()

const canvasRef = ref<HTMLCanvasElement | null>(null)
const isDrawing = ref(false)
const hasSignature = ref(false)
let ctx: CanvasRenderingContext2D | null = null
let lastX = 0
let lastY = 0

onMounted(() => {
  initCanvas()
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})

function initCanvas() {
  const canvas = canvasRef.value
  if (!canvas) return

  // Set canvas size
  const wrapper = canvas.parentElement
  if (wrapper) {
    canvas.width = wrapper.clientWidth
    canvas.height = props.height
  } else {
    canvas.width = props.width
    canvas.height = props.height
  }

  ctx = canvas.getContext('2d')
  if (ctx) {
    ctx.strokeStyle = props.lineColor
    ctx.lineWidth = props.lineWidth
    ctx.lineCap = 'round'
    ctx.lineJoin = 'round'
  }
}

function handleResize() {
  // Save current signature
  const canvas = canvasRef.value
  if (!canvas || !ctx) return

  const imageData = canvas.toDataURL()

  // Resize canvas
  const wrapper = canvas.parentElement
  if (wrapper) {
    canvas.width = wrapper.clientWidth
    canvas.height = props.height
  }

  // Restore context settings
  ctx.strokeStyle = props.lineColor
  ctx.lineWidth = props.lineWidth
  ctx.lineCap = 'round'
  ctx.lineJoin = 'round'

  // Restore signature if exists
  if (hasSignature.value) {
    const img = new Image()
    img.onload = () => {
      ctx?.drawImage(img, 0, 0)
    }
    img.src = imageData
  }
}

function getCoordinates(e: MouseEvent | Touch): { x: number; y: number } {
  const canvas = canvasRef.value
  if (!canvas) return { x: 0, y: 0 }

  const rect = canvas.getBoundingClientRect()
  const scaleX = canvas.width / rect.width
  const scaleY = canvas.height / rect.height

  return {
    x: (e.clientX - rect.left) * scaleX,
    y: (e.clientY - rect.top) * scaleY
  }
}

function startDrawing(e: MouseEvent) {
  isDrawing.value = true
  const { x, y } = getCoordinates(e)
  lastX = x
  lastY = y
}

function draw(e: MouseEvent) {
  if (!isDrawing.value || !ctx) return

  const { x, y } = getCoordinates(e)

  ctx.beginPath()
  ctx.moveTo(lastX, lastY)
  ctx.lineTo(x, y)
  ctx.stroke()

  lastX = x
  lastY = y
  hasSignature.value = true
}

function stopDrawing() {
  isDrawing.value = false
}

function handleTouchStart(e: TouchEvent) {
  if (e.touches.length !== 1) return

  isDrawing.value = true
  const { x, y } = getCoordinates(e.touches[0])
  lastX = x
  lastY = y
}

function handleTouchMove(e: TouchEvent) {
  if (!isDrawing.value || !ctx || e.touches.length !== 1) return

  const { x, y } = getCoordinates(e.touches[0])

  ctx.beginPath()
  ctx.moveTo(lastX, lastY)
  ctx.lineTo(x, y)
  ctx.stroke()

  lastX = x
  lastY = y
  hasSignature.value = true
}

function clearSignature() {
  const canvas = canvasRef.value
  if (!canvas || !ctx) return

  ctx.clearRect(0, 0, canvas.width, canvas.height)
  hasSignature.value = false
}

function confirmSignature() {
  const canvas = canvasRef.value
  if (!canvas || !hasSignature.value) return

  // Get signature as PNG data URL
  const dataUrl = canvas.toDataURL('image/png')
  emit('confirm', dataUrl)
}

// Expose methods for parent component
defineExpose({
  clear: clearSignature,
  getSignatureData: () => {
    const canvas = canvasRef.value
    if (!canvas || !hasSignature.value) return null
    return canvas.toDataURL('image/png')
  }
})
</script>

<style scoped>
.signature-pad-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 20px;
}

.signature-header {
  text-align: center;
}

.signature-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0 0 4px;
}

.signature-hint {
  font-size: 13px;
  color: #666;
  margin: 0;
}

.signature-canvas-wrapper {
  position: relative;
  background: #fafbfc;
  border: 2px dashed #d0d0d0;
  border-radius: 12px;
  overflow: hidden;
  transition: border-color 0.2s ease;
}

.signature-canvas-wrapper:hover {
  border-color: #b0b0b0;
}

.signature-canvas-wrapper.has-signature {
  border-style: solid;
  border-color: var(--color-primary, #006d4b);
}

.signature-canvas {
  display: block;
  width: 100%;
  cursor: crosshair;
  touch-action: none;
}

.signature-placeholder {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  color: #bbb;
  pointer-events: none;
}

.signature-placeholder svg {
  width: 32px;
  height: 32px;
}

.signature-placeholder span {
  font-size: 14px;
}

.signature-canvas-wrapper.has-signature .signature-placeholder {
  display: none;
}

.signature-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.btn-clear {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 10px 16px;
  background: #fff;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  color: #666;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-clear:hover:not(:disabled) {
  background: #f5f5f5;
  border-color: #d0d0d0;
  color: #ef4444;
}

.btn-clear:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.btn-clear svg {
  width: 16px;
  height: 16px;
}

.btn-group-main {
  display: flex;
  align-items: center;
  gap: 10px;
}

.btn-cancel {
  padding: 10px 20px;
  background: transparent;
  border: none;
  border-radius: 8px;
  color: #666;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-cancel:hover {
  background: #f5f5f5;
  color: #333;
}

.btn-confirm {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 24px;
  background: var(--color-primary, #006d4b);
  border: none;
  border-radius: 8px;
  color: #fff;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-confirm:hover:not(:disabled) {
  background: #009759;
}

.btn-confirm:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-confirm svg {
  width: 18px;
  height: 18px;
}

.spinner {
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>

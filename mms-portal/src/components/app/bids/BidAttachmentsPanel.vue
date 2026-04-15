<template>
  <section class="bap">
    <header class="bap-head">
      <h3>
        <Icon icon="mdi:attachment" class="w-4 h-4" />
        {{ $t('Attachments') || 'Attachments' }}
        <span class="bap-count">{{ attachments.length }}</span>
      </h3>
      <label v-if="canEdit" class="btn-primary btn-sm bap-upload-btn">
        <Icon icon="mdi:upload" class="w-4 h-4" />
        {{ $t('Upload') || 'Upload' }}
        <input ref="fileInput" type="file" multiple hidden @change="onFilesPicked" />
      </label>
    </header>

    <div v-if="uploading" class="bap-uploading">
      <div class="spinner"></div>
      <span>{{ $t('Uploading') || 'Uploading…' }}</span>
    </div>

    <div v-if="!uploading && attachments.length === 0" class="bap-empty">
      <Icon icon="mdi:folder-open-outline" class="w-8 h-8" />
      <p>{{ $t('NoAttachments') || 'No attachments yet.' }}</p>
    </div>

    <div v-if="!uploading && attachments.length > 0" class="bap-list">
      <article v-for="a in attachments" :key="a.id" class="bap-item">
        <div class="bap-icon-wrap" :class="fileIconColor(a.name)">
          <Icon :icon="fileIcon(a.name)" class="w-5 h-5" />
        </div>
        <div class="bap-item-info">
          <div class="bap-item-name">{{ a.name }}</div>
          <div class="bap-item-meta">
            <span class="bap-size">{{ formatSize(a.size) }}</span>
            <span v-if="a.createdDate" class="bap-date">{{ formatDate(a.createdDate) }}</span>
            <span v-if="a.privacyName" class="bap-privacy">{{ a.privacyName }}</span>
          </div>
        </div>
        <div class="bap-item-actions">
          <button class="icon-btn" :title="$t('Download') || 'Download'" @click="download(a)">
            <Icon icon="mdi:download" class="w-4 h-4" />
          </button>
          <button v-if="canEdit" class="icon-btn icon-btn-danger" :title="$t('Delete')" @click="confirmDelete(a)">
            <Icon icon="mdi:delete-outline" class="w-4 h-4" />
          </button>
        </div>
      </article>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import BidsService, { type BidAttachment } from '@/services/BidsService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

interface Props { bidId: number; canEdit: boolean }
const props = defineProps<Props>()

const { t } = useI18n()
const { toast } = useToast()

const attachments = ref<BidAttachment[]>([])
const uploading = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)

async function load() {
  try {
    const res: any = await BidsService.listAttachments(props.bidId)
    attachments.value = (res?.data ?? res ?? []) as BidAttachment[]
  } catch {
    attachments.value = []
  }
}

async function onFilesPicked(e: Event) {
  const input = e.target as HTMLInputElement
  if (!input.files || input.files.length === 0) return
  const files = Array.from(input.files)
  uploading.value = true
  try {
    const res: any = await BidsService.uploadAttachments(props.bidId, files)
    attachments.value = (res?.data ?? res ?? []) as BidAttachment[]
    toast.success(t('Uploaded') || 'Uploaded')
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    uploading.value = false
    if (fileInput.value) fileInput.value.value = ''
  }
}

async function confirmDelete(a: BidAttachment) {
  if (!confirm(t('ConfirmDelete') || 'Delete this attachment?')) return
  try {
    await BidsService.deleteAttachment(a.id)
    toast.success(t('Deleted') || 'Deleted')
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  }
}

/** Open the protected /api/attachments endpoint in a new tab — token flow is handled there. */
function download(a: BidAttachment) {
  const base = import.meta.env.VITE_API_URL || ''
  window.open(`${base}/api/attachments/${a.id}`, '_blank')
}

// ─── Helpers ───
function fileIcon(name: string): string {
  const ext = name.split('.').pop()?.toLowerCase() || ''
  if (['pdf'].includes(ext)) return 'mdi:file-pdf-box'
  if (['doc', 'docx'].includes(ext)) return 'mdi:file-word'
  if (['xls', 'xlsx', 'csv'].includes(ext)) return 'mdi:file-excel'
  if (['png', 'jpg', 'jpeg', 'gif', 'svg', 'webp'].includes(ext)) return 'mdi:file-image'
  return 'mdi:file-document-outline'
}
function fileIconColor(name: string): string {
  const ext = name.split('.').pop()?.toLowerCase() || ''
  if (['pdf'].includes(ext)) return 'file-pdf'
  if (['doc', 'docx'].includes(ext)) return 'file-word'
  if (['xls', 'xlsx', 'csv'].includes(ext)) return 'file-excel'
  if (['png', 'jpg', 'jpeg', 'gif', 'svg', 'webp'].includes(ext)) return 'file-image'
  return 'file-default'
}
function formatSize(bytes: number): string {
  if (!bytes) return '—'
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}
function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString()
}

watch(() => props.bidId, () => { if (props.bidId) load() })
onMounted(load)
</script>

<style scoped>
.bap { display: flex; flex-direction: column; gap: 12px; }
.bap-head { display: flex; justify-content: space-between; align-items: center; }
.bap-head h3 {
  margin: 0; display: flex; align-items: center; gap: 6px;
  font-size: 14px; font-weight: 700; color: #1a2e25;
}
.bap-count {
  display: inline-flex; align-items: center; justify-content: center;
  min-width: 22px; height: 20px; padding: 0 7px;
  background: rgba(0, 109, 75, 0.1); color: #006d4b;
  border-radius: 10px; font-size: 11px; font-weight: 700;
}

.btn-primary {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 8px 14px; border-radius: 8px; font-size: 13px; font-weight: 600;
  cursor: pointer; font-family: inherit; transition: all 0.15s; border: 1px solid transparent;
  background: #006d4b; color: #fff;
}
.btn-primary:hover:not(:disabled) { background: #005339; }
.btn-primary.btn-sm { padding: 6px 11px; font-size: 12px; }
.bap-upload-btn { cursor: pointer; }

.bap-uploading {
  display: flex; align-items: center; justify-content: center; gap: 10px;
  padding: 24px; color: #6b8a7d; font-size: 13px;
}
.spinner {
  width: 24px; height: 24px; border: 3px solid #e4ede8;
  border-top-color: #006d4b; border-radius: 50%;
  animation: bap-spin 0.8s linear infinite;
}
@keyframes bap-spin { to { transform: rotate(360deg); } }

.bap-empty {
  display: flex; flex-direction: column; align-items: center;
  justify-content: center;
  gap: 10px; min-height: 200px; padding: 32px 20px;
  color: #93afa4; font-size: 13px; text-align: center;
  border: 1px dashed #d4e0da; border-radius: 10px; background: #f7faf8;
}
.bap-empty p { margin: 0; max-width: 480px; color: #6b8a7d; }

.bap-list { display: flex; flex-direction: column; gap: 8px; }
.bap-item {
  display: flex; align-items: center; gap: 12px;
  padding: 10px 12px; background: #fff;
  border: 1px solid #e4ede8; border-radius: 10px;
  transition: all 0.15s;
}
.bap-item:hover { border-color: #c8ddd3; box-shadow: 0 2px 8px rgba(0, 109, 75, 0.04); }

.bap-icon-wrap {
  display: flex; align-items: center; justify-content: center;
  width: 40px; height: 40px; border-radius: 8px; flex-shrink: 0;
}
.bap-icon-wrap.file-pdf     { background: #fee2e2; color: #b91c1c; }
.bap-icon-wrap.file-word    { background: #dbeafe; color: #1d4ed8; }
.bap-icon-wrap.file-excel   { background: #dcfce7; color: #15803d; }
.bap-icon-wrap.file-image   { background: #fef3c7; color: #92400e; }
.bap-icon-wrap.file-default { background: #f1f5f9; color: #475569; }

.bap-item-info { flex: 1; min-width: 0; }
.bap-item-name {
  font-size: 13px; font-weight: 600; color: #1a2e25;
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}
.bap-item-meta { display: flex; gap: 10px; margin-top: 2px; font-size: 11px; color: #6b8a7d; }
.bap-privacy {
  padding: 1px 7px; border-radius: 6px; font-size: 10px;
  background: rgba(0, 109, 75, 0.08); color: #006d4b; font-weight: 600;
}

.bap-item-actions { display: flex; gap: 4px; }
.icon-btn {
  background: transparent; border: 1px solid #e4ede8; border-radius: 6px;
  padding: 5px 7px; cursor: pointer; color: #475569; transition: all 0.15s;
}
.icon-btn:hover { background: #f4f8f6; color: #006d4b; }
.icon-btn-danger:hover { background: #fef2f2; color: #b91c1c; border-color: #fca5a5; }
</style>

<template>
  <Card class="flex-1 overflow-hidden">
    <template #header>
      <div class="flex items-center justify-between">
        <h3 class="text-sm font-semibold">{{ $t('Attachments') }}</h3>
        <button
          v-if="selectedFile"
          type="button"
          class="p-1 text-zinc-500 hover:bg-zinc-100 rounded"
          @click="$emit('close-file')"
        >
          <Icon icon="mdi:close" class="w-4 h-4" />
        </button>
      </div>
    </template>

    <div class="space-y-2 max-h-48 overflow-y-auto">
      <!-- Meeting Attachments -->
      <div v-if="meetingAttachments.length > 0" class="space-y-1">
        <p class="text-xs text-zinc-500 font-medium">{{ $t('MeetingAttachments') }}</p>
        <button
          v-for="attachment in meetingAttachments"
          :key="attachment.id"
          type="button"
          :class="[
            'w-full text-left p-2 rounded flex items-center gap-2 text-sm transition-colors',
            selectedFile === attachment.id
              ? 'bg-primary-50 text-primary'
              : 'hover:bg-zinc-100'
          ]"
          @click="$emit('load-attachment', attachment.id)"
        >
          <Icon :icon="getFileIcon(attachment.fileName)" class="w-4 h-4 flex-shrink-0" />
          <span class="truncate">{{ attachment.fileName }}</span>
        </button>
      </div>

      <!-- Agenda Attachments -->
      <div v-for="agenda in agendaItemsWithAttachments" :key="agenda.id" class="space-y-1">
        <p class="text-xs text-zinc-500 font-medium">{{ truncate(agenda.title, 30) }}</p>
        <button
          v-for="attachment in agenda.attachments"
          :key="attachment.id"
          type="button"
          :class="[
            'w-full text-left p-2 rounded flex items-center gap-2 text-sm transition-colors',
            selectedFile === attachment.id
              ? 'bg-primary-50 text-primary'
              : 'hover:bg-zinc-100'
          ]"
          @click="$emit('load-attachment', attachment.id)"
        >
          <Icon :icon="getFileIcon(attachment.fileName)" class="w-4 h-4 flex-shrink-0" />
          <span class="truncate">{{ attachment.fileName }}</span>
        </button>
      </div>

      <!-- Empty State -->
      <div
        v-if="meetingAttachments.length === 0 && agendaItemsWithAttachments.length === 0"
        class="text-center py-4 text-zinc-400"
      >
        <Icon icon="mdi:paperclip" class="w-8 h-8 mx-auto mb-2" />
        <p class="text-sm">{{ $t('NoAttachments') }}</p>
      </div>
    </div>
  </Card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'

const props = defineProps<{
  meetingId: number
  agendaItems: any[]
  selectedFile: string | null
  meetingOwner: boolean
  statusId: number
  viewMode: boolean
}>()

defineEmits(['load-attachment', 'close-file'])

// Extract meeting-level attachments from agenda items or use empty array
const meetingAttachments = computed(() => {
  // This would typically come from the meeting details
  return []
})

const agendaItemsWithAttachments = computed(() => {
  return props.agendaItems.filter(
    agenda => agenda.attachments && agenda.attachments.length > 0
  )
})

const getFileIcon = (fileName: string) => {
  const ext = fileName?.split('.').pop()?.toLowerCase()
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
    gif: 'mdi:file-image'
  }
  return icons[ext || ''] || 'mdi:file-document'
}

const truncate = (text: string, length: number) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}
</script>

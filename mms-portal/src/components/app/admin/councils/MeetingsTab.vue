<template>
  <div class="tab-panel">
    <div class="panel-header">
      <h3 class="panel-title">{{ $t('Meetings') }}</h3>
    </div>
    <div class="meetings-list">
      <div
        v-for="meeting in meetings"
        :key="meeting.id"
        class="meeting-card"
        @click="viewMeeting(meeting)"
      >
        <div class="meeting-date">
          <span class="date-day">{{ formatDateDay(meeting.date) }}</span>
          <span class="date-month">{{ formatDateMonth(meeting.date) }}</span>
        </div>
        <div class="meeting-info">
          <h4 class="meeting-title">{{ meeting.title }}</h4>
          <div class="meeting-meta">
            <span v-if="meeting.location" class="meta-item">
              <Icon icon="mdi:map-marker" class="w-4 h-4" />
              {{ meeting.location }}
            </span>
            <span v-if="meeting.referenceNumber" class="meta-item">
              <Icon icon="mdi:pound" class="w-4 h-4" />
              {{ meeting.referenceNumber }}
            </span>
          </div>
        </div>
        <div class="meeting-status" :class="getStatusClass(meeting.statusId)">
          {{ meeting.statusName }}
        </div>
      </div>
      <div v-if="meetings.length === 0 && !loading" class="empty-state">
        <Icon icon="mdi:calendar-blank-outline" class="w-16 h-16 text-zinc-200" />
        <p class="text-zinc-400">{{ $t('NoMeetings') }}</p>
      </div>
    </div>
    <!-- Pagination -->
    <div v-if="totalCount > pageSize" class="pagination">
      <button
        class="page-btn"
        :disabled="page === 1"
        @click="handlePageChange({ page: page - 2, rows: pageSize })"
      >
        <Icon icon="mdi:chevron-right" class="w-5 h-5" />
      </button>
      <span class="page-info">
        {{ page }} / {{ Math.ceil(totalCount / pageSize) }}
      </span>
      <button
        class="page-btn"
        :disabled="page >= Math.ceil(totalCount / pageSize)"
        @click="handlePageChange({ page: page, rows: pageSize })"
      >
        <Icon icon="mdi:chevron-left" class="w-5 h-5" />
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'

// Types
interface Meeting {
  id: string
  title: string
  date: string
  location?: string
  referenceNumber?: string
  statusId: number
  statusName: string
}

// Props
const props = defineProps<{
  committeeId: string | null
}>()

// Emits
const emit = defineEmits<{
  (e: 'update:count', count: number): void
}>()

const router = useRouter()

// State
const loading = ref(false)
const meetings = ref<Meeting[]>([])
const totalCount = ref(0)
const page = ref(1)
const pageSize = ref(10)

// Methods
const loadMeetings = async () => {
  if (!props.committeeId) return
  loading.value = true
  try {
    const response = await CouncilCommitteesService.listCouncilCommitteeMeetings(
      props.committeeId,
      page.value,
      pageSize.value
    )
    // API returns { data, total } or { data: { data, total } }
    const result = response.data || response
    meetings.value = result.data || result || []
    totalCount.value = result.total || 0
    emit('update:count', totalCount.value)
  } catch (error) {
    console.error('Failed to load meetings:', error)
  } finally {
    loading.value = false
  }
}

const handlePageChange = (event: { page: number; rows: number }) => {
  page.value = event.page + 1
  pageSize.value = event.rows
  loadMeetings()
}

const viewMeeting = (meeting: Meeting) => {
  if (meeting.statusId > 1) {
    router.push(`/meetingRoom/${meeting.id}?viewMode=true`)
  }
}

const formatDateDay = (date: string) => {
  if (!date) return '-'
  return new Date(date).getDate()
}

const formatDateMonth = (date: string) => {
  if (!date) return ''
  return new Date(date).toLocaleDateString('ar-EG', { month: 'short' })
}

const getStatusClass = (statusId: number) => {
  const classes: Record<number, string> = {
    1: 'draft',
    2: 'pending',
    3: 'approved',
    4: 'running',
    5: 'finished',
    6: 'cancelled'
  }
  return classes[statusId] || 'default'
}

// Watch for committeeId changes
watch(() => props.committeeId, () => {
  if (props.committeeId) {
    page.value = 1
    loadMeetings()
  }
}, { immediate: true })

// Expose refresh method
defineExpose({ refresh: loadMeetings })
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

/* Meetings List */
.meetings-list {
  @apply space-y-3;
}

.meeting-card {
  @apply flex items-center gap-4 p-4 bg-zinc-50 rounded-xl border border-zinc-100 cursor-pointer transition-all;
  @apply hover:border-primary/20 hover:shadow-sm;
}

.meeting-date {
  @apply w-14 h-14 rounded-xl bg-primary/10 flex flex-col items-center justify-center flex-shrink-0;
}

.date-day {
  @apply text-xl font-bold text-primary;
}

.date-month {
  @apply text-xs text-primary/70;
}

.meeting-info {
  @apply flex-1 min-w-0;
}

.meeting-title {
  @apply font-medium text-zinc-900 truncate;
}

.meeting-meta {
  @apply flex items-center gap-4 mt-1;
}

.meta-item {
  @apply flex items-center gap-1 text-xs text-zinc-500;
}

.meeting-status {
  @apply px-3 py-1 text-xs font-medium rounded-full;
}

.meeting-status.draft { @apply bg-zinc-100 text-zinc-600; }
.meeting-status.pending { @apply bg-warning/10 text-warning; }
.meeting-status.approved { @apply bg-info/10 text-info; }
.meeting-status.running { @apply bg-primary/10 text-primary; }
.meeting-status.finished { @apply bg-success/10 text-success; }
.meeting-status.cancelled { @apply bg-error/10 text-error; }

/* Pagination */
.pagination {
  @apply flex items-center justify-center gap-4 mt-4 pt-4 border-t border-zinc-100;
}

.page-btn {
  @apply p-2 rounded-lg bg-zinc-100 text-zinc-600 transition-colors;
  @apply hover:bg-primary/10 hover:text-primary disabled:opacity-50 disabled:cursor-not-allowed;
}

.page-info {
  @apply text-sm text-zinc-500;
}

/* Empty State */
.empty-state {
  @apply flex flex-col items-center justify-center py-12;
}
</style>

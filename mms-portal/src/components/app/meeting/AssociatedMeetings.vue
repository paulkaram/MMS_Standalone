<template>
  <div class="associated-meetings">
    <!-- Search and Add -->
    <div v-if="!viewMode" class="search-section">
      <div class="search-input-wrapper">
        <Icon icon="mdi:magnify" class="search-icon" />
        <input
          v-model="searchQuery"
          type="text"
          class="search-input"
          :placeholder="$t('SearchMeetings')"
        >
        <Icon v-if="searching" icon="mdi:loading" class="loading-icon animate-spin" />
      </div>

      <!-- Search Results Dropdown -->
      <div v-if="showResults && searchResults.length > 0" class="search-results" @mousedown.prevent>
        <div
          v-for="result in searchResults"
          :key="result.id"
          class="result-item"
          @click="addFromResult(result)"
        >
          <Icon icon="mdi:calendar-outline" class="w-4 h-4 text-zinc-400" />
          <div class="result-info">
            <span class="result-title">{{ result.title }}</span>
            <div class="result-meta">
              <span v-if="result.referenceNumber" class="result-ref">{{ result.referenceNumber }}</span>
              <span v-if="result.date" class="result-date">{{ formatDate(result.date) }}</span>
            </div>
          </div>
          <Icon icon="mdi:plus" class="w-4 h-4 text-primary" />
        </div>
      </div>

      <!-- No Results -->
      <div v-else-if="showResults && searchQuery.length >= 2 && !searching" class="no-results">
        <span>{{ $t('NoMeetingsFound') }}</span>
      </div>
    </div>

    <!-- Meetings List -->
    <div class="meetings-list">
      <div class="list-header">
        <span>{{ $t('LinkedMeetings') }}</span>
        <span class="count">{{ meetings.length }}</span>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="loading-state">
        <div v-for="i in 2" :key="i" class="skeleton-item"></div>
      </div>

      <!-- Empty State -->
      <div v-else-if="meetings.length === 0" class="empty-state">
        <Icon icon="mdi:link-variant-off" class="w-8 h-8" />
        <span>{{ $t('NoLinkedMeetings') }}</span>
      </div>

      <!-- Meetings -->
      <div v-else class="meetings-grid">
        <div
          v-for="meeting in meetings"
          :key="meeting.id"
          class="meeting-item"
        >
          <div class="meeting-icon">
            <Icon icon="mdi:calendar-check" class="w-5 h-5" />
          </div>
          <div class="meeting-info">
            <p class="meeting-title">{{ meeting.name || meeting.title }}</p>
            <div class="meeting-meta">
              <span v-if="meeting.referenceNumber" class="meeting-ref">{{ meeting.referenceNumber }}</span>
              <span v-if="meeting.date" class="meeting-date">{{ formatDate(meeting.date) }}</span>
            </div>
          </div>
          <div class="meeting-actions">
            <button type="button" @click="openMeeting(meeting)" :title="$t('Open')">
              <Icon icon="mdi:open-in-new" class="w-4 h-4" />
            </button>
            <button v-if="!viewMode" type="button" class="remove-btn" @click="removeMeeting(meeting)" :title="$t('Remove')">
              <Icon icon="mdi:link-variant-remove" class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import MeetingsService from '@/services/MeetingsService'

export interface AssociatedMeeting {
  id: string | number
  name?: string
  title?: string
  referenceNumber?: string
  date?: string
}

interface SearchResult {
  id: string
  title: string
  referenceNumber?: string
  date?: string
}

interface Props {
  modelValue: AssociatedMeeting[]
  meetingId?: number | string
  viewMode?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  viewMode: false,
  meetingId: undefined
})

const emit = defineEmits<{
  'update:modelValue': [value: AssociatedMeeting[]]
}>()

const router = useRouter()

// State
const meetings = ref<AssociatedMeeting[]>([...props.modelValue])
const loading = ref(false)

// Flag to prevent infinite loop between watchers
let isUpdatingFromProps = false
const searching = ref(false)
const adding = ref(false)
const searchQuery = ref('')
const searchResults = ref<SearchResult[]>([])
const showResults = ref(false)

let searchTimeout: ReturnType<typeof setTimeout> | null = null

// Watch for external changes
watch(() => props.modelValue, (newVal) => {
  if (JSON.stringify(newVal) !== JSON.stringify(meetings.value)) {
    isUpdatingFromProps = true
    meetings.value = [...newVal]
    nextTick(() => {
      isUpdatingFromProps = false
    })
  }
}, { deep: true })

// Watch for local changes and emit
watch(meetings, (newVal) => {
  if (!isUpdatingFromProps) {
    emit('update:modelValue', [...newVal])
  }
}, { deep: true })

// Debounced search via watch
watch(searchQuery, (newVal) => {
  if (searchTimeout) {
    clearTimeout(searchTimeout)
  }

  if (newVal.length < 2) {
    searchResults.value = []
    showResults.value = false
    return
  }

  searchTimeout = setTimeout(() => {
    searchMeetings()
  }, 300)
})

const searchMeetings = async () => {
  searching.value = true
  showResults.value = true

  try {
    const response = await MeetingsService.listMeetingsForAutoComplete(searchQuery.value)
    const data = response?.data || response || []
    const existingIds = meetings.value.map(m => m.id.toString())

    searchResults.value = (Array.isArray(data) ? data : [])
      .filter((m: any) => m && m.id && !existingIds.includes(m.id.toString()) && m.id.toString() !== props.meetingId?.toString())
      .map((m: any) => ({
        id: m.id.toString(),
        title: m.name || m.title || m.titleAr,
        referenceNumber: m.referenceNumber,
        date: m.date
      }))
  } catch (error) {
    console.error('Failed to search meetings:', error)
    searchResults.value = []
  } finally {
    searching.value = false
  }
}

const addFromResult = async (result: SearchResult) => {
  // Check for duplicates
  if (meetings.value.some(m => m.id.toString() === result.id)) {
    return
  }

  adding.value = true
  try {
    if (props.meetingId) {
      const response = await MeetingsService.addAssociatedMeeting(props.meetingId.toString(), result.id)
      if (response?.data) {
        meetings.value = response.data
      } else {
        meetings.value.push({
          id: result.id,
          name: result.title,
          title: result.title,
          referenceNumber: result.referenceNumber,
          date: result.date
        })
      }
    } else {
      meetings.value.push({
        id: result.id,
        name: result.title,
        title: result.title,
        referenceNumber: result.referenceNumber,
        date: result.date
      })
    }

    // Reset search
    searchQuery.value = ''
    searchResults.value = []
    showResults.value = false
  } catch (error) {
    console.error('Failed to add associated meeting:', error)
  } finally {
    adding.value = false
  }
}

const removeMeeting = async (meeting: AssociatedMeeting) => {
  try {
    if (props.meetingId) {
      const response = await MeetingsService.removeAssociatedMeeting(props.meetingId.toString(), meeting.id.toString())
      // API returns updated list
      if (response?.data) {
        meetings.value = response.data
      } else {
        meetings.value = meetings.value.filter(m => m.id !== meeting.id)
      }
    } else {
      meetings.value = meetings.value.filter(m => m.id !== meeting.id)
    }
  } catch (error) {
    console.error('Failed to remove associated meeting:', error)
  }
}

const openMeeting = (meeting: AssociatedMeeting) => {
  router.push(`/meetingRoom/${meeting.id}`)
}

const formatDate = (date: string): string => {
  if (!date) return ''
  try {
    return new Date(date).toLocaleDateString('ar-EG', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    })
  } catch {
    return date
  }
}

// Close results on outside click
const handleClickOutside = (e: MouseEvent) => {
  const target = e.target as HTMLElement
  if (!target.closest('.search-section')) {
    showResults.value = false
  }
}

// Lifecycle
onMounted(async () => {
  document.addEventListener('click', handleClickOutside)

  if (props.meetingId && meetings.value.length === 0) {
    loading.value = true
    try {
      const response = await MeetingsService.getAssociatedMeetings(props.meetingId.toString())
      const data = response?.data || response || []
      if (Array.isArray(data) && data.length > 0) {
        meetings.value = data
      }
    } catch (error) {
      console.error('Failed to load associated meetings:', error)
    } finally {
      loading.value = false
    }
  }
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  if (searchTimeout) {
    clearTimeout(searchTimeout)
  }
})
</script>

<style scoped>
.associated-meetings {
  @apply space-y-4;
}

/* Search Section */
.search-section {
  @apply relative;
}

.search-input-wrapper {
  @apply flex-1 relative;
}

.search-icon {
  @apply absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-zinc-400 pointer-events-none;
}

.loading-icon {
  @apply absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-primary;
}

.search-input {
  @apply w-full pl-10 pr-10 py-2.5 text-sm border border-zinc-200 rounded-lg bg-white;
  @apply focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none;
  @apply placeholder:text-zinc-400 transition-all duration-200;
}

/* Search Results */
.search-results {
  @apply absolute top-full left-0 right-0 mt-2 bg-white rounded-xl border border-zinc-200 shadow-lg z-20;
  @apply max-h-64 overflow-auto;
}

.result-item {
  @apply flex items-center gap-3 px-4 py-3 cursor-pointer transition-colors;
  @apply hover:bg-zinc-50;
}

.result-info {
  @apply flex-1 min-w-0;
}

.result-title {
  @apply block text-sm font-medium text-zinc-900 truncate;
}

.result-meta {
  @apply flex items-center gap-2 mt-0.5;
}

.result-ref {
  @apply text-xs text-primary font-mono;
}

.result-date {
  @apply text-xs text-zinc-500;
}

.no-results {
  @apply absolute top-full left-0 right-0 mt-2 px-4 py-3 bg-white rounded-xl border border-zinc-200 shadow-lg;
  @apply text-sm text-zinc-500 text-center;
}

/* Meetings List */
.meetings-list {
  @apply bg-white rounded-xl border border-zinc-100;
}

.list-header {
  @apply flex items-center justify-between px-4 py-3 border-b border-zinc-100;
  @apply text-sm font-semibold text-zinc-900;
}

.count {
  @apply px-2 py-0.5 rounded-full text-xs font-medium bg-zinc-100 text-zinc-600;
}

/* Loading State */
.loading-state {
  @apply p-4 space-y-3;
}

.skeleton-item {
  @apply h-14 bg-zinc-100 rounded-lg animate-pulse;
}

/* Empty State */
.empty-state {
  @apply flex flex-col items-center justify-center py-8 text-zinc-400;
}

.empty-state span {
  @apply text-sm mt-2;
}

/* Meetings Grid */
.meetings-grid {
  @apply divide-y divide-zinc-50;
}

.meeting-item {
  @apply flex items-center gap-3 px-4 py-3 hover:bg-zinc-50/50 transition-colors;
}

.meeting-icon {
  @apply w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center text-primary flex-shrink-0;
}

.meeting-info {
  @apply flex-1 min-w-0;
}

.meeting-title {
  @apply text-sm font-medium text-zinc-900 truncate;
}

.meeting-meta {
  @apply flex items-center gap-2 mt-0.5;
}

.meeting-ref {
  @apply text-xs text-primary font-mono;
}

.meeting-date {
  @apply text-xs text-zinc-500;
}

.meeting-actions {
  @apply flex items-center gap-1;
}

.meeting-actions button {
  @apply w-8 h-8 rounded-lg flex items-center justify-center text-zinc-400;
  @apply hover:bg-zinc-100 hover:text-zinc-600 transition-colors;
}

.meeting-actions .remove-btn:hover {
  @apply bg-red-50 text-red-500;
}
</style>

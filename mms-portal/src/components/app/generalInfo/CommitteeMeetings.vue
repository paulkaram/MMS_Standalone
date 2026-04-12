<template>
  <div class="meetings-section">
    <!-- Loading -->
    <div v-if="loading && meetings.length === 0" class="loading-state">
      <div class="spinner"></div>
    </div>

    <!-- Meetings List -->
    <div v-else-if="meetings.length > 0" class="meetings-list">
      <div
        v-for="meeting in meetings"
        :key="meeting.id"
        class="meeting-card"
        @click="viewMeeting(meeting)"
      >
        <!-- Status Indicator Line -->
        <div class="status-line" :style="{ backgroundColor: getMeetingStatusColor(meeting.statusId).text }"></div>

        <!-- Card Content -->
        <div class="card-body">
          <!-- Top Row: Title & Status -->
          <div class="card-top">
            <h4 class="meeting-title">{{ meeting.title }}</h4>
            <span
              class="status-badge"
              :style="getStatusStyle(getMeetingStatusColor(meeting.statusId))"
            >
              {{ meeting.statusName }}
            </span>
          </div>

          <!-- Bottom Row: Meta Info -->
          <div class="card-bottom">
            <div class="meta-group">
              <!-- Reference Number -->
              <div class="meta-chip" v-if="meeting.referenceNumber">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M7 20l4-16m2 16l4-16M6 9h14M4 15h14"/>
                </svg>
                <span>{{ meeting.referenceNumber }}</span>
              </div>

              <!-- Date -->
              <div class="meta-chip date-chip">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
                  <line x1="16" y1="2" x2="16" y2="6"/>
                  <line x1="8" y1="2" x2="8" y2="6"/>
                  <line x1="3" y1="10" x2="21" y2="10"/>
                </svg>
                <span>{{ formatDate(meeting.date) }}</span>
              </div>

              <!-- Time -->
              <div class="meta-chip time-chip" v-if="meeting.startTime">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <circle cx="12" cy="12" r="10"/>
                  <polyline points="12 6 12 12 16 14"/>
                </svg>
                <span>{{ meeting.startTime }}</span>
              </div>
            </div>

            <!-- Arrow Button -->
            <button class="arrow-btn" v-if="!isDraft(meeting.statusId)">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M9 18l6-6-6-6" stroke-linecap="round" stroke-linejoin="round"/>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
        <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
        <line x1="16" y1="2" x2="16" y2="6"/>
        <line x1="8" y1="2" x2="8" y2="6"/>
        <line x1="3" y1="10" x2="21" y2="10"/>
      </svg>
      <p>{{ $t('NoMeetings') }}</p>
    </div>

    <!-- Pagination -->
    <div v-if="totalCount > pageSize" class="pagination">
      <button class="page-btn" :disabled="page === 1" @click="goToPage(page - 1)">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M15 18l-6-6 6-6" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </button>
      <span class="page-info">{{ page }} / {{ totalPages }}</span>
      <button class="page-btn" :disabled="page >= totalPages" @click="goToPage(page + 1)">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M9 18l6-6-6-6" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </button>
    </div>

    <!-- Meeting Details Modal -->
    <Modal
      v-model="meetingDialogOpened"
      :title="selectedMeeting?.title || $t('MeetingDetails')"
      icon="mdi:calendar"
      size="md"
    >
      <div v-if="selectedMeeting" class="cm-detail">
        <div class="cm-detail-grid">
          <div class="cm-detail-item">
            <span class="cm-detail-label">{{ $t('Status') }}</span>
            <span class="cm-detail-badge" :style="getStatusStyle(getMeetingStatusColor(selectedMeeting.statusId))">
              {{ selectedMeeting.statusName }}
            </span>
          </div>
          <div class="cm-detail-item">
            <span class="cm-detail-label">{{ $t('Date') }}</span>
            <span class="cm-detail-value">
              <Icon icon="mdi:calendar" class="w-3.5 h-3.5" style="color:#94a3b8" />
              {{ formatFullDate(selectedMeeting.date) }}
            </span>
          </div>
          <div class="cm-detail-item">
            <span class="cm-detail-label">{{ $t('Location') }}</span>
            <span class="cm-detail-value">
              <Icon icon="mdi:map-marker" class="w-3.5 h-3.5" style="color:#94a3b8" />
              {{ selectedMeeting.location || '-' }}
            </span>
          </div>
          <div class="cm-detail-item">
            <span class="cm-detail-label">{{ $t('ReferenceNumber') }}</span>
            <span class="cm-detail-ref">{{ selectedMeeting.referenceNumber || '-' }}</span>
          </div>
        </div>
      </div>
      <template #footer>
        <button class="cm-btn-close" @click="meetingDialogOpened = false">{{ $t('Close') }}</button>
        <router-link
          v-if="selectedMeeting && !isDraft(selectedMeeting.statusId)"
          :to="{ name: 'meetingRoom', params: { id: selectedMeeting.id } }"
          class="cm-btn-open"
        >
          <Icon icon="mdi:open-in-new" class="w-4 h-4" />
          {{ $t('OpenMeeting') }}
        </router-link>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'
import { formatDate as formatDateHelper, getDateLocale } from '@/helpers/dateFormat'
import Modal from '@/components/ui/Modal.vue'
import Icon from '@/components/ui/Icon.vue'
import { MeetingStatusEnum } from '@/helpers/enumerations'
import { getMeetingStatusColor, getStatusStyle } from '@/helpers/statusColors'

const props = defineProps<{
  committeeId: string
  userPermissions?: any
}>()

const emit = defineEmits<{
  'update:count': [count: number]
}>()

// State
const loading = ref(false)
const meetings = ref<any[]>([])
const totalCount = ref(0)
const page = ref(1)
const pageSize = ref(5)
const meetingDialogOpened = ref(false)
const selectedMeeting = ref<any>(null)

// Computed
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

// Methods
const loadMeetings = async () => {
  loading.value = true
  try {
    const result = await CouncilCommitteesService.listCouncilCommitteeMeetings(
      props.committeeId,
      page.value,
      pageSize.value
    )
    // API returns { success, data: { data: [], total } }
    meetings.value = result.data?.data || result.data || []
    totalCount.value = result.data?.total || result.total || 0
    emit('update:count', totalCount.value)
  } catch (error) {
    console.error('Failed to load meetings:', error)
  } finally {
    loading.value = false
  }
}

const goToPage = (newPage: number) => {
  page.value = newPage
  loadMeetings()
}

const viewMeeting = (meeting: any) => {
  selectedMeeting.value = meeting
  meetingDialogOpened.value = true
}

const isDraft = (statusId: number) => statusId === MeetingStatusEnum.Draft

const formatDate = (date: string) => {
  if (!date) return '-'
  return new Date(date).toLocaleDateString(getDateLocale(), {
    day: 'numeric',
    month: 'short',
    calendar: 'gregory'
  })
}

const formatFullDate = (date: string) => {
  if (!date) return '-'
  return formatDateHelper(new Date(date))
}

// Watch
watch(() => props.committeeId, () => {
  page.value = 1
  loadMeetings()
})

// Lifecycle
onMounted(() => {
  loadMeetings()
})
</script>

<style scoped>
.meetings-section {
  /* No wrapper styling - parent card provides it */
}

/* Loading */
.loading-state {
  display: flex;
  justify-content: center;
  padding: 40px;
}

.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid #e4e4e7;
  border-top-color: #006d4b;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Meetings List */
.meetings-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

/* Meeting Card - Compact horizontal design */
.meeting-card {
  display: flex;
  align-items: stretch;
  background: #fff;
  border: 1px solid #e4e4e7;
  border-radius: 10px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.2s ease;
}

.meeting-card:hover {
  border-color: #d4d4d8;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

/* Status Line - Left colored border */
.status-line {
  width: 4px;
  flex-shrink: 0;
}

/* Card Body */
.card-body {
  flex: 1;
  padding: 12px 14px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;
}

/* Top Row */
.card-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.meeting-title {
  flex: 1;
  font-size: 14px;
  font-weight: 600;
  color: #27272a;
  margin: 0;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* Status Badge */
.status-badge {
  flex-shrink: 0;
  font-size: 11px;
  padding: 4px 10px;
  border-radius: 6px;
  font-weight: 600;
  white-space: nowrap;
  border: 1px solid;
}

/* Bottom Row */
.card-bottom {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.meta-group {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

/* Meta Chips */
.meta-chip {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 11px;
  color: #71717a;
  padding: 3px 8px;
  background: #f4f4f5;
  border-radius: 6px;
}

.meta-chip svg {
  width: 12px;
  height: 12px;
  flex-shrink: 0;
  opacity: 0.7;
}

.date-chip {
  background: rgba(0, 109, 75, 0.08);
  color: #006d4b;
}

.date-chip svg {
  opacity: 1;
}

.time-chip {
  background: rgba(59, 130, 246, 0.08);
  color: #2563eb;
}

.time-chip svg {
  opacity: 1;
}

/* Arrow Button */
.arrow-btn {
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fafafa;
  border: 1px solid #e4e4e7;
  border-radius: 6px;
  color: #a1a1aa;
  cursor: pointer;
  transition: all 0.2s;
  flex-shrink: 0;
}

.arrow-btn:hover {
  background: #006d4b;
  border-color: #006d4b;
  color: #fff;
}

.arrow-btn svg {
  width: 14px;
  height: 14px;
}

[dir="rtl"] .arrow-btn svg {
  transform: scaleX(-1);
}

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px;
  color: #a1a1aa;
}

.empty-state svg {
  width: 48px;
  height: 48px;
  margin-bottom: 12px;
}

.empty-state p {
  font-size: 14px;
  margin: 0;
}

/* Pagination */
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid #e4e4e7;
}

.page-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fff;
  border: 1px solid #e4e4e7;
  border-radius: 6px;
  color: #71717a;
  cursor: pointer;
  transition: all 0.2s;
}

.page-btn:hover:not(:disabled) {
  background: #fafafa;
  border-color: #d4d4d8;
  color: #3f3f46;
}

.page-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-btn svg {
  width: 16px;
  height: 16px;
}

[dir="rtl"] .page-btn svg {
  transform: scaleX(-1);
}

.page-info {
  font-size: 13px;
  color: #71717a;
}

/* Modal */
/* Meeting Detail Modal */
.cm-detail { padding: 4px 0; }
.cm-detail-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
.cm-detail-item { display: flex; flex-direction: column; gap: 4px; }
.cm-detail-label { font-size: 11px; color: #94a3b8; font-weight: 500; }
.cm-detail-value { font-size: 14px; color: #0f172a; font-weight: 500; display: flex; align-items: center; gap: 5px; }
.cm-detail-badge { display: inline-block; padding: 3px 10px; border-radius: 6px; font-size: 12px; font-weight: 600; }
.cm-detail-ref {
  display: inline-block; padding: 3px 10px;
  background: rgba(0, 109, 75, 0.1); color: #006d4b;
  border-radius: 6px; font-size: 13px; font-weight: 700;
  font-family: 'Consolas', monospace;
}
.cm-btn-close {
  padding: 9px 18px; border-radius: 8px;
  background: #fff; border: 1px solid #e2e8f0;
  color: #475569; font-size: 13px; font-weight: 500;
  cursor: pointer; transition: all 0.15s; font-family: inherit;
}
.cm-btn-close:hover { background: #f8fafc; }
.cm-btn-open {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 9px 18px; border-radius: 8px;
  background: linear-gradient(135deg, #006d4b 0%, #006d4b 100%);
  color: #fff; font-size: 13px; font-weight: 600;
  text-decoration: none; transition: all 0.15s;
}
.cm-btn-open:hover { opacity: 0.9; box-shadow: 0 4px 12px rgba(0,174,140,0.25); }
</style>

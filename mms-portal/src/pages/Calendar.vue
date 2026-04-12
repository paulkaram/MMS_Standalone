<template>
  <div class="space-y-4">
    <!-- Page Header -->
    <PageHeader
      :title="$t('MeetingsCalendar')"
      :subtitle="$t('ViewAndManageMeetings')"
    >
      <template #actions>
        <button class="btn-clean primary" @click="$router.push('/addMeeting')">
          <Icon icon="mdi:plus" class="w-4 h-4" />
          {{ $t('NewMeeting') }}
        </button>
      </template>
    </PageHeader>

    <!-- Calendar Container -->
    <div class="grid grid-cols-1 xl:grid-cols-4 gap-4">
      <!-- Main Content Area -->
      <div class="xl:col-span-3">
        <div class="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <!-- Clean Unified Header -->
          <div class="calendar-header">
            <!-- Right Section: Main View Toggle -->
            <div class="header-section">
              <div class="view-toggle-group">
                <button
                  :class="['view-btn', { active: mainView === 'calendar' }]"
                  @click="mainView = 'calendar'"
                >
                  <span>{{ $t('Calendar') }}</span>
                  <svg class="btn-icon" viewBox="0 0 24 24" fill="currentColor">
                    <path d="M19 19H5V8h14m-3-7v2H8V1H6v2H5c-1.11 0-2 .89-2 2v14a2 2 0 002 2h14a2 2 0 002-2V5a2 2 0 00-2-2h-1V1h-2v2zM9 10H7v2h2v-2m4 0h-2v2h2v-2m4 0h-2v2h2v-2m-8 4H7v2h2v-2m4 0h-2v2h2v-2m4 0h-2v2h2v-2z"/>
                  </svg>
                </button>
                <button
                  :class="['view-btn', { active: mainView === 'list' }]"
                  @click="mainView = 'list'"
                >
                  <span>{{ $t('List') }}</span>
                  <svg class="btn-icon" viewBox="0 0 24 24" fill="currentColor">
                    <path d="M3 13h2v-2H3v2zm0 4h2v-2H3v2zm0-8h2V7H3v2zm4 4h14v-2H7v2zm0 4h14v-2H7v2zM7 7v2h14V7H7z"/>
                  </svg>
                </button>
                <button
                  :class="['view-btn', { active: mainView === 'timeline' }]"
                  @click="mainView = 'timeline'"
                >
                  <span>{{ $t('Timeline') }}</span>
                  <svg class="btn-icon" viewBox="0 0 24 24" fill="currentColor">
                    <path d="M15 11V5l-3-3-3 3v2H3v14h18V11h-6zm-8 8H5v-2h2v2zm0-4H5v-2h2v2zm0-4H5V9h2v2zm6 8h-2v-2h2v2zm0-4h-2v-2h2v2zm0-4h-2V9h2v2zm0-4h-2V5h2v2zm6 12h-2v-2h2v2zm0-4h-2v-2h2v2z"/>
                  </svg>
                </button>
              </div>

              <!-- Divider -->
              <div v-if="mainView === 'calendar'" class="header-divider"></div>

              <!-- Calendar Period Toggle -->
              <div v-if="mainView === 'calendar'" class="period-toggle-group">
                <button
                  :class="['period-btn', { active: currentView === 'dayGridMonth' }]"
                  @click="changeView('dayGridMonth')"
                >
                  {{ $t('Monthly') }}
                </button>
                <button
                  :class="['period-btn', { active: currentView === 'timeGridWeek' }]"
                  @click="changeView('timeGridWeek')"
                >
                  {{ $t('Weekly') }}
                </button>
                <button
                  :class="['period-btn', { active: currentView === 'timeGridDay' }]"
                  @click="changeView('timeGridDay')"
                >
                  {{ $t('Daily') }}
                </button>
              </div>

              <!-- List/Timeline Sort -->
              <div v-if="mainView === 'list' || mainView === 'timeline'" class="list-controls">
                <CustomSelect
                  v-model="listSortOrder"
                  :options="sortOptions"
                  size="small"
                  class="sort-dropdown"
                />
              </div>
              <span v-if="mainView === 'list' || mainView === 'timeline'" class="results-count-badge">{{ filteredEvents.length }}</span>
            </div>

            <!-- Left Section: Navigation (Calendar only) -->
            <div v-if="mainView === 'calendar'" class="header-section">
              <div class="nav-group">
                <button class="nav-btn" @click="goToToday">{{ $t('Today') }}</button>
                <div class="nav-arrows">
                  <button class="arrow-btn" @click="navigatePrev">
                    <svg viewBox="0 0 24 24" fill="currentColor">
                      <path v-if="isRtl" d="M8.59 16.59L13.17 12 8.59 7.41 10 6l6 6-6 6-1.41-1.41z"/>
                      <path v-else d="M15.41 16.59L10.83 12l4.58-4.59L14 6l-6 6 6 6 1.41-1.41z"/>
                    </svg>
                  </button>
                  <span class="current-period">{{ currentTitle }}</span>
                  <button class="arrow-btn" @click="navigateNext">
                    <svg viewBox="0 0 24 24" fill="currentColor">
                      <path v-if="isRtl" d="M15.41 16.59L10.83 12l4.58-4.59L14 6l-6 6 6 6 1.41-1.41z"/>
                      <path v-else d="M8.59 16.59L13.17 12 8.59 7.41 10 6l6 6-6 6-1.41-1.41z"/>
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Content Views -->
          <Transition name="view-slide" mode="out-in">
            <!-- Calendar View -->
            <div v-if="mainView === 'calendar'" key="calendar" class="calendar-wrapper">
              <FullCalendar
                ref="calendarRef"
                :options="calendarOptions"
              />
            </div>

            <!-- List View -->
            <div v-else-if="mainView === 'list'" key="list" class="list-view-container">
              <div v-if="sortedMeetings.length > 0" class="meetings-list">
                <div
                  v-for="(meeting, index) in sortedMeetings"
                  :key="meeting.id"
                  class="meeting-list-card"
                  :style="{ '--delay': `${index * 40}ms` }"
                  @click="openMeetingById(meeting.extendedProps?.meetingId || meeting.id)"
                >
                  <div
                    class="card-accent"
                    :style="{ backgroundColor: meeting.backgroundColor }"
                  ></div>
                  <div class="card-content">
                    <div class="card-header">
                      <div class="card-icon" :style="{ backgroundColor: meeting.backgroundColor + '20', color: meeting.backgroundColor }">
                        <Icon icon="mdi:calendar-clock" class="w-5 h-5" />
                      </div>
                      <div
                        class="status-badge"
                        :style="{
                          backgroundColor: meeting.backgroundColor + '15',
                          color: meeting.backgroundColor
                        }"
                      >
                        {{ getStatusName(meeting.extendedProps?.statusId) }}
                      </div>
                    </div>
                    <h4 class="card-title">{{ meeting.title }}</h4>
                    <div class="card-meta">
                      <div class="meta-item">
                        <Icon icon="mdi:calendar" class="w-4 h-4" />
                        <span>{{ formatMeetingDate(meeting.start) }}</span>
                      </div>
                      <div class="meta-item">
                        <Icon icon="mdi:clock-outline" class="w-4 h-4" />
                        <span>{{ formatMeetingTime(meeting.start) }}</span>
                      </div>
                    </div>
                  </div>
                  <div class="card-arrow">
                    <Icon :icon="isRtl ? 'mdi:arrow-left' : 'mdi:arrow-right'" class="w-5 h-5" />
                  </div>
                </div>
              </div>

              <div v-else class="empty-state">
                <div class="empty-icon">
                  <Icon icon="mdi:calendar-blank-outline" class="w-16 h-16" />
                </div>
                <p class="empty-title">{{ $t('NoMeetings') }}</p>
                <p class="empty-subtitle">{{ $t('NoMeetingsMatchFilter') }}</p>
              </div>
            </div>

            <!-- Timeline View -->
            <div v-else-if="mainView === 'timeline'" key="timeline" class="timeline-view-container">
              <div v-if="groupedByDate.length > 0" class="timeline-wrapper">
                <div
                  v-for="(group, groupIndex) in groupedByDate"
                  :key="group.dateKey"
                  class="timeline-group"
                  :style="{ '--group-delay': `${groupIndex * 80}ms` }"
                >
                  <div class="timeline-date-marker">
                    <div class="date-badge" :class="{ today: group.isToday }">
                      <span class="date-day">{{ group.dayNum }}</span>
                      <span class="date-month">{{ group.monthName }}</span>
                      <span class="date-weekday">{{ group.weekday }}</span>
                    </div>
                    <div v-if="group.isToday" class="today-badge">{{ $t('Today') }}</div>
                  </div>

                  <div class="timeline-track">
                    <div class="track-line"></div>
                  </div>

                  <div class="timeline-events">
                    <div
                      v-for="(meeting, mIndex) in group.meetings"
                      :key="meeting.id"
                      class="timeline-event-card"
                      :style="{ '--item-delay': `${mIndex * 50}ms` }"
                      @click="openMeetingById(meeting.extendedProps?.meetingId || meeting.id)"
                    >
                      <div class="event-time">
                        {{ formatMeetingTime(meeting.start) }}
                      </div>
                      <div class="event-connector">
                        <div
                          class="connector-dot"
                          :style="{ backgroundColor: meeting.backgroundColor }"
                        ></div>
                      </div>
                      <div class="event-content">
                        <div class="event-header">
                          <h5 class="event-title">{{ meeting.title }}</h5>
                          <div
                            class="event-status"
                            :style="{
                              backgroundColor: meeting.backgroundColor + '15',
                              color: meeting.backgroundColor
                            }"
                          >
                            {{ getStatusName(meeting.extendedProps?.statusId) }}
                          </div>
                        </div>
                        <div class="event-meta">
                          <span v-if="meeting.extendedProps?.location" class="event-tag">
                            <Icon icon="mdi:map-marker-outline" class="w-3 h-3" />
                            {{ meeting.extendedProps.location }}
                          </span>
                          <span class="event-tag">
                            <Icon icon="mdi:clock-outline" class="w-3 h-3" />
                            {{ formatDuration(meeting.start, meeting.end) }}
                          </span>
                        </div>
                      </div>
                      <div class="event-arrow">
                        <Icon :icon="isRtl ? 'mdi:chevron-left' : 'mdi:chevron-right'" class="w-5 h-5" />
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div v-else class="empty-state">
                <div class="empty-icon">
                  <Icon icon="mdi:timeline-clock-outline" class="w-16 h-16" />
                </div>
                <p class="empty-title">{{ $t('NoMeetings') }}</p>
                <p class="empty-subtitle">{{ $t('NoMeetingsMatchFilter') }}</p>
              </div>
            </div>
          </Transition>
        </div>
      </div>

      <!-- Sidebar -->
      <div class="sidebar-stack">
        <!-- Status Filter Card -->
        <div class="sidebar-card">
          <div class="sidebar-card-header">
            <span class="sidebar-card-title">{{ $t('Filters') }}</span>
            <span v-if="activeFilterId !== 0" class="filter-clear" @click="toggleFilter(0)">{{ $t('All') }}</span>
          </div>
          <div class="filter-list">
            <button
              v-for="status in statusTypes"
              :key="status.id"
              :class="['filter-row', { active: activeFilterId === status.id }]"
              @click="toggleFilter(status.id)"
            >
              <span class="filter-bar" :style="{ backgroundColor: status.color }"></span>
              <span class="filter-label">{{ status.name }}</span>
              <span class="filter-count">{{ getFilterCount(status.id) }}</span>
            </button>
          </div>
        </div>

        <!-- Quick Stats Card -->
        <div class="sidebar-card stats-dark">
          <div class="stats-content">
            <div class="stats-row-header">
              <span class="stats-title">{{ $t('ThisMonth') }}</span>
            </div>
            <div class="stats-total-row">
              <span class="stats-total-number">{{ monthStats.total }}</span>
              <span class="stats-total-label">{{ $t('TotalMeetings') }}</span>
            </div>
            <div class="stats-grid">
              <div class="stats-mini">
                <span class="stats-mini-value">{{ monthStats.upcoming }}</span>
                <span class="stats-mini-label">{{ $t('Upcoming') }}</span>
              </div>
              <div class="stats-mini">
                <span class="stats-mini-value">{{ monthStats.completed }}</span>
                <span class="stats-mini-label">{{ $t('Completed') }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Upcoming Meetings Card -->
        <div class="sidebar-card">
          <div class="sidebar-card-header">
            <span class="sidebar-card-title">{{ $t('UpcomingMeetings') }}</span>
            <span class="upcoming-count">{{ upcomingMeetings.length }}</span>
          </div>
          <div v-if="upcomingMeetings.length > 0" class="upcoming-list">
            <div
              v-for="meeting in upcomingMeetings.slice(0, 3)"
              :key="meeting.id"
              class="upcoming-row"
              @click="openMeeting(meeting)"
            >
              <div class="upcoming-color" :style="{ backgroundColor: meeting.color }"></div>
              <div class="upcoming-info">
                <p class="upcoming-title">{{ meeting.title }}</p>
                <p class="upcoming-time">{{ meeting.time }}</p>
              </div>
            </div>
          </div>
          <div v-else class="upcoming-empty">
            <p>{{ $t('NoUpcomingMeetings') }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import Icon from '@/components/ui/Icon.vue'
import CustomSelect from '@/components/ui/CustomSelect.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin from '@fullcalendar/daygrid'
import timeGridPlugin from '@fullcalendar/timegrid'
import interactionPlugin from '@fullcalendar/interaction'
import arLocale from '@fullcalendar/core/locales/ar'
import moment from 'moment-hijri'
import { useUserStore } from '@/stores/user'
import MeetingsService from '@/services/MeetingsService'
import LookupsService from '@/services/LookupsService'

const { t } = useI18n()
const router = useRouter()
const userStore = useUserStore()

const isRtl = computed(() => userStore.isRtl)
const calendarRef = ref<InstanceType<typeof FullCalendar> | null>(null)
const currentTitle = ref('')
const currentView = ref('dayGridMonth')

// Main view state (calendar, list, timeline)
const mainView = ref<'calendar' | 'list' | 'timeline'>('calendar')
const listSortOrder = ref<'asc' | 'desc'>('asc')
const sortOptions = computed(() => [
  { id: 'asc', name: t('NearestFirst') },
  { id: 'desc', name: t('FarthestFirst') }
])

// Status filters - loaded from API
const selectedStatus = ref(0)
const activeFilterId = ref(0)
const statusTypes = ref<Array<{ id: number; name: string; color: string }>>([])

// Status colors mapping by ID
const statusColors: Record<number, string> = {
  0: '#6f9ff2',  // All
  1: '#9e9e9e',  // Draft - Gray
  2: '#ff9800',  // PendingMeetingApproval - Orange
  3: '#884dff',  // Approved - Purple
  4: '#52dbce',  // Started/Running - Teal
  5: '#f44336',  // Finished - Red
  6: '#f2af6f',  // PendingInitialMeetingMinutesApproval - Light Orange
  7: '#2196f3',  // InitialMeetingMinutesApproved - Blue
  8: '#4caf50',  // PendingFinalMeetingMinutesSign - Green
  9: '#4fbab0',  // FinalMeetingMinutesSigned - Teal Green
}

function getStatusName(statusId?: number): string {
  if (statusId === undefined) return ''
  const status = statusTypes.value.find(s => s.id === statusId)
  return status?.name || ''
}

function getFilterCount(statusId: number): number {
  if (statusId === 0) return events.value.length
  return events.value.filter(e => e.extendedProps?.statusId === statusId).length
}

// Load meeting statuses from API
async function loadMeetingStatuses() {
  try {
    const response = await LookupsService.getMeetingStatuses()
    const statuses = Array.isArray(response) ? response : (response?.data || [])

    statusTypes.value = [
      { id: 0, name: t('All'), color: statusColors[0] }
    ]

    statuses.forEach((status: any) => {
      statusTypes.value.push({
        id: Number(status.id),
        name: status.name,
        color: statusColors[Number(status.id)] || '#6f9ff2'
      })
    })
  } catch (error) {
    console.error('Failed to load meeting statuses:', error)
  }
}
const events = ref<any[]>([])
const filteredEvents = ref<any[]>([])

const monthStats = reactive({
  total: 0,
  upcoming: 0,
  completed: 0
})

// Sorted meetings for list view
const sortedMeetings = computed(() => {
  const sorted = [...filteredEvents.value].sort((a, b) => {
    const dateA = new Date(a.start).getTime()
    const dateB = new Date(b.start).getTime()
    return listSortOrder.value === 'asc' ? dateA - dateB : dateB - dateA
  })
  return sorted
})

// Grouped meetings by date for timeline view
const groupedByDate = computed(() => {
  const groups: Record<string, any> = {}
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const locale = isRtl.value ? 'ar-EG' : 'en-US'

  sortedMeetings.value.forEach(meeting => {
    const date = new Date(meeting.start)
    const dateKey = date.toISOString().split('T')[0]

    if (!groups[dateKey]) {
      const meetingDate = new Date(date)
      meetingDate.setHours(0, 0, 0, 0)

      groups[dateKey] = {
        dateKey,
        dayNum: date.getDate(),
        monthName: date.toLocaleDateString(locale, { month: 'short' }),
        weekday: date.toLocaleDateString(locale, { weekday: 'short' }),
        isToday: meetingDate.getTime() === today.getTime(),
        meetings: []
      }
    }
    groups[dateKey].meetings.push(meeting)
  })

  return Object.values(groups).sort((a: any, b: any) => {
    return listSortOrder.value === 'asc'
      ? a.dateKey.localeCompare(b.dateKey)
      : b.dateKey.localeCompare(a.dateKey)
  })
})

const upcomingMeetings = computed(() => {
  const now = new Date()
  return events.value
    .filter(e => new Date(e.start) > now)
    .sort((a, b) => new Date(a.start).getTime() - new Date(b.start).getTime())
    .map(e => ({
      id: e.extendedProps?.meetingId || e.id,
      title: e.title,
      time: new Date(e.start).toLocaleDateString(isRtl.value ? 'ar-EG' : 'en-US', {
        weekday: 'short',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      }),
      color: e.backgroundColor
    }))
})

// Format helpers
function formatMeetingDate(dateStr: string): string {
  const date = new Date(dateStr)
  return date.toLocaleDateString(isRtl.value ? 'ar-EG' : 'en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

function formatMeetingTime(dateStr: string): string {
  const date = new Date(dateStr)
  return date.toLocaleTimeString(isRtl.value ? 'ar-EG' : 'en-US', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

function formatDuration(startStr: string, endStr: string): string {
  const start = new Date(startStr)
  const end = new Date(endStr)
  const diffMs = end.getTime() - start.getTime()
  const diffMins = Math.round(diffMs / 60000)

  if (diffMins < 60) {
    return `${diffMins} ${t('Minutes')}`
  }
  const hours = Math.floor(diffMins / 60)
  const mins = diffMins % 60
  if (mins === 0) {
    return `${hours} ${t('Hours')}`
  }
  return isRtl.value
    ? `${hours} ${t('Hours')} و ${mins} ${t('Minutes')}`
    : `${hours} ${t('Hours')} ${mins} ${t('Minutes')}`
}

// Convert to Hijri
function toHijri(date: Date): string {
  return moment(date).format('iD')
}

// Calendar options
const calendarOptions = computed(() => ({
  plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
  initialView: 'dayGridMonth',
  locale: isRtl.value ? arLocale : undefined,
  direction: isRtl.value ? 'rtl' : 'ltr',
  headerToolbar: false,
  height: 'auto',
  contentHeight: 600,
  timeZone: 'local', // Treat all dates as local time to avoid UTC conversion
  events: filteredEvents.value,
  eventClick: handleEventClick,
  datesSet: handleDatesSet,
  dayCellDidMount: (arg: any) => {
    const hijriDay = toHijri(arg.date)
    const topEl = arg.el.querySelector('.fc-daygrid-day-top')
    if (topEl) {
      topEl.innerHTML = `<div class="day-numbers"><span class="gregorian-day">${arg.dayNumberText}</span><span class="hijri-day">${hijriDay}</span></div>`
    }
  },
  dayHeaderFormat: { weekday: 'long' as const },
  eventDisplay: 'block',
  eventTimeFormat: {
    hour: '2-digit',
    minute: '2-digit',
    meridiem: false
  }
}))

function goToToday() {
  calendarRef.value?.getApi().today()
}

function navigatePrev() {
  calendarRef.value?.getApi().prev()
}

function navigateNext() {
  calendarRef.value?.getApi().next()
}

function changeView(view: string) {
  currentView.value = view
  calendarRef.value?.getApi().changeView(view)
}

function handleDatesSet(info: any) {
  currentTitle.value = info.view.title
  loadMeetings(info.start, info.end)
}

function handleEventClick(info: any) {
  const meetingId = info.event.extendedProps?.meetingId || info.event.id
  router.push(`/meetingRoom/${meetingId}`)
}

function openMeeting(meeting: any) {
  router.push(`/meetingRoom/${meeting.id}`)
}

function openMeetingById(meetingId: number | string) {
  router.push(`/meetingRoom/${meetingId}`)
}

function toggleFilter(statusId: number) {
  activeFilterId.value = statusId
  selectedStatus.value = statusId
  applyFilter()
}

function applyFilter() {
  if (selectedStatus.value === 0) {
    filteredEvents.value = [...events.value]
  } else {
    filteredEvents.value = events.value.filter(
      event => event.extendedProps?.statusId === selectedStatus.value
    )
  }
}

// Format date as local ISO string (YYYY-MM-DDTHH:mm:ss) without UTC conversion
function toLocalISOString(date: Date): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  const hours = String(date.getHours()).padStart(2, '0')
  const minutes = String(date.getMinutes()).padStart(2, '0')
  const seconds = String(date.getSeconds()).padStart(2, '0')
  return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`
}

async function loadMeetings(startDate: Date, endDate: Date) {
  try {
    const response = await MeetingsService.listMeetingsForCalendar({
      StartDate: toLocalISOString(startDate),
      EndDate: toLocalISOString(endDate)
    })

    const meetingsData = response?.data || response?.items || response || []

    events.value = meetingsData.map((meeting: any) => {
      // Extract date part only (YYYY-MM-DD) to avoid timezone issues
      let dateStr = ''
      if (meeting.date) {
        // Handle both "2024-01-24" and "2024-01-24T00:00:00" formats
        dateStr = meeting.date.substring(0, 10)
      }

      return {
        id: meeting.id,
        title: meeting.title || meeting.titleAr,
        start: dateStr
          ? `${dateStr}T${meeting.startTime || '09:00'}:00`
          : meeting.startDate,
        end: dateStr
          ? `${dateStr}T${meeting.endTime || '10:00'}:00`
          : meeting.endDate,
        backgroundColor: statusColors[meeting.statusId] || '#6f9ff2',
        borderColor: 'transparent',
        textColor: '#ffffff',
        extendedProps: {
          meetingId: meeting.id,
          statusId: meeting.statusId,
          typeId: meeting.meetingTypeId,
          url: meeting.meetingUrl,
          location: meeting.location || meeting.locationName
        }
      }
    })

    // Update stats
    const now = new Date()
    monthStats.total = events.value.length
    monthStats.upcoming = events.value.filter(e => new Date(e.start) > now).length
    monthStats.completed = events.value.filter(e => new Date(e.end) < now).length

    applyFilter()
  } catch (error) {
    console.error('Failed to load meetings:', error)
    events.value = []
    filteredEvents.value = []
  }
}

// Load statuses on mount
onMounted(() => {
  loadMeetingStatuses()
})
</script>

<style scoped>
/* ═══════════════════════════════════════════════════════════════════════════ */
/* CLEAN UNIFIED HEADER */
/* ═══════════════════════════════════════════════════════════════════════════ */

/* ===== Calendar Toolbar — matching IAM filter-system ===== */
.calendar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 20px;
  background: linear-gradient(to bottom, #fff, #f8fafc);
  border-bottom: 1px solid #e2e8f0;
  gap: 16px;
}

.header-section {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-divider {
  width: 1px;
  height: 28px;
  background: #e2e8f0;
}

/* ===== View Toggle — IAM filter-pill pattern ===== */
.view-toggle-group {
  display: inline-flex;
  align-items: center;
  background: #f1f5f9;
  border-radius: 10px;
  padding: 4px;
  gap: 4px;
  border: 1px solid #e2e8f0;
}

.view-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 14px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 500;
  color: #475569;
  transition: all 0.15s ease;
  white-space: nowrap;
  border: 1px solid transparent;
  background: transparent;
  cursor: pointer;
}

.view-btn:hover {
  color: #334155;
  background: rgba(0, 109, 75, 0.04);
  border-color: rgba(0, 109, 75, 0.15);
}

.view-btn.active {
  background: #006d4b;
  color: #ffffff;
  border-color: #006d4b;
  box-shadow: 0 2px 8px rgba(0, 109, 75, 0.25);
}

.view-btn.active .btn-icon {
  opacity: 1;
}

.btn-icon {
  width: 14px;
  height: 14px;
  flex-shrink: 0;
  opacity: 0.6;
}

/* ===== Period Toggle — IAM filter-pill style ===== */
.period-toggle-group {
  display: inline-flex;
  align-items: center;
  background: #f1f5f9;
  border-radius: 8px;
  padding: 3px;
  gap: 2px;
  border: 1px solid #e2e8f0;
}

.period-btn {
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  color: #475569;
  border: 1px solid transparent;
  background: transparent;
  cursor: pointer;
  transition: all 0.15s ease;
}

.period-btn:hover {
  color: #334155;
  background: rgba(0, 109, 75, 0.04);
}

.period-btn.active {
  background: #fff;
  color: #006d4b;
  border-color: #e2e8f0;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
}

/* ===== List/Timeline Controls ===== */
.list-controls {
  display: flex;
  align-items: center;
  gap: 12px;
}

.results-count-badge {
  display: inline-flex; align-items: center; justify-content: center;
  min-width: 26px; height: 26px; padding: 0 8px;
  background: rgba(0, 109, 75, 0.1); color: #006d4b;
  border-radius: 13px; font-size: 12px; font-weight: 700;
  margin-inline-start: auto;
}

.sort-dropdown {
  min-width: 140px;
}

.sort-dropdown :deep(.custom-select) {
  padding: 6px 12px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 13px;
}

.sort-dropdown :deep(.custom-select:hover) {
  border-color: #006d4b;
}

.sort-dropdown :deep(.custom-select:focus),
.sort-dropdown :deep(.custom-select.open) {
  border-color: #006d4b;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

.sort-dropdown :deep(.select-value) {
  font-size: 13px;
  font-weight: 500;
}

.sort-dropdown :deep(.select-arrow) {
  width: 1rem;
  height: 1rem;
}

/* ===== Navigation — IAM pagination style ===== */
.nav-group {
  display: flex;
  align-items: center;
  gap: 12px;
}

.nav-btn {
  padding: 6px 14px;
  font-size: 13px;
  font-weight: 500;
  color: #006d4b;
  background: rgba(0, 109, 75, 0.08);
  border: 1px solid rgba(0, 109, 75, 0.15);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.nav-btn:hover {
  background: rgba(0, 109, 75, 0.15);
  border-color: rgba(0, 109, 75, 0.3);
}

.nav-arrows {
  display: flex;
  align-items: center;
  gap: 4px;
}

.arrow-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  color: #64748b;
  border: none;
  background: transparent;
  cursor: pointer;
  transition: all 0.2s ease;
}

.arrow-btn:hover {
  color: #006d4b;
  background: rgba(0, 109, 75, 0.08);
}

.arrow-btn svg {
  width: 18px;
  height: 18px;
}


.current-period {
  min-width: 120px;
  text-align: center;
  font-size: 14px;
  font-weight: 600;
  color: #006d4b;
  letter-spacing: -0.2px;
}

/* View transition */
.view-slide-enter-active,
.view-slide-leave-active {
  transition: all 0.25s ease;
}

.view-slide-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.view-slide-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* SIDEBAR                                                                   */
/* ═══════════════════════════════════════════════════════════════════════════ */

.sidebar-stack {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.sidebar-card {
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  overflow: hidden;
}

.sidebar-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 14px;
  border-bottom: 1px solid #f1f5f9;
}

.sidebar-card-title {
  font-size: 12px;
  font-weight: 600;
  color: #1e293b;
  letter-spacing: 0.01em;
}

/* --- Filter list (structured vertical rows) --- */
.filter-list {
  display: flex;
  flex-direction: column;
}

.filter-row {
  display: flex;
  align-items: center;
  gap: 0;
  padding: 0;
  font-size: 12px;
  font-weight: 500;
  color: #475569;
  background: transparent;
  border: none;
  border-bottom: 1px solid #f1f5f9;
  cursor: pointer;
  transition: background 0.12s ease;
  text-align: start;
  width: 100%;
  position: relative;
}

.filter-row:last-child {
  border-bottom: none;
}

.filter-row:hover {
  background: #f8fafc;
}

.filter-row.active {
  background: #f0fdf9;
}

.filter-row.active .filter-label {
  color: #004730;
  font-weight: 600;
}

.filter-row.active .filter-count {
  background: #004730;
  color: #fff;
}

.filter-bar {
  width: 3px;
  align-self: stretch;
  flex-shrink: 0;
  border-radius: 0;
  transition: width 0.12s ease;
}

.filter-row.active .filter-bar {
  width: 4px;
}

.filter-label {
  flex: 1;
  min-width: 0;
  padding: 9px 10px;
  line-height: 1.4;
  color: inherit;
}

.filter-count {
  flex-shrink: 0;
  min-width: 22px;
  height: 18px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  margin-inline-end: 10px;
  font-size: 10px;
  font-weight: 600;
  color: #94a3b8;
  background: #f1f5f9;
  border-radius: 4px;
  padding: 0 5px;
}

.filter-clear {
  font-size: 11px;
  font-weight: 500;
  color: #006d4b;
  cursor: pointer;
  transition: color 0.12s ease;
}

.filter-clear:hover {
  color: #007e65;
}

/* --- Stats card (dark) --- */
.stats-dark {
  background: linear-gradient(135deg, #006d4b 0%, #007a55 100%);
  border-color: #63a58f;
  color: #fff;
}

.stats-content {
  padding: 14px;
}

.stats-row-header {
  margin-bottom: 10px;
}

.stats-title {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: rgba(255, 255, 255, 0.55);
}

.stats-total-row {
  display: flex;
  align-items: baseline;
  gap: 8px;
  margin-bottom: 12px;
}

.stats-total-number {
  font-size: 26px;
  font-weight: 700;
  color: #006d4b;
  line-height: 1;
}

.stats-total-label {
  font-size: 11px;
  color: rgba(255, 255, 255, 0.4);
}

.stats-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}

.stats-mini {
  background: rgba(0, 109, 75, 0.08);
  border: 1px solid rgba(0, 109, 75, 0.15);
  border-radius: 8px;
  padding: 8px;
  text-align: center;
}

.stats-mini-value {
  display: block;
  font-size: 18px;
  font-weight: 700;
  color: #006d4b;
  line-height: 1.2;
}

.stats-mini-label {
  display: block;
  font-size: 10px;
  color: rgba(255, 255, 255, 0.4);
  margin-top: 2px;
}

/* --- Upcoming meetings --- */
.upcoming-count {
  font-size: 11px;
  font-weight: 600;
  color: #006d4b;
  background: rgba(0, 109, 75, 0.08);
  padding: 1px 8px;
  border-radius: 9999px;
  line-height: 1.5;
}

.upcoming-list {
  display: flex;
  flex-direction: column;
}

.upcoming-row {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 9px 14px;
  cursor: pointer;
  transition: background 0.15s ease;
}

.upcoming-row + .upcoming-row {
  border-top: 1px solid #f1f5f9;
}

.upcoming-row:hover {
  background: #f8fafc;
}

.upcoming-color {
  width: 3px;
  height: 28px;
  border-radius: 2px;
  flex-shrink: 0;
}

.upcoming-info {
  flex: 1;
  min-width: 0;
}

.upcoming-title {
  font-size: 12px;
  font-weight: 500;
  color: #1e293b;
  line-height: 1.3;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.upcoming-time {
  font-size: 11px;
  color: #94a3b8;
  margin-top: 1px;
}

.upcoming-empty {
  padding: 20px 14px;
  text-align: center;
  color: #94a3b8;
  font-size: 12px;
}

/* Calendar Wrapper */
.calendar-wrapper {
  padding: 0.5rem;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* LIST VIEW STYLES */
/* ═══════════════════════════════════════════════════════════════════════════ */

.list-view-container {
  padding: 1rem;
  min-height: 500px;
}

.meetings-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.meeting-list-card {
  position: relative;
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 14px 20px 14px 16px;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s ease;
  animation: cardSlideIn 0.35s ease forwards;
  animation-delay: var(--delay);
  opacity: 0;
  overflow: hidden;
}

[dir="rtl"] .meeting-list-card {
  padding: 14px 16px 14px 20px;
}

@keyframes cardSlideIn {
  from {
    opacity: 0;
    transform: translateX(20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

.meeting-list-card:hover {
  border-color: #006d4b;
  box-shadow: 0 4px 20px rgba(0, 109, 75, 0.12);
}

.card-accent {
  position: absolute;
  top: 0;
  inset-inline-start: 0;
  bottom: 0;
  width: 3px;
}

.card-content {
  flex: 1;
  min-width: 0;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  margin-bottom: 0.5rem;
}

.card-icon {
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 0.5rem;
  background: rgba(0, 109, 75, 0.1) !important;
  color: #006d4b !important;
}

.status-badge {
  padding: 3px 10px;
  border-radius: 20px;
  font-size: 11px;
  font-weight: 700;
  background: #006d4b !important;
}

.card-title {
  font-size: 0.95rem;
  font-weight: 600;
  color: #27272a;
  margin-bottom: 0.5rem;
  line-height: 1.4;
}

.card-meta {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex-wrap: wrap;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  font-size: 0.75rem;
  color: #71717a;
}

.card-arrow {
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.meeting-list-card:hover .card-arrow {
  background: #006d4b;
  color: #fff;
}

[dir="ltr"] .meeting-list-card:hover .card-arrow { transform: translateX(4px); }
[dir="rtl"] .meeting-list-card:hover .card-arrow { transform: translateX(-4px); }

/* ═══════════════════════════════════════════════════════════════════════════ */
/* TIMELINE VIEW STYLES */
/* ═══════════════════════════════════════════════════════════════════════════ */

.timeline-view-container {
  padding: 1rem;
  min-height: 500px;
}

.timeline-wrapper {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.timeline-group {
  display: grid;
  grid-template-columns: 80px 24px 1fr;
  gap: 0.75rem;
  animation: groupFadeIn 0.5s ease forwards;
  animation-delay: var(--group-delay);
  opacity: 0;
}

@keyframes groupFadeIn {
  from {
    opacity: 0;
    transform: translateY(15px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.timeline-date-marker {
  text-align: center;
}

.date-badge {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 0.625rem 0.5rem;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 0.75rem;
  transition: all 0.2s ease;
}

.date-badge.today {
  background: linear-gradient(135deg, #006d4b 0%, #004730 100%);
  border-color: #006d4b;
  box-shadow: 0 4px 12px rgba(0, 109, 75, 0.25);
}

.date-day {
  font-size: 1.5rem;
  font-weight: 700;
  color: #27272a;
  line-height: 1;
}

.date-badge.today .date-day {
  color: white;
}

.date-month {
  font-size: 0.65rem;
  font-weight: 600;
  color: #71717a;
  margin-top: 0.125rem;
}

.date-badge.today .date-month {
  color: rgba(255, 255, 255, 0.85);
}

.date-weekday {
  font-size: 0.6rem;
  color: #a1a1aa;
  margin-top: 0.25rem;
}

.date-badge.today .date-weekday {
  color: rgba(255, 255, 255, 0.7);
}

.today-badge {
  margin-top: 0.375rem;
  padding: 0.125rem 0.5rem;
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
  font-size: 0.6rem;
  font-weight: 600;
  border-radius: 9999px;
}

.timeline-track {
  display: flex;
  justify-content: center;
  padding-top: 0.5rem;
}

.track-line {
  width: 2px;
  height: 100%;
  background: linear-gradient(to bottom, #e2e8f0, transparent);
  border-radius: 1px;
}

.timeline-events {
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
  padding-bottom: 1rem;
}

.timeline-event-card {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 0.875rem;
  background: #fafafa;
  border: 1px solid #e4e4e7;
  border-radius: 0.75rem;
  cursor: pointer;
  transition: all 0.25s ease;
  animation: eventSlideIn 0.4s ease forwards;
  animation-delay: var(--item-delay);
  opacity: 0;
}

@keyframes eventSlideIn {
  from {
    opacity: 0;
    transform: translateX(15px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

.timeline-event-card:hover {
  background: #ffffff;
  border-color: #006d4b;
  box-shadow: 0 4px 16px rgba(0, 109, 75, 0.1);
  transform: translateX(-4px);
}

.event-time {
  font-size: 0.8rem;
  font-weight: 600;
  color: #3f3f46;
  width: 50px;
  flex-shrink: 0;
  padding-top: 0.125rem;
}

.event-connector {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.375rem;
  flex-shrink: 0;
  padding-top: 0.25rem;
}

.connector-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  border: 2px solid white;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.15);
}

.event-content {
  flex: 1;
  min-width: 0;
}

.event-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.75rem;
  margin-bottom: 0.375rem;
}

.event-title {
  font-size: 0.85rem;
  font-weight: 600;
  color: #27272a;
  line-height: 1.4;
}

.event-status {
  padding: 0.125rem 0.5rem;
  border-radius: 9999px;
  font-size: 0.6rem;
  font-weight: 600;
  white-space: nowrap;
  flex-shrink: 0;
}

.event-meta {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.event-tag {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.125rem 0.5rem;
  background: #e4e4e7;
  color: #71717a;
  border-radius: 9999px;
  font-size: 0.65rem;
}

.event-arrow {
  width: 1.75rem;
  height: 1.75rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
  transition: all 0.2s ease;
  flex-shrink: 0;
  align-self: center;
}

.timeline-event-card:hover .event-arrow {
  background: #006d4b;
  color: #fff;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* EMPTY STATE */
/* ═══════════════════════════════════════════════════════════════════════════ */

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  text-align: center;
}

.empty-icon {
  width: 5rem;
  height: 5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  background: #f4f4f5;
  color: #d4d4d8;
  margin-bottom: 1rem;
}

.empty-title {
  font-size: 1rem;
  font-weight: 600;
  color: #71717a;
  margin-bottom: 0.25rem;
}

.empty-subtitle {
  font-size: 0.875rem;
  color: #a1a1aa;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* FULLCALENDAR STYLES */
/* ═══════════════════════════════════════════════════════════════════════════ */

/* FullCalendar Base */
.calendar-wrapper :deep(.fc) {
  font-family: 'Tajawal', sans-serif;
}

/* Header Cells */
.calendar-wrapper :deep(.fc-col-header-cell) {
  padding: 0.5rem 0;
  background: #f8fafc;
  border-color: #e2e8f0;
}

.calendar-wrapper :deep(.fc-col-header-cell-cushion) {
  font-weight: 600;
  color: #475569;
  font-size: 0.75rem;
}

/* Day Cells */
.calendar-wrapper :deep(.fc-daygrid-day) {
  border-color: #e2e8f0;
}

.calendar-wrapper :deep(.fc-daygrid-day:hover) {
  background-color: #f8fafc;
}

.calendar-wrapper :deep(.fc-daygrid-day-frame) {
  padding: 0.25rem;
  min-height: 107px;
}

.calendar-wrapper :deep(.fc-daygrid-day-top) {
  padding: 0.25rem;
  width: 100%;
}

/* Hide FullCalendar's default day number */
.calendar-wrapper :deep(.fc-daygrid-day-top > a.fc-daygrid-day-number) {
  display: none !important;
}

.calendar-wrapper :deep(.day-numbers) {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.calendar-wrapper :deep(.gregorian-day) {
  font-weight: 600;
  color: #1e293b;
  font-size: 0.8rem;
  flex-shrink: 0;
}

.calendar-wrapper :deep(.hijri-day) {
  font-size: 0.6rem;
  color: #94a3b8;
  background: #f1f5f9;
  padding: 0.125rem 0.375rem;
  border-radius: 0.25rem;
  flex-shrink: 0;
}

/* Today - Reset all FullCalendar today backgrounds */
.calendar-wrapper :deep(.fc-day-today) {
  background-color: rgba(0, 109, 75, 0.04) !important;
}

.calendar-wrapper :deep(.fc-day-today .fc-daygrid-day-frame),
.calendar-wrapper :deep(.fc-day-today .fc-daygrid-day-top),
.calendar-wrapper :deep(.fc-day-today .fc-daygrid-day-number),
.calendar-wrapper :deep(.fc-day-today .day-numbers) {
  background: transparent !important;
  background-color: transparent !important;
}

.calendar-wrapper :deep(.fc-day-today .gregorian-day) {
  background-color: #006d4b !important;
  color: white !important;
  width: 28px;
  height: 28px;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  line-height: 1;
  font-size: 0.8rem;
}

.calendar-wrapper :deep(.fc-day-today .hijri-day) {
  background: rgba(0, 109, 75, 0.15);
  color: #006d4b;
}

/* Events */
.calendar-wrapper :deep(.fc-event) {
  border-radius: 0.25rem;
  padding: 0.125rem 0.25rem;
  font-size: 0.65rem;
  font-weight: 500;
  border: none;
  margin: 1px;
  cursor: pointer;
}

.calendar-wrapper :deep(.fc-event:hover) {
  filter: brightness(0.95);
}

.calendar-wrapper :deep(.fc-event-title) {
  font-weight: 500;
}

/* Time Grid */
.calendar-wrapper :deep(.fc-timegrid-slot) {
  height: 2rem;
  border-color: #e2e8f0;
}

.calendar-wrapper :deep(.fc-timegrid-slot-label) {
  font-size: 0.65rem;
  color: #94a3b8;
}

/* Weekend (Friday & Saturday for KSA) */
.calendar-wrapper :deep(.fc-day-sat),
.calendar-wrapper :deep(.fc-day-fri) {
  background-color: rgba(0, 0, 0, 0.015);
}

/* Other Month Days */
.calendar-wrapper :deep(.fc-day-other .gregorian-day) {
  color: #a1a1aa;
}

.calendar-wrapper :deep(.fc-day-other .hijri-day) {
  color: #a1a1aa;
  background: #f4f4f5;
}

/* More Events Link */
.calendar-wrapper :deep(.fc-more-link) {
  font-size: 0.65rem;
  font-weight: 600;
  color: #006d4b;
}

/* Scrollgrid */
.calendar-wrapper :deep(.fc-scrollgrid) {
  border: none;
}

.calendar-wrapper :deep(.fc-scrollgrid-section > td) {
  border: none;
}
</style>

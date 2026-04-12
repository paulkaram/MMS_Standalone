<template>
  <div class="home-page">
    <!-- Hero Section with themed background -->
    <div class="hero-section">
      <div class="hero-bg"></div>
      <div class="hero-decorations">
        <div class="deco-circle deco-1"></div>
        <div class="deco-circle deco-2"></div>
        <div class="deco-line"></div>
      </div>
      <div class="hero-content">
        <div>
          <h1 class="text-2xl font-bold text-white">
            {{ $t('Welcome') }}, {{ user?.fullName }}
          </h1>
          <p class="text-sm text-white/70 mt-1">
            {{ $t('HomeDescription') }}
          </p>
        </div>
      </div>

      <!-- Quick stats inside hero -->
      <div class="stats-container">
        <StatCard
          v-for="stat in stats"
          :key="stat.key"
          :title="stat.title"
          :value="stat.value"
          :icon="stat.icon"
          :color="stat.color"
          :change="stat.change"
        />
      </div>
    </div>

    <!-- Main content -->
    <div class="content-section">
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Meetings Section with View Toggle -->
        <div class="lg:col-span-2">
          <Card title="الاجتماعات القادمة">
            <template #headerActions>
              <div class="flex items-center gap-2">
                <!-- View Toggle -->
                <div class="view-toggle">
                  <button
                    :class="['view-toggle-btn', { active: activeView === 'calendar' }]"
                    @click="activeView = 'calendar'"
                    :title="'عرض التقويم'"
                  >
                    <Icon icon="mdi:calendar-month" class="w-4 h-4" />
                  </button>
                  <button
                    :class="['view-toggle-btn', { active: activeView === 'list' }]"
                    @click="activeView = 'list'"
                    :title="'عرض القائمة'"
                  >
                    <Icon icon="mdi:format-list-bulleted" class="w-4 h-4" />
                  </button>
                  <button
                    :class="['view-toggle-btn', { active: activeView === 'timeline' }]"
                    @click="activeView = 'timeline'"
                    :title="'عرض الجدول الزمني'"
                  >
                    <Icon icon="mdi:timeline-clock" class="w-4 h-4" />
                  </button>
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  @click="$router.push('/meetings')"
                >
                  عرض الكل
                </Button>
              </div>
            </template>

            <!-- Calendar View -->
            <Transition name="view-fade" mode="out-in">
              <div v-if="activeView === 'calendar'" key="calendar" class="calendar-view">
                <div class="calendar-header">
                  <button class="calendar-nav-btn" @click="prevMonth">
                    <Icon :icon="isRtl ? 'mdi:chevron-right' : 'mdi:chevron-left'" class="w-5 h-5" />
                  </button>
                  <h3 class="calendar-title">{{ currentMonthYear }}</h3>
                  <button class="calendar-nav-btn" @click="nextMonth">
                    <Icon :icon="isRtl ? 'mdi:chevron-left' : 'mdi:chevron-right'" class="w-5 h-5" />
                  </button>
                </div>

                <div class="calendar-grid">
                  <!-- Day headers -->
                  <div
                    v-for="day in weekDays"
                    :key="day"
                    class="calendar-day-header"
                  >
                    {{ day }}
                  </div>

                  <!-- Calendar days -->
                  <div
                    v-for="(day, index) in calendarDays"
                    :key="index"
                    :class="[
                      'calendar-day',
                      {
                        'other-month': !day.isCurrentMonth,
                        'today': day.isToday,
                        'has-meetings': day.meetings.length > 0
                      }
                    ]"
                    @click="day.meetings.length && selectDay(day)"
                  >
                    <span class="day-number">{{ day.day }}</span>
                    <div v-if="day.meetings.length > 0" class="meeting-dots">
                      <span
                        v-for="(meeting, mIndex) in day.meetings.slice(0, 3)"
                        :key="mIndex"
                        class="meeting-dot"
                        :style="{ backgroundColor: getMeetingColor(meeting.priority) }"
                      ></span>
                      <span v-if="day.meetings.length > 3" class="more-indicator">
                        +{{ day.meetings.length - 3 }}
                      </span>
                    </div>
                  </div>
                </div>

                <!-- Selected Day Meetings -->
                <Transition name="slide-up">
                  <div v-if="selectedDay && selectedDay.meetings.length > 0" class="selected-day-meetings">
                    <div class="selected-day-header">
                      <h4>{{ formatSelectedDate(selectedDay.date) }}</h4>
                      <button class="close-btn" @click="selectedDay = null">
                        <Icon icon="mdi:close" class="w-4 h-4" />
                      </button>
                    </div>
                    <div class="selected-meetings-list">
                      <div
                        v-for="meeting in selectedDay.meetings"
                        :key="meeting.id"
                        class="selected-meeting-item"
                        @click="$router.push(`/addMeeting/${meeting.id}`)"
                      >
                        <div class="meeting-time-badge">{{ meeting.time }}</div>
                        <div class="meeting-info">
                          <h5>{{ meeting.title }}</h5>
                          <p>{{ meeting.committee }}</p>
                        </div>
                        <Icon :icon="isRtl ? 'mdi:chevron-left' : 'mdi:chevron-right'" class="w-5 h-5 text-zinc-400" />
                      </div>
                    </div>
                  </div>
                </Transition>
              </div>

              <!-- List View -->
              <div v-else-if="activeView === 'list'" key="list" class="list-view">
                <div v-if="upcomingMeetings.length > 0" class="meetings-list">
                  <div
                    v-for="(meeting, index) in upcomingMeetings"
                    :key="meeting.id"
                    class="meeting-card"
                    :style="{ '--delay': `${index * 50}ms` }"
                    @click="$router.push(`/addMeeting/${meeting.id}`)"
                  >
                    <div class="meeting-card-accent" :style="{ backgroundColor: getMeetingColor(meeting.priority) }"></div>
                    <div class="meeting-card-content">
                      <div class="meeting-card-header">
                        <div class="meeting-icon-wrapper">
                          <Icon icon="mdi:calendar-clock" class="w-5 h-5" />
                        </div>
                        <div class="meeting-status-badge" :class="meeting.status || 'upcoming'">
                          {{ getStatusLabel(meeting.status) }}
                        </div>
                      </div>
                      <h4 class="meeting-card-title">{{ meeting.title }}</h4>
                      <p class="meeting-card-committee">{{ meeting.committee }}</p>
                      <div class="meeting-card-meta">
                        <div class="meta-item">
                          <Icon icon="mdi:calendar" class="w-4 h-4" />
                          <span>{{ meeting.date }}</span>
                        </div>
                        <div class="meta-item">
                          <Icon icon="mdi:clock-outline" class="w-4 h-4" />
                          <span>{{ meeting.time }}</span>
                        </div>
                        <div v-if="meeting.attendeesCount" class="meta-item">
                          <Icon icon="mdi:account-group" class="w-4 h-4" />
                          <span>{{ meeting.attendeesCount }} {{ $t('Attendees') }}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <div v-else class="empty-state">
                  <div class="empty-icon">
                    <Icon icon="mdi:calendar-blank" class="w-16 h-16" />
                  </div>
                  <p class="empty-title">{{ $t('NoUpcomingMeetings') }}</p>
                  <p class="empty-subtitle">{{ $t('MeetingScheduleWillAppearHere') }}</p>
                </div>
              </div>

              <!-- Timeline View -->
              <div v-else-if="activeView === 'timeline'" key="timeline" class="timeline-view">
                <div v-if="groupedMeetings.length > 0" class="timeline-container">
                  <div
                    v-for="(group, groupIndex) in groupedMeetings"
                    :key="group.date"
                    class="timeline-group"
                    :style="{ '--delay': `${groupIndex * 100}ms` }"
                  >
                    <div class="timeline-date-badge">
                      <div class="date-day">{{ group.dayNum }}</div>
                      <div class="date-month">{{ group.monthName }}</div>
                      <div v-if="group.isToday" class="today-indicator">اليوم</div>
                    </div>
                    <div class="timeline-line"></div>
                    <div class="timeline-meetings">
                      <div
                        v-for="(meeting, mIndex) in group.meetings"
                        :key="meeting.id"
                        class="timeline-meeting-card"
                        :style="{ '--item-delay': `${mIndex * 50}ms` }"
                        @click="$router.push(`/addMeeting/${meeting.id}`)"
                      >
                        <div class="timeline-time">{{ meeting.time }}</div>
                        <div class="timeline-connector">
                          <div class="connector-dot"></div>
                          <div class="connector-line"></div>
                        </div>
                        <div class="timeline-content">
                          <h5>{{ meeting.title }}</h5>
                          <p>{{ meeting.committee }}</p>
                          <div class="timeline-tags">
                            <span v-if="meeting.location" class="timeline-tag">
                              <Icon icon="mdi:map-marker" class="w-3 h-3" />
                              {{ meeting.location }}
                            </span>
                            <span v-if="meeting.attendeesCount" class="timeline-tag">
                              <Icon icon="mdi:account-group" class="w-3 h-3" />
                              {{ meeting.attendeesCount }}
                            </span>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <div v-else class="empty-state">
                  <div class="empty-icon">
                    <Icon icon="mdi:timeline-clock" class="w-16 h-16" />
                  </div>
                  <p class="empty-title">{{ $t('NoUpcomingMeetings') }}</p>
                  <p class="empty-subtitle">{{ $t('MeetingScheduleWillAppearHere') }}</p>
                </div>
              </div>
            </Transition>
          </Card>
        </div>

        <!-- Tasks -->
        <Card title="المهام المعلقة">
          <template #headerActions>
            <Button
              variant="ghost"
              size="sm"
              @click="$router.push('/tasks')"
            >
              عرض الكل
            </Button>
          </template>

          <div
            v-if="pendingTasks.length > 0"
            class="tasks-list"
          >
            <div
              v-for="(task, index) in pendingTasks"
              :key="task.id"
              class="task-item"
              :style="{ '--delay': `${index * 60}ms` }"
            >
              <div class="task-checkbox">
                <input
                  type="checkbox"
                  :id="`task-${task.id}`"
                  class="task-input"
                >
                <label :for="`task-${task.id}`" class="task-check-label">
                  <Icon icon="mdi:check" class="w-3 h-3" />
                </label>
              </div>
              <div class="task-content">
                <p class="task-title">{{ task.title }}</p>
                <p class="task-due">{{ task.dueDate }}</p>
              </div>
              <span :class="['task-priority', task.priority]">
                {{ task.priorityLabel }}
              </span>
            </div>
          </div>

          <div
            v-else
            class="empty-state compact"
          >
            <Icon
              icon="mdi:checkbox-marked-circle-outline"
              class="w-12 h-12 text-zinc-300"
            />
            <p class="empty-title">لا توجد مهام معلقة</p>
          </div>
        </Card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import { useUserStore } from '@/stores/user'
import Card from '@/components/ui/Card.vue'
import Button from '@/components/ui/Button.vue'
import StatCard from '@/components/app/dashboard/StatCard.vue'

// Store
const userStore = useUserStore()
const user = computed(() => userStore.loggedInUser)
const isRtl = computed(() => userStore.language === 'ar')

// View state
const activeView = ref<'calendar' | 'list' | 'timeline'>('calendar')
const currentDate = ref(new Date())
const selectedDay = ref<any>(null)

// Week days (Arabic - starting from Saturday for Arabic calendar)
const weekDays = ['السبت', 'الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة']

// Current month/year display
const currentMonthYear = computed(() => {
  const months = [
    'يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو',
    'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'
  ]
  return `${months[currentDate.value.getMonth()]} ${currentDate.value.getFullYear()}`
})

// Navigation
function prevMonth() {
  const newDate = new Date(currentDate.value)
  newDate.setMonth(newDate.getMonth() - 1)
  currentDate.value = newDate
  selectedDay.value = null
}

function nextMonth() {
  const newDate = new Date(currentDate.value)
  newDate.setMonth(newDate.getMonth() + 1)
  currentDate.value = newDate
  selectedDay.value = null
}

// Calendar days computation
const calendarDays = computed(() => {
  const year = currentDate.value.getFullYear()
  const month = currentDate.value.getMonth()

  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)

  // Adjust for Saturday start (Arabic calendar)
  let startDay = firstDay.getDay() + 1
  if (startDay === 7) startDay = 0

  const days: any[] = []
  const today = new Date()

  // Previous month days
  const prevMonthLastDay = new Date(year, month, 0).getDate()
  for (let i = startDay - 1; i >= 0; i--) {
    const day = prevMonthLastDay - i
    days.push({
      day,
      date: new Date(year, month - 1, day),
      isCurrentMonth: false,
      isToday: false,
      meetings: getMeetingsForDate(new Date(year, month - 1, day))
    })
  }

  // Current month days
  for (let i = 1; i <= lastDay.getDate(); i++) {
    const date = new Date(year, month, i)
    const isToday =
      date.getDate() === today.getDate() &&
      date.getMonth() === today.getMonth() &&
      date.getFullYear() === today.getFullYear()

    days.push({
      day: i,
      date,
      isCurrentMonth: true,
      isToday,
      meetings: getMeetingsForDate(date)
    })
  }

  // Next month days
  const remaining = 42 - days.length
  for (let i = 1; i <= remaining; i++) {
    days.push({
      day: i,
      date: new Date(year, month + 1, i),
      isCurrentMonth: false,
      isToday: false,
      meetings: getMeetingsForDate(new Date(year, month + 1, i))
    })
  }

  return days
})

function selectDay(day: any) {
  selectedDay.value = day
}

function formatSelectedDate(date: Date) {
  const options: Intl.DateTimeFormatOptions = {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }
  return date.toLocaleDateString('ar-EG', options)
}

// Get meetings for a specific date
function getMeetingsForDate(date: Date): any[] {
  // Mock implementation - in real app, filter from API data
  const dateStr = date.toISOString().split('T')[0]
  // Example: return meetings that match this date
  return upcomingMeetings.value.filter(m => {
    // Convert Arabic date to comparable format (mock)
    return false // Replace with actual date comparison
  })
}

// Meeting color by priority
function getMeetingColor(priority?: string) {
  const colors: Record<string, string> = {
    high: '#ef4444',
    medium: '#f59e0b',
    low: '#006d4b',
    default: '#006d4b'
  }
  return colors[priority || 'default']
}

// Status label
function getStatusLabel(status?: string) {
  const labels: Record<string, string> = {
    upcoming: 'قادم',
    today: 'اليوم',
    ongoing: 'جاري',
    completed: 'منتهي'
  }
  return labels[status || 'upcoming']
}

// Grouped meetings for timeline
const groupedMeetings = computed(() => {
  const groups: any[] = []
  const today = new Date()

  // Group meetings by date
  upcomingMeetings.value.forEach(meeting => {
    // Mock: create groups based on meeting dates
    // In real implementation, parse actual dates
  })

  // Mock grouped data
  return [
    {
      date: '2024-01-15',
      dayNum: '15',
      monthName: 'رمضان',
      isToday: true,
      meetings: upcomingMeetings.value.slice(0, 1)
    },
    {
      date: '2024-01-17',
      dayNum: '17',
      monthName: 'رمضان',
      isToday: false,
      meetings: upcomingMeetings.value.slice(1, 2)
    },
    {
      date: '2024-01-20',
      dayNum: '20',
      monthName: 'رمضان',
      isToday: false,
      meetings: upcomingMeetings.value.slice(2)
    }
  ]
})

// Stats
const stats = ref([
  {
    key: 'meetings',
    title: 'الاجتماعات هذا الشهر',
    value: '12',
    icon: 'mdi:calendar-check',
    color: 'primary',
    change: '+2 عن الشهر الماضي'
  },
  {
    key: 'tasks',
    title: 'المهام المعلقة',
    value: '8',
    icon: 'mdi:checkbox-marked-outline',
    color: 'warning',
    change: '3 مهام مستحقة اليوم'
  },
  {
    key: 'recommendations',
    title: 'التوصيات',
    value: '24',
    icon: 'mdi:lightbulb-outline',
    color: 'success',
    change: '+5 هذا الأسبوع'
  },
  {
    key: 'attendance',
    title: 'نسبة الحضور',
    value: '95%',
    icon: 'mdi:account-check',
    color: 'info',
    change: 'ممتاز'
  }
])

// Mock data - replace with actual API calls
const upcomingMeetings = ref([
  {
    id: 1,
    title: 'اجتماع مجلس الإدارة الدوري',
    committee: 'مجلس الإدارة',
    date: '15 رمضان 1446',
    time: '10:00 ص',
    priority: 'high',
    status: 'today',
    location: 'قاعة الاجتماعات الرئيسية',
    attendeesCount: 12
  },
  {
    id: 2,
    title: 'اجتماع لجنة المراجعة',
    committee: 'لجنة المراجعة',
    date: '17 رمضان 1446',
    time: '2:00 م',
    priority: 'medium',
    status: 'upcoming',
    location: 'قاعة 2',
    attendeesCount: 8
  },
  {
    id: 3,
    title: 'اجتماع لجنة الترشيحات',
    committee: 'لجنة الترشيحات والمكافآت',
    date: '20 رمضان 1446',
    time: '11:00 ص',
    priority: 'low',
    status: 'upcoming',
    location: 'قاعة 3',
    attendeesCount: 6
  }
])

const pendingTasks = ref([
  {
    id: 1,
    title: 'مراجعة محضر الاجتماع السابق',
    dueDate: 'مستحق اليوم',
    priority: 'high',
    priorityLabel: 'عاجل'
  },
  {
    id: 2,
    title: 'إعداد تقرير الربع الثالث',
    dueDate: 'مستحق غداً',
    priority: 'medium',
    priorityLabel: 'متوسط'
  },
  {
    id: 3,
    title: 'مراجعة طلبات العضوية الجديدة',
    dueDate: 'مستحق خلال 3 أيام',
    priority: 'low',
    priorityLabel: 'عادي'
  }
])
</script>

<style scoped>
.home-page {
  @apply space-y-6;
}

/* Hero Section */
.hero-section {
  @apply relative rounded-2xl overflow-hidden;
  padding: 2rem;
  margin: -1.5rem -1.5rem 0 -1.5rem;
}

.hero-bg {
  @apply absolute inset-0;
  background: linear-gradient(135deg, #006d4b 0%, #006d4b 50%, #006d4b 200%);
}

.hero-decorations {
  @apply absolute inset-0 overflow-hidden pointer-events-none;
}

.deco-circle {
  @apply absolute rounded-full;
  background: rgba(255, 255, 255, 0.05);
}

.deco-1 {
  width: 300px;
  height: 300px;
  top: -100px;
  right: -50px;
  filter: blur(40px);
}

.deco-2 {
  width: 200px;
  height: 200px;
  bottom: -80px;
  left: 10%;
  filter: blur(30px);
}

.deco-line {
  @apply absolute;
  top: 0;
  left: 20%;
  right: 20%;
  height: 1px;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
}

.hero-content {
  @apply relative flex items-center gap-4 mb-6;
}


.stats-container {
  @apply relative grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4;
}

/* Content Section */
.content-section {
  @apply relative;
  padding-top: 1rem;
}

/* View Toggle */
.view-toggle {
  @apply flex items-center gap-1 p-1 rounded-lg;
  background: #f4f4f5;
}

.view-toggle-btn {
  @apply flex items-center justify-center w-8 h-8 rounded-md transition-all duration-200;
  color: #a1a1aa;
}

.view-toggle-btn:hover {
  color: #71717a;
  background: rgba(0, 0, 0, 0.05);
}

.view-toggle-btn.active {
  background: #ffffff;
  color: #006d4b;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

/* View Transitions */
.view-fade-enter-active,
.view-fade-leave-active {
  transition: all 0.25s ease;
}

.view-fade-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.view-fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* Calendar View */
.calendar-view {
  @apply space-y-4;
}

.calendar-header {
  @apply flex items-center justify-between px-2;
}

.calendar-nav-btn {
  @apply w-8 h-8 flex items-center justify-center rounded-lg transition-all duration-200;
  color: #71717a;
}

.calendar-nav-btn:hover {
  background: #f4f4f5;
  color: #3f3f46;
}

.calendar-title {
  @apply text-lg font-bold text-zinc-900;
}

.calendar-grid {
  @apply grid grid-cols-7 gap-1;
}

.calendar-day-header {
  @apply text-xs font-semibold text-zinc-500 text-center py-2;
}

.calendar-day {
  @apply relative flex flex-col items-center justify-start p-2 rounded-lg transition-all duration-200 cursor-default;
  min-height: 72px;
}

.calendar-day:hover {
  background: #fafafa;
}

.calendar-day.other-month {
  opacity: 0.4;
}

.calendar-day.today {
  background: rgba(0, 109, 75, 0.08);
}

.calendar-day.today .day-number {
  @apply w-7 h-7 flex items-center justify-center rounded-full;
  background: #006d4b;
  color: white;
  font-weight: 700;
}

.calendar-day.has-meetings {
  cursor: pointer;
}

.calendar-day.has-meetings:hover {
  background: rgba(0, 109, 75, 0.12);
  transform: scale(1.02);
}

.day-number {
  @apply text-sm font-medium text-zinc-700;
}

.meeting-dots {
  @apply flex items-center gap-1 mt-1;
}

.meeting-dot {
  @apply w-1.5 h-1.5 rounded-full;
}

.more-indicator {
  @apply text-xs text-zinc-500 font-medium;
}

/* Selected Day Popup */
.selected-day-meetings {
  @apply mt-4 p-4 rounded-xl;
  background: #fafafa;
  border: 1px solid #e4e4e7;
}

.selected-day-header {
  @apply flex items-center justify-between mb-3;
}

.selected-day-header h4 {
  @apply text-sm font-semibold text-zinc-900;
}

.close-btn {
  @apply w-6 h-6 flex items-center justify-center rounded-full transition-colors;
  color: #a1a1aa;
}

.close-btn:hover {
  background: #e4e4e7;
  color: #71717a;
}

.selected-meetings-list {
  @apply space-y-2;
}

.selected-meeting-item {
  @apply flex items-center gap-3 p-3 rounded-lg bg-white cursor-pointer transition-all duration-200;
  border: 1px solid #e4e4e7;
}

.selected-meeting-item:hover {
  border-color: #006d4b;
  box-shadow: 0 2px 8px rgba(0, 109, 75, 0.15);
}

.meeting-time-badge {
  @apply px-2 py-1 rounded-md text-xs font-semibold;
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
}

.meeting-info {
  @apply flex-1;
}

.meeting-info h5 {
  @apply text-sm font-medium text-zinc-900;
}

.meeting-info p {
  @apply text-xs text-zinc-500;
}

/* Slide Up Transition */
.slide-up-enter-active,
.slide-up-leave-active {
  transition: all 0.3s ease;
}

.slide-up-enter-from,
.slide-up-leave-to {
  opacity: 0;
  transform: translateY(10px);
}

/* List View */
.list-view {
  @apply space-y-0;
}

/* ── List View — MMS Theme ── */
.meetings-list { display: flex; flex-direction: column; gap: 12px; }

.meeting-card {
  position: relative;
  display: flex; align-items: stretch;
  background: #fff; border: 1px solid #e2e8f0; border-radius: 12px;
  cursor: pointer; transition: all 0.2s ease; overflow: hidden;
  animation: mc-slideIn 0.35s ease forwards;
  animation-delay: var(--delay); opacity: 0;
}

@keyframes mc-slideIn {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}

.meeting-card:hover {
  border-color: #006d4b;
  box-shadow: 0 4px 16px rgba(0, 109, 75, 0.12);
}

.meeting-card-accent {
  position: absolute; top: 0; bottom: 0;
  inset-inline-start: 0; width: 4px;
}

.meeting-card-content { flex: 1; padding: 16px 20px; }

.meeting-card-header {
  display: flex; align-items: center; gap: 10px; margin-bottom: 10px;
}

.meeting-icon-wrapper {
  width: 32px; height: 32px; display: flex; align-items: center; justify-content: center;
  border-radius: 8px; background: rgba(0, 109, 75, 0.1); color: #006d4b; flex-shrink: 0;
}

.meeting-status-badge {
  padding: 3px 10px; border-radius: 20px; font-size: 11px; font-weight: 700;
}
.meeting-status-badge.upcoming { background: #006d4b; color: #60a5fa; }
.meeting-status-badge.today { background: rgba(0, 109, 75, 0.1); color: #006d4b; }
.meeting-status-badge.ongoing { background: #006d4b; color: #fbbf24; }
.meeting-status-badge.completed, .meeting-status-badge.signed, .meeting-status-badge.finished {
  background: #006d4b; color: #4ade80;
}

.meeting-card-title { font-size: 15px; font-weight: 600; color: #0f172a; margin: 0 0 4px; }
.meeting-card-committee { font-size: 13px; color: #94a3b8; margin: 0 0 10px; }

.meeting-card-meta { display: flex; align-items: center; gap: 16px; flex-wrap: wrap; }
.meta-item { display: flex; align-items: center; gap: 5px; font-size: 12px; color: #64748b; }

/* Timeline View */
.timeline-view {
  @apply py-2;
}

.timeline-container {
  @apply space-y-6;
}

.timeline-group {
  @apply flex gap-4;
  animation: fadeInUp 0.5s ease forwards;
  animation-delay: var(--delay);
  opacity: 0;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.timeline-date-badge {
  @apply flex-shrink-0 w-16 text-center;
}

.date-day {
  @apply text-2xl font-bold text-zinc-900;
}

.date-month {
  @apply text-xs text-zinc-500 font-medium;
}

.today-indicator {
  @apply mt-1 px-2 py-0.5 rounded-full text-xs font-semibold;
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
}

.timeline-line {
  @apply w-px flex-shrink-0;
  background: linear-gradient(to bottom, #e4e4e7, transparent);
}

.timeline-meetings {
  @apply flex-1 space-y-3 pb-6;
}

.timeline-meeting-card {
  display: flex; gap: 12px; padding: 14px 16px;
  border-radius: 12px; cursor: pointer; transition: all 0.2s ease;
  background: #fff; border: 1px solid #e2e8f0;
  animation: tl-slideIn 0.35s ease forwards;
  animation-delay: var(--item-delay); opacity: 0;
}

@keyframes tl-slideIn {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}

.timeline-meeting-card:hover {
  background: #fff;
  border-color: #006d4b;
  box-shadow: 0 4px 12px rgba(0, 109, 75, 0.1);
}

.timeline-time {
  @apply text-sm font-semibold text-zinc-700 w-16 flex-shrink-0;
}

.timeline-connector {
  @apply flex flex-col items-center gap-1 flex-shrink-0;
}

.connector-dot {
  @apply w-3 h-3 rounded-full border-2 border-white;
  background: #006d4b;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.2);
}

.connector-line {
  @apply flex-1 w-px;
  background: #e4e4e7;
}

.timeline-content {
  @apply flex-1;
}

.timeline-content h5 {
  @apply text-sm font-semibold text-zinc-900 mb-0.5;
}

.timeline-content p {
  @apply text-xs text-zinc-500 mb-2;
}

.timeline-tags {
  @apply flex items-center gap-2 flex-wrap;
}

.timeline-tag {
  @apply flex items-center gap-1 px-2 py-0.5 rounded-full text-xs;
  background: #e4e4e7;
  color: #71717a;
}

/* Empty State */
.empty-state {
  @apply flex flex-col items-center justify-center py-12 text-center;
}

.empty-state.compact {
  @apply py-8;
}

.empty-icon {
  @apply w-20 h-20 flex items-center justify-center rounded-full mb-4;
  background: #f4f4f5;
  color: #d4d4d8;
}

.empty-title {
  @apply text-base font-medium text-zinc-600 mb-1;
}

.empty-subtitle {
  @apply text-sm text-zinc-400;
}

/* Tasks List */
.tasks-list {
  @apply space-y-2;
}

.task-item {
  @apply flex items-start gap-3 p-3 rounded-lg transition-all duration-200;
  background: #fafafa;
  animation: taskSlide 0.4s ease forwards;
  animation-delay: var(--delay);
  opacity: 0;
}

@keyframes taskSlide {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.task-item:hover {
  background: #f4f4f5;
}

.task-checkbox {
  @apply relative flex-shrink-0;
}

.task-input {
  @apply sr-only;
}

.task-check-label {
  @apply w-5 h-5 flex items-center justify-center rounded border-2 border-zinc-300 cursor-pointer transition-all duration-200;
  color: transparent;
}

.task-input:checked + .task-check-label {
  background: #006d4b;
  border-color: #006d4b;
  color: white;
}

.task-input:focus + .task-check-label {
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.2);
}

.task-content {
  @apply flex-1 min-w-0;
}

.task-title {
  @apply text-sm text-zinc-900 font-medium;
}

.task-due {
  @apply text-xs text-zinc-500 mt-0.5;
}

.task-priority {
  @apply px-2 py-0.5 rounded-full text-xs font-medium flex-shrink-0;
}

.task-priority.high {
  background: rgba(239, 68, 68, 0.1);
  color: #ef4444;
}

.task-priority.medium {
  background: rgba(245, 158, 11, 0.1);
  color: #f59e0b;
}

.task-priority.low {
  background: rgba(59, 130, 246, 0.1);
  color: #3b82f6;
}
</style>

<template>
  <div class="admin-dashboard">
    <!-- Page Header -->
    <PageHeader
      :title="$t('Dashboard')"
      :subtitle="$t('DashboardOverview')"
    >
      <template #actions>
        <button class="btn-clean secondary" @click="fetchDashboardData">
          <Icon icon="mdi:refresh" class="w-[18px] h-[18px]" />
          {{ $t('Refresh') }}
        </button>
      </template>
    </PageHeader>

    <!-- Key Metrics Row — inline dark stat cards (matching IAM/Case Portal exactly) -->
    <div class="key-metrics">
      <div class="dark-stat-card">
        <svg class="dark-stat-wave" viewBox="0 0 200 80" preserveAspectRatio="none">
          <path d="M0 45 C40 25, 80 55, 120 40 S200 50 200 50 L200 80 L0 80Z" fill="rgba(0,174,140,0.18)" />
          <path d="M0 55 C50 40, 90 65, 140 50 S200 60 200 60 L200 80 L0 80Z" fill="rgba(0,174,140,0.10)" />
        </svg>
        <div class="dark-stat-top">
          <Icon icon="mdi:account-group" class="w-5 h-5 dark-stat-icon" />
          <span class="dark-stat-label">{{ $t('Councils') }}</span>
        </div>
        <div class="dark-stat-bottom">
          <span class="dark-stat-value">{{ councilsCount }}</span>
          <span class="dark-stat-trend trend-positive">{{ committeesCount }} {{ $t('Committees') }}</span>
        </div>
      </div>

      <div class="dark-stat-card">
        <svg class="dark-stat-wave" viewBox="0 0 200 80" preserveAspectRatio="none">
          <path d="M0 45 C40 25, 80 55, 120 40 S200 50 200 50 L200 80 L0 80Z" fill="rgba(0,174,140,0.18)" />
          <path d="M0 55 C50 40, 90 65, 140 50 S200 60 200 60 L200 80 L0 80Z" fill="rgba(0,174,140,0.10)" />
        </svg>
        <div class="dark-stat-top">
          <Icon icon="mdi:calendar-check" class="w-5 h-5 dark-stat-icon" />
          <span class="dark-stat-label">{{ $t('TotalMeetings') }}</span>
        </div>
        <div class="dark-stat-bottom">
          <span class="dark-stat-value">{{ dashboardStats?.totalMeetings || 0 }}</span>
          <span class="dark-stat-trend trend-positive">{{ dashboardStats?.upcomingMeetings || 0 }} {{ $t('Upcoming') }}</span>
        </div>
      </div>

      <div class="dark-stat-card">
        <svg class="dark-stat-wave" viewBox="0 0 200 80" preserveAspectRatio="none">
          <path d="M0 45 C40 25, 80 55, 120 40 S200 50 200 50 L200 80 L0 80Z" fill="rgba(0,174,140,0.18)" />
          <path d="M0 55 C50 40, 90 65, 140 50 S200 60 200 60 L200 80 L0 80Z" fill="rgba(0,174,140,0.10)" />
        </svg>
        <div class="dark-stat-top">
          <Icon icon="mdi:account-check" class="w-5 h-5 dark-stat-icon" />
          <span class="dark-stat-label">{{ $t('ActiveUsers') }}</span>
        </div>
        <div class="dark-stat-bottom">
          <span class="dark-stat-value">{{ activeUsersCount }}</span>
          <span class="dark-stat-trend trend-positive">{{ totalUsersCount }} {{ $t('TotalUsers') }}</span>
        </div>
      </div>

      <div class="dark-stat-card" :class="{ 'stat-alert': (dashboardStats?.overdueTask || 0) > 0 }">
        <svg class="dark-stat-wave" viewBox="0 0 200 80" preserveAspectRatio="none">
          <path d="M0 45 C40 25, 80 55, 120 40 S200 50 200 50 L200 80 L0 80Z" :fill="(dashboardStats?.overdueTask || 0) > 0 ? 'rgba(239,68,68,0.18)' : 'rgba(0,174,140,0.18)'" />
          <path d="M0 55 C50 40, 90 65, 140 50 S200 60 200 60 L200 80 L0 80Z" :fill="(dashboardStats?.overdueTask || 0) > 0 ? 'rgba(239,68,68,0.10)' : 'rgba(0,174,140,0.10)'" />
        </svg>
        <div class="dark-stat-top">
          <Icon icon="mdi:clipboard-check-outline" class="w-5 h-5 dark-stat-icon" />
          <span class="dark-stat-label">{{ $t('PendingTasks') }}</span>
        </div>
        <div class="dark-stat-bottom">
          <span class="dark-stat-value">{{ dashboardStats?.pendingTasks || 0 }}</span>
          <span :class="['dark-stat-trend', (dashboardStats?.overdueTask || 0) > 0 ? 'trend-negative' : 'trend-positive']">{{ dashboardStats?.overdueTask || 0 }} {{ $t('Overdue') }}</span>
        </div>
      </div>
    </div>

    <!-- Secondary Stats Row (Case Portal style with accent border-top) -->
    <section class="stats-section">
      <div class="metric-card" style="--accent: #006d4b;">
        <div class="metric-card-content">
          <div class="metric-header">
            <div class="metric-icon" style="background: #d4f5ed; color: #006d4b;">
              <Icon icon="mdi:calendar-month" class="w-5 h-5" />
            </div>
            <div class="metric-label">{{ $t('TotalMeetings') }}</div>
          </div>
          <div class="metric-main">
            <div class="metric-value">{{ dashboardStats?.totalMeetings || 0 }}</div>
          </div>
        </div>
      </div>

      <div class="metric-card" style="--accent: #068275;">
        <div class="metric-card-content">
          <div class="metric-header">
            <div class="metric-icon" style="background: #d0e8e4; color: #068275;">
              <Icon icon="mdi:checkbox-marked-outline" class="w-5 h-5" />
            </div>
            <div class="metric-label">{{ $t('PendingApprovals') }}</div>
          </div>
          <div class="metric-main">
            <div class="metric-value">{{ dashboardStats?.upcomingMeetings || 0 }}</div>
          </div>
        </div>
      </div>

      <div class="metric-card" style="--accent: #007E65;">
        <div class="metric-card-content">
          <div class="metric-header">
            <div class="metric-icon" style="background: #e0eeec; color: #007E65;">
              <Icon icon="mdi:file-document-outline" class="w-5 h-5" />
            </div>
            <div class="metric-label">{{ $t('InitialMOM') }}</div>
          </div>
          <div class="metric-main">
            <div class="metric-value">{{ momInitialCount }}</div>
          </div>
        </div>
      </div>

      <div class="metric-card" style="--accent: #004730;">
        <div class="metric-card-content">
          <div class="metric-header">
            <div class="metric-icon" style="background: #dce4ed; color: #004730;">
              <Icon icon="mdi:file-check-outline" class="w-5 h-5" />
            </div>
            <div class="metric-label">{{ $t('FinalMOM') }}</div>
          </div>
          <div class="metric-main">
            <div class="metric-value">{{ momFinalCount }}</div>
          </div>
        </div>
      </div>

      <div class="metric-card" style="--accent: #068275;">
        <div class="metric-card-content">
          <div class="metric-header">
            <div class="metric-icon" style="background: #d0e8e4; color: #068275;">
              <Icon icon="mdi:check-circle-outline" class="w-5 h-5" />
            </div>
            <div class="metric-label">{{ $t('PendingTasks') }}</div>
          </div>
          <div class="metric-main">
            <div class="metric-value">{{ dashboardStats?.pendingTasks || 0 }}</div>
          </div>
        </div>
      </div>

      <div class="metric-card" style="--accent: #0d3a4d;">
        <div class="metric-card-content">
          <div class="metric-header">
            <div class="metric-icon" style="background: #d8e3ed; color: #0d3a4d;">
              <Icon icon="mdi:alert-outline" class="w-5 h-5" />
            </div>
            <div class="metric-label">{{ $t('OverdueTasks') }}</div>
          </div>
          <div class="metric-main">
            <div class="metric-value">{{ dashboardStats?.overdueTask || 0 }}</div>
          </div>
        </div>
      </div>
    </section>

    <!-- Charts Section -->
    <section class="charts-section">
      <!-- Meetings by Status Chart -->
      <div class="db-card">
        <div class="db-card-header">
          <div class="db-card-title-group">
            <h3 class="db-card-title">{{ $t('MeetingsByStatus') }}</h3>
          </div>
        </div>
        <div class="db-card-body">
          <BarChart
            v-if="hasBarChartData"
            :categories="meetingsChartData.categories"
            :series="meetingsChartData.series"
            :colors="meetingBarColors"
            height="300"
            :show-legend="true"
            :distributed="true"
          />
          <div v-else class="db-card-empty">
            <Icon icon="mdi:chart-bar" class="w-8 h-8" />
            <span>{{ $t('NoData') }}</span>
          </div>
        </div>
      </div>

      <!-- Recommendations Progress Chart -->
      <div class="db-card">
        <div class="db-card-header">
          <div class="db-card-title-group">
            <h3 class="db-card-title">{{ $t('RecommendationsProgress') }}</h3>
          </div>
        </div>
        <div class="db-card-body centered">
          <PieChart
            v-if="hasRecommendationsData"
            :labels="recommendationsLabels"
            :series="recommendationsSeries"
            :colors="['#006d4b', '#F59E0B']"
            height="240"
            :donut="true"
            legend-position="bottom"
          />
          <div v-else class="db-card-empty">
            <Icon icon="mdi:lightbulb-outline" class="w-8 h-8" />
            <span>{{ $t('NoData') }}</span>
          </div>
        </div>
      </div>
    </section>

    <!-- Meetings Lists Section -->
    <section class="lists-section">
      <!-- Today's Meetings -->
      <div class="db-card">
        <div class="db-card-header">
          <div class="db-card-title-group">
            <h3 class="db-card-title">{{ $t('TodaysMeetings') }}</h3>
          </div>
          <span v-if="todayMeetings.length > 0" class="db-card-badge">{{ todayMeetings.length }}</span>
        </div>
        <div class="db-card-body list-body">
          <div v-if="loadingMeetings" class="db-card-loading">
            <div class="skeleton-row" v-for="i in 3" :key="i"></div>
          </div>
          <div v-else-if="todayMeetings.length === 0" class="db-card-empty">
            <Icon icon="mdi:calendar-blank" class="w-10 h-10" />
            <span>{{ $t('NoMeetingsToday') }}</span>
            <span class="empty-hint">{{ $t('EnjoyYourDay') }}</span>
          </div>
          <div v-else class="meeting-list">
            <div
              v-for="meeting in todayMeetings"
              :key="meeting.id"
              :class="['meeting-row', { 'is-live': meeting.isNow }]"
              @click="goToMeeting(meeting.id)"
            >
              <div class="meeting-time-col">
                <span class="meeting-time-text">{{ meeting.time }}</span>
                <div v-if="meeting.isNow" class="live-dot-wrap">
                  <span class="live-dot"></span>
                  <span class="live-label">{{ $t('Live') }}</span>
                </div>
              </div>
              <div class="meeting-info-col">
                <h4 class="meeting-name">{{ meeting.title }}</h4>
                <div class="meeting-meta-row">
                  <span v-if="meeting.location" class="meta-tag">
                    <Icon icon="mdi:map-marker" class="w-3.5 h-3.5" />
                    {{ meeting.location }}
                  </span>
                  <span v-if="meeting.councilName" class="meta-tag">
                    <Icon icon="mdi:account-group" class="w-3.5 h-3.5" />
                    {{ meeting.councilName }}
                  </span>
                </div>
              </div>
              <Icon :icon="isRtl ? 'mdi:chevron-left' : 'mdi:chevron-right'" class="w-5 h-5 meeting-arrow" />
            </div>
          </div>
        </div>
      </div>

      <!-- Upcoming Meetings -->
      <div class="db-card">
        <div class="db-card-header">
          <div class="db-card-title-group">
            <h3 class="db-card-title">{{ $t('UpcomingMeetings') }}</h3>
          </div>
          <router-link to="/meetings" class="db-card-link">
            {{ $t('ViewAll') }}
            <Icon :icon="isRtl ? 'mdi:chevron-left' : 'mdi:chevron-right'" class="w-4 h-4" />
          </router-link>
        </div>
        <div class="db-card-body list-body">
          <div v-if="loadingMeetings" class="db-card-loading">
            <div class="skeleton-row" v-for="i in 4" :key="i"></div>
          </div>
          <div v-else-if="upcomingMeetings.length === 0" class="db-card-empty">
            <Icon icon="mdi:calendar-blank" class="w-10 h-10" />
            <span>{{ $t('NoUpcomingMeetings') }}</span>
            <router-link to="/addMeeting" class="empty-action-btn">{{ $t('ScheduleMeeting') }}</router-link>
          </div>
          <div v-else class="meeting-list">
            <div
              v-for="meeting in upcomingMeetings.slice(0, 5)"
              :key="meeting.id"
              class="meeting-row"
              @click="goToMeeting(meeting.id)"
            >
              <div class="date-box">
                <span class="date-day">{{ getDayNumber(meeting.date) }}</span>
                <span class="date-month">{{ getMonthName(meeting.date) }}</span>
              </div>
              <div class="meeting-info-col">
                <h4 class="meeting-name">{{ meeting.title }}</h4>
                <div class="meeting-meta-row">
                  <span class="meta-tag">
                    <Icon icon="mdi:clock-outline" class="w-3.5 h-3.5" />
                    {{ meeting.time }}
                  </span>
                  <span v-if="meeting.councilName" class="meta-tag">
                    <Icon icon="mdi:account-group" class="w-3.5 h-3.5" />
                    {{ meeting.councilName }}
                  </span>
                </div>
              </div>
              <Icon :icon="isRtl ? 'mdi:chevron-left' : 'mdi:chevron-right'" class="w-5 h-5 meeting-arrow" />
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Recent Activity -->
    <section class="activity-section">
      <div class="db-card">
        <div class="db-card-header">
          <div class="db-card-title-group">
            <h3 class="db-card-title">{{ $t('RecentActivity') }}</h3>
          </div>
          <span v-if="recentActivities.length > 0" class="db-card-badge">{{ recentActivities.length }}</span>
        </div>
        <div class="db-card-body">
          <div v-if="loadingActivities" class="db-card-loading">
            <div class="skeleton-row" v-for="i in 5" :key="i"></div>
          </div>
          <div v-else-if="recentActivities.length === 0" class="db-card-empty">
            <Icon icon="mdi:history" class="w-10 h-10" />
            <span>{{ $t('NoRecentActivity') }}</span>
            <span class="empty-hint">{{ $t('ActivityWillAppear') }}</span>
          </div>
          <div v-else class="activity-feed">
            <div
              v-for="activity in recentActivities.slice(0, 8)"
              :key="activity.id"
              class="activity-row"
            >
              <div class="activity-dot" :class="activity.type"></div>
              <div class="activity-info">
                <p class="activity-text">{{ activity.title }}</p>
                <span class="activity-time">{{ formatActivityTime(activity.timestamp) }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/components/layout/PageHeader.vue'
import Icon from '@/components/ui/Icon.vue'
import BarChart from '@/components/ui/Charts/BarChart.vue'
import PieChart from '@/components/ui/Charts/PieChart.vue'
import DashboardService from '@/services/DashboardService'
import { useUserStore } from '@/stores/user'
import type { DashboardStats, UpcomingMeeting, RecentActivity } from '@/services/DashboardService'

const { t } = useI18n()
const router = useRouter()
const userStore = useUserStore()

// User name and initials
const userName = computed(() => userStore.user?.fullName || t('Admin'))
// State
const loading = ref(true)
const loadingMeetings = ref(true)
const loadingActivities = ref(true)
const currentTime = ref(new Date())
const dashboardStats = ref<DashboardStats | null>(null)
const upcomingMeetings = ref<UpcomingMeeting[]>([])
const todayMeetings = ref<any[]>([])
const recentActivities = ref<RecentActivity[]>([])
const meetingChartData = ref<{ labels: string[], series: { name: string, data: number[] }[] }>({ labels: [], series: [] })

// Councils & Committees
const councilsCount = ref(0)
const committeesCount = ref(0)

// Users
const activeUsersCount = ref(0)
const totalUsersCount = ref(0)

// MOM counts
const momInitialCount = ref(0)
const momFinalCount = ref(0)

// Recommendations
const recommendationsSeries = ref<number[]>([])
const recommendationsLabels = ref<string[]>([])

// Timer
let clockInterval: number | null = null

// Computed
const greetingLabel = computed(() => {
  const hour = currentTime.value.getHours()
  if (hour < 12) return t('GoodMorning')
  if (hour < 17) return t('GoodAfternoon')
  return t('GoodEvening')
})

const isRtl = computed(() => userStore.isRtl)
const dateLocale = computed(() => isRtl.value ? 'ar-EG' : 'en-US')

const todayFormatted = computed(() => {
  return currentTime.value.toLocaleDateString(dateLocale.value, {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
})

const currentDayName = computed(() => {
  return currentTime.value.toLocaleDateString(dateLocale.value, {
    weekday: 'long',
    day: 'numeric',
    month: 'long'
  })
})

// Chart colors - emerald theme palette
const meetingBarColors = [
  '#006d4b', '#0d7377', '#14919b', '#1a6b4f', '#2d9d76',
  '#3d8b6e', '#5fa37f', '#45c994', '#84c69b', '#003423'
]

// Chart Data
const meetingsChartData = computed(() => {
  const data = meetingChartData.value
  const labels = data?.labels || data?.Labels || []
  const series = data?.series || data?.Series || []
  const seriesData = series?.[0]?.data || series?.[0]?.Data || []
  return {
    categories: labels,
    series: [{ name: t('Meetings'), data: seriesData }]
  }
})

const hasBarChartData = computed(() => {
  const chartData = meetingsChartData.value
  return chartData.categories.length > 0 &&
         chartData.series.length > 0 &&
         chartData.series[0]?.data?.length > 0
})

const hasRecommendationsData = computed(() => {
  return recommendationsSeries.value.some(v => v > 0)
})

// Methods
const getDayNumber = (dateString: string) => new Date(dateString).getDate()

const getMonthName = (dateString: string) => {
  return new Date(dateString).toLocaleDateString(dateLocale.value, { month: 'short' })
}

const formatActivityTime = (timestamp: string) => {
  const date = new Date(timestamp)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMins / 60)
  const diffDays = Math.floor(diffHours / 24)

  if (diffMins < 1) return t('JustNow')
  if (diffMins < 60) return `${diffMins} ${t('MinutesShort')}`
  if (diffHours < 24) return `${diffHours} ${t('HoursShort')}`
  if (diffDays < 7) return `${diffDays} ${t('DaysShort')}`

  return date.toLocaleDateString(dateLocale.value, { month: 'short', day: 'numeric' })
}

const goToMeeting = (meetingId: number) => {
  router.push(`/meetingRoom/${meetingId}`)
}

const isCurrentMeeting = (meeting: UpcomingMeeting) => {
  const now = new Date()
  const meetingDate = new Date(meeting.date)
  if (meetingDate.toDateString() !== now.toDateString()) return false
  const [hours, minutes] = (meeting.time || '00:00').split(':').map(Number)
  const meetingTime = new Date(meetingDate)
  meetingTime.setHours(hours, minutes, 0, 0)
  const meetingEnd = new Date(meetingTime)
  meetingEnd.setHours(meetingEnd.getHours() + 1)
  return now >= meetingTime && now <= meetingEnd
}

// Data Fetching
const fetchDashboardData = async () => {
  loading.value = true
  try {
    const [stats, chartData, councilsData, usersData, momData, recommendationsData] = await Promise.all([
      DashboardService.getStats(),
      DashboardService.getMeetingsCount(),
      DashboardService.getCouncilsCommitteesCount(),
      DashboardService.getUsersCount(),
      DashboardService.getMeetingMinutesCount(),
      DashboardService.getRecommendationsCount()
    ])

    dashboardStats.value = stats
    meetingChartData.value = chartData || { labels: [], series: [] }

    // Councils & Committees - values array: [councils, committees]
    const ccValues = councilsData?.values || councilsData?.Values || []
    councilsCount.value = ccValues[0] || 0
    committeesCount.value = ccValues[1] || 0

    // Users - values array: [active, inactive]
    const userValues = usersData?.values || usersData?.Values || []
    activeUsersCount.value = userValues[0] || 0
    totalUsersCount.value = (userValues[0] || 0) + (userValues[1] || 0)

    // MOM - values array: [initial, final]
    const momValues = momData?.values || momData?.Values || []
    momInitialCount.value = momValues[0] || 0
    momFinalCount.value = momValues[1] || 0

    // Recommendations - values array: [complete%, incomplete%]
    const recValues = recommendationsData?.values || recommendationsData?.Values || []
    recommendationsSeries.value = recValues.length ? recValues : [0, 0]
    recommendationsLabels.value = [
      t('Completed'),
      t('InProgress')
    ]

  } catch (error) {
    console.error('Failed to fetch dashboard data:', error)
  } finally {
    loading.value = false
  }
}

const fetchMeetings = async () => {
  loadingMeetings.value = true
  try {
    const meetings = await DashboardService.getUpcomingMeetings(15)
    const today = new Date().toDateString()

    todayMeetings.value = (meetings || [])
      .filter(m => new Date(m.date).toDateString() === today)
      .map(m => ({ ...m, isNow: isCurrentMeeting(m) }))

    upcomingMeetings.value = (meetings || [])
      .filter(m => new Date(m.date).toDateString() !== today)
  } catch (error) {
    console.error('Failed to fetch meetings:', error)
  } finally {
    loadingMeetings.value = false
  }
}

const fetchActivities = async () => {
  loadingActivities.value = true
  try {
    const activities = await DashboardService.getRecentActivities(10)
    recentActivities.value = activities || []
  } catch (error) {
    console.error('Failed to fetch activities:', error)
  } finally {
    loadingActivities.value = false
  }
}

// Lifecycle
onMounted(() => {
  fetchDashboardData()
  fetchMeetings()
  fetchActivities()
  clockInterval = window.setInterval(() => {
    currentTime.value = new Date()
  }, 60000)
})

onUnmounted(() => {
  if (clockInterval) clearInterval(clockInterval)
})
</script>

<style scoped>
/* ===== CSS Variables ===== */
.admin-dashboard {
  --emerald-500: #006d4b;
  --emerald-600: #005a3e;
  --teal-500: #006d4b;

  position: relative;
  font-family: 'Tajawal', sans-serif;
  color: #27272a;
}

/* ===== Key Metrics (matching IAM .key-metrics) ===== */
.key-metrics {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-bottom: 24px;
}

@media (max-width: 1200px) {
  .key-metrics {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 640px) {
  .key-metrics {
    grid-template-columns: 1fr;
  }
}

/* ===== MISA Light Stat Cards ===== */
.dark-stat-card {
  position: relative;
  background: #fff;
  border-radius: 12px;
  padding: 24px;
  color: #1a2e25;
  overflow: hidden;
  transition: all 0.3s ease;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06), 0 2px 12px rgba(0, 109, 75, 0.04);
  border: 1px solid #e4ede8;
  min-height: 140px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.dark-stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 20px rgba(0, 109, 75, 0.1);
  border-color: #c8ddd3;
}

.dark-stat-card.stat-alert {
  border-color: rgba(239, 68, 68, 0.3);
}

.dark-stat-wave {
  position: absolute;
  left: 0;
  bottom: 0;
  width: 100%;
  height: 60px;
  pointer-events: none;
  opacity: 0.5;
}

.dark-stat-top {
  display: flex;
  align-items: center;
  gap: 8px;
  z-index: 1;
  position: relative;
}

.dark-stat-icon {
  color: #006d4b;
}

.stat-alert .dark-stat-icon {
  color: #ef4444;
}

.dark-stat-label {
  font-size: 13px;
  font-weight: 600;
  color: #6b8a7d;
  letter-spacing: 0.02em;
}

.dark-stat-bottom {
  z-index: 1;
  position: relative;
  margin-top: auto;
}

.dark-stat-value {
  font-size: 32px;
  font-weight: 800;
  color: #1a2e25;
  line-height: 1;
  display: block;
}

.dark-stat-trend {
  font-size: 13px;
  font-weight: 600;
  color: #6b8a7d;
  margin-top: 6px;
  display: flex;
  align-items: center;
  gap: 4px;
  z-index: 1;
  position: relative;
}

.dark-stat-trend.trend-positive { color: #22c55e; }
.dark-stat-trend.trend-negative { color: #ef4444; }

/* ===== Secondary Stats (Case Portal metric-card with accent border-top) ===== */
.stats-section {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 16px;
  margin-bottom: 2rem;
}

.metric-card {
  background: #ffffff;
  border-radius: 12px;
  border: 1px solid #e5e7eb;
  border-top: 3px solid var(--accent, #006d4b);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.metric-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.08);
}

.metric-card-content {
  padding: 18px 16px;
  display: flex;
  flex-direction: column;
  height: 100%;
  box-sizing: border-box;
}

.metric-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 14px;
}

.metric-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: transform 0.3s ease;
}

.metric-card:hover .metric-icon {
  transform: scale(1.05);
}

.metric-label {
  font-size: 12px;
  font-weight: 600;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  line-height: 1.3;
  min-width: 0;
}

.metric-main {
  display: flex;
  align-items: baseline;
  gap: 10px;
}

.metric-value {
  font-size: 28px;
  font-weight: 700;
  color: #1e293b;
  line-height: 1;
}

/* ===== Charts Section ===== */
.charts-section {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 20px;
  margin-bottom: 24px;
}

/* ===== Lists Section ===== */
.lists-section {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 20px;
  margin-bottom: 24px;
}

/* ===== Activity Section ===== */
.activity-section {
  margin-bottom: 24px;
}

/* ===== Dashboard Card (Case Portal pattern) ===== */
.db-card {
  background: #ffffff;
  border-radius: 12px;
  border: 1px solid #e5e7eb;
  overflow: hidden;
}

.db-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid #f0f0f0;
}

.db-card-title-group {
  display: flex;
  align-items: center;
  gap: 12px;
}

.db-card-title {
  font-size: 0.95rem;
  font-weight: 600;
  color: #1e293b;
  margin: 0;
  line-height: 1.3;
}

.db-card-subtitle {
  font-size: 0.75rem;
  color: #94a3b8;
  margin: 2px 0 0;
}

.db-card-badge {
  background: #006d4b;
  color: white;
  font-size: 0.7rem;
  font-weight: 600;
  padding: 3px 10px;
  border-radius: 20px;
  min-width: 24px;
  text-align: center;
}

.db-card-link {
  display: flex;
  align-items: center;
  gap: 2px;
  font-size: 0.8rem;
  font-weight: 500;
  color: #006d4b;
  text-decoration: none;
  transition: color 0.2s ease;
}

.db-card-link:hover {
  color: #007E65;
  text-decoration: underline;
}

.db-card-body {
  padding: 20px;
}

.db-card-body.list-body {
  padding: 8px 12px;
  max-height: 380px;
  overflow-y: auto;
}

.db-card-body.centered {
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Loading skeleton */
.db-card-loading {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 8px 0;
}

.skeleton-row {
  height: 52px;
  background: linear-gradient(90deg, #f1f5f9 25%, #e2e8f0 50%, #f1f5f9 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s ease-in-out infinite;
  border-radius: 8px;
}

@keyframes shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

/* Empty state */
.db-card-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  color: #94a3b8;
  gap: 8px;
  text-align: center;
}

.db-card-empty span {
  font-size: 0.85rem;
}

.empty-hint {
  font-size: 0.78rem;
  color: #cbd5e1;
}

.empty-action-btn {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  margin-top: 8px;
  padding: 6px 16px;
  font-size: 0.8rem;
  font-weight: 500;
  color: white;
  background: linear-gradient(90deg, #004730 0%, #006d4b 100%);
  border-radius: 6px;
  text-decoration: none;
  transition: opacity 0.2s ease;
}

.empty-action-btn:hover {
  opacity: 0.9;
}

/* ===== Meeting List ===== */
.meeting-list {
  display: flex;
  flex-direction: column;
}

.meeting-row {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 10px 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s ease;
}

.meeting-row:hover {
  background: #f8fafc;
}

.meeting-row.is-live {
  background: rgba(0, 109, 75, 0.04);
}

/* Time column (today's meetings) */
.meeting-time-col {
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 56px;
  flex-shrink: 0;
}

.meeting-time-text {
  font-size: 0.9rem;
  font-weight: 600;
  color: #1e293b;
}

.live-dot-wrap {
  display: flex;
  align-items: center;
  gap: 4px;
  margin-top: 4px;
}

.live-dot {
  width: 7px;
  height: 7px;
  background: #006d4b;
  border-radius: 50%;
  animation: blink 1s ease-in-out infinite;
}

@keyframes blink {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.3; }
}

.live-label {
  font-size: 0.65rem;
  font-weight: 600;
  color: #006d4b;
  text-transform: uppercase;
}

/* Date box (upcoming meetings) */
.date-box {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  background: linear-gradient(135deg, #e6f9f5 0%, #d4f5ed 100%);
  border-radius: 10px;
  flex-shrink: 0;
}

.date-day {
  font-size: 1.15rem;
  font-weight: 700;
  color: #007E65;
  line-height: 1;
}

.date-month {
  font-size: 0.6rem;
  font-weight: 600;
  color: #006d4b;
  text-transform: uppercase;
  margin-top: 2px;
}

/* Meeting info */
.meeting-info-col {
  flex: 1;
  min-width: 0;
}

.meeting-name {
  font-size: 0.85rem;
  font-weight: 500;
  color: #1e293b;
  margin: 0 0 4px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  line-height: 1.3;
}

.meeting-meta-row {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.meta-tag {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 0.75rem;
  color: #64748b;
}

.meta-tag svg,
.meta-tag span {
  flex-shrink: 0;
}

.meeting-arrow {
  color: #cbd5e1;
  flex-shrink: 0;
  transition: color 0.2s ease;
}

.meeting-row:hover .meeting-arrow {
  color: #006d4b;
}

/* ===== Activity Feed ===== */
.activity-feed {
  display: flex;
  flex-direction: column;
}

.activity-row {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 10px 0;
  border-bottom: 1px solid #f8fafc;
}

.activity-row:last-child {
  border-bottom: none;
}

.activity-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  margin-top: 5px;
  flex-shrink: 0;
  background: #cbd5e1;
}

.activity-dot.meeting { background: #006d4b; }
.activity-dot.task { background: #3b82f6; }
.activity-dot.approval { background: #8b5cf6; }
.activity-dot.recommendation { background: #f59e0b; }

.activity-info {
  flex: 1;
  min-width: 0;
}

.activity-text {
  display: block;
  font-size: 0.82rem;
  color: #334155;
  line-height: 1.4;
  margin: 0;
}

.activity-time {
  display: block;
  font-size: 0.72rem;
  color: #94a3b8;
  margin-top: 3px;
}

/* ===== Responsive ===== */
@media (max-width: 1280px) {
  .stats-section {
    grid-template-columns: repeat(3, 1fr);
  }
  .charts-section {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .stats-section {
    grid-template-columns: repeat(2, 1fr);
  }
  .lists-section {
    grid-template-columns: 1fr;
  }
  .activity-feed {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 480px) {
  .stats-section {
    grid-template-columns: 1fr;
  }
}
</style>

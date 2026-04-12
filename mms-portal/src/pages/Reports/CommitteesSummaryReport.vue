<template>
  <div class="space-y-5">
    <!-- Page Header -->
    <PageHeader
      :title="$t('CommitteesSummaryReport')"
      :subtitle="$t('CommitteesSummaryDesc')"
    />

    <!-- Grid Container -->
    <div class="mg-container">
      <!-- Loading -->
      <div v-if="loading" class="mg-state">
        <Icon icon="mdi:loading" class="w-6 h-6 animate-spin" style="color: #008080" />
      </div>

      <!-- Empty -->
      <div v-else-if="committees.length === 0" class="mg-state">
        <Icon icon="mdi:chart-bar" class="w-12 h-12" style="color: #ccc" />
        <p>{{ $t('NoData') }}</p>
      </div>

      <!-- Table -->
      <template v-else>
        <div class="mg-toolbar">
          <span class="mg-count-badge">{{ totalCount }} {{ $t('Committees') }}</span>
        </div>

        <div class="mg-table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>#</th>
                <th>{{ $t('Name') }}</th>
                <th>{{ $t('Status') }}</th>
                <th>{{ $t('StartDate') }}</th>
                <th>{{ $t('EndDate') }}</th>
                <th>{{ $t('MeetingsCount') }}</th>
                <th>{{ $t('SubCommitteesCount') }}</th>
                <th>{{ $t('MembersCount') }}</th>
                <th>{{ $t('RecommendationsCount') }}</th>
                <th>{{ $t('AttendanceRate') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, index) in committees" :key="row.id">
                <td><span class="mg-id">{{ (page - 1) * pageSize + index + 1 }}</span></td>
                <td><span class="mg-title">{{ truncate(row.name, 50) }}</span></td>
                <td>
                  <span :class="['mg-pill', row.isActive ? 'completed' : 'cancelled']">
                    {{ row.isActive ? ($t('Active')) : ($t('Inactive')) }}
                  </span>
                </td>
                <td>{{ formatDate(row.startDate) }}</td>
                <td>{{ formatDate(row.endDate) }}</td>
                <td>
                  <button class="cs-count-btn" @click="showMeetingDetails(row)">
                    {{ row.meetingsCount }}
                  </button>
                </td>
                <td>{{ row.subCommitteesCount }}</td>
                <td>{{ row.membersCount }}</td>
                <td>{{ row.recommendationsCount }}</td>
                <td>
                  <div class="cs-rate">
                    <span class="cs-rate-val">{{ row.attendanceRate }}%</span>
                    <div class="cs-rate-track">
                      <div class="cs-rate-fill" :style="{ width: row.attendanceRate + '%' }"></div>
                    </div>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div v-if="totalCount > pageSize" class="mg-pag">
          <span class="mg-pag-info">
            {{ $t('Showing') }} <strong>{{ (page - 1) * pageSize + 1 }}</strong>
            {{ $t('To') }} <strong>{{ Math.min(page * pageSize, totalCount) }}</strong>
            {{ $t('Of') }} <strong>{{ totalCount }}</strong>
          </span>
          <div class="mg-pag-btns">
            <button class="mg-pg" :disabled="page === 1" @click="goToPage(1)">
              <Icon icon="mdi:chevron-double-left" class="w-4 h-4" />
            </button>
            <button class="mg-pg" :disabled="page === 1" @click="goToPage(page - 1)">
              <Icon icon="mdi:chevron-left" class="w-4 h-4" />
            </button>
            <button v-for="p in visiblePages" :key="p" class="mg-pg num" :class="{ active: p === page }" @click="goToPage(p)">{{ p }}</button>
            <button class="mg-pg" :disabled="page === totalPages" @click="goToPage(page + 1)">
              <Icon icon="mdi:chevron-right" class="w-4 h-4" />
            </button>
            <button class="mg-pg" :disabled="page === totalPages" @click="goToPage(totalPages)">
              <Icon icon="mdi:chevron-double-right" class="w-4 h-4" />
            </button>
          </div>
        </div>
      </template>
    </div>

    <!-- Meeting Details Dialog -->
    <Modal
      v-model="showMeetingCountDialog"
      :title="$t('MeetingsDetails')"
      size="lg"
      @close="closeChartDialog"
    >
      <!-- Loading State -->
      <div v-if="loadingChart" class="loading-chart">
        <div class="loader-spinner"></div>
        <p>{{ $t('Loading') }}</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="!statusMeetingsCount || statusMeetingsCount.length === 0" class="empty-chart">
        <Icon icon="mdi:chart-bar" class="w-16 h-16 text-zinc-300" />
        <p class="text-zinc-500 mt-3">{{ $t('NoData') }}</p>
      </div>

      <!-- Chart Content -->
      <div v-else class="chart-container">
        <!-- Total Summary -->
        <div class="chart-summary">
          <div class="summary-icon">
            <Icon icon="mdi:calendar-multiple" class="w-6 h-6" />
          </div>
          <div class="summary-info">
            <span class="summary-label">{{ $t('TotalMeetings') }}</span>
            <span class="summary-value">{{ getTotalMeetings() }}</span>
          </div>
        </div>

        <!-- Bar Chart -->
        <div class="chart-bars">
          <div
            v-for="(item, index) in statusMeetingsCount"
            :key="item.statusId || index"
            class="chart-bar-item"
          >
            <div class="bar-header">
              <span class="bar-label">{{ item.statusName || item.name || 'غير محدد' }}</span>
              <span class="bar-value">{{ item.count || item.meetingsCount || 0 }}</span>
            </div>
            <div class="bar-track">
              <div
                class="bar-fill"
                :style="{
                  width: getBarWidth(item.count || item.meetingsCount || 0) + '%',
                  backgroundColor: getBarColor(index)
                }"
              ></div>
            </div>
          </div>
        </div>
      </div>

      <template #footer>
        <Button variant="outline" @click="closeChartDialog">
          {{ $t('Close') }}
        </Button>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/components/layout/PageHeader.vue'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import Modal from '@/components/ui/Modal.vue'
import ReportsService from '@/services/ReportsService'

const { t, locale } = useI18n()

// State
const loading = ref(false)
const committees = ref<any[]>([])
const totalCount = ref(0)
const page = ref(1)
const pageSize = ref(10)

const showMeetingCountDialog = ref(false)
const loadingChart = ref(false)
const selectedCommitteeId = ref<string | null>(null)
const statusMeetingsCount = ref<any[]>([])

const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))
const visiblePages = computed(() => {
  const pages: number[] = []
  const s = Math.max(1, page.value - 2)
  const e = Math.min(totalPages.value, s + 4)
  for (let i = s; i <= e; i++) pages.push(i)
  return pages
})

// Methods
const loadCommittees = async () => {
  loading.value = true
  try {
    const result: any = await ReportsService.getCommitteesSummary(page.value, pageSize.value)
    const data = result?.data?.data || result?.data || result || []
    const total = result?.data?.total || result?.total || 0

    committees.value = Array.isArray(data) ? data : []
    totalCount.value = total
  } catch (error) {
    console.error('Failed to load committees:', error)
    committees.value = []
    totalCount.value = 0
  } finally {
    loading.value = false
  }
}

const goToPage = (p: number) => {
  page.value = p
  loadCommittees()
}

const showMeetingDetails = async (committee: any) => {
  // Reset state first
  statusMeetingsCount.value = []
  loadingChart.value = true
  selectedCommitteeId.value = committee.id
  showMeetingCountDialog.value = true

  try {
    const result: any = await ReportsService.getStatusMeetingsCount(committee.id)

    // Handle the chart data structure: { labels: [], series: [{ data: [], name: '' }] }
    const chartData = result?.data || result

    if (chartData?.labels && chartData?.series?.[0]?.data) {
      const labels = chartData.labels
      const counts = chartData.series[0].data

      // Combine labels and counts into our format
      statusMeetingsCount.value = labels.map((label: string, index: number) => ({
        statusId: index,
        statusName: label,
        count: counts[index] || 0
      })).filter((item: any) => item.count > 0) // Only show statuses with meetings
    } else {
      statusMeetingsCount.value = []
    }
  } catch (error) {
    console.error('Failed to load status meetings count:', error)
    statusMeetingsCount.value = []
  } finally {
    loadingChart.value = false
  }
}

const closeChartDialog = () => {
  showMeetingCountDialog.value = false
  // Reset state after close
  setTimeout(() => {
    statusMeetingsCount.value = []
    loadingChart.value = false
  }, 300)
}

const formatDate = (date: string) => {
  if (!date) return '-'
  const loc = locale.value === 'ar' ? 'ar-EG' : 'en-US'
  return new Date(date).toLocaleDateString(loc, { year: 'numeric', month: 'short', day: 'numeric', calendar: 'gregory' })
}

// Chart helpers
const getTotalMeetings = () => {
  return statusMeetingsCount.value.reduce((sum, item) => sum + (item.count || item.meetingsCount || 0), 0)
}

const getBarWidth = (count: number) => {
  const total = getTotalMeetings()
  if (total === 0) return 5 // Minimum width
  return Math.max(5, Math.round((count / total) * 100))
}

const chartColors = [
  '#006d4b', // Primary green
  '#3b82f6', // Blue
  '#f59e0b', // Amber
  '#14919b', // Teal
  '#8b5cf6', // Purple
  '#ef4444', // Red
  '#ec4899', // Pink
  '#6366f1', // Indigo
  '#84cc16', // Lime
  '#06b6d4', // Cyan
]

const getBarColor = (index: number) => {
  return chartColors[index % chartColors.length]
}

const truncate = (text: string, length: number) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}

// Watch for modal close (from X button)
watch(showMeetingCountDialog, (newVal) => {
  if (!newVal) {
    // Reset state when modal closes
    setTimeout(() => {
      statusMeetingsCount.value = []
      loadingChart.value = false
    }, 300)
  }
})

// Lifecycle
onMounted(() => {
  loadCommittees()
})
</script>

<style scoped>
/* Toolbar */
.mg-toolbar {
  display: flex; align-items: center; justify-content: flex-end;
  padding: 10px 16px; border-bottom: 1px solid #eaeaea;
}
.mg-count-badge {
  font-size: 0.8rem; color: #64748b; background: #f1f5f9;
  padding: 4px 12px; border-radius: 12px; font-weight: 500;
}

/* Meetings count button */
.cs-count-btn {
  display: inline-flex; align-items: center; justify-content: center;
  min-width: 36px; padding: 4px 10px; border-radius: 8px; border: 1px solid #c8ddd3;
  background: #e8f5ef; color: #006d4b; font-weight: 700; font-size: 13px;
  cursor: pointer; transition: all 0.15s;
}
.cs-count-btn:hover { opacity: 0.85; transform: translateY(-1px); }

/* Attendance rate */
.cs-rate { display: flex; flex-direction: column; gap: 4px; min-width: 70px; }
.cs-rate-val { font-size: 12px; font-weight: 700; color: #006d4b; }
.cs-rate-track { height: 5px; background: #e5e7eb; border-radius: 4px; overflow: hidden; }
.cs-rate-fill { height: 100%; background: #006d4b; border-radius: 4px; transition: width 0.5s ease-out; }

/* Chart Modal */
.loading-chart {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  padding: 48px 0; color: #94a3b8;
}
.loader-spinner {
  width: 36px; height: 36px; border: 3px solid #e5e7eb; border-top-color: #006d4b;
  border-radius: 50%; margin-bottom: 12px; animation: spin 0.8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

.empty-chart { display: flex; flex-direction: column; align-items: center; padding: 48px 0; }

.chart-container { display: flex; flex-direction: column; gap: 20px; }
.chart-summary {
  display: flex; align-items: center; gap: 14px; padding: 14px;
  border-radius: 10px; background: #f8fafc; border: 1px solid #e6eaef;
}
.summary-icon {
  width: 44px; height: 44px; border-radius: 10px; display: flex; align-items: center; justify-content: center;
  background: rgba(0, 109, 75, 0.1); color: #006d4b;
}
.summary-info { display: flex; flex-direction: column; }
.summary-label { font-size: 12px; color: #94a3b8; }
.summary-value { font-size: 28px; font-weight: 700; color: #004730; }

.chart-bars { display: flex; flex-direction: column; gap: 14px; }
.chart-bar-item { display: flex; flex-direction: column; gap: 6px; }
.bar-header { display: flex; align-items: center; justify-content: space-between; }
.bar-label { font-size: 13px; font-weight: 500; color: #334155; }
.bar-value { font-size: 13px; font-weight: 700; color: #004730; background: #f1f5f9; padding: 2px 8px; border-radius: 4px; }
.bar-track { height: 10px; background: #f1f5f9; border-radius: 6px; overflow: hidden; }
.bar-fill { height: 100%; border-radius: 6px; animation: barGrow 0.6s ease-out forwards; }
@keyframes barGrow { from { width: 0%; } }

/* Responsive */
@media (max-width: 768px) {
  .chart-legend {
    @apply flex-col gap-2;
  }
}
</style>

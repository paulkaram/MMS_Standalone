<template>
  <div class="space-y-4">
    <!-- Page Header -->
    <PageHeader
      :title="$t('Recommendations')"
      :subtitle="$t('ManageAndTrackRecommendations')"
    />

    <!-- Main Content Grid -->
    <div class="grid grid-cols-1 xl:grid-cols-4 gap-4">
      <!-- Main Table Section -->
      <div class="xl:col-span-3">
        <!-- Search Filters -->
        <div class="filter-card mb-4">

          <div class="filters-row">
            <div class="filter-field">
              <label>{{ $t('ReferenceNumber') }}</label>
              <input
                v-model="searchModel.meetingReferenceNo"
                type="text"
                :placeholder="$t('EnterReferenceNumber')"
              />
            </div>
            <div class="filter-field">
              <label>{{ $t('FromDate') }}</label>
              <DatePicker
                v-model="searchModel.fromDate"
                :placeholder="$t('SelectDate')"
                :clearable="true"
                container-class="filter-datepicker"
              />
            </div>
            <div class="filter-field">
              <label>{{ $t('ToDate') }}</label>
              <DatePicker
                v-model="searchModel.toDate"
                :placeholder="$t('SelectDate')"
                :clearable="true"
                :min-date="searchModel.fromDate"
                container-class="filter-datepicker"
              />
            </div>
            <div class="filter-field">
              <label>{{ $t('Title') }}</label>
              <input
                v-model="searchModel.title"
                type="text"
                :placeholder="$t('EnterTitle')"
              />
            </div>
          </div>

          <div class="flex items-center gap-2 mt-4">
            <button
              class="btn-primary"
              :disabled="loading"
              @click="loadRecommendations"
            >
              <svg v-if="!loading" class="w-4 h-4" viewBox="0 0 24 24" fill="currentColor">
                <path d="M9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.44,13.73L14.71,14H15.5L20.5,19L19,20.5L14,15.5V14.71L13.73,14.44C12.59,15.41 11.11,16 9.5,16A6.5,6.5 0 0,1 3,9.5A6.5,6.5 0 0,1 9.5,3M9.5,5C7,5 5,7 5,9.5C5,12 7,14 9.5,14C12,14 14,12 14,9.5C14,7 12,5 9.5,5Z"/>
              </svg>
              <div v-else class="spinner small"></div>
              {{ $t('Search') }}
            </button>
            <button
              class="btn-outline"
              @click="reset"
            >
              <svg class="w-4 h-4" viewBox="0 0 24 24" fill="currentColor">
                <path d="M17.65,6.35C16.2,4.9 14.21,4 12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20C15.73,20 18.84,17.45 19.73,14H17.65C16.83,16.33 14.61,18 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6C13.66,6 15.14,6.69 16.22,7.78L13,11H20V4L17.65,6.35Z"/>
              </svg>
              {{ $t('Reset') }}
            </button>
          </div>
        </div>

        <!-- Data Grid -->
        <div class="mg-container">
          <!-- Loading -->
          <div v-if="loading" class="mg-state">
            <Icon icon="mdi:loading" class="w-6 h-6 animate-spin" style="color: #006d4b" />
          </div>

          <!-- Empty -->
          <div v-else-if="recommendations.length === 0" class="mg-state">
            <Icon icon="mdi:lightbulb-off-outline" class="w-12 h-12" style="color: #ccc" />
            <p>{{ $t('NoRecommendationsFound') }}</p>
            <p style="font-size: 12px; color: #94a3b8">{{ $t('TryAdjustingFilters') }}</p>
          </div>

          <!-- Table -->
          <template v-else>
            <div class="mg-toolbar">
              <span class="mg-count">{{ totalCount }} {{ $t('Recommendation') }}</span>
            </div>

            <div class="mg-table-wrap">
              <table class="data-table">
                <thead>
                  <tr>
                    <th>{{ $t('Id') }}</th>
                    <th>{{ $t('Owner') }}</th>
                    <th>{{ $t('ReferenceNumber') }}</th>
                    <th>{{ $t('Date') }}</th>
                    <th>{{ $t('Title') }}</th>
                    <th>{{ $t('Progress') }}</th>
                    <th>{{ $t('DueDate') }}</th>
                    <th>{{ $t('Status') }}</th>
                    <th>{{ $t('Actions') }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="rec in recommendations" :key="rec.id">
                    <td><span class="mg-id">{{ rec.id }}</span></td>
                    <td>
                      <div style="display:flex;align-items:center;gap:8px">
                        <UserAvatar :userId="rec.owner" :name="rec.ownerName || ''" size="xs" />
                        <span class="mg-title">{{ rec.ownerName }}</span>
                      </div>
                    </td>
                    <td><span class="mg-ref">{{ rec.meetingReferenceNo }}</span></td>
                    <td>{{ formatDate(rec.createdAt) }}</td>
                    <td><span class="mg-title" :title="rec.text">{{ truncate(rec.text, 40) }}</span></td>
                    <td>
                      <div class="rec-progress-cell">
                        <span class="rec-progress-badge" :style="{ color: getProgressColor(rec.percentage) }">{{ rec.percentage }}%</span>
                        <div class="rec-progress-track">
                          <div class="rec-progress-fill" :style="{ width: rec.percentage + '%', backgroundColor: getProgressColor(rec.percentage) }"></div>
                        </div>
                      </div>
                    </td>
                    <td>{{ formatDate(rec.dueDate) }}</td>
                    <td><span class="mg-pill" :class="getStatusClass(rec.status)">{{ rec.statusName || rec.status }}</span></td>
                    <td>
                      <div class="mg-actions">
                        <button v-if="canEdit(rec)" class="mg-act" :title="$t('Edit')" @click="openRecommendation(rec, false)">
                          <Icon icon="mdi:pencil-outline" class="w-4 h-4" />
                        </button>
                        <button v-else class="mg-act" :title="$t('View')" @click="openRecommendation(rec, true)">
                          <Icon icon="mdi:eye-outline" class="w-4 h-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- Pagination -->
            <div v-if="totalPages > 1" class="mg-pag">
              <span class="mg-pag-info">
                {{ $t('Showing') }} <strong>{{ (page - 1) * pageSize + 1 }}</strong>
                {{ $t('To') }} <strong>{{ Math.min(page * pageSize, totalCount) }}</strong>
                {{ $t('Of') }} <strong>{{ totalCount }}</strong>
              </span>
              <div class="mg-pag-btns">
                <button class="mg-pg" :disabled="page === 1" @click="handlePageChange(1)">
                  <Icon icon="mdi:chevron-double-left" class="w-4 h-4" />
                </button>
                <button class="mg-pg" :disabled="page === 1" @click="handlePageChange(page - 1)">
                  <Icon icon="mdi:chevron-left" class="w-4 h-4" />
                </button>
                <button
                  v-for="p in visiblePages"
                  :key="p"
                  class="mg-pg"
                  :class="{ active: p === page }"
                  @click="handlePageChange(p)"
                >{{ p }}</button>
                <button class="mg-pg" :disabled="page === totalPages" @click="handlePageChange(page + 1)">
                  <Icon icon="mdi:chevron-right" class="w-4 h-4" />
                </button>
                <button class="mg-pg" :disabled="page === totalPages" @click="handlePageChange(totalPages)">
                  <Icon icon="mdi:chevron-double-right" class="w-4 h-4" />
                </button>
              </div>
            </div>
          </template>
        </div>
      </div>

      <!-- Sidebar -->
      <div class="space-y-4">
        <!-- Quick Stats Card -->
        <div class="stats-card">
          <svg class="stats-card-wave" viewBox="0 0 100 100" preserveAspectRatio="none"><path d="M0,0 Q50,50 0,100 L100,100 L100,0 Z" fill="#006d4b"/></svg>
          <div class="relative z-10">
            <div class="flex items-center justify-between mb-1">
              <span class="material-symbols-outlined text-[#006d4b] text-xl">bar_chart</span>
              <span class="stats-card-label">{{ $t('QuickStats') }}</span>
            </div>
            <p class="stats-card-value">{{ stats.total }}</p>
            <p class="stats-card-sub mb-3">{{ $t('TotalRecommendations') }}</p>

            <div class="grid grid-cols-2 gap-2">
              <div class="stats-mini-card">
                <p class="text-lg font-bold text-[#006d4b]">{{ stats.completed }}</p>
                <p class="text-[9px] text-zinc-500">{{ $t('Completed') }}</p>
              </div>
              <div class="stats-mini-card">
                <p class="text-lg font-bold text-[#006d4b]">{{ stats.inProgress }}</p>
                <p class="text-[9px] text-zinc-500">{{ $t('InProgress') }}</p>
              </div>
              <div class="stats-mini-card">
                <p class="text-lg font-bold text-[#006d4b]">{{ stats.pending }}</p>
                <p class="text-[9px] text-zinc-500">{{ $t('Pending') }}</p>
              </div>
              <div class="stats-mini-card warning">
                <p class="text-lg font-bold text-[#006d4b]">{{ stats.overdue }}</p>
                <p class="text-[9px] text-zinc-500">{{ $t('Overdue') }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Average Progress Card -->
        <div class="avg-progress-card">
          <svg class="avg-progress-wave" viewBox="0 0 100 100" preserveAspectRatio="none"><path d="M0,0 Q50,50 0,100 L100,100 L100,0 Z" fill="#006d4b"/></svg>
          <div class="relative z-10">
            <div class="flex items-center justify-between mb-1">
              <span class="material-symbols-outlined text-[#006d4b] text-xl">trending_up</span>
              <span class="avg-progress-label">{{ $t('AverageProgress') }}</span>
            </div>
            <p class="avg-progress-value">{{ stats.averageProgress }}%</p>
            <div class="avg-progress-track">
              <div class="avg-progress-fill" :style="{ width: stats.averageProgress + '%' }"></div>
            </div>
            <p class="avg-progress-sub">{{ stats.completed }} {{ $t('CompletedOf') }} {{ stats.total }}</p>
          </div>
        </div>

        <!-- My Pending Actions Card -->
        <div class="attention-card">
          <h3 class="font-semibold text-zinc-900 mb-3 flex items-center justify-between text-sm">
            <span class="flex items-center gap-2">
              <div class="w-7 h-7 rounded-lg bg-[#006d4b] flex items-center justify-center">
                <span class="material-symbols-outlined text-[#006d4b] text-sm">notification_important</span>
              </div>
              {{ $t('NeedsAttention') }}
            </span>
          </h3>

          <!-- Urgent Items (Overdue) -->
          <div v-if="urgentRecommendations.length > 0" class="mb-3">
            <div class="flex items-center gap-1.5 mb-2">
              <span class="w-2 h-2 rounded-full bg-red-500 animate-pulse"></span>
              <span class="text-[10px] font-semibold text-red-600 uppercase">{{ $t('Overdue') }} ({{ urgentRecommendations.length }})</span>
            </div>
            <div class="space-y-1.5">
              <div
                v-for="rec in urgentRecommendations.slice(0, 2)"
                :key="rec.id"
                class="urgent-item"
                @click="openRecommendation(rec, false)"
              >
                <div class="flex-1 min-w-0">
                  <p class="text-xs font-medium text-zinc-900 truncate">{{ truncate(rec.text, 25) }}</p>
                  <p class="text-[10px] text-red-500">{{ getDaysOverdue(rec.dueDate) }}</p>
                </div>
                <div class="urgent-progress">{{ rec.percentage }}%</div>
              </div>
            </div>
          </div>

          <!-- Due Soon Items -->
          <div v-if="dueSoonRecommendations.length > 0">
            <div class="flex items-center gap-1.5 mb-2">
              <span class="w-2 h-2 rounded-full bg-yellow-500"></span>
              <span class="text-[10px] font-semibold text-yellow-600 uppercase">{{ $t('DueSoon') }} ({{ dueSoonRecommendations.length }})</span>
            </div>
            <div class="space-y-1.5">
              <div
                v-for="rec in dueSoonRecommendations.slice(0, 2)"
                :key="rec.id"
                class="due-soon-item"
                @click="openRecommendation(rec, false)"
              >
                <div class="flex-1 min-w-0">
                  <p class="text-xs font-medium text-zinc-900 truncate">{{ truncate(rec.text, 25) }}</p>
                  <p class="text-[10px] text-yellow-600">{{ getDaysUntilDue(rec.dueDate) }}</p>
                </div>
                <div class="due-soon-progress">{{ rec.percentage }}%</div>
              </div>
            </div>
          </div>

          <!-- No urgent items -->
          <div v-if="urgentRecommendations.length === 0 && dueSoonRecommendations.length === 0" class="text-center py-4">
            <div class="w-10 h-10 rounded-full bg-green-50 flex items-center justify-center mx-auto mb-2">
              <svg class="w-5 h-5 text-green-500" viewBox="0 0 24 24" fill="currentColor">
                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
              </svg>
            </div>
            <p class="text-xs text-zinc-500">{{ $t('AllCaughtUp') }}</p>
          </div>

          <!-- View All Mine Button -->
          <button
            v-if="myRecommendations.length > 0"
            class="w-full mt-3 py-2 text-xs font-medium text-primary bg-primary/5 hover:bg-primary/10 rounded-lg transition-colors"
            @click="filterMyRecommendations"
          >
            {{ $t('ViewAllMine') }} ({{ myRecommendations.length }})
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/components/layout/PageHeader.vue'
import DatePicker from '@/components/ui/DatePicker.vue'
import Icon from '@/components/ui/Icon.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import { formatDate as formatDateHelper } from '@/helpers/dateFormat'
import RecommendationsService, { type Recommendation } from '@/services/RecommendationsService'
import { useUserStore } from '@/stores/user'

const router = useRouter()
const { t } = useI18n()
const userStore = useUserStore()

// State
const loading = ref(false)
const recommendations = ref<Recommendation[]>([])
const totalCount = ref(0)
const page = ref(1)
const pageSize = ref(10)

const searchModel = ref({
  meetingReferenceNo: null as string | null,
  fromDate: null as string | null,
  toDate: null as string | null,
  title: null as string | null
})

// Computed
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

const visiblePages = computed(() => {
  const total = totalPages.value
  const current = page.value
  const pages: number[] = []
  const start = Math.max(1, current - 2)
  const end = Math.min(total, start + 4)
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
})

const getProgressColor = (percentage: number) => {
  if (percentage >= 100) return '#006d4b'
  if (percentage >= 50) return '#f59e0b'
  return '#3b82f6'
}

const stats = computed(() => {
  const recs = Array.isArray(recommendations.value) ? recommendations.value : []
  const completed = recs.filter(r => r.percentage === 100).length
  const inProgress = recs.filter(r => r.percentage > 0 && r.percentage < 100).length
  const pending = recs.filter(r => r.percentage === 0).length
  const overdue = recs.filter(r => {
    if (!r.dueDate) return false
    return new Date(r.dueDate) < new Date() && r.percentage < 100
  }).length
  const avgProgress = recs.length > 0
    ? Math.round(recs.reduce((sum, r) => sum + (r.percentage || 0), 0) / recs.length)
    : 0

  return {
    total: totalCount.value,
    completed,
    inProgress,
    pending,
    overdue,
    averageProgress: avgProgress
  }
})

const myRecommendations = computed(() => {
  const recs = Array.isArray(recommendations.value) ? recommendations.value : []
  const userId = userStore.loggedInUser?.id
  // Use loose equality to handle type coercion
  return recs.filter(r => r.owner == userId)
})

// Overdue recommendations (past due date and not completed)
const urgentRecommendations = computed(() => {
  const now = new Date()
  now.setHours(0, 0, 0, 0)
  return myRecommendations.value
    .filter(r => {
      if (!r.dueDate || r.percentage >= 100) return false
      const dueDate = new Date(r.dueDate)
      dueDate.setHours(0, 0, 0, 0)
      return dueDate < now
    })
    .sort((a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime())
})

// Due within next 7 days (not overdue, not completed)
const dueSoonRecommendations = computed(() => {
  const now = new Date()
  now.setHours(0, 0, 0, 0)
  const weekFromNow = new Date(now)
  weekFromNow.setDate(weekFromNow.getDate() + 7)

  return myRecommendations.value
    .filter(r => {
      if (!r.dueDate || r.percentage >= 100) return false
      const dueDate = new Date(r.dueDate)
      dueDate.setHours(0, 0, 0, 0)
      return dueDate >= now && dueDate <= weekFromNow
    })
    .sort((a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime())
})

// Methods
const loadRecommendations = async () => {
  loading.value = true
  try {
    // Sanitize search params - convert empty strings to null
    const params = {
      meetingReferenceNo: searchModel.value.meetingReferenceNo?.trim() || null,
      fromDate: searchModel.value.fromDate || null,
      toDate: searchModel.value.toDate || null,
      title: searchModel.value.title?.trim() || null
    }
    const response: any = await RecommendationsService.searchRecommendations(
      params,
      page.value,
      pageSize.value
    )
    // API returns { data: { data: [], total: number } } or { data: [], total: number }
    const result = response?.data || response
    recommendations.value = result?.data || []
    totalCount.value = result?.total || 0
  } catch (error) {
    recommendations.value = []
    totalCount.value = 0
  } finally {
    loading.value = false
  }
}

const handlePageChange = (newPage: number) => {
  page.value = newPage
  loadRecommendations()
}

const reset = () => {
  searchModel.value = {
    meetingReferenceNo: null,
    fromDate: null,
    toDate: null,
    title: null
  }
  page.value = 1
  loadRecommendations()
}

const canEdit = (recommendation: Recommendation) => {
  // Use loose equality to handle type coercion (owner might be number, id might be string)
  return recommendation.owner == userStore.loggedInUser?.id
}

const openRecommendation = (recommendation: Recommendation, viewMode: boolean) => {
  if (viewMode) {
    router.push({ name: 'recommendation-details', params: { id: recommendation.id }, query: { viewMode: 'true' } })
  } else {
    router.push({ name: 'recommendation-details', params: { id: recommendation.id } })
  }
}

const formatDate = (date: string) => {
  if (!date) return '-'
  return formatDateHelper(new Date(date))
}

const getDaysOverdue = (dueDate: string) => {
  if (!dueDate) return ''
  const now = new Date()
  now.setHours(0, 0, 0, 0)
  const due = new Date(dueDate)
  due.setHours(0, 0, 0, 0)
  const diffDays = Math.floor((now.getTime() - due.getTime()) / (1000 * 60 * 60 * 24))
  if (diffDays === 1) return t('OverdueByOneDay')
  return `${t('OverdueBy')} ${diffDays} ${t('Days')}`
}

const getDaysUntilDue = (dueDate: string) => {
  if (!dueDate) return ''
  const now = new Date()
  now.setHours(0, 0, 0, 0)
  const due = new Date(dueDate)
  due.setHours(0, 0, 0, 0)
  const diffDays = Math.floor((due.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))
  if (diffDays === 0) return t('DueToday')
  if (diffDays === 1) return t('DueTomorrow')
  return `${t('DueIn')} ${diffDays} ${t('Days')}`
}

const filterMyRecommendations = () => {
  // This would ideally filter the main table, but for now we'll add a search param
  // You can enhance this to add an "onlyMine" filter to searchModel
  searchModel.value = {
    meetingReferenceNo: null,
    fromDate: null,
    toDate: null,
    title: null
  }
  // For now, just scroll to the table - in a full implementation you'd add owner filter
  document.querySelector('.data-table')?.scrollIntoView({ behavior: 'smooth' })
}

const truncate = (text: string, length: number) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}

const getStatusClass = (status: string) => {
  const statusLower = (status || '').toLowerCase()
  if (statusLower.includes('complet') || statusLower.includes('مكتمل')) return 'success'
  if (statusLower.includes('progress') || statusLower.includes('تنفيذ')) return 'warning'
  if (statusLower.includes('overdue') || statusLower.includes('متأخر')) return 'error'
  return 'default'
}

// Lifecycle
onMounted(() => {
  loadRecommendations()
})
</script>

<style scoped>
/* Filter Card */
.filter-card {
  @apply bg-white rounded-xl p-5;
  border: 1px solid #e5e7eb;
}

/* Table Section */
.table-section {
  @apply bg-white rounded-xl overflow-hidden;
  border: 1px solid #e5e7eb;
}

.table-toolbar {
  @apply flex items-center justify-end px-5 py-3;
  border-bottom: 1px solid #f3f4f6;
}

.record-count {
  @apply text-sm text-zinc-500 bg-zinc-100 px-3 py-1 rounded-full;
}

/* Filters Row - Aligned Grid */
.filters-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1rem;
  align-items: end;
}

@media (max-width: 1024px) {
  .filters-row {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 640px) {
  .filters-row {
    grid-template-columns: 1fr;
  }
}

.filter-field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.filter-field > label {
  @apply text-xs font-medium text-zinc-600;
}

.filter-field > input {
  @apply w-full px-3 py-2.5 text-sm border border-zinc-300 rounded-lg transition-all;
  @apply focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/10;
  @apply hover:border-zinc-400;
  height: 42px;
}

.filter-field > input::placeholder {
  @apply text-zinc-500;
}

/* DatePicker alignment fix */
.filter-field :deep(.filter-datepicker) {
  width: 100%;
}

.filter-field :deep(.filter-datepicker .form-label) {
  display: none;
}

.filter-field :deep(.filter-datepicker button) {
  height: 42px;
}

/* Buttons */
.btn-primary {
  @apply inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white rounded-lg transition-all;
  background: linear-gradient(135deg, #006d4b 0%, #003423 100%);
}

.btn-primary:hover:not(:disabled) {
  @apply shadow-lg;
  transform: translateY(-1px);
}

.btn-primary:disabled {
  @apply opacity-60 cursor-not-allowed;
}

.btn-outline {
  @apply inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-zinc-700 bg-white border border-gray-200 rounded-lg transition-all;
}

.btn-outline:hover {
  @apply bg-zinc-50 border-zinc-300;
}

/* Data Table */
.data-table {
  @apply w-full;
}

.data-table thead {
  background-color: #006d4b;
}

.data-table th {
  @apply px-5 py-4 text-xs font-semibold uppercase tracking-wider text-start;
  color: #d4d4d8;
}

.data-table tbody tr {
  @apply border-t border-gray-100 transition-colors;
}

.data-table tbody tr:hover {
  background-color: rgba(0, 109, 75, 0.05);
}

.data-table td {
  @apply px-4 py-3;
}

.ref-badge {
  @apply inline-block px-2 py-1 text-xs font-semibold rounded-md;
  background: linear-gradient(135deg, #006d4b 0%, #003423 100%);
  color: white;
}

.truncate-text {
  @apply block max-w-[200px] truncate;
}

/* Rate Card (table cell progress) */
.rate-card {
  background: #f4f8f6;
  border: 1px solid #e4ede8;
  @apply rounded-lg px-3 py-2 min-w-[80px] inline-flex flex-col gap-1.5;
}

.rate-top {
  @apply flex items-center justify-between;
}

.rate-value {
  @apply text-sm font-bold;
  color: #1a2e25;
}

.rate-track {
  @apply h-1.5 rounded-full overflow-hidden;
  background: #e4ede8;
}

.rate-fill {
  @apply h-full rounded-full;
  background: #006d4b;
  transition: width 0.5s ease-out;
}

/* Status Badge */
.status-badge {
  @apply inline-block px-2 py-1 text-xs font-medium rounded-full;
}

.status-badge.success {
  @apply bg-green-100 text-green-700;
}

.status-badge.warning {
  @apply bg-yellow-100 text-yellow-700;
}

.status-badge.error {
  @apply bg-red-100 text-red-700;
}

.status-badge.default {
  @apply bg-zinc-100 text-zinc-600;
}

/* Action Buttons */
.action-buttons {
  @apply flex items-center gap-1;
}

.action-btn {
  @apply w-8 h-8 flex items-center justify-center rounded-lg transition-all;
}

.action-btn svg {
  @apply w-4 h-4;
}

.action-btn.edit {
  @apply text-primary hover:bg-primary/10;
}

.action-btn.view {
  @apply text-zinc-500 hover:bg-zinc-100;
}

/* Empty State */
.empty-state {
  @apply flex flex-col items-center justify-center py-16 text-center;
}

.empty-icon {
  @apply w-16 h-16 flex items-center justify-center bg-zinc-100 rounded-full mb-4;
}

.empty-icon svg {
  @apply w-8 h-8 text-zinc-500;
}

.empty-title {
  @apply text-zinc-900 font-medium mb-1;
}

.empty-subtitle {
  @apply text-zinc-500 text-sm;
}

/* Pagination */
.pagination {
  @apply flex items-center justify-center gap-4 px-5 py-3;
  background: #f8f9fa;
  border-top: 1px solid #e5e7eb;
}

.pagination button {
  @apply w-8 h-8 flex items-center justify-center rounded-lg transition-all text-zinc-500;
  background: #fff;
  border: 1px solid #e5e7eb;
}

.pagination button svg {
  @apply w-4 h-4;
}

.pagination button:hover:not(:disabled) {
  @apply text-zinc-700;
  background: #f4f4f5;
}

.pagination button:disabled {
  @apply opacity-30 cursor-not-allowed;
}

.pagination-info {
  @apply text-[13px] text-zinc-500;
}

/* Stats Card */
.stats-card {
  background: #fff;
  @apply p-5 rounded-xl relative overflow-hidden;
  border: 1px solid #e4ede8;
  border-top: 3px solid #006d4b;
  color: #1a2e25;
}

.stats-card-wave {
  @apply absolute top-0 h-full w-28 opacity-5;
  right: 0;
}

[dir="rtl"] .stats-card-wave {
  right: auto;
  left: 0;
  transform: scaleX(-1);
}

.stats-card-label {
  @apply font-bold uppercase tracking-widest;
  font-size: 10px;
  color: #5a7a6d;
}

.stats-card-value {
  @apply text-4xl font-bold mt-1;
  color: #1a2e25;
}

.stats-card-sub {
  @apply text-xs;
  color: #5a7a6d;
}

.stats-mini-card {
  background: #f4f8f6;
  border-radius: 0.5rem;
  padding: 0.5rem;
  text-align: center;
  border: 1px solid #e4ede8;
  transition: all 0.2s ease;
}

.stats-mini-card:hover {
  background: #eaf2ee;
  border-color: #c8ddd3;
}

.stats-mini-card.warning p:first-child {
  color: #ef4444 !important;
}

/* Average Progress Card */
.avg-progress-card {
  background: #fff;
  @apply p-5 rounded-xl relative overflow-hidden;
  border: 1px solid #e4ede8;
  border-top: 3px solid #006d4b;
  color: #1a2e25;
}

.avg-progress-wave {
  @apply absolute top-0 h-full w-28 opacity-5;
  right: 0;
}

[dir="rtl"] .avg-progress-wave {
  right: auto;
  left: 0;
  transform: scaleX(-1);
}

.avg-progress-label {
  @apply font-bold uppercase tracking-widest;
  font-size: 10px;
  color: #5a7a6d;
}

.avg-progress-value {
  @apply text-4xl font-bold mt-1 mb-3;
  color: #1a2e25;
}

.avg-progress-track {
  @apply h-2 rounded-full overflow-hidden mb-2;
  background: #e4ede8;
  border: 1px solid #d0ddd6;
}

.avg-progress-fill {
  @apply h-full rounded-full;
  background: linear-gradient(90deg, #006d4b, #63a58f);
  transition: width 0.6s ease-out;
}

.avg-progress-sub {
  @apply text-xs;
  color: #5a7a6d;
}

/* Attention Card */
.attention-card {
  @apply bg-white rounded-xl p-4;
  border: 1px solid #e5e7eb;
}

/* Urgent Item (Overdue) */
.urgent-item {
  @apply flex items-center gap-2 p-2 rounded-lg cursor-pointer transition-all;
  background: rgba(239, 68, 68, 0.05);
  border: 1px solid rgba(239, 68, 68, 0.15);
}

.urgent-item:hover {
  background: rgba(239, 68, 68, 0.1);
  border-color: rgba(239, 68, 68, 0.25);
}

.urgent-progress {
  @apply flex-shrink-0 px-2 py-1 rounded text-[10px] font-bold;
  background: rgba(239, 68, 68, 0.1);
  color: #dc2626;
}

/* Due Soon Item */
.due-soon-item {
  @apply flex items-center gap-2 p-2 rounded-lg cursor-pointer transition-all;
  background: rgba(245, 158, 11, 0.05);
  border: 1px solid rgba(245, 158, 11, 0.15);
}

.due-soon-item:hover {
  background: rgba(245, 158, 11, 0.1);
  border-color: rgba(245, 158, 11, 0.25);
}

.due-soon-progress {
  @apply flex-shrink-0 px-2 py-1 rounded text-[10px] font-bold;
  background: rgba(245, 158, 11, 0.1);
  color: #d97706;
}

/* Spinner */
.spinner {
  width: 24px;
  height: 24px;
  border: 3px solid #e8e8e8;
  border-top-color: var(--color-primary, #006d4b);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

.spinner.small {
  width: 16px;
  height: 16px;
  border-width: 2px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* ── MG Progress Cell ── */
.rec-progress-cell {
  display: flex; flex-direction: column; gap: 4px; min-width: 80px;
}
.rec-progress-badge {
  display: inline-block; padding: 2px 8px;
  background: #006d4b; border-radius: 6px;
  font-size: 11px; font-weight: 800; text-align: center;
}
.rec-progress-track {
  width: 100%; height: 4px; background: #e2e8f0; border-radius: 2px; overflow: hidden;
}
.rec-progress-fill {
  height: 100%; border-radius: 2px; transition: width 0.3s ease;
}

/* ── MG toolbar override for count-only ── */
.mg-toolbar { display: flex; align-items: center; justify-content: flex-end; padding: 10px 16px; border-bottom: 1px solid #eaeaea; }
.mg-count { font-size: 13px; color: #6a7a7a; font-weight: 500; }

/* ── Status pills ── */
.mg-pill.in-progress { background: #fffbeb; color: #b45309; }
.mg-pill.inprogress { background: #fffbeb; color: #b45309; }
</style>

<template>
  <div class="space-y-5">
    <!-- Page Header -->
    <PageHeader
      :title="$t('AttendanceReport')"
      :subtitle="$t('AttendanceReportDesc')"
    />

    <!-- Grid Container -->
    <div class="mg-container">
      <!-- Loading -->
      <div v-if="loading" class="mg-state">
        <Icon icon="mdi:loading" class="w-6 h-6 animate-spin" style="color: #008080" />
      </div>

      <template v-else>
        <!-- Search Filters + Stats -->
        <div class="ar-toolbar">
          <div class="ar-filters">
            <div class="ar-filter-field">
              <label>{{ $t('ReferenceNumber') }}</label>
              <input v-model="searchModel.meetingReferenceNo" type="text" :placeholder="$t('ReferenceNumber')" />
            </div>
            <div class="ar-filter-field">
              <label>{{ $t('Title') }}</label>
              <input v-model="searchModel.title" type="text" :placeholder="$t('Title')" />
            </div>
            <div class="ar-filter-field">
              <DatePicker v-model="searchModel.fromDate" :label="$t('FromDate')" />
            </div>
            <div class="ar-filter-field">
              <DatePicker v-model="searchModel.toDate" :label="$t('ToDate')" />
            </div>
            <div class="ar-filter-actions">
              <Button variant="primary" icon-left="mdi:magnify" :loading="loading" @click="search">{{ $t('Search') }}</Button>
              <Button variant="outline" icon-left="mdi:refresh" @click="reset">{{ $t('Reset') }}</Button>
            </div>
          </div>
          <div class="ar-stats">
            <div class="ar-stat"><span class="ar-stat-val">{{ totalCount }}</span><span class="ar-stat-lbl">{{ $t('TotalRecords') }}</span></div>
            <div class="ar-stat"><span class="ar-stat-val ar-green">{{ attendedCount }}</span><span class="ar-stat-lbl">{{ $t('Attended') }}</span></div>
            <div class="ar-stat"><span class="ar-stat-val ar-red">{{ absentCount }}</span><span class="ar-stat-lbl">{{ $t('Absent') }}</span></div>
            <div class="ar-stat"><span class="ar-stat-val ar-teal">{{ attendanceRate }}%</span><span class="ar-stat-lbl">{{ $t('AttendanceRate') }}</span></div>
          </div>
        </div>

        <!-- Table -->
        <template v-if="attendanceItems.length > 0">
          <div class="mg-table-wrap">
            <table class="data-table">
              <thead>
                <tr>
                  <th>{{ $t('Name') }}</th>
                  <th>{{ $t('Title') }}</th>
                  <th>{{ $t('MeetingNumber') }}</th>
                  <th>{{ $t('Date') }}</th>
                  <th>{{ $t('Attended') }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="row in attendanceItems" :key="row.id || row.meetingId + row.userName">
                  <td>
                    <div class="ar-user">
                      <UserAvatar :userId="row.userId" :name="row.userName || ''" size="xs" />
                      <span class="mg-title">{{ row.userName || '-' }}</span>
                    </div>
                  </td>
                  <td><span class="mg-title">{{ truncate(row.title, 40) }}</span></td>
                  <td><span class="mg-ref">#{{ row.meetingId }}</span></td>
                  <td>{{ formatDate(row.meetingDate) }}</td>
                  <td>
                    <span :class="['mg-pill', row.attended ? 'completed' : 'cancelled']">
                      {{ row.attended ? ($t('Present')) : ($t('Absent')) }}
                    </span>
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

        <!-- Empty -->
        <div v-else class="mg-state">
          <Icon icon="mdi:account-off-outline" class="w-12 h-12" style="color: #ccc" />
          <p>{{ $t('NoData') }}</p>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/components/layout/PageHeader.vue'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import DatePicker from '@/components/ui/DatePicker.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import ReportsService from '@/services/ReportsService'

const { t, locale } = useI18n()

// State
const loading = ref(false)
const attendanceItems = ref<any[]>([])
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
const attendedCount = computed(() => {
  if (!Array.isArray(attendanceItems.value)) return 0
  return attendanceItems.value.filter(item => item.attended).length
})
const absentCount = computed(() => {
  if (!Array.isArray(attendanceItems.value)) return 0
  return attendanceItems.value.filter(item => !item.attended).length
})
const attendanceRate = computed(() => {
  if (!Array.isArray(attendanceItems.value) || attendanceItems.value.length === 0) return 0
  return Math.round((attendedCount.value / attendanceItems.value.length) * 100)
})

const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))
const visiblePages = computed(() => {
  const pages: number[] = []
  const s = Math.max(1, page.value - 2)
  const e = Math.min(totalPages.value, s + 4)
  for (let i = s; i <= e; i++) pages.push(i)
  return pages
})

// Methods
const loadAttendanceReport = async () => {
  loading.value = true
  try {
    const response: any = await ReportsService.getAttendanceReport(
      searchModel.value,
      page.value,
      pageSize.value
    )
    const data = response?.data?.data || response?.data || response || []
    const total = response?.data?.total || response?.total || 0
    attendanceItems.value = Array.isArray(data) ? data : []
    totalCount.value = total
  } catch (error) {
    console.error('Failed to load attendance report:', error)
    attendanceItems.value = []
    totalCount.value = 0
  } finally {
    loading.value = false
  }
}

const search = () => {
  page.value = 1
  loadAttendanceReport()
}

const goToPage = (p: number) => {
  page.value = p
  loadAttendanceReport()
}

const reset = () => {
  searchModel.value = { meetingReferenceNo: null, fromDate: null, toDate: null, title: null }
  page.value = 1
  loadAttendanceReport()
}

const formatDate = (date: string) => {
  if (!date) return '-'
  const loc = locale.value === 'ar' ? 'ar-EG' : 'en-US'
  return new Date(date).toLocaleDateString(loc, { year: 'numeric', month: 'short', day: 'numeric', calendar: 'gregory' })
}

const truncate = (text: string, length: number) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}

// Lifecycle
onMounted(() => {
  loadAttendanceReport()
})
</script>

<style scoped>
/* Toolbar with filters + stats */
.ar-toolbar {
  padding: 14px 16px;
  border-bottom: 1px solid #eaeaea;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.ar-filters {
  display: flex;
  align-items: flex-end;
  gap: 12px;
  flex-wrap: wrap;
}

.ar-filter-field {
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex: 1;
  min-width: 140px;
}

.ar-filter-field label {
  font-size: 11px;
  font-weight: 500;
  color: #64748b;
}

.ar-filter-field input {
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 9px 12px;
  font-size: 14px;
  outline: none;
  transition: border-color 0.15s;
}

.ar-filter-field input:focus {
  border-color: #006d4b;
  box-shadow: 0 0 0 2px rgba(0, 109, 75, 0.1);
}

.ar-filter-actions {
  display: flex;
  gap: 6px;
  align-items: flex-end;
  padding-bottom: 1px;
}

/* Inline stats */
.ar-stats {
  display: flex;
  gap: 20px;
  padding: 10px 0 0;
  border-top: 1px solid #f1f5f9;
}

.ar-stat {
  display: flex;
  align-items: baseline;
  gap: 6px;
}

.ar-stat-val {
  font-size: 20px;
  font-weight: 700;
  color: #004730;
}

.ar-stat-val.ar-green { color: #006d4b; }
.ar-stat-val.ar-red { color: #ef4444; }
.ar-stat-val.ar-teal { color: #007E65; }

.ar-stat-lbl {
  font-size: 11px;
  color: #94a3b8;
  font-weight: 500;
}

/* User cell */
.ar-user {
  display: flex;
  align-items: center;
  gap: 8px;
}

@media (max-width: 768px) {
  .ar-filters { flex-direction: column; }
  .ar-stats { flex-wrap: wrap; gap: 12px; }
}
</style>

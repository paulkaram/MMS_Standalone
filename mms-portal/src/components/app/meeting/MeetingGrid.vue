<template>
  <div class="mg-container">
    <!-- Loading -->
    <div v-if="loading" class="mg-state">
      <Icon icon="mdi:loading" class="w-6 h-6 animate-spin" style="color: #008080" />
    </div>

    <!-- Empty -->
    <div v-else-if="meetings.length === 0" class="mg-state">
      <Icon icon="mdi:calendar-blank-outline" class="w-12 h-12" style="color: #ccc" />
      <p>{{ $t('NoMeetingsFound') }}</p>
    </div>

    <!-- Table -->
    <template v-else>
      <div class="mg-table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>{{ $t('Id') }}</th>
              <th>{{ $t('Title') }}</th>
              <th v-if="showCol('status')">{{ $t('Status') }}</th>
              <th v-if="showCol('reference')">{{ $t('ReferenceNumber') }}</th>
              <th>{{ $t('Date') }}</th>
              <th>{{ $t('Time') }}</th>
              <th v-if="showCol('location')">{{ $t('Location') }}</th>
              <th v-if="showCol('creator')">{{ $t('Creator') }}</th>
              <th>{{ $t('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="m in meetings" :key="m.id">
              <td><span class="mg-id">{{ m.id }}</span></td>
              <td><span class="mg-title" :title="m.title || m.titleAr">{{ truncate(m.title || m.titleAr, 40) }}</span></td>
              <td v-if="showCol('status')">
                <span :class="['mg-pill', statusClass(m.statusId)]">{{ m.statusName || statusText(m.statusId) }}</span>
              </td>
              <td v-if="showCol('reference')">
                <span class="mg-ref">{{ m.referenceNumber }}</span>
              </td>
              <td>{{ fmtDate(m.date) }}</td>
              <td class="mg-nowrap">{{ fmtTime(m.startTime) }} – {{ fmtTime(m.endTime) }}</td>
              <td v-if="showCol('location')">{{ m.location || '-' }}</td>
              <td v-if="showCol('creator')">{{ m.createdByName || m.createdBy || m.creatorName || '-' }}</td>
              <td>
                <div class="mg-actions">
                  <slot name="actions" :meeting="m">
                    <button v-if="m.statusId === 1" class="mg-act" :title="$t('Edit')" @click.stop="$emit('edit', m)">
                      <Icon icon="mdi:pencil-outline" class="w-4 h-4" />
                    </button>
                    <button v-if="m.statusId === 1" class="mg-act danger" :title="$t('Delete')" @click.stop="$emit('delete', m)">
                      <Icon icon="mdi:trash-can-outline" class="w-4 h-4" />
                    </button>
                    <button v-if="m.statusId !== 1" class="mg-act" :title="$t('View')" @click.stop="$emit('view', m)">
                      <Icon icon="mdi:eye-outline" class="w-4 h-4" />
                    </button>
                  </slot>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="totalCount > pageSize" class="mg-pag">
        <span class="mg-pag-info">
          {{ $t('Showing') }} <strong>{{ (currentPage - 1) * pageSize + 1 }}</strong>
          {{ $t('To') }} <strong>{{ Math.min(currentPage * pageSize, totalCount) }}</strong>
          {{ $t('Of') }} <strong>{{ totalCount }}</strong>
        </span>
        <div class="mg-pag-btns">
          <button class="mg-pg" :disabled="currentPage === 1" @click="goTo(1)">
            <Icon :icon="isRTL ? 'mdi:chevron-double-right' : 'mdi:chevron-double-left'" class="w-4 h-4" />
          </button>
          <button class="mg-pg" :disabled="currentPage === 1" @click="goTo(currentPage - 1)">
            <Icon :icon="isRTL ? 'mdi:chevron-right' : 'mdi:chevron-left'" class="w-4 h-4" />
          </button>
          <button v-for="p in visiblePages" :key="p" class="mg-pg num" :class="{ active: p === currentPage }" @click="goTo(p)">{{ p }}</button>
          <button class="mg-pg" :disabled="currentPage === totalPages" @click="goTo(currentPage + 1)">
            <Icon :icon="isRTL ? 'mdi:chevron-left' : 'mdi:chevron-right'" class="w-4 h-4" />
          </button>
          <button class="mg-pg" :disabled="currentPage === totalPages" @click="goTo(totalPages)">
            <Icon :icon="isRTL ? 'mdi:chevron-double-left' : 'mdi:chevron-double-right'" class="w-4 h-4" />
          </button>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import { getLocale } from '@/plugins/i18n'
import MeetingsService from '@/services/MeetingsService'

interface Props {
  searchParams?: Record<string, any>
  columns?: string[]
  pageSize?: number
}

const props = withDefaults(defineProps<Props>(), {
  searchParams: () => ({}),
  columns: () => ['status', 'reference', 'location'],
  pageSize: 10
})

defineEmits<{
  (e: 'view', m: any): void
  (e: 'edit', m: any): void
  (e: 'delete', m: any): void
}>()

const isRTL = computed(() => getLocale() === 'ar')
const loading = ref(false)
const meetings = ref<any[]>([])
const totalCount = ref(0)
const currentPage = ref(1)
const totalPages = computed(() => Math.ceil(totalCount.value / props.pageSize))
const visiblePages = computed(() => {
  const pages: number[] = []
  const s = Math.max(1, currentPage.value - 2)
  const e = Math.min(totalPages.value, s + 4)
  for (let i = s; i <= e; i++) pages.push(i)
  return pages
})

const showCol = (c: string) => props.columns.includes(c)

async function load() {
  loading.value = true
  try {
    const defaults = {
      statusId: null, committeeId: null, meetingId: null,
      fromDate: null, toDate: null, location: null, title: null,
      onlyMyMeetings: true
    }
    const params = { ...defaults, ...props.searchParams }
    const response: any = await MeetingsService.searchUserMeetings(params, currentPage.value, props.pageSize)
    const result = response?.data || response
    meetings.value = result?.data || []
    totalCount.value = result?.total || 0
  } catch {
    meetings.value = []
    totalCount.value = 0
  } finally {
    loading.value = false
  }
}

function goTo(p: number) {
  currentPage.value = p
  load()
}

function reload() {
  currentPage.value = 1
  load()
}

watch(() => props.searchParams, () => reload(), { deep: true })

onMounted(() => load())

// Expose reload for parent
defineExpose({ reload })

// Formatters
function fmtDate(date: string) {
  if (!date) return '-'
  try {
    const loc = getLocale() === 'ar' ? 'ar-EG' : 'en-US'
    return new Date(date).toLocaleDateString(loc, { year: 'numeric', month: 'short', day: 'numeric', calendar: 'gregory' })
  } catch { return date }
}

function fmtTime(time: string) {
  if (!time) return '-'
  try {
    const [h, m] = time.split(':').map(Number)
    const ar = getLocale() === 'ar'
    const p = h >= 12 ? (ar ? 'م' : 'PM') : (ar ? 'ص' : 'AM')
    return `${h % 12 || 12}:${m.toString().padStart(2, '0')} ${p}`
  } catch { return time }
}

function truncate(t: string, n: number) {
  if (!t) return '-'
  return t.length > n ? t.substring(0, n) + '...' : t
}

function statusClass(id: number) {
  const m: Record<number, string> = { 1:'draft', 2:'approved', 3:'started', 4:'completed', 5:'cancelled', 6:'pending-mom', 7:'approved-mom', 8:'pending-sign', 9:'signed' }
  return m[id] || 'draft'
}

function statusText(id: number) {
  const m: Record<number, string> = { 1:'Draft', 2:'Approved', 3:'Started', 4:'Finished', 5:'Cancelled', 6:'Pending MOM', 7:'MOM Approved', 8:'Pending Sign', 9:'Signed' }
  return m[id] || 'Draft'
}
</script>

<style scoped>
/* MeetingGrid-specific (non-shared) */
.mg-nowrap { white-space: nowrap; }
</style>

<template>
  <div class="data-table-wrapper">
    <!-- Header slot -->
    <div
      v-if="$slots.header || title"
      class="flex items-center justify-between gap-4 mb-4"
    >
      <div>
        <h3
          v-if="title"
          class="text-lg font-semibold text-zinc-900"
        >
          {{ title }}
        </h3>
        <p
          v-if="subtitle"
          class="text-sm text-zinc-500 mt-0.5"
        >
          {{ subtitle }}
        </p>
      </div>

      <div class="flex items-center gap-2">
        <!-- Search -->
        <div
          v-if="searchable"
          class="relative"
        >
          <input
            v-model="searchQuery"
            type="text"
            :placeholder="searchPlaceholder"
            class="w-64 px-4 py-2 ps-10 text-sm border border-gray-300 rounded-lg focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/20"
            @input="handleSearch"
          >
          <Icon
            icon="mdi:magnify"
            class="absolute start-3 top-1/2 -translate-y-1/2 w-5 h-5 text-zinc-400"
          />
        </div>

        <!-- Header actions -->
        <slot name="header" />
      </div>
    </div>

    <!-- PrimeVue DataTable -->
    <PrimeDataTable
      v-model:selection="selectedRows"
      :value="data"
      :loading="loading"
      :paginator="false"
      :rows="pageSize"
      :total-records="totalRecords"
      :rows-per-page-options="rowsPerPageOptions"
      :lazy="serverSide"
      :selection-mode="selectionMode"
      :data-key="dataKey"
      :row-hover="rowHover"
      :striped-rows="striped"
      :show-gridlines="gridLines"
      :removable-sort="removableSort"
      :scrollable="scrollable"
      :scroll-height="scrollHeight"
      :responsive-layout="responsiveLayout"
      :class="tableClasses"
      @page="onPage"
      @sort="onSort as any"
      @row-click="onRowClick"
      @row-select="onRowSelect"
      @row-unselect="onRowUnselect"
    >
      <!-- Empty message -->
      <template #empty>
        <div class="flex flex-col items-center justify-center py-12 text-zinc-500">
          <Icon
            icon="mdi:database-off-outline"
            class="w-16 h-16 mb-4 text-zinc-300"
          />
          <p class="text-lg font-medium">{{ emptyMessage }}</p>
          <p
            v-if="emptySubMessage"
            class="text-sm mt-1"
          >
            {{ emptySubMessage }}
          </p>
        </div>
      </template>

      <!-- Loading overlay -->
      <template #loading>
        <div class="flex items-center justify-center py-12">
          <div class="relative w-12 h-12">
            <div class="absolute inset-0 border-4 border-zinc-200 rounded-full" />
            <div class="absolute inset-0 border-4 border-primary rounded-full animate-spin border-t-transparent" />
          </div>
        </div>
      </template>

      <!-- Dynamic columns -->
      <PrimeColumn
        v-for="col in columns"
        :key="col.field || col.key"
        :field="col.field || col.key"
        :header="col.header || col.label"
        :sortable="col.sortable !== false"
        :style="col.style"
        :class="col.class"
        :header-style="col.headerStyle"
        :body-style="col.bodyStyle"
      >
        <template
          v-if="col.body || $slots[`body-${col.field || col.key}`]"
          #body="slotProps"
        >
          <slot
            :name="`body-${col.field || col.key}`"
            v-bind="slotProps"
          >
            <component
              :is="col.body"
              v-if="col.body"
              v-bind="slotProps"
            />
          </slot>
        </template>

        <template
          v-if="col.headerTemplate || $slots[`header-${col.field || col.key}`]"
          #header="slotProps"
        >
          <slot
            :name="`header-${col.field || col.key}`"
            v-bind="slotProps"
          >
            <component
              :is="col.headerTemplate"
              v-if="col.headerTemplate"
              v-bind="slotProps"
            />
          </slot>
        </template>
      </PrimeColumn>

      <!-- Selection column -->
      <PrimeColumn
        v-if="selectionMode"
        selection-mode="multiple"
        style="width: 3rem"
        :exportable="false"
      />

      <!-- Actions column -->
      <PrimeColumn
        v-if="$slots.actions"
        :header="actionsHeader"
        :style="actionsStyle"
        :exportable="false"
      >
        <template #body="slotProps">
          <slot
            name="actions"
            v-bind="slotProps"
          />
        </template>
      </PrimeColumn>
    </PrimeDataTable>

    <!-- Custom Pagination (matching MyMeetings style) -->
    <div v-if="paginator && totalRecords > 0" class="pagination">
      <div class="pagination-info">
        عرض
        <strong>{{ paginationFirst }}</strong>
        إلى
        <strong>{{ paginationLast }}</strong>
        من
        <strong>{{ totalRecords }}</strong>
        سجل
      </div>
      <div class="pagination-controls">
        <button
          class="page-btn"
          :disabled="currentPage === 1"
          @click="goToPage(1)"
        >
          <Icon icon="mdi:chevron-double-right" class="w-4 h-4" />
        </button>
        <button
          class="page-btn"
          :disabled="currentPage === 1"
          @click="goToPage(currentPage - 1)"
        >
          <Icon icon="mdi:chevron-right" class="w-4 h-4" />
        </button>
        <div class="page-numbers">
          <button
            v-for="page in visiblePages"
            :key="page"
            class="page-num"
            :class="{ active: page === currentPage }"
            @click="goToPage(page)"
          >
            {{ page }}
          </button>
        </div>
        <button
          class="page-btn"
          :disabled="currentPage === totalPages"
          @click="goToPage(currentPage + 1)"
        >
          <Icon icon="mdi:chevron-left" class="w-4 h-4" />
        </button>
        <button
          class="page-btn"
          :disabled="currentPage === totalPages"
          @click="goToPage(totalPages)"
        >
          <Icon icon="mdi:chevron-double-left" class="w-4 h-4" />
        </button>
      </div>
      <div class="page-size-selector">
        <div class="page-select-wrapper">
          <select v-model="internalPageSize" class="page-size-select" @change="onPageSizeChange">
            <option v-for="opt in rowsPerPageOptions" :key="opt" :value="opt">{{ opt }}</option>
          </select>
          <Icon icon="mdi:chevron-down" class="select-icon" />
        </div>
        <span class="page-size-label">لكل صفحة</span>
      </div>
    </div>

    <!-- Footer slot -->
    <div
      v-if="$slots.footer"
      class="mt-4"
    >
      <slot name="footer" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import PrimeDataTable from 'primevue/datatable'
import PrimeColumn from 'primevue/column'
import Icon from '@/components/ui/Icon.vue'
import { useDebounceFn } from '@vueuse/core'

// Types
export interface ColumnDef {
  field?: string
  header?: string
  // Allow alternative property names for backwards compatibility
  key?: string
  label?: string
  sortable?: boolean
  style?: string | object
  class?: string
  headerStyle?: string | object
  bodyStyle?: string | object
  body?: any
  headerTemplate?: any
  width?: string
}

interface PageEvent {
  page: number
  rows: number
  first: number
}

interface SortEvent {
  sortField: string | undefined
  sortOrder: number
}

// Props
interface Props {
  data: any[]
  columns: ColumnDef[]
  title?: string
  subtitle?: string
  loading?: boolean
  paginator?: boolean
  serverSide?: boolean
  page?: number
  pageSize?: number
  totalRecords?: number
  rowsPerPageOptions?: number[]
  selectionMode?: 'single' | 'multiple' | undefined
  dataKey?: string
  rowHover?: boolean
  striped?: boolean
  gridLines?: boolean
  removableSort?: boolean
  scrollable?: boolean
  scrollHeight?: string
  responsiveLayout?: 'scroll' | 'stack'
  searchable?: boolean
  searchPlaceholder?: string
  emptyMessage?: string
  emptySubMessage?: string
  actionsHeader?: string
  actionsStyle?: string | object
}

const props = withDefaults(defineProps<Props>(), {
  title: undefined,
  subtitle: undefined,
  loading: false,
  paginator: true,
  serverSide: true,
  page: 1,
  pageSize: 10,
  totalRecords: 0,
  rowsPerPageOptions: () => [10, 25, 50, 100],
  selectionMode: undefined,
  dataKey: 'id',
  rowHover: true,
  striped: false,
  gridLines: false,
  removableSort: true,
  scrollable: false,
  scrollHeight: undefined,
  responsiveLayout: 'scroll',
  searchable: true,
  searchPlaceholder: 'بحث...',
  emptyMessage: 'لا توجد بيانات',
  emptySubMessage: '',
  actionsHeader: 'الإجراءات',
  actionsStyle: () => ({ width: '120px' })
})

// Emits
const emit = defineEmits<{
  page: [event: PageEvent]
  sort: [event: SortEvent]
  search: [query: string]
  'row-click': [event: { data: any; index: number }]
  'row-select': [event: { data: any }]
  'row-unselect': [event: { data: any }]
  'update:selection': [value: any[]]
}>()

// State
const selectedRows = ref<any[]>([])
const searchQuery = ref('')
const currentPage = ref(1)
const internalPageSize = ref(props.pageSize)

// Table classes
const tableClasses = computed(() => {
  return [
    'p-datatable-sm',
    props.striped ? 'p-datatable-striped' : '',
    props.gridLines ? 'p-datatable-gridlines' : ''
  ].filter(Boolean).join(' ')
})

// Current page report text (Arabic)
const currentPageReportText = computed(() => {
  return 'عرض {first} إلى {last} من {totalRecords} سجل'
})

// Pagination computed properties
const totalPages = computed(() => Math.ceil(props.totalRecords / internalPageSize.value) || 1)

const visiblePages = computed(() => {
  const pages: number[] = []
  const total = totalPages.value
  const current = currentPage.value

  let start = Math.max(1, current - 2)
  let end = Math.min(total, start + 4)

  if (end - start < 4) {
    start = Math.max(1, end - 4)
  }

  for (let i = start; i <= end; i++) {
    pages.push(i)
  }

  return pages
})

const paginationFirst = computed(() => {
  return (currentPage.value - 1) * internalPageSize.value + 1
})

const paginationLast = computed(() => {
  return Math.min(currentPage.value * internalPageSize.value, props.totalRecords)
})

// Debounced search
const handleSearch = useDebounceFn(() => {
  emit('search', searchQuery.value)
}, 300)

// Event handlers
function onPage(event: PageEvent) {
  emit('page', {
    page: event.page,
    rows: event.rows,
    first: event.first
  })
}

function onSort(event: SortEvent) {
  emit('sort', {
    sortField: event.sortField,
    sortOrder: event.sortOrder
  })
}

function onRowClick(event: { data: any; index: number }) {
  emit('row-click', event)
}

function onRowSelect(event: { data: any }) {
  emit('row-select', event)
  emit('update:selection', selectedRows.value)
}

function onRowUnselect(event: { data: any }) {
  emit('row-unselect', event)
  emit('update:selection', selectedRows.value)
}

// Pagination methods
function goToPage(page: number) {
  if (page < 1 || page > totalPages.value) return
  currentPage.value = page
  emit('page', {
    page: page - 1, // PrimeVue uses 0-indexed pages
    rows: internalPageSize.value,
    first: (page - 1) * internalPageSize.value
  })
}

function onPageSizeChange() {
  currentPage.value = 1
  emit('page', {
    page: 0,
    rows: internalPageSize.value,
    first: 0
  })
}

// Watch for external selection changes
watch(
  () => props.data,
  () => {
    // Clear selection when data changes
    selectedRows.value = []
  }
)

// Expose methods
function clearSelection() {
  selectedRows.value = []
}

function getSelection() {
  return selectedRows.value
}

defineExpose({
  clearSelection,
  getSelection
})
</script>

<style scoped>
/* ===== Table Container ===== */
.data-table-wrapper :deep(.p-datatable) {
  border: none;
  border-radius: 0;
  overflow: hidden;
}

.data-table-wrapper :deep(.p-datatable-header) {
  @apply bg-white border-b border-gray-200 px-4 py-3;
}

.data-table-wrapper :deep(.p-datatable-thead > tr > th) {
  background-color: #006d4b;
  color: #d4d4d8;
  @apply font-semibold text-xs uppercase tracking-wider border-b border-gray-200 px-5 py-4;
}

.data-table-wrapper :deep(.p-datatable-tbody > tr) {
  @apply border-b border-gray-100 transition-colors;
}

.data-table-wrapper :deep(.p-datatable-tbody > tr:last-child) {
  border-bottom: none;
}

.data-table-wrapper :deep(.p-datatable-tbody > tr:hover) {
  background-color: rgba(0, 109, 75, 0.04);
}

.data-table-wrapper :deep(.p-datatable-tbody > tr > td) {
  @apply text-sm text-gray-700 px-5 py-3.5;
}

/* ===== Pagination Footer ===== */
.pagination {
  @apply flex flex-wrap items-center justify-between gap-4 px-5 py-3;
  background: #f8f9fa;
  border-top: 1px solid #e5e7eb;
}

.pagination-info {
  @apply text-[13px] text-zinc-500;
}

.pagination-info strong {
  @apply text-zinc-700 font-semibold;
}

.pagination-controls {
  @apply flex items-center gap-0.5;
  background: #fff;
  padding: 3px;
  border-radius: 10px;
  border: 1px solid #e5e7eb;
}

.page-btn {
  @apply w-8 h-8 rounded-lg flex items-center justify-center text-zinc-400 transition-all duration-150;
}

.page-btn:hover:not(:disabled) {
  @apply text-zinc-700;
  background: #f4f4f5;
}

.page-btn:disabled {
  @apply opacity-30 cursor-not-allowed;
}

.page-numbers {
  @apply flex items-center gap-0.5;
}

.page-num {
  @apply w-8 h-8 rounded-lg flex items-center justify-center text-sm font-medium text-zinc-500 transition-all duration-150;
}

.page-num:hover:not(.active) {
  @apply text-zinc-700;
  background: #f4f4f5;
}

.page-num.active {
  @apply text-white font-semibold;
  background: #006d4b;
}

.page-size-selector {
  @apply flex items-center gap-2;
}

.page-size-selector .page-select-wrapper {
  @apply relative inline-flex;
}

.page-size-selector .page-size-select {
  @apply appearance-none px-3 py-1.5 pe-8 border border-gray-200 rounded-lg text-sm bg-white transition-all duration-150 cursor-pointer font-medium text-gray-600;
  @apply hover:border-gray-300;
  @apply focus:outline-none focus:ring-2 focus:ring-zinc-200 focus:border-zinc-400;
  min-width: 64px;
}

.page-size-selector .page-select-wrapper .select-icon {
  @apply absolute top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-zinc-400 pointer-events-none;
  right: 8px;
}

[dir="rtl"] .page-size-selector .page-select-wrapper .select-icon {
  right: auto;
  left: 8px;
}

.page-size-selector .page-size-label {
  @apply text-[13px] text-zinc-400;
}

/* RTL support */
[dir="rtl"] .data-table-wrapper :deep(.p-sortable-column-icon) {
  @apply ms-2 me-0;
}

/* Responsive */
@media (max-width: 768px) {
  .pagination {
    @apply flex-col items-center gap-3 py-4;
  }
}
</style>

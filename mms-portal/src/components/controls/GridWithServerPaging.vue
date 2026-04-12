<template>
  <div class="grid-with-server-paging">
    <!-- Header -->
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
      </div>

      <div class="flex items-center gap-2">
        <!-- Search -->
        <div
          v-if="searchable"
          class="relative"
        >
          <input
            v-model="searchText"
            type="text"
            :placeholder="searchPlaceholder"
            class="w-64 px-4 py-2 ps-10 text-sm border border-zinc-200 rounded-lg focus:outline-none focus:border-primary"
            @keyup.enter="handleSearch"
          >
          <Icon
            icon="mdi:magnify"
            class="absolute start-3 top-1/2 -translate-y-1/2 w-5 h-5 text-zinc-400 cursor-pointer"
            @click="handleSearch"
          />
        </div>

        <!-- Header slot -->
        <slot name="header" />

        <!-- Add button -->
        <Button
          v-if="showAddButton"
          variant="primary"
          icon-left="mdi:plus"
          @click="$emit('add')"
        >
          {{ addButtonText }}
        </Button>
      </div>
    </div>

    <!-- Card wrapper -->
    <Card
      variant="outlined"
      :padding="'none'"
    >
      <!-- Loading overlay -->
      <div
        v-if="loading"
        class="absolute inset-0 bg-white/50 z-10 flex items-center justify-center"
      >
        <div class="relative w-10 h-10">
          <div class="absolute inset-0 border-4 border-zinc-200 rounded-full" />
          <div class="absolute inset-0 border-4 border-primary rounded-full animate-spin border-t-transparent" />
        </div>
      </div>

      <!-- Table -->
      <table class="data-table w-full">
        <thead>
          <tr>
            <!-- Selection header -->
            <th
              v-if="selectable"
              class="w-12"
            >
              <input
                type="checkbox"
                class="rounded border-zinc-300 text-primary focus:ring-primary"
                :checked="isAllSelected"
                @change="toggleSelectAll"
              >
            </th>

            <!-- Dynamic headers -->
            <th
              v-for="header in headers"
              :key="header.value"
              :style="header.width ? { width: header.width } : {}"
              :class="[
                header.sortable !== false ? 'cursor-pointer hover:bg-white/5' : '',
                header.align ? `text-${header.align}` : ''
              ]"
              @click="header.sortable !== false && handleSort(header.value)"
            >
              <div class="flex items-center gap-1">
                {{ header.text }}
                <Icon
                  v-if="header.sortable !== false && sortBy === header.value"
                  :icon="sortDesc ? 'mdi:arrow-down' : 'mdi:arrow-up'"
                  class="w-4 h-4"
                />
              </div>
            </th>

            <!-- Actions header -->
            <th
              v-if="$slots.actions"
              :style="{ width: actionsWidth }"
            >
              {{ actionsText }}
            </th>
          </tr>
        </thead>

        <tbody>
          <!-- Empty state -->
          <tr v-if="!loading && items.length === 0">
            <td
              :colspan="totalColumns"
              class="text-center py-12"
            >
              <div class="flex flex-col items-center text-zinc-500">
                <Icon
                  icon="mdi:database-off-outline"
                  class="w-16 h-16 mb-4 text-zinc-300"
                />
                <p class="text-lg font-medium">{{ emptyText }}</p>
              </div>
            </td>
          </tr>

          <!-- Data rows -->
          <tr
            v-for="(item, index) in items"
            :key="getItemKey(item, index)"
            :class="[
              'hover:bg-zinc-50/50 transition-colors',
              isSelected(item) ? 'bg-primary/5' : ''
            ]"
            @click="handleRowClick(item)"
          >
            <!-- Selection checkbox -->
            <td v-if="selectable">
              <input
                type="checkbox"
                class="rounded border-zinc-300 text-primary focus:ring-primary"
                :checked="isSelected(item)"
                @click.stop
                @change="toggleSelect(item)"
              >
            </td>

            <!-- Dynamic cells -->
            <td
              v-for="header in headers"
              :key="header.value"
              :class="header.align ? `text-${header.align}` : ''"
            >
              <slot
                :name="`item.${header.value}`"
                :item="item"
                :value="getNestedValue(item, header.value)"
              >
                {{ getNestedValue(item, header.value) }}
              </slot>
            </td>

            <!-- Actions cell -->
            <td
              v-if="$slots.actions"
              @click.stop
            >
              <slot
                name="actions"
                :item="item"
                :index="index"
              />
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Pagination -->
      <div
        v-if="showPagination && totalItems > 0"
        class="grid-pagination"
      >
        <!-- Items info -->
        <div class="gp-info">
          عرض <strong>{{ startItem }}</strong> إلى <strong>{{ endItem }}</strong> من <strong>{{ totalItems }}</strong> سجل
        </div>

        <!-- Page navigation -->
        <div class="gp-controls">
          <button
            type="button"
            class="gp-btn"
            :disabled="currentPage <= 1"
            @click="goToPage(1)"
          >
            <Icon icon="mdi:chevron-double-right" class="w-4 h-4" />
          </button>
          <button
            type="button"
            class="gp-btn"
            :disabled="currentPage <= 1"
            @click="goToPage(currentPage - 1)"
          >
            <Icon icon="mdi:chevron-right" class="w-4 h-4" />
          </button>

          <template
            v-for="pg in visiblePages"
            :key="pg"
          >
            <button
              v-if="pg !== '...'"
              type="button"
              :class="['gp-num', currentPage === pg ? 'active' : '']"
              @click="goToPage(pg as number)"
            >
              {{ pg }}
            </button>
            <span v-else class="px-1 text-zinc-400">...</span>
          </template>

          <button
            type="button"
            class="gp-btn"
            :disabled="currentPage >= totalPages"
            @click="goToPage(currentPage + 1)"
          >
            <Icon icon="mdi:chevron-left" class="w-4 h-4" />
          </button>
          <button
            type="button"
            class="gp-btn"
            :disabled="currentPage >= totalPages"
            @click="goToPage(totalPages)"
          >
            <Icon icon="mdi:chevron-double-left" class="w-4 h-4" />
          </button>
        </div>

        <!-- Page size selector -->
        <div class="gp-size">
          <div class="gp-select-wrap">
            <select v-model="currentPageSize" class="gp-select" @change="handlePageSizeChange">
              <option v-for="opt in pageSizeOptions" :key="opt" :value="opt">{{ opt }}</option>
            </select>
            <Icon icon="mdi:chevron-down" class="gp-select-icon" />
          </div>
          <span class="gp-size-label">لكل صفحة</span>
        </div>
      </div>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import { useUserStore } from '@/stores/user'
import Card from '@/components/ui/Card.vue'
import Button from '@/components/ui/Button.vue'

// Types
interface Header {
  text: string
  value: string
  sortable?: boolean
  width?: string
  align?: 'start' | 'center' | 'end'
}

// Props
interface Props {
  items: any[]
  headers: Header[]
  title?: string
  loading?: boolean
  totalItems?: number
  pageSize?: number
  pageSizeOptions?: number[]
  itemKey?: string
  searchable?: boolean
  searchPlaceholder?: string
  selectable?: boolean
  showAddButton?: boolean
  addButtonText?: string
  actionsText?: string
  actionsWidth?: string
  emptyText?: string
  showPagination?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  title: undefined,
  loading: false,
  totalItems: 0,
  pageSize: 10,
  pageSizeOptions: () => [10, 25, 50, 100],
  itemKey: 'id',
  searchable: true,
  searchPlaceholder: 'بحث...',
  selectable: false,
  showAddButton: false,
  addButtonText: 'إضافة',
  actionsText: 'الإجراءات',
  actionsWidth: '120px',
  emptyText: 'لا توجد بيانات',
  showPagination: true
})

// Emits
const emit = defineEmits<{
  load: [params: { page: number; pageSize: number; sortBy: string; sortDesc: boolean; search: string }]
  add: []
  'row-click': [item: any]
  'selection-change': [items: any[]]
}>()

// Store
const userStore = useUserStore()
const isRtl = computed(() => userStore.isRtl)

// State
const currentPage = ref(1)
const currentPageSize = ref(props.pageSize)
const sortBy = ref('')
const sortDesc = ref(false)
const searchText = ref('')
const selectedItems = ref<any[]>([])

// Computed
const totalPages = computed(() => {
  return Math.ceil(props.totalItems / currentPageSize.value) || 1
})

const startItem = computed(() => {
  return (currentPage.value - 1) * currentPageSize.value + 1
})

const endItem = computed(() => {
  return Math.min(currentPage.value * currentPageSize.value, props.totalItems)
})

const totalColumns = computed(() => {
  let count = props.headers.length
  if (props.selectable) count++
  // Check if actions slot exists via template
  count++ // Assuming actions column exists
  return count
})

const isAllSelected = computed(() => {
  return props.items.length > 0 && selectedItems.value.length === props.items.length
})

const visiblePages = computed(() => {
  const pages: (number | string)[] = []
  const total = totalPages.value
  const current = currentPage.value

  if (total <= 7) {
    for (let i = 1; i <= total; i++) {
      pages.push(i)
    }
  } else {
    pages.push(1)

    if (current > 3) {
      pages.push('...')
    }

    const start = Math.max(2, current - 1)
    const end = Math.min(total - 1, current + 1)

    for (let i = start; i <= end; i++) {
      pages.push(i)
    }

    if (current < total - 2) {
      pages.push('...')
    }

    pages.push(total)
  }

  return pages
})

// Methods
function getItemKey(item: any, index: number): string | number {
  return item[props.itemKey] ?? index
}

function getNestedValue(obj: any, path: string): any {
  return path.split('.').reduce((acc, part) => acc?.[part], obj)
}

function handleSort(field: string) {
  if (sortBy.value === field) {
    sortDesc.value = !sortDesc.value
  } else {
    sortBy.value = field
    sortDesc.value = false
  }
  loadData()
}

function handleSearch() {
  currentPage.value = 1
  loadData()
}

function handlePageSizeChange() {
  currentPage.value = 1
  loadData()
}

function goToPage(page: number) {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    loadData()
  }
}

function handleRowClick(item: any) {
  emit('row-click', item)
}

function isSelected(item: any): boolean {
  const key = item[props.itemKey]
  return selectedItems.value.some(i => i[props.itemKey] === key)
}

function toggleSelect(item: any) {
  const key = item[props.itemKey]
  const index = selectedItems.value.findIndex(i => i[props.itemKey] === key)

  if (index >= 0) {
    selectedItems.value.splice(index, 1)
  } else {
    selectedItems.value.push(item)
  }

  emit('selection-change', selectedItems.value)
}

function toggleSelectAll() {
  if (isAllSelected.value) {
    selectedItems.value = []
  } else {
    selectedItems.value = [...props.items]
  }

  emit('selection-change', selectedItems.value)
}

function loadData() {
  emit('load', {
    page: currentPage.value,
    pageSize: currentPageSize.value,
    sortBy: sortBy.value,
    sortDesc: sortDesc.value,
    search: searchText.value
  })
}

function refresh() {
  loadData()
}

function clearSelection() {
  selectedItems.value = []
}

// Watch pageSize prop changes
watch(
  () => props.pageSize,
  (newSize) => {
    currentPageSize.value = newSize
  }
)

// Expose methods
defineExpose({
  refresh,
  clearSelection,
  getSelection: () => selectedItems.value
})
</script>

<style scoped>
/* ===== Table Styling ===== */
.data-table {
  border-collapse: collapse;
}

.data-table thead th {
  background-color: #006d4b;
  color: #d4d4d8;
  @apply font-semibold text-xs uppercase tracking-wider px-5 py-4 text-start;
}

.data-table tbody tr {
  @apply border-b border-gray-100 transition-colors;
}

.data-table tbody tr:last-child {
  border-bottom: none;
}

.data-table tbody tr:hover {
  background-color: rgba(0, 109, 75, 0.04);
}

.data-table tbody td {
  @apply text-sm text-gray-700 px-5 py-3.5;
}

/* ===== Pagination ===== */
.grid-pagination {
  @apply flex flex-wrap items-center justify-between gap-4 px-5 py-3;
  background: #f8f9fa;
  border-top: 1px solid #e5e7eb;
}

.gp-info {
  @apply text-[13px] text-zinc-500;
}

.gp-info strong {
  @apply text-zinc-700 font-semibold;
}

.gp-controls {
  @apply flex items-center gap-0.5;
  background: #fff;
  padding: 3px;
  border-radius: 10px;
  border: 1px solid #e5e7eb;
}

.gp-btn {
  @apply w-8 h-8 rounded-lg flex items-center justify-center text-zinc-400 transition-all duration-150;
}

.gp-btn:hover:not(:disabled) {
  @apply text-zinc-700;
  background: #f4f4f5;
}

.gp-btn:disabled {
  @apply opacity-30 cursor-not-allowed;
}

.gp-num {
  @apply w-8 h-8 rounded-lg flex items-center justify-center text-sm font-medium text-zinc-500 transition-all duration-150;
}

.gp-num:hover:not(.active) {
  @apply text-zinc-700;
  background: #f4f4f5;
}

.gp-num.active {
  @apply text-white font-semibold;
  background: #006d4b;
}

.gp-size {
  @apply flex items-center gap-2;
}

.gp-select-wrap {
  @apply relative inline-flex;
}

.gp-select {
  @apply appearance-none px-3 py-1.5 pe-8 border border-gray-200 rounded-lg text-sm bg-white transition-all duration-150 cursor-pointer font-medium text-gray-600;
  @apply hover:border-gray-300;
  @apply focus:outline-none focus:ring-2 focus:ring-zinc-200 focus:border-zinc-400;
  min-width: 64px;
}

.gp-select-icon {
  @apply absolute top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-zinc-400 pointer-events-none;
  right: 8px;
}

[dir="rtl"] .gp-select-icon {
  right: auto;
  left: 8px;
}

.gp-size-label {
  @apply text-[13px] text-zinc-400;
}

@media (max-width: 768px) {
  .grid-pagination {
    @apply flex-col items-center gap-3 py-4;
  }
}
</style>

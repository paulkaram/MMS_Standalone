<template>
  <aside class="sidebar-panel">
    <div class="sidebar-header">
      <h3 class="sidebar-title">
        <Icon icon="mdi:file-tree" class="w-5 h-5" />
        {{ $t('OrganizationStructure') }}
      </h3>
      <label class="filter-toggle">
        <span class="filter-label">{{ $t('Inactive') }}</span>
        <input
          type="checkbox"
          v-model="showInactive"
          class="toggle-input"
        />
        <span class="toggle-slider-sm"></span>
      </label>
    </div>

    <!-- Search -->
    <div class="sidebar-search">
      <div class="search-input-wrapper">
        <Icon icon="mdi:magnify" class="search-icon" />
        <input
          v-model="searchQuery"
          type="text"
          class="search-input"
          :placeholder="$t('Search')"
        />
        <button
          v-if="searchQuery"
          type="button"
          class="clear-btn"
          @click="searchQuery = ''"
        >
          <Icon icon="mdi:close" class="w-4 h-4" />
        </button>
      </div>
    </div>

    <!-- Tree View -->
    <div class="sidebar-tree">
      <div v-if="loading" class="tree-loading">
        <Icon icon="mdi:loading" class="w-6 h-6 animate-spin text-primary" />
      </div>
      <template v-else>
        <div
          v-for="council in filteredTree"
          :key="council.id"
          class="tree-node"
        >
          <TreeNodeItem
            :item="council"
            :selected-id="selectedId"
            :expanded-ids="expandedIds"
            :level="0"
            :checkbox-mode="checkboxMode"
            :checked-ids="internalCheckedIds"
            @select="handleNodeSelect"
            @toggle="toggleExpand"
            @check="handleCheck"
          />
        </div>
        <div v-if="filteredTree.length === 0" class="tree-empty">
          <Icon icon="mdi:folder-search" class="w-10 h-10 text-zinc-300 mb-2" />
          <p class="text-sm text-zinc-400">{{ $t('NoResults') }}</p>
        </div>
      </template>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, h, defineComponent } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'

// Types
interface TreeItem {
  id: string
  name: string
  typeId: number
  isActive?: boolean
  children?: TreeItem[]
}

// Props
const props = withDefaults(defineProps<{
  selectedId?: string | null
  checkboxMode?: boolean
  checkedIds?: number[]
  useMyOrganization?: boolean
}>(), {
  selectedId: null,
  checkboxMode: false,
  checkedIds: () => [],
  useMyOrganization: false
})

// Emits
const emit = defineEmits<{
  (e: 'select', item: TreeItem): void
  (e: 'update:checkedIds', ids: number[]): void
}>()

// Internal checked ids state (synced with prop)
const internalCheckedIds = ref<string[]>([])

// Sync prop -> internal
watch(() => props.checkedIds, (newIds) => {
  internalCheckedIds.value = newIds.map(String)
}, { immediate: true })

// Helper: get all descendant IDs (non-root, numeric IDs only)
const getAllDescendantIds = (item: TreeItem): string[] => {
  let ids: string[] = []
  if (item.children) {
    for (const child of item.children) {
      if (!child.id.startsWith('root-')) {
        ids.push(child.id)
      }
      ids = ids.concat(getAllDescendantIds(child))
    }
  }
  return ids
}

// Helper: find node in tree
const findNodeInTree = (nodes: TreeItem[], id: string): TreeItem | null => {
  for (const node of nodes) {
    if (node.id === id) return node
    if (node.children) {
      const found = findNodeInTree(node.children, id)
      if (found) return found
    }
  }
  return null
}

// Handle checkbox toggle with cascading
const handleCheck = (item: TreeItem) => {
  const currentSet = new Set(internalCheckedIds.value)
  const itemId = item.id
  const isCurrentlyChecked = currentSet.has(itemId)
  const descendantIds = getAllDescendantIds(item)

  if (isCurrentlyChecked) {
    // Uncheck: remove self + all descendants
    currentSet.delete(itemId)
    for (const id of descendantIds) {
      currentSet.delete(id)
    }
  } else {
    // Check: add self + all descendants (skip root-* ids)
    if (!itemId.startsWith('root-')) {
      currentSet.add(itemId)
    }
    for (const id of descendantIds) {
      currentSet.add(id)
    }
  }

  internalCheckedIds.value = Array.from(currentSet)
  // Emit as numbers for the parent (filter out non-numeric ids)
  const numericIds = internalCheckedIds.value
    .filter(id => !id.startsWith('root-'))
    .map(Number)
    .filter(n => !isNaN(n))
  emit('update:checkedIds', numericIds)
}

// Tree Node Component
const TreeNodeItem = defineComponent({
  name: 'TreeNodeItem',
  props: {
    item: { type: Object as () => TreeItem, required: true },
    selectedId: { type: String, default: null },
    expandedIds: { type: Array as () => string[], required: true },
    level: { type: Number, required: true },
    checkboxMode: { type: Boolean, default: false },
    checkedIds: { type: Array as () => string[], default: () => [] }
  },
  emits: ['select', 'toggle', 'check'],
  setup(props, { emit }) {
    const hasChildren = computed(() => props.item.children && props.item.children.length > 0)
    const isExpanded = computed(() => props.expandedIds.includes(props.item.id))
    const isSelected = computed(() => props.selectedId === props.item.id)
    const isRoot = computed(() => props.level === 0)
    const isInactive = computed(() => !isRoot.value && props.item.isActive === false)
    const isChecked = computed(() => props.checkedIds.includes(props.item.id))

    // Determine icon based on type (root = building, council = domain, committee = people)
    const getIcon = () => {
      if (isRoot.value) return 'mdi:office-building'
      return props.item.typeId === 1 ? 'mdi:domain' : 'mdi:account-multiple'
    }

    return () => h('div', { class: 'tree-item-wrapper' }, [
      h('div', {
        class: ['tree-item', { selected: !props.checkboxMode && isSelected.value, inactive: isInactive.value }],
        style: { paddingInlineStart: `${props.level * 16 + 8}px` },
        onClick: () => {
          if (!props.checkboxMode) {
            emit('select', props.item)
          }
        }
      }, [
        hasChildren.value
          ? h('button', {
              class: 'expand-btn',
              onClick: (e: Event) => { e.stopPropagation(); emit('toggle', props.item.id) }
            }, [
              h(Icon, { icon: isExpanded.value ? 'mdi:chevron-down' : 'mdi:chevron-left', class: 'w-4 h-4' })
            ])
          : h('span', { class: 'expand-placeholder' }),
        // Checkbox (only in checkbox mode)
        props.checkboxMode
          ? h('span', {
              class: ['tree-checkbox', { checked: isChecked.value }],
              onClick: (e: Event) => { e.stopPropagation(); emit('check', props.item) }
            }, [
              isChecked.value
                ? h(Icon, { icon: 'mdi:check', class: 'tree-checkbox-icon' })
                : null
            ])
          : null,
        h(Icon, {
          icon: getIcon(),
          class: ['tree-icon', isRoot.value ? 'root' : (props.item.typeId === 1 ? 'council' : 'committee')]
        }),
        h('span', {
          class: ['tree-label', { 'cursor-pointer': props.checkboxMode }],
          onClick: props.checkboxMode ? (e: Event) => { e.stopPropagation(); emit('check', props.item) } : undefined
        }, props.item.name),
        isInactive.value ? h('span', { class: 'inactive-badge' }, 'غير نشط') : null
      ]),
      hasChildren.value && isExpanded.value
        ? h('div', { class: 'tree-children' },
            props.item.children!.map((child: TreeItem) =>
              h(TreeNodeItem, {
                key: child.id,
                item: child,
                selectedId: props.selectedId,
                expandedIds: props.expandedIds,
                level: props.level + 1,
                checkboxMode: props.checkboxMode,
                checkedIds: props.checkedIds,
                onSelect: (item: TreeItem) => emit('select', item),
                onToggle: (id: string) => emit('toggle', id),
                onCheck: (item: TreeItem) => emit('check', item)
              })
            )
          )
        : null
    ])
  }
})

// State
const loading = ref(false)
const treeData = ref<TreeItem[]>([])
const searchQuery = ref('')
const expandedIds = ref<string[]>([])
const showInactive = ref(false)

// Filter to show only inactive items with their parent chain
const filterInactiveOnly = (nodes: TreeItem[]): TreeItem[] => {
  return nodes.reduce((acc: TreeItem[], node) => {
    const filteredChildren = node.children ? filterInactiveOnly(node.children) : []
    const isInactive = node.isActive === false
    // Include if inactive OR has inactive descendants
    if (isInactive || filteredChildren.length > 0) {
      acc.push({ ...node, children: filteredChildren })
    }
    return acc
  }, [])
}

// Filtered tree based on inactive toggle and search
const filteredTree = computed(() => {
  let result = treeData.value

  // Apply inactive filter if toggle is ON
  if (showInactive.value) {
    result = filterInactiveOnly(result)
  }

  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    const filterBySearch = (nodes: TreeItem[]): TreeItem[] => {
      return nodes.reduce((acc: TreeItem[], node) => {
        const matches = node.name?.toLowerCase().includes(query)
        const filteredChildren = node.children ? filterBySearch(node.children) : []
        if (matches || filteredChildren.length > 0) {
          acc.push({ ...node, children: filteredChildren })
        }
        return acc
      }, [])
    }
    result = filterBySearch(result)
  }

  return result
})

// Helper to map tree nodes preserving full structure (including root with null id)
const mapTreeNodes = (nodes: any[]): TreeItem[] => {
  if (!Array.isArray(nodes)) return []

  return nodes.map((item, index) => ({
    // Use index-based id for root node with null id
    id: item.id || `root-${index}`,
    name: item.name,
    typeId: item.typeId,
    // API returns isActive (camelCase from IsActive in C#)
    isActive: item.isActive ?? true,
    children: item.children ? mapTreeNodes(item.children) : []
  }))
}

// Methods
const loadOrganizationStructure = async () => {
  loading.value = true
  try {
    const response = props.useMyOrganization
      ? await CouncilCommitteesService.listMyOrganization(true)
      : await CouncilCommitteesService.listOrganization(true)
    // API returns { data, success, message } wrapper
    const rawData = response.data || response

    // Map full tree structure including root node
    treeData.value = mapTreeNodes(Array.isArray(rawData) ? rawData : [])

    // Expand all nodes by default for better visibility
    const getAllIds = (items: TreeItem[]): string[] => {
      let ids: string[] = []
      for (const item of items) {
        if (item.id) ids.push(item.id)
        if (item.children?.length) {
          ids = ids.concat(getAllIds(item.children))
        }
      }
      return ids
    }
    expandedIds.value = getAllIds(treeData.value)
  } catch (error) {
    console.error('Failed to load organization structure:', error)
  } finally {
    loading.value = false
  }
}

const toggleExpand = (id: string) => {
  const index = expandedIds.value.indexOf(id)
  if (index > -1) {
    expandedIds.value.splice(index, 1)
  } else {
    expandedIds.value.push(id)
  }
}

const handleNodeSelect = (item: TreeItem) => {
  emit('select', item)
}

const refresh = () => {
  loadOrganizationStructure()
}

// Expose methods
defineExpose({ refresh })

// Lifecycle
onMounted(() => {
  loadOrganizationStructure()
})
</script>

<style scoped>
.sidebar-panel {
  @apply w-80 flex-shrink-0 rounded-2xl flex flex-col overflow-hidden;
  background: #ffffff;
  border: 1px solid #e6eaef;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.sidebar-header {
  @apply px-4 py-3 flex items-center justify-between;
  background: #f8fafc;
  border-bottom: 1px solid #e6eaef;
}

.sidebar-title {
  @apply flex items-center gap-2 font-semibold text-sm;
  color: #004730;
}

/* Modern Toggle Filter */
.filter-toggle {
  @apply flex items-center gap-2 cursor-pointer select-none;
}

.filter-label {
  @apply text-xs text-zinc-500;
}

.toggle-input {
  @apply sr-only;
}

.toggle-slider-sm {
  @apply relative w-8 h-4 bg-zinc-200 rounded-full transition-colors duration-200 ease-in-out;
  @apply after:content-[''] after:absolute after:top-0.5 after:start-0.5;
  @apply after:bg-white after:rounded-full after:h-3 after:w-3;
  @apply after:transition-all after:duration-200 after:ease-in-out;
  @apply after:shadow-sm;
}

.toggle-input:checked + .toggle-slider-sm {
  @apply bg-primary;
}

.toggle-input:checked + .toggle-slider-sm::after {
  transform: translateX(1rem);
}

[dir="rtl"] .toggle-input:checked + .toggle-slider-sm::after {
  transform: translateX(-1rem);
}

.sidebar-search {
  @apply p-3;
  border-bottom: 1px solid #e6eaef;
}

.search-input-wrapper {
  @apply relative;
}

.search-icon {
  @apply absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-zinc-400;
}

.search-input {
  @apply w-full ps-9 pe-8 py-2 text-sm rounded-lg;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  color: #004730;
}

.search-input::placeholder {
  color: #94a3b8;
}

.search-input:focus {
  outline: none;
  background: #ffffff;
  border-color: #006d4b;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

.clear-btn {
  @apply absolute end-2 top-1/2 -translate-y-1/2 p-1 text-zinc-400 hover:text-zinc-600 rounded;
}

.sidebar-tree {
  @apply flex-1 overflow-y-auto p-2;
}

.tree-loading {
  @apply flex items-center justify-center py-8;
}

.tree-empty {
  @apply flex flex-col items-center justify-center py-8 text-center;
}

/* Tree Styles */
:deep(.tree-item-wrapper) {
  @apply mb-0.5;
}

:deep(.tree-item) {
  @apply flex items-center gap-2 py-2 pe-3 rounded-lg cursor-pointer transition-all duration-200;
  @apply hover:bg-zinc-100;
  border-inline-start: 2px solid transparent;
}

:deep(.tree-item.selected) {
  background: rgba(0, 109, 75, 0.08);
  color: #006d4b;
  border-inline-start-color: #006d4b;
}

:deep(.expand-btn) {
  @apply p-0.5 rounded transition-colors;
  color: #006d4b;
}

:deep(.expand-btn:hover) {
  color: #007E65;
}

:deep(.expand-placeholder) {
  @apply w-5;
}

:deep(.tree-checkbox) {
  @apply w-[18px] h-[18px] rounded-md border-2 border-zinc-300 cursor-pointer flex-shrink-0 flex items-center justify-center transition-all duration-200 ease-in-out;
}

:deep(.tree-checkbox:hover) {
  @apply border-primary/50 bg-primary/5;
}

:deep(.tree-checkbox.checked) {
  @apply bg-primary border-primary;
  box-shadow: 0 1px 3px rgba(0, 109, 75, 0.3);
}

:deep(.tree-checkbox-icon) {
  @apply w-3 h-3 text-white;
  stroke-width: 3;
}

:deep(.tree-icon) {
  @apply w-5 h-5;
}

:deep(.tree-icon.root) {
  @apply text-zinc-600;
}

:deep(.tree-icon.council) {
  @apply text-primary;
}

:deep(.tree-icon.committee) {
  @apply text-secondary;
}

:deep(.tree-label) {
  @apply text-sm font-medium truncate;
  color: #334155;
}

:deep(.tree-item.selected .tree-label) {
  color: #004730;
  font-weight: 600;
}

:deep(.tree-item.inactive) {
  @apply opacity-60;
}

:deep(.inactive-badge) {
  @apply text-xs px-1.5 py-0.5 bg-zinc-200 text-zinc-500 rounded ms-auto;
}

/* Responsive */
@media (max-width: 1024px) {
  .sidebar-panel {
    @apply w-full max-h-64;
  }
}
</style>

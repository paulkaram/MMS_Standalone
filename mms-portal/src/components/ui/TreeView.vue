<template>
  <div class="tree-view">
    <!-- Search -->
    <div v-if="searchable" class="mb-4">
      <Input
        v-model="searchQuery"
        :placeholder="searchPlaceholder"
        icon-left="mdi:magnify"
        size="sm"
      />
    </div>

    <!-- Tree -->
    <div class="space-y-1">
      <TreeNode
        v-for="node in filteredItems"
        :key="getNodeKey(node)"
        :node="node"
        :level="0"
        :selected-id="selectedId"
        :expanded-ids="expandedIds"
        :item-key="itemKey"
        :item-text="itemText"
        :item-children="itemChildren"
        :item-icon="itemIcon"
        @select="handleSelect"
        @toggle="handleToggle"
      />
    </div>

    <!-- Empty State -->
    <div v-if="filteredItems.length === 0" class="text-center py-8 text-zinc-500">
      <Icon icon="mdi:folder-open" class="w-12 h-12 mx-auto mb-2 text-zinc-300" />
      <p class="text-sm">{{ emptyText }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Input from './Input.vue'
import TreeNode from './TreeNode.vue'

interface Props {
  items: any[]
  itemKey?: string
  itemText?: string
  itemChildren?: string
  itemIcon?: string | ((item: any) => string)
  selectedId?: string | number | null
  searchable?: boolean
  searchPlaceholder?: string
  emptyText?: string
  defaultExpanded?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  itemKey: 'id',
  itemText: 'name',
  itemChildren: 'children',
  itemIcon: undefined,
  selectedId: null,
  searchable: false,
  searchPlaceholder: 'بحث...',
  emptyText: 'لا توجد عناصر',
  defaultExpanded: false
})

const emit = defineEmits<{
  'update:selectedId': [id: string | number | null]
  'select': [item: any]
}>()

// State
const searchQuery = ref('')
const expandedIds = ref<Set<string | number>>(new Set())

// Initialize expanded state
watch(() => props.items, (items) => {
  if (props.defaultExpanded) {
    expandAll(items)
  }
}, { immediate: true })

// Methods
const getNodeKey = (node: any): string | number => {
  return node[props.itemKey]
}

const expandAll = (items: any[]) => {
  items.forEach(item => {
    expandedIds.value.add(item[props.itemKey])
    const children = item[props.itemChildren]
    if (children?.length) {
      expandAll(children)
    }
  })
}

const handleSelect = (item: any) => {
  emit('update:selectedId', item[props.itemKey])
  emit('select', item)
}

const handleToggle = (item: any) => {
  const key = item[props.itemKey]
  if (expandedIds.value.has(key)) {
    expandedIds.value.delete(key)
  } else {
    expandedIds.value.add(key)
  }
}

// Filter items based on search
const filteredItems = computed(() => {
  if (!searchQuery.value.trim()) {
    return props.items
  }

  const query = searchQuery.value.toLowerCase()
  return filterTree(props.items, query)
})

const filterTree = (items: any[], query: string): any[] => {
  return items.reduce((acc: any[], item) => {
    const text = item[props.itemText]?.toLowerCase() || ''
    const children = item[props.itemChildren]

    if (text.includes(query)) {
      acc.push(item)
      // Expand matching items
      expandedIds.value.add(item[props.itemKey])
    } else if (children?.length) {
      const filteredChildren = filterTree(children, query)
      if (filteredChildren.length) {
        acc.push({ ...item, [props.itemChildren]: filteredChildren })
        expandedIds.value.add(item[props.itemKey])
      }
    }

    return acc
  }, [])
}

// Watch search to expand/collapse
watch(searchQuery, (val) => {
  if (!val) {
    // Reset to default state when search is cleared
    expandedIds.value.clear()
    if (props.defaultExpanded) {
      expandAll(props.items)
    }
  }
})
</script>

<template>
  <div class="tree-node">
    <!-- Node Content -->
    <div
      :class="[
        'flex items-center gap-2 px-3 py-2 rounded-lg cursor-pointer transition-colors',
        isSelected ? 'bg-primary-50 text-primary' : 'hover:bg-zinc-100'
      ]"
      :style="{ paddingInlineStart: `${level * 1.5 + 0.75}rem` }"
      @click="handleClick"
    >
      <!-- Expand/Collapse Icon -->
      <button
        v-if="hasChildren"
        type="button"
        class="w-5 h-5 flex items-center justify-center text-zinc-400 hover:text-zinc-600"
        @click.stop="$emit('toggle', node)"
      >
        <Icon
          :icon="isExpanded ? 'mdi:chevron-down' : 'mdi:chevron-left'"
          class="w-4 h-4 transition-transform"
          :class="{ 'rotate-90': !isExpanded && $i18n?.locale !== 'ar' }"
        />
      </button>
      <span v-else class="w-5"></span>

      <!-- Node Icon -->
      <Icon
        :icon="nodeIcon"
        :class="[
          'w-5 h-5',
          isSelected ? 'text-primary' : 'text-zinc-400'
        ]"
      />

      <!-- Node Text -->
      <span class="flex-1 text-sm font-medium truncate">
        {{ nodeText }}
      </span>

      <!-- Badge (children count) -->
      <span
        v-if="hasChildren && showChildCount"
        class="text-xs text-zinc-400"
      >
        ({{ childrenCount }})
      </span>
    </div>

    <!-- Children -->
    <div v-if="hasChildren && isExpanded" class="children">
      <TreeNode
        v-for="child in children"
        :key="child[itemKey]"
        :node="child"
        :level="level + 1"
        :selected-id="selectedId"
        :expanded-ids="expandedIds"
        :item-key="itemKey"
        :item-text="itemText"
        :item-children="itemChildren"
        :item-icon="itemIcon"
        @select="$emit('select', $event)"
        @toggle="$emit('toggle', $event)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'

interface Props {
  node: any
  level: number
  selectedId: string | number | null
  expandedIds: Set<string | number>
  itemKey: string
  itemText: string
  itemChildren: string
  itemIcon?: string | ((item: any) => string)
  showChildCount?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  itemIcon: undefined,
  showChildCount: true
})

const emit = defineEmits<{
  'select': [item: any]
  'toggle': [item: any]
}>()

// Computed
const nodeId = computed(() => props.node[props.itemKey])
const nodeText = computed(() => props.node[props.itemText])
const children = computed(() => props.node[props.itemChildren] || [])
const hasChildren = computed(() => children.value.length > 0)
const childrenCount = computed(() => children.value.length)
const isSelected = computed(() => props.selectedId === nodeId.value)
const isExpanded = computed(() => props.expandedIds.has(nodeId.value))

const nodeIcon = computed(() => {
  if (typeof props.itemIcon === 'function') {
    return props.itemIcon(props.node)
  }
  if (props.itemIcon) {
    return props.node[props.itemIcon] || 'mdi:folder'
  }
  // Default icons based on expanded state and children
  if (hasChildren.value) {
    return isExpanded.value ? 'mdi:folder-open' : 'mdi:folder'
  }
  return 'mdi:file-document'
})

// Methods
const handleClick = () => {
  emit('select', props.node)
  if (hasChildren.value) {
    emit('toggle', props.node)
  }
}
</script>

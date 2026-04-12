<template>
  <div class="tabs">
    <!-- Tab Headers -->
    <div class="border-b border-gray-200">
      <nav class="flex gap-1 overflow-x-auto" :class="{ 'justify-center': centered }">
        <button
          v-for="(tab, index) in tabs"
          :key="tab.key || index"
          type="button"
          :class="[
            'px-4 py-3 text-sm font-medium border-b-2 transition-colors whitespace-nowrap',
            modelValue === index
              ? 'border-primary text-primary'
              : 'border-transparent text-zinc-500 hover:text-zinc-700 hover:border-zinc-300'
          ]"
          @click="selectTab(index)"
        >
          <span class="flex items-center gap-2">
            <Icon v-if="tab.icon" :icon="tab.icon" class="w-4 h-4" />
            {{ tab.label }}
            <span
              v-if="tab.badge !== undefined"
              :class="[
                'px-2 py-0.5 text-xs rounded-full',
                modelValue === index
                  ? 'bg-primary-100 text-primary'
                  : 'bg-zinc-100 text-zinc-600'
              ]"
            >
              {{ tab.badge }}
            </span>
          </span>
        </button>
      </nav>
    </div>

    <!-- Tab Content -->
    <div class="tab-content mt-4">
      <slot :name="`tab-${modelValue}`"></slot>
      <slot :active-index="modelValue"></slot>
    </div>
  </div>
</template>

<script setup lang="ts">
import Icon from '@/components/ui/Icon.vue'

interface Tab {
  key?: string
  label: string
  icon?: string
  badge?: number | string
  disabled?: boolean
}

interface Props {
  modelValue: number
  tabs: Tab[]
  centered?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  centered: false
})

const emit = defineEmits<{
  'update:modelValue': [index: number]
  'change': [index: number]
}>()

const selectTab = (index: number) => {
  if (props.tabs[index]?.disabled) return
  emit('update:modelValue', index)
  emit('change', index)
}
</script>

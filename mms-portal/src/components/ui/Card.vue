<template>
  <div :class="cardClasses">
    <!-- Header -->
    <div
      v-if="$slots.header || title"
      :class="headerClasses"
    >
      <slot name="header">
        <div class="flex items-center justify-between gap-4">
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

          <!-- Header actions -->
          <div
            v-if="$slots.headerActions"
            class="flex items-center gap-2"
          >
            <slot name="headerActions" />
          </div>
        </div>
      </slot>
    </div>

    <!-- Body -->
    <div
      v-if="$slots.default"
      :class="bodyClasses"
    >
      <slot />
    </div>

    <!-- Footer -->
    <div
      v-if="$slots.footer"
      :class="footerClasses"
    >
      <slot name="footer" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

// Props
interface Props {
  title?: string
  subtitle?: string
  variant?: 'default' | 'outlined' | 'elevated' | 'flat'
  padding?: 'none' | 'sm' | 'md' | 'lg'
  noPadding?: boolean
  hoverable?: boolean
  clickable?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  title: undefined,
  subtitle: undefined,
  variant: 'default',
  padding: 'md',
  noPadding: false,
  hoverable: false,
  clickable: false
})

// Computed classes
const cardClasses = computed(() => {
  const base = [
    'rounded-xl overflow-hidden',
    'transition-all duration-200'
  ]

  // Variant styles
  const variants = {
    default: 'bg-white shadow-sm border border-gray-200',
    outlined: 'bg-white border-2 border-gray-300',
    elevated: 'bg-white shadow-lg ring-1 ring-black/5',
    flat: 'bg-gray-50'
  }

  // Interactive states
  const interactive = []
  if (props.hoverable) {
    interactive.push('hover:shadow-md hover:border-zinc-200')
  }
  if (props.clickable) {
    interactive.push('cursor-pointer active:scale-[0.99]')
  }

  return [
    ...base,
    variants[props.variant],
    ...interactive
  ]
})

const headerClasses = computed(() => {
  const paddings = {
    none: '',
    sm: 'px-4 py-3',
    md: 'px-6 py-4',
    lg: 'px-8 py-5'
  }

  return [
    'border-b border-gray-200',
    props.noPadding ? '' : paddings[props.padding]
  ]
})

const bodyClasses = computed(() => {
  const paddings = {
    none: '',
    sm: 'p-4',
    md: 'p-6',
    lg: 'p-8'
  }

  return props.noPadding ? '' : paddings[props.padding]
})

const footerClasses = computed(() => {
  const paddings = {
    none: '',
    sm: 'px-4 py-3',
    md: 'px-6 py-4',
    lg: 'px-8 py-5'
  }

  return [
    'border-t border-gray-200 bg-gray-50/50',
    props.noPadding ? '' : paddings[props.padding]
  ]
})
</script>

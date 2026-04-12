<template>
  <button
    :type="type"
    :disabled="disabled || loading"
    :class="buttonClasses"
    @click="handleClick"
  >
    <!-- Loading spinner -->
    <span
      v-if="loading"
      class="absolute inset-0 flex items-center justify-center"
    >
      <svg
        class="animate-spin h-5 w-5"
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
      >
        <circle
          class="opacity-25"
          cx="12"
          cy="12"
          r="10"
          stroke="currentColor"
          stroke-width="4"
        />
        <path
          class="opacity-75"
          fill="currentColor"
          d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
        />
      </svg>
    </span>

    <!-- Button content -->
    <span
      :class="[
        'inline-flex items-center gap-2',
        { 'invisible': loading }
      ]"
    >
      <!-- Left icon -->
      <Icon
        v-if="iconLeft"
        :icon="iconLeft"
        :class="iconClasses"
      />

      <!-- Slot content -->
      <slot />

      <!-- Right icon -->
      <Icon
        v-if="iconRight"
        :icon="iconRight"
        :class="iconClasses"
      />
    </span>
  </button>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'

// Props
interface Props {
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger' | 'success'
  size?: 'sm' | 'md' | 'lg'
  type?: 'button' | 'submit' | 'reset'
  disabled?: boolean
  loading?: boolean
  block?: boolean
  iconLeft?: string
  iconRight?: string
  rounded?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'primary',
  size: 'md',
  type: 'button',
  disabled: false,
  loading: false,
  block: false,
  iconLeft: undefined,
  iconRight: undefined,
  rounded: false
})

// Emits
const emit = defineEmits<{
  click: [event: MouseEvent]
}>()

// Computed classes
const buttonClasses = computed(() => {
  const base = [
    'relative inline-flex items-center justify-center',
    'font-medium transition-all duration-200',
    'focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-2',
    'disabled:cursor-not-allowed disabled:opacity-50'
  ]

  // Variant styles
  const variants = {
    primary: [
      'text-white shadow-sm',
      'hover:opacity-90 active:opacity-80',
      'focus-visible:ring-primary/50',
      'btn-primary-gradient'
    ],
    secondary: [
      'bg-zinc-100 text-zinc-900 shadow-sm',
      'hover:bg-zinc-200 active:bg-zinc-300',
      'focus-visible:ring-zinc-400'
    ],
    outline: [
      'border border-zinc-200 text-zinc-600 bg-white',
      'hover:bg-zinc-50 hover:border-zinc-300',
      'focus-visible:ring-zinc-400'
    ],
    ghost: [
      'text-gray-700 bg-transparent',
      'hover:bg-gray-100 active:bg-gray-200',
      'focus-visible:ring-gray-500'
    ],
    danger: [
      'bg-error text-white',
      'hover:bg-error-600 active:bg-error-700',
      'focus-visible:ring-error'
    ],
    success: [
      'bg-success text-white',
      'hover:bg-success-600 active:bg-success-700',
      'focus-visible:ring-success'
    ]
  }

  // Size styles
  const sizes = {
    sm: 'px-3 py-2 text-xs gap-1.5',
    md: 'px-4 py-2.5 text-sm gap-2',
    lg: 'px-5 py-3 text-sm gap-2'
  }

  // Border radius
  const radius = props.rounded ? 'rounded-full' : 'rounded-lg'

  // Width
  const width = props.block ? 'w-full' : ''

  return [
    ...base,
    ...variants[props.variant],
    sizes[props.size],
    radius,
    width
  ]
})

const iconClasses = computed(() => {
  const sizes = {
    sm: 'w-4 h-4',
    md: 'w-5 h-5',
    lg: 'w-6 h-6'
  }
  return sizes[props.size]
})

// Methods
function handleClick(event: MouseEvent) {
  if (!props.disabled && !props.loading) {
    emit('click', event)
  }
}
</script>

<style scoped>
.btn-primary-gradient {
  background: linear-gradient(135deg, #004730 0%, #006d4b 100%);
}
</style>

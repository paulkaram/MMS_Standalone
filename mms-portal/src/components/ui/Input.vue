<template>
  <div :class="['w-full', containerClass]">
    <!-- Label -->
    <label
      v-if="label"
      :for="inputId"
      class="form-label"
    >
      {{ label }}
      <span
        v-if="required"
        class="text-error"
      >*</span>
    </label>

    <!-- Input wrapper -->
    <div class="relative">
      <!-- Left icon/prefix -->
      <div
        v-if="$slots.prefix || iconLeft"
        class="absolute inset-y-0 start-0 flex items-center ps-3 pointer-events-none text-zinc-400"
      >
        <slot name="prefix">
          <Icon
            v-if="iconLeft"
            :icon="iconLeft"
            class="w-5 h-5"
          />
        </slot>
      </div>

      <!-- Input element -->
      <input
        :id="inputId"
        ref="inputRef"
        v-model="model"
        :type="computedType"
        :placeholder="placeholder"
        :disabled="disabled"
        :readonly="readonly"
        :required="required"
        :autocomplete="autocomplete"
        :maxlength="maxlength"
        :min="min"
        :max="max"
        :step="step"
        :class="inputClasses"
        @focus="handleFocus"
        @blur="handleBlur"
        @input="handleInput"
        @keydown.enter="handleEnter"
      >

      <!-- Right icon/suffix -->
      <div
        v-if="$slots.suffix || iconRight || type === 'password' || clearable"
        class="absolute inset-y-0 end-0 flex items-center pe-3 gap-1"
      >
        <!-- Clear button -->
        <button
          v-if="clearable && model"
          type="button"
          class="text-zinc-400 hover:text-zinc-600 transition-colors"
          @click="clearInput"
        >
          <Icon
            icon="mdi:close-circle"
            class="w-5 h-5"
          />
        </button>

        <!-- Password toggle -->
        <button
          v-if="type === 'password'"
          type="button"
          class="text-zinc-400 hover:text-zinc-600 transition-colors"
          @click="togglePassword"
        >
          <Icon
            :icon="showPassword ? 'mdi:eye-off' : 'mdi:eye'"
            class="w-5 h-5"
          />
        </button>

        <!-- Custom suffix -->
        <slot name="suffix">
          <Icon
            v-if="iconRight"
            :icon="iconRight"
            class="w-5 h-5 text-zinc-400"
          />
        </slot>
      </div>
    </div>

    <!-- Helper text / Error message -->
    <p
      v-if="error"
      class="form-error"
    >
      {{ error }}
    </p>
    <p
      v-else-if="hint"
      class="form-helper"
    >
      {{ hint }}
    </p>

    <!-- Character counter -->
    <p
      v-if="maxlength && showCounter"
      class="text-xs text-zinc-400 mt-1 text-end"
    >
      {{ String(model || '').length }} / {{ maxlength }}
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, useId } from 'vue'
import Icon from '@/components/ui/Icon.vue'

// Props
interface Props {
  modelValue?: string | number
  type?: 'text' | 'password' | 'email' | 'number' | 'tel' | 'url' | 'search'
  label?: string
  placeholder?: string
  hint?: string
  error?: string
  disabled?: boolean
  readonly?: boolean
  required?: boolean
  clearable?: boolean
  iconLeft?: string
  iconRight?: string
  autocomplete?: string
  maxlength?: number
  min?: number | string
  max?: number | string
  step?: number | string
  showCounter?: boolean
  containerClass?: string
  size?: 'sm' | 'md' | 'lg'
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '',
  type: 'text',
  label: undefined,
  placeholder: undefined,
  hint: undefined,
  error: undefined,
  disabled: false,
  readonly: false,
  required: false,
  clearable: false,
  iconLeft: undefined,
  iconRight: undefined,
  autocomplete: 'off',
  maxlength: undefined,
  min: undefined,
  max: undefined,
  step: undefined,
  showCounter: false,
  containerClass: '',
  size: 'md'
})

// Emits
const emit = defineEmits<{
  'update:modelValue': [value: string | number]
  focus: [event: FocusEvent]
  blur: [event: FocusEvent]
  input: [event: Event]
  enter: [event: KeyboardEvent]
  clear: []
}>()

// Refs
const inputRef = ref<HTMLInputElement | null>(null)
const showPassword = ref(false)
const isFocused = ref(false)

// Generate unique ID
const inputId = useId()

// v-model binding
const model = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

// Computed type for password toggle
const computedType = computed(() => {
  if (props.type === 'password') {
    return showPassword.value ? 'text' : 'password'
  }
  return props.type
})

// Input classes
const inputClasses = computed(() => {
  const base = [
    'form-input-base',
    'w-full'
  ]

  // Size variants
  const sizes = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2.5 text-sm',
    lg: 'px-4 py-3 text-base'
  }

  // Padding adjustments for icons
  const hasLeftIcon = props.iconLeft || !!props.iconLeft
  const hasRightIcon = props.iconRight || props.type === 'password' || props.clearable

  const paddingLeft = hasLeftIcon ? 'ps-10' : ''
  const paddingRight = hasRightIcon ? 'pe-10' : ''

  // Error state
  const errorClass = props.error ? 'form-input-error' : ''

  return [
    ...base,
    sizes[props.size],
    paddingLeft,
    paddingRight,
    errorClass
  ]
})

// Methods
function togglePassword() {
  showPassword.value = !showPassword.value
}

function clearInput() {
  model.value = ''
  emit('clear')
  inputRef.value?.focus()
}

function handleFocus(event: FocusEvent) {
  isFocused.value = true
  emit('focus', event)
}

function handleBlur(event: FocusEvent) {
  isFocused.value = false
  emit('blur', event)
}

function handleInput(event: Event) {
  emit('input', event)
}

function handleEnter(event: KeyboardEvent) {
  emit('enter', event)
}

// Expose methods for parent component access
function focus() {
  inputRef.value?.focus()
}

function blur() {
  inputRef.value?.blur()
}

function select() {
  inputRef.value?.select()
}

defineExpose({
  focus,
  blur,
  select,
  inputRef
})
</script>

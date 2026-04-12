<template>
  <div :class="['w-full', containerClass, { 'datepicker-dark': dark }]">
    <!-- Label -->
    <label
      v-if="label"
      :for="datePickerId"
      :class="['form-label', { 'text-zinc-200': dark }]"
    >
      {{ label }}
      <span
        v-if="required"
        class="text-error"
      >*</span>
    </label>

    <Popover v-slot="{ open, close }" class="relative">
      <PopoverButton
        ref="buttonRef"
        :id="datePickerId"
        :disabled="disabled"
        :class="buttonClasses"
        @click="updatePanelPosition"
      >
        <span
          v-if="displayValue"
          class="block truncate"
        >
          {{ displayValue }}
        </span>
        <span
          v-else
          :class="['block truncate', dark ? 'text-zinc-500' : 'text-zinc-400']"
        >
          {{ placeholder || t('SelectDate') }}
        </span>

        <span class="pointer-events-none absolute inset-y-0 end-0 flex items-center pe-3">
          <svg :class="['h-5 w-5', dark ? 'text-zinc-500' : 'text-zinc-400']" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
            <path d="M19 19H5V8h14m-3-7v2H8V1H6v2H5c-1.11 0-2 .89-2 2v14a2 2 0 002 2h14a2 2 0 002-2V5a2 2 0 00-2-2h-1V1h-2v2z"/>
          </svg>
        </span>
      </PopoverButton>

      <Teleport to="body">
        <Transition
          enter-active-class="transition duration-200 ease-out"
          enter-from-class="translate-y-1 opacity-0"
          enter-to-class="translate-y-0 opacity-100"
          leave-active-class="transition duration-150 ease-in"
          leave-from-class="translate-y-0 opacity-100"
          leave-to-class="translate-y-1 opacity-0"
        >
          <PopoverPanel
            v-if="open"
            static
            :class="[
              'fixed w-72 origin-top-start rounded-xl p-4 shadow-lg ring-1',
              dark ? 'bg-[#22252c] ring-white/10' : 'bg-white ring-black/5'
            ]"
            :style="panelStyle"
          >
          <!-- Month/Year navigation -->
          <div class="flex items-center justify-between mb-4">
            <button
              type="button"
              :class="['p-1 rounded-lg', dark ? 'hover:bg-white/10 text-zinc-300' : 'hover:bg-zinc-100']"
              @click="previousMonth"
            >
              <svg v-if="isRtl" class="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
                <path d="M8.59 16.59L13.17 12 8.59 7.41 10 6l6 6-6 6-1.41-1.41z"/>
              </svg>
              <svg v-else class="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
                <path d="M15.41 16.59L10.83 12l4.58-4.59L14 6l-6 6 6 6 1.41-1.41z"/>
              </svg>
            </button>

            <div class="flex items-center gap-2">
              <select
                v-model="currentMonth"
                :class="['text-sm font-medium bg-transparent focus:outline-none cursor-pointer', dark ? 'text-zinc-200' : '']"
              >
                <option
                  v-for="(month, index) in monthNames"
                  :key="index"
                  :value="index"
                  :class="dark ? 'bg-[#22252c] text-zinc-200' : ''"
                >
                  {{ month }}
                </option>
              </select>

              <select
                v-model="currentYear"
                :class="['text-sm font-medium bg-transparent focus:outline-none cursor-pointer', dark ? 'text-zinc-200' : '']"
              >
                <option
                  v-for="year in yearRange"
                  :key="year"
                  :value="year"
                  :class="dark ? 'bg-[#22252c] text-zinc-200' : ''"
                >
                  {{ year }}
                </option>
              </select>
            </div>

            <button
              type="button"
              :class="['p-1 rounded-lg', dark ? 'hover:bg-white/10 text-zinc-300' : 'hover:bg-zinc-100']"
              @click="nextMonth"
            >
              <svg v-if="isRtl" class="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
                <path d="M15.41 16.59L10.83 12l4.58-4.59L14 6l-6 6 6 6 1.41-1.41z"/>
              </svg>
              <svg v-else class="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
                <path d="M8.59 16.59L13.17 12 8.59 7.41 10 6l6 6-6 6-1.41-1.41z"/>
              </svg>
            </button>
          </div>

          <!-- Day names -->
          <div class="grid grid-cols-7 gap-1 mb-2">
            <div
              v-for="day in dayNames"
              :key="day"
              :class="['text-center text-xs font-medium py-1', dark ? 'text-zinc-500' : 'text-zinc-500']"
            >
              {{ day }}
            </div>
          </div>

          <!-- Calendar days -->
          <div class="grid grid-cols-7 gap-1">
            <!-- Empty cells for days before month starts -->
            <div
              v-for="n in firstDayOfMonth"
              :key="'empty-' + n"
              class="w-8 h-8"
            />

            <!-- Days of the month -->
            <button
              v-for="day in daysInMonth"
              :key="day"
              type="button"
              :class="getDayClasses(day)"
              :disabled="isDateDisabled(day)"
              @click="selectDate(day, close)"
            >
              {{ day }}
            </button>
          </div>

          <!-- Today button -->
          <div :class="['mt-4 pt-4 border-t flex justify-between', dark ? 'border-white/10' : 'border-zinc-100']">
            <button
              v-if="clearable && modelValue"
              type="button"
              :class="['text-sm', dark ? 'text-zinc-400 hover:text-zinc-200' : 'text-zinc-500 hover:text-zinc-700']"
              @click="clearDate(close)"
            >
              مسح
            </button>
            <button
              type="button"
              :class="['text-sm ms-auto', dark ? 'text-amber-400 hover:text-amber-300' : 'text-primary hover:text-primary-600']"
              @click="selectToday(close)"
            >
              اليوم
            </button>
          </div>
          </PopoverPanel>
        </Transition>
      </Teleport>
    </Popover>

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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, useId } from 'vue'
import { Popover, PopoverButton, PopoverPanel } from '@headlessui/vue'
import { useI18n } from 'vue-i18n'
import Icon from '@/components/ui/Icon.vue'
import { useUserStore } from '@/stores/user'

const { t } = useI18n()

// Props
interface Props {
  modelValue?: Date | string | null
  label?: string
  placeholder?: string
  hint?: string
  error?: string
  disabled?: boolean
  required?: boolean
  clearable?: boolean
  minDate?: Date | string
  maxDate?: Date | string
  format?: string
  containerClass?: string
  dark?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: null,
  label: undefined,
  placeholder: undefined,
  hint: undefined,
  error: undefined,
  disabled: false,
  required: false,
  clearable: true,
  minDate: undefined,
  maxDate: undefined,
  format: 'YYYY-MM-DD',
  containerClass: '',
  dark: false
})

// Emits
const emit = defineEmits<{
  'update:modelValue': [value: Date | string | null]
}>()

// Generate unique ID
const datePickerId = useId()

// Button ref for positioning
const buttonRef = ref<HTMLElement | null>(null)
const panelStyle = ref<Record<string, string>>({
  top: '0px',
  left: '0px',
  zIndex: '99999'
})

const updatePanelPosition = () => {
  if (!buttonRef.value?.$el) return
  const rect = buttonRef.value.$el.getBoundingClientRect()
  panelStyle.value = {
    top: `${rect.bottom + 8}px`,
    left: `${rect.left}px`,
    zIndex: '99999'
  }
}

// Store
const userStore = useUserStore()
const isRtl = computed(() => userStore.isRtl)

// Current view state
const currentMonth = ref(new Date().getMonth())
const currentYear = ref(new Date().getFullYear())

// Arabic month names
const monthNames = computed(() => {
  if (isRtl.value) {
    return [
      'يناير', 'فبراير', 'مارس', 'إبريل', 'مايو', 'يونيو',
      'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'
    ]
  }
  return [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ]
})

// Arabic day names (short)
const dayNames = computed(() => {
  if (isRtl.value) {
    return ['ح', 'ن', 'ث', 'ر', 'خ', 'ج', 'س']
  }
  return ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa']
})

// Year range for selector
const yearRange = computed(() => {
  const years = []
  const startYear = 1970
  const endYear = 2100
  for (let i = startYear; i <= endYear; i++) {
    years.push(i)
  }
  return years
})

// Days in current month
const daysInMonth = computed(() => {
  return new Date(currentYear.value, currentMonth.value + 1, 0).getDate()
})

// First day of month (0 = Sunday)
const firstDayOfMonth = computed(() => {
  return new Date(currentYear.value, currentMonth.value, 1).getDay()
})

// Parse model value to Date
const selectedDate = computed(() => {
  if (!props.modelValue) return null
  if (props.modelValue instanceof Date) return props.modelValue
  return new Date(props.modelValue)
})

// Display value
const displayValue = computed(() => {
  if (!selectedDate.value) return ''

  const day = selectedDate.value.getDate()
  const month = selectedDate.value.getMonth() + 1
  const year = selectedDate.value.getFullYear()

  return `${day}/${month}/${year}`
})

// Initialize current month/year from model value
watch(
  () => props.modelValue,
  (newValue) => {
    if (newValue) {
      const date = newValue instanceof Date ? newValue : new Date(newValue)
      currentMonth.value = date.getMonth()
      currentYear.value = date.getFullYear()
    }
  },
  { immediate: true }
)

// Navigation
function previousMonth() {
  if (currentMonth.value === 0) {
    currentMonth.value = 11
    currentYear.value--
  } else {
    currentMonth.value--
  }
}

function nextMonth() {
  if (currentMonth.value === 11) {
    currentMonth.value = 0
    currentYear.value++
  } else {
    currentMonth.value++
  }
}

// Check if a day is selected
function isSelected(day: number): boolean {
  if (!selectedDate.value) return false
  return (
    selectedDate.value.getDate() === day &&
    selectedDate.value.getMonth() === currentMonth.value &&
    selectedDate.value.getFullYear() === currentYear.value
  )
}

// Check if a day is today
function isToday(day: number): boolean {
  const today = new Date()
  return (
    today.getDate() === day &&
    today.getMonth() === currentMonth.value &&
    today.getFullYear() === currentYear.value
  )
}

// Check if date is disabled
function isDateDisabled(day: number): boolean {
  // Create date at start of day for comparison
  const date = new Date(currentYear.value, currentMonth.value, day, 0, 0, 0, 0)

  if (props.minDate) {
    const min = props.minDate instanceof Date ? props.minDate : new Date(props.minDate)
    // Compare only date parts (ignore time)
    const minDateOnly = new Date(min.getFullYear(), min.getMonth(), min.getDate(), 0, 0, 0, 0)
    if (date < minDateOnly) return true
  }

  if (props.maxDate) {
    const max = props.maxDate instanceof Date ? props.maxDate : new Date(props.maxDate)
    // Compare only date parts (ignore time)
    const maxDateOnly = new Date(max.getFullYear(), max.getMonth(), max.getDate(), 0, 0, 0, 0)
    if (date > maxDateOnly) return true
  }

  return false
}

// Get day button classes
function getDayClasses(day: number) {
  const base = [
    'w-8 h-8 flex items-center justify-center text-sm rounded-lg transition-colors'
  ]

  if (isDateDisabled(day)) {
    return [...base, props.dark ? 'text-zinc-600 cursor-not-allowed' : 'text-zinc-300 cursor-not-allowed']
  }

  if (isSelected(day)) {
    return [...base, props.dark ? 'bg-amber-500 text-white font-medium' : 'bg-primary text-white font-medium']
  }

  if (isToday(day)) {
    return [...base, props.dark ? 'bg-amber-500/20 text-amber-400 font-medium hover:bg-amber-500/30' : 'bg-primary/10 text-primary font-medium hover:bg-primary/20']
  }

  return [...base, props.dark ? 'hover:bg-white/10 text-zinc-300' : 'hover:bg-zinc-100 text-zinc-700']
}

// Select date
function selectDate(day: number, close: () => void) {
  // Create date at noon (12:00) to avoid timezone boundary issues when converting to UTC
  const date = new Date(currentYear.value, currentMonth.value, day, 12, 0, 0, 0)
  emit('update:modelValue', date)
  close()
}

// Select today
function selectToday(close: () => void) {
  // Create today's date at noon to avoid timezone issues
  const now = new Date()
  const date = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 12, 0, 0, 0)
  emit('update:modelValue', date)
  close()
}

// Clear date
function clearDate(close: () => void) {
  emit('update:modelValue', null)
  close()
}

// Button classes
const buttonClasses = computed(() => {
  if (props.dark) {
    const base = [
      'relative w-full cursor-pointer rounded-lg text-start',
      'border px-4 py-2.5 text-sm',
      'bg-[#22252c] text-zinc-200 border-[rgba(255,255,255,0.1)]',
      'focus:outline-none focus-visible:ring-2 focus-visible:ring-amber-500 focus-visible:ring-offset-2 focus-visible:ring-offset-[#1c1e24]',
      'disabled:cursor-not-allowed disabled:bg-[#1a1c21] disabled:text-zinc-600'
    ]

    const errorClass = props.error
      ? 'border-error focus-visible:ring-error'
      : 'hover:border-[rgba(255,255,255,0.2)]'

    return [...base, errorClass]
  }

  const base = [
    'relative w-full cursor-pointer rounded-lg bg-white text-start',
    'border px-4 py-2.5 text-sm',
    'focus:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2',
    'disabled:cursor-not-allowed disabled:bg-gray-50 disabled:text-gray-500 disabled:opacity-60'
  ]

  const errorClass = props.error
    ? 'border-error focus-visible:ring-error'
    : 'border-gray-300 hover:border-gray-400'

  return [...base, errorClass]
})
</script>

<template>
  <div :class="['w-full', containerClass]">
    <!-- Label -->
    <label
      v-if="label"
      :for="datePickerId"
      class="form-label"
    >
      {{ label }}
      <span
        v-if="required"
        class="text-error"
      >*</span>
    </label>

    <Popover class="relative">
      <PopoverButton
        :id="datePickerId"
        :disabled="disabled"
        :class="buttonClasses"
      >
        <span
          v-if="displayValue"
          class="block truncate"
        >
          {{ displayValue }}
        </span>
        <span
          v-else
          class="block truncate text-zinc-400"
        >
          {{ placeholder }}
        </span>

        <span class="pointer-events-none absolute inset-y-0 end-0 flex items-center pe-3 gap-2">
          <!-- Calendar type toggle -->
          <button
            v-if="showToggle"
            type="button"
            class="text-xs px-1.5 py-0.5 rounded bg-zinc-100 text-zinc-600 hover:bg-zinc-200"
            @click.stop="toggleCalendarType"
          >
            {{ isHijri ? 'م' : 'هـ' }}
          </button>
          <Icon
            icon="mdi:calendar"
            class="h-5 w-5 text-zinc-400"
            aria-hidden="true"
          />
        </span>
      </PopoverButton>

      <Transition
        enter-active-class="transition duration-200 ease-out"
        enter-from-class="translate-y-1 opacity-0"
        enter-to-class="translate-y-0 opacity-100"
        leave-active-class="transition duration-150 ease-in"
        leave-from-class="translate-y-0 opacity-100"
        leave-to-class="translate-y-1 opacity-0"
      >
        <PopoverPanel
          v-slot="{ close }"
          class="absolute z-50 mt-1 w-64 origin-top-start rounded-lg bg-white p-3 shadow-lg ring-1 ring-black/5"
        >
          <!-- Month/Year navigation -->
          <div class="flex items-center justify-between mb-2">
            <button
              type="button"
              class="p-1 hover:bg-zinc-100 rounded"
              @click="previousMonth"
            >
              <Icon
                icon="mdi:chevron-right"
                class="w-4 h-4"
              />
            </button>

            <div class="flex items-center gap-1">
              <select
                v-model="currentMonth"
                class="text-xs font-medium bg-transparent focus:outline-none cursor-pointer"
              >
                <option
                  v-for="(month, index) in monthNames"
                  :key="index"
                  :value="index"
                >
                  {{ month }}
                </option>
              </select>

              <select
                v-model="currentYear"
                class="text-xs font-medium bg-transparent focus:outline-none cursor-pointer"
              >
                <option
                  v-for="year in yearRange"
                  :key="year"
                  :value="year"
                >
                  {{ year }}
                </option>
              </select>

              <button
                v-if="showToggle"
                type="button"
                class="text-[10px] px-1.5 py-0.5 rounded bg-zinc-100 text-zinc-600 hover:bg-zinc-200 ms-1"
                @click="toggleCalendarType"
              >
                {{ isHijri ? 'م' : 'هـ' }}
              </button>
            </div>

            <button
              type="button"
              class="p-1 hover:bg-zinc-100 rounded"
              @click="nextMonth"
            >
              <Icon
                icon="mdi:chevron-left"
                class="w-4 h-4"
              />
            </button>
          </div>

          <!-- Day names -->
          <div class="grid grid-cols-7 gap-0.5 mb-1">
            <div
              v-for="day in dayNames"
              :key="day"
              class="text-center text-[10px] font-medium text-zinc-400 py-0.5"
            >
              {{ day }}
            </div>
          </div>

          <!-- Calendar days -->
          <div class="grid grid-cols-7 gap-0.5">
            <!-- Empty cells for days before month starts -->
            <div
              v-for="n in firstDayOfMonth"
              :key="'empty-' + n"
              class="w-8 h-7"
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
          <div class="mt-2 pt-2 border-t border-zinc-100 flex justify-between items-center">
            <button
              v-if="clearable && modelValue"
              type="button"
              class="text-xs text-zinc-500 hover:text-zinc-700"
              @click="clearDate(close)"
            >
              مسح
            </button>
            <button
              type="button"
              class="text-xs text-primary hover:text-primary-600 ms-auto"
              @click="selectToday(close)"
            >
              اليوم
            </button>
          </div>
        </PopoverPanel>
      </Transition>
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
import Icon from '@/components/ui/Icon.vue'
import moment from 'moment-hijri'

// Configure moment-hijri for Arabic
moment.locale('ar-SA')

// Types
interface HijriDate {
  iYear: number
  iMonth: number
  iDate: number
}

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
  showToggle?: boolean
  defaultToHijri?: boolean
  containerClass?: string
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: null,
  label: undefined,
  placeholder: 'اختر تاريخ',
  hint: undefined,
  error: undefined,
  disabled: false,
  required: false,
  clearable: true,
  minDate: undefined,
  maxDate: undefined,
  showToggle: true,
  defaultToHijri: true,
  containerClass: ''
})

// Emits
const emit = defineEmits<{
  'update:modelValue': [value: Date | string | null]
  'update:hijriDate': [value: HijriDate | null]
}>()

// Generate unique ID
const datePickerId = useId()

// Calendar type state
const isHijri = ref(props.defaultToHijri)

// Current view state
const currentMonth = ref(0)
const currentYear = ref(0)

// Hijri month names
const hijriMonthNames = [
  'محرم', 'صفر', 'ربيع الأول', 'ربيع الثاني',
  'جمادى الأولى', 'جمادى الآخرة', 'رجب', 'شعبان',
  'رمضان', 'شوال', 'ذو القعدة', 'ذو الحجة'
]

// Gregorian month names (Arabic)
const gregorianMonthNames = [
  'يناير', 'فبراير', 'مارس', 'إبريل', 'مايو', 'يونيو',
  'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'
]

// Month names based on calendar type
const monthNames = computed(() => {
  return isHijri.value ? hijriMonthNames : gregorianMonthNames
})

// Arabic day names (short)
const dayNames = ['ح', 'ن', 'ث', 'ر', 'خ', 'ج', 'س']

// Year range for selector
const yearRange = computed(() => {
  const years = []
  if (isHijri.value) {
    // Hijri years range (approximate)
    for (let i = 1400; i <= 1500; i++) {
      years.push(i)
    }
  } else {
    // Gregorian years range
    for (let i = 1970; i <= 2100; i++) {
      years.push(i)
    }
  }
  return years
})

// Days in current month
const daysInMonth = computed(() => {
  if (isHijri.value) {
    // Get days in Hijri month using moment-hijri
    const m = moment().iYear(currentYear.value).iMonth(currentMonth.value)
    return m.iDaysInMonth()
  } else {
    return new Date(currentYear.value, currentMonth.value + 1, 0).getDate()
  }
})

// First day of month (0 = Sunday)
const firstDayOfMonth = computed(() => {
  if (isHijri.value) {
    // Get the Gregorian date of first day of Hijri month
    const m = moment().iYear(currentYear.value).iMonth(currentMonth.value).iDate(1)
    return m.day()
  } else {
    return new Date(currentYear.value, currentMonth.value, 1).getDay()
  }
})

// Selected Hijri date
const selectedHijriDate = computed<HijriDate | null>(() => {
  if (!props.modelValue) return null

  const date = props.modelValue instanceof Date ? props.modelValue : new Date(props.modelValue)
  const m = moment(date)

  return {
    iYear: m.iYear(),
    iMonth: m.iMonth(),
    iDate: m.iDate()
  }
})

// Gregorian equivalent display
const gregorianEquivalent = computed(() => {
  if (!props.modelValue) return ''

  const date = props.modelValue instanceof Date ? props.modelValue : new Date(props.modelValue)
  const day = date.getDate()
  const month = gregorianMonthNames[date.getMonth()]
  const year = date.getFullYear()

  return `${day} ${month} ${year}م`
})

// Display value
const displayValue = computed(() => {
  if (!props.modelValue) return ''

  const date = props.modelValue instanceof Date ? props.modelValue : new Date(props.modelValue)
  const m = moment(date)

  if (isHijri.value) {
    const day = m.iDate()
    const month = hijriMonthNames[m.iMonth()]
    const year = m.iYear()
    return `${day} ${month} ${year}هـ`
  } else {
    const day = date.getDate()
    const month = gregorianMonthNames[date.getMonth()]
    const year = date.getFullYear()
    return `${day} ${month} ${year}م`
  }
})

// Initialize current month/year from model value or today
function initializeFromDate(date: Date | null) {
  if (date) {
    if (isHijri.value) {
      const m = moment(date)
      currentMonth.value = m.iMonth()
      currentYear.value = m.iYear()
    } else {
      currentMonth.value = date.getMonth()
      currentYear.value = date.getFullYear()
    }
  } else {
    // Initialize to today
    if (isHijri.value) {
      const m = moment()
      currentMonth.value = m.iMonth()
      currentYear.value = m.iYear()
    } else {
      const now = new Date()
      currentMonth.value = now.getMonth()
      currentYear.value = now.getFullYear()
    }
  }
}

// Watch model value changes
watch(
  () => props.modelValue,
  (newValue) => {
    const date = newValue instanceof Date ? newValue : (newValue ? new Date(newValue) : null)
    initializeFromDate(date)
  },
  { immediate: true }
)

// Watch calendar type changes
watch(isHijri, () => {
  const date = props.modelValue instanceof Date ? props.modelValue : (props.modelValue ? new Date(props.modelValue) : null)
  initializeFromDate(date)
})

// Toggle calendar type
function toggleCalendarType() {
  isHijri.value = !isHijri.value
}

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
  if (!selectedHijriDate.value) return false

  if (isHijri.value) {
    return (
      selectedHijriDate.value.iDate === day &&
      selectedHijriDate.value.iMonth === currentMonth.value &&
      selectedHijriDate.value.iYear === currentYear.value
    )
  } else {
    const date = props.modelValue instanceof Date ? props.modelValue : new Date(props.modelValue!)
    return (
      date.getDate() === day &&
      date.getMonth() === currentMonth.value &&
      date.getFullYear() === currentYear.value
    )
  }
}

// Check if a day is today
function isToday(day: number): boolean {
  if (isHijri.value) {
    const m = moment()
    return (
      m.iDate() === day &&
      m.iMonth() === currentMonth.value &&
      m.iYear() === currentYear.value
    )
  } else {
    const today = new Date()
    return (
      today.getDate() === day &&
      today.getMonth() === currentMonth.value &&
      today.getFullYear() === currentYear.value
    )
  }
}

// Check if date is disabled
function isDateDisabled(day: number): boolean {
  let date: Date

  if (isHijri.value) {
    const m = moment().iYear(currentYear.value).iMonth(currentMonth.value).iDate(day).startOf('day')
    date = m.toDate()
  } else {
    // Create date at start of day for comparison
    date = new Date(currentYear.value, currentMonth.value, day, 0, 0, 0, 0)
  }

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
    'w-8 h-7 flex items-center justify-center text-xs rounded transition-colors'
  ]

  if (isDateDisabled(day)) {
    return [...base, 'text-zinc-300 cursor-not-allowed']
  }

  if (isSelected(day)) {
    return [...base, 'bg-primary text-white font-medium']
  }

  if (isToday(day)) {
    return [...base, 'bg-primary/10 text-primary font-medium hover:bg-primary/20']
  }

  return [...base, 'hover:bg-zinc-100 text-zinc-700']
}

// Select date
function selectDate(day: number, close: () => void) {
  let date: Date

  if (isHijri.value) {
    // Use startOf('day') and add 12 hours to avoid timezone issues
    const m = moment().iYear(currentYear.value).iMonth(currentMonth.value).iDate(day).startOf('day').add(12, 'hours')
    date = m.toDate()
  } else {
    // Create date at noon (12:00) to avoid timezone boundary issues when converting to UTC
    date = new Date(currentYear.value, currentMonth.value, day, 12, 0, 0, 0)
  }

  emit('update:modelValue', date)

  // Emit Hijri date as well
  const hijriMoment = moment(date)
  emit('update:hijriDate', {
    iYear: hijriMoment.iYear(),
    iMonth: hijriMoment.iMonth(),
    iDate: hijriMoment.iDate()
  })

  close()
}

// Select today
function selectToday(close: () => void) {
  // Create today's date at noon to avoid timezone issues
  const now = new Date()
  const date = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 12, 0, 0, 0)
  emit('update:modelValue', date)

  const hijriMoment = moment(date)
  emit('update:hijriDate', {
    iYear: hijriMoment.iYear(),
    iMonth: hijriMoment.iMonth(),
    iDate: hijriMoment.iDate()
  })

  close()
}

// Clear date
function clearDate(close: () => void) {
  emit('update:modelValue', null)
  emit('update:hijriDate', null)
  close()
}

// Button classes
const buttonClasses = computed(() => {
  const base = [
    'relative w-full cursor-pointer rounded-lg bg-white text-start',
    'border border-gray-300 px-4 text-sm h-[44px] flex items-center',
    'transition-all duration-150',
    'hover:border-gray-400',
    'focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/20',
    'disabled:cursor-not-allowed disabled:bg-gray-50 disabled:text-gray-500 disabled:opacity-60'
  ]

  const errorClass = props.error
    ? 'border-error'
    : ''

  return [...base, errorClass]
})
</script>

<style scoped>
/* Focus style for consistency */
button:focus {
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}
</style>

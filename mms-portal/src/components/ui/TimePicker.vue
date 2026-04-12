<template>
  <div :class="['w-full', containerClass]">
    <!-- Label -->
    <label
      v-if="label"
      :for="pickerId"
      class="form-label"
    >
      {{ label }}
      <span v-if="required" class="text-error">*</span>
    </label>

    <Popover class="relative">
      <PopoverButton
        :id="pickerId"
        :disabled="disabled"
        :class="buttonClasses"
      >
        <span v-if="displayValue" class="block truncate">
          {{ displayValue }}
        </span>
        <span v-else class="block truncate text-zinc-400">
          {{ placeholder }}
        </span>

        <span class="pointer-events-none absolute inset-y-0 end-0 flex items-center pe-3">
          <Icon
            icon="mdi:clock-outline"
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
          class="absolute z-50 mt-2 w-64 origin-top-start rounded-xl bg-white p-4 shadow-lg ring-1 ring-black/5"
        >
          <div class="time-picker-content">
            <!-- Time display -->
            <div class="time-display">
              <span class="time-value">{{ selectedHour }}:{{ selectedMinute }}</span>
              <span class="time-period">{{ selectedPeriod }}</span>
            </div>

            <!-- Hour and Minute selectors -->
            <div class="time-selectors">
              <!-- Hours -->
              <div class="time-column">
                <div class="time-column-label">{{ $t('Hour') }}</div>
                <div class="time-scroll" ref="hourScrollRef">
                  <button
                    v-for="hour in hours"
                    :key="hour"
                    type="button"
                    :class="['time-option', { active: selectedHour === hour }]"
                    @click="selectHour(hour)"
                  >
                    {{ hour }}
                  </button>
                </div>
              </div>

              <!-- Minutes -->
              <div class="time-column">
                <div class="time-column-label">{{ $t('Minute') }}</div>
                <div class="time-scroll" ref="minuteScrollRef">
                  <button
                    v-for="minute in minutes"
                    :key="minute"
                    type="button"
                    :class="['time-option', { active: selectedMinute === minute }]"
                    @click="selectMinute(minute)"
                  >
                    {{ minute }}
                  </button>
                </div>
              </div>

              <!-- AM/PM -->
              <div class="time-column period-column">
                <div class="time-column-label">{{ $t('Period') }}</div>
                <div class="period-buttons">
                  <button
                    type="button"
                    :class="['period-btn', { active: selectedPeriod === 'AM' }]"
                    @click="selectPeriod('AM')"
                  >
                    {{ $t('AM') }}
                  </button>
                  <button
                    type="button"
                    :class="['period-btn', { active: selectedPeriod === 'PM' }]"
                    @click="selectPeriod('PM')"
                  >
                    {{ $t('PM') }}
                  </button>
                </div>
              </div>
            </div>

            <!-- Actions -->
            <div class="time-actions">
              <button
                type="button"
                class="time-action-btn cancel"
                @click="close()"
              >
                {{ $t('Cancel') }}
              </button>
              <button
                type="button"
                class="time-action-btn confirm"
                @click="confirmSelection(close)"
              >
                {{ $t('Confirm') }}
              </button>
            </div>
          </div>
        </PopoverPanel>
      </Transition>
    </Popover>

    <!-- Helper text / Error message -->
    <p v-if="error" class="form-error">{{ error }}</p>
    <p v-else-if="hint" class="form-helper">{{ hint }}</p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, useId } from 'vue'
import { useI18n } from 'vue-i18n'
import { Popover, PopoverButton, PopoverPanel } from '@headlessui/vue'
import Icon from '@/components/ui/Icon.vue'

const { t } = useI18n()

interface Props {
  modelValue?: string | null
  label?: string
  placeholder?: string
  hint?: string
  error?: string
  disabled?: boolean
  required?: boolean
  minuteStep?: number
  containerClass?: string
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: null,
  label: undefined,
  placeholder: 'اختر الوقت',
  hint: undefined,
  error: undefined,
  disabled: false,
  required: false,
  minuteStep: 5,
  containerClass: ''
})

const emit = defineEmits<{
  'update:modelValue': [value: string | null]
}>()

const pickerId = useId()
const hourScrollRef = ref<HTMLElement | null>(null)
const minuteScrollRef = ref<HTMLElement | null>(null)

// Hours (1-12 for 12-hour format)
const hours = computed(() => {
  const hrs = []
  for (let i = 1; i <= 12; i++) {
    hrs.push(i.toString().padStart(2, '0'))
  }
  return hrs
})

// Minutes based on step
const minutes = computed(() => {
  const mins = []
  for (let i = 0; i < 60; i += props.minuteStep) {
    mins.push(i.toString().padStart(2, '0'))
  }
  return mins
})

// Selected values
const selectedHour = ref('12')
const selectedMinute = ref('00')
const selectedPeriod = ref<'AM' | 'PM'>('AM')

// Parse model value on mount and when it changes
watch(() => props.modelValue, (newValue) => {
  if (newValue) {
    parseTimeValue(newValue)
  }
}, { immediate: true })

function parseTimeValue(value: string) {
  // Parse HH:MM format (24-hour)
  const match = value.match(/^(\d{1,2}):(\d{2})$/)
  if (match) {
    let hour = parseInt(match[1], 10)
    const minute = match[2]

    // Convert to 12-hour format
    if (hour === 0) {
      selectedHour.value = '12'
      selectedPeriod.value = 'AM'
    } else if (hour === 12) {
      selectedHour.value = '12'
      selectedPeriod.value = 'PM'
    } else if (hour > 12) {
      selectedHour.value = (hour - 12).toString().padStart(2, '0')
      selectedPeriod.value = 'PM'
    } else {
      selectedHour.value = hour.toString().padStart(2, '0')
      selectedPeriod.value = 'AM'
    }

    // Round minute to nearest step
    const minuteNum = parseInt(minute, 10)
    const roundedMinute = Math.round(minuteNum / props.minuteStep) * props.minuteStep
    selectedMinute.value = (roundedMinute % 60).toString().padStart(2, '0')
  }
}

// Display value
const displayValue = computed(() => {
  if (!props.modelValue) return ''

  const periodLabel = selectedPeriod.value === 'AM' ? t('AM') : t('PM')
  return `${selectedHour.value}:${selectedMinute.value} ${periodLabel}`
})

// Select handlers
function selectHour(hour: string) {
  selectedHour.value = hour
}

function selectMinute(minute: string) {
  selectedMinute.value = minute
}

function selectPeriod(period: 'AM' | 'PM') {
  selectedPeriod.value = period
}

// Confirm selection
function confirmSelection(close: () => void) {
  // Convert to 24-hour format
  let hour = parseInt(selectedHour.value, 10)

  if (selectedPeriod.value === 'AM') {
    if (hour === 12) hour = 0
  } else {
    if (hour !== 12) hour = hour + 12
  }

  const timeValue = `${hour.toString().padStart(2, '0')}:${selectedMinute.value}`
  emit('update:modelValue', timeValue)
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

  const errorClass = props.error ? 'border-error' : ''

  return [...base, errorClass]
})
</script>

<style scoped>
button:focus {
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

.time-picker-content {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.time-display {
  display: flex;
  align-items: baseline;
  justify-content: center;
  gap: 0.375rem;
  padding: 0.5rem;
  background: #fafafa;
  border-radius: 0.375rem;
  margin-bottom: 0.5rem;
}

.time-value {
  font-size: 1.25rem;
  font-weight: 600;
  color: #18181b;
  font-variant-numeric: tabular-nums;
}

.time-period {
  font-size: 0.75rem;
  font-weight: 500;
  color: #a1a1aa;
}

.time-selectors {
  display: flex;
  gap: 0.5rem;
}

.time-column {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.time-column-label {
  font-size: 0.75rem;
  font-weight: 500;
  color: #71717a;
  text-align: center;
  padding-bottom: 0.25rem;
}

.time-scroll {
  max-height: 120px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 0;
  scrollbar-width: thin;
  scrollbar-color: #d4d4d8 transparent;
}

.time-scroll::-webkit-scrollbar {
  width: 4px;
}

.time-scroll::-webkit-scrollbar-thumb {
  background-color: #d4d4d8;
  border-radius: 2px;
}

.time-option {
  padding: 0.375rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: #71717a;
  border: none;
  background: transparent;
  border-radius: 0.375rem;
  cursor: pointer;
  transition: all 0.15s ease;
  text-align: center;
}

.time-option:hover {
  background: #f4f4f5;
  color: #18181b;
}

.time-option.active {
  background: #f0fdf4;
  color: #059669;
  font-weight: 600;
}

.period-column {
  flex: 0.8;
}

.period-buttons {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.period-btn {
  padding: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: #71717a;
  border: 1px solid #e4e4e7;
  background: white;
  border-radius: 0.375rem;
  cursor: pointer;
  transition: all 0.15s ease;
}

.period-btn:hover {
  background: #fafafa;
  border-color: #d4d4d8;
}

.period-btn.active {
  background: #f0fdf4;
  border-color: #86efac;
  color: #059669;
  font-weight: 600;
}

.time-actions {
  display: flex;
  gap: 0.5rem;
  padding-top: 0.75rem;
  border-top: 1px solid #f4f4f5;
}

.time-action-btn {
  flex: 1;
  padding: 0.625rem 1rem;
  font-size: 0.875rem;
  font-weight: 500;
  border-radius: 0.5rem;
  cursor: pointer;
  transition: all 0.15s ease;
}

.time-action-btn.cancel {
  background: white;
  border: 1px solid #e4e4e7;
  color: #71717a;
}

.time-action-btn.cancel:hover {
  background: #fafafa;
  border-color: #d4d4d8;
}

.time-action-btn.confirm {
  background: var(--color-primary, #006d4b);
  border: 1px solid var(--color-primary, #006d4b);
  color: white;
}

.time-action-btn.confirm:hover {
  opacity: 0.9;
}
</style>

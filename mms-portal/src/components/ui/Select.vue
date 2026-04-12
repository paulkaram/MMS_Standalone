<template>
  <div :class="['w-full', containerClass]">
    <!-- Label -->
    <label
      v-if="label"
      :id="`${selectId}-label`"
      class="form-label"
    >
      {{ label }}
      <span
        v-if="required"
        class="text-error"
      >*</span>
    </label>

    <Listbox
      v-model="model"
      :disabled="disabled"
      :multiple="multiple"
      v-slot="{ open }"
    >
      <div class="relative">
        <ListboxButton
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
            {{ placeholder || t('Select') }}
          </span>

          <span class="pointer-events-none absolute inset-y-0 end-0 flex items-center pe-3">
            <Icon
              icon="mdi:chevron-down"
              class="h-5 w-5 text-zinc-400"
              aria-hidden="true"
            />
          </span>
        </ListboxButton>

        <Transition
          enter-active-class="transition duration-100 ease-out"
          enter-from-class="opacity-0 scale-95"
          enter-to-class="opacity-100 scale-100"
          leave-active-class="transition duration-75 ease-in"
          leave-from-class="opacity-100 scale-100"
          leave-to-class="opacity-0 scale-95"
        >
          <ListboxOptions
            class="absolute z-50 mt-1 w-full max-h-60 overflow-auto rounded-lg bg-white py-1 shadow-lg ring-1 ring-black/5 focus:outline-none"
          >
              <!-- Search input for filterable -->
              <div
                v-if="filterable"
                class="px-3 py-2 border-b border-zinc-100"
              >
                <input
                  v-model="searchQuery"
                  type="text"
                  class="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/20"
                  :placeholder="searchPlaceholder || t('Search')"
                  @click.stop
                >
              </div>

              <!-- Empty state -->
              <div
                v-if="filteredOptions.length === 0"
                class="px-4 py-3 text-sm text-zinc-500 text-center"
              >
                {{ emptyText || t('NoOptions') }}
              </div>

              <!-- Options -->
              <ListboxOption
                v-for="option in filteredOptions"
                v-slot="{ active, selected }"
                :key="String(getOptionValue(option))"
                :value="returnObject ? option : getOptionValue(option)"
                as="template"
              >
                <li
                  :class="[
                    active ? 'bg-primary/10 text-primary' : 'text-zinc-900',
                    'relative cursor-pointer select-none py-2.5 ps-10 pe-4'
                  ]"
                >
                  <span
                    :class="[
                      selected ? 'font-medium' : 'font-normal',
                      'block truncate'
                    ]"
                  >
                    {{ getOptionLabel(option) }}
                  </span>
                  <span
                    v-if="selected"
                    class="absolute inset-y-0 start-0 flex items-center ps-3 text-primary"
                  >
                    <Icon
                      icon="mdi:check"
                      class="h-5 w-5"
                      aria-hidden="true"
                    />
                  </span>
                </li>
              </ListboxOption>
          </ListboxOptions>
        </Transition>
      </div>
    </Listbox>

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
import { ref, computed, useId } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  Listbox,
  ListboxButton,
  ListboxOptions,
  ListboxOption
} from '@headlessui/vue'
import Icon from '@/components/ui/Icon.vue'

const { t } = useI18n()

// Types
type OptionValue = string | number | boolean | object

interface SelectOption {
  [key: string]: unknown
}

// Props
interface Props {
  modelValue?: OptionValue | OptionValue[] | null
  options: SelectOption[]
  itemText?: string
  itemValue?: string
  label?: string
  placeholder?: string
  hint?: string
  error?: string
  disabled?: boolean
  required?: boolean
  multiple?: boolean
  filterable?: boolean
  searchPlaceholder?: string
  emptyText?: string
  returnObject?: boolean
  containerClass?: string
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: null,
  itemText: 'label',
  itemValue: 'value',
  label: undefined,
  placeholder: undefined,
  hint: undefined,
  error: undefined,
  disabled: false,
  required: false,
  multiple: false,
  filterable: false,
  searchPlaceholder: undefined,
  emptyText: undefined,
  returnObject: false,
  containerClass: ''
})

// Emits
const emit = defineEmits<{
  'update:modelValue': [value: OptionValue | OptionValue[] | null]
}>()

// Generate unique ID
const selectId = useId()


// Search query for filtering
const searchQuery = ref('')

// v-model binding
const model = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

// Get option value
function getOptionValue(option: SelectOption): OptionValue {
  if (typeof option === 'object' && option !== null) {
    return option[props.itemValue] as OptionValue
  }
  return option as OptionValue
}

// Get option label
function getOptionLabel(option: SelectOption): string {
  if (typeof option === 'object' && option !== null) {
    return String(option[props.itemText] ?? '')
  }
  return String(option)
}

// Filter options based on search query
const filteredOptions = computed(() => {
  if (!props.filterable || !searchQuery.value) {
    return props.options
  }

  const query = searchQuery.value.toLowerCase()
  return props.options.filter(option => {
    const label = getOptionLabel(option).toLowerCase()
    return label.includes(query)
  })
})

// Display value for selected items
const displayValue = computed(() => {
  if (!model.value) return ''

  if (props.multiple && Array.isArray(model.value)) {
    if (model.value.length === 0) return ''
    return model.value
      .map(val => {
        const option = props.options.find(opt => {
          if (props.returnObject) {
            return JSON.stringify(opt) === JSON.stringify(val)
          }
          return getOptionValue(opt) === val
        })
        return option ? getOptionLabel(option) : val
      })
      .join(', ')
  }

  // Single select
  const option = props.options.find(opt => {
    if (props.returnObject) {
      return JSON.stringify(opt) === JSON.stringify(model.value)
    }
    return getOptionValue(opt) === model.value
  })

  return option ? getOptionLabel(option) : ''
})

// Button classes
const buttonClasses = computed(() => {
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

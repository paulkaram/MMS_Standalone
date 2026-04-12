<template>
  <div :class="['w-full', containerClass]">
    <!-- Label -->
    <label
      v-if="label"
      :id="`${comboboxId}-label`"
      class="form-label"
    >
      {{ label }}
      <span
        v-if="required"
        class="text-error"
      >*</span>
    </label>

    <HeadlessCombobox
      v-model="model"
      :disabled="disabled"
      :multiple="multiple"
      nullable
    >
      <div class="relative">
        <div class="relative">
          <ComboboxInput
            :class="inputClasses"
            :display-value="(getDisplayValue as any)"
            :placeholder="placeholder || t('SearchOrSelect')"
            @change="searchQuery = $event.target.value"
          />
          <ComboboxButton class="absolute inset-y-0 end-0 flex items-center pe-3">
            <Icon
              v-if="loading"
              icon="mdi:loading"
              class="h-5 w-5 text-zinc-400 animate-spin"
              aria-hidden="true"
            />
            <Icon
              v-else
              icon="mdi:chevron-down"
              class="h-5 w-5 text-zinc-400"
              aria-hidden="true"
            />
          </ComboboxButton>
        </div>

        <Transition
          leave-active-class="transition duration-100 ease-in"
          leave-from-class="opacity-100"
          leave-to-class="opacity-0"
          @after-leave="searchQuery = ''"
        >
          <ComboboxOptions
            class="absolute z-50 mt-1 max-h-60 w-full overflow-auto rounded-lg bg-white py-1 shadow-lg ring-1 ring-black/5 focus:outline-none"
          >
            <!-- Loading state -->
            <div
              v-if="loading"
              class="px-4 py-3 text-sm text-zinc-500 text-center"
            >
              <Icon
                icon="mdi:loading"
                class="w-5 h-5 animate-spin inline-block"
              />
              <span class="ms-2">جاري التحميل...</span>
            </div>

            <!-- Empty state -->
            <div
              v-else-if="filteredOptions.length === 0"
              class="px-4 py-3 text-sm text-zinc-500 text-center"
            >
              {{ emptyText || t('NoResultsFound') }}
            </div>

            <!-- Options -->
            <ComboboxOption
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
            </ComboboxOption>
          </ComboboxOptions>
        </Transition>
      </div>
    </HeadlessCombobox>

    <!-- Selected items (for multiple) -->
    <div
      v-if="multiple && Array.isArray(model) && model.length > 0"
      class="flex flex-wrap gap-1 mt-2"
    >
      <span
        v-for="(item, index) in model"
        :key="index"
        class="inline-flex items-center gap-1 px-2 py-1 bg-primary/10 text-primary text-sm rounded-full"
      >
        {{ getSelectedLabel(item) }}
        <button
          type="button"
          class="hover:bg-primary/20 rounded-full p-0.5"
          @click="removeItem(index)"
        >
          <Icon
            icon="mdi:close"
            class="w-3.5 h-3.5"
          />
        </button>
      </span>
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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, useId, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  Combobox as HeadlessCombobox,
  ComboboxInput,
  ComboboxButton,
  ComboboxOptions,
  ComboboxOption
} from '@headlessui/vue'
import Icon from '@/components/ui/Icon.vue'

const { t } = useI18n()

// Types
type OptionValue = string | number | boolean | object

interface ComboboxOption {
  [key: string]: unknown
}

// Props
interface Props {
  modelValue?: OptionValue | OptionValue[] | null
  options: ComboboxOption[]
  itemText?: string
  itemValue?: string
  label?: string
  placeholder?: string
  hint?: string
  error?: string
  disabled?: boolean
  required?: boolean
  multiple?: boolean
  loading?: boolean
  emptyText?: string
  returnObject?: boolean
  containerClass?: string
  // For async search
  searchFn?: (query: string) => Promise<ComboboxOption[]>
  minSearchLength?: number
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
  loading: false,
  emptyText: undefined,
  returnObject: false,
  containerClass: '',
  searchFn: undefined,
  minSearchLength: 0
})

// Emits
const emit = defineEmits<{
  'update:modelValue': [value: OptionValue | OptionValue[] | null]
  search: [query: string]
}>()

// Generate unique ID
const comboboxId = useId()

// Search query
const searchQuery = ref('')

// v-model binding
const model = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

// Get option value
function getOptionValue(option: ComboboxOption): OptionValue {
  if (typeof option === 'object' && option !== null) {
    return option[props.itemValue] as OptionValue
  }
  return option as OptionValue
}

// Get option label
function getOptionLabel(option: ComboboxOption): string {
  if (typeof option === 'object' && option !== null) {
    return String(option[props.itemText] ?? '')
  }
  return String(option)
}

// Get display value for input
function getDisplayValue(value: OptionValue | null): string {
  if (!value) return ''

  if (props.multiple) {
    return '' // Don't show anything in input for multiple selection
  }

  const option = props.options.find(opt => {
    if (props.returnObject) {
      return JSON.stringify(opt) === JSON.stringify(value)
    }
    return getOptionValue(opt) === value
  })

  return option ? getOptionLabel(option) : ''
}

// Get selected item label
function getSelectedLabel(item: OptionValue): string {
  const option = props.options.find(opt => {
    if (props.returnObject) {
      return JSON.stringify(opt) === JSON.stringify(item)
    }
    return getOptionValue(opt) === item
  })

  return option ? getOptionLabel(option) : String(item)
}

// Remove item from selection (for multiple)
function removeItem(index: number) {
  if (Array.isArray(model.value)) {
    const newValue = [...model.value]
    newValue.splice(index, 1)
    model.value = newValue
  }
}

// Filter options based on search query
// When using async search (@search event), don't filter locally - show all options from API
const filteredOptions = computed(() => {
  // If searchFn is provided or search event is being used, skip local filtering
  // Just return all options as they come from the API
  return props.options
})

// Input classes
const inputClasses = computed(() => {
  const base = [
    'w-full rounded-lg bg-white h-[44px]',
    'border border-gray-300 px-4 text-sm pe-10',
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

// Watch search query for async search
watch(searchQuery, (newQuery) => {
  if (newQuery.length >= props.minSearchLength) {
    emit('search', newQuery)
  }
})
</script>

<style scoped>
/* Focus style for input */
:deep(input:focus) {
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}
</style>

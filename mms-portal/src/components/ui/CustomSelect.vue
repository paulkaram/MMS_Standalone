<template>
  <div class="custom-select-wrapper" :class="{ disabled: disabled, dark: dark }">
    <label v-if="label" class="select-label">{{ label }}<span v-if="required" class="text-red-500 mr-1">*</span></label>
    <div
      ref="selectRef"
      class="custom-select"
      :class="{
        open: isOpen,
        'has-value': hasValue,
        'has-error': error,
        small: size === 'small',
        large: size === 'large'
      }"
      @click="toggle"
    >
      <span class="select-value" :class="{ placeholder: !hasValue }">
        {{ hasValue ? (displayLabel || findLabel()) : (placeholder || t('Select')) }}
      </span>
      <Icon icon="mdi:chevron-down" class="select-arrow" :class="{ rotated: isOpen }" />
    </div>
    <p v-if="error" class="error-message">{{ error }}</p>

    <!-- Teleport dropdown to body to avoid stacking context issues -->
    <Teleport to="body">
      <Transition name="dropdown">
        <div
          v-if="isOpen"
          ref="dropdownRef"
          class="select-dropdown-portal"
          :class="{ dark: dark }"
          :style="dropdownStyle"
        >
          <!-- Search input for searchable dropdowns -->
          <div v-if="searchable" class="search-input-wrapper">
            <Icon icon="mdi:magnify" class="search-icon" />
            <input
              ref="searchInput"
              v-model="searchQuery"
              type="text"
              class="search-input"
              :placeholder="searchPlaceholder || t('Search')"
              @click.stop
            >
          </div>

          <!-- Options list -->
          <div class="options-list">
            <div
              v-if="clearable && hasValue"
              class="select-option clear-option"
              @click.stop="clearSelection"
            >
              <Icon icon="mdi:close" class="w-4 h-4" />
              <span>{{ clearText || t('ClearSelection') }}</span>
            </div>

            <div
              v-for="option in filteredOptions"
              :key="getOptionValue(option)"
              class="select-option"
              :class="{
                selected: isSelected(option),
                disabled: option.disabled
              }"
              @click.stop="selectOption(option)"
            >
              <Icon v-if="option.icon" :icon="option.icon" class="option-icon" />
              <span class="option-label">{{ getOptionLabel(option) }}</span>
              <Icon v-if="isSelected(option)" icon="mdi:check" class="check-icon" />
            </div>

            <div v-if="filteredOptions.length === 0" class="no-options">
              {{ noOptionsText || t('NoOptions') }}
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted, nextTick } from 'vue'
import { useI18n } from 'vue-i18n'
import Icon from '@/components/ui/Icon.vue'

const { t } = useI18n()

export interface SelectOption {
  id?: string | number
  value?: string | number
  name?: string
  label?: string
  title?: string
  icon?: string
  disabled?: boolean
  [key: string]: any
}

interface Props {
  modelValue: string | number | null | undefined
  options: SelectOption[]
  label?: string
  placeholder?: string
  valueKey?: string
  labelKey?: string
  disabled?: boolean
  clearable?: boolean
  searchable?: boolean
  searchPlaceholder?: string
  clearText?: string
  noOptionsText?: string
  error?: string
  size?: 'small' | 'medium' | 'large'
  required?: boolean
  dark?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  label: '',
  placeholder: '',
  valueKey: 'id',
  labelKey: 'name',
  disabled: false,
  clearable: false,
  searchable: false,
  searchPlaceholder: '',
  clearText: '',
  noOptionsText: '',
  error: '',
  size: 'medium',
  required: false,
  dark: false
})

const emit = defineEmits<{
  'update:modelValue': [value: string | number | null]
  'change': [option: SelectOption | null]
}>()

const selectRef = ref<HTMLElement | null>(null)
const dropdownRef = ref<HTMLElement | null>(null)
const searchInput = ref<HTMLInputElement | null>(null)
const isOpen = ref(false)
const searchQuery = ref('')
const dropdownStyle = ref<Record<string, string>>({})

const getOptionValue = (option: SelectOption): string | number => {
  return option[props.valueKey] ?? option.value ?? option.id ?? ''
}

const getOptionLabel = (option: SelectOption): string => {
  return option[props.labelKey] || option.label || option.name || option.title || String(getOptionValue(option))
}

const hasValue = computed(() => {
  return props.modelValue !== null && props.modelValue !== undefined && props.modelValue !== ''
})

const selectedOption = computed(() => {
  if (!hasValue.value) return null
  return props.options.find(opt => String(getOptionValue(opt)) === String(props.modelValue)) || null
})

const displayLabel = ref('')

function findLabel(): string {
  if (!props.modelValue) return ''
  const opt = props.options.find(o => String(o[props.valueKey] ?? o.value ?? o.id ?? '') === String(props.modelValue))
  return opt ? (opt[props.labelKey] || opt.label || opt.name || opt.title || String(props.modelValue)) : ''
}

// Keep displayLabel in sync when value/options change externally
watch([() => props.modelValue, () => props.options.length], () => {
  if (!props.modelValue) {
    displayLabel.value = ''
  } else {
    const found = findLabel()
    if (found) displayLabel.value = found
  }
}, { immediate: true })

const filteredOptions = computed(() => {
  if (!props.searchable || !searchQuery.value) {
    return props.options
  }
  const query = searchQuery.value.toLowerCase()
  return props.options.filter(opt => {
    const label = getOptionLabel(opt).toLowerCase()
    return label.includes(query)
  })
})

const isSelected = (option: SelectOption): boolean => {
  return String(getOptionValue(option)) === String(props.modelValue)
}

const updateDropdownPosition = () => {
  if (!selectRef.value) return

  const rect = selectRef.value.getBoundingClientRect()
  dropdownStyle.value = {
    position: 'fixed',
    top: `${rect.bottom + 4}px`,
    left: `${rect.left}px`,
    width: `${rect.width}px`,
    zIndex: '99999'
  }
}

const toggle = () => {
  if (props.disabled) return
  isOpen.value = !isOpen.value
  if (isOpen.value) {
    updateDropdownPosition()
    if (props.searchable) {
      nextTick(() => {
        searchInput.value?.focus()
      })
    }
  }
}

const selectOption = (option: SelectOption) => {
  if (option.disabled) return
  const value = getOptionValue(option)
  displayLabel.value = getOptionLabel(option)
  emit('update:modelValue', value)
  emit('change', option)
  isOpen.value = false
  searchQuery.value = ''
}

const clearSelection = () => {
  displayLabel.value = ''
  emit('update:modelValue', null)
  emit('change', null)
  isOpen.value = false
  searchQuery.value = ''
}

const handleClickOutside = (e: MouseEvent) => {
  if (!isOpen.value) return

  const target = e.target as HTMLElement
  const clickedOnSelect = selectRef.value?.contains(target)
  const clickedOnDropdown = dropdownRef.value?.contains(target)
  // Also check if clicked inside any teleported dropdown (by CSS class)
  const clickedOnPortal = target.closest?.('.select-dropdown-portal')

  if (!clickedOnSelect && !clickedOnDropdown && !clickedOnPortal) {
    isOpen.value = false
    searchQuery.value = ''
  }
}

// Close on escape key
const handleKeydown = (e: KeyboardEvent) => {
  if (e.key === 'Escape' && isOpen.value) {
    isOpen.value = false
    searchQuery.value = ''
  }
}

// Update position on scroll/resize
const handleScrollResize = () => {
  if (isOpen.value) {
    updateDropdownPosition()
  }
}

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside)
  document.addEventListener('keydown', handleKeydown)
  window.addEventListener('scroll', handleScrollResize, true)
  window.addEventListener('resize', handleScrollResize)
})

onUnmounted(() => {
  document.removeEventListener('mousedown', handleClickOutside)
  document.removeEventListener('keydown', handleKeydown)
  window.removeEventListener('scroll', handleScrollResize, true)
  window.removeEventListener('resize', handleScrollResize)
})
</script>

<style scoped>
.custom-select-wrapper {
  @apply w-full;
}

.custom-select-wrapper.disabled {
  @apply opacity-50 pointer-events-none;
}

.select-label {
  @apply block text-sm font-medium text-gray-700 mb-1.5;
}

.custom-select {
  @apply relative flex items-center justify-between px-3 py-2.5 rounded-lg border border-gray-300 bg-white cursor-pointer;
  @apply hover:border-gray-400 transition-all duration-200;
}

.custom-select.small {
  @apply py-2 text-sm;
}

.custom-select.large {
  @apply py-3 text-base;
}

.custom-select.open {
  @apply border-primary ring-2 ring-primary/20;
}

.custom-select.has-error {
  @apply border-red-500;
}

.select-value {
  @apply text-sm text-zinc-900 truncate flex-1;
}

.select-value.placeholder {
  @apply text-zinc-400;
}

.select-arrow {
  @apply w-5 h-5 text-zinc-400 transition-transform duration-200 flex-shrink-0 mr-1;
}

.select-arrow.rotated {
  @apply rotate-180;
}

.error-message {
  @apply mt-1 text-xs text-red-500;
}

/* Dark mode styles */
.custom-select-wrapper.dark .select-label {
  @apply text-zinc-200;
}

.custom-select-wrapper.dark .custom-select {
  @apply bg-[#22252c] border-[rgba(255,255,255,0.1)] text-zinc-200;
}

.custom-select-wrapper.dark .custom-select:hover {
  @apply border-[rgba(255,255,255,0.2)];
}

.custom-select-wrapper.dark .custom-select.open {
  @apply border-amber-500 ring-amber-500/20;
}

.custom-select-wrapper.dark .select-value {
  @apply text-zinc-200;
}

.custom-select-wrapper.dark .select-value.placeholder {
  @apply text-zinc-500;
}

.custom-select-wrapper.dark .select-arrow {
  @apply text-zinc-500;
}

/* Dropdown animation */
.dropdown-enter-active,
.dropdown-leave-active {
  @apply transition-all duration-200;
}

.dropdown-enter-from,
.dropdown-leave-to {
  @apply opacity-0 -translate-y-2;
}
</style>

<style>
/* Global styles for teleported dropdown */
.select-dropdown-portal {
  background: white;
  border-radius: 12px;
  border: 1px solid #e4e4e7;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
  max-height: 256px;
  overflow: hidden;
}

.select-dropdown-portal .search-input-wrapper {
  position: relative;
  padding: 8px 12px;
  border-bottom: 1px solid #f4f4f5;
}

.select-dropdown-portal .search-icon {
  position: absolute;
  right: 20px;
  top: 50%;
  transform: translateY(-50%);
  width: 16px;
  height: 16px;
  color: #a1a1aa;
}

.select-dropdown-portal .search-input {
  width: 100%;
  padding: 6px 32px 6px 8px;
  font-size: 14px;
  border: none;
  outline: none;
  background: transparent;
}

.select-dropdown-portal .search-input::placeholder {
  color: #a1a1aa;
}

.select-dropdown-portal .options-list {
  max-height: 208px;
  overflow-y: auto;
}

.select-dropdown-portal .select-option {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  font-size: 14px;
  color: #3f3f46;
  cursor: pointer;
  transition: background-color 0.15s;
}

.select-dropdown-portal .select-option:hover {
  background-color: #fafafa;
}

.select-dropdown-portal .select-option.selected {
  background-color: rgba(5, 150, 105, 0.05);
  color: #059669;
  font-weight: 500;
}

.select-dropdown-portal .select-option.disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.select-dropdown-portal .select-option.disabled:hover {
  background-color: transparent;
}

.select-dropdown-portal .select-option.clear-option {
  color: #71717a;
  border-bottom: 1px solid #f4f4f5;
}

.select-dropdown-portal .option-icon {
  width: 16px;
  height: 16px;
  color: #a1a1aa;
  flex-shrink: 0;
}

.select-dropdown-portal .option-label {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.select-dropdown-portal .check-icon {
  width: 16px;
  height: 16px;
  color: #059669;
  flex-shrink: 0;
}

.select-dropdown-portal .no-options {
  padding: 16px 12px;
  font-size: 14px;
  color: #a1a1aa;
  text-align: center;
}

/* Dark mode dropdown styles */
.select-dropdown-portal.dark {
  background: #22252c;
  border-color: rgba(255, 255, 255, 0.1);
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.4);
}

.select-dropdown-portal.dark .search-input-wrapper {
  border-bottom-color: rgba(255, 255, 255, 0.1);
}

.select-dropdown-portal.dark .search-input {
  color: #e4e4e7;
}

.select-dropdown-portal.dark .search-input::placeholder {
  color: #71717a;
}

.select-dropdown-portal.dark .select-option {
  color: #e4e4e7;
}

.select-dropdown-portal.dark .select-option:hover {
  background-color: rgba(255, 255, 255, 0.05);
}

.select-dropdown-portal.dark .select-option.selected {
  background-color: rgba(245, 158, 11, 0.15);
  color: #fbbf24;
}

.select-dropdown-portal.dark .select-option.clear-option {
  color: #a1a1aa;
  border-bottom-color: rgba(255, 255, 255, 0.1);
}

.select-dropdown-portal.dark .option-icon {
  color: #71717a;
}

.select-dropdown-portal.dark .check-icon {
  color: #fbbf24;
}

.select-dropdown-portal.dark .no-options {
  color: #71717a;
}
</style>

<template>
  <div ref="rootEl" class="tag-picker" :class="{ disabled }">
    <div v-if="selectedTags.length > 0" class="selected-tags">
      <span
        v-for="tag in selectedTags"
        :key="tag.id"
        class="tag-chip"
        :style="{ background: tagColor(tag) + '18', color: tagColor(tag), borderColor: tagColor(tag) + '55' }"
      >
        {{ tag.name }}
        <button
          v-if="!disabled"
          type="button"
          class="chip-remove"
          @click="removeTag(tag.id)"
        >
          <Icon icon="mdi:close" class="w-3 h-3" />
        </button>
      </span>
    </div>

    <div v-if="!disabled" class="picker-input">
      <div class="dropdown-trigger" @click.stop="toggleDropdown">
        <Icon icon="mdi:tag-plus-outline" class="w-4 h-4" />
        <span>{{ $t('SelectTags') }}</span>
      </div>

      <div v-if="dropdownOpen" class="dropdown-panel" @click.stop>
        <div class="dropdown-search">
          <Icon icon="mdi:magnify" class="w-4 h-4" />
          <input
            v-model="filter"
            type="text"
            :placeholder="$t('SearchTags')"
            @click.stop
          />
        </div>
        <div class="dropdown-list">
          <div
            v-for="tag in filteredAvailableTags"
            :key="tag.id"
            class="dropdown-item"
            @click="addTag(tag)"
          >
            <span class="dot" :style="{ background: tagColor(tag) }"></span>
            {{ tag.name }}
          </div>
          <div v-if="filteredAvailableTags.length === 0" class="dropdown-empty">
            {{ $t('NoTags') }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import TagsService, { type TagPickerItem } from '@/services/TagsService'

interface Props {
  modelValue: number[]
  disabled?: boolean
}
const props = withDefaults(defineProps<Props>(), {
  disabled: false
})
const emit = defineEmits<{ 'update:modelValue': [value: number[]] }>()

const rootEl = ref<HTMLElement | null>(null)
const availableTags = ref<TagPickerItem[]>([])
const dropdownOpen = ref(false)
const filter = ref('')

const selectedTags = computed<TagPickerItem[]>(() =>
  availableTags.value.filter(t => props.modelValue.includes(t.id))
)

const filteredAvailableTags = computed<TagPickerItem[]>(() => {
  const unselected = availableTags.value.filter(t => !props.modelValue.includes(t.id))
  if (!filter.value) return unselected
  const q = filter.value.toLowerCase()
  return unselected.filter(t => t.name.toLowerCase().includes(q))
})

const tagColor = (tag: TagPickerItem): string => tag.color || '#006d4b'

const loadTags = async () => {
  try {
    // Single endpoint that returns localized name + color, no permission gate
    const res: any = await TagsService.listForPicker()
    availableTags.value = res?.data ?? res ?? []
  } catch (err) {
    console.error('Failed to load tags:', err)
  }
}

const addTag = (tag: TagPickerItem) => {
  if (!props.modelValue.includes(tag.id)) {
    emit('update:modelValue', [...props.modelValue, tag.id])
  }
  filter.value = ''
  dropdownOpen.value = false
}

const removeTag = (id: number) => {
  emit('update:modelValue', props.modelValue.filter(x => x !== id))
}

const toggleDropdown = () => {
  dropdownOpen.value = !dropdownOpen.value
  filter.value = ''
}

/**
 * Click-outside detection. We listen on BOTH mousedown and click in capture
 * phase — mousedown for snappiness, click as a fallback in case some parent
 * (e.g. Headless UI's Dialog) intercepts mousedown. composedPath() walks
 * through shadow DOM / teleported portals so the containment check holds
 * even when the dropdown sits in a different DOM subtree than rootEl.
 */
const handleOutside = (e: Event) => {
  if (!dropdownOpen.value || !rootEl.value) return
  const path = (typeof (e as any).composedPath === 'function' ? (e as any).composedPath() : []) as EventTarget[]
  const target = e.target as Node | null
  const inside =
    path.includes(rootEl.value) ||
    (target !== null && rootEl.value.contains(target))
  if (!inside) dropdownOpen.value = false
}

onMounted(() => {
  loadTags()
  document.addEventListener('mousedown', handleOutside, true)
  document.addEventListener('click', handleOutside, true)
})

onBeforeUnmount(() => {
  document.removeEventListener('mousedown', handleOutside, true)
  document.removeEventListener('click', handleOutside, true)
})

watch(() => props.modelValue, () => {
  if (availableTags.value.length === 0) loadTags()
})
</script>

<style scoped>
.tag-picker {
  display: flex;
  flex-direction: column;
  gap: 6px;
  position: relative;
}
.tag-picker.disabled { opacity: 0.7; pointer-events: none; }

.selected-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.tag-chip {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 3px 8px 3px 10px;
  font-size: 11px;
  font-weight: 600;
  border-radius: 12px;
  border: 1px solid;
}

.chip-remove {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 14px; height: 14px;
  background: transparent;
  border: none;
  border-radius: 50%;
  cursor: pointer;
  color: currentColor;
  opacity: 0.6;
  transition: opacity 0.15s;
  padding: 0;
}
.chip-remove:hover { opacity: 1; }

.picker-input { position: relative; }

.dropdown-trigger {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  background: #f7faf8;
  border: 1px dashed #c8ddd3;
  border-radius: 8px;
  font-size: 12px;
  color: #5a7a6d;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s;
}
.dropdown-trigger:hover {
  background: #eef5f1;
  border-color: #006d4b;
  color: #006d4b;
}

.dropdown-panel {
  position: absolute;
  inset-inline-start: 0;
  top: calc(100% + 4px);
  width: 260px;
  max-height: 280px;
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 10px;
  box-shadow: 0 6px 24px rgba(0, 0, 0, 0.08);
  z-index: 100;
  display: flex;
  flex-direction: column;
}

.dropdown-search {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 10px;
  border-bottom: 1px solid #e4ede8;
  color: #93afa4;
}
.dropdown-search input {
  border: none;
  outline: none;
  flex: 1;
  font-size: 13px;
  background: transparent;
  color: #1a2e25;
  font-family: inherit;
}
.dropdown-search input::placeholder { color: #a3b5ad; }

.dropdown-list {
  overflow-y: auto;
  flex: 1;
  padding: 4px;
}

.dropdown-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 10px;
  font-size: 13px;
  color: #1a2e25;
  cursor: pointer;
  border-radius: 6px;
}
.dropdown-item:hover { background: #f4f8f6; }

.dot {
  width: 10px; height: 10px;
  border-radius: 50%;
  flex-shrink: 0;
}

.dropdown-empty {
  padding: 14px;
  text-align: center;
  font-size: 12px;
  color: #93afa4;
}
</style>

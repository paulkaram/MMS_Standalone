<template>
  <TransitionRoot
    appear
    :show="modelValue"
    as="template"
  >
    <Dialog
      as="div"
      class="relative z-modal"
      @close="handleClose"
    >
      <!-- Backdrop -->
      <TransitionChild
        as="template"
        enter="duration-200 ease-out"
        enter-from="opacity-0"
        enter-to="opacity-100"
        leave="duration-150 ease-in"
        leave-from="opacity-100"
        leave-to="opacity-0"
      >
        <div class="fixed inset-0 bg-black/50 backdrop-blur-sm" />
      </TransitionChild>

      <!-- Modal container -->
      <div class="fixed inset-0 overflow-y-auto">
        <div
          :class="[
            'flex min-h-full items-center justify-center p-4',
            centered ? '' : 'sm:items-start sm:pt-20'
          ]"
        >
          <TransitionChild
            as="template"
            enter="duration-200 ease-out"
            enter-from="opacity-0 scale-95"
            enter-to="opacity-100 scale-100"
            leave="duration-150 ease-in"
            leave-from="opacity-100 scale-100"
            leave-to="opacity-0 scale-95"
          >
            <DialogPanel :class="panelClasses">
              <!-- Header -->
              <div
                v-if="$slots.header || title"
                :class="['modal-header', `modal-header--${variant}`]"
              >
                <slot name="header">
                  <div class="header-content">
                    <div class="header-icon">
                      <Icon :icon="icon || 'mdi:information'" class="w-5 h-5" />
                    </div>
                    <div class="header-text">
                      <DialogTitle as="h3" class="header-title">
                        {{ title }}
                      </DialogTitle>
                      <p v-if="description" class="header-desc">
                        {{ description }}
                      </p>
                    </div>
                  </div>
                </slot>
                <!-- Close button -->
                <button
                  v-if="!persistent"
                  type="button"
                  class="close-btn"
                  @click="handleClose"
                >
                  <Icon icon="mdi:close" class="w-5 h-5" />
                </button>
              </div>

              <!-- Body -->
              <div
                :class="[
                  noPadding ? '' : 'px-6 py-5',
                  scrollable ? 'max-h-[60vh] overflow-y-auto' : ''
                ]"
              >
                <slot />
              </div>

              <!-- Footer -->
              <div
                v-if="$slots.footer"
                class="px-6 py-4 border-t border-gray-200 bg-gray-50 flex items-center justify-end gap-3"
              >
                <slot name="footer" />
              </div>
            </DialogPanel>
          </TransitionChild>
        </div>
      </div>
    </Dialog>
  </TransitionRoot>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import {
  Dialog,
  DialogPanel,
  DialogTitle,
  TransitionRoot,
  TransitionChild
} from '@headlessui/vue'
import Icon from '@/components/ui/Icon.vue'

// Props
interface Props {
  modelValue: boolean
  title?: string
  description?: string
  icon?: string
  variant?: 'default' | 'danger' | 'warning' | 'success'
  size?: 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | '4xl' | 'full'
  persistent?: boolean
  centered?: boolean
  scrollable?: boolean
  noPadding?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  title: undefined,
  description: undefined,
  icon: undefined,
  variant: 'default',
  size: 'md',
  persistent: false,
  centered: true,
  scrollable: false,
  noPadding: false
})

// Emits
const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  close: []
}>()

// Panel classes
const panelClasses = computed(() => {
  const base = [
    'relative w-full transform overflow-hidden rounded-lg',
    'bg-white shadow-2xl ring-1 ring-black/5 transition-all'
  ]

  // Size variants
  const sizes: Record<string, string> = {
    sm: 'max-w-sm',
    md: 'max-w-md',
    lg: 'max-w-lg',
    xl: 'max-w-xl',
    '2xl': 'max-w-2xl',
    '3xl': 'max-w-3xl',
    '4xl': 'max-w-4xl',
    full: 'max-w-[90vw] min-h-[80vh]'
  }

  return [...base, sizes[props.size]]
})

// Methods
function handleClose() {
  if (!props.persistent) {
    emit('update:modelValue', false)
    emit('close')
  }
}

// Expose close method for programmatic closing
function close() {
  emit('update:modelValue', false)
  emit('close')
}

defineExpose({
  close
})
</script>

<style scoped>
/* Modal Header Base */
.modal-header {
  @apply flex items-center justify-between px-5 py-3;
}

/* Header Variants */
.modal-header--default {
  background: linear-gradient(135deg, #006d4b 0%, #006d4b 100%);
}

.modal-header--danger {
  background: linear-gradient(135deg, #7f1d1d 0%, #dc2626 100%);
}

.modal-header--warning {
  background: linear-gradient(135deg, #78350f 0%, #f59e0b 100%);
}

.modal-header--success {
  background: linear-gradient(135deg, #064e3b 0%, #10b981 100%);
}

.header-content {
  @apply flex items-center gap-3;
}

.header-icon {
  @apply w-8 h-8 rounded-lg bg-white/20 flex items-center justify-center text-white;
}

.header-text {
  @apply flex flex-col;
}

.header-title {
  @apply text-sm font-semibold text-white;
}

.header-desc {
  @apply text-sm text-white/70 mt-0.5;
}

.close-btn {
  @apply p-2 rounded-lg text-white/70 hover:text-white hover:bg-white/10 transition-colors;
}
</style>

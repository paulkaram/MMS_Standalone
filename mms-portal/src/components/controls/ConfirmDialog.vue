<template>
  <TransitionRoot
    appear
    :show="isOpen"
    as="template"
  >
    <Dialog
      as="div"
      class="relative z-modal"
      @close="handleCancel"
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

      <!-- Dialog container -->
      <div class="fixed inset-0 overflow-y-auto">
        <div class="flex min-h-full items-center justify-center p-4">
          <TransitionChild
            as="template"
            enter="duration-200 ease-out"
            enter-from="opacity-0 scale-95"
            enter-to="opacity-100 scale-100"
            leave="duration-150 ease-in"
            leave-from="opacity-100 scale-100"
            leave-to="opacity-0 scale-95"
          >
            <DialogPanel class="w-full max-w-sm transform overflow-hidden rounded-2xl bg-white shadow-xl transition-all">
              <!-- Icon -->
              <div class="flex justify-center pt-8">
                <div
                  :class="[
                    'w-16 h-16 rounded-full flex items-center justify-center',
                    iconBgClass
                  ]"
                >
                  <Icon
                    :icon="icon"
                    :class="['w-8 h-8', iconColorClass]"
                  />
                </div>
              </div>

              <!-- Content -->
              <div class="px-6 py-5 text-center">
                <DialogTitle
                  as="h3"
                  class="text-lg font-bold text-zinc-900"
                >
                  {{ options.title }}
                </DialogTitle>
                <p
                  v-if="options.message"
                  class="mt-2 text-sm text-zinc-600"
                >
                  {{ options.message }}
                </p>
              </div>

              <!-- Actions -->
              <div class="px-6 py-4 bg-zinc-50 flex items-center justify-center gap-3">
                <Button
                  variant="outline"
                  @click="handleCancel"
                >
                  {{ options.cancelText }}
                </Button>
                <Button
                  :variant="confirmVariant"
                  @click="handleConfirm"
                >
                  {{ options.confirmText }}
                </Button>
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
import Button from '@/components/ui/Button.vue'
import { useConfirm, type ConfirmType } from '@/composables/useConfirm'

const { isOpen, options, handleConfirm, handleCancel } = useConfirm()

// Computed
const icon = computed(() => {
  const icons: Record<ConfirmType, string> = {
    info: 'mdi:information-outline',
    warning: 'mdi:alert-outline',
    danger: 'mdi:trash-can-outline',
    success: 'mdi:check-circle-outline'
  }
  return icons[options.value.type || 'info']
})

const iconBgClass = computed(() => {
  const classes: Record<ConfirmType, string> = {
    info: 'bg-blue-100',
    warning: 'bg-amber-100',
    danger: 'bg-red-100',
    success: 'bg-emerald-100'
  }
  return classes[options.value.type || 'info']
})

const iconColorClass = computed(() => {
  const classes: Record<ConfirmType, string> = {
    info: 'text-blue-600',
    warning: 'text-amber-600',
    danger: 'text-red-600',
    success: 'text-emerald-600'
  }
  return classes[options.value.type || 'info']
})

const confirmVariant = computed(() => {
  const variants: Record<ConfirmType, 'primary' | 'danger' | 'success'> = {
    info: 'primary',
    warning: 'primary',
    danger: 'danger',
    success: 'success'
  }
  return variants[options.value.type || 'info']
})
</script>

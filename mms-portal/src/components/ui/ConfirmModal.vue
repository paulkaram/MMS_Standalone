<template>
  <Modal
    :model-value="modelValue"
    :title="title"
    variant="warning"
    icon="mdi:alert-circle-outline"
    size="sm"
    @update:model-value="$emit('update:modelValue', $event)"
    @close="$emit('cancel')"
  >
    <p class="text-zinc-600">{{ message }}</p>

    <template #footer>
      <Button
        variant="outline"
        @click="handleCancel"
      >
        {{ cancelText }}
      </Button>
      <Button
        :variant="confirmVariant"
        :loading="loading"
        @click="handleConfirm"
      >
        {{ confirmText }}
      </Button>
    </template>
  </Modal>
</template>

<script setup lang="ts">
import Modal from '@/components/ui/Modal.vue'
import Button from '@/components/ui/Button.vue'

interface Props {
  modelValue: boolean
  title?: string
  message?: string
  confirmText?: string
  cancelText?: string
  confirmVariant?: 'primary' | 'danger' | 'success'
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  title: 'تأكيد',
  message: 'هل أنت متأكد من هذا الإجراء؟',
  confirmText: 'تأكيد',
  cancelText: 'إلغاء',
  confirmVariant: 'primary',
  loading: false
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirm: []
  cancel: []
}>()

function handleConfirm() {
  emit('confirm')
  emit('update:modelValue', false)
}

function handleCancel() {
  emit('cancel')
  emit('update:modelValue', false)
}
</script>

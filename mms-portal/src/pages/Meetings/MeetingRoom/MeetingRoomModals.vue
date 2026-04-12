<template>
  <!-- Start Meeting Confirmation -->
  <Modal
    :model-value="showConfirmStart"
    :title="$t('StartMeeting')"
    icon="mdi:play-circle"
    size="md"
    @update:model-value="(val: boolean) => $emit('update:showConfirmStart', val)"
  >
    <p class="text-sm text-gray-600">{{ $t('ConfirmStartMeeting') }}</p>
    <template #footer>
      <button class="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-50" @click="$emit('update:showConfirmStart', false)">{{ $t('Cancel') }}</button>
      <button class="px-4 py-2 text-sm font-medium text-white rounded-lg flex items-center gap-2" style="background:linear-gradient(135deg,#006d4b 0%,#006d4b 100%)" @click="$emit('start-meeting')">
        <Icon icon="mdi:play-circle" class="w-4 h-4" />
        {{ $t('StartMeeting') }}
      </button>
    </template>
  </Modal>

  <!-- End Meeting Confirmation -->
  <Modal
    :model-value="showConfirmEnd"
    :title="$t('EndMeeting')"
    icon="mdi:stop-circle"
    variant="danger"
    size="sm"
    @update:model-value="(val: boolean) => $emit('update:showConfirmEnd', val)"
  >
    <p class="text-sm text-gray-600">{{ $t('ConfirmEndMeeting') }}</p>
    <template #footer>
      <button class="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-50" @click="$emit('update:showConfirmEnd', false)">{{ $t('Cancel') }}</button>
      <button class="px-4 py-2 text-sm font-medium text-white rounded-lg flex items-center gap-2 bg-red-600 hover:bg-red-700" @click="$emit('end-meeting')">
        <Icon icon="mdi:stop-circle" class="w-4 h-4" />
        {{ $t('EndMeeting') }}
      </button>
    </template>
  </Modal>
</template>

<script setup lang="ts">
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'

defineProps<{
  showConfirmStart: boolean
  showConfirmEnd: boolean
}>()

defineEmits<{
  (e: 'update:showConfirmStart', value: boolean): void
  (e: 'update:showConfirmEnd', value: boolean): void
  (e: 'start-meeting'): void
  (e: 'end-meeting'): void
}>()
</script>

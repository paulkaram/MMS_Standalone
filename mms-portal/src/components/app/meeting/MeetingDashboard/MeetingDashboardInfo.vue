<template>
  <Card class="w-80 shadow-lg" @click.stop>
    <div class="p-4">
      <h3 class="text-lg font-semibold mb-4 flex items-center justify-between">
        {{ $t('MeetingInfo') }}
        <button
          type="button"
          class="p-1 hover:bg-zinc-100 rounded"
          @click="$emit('close')"
        >
          <Icon icon="mdi:close" class="w-4 h-4" />
        </button>
      </h3>

      <div class="space-y-3">
        <div>
          <label class="text-sm text-zinc-500">{{ $t('Title') }}</label>
          <p class="font-medium">{{ meetingDetails.title || meetingDetails.name }}</p>
        </div>

        <div>
          <label class="text-sm text-zinc-500">{{ $t('Committee') }}</label>
          <p class="font-medium">{{ meetingDetails.committeeName || '-' }}</p>
        </div>

        <div>
          <label class="text-sm text-zinc-500">{{ $t('Date') }}</label>
          <p class="font-medium">{{ formatDate(meetingDetails.date) }}</p>
        </div>

        <div>
          <label class="text-sm text-zinc-500">{{ $t('Time') }}</label>
          <p class="font-medium">{{ meetingDetails.fromTime }} - {{ meetingDetails.toTime }}</p>
        </div>

        <div>
          <label class="text-sm text-zinc-500">{{ $t('Location') }}</label>
          <p class="font-medium">{{ meetingDetails.location || '-' }}</p>
        </div>

        <div>
          <label class="text-sm text-zinc-500">{{ $t('Status') }}</label>
          <span
            :class="[
              'inline-flex px-2 py-1 rounded-full text-xs font-medium',
              getStatusClass(meetingDetails.statusId)
            ]"
          >
            {{ meetingDetails.statusName }}
          </span>
        </div>

        <div>
          <label class="text-sm text-zinc-500">{{ $t('ReferenceNumber') }}</label>
          <p class="font-medium">{{ meetingDetails.referenceNumber || '-' }}</p>
        </div>
      </div>
    </div>
  </Card>
</template>

<script setup lang="ts">
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'

defineProps<{
  meetingDetails: any
}>()

defineEmits(['close'])

const formatDate = (date: string) => {
  if (!date) return '-'
  return new Date(date).toLocaleDateString('ar-EG')
}

const getStatusClass = (statusId: number) => {
  const classes: Record<number, string> = {
    1: 'bg-zinc-100 text-zinc-600',
    2: 'bg-blue-100 text-blue-700',
    3: 'bg-green-100 text-green-700',
    4: 'bg-yellow-100 text-yellow-700',
    5: 'bg-orange-100 text-orange-700',
    6: 'bg-teal-100 text-teal-700',
    7: 'bg-purple-100 text-purple-700',
    8: 'bg-emerald-100 text-emerald-700'
  }
  return classes[statusId] || 'bg-zinc-100 text-zinc-600'
}
</script>

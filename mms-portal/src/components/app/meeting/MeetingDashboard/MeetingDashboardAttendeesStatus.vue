<template>
  <Card class="w-80 max-h-96 overflow-hidden shadow-lg" @click.stop>
    <div class="p-4">
      <h3 class="text-lg font-semibold mb-4 flex items-center justify-between">
        {{ $t('AttendeesStatus') }}
        <button
          type="button"
          class="p-1 hover:bg-zinc-100 rounded"
          @click="$emit('close')"
        >
          <Icon icon="mdi:close" class="w-4 h-4" />
        </button>
      </h3>

      <div class="space-y-2 max-h-72 overflow-y-auto">
        <div
          v-for="attendee in attendees"
          :key="attendee.id"
          class="flex items-center justify-between p-2 bg-zinc-50 rounded-lg"
        >
          <div class="flex items-center gap-2">
            <div class="relative">
              <div class="w-8 h-8 rounded-full bg-zinc-200 flex items-center justify-center">
                <Icon
                  v-if="!attendee.hasProfilePicture"
                  icon="mdi:account"
                  class="w-5 h-5 text-zinc-500"
                />
              </div>
              <span
                :class="[
                  'absolute bottom-0 right-0 w-2.5 h-2.5 rounded-full border-2 border-white',
                  isOnline(attendee.userId) ? 'bg-green-500' : 'bg-zinc-400'
                ]"
              />
            </div>
            <div>
              <p class="text-sm font-medium">{{ attendee.fullName || attendee.userName }}</p>
              <p class="text-xs text-zinc-500">{{ attendee.roleName }}</p>
            </div>
          </div>
          <span
            :class="[
              'text-xs px-2 py-0.5 rounded-full',
              isOnline(attendee.userId)
                ? 'bg-green-100 text-green-700'
                : 'bg-zinc-100 text-zinc-600'
            ]"
          >
            {{ isOnline(attendee.userId) ? ($t('Online')) : ($t('Offline')) }}
          </span>
        </div>
      </div>

      <div class="mt-4 pt-4 border-t border-zinc-200">
        <div class="flex items-center justify-between text-sm">
          <span class="text-zinc-600">{{ $t('TotalAttendees') }}</span>
          <span class="font-semibold">{{ onlineCount }} / {{ attendees.length }}</span>
        </div>
      </div>
    </div>
  </Card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'

const props = defineProps<{
  meetingId: number
  meetingOwner: boolean
  attendees: any[]
  currentAttendanceList: string[]
}>()

defineEmits(['close'])

const isOnline = (userId: string) => {
  return props.currentAttendanceList.includes(userId)
}

const onlineCount = computed(() => {
  return props.attendees.filter(a => isOnline(a.userId)).length
})
</script>

<template>
  <Card class="p-4">
    <div class="flex items-center gap-4 overflow-x-auto">
      <div
        v-for="attendee in attendees"
        :key="attendee.id"
        class="flex-shrink-0 flex items-center gap-2"
      >
        <div class="relative">
          <div
            :class="[
              'w-10 h-10 rounded-full bg-zinc-200 flex items-center justify-center',
              isOnline(attendee.userId) ? 'ring-2 ring-green-500' : ''
            ]"
          >
            <Icon
              v-if="!attendee.hasProfilePicture"
              icon="mdi:account"
              class="w-6 h-6 text-zinc-500"
            />
            <img
              v-else
              :src="getProfileUrl(attendee.userId)"
              :alt="attendee.fullName"
              class="w-full h-full rounded-full object-cover"
            />
          </div>
          <span
            :class="[
              'absolute bottom-0 right-0 w-3 h-3 rounded-full border-2 border-white',
              isOnline(attendee.userId) ? 'bg-green-500' : 'bg-zinc-400'
            ]"
          />
        </div>
        <span class="text-sm text-zinc-700 whitespace-nowrap">
          {{ attendee.fullName || attendee.userName }}
        </span>
      </div>
    </div>
  </Card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'

const props = defineProps<{
  attendees: any[]
  currentAttendanceList: string[]
}>()

const isOnline = (userId: string) => {
  return props.currentAttendanceList.includes(userId)
}

const getProfileUrl = (userId: string) => {
  const apiUrl = import.meta.env.VITE_API_URL || ''
  return `${apiUrl}users/${userId}/profile-image`
}
</script>

<template>
  <Teleport to="body">
    <TransitionGroup
      name="notification"
      tag="div"
      class="meeting-notifications-container"
    >
      <div
        v-for="meeting in visibleMeetings"
        :key="meeting.id"
        class="meeting-notification-card"
      >
        <!-- Live indicator -->
        <div class="live-indicator">
          <span class="pulse-dot"></span>
          <span class="live-text">{{ $t('Live') }}</span>
        </div>

        <!-- Content -->
        <div class="notification-content">
          <h4 class="meeting-title">{{ meeting.title }}</h4>
          <p v-if="meeting.committeeName" class="committee-name">
            {{ meeting.committeeName }}
          </p>
        </div>

        <!-- Join button -->
        <button class="join-btn" @click="joinMeeting(meeting.id)">
          {{ $t('Join') }}
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" class="join-icon">
            <path
              d="M5 12H19M19 12L12 5M19 12L12 19"
              stroke="currentColor"
              stroke-width="2.5"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
          </svg>
        </button>

        <!-- Close button -->
        <button
          class="dismiss-btn"
          @click="dismissMeeting(meeting.id)"
          :aria-label="$t('Close')"
        >
          <svg width="10" height="10" viewBox="0 0 14 14" fill="none">
            <path
              d="M1 1L13 13M1 13L13 1"
              stroke="currentColor"
              stroke-width="2"
              stroke-linecap="round"
            />
          </svg>
        </button>
      </div>
    </TransitionGroup>
  </Teleport>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useMeetingNotificationsStore } from '@/stores/meetingNotifications'

const router = useRouter()
const notificationsStore = useMeetingNotificationsStore()

const visibleMeetings = computed(() => notificationsStore.visibleMeetings)

function dismissMeeting(meetingId: number) {
  notificationsStore.dismissMeeting(meetingId)
}

function joinMeeting(meetingId: number) {
  router.push({ name: 'meetingRoom', params: { id: meetingId.toString() } })
}
</script>

<style scoped>
.meeting-notifications-container {
  position: fixed;
  bottom: 24px;
  left: 24px;
  z-index: 99999;
  display: flex;
  flex-direction: column-reverse;
  gap: 12px;
  pointer-events: none;
}

.meeting-notification-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  background: #ffffff;
  border-radius: 50px;
  box-shadow:
    0 4px 20px rgba(0, 0, 0, 0.12),
    0 0 0 1px rgba(0, 0, 0, 0.04);
  pointer-events: auto;
  transform-origin: bottom left;
  animation: slideIn 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(20px) scale(0.95);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

/* Live indicator badge */
.live-indicator {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 5px 10px;
  background: #dc2626;
  border-radius: 20px;
  color: white;
  font-size: 11px;
  font-weight: 600;
}

.pulse-dot {
  width: 6px;
  height: 6px;
  background: white;
  border-radius: 50%;
  animation: pulse 1.5s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.4;
  }
}

/* Content */
.notification-content {
  flex: 1;
  min-width: 0;
}

.meeting-title {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
  color: #27272a;
  line-height: 1.3;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 200px;
}

.committee-name {
  margin: 2px 0 0 0;
  font-size: 12px;
  color: #71717a;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* Join button */
.join-btn {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 8px 16px;
  background: #27272a;
  border: none;
  border-radius: 20px;
  color: white;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.join-btn:hover {
  background: #18181b;
}

.join-icon {
  flex-shrink: 0;
  width: 14px;
  height: 14px;
}

[dir="rtl"] .join-icon {
  transform: scaleX(-1);
}

/* Dismiss button */
.dismiss-btn {
  flex-shrink: 0;
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: none;
  border-radius: 50%;
  color: #a1a1aa;
  cursor: pointer;
  transition: all 0.2s ease;
  margin-right: -4px;
}

.dismiss-btn:hover {
  background: #f4f4f5;
  color: #71717a;
}

/* Transitions */
.notification-enter-active {
  animation: slideIn 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.notification-leave-active {
  animation: slideOut 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes slideOut {
  from {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
  to {
    opacity: 0;
    transform: translateY(20px) scale(0.95);
  }
}

.notification-move {
  transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

/* Responsive adjustments */
@media (max-width: 480px) {
  .meeting-notifications-container {
    left: 12px;
    right: 12px;
    bottom: 12px;
  }

  .meeting-notification-card {
    border-radius: 16px;
    padding: 10px 14px;
  }

  .meeting-title {
    max-width: 140px;
  }
}
</style>

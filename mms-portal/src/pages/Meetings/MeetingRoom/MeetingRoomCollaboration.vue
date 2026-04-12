<template>
  <section class="panel" :class="{ collapsed }">
    <!-- Collapsed State -->
    <div v-if="collapsed" class="collapsedPanel">
      <button class="expandBtn" @click="$emit('toggle-collapse')" :title="$t('ExpandPanel')">
        <Icon :icon="isRTL ? 'chevron_right' : 'chevron_left'" style="font-size: 18px;" />
      </button>
      <div class="collapsedIcons">
        <div class="collapsedIcon" :title="$t('Attendance')">
          <Icon icon="group" style="font-size: 16px;" />
          <span class="badge">{{ presentCount }}/{{ attendees.length }}</span>
        </div>
        <div class="collapsedIcon" :title="$t('Chat')">
          <Icon icon="forum" style="font-size: 16px;" />
          <span class="badge" v-if="chatMessages.length">{{ chatMessages.length }}</span>
        </div>
        <div class="collapsedStatus" :class="getStatusClass()">
          <Icon icon="wifi" style="font-size: 14px;" />
        </div>
      </div>
    </div>

    <!-- Expanded State -->
    <template v-else>
    <div class="panelHeader">
      <div class="titleRow">
        <button class="collapseBtn" @click="$emit('toggle-collapse')" :title="$t('CollapsePanel')">
          <Icon :icon="isRTL ? 'chevron_left' : 'chevron_right'" style="font-size: 16px;" />
        </button>
        <div class="badge">
          <Icon icon="group" style="font-size: 18px;" />
        </div>
        <div>
          <h3>{{ $t('Collaboration') }}</h3>
          <div class="sub">{{ $t('AttendanceAndChat') }}</div>
        </div>
      </div>
      <div class="statusIndicator" :class="getStatusClass()">
        <Icon icon="wifi" style="font-size: 14px;" />
      </div>
    </div>

    <div class="panelBody">
      <!-- Attendees Card -->
      <div class="card" style="flex: 0 0 46%;">
        <div class="cardHeader">
          <div class="label"><Icon icon="person_check" style="font-size: 14px;" /> {{ $t('Attendance') }}</div>
          <div class="sub">{{ presentCount }}/{{ attendees.length }} {{ $t('PresentLabel') }}</div>
        </div>
        <div class="cardBody">
          <div class="attendeesList">
            <div
              v-for="att in attendees"
              :key="att.userId || att.id"
              class="attendeeItem"
              :class="{ online: isAttendeeOnline(att) }"
            >
              <!-- Avatar with online indicator -->
              <UserAvatar
                :userId="att.userId || att.id"
                :name="att.userFullName || att.fullName || ''"
                :online="isAttendeeOnline(att)"
                show-status
                size="sm"
              />

              <!-- Name -->
              <div class="attendeeInfo">
                <strong class="attendeeName">{{ att.userFullName || att.fullName || att.name || att.userName || $t('User') }}</strong>
              </div>

              <!-- Status & Actions -->
              <div class="attendeeStatus">
                <!-- Attendance Actions (only for creator when meeting is live) -->
                <div class="attendanceActions" v-if="canControl && isLive">
                  <button
                    class="actionBtn present"
                    :class="{ active: att.attended }"
                    @click="$emit('mark-attendance', att, true)"
                    :title="$t('MarkPresent')"
                  >
                    <Icon icon="check" style="font-size: 12px;" />
                  </button>
                  <button
                    class="actionBtn absent"
                    :class="{ active: att.attended === false }"
                    @click="$emit('mark-attendance', att, false)"
                    :title="$t('MarkAbsent')"
                  >
                    <Icon icon="close" style="font-size: 12px;" />
                  </button>
                </div>

                <!-- Attendance Status (when not controlling) -->
                <div v-else-if="isLive" class="attendanceStatus">
                  <span v-if="att.attended" class="statusBadge attended">
                    <Icon icon="check" style="font-size: 10px;" /> {{ $t('PresentLabel') }}
                  </span>
                  <span v-else-if="att.attended === false" class="statusBadge absent">
                    <Icon icon="close" style="font-size: 10px;" /> {{ $t('AbsentLabel') }}
                  </span>
                  <span v-else class="statusBadge pending">
                    <Icon icon="schedule" style="font-size: 10px;" /> {{ $t('AwaitingApproval') }}
                  </span>
                </div>
              </div>
            </div>

            <div v-if="!attendees.length" class="emptyState">
              <Icon icon="group" style="font-size: 24px;" />
              <span>{{ $t('NoAttendees') }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Chat Card -->
      <div class="card" style="flex: 1 1 auto;">
        <div class="cardHeader">
          <div class="label"><Icon icon="forum" style="font-size: 14px;" /> {{ $t('RoomChat') }}</div>
          <div class="chatCount" v-if="chatMessages.length">{{ chatMessages.length }}</div>
        </div>
        <div class="cardBody chatBody">
          <div class="chatWrap">
            <div class="messages" ref="chatMessagesRef">
              <div
                v-for="(msg, idx) in chatMessages"
                :key="idx"
                class="chatMessage"
                :class="{ me: msg.me }"
              >
                <!-- Avatar -->
                <UserAvatar
                  :userId="msg.userId"
                  :name="msg.userName || ''"
                  size="xs"
                />

                <!-- Message Content -->
                <div class="msgContent">
                  <div class="msgHeader">
                    <span class="msgAuthor">{{ msg.userName }}</span>
                    <span class="msgTime">{{ formatMessageTime(msg.sentAt || msg.createdAt || msg.timestamp) }}</span>
                  </div>
                  <div class="msgBubble" :class="{ me: msg.me }">
                    <span class="msgText">{{ msg.messageText }}</span>
                  </div>
                </div>
              </div>

              <!-- Empty State -->
              <div v-if="!chatMessages.length" class="emptyChatState">
                <div class="emptyChatIcon">
                  <Icon icon="forum" style="font-size: 28px;" />
                </div>
                <span class="emptyChatTitle">{{ $t('NoMessages') }}</span>
                <span class="emptyChatSub">{{ $t('StartChatWithParticipants') }}</span>
              </div>
            </div>

            <!-- Chat Input -->
            <div class="chatInput" :class="{ disabled: !isLive }">
              <input
                :value="chatInputText"
                @input="$emit('update:chatInputText', ($event.target as HTMLInputElement).value)"
                class="input"
                :placeholder="isLive ? $t('TypeMessage') : $t('ChatAvailableDuringMeeting')"
                maxlength="160"
                :disabled="!isLive"
                @keydown.enter="isLive && $emit('send-message')"
              />
              <button class="btn primary" @click="$emit('send-message')" :disabled="!isLive || !chatInputText.trim()">
                <Icon icon="send" style="font-size: 14px;" /> {{ $t('Send') }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
    </template>
  </section>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import { getLocale } from '@/plugins/i18n'

const isRTL = computed(() => getLocale() === 'ar')

const props = defineProps<{
  attendees: any[]
  chatMessages: any[]
  chatInputText: string
  presentCount: number
  canControl: boolean
  isLive: boolean
  meetingStatusId: number
  onlineAttendeeIds: string[]
  collapsed: boolean
}>()

defineEmits<{
  (e: 'mark-attendance', att: any, present: boolean): void
  (e: 'send-message'): void
  (e: 'update:chatInputText', value: string): void
  (e: 'toggle-collapse'): void
}>()

const chatMessagesRef = ref<HTMLElement | null>(null)

// Expose chatMessagesRef for parent to scroll
defineExpose({ chatMessagesRef })

// Helper functions
const isAttendeeOnline = (att: any) => {
  const id = String(att.userId || att.id)
  const isOnline = props.onlineAttendeeIds.some(oid => String(oid) === id)
  return isOnline
}

// Meeting status indicator class
// 1=Draft, 2=PendingApproval, 3=Approved, 4=Started, 5=Finished,
// 6=PendingInitialMOM, 7=InitialMOMApproved, 8=PendingFinalMOM, 9=FinalMOMSigned, 10=Canceled
const getStatusClass = () => {
  const status = props.meetingStatusId || 0
  if (status === 4) return 'running' // Started - blue, animated
  if (status === 9 || status === 10) return 'completed' // FinalMOMSigned or Canceled - red (closed)
  if (status >= 5 && status <= 8) return 'finished' // Finished, pending MOM states - green
  if (status === 3) return 'approved' // Approved - ready to start
  return 'pending' // Draft, PendingApproval - gray
}

const formatMessageTime = (timestamp: string | undefined) => {
  if (!timestamp) return ''
  try {
    const date = new Date(timestamp)
    return date.toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit', hour12: false })
  } catch {
    return ''
  }
}

</script>

<style scoped>
/* Attendees List */
.attendeesList {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.attendeeItem {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 8px;
  transition: all 0.15s ease;
}

.attendeeItem:hover {
  background: rgba(255, 255, 255, 0.06);
}

.attendeeItem.online {
  background: rgba(0, 217, 126, 0.05);
}

/* Attendee Info */
.attendeeInfo {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.attendeeName {
  font-size: 13px;
  font-weight: 600;
  color: #f1f5f9;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}


/* Status & Actions */
.attendeeStatus {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 6px;
}

.statusBadge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 3px 8px;
  border-radius: 10px;
  font-size: 10px;
  font-weight: 500;
}

.statusBadge.online {
  background: rgba(34, 197, 94, 0.15);
  color: #22c55e;
}

.statusBadge.offline {
  background: rgba(107, 114, 128, 0.15);
  color: #a1a1aa;
}

.statusBadge.attended {
  background: rgba(34, 197, 94, 0.15);
  color: #22c55e;
}

.statusBadge.absent {
  background: rgba(239, 68, 68, 0.15);
  color: #ef4444;
}

.statusBadge.pending {
  background: rgba(107, 114, 128, 0.15);
  color: #a1a1aa;
}

/* Attendance Actions */
.attendanceActions {
  display: flex;
  gap: 4px;
}

.actionBtn {
  width: 26px;
  height: 26px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s ease;
}

.actionBtn.present {
  background: rgba(34, 197, 94, 0.1);
  color: #71717a;
}

.actionBtn.present:hover,
.actionBtn.present.active {
  background: #22c55e;
  color: #fff;
}

.actionBtn.absent {
  background: rgba(239, 68, 68, 0.1);
  color: #71717a;
}

.actionBtn.absent:hover,
.actionBtn.absent.active {
  background: #ef4444;
  color: #fff;
}

/* Empty State */
.emptyState {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 24px;
  color: #71717a;
  gap: 8px;
}

.emptyState span {
  font-size: 12px;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* ENHANCED CHAT STYLES */
/* ═══════════════════════════════════════════════════════════════════════════ */

.chatBody {
  padding: 8px !important;
}

.chatWrap {
  height: 100%;
  display: flex;
  flex-direction: column;
  min-height: 0;
  overflow: hidden;
}

.messages {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 4px;
}

.messages::-webkit-scrollbar {
  width: 4px;
}
.messages::-webkit-scrollbar-track {
  background: transparent;
}
.messages::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 2px;
}

/* Chat Message - Others aligned right (RTL), Me aligned left */
.chatMessage {
  display: flex;
  gap: 6px;
  align-items: flex-end;
  max-width: 88%;
  margin-inline-start: 0;
  margin-inline-end: auto;
}

.chatMessage.me {
  flex-direction: row-reverse;
  margin-inline-start: auto;
  margin-inline-end: 0;
}

/* Message Content */
.msgContent {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.chatMessage.me .msgContent {
  align-items: flex-end;
}

.msgHeader {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 0 2px;
}

.chatMessage.me .msgHeader {
  flex-direction: row-reverse;
}

.msgAuthor {
  font-size: 11px;
  font-weight: 600;
  color: #e2e8f0;
}

.msgTime {
  font-size: 10px;
  color: #94a3b8;
  font-variant-numeric: tabular-nums;
}

/* Message Bubble */
.msgBubble {
  padding: 8px 12px;
  border-radius: 12px;
  border-top-right-radius: 4px;
  background: #2a2d35;
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.msgBubble.me {
  background: rgba(0, 217, 126, 0.12);
  border-color: rgba(0, 217, 126, 0.2);
  border-top-right-radius: 12px;
  border-top-left-radius: 4px;
}

.msgText {
  font-size: 12px;
  line-height: 1.4;
  color: #e2e8f0;
  word-wrap: break-word;
}

.msgBubble.me .msgText {
  color: #d1fae5;
}

/* Empty Chat State */
.emptyChatState {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 24px 12px;
  gap: 8px;
}

.emptyChatIcon {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: rgba(100, 116, 139, 0.1);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #64748b;
}

.emptyChatTitle {
  font-size: 12px;
  font-weight: 600;
  color: #94a3b8;
}

.emptyChatSub {
  font-size: 11px;
  color: #64748b;
}

/* Chat Input - use global styles */
.chatInput {
  margin-top: 8px;
  display: flex;
  gap: 6px;
  align-items: center;
}

.chatInput.disabled {
  opacity: 0.6;
}

.chatInput.disabled input {
  cursor: not-allowed;
  background: rgba(255, 255, 255, 0.02);
}

.chatInput.disabled button {
  cursor: not-allowed;
  opacity: 0.5;
}

.chatCount {
  background: rgba(0, 217, 126, 0.15);
  color: #00d97e;
  padding: 2px 8px;
  border-radius: 10px;
  font-size: 10px;
  font-weight: 600;
}

/* Status Indicator */
.statusIndicator {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: rgba(107, 114, 128, 0.15);
  color: #71717a;
  border: 1px solid rgba(107, 114, 128, 0.3);
  transition: all 0.3s ease;
}

/* Pending/Draft - Gray */
.statusIndicator.pending {
  background: rgba(107, 114, 128, 0.15);
  color: #71717a;
  border-color: rgba(107, 114, 128, 0.3);
}

/* Approved - Ready to start - Orange */
.statusIndicator.approved {
  background: rgba(245, 158, 11, 0.15);
  color: #f59e0b;
  border-color: rgba(245, 158, 11, 0.3);
}

/* Running/Started - Blue with animation */
.statusIndicator.running {
  background: rgba(59, 130, 246, 0.15);
  color: #3b82f6;
  border-color: rgba(59, 130, 246, 0.4);
  animation: statusPulse 1.5s ease-in-out infinite;
  box-shadow: 0 0 12px rgba(59, 130, 246, 0.4);
}

.statusIndicator.running svg {
  animation: wifiPulse 1.5s ease-in-out infinite;
}

/* Finished - Green */
.statusIndicator.finished {
  background: rgba(34, 197, 94, 0.15);
  color: #22c55e;
  border-color: rgba(34, 197, 94, 0.3);
}

/* Completed/Closed - Red */
.statusIndicator.completed {
  background: rgba(239, 68, 68, 0.15);
  color: #ef4444;
  border-color: rgba(239, 68, 68, 0.3);
}

@keyframes statusPulse {
  0%, 100% {
    box-shadow: 0 0 8px rgba(59, 130, 246, 0.3);
    transform: scale(1);
  }
  50% {
    box-shadow: 0 0 16px rgba(59, 130, 246, 0.5);
    transform: scale(1.05);
  }
}

@keyframes wifiPulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

</style>

<!-- Light theme overrides - non-scoped to properly cascade -->
<style>
.meeting-room.light .attendeeItem {
  background: rgba(15, 23, 42, 0.03) !important;
}

.meeting-room.light .attendeeItem:hover {
  background: rgba(15, 23, 42, 0.06) !important;
}

.meeting-room.light .attendeeItem.online {
  background: rgba(5, 150, 105, 0.06) !important;
}

.meeting-room.light .attendeeName {
  color: #0f172a !important;
}

.meeting-room.light .statusBadge.online,
.meeting-room.light .statusBadge.attended {
  background: rgba(5, 150, 105, 0.12) !important;
  color: #059669 !important;
}

.meeting-room.light .statusBadge.absent {
  background: rgba(220, 38, 38, 0.12) !important;
  color: #dc2626 !important;
}

.meeting-room.light .statusBadge.offline,
.meeting-room.light .statusBadge.pending {
  background: rgba(100, 116, 139, 0.12) !important;
  color: #64748b !important;
}

.meeting-room.light .actionBtn.present {
  background: rgba(5, 150, 105, 0.1) !important;
  color: #64748b !important;
}

.meeting-room.light .actionBtn.present:hover,
.meeting-room.light .actionBtn.present.active {
  background: #059669 !important;
  color: #fff !important;
}

.meeting-room.light .actionBtn.absent {
  background: rgba(220, 38, 38, 0.1) !important;
  color: #64748b !important;
}

.meeting-room.light .actionBtn.absent:hover,
.meeting-room.light .actionBtn.absent.active {
  background: #dc2626 !important;
  color: #fff !important;
}

/* Light theme - Chat */
/* Light theme - Chat messages */
.meeting-room.light .msgBubble {
  background: #f1f5f9 !important;
  border: none !important;
  border-radius: 12px 12px 12px 4px !important;
  padding: 8px 12px !important;
}

[dir="rtl"] .meeting-room.light .msgBubble {
  border-radius: 12px 12px 4px 12px !important;
}

.meeting-room.light .msgBubble.me {
  background: linear-gradient(135deg, #006d4b 0%, #006d4b 100%) !important;
  border: none !important;
  border-radius: 12px 12px 4px 12px !important;
}

[dir="rtl"] .meeting-room.light .msgBubble.me {
  border-radius: 12px 12px 12px 4px !important;
}

.meeting-room.light .msgText {
  color: #1a1a1a !important;
  font-size: 12px !important;
}

.meeting-room.light .msgBubble.me .msgText {
  color: #fff !important;
}

.meeting-room.light .msgAuthor {
  color: #1a1a1a !important;
  font-weight: 600 !important;
  font-size: 11px !important;
}

.meeting-room.light .msgTime {
  color: #94a3b8 !important;
  font-size: 10px !important;
}

.meeting-room.light .chatCount {
  background: rgba(0, 109, 75, 0.1) !important;
  color: #006d4b !important;
  font-size: 10px !important;
  font-weight: 700 !important;
  padding: 2px 6px !important;
  border-radius: 10px !important;
}

/* Light theme - Chat empty state */
.meeting-room.light .emptyChatIcon {
  width: 52px !important;
  height: 52px !important;
  border-radius: 14px !important;
  background: rgba(0, 109, 75, 0.1) !important;
  color: #006d4b !important;
}

.meeting-room.light .emptyChatTitle {
  color: #1a1a1a !important;
  font-size: 13px !important;
  font-weight: 700 !important;
}

.meeting-room.light .emptyChatSub {
  color: #94a3b8 !important;
  font-size: 11px !important;
  text-align: center;
}

.meeting-room.light .emptyState {
  color: #64748b !important;
}

/* Light theme - Chat Input */
.meeting-room.light .chatInput {
  background: transparent;
  border: none;
  padding: 10px 12px;
  margin-top: 0;
  gap: 8px;
}

.meeting-room.light .chatInput input,
.meeting-room.light .chatInput .input {
  background: #fff !important;
  color: #1a1a1a !important;
  border: 1px solid #e0e0e0 !important;
  border-radius: 8px !important;
  padding: 8px 12px !important;
  font-size: 13px !important;
  flex: 1 !important;
  min-width: 0 !important;
}

.meeting-room.light .chatInput input::placeholder,
.meeting-room.light .chatInput .input::placeholder {
  color: #94a3b8 !important;
}

.meeting-room.light .chatInput input:focus,
.meeting-room.light .chatInput .input:focus {
  border-color: #006d4b !important;
  box-shadow: 0 0 0 2px rgba(0, 109, 75, 0.1) !important;
}

.meeting-room.light .chatInput .btn.primary {
  background: linear-gradient(135deg, #006d4b 0%, #006d4b 100%) !important;
  border: none !important;
  color: #fff !important;
  border-radius: 8px !important;
  padding: 8px 14px !important;
  font-weight: 600 !important;
  font-size: 12px !important;
  white-space: nowrap;
  flex-shrink: 0 !important;
}

.meeting-room.light .chatInput .btn.primary:hover:not(:disabled) {
  opacity: 0.9;
}

.meeting-room.light .chatInput .btn.primary:disabled {
  opacity: 0.4;
}
</style>

<template>
  <Modal
    :model-value="show"
    :title="title || $t('SendMOMForApproval')"
    icon="mdi:send"
    size="lg"
    no-padding
    @update:model-value="(val: boolean) => { if (!val) $emit('close') }"
  >

          <!-- Body -->
          <div class="modalBody">
            <!-- Select All Toggle -->
            <div class="selectAllRow">
              <label class="checkboxLabel selectAll">
                <input
                  type="checkbox"
                  :checked="allSelected"
                  :indeterminate="someSelected && !allSelected"
                  @change="toggleSelectAll"
                />
                <span class="checkmark">
                  <Icon v-if="allSelected" icon="check" style="font-size: 12px;" />
                  <Icon v-else-if="someSelected" icon="remove" style="font-size: 12px;" />
                </span>
                <span>{{ allSelected ? $t('DeselectAll') : $t('SelectAll') }}</span>
              </label>
              <span class="selectedCount">{{ selectedCount }} / {{ attendees.length }}</span>
            </div>

            <!-- Attendees List -->
            <div class="attendeesList">
              <div
                v-for="attendee in attendees"
                :key="getAttendeeId(attendee)"
                class="attendeeItem"
                :class="{
                  selected: isSelected(attendee),
                  disabled: isPendingUser(attendee)
                }"
                @click="!isPendingUser(attendee) && toggleAttendee(attendee)"
              >
                <span class="checkmark" :class="{ pendingMark: isPendingUser(attendee) }">
                  <Icon v-if="isSelected(attendee)" icon="mdi:check" class="w-3 h-3" />
                  <Icon v-else-if="isPendingUser(attendee)" icon="mdi:clock-outline" class="w-3 h-3" />
                </span>
                <UserAvatar :userId="getAttendeeId(attendee)" :name="getAttendeeName(attendee)" size="sm" />
                <div class="attendeeInfo">
                  <span class="attendeeName">{{ getAttendeeName(attendee) }}</span>
                  <span v-if="isPendingUser(attendee)" class="attendeeStatus pending">{{ $t('AwaitingApproval') }}</span>
                  <span v-else-if="getAttendeeRole(attendee)" class="attendeeRole">{{ getAttendeeRole(attendee) }}</span>
                </div>
                <!-- Signature Toggle (only for Final MOM) -->
                <button
                  v-if="showSignatureOption && isSelected(attendee)"
                  type="button"
                  class="signatureToggle"
                  :class="{ active: needsSignature(attendee) }"
                  @click.stop="toggleSignature(attendee)"
                  :title="needsSignature(attendee) ? $t('NeedsSignature') : $t('NoSignatureNeeded')"
                >
                  <Icon icon="edit" style="font-size: 14px;" />
                  <span>{{ needsSignature(attendee) ? $t('Signature') : $t('NoSignature') }}</span>
                </button>
              </div>
            </div>

          </div>

          <!-- Due Date (Optional) - outside modalBody to avoid overflow clip -->
          <div class="dueDateSection">
            <label class="dueDateLabel">
              <Icon icon="calendar_today" style="font-size: 14px;" />
              <span>{{ $t('DueDateOptional') }}</span>
            </label>
            <DatePicker
              v-model="dueDate"
              :placeholder="$t('ChooseDueDate')"
              clearable
              size="small"
            />
          </div>

          <template #footer>
            <button class="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-50" @click="$emit('close')">{{ $t('Cancel') }}</button>
            <button
              class="px-4 py-2 text-sm font-medium text-white rounded-lg flex items-center gap-2 disabled:opacity-50"
              style="background:linear-gradient(135deg,#006d4b 0%,#006d4b 100%)"
              :disabled="selectedUserIds.length === 0 || sending"
              @click="handleSend"
            >
              <Icon v-if="sending" icon="mdi:loading" class="w-4 h-4 animate-spin" />
              <Icon v-else icon="mdi:send" class="w-4 h-4" />
              {{ sending ? $t('SendingLabel') : (submitLabel || $t('Send')) }}
            </button>
          </template>
  </Modal>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import DatePicker from '@/components/ui/DatePicker.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import type { MeetingAttendee } from './types'

interface Props {
  show: boolean
  attendees: MeetingAttendee[]
  sending: boolean
  /** User IDs who already have pending approval tasks for current version */
  pendingUserIds?: string[]
  /** Modal title */
  title?: string
  /** Submit button label */
  submitLabel?: string
  /** Show signature option (for Final MOM) */
  showSignatureOption?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  pendingUserIds: () => [],
  title: '',
  submitLabel: '',
  showSignatureOption: false
})

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'send', userIds: string[], dueDateDays?: number, signatureUserIds?: string[]): void
}>()

// State
const selectedUserIds = ref<string[]>([])
const signatureUserIds = ref<string[]>([])
const dueDate = ref<Date | null>(null)

// Min date is today
const minDate = computed(() => new Date())

// Helper functions (defined before computed that use them)
function getAttendeeId(attendee: MeetingAttendee): string {
  return String(attendee.userId || attendee.id || attendee.odooId || attendee.odoo_id || '')
}

function getAttendeeName(attendee: MeetingAttendee): string {
  return attendee.userFullName || attendee.fullName || attendee.name || attendee.userName || 'Unknown'
}

function getAttendeeRole(attendee: MeetingAttendee): string {
  // You could map role IDs to names here if needed
  return ''
}

function isPendingUser(attendee: MeetingAttendee): boolean {
  const id = getAttendeeId(attendee)
  return props.pendingUserIds?.includes(id) || false
}

// Get available attendees (excluding those with pending tasks)
const availableAttendees = computed(() => {
  return props.attendees.filter(a => !isPendingUser(a))
})

// Computed
const allSelected = computed(() => {
  return availableAttendees.value.length > 0 &&
    selectedUserIds.value.length === availableAttendees.value.length
})

const someSelected = computed(() => {
  return selectedUserIds.value.length > 0 &&
    selectedUserIds.value.length < availableAttendees.value.length
})

const selectedCount = computed(() => selectedUserIds.value.length)

const pendingCount = computed(() => props.pendingUserIds?.length || 0)

// Reset selection when modal opens
watch(() => props.show, (isOpen) => {
  if (isOpen) {
    selectedUserIds.value = []
    signatureUserIds.value = []
    dueDate.value = null
  }
})

function isSelected(attendee: MeetingAttendee): boolean {
  const id = getAttendeeId(attendee)
  return selectedUserIds.value.includes(id)
}

function toggleAttendee(attendee: MeetingAttendee): void {
  // Don't toggle if user already has pending task
  if (isPendingUser(attendee)) return

  const id = getAttendeeId(attendee)
  const index = selectedUserIds.value.indexOf(id)
  if (index === -1) {
    selectedUserIds.value.push(id)
  } else {
    selectedUserIds.value.splice(index, 1)
  }
}

function toggleSelectAll(): void {
  if (allSelected.value) {
    selectedUserIds.value = []
    signatureUserIds.value = []
  } else {
    // Only select available attendees (not pending ones)
    selectedUserIds.value = availableAttendees.value.map(a => getAttendeeId(a))
  }
}

function needsSignature(attendee: MeetingAttendee): boolean {
  const id = getAttendeeId(attendee)
  return signatureUserIds.value.includes(id)
}

function toggleSignature(attendee: MeetingAttendee): void {
  const id = getAttendeeId(attendee)
  // Can only toggle signature if user is selected
  if (!selectedUserIds.value.includes(id)) return

  const index = signatureUserIds.value.indexOf(id)
  if (index === -1) {
    signatureUserIds.value.push(id)
  } else {
    signatureUserIds.value.splice(index, 1)
  }
}

function handleSend(): void {
  // Convert Date to days from now (as integer) for backend
  let dueDateDays: number | undefined
  if (dueDate.value) {
    const now = new Date()
    now.setHours(0, 0, 0, 0)
    const dueDateTime = new Date(dueDate.value)
    dueDateTime.setHours(0, 0, 0, 0)
    dueDateDays = Math.ceil((dueDateTime.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))
    if (dueDateDays < 0) dueDateDays = 0
  }
  // Pass signature user IDs only if showSignatureOption is enabled
  const sigIds = props.showSignatureOption ? signatureUserIds.value : undefined
  emit('send', selectedUserIds.value, dueDateDays, sigIds)
}
</script>

<style scoped>
/* Modal Overlay */
.approvalModalOverlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.7);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10002;
  padding: 20px;
}

/* Modal Container */
.approvalModal {
  background: #ffffff;
  border: 1px solid #e0e0e0;
  border-radius: 16px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.4);
  width: 100%;
  max-width: 480px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  overflow: visible;
}

/* Header */
.modalHeader {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  background: linear-gradient(135deg, #22c55e 0%, #16a34a 100%);
  color: #fff;
}

.headerTitle {
  display: flex;
  align-items: center;
  gap: 10px;
}

.headerTitle h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
}

.closeBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: none;
  background: rgba(255, 255, 255, 0.15);
  color: #fff;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.closeBtn:hover {
  background: rgba(255, 255, 255, 0.25);
}

/* Body */
.modalBody {
  flex: 1;
  overflow-y: auto;
  overflow-x: visible;
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.modalBody::-webkit-scrollbar {
  width: 6px;
}

.modalBody::-webkit-scrollbar-track {
  background: transparent;
}

.modalBody::-webkit-scrollbar-thumb {
  background: #d1d5db;
  border-radius: 3px;
}

/* Select All Row */
.selectAllRow {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 14px;
  background: #f8faf9;
  border: 1px solid #e0e0e0;
  border-radius: 10px;
}

.selectedCount {
  font-size: 12px;
  color: #6b7280;
}

/* Checkbox Label */
.checkboxLabel {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  font-size: 13px;
  color: #1a1a1a;
}

.checkboxLabel input {
  display: none;
}

.checkmark {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
  border: 2px solid #d1d5db;
  border-radius: 4px;
  background: transparent;
  transition: all 0.15s ease;
  color: #fff;
}

.checkboxLabel input:checked + .checkmark,
.attendeeItem.selected > .checkmark {
  background: #006d4b;
  border-color: #006d4b;
}

.checkboxLabel.selectAll .checkmark {
  border-radius: 4px;
}

/* Attendees List */
.attendeesList {
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 280px;
  overflow-y: auto;
  padding: 4px;
}

.attendeesList::-webkit-scrollbar {
  width: 4px;
}

.attendeesList::-webkit-scrollbar-track {
  background: transparent;
}

.attendeesList::-webkit-scrollbar-thumb {
  background: #d1d5db;
  border-radius: 2px;
}

.attendeeItem {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 14px;
  background: #f8faf9;
  border: 1px solid #e0e0e0;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.attendeeItem:hover {
  background: #f0faf7;
  border-color: #006d4b;
}

.attendeeItem.selected {
  background: rgba(34, 197, 94, 0.08);
  border-color: rgba(34, 197, 94, 0.3);
}

.attendeeItem.disabled {
  opacity: 0.6;
  cursor: not-allowed;
  background: rgba(251, 191, 36, 0.05);
  border-color: rgba(251, 191, 36, 0.2);
}

.attendeeItem.disabled:hover {
  background: rgba(251, 191, 36, 0.05);
  border-color: rgba(251, 191, 36, 0.2);
}

/* Attendee item is clickable */

.signatureToggle {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  background: rgba(100, 116, 139, 0.15);
  border: 1px solid rgba(100, 116, 139, 0.3);
  border-radius: 6px;
  color: #6b7280;
  font-size: 11px;
  cursor: pointer;
  transition: all 0.15s ease;
  margin-right: auto;
}

.signatureToggle:hover {
  background: rgba(100, 116, 139, 0.25);
  border-color: rgba(100, 116, 139, 0.5);
}

.signatureToggle.active {
  background: rgba(59, 130, 246, 0.15);
  border-color: rgba(59, 130, 246, 0.4);
  color: #60a5fa;
}

.signatureToggle.active:hover {
  background: rgba(59, 130, 246, 0.25);
  border-color: rgba(59, 130, 246, 0.6);
}

.attendeeInfo {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.attendeeName {
  font-size: 13px;
  font-weight: 500;
  color: #1a1a1a;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.attendeeRole {
  font-size: 11px;
  color: #6b7280;
}

.attendeeStatus {
  font-size: 10px;
  padding: 2px 8px;
  border-radius: 4px;
  font-weight: 500;
}

.attendeeStatus.pending {
  color: #fbbf24;
  background: rgba(251, 191, 36, 0.15);
}

.checkmark.pendingMark {
  border-color: rgba(251, 191, 36, 0.5);
  color: #fbbf24;
}

/* Due Date Section */
.dueDateSection {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 12px 20px 16px;
  border-top: 1px solid #f1f5f9;
  position: relative;
  overflow: visible;
}

.dueDateLabel {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  font-weight: 600;
  color: #475569;
}

:deep(.dueDatePicker) {
  width: 100%;
}

:deep(.dueDatePicker button) {
  background: #fff !important;
  border: 1px solid #e0e0e0 !important;
  color: #1a1a1a !important;
  border-radius: 8px !important;
}

:deep(.dueDatePicker button:hover) {
  border-color: #006d4b !important;
}

:deep(.dueDatePicker [data-headlessui-state]),
:deep(.dueDatePicker .absolute) {
  z-index: 10300 !important;
}

/* Footer */
.modalFooter {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 10px;
  padding: 16px 20px;
  border-top: 1px solid #e0e0e0;
  background: #fafbfc;
}

.btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 10px 18px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn.ghost {
  background: transparent;
  border: 1px solid #d1d5db;
  color: #6b7280;
}

.btn.ghost:hover {
  background: #f8faf9;
  color: #1a1a1a;
}

.btn.primary {
  background: #22c55e;
  border: none;
  color: #fff;
}

.btn.primary:hover:not(:disabled) {
  background: #16a34a;
}

.btn.primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Spin animation */
.spinIcon {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* Modal Transitions */
.modal-enter-active,
.modal-leave-active {
  transition: all 0.25s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .approvalModal,
.modal-leave-to .approvalModal {
  transform: scale(0.95) translateY(-10px);
}
</style>

<template>
  <Modal
    :model-value="show"
    :title="title || $t('ApprovalStatusInitialMOM')"
    icon="mdi:account-group"
    size="lg"
    no-padding
    scrollable
    @update:model-value="(val: boolean) => { if (!val) $emit('close') }"
  >

          <!-- Content -->
          <div class="modalContent">
            <!-- Loading State -->
            <div v-if="loading" class="loadingState">
              <Icon icon="progress_activity" class="spinIcon" style="font-size: 24px;" />
              <span>{{ $t('LoadingApprovalStatus') }}</span>
            </div>

            <!-- Empty State -->
            <div v-else-if="!approvals || approvals.length === 0" class="emptyState">
              <Icon icon="inbox" style="font-size: 48px;" />
              <p>{{ $t('MOMNotSentYet') }}</p>
              <span>{{ $t('UseSendForApprovalButton') }}</span>
            </div>

            <!-- Approvals List -->
            <div v-else class="approvalsList">
              <!-- Summary Stats -->
              <div class="summaryStats">
                <div class="statItem total">
                  <span class="statValue">{{ approvals.length }}</span>
                  <span class="statLabel">{{ $t('Total') }}</span>
                </div>
                <div class="statItem approved">
                  <span class="statValue">{{ approvedCount }}</span>
                  <span class="statLabel">{{ $t('Approved') }}</span>
                </div>
                <div class="statItem rejected">
                  <span class="statValue">{{ rejectedCount }}</span>
                  <span class="statLabel">{{ $t('RejectedLabel') }}</span>
                </div>
                <div class="statItem pending">
                  <span class="statValue">{{ pendingCount }}</span>
                  <span class="statLabel">{{ $t('Pending') }}</span>
                </div>
              </div>

              <!-- Approvals Cards -->
              <div class="approvalsGrid">
                <div
                  v-for="approval in approvals"
                  :key="approval.id"
                  class="approvalCard"
                  :class="getStatusClass(approval.statusId)"
                >
                  <div class="cardHeader">
                    <div class="userInfo">
                      <UserAvatar :userId="approval.userId" :name="approval.userName || ''" size="sm" />
                      <span class="userName">{{ approval.userName }}</span>
                    </div>
                    <span :class="['statusBadge', getStatusClass(approval.statusId)]">
                      {{ approval.statusName }}
                    </span>
                  </div>

                  <div v-if="approval.comment" class="commentSection">
                    <div class="commentLabel">
                      <Icon icon="chat" style="font-size: 12px;" />
                      <span>{{ $t('Comment') }}</span>
                    </div>
                    <p class="commentText">{{ approval.comment }}</p>
                  </div>

                  <div class="cardFooter">
                    <div v-if="approval.version" class="versionInfo">
                      <Icon icon="description" style="font-size: 12px;" />
                      <span>{{ $t('VersionLabel') }} {{ approval.version }}</span>
                    </div>
                    <div v-if="approval.approveDate" class="dateInfo">
                      <Icon icon="calendar_today" style="font-size: 12px;" />
                      <span>{{ formatDate(approval.approveDate) }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Footer -->
          <template #footer>
            <button class="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-50" @click="$emit('close')">{{ $t('Close') }}</button>
          </template>
  </Modal>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import type { MeetingUserApproval } from '@/services/MeetingsService'
import { TaskStatus } from '@/services/MeetingsService'

interface Props {
  show: boolean
  loading: boolean
  approvals: MeetingUserApproval[]
  title?: string
}

const props = withDefaults(defineProps<Props>(), {
  title: ''
})

defineEmits<{
  (e: 'close'): void
}>()

// Computed counts
const approvedCount = computed(() =>
  props.approvals.filter(a => a.statusId === TaskStatus.Approved).length
)

const rejectedCount = computed(() =>
  props.approvals.filter(a => a.statusId === TaskStatus.Rejected).length
)

const pendingCount = computed(() =>
  props.approvals.filter(a => a.statusId === TaskStatus.PendingApproval).length
)

// Get CSS class based on status
function getStatusClass(statusId: number): string {
  switch (statusId) {
    case TaskStatus.Approved:
      return 'approved'
    case TaskStatus.Rejected:
      return 'rejected'
    case TaskStatus.Cancelled:
      return 'cancelled'
    default:
      return 'pending'
  }
}

// Format date
function formatDate(dateStr?: string): string {
  if (!dateStr) return ''
  try {
    const date = new Date(dateStr)
    return date.toLocaleDateString('ar-EG', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch {
    return dateStr
  }
}
</script>

<style>
/* Not scoped - Teleport moves content to body */
.modalOverlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10000;
  padding: 24px;
}

.modalContainer {
  width: 100%;
  max-width: 640px;
  max-height: 80vh;
  background: var(--card);
  border-radius: 16px;
  border: 1px solid var(--border);
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.5);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* Header */
.modalHeader {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid var(--border);
}

.headerTitle {
  display: flex;
  align-items: center;
  gap: 12px;
  color: var(--text);
}

.headerTitle h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
}

.headerTitle svg {
  color: #60a5fa;
}

.closeBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  background: transparent;
  border: none;
  border-radius: 8px;
  color: var(--muted);
  cursor: pointer;
  transition: all 0.15s ease;
}

.closeBtn:hover {
  background: var(--hover-bg);
  color: var(--text);
}

/* Content */
.modalContent {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
}

/* Loading State */
.loadingState {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 48px 24px;
  color: var(--muted);
}

.spinIcon {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* Empty State */
.emptyState {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 48px 24px;
  color: var(--muted);
  text-align: center;
}

.emptyState svg {
  opacity: 0.5;
}

.emptyState p {
  margin: 0;
  font-size: 15px;
  color: var(--muted);
}

.emptyState span {
  font-size: 13px;
}

/* Summary Stats */
.summaryStats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
  margin-bottom: 24px;
}

.statItem {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: 16px 12px;
  background: var(--hover-bg);
  border: 1px solid var(--border);
  border-radius: 12px;
}

.statItem.total {
  background: #f8fafc;
  border-color: #e2e8f0;
}

.statItem.approved {
  background: #f8fafc;
  border-color: #e2e8f0;
}

.statItem.rejected {
  background: #f8fafc;
  border-color: #e2e8f0;
}

.statItem.pending {
  background: #f8fafc;
  border-color: #e2e8f0;
}

.statValue {
  font-size: 24px;
  font-weight: 700;
  color: var(--text);
}

.statItem.approved .statValue {
  color: #4ade80;
}

.statItem.rejected .statValue {
  color: #f87171;
}

.statItem.pending .statValue {
  color: #fbbf24;
}

.statLabel {
  font-size: 11px;
  color: var(--muted);
  font-weight: 500;
}

/* Approvals Grid */
.approvalsGrid {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.approvalCard {
  padding: 16px;
  background: var(--hover-bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  transition: all 0.15s ease;
}

.approvalCard.approved {
  border-color: rgba(34, 197, 94, 0.2);
}

.approvalCard.rejected {
  border-color: rgba(239, 68, 68, 0.2);
}

.approvalCard.pending {
  border-color: rgba(251, 191, 36, 0.2);
}

.cardHeader {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.userInfo {
  display: flex;
  align-items: center;
  gap: 12px;
}

.userName {
  font-size: 14px;
  font-weight: 500;
  color: var(--text);
}

/* Status Badge */
.statusBadge {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 700;
}

.statusBadge.approved {
  background: #006d4b;
  color: #4ade80;
}

.statusBadge.rejected {
  background: #006d4b;
  color: #f87171;
}

.statusBadge.pending {
  background: #006d4b;
  color: #fbbf24;
}

.statusBadge.cancelled {
  background: #006d4b;
  color: #94a3b8;
}

/* Comment Section */
.commentSection {
  padding: 12px;
  background: var(--card2);
  border-radius: 8px;
  margin-bottom: 12px;
}

.commentLabel {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 11px;
  color: var(--muted);
  margin-bottom: 6px;
}

.commentText {
  margin: 0;
  font-size: 13px;
  color: var(--muted);
  line-height: 1.5;
}

/* Card Footer */
.cardFooter {
  display: flex;
  align-items: center;
  gap: 16px;
  padding-top: 12px;
  border-top: 1px solid var(--border);
}

.versionInfo,
.dateInfo {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 11px;
  color: var(--muted);
}

/* Modal Footer */
.modalFooter {
  display: flex;
  justify-content: flex-end;
  padding: 16px 24px;
  border-top: 1px solid var(--border);
}

.btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn.ghost {
  background: transparent;
  border: 1px solid var(--border2);
  color: var(--muted);
}

.btn.ghost:hover {
  background: var(--hover-bg);
  border-color: var(--border2);
  color: var(--text);
}

/* Scrollbar */
.modalContent::-webkit-scrollbar {
  width: 6px;
}

.modalContent::-webkit-scrollbar-track {
  background: transparent;
}

.modalContent::-webkit-scrollbar-thumb {
  background: var(--border2);
  border-radius: 3px;
}
</style>

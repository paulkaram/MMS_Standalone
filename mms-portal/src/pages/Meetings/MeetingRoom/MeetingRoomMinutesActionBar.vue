<template>
  <div class="minutesActionBarWrapper">
    <div class="minutesActionBar">
      <!-- Version Selector -->
      <div class="versionSelector" v-click-outside="() => showVersionDropdown = false">
        <button
          class="versionBtn"
          :disabled="loadingVersions || versions.length === 0"
          @click="showVersionDropdown = !showVersionDropdown"
        >
          <Icon icon="description" class="w-3.5 h-3.5" />
          <span>{{ currentVersionLabel }}</span>
          <span v-if="currentVersionStatus" :class="['statusBadge', currentVersionStatus]">
            {{ statusLabels[currentVersionStatus] || currentVersionStatus }}
          </span>
          <Icon icon="expand_more" class="w-3 h-3" :class="{ rotated: showVersionDropdown }" />
        </button>

        <div v-if="showVersionDropdown && sortedVersions.length > 0" class="versionDropdown">
          <div class="versionList">
            <button
              v-for="version in sortedVersions"
              :key="version.id"
              class="versionItem"
              :class="{ active: version.id === currentVersionId }"
              @click="handleSelectVersion(version)"
            >
              <span class="versionNumber">{{ $t('VersionLabel') }} {{ version.version }}.0</span>
              <span v-if="version.status" :class="['statusBadge small', version.status]">
                {{ statusLabels[version.status] || version.status }}
              </span>
              <span class="versionDate">{{ formatDate(version.createdAt) }}</span>
            </button>
          </div>
        </div>
      </div>

    <!-- Action Buttons -->
    <div class="actionButtons">
      <!-- Regenerate Button -->
      <button
        v-if="!isReadOnly"
        class="actionBtn regenerate"
        :disabled="regenerating"
        @click="$emit('regenerate')"
      >
        <Icon v-if="regenerating" icon="progress_activity" class="w-3.5 h-3.5 spinIcon" />
        <Icon v-else icon="refresh" class="w-3.5 h-3.5" />
        <span>{{ regenerating ? $t('Regenerating') : $t('Regenerate') }}</span>
      </button>

      <!-- Send for Approval Button -->
      <button
        v-if="!isReadOnly"
        class="actionBtn sendApproval"
        :disabled="sendingForApproval"
        @click="$emit('open-approval-modal')"
      >
        <Icon v-if="sendingForApproval" icon="progress_activity" class="w-3.5 h-3.5 spinIcon" />
        <Icon v-else icon="send" class="w-3.5 h-3.5" />
        <span>{{ sendingForApproval ? $t('SendingLabel') : $t('SendForApprovalBtn') }}</span>
      </button>

      <!-- Approve Final Button (shown when all approvals are complete) -->
      <button
        v-if="showApproveFinal && !isReadOnly"
        class="actionBtn approveFinal"
        :disabled="approvingFinal"
        @click="$emit('approve-final')"
      >
        <Icon v-if="approvingFinal" icon="progress_activity" class="w-3.5 h-3.5 spinIcon" />
        <Icon v-else icon="task_alt" class="w-3.5 h-3.5" />
        <span>{{ approvingFinal ? $t('ApprovingLabel') : $t('ApproveFinal') }}</span>
      </button>

      <!-- View Approvals Button -->
      <button
        class="actionBtn viewApprovals"
        @click="$emit('view-approvals')"
      >
        <Icon icon="group" class="w-3.5 h-3.5" />
        <span>{{ $t('ViewApprovals') }}</span>
        <span v-if="approvalsCount > 0" class="approvalsBadge">{{ approvalsCount }}</span>
      </button>

      <!-- Create Final MOM (Bypass Approval) - Hidden for Final MOM -->
      <button
        v-if="!hideCreateFinal && !isReadOnly"
        class="actionBtn createFinal"
        :disabled="creatingFinal"
        @click="$emit('create-final')"
      >
        <Icon v-if="creatingFinal" icon="progress_activity" class="w-3.5 h-3.5 spinIcon" />
        <Icon v-else icon="task_alt" class="w-3.5 h-3.5" />
        <span>{{ creatingFinal ? $t('CreatingLabel') : $t('ApproveDirectlyBtn') }}</span>
      </button>
    </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import type { MinutesVersion, MinutesStatus } from './types/minutes'

interface Props {
  versions: MinutesVersion[]
  currentVersionId: number | null
  loadingVersions: boolean
  regenerating: boolean
  sendingForApproval: boolean
  creatingFinal: boolean
  approvalsCount: number
  /** Hide the "اعتماد مباشر" button (for Final MOM) */
  hideCreateFinal?: boolean
  /** Show the "اعتماد نهائي" button (when all approvals are complete) */
  showApproveFinal?: boolean
  /** Loading state for approve final button */
  approvingFinal?: boolean
  /** Read-only mode - disable all action buttons (when meeting is finalized) */
  isReadOnly?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  creatingFinal: false,
  approvalsCount: 0,
  hideCreateFinal: false,
  showApproveFinal: false,
  approvingFinal: false,
  isReadOnly: false
})

const emit = defineEmits<{
  (e: 'regenerate'): void
  (e: 'open-approval-modal'): void
  (e: 'select-version', versionId: number): void
  (e: 'create-final'): void
  (e: 'view-approvals'): void
  (e: 'approve-final'): void
}>()

// Dropdown state
const showVersionDropdown = ref(false)

// Custom click-outside directive
const vClickOutside = {
  mounted(el: HTMLElement & { _clickOutside?: (e: MouseEvent) => void }, binding: any) {
    el._clickOutside = (event: MouseEvent) => {
      if (!(el === event.target || el.contains(event.target as Node))) {
        binding.value(event)
      }
    }
    document.addEventListener('click', el._clickOutside)
  },
  unmounted(el: HTMLElement & { _clickOutside?: (e: MouseEvent) => void }) {
    if (el._clickOutside) {
      document.removeEventListener('click', el._clickOutside)
    }
  }
}

// Status labels - will be resolved via $t() but kept as fallback
const statusLabels: Record<string, string> = {
  draft: 'Draft',
  pending_review: 'Pending Review',
  under_revision: 'Under Revision',
  pending_approval: 'Pending Approval',
  approved: 'Approved',
  published: 'منشور',
  archived: 'مؤرشف'
}

// Sort versions by version number (descending)
const sortedVersions = computed(() => {
  return [...props.versions].sort((a, b) => b.version - a.version)
})

// Current version info
const currentVersion = computed(() => {
  if (!props.currentVersionId) return null
  return props.versions.find(v => v.id === props.currentVersionId) || null
})

const currentVersionLabel = computed(() => {
  if (props.loadingVersions) return '...'
  if (!currentVersion.value) return 'v1.0'
  return `v${currentVersion.value.version}.0`
})

const currentVersionStatus = computed((): MinutesStatus | null => {
  return currentVersion.value?.status || null
})

// Format date for display
function formatDate(dateStr?: string): string {
  if (!dateStr) return ''
  try {
    const date = new Date(dateStr)
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      calendar: 'gregory'
    })
  } catch {
    return dateStr
  }
}

// Select a version
function handleSelectVersion(version: MinutesVersion): void {
  emit('select-version', version.id)
  showVersionDropdown.value = false
}
</script>

<style scoped>
/* Wrapper */
.minutesActionBarWrapper {
  display: flex;
  flex-direction: column;
  background: #006d4b;
  border-radius: 0 0 8px 8px;
  margin: 0;
}

/* Main Action Bar */
.minutesActionBar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 10px 16px;
}

/* Version Selector */
.versionSelector {
  position: relative;
}

.versionBtn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 8px;
  color: #fff;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
}

.versionBtn .w-3\.5 {
  color: #006d4b;
}

.versionBtn:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.15);
  border-color: #006d4b;
}

.versionBtn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.versionBtn .rotated {
  transform: rotate(180deg);
}

.versionBtn .icon-ms {
  transition: transform 0.2s ease;
}

.versionDropdown {
  position: absolute;
  bottom: calc(100% + 4px);
  left: 0;
  width: 220px;
  background: #006d4b;
  border: 1px solid rgba(0, 109, 75, 0.2);
  border-radius: 10px;
  box-shadow: 0 -8px 30px rgba(0, 0, 0, 0.4);
  z-index: 10200;
}

.versionList {
  max-height: 280px;
  overflow-y: auto;
  padding: 8px;
}

.versionList::-webkit-scrollbar {
  width: 4px;
}

.versionList::-webkit-scrollbar-track {
  background: transparent;
}

.versionList::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.15);
  border-radius: 2px;
}

.versionItem {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 6px;
  width: 100%;
  padding: 10px 12px;
  background: transparent;
  border: none;
  border-radius: 8px;
  color: #d1d5db;
  font-size: 13px;
  cursor: pointer;
  transition: all 0.15s ease;
  text-align: start;
}

.versionItem:hover {
  background: rgba(255, 255, 255, 0.08);
  color: #fff;
}

.versionItem.active {
  background: rgba(0, 109, 75, 0.15);
  color: #fff;
}

.versionItem.active .versionNumber {
  color: #006d4b;
}

.versionItem .versionNumber {
  font-weight: 600;
  font-size: 13px;
}

.versionItem .versionDate {
  font-size: 11px;
  color: #94a3b8;
}

/* Status Badges */
.statusBadge {
  padding: 3px 8px;
  border-radius: 12px;
  font-size: 10px;
  font-weight: 600;
  text-transform: uppercase;
}

.statusBadge.small {
  padding: 2px 6px;
  font-size: 9px;
}

.statusBadge.draft {
  background: rgba(251, 191, 36, 0.2);
  color: #fbbf24;
}

.statusBadge.pending_review,
.statusBadge.pending_approval {
  background: rgba(59, 130, 246, 0.15);
  color: #60a5fa;
}

.statusBadge.under_revision {
  background: rgba(168, 85, 247, 0.15);
  color: #a78bfa;
}

.statusBadge.approved,
.statusBadge.published {
  background: rgba(0, 109, 75, 0.15);
  color: #006d4b;
}

.statusBadge.archived {
  background: rgba(255, 255, 255, 0.08);
  color: #94a3b8;
}

/* Action Buttons */
.actionButtons {
  display: flex;
  align-items: center;
  gap: 10px;
}

.actionBtn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  border: none;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.actionBtn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.actionBtn.regenerate {
  background: rgba(255, 255, 255, 0.08);
  color: #d1d5db;
  border: 1px solid rgba(255, 255, 255, 0.15);
}

.actionBtn.regenerate:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.15);
  color: #fff;
  border-color: rgba(255, 255, 255, 0.3);
}

.actionBtn.sendApproval {
  background: #006d4b;
  color: #fff;
  border: none;
}

.actionBtn.sendApproval:hover:not(:disabled) {
  background: #008f73;
}

.actionBtn.createFinal {
  background: rgba(255, 255, 255, 0.08);
  color: #d1d5db;
  border: 1px solid rgba(255, 255, 255, 0.15);
}

.actionBtn.createFinal:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.15);
  color: #006d4b;
  border-color: #006d4b;
}

.actionBtn.approveFinal {
  background: #006d4b;
  color: #fff;
  border: none;
}

.actionBtn.approveFinal:hover:not(:disabled) {
  background: #008f73;
}

.actionBtn.viewApprovals {
  background: rgba(255, 255, 255, 0.08);
  color: #d1d5db;
  border: 1px solid rgba(255, 255, 255, 0.15);
  position: relative;
}

.actionBtn.viewApprovals:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.15);
  color: #fff;
  border-color: rgba(255, 255, 255, 0.3);
}

.approvalsBadge {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 18px;
  height: 18px;
  padding: 0 5px;
  background: #3b82f6;
  color: #fff;
  border-radius: 10px;
  font-size: 10px;
  font-weight: 600;
}

/* Spin animation */
.spinIcon {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* RTL adjustments */
[dir="rtl"] .versionDropdown {
  right: auto;
  left: 0;
}
</style>

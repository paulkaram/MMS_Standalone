<template>
  <div class="bid-detail-page">
    <!-- Loading -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
    </div>

    <template v-else-if="bid">
      <PageHeader :title="bid.subject" :subtitle="bid.referenceNumber">
        <template #actions>
          <button class="btn-back" @click="goBack">
            <Icon icon="mdi:arrow-left" class="w-4 h-4" />
            {{ $t('Back') }}
          </button>
        </template>
      </PageHeader>

      <!-- Lifecycle Timeline -->
      <div class="timeline-card">
        <div class="timeline-header">
          <Icon icon="mdi:timeline-check-outline" class="w-5 h-5" style="color: #006d4b" />
          <h3>{{ $t('LifecycleTimeline') }}</h3>
          <span class="step-counter">{{ $t('Step') || 'Step' }} {{ bid.statusStepOrder }} / 12</span>
        </div>
        <div class="timeline-track">
          <div
            v-for="status in visibleStatuses"
            :key="status.id"
            class="timeline-step"
            :class="getStepClass(status)"
            :title="status.nameEn"
          >
            <div class="step-dot">
              <Icon v-if="status.stepOrder < bid.statusStepOrder" icon="mdi:check" class="w-3 h-3" />
              <span v-else>{{ status.stepOrder }}</span>
            </div>
            <div class="step-label">{{ localizedStatus(status) }}</div>
          </div>
        </div>
      </div>

      <!-- Detail Grid -->
      <div class="detail-grid">
        <!-- Info Card -->
        <section class="detail-card info-card">
          <h3>
            <Icon icon="mdi:information-outline" class="w-4 h-4" />
            {{ $t('BidDetails') }}
          </h3>
          <div class="info-rows">
            <div class="info-row">
              <span class="info-label">{{ $t('Status') }}</span>
              <span class="status-pill" :class="getStatusClass(bid.statusId)">{{ bid.statusName }}</span>
            </div>
            <div v-if="bid.teamLeaderName" class="info-row">
              <span class="info-label">{{ $t('TeamLeader') }}</span>
              <span>{{ bid.teamLeaderName }}</span>
            </div>
            <div v-if="bid.externalMeetingNumber" class="info-row">
              <span class="info-label">{{ $t('ExternalMeetingNumber') }}</span>
              <span>{{ bid.externalMeetingNumber }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">{{ $t('StartDate') }}</span>
              <span>{{ formatDate(bid.startDate) }}</span>
            </div>
            <div class="info-row" :class="{ overdue: bid.isOverdue }">
              <span class="info-label">{{ $t('DueDate') }}</span>
              <span>
                {{ formatDate(bid.dueDate) }}
                <em v-if="bid.isOverdue" class="overdue-badge">{{ $t('Overdue') }}</em>
              </span>
            </div>
            <div class="info-row">
              <span class="info-label">{{ $t('CreatedBy') || 'Created By' }}</span>
              <span>{{ bid.createdByName }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">{{ $t('CreatedDate') || 'Created' }}</span>
              <span>{{ formatDate(bid.createdDate) }}</span>
            </div>
          </div>
          <div v-if="bid.description" class="info-description" v-html="bid.description"></div>
        </section>

        <!-- Transition Card -->
        <section class="detail-card transition-card">
          <h3>
            <Icon icon="mdi:transit-connection-variant" class="w-4 h-4" />
            {{ $t('AdvanceStatus') }}
          </h3>
          <div v-if="allowedNext.length > 0" class="transition-actions">
            <div
              v-for="status in allowedNext"
              :key="status.id"
              class="transition-option"
              :class="{ selected: selectedTargetId === status.id }"
              @click="selectedTargetId = status.id"
            >
              <div class="transition-option-title">
                {{ localizedStatus(status) }}
              </div>
              <div class="transition-option-order">
                {{ $t('Step') || 'Step' }} {{ status.stepOrder }}
              </div>
            </div>
            <div class="transition-note-wrap">
              <label>{{ $t('TransitionNote') }}</label>
              <textarea v-model="transitionNote" rows="2" class="form-input" />
            </div>
            <button
              class="btn-primary full-width"
              :disabled="!selectedTargetId || transitioning"
              @click="performTransition"
            >
              <Icon icon="mdi:arrow-right-bold" class="w-4 h-4" />
              {{ $t('Transition') }}
            </button>
          </div>
          <div v-else class="empty-inline">{{ $t('NoNextStatuses') }}</div>
        </section>

        <!-- Stakeholders Card -->
        <section class="detail-card">
          <h3>
            <Icon icon="mdi:account-group" class="w-4 h-4" />
            {{ $t('Stakeholders') }}
            <span class="count-badge">{{ bid.stakeholders.length }}</span>
          </h3>
          <div v-if="bid.stakeholders.length > 0" class="stakeholder-list">
            <div v-for="sh in bid.stakeholders" :key="sh.id" class="stakeholder-row">
              <UserAvatar
                :userId="sh.userId || ''"
                :name="sh.name"
                size="sm"
              />
              <div class="stakeholder-info">
                <div class="stakeholder-name">
                  {{ sh.name }}
                  <span v-if="sh.isTeamLeader" class="leader-badge">{{ $t('TeamLeader') }}</span>
                  <span v-if="sh.isExternal" class="external-badge">{{ $t('ExternalMember') }}</span>
                </div>
                <div v-if="sh.email" class="stakeholder-email">{{ sh.email }}</div>
              </div>
            </div>
          </div>
          <div v-else class="empty-inline">{{ $t('NoData') }}</div>
        </section>

        <!-- History Card -->
        <section class="detail-card history-card">
          <h3>
            <Icon icon="mdi:history" class="w-4 h-4" />
            {{ $t('History') }}
          </h3>
          <div v-if="bid.history.length > 0" class="history-list">
            <div v-for="h in bid.history" :key="h.id" class="history-row">
              <div class="history-dot"></div>
              <div class="history-content">
                <div class="history-title">
                  <strong v-if="h.fromStatusName">{{ h.fromStatusName }}</strong>
                  <Icon v-if="h.fromStatusName" icon="mdi:arrow-right" class="w-3 h-3 history-arrow" />
                  <strong>{{ h.toStatusName }}</strong>
                </div>
                <div class="history-meta">
                  <span>{{ h.changedByName }}</span>
                  <span>&middot;</span>
                  <span>{{ formatDateTime(h.changedDate) }}</span>
                </div>
                <div v-if="h.note" class="history-note">{{ h.note }}</div>
              </div>
            </div>
          </div>
          <div v-else class="empty-inline">{{ $t('NoData') }}</div>
        </section>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageHeader from '@/components/layout/PageHeader.vue'
import Icon from '@/components/ui/Icon.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import BidsService, { type BidDetail, type BidStatusDto, BidStatus } from '@/services/BidsService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'
import { useUserStore } from '@/stores/user'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()
const { toast } = useToast()
const userStore = useUserStore()

const bid = ref<BidDetail | null>(null)
const allStatuses = ref<BidStatusDto[]>([])
const allowedNextIds = ref<number[]>([])
const selectedTargetId = ref<number | null>(null)
const transitionNote = ref('')
const loading = ref(false)
const transitioning = ref(false)

const bidId = computed(() => Number(route.params.id))

const visibleStatuses = computed(() =>
  allStatuses.value
    .filter(s => s.id !== BidStatus.Returned)
    .sort((a, b) => a.stepOrder - b.stepOrder)
)

const allowedNext = computed(() =>
  allStatuses.value.filter(s => allowedNextIds.value.includes(s.id))
)

const isRtl = computed(() => userStore.isRtl)

const localizedStatus = (s: BidStatusDto) => isRtl.value ? s.nameAr : s.nameEn

const load = async () => {
  loading.value = true
  try {
    const [bidRes, statusRes, nextRes]: any = await Promise.all([
      BidsService.getById(bidId.value),
      BidsService.listStatuses(),
      BidsService.allowedNextStatuses(bidId.value)
    ])
    bid.value = bidRes?.data ?? bidRes
    allStatuses.value = statusRes?.data ?? statusRes ?? []
    allowedNextIds.value = nextRes?.data ?? nextRes ?? []
    selectedTargetId.value = allowedNextIds.value[0] || null
  } catch (err) {
    console.error('Failed to load bid:', err)
  } finally {
    loading.value = false
  }
}

const performTransition = async () => {
  if (!selectedTargetId.value || !bidId.value) return
  transitioning.value = true
  try {
    await BidsService.transition(bidId.value, selectedTargetId.value, transitionNote.value)
    toast.success(t('StatusChangedSuccessfully'))
    transitionNote.value = ''
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    transitioning.value = false
  }
}

const goBack = () => router.back()

const getStatusClass = (statusId: number): string => {
  switch (statusId) {
    case BidStatus.Draft: return 'status-draft'
    case BidStatus.PendingManagerApproval: return 'status-pending'
    case BidStatus.VisionPreparation:
    case BidStatus.VisionsCompleted: return 'status-vision'
    case BidStatus.PreparatoryMeeting:
    case BidStatus.MinisterialMeeting: return 'status-meeting'
    case BidStatus.ExternalMeetingDone:
    case BidStatus.AwaitingOpinion: return 'status-external'
    case BidStatus.FinalMinutes:
    case BidStatus.AssignmentsCreated: return 'status-final'
    case BidStatus.Completed: return 'status-completed'
    case BidStatus.Returned: return 'status-returned'
    default: return ''
  }
}

const getStepClass = (status: BidStatusDto): string => {
  if (!bid.value) return ''
  if (status.stepOrder < bid.value.statusStepOrder) return 'completed'
  if (status.stepOrder === bid.value.statusStepOrder) return 'current'
  return 'upcoming'
}

const formatDate = (iso: string): string => {
  if (!iso) return ''
  return new Date(iso).toLocaleDateString()
}

const formatDateTime = (iso: string): string => {
  if (!iso) return ''
  return new Date(iso).toLocaleString()
}

onMounted(load)
</script>

<style scoped>
.bid-detail-page {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.loading-state {
  display: flex;
  justify-content: center;
  padding: 80px 0;
}

.spinner {
  width: 36px; height: 36px;
  border: 3px solid #e2e8f0;
  border-top-color: #006d4b;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

.btn-back {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 14px;
  background: #fff;
  color: #006d4b;
  border: 1px solid #c8ddd3;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  font-family: inherit;
}
.btn-back:hover { background: #f4f8f6; }

/* Timeline */
.timeline-card {
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 12px;
  padding: 20px;
}

.timeline-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 20px;
}
.timeline-header h3 {
  flex: 1;
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  color: #1a2e25;
}

.step-counter {
  font-size: 12px;
  color: #5a7a6d;
  background: #e8f5ef;
  padding: 3px 10px;
  border-radius: 10px;
  font-weight: 600;
  border: 1px solid #c8ddd3;
}

.timeline-track {
  display: grid;
  grid-template-columns: repeat(11, 1fr);
  gap: 4px;
  position: relative;
}

.timeline-step {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  padding: 0 2px;
  position: relative;
}

.step-dot {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
  background: #f1f5f9;
  color: #94a3b8;
  border: 2px solid #e2e8f0;
  transition: all 0.2s;
  z-index: 2;
}
.timeline-step.completed .step-dot {
  background: #006d4b;
  color: #fff;
  border-color: #006d4b;
}
.timeline-step.current .step-dot {
  background: #fff;
  color: #006d4b;
  border-color: #006d4b;
  box-shadow: 0 0 0 4px rgba(0, 109, 75, 0.15);
}

.timeline-step::before {
  content: '';
  position: absolute;
  top: 12px;
  inset-inline-start: -50%;
  width: 100%;
  height: 2px;
  background: #e2e8f0;
  z-index: 1;
}
.timeline-step:first-child::before { display: none; }
.timeline-step.completed::before { background: #006d4b; }

.step-label {
  font-size: 10px;
  color: #5a7a6d;
  font-weight: 500;
  text-align: center;
  min-height: 28px;
  line-height: 1.3;
}
.timeline-step.current .step-label {
  color: #006d4b;
  font-weight: 700;
}

/* Detail Grid */
.detail-grid {
  display: grid;
  grid-template-columns: 1.3fr 1fr;
  gap: 16px;
}
.detail-grid .info-card,
.detail-grid .history-card {
  grid-column: 1;
}
.detail-grid .transition-card {
  grid-column: 2;
}
@media (max-width: 960px) {
  .detail-grid { grid-template-columns: 1fr; }
  .detail-grid .info-card,
  .detail-grid .history-card,
  .detail-grid .transition-card { grid-column: 1; }
}

.detail-card {
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 12px;
  padding: 18px;
}

.detail-card h3 {
  display: flex;
  align-items: center;
  gap: 6px;
  margin: 0 0 14px;
  font-size: 14px;
  font-weight: 700;
  color: #1a2e25;
}

.count-badge {
  margin-inline-start: auto;
  background: #e8f5ef;
  color: #006d4b;
  padding: 1px 8px;
  border-radius: 10px;
  font-size: 11px;
  font-weight: 700;
}

.info-rows { display: flex; flex-direction: column; gap: 8px; }

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 10px;
  background: #f7faf8;
  border-radius: 8px;
  font-size: 13px;
  color: #1a2e25;
}
.info-label {
  color: #5a7a6d;
  font-weight: 600;
  font-size: 12px;
}
.info-row.overdue { background: #fef2f2; color: #dc2626; }

.overdue-badge {
  font-style: normal;
  padding: 1px 6px;
  background: #fecaca;
  border-radius: 6px;
  font-size: 10px;
  font-weight: 700;
  margin-inline-start: 4px;
}

.info-description {
  margin-top: 12px;
  padding: 12px;
  background: #f7faf8;
  border: 1px solid #e4ede8;
  border-radius: 8px;
  font-size: 13px;
  color: #374151;
  line-height: 1.6;
}

/* Status pill */
.status-pill {
  padding: 2px 10px;
  font-size: 11px;
  font-weight: 600;
  border-radius: 10px;
  border: 1px solid;
}
.status-pill.status-draft { background: #f1f5f9; color: #475569; border-color: #cbd5e1; }
.status-pill.status-pending { background: #fef3c7; color: #92400e; border-color: #fde68a; }
.status-pill.status-vision { background: #dbeafe; color: #1d4ed8; border-color: #93c5fd; }
.status-pill.status-meeting { background: #e0e7ff; color: #4338ca; border-color: #a5b4fc; }
.status-pill.status-external { background: #fce7f3; color: #9d174d; border-color: #f9a8d4; }
.status-pill.status-final { background: #dcfce7; color: #15803d; border-color: #86efac; }
.status-pill.status-completed { background: #d1fae5; color: #047857; border-color: #6ee7b7; }
.status-pill.status-returned { background: #fee2e2; color: #b91c1c; border-color: #fca5a5; }

/* Transition */
.transition-actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.transition-option {
  padding: 10px 12px;
  border: 1.5px solid #e4ede8;
  border-radius: 8px;
  background: #f7faf8;
  cursor: pointer;
  transition: all 0.15s;
}
.transition-option:hover { background: #eef5f1; }
.transition-option.selected {
  background: #e8f5ef;
  border-color: #006d4b;
}
.transition-option-title {
  font-size: 13px;
  font-weight: 600;
  color: #1a2e25;
}
.transition-option-order {
  font-size: 11px;
  color: #5a7a6d;
  margin-top: 2px;
}

.transition-note-wrap { display: flex; flex-direction: column; gap: 4px; margin-top: 6px; }
.transition-note-wrap label {
  font-size: 12px;
  font-weight: 600;
  color: #374a41;
}
.form-input {
  padding: 8px 10px;
  border: 1.5px solid #d4e0da;
  border-radius: 8px;
  font-size: 13px;
  background: #f7faf8;
  color: #1a2e25;
  font-family: inherit;
  resize: vertical;
}
.form-input:focus {
  outline: none; border-color: #006d4b; background: #fff;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

.btn-primary {
  padding: 10px 14px;
  background: #006d4b;
  color: #fff;
  border: none;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  font-family: inherit;
  transition: background 0.2s;
}
.btn-primary:hover:not(:disabled) { background: #005a3e; }
.btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
.btn-primary.full-width { width: 100%; margin-top: 4px; }

.empty-inline {
  padding: 18px;
  text-align: center;
  font-size: 12px;
  color: #93afa4;
}

/* Stakeholders */
.stakeholder-list { display: flex; flex-direction: column; gap: 8px; }
.stakeholder-row {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  background: #f7faf8;
  border-radius: 8px;
}
.stakeholder-info { flex: 1; min-width: 0; }
.stakeholder-name {
  font-size: 13px;
  font-weight: 600;
  color: #1a2e25;
  display: inline-flex;
  gap: 4px;
  flex-wrap: wrap;
  align-items: center;
}
.leader-badge {
  padding: 1px 7px;
  background: #fef3c7;
  color: #92400e;
  border: 1px solid #fde68a;
  border-radius: 8px;
  font-size: 10px;
  font-weight: 700;
}
.external-badge {
  padding: 1px 7px;
  background: #eef2ff;
  color: #3b4cca;
  border: 1px solid #c5cbf5;
  border-radius: 8px;
  font-size: 10px;
  font-weight: 700;
}
.stakeholder-email {
  font-size: 11px;
  color: #5a7a6d;
  margin-top: 2px;
}

/* History */
.history-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  position: relative;
  padding-inline-start: 6px;
}
.history-row {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  position: relative;
}
.history-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: #006d4b;
  margin-top: 6px;
  flex-shrink: 0;
}
.history-content { flex: 1; min-width: 0; }
.history-title {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 13px;
  color: #1a2e25;
}
.history-title strong { font-weight: 700; }
.history-arrow { color: #5a7a6d; }
.history-meta {
  display: flex;
  gap: 6px;
  font-size: 11px;
  color: #5a7a6d;
  margin-top: 2px;
}
.history-note {
  margin-top: 6px;
  padding: 6px 10px;
  background: #f7faf8;
  border-inline-start: 3px solid #c8ddd3;
  border-radius: 4px;
  font-size: 12px;
  color: #374151;
}
</style>

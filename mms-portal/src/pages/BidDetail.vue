<template>
  <div class="bd-page">
    <!-- Loading -->
    <div v-if="loading" class="bd-loading">
      <div class="bd-spinner"></div>
    </div>

    <template v-else-if="bid">
      <!-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
           Sticky action bar — title, status, primary CTA
           ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ -->
      <header class="bd-bar">
        <div class="bd-bar-left">
          <button class="bd-iconbtn" @click="goBack" :title="$t('Back')">
            <Icon :icon="isRtl ? 'mdi:arrow-right' : 'mdi:arrow-left'" class="w-4 h-4" />
          </button>
          <div class="bd-titles">
            <h1 class="bd-title">{{ bid.subject }}</h1>
            <div class="bd-meta">
              <span class="bd-ref">
                <Icon icon="mdi:pound" class="w-3 h-3" />
                {{ bid.referenceNumber }}
              </span>
              <span v-if="bid.committeeName" class="bd-committee">
                <Icon icon="mdi:briefcase-outline" class="w-3 h-3" />
                {{ bid.committeeName }}
              </span>
              <span class="bd-status-pill" :class="getStatusClass(bid.statusId)">{{ bid.statusName }}</span>
              <span v-if="bid.isOverdue" class="bd-overdue">
                <Icon icon="mdi:alert-circle-outline" class="w-3 h-3" />
                {{ $t('Overdue') }}
              </span>
            </div>
          </div>
        </div>

        <div class="bd-bar-right">
          <!-- Primary action (Submit for Approval) — only the creator in Draft/Returned -->
          <button
            v-if="isCreatorDrafting && primaryTransition"
            class="bd-cta-submit"
            :disabled="transitioning"
            @click="submitDraft(primaryTransition.id)"
          >
            <Icon icon="mdi:send" class="w-4 h-4" />
            <span>{{ isRtl ? primaryTransition.labelAr : primaryTransition.labelEn }}</span>
          </button>
          <button
            v-else-if="hasOpenTask"
            class="bd-cta-tasks"
            @click="$router.push('/tasks')"
          >
            <Icon icon="mdi:clipboard-check-outline" class="w-4 h-4" />
            <span>{{ $t('GoToTasks') || 'Go to Tasks' }}</span>
          </button>
        </div>
      </header>

      <!-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
           Hero — compact timeline + key info
           ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ -->
      <section class="bd-hero">
        <!-- Compact horizontal timeline -->
        <div class="bd-hero-timeline">
          <div class="bd-tl-head">
            <Icon icon="mdi:transit-connection-variant" class="w-4 h-4" />
            <h3>{{ $t('LifecycleTimeline') }}</h3>
            <span class="bd-step-counter">{{ bid.statusStepOrder }} / 12</span>
          </div>
          <div class="bd-tl-track">
            <div
              v-for="status in visibleStatuses"
              :key="status.id"
              class="bd-tl-step"
              :class="getStepClass(status)"
              :title="localizedStatus(status)"
            >
              <div class="bd-tl-dot">
                <Icon v-if="status.stepOrder < bid.statusStepOrder" icon="mdi:check" class="w-3 h-3" />
              </div>
            </div>
          </div>
          <div class="bd-tl-current">
            <span class="bd-tl-label">{{ $t('CurrentStep') || 'Current step' }}:</span>
            <strong>{{ workflowInstance?.currentStepName || bid.statusName }}</strong>
          </div>
        </div>

        <!-- Key info grid -->
        <div class="bd-hero-info">
          <div class="bd-info-row">
            <span class="bd-info-key">{{ $t('TeamLeader') }}</span>
            <span class="bd-info-val">{{ bid.teamLeaderName || '—' }}</span>
          </div>
          <div class="bd-info-row">
            <span class="bd-info-key">{{ $t('CreatedBy') || 'Created by' }}</span>
            <span class="bd-info-val">{{ bid.createdByName }}</span>
          </div>
          <div class="bd-info-row">
            <span class="bd-info-key">{{ $t('StartDate') }}</span>
            <span class="bd-info-val">{{ formatDate(bid.startDate) }}</span>
          </div>
          <div class="bd-info-row" :class="{ 'is-overdue': bid.isOverdue }">
            <span class="bd-info-key">{{ $t('DueDate') }}</span>
            <span class="bd-info-val">{{ formatDate(bid.dueDate) }}</span>
          </div>
          <div v-if="bid.externalMeetingNumber" class="bd-info-row">
            <span class="bd-info-key">{{ $t('ExternalMeetingNumber') }}</span>
            <span class="bd-info-val">{{ bid.externalMeetingNumber }}</span>
          </div>
          <div class="bd-info-row bd-info-row--full">
            <span class="bd-info-key">{{ $t('CreatedDate') }}</span>
            <span class="bd-info-val">{{ formatDate(bid.createdDate) }}</span>
          </div>
        </div>
      </section>

      <!-- Hint banner — only when there's something to communicate -->
      <div v-if="isCreatorDrafting" class="bd-hint">
        <Icon icon="mdi:information-outline" class="w-4 h-4" />
        <span>{{ $t('CompleteBidThenSubmit') || 'Add items, stakeholders, and attachments. When ready, submit for approval using the button above.' }}</span>
      </div>

      <!-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
           Stacked content panels — single scroll, all visible
           ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ -->

      <!-- Description (top — sets context before the user sees items).
           Editable inline while the bid is in a shapeable status (Draft/Returned/VisionPreparation)
           and the user is the creator or team leader. -->
      <section v-if="bid.description || canEditItems" class="bd-card bd-description">
        <div class="bd-desc-head">
          <Icon icon="mdi:text-box-outline" class="w-4 h-4" />
          <h4>{{ $t('Description') }}</h4>
          <button
            v-if="canEditItems && !descEditing"
            class="bd-desc-edit-btn"
            @click="startEditDescription"
          >
            <Icon icon="mdi:pencil-outline" class="w-3 h-3" />
            {{ bid.description ? ($t('Edit') || 'Edit') : ($t('AddDescription') || 'Add description') }}
          </button>
        </div>

        <!-- Display mode -->
        <div v-if="!descEditing">
          <div v-if="bid.description" class="bd-desc-body" v-html="bid.description"></div>
          <div v-else class="bd-desc-empty">
            {{ $t('NoDescriptionYet') || 'No description yet.' }}
          </div>
        </div>

        <!-- Edit mode -->
        <div v-else class="bd-desc-edit">
          <TextEditor
            v-model="descDraft"
            :placeholder="$t('DescriptionPlaceholder') || 'Describe the bid context, scope, and intent.'"
          />
          <div class="bd-desc-actions">
            <button class="bd-btn-secondary" :disabled="descSaving" @click="cancelEditDescription">
              {{ $t('Cancel') }}
            </button>
            <button class="bd-btn-primary" :disabled="descSaving" @click="saveDescription">
              <Icon icon="mdi:check" class="w-3 h-3" />
              {{ $t('Save') }}
            </button>
          </div>
        </div>
      </section>

      <!-- Items -->
      <section class="bd-card">
        <BidItemsPanel
          :bid-id="bid.id"
          :items="bidItems"
          :can-edit="canEditItems"
          @changed="load"
        />
      </section>

      <!-- Stakeholders + Attachments — side-by-side row -->
      <div class="bd-row">
        <!-- Stakeholders are display-only on the bid page; assignment happens
             via a downstream task on the Tasks page. :can-edit is hardcoded
             false so the Add button is hidden here. -->
        <section class="bd-card">
          <BidStakeholdersPanel
            :bid-id="bid.id"
            :committee-id="bid.committeeId"
            :stakeholders="bid.stakeholders"
            :can-edit="false"
            @changed="load"
          />
        </section>
        <section class="bd-card">
          <BidAttachmentsPanel :bid-id="bid.id" :can-edit="canEditItems" />
        </section>
      </div>

      <!-- Visions — only shown once initiated -->
      <section v-if="visionsSummary && visionsSummary.totalVisions > 0" class="bd-card">
        <header class="bd-section-head">
          <h3>
            <Icon icon="mdi:eye-outline" class="w-4 h-4" />
            {{ $t('Visions') || 'Visions' }}
            <span class="bd-section-count">{{ visionsSummary.submittedVisions }} / {{ visionsSummary.totalVisions }}</span>
          </h3>
        </header>

        <div class="bd-vis-progress">
          <div class="bd-vis-bar">
            <div
              class="bd-vis-bar-fill"
              :style="{ width: visionsOverallPct + '%' }"
              :class="{ complete: visionsSummary.allSubmitted }"
            ></div>
          </div>
          <div class="bd-vis-pct">{{ visionsOverallPct }}%</div>
        </div>

        <div class="bd-vis-list">
          <div
            v-for="sh in visionsSummary.byStakeholder"
            :key="(sh.userId || '') + '|' + (sh.externalMemberId || '')"
            class="bd-vis-row"
            :class="{ complete: sh.completed }"
          >
            <UserAvatar :userId="sh.userId || ''" :name="sh.name" size="sm" />
            <div class="bd-vis-info">
              <div class="bd-vis-name">
                {{ sh.name }}
                <span v-if="sh.isExternal" class="bd-vis-ext">{{ $t('ExternalMember') }}</span>
              </div>
              <div class="bd-vis-progress-row">
                <div class="bd-vis-mini">
                  <div class="bd-vis-mini-fill" :class="{ complete: sh.completed }" :style="{ width: (sh.total ? (sh.submitted / sh.total * 100) : 0) + '%' }"></div>
                </div>
                <span class="bd-vis-count">{{ sh.submitted }} / {{ sh.total }}</span>
              </div>
            </div>
            <Icon v-if="sh.completed" icon="mdi:check-circle" class="w-5 h-5 bd-vis-check" />
          </div>
        </div>

        <button
          v-if="myPendingVisions.length > 0"
          class="bd-cta-submit bd-cta-block"
          @click="openVisionsModal"
        >
          <Icon icon="mdi:pencil-outline" class="w-4 h-4" />
          {{ $t('SubmitMyVisions') || 'Submit My Visions' }}
          <span class="bd-cta-chip">{{ myPendingVisions.length }}</span>
        </button>
      </section>

      <!-- Opinions — only shown once stakeholders have started voting on minutes -->
      <section v-show="hasOpinions" class="bd-card">
        <BidOpinionsPanel :bid-id="bid.id" @has-data="(v) => (hasOpinions = v)" />
      </section>

      <!-- History -->
      <section class="bd-card">
        <header class="bd-section-head">
          <h3>
            <Icon icon="mdi:history" class="w-4 h-4" />
            {{ $t('History') }}
            <span class="bd-section-count">{{ bid.history.length }}</span>
          </h3>
        </header>
        <div v-if="bid.history.length === 0" class="bd-empty-inline">
          {{ $t('NoHistoryYet') || 'No history yet.' }}
        </div>
        <div v-else class="bd-history">
          <div v-for="h in bid.history" :key="h.id" class="bd-hist-row">
            <div class="bd-hist-dot"></div>
            <div class="bd-hist-body">
              <div class="bd-hist-line">
                <strong v-if="h.fromStatusName">{{ h.fromStatusName }}</strong>
                <Icon v-if="h.fromStatusName" icon="mdi:arrow-right" class="w-3 h-3 bd-hist-arrow" />
                <strong>{{ h.toStatusName }}</strong>
              </div>
              <div class="bd-hist-meta">
                <span>{{ h.changedByName }}</span>
                <span>·</span>
                <span>{{ formatDateTime(h.changedDate) }}</span>
              </div>
              <div v-if="h.note" class="bd-hist-note">{{ h.note }}</div>
            </div>
          </div>
        </div>
      </section>

      <!-- My Visions Modal — opened from the Visions tab -->
      <Modal v-model="visionsModalOpen" :title="$t('SubmitMyVisions') || 'Submit My Visions'" size="xl">
        <div v-if="myVisions.length === 0" class="bd-empty-inline">{{ $t('NoData') }}</div>
        <div v-else class="bd-visforms">
          <div
            v-for="v in myVisions"
            :key="v.id"
            class="bd-visform"
            :class="{ submitted: v.statusId === 2 }"
          >
            <div class="bd-visform-head">
              <span class="bd-visform-item">
                <Icon icon="mdi:format-list-bulleted" class="w-3 h-3" />
                {{ v.bidItemTitle || ('#' + v.bidItemId) }}
              </span>
              <span v-if="v.statusId === 2" class="bd-visform-submitted">
                <Icon icon="mdi:check" class="w-3 h-3" />
                {{ $t('Submitted') || 'Submitted' }}
              </span>
            </div>
            <TextEditor
              v-model="visionDrafts[v.id]"
              :placeholder="$t('YourVision') || 'Your vision / opinion'"
              :readonly="v.statusId === 2"
            />
            <div v-if="v.statusId !== 2" class="bd-visform-actions">
              <button class="bd-btn-secondary" :disabled="savingVisionId === v.id" @click="saveVisionDraft(v)">
                {{ $t('SaveDraft') || 'Save Draft' }}
              </button>
              <button class="bd-btn-primary" :disabled="savingVisionId === v.id || !(visionDrafts[v.id] || '').trim()" @click="submitVision(v)">
                <Icon icon="mdi:send" class="w-3 h-3" />
                {{ $t('Submit') || 'Submit' }}
              </button>
            </div>
          </div>
        </div>
      </Modal>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageHeader from '@/components/layout/PageHeader.vue'
import Icon from '@/components/ui/Icon.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import BidsService, { type BidDetail, type BidStatusDto, type BidItem, BidStatus } from '@/services/BidsService'
import VisionsService, { VisionStatus, type BidItemVision, type BidVisionsSummary } from '@/services/VisionsService'
import { WorkflowService, type WorkflowInstance } from '@/services/WorkflowService'
import BidItemsPanel from '@/components/app/bids/BidItemsPanel.vue'
import BidStakeholdersPanel from '@/components/app/bids/BidStakeholdersPanel.vue'
import BidAttachmentsPanel from '@/components/app/bids/BidAttachmentsPanel.vue'
import BidOpinionsPanel from '@/components/app/bids/BidOpinionsPanel.vue'
import Modal from '@/components/ui/Modal.vue'
import TextEditor from '@/components/ui/TextEditor.vue'
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

// Workflow instance — drives the transition card (RBAC)
const workflowInstance = ref<WorkflowInstance | null>(null)
const selectedTransitionId = ref<number | null>(null)

// Bid items — extracted from the BidDetail.items list (read by load())
const bidItems = computed<BidItem[]>(() => {
  const raw = (bid.value as any)?.items || []
  return raw.map((i: any) => ({
    id: i.id,
    referenceNumber: i.referenceNumber || '',
    externalReferenceNumber: i.externalReferenceNumber || null,
    content: i.content || '',
    internalNote: i.internalNote || null,
    order: i.order ?? 0
  }))
})

// Items can only be edited while the bid is shapeable: Draft, Returned, or VisionPreparation.
// The backend enforces the same rule + the creator/team-leader check; this just hides the buttons.
const canEditItems = computed(() => {
  if (!bid.value) return false
  const editableStatuses = [BidStatus.Draft, BidStatus.Returned, BidStatus.VisionPreparation]
  if (!editableStatuses.includes(bid.value.statusId)) return false
  const myId = userStore.loggedInUser?.id || ''
  return bid.value.createdBy === myId
      || bid.value.teamLeaderUserId === myId
})

// Visions
const visionsSummary = ref<BidVisionsSummary | null>(null)
const myVisions = ref<BidItemVision[]>([])
const visionsModalOpen = ref(false)
const visionDrafts = ref<Record<number, string>>({})
const savingVisionId = ref<number | null>(null)

const myPendingVisions = computed(() => myVisions.value.filter(v => v.statusId === VisionStatus.Draft))

const visionsOverallPct = computed(() => {
  if (!visionsSummary.value || visionsSummary.value.totalVisions === 0) return 0
  return Math.round((visionsSummary.value.submittedVisions / visionsSummary.value.totalVisions) * 100)
})

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
    const [bidRes, statusRes, nextRes, instanceRes]: any = await Promise.all([
      BidsService.getById(bidId.value),
      BidsService.listStatuses(),
      BidsService.allowedNextStatuses(bidId.value),
      WorkflowService.getInstanceForBid(bidId.value).catch(() => null)
    ])
    bid.value = bidRes?.data ?? bidRes
    allStatuses.value = statusRes?.data ?? statusRes ?? []
    allowedNextIds.value = nextRes?.data ?? nextRes ?? []
    selectedTargetId.value = allowedNextIds.value[0] || null

    workflowInstance.value = instanceRes
    selectedTransitionId.value = instanceRes?.availableTransitions?.[0]?.id ?? null

    // Load vision data in parallel with the rest — non-critical, fail-silent
    await loadVisions()
  } catch (err) {
    console.error('Failed to load bid:', err)
  } finally {
    loading.value = false
  }
}

/**
 * Creator in Draft (or Returned) gets the Submit button in the sticky action bar —
 * this is the only case where workflow actions live on the detail page instead
 * of the Tasks page. Rationale: the creator is already authoring the bid,
 * so a separate "task to do your own bid" is pure noise.
 */
const isCreatorDrafting = computed(() => {
  if (!bid.value || !workflowInstance.value) return false
  const status = bid.value.statusId
  const myId = userStore.loggedInUser?.id
  return (status === BidStatus.Draft || status === BidStatus.Returned)
      && bid.value.createdBy === myId
      && workflowInstance.value.availableTransitions.length > 0
})

/** First Advance/Approve transition — drives the primary action button. */
const primaryTransition = computed(() => {
  const list = workflowInstance.value?.availableTransitions || []
  return list.find(t => t.actionType === 1 || t.actionType === 2) || list[0] || null
})

/** True when the bid has at least one open transition the user could act on
 *  but they aren't the creator-in-draft (so they should go to Tasks). */
const hasOpenTask = computed(() => {
  if (!workflowInstance.value || workflowInstance.value.currentStepIsTerminal) return false
  return workflowInstance.value.canCurrentUserAct && !isCreatorDrafting.value
})

// (Tabs removed — content is shown as a single scrolling stack of cards.)

// Hides the Opinions card entirely when there are no opinion rows yet (so we
// don't render an empty wrapper after the BidOpinionsPanel's internal v-if hides itself).
const hasOpinions = ref(false)

// ─── Description inline edit ───────────────────────────────
const descEditing = ref(false)
const descDraft = ref('')
const descSaving = ref(false)

function startEditDescription() {
  descDraft.value = bid.value?.description || ''
  descEditing.value = true
}
function cancelEditDescription() {
  descEditing.value = false
  descDraft.value = ''
}
async function saveDescription() {
  if (!bid.value) return
  descSaving.value = true
  try {
    // Focused PATCH so we only touch the description column — the full UpdateAsync
    // would also re-replace stakeholders (it treats the whole DTO as authoritative).
    await BidsService.updateDescription(bid.value.id, descDraft.value)
    toast.success(t('Saved') || 'Saved')
    descEditing.value = false
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    descSaving.value = false
  }
}

async function submitDraft(transitionId: number) {
  if (!workflowInstance.value) return
  transitioning.value = true
  try {
    await WorkflowService.fireTransition(workflowInstance.value.id, transitionId, transitionNote.value)
    transitionNote.value = ''
    toast.success(t('BidSubmittedForApproval') || 'Submitted for approval')
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    transitioning.value = false
  }
}

const loadVisions = async () => {
  if (!bidId.value) return
  try {
    const [summary, mine] = await Promise.all([
      VisionsService.getSummary(bidId.value).catch(() => null),
      VisionsService.listMine(bidId.value).catch(() => [])
    ])
    visionsSummary.value = summary && summary.totalVisions > 0 ? summary : null
    myVisions.value = mine || []
    // Seed drafts from existing comments
    const drafts: Record<number, string> = {}
    for (const v of myVisions.value) drafts[v.id] = v.comment || ''
    visionDrafts.value = drafts
  } catch (err) {
    console.error('Failed to load visions:', err)
  }
}

const openVisionsModal = () => {
  visionsModalOpen.value = true
}

const saveVisionDraft = async (v: BidItemVision) => {
  savingVisionId.value = v.id
  try {
    await VisionsService.saveDraft(v.id, visionDrafts.value[v.id] || '')
    toast.success(t('DraftSaved') || 'Draft saved')
    await loadVisions()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    savingVisionId.value = null
  }
}

const submitVision = async (v: BidItemVision) => {
  const comment = (visionDrafts.value[v.id] || '').trim()
  if (!comment) return
  savingVisionId.value = v.id
  try {
    await VisionsService.submit(v.id, comment)
    toast.success(t('VisionSubmitted') || 'Vision submitted')
    await loadVisions()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    savingVisionId.value = null
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
/* ──────────────────────────────────────────────────────────────────
   BidDetail — clean, sectional layout. Sticky bar + hero + tabs.
   Colors: MISA primary #006d4b, neutrals, no jarring yellow blocks.
   ────────────────────────────────────────────────────────────────── */

.bd-page {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding-bottom: 24px;
}

/* Loading */
.bd-loading { display: flex; justify-content: center; padding: 80px 0; }
.bd-spinner {
  width: 36px; height: 36px;
  border: 3px solid #e2e8f0; border-top-color: #006d4b;
  border-radius: 50%;
  animation: bd-spin 0.7s linear infinite;
}
@keyframes bd-spin { to { transform: rotate(360deg); } }

/* ─────── Action bar (NOT sticky — sits with the rest of the content) ─────── */
.bd-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 14px 18px;
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 12px;
}

.bd-bar-left { display: flex; align-items: center; gap: 12px; min-width: 0; flex: 1; }

.bd-iconbtn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 36px; height: 36px;
  border-radius: 8px;
  background: #f4f8f6;
  color: #475569;
  border: 1px solid #e4ede8;
  cursor: pointer;
  transition: all 0.15s;
  flex-shrink: 0;
}
.bd-iconbtn:hover { background: #006d4b; color: #fff; border-color: #006d4b; }

.bd-titles { min-width: 0; flex: 1; }
.bd-title {
  margin: 0;
  font-size: 17px;
  font-weight: 800;
  color: #0f172a;
  line-height: 1.25;
  letter-spacing: -0.2px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.bd-meta {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 5px;
  flex-wrap: wrap;
}
.bd-ref, .bd-committee {
  display: inline-flex; align-items: center; gap: 4px;
  font-size: 11px; font-weight: 600;
  padding: 2px 8px; border-radius: 8px;
}
.bd-ref       { background: rgba(0, 109, 75, 0.08); color: #006d4b; }
.bd-committee { background: #f1f5f9; color: #475569; }
.bd-status-pill {
  padding: 2px 10px; border-radius: 10px;
  font-size: 11px; font-weight: 700;
  border: 1px solid;
}
.bd-status-pill.status-draft     { background: #f1f5f9; color: #475569; border-color: #cbd5e1; }
.bd-status-pill.status-pending   { background: #fef3c7; color: #92400e; border-color: #fde68a; }
.bd-status-pill.status-vision    { background: #dbeafe; color: #1d4ed8; border-color: #93c5fd; }
.bd-status-pill.status-meeting   { background: #e0e7ff; color: #4338ca; border-color: #a5b4fc; }
.bd-status-pill.status-external  { background: #fce7f3; color: #9d174d; border-color: #f9a8d4; }
.bd-status-pill.status-final     { background: #dcfce7; color: #15803d; border-color: #86efac; }
.bd-status-pill.status-completed { background: #d1fae5; color: #047857; border-color: #6ee7b7; }
.bd-status-pill.status-returned  { background: #fee2e2; color: #b91c1c; border-color: #fca5a5; }
.bd-overdue {
  display: inline-flex; align-items: center; gap: 3px;
  padding: 2px 8px; border-radius: 8px;
  background: #fef2f2; color: #b91c1c; border: 1px solid #fca5a5;
  font-size: 10.5px; font-weight: 700;
}

.bd-bar-right { display: flex; align-items: center; gap: 8px; flex-shrink: 0; }

.bd-cta-submit {
  display: inline-flex; align-items: center; gap: 8px;
  padding: 11px 22px;
  background: linear-gradient(135deg, #006d4b 0%, #005339 100%);
  color: #fff;
  border: none;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  letter-spacing: 0.2px;
  box-shadow: 0 4px 14px rgba(0, 109, 75, 0.28);
  transition: transform 0.15s, box-shadow 0.2s;
}
.bd-cta-submit:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 6px 18px rgba(0, 109, 75, 0.38);
}
.bd-cta-submit:disabled { opacity: 0.6; cursor: not-allowed; transform: none; box-shadow: none; }

.bd-cta-tasks {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 9px 16px;
  background: #fff;
  color: #006d4b;
  border: 1.5px solid #006d4b;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  transition: all 0.15s;
}
.bd-cta-tasks:hover { background: #006d4b; color: #fff; }

/* ─────── Hero card ─────── */
.bd-hero {
  display: grid;
  grid-template-columns: 1fr 360px;
  gap: 16px;
  padding: 18px;
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 12px;
}
@media (max-width: 900px) { .bd-hero { grid-template-columns: 1fr; } }

.bd-hero-timeline { display: flex; flex-direction: column; gap: 12px; min-width: 0; }
.bd-tl-head {
  display: flex; align-items: center; gap: 8px;
  font-size: 13px; color: #006d4b;
}
.bd-tl-head h3 {
  margin: 0; font-size: 13px; font-weight: 700;
  color: #1a2e25; flex: 1;
}
.bd-step-counter {
  font-size: 11px; font-weight: 700;
  padding: 2px 9px; border-radius: 8px;
  background: rgba(0, 109, 75, 0.08); color: #006d4b;
}

.bd-tl-track {
  display: flex; align-items: center; gap: 4px;
  padding: 4px 0;
}
.bd-tl-step {
  flex: 1; display: flex; justify-content: center;
  position: relative;
}
.bd-tl-step::before {
  content: '';
  position: absolute;
  top: 50%;
  inset-inline-start: 50%;
  inset-inline-end: -50%;
  height: 2px;
  background: #e4ede8;
  z-index: 0;
}
.bd-tl-step:last-child::before { display: none; }
.bd-tl-step.completed::before  { background: #006d4b; }
.bd-tl-step.current::before    { background: linear-gradient(90deg, #006d4b 50%, #e4ede8 50%); }
[dir="rtl"] .bd-tl-step.current::before { background: linear-gradient(-90deg, #006d4b 50%, #e4ede8 50%); }

.bd-tl-dot {
  position: relative; z-index: 1;
  width: 22px; height: 22px;
  border-radius: 50%;
  background: #fff;
  border: 2px solid #cbd5e1;
  display: flex; align-items: center; justify-content: center;
  color: #006d4b;
  font-size: 11px; font-weight: 700;
  transition: all 0.2s;
}
.bd-tl-step.completed .bd-tl-dot { background: #006d4b; border-color: #006d4b; color: #fff; }
.bd-tl-step.current .bd-tl-dot {
  background: #fff;
  border-color: #006d4b;
  border-width: 3px;
  box-shadow: 0 0 0 4px rgba(0, 109, 75, 0.15);
  width: 24px; height: 24px;
}

.bd-tl-current {
  display: flex; align-items: center; gap: 8px;
  padding: 9px 12px; background: #f7faf8;
  border: 1px solid #e4ede8; border-radius: 8px;
  font-size: 12.5px;
}
.bd-tl-label { color: #6b8a7d; font-weight: 500; }
.bd-tl-current strong { color: #006d4b; font-weight: 700; }

/* Hero info grid */
.bd-hero-info {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0;
  padding: 14px;
  background: #f7faf8;
  border: 1px solid #e4ede8;
  border-radius: 10px;
  align-content: start;
}
.bd-info-row {
  display: flex; flex-direction: column; gap: 2px;
  padding: 8px 6px;
  border-bottom: 1px dashed #e4ede8;
}
.bd-info-row:nth-last-child(-n+2) { border-bottom: none; }
.bd-info-row--full { grid-column: 1 / -1; }
.bd-info-row.is-overdue .bd-info-val { color: #b91c1c; font-weight: 700; }
.bd-info-key {
  font-size: 10.5px; font-weight: 700;
  color: #6b8a7d; text-transform: uppercase; letter-spacing: 0.4px;
}
.bd-info-val { font-size: 13px; font-weight: 600; color: #1a2e25; }

/* ─────── Hint banner ─────── */
.bd-hint {
  display: flex; align-items: center; gap: 8px;
  padding: 11px 14px;
  background: #ecfdf5;
  border: 1px solid #a7f3d0;
  border-radius: 10px;
  color: #047857;
  font-size: 13px;
}

/* ─────── Stacked content cards ─────── */
.bd-card {
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 12px;
  padding: 18px;
}

/* Side-by-side row (Stakeholders + Attachments). Stacks on small screens. */
.bd-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}
@media (max-width: 900px) {
  .bd-row { grid-template-columns: 1fr; }
}

/* Section headers inside cards (Visions / History) */
.bd-section-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 14px;
  padding-bottom: 12px;
  border-bottom: 1px solid #f1f5f9;
}
.bd-section-head h3 {
  margin: 0;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 15px;
  font-weight: 700;
  color: #1a2e25;
}
.bd-section-count {
  margin-inline-start: auto;
  padding: 2px 9px;
  background: rgba(0, 109, 75, 0.08);
  color: #006d4b;
  border-radius: 10px;
  font-size: 11px;
  font-weight: 700;
}

/* Empty state — soft, professional */
.bd-empty {
  display: flex; flex-direction: column; align-items: center;
  gap: 10px; padding: 60px 20px;
  color: #93afa4; text-align: center;
}
.bd-empty p { margin: 0; font-size: 13px; max-width: 480px; }

.bd-empty-inline {
  padding: 24px; text-align: center;
  color: #93afa4; font-size: 13px;
}

/* Description block — sits inside its own bd-card (no inner background) */
.bd-description { padding: 18px; }

.bd-desc-head {
  display: flex; align-items: center; gap: 8px;
  margin-bottom: 12px;
  padding-bottom: 10px;
  border-bottom: 1px solid #f1f5f9;
  color: #006d4b;
}
.bd-desc-head h4 {
  margin: 0; font-size: 14px; font-weight: 700; color: #1a2e25;
  flex: 1;          /* push the edit button to the row's far end */
}
.bd-desc-edit-btn {
  display: inline-flex; align-items: center; gap: 5px;
  padding: 5px 11px;
  background: #fff;
  border: 1px solid #d4e0da;
  border-radius: 7px;
  color: #006d4b;
  font-size: 12px; font-weight: 600;
  cursor: pointer; font-family: inherit;
  transition: all 0.15s;
}
.bd-desc-edit-btn:hover {
  background: #006d4b; color: #fff; border-color: #006d4b;
  box-shadow: 0 2px 8px rgba(0, 109, 75, 0.2);
}

.bd-desc-body {
  font-size: 13.5px; color: #1a2e25; line-height: 1.6;
}
.bd-desc-empty {
  padding: 18px; text-align: center;
  color: #93afa4; font-size: 13px; font-style: italic;
  background: #f7faf8; border: 1px dashed #d4e0da; border-radius: 8px;
}

/* Edit mode — editor + action row with breathing room */
.bd-desc-edit { display: flex; flex-direction: column; gap: 14px; }
.bd-desc-actions {
  display: flex; justify-content: flex-end; gap: 8px;
  padding-top: 12px;
  border-top: 1px solid #f1f5f9;
}

/* ─────── Visions tab ─────── */
.bd-vis-progress {
  display: flex; align-items: center; gap: 12px;
  margin-bottom: 14px;
}
.bd-vis-bar {
  flex: 1; height: 10px;
  background: #e4ede8; border-radius: 8px; overflow: hidden;
}
.bd-vis-bar-fill {
  height: 100%;
  background: linear-gradient(90deg, #63a58f, #006d4b);
  transition: width 0.3s ease;
}
.bd-vis-bar-fill.complete {
  background: linear-gradient(90deg, #22c55e, #15803d);
}
.bd-vis-pct { font-size: 12px; font-weight: 700; color: #006d4b; min-width: 90px; text-align: end; }

.bd-vis-list { display: flex; flex-direction: column; gap: 8px; margin-bottom: 14px; }
.bd-vis-row {
  display: flex; align-items: center; gap: 10px;
  padding: 10px 12px;
  background: #f7faf8;
  border: 1px solid #e4ede8;
  border-radius: 10px;
}
.bd-vis-row.complete { background: #f0fdf4; border-color: #bbf7d0; }
.bd-vis-info { flex: 1; min-width: 0; }
.bd-vis-name {
  font-size: 12.5px; font-weight: 600; color: #1a2e25;
  display: flex; align-items: center; gap: 6px;
}
.bd-vis-ext {
  padding: 1px 6px; background: #eef2ff; color: #3b4cca;
  border: 1px solid #c5cbf5; border-radius: 6px;
  font-size: 10px; font-weight: 700; text-transform: uppercase;
}
.bd-vis-progress-row { display: flex; align-items: center; gap: 8px; margin-top: 4px; }
.bd-vis-mini { flex: 1; height: 4px; background: #e4ede8; border-radius: 3px; overflow: hidden; }
.bd-vis-mini-fill { height: 100%; background: #63a58f; transition: width 0.25s; }
.bd-vis-mini-fill.complete { background: #22c55e; }
.bd-vis-count { font-size: 10.5px; font-weight: 700; color: #5a7a6d; min-width: 36px; text-align: end; }
.bd-vis-check { color: #22c55e; flex-shrink: 0; }

.bd-cta-block { display: flex; width: 100%; justify-content: center; }
.bd-cta-chip {
  display: inline-flex; align-items: center; justify-content: center;
  min-width: 18px; height: 18px; padding: 0 6px;
  background: rgba(255, 255, 255, 0.22); border-radius: 9px;
  font-size: 10.5px; font-weight: 700; margin-inline-start: 4px;
}

/* ─────── History tab ─────── */
.bd-history { display: flex; flex-direction: column; gap: 0; }
.bd-hist-row {
  display: flex; gap: 12px;
  padding: 12px 0;
  border-bottom: 1px dashed #e4ede8;
}
.bd-hist-row:last-child { border-bottom: none; }
.bd-hist-dot {
  width: 10px; height: 10px; border-radius: 50%;
  background: #006d4b; margin-top: 6px; flex-shrink: 0;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.15);
}
.bd-hist-body { flex: 1; min-width: 0; }
.bd-hist-line {
  display: flex; align-items: center; gap: 6px;
  font-size: 13px; color: #1a2e25;
}
.bd-hist-arrow { color: #6b8a7d; }
.bd-hist-meta {
  font-size: 11.5px; color: #6b8a7d;
  margin-top: 3px; display: flex; gap: 5px;
}
.bd-hist-note {
  margin-top: 5px; padding: 7px 10px;
  background: #f7faf8; border-radius: 6px;
  font-size: 12px; color: #475569;
}

/* ─────── Visions submission modal ─────── */
.bd-visforms { display: flex; flex-direction: column; gap: 12px; }
.bd-visform {
  padding: 12px;
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 10px;
}
.bd-visform.submitted { background: #f0fdf4; border-color: #bbf7d0; }
.bd-visform-head {
  display: flex; justify-content: space-between; align-items: center;
  margin-bottom: 8px;
}
.bd-visform-item {
  display: inline-flex; align-items: center; gap: 5px;
  padding: 3px 9px; background: rgba(0, 109, 75, 0.08);
  color: #006d4b; border-radius: 8px;
  font-size: 12px; font-weight: 600;
}
.bd-visform-submitted {
  display: inline-flex; align-items: center; gap: 3px;
  padding: 2px 8px; background: #dcfce7; color: #15803d;
  border: 1px solid #86efac; border-radius: 8px;
  font-size: 10.5px; font-weight: 700;
}
.bd-visform-actions {
  display: flex; justify-content: flex-end; gap: 8px;
  margin-top: 10px;
}

.bd-btn-primary, .bd-btn-secondary {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 8px 14px;
  border-radius: 8px; font-size: 13px; font-weight: 600;
  cursor: pointer; font-family: inherit;
  border: 1px solid transparent; transition: all 0.15s;
}
.bd-btn-primary { background: #006d4b; color: #fff; }
.bd-btn-primary:hover:not(:disabled) { background: #005339; }
.bd-btn-primary:disabled { opacity: 0.55; cursor: not-allowed; }
.bd-btn-secondary { background: #fff; color: #374a41; border-color: #d4e0da; }
.bd-btn-secondary:hover { background: #f4f8f6; }

/* ─────── Responsive ─────── */
@media (max-width: 768px) {
  .bd-bar { flex-direction: column; align-items: stretch; }
  .bd-bar-right { width: 100%; }
  .bd-cta-submit, .bd-cta-tasks { width: 100%; justify-content: center; }
  .bd-hero-info { grid-template-columns: 1fr; }
}
</style>

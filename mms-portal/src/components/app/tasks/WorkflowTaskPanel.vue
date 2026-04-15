<template>
  <div class="wfp">
    <div v-if="loading" class="wfp-loading"><div class="spinner"></div></div>

    <template v-else-if="instance">
      <!-- Bid context header -->
      <header class="wfp-header">
        <div class="wfp-header-left">
          <div class="wfp-bid-ref">
            <Icon icon="mdi:pound" class="w-3 h-3" />
            <span>{{ bidRef }}</span>
          </div>
          <h2 class="wfp-bid-subject">{{ bidSubject || $t('Bid') }}</h2>
          <div v-if="committeeName" class="wfp-committee">
            <Icon icon="mdi:briefcase-outline" class="w-3 h-3" />
            <span>{{ committeeName }}</span>
          </div>
        </div>
        <button class="wfp-link" @click="openBidDetail" :title="$t('OpenBidDetail') || 'Open bid detail'">
          <Icon icon="mdi:open-in-new" class="w-4 h-4" />
          {{ $t('OpenBid') || 'Open bid' }}
        </button>
      </header>

      <!-- Current step + task body -->
      <section class="wfp-current">
        <div class="wfp-current-row">
          <span class="wfp-label">{{ $t('CurrentStep') || 'Current step' }}</span>
          <strong>{{ instance.currentStepName }}</strong>
          <span class="wfp-step-order">#{{ instance.currentStepOrder }}</span>
        </div>
        <div v-if="taskTitle" class="wfp-task-title">{{ taskTitle }}</div>
        <div v-if="taskBody" class="wfp-task-body">{{ taskBody }}</div>
        <div v-if="dueDate" class="wfp-due" :class="{ overdue: isOverdue }">
          <Icon icon="mdi:calendar-clock" class="w-3 h-3" />
          <span>{{ $t('Due') || 'Due' }}: {{ formatDate(dueDate) }}</span>
          <em v-if="isOverdue" class="wfp-overdue-tag">{{ $t('Overdue') || 'Overdue' }}</em>
        </div>
      </section>

      <!-- Note input -->
      <section class="wfp-note">
        <label>{{ $t('TransitionNote') || 'Note' }}</label>
        <textarea v-model="note" rows="3" :placeholder="$t('OptionalNotePlaceholder') || 'Optional note for the next actor / audit trail'"></textarea>
      </section>

      <!-- Transitions / actions -->
      <section v-if="instance.currentStepIsTerminal" class="wfp-terminal">
        <Icon icon="mdi:check-circle" class="w-5 h-5" />
        <span>{{ $t('WorkflowCompleted') || 'Workflow completed' }}: {{ instance.currentStepName }}</span>
      </section>

      <section v-else class="wfp-actions">
        <div v-if="!instance.canCurrentUserAct" class="wfp-not-actor">
          <Icon icon="mdi:lock-outline" class="w-4 h-4" />
          <span>{{ $t('NotAuthorizedToAdvance') || 'You are not the assigned actor for this step.' }}</span>
        </div>
        <template v-else>
          <button
            v-for="t in instance.availableTransitions"
            :key="t.id"
            class="wfp-action-btn"
            :class="actionClass(t.actionType)"
            :disabled="firing"
            @click="fire(t.id)"
          >
            <Icon
              :icon="actionIcon(t.actionType)"
              class="w-4 h-4"
            />
            {{ isRtl ? t.labelAr : t.labelEn }}
          </button>
          <div v-if="instance.availableTransitions.length === 0" class="wfp-empty">
            {{ $t('NoNextStatuses') || 'No further actions available from this step.' }}
          </div>
        </template>
      </section>

      <!-- History (compact) -->
      <section v-if="history.length > 0" class="wfp-history">
        <div class="wfp-history-title">{{ $t('History') || 'History' }}</div>
        <div v-for="h in history" :key="h.id" class="wfp-hist-row">
          <div class="wfp-hist-dot"></div>
          <div class="wfp-hist-body">
            <div class="wfp-hist-line">
              <strong v-if="h.fromStepName">{{ h.fromStepName }}</strong>
              <Icon v-if="h.fromStepName" icon="mdi:arrow-right" class="w-3 h-3 wfp-hist-arrow" />
              <strong>{{ h.toStepName }}</strong>
            </div>
            <div class="wfp-hist-meta">
              <span>{{ h.changedByName }}</span>
              <span>·</span>
              <span>{{ formatDateTime(h.changedDate) }}</span>
            </div>
            <div v-if="h.note" class="wfp-hist-note">{{ h.note }}</div>
          </div>
        </div>
      </section>
    </template>

    <div v-else class="wfp-empty-state">{{ $t('NoWorkflowInstance') || 'No workflow data found.' }}</div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import { WorkflowService, WorkflowActionType, type WorkflowInstance, type WorkflowHistoryItem } from '@/services/WorkflowService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'
import { useUserStore } from '@/stores/user'

interface Props {
  bidId: number
  bidReferenceNumber?: string | null
  bidSubject?: string | null
  committeeName?: string | null
  taskTitle?: string | null
  taskBody?: string | null
  dueDate?: string | null
}

const props = withDefaults(defineProps<Props>(), {
  bidReferenceNumber: null,
  bidSubject: null,
  committeeName: null,
  taskTitle: null,
  taskBody: null,
  dueDate: null
})

const emit = defineEmits<{ advanced: [] }>()

const router = useRouter()
const { t } = useI18n()
const { toast } = useToast()
const userStore = useUserStore()
const isRtl = computed(() => userStore.isRtl)

const loading = ref(false)
const firing = ref(false)
const instance = ref<WorkflowInstance | null>(null)
const history = ref<WorkflowHistoryItem[]>([])
const note = ref('')

const bidRef = computed(() => props.bidReferenceNumber || `BID-${props.bidId}`)
const isOverdue = computed(() => {
  if (!props.dueDate) return false
  return new Date(props.dueDate).getTime() < Date.now()
})

async function load() {
  loading.value = true
  try {
    const inst = await WorkflowService.getInstanceForBid(props.bidId).catch(() => null)
    instance.value = inst
    if (inst) {
      history.value = await WorkflowService.getHistory(inst.id).catch(() => [])
    } else {
      history.value = []
    }
  } finally {
    loading.value = false
  }
}

async function fire(transitionId: number) {
  if (!instance.value) return
  firing.value = true
  try {
    await WorkflowService.fireTransition(instance.value.id, transitionId, note.value)
    note.value = ''
    toast.success(t('StatusChangedSuccessfully') || 'Status changed')
    emit('advanced')
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('NotAuthorizedToAdvance') || t('ErrorOccured'))
  } finally {
    firing.value = false
  }
}

const openBidDetail = () => router.push(`/bids/${props.bidId}`)

function actionClass(actionType: number): string {
  switch (actionType) {
    case WorkflowActionType.Approve: return 'tx-approve'
    case WorkflowActionType.Reject:  return 'tx-reject'
    case WorkflowActionType.Auto:    return 'tx-auto'
    default: return 'tx-advance'
  }
}

function actionIcon(actionType: number): string {
  switch (actionType) {
    case WorkflowActionType.Approve: return 'mdi:check'
    case WorkflowActionType.Reject:  return 'mdi:close'
    case WorkflowActionType.Auto:    return 'mdi:auto-fix'
    default: return 'mdi:arrow-right'
  }
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString()
}
function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString()
}

watch(() => props.bidId, () => { if (props.bidId) load() })
onMounted(() => { if (props.bidId) load() })
</script>

<style scoped>
.wfp { display: flex; flex-direction: column; gap: 16px; padding: 20px; }

.wfp-loading { display: flex; justify-content: center; padding: 60px 0; }
.spinner { width: 32px; height: 32px; border: 3px solid #e4ede8; border-top-color: #006d4b; border-radius: 50%; animation: wfp-spin 0.8s linear infinite; }
@keyframes wfp-spin { to { transform: rotate(360deg); } }

.wfp-header {
  display: flex; justify-content: space-between; align-items: flex-start; gap: 12px;
  padding-bottom: 14px; border-bottom: 1px solid #e4ede8;
}
.wfp-bid-ref {
  display: inline-flex; align-items: center; gap: 4px;
  font-size: 11px; font-weight: 700; padding: 3px 9px;
  background: rgba(0, 109, 75, 0.08); color: #006d4b; border-radius: 8px;
}
.wfp-bid-subject { margin: 6px 0 4px; font-size: 18px; font-weight: 800; color: #1a2e25; line-height: 1.3; }
.wfp-committee {
  display: inline-flex; align-items: center; gap: 5px;
  font-size: 12px; color: #6b8a7d; padding: 2px 9px;
  background: #f4f8f6; border-radius: 8px;
}
.wfp-link {
  display: inline-flex; align-items: center; gap: 5px; padding: 7px 12px;
  background: transparent; border: 1px solid #d4e0da; border-radius: 8px;
  color: #475569; font-size: 12px; font-weight: 600; cursor: pointer; font-family: inherit; transition: all 0.15s;
}
.wfp-link:hover { background: #f4f8f6; border-color: #006d4b; color: #006d4b; }

.wfp-current {
  background: #f7faf8; border: 1px solid #e4ede8; border-radius: 10px; padding: 14px;
  display: flex; flex-direction: column; gap: 8px;
}
.wfp-current-row { display: flex; align-items: center; gap: 8px; }
.wfp-label { font-size: 12px; font-weight: 600; color: #6b8a7d; }
.wfp-current-row strong { font-size: 14px; font-weight: 700; color: #006d4b; }
.wfp-step-order {
  margin-inline-start: auto; font-size: 11px; font-weight: 700; color: #6b8a7d;
  background: #fff; padding: 2px 8px; border-radius: 8px; border: 1px solid #d4e0da;
}
.wfp-task-title { font-size: 13.5px; font-weight: 700; color: #1a2e25; }
.wfp-task-body { font-size: 13px; color: #475569; line-height: 1.5; white-space: pre-line; }
.wfp-due {
  display: inline-flex; align-items: center; gap: 5px; padding: 4px 10px;
  background: #fff; border: 1px solid #e4ede8; border-radius: 8px;
  font-size: 11.5px; color: #6b8a7d; align-self: flex-start;
}
.wfp-due.overdue { background: #fef2f2; border-color: #fca5a5; color: #b91c1c; font-weight: 600; }
.wfp-overdue-tag {
  font-style: normal; padding: 1px 6px; background: #fee2e2; color: #b91c1c;
  border-radius: 6px; font-size: 10px; font-weight: 700; margin-inline-start: 4px;
}

.wfp-note { display: flex; flex-direction: column; gap: 5px; }
.wfp-note label { font-size: 12px; font-weight: 600; color: #374a41; }
.wfp-note textarea {
  padding: 10px 12px; border: 1.5px solid #d4e0da; border-radius: 8px;
  font-size: 13px; background: #f7faf8; color: #1a2e25; font-family: inherit; resize: vertical;
}
.wfp-note textarea:focus { outline: none; border-color: #006d4b; background: #fff; box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1); }

.wfp-terminal {
  display: flex; align-items: center; gap: 8px; padding: 14px;
  background: #dcfce7; border: 1px solid #86efac; border-radius: 10px;
  font-size: 14px; font-weight: 700; color: #15803d;
}

.wfp-actions { display: flex; flex-wrap: wrap; gap: 8px; }
.wfp-not-actor {
  display: flex; align-items: center; gap: 8px; padding: 12px;
  background: #fef9c3; border: 1px solid #fde68a; border-radius: 8px;
  color: #92400e; font-size: 12.5px; line-height: 1.45; flex: 1;
}
.wfp-action-btn {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 10px 18px; font-size: 13px; font-weight: 700; border-radius: 8px;
  border: 1px solid transparent; cursor: pointer; font-family: inherit; transition: all 0.15s;
}
.wfp-action-btn:disabled { opacity: 0.55; cursor: not-allowed; }
.wfp-action-btn.tx-advance { background: #006d4b; color: #fff; }
.wfp-action-btn.tx-advance:hover:not(:disabled) { background: #005339; box-shadow: 0 4px 14px rgba(0, 109, 75, 0.25); }
.wfp-action-btn.tx-approve { background: #15803d; color: #fff; }
.wfp-action-btn.tx-approve:hover:not(:disabled) { background: #14532d; box-shadow: 0 4px 14px rgba(21, 128, 61, 0.25); }
.wfp-action-btn.tx-reject  { background: #fff; color: #b91c1c; border-color: #fca5a5; }
.wfp-action-btn.tx-reject:hover:not(:disabled)  { background: #fef2f2; border-color: #ef4444; }
.wfp-action-btn.tx-auto    { background: #f1f5f9; color: #475569; }

.wfp-empty { padding: 16px; text-align: center; color: #93afa4; font-size: 13px; }
.wfp-empty-state { padding: 60px 20px; text-align: center; color: #93afa4; font-size: 13px; }

.wfp-history { display: flex; flex-direction: column; gap: 0; padding-top: 14px; border-top: 1px solid #e4ede8; }
.wfp-history-title { font-size: 12px; font-weight: 700; color: #6b8a7d; text-transform: uppercase; letter-spacing: 0.4px; margin-bottom: 10px; }
.wfp-hist-row { display: flex; gap: 10px; padding: 8px 0; border-bottom: 1px dashed #f1f5f9; }
.wfp-hist-row:last-child { border-bottom: none; }
.wfp-hist-dot { width: 8px; height: 8px; border-radius: 50%; background: #006d4b; margin-top: 6px; flex-shrink: 0; }
.wfp-hist-body { flex: 1; min-width: 0; }
.wfp-hist-line { display: flex; align-items: center; gap: 5px; font-size: 13px; color: #1a2e25; }
.wfp-hist-line strong { font-weight: 700; }
.wfp-hist-arrow { color: #6b8a7d; }
.wfp-hist-meta { font-size: 11px; color: #6b8a7d; margin-top: 3px; display: flex; gap: 5px; }
.wfp-hist-note { font-size: 12px; color: #475569; margin-top: 4px; padding: 6px 9px; background: #f7faf8; border-radius: 6px; }
</style>

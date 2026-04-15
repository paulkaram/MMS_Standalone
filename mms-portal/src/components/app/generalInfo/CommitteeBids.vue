<template>
  <div class="bids-section">
    <!-- Toolbar -->
    <div class="bids-toolbar">
      <button class="btn-add" @click="openAddModal">
        <Icon icon="mdi:plus" class="w-4 h-4" />
        {{ $t('AddBid') }}
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading && bids.length === 0" class="loading-state">
      <div class="spinner"></div>
    </div>

    <!-- Bids list -->
    <div v-else-if="bids.length > 0" class="bids-list">
      <div
        v-for="bid in bids"
        :key="bid.id"
        class="bid-card"
        :class="{ overdue: bid.isOverdue }"
        @click="openDetail(bid)"
      >
        <div class="bid-card-header">
          <div class="bid-refs">
            <span class="ref-badge">
              <Icon icon="mdi:pound" class="w-3 h-3" />
              {{ bid.referenceNumber }}
            </span>
            <span v-if="bid.externalMeetingNumber" class="ref-badge external">
              <Icon icon="mdi:link-variant" class="w-3 h-3" />
              {{ bid.externalMeetingNumber }}
            </span>
          </div>
          <span class="status-pill" :class="getStatusClass(bid.statusId)">
            {{ bid.statusName }}
          </span>
        </div>

        <h4 class="bid-subject">{{ bid.subject }}</h4>

        <!-- Step progress -->
        <div class="progress-wrap">
          <div class="progress-track">
            <div
              class="progress-fill"
              :class="getStatusClass(bid.statusId)"
              :style="{ width: (bid.statusStepOrder / 12 * 100) + '%' }"
            ></div>
          </div>
          <span class="progress-label">{{ bid.statusStepOrder }} / 12</span>
        </div>

        <div class="bid-meta">
          <span v-if="bid.teamLeaderName" class="meta-item">
            <Icon icon="mdi:account-star" class="w-3 h-3" />
            {{ bid.teamLeaderName }}
          </span>
          <span class="meta-item" :class="{ overdue: bid.isOverdue }">
            <Icon icon="mdi:calendar-clock" class="w-3 h-3" />
            {{ formatDate(bid.dueDate) }}
            <em v-if="bid.isOverdue" class="overdue-badge">{{ $t('Overdue') }}</em>
          </span>
          <span v-if="bid.stakeholdersCount > 0" class="meta-item">
            <Icon icon="mdi:account-group" class="w-3 h-3" />
            {{ bid.stakeholdersCount }}
          </span>
          <span v-if="bid.itemsCount > 0" class="meta-item">
            <Icon icon="mdi:format-list-bulleted" class="w-3 h-3" />
            {{ bid.itemsCount }}
          </span>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-else class="empty-state">
      <Icon icon="mdi:briefcase-outline" class="w-10 h-10" />
      <p>{{ $t('NoBids') }}</p>
    </div>

    <!-- Add/Edit Modal -->
    <Modal v-model="modalOpen" :title="editingBid ? $t('EditBid') : $t('AddBid')" size="lg">
      <form class="bid-form" @submit.prevent="saveBid">
        <div class="form-field">
          <label>{{ $t('Subject') }} <span class="required">*</span></label>
          <input v-model="form.subject" type="text" class="form-input" required />
        </div>

        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('ExternalMeetingNumber') }}</label>
            <input v-model="form.externalMeetingNumber" type="text" class="form-input" />
          </div>
          <div class="form-field">
            <label>{{ $t('TeamLeader') }}</label>
            <CustomSelect
              v-model="form.teamLeaderUserId"
              :options="users"
              value-key="id"
              label-key="name"
              :placeholder="$t('Select') || '-'"
              searchable
              clearable
            />
          </div>
        </div>

        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('StartDate') }} <span class="required">*</span></label>
            <DatePicker
              v-model="form.startDate"
              :placeholder="$t('StartDate')"
              required
            />
          </div>
          <div class="form-field">
            <label>{{ $t('DueDate') }} <span class="required">*</span></label>
            <DatePicker
              v-model="form.dueDate"
              :min-date="form.startDate || undefined"
              :placeholder="$t('DueDate')"
              required
            />
          </div>
        </div>

        <div class="form-field">
          <label>{{ $t('Description') }}</label>
          <textarea v-model="form.description" rows="4" class="form-input"></textarea>
        </div>

        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="modalOpen = false">{{ $t('Cancel') }}</button>
          <button type="submit" class="btn-primary" :disabled="saving">{{ $t('Save') }}</button>
        </div>
      </form>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import CustomSelect from '@/components/ui/CustomSelect.vue'
import DatePicker from '@/components/ui/DatePicker.vue'
import BidsService, { type Bid, type BidPost, BidStatus } from '@/services/BidsService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

interface Props {
  committeeId: string | number
}
const props = defineProps<Props>()
const emit = defineEmits<{ 'update:count': [value: number] }>()

const router = useRouter()
const { t } = useI18n()
const { toast } = useToast()

const bids = ref<Bid[]>([])
const users = ref<{ id: string; name: string }[]>([])
const loading = ref(false)
const saving = ref(false)
const modalOpen = ref(false)
const editingBid = ref<Bid | null>(null)

const committeeIdNum = computed(() => Number(props.committeeId))

// Form type — date fields accept Date from DatePicker or string from initial value
type BidFormModel = Omit<BidPost, 'startDate' | 'dueDate'> & {
  startDate: string | Date | null
  dueDate: string | Date | null
}

const emptyForm = (): BidFormModel => ({
  committeeId: committeeIdNum.value,
  externalMeetingNumber: null,
  subject: '',
  description: null,
  teamLeaderUserId: null,
  startDate: new Date().toISOString().split('T')[0],
  dueDate: new Date().toISOString().split('T')[0],
  stakeholders: []
})

const form = reactive<BidFormModel>(emptyForm())

// Normalize a Date|string|null to YYYY-MM-DD for the backend
const toIsoDate = (v: string | Date | null | undefined): string => {
  if (!v) return ''
  const d = v instanceof Date ? v : new Date(v)
  if (isNaN(d.getTime())) return ''
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

const load = async () => {
  if (!committeeIdNum.value) return
  loading.value = true
  try {
    const res: any = await BidsService.listByCommittee(committeeIdNum.value)
    bids.value = res?.data ?? res ?? []
    emit('update:count', bids.value.length)
  } catch (err) {
    console.error('Failed to load bids:', err)
  } finally {
    loading.value = false
  }
}

/** Scoped to the committee this panel is rendered for — no system-wide users. */
const loadUsers = async () => {
  try {
    const res: any = await BidsService.listCommitteeMembersForPicker(committeeIdNum.value)
    users.value = (res?.data ?? res ?? []).map((u: any) => ({ id: String(u.id), name: u.name }))
  } catch {
    users.value = []
  }
}

const openAddModal = () => {
  editingBid.value = null
  Object.assign(form, emptyForm())
  modalOpen.value = true
}

const openDetail = (bid: Bid) => {
  router.push(`/bids/${bid.id}`)
}

const saveBid = async () => {
  if (!form.subject.trim()) return
  saving.value = true
  try {
    const payload: BidPost = {
      ...form,
      startDate: toIsoDate(form.startDate),
      dueDate: toIsoDate(form.dueDate)
    }
    if (editingBid.value) {
      await BidsService.update(editingBid.value.id, payload)
      toast.success(t('BidUpdatedSuccessfully'))
    } else {
      await BidsService.create(payload)
      toast.success(t('BidCreatedSuccessfully'))
    }
    modalOpen.value = false
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    saving.value = false
  }
}

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

const formatDate = (iso: string): string => {
  if (!iso) return ''
  return new Date(iso).toLocaleDateString()
}

watch(() => props.committeeId, load)

onMounted(() => {
  load()
  loadUsers()
})
</script>

<style scoped>
.bids-section { display: flex; flex-direction: column; gap: 14px; }

.bids-toolbar { display: flex; justify-content: flex-end; }

.btn-add {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 8px 14px;
  background: #006d4b; color: #fff;
  border: none; border-radius: 8px;
  font-size: 13px; font-weight: 600; cursor: pointer;
  font-family: inherit; transition: background 0.2s;
}
.btn-add:hover { background: #005a3e; }

.loading-state { display: flex; justify-content: center; padding: 32px 0; }

.spinner {
  width: 28px; height: 28px;
  border: 3px solid #e2e8f0;
  border-top-color: #006d4b;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

.bids-list { display: flex; flex-direction: column; gap: 10px; }

.bid-card {
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 10px;
  padding: 14px 16px;
  cursor: pointer;
  transition: all 0.15s;
}
.bid-card:hover {
  border-color: #c8ddd3;
  box-shadow: 0 2px 10px rgba(0, 109, 75, 0.06);
  transform: translateY(-1px);
}
.bid-card.overdue {
  border-left: 3px solid #ef4444;
}
[dir="rtl"] .bid-card.overdue {
  border-left: 1px solid #e4ede8;
  border-right: 3px solid #ef4444;
}

.bid-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 8px;
}

.bid-refs {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.ref-badge {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  padding: 2px 8px;
  font-size: 11px;
  font-weight: 600;
  border-radius: 10px;
  background: #e8f5ef;
  color: #006d4b;
  border: 1px solid #c8ddd3;
}
.ref-badge.external {
  background: #eef2ff;
  color: #3b4cca;
  border-color: #c5cbf5;
}

.status-pill {
  padding: 3px 10px;
  font-size: 11px;
  font-weight: 600;
  border-radius: 10px;
  border: 1px solid;
  white-space: nowrap;
}
.status-pill.status-draft { background: #f1f5f9; color: #475569; border-color: #cbd5e1; }
.status-pill.status-pending { background: #fef3c7; color: #92400e; border-color: #fde68a; }
.status-pill.status-vision { background: #dbeafe; color: #1d4ed8; border-color: #93c5fd; }
.status-pill.status-meeting { background: #e0e7ff; color: #4338ca; border-color: #a5b4fc; }
.status-pill.status-external { background: #fce7f3; color: #9d174d; border-color: #f9a8d4; }
.status-pill.status-final { background: #dcfce7; color: #15803d; border-color: #86efac; }
.status-pill.status-completed { background: #d1fae5; color: #047857; border-color: #6ee7b7; }
.status-pill.status-returned { background: #fee2e2; color: #b91c1c; border-color: #fca5a5; }

.bid-subject {
  font-size: 14px;
  font-weight: 600;
  color: #1a2e25;
  margin: 0 0 10px;
  line-height: 1.4;
}

.progress-wrap {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 10px;
}
.progress-track {
  flex: 1;
  height: 6px;
  background: #e4ede8;
  border-radius: 3px;
  overflow: hidden;
}
.progress-fill {
  height: 100%;
  background: #006d4b;
  transition: width 0.3s ease;
}
.progress-fill.status-returned { background: #ef4444; }
.progress-fill.status-completed { background: #22c55e; }
.progress-label {
  font-size: 10px;
  color: #6b8a7d;
  font-weight: 600;
  letter-spacing: 0.3px;
  min-width: 32px;
  text-align: center;
}

.bid-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  font-size: 11px;
  color: #5a7a6d;
}
.meta-item {
  display: inline-flex;
  align-items: center;
  gap: 3px;
}
.meta-item.overdue { color: #dc2626; font-weight: 600; }
.overdue-badge {
  font-style: normal;
  padding: 1px 6px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
  font-size: 10px;
  font-weight: 700;
  margin-inline-start: 4px;
}

.empty-state {
  display: flex; flex-direction: column; align-items: center;
  padding: 40px 20px; color: #93afa4; gap: 10px;
}
.empty-state p { font-size: 13px; margin: 0; }

/* Form */
.bid-form { display: flex; flex-direction: column; gap: 12px; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.form-field { display: flex; flex-direction: column; gap: 4px; }
.form-field label { font-size: 12px; font-weight: 600; color: #374a41; }
.form-field .required { color: #dc2626; }

.form-input {
  padding: 10px 12px;
  border: 1.5px solid #d4e0da; border-radius: 8px;
  font-size: 14px; background: #f7faf8;
  color: #1a2e25; font-family: inherit;
}
.form-input:focus {
  outline: none; border-color: #006d4b;
  background: #fff;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

.form-actions {
  display: flex; justify-content: flex-end; gap: 8px;
  padding-top: 8px; border-top: 1px solid #e4ede8;
  margin-top: 6px;
}

.btn-primary, .btn-secondary {
  padding: 9px 18px; font-size: 13px; font-weight: 600;
  border-radius: 8px; cursor: pointer;
  transition: all 0.2s; font-family: inherit;
}
.btn-primary { background: #006d4b; color: #fff; border: 1px solid #006d4b; }
.btn-primary:hover:not(:disabled) { background: #005a3e; }
.btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
.btn-secondary { background: #fff; color: #374a41; border: 1px solid #d4e0da; }
.btn-secondary:hover { background: #f4f8f6; }
</style>

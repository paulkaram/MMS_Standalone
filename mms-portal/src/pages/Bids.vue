<template>
  <div class="bids-page">
    <PageHeader :title="$t('Bids')" :subtitle="$t('BidsDesc')">
      <template #actions>
        <button class="btn-primary" @click="openAddModal">
          <Icon icon="mdi:plus" class="w-4 h-4" />
          {{ $t('AddBid') }}
        </button>
      </template>
    </PageHeader>

    <!-- Stats row -->
    <div class="stats-row">
      <div class="stat-tile" @click="statusFilter = 0" :class="{ active: statusFilter === 0 }">
        <div class="stat-number">{{ bids.length }}</div>
        <div class="stat-label">{{ $t('AllBids') || $t('All') }}</div>
      </div>
      <div class="stat-tile active-tile" @click="statusFilter = 11; showCompleted = true" :class="{ active: statusFilter === 11 }">
        <div class="stat-number">{{ completedCount }}</div>
        <div class="stat-label">{{ $t('Completed') }}</div>
      </div>
      <div class="stat-tile overdue-tile" @click="overdueOnly = !overdueOnly" :class="{ active: overdueOnly }">
        <div class="stat-number">{{ overdueCount }}</div>
        <div class="stat-label">{{ $t('Overdue') }}</div>
      </div>
      <div class="stat-tile" @click="statusFilter = 2">
        <div class="stat-number">{{ pendingCount }}</div>
        <div class="stat-label">{{ $t('PendingManagerApproval') }}</div>
      </div>
    </div>

    <!-- Toolbar -->
    <div class="toolbar">
      <div class="search-box">
        <Icon icon="mdi:magnify" class="w-4 h-4" />
        <input v-model="search" type="text" :placeholder="$t('SearchBids')" />
      </div>
      <select v-model="committeeFilter" class="filter-select">
        <option :value="0">{{ $t('AllCommittees') }}</option>
        <option v-for="c in committeeOptions" :key="c.id" :value="c.id">{{ c.name }}</option>
      </select>
      <select v-model="statusFilter" class="filter-select">
        <option :value="0">{{ $t('AllStatuses') }}</option>
        <option v-for="s in statusOptions" :key="s.id" :value="s.id">{{ localizedStatus(s) }}</option>
      </select>
      <span class="result-count">{{ filteredBids.length }} / {{ bids.length }}</span>
    </div>

    <!-- Content -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
    </div>

    <div v-else-if="filteredBids.length > 0" class="bids-grid">
      <div
        v-for="bid in filteredBids"
        :key="bid.id"
        class="bid-card"
        :class="{ overdue: bid.isOverdue }"
        @click="openDetail(bid)"
      >
        <div class="bid-card-top">
          <span class="ref-badge">
            <Icon icon="mdi:pound" class="w-3 h-3" />
            {{ bid.referenceNumber }}
          </span>
          <span class="status-pill" :class="getStatusClass(bid.statusId)">
            {{ bid.statusName }}
          </span>
        </div>

        <h4 class="bid-subject">{{ bid.subject }}</h4>

        <div v-if="bid.committeeName" class="committee-name">
          <Icon icon="mdi:briefcase-outline" class="w-3 h-3" />
          {{ bid.committeeName }}
        </div>

        <!-- Progress bar -->
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

        <!-- Meta footer -->
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

    <div v-else class="empty-state">
      <Icon icon="mdi:briefcase-outline" class="w-12 h-12" />
      <h3>{{ $t('NoBids') }}</h3>
      <p>{{ $t('NoBidsHint') || $t('NoBids') }}</p>
      <button class="btn-primary" @click="openAddModal">
        <Icon icon="mdi:plus" class="w-4 h-4" />
        {{ $t('AddBid') }}
      </button>
    </div>

    <!-- Add Bid Modal (asks for committee first) -->
    <Modal v-model="modalOpen" :title="$t('AddBid')" size="lg">
      <form class="bid-form" @submit.prevent="saveBid">
        <div class="form-field">
          <label>{{ $t('Committee') }} <span class="required">*</span></label>
          <select v-model.number="form.committeeId" class="form-input" required>
            <option :value="0" disabled>{{ $t('SelectCommittee') || $t('Select') }}</option>
            <option v-for="c in committeeOptions" :key="c.id" :value="c.id">{{ c.name }}</option>
          </select>
        </div>

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
            <select v-model="form.teamLeaderUserId" class="form-input">
              <option :value="null">-</option>
              <option v-for="u in users" :key="u.id" :value="u.id">{{ u.name }}</option>
            </select>
          </div>
        </div>

        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('StartDate') }} <span class="required">*</span></label>
            <input v-model="form.startDate" type="date" class="form-input" required />
          </div>
          <div class="form-field">
            <label>{{ $t('DueDate') }} <span class="required">*</span></label>
            <input v-model="form.dueDate" type="date" class="form-input" required />
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
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '@/components/layout/PageHeader.vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import BidsService, { type Bid, type BidPost, type BidStatusDto, BidStatus } from '@/services/BidsService'
import UsersService from '@/services/UsersService'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'
import { useUserStore } from '@/stores/user'

const router = useRouter()
const { t } = useI18n()
const { toast } = useToast()
const userStore = useUserStore()

const isRtl = computed(() => userStore.isRtl)

const bids = ref<Bid[]>([])
const statusOptions = ref<BidStatusDto[]>([])
const committeeOptions = ref<{ id: number; name: string }[]>([])
const users = ref<{ id: string; name: string }[]>([])

const loading = ref(false)
const saving = ref(false)
const modalOpen = ref(false)

const search = ref('')
const committeeFilter = ref(0)
const statusFilter = ref(0)
const overdueOnly = ref(false)
const showCompleted = ref(true)

const emptyForm = (): BidPost => ({
  committeeId: 0,
  externalMeetingNumber: null,
  subject: '',
  description: null,
  teamLeaderUserId: null,
  startDate: new Date().toISOString().split('T')[0],
  dueDate: new Date().toISOString().split('T')[0],
  stakeholders: []
})

const form = reactive<BidPost>(emptyForm())

const completedCount = computed(() => bids.value.filter(b => b.statusId === BidStatus.Completed).length)
const overdueCount = computed(() => bids.value.filter(b => b.isOverdue).length)
const pendingCount = computed(() => bids.value.filter(b => b.statusId === BidStatus.PendingManagerApproval).length)

const filteredBids = computed(() => {
  let result = bids.value

  if (committeeFilter.value > 0) {
    result = result.filter(b => b.committeeId === committeeFilter.value)
  }

  if (statusFilter.value > 0) {
    result = result.filter(b => b.statusId === statusFilter.value)
  }

  if (overdueOnly.value) {
    result = result.filter(b => b.isOverdue)
  }

  if (search.value) {
    const q = search.value.toLowerCase()
    result = result.filter(b =>
      b.subject.toLowerCase().includes(q) ||
      b.referenceNumber.toLowerCase().includes(q) ||
      (b.externalMeetingNumber || '').toLowerCase().includes(q)
    )
  }

  return result
})

const localizedStatus = (s: BidStatusDto) => isRtl.value ? s.nameAr : s.nameEn

const load = async () => {
  loading.value = true
  try {
    const [bidsRes, statusRes]: any = await Promise.all([
      BidsService.listMine(),
      BidsService.listStatuses()
    ])
    bids.value = bidsRes?.data ?? bidsRes ?? []
    statusOptions.value = statusRes?.data ?? statusRes ?? []
  } catch (err) {
    console.error('Failed to load bids:', err)
  } finally {
    loading.value = false
  }
}

const loadCommittees = async () => {
  try {
    const res: any = await CouncilCommitteesService.listUserCommittees()
    const list = res?.data ?? res ?? []
    committeeOptions.value = list.map((c: any) => ({
      id: c.id,
      name: c.name || c.nameAr || c.nameEn || ''
    }))
  } catch {
    committeeOptions.value = []
  }
}

const loadUsers = async () => {
  try {
    const res: any = await UsersService.searchUsers('')
    users.value = (res?.data ?? res ?? []).map((u: any) => ({ id: String(u.id), name: u.name }))
  } catch {
    users.value = []
  }
}

const openAddModal = () => {
  Object.assign(form, emptyForm())
  modalOpen.value = true
}

const openDetail = (bid: Bid) => {
  router.push(`/bids/${bid.id}`)
}

const saveBid = async () => {
  if (!form.subject.trim() || !form.committeeId) return
  saving.value = true
  try {
    await BidsService.create(form)
    toast.success(t('BidCreatedSuccessfully'))
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

onMounted(() => {
  load()
  loadCommittees()
  loadUsers()
})
</script>

<style scoped>
.bids-page {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.btn-primary, .btn-secondary {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 9px 16px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  font-family: inherit;
  transition: all 0.2s;
}
.btn-primary {
  background: #006d4b; color: #fff;
  border: 1px solid #006d4b;
}
.btn-primary:hover:not(:disabled) { background: #005a3e; }
.btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }

.btn-secondary {
  background: #fff; color: #374a41;
  border: 1px solid #d4e0da;
}
.btn-secondary:hover { background: #f4f8f6; }

/* Stats row */
.stats-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
}
.stat-tile {
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 10px;
  padding: 16px;
  cursor: pointer;
  transition: all 0.15s;
  border-top: 3px solid #006d4b;
}
.stat-tile:hover { box-shadow: 0 2px 10px rgba(0, 109, 75, 0.06); }
.stat-tile.active {
  background: #eef5f1;
  border-color: #006d4b;
}
.stat-tile.active-tile { border-top-color: #22c55e; }
.stat-tile.active-tile.active { background: #f0fdf4; }
.stat-tile.overdue-tile { border-top-color: #ef4444; }
.stat-tile.overdue-tile.active { background: #fef2f2; border-color: #ef4444; }

.stat-number {
  font-size: 26px;
  font-weight: 800;
  color: #1a2e25;
  line-height: 1;
}
.stat-label {
  font-size: 12px;
  color: #5a7a6d;
  margin-top: 6px;
  font-weight: 600;
}

/* Toolbar */
.toolbar {
  display: flex;
  align-items: center;
  gap: 10px;
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 10px;
  padding: 10px 14px;
}
.search-box {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  background: #f7faf8;
  border: 1px solid #d4e0da;
  border-radius: 8px;
  flex: 1;
  color: #93afa4;
}
.search-box input {
  border: none;
  background: transparent;
  outline: none;
  font-size: 13px;
  font-family: inherit;
  color: #1a2e25;
  flex: 1;
}
.filter-select {
  padding: 6px 10px;
  border: 1px solid #d4e0da;
  border-radius: 8px;
  background: #f7faf8;
  font-size: 12px;
  color: #1a2e25;
  font-family: inherit;
  cursor: pointer;
}
.result-count {
  font-size: 12px;
  color: #6b8a7d;
  font-weight: 600;
  white-space: nowrap;
}

/* Loading / empty */
.loading-state {
  display: flex; justify-content: center; padding: 60px 0;
}
.spinner {
  width: 32px; height: 32px;
  border: 3px solid #e4ede8; border-top-color: #006d4b;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

.empty-state {
  display: flex; flex-direction: column;
  align-items: center; justify-content: center;
  padding: 64px 24px;
  background: #fff;
  border: 1px dashed #d4e0da;
  border-radius: 12px;
  gap: 12px;
  color: #93afa4;
}
.empty-state h3 {
  margin: 0; font-size: 16px; font-weight: 700;
  color: #1a2e25;
}
.empty-state p {
  margin: 0; font-size: 13px;
}

/* Grid */
.bids-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 14px;
}

.bid-card {
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 12px;
  padding: 16px;
  cursor: pointer;
  transition: all 0.2s;
}
.bid-card:hover {
  box-shadow: 0 4px 16px rgba(0, 109, 75, 0.08);
  transform: translateY(-2px);
  border-color: #c8ddd3;
}
.bid-card.overdue {
  border-left: 4px solid #ef4444;
}
[dir="rtl"] .bid-card.overdue {
  border-left: 1px solid #e4ede8;
  border-right: 4px solid #ef4444;
}

.bid-card-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
  margin-bottom: 10px;
}
.ref-badge {
  display: inline-flex; align-items: center; gap: 3px;
  padding: 2px 8px;
  font-size: 11px; font-weight: 600;
  border-radius: 10px;
  background: #e8f5ef; color: #006d4b;
  border: 1px solid #c8ddd3;
}
.status-pill {
  padding: 3px 10px;
  font-size: 10px; font-weight: 600;
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
  margin: 0 0 8px;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.committee-name {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 11px;
  color: #5a7a6d;
  padding: 2px 8px;
  background: #f4f8f6;
  border-radius: 10px;
  margin-bottom: 10px;
}

.progress-wrap {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 10px;
}
.progress-track {
  flex: 1;
  height: 5px;
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
  font-weight: 700;
  min-width: 32px;
  text-align: center;
}

.bid-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  font-size: 11px;
  color: #5a7a6d;
}
.meta-item { display: inline-flex; align-items: center; gap: 3px; }
.meta-item.overdue { color: #dc2626; font-weight: 600; }
.overdue-badge {
  font-style: normal;
  padding: 1px 5px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
  font-size: 9px;
  font-weight: 700;
  margin-inline-start: 4px;
}

/* Form */
.bid-form { display: flex; flex-direction: column; gap: 12px; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.form-field { display: flex; flex-direction: column; gap: 4px; }
.form-field label { font-size: 12px; font-weight: 600; color: #374a41; }
.form-field .required { color: #dc2626; }
.form-input {
  padding: 10px 12px;
  border: 1.5px solid #d4e0da;
  border-radius: 8px;
  font-size: 14px;
  background: #f7faf8;
  color: #1a2e25;
  font-family: inherit;
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

@media (max-width: 768px) {
  .stats-row { grid-template-columns: repeat(2, 1fr); }
  .toolbar { flex-wrap: wrap; }
  .form-row { grid-template-columns: 1fr; }
}
</style>

<template>
  <div class="space-y-5">
    <!-- Page Header -->
    <PageHeader :title="$t('Meetings')" :subtitle="$t('MeetingsDescription')">
      <template #actions>
        <Button variant="primary" icon-left="mdi:plus" @click="$router.push('/addMeeting')">
          {{ $t('NewMeeting') }}
        </Button>
      </template>
    </PageHeader>

    <!-- Stats Cards -->
    <div class="grid grid-cols-2 lg:grid-cols-5 gap-4">
      <div class="cursor-pointer" @click="filterByStatus(null)">
        <StatCard :title="$t('AllMeetings')" :value="totalCount" icon="mdi:calendar-month" color="primary" />
      </div>
      <div class="cursor-pointer" @click="filterByStatus(MeetingStatus.Approved)">
        <StatCard :title="$t('Scheduled')" :value="scheduledCount" icon="mdi:calendar-check" color="info" />
      </div>
      <div class="cursor-pointer" @click="filterByInProgress">
        <StatCard :title="$t('InProgress')" :value="inProgressCount" icon="mdi:progress-clock" color="warning" />
      </div>
      <div class="cursor-pointer" @click="filterByStatus(MeetingStatus.FinalMeetingMinutesSigned)">
        <StatCard :title="$t('Completed')" :value="completedCount" icon="mdi:check-circle" color="success" />
      </div>
      <div class="cursor-pointer" @click="filterByStatus(MeetingStatus.Canceled)">
        <StatCard :title="$t('Canceled')" :value="canceledCount" icon="mdi:cancel" color="error" />
      </div>
    </div>

    <!-- Search Filters -->
    <div class="filter-card">
      <button type="button" class="filter-toggle" @click="showFilters = !showFilters">
        <div class="flex items-center gap-2">
          <Icon icon="mdi:filter-variant" class="w-5 h-5" style="color: #006d4b" />
          <span class="font-medium">{{ $t('SearchAndFilter') }}</span>
        </div>
        <Icon icon="mdi:chevron-down" class="w-5 h-5 text-zinc-400 transition-transform" :class="{ 'rotate-180': showFilters }" />
      </button>
      <Transition name="slide">
        <div v-if="showFilters" class="filter-body">
          <div class="filter-row">
            <div class="filter-field">
              <label>{{ $t('MeetingNumber') }}</label>
              <input v-model="searchParams.meetingId" type="number" :placeholder="$t('EnterNumber')" />
            </div>
            <div class="filter-field">
              <label>{{ $t('Subject') }}</label>
              <input v-model="searchParams.title" type="text" :placeholder="$t('SearchByTitle')" />
            </div>
            <div class="filter-field">
              <CustomSelect v-model="searchParams.statusId" :options="statusItems" :label="$t('Status')" :placeholder="$t('All')" value-key="id" label-key="name" clearable />
            </div>
            <div class="filter-field">
              <CustomSelect v-model="searchParams.committeeId" :options="committeeItems" :label="$t('Committee')" :placeholder="$t('All')" value-key="id" label-key="name" clearable searchable />
            </div>
          </div>
          <div class="filter-row">
            <div class="filter-field">
              <label>{{ $t('Location') }}</label>
              <input v-model="searchParams.location" type="text" :placeholder="$t('MeetingRoom')" />
            </div>
            <div class="filter-field">
              <DatePicker v-model="searchParams.fromDate" :label="$t('FromDate')" />
            </div>
            <div class="filter-field">
              <DatePicker v-model="searchParams.toDate" :label="$t('ToDate')" />
            </div>
          </div>
          <div class="filter-footer">
            <label class="filter-switch">
              <input v-model="searchParams.onlyMyMeetings" type="checkbox" class="sr-only peer" />
              <span class="filter-switch-track"></span>
              <span>{{ $t('OnlyMyMeetings') }}</span>
            </label>
            <div class="filter-btns">
              <Button variant="primary" icon-left="mdi:magnify" @click="doSearch">{{ $t('Search') }}</Button>
              <Button variant="outline" icon-left="mdi:refresh" @click="resetFilters">{{ $t('Reset') }}</Button>
            </div>
          </div>
        </div>
      </Transition>
    </div>

    <!-- Meetings Grid -->
    <MeetingGrid
      ref="gridRef"
      :search-params="activeSearchParams"
      :columns="['status', 'reference', 'location', 'creator']"
      :clickable="true"
      @view="viewMeeting"
      @edit="editMeeting"
    >
      <template #actions="{ meeting }">
        <button class="mg-act" :title="$t('View')" @click.stop="viewMeeting(meeting)">
          <Icon icon="mdi:eye-outline" class="w-4 h-4" />
        </button>
        <button class="mg-act" :title="$t('Attendees')" @click.stop="showAttendees(meeting)">
          <Icon icon="mdi:account-group-outline" class="w-4 h-4" />
        </button>
        <button class="mg-act" :title="$t('Agenda')" @click.stop="showAgenda(meeting)">
          <Icon icon="mdi:format-list-checks" class="w-4 h-4" />
        </button>
        <button v-if="canEdit(meeting.statusId)" class="mg-act" :title="$t('Edit')" @click.stop="editMeeting(meeting)">
          <Icon icon="mdi:pencil-outline" class="w-4 h-4" />
        </button>
        <button v-if="canCancel(meeting.statusId)" class="mg-act danger" :title="$t('Cancel')" @click.stop="cancelMeeting(meeting)">
          <Icon icon="mdi:close-circle-outline" class="w-4 h-4" />
        </button>
      </template>
    </MeetingGrid>

    <!-- Attendees Modal -->
    <Modal v-model="attendeesDialog" :title="$t('Attendees')" icon="mdi:account-group" size="md">
      <div v-if="loadingAttendees" class="modal-loading"><Icon icon="mdi:loading" class="w-8 h-8 animate-spin text-primary" /></div>
      <div v-else-if="attendees.length === 0" class="modal-empty">
        <Icon icon="mdi:account-group-outline" class="w-12 h-12 text-zinc-300" />
        <p>{{ $t('NoAttendees') }}</p>
      </div>
      <div v-else class="modal-list">
        <div v-for="(a, i) in attendees" :key="a.userId || i" class="modal-list-item">
          <div class="modal-avatar">
            <img v-if="a.userId" :src="profileUrl(a.userId)" class="w-full h-full object-cover rounded-full" @error="(e: any) => e.target.style.display='none'" />
            <span v-else class="text-white font-bold text-xs">{{ initials(a.fullName || a.userFullName || a.name) }}</span>
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-medium text-zinc-800 truncate">{{ a.fullName || a.userFullName || a.name }}</div>
            <div class="text-xs text-zinc-400">{{ a.jobTitle || a.roleName || '-' }}</div>
          </div>
          <span :class="['modal-badge', a.attended ? 'success' : 'muted']">
            <Icon :icon="a.attended ? 'mdi:check-circle' : 'mdi:close-circle'" class="w-4 h-4" />
            {{ a.attended ? $t('Attended') : $t('Absent') }}
          </span>
        </div>
      </div>
    </Modal>

    <!-- Agenda Modal -->
    <Modal v-model="agendaDialog" :title="$t('Agenda')" icon="mdi:format-list-checks" size="lg">
      <div v-if="loadingAgenda" class="modal-loading"><Icon icon="mdi:loading" class="w-8 h-8 animate-spin text-primary" /></div>
      <div v-else-if="agendaItems.length === 0" class="modal-empty">
        <Icon icon="mdi:format-list-bulleted" class="w-12 h-12 text-zinc-300" />
        <p>{{ $t('NoAgendaItems') }}</p>
      </div>
      <div v-else class="modal-list">
        <div v-for="(item, i) in agendaItems" :key="item.id" class="modal-list-item">
          <span class="agenda-num">{{ i + 1 }}</span>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-medium text-zinc-800">{{ item.title || item.titleAr }}</div>
            <div v-if="item.description" class="text-xs text-zinc-400 mt-0.5">{{ item.description }}</div>
          </div>
          <span v-if="item.duration" class="text-xs text-zinc-400 whitespace-nowrap">{{ item.duration }} {{ $t('Minutes') }}</span>
        </div>
      </div>
    </Modal>

    <!-- Approvals Modal -->
    <Modal v-model="approvalsDialog" :title="$t('Approvals')" icon="mdi:check-decagram" size="md">
      <div v-if="loadingApprovals" class="modal-loading"><Icon icon="mdi:loading" class="w-8 h-8 animate-spin text-primary" /></div>
      <div v-else-if="approvalsList.length === 0" class="modal-empty">
        <Icon icon="mdi:check-decagram-outline" class="w-12 h-12 text-zinc-300" />
        <p>{{ $t('NoApprovalsRequired') }}</p>
      </div>
      <div v-else class="modal-list">
        <div v-for="(a, i) in approvalsList" :key="a.userId || i" class="modal-list-item">
          <div class="modal-avatar">
            <img v-if="a.userId" :src="profileUrl(a.userId)" class="w-full h-full object-cover rounded-full" @error="(e: any) => e.target.style.display='none'" />
            <span v-else class="text-white font-bold text-xs">{{ initials(a.fullName || a.userFullName || a.name) }}</span>
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-medium text-zinc-800 truncate">{{ a.fullName || a.userFullName || a.name }}</div>
            <div class="text-xs text-zinc-400">{{ a.jobTitle || a.roleName || '-' }}</div>
          </div>
          <span :class="['modal-badge', a.approved ? 'success' : 'muted']">
            <Icon :icon="a.approved ? 'mdi:check-circle' : 'mdi:clock-outline'" class="w-4 h-4" />
            {{ a.approved ? $t('Approved') : $t('Pending') }}
          </span>
        </div>
      </div>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import StatCard from '@/components/app/dashboard/StatCard.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import Modal from '@/components/ui/Modal.vue'
import CustomSelect from '@/components/ui/CustomSelect.vue'
import DatePicker from '@/components/ui/DatePicker.vue'
import MeetingGrid from '@/components/app/meeting/MeetingGrid.vue'
import MeetingsService from '@/services/MeetingsService'
import LookupsService from '@/services/LookupsService'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'
import { useConfirm } from '@/composables/useConfirm'
import { MeetingStatusEnum } from '@/helpers/enumerations'
import type { Attendee, AgendaItem } from '@/services/MeetingsService'

const MeetingStatus = MeetingStatusEnum
const router = useRouter()
const { confirm } = useConfirm()
const apiBaseUrl = import.meta.env.VITE_API_URL || ''
const gridRef = ref<InstanceType<typeof MeetingGrid>>()

// Filters
const showFilters = ref(false)
const searchParams = reactive({
  statusId: null as number | null,
  committeeId: null as number | null,
  meetingId: null as number | null,
  fromDate: null as string | null,
  toDate: null as string | null,
  location: null as string | null,
  title: null as string | null,
  onlyMyMeetings: true
})
const activeSearchParams = ref({ ...searchParams })

// Lookups
const statusItems = ref<any[]>([])
const committeeItems = ref<any[]>([])

// Stats
const totalCount = ref(0)
const scheduledCount = ref(0)
const inProgressCount = ref(0)
const completedCount = ref(0)
const canceledCount = ref(0)

// Dialogs
const attendeesDialog = ref(false)
const agendaDialog = ref(false)
const approvalsDialog = ref(false)
const attendees = ref<Attendee[]>([])
const agendaItems = ref<AgendaItem[]>([])
const approvalsList = ref<any[]>([])
const loadingAttendees = ref(false)
const loadingAgenda = ref(false)
const loadingApprovals = ref(false)

// Search
function doSearch() {
  activeSearchParams.value = { ...searchParams }
}

function resetFilters() {
  Object.assign(searchParams, {
    statusId: null, committeeId: null, meetingId: null,
    fromDate: null, toDate: null, location: null, title: null, onlyMyMeetings: true
  })
  doSearch()
}

function filterByStatus(statusId: number | null) {
  searchParams.statusId = statusId
  doSearch()
}

function filterByInProgress() {
  searchParams.statusId = MeetingStatusEnum.Started
  doSearch()
}

// Actions
function viewMeeting(m: any) { router.push(`/meetingRoom/${m.id}`) }
function editMeeting(m: any) { router.push(`/addMeeting/${m.id}`) }
const canEdit = (s: number) => s <= MeetingStatusEnum.Started
const canCancel = (s: number) => s < MeetingStatusEnum.Started

async function cancelMeeting(m: any) {
  const ok = await confirm({ title: $t('Cancel'), message: $t('ConfirmCancelMeeting'), type: 'danger' })
  if (!ok) return
  try { await MeetingsService.cancelMeeting(m.id); gridRef.value?.reload() } catch {}
}

async function showAttendees(m: any) {
  attendeesDialog.value = true; loadingAttendees.value = true
  try { const r = await MeetingsService.getAttendees(m.id); attendees.value = Array.isArray(r) ? r : (r?.data || []) }
  catch { attendees.value = [] } finally { loadingAttendees.value = false }
}

async function showAgenda(m: any) {
  agendaDialog.value = true; loadingAgenda.value = true
  try { const r = await MeetingsService.getAgendaItems(m.id); agendaItems.value = Array.isArray(r) ? r : (r?.data || []) }
  catch { agendaItems.value = [] } finally { loadingAgenda.value = false }
}

// Helpers
const profileUrl = (id: any) => `${apiBaseUrl}/api/users/profile-image/${id}`
const initials = (n: string) => { if (!n) return '?'; const p = n.split(' '); return p.length > 1 ? p[0][0] + p[1][0] : n.substring(0, 2) }

// Load lookups & stats
async function loadLookups() {
  try {
    const [s, c] = await Promise.all([LookupsService.getMeetingStatuses(), CouncilCommitteesService.listUserCommittees()])
    statusItems.value = Array.isArray(s) ? s : (s?.data || [])
    committeeItems.value = Array.isArray(c) ? c : (c?.data || [])
  } catch {}
}

async function loadStats() {
  try {
    const bp = { onlyMyMeetings: searchParams.onlyMyMeetings }
    const [all, sched, started, fin, pim, ima, pfs, comp, canc] = await Promise.all([
      MeetingsService.searchUserMeetings({ ...bp }, 1, 1),
      MeetingsService.searchUserMeetings({ ...bp, statusId: MeetingStatusEnum.Approved }, 1, 1),
      MeetingsService.searchUserMeetings({ ...bp, statusId: MeetingStatusEnum.Started }, 1, 1),
      MeetingsService.searchUserMeetings({ ...bp, statusId: MeetingStatusEnum.Finished }, 1, 1),
      MeetingsService.searchUserMeetings({ ...bp, statusId: MeetingStatusEnum.PendingInitialMeetingMinutesApproval }, 1, 1),
      MeetingsService.searchUserMeetings({ ...bp, statusId: MeetingStatusEnum.InitialMeetingMinutesApproved }, 1, 1),
      MeetingsService.searchUserMeetings({ ...bp, statusId: MeetingStatusEnum.PendingFinalMeetingMinutesSign }, 1, 1),
      MeetingsService.searchUserMeetings({ ...bp, statusId: MeetingStatusEnum.FinalMeetingMinutesSigned }, 1, 1),
      MeetingsService.searchUserMeetings({ ...bp, statusId: MeetingStatusEnum.Canceled }, 1, 1)
    ])
    const gt = (r: any) => r?.data?.total || r?.total || 0
    totalCount.value = gt(all)
    scheduledCount.value = gt(sched)
    inProgressCount.value = gt(started) + gt(fin) + gt(pim) + gt(ima) + gt(pfs)
    completedCount.value = gt(comp)
    canceledCount.value = gt(canc)
  } catch {}
}

onMounted(() => { loadLookups(); loadStats() })
</script>

<style scoped>
/* Filter card */
.filter-card {
  background: #fff; border: 1px solid #e6eaef; border-radius: 10px;
  overflow: hidden; box-shadow: 0 2px 6px rgba(0,0,0,0.04);
}
.filter-toggle {
  width: 100%; display: flex; align-items: center; justify-content: space-between;
  padding: 14px 16px; background: none; border: none; cursor: pointer;
  font-size: 14px; color: #004730; transition: background 0.15s;
}
.filter-toggle:hover { background: #f8fafc; }
.filter-body { padding: 16px; border-top: 1px solid #eaeaea; display: flex; flex-direction: column; gap: 14px; }
.filter-row { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; }
.filter-field { display: flex; flex-direction: column; gap: 5px; }
.filter-field label { font-size: 12px; font-weight: 500; color: #64748b; }
.filter-field input {
  border: 1px solid #e2e8f0; border-radius: 8px; padding: 9px 12px;
  font-size: 14px; outline: none; transition: border-color 0.15s; width: 100%;
}
.filter-field input:focus { border-color: #006d4b; box-shadow: 0 0 0 3px rgba(0,174,140,0.1); }
.filter-footer {
  display: flex; align-items: center; justify-content: space-between;
  padding-top: 14px; border-top: 1px solid #f1f5f9;
}
.filter-switch {
  display: flex; align-items: center; gap: 8px; cursor: pointer; font-size: 13px; color: #334155;
}
.filter-switch-track {
  position: relative; width: 38px; height: 20px; background: #d1d5db; border-radius: 10px;
  flex-shrink: 0; transition: background 0.2s;
}
.filter-switch-track::after {
  content: ''; position: absolute; top: 2px; inset-inline-start: 2px; width: 16px; height: 16px;
  background: #fff; border-radius: 50%; box-shadow: 0 1px 2px rgba(0,0,0,0.15); transition: transform 0.2s;
}
.filter-switch:has(input:checked) .filter-switch-track { background: #006d4b; }
.filter-switch:has(input:checked) .filter-switch-track::after { transform: translateX(18px); }
[dir="rtl"] .filter-switch:has(input:checked) .filter-switch-track::after { transform: translateX(-18px); }
.filter-btns { display: flex; gap: 8px; }

@media (max-width: 900px) {
  .filter-row { grid-template-columns: repeat(2, 1fr); }
}
@media (max-width: 600px) {
  .filter-row { grid-template-columns: 1fr; }
}

/* Action buttons (matching MeetingGrid) */
.mg-act {
  width: 32px; height: 32px; display: flex; align-items: center; justify-content: center;
  border-radius: 8px; border: none; background: none;
  color: #008080; cursor: pointer; transition: color 0.2s, transform 0.2s;
}
.mg-act:hover { color: #004c4c; transform: scale(1.15); }
.mg-act.danger { color: #e53e3e; }
.mg-act.danger:hover { color: #c53030; }

/* Modals */
.modal-loading { display: flex; justify-content: center; padding: 40px; }
.modal-empty { display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 40px; color: #94a3b8; }
.modal-list { display: flex; flex-direction: column; }
.modal-list-item { display: flex; align-items: center; gap: 12px; padding: 10px 0; border-bottom: 1px solid #f1f5f9; }
.modal-list-item:last-child { border-bottom: none; }
.modal-avatar {
  width: 36px; height: 36px; border-radius: 50%; background: #006d4b;
  display: flex; align-items: center; justify-content: center; flex-shrink: 0; overflow: hidden;
}
.modal-badge {
  display: inline-flex; align-items: center; gap: 4px;
  padding: 4px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: 600; white-space: nowrap;
}
.modal-badge.success { background: #e6f9f0; color: #0d7a4a; }
.modal-badge.muted { background: #f0f0f0; color: #666; }
.agenda-num {
  width: 28px; height: 28px; border-radius: 50%; background: rgba(0, 109, 75, 0.1); color: #006d4b;
  display: flex; align-items: center; justify-content: center;
  font-size: 0.75rem; font-weight: 700; flex-shrink: 0;
}

/* Transitions */
.slide-enter-active, .slide-leave-active { transition: all 0.25s ease; overflow: hidden; }
.slide-enter-from, .slide-leave-to { opacity: 0; max-height: 0; padding-top: 0; padding-bottom: 0; }
.slide-enter-to, .slide-leave-from { opacity: 1; max-height: 500px; }
</style>

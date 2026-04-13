<template>
  <div class="space-y-5">
    <PageHeader
      :title="$t('Delegations')"
      :subtitle="$t('DelegationsDesc')"
    >
      <template #actions>
        <button class="btn-clean primary" @click="openAdd">
          <Icon icon="mdi:plus" class="w-4 h-4" />
          {{ $t('AddDelegation') }}
        </button>
      </template>
    </PageHeader>

    <!-- Tabs -->
    <div class="tabs-bar">
      <button :class="['tab-btn', { active: activeTab === 'outgoing' }]" @click="activeTab = 'outgoing'">
        <Icon icon="mdi:arrow-up-bold-circle-outline" class="w-4 h-4" />
        {{ $t('OutgoingDelegations') }}
        <span class="tab-count">{{ outgoing.length }}</span>
      </button>
      <button :class="['tab-btn', { active: activeTab === 'incoming' }]" @click="activeTab = 'incoming'">
        <Icon icon="mdi:arrow-down-bold-circle-outline" class="w-4 h-4" />
        {{ $t('IncomingDelegations') }}
        <span class="tab-count">{{ incoming.length }}</span>
      </button>
    </div>

    <div class="mg-container">
      <div v-if="loading" class="mg-state">
        <Icon icon="mdi:loading" class="w-6 h-6 animate-spin" style="color: #006d4b" />
      </div>

      <template v-else>
        <div v-if="currentList.length > 0" class="delegation-list">
          <div
            v-for="d in currentList"
            :key="d.id"
            class="delegation-card"
            :class="getStatusClass(d)"
          >
            <div class="card-header">
              <div class="card-title">
                <UserAvatar
                  :userId="activeTab === 'outgoing' ? d.toUserId : d.fromUserId"
                  :name="activeTab === 'outgoing' ? d.toUserName : d.fromUserName"
                  size="sm"
                />
                <div class="card-title-text">
                  <h4>
                    <template v-if="activeTab === 'outgoing'">
                      {{ $t('DelegateTo') }}: {{ d.toUserName }}
                    </template>
                    <template v-else>
                      {{ d.fromUserName }}
                    </template>
                  </h4>
                  <span class="card-type-badge" :class="d.typeId === 1 ? 'type-general' : 'type-task'">
                    {{ d.typeId === 1 ? $t('General') : $t('TaskSpecific') }}
                  </span>
                </div>
              </div>
              <span class="status-pill" :class="getStatusClass(d)">
                {{ getStatusLabel(d) }}
              </span>
            </div>

            <div class="card-body">
              <div class="card-row">
                <Icon icon="mdi:calendar-range" class="w-4 h-4" />
                <span>{{ formatDate(d.startDate) }} &mdash; {{ formatDate(d.endDate) }}</span>
              </div>
              <div v-if="d.reason" class="card-row">
                <Icon icon="mdi:comment-text-outline" class="w-4 h-4" />
                <span>{{ d.reason }}</span>
              </div>
              <div v-if="d.typeId === 2" class="card-row">
                <Icon icon="mdi:clipboard-list-outline" class="w-4 h-4" />
                <span>{{ d.taskIds.length }} {{ $t('TasksCount') }}</span>
              </div>
            </div>

            <div v-if="activeTab === 'outgoing' && d.isActive && d.isCurrentlyActive" class="card-actions">
              <button class="btn-revoke" @click="confirmRevoke(d)">
                <Icon icon="mdi:cancel" class="w-4 h-4" />
                {{ $t('Revoke') }}
              </button>
            </div>
          </div>
        </div>

        <div v-else class="empty-state">
          <Icon icon="mdi:handshake-outline" class="w-10 h-10" />
          <p>{{ $t('NoDelegations') }}</p>
        </div>
      </template>
    </div>

    <!-- Add Modal -->
    <Modal v-model="modalOpen" :title="$t('AddDelegation')" size="lg">
      <form class="delegation-form" @submit.prevent="save">
        <div class="form-field">
          <label>{{ $t('DelegateTo') }} <span class="required">*</span></label>
          <select v-model="form.toUserId" class="form-input" required>
            <option value="" disabled>{{ $t('SelectUser') }}</option>
            <option v-for="u in usersList" :key="u.id" :value="u.id">{{ u.name }}</option>
          </select>
        </div>

        <div class="form-field">
          <label>{{ $t('DelegationType') }} <span class="required">*</span></label>
          <div class="radio-group">
            <label class="radio-label">
              <input v-model.number="form.typeId" type="radio" :value="1" />
              <span>{{ $t('General') }}</span>
            </label>
            <label class="radio-label">
              <input v-model.number="form.typeId" type="radio" :value="2" />
              <span>{{ $t('TaskSpecific') }}</span>
            </label>
          </div>
        </div>

        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('StartDate') }} <span class="required">*</span></label>
            <input v-model="form.startDate" type="date" class="form-input" required />
          </div>
          <div class="form-field">
            <label>{{ $t('EndDate') }} <span class="required">*</span></label>
            <input v-model="form.endDate" type="date" class="form-input" required />
          </div>
        </div>

        <div v-if="form.typeId === 2" class="form-field">
          <label>{{ $t('SelectTasks') }} <span class="required">*</span></label>
          <div v-if="loadingTasks" class="loading-inline">
            <Icon icon="mdi:loading" class="w-4 h-4 animate-spin" />
          </div>
          <div v-else-if="myTasks.length > 0" class="task-picker">
            <label v-for="task in myTasks" :key="task.id" class="task-check">
              <input
                type="checkbox"
                :value="task.id"
                v-model="form.taskIds"
              />
              <span>#{{ task.id }} &mdash; {{ task.typeName || task.name || $t('Task') }}</span>
            </label>
          </div>
          <div v-else class="no-tasks">{{ $t('NoTasks') }}</div>
        </div>

        <div class="form-field">
          <label>{{ $t('Reason') }}</label>
          <textarea v-model="form.reason" rows="3" class="form-input"></textarea>
        </div>

        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="modalOpen = false">{{ $t('Cancel') }}</button>
          <button type="submit" class="btn-primary" :disabled="saving">{{ $t('Save') }}</button>
        </div>
      </form>
    </Modal>

    <ConfirmModal
      v-model="confirmRevokeOpen"
      :title="$t('Revoke')"
      :message="$t('ConfirmRevoke')"
      :confirm-text="$t('Revoke')"
      :cancel-text="$t('Cancel')"
      variant="danger"
      @confirm="performRevoke"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import ConfirmModal from '@/components/ui/ConfirmModal.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import DelegationsService, { type Delegation, DelegationType } from '@/services/DelegationsService'
import UsersService from '@/services/UsersService'
import { mainApiAxios as axios } from '@/plugins/axios'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const { toast } = useToast()

const activeTab = ref<'outgoing' | 'incoming'>('outgoing')
const outgoing = ref<Delegation[]>([])
const incoming = ref<Delegation[]>([])
const usersList = ref<{ id: string; name: string }[]>([])
const myTasks = ref<any[]>([])
const loading = ref(false)
const loadingTasks = ref(false)
const saving = ref(false)
const modalOpen = ref(false)
const confirmRevokeOpen = ref(false)
const pendingRevokeId = ref<number | null>(null)

const emptyForm = () => ({
  toUserId: '',
  typeId: DelegationType.General,
  startDate: new Date().toISOString().split('T')[0],
  endDate: new Date().toISOString().split('T')[0],
  reason: '',
  taskIds: [] as number[]
})

const form = reactive(emptyForm())

const currentList = computed(() =>
  activeTab.value === 'outgoing' ? outgoing.value : incoming.value
)

const load = async () => {
  loading.value = true
  try {
    const [out, inc]: any = await Promise.all([
      DelegationsService.listOutgoing(),
      DelegationsService.listIncoming()
    ])
    outgoing.value = out?.data ?? out ?? []
    incoming.value = inc?.data ?? inc ?? []
  } catch (err) {
    console.error('Failed to load delegations:', err)
  } finally {
    loading.value = false
  }
}

const loadUsers = async () => {
  try {
    const res: any = await UsersService.searchUsers('')
    usersList.value = (res?.data ?? res ?? []).map((u: any) => ({
      id: String(u.id),
      name: u.name
    }))
  } catch (err) {
    console.error('Failed to load users:', err)
  }
}

const loadMyTasks = async () => {
  loadingTasks.value = true
  try {
    const res: any = await axios.get('tasks/my')
    myTasks.value = res?.data ?? res ?? []
  } catch {
    myTasks.value = []
  } finally {
    loadingTasks.value = false
  }
}

const openAdd = () => {
  Object.assign(form, emptyForm())
  modalOpen.value = true
  if (form.typeId === DelegationType.TaskSpecific && myTasks.value.length === 0) {
    loadMyTasks()
  }
}

const save = async () => {
  if (!form.toUserId) return
  if (form.typeId === DelegationType.TaskSpecific && form.taskIds.length === 0) {
    toast.error(t('SelectTasks'))
    return
  }
  saving.value = true
  try {
    await DelegationsService.create({
      toUserId: form.toUserId,
      typeId: form.typeId,
      startDate: form.startDate,
      endDate: form.endDate,
      reason: form.reason || null,
      taskIds: form.taskIds
    })
    toast.success(t('DelegationCreatedSuccessfully'))
    modalOpen.value = false
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('OverlappingDelegation'))
  } finally {
    saving.value = false
  }
}

const confirmRevoke = (d: Delegation) => {
  pendingRevokeId.value = d.id
  confirmRevokeOpen.value = true
}

const performRevoke = async () => {
  if (!pendingRevokeId.value) return
  try {
    await DelegationsService.revoke(pendingRevokeId.value)
    toast.success(t('DelegationRevokedSuccessfully'))
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    pendingRevokeId.value = null
    confirmRevokeOpen.value = false
  }
}

const getStatusClass = (d: Delegation): string => {
  if (!d.isActive) return 'status-revoked'
  if (d.isCurrentlyActive) return 'status-active'
  const now = new Date()
  if (new Date(d.endDate) < now) return 'status-expired'
  return 'status-upcoming'
}

const getStatusLabel = (d: Delegation): string => {
  if (!d.isActive) return t('Revoked')
  if (d.isCurrentlyActive) return t('CurrentlyActive')
  const now = new Date()
  if (new Date(d.endDate) < now) return t('Expired')
  return t('Upcoming')
}

const formatDate = (iso: string): string => {
  if (!iso) return ''
  return new Date(iso).toLocaleDateString()
}

onMounted(() => {
  load()
  loadUsers()
})
</script>

<style scoped>
.space-y-5 > * + * { margin-top: 20px; }

.btn-clean {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 8px 14px; border-radius: 8px; border: none;
  font-size: 13px; font-weight: 600; cursor: pointer;
  font-family: inherit; transition: all 0.2s;
}
.btn-clean.primary { background: #006d4b; color: #fff; }
.btn-clean.primary:hover { background: #005a3e; }

.tabs-bar {
  display: flex;
  gap: 4px;
  background: #f1f5f9;
  padding: 4px;
  border-radius: 10px;
  width: fit-content;
}
.tab-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 14px;
  border: none;
  background: transparent;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  color: #6b8a7d;
  cursor: pointer;
  transition: all 0.2s;
  font-family: inherit;
}
.tab-btn:hover { color: #006d4b; }
.tab-btn.active {
  background: #fff;
  color: #006d4b;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
}
.tab-count {
  background: #e8f5ef;
  color: #006d4b;
  padding: 1px 7px;
  border-radius: 10px;
  font-size: 11px;
  font-weight: 700;
}

.mg-container {
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 12px;
  padding: 14px;
  min-height: 200px;
}

.mg-state { display: flex; justify-content: center; padding: 40px 0; }

.delegation-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.delegation-card {
  background: #fff;
  border: 1px solid #e4ede8;
  border-left: 4px solid #d4e0da;
  border-radius: 10px;
  padding: 14px;
  transition: all 0.2s;
}
[dir="rtl"] .delegation-card {
  border-left: none;
  border-right: 4px solid #d4e0da;
}
.delegation-card:hover { box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04); }

.delegation-card.status-active {
  border-left-color: #22c55e;
}
[dir="rtl"] .delegation-card.status-active { border-right-color: #22c55e; }

.delegation-card.status-upcoming {
  border-left-color: #3b82f6;
}
[dir="rtl"] .delegation-card.status-upcoming { border-right-color: #3b82f6; }

.delegation-card.status-expired {
  border-left-color: #94a3b8;
  opacity: 0.75;
}
[dir="rtl"] .delegation-card.status-expired { border-right-color: #94a3b8; }

.delegation-card.status-revoked {
  border-left-color: #ef4444;
  opacity: 0.7;
}
[dir="rtl"] .delegation-card.status-revoked { border-right-color: #ef4444; }

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 10px;
}

.card-title {
  display: flex;
  align-items: center;
  gap: 10px;
}

.card-title-text h4 {
  margin: 0;
  font-size: 14px;
  font-weight: 700;
  color: #1a2e25;
}

.card-type-badge {
  display: inline-block;
  margin-top: 2px;
  padding: 2px 8px;
  font-size: 10px;
  font-weight: 600;
  border-radius: 10px;
}
.card-type-badge.type-general {
  background: #fff7e8;
  color: #b45309;
  border: 1px solid #fde2b6;
}
.card-type-badge.type-task {
  background: #eef5f1;
  color: #006d4b;
  border: 1px solid #c8ddd3;
}

.status-pill {
  padding: 3px 10px;
  font-size: 11px;
  font-weight: 600;
  border-radius: 10px;
  border: 1px solid;
}
.status-pill.status-active {
  background: #dcfce7;
  color: #15803d;
  border-color: #86efac;
}
.status-pill.status-upcoming {
  background: #dbeafe;
  color: #1d4ed8;
  border-color: #93c5fd;
}
.status-pill.status-expired {
  background: #f1f5f9;
  color: #64748b;
  border-color: #cbd5e1;
}
.status-pill.status-revoked {
  background: #fee2e2;
  color: #b91c1c;
  border-color: #fca5a5;
}

.card-body {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 13px;
  color: #5a7a6d;
}
.card-row {
  display: flex;
  align-items: center;
  gap: 6px;
}

.card-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px solid #f0f4f2;
}

.btn-revoke {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 6px 12px;
  font-size: 12px;
  font-weight: 600;
  border-radius: 6px;
  border: 1px solid #fecaca;
  background: #fff;
  color: #dc2626;
  cursor: pointer;
  transition: all 0.15s;
  font-family: inherit;
}
.btn-revoke:hover {
  background: #fef2f2;
  border-color: #ef4444;
}

.empty-state {
  display: flex; flex-direction: column; align-items: center;
  padding: 48px 20px; color: #93afa4; gap: 10px;
}
.empty-state p { font-size: 13px; margin: 0; }

.delegation-form {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

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

.radio-group {
  display: flex;
  gap: 14px;
  padding: 4px 0;
}
.radio-label {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #1a2e25;
  cursor: pointer;
}
.radio-label input[type="radio"] {
  accent-color: #006d4b;
  cursor: pointer;
}

.task-picker {
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 180px;
  overflow-y: auto;
  padding: 8px;
  border: 1px solid #e4ede8;
  border-radius: 8px;
  background: #f7faf8;
}

.task-check {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 6px;
  border-radius: 6px;
  font-size: 13px;
  color: #1a2e25;
  cursor: pointer;
  transition: background 0.15s;
}
.task-check:hover { background: #fff; }
.task-check input[type="checkbox"] {
  width: 16px; height: 16px;
  accent-color: #006d4b;
  cursor: pointer;
}

.no-tasks {
  padding: 12px;
  text-align: center;
  font-size: 12px;
  color: #93afa4;
}

.loading-inline {
  display: flex;
  justify-content: center;
  padding: 12px;
  color: #006d4b;
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

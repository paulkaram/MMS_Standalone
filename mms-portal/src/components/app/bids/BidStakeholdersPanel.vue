<template>
  <section class="bsp">
    <header class="bsp-head">
      <h3>
        <Icon icon="mdi:account-group" class="w-4 h-4" />
        {{ $t('Stakeholders') || 'Stakeholders' }}
        <span class="bsp-count">{{ stakeholders.length }}</span>
      </h3>
      <button v-if="canEdit" class="btn-primary btn-sm" @click="openAddModal">
        <Icon icon="mdi:plus" class="w-4 h-4" />
        {{ $t('AddStakeholder') || 'Add Stakeholder' }}
      </button>
    </header>

    <div v-if="stakeholders.length === 0" class="bsp-empty">
      <Icon icon="mdi:account-group" class="w-8 h-8" />
      <p>{{ $t('NoStakeholdersYet') || 'No stakeholders yet. Add at least one before the workflow can fan out visions.' }}</p>
    </div>

    <div v-else class="bsp-list">
      <div v-for="sh in stakeholders" :key="sh.id" class="bsp-row">
        <UserAvatar :userId="sh.userId || ''" :name="sh.name" size="sm" />
        <div class="bsp-info">
          <div class="bsp-name">
            {{ sh.name }}
            <span v-if="sh.isTeamLeader" class="bsp-badge badge-tl">{{ $t('TeamLeader') }}</span>
            <span v-if="sh.isExternal" class="bsp-badge badge-ext">{{ $t('ExternalMember') }}</span>
          </div>
          <div v-if="sh.email" class="bsp-email">{{ sh.email }}</div>
        </div>
        <button v-if="canEdit" class="icon-btn icon-btn-danger" @click="removeStakeholder(sh)" :title="$t('Remove') || 'Remove'">
          <Icon icon="mdi:delete-outline" class="w-4 h-4" />
        </button>
      </div>
    </div>

    <!-- Add modal -->
    <Modal v-model="modalOpen" :title="$t('AddStakeholder') || 'Add Stakeholder'" size="md">
      <form class="bsp-form" @submit.prevent="save">
        <div class="form-field">
          <label>{{ $t('StakeholderType') || 'Type' }} *</label>
          <div class="type-toggle">
            <button type="button" class="type-btn" :class="{ active: form.type === 'internal' }" @click="switchType('internal')">
              <Icon icon="mdi:account" class="w-4 h-4" />
              {{ $t('InternalUser') || 'Internal User' }}
            </button>
            <button type="button" class="type-btn" :class="{ active: form.type === 'external' }" @click="switchType('external')">
              <Icon icon="mdi:account-tie" class="w-4 h-4" />
              {{ $t('ExternalMember') || 'External Member' }}
            </button>
          </div>
        </div>

        <div v-if="form.type === 'internal'" class="form-field">
          <label>{{ $t('User') || 'User' }} *</label>
          <CustomSelect
            v-model="form.userId"
            :options="internalOptions"
            value-key="id"
            label-key="name"
            :placeholder="$t('SelectFromCommitteeMembers') || 'Select from committee members'"
            searchable
          />
        </div>

        <div v-else class="form-field">
          <label>{{ $t('ExternalMember') || 'External Member' }} *</label>
          <CustomSelect
            v-model="form.externalMemberId"
            :options="externalOptions"
            value-key="id"
            label-key="name"
            :placeholder="$t('SelectFromExternalMembers') || 'Select from external members'"
            searchable
          />
        </div>

        <label class="cb">
          <input type="checkbox" v-model="form.isTeamLeader" />
          <span>{{ $t('MarkAsTeamLeader') || 'Mark as team leader for this bid' }}</span>
        </label>

        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="modalOpen = false">{{ $t('Cancel') }}</button>
          <button type="submit" class="btn-primary" :disabled="saving || !isFormValid">{{ $t('Add') || 'Add' }}</button>
        </div>
      </form>
    </Modal>
  </section>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import CustomSelect from '@/components/ui/CustomSelect.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import BidsService, { type BidStakeholder } from '@/services/BidsService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

interface Props { bidId: number; committeeId: number; stakeholders: BidStakeholder[]; canEdit: boolean }
const props = defineProps<Props>()
const emit = defineEmits<{ changed: [] }>()

const { t } = useI18n()
const { toast } = useToast()

const modalOpen = ref(false)
const saving = ref(false)

interface FormState {
  type: 'internal' | 'external'
  userId: string | null
  externalMemberId: number | null
  isTeamLeader: boolean
}
const form = ref<FormState>({ type: 'internal', userId: null, externalMemberId: null, isTeamLeader: false })

const internalOptions = ref<{ id: string; name: string }[]>([])
const externalOptions = ref<{ id: string; name: string }[]>([])

const isFormValid = computed(() => form.value.type === 'internal' ? !!form.value.userId : !!form.value.externalMemberId)

async function loadPickers() {
  if (!props.committeeId) return
  try {
    const [internal, external]: any = await Promise.all([
      BidsService.listCommitteeMembersForPicker(props.committeeId),
      BidsService.listExternalMembersForPicker(props.committeeId).catch(() => [])
    ])
    internalOptions.value = (internal?.data ?? internal ?? []).map((u: any) => ({ id: String(u.id), name: u.name }))
    externalOptions.value = (external?.data ?? external ?? []).map((u: any) => ({ id: String(u.id), name: u.name }))
  } catch {
    /* non-critical, modal just shows empty pickers */
  }
}

function openAddModal() {
  form.value = { type: 'internal', userId: null, externalMemberId: null, isTeamLeader: false }
  modalOpen.value = true
  if (internalOptions.value.length === 0) loadPickers()
}

function switchType(t: 'internal' | 'external') {
  form.value.type = t
  form.value.userId = null
  form.value.externalMemberId = null
}

async function save() {
  saving.value = true
  try {
    await BidsService.addStakeholder(props.bidId, {
      userId: form.value.type === 'internal' ? form.value.userId : null,
      externalMemberId: form.value.type === 'external' && form.value.externalMemberId
        ? Number(form.value.externalMemberId) : null,
      isTeamLeader: form.value.isTeamLeader
    })
    toast.success(t('Added') || 'Added')
    modalOpen.value = false
    emit('changed')
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    saving.value = false
  }
}

async function removeStakeholder(sh: BidStakeholder) {
  if (!confirm(t('ConfirmRemoveStakeholder') || 'Remove this stakeholder?')) return
  try {
    await BidsService.removeStakeholder(sh.id)
    toast.success(t('Removed') || 'Removed')
    emit('changed')
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  }
}

watch(() => props.committeeId, (cid) => { if (cid) loadPickers() })
onMounted(loadPickers)
</script>

<style scoped>
.bsp { display: flex; flex-direction: column; gap: 12px; }
.bsp-head { display: flex; justify-content: space-between; align-items: center; }
.bsp-head h3 { margin: 0; display: flex; align-items: center; gap: 6px; font-size: 14px; font-weight: 700; color: #1a2e25; }
.bsp-count {
  display: inline-flex; align-items: center; justify-content: center;
  min-width: 22px; height: 20px; padding: 0 7px;
  background: rgba(0, 109, 75, 0.1); color: #006d4b;
  border-radius: 10px; font-size: 11px; font-weight: 700;
}

.btn-primary, .btn-secondary {
  display: inline-flex; align-items: center; gap: 6px; padding: 8px 14px;
  border-radius: 8px; font-size: 13px; font-weight: 600; cursor: pointer;
  font-family: inherit; transition: all 0.15s; border: 1px solid transparent;
}
.btn-primary { background: #006d4b; color: #fff; }
.btn-primary:hover:not(:disabled) { background: #005339; }
.btn-primary:disabled { opacity: 0.55; cursor: not-allowed; }
.btn-primary.btn-sm { padding: 6px 11px; font-size: 12px; }
.btn-secondary { background: #fff; color: #374a41; border-color: #d4e0da; }
.btn-secondary:hover { background: #f4f8f6; }

.bsp-empty {
  display: flex; flex-direction: column; align-items: center;
  justify-content: center;
  gap: 10px; min-height: 200px; padding: 32px 20px; text-align: center;
  background: #f7faf8; border: 1px dashed #d4e0da; border-radius: 10px;
  color: #93afa4;
}
.bsp-empty p { margin: 0; font-size: 13px; max-width: 480px; color: #6b8a7d; }

.bsp-list { display: flex; flex-direction: column; gap: 8px; }
.bsp-row {
  display: flex; align-items: center; gap: 10px;
  padding: 10px 12px; background: #f7faf8; border: 1px solid #e4ede8;
  border-radius: 10px; transition: all 0.15s;
}
.bsp-row:hover { border-color: #c8ddd3; }
.bsp-info { flex: 1; min-width: 0; }
.bsp-name {
  font-size: 13px; font-weight: 600; color: #1a2e25;
  display: flex; align-items: center; gap: 6px;
}
.bsp-email { font-size: 11px; color: #6b8a7d; margin-top: 2px; }
.bsp-badge {
  padding: 1px 7px; border-radius: 7px; font-size: 10px; font-weight: 700;
  text-transform: uppercase; letter-spacing: 0.3px;
}
.badge-tl  { background: #dbeafe; color: #1d4ed8; border: 1px solid #93c5fd; }
.badge-ext { background: #eef2ff; color: #3b4cca; border: 1px solid #c5cbf5; }

.icon-btn { background: transparent; border: 1px solid #e4ede8; border-radius: 6px; padding: 4px 6px; cursor: pointer; color: #475569; transition: all 0.15s; }
.icon-btn:hover { background: #fff; }
.icon-btn-danger:hover { background: #fef2f2; color: #b91c1c; border-color: #fca5a5; }

/* Modal */
.bsp-form { display: flex; flex-direction: column; gap: 12px; }
.form-field { display: flex; flex-direction: column; gap: 4px; }
.form-field label { font-size: 12px; font-weight: 600; color: #374a41; }

.type-toggle { display: flex; gap: 6px; }
.type-btn {
  flex: 1; display: inline-flex; align-items: center; justify-content: center; gap: 6px;
  padding: 9px 12px; background: #f7faf8; border: 1.5px solid #d4e0da;
  border-radius: 8px; color: #475569; font-size: 13px; font-weight: 600;
  cursor: pointer; font-family: inherit; transition: all 0.15s;
}
.type-btn:hover { border-color: #63a58f; }
.type-btn.active { background: #eef5f1; border-color: #006d4b; color: #006d4b; }

.cb { display: inline-flex; align-items: center; gap: 6px; font-size: 13px; color: #1a2e25; font-weight: 500; cursor: pointer; padding: 4px 0; }
.cb input { accent-color: #006d4b; }

.form-actions { display: flex; justify-content: flex-end; gap: 8px; padding-top: 8px; border-top: 1px solid #e4ede8; margin-top: 6px; }
</style>

<template>
  <div class="space-y-5">
    <PageHeader
      :title="$t('ExternalMembers')"
      :subtitle="$t('ExternalMembersDesc')"
      :breadcrumbs="[{ label: $t('Settings'), to: '/settings' }]"
    >
      <template #actions>
        <button class="btn-clean primary" @click="openAdd">
          <Icon icon="mdi:plus" class="w-4 h-4" />
          {{ $t('AddExternalMember') }}
        </button>
      </template>
    </PageHeader>

    <div class="mg-container">
      <div v-if="loading" class="mg-state">
        <Icon icon="mdi:loading" class="w-6 h-6 animate-spin" style="color: #006d4b" />
      </div>

      <template v-else>
        <div class="mg-toolbar">
          <div class="mg-search">
            <Icon icon="mdi:magnify" class="w-4 h-4" style="color: #9ca3af" />
            <input v-model="search" type="text" :placeholder="$t('SearchExternalMembers')" />
          </div>
          <span class="mg-count">{{ filtered.length }} {{ $t('Of') }} {{ members.length }}</span>
        </div>

        <div v-if="filtered.length > 0" class="mg-table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>#</th>
                <th>{{ $t('NameAr') }}</th>
                <th>{{ $t('NameEn') }}</th>
                <th>{{ $t('Email') }}</th>
                <th>{{ $t('Organization') }}</th>
                <th>{{ $t('Position') }}</th>
                <th>{{ $t('CommitteesCount') }}</th>
                <th>{{ $t('Status') }}</th>
                <th>{{ $t('Actions') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(m, i) in filtered" :key="m.id">
                <td><span class="mg-id">{{ i + 1 }}</span></td>
                <td><span class="mg-title">{{ m.fullnameAr }}</span></td>
                <td>{{ m.fullnameEn }}</td>
                <td><span class="email-cell">{{ m.email }}</span></td>
                <td>{{ m.organization || '—' }}</td>
                <td>{{ m.position || '—' }}</td>
                <td>
                  <span class="usage-badge" :class="{ zero: m.committeesCount === 0 }">{{ m.committeesCount }}</span>
                </td>
                <td>
                  <span :class="['status-pill', m.isActive ? 'active' : 'inactive']">
                    {{ m.isActive ? $t('Active') : $t('Inactive') }}
                  </span>
                </td>
                <td>
                  <div class="mg-actions">
                    <button class="mg-act" :title="$t('Edit')" @click="openEdit(m)">
                      <Icon icon="mdi:pencil" class="w-4 h-4" />
                    </button>
                    <button class="mg-act danger" :title="$t('Delete')" :disabled="m.committeesCount > 0" @click="confirmDelete(m)">
                      <Icon icon="mdi:delete" class="w-4 h-4" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else class="empty-state">
          <Icon icon="mdi:account-group-outline" class="w-10 h-10" />
          <p>{{ $t('NoExternalMembers') }}</p>
        </div>
      </template>
    </div>

    <Modal v-model="modalOpen" :title="editing ? $t('EditExternalMember') : $t('AddExternalMember')" size="lg">
      <form class="member-form" @submit.prevent="save">
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('NameAr') }} <span class="required">*</span></label>
            <input v-model="form.fullnameAr" type="text" class="form-input" required />
          </div>
          <div class="form-field">
            <label>{{ $t('NameEn') }} <span class="required">*</span></label>
            <input v-model="form.fullnameEn" type="text" class="form-input" required />
          </div>
        </div>
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('Email') }} <span class="required">*</span></label>
            <input v-model="form.email" type="email" class="form-input" required />
          </div>
          <div class="form-field">
            <label>{{ $t('Mobile') }}</label>
            <input v-model="form.mobile" type="text" class="form-input" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('Organization') }}</label>
            <input v-model="form.organization" type="text" class="form-input" />
          </div>
          <div class="form-field">
            <label>{{ $t('Position') }}</label>
            <input v-model="form.position" type="text" class="form-input" />
          </div>
        </div>
        <div class="form-field checkbox-field">
          <label class="checkbox-label">
            <input v-model="form.isActive" type="checkbox" />
            <span>{{ $t('Active') }}</span>
          </label>
        </div>
        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="modalOpen = false">{{ $t('Cancel') }}</button>
          <button type="submit" class="btn-primary" :disabled="saving">{{ $t('Save') }}</button>
        </div>
      </form>
    </Modal>

    <ConfirmModal
      v-model="confirmOpen"
      :title="$t('DeleteExternalMember')"
      :message="$t('ConfirmDeleteExternalMember')"
      :confirm-text="$t('Delete')"
      :cancel-text="$t('Cancel')"
      variant="danger"
      @confirm="performDelete"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import ConfirmModal from '@/components/ui/ConfirmModal.vue'
import ExternalMembersService, { type ExternalMember } from '@/services/ExternalMembersService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const { toast } = useToast()

const members = ref<ExternalMember[]>([])
const search = ref('')
const loading = ref(false)
const saving = ref(false)
const modalOpen = ref(false)
const confirmOpen = ref(false)
const editing = ref<ExternalMember | null>(null)
const pendingDeleteId = ref<number | null>(null)

const emptyForm = () => ({
  fullnameAr: '',
  fullnameEn: '',
  email: '',
  mobile: '',
  organization: '',
  position: '',
  isActive: true
})

const form = reactive(emptyForm())

const filtered = computed(() => {
  if (!search.value) return members.value
  const q = search.value.toLowerCase()
  return members.value.filter(m =>
    m.fullnameAr.includes(q) ||
    m.fullnameEn.toLowerCase().includes(q) ||
    m.email.toLowerCase().includes(q) ||
    (m.organization || '').toLowerCase().includes(q)
  )
})

const load = async () => {
  loading.value = true
  try {
    const res: any = await ExternalMembersService.listAdmin()
    members.value = res?.data ?? res ?? []
  } catch (err) {
    console.error('Failed to load external members:', err)
  } finally {
    loading.value = false
  }
}

const openAdd = () => {
  editing.value = null
  Object.assign(form, emptyForm())
  modalOpen.value = true
}

const openEdit = (m: ExternalMember) => {
  editing.value = m
  Object.assign(form, {
    fullnameAr: m.fullnameAr,
    fullnameEn: m.fullnameEn,
    email: m.email,
    mobile: m.mobile || '',
    organization: m.organization || '',
    position: m.position || '',
    isActive: m.isActive
  })
  modalOpen.value = true
}

const save = async () => {
  if (!form.fullnameAr.trim() || !form.fullnameEn.trim() || !form.email.trim()) return
  saving.value = true
  try {
    if (editing.value) {
      await ExternalMembersService.update(editing.value.id, form)
      toast.success(t('ItemUpdatedSuccessfully'))
    } else {
      await ExternalMembersService.create(form)
      toast.success(t('ItemCreatedSuccessfully'))
    }
    modalOpen.value = false
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    saving.value = false
  }
}

const confirmDelete = (m: ExternalMember) => {
  if (m.committeesCount > 0) {
    toast.error(t('CannotDeleteExternalMember'))
    return
  }
  pendingDeleteId.value = m.id
  confirmOpen.value = true
}

const performDelete = async () => {
  if (!pendingDeleteId.value) return
  try {
    await ExternalMembersService.delete(pendingDeleteId.value)
    toast.success(t('ItemDeletedSuccessfully'))
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('CannotDeleteExternalMember'))
  } finally {
    pendingDeleteId.value = null
    confirmOpen.value = false
  }
}

onMounted(load)
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

.mg-container {
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 12px;
  overflow: hidden;
}

.mg-state { display: flex; justify-content: center; padding: 60px 0; }

.mg-toolbar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 12px 16px; border-bottom: 1px solid #e4ede8;
  background: #f7faf8;
}

.mg-search {
  display: flex; align-items: center; gap: 6px;
  padding: 6px 12px; background: #fff;
  border: 1px solid #d4e0da; border-radius: 8px;
  flex: 1; max-width: 320px;
}
.mg-search input {
  border: none; outline: none; background: transparent;
  font-size: 13px; color: #1a2e25; flex: 1;
  font-family: inherit;
}

.mg-count { font-size: 12px; color: #6b8a7d; font-weight: 500; }

.mg-table-wrap { overflow-x: auto; }
.data-table { width: 100%; border-collapse: collapse; }
.data-table thead { background-color: #006d4b; }
.data-table th {
  color: #fff; font-weight: 600; font-size: 12px;
  padding: 12px 16px; text-align: start;
}
.data-table tbody tr { border-bottom: 1px solid #f0f4f2; transition: background 0.15s; }
.data-table tbody tr:hover { background: rgba(0, 109, 75, 0.04); }
.data-table td { padding: 12px 16px; font-size: 13px; color: #374151; }

.mg-id {
  display: inline-block; min-width: 20px;
  font-weight: 700; color: #006d4b;
}
.mg-title { font-weight: 500; color: #1e293b; }

.email-cell { font-family: 'Consolas', monospace; font-size: 12px; color: #006d4b; }

.usage-badge {
  display: inline-block; padding: 2px 8px;
  background: #e8f5ef; color: #006d4b;
  border: 1px solid #c8ddd3; border-radius: 10px;
  font-size: 11px; font-weight: 600;
}
.usage-badge.zero { background: #f5f5f5; color: #94a3b8; border-color: #e5e7eb; }

.status-pill {
  display: inline-block; padding: 2px 10px;
  font-size: 11px; font-weight: 600; border-radius: 10px;
}
.status-pill.active { background: #e8f5ef; color: #006d4b; border: 1px solid #c8ddd3; }
.status-pill.inactive { background: #fef2f2; color: #dc2626; border: 1px solid #fecaca; }

.mg-actions { display: flex; gap: 4px; }
.mg-act {
  display: inline-flex; align-items: center; justify-content: center;
  width: 30px; height: 30px;
  border: 1px solid #e4ede8; background: #fff;
  border-radius: 6px; color: #5a7a6d;
  cursor: pointer; transition: all 0.15s;
}
.mg-act:hover:not(:disabled) {
  background: #f4f8f6; border-color: #006d4b; color: #006d4b;
}
.mg-act.danger:hover:not(:disabled) {
  background: #fef2f2; border-color: #ef4444; color: #ef4444;
}
.mg-act:disabled { opacity: 0.35; cursor: not-allowed; }

.empty-state {
  display: flex; flex-direction: column; align-items: center;
  padding: 48px 20px; color: #93afa4; gap: 10px;
}
.empty-state p { font-size: 13px; margin: 0; }

.member-form { display: flex; flex-direction: column; gap: 12px; }
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

.checkbox-field { gap: 2px; }
.checkbox-label {
  display: flex; align-items: center; gap: 8px;
  font-size: 13px; color: #1a2e25;
  cursor: pointer; font-weight: 500;
}
.checkbox-label input[type="checkbox"] {
  width: 16px; height: 16px;
  accent-color: #006d4b; cursor: pointer;
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

<template>
  <Modal
    :model-value="show"
    :title="isEdit ? $t('EditRecommendation') : $t('AddRecommendation')"
    icon="mdi:lightbulb-outline"
    size="lg"
    @update:model-value="(val: boolean) => { if (!val) $emit('close') }"
  >
    <form class="rec-form" @submit.prevent="handleSubmit">
      <!-- Recommendation Text -->
      <div class="rec-field">
        <label class="rec-label">{{ $t('RecommendationText') }} <span class="rec-req">*</span></label>
        <input v-model="form.text" type="text" class="rec-input" :placeholder="$t('EnterRecommendationText')" />
      </div>

      <!-- Details -->
      <div class="rec-field">
        <label class="rec-label">{{ $t('Details') }}</label>
        <textarea v-model="form.description" class="rec-input rec-textarea" :placeholder="$t('EnterAdditionalDetails')" rows="3"></textarea>
      </div>

      <!-- Priority & Due Date -->
      <div class="rec-row">
        <div class="rec-field">
          <CustomSelect
            v-model="form.priorityId"
            :options="priorities"
            :label="$t('Priority')"
            :placeholder="$t('ChoosePriority')"
            value-key="id"
            label-key="name"
            clearable
          />
        </div>
        <div class="rec-field">
          <DatePicker v-model="form.dueDate" :label="$t('DueDateOptional')" />
        </div>
      </div>

      <!-- Department & Owner -->
      <div class="rec-row">
        <div class="rec-field">
          <CustomSelect
            v-model="form.departmentId"
            :options="departments"
            :label="$t('Department')"
            :placeholder="$t('ChooseDepartment')"
            value-key="id"
            label-key="name"
            clearable
            searchable
            @change="onDepartmentChange"
          />
        </div>
        <div class="rec-field">
          <label class="rec-label">{{ $t('Owner') }} <span class="rec-req">*</span></label>
          <Combobox
            v-model="form.owner"
            :options="ownerOptions"
            item-value="id"
            item-text="name"
            :placeholder="form.departmentId ? $t('SearchUser') : $t('ChooseDepartmentFirst')"
            :loading="searchingOwner"
            :disabled="!form.departmentId"
            :min-search-length="0"
            @search="searchOwner"
          />
        </div>
      </div>
    </form>

    <template #footer>
      <button type="button" class="rec-btn rec-btn-cancel" @click="$emit('close')">{{ $t('Cancel') }}</button>
      <button type="button" class="rec-btn rec-btn-submit" :disabled="!isValid || saving" @click="handleSubmit">
        <Icon v-if="saving" icon="mdi:loading" class="w-4 h-4 animate-spin" />
        {{ saving ? $t('Loading') : (isEdit ? $t('SaveChanges') : $t('AddRecommendation')) }}
      </button>
    </template>
  </Modal>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import CustomSelect from '@/components/ui/CustomSelect.vue'
import Combobox from '@/components/ui/Combobox.vue'
import DatePicker from '@/components/ui/DatePicker.vue'
import LookupsService, { type LookupItem } from '@/services/LookupsService'
import StructuresService from '@/services/StructuresService'

const props = defineProps<{
  show: boolean
  agendaId: number
  recommendation?: any
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'save', data: any): void
}>()

// Form
const form = ref({
  text: '',
  description: '',
  priorityId: null as number | null,
  dueDate: null as Date | string | null,
  departmentId: null as string | null,
  owner: null as string | null
})

// Lookups
const priorities = ref<LookupItem[]>([])
const departments = ref<any[]>([])
const ownerOptions = ref<any[]>([])

// State
const searchingOwner = ref(false)
const saving = ref(false)

// Computed
const isEdit = computed(() => !!props.recommendation?.id)
const isValid = computed(() => !!(form.value.text?.trim() && form.value.owner))

// Load priorities
const loadPriorities = async () => {
  try {
    const result = await LookupsService.getRecommendationPriorities()
    const data = result?.data || result
    priorities.value = Array.isArray(data) ? data : []
  } catch (e) {
    console.error('Failed to load priorities:', e)
  }
}

// Load departments from IAM
const loadDepartments = async () => {
  try {
    const result: any = await StructuresService.listIamOrganization()
    const data = result?.data || result
    const flat: any[] = Array.isArray(data) ? data : (data?.items || data?.value || [])
    departments.value = flat.map((d: any) => ({
      id: String(d.id ?? d.Id),
      name: d.name ?? d.Name ?? d.nameAr ?? ''
    }))
  } catch (e) {
    console.error('Failed to load departments:', e)
    departments.value = []
  }
}

// All users loaded for the selected department
const deptUsers = ref<any[]>([])

// Load users for a department (preserveOwner = true when populating edit form)
const loadDeptUsers = async (deptId: string) => {
  searchingOwner.value = true
  try {
    const result: any = await StructuresService.listIamDepartmentUsers(deptId)
    const data = result?.data || result
    const users = Array.isArray(data) ? data : (data?.items || data?.value || [])
    const getName = (u: any) => u.displayName ?? u.fullName ?? u.name ?? `${u.firstName || ''} ${u.lastName || ''}`.trim()
    const getId = (u: any) => u.id ?? u.Id ?? u.userId ?? u.UserId ?? getName(u)
    deptUsers.value = users.map((u: any) => ({ id: String(getId(u)), name: getName(u), email: u.email ?? u.Email ?? '' }))
    ownerOptions.value = [...deptUsers.value]
  } catch (e) {
    console.error('Failed to load department users:', e)
  } finally {
    searchingOwner.value = false
  }
}

// When department changes via user interaction, clear owner and reload
const onDepartmentChange = async () => {
  form.value.owner = null
  ownerOptions.value = []
  deptUsers.value = []
  if (!form.value.departmentId) return
  await loadDeptUsers(form.value.departmentId)
}

// Filter within loaded department users (shows all when query empty)
const searchOwner = (query: string) => {
  if (!query || !query.trim()) {
    ownerOptions.value = [...deptUsers.value]
    return
  }
  const q = query.toLowerCase()
  ownerOptions.value = deptUsers.value.filter(u =>
    u.name.toLowerCase().includes(q) || (u.email && u.email.toLowerCase().includes(q))
  )
}

const resetForm = () => {
  form.value = { text: '', description: '', priorityId: null, dueDate: null, departmentId: null, owner: null }
  ownerOptions.value = []
  deptUsers.value = []
}

const populateForm = async (rec: any) => {
  const ownerId = rec.owner ? String(rec.owner) : null

  form.value = {
    text: rec.text || '',
    description: rec.description || '',
    priorityId: rec.priorityId || null,
    dueDate: rec.dueDate ? new Date(rec.dueDate) : null,
    departmentId: rec.ownerStructureId ? String(rec.ownerStructureId) : null,
    owner: ownerId
  }

  // Load department users without clearing owner
  if (rec.ownerStructureId) {
    await loadDeptUsers(String(rec.ownerStructureId))
  }

  // Ensure the current owner is in options so Combobox can display the name
  if (ownerId && rec.ownerName) {
    if (!ownerOptions.value.find(o => String(o.id) === ownerId)) {
      ownerOptions.value.push({ id: ownerId, name: rec.ownerName })
      deptUsers.value.push({ id: ownerId, name: rec.ownerName, email: '' })
    }
  }

  // Re-set owner after loading (ensure ref is correct)
  form.value.owner = ownerId
}

const handleSubmit = async () => {
  if (!isValid.value) return
  saving.value = true

  try {
    let dueDateStr = null
    if (form.value.dueDate) {
      const date = form.value.dueDate instanceof Date ? form.value.dueDate : new Date(form.value.dueDate)
      dueDateStr = date.toISOString().split('T')[0]
    }

    emit('save', {
      meetingAgendaId: props.agendaId,
      text: form.value.text?.trim(),
      description: form.value.description?.trim() || '',
      owner: form.value.owner,
      dueDate: dueDateStr,
      priorityId: form.value.priorityId,
      ownerStructureId: form.value.departmentId,
      ...(isEdit.value && { id: props.recommendation.id })
    })
  } catch (e) {
    console.error('Failed to submit:', e)
    saving.value = false
  }
}

// Watch modal open/close
watch(() => props.show, async (show) => {
  if (show) {
    const promises: Promise<void>[] = []
    if (priorities.value.length === 0) promises.push(loadPriorities())
    if (departments.value.length === 0) promises.push(loadDepartments())
    await Promise.all(promises)
    if (props.recommendation) {
      await populateForm(props.recommendation)
    } else {
      resetForm()
    }
  } else {
    saving.value = false
  }
})

watch(() => props.recommendation, async (rec, oldRec) => {
  if (props.show && rec && rec !== oldRec) await populateForm(rec)
  else if (!rec && props.show) resetForm()
})
</script>

<style scoped>
.rec-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 4px 0;
}

.rec-field {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.rec-label {
  font-size: 13px;
  font-weight: 500;
  color: #334155;
}

.rec-req {
  color: #ef4444;
  margin-inline-start: 2px;
}

.rec-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}

.rec-input {
  width: 100%;
  padding: 9px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 14px;
  font-family: inherit;
  color: #1e293b;
  background: #fff;
  outline: none;
  transition: border-color 0.15s;
}

.rec-input:focus {
  border-color: #006d4b;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

.rec-input::placeholder {
  color: #94a3b8;
}

.rec-textarea {
  resize: vertical;
  min-height: 76px;
}

/* Footer buttons */
.rec-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 9px 20px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  border: none;
  transition: all 0.15s;
}

.rec-btn-cancel {
  background: #fff;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.rec-btn-cancel:hover {
  background: #f8fafc;
}

.rec-btn-submit {
  background: linear-gradient(135deg, #004730 0%, #006d4b 100%);
  color: #fff;
}

.rec-btn-submit:hover:not(:disabled) {
  opacity: 0.9;
}

.rec-btn-submit:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

@media (max-width: 640px) {
  .rec-row {
    grid-template-columns: 1fr;
  }
}
</style>

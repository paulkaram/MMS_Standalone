<template>
  <div class="wfd-page">
    <PageHeader :title="$t('WorkflowDesigner') || 'Workflow Designer'" :subtitle="$t('WorkflowDesignerDesc') || 'Configure who acts at every bid step (no hardcoded roles)'">
      <template #actions>
        <button class="btn-primary" @click="openCreateTemplate">
          <Icon icon="mdi:plus" class="w-4 h-4" />
          {{ $t('NewTemplate') || 'New Template' }}
        </button>
      </template>
    </PageHeader>

    <div v-if="loading" class="loading-state"><div class="spinner"></div></div>

    <div v-else class="wfd-layout">
      <!-- LEFT: Template list -->
      <aside class="wfd-aside">
        <h3 class="wfd-aside-title">{{ $t('Templates') || 'Templates' }}</h3>
        <div v-for="t in templates" :key="t.id" class="wfd-tpl-card" :class="{ active: selectedTemplateId === t.id }" @click="selectTemplate(t.id)">
          <div class="wfd-tpl-name">{{ isRtl ? t.nameAr : t.nameEn }}</div>
          <div class="wfd-tpl-meta">
            <span class="wfd-tpl-tag" :class="t.committeeId ? 'tag-scope' : 'tag-global'">
              {{ t.committeeId ? (t.committeeName || $t('Committee')) : ($t('Global') || 'Global') }}
            </span>
            <span v-if="!t.isActive" class="wfd-tpl-tag tag-inactive">{{ $t('Inactive') || 'Inactive' }}</span>
            <span class="wfd-tpl-counts">
              {{ t.stepsCount }} {{ $t('Steps') || 'steps' }} ·
              {{ t.instancesCount }} {{ $t('Bids') || 'bids' }}
            </span>
          </div>
        </div>
        <div v-if="templates.length === 0" class="empty-inline">{{ $t('NoTemplates') || 'No templates yet.' }}</div>
      </aside>

      <!-- RIGHT: Editor -->
      <main class="wfd-main">
        <div v-if="!template" class="empty-state">
          <Icon icon="mdi:transit-connection-variant" class="w-12 h-12" />
          <p>{{ $t('SelectTemplateToEdit') || 'Select a template to edit, or create a new one.' }}</p>
        </div>

        <template v-else>
          <!-- Template header -->
          <div class="wfd-tpl-header">
            <div>
              <h2>{{ isRtl ? template.nameAr : template.nameEn }}</h2>
              <div class="wfd-tpl-sub">v{{ template.version }} · {{ template.committeeName || $t('Global') }}</div>
            </div>
            <div class="wfd-tpl-actions">
              <button class="btn-secondary" @click="openEditTemplate"><Icon icon="mdi:pencil-outline" class="w-4 h-4" />{{ $t('Edit') }}</button>
              <button class="btn-danger" @click="deleteTemplate"><Icon icon="mdi:delete-outline" class="w-4 h-4" />{{ $t('Delete') }}</button>
            </div>
          </div>

          <!-- Tabs -->
          <div class="wfd-tabs">
            <button :class="['wfd-tab', { active: tab === 'steps' }]" @click="tab = 'steps'">
              <Icon icon="mdi:format-list-numbered" class="w-4 h-4" />{{ $t('Steps') || 'Steps' }} ({{ template.steps.length }})
            </button>
            <button :class="['wfd-tab', { active: tab === 'transitions' }]" @click="tab = 'transitions'">
              <Icon icon="mdi:arrow-right" class="w-4 h-4" />{{ $t('Transitions') || 'Transitions' }} ({{ template.transitions.length }})
            </button>
          </div>

          <!-- STEPS TAB -->
          <section v-if="tab === 'steps'" class="wfd-pane">
            <div class="wfd-pane-head">
              <button class="btn-primary" @click="openStepEditor(null)"><Icon icon="mdi:plus" class="w-4 h-4" />{{ $t('AddStep') || 'Add Step' }}</button>
            </div>
            <table class="wfd-table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>{{ $t('Name') }}</th>
                  <th>{{ $t('Actor') || 'Actor' }}</th>
                  <th>{{ $t('SLA') || 'SLA' }}</th>
                  <th>{{ $t('Flags') || 'Flags' }}</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="s in template.steps" :key="s.id">
                  <td class="wfd-step-order">{{ s.stepOrder }}</td>
                  <td>{{ isRtl ? s.nameAr : s.nameEn }}</td>
                  <td>
                    <span class="wfd-actor-badge">{{ actorSourceTypeLabel(s.actorSourceType, isRtl) }}</span>
                    <span v-if="s.actorTargetId" class="wfd-actor-target">{{ resolveTargetLabel(s.actorSourceType, s.actorTargetId) }}</span>
                  </td>
                  <td>{{ s.slaDays ? `${s.slaDays}d` : '—' }}</td>
                  <td>
                    <span v-if="s.isTerminal" class="wfd-flag flag-terminal">{{ $t('Terminal') || 'Terminal' }}</span>
                    <span v-if="s.isAutoAdvance" class="wfd-flag flag-auto">{{ $t('Auto') || 'Auto' }}</span>
                  </td>
                  <td class="wfd-actions-cell">
                    <button class="icon-btn" @click="openStepEditor(s)"><Icon icon="mdi:pencil-outline" class="w-4 h-4" /></button>
                    <button class="icon-btn icon-btn-danger" @click="deleteStep(s.id)"><Icon icon="mdi:delete-outline" class="w-4 h-4" /></button>
                  </td>
                </tr>
              </tbody>
            </table>
            <div v-if="template.steps.length === 0" class="empty-inline">{{ $t('NoSteps') || 'No steps defined yet.' }}</div>
          </section>

          <!-- TRANSITIONS TAB -->
          <section v-if="tab === 'transitions'" class="wfd-pane">
            <div class="wfd-pane-head">
              <button class="btn-primary" @click="openTransitionEditor(null)"><Icon icon="mdi:plus" class="w-4 h-4" />{{ $t('AddTransition') || 'Add Transition' }}</button>
            </div>
            <table class="wfd-table">
              <thead>
                <tr>
                  <th>{{ $t('From') || 'From' }}</th>
                  <th></th>
                  <th>{{ $t('To') || 'To' }}</th>
                  <th>{{ $t('Label') || 'Label' }}</th>
                  <th>{{ $t('Action') || 'Action' }}</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="tr in template.transitions" :key="tr.id">
                  <td>{{ stepName(tr.fromStepId) }}</td>
                  <td><Icon icon="mdi:arrow-right" class="w-4 h-4 wfd-arrow" /></td>
                  <td>{{ stepName(tr.toStepId) }}</td>
                  <td>{{ isRtl ? tr.labelAr : tr.labelEn }}</td>
                  <td><span class="wfd-action-badge" :class="`action-${tr.actionType}`">{{ actionTypeLabel(tr.actionType, isRtl) }}</span></td>
                  <td class="wfd-actions-cell">
                    <button class="icon-btn" @click="openTransitionEditor(tr)"><Icon icon="mdi:pencil-outline" class="w-4 h-4" /></button>
                    <button class="icon-btn icon-btn-danger" @click="deleteTransition(tr.id)"><Icon icon="mdi:delete-outline" class="w-4 h-4" /></button>
                  </td>
                </tr>
              </tbody>
            </table>
            <div v-if="template.transitions.length === 0" class="empty-inline">{{ $t('NoTransitions') || 'No transitions defined yet.' }}</div>
          </section>
        </template>
      </main>
    </div>

    <!-- TEMPLATE MODAL -->
    <Modal v-model="tplModalOpen" :title="tplForm.id ? ($t('EditTemplate') || 'Edit Template') : ($t('NewTemplate') || 'New Template')" size="md">
      <form class="wfd-form" @submit.prevent="saveTemplate">
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('NameEn') || 'Name (English)' }} *</label>
            <input v-model="tplForm.nameEn" class="form-input" required />
          </div>
          <div class="form-field">
            <label>{{ $t('NameAr') || 'Name (Arabic)' }} *</label>
            <input v-model="tplForm.nameAr" class="form-input" required />
          </div>
        </div>
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('Scope') || 'Scope' }}</label>
            <CustomSelect
              v-model="tplForm.committeeId"
              :options="scopeOptions"
              value-key="id"
              label-key="name"
              clearable
            />
          </div>
          <div class="form-field">
            <label>{{ $t('Active') || 'Active' }}</label>
            <label class="toggle">
              <input type="checkbox" v-model="tplForm.isActive" />
              <span>{{ tplForm.isActive ? ($t('Yes') || 'Yes') : ($t('No') || 'No') }}</span>
            </label>
          </div>
        </div>
        <div class="form-field">
          <label>{{ $t('DescriptionEn') || 'Description (English)' }}</label>
          <textarea v-model="tplForm.descriptionEn" rows="2" class="form-input"></textarea>
        </div>
        <div class="form-field">
          <label>{{ $t('DescriptionAr') || 'Description (Arabic)' }}</label>
          <textarea v-model="tplForm.descriptionAr" rows="2" class="form-input"></textarea>
        </div>
        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="tplModalOpen = false">{{ $t('Cancel') }}</button>
          <button type="submit" class="btn-primary" :disabled="saving">{{ $t('Save') }}</button>
        </div>
      </form>
    </Modal>

    <!-- STEP MODAL -->
    <Modal v-model="stepModalOpen" :title="stepForm.id ? ($t('EditStep') || 'Edit Step') : ($t('AddStep') || 'Add Step')" size="lg">
      <form class="wfd-form" @submit.prevent="saveStep">
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('StepOrder') || 'Order' }} *</label>
            <input v-model.number="stepForm.stepOrder" type="number" class="form-input" required />
          </div>
          <div class="form-field">
            <label>{{ $t('LegacyStatus') || 'Legacy Bid Status (optional)' }}</label>
            <input v-model.number="stepForm.legacyBidStatusId" type="number" class="form-input" placeholder="1-12" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('NameEn') }} *</label>
            <input v-model="stepForm.nameEn" class="form-input" required />
          </div>
          <div class="form-field">
            <label>{{ $t('NameAr') }} *</label>
            <input v-model="stepForm.nameAr" class="form-input" required />
          </div>
        </div>

        <div class="wfd-section-title">{{ $t('ActorAssignment') || 'Actor Assignment' }}</div>
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('ActorSource') || 'Actor Source' }} *</label>
            <CustomSelect
              v-model="stepForm.actorSourceType"
              :options="actorSourceOptions"
              value-key="id"
              label-key="name"
            />
          </div>
          <div class="form-field" v-if="needsActorTarget(stepForm.actorSourceType)">
            <label>{{ $t('ActorTarget') || 'Actor Target' }} *</label>
            <CustomSelect
              v-model="stepForm.actorTargetId"
              :options="actorTargetOptions(stepForm.actorSourceType)"
              value-key="id"
              label-key="name"
              searchable
            />
          </div>
        </div>

        <div class="wfd-section-title">{{ $t('TaskTemplate') || 'Task Template' }}</div>
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('TaskTitleEn') || 'Task title (English)' }}</label>
            <input v-model="stepForm.taskTitleEn" class="form-input" />
          </div>
          <div class="form-field">
            <label>{{ $t('TaskTitleAr') || 'Task title (Arabic)' }}</label>
            <input v-model="stepForm.taskTitleAr" class="form-input" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('SlaDays') || 'SLA (days)' }}</label>
            <input v-model.number="stepForm.slaDays" type="number" class="form-input" placeholder="0 = no SLA" />
          </div>
          <div class="form-field">
            <label>{{ $t('Flags') || 'Flags' }}</label>
            <div class="checkbox-row">
              <label class="cb"><input type="checkbox" v-model="stepForm.isTerminal" />{{ $t('Terminal') || 'Terminal' }}</label>
              <label class="cb"><input type="checkbox" v-model="stepForm.isAutoAdvance" />{{ $t('AutoAdvance') || 'Auto-advance' }}</label>
            </div>
          </div>
        </div>

        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="stepModalOpen = false">{{ $t('Cancel') }}</button>
          <button type="submit" class="btn-primary" :disabled="saving">{{ $t('Save') }}</button>
        </div>
      </form>
    </Modal>

    <!-- TRANSITION MODAL -->
    <Modal v-model="trModalOpen" :title="trForm.id ? ($t('EditTransition') || 'Edit Transition') : ($t('AddTransition') || 'Add Transition')" size="md">
      <form class="wfd-form" @submit.prevent="saveTransition">
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('FromStep') || 'From step' }} *</label>
            <CustomSelect v-model="trForm.fromStepId" :options="stepOptions" value-key="id" label-key="name" required />
          </div>
          <div class="form-field">
            <label>{{ $t('ToStep') || 'To step' }} *</label>
            <CustomSelect v-model="trForm.toStepId" :options="stepOptions" value-key="id" label-key="name" required />
          </div>
        </div>
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('LabelEn') || 'Label (English)' }} *</label>
            <input v-model="trForm.labelEn" class="form-input" required />
          </div>
          <div class="form-field">
            <label>{{ $t('LabelAr') || 'Label (Arabic)' }} *</label>
            <input v-model="trForm.labelAr" class="form-input" required />
          </div>
        </div>
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('Action') || 'Action' }} *</label>
            <CustomSelect v-model="trForm.actionType" :options="actionTypeOptions" value-key="id" label-key="name" />
          </div>
          <div class="form-field">
            <label>{{ $t('DisplayOrder') || 'Display Order' }}</label>
            <input v-model.number="trForm.displayOrder" type="number" class="form-input" />
          </div>
        </div>
        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="trModalOpen = false">{{ $t('Cancel') }}</button>
          <button type="submit" class="btn-primary" :disabled="saving">{{ $t('Save') }}</button>
        </div>
      </form>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import CustomSelect from '@/components/ui/CustomSelect.vue'
import {
  WorkflowDesignerService,
  ActorSourceType,
  WorkflowActionType,
  actorSourceTypeLabel,
  actionTypeLabel,
  type WorkflowTemplate,
  type WorkflowTemplateListItem,
  type WorkflowStepPost,
  type WorkflowTransitionPost,
  type WorkflowTemplatePost,
  type ActorOptions
} from '@/services/WorkflowService'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'
import { useUserStore } from '@/stores/user'

const { t } = useI18n()
const { toast } = useToast()
const userStore = useUserStore()
const isRtl = computed(() => userStore.isRtl)

const loading = ref(false)
const saving = ref(false)
const tab = ref<'steps' | 'transitions'>('steps')

const templates = ref<WorkflowTemplateListItem[]>([])
const selectedTemplateId = ref<number | null>(null)
const template = ref<WorkflowTemplate | null>(null)

const actorOptions = ref<ActorOptions>({ roles: [], groups: [], committeeRoles: [], permissions: [] })
const committees = ref<{ id: number; name: string }[]>([])

// Modal state
const tplModalOpen = ref(false)
const tplForm = ref<{ id?: number } & WorkflowTemplatePost>({ nameEn: '', nameAr: '', isActive: true, committeeId: null })

const stepModalOpen = ref(false)
const stepForm = ref<{ id?: number } & WorkflowStepPost>(emptyStepForm())

const trModalOpen = ref(false)
const trForm = ref<{ id?: number } & WorkflowTransitionPost>(emptyTrForm())

function emptyStepForm(): WorkflowStepPost {
  return {
    stepOrder: 1, nameEn: '', nameAr: '', isTerminal: false, isAutoAdvance: false,
    actorSourceType: ActorSourceType.TeamLeader, actorTargetId: null,
    taskTitleEn: '', taskTitleAr: '', taskBodyEn: '', taskBodyAr: '',
    slaDays: null, legacyBidStatusId: null
  }
}

function emptyTrForm(): WorkflowTransitionPost {
  return { fromStepId: 0, toStepId: 0, labelEn: '', labelAr: '', actionType: WorkflowActionType.Advance, displayOrder: 0 }
}

// ─────── Lookups ───────
const actorSourceOptions = computed(() => Object.entries(ActorSourceType).map(([, v]) => ({
  id: v, name: actorSourceTypeLabel(v, isRtl.value)
})))

const actionTypeOptions = computed(() => Object.entries(WorkflowActionType).map(([, v]) => ({
  id: v, name: actionTypeLabel(v, isRtl.value)
})))

const scopeOptions = computed(() => committees.value.map(c => ({ id: c.id, name: c.name })))

const stepOptions = computed(() => template.value
  ? template.value.steps.map(s => ({ id: s.id, name: `#${s.stepOrder} · ${isRtl.value ? s.nameAr : s.nameEn}` }))
  : [])

function needsActorTarget(sourceType: number) {
  return sourceType === ActorSourceType.Role
      || sourceType === ActorSourceType.Group
      || sourceType === ActorSourceType.CommitteeRole
      || sourceType === ActorSourceType.Permission
      || sourceType === ActorSourceType.SpecificUser
}

function actorTargetOptions(sourceType: number): { id: string; name: string }[] {
  let list: { id: string; labelAr: string; labelEn: string }[] = []
  switch (sourceType) {
    case ActorSourceType.Role:          list = actorOptions.value.roles; break
    case ActorSourceType.Group:         list = actorOptions.value.groups; break
    case ActorSourceType.CommitteeRole: list = actorOptions.value.committeeRoles; break
    case ActorSourceType.Permission:    list = actorOptions.value.permissions; break
    default: return []
  }
  return list.map(x => ({ id: x.id, name: isRtl.value ? x.labelAr : x.labelEn }))
}

function resolveTargetLabel(sourceType: number, targetId: string): string {
  if (sourceType === ActorSourceType.SpecificUser) return targetId
  const opts = actorTargetOptions(sourceType)
  return opts.find(o => o.id === targetId)?.name || targetId
}

function stepName(stepId: number): string {
  if (!template.value) return '—'
  const s = template.value.steps.find(x => x.id === stepId)
  return s ? `#${s.stepOrder} ${isRtl.value ? s.nameAr : s.nameEn}` : '—'
}

// ─────── Loaders ───────
async function loadAll() {
  loading.value = true
  try {
    const [tpls, opts, committeesRes]: any = await Promise.all([
      WorkflowDesignerService.listTemplates(),
      WorkflowDesignerService.getActorOptions(),
      CouncilCommitteesService.listUserCommittees().catch(() => [])
    ])
    templates.value = tpls || []
    actorOptions.value = opts || { roles: [], groups: [], committeeRoles: [], permissions: [] }
    const cList = (committeesRes?.data ?? committeesRes ?? []) as any[]
    committees.value = cList.map((c: any) => ({ id: c.id, name: c.name || c.nameAr || c.nameEn || '' }))

    if (templates.value.length > 0 && !selectedTemplateId.value) {
      await selectTemplate(templates.value[0].id)
    }
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    loading.value = false
  }
}

async function selectTemplate(id: number) {
  selectedTemplateId.value = id
  template.value = await WorkflowDesignerService.getTemplate(id)
}

// ─────── Template CRUD ───────
function openCreateTemplate() {
  tplForm.value = { nameEn: '', nameAr: '', isActive: true, committeeId: null }
  tplModalOpen.value = true
}

function openEditTemplate() {
  if (!template.value) return
  tplForm.value = {
    id: template.value.id,
    nameEn: template.value.nameEn,
    nameAr: template.value.nameAr,
    descriptionEn: template.value.descriptionEn,
    descriptionAr: template.value.descriptionAr,
    committeeId: template.value.committeeId,
    isActive: template.value.isActive,
    initiatorActorSourceType: template.value.initiatorActorSourceType,
    initiatorActorTargetId: template.value.initiatorActorTargetId
  }
  tplModalOpen.value = true
}

async function saveTemplate() {
  saving.value = true
  try {
    if (tplForm.value.id) {
      await WorkflowDesignerService.updateTemplate(tplForm.value.id, tplForm.value)
    } else {
      const created = await WorkflowDesignerService.createTemplate(tplForm.value)
      selectedTemplateId.value = created.id
    }
    tplModalOpen.value = false
    await loadAll()
    if (selectedTemplateId.value) await selectTemplate(selectedTemplateId.value)
    toast.success(t('Saved') || 'Saved')
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    saving.value = false
  }
}

async function deleteTemplate() {
  if (!template.value) return
  if (!confirm(t('ConfirmDelete') || 'Delete this template?')) return
  try {
    await WorkflowDesignerService.deleteTemplate(template.value.id)
    template.value = null
    selectedTemplateId.value = null
    await loadAll()
    toast.success(t('Deleted') || 'Deleted')
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  }
}

// ─────── Step CRUD ───────
function openStepEditor(step: any) {
  if (step) {
    stepForm.value = { ...step }
  } else {
    const next = (template.value?.steps.reduce((m, s) => Math.max(m, s.stepOrder), 0) || 0) + 1
    stepForm.value = { ...emptyStepForm(), stepOrder: next }
  }
  stepModalOpen.value = true
}

async function saveStep() {
  if (!template.value) return
  saving.value = true
  try {
    if (stepForm.value.id) {
      await WorkflowDesignerService.updateStep(stepForm.value.id, stepForm.value)
    } else {
      await WorkflowDesignerService.addStep(template.value.id, stepForm.value)
    }
    stepModalOpen.value = false
    await selectTemplate(template.value.id)
    toast.success(t('Saved') || 'Saved')
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    saving.value = false
  }
}

async function deleteStep(stepId: number) {
  if (!confirm(t('ConfirmDelete') || 'Delete this step?')) return
  try {
    await WorkflowDesignerService.deleteStep(stepId)
    if (template.value) await selectTemplate(template.value.id)
    toast.success(t('Deleted') || 'Deleted')
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  }
}

// ─────── Transition CRUD ───────
function openTransitionEditor(tr: any) {
  if (tr) {
    trForm.value = { ...tr }
  } else {
    trForm.value = emptyTrForm()
  }
  trModalOpen.value = true
}

async function saveTransition() {
  if (!template.value) return
  saving.value = true
  try {
    if (trForm.value.id) {
      await WorkflowDesignerService.updateTransition(trForm.value.id, trForm.value)
    } else {
      await WorkflowDesignerService.addTransition(template.value.id, trForm.value)
    }
    trModalOpen.value = false
    await selectTemplate(template.value.id)
    toast.success(t('Saved') || 'Saved')
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    saving.value = false
  }
}

async function deleteTransition(transitionId: number) {
  if (!confirm(t('ConfirmDelete') || 'Delete this transition?')) return
  try {
    await WorkflowDesignerService.deleteTransition(transitionId)
    if (template.value) await selectTemplate(template.value.id)
    toast.success(t('Deleted') || 'Deleted')
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  }
}

onMounted(loadAll)
</script>

<style scoped>
.wfd-page { display: flex; flex-direction: column; gap: 16px; }
.btn-primary, .btn-secondary, .btn-danger {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 9px 16px; border-radius: 8px; font-size: 13px; font-weight: 600;
  cursor: pointer; font-family: inherit; transition: all 0.15s; border: 1px solid transparent;
}
.btn-primary { background: #006d4b; color: #fff; }
.btn-primary:hover:not(:disabled) { background: #005339; }
.btn-primary:disabled { opacity: 0.55; cursor: not-allowed; }
.btn-secondary { background: #fff; color: #374a41; border-color: #d4e0da; }
.btn-secondary:hover { background: #f4f8f6; }
.btn-danger { background: #fff; color: #b91c1c; border-color: #fca5a5; }
.btn-danger:hover { background: #fef2f2; border-color: #ef4444; }

.loading-state { display: flex; justify-content: center; padding: 60px 0; }
.spinner { width: 32px; height: 32px; border: 3px solid #e4ede8; border-top-color: #006d4b; border-radius: 50%; animation: spin 0.8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

.wfd-layout { display: grid; grid-template-columns: 280px 1fr; gap: 16px; }
@media (max-width: 900px) { .wfd-layout { grid-template-columns: 1fr; } }

.wfd-aside {
  background: #fff; border: 1px solid #e4ede8; border-radius: 12px;
  padding: 14px; height: fit-content;
}
.wfd-aside-title { margin: 0 0 10px; font-size: 13px; font-weight: 700; color: #1a2e25; text-transform: uppercase; letter-spacing: 0.5px; }
.wfd-tpl-card {
  padding: 10px 12px; border: 1px solid #e4ede8; border-radius: 8px;
  margin-bottom: 8px; cursor: pointer; transition: all 0.15s;
}
.wfd-tpl-card:hover { background: #f4f8f6; border-color: #c8ddd3; }
.wfd-tpl-card.active { background: #eef5f1; border-color: #006d4b; }
.wfd-tpl-name { font-size: 13.5px; font-weight: 700; color: #1a2e25; }
.wfd-tpl-meta { display: flex; gap: 6px; align-items: center; flex-wrap: wrap; margin-top: 5px; }
.wfd-tpl-tag {
  font-size: 10px; font-weight: 700; padding: 2px 7px; border-radius: 8px;
  text-transform: uppercase; letter-spacing: 0.3px;
}
.tag-global { background: rgba(0, 109, 75, 0.1); color: #006d4b; }
.tag-scope  { background: #dbeafe; color: #1d4ed8; }
.tag-inactive { background: #f1f5f9; color: #64748b; }
.wfd-tpl-counts { font-size: 11px; color: #6b8a7d; margin-inline-start: auto; }

.wfd-main { background: #fff; border: 1px solid #e4ede8; border-radius: 12px; padding: 18px; min-height: 400px; }
.empty-state { display: flex; flex-direction: column; align-items: center; padding: 80px 20px; gap: 12px; color: #93afa4; }

.wfd-tpl-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 12px; padding-bottom: 14px; border-bottom: 1px solid #e4ede8; margin-bottom: 14px; }
.wfd-tpl-header h2 { margin: 0; font-size: 18px; font-weight: 800; color: #1a2e25; }
.wfd-tpl-sub { font-size: 12px; color: #6b8a7d; margin-top: 4px; }
.wfd-tpl-actions { display: flex; gap: 8px; }

.wfd-tabs { display: flex; gap: 4px; margin-bottom: 14px; border-bottom: 2px solid #e4ede8; }
.wfd-tab {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 9px 16px; background: transparent; border: none; border-bottom: 2px solid transparent;
  margin-bottom: -2px; font-size: 13px; font-weight: 600; color: #6b8a7d; cursor: pointer; font-family: inherit;
}
.wfd-tab:hover { color: #006d4b; }
.wfd-tab.active { color: #006d4b; border-bottom-color: #006d4b; }

.wfd-pane-head { display: flex; justify-content: flex-end; margin-bottom: 12px; }

.wfd-table { width: 100%; border-collapse: collapse; }
.wfd-table thead th {
  text-align: start; padding: 8px 10px; font-size: 11px; font-weight: 700;
  color: #6b8a7d; text-transform: uppercase; letter-spacing: 0.4px;
  border-bottom: 1px solid #e4ede8; background: #f7faf8;
}
.wfd-table tbody td { padding: 9px 10px; border-bottom: 1px solid #f1f5f9; font-size: 13px; color: #1a2e25; }
.wfd-step-order { font-weight: 700; color: #006d4b; width: 40px; }

.wfd-actor-badge {
  display: inline-block; font-size: 11px; font-weight: 600;
  padding: 2px 8px; border-radius: 8px; background: rgba(0, 109, 75, 0.08);
  color: #006d4b; margin-inline-end: 6px;
}
.wfd-actor-target { font-size: 12px; color: #475569; }

.wfd-flag {
  display: inline-block; font-size: 10px; font-weight: 700;
  padding: 1px 7px; border-radius: 7px; margin-inline-end: 4px;
  text-transform: uppercase; letter-spacing: 0.3px;
}
.flag-terminal { background: #dcfce7; color: #15803d; }
.flag-auto     { background: #fef3c7; color: #92400e; }

.wfd-action-badge {
  display: inline-block; font-size: 11px; font-weight: 700;
  padding: 2px 8px; border-radius: 8px;
}
.action-1 { background: #e0e7ff; color: #4338ca; }
.action-2 { background: #dcfce7; color: #15803d; }
.action-3 { background: #fee2e2; color: #b91c1c; }
.action-4 { background: #f1f5f9; color: #475569; }

.wfd-arrow { color: #6b8a7d; }
.wfd-actions-cell { text-align: end; white-space: nowrap; }

.icon-btn {
  background: transparent; border: 1px solid #e4ede8; border-radius: 6px;
  padding: 5px 7px; cursor: pointer; color: #475569; transition: all 0.15s; margin-inline-start: 4px;
}
.icon-btn:hover { background: #f4f8f6; color: #006d4b; }
.icon-btn-danger:hover { background: #fef2f2; color: #b91c1c; border-color: #fca5a5; }

.empty-inline { padding: 24px; text-align: center; color: #93afa4; font-size: 13px; }

/* Form modal */
.wfd-form { display: flex; flex-direction: column; gap: 12px; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.form-field { display: flex; flex-direction: column; gap: 4px; }
.form-field label { font-size: 12px; font-weight: 600; color: #374a41; }
.form-input {
  padding: 10px 12px; border: 1.5px solid #d4e0da; border-radius: 8px;
  font-size: 14px; background: #f7faf8; color: #1a2e25; font-family: inherit;
}
.form-input:focus { outline: none; border-color: #006d4b; background: #fff; box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1); }
.form-actions { display: flex; justify-content: flex-end; gap: 8px; padding-top: 8px; border-top: 1px solid #e4ede8; margin-top: 6px; }

.wfd-section-title {
  font-size: 12px; font-weight: 700; color: #006d4b; text-transform: uppercase;
  letter-spacing: 0.4px; padding-top: 6px; border-top: 1px dashed #e4ede8; margin-top: 4px;
}

.checkbox-row { display: flex; gap: 14px; padding-top: 6px; }
.cb { display: inline-flex; align-items: center; gap: 5px; font-size: 13px; color: #1a2e25; font-weight: 500; cursor: pointer; }
.cb input { accent-color: #006d4b; }

.toggle { display: inline-flex; align-items: center; gap: 6px; padding-top: 6px; }
.toggle input { accent-color: #006d4b; }

@media (max-width: 768px) {
  .form-row { grid-template-columns: 1fr; }
}
</style>

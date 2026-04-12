<template>
  <div class="space-y-5">
    <!-- Page Header -->
    <PageHeader :title="$t('MomTemplates')" :subtitle="$t('ManageMomTemplates')" :breadcrumbs="[{ label: 'Settings', to: '/settings' }]">
      <template #actions>
        <button class="btn-clean primary" @click="openAddTemplate">
          <Icon icon="mdi:plus" class="w-4 h-4" />
          {{ $t('AddTemplate') }}
        </button>
      </template>
    </PageHeader>

    <!-- Grid Container -->
    <div class="mg-container">
      <!-- Loading -->
      <div v-if="loading" class="mg-state">
        <Icon icon="mdi:loading" class="w-6 h-6 animate-spin" style="color: #008080" />
      </div>

      <template v-else>
        <!-- Search & Filter Bar -->
        <div class="mg-toolbar">
          <div class="mg-search">
            <Icon icon="mdi:magnify" class="w-4 h-4" style="color: #9ca3af" />
            <input v-model="searchQuery" type="text" :placeholder="$t('SearchTemplates')" />
          </div>
          <div class="mt-filters">
            <CustomSelect
              v-model="filterBranchId"
              :options="branchOptions"
              :placeholder="$t('AllBranches')"
              class="w-44"
              @change="loadTemplates"
            />
            <CustomSelect
              v-model="filterType"
              :options="filterTypeOptions"
              :placeholder="$t('AllTypes')"
              class="w-36"
            />
            <span class="mg-count">{{ filteredTemplates.length }}/{{ templates.length }}</span>
          </div>
        </div>

        <!-- Table -->
        <template v-if="filteredTemplates.length > 0">
          <div class="mg-table-wrap">
            <table class="data-table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>{{ $t('TemplateName') }}</th>
                  <th>{{ $t('TemplateType') }}</th>
                  <th>{{ $t('Branch') }}</th>
                  <th>{{ $t('Status') }}</th>
                  <th>{{ $t('Default') }}</th>
                  <th>{{ $t('Actions') }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(template, index) in filteredTemplates" :key="template.id">
                  <td><span class="mg-id">{{ index + 1 }}</span></td>
                  <td>
                    <span class="mg-title">{{ template.nameAr }}</span>
                    <span class="block text-xs text-zinc-400" dir="ltr">{{ template.nameEn }}</span>
                  </td>
                  <td>
                    <span :class="['mg-pill', template.templateType === 1 ? 'pending-mom' : 'approved-mom']">
                      {{ template.templateType === 1 ? ($t('InitialMinutes')) : ($t('FinalMinutes')) }}
                    </span>
                  </td>
                  <td>{{ template.branchId ? template.branchNameAr : ($t('SystemDefault')) }}</td>
                  <td>
                    <span :class="['mg-pill', template.isActive ? 'completed' : 'cancelled']">
                      {{ template.isActive ? ($t('Active')) : ($t('Inactive')) }}
                    </span>
                  </td>
                  <td>
                    <Icon v-if="template.isDefault" icon="mdi:star" class="w-5 h-5 text-amber-500" />
                    <span v-else class="text-zinc-300">-</span>
                  </td>
                  <td>
                    <div class="mg-actions">
                      <button class="mg-act" :title="$t('Edit')" @click="openEditTemplate(template)">
                        <Icon icon="mdi:pencil-outline" class="w-4 h-4" />
                      </button>
                      <button class="mg-act" :title="$t('Duplicate')" @click="duplicateTemplate(template)">
                        <Icon icon="mdi:content-copy" class="w-4 h-4" />
                      </button>
                      <button v-if="template.branchId !== null" class="mg-act danger" :title="$t('Delete')" @click="deleteTemplate(template.id)">
                        <Icon icon="mdi:trash-can-outline" class="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>

        <!-- Empty -->
        <div v-else class="mg-state">
          <Icon icon="mdi:file-document-remove-outline" class="w-12 h-12" style="color: #ccc" />
          <p>{{ $t('NoTemplatesFound') }}</p>
        </div>
      </template>
    </div>

    <!-- Add/Edit Template Dialog -->
    <Modal
      v-model="showTemplateDialog"
      :title="selectedTemplate ? ($t('EditTemplate')) : ($t('AddTemplate'))"
      :icon="selectedTemplate ? 'mdi:file-document-edit' : 'mdi:file-document-plus'"
      size="full"
      class="mom-template-modal"
    >
      <div class="template-dialog-content">
        <!-- Tabs Navigation -->
        <div class="tabs-nav">
          <button
            v-for="tab in tabs"
            :key="tab.key"
            class="tab-btn"
            :class="{ active: activeTab === tab.key }"
            @click="activeTab = tab.key"
          >
            <Icon :icon="tab.icon" class="w-4 h-4" />
            {{ tab.label }}
          </button>
        </div>

        <!-- Tab Content -->
        <div class="tab-content">
          <!-- General Tab -->
          <div v-show="activeTab === 'general'" class="tab-panel">
            <div class="form-section">
              <h4 class="section-title">
                <Icon icon="mdi:information" class="w-4 h-4" />
                {{ $t('BasicInfo') }}
              </h4>
              <div class="form-grid">
                <div class="form-group">
                  <label class="form-label">
                    <Icon icon="mdi:abjad-arabic" class="w-4 h-4" />
                    {{ $t('NameAr') }} <span class="required">*</span>
                  </label>
                  <input v-model="formData.nameAr" type="text" class="form-input" dir="rtl" required />
                </div>
                <div class="form-group">
                  <label class="form-label">
                    <Icon icon="mdi:alphabetical" class="w-4 h-4" />
                    {{ $t('NameEn') }} <span class="required">*</span>
                  </label>
                  <input v-model="formData.nameEn" type="text" class="form-input" dir="ltr" required />
                </div>
              </div>
              <div class="form-grid">
                <div class="form-group">
                  <CustomSelect
                    v-model="formData.templateType"
                    :options="templateTypeOptions"
                    :label="$t('TemplateType')"
                    value-key="value"
                    label-key="label"
                  />
                </div>
                <div class="form-group">
                  <CustomSelect
                    v-model="formData.branchId"
                    :options="branchOptionsWithNull"
                    :label="$t('Branch')"
                    :placeholder="$t('SystemDefault')"
                    value-key="value"
                    label-key="label"
                    clearable
                  />
                </div>
              </div>
            </div>

            <div class="form-section">
              <h4 class="section-title">
                <Icon icon="mdi:toggle-switch" class="w-4 h-4" />
                {{ $t('Settings') }}
              </h4>
              <div class="toggles-row">
                <label class="toggle-item">
                  <span class="toggle-label">{{ $t('Active') }}</span>
                  <label class="toggle-switch">
                    <input v-model="formData.isActive" type="checkbox" />
                    <span class="toggle-track"></span>
                  </label>
                </label>
                <label class="toggle-item">
                  <span class="toggle-label">{{ $t('SetAsDefault') }}</span>
                  <label class="toggle-switch">
                    <input v-model="formData.isDefault" type="checkbox" />
                    <span class="toggle-track"></span>
                  </label>
                </label>
              </div>
            </div>
          </div>

          <!-- HTML Template Tab -->
          <div v-show="activeTab === 'html'" class="tab-panel">
            <div class="form-section">
              <h4 class="section-title">
                <Icon icon="mdi:code-tags" class="w-4 h-4" />
                {{ $t('HtmlTemplate') }}
              </h4>
              <p class="section-description">
                {{ $t('HtmlTemplateDescription') }}
              </p>
              <div class="placeholders-list">
                <code v-for="placeholder in htmlPlaceholders" :key="placeholder" class="placeholder-tag">
                  {{ placeholder }}
                </code>
              </div>
            </div>
            <div class="html-editor-wrapper">
              <textarea
                v-model="formData.htmlTemplate"
                class="html-editor"
                dir="ltr"
                :placeholder="$t('EnterHtmlTemplate')"
                rows="20"
              ></textarea>
            </div>
          </div>

        </div>
      </div>

      <template #footer>
        <Button variant="outline" @click="showTemplateDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="primary" :loading="saving" @click="saveTemplate">
          <Icon v-if="!saving" icon="mdi:check" class="w-4 h-4" />
          {{ selectedTemplate ? ($t('SaveChanges')) : ($t('Add')) }}
        </Button>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import Modal from '@/components/ui/Modal.vue'
import CustomSelect from '@/components/ui/CustomSelect.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import MomTemplateService from '@/services/MomTemplateService'
import { useToast } from '@/composables/useToast'
import { useConfirm } from '@/composables/useConfirm'
import { useI18n } from 'vue-i18n'
import type { MomTemplateListItem, MomTemplateConfig, BranchListItem } from '@/types/momTemplate'
import { defaultMomTemplateConfig } from '@/types/momTemplate'

const { toast } = useToast()
const { confirm } = useConfirm()
const { t } = useI18n()

// State
const loading = ref(false)
const saving = ref(false)
const showTemplateDialog = ref(false)
const activeTab = ref('general')
const selectedTemplate = ref<MomTemplateListItem | null>(null)
const templates = ref<MomTemplateListItem[]>([])
const branches = ref<BranchListItem[]>([])
const filterBranchId = ref<number | null>(null)
const filterType = ref<number | null>(null)
const searchQuery = ref('')

// Form Data
const formData = ref({
  nameAr: '',
  nameEn: '',
  templateType: 1,
  branchId: null as number | null,
  isActive: true,
  isDefault: false,
  config: JSON.parse(JSON.stringify(defaultMomTemplateConfig)) as MomTemplateConfig,
  htmlTemplate: '' as string | null
})

// Tabs
const tabs = [
  { key: 'general', label: t('General'), icon: 'mdi:information' },
  { key: 'html', label: t('HtmlTemplate'), icon: 'mdi:code-tags' }
]

// Options
const templateTypeOptions = [
  { value: 1, label: t('InitialMinutes') },
  { value: 2, label: t('FinalMinutes') }
]

const filterTypeOptions = [
  { value: null, label: t('AllTypes') },
  { value: 1, label: t('InitialMinutes') },
  { value: 2, label: t('FinalMinutes') }
]

// Computed
const branchOptions = computed(() => [
  { value: null, label: t('AllBranches') },
  ...branches.value.map(b => ({ value: b.id, label: b.nameAr }))
])

const branchOptionsWithNull = computed(() => [
  { value: null, label: t('SystemDefault') },
  ...branches.value.map(b => ({ value: b.id, label: b.nameAr }))
])

const filteredTemplates = computed(() => {
  let result = templates.value

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(t =>
      t.nameAr.toLowerCase().includes(query) ||
      t.nameEn.toLowerCase().includes(query)
    )
  }

  if (filterType.value !== null) {
    result = result.filter(t => t.templateType === filterType.value)
  }

  return result
})

const initialTemplatesCount = computed(() => templates.value.filter(t => t.templateType === 1).length)
const finalTemplatesCount = computed(() => templates.value.filter(t => t.templateType === 2).length)
const activeTemplatesCount = computed(() => templates.value.filter(t => t.isActive).length)

// HTML Template Placeholders
const htmlPlaceholders = [
  '{{Title}}',
  '{{Date}}',
  '{{Time}}',
  '{{Location}}',
  '{{MeetingNumber}}',
  '{{ReferenceNumber}}',
  '{{CommitteeName}}',
  '{{Attendees}}',
  '{{Agenda}}',
  '{{Recommendations}}',
  '{{Voting}}',
  '{{Summary}}',
  '{{Signatures}}',
  '{{Logo}}',
  '{{Footer}}'
]

// Methods
const loadTemplates = async () => {
  loading.value = true
  try {
    const response: any = await MomTemplateService.list(filterBranchId.value || undefined)
    // Handle nested response: response.data.data or response.data or response
    const apiResponse = response.data || response
    const data = apiResponse.data || apiResponse
    templates.value = Array.isArray(data) ? data : []
  } catch (error) {
    console.error('Failed to load templates:', error)
    toast.error(t('ErrorOccurred'))
    templates.value = []
  } finally {
    loading.value = false
  }
}

const loadBranches = async () => {
  try {
    const response: any = await MomTemplateService.getBranches()
    const apiResponse = response.data || response
    const data = apiResponse.data || apiResponse
    branches.value = Array.isArray(data) ? data : []
  } catch (error) {
    console.error('Failed to load branches:', error)
    branches.value = []
  }
}

const openAddTemplate = () => {
  selectedTemplate.value = null
  activeTab.value = 'general'
  formData.value = {
    nameAr: '',
    nameEn: '',
    templateType: 1,
    branchId: null,
    isActive: true,
    isDefault: false,
    config: JSON.parse(JSON.stringify(defaultMomTemplateConfig)),
    htmlTemplate: ''
  }
  showTemplateDialog.value = true
}

const openEditTemplate = async (template: MomTemplateListItem) => {
  loading.value = true
  try {
    const response: any = await MomTemplateService.getById(template.id)
    const apiResponse = response.data || response
    const data = apiResponse.data || apiResponse
    if (data) {
      selectedTemplate.value = template
      activeTab.value = 'general'
      formData.value = {
        nameAr: data.nameAr,
        nameEn: data.nameEn,
        templateType: data.templateType,
        branchId: data.branchId,
        isActive: data.isActive,
        isDefault: data.isDefault,
        config: data.config || JSON.parse(JSON.stringify(defaultMomTemplateConfig)),
        htmlTemplate: data.htmlTemplate || ''
      }
      showTemplateDialog.value = true
    }
  } catch (error) {
    console.error('Failed to load template:', error)
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const duplicateTemplate = async (template: MomTemplateListItem) => {
  loading.value = true
  try {
    const response: any = await MomTemplateService.getById(template.id)
    const apiResponse = response.data || response
    const data = apiResponse.data || apiResponse
    if (data) {
      selectedTemplate.value = null
      activeTab.value = 'general'
      formData.value = {
        nameAr: data.nameAr + ' (نسخة)',
        nameEn: data.nameEn + ' (Copy)',
        templateType: data.templateType,
        branchId: data.branchId,
        isActive: true,
        isDefault: false,
        config: data.config || JSON.parse(JSON.stringify(defaultMomTemplateConfig)),
        htmlTemplate: data.htmlTemplate || ''
      }
      showTemplateDialog.value = true
    }
  } catch (error) {
    console.error('Failed to duplicate template:', error)
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const saveTemplate = async () => {
  if (!formData.value.nameAr || !formData.value.nameEn) {
    toast.error(t('RequiredFieldsMissing'))
    return
  }

  saving.value = true
  try {
    if (selectedTemplate.value) {
      await MomTemplateService.update(selectedTemplate.value.id, {
        id: selectedTemplate.value.id,
        ...formData.value
      })
      toast.success(t('UpdateSuccess'))
      showTemplateDialog.value = false
      await loadTemplates()
    } else {
      await MomTemplateService.create(formData.value)
      toast.success(t('AddSuccess'))
      showTemplateDialog.value = false
      await loadTemplates()
    }
  } catch (error: any) {
    console.error('Failed to save template:', error)
    const message = error?.response?.data?.message || error?.message || t('ErrorOccurred')
    toast.error(message)
  } finally {
    saving.value = false
  }
}

const deleteTemplate = async (id: number) => {
  const confirmed = await confirm({
    title: t('DeleteTemplate'),
    message: t('DeleteTemplateConfirm'),
    type: 'danger',
    confirmText: t('Delete'),
    cancelText: t('Cancel')
  })
  if (!confirmed) return

  loading.value = true
  try {
    const response: any = await MomTemplateService.delete(id)
    const apiResponse = response.data || response
    if (apiResponse.success) {
      toast.success(t('DeleteSuccess'))
      await loadTemplates()
    } else {
      toast.error(apiResponse.message || t('CannotDeleteSystemTemplate'))
    }
  } catch (error) {
    console.error('Failed to delete template:', error)
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

// Lifecycle
onMounted(async () => {
  await Promise.all([loadTemplates(), loadBranches()])
})
</script>

<style scoped>
/* Toolbar */
.mg-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid #eaeaea;
  flex-wrap: wrap;
  gap: 10px;
}

.mg-search {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 1;
  max-width: 280px;
}

.mg-search input {
  flex: 1;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 6px 10px;
  font-size: 13px;
  outline: none;
  transition: border-color 0.15s;
}

.mg-search input:focus {
  border-color: #006d4b;
  box-shadow: 0 0 0 2px rgba(0, 109, 75, 0.1);
}

.mt-filters {
  display: flex;
  align-items: center;
  gap: 8px;
}

.mg-count {
  font-size: 0.75rem;
  color: #94a3b8;
  white-space: nowrap;
  background: #f1f5f9;
  padding: 4px 10px;
  border-radius: 12px;
  font-weight: 500;
}

/* Dialog */
.template-dialog-content {
  @apply space-y-4;
}

.tabs-nav {
  @apply flex flex-wrap gap-2 p-1 bg-zinc-100 rounded-xl;
}

.tab-btn {
  @apply flex items-center gap-2 px-4 py-2 text-sm font-medium rounded-lg transition-all;
  @apply text-zinc-600 hover:text-zinc-900;
}

.tab-btn.active {
  background: #006d4b;
  box-shadow: none;
  @apply text-white;
}

.tab-content {
  @apply min-h-[400px];
}

.tab-panel {
  @apply space-y-6;
}

.form-section {
  @apply space-y-4;
}

.section-title {
  @apply flex items-center gap-2 text-sm font-semibold text-zinc-700 pb-2 border-b border-gray-200;
}

.form-grid {
  @apply grid grid-cols-1 md:grid-cols-2 gap-4;
}

.form-group {
  @apply space-y-1.5;
}

.form-label {
  @apply flex items-center gap-2 text-sm font-medium text-zinc-700;
}

.form-label .required {
  @apply text-red-500;
}

.form-input {
  @apply w-full px-4 py-2.5 border border-gray-200 rounded-xl text-sm;
  @apply focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary;
}

/* Toggles */
.toggles-row {
  @apply flex flex-wrap gap-6;
}

.toggle-item {
  @apply flex items-center gap-3;
}

.toggle-item.standalone {
  @apply p-4 bg-zinc-50 rounded-xl justify-between;
}

.toggle-label {
  @apply flex items-center gap-2 text-sm font-medium text-zinc-700;
}

.toggle-switch {
  @apply relative inline-flex cursor-pointer;
}

.toggle-switch input {
  @apply sr-only;
}

.toggle-track {
  @apply w-11 h-6 bg-zinc-300 rounded-full transition-colors duration-200;
  @apply after:content-[''] after:absolute after:top-[2px] after:start-[2px];
  @apply after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all after:shadow-sm;
}

.toggle-switch input:checked + .toggle-track {
  @apply bg-primary;
}

.toggle-switch input:checked + .toggle-track::after {
  transform: translateX(20px);
}

[dir="rtl"] .toggle-switch input:checked + .toggle-track::after {
  transform: translateX(-20px);
}

/* HTML Tab */
.section-description {
  @apply text-sm text-zinc-500 mb-3;
}

.placeholders-list {
  @apply flex flex-wrap gap-2 mb-4;
}

.placeholder-tag {
  @apply px-2 py-1 bg-blue-50 text-blue-700 rounded text-xs font-mono;
}

.html-editor-wrapper {
  @apply relative;
}

.html-editor {
  @apply w-full px-4 py-3 border border-gray-200 rounded-xl text-sm font-mono;
  @apply focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary;
  @apply bg-zinc-50 min-h-[400px] resize-y;
  line-height: 1.6;
}

/* Responsive */
@media (max-width: 768px) {
  .toolbar {
    @apply flex-col items-stretch;
  }

  .toolbar-start, .toolbar-end {
    @apply flex-col items-stretch;
  }

  .search-input {
    @apply w-full;
  }

  .tabs-nav {
    @apply overflow-x-auto;
  }

  .tab-btn {
    @apply whitespace-nowrap;
  }
}
</style>

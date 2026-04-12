<template>
  <Modal
    :model-value="modelValue"
    :title="isEdit ? ($t('Edit')) : ($t('Add'))"
    size="2xl"
    @update:model-value="$emit('update:modelValue', $event)"
  >
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <!-- Active Switch -->
      <label class="toggle-switch">
        <input
          type="checkbox"
          v-model="form.active"
          class="toggle-input"
        >
        <span class="toggle-slider"></span>
        <span class="toggle-label">{{ $t('Active') }}</span>
      </label>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Name Arabic -->
        <Input
          v-model="form.nameAr"
          :label="$t('NameAr')"
          required
        />

        <!-- Name English -->
        <Input
          v-model="form.nameEn"
          :label="$t('NameEn')"
          required
        />

        <!-- Code -->
        <Input
          v-model="form.code"
          :label="$t('Code')"
          required
        />

        <!-- Type -->
        <Select
          v-model="form.typeId"
          :options="committeeTypes"
          item-text="name"
          item-value="id"
          :label="$t('Type')"
          :loading="loadingLookups"
          required
        />

        <!-- Parent Council/Committee -->
        <Select
          v-if="showParentOptions"
          v-model="form.parentId"
          :options="committees"
          item-text="name"
          item-value="id"
          :label="$t('ParentCouncilOrCommittee')"
          :loading="loadingLookups"
        />

        <!-- Classification -->
        <Select
          v-model="form.committeeClassificationId"
          :options="committeeClassifications"
          item-text="name"
          item-value="id"
          :label="$t('CommitteeClassification')"
          :loading="loadingLookups"
        />

        <!-- Style -->
        <Select
          v-model="form.committeeStyleId"
          :options="committeeStyles"
          item-text="name"
          item-value="id"
          :label="$t('CommitteeStyle')"
          :loading="loadingLookups"
        />

        <!-- Status -->
        <Select
          v-model="form.committeeStatusId"
          :options="committeeStatuses"
          item-text="name"
          item-value="id"
          :label="$t('CommitteeStatus')"
          :loading="loadingLookups"
        />
      </div>

      <!-- Date Row -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Start Date -->
        <DatePicker
          v-model="form.startDate"
          :label="$t('StartDate')"
          required
        />

        <!-- End Date -->
        <DatePicker
          v-model="form.endDate"
          :label="$t('EndDate')"
          required
        />
      </div>

      <!-- File Upload -->
      <div class="file-upload-wrapper">
        <label class="block text-sm font-medium text-zinc-700 mb-1">
          {{ $t('Attachment') }}
        </label>
        <!-- Show selected file -->
        <div v-if="form.file || form.fileName" class="file-preview">
          <div class="file-info">
            <Icon icon="mdi:file-document" class="file-icon" />
            <span class="file-name">{{ form.file?.name || form.fileName }}</span>
          </div>
          <button type="button" class="file-remove" @click="removeFile" :title="$t('Remove')">
            <Icon icon="mdi:close" class="w-4 h-4" />
          </button>
        </div>
        <!-- Upload zone -->
        <label v-else class="file-dropzone" :class="{ 'drag-over': isDragOver }"
          @dragover.prevent="isDragOver = true"
          @dragleave.prevent="isDragOver = false"
          @drop.prevent="handleFileDrop"
        >
          <input
            type="file"
            ref="fileInput"
            class="hidden"
            @change="handleFileSelect"
          >
          <Icon icon="mdi:cloud-upload-outline" class="dropzone-icon" />
          <span class="dropzone-text">{{ $t('DragFileOrClick') }}</span>
          <span class="dropzone-hint">{{ $t('SupportedFormats') }}</span>
        </label>
      </div>

      <!-- Has Additional Members -->
      <label class="toggle-switch">
        <input
          type="checkbox"
          v-model="form.hasAdditionalMembers"
          class="toggle-input"
        >
        <span class="toggle-slider"></span>
        <span class="toggle-label">{{ $t('AreThereAdditionalMembers') }}</span>
      </label>

      <!-- Additional Member Name -->
      <div v-if="form.hasAdditionalMembers">
        <Input
          v-model="form.additionalMemberName"
          :label="$t('AdditionalMemberName')"
          required
        />
      </div>

      <!-- Is Internal -->
      <label class="toggle-switch">
        <input type="checkbox" v-model="form.isInternal" class="toggle-input">
        <span class="toggle-slider"></span>
        <span class="toggle-label">{{ $t('IsInternal') }}</span>
      </label>

      <!-- Is Presentation Related -->
      <label class="toggle-switch">
        <input type="checkbox" v-model="form.isPresentationRelated" class="toggle-input">
        <span class="toggle-slider"></span>
        <span class="toggle-label">{{ $t('IsPresentationRelated') }}</span>
      </label>

      <!-- Description -->
      <div>
        <label class="block text-sm font-medium text-zinc-700 mb-1">
          {{ $t('Description') }}
        </label>
        <textarea
          v-model="form.description"
          rows="3"
          class="w-full px-4 py-2 border border-zinc-200 rounded-lg focus:outline-none focus:border-primary"
        />
      </div>

    </form>

    <template #footer>
      <Button variant="outline" @click="$emit('update:modelValue', false)">
        {{ $t('Cancel') }}
      </Button>
      <Button variant="primary" :loading="saving" @click="handleSubmit">
        {{ isEdit ? ($t('Update')) : ($t('Add')) }}
      </Button>
    </template>
  </Modal>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import Modal from '@/components/ui/Modal.vue'
import Button from '@/components/ui/Button.vue'
import Input from '@/components/ui/Input.vue'
import Select from '@/components/ui/Select.vue'
import DatePicker from '@/components/ui/DatePicker.vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'
import LookupsService from '@/services/LookupsService'

// Constants
const COUNCIL_TYPE_ID = '1' // CommitteeType.Council

interface Props {
  modelValue: boolean
  councilCommitteeId?: string | null
  isEdit?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  councilCommitteeId: null,
  isEdit: false
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  'saved': [data: { id: string; name: string }]
}>()

const { locale } = useI18n()

// State
const loadingLookups = ref(false)
const saving = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)
const isDragOver = ref(false)

const form = ref({
  id: null as string | null,
  nameAr: '',
  nameEn: '',
  code: '',
  description: '',
  typeId: null as string | null,
  committeeClassificationId: null as string | null,
  committeeStyleId: null as string | null,
  committeeStatusId: null as string | null,
  parentId: null as string | null,
  active: true,
  file: null as File | null,
  fileName: null as string | null,
  startDate: null as string | null,
  endDate: null as string | null,
  hasAdditionalMembers: false,
  additionalMemberName: null as string | null,
  isInternal: false,
  isPresentationRelated: false
})

// Lookups
const committeeTypes = ref<any[]>([])
const committees = ref<any[]>([])
const committeeClassifications = ref<any[]>([])
const committeeStyles = ref<any[]>([])
const committeeStatuses = ref<any[]>([])

// Computed
const showParentOptions = computed(() => {
  return form.value.typeId !== COUNCIL_TYPE_ID
})

// Watch type changes
watch(() => form.value.typeId, (newVal) => {
  if (newVal === COUNCIL_TYPE_ID) {
    form.value.parentId = null
  }
})

// Methods
const loadLookups = async () => {
  loadingLookups.value = true
  try {
    const [types, comms, classifications, styles, statuses] = await Promise.all([
      LookupsService.listCommitteeTypes(),
      LookupsService.listCommittees(),
      LookupsService.listCommitteeClassifications(),
      LookupsService.listCommitteeStyles(),
      LookupsService.listCommitteeStatuses()
    ])
    // Extract data from API response - handle both wrapped { data, success, message } and direct array
    const extractData = (response: any): any[] => {
      if (Array.isArray(response)) {
        return response
      }
      if (response?.data && Array.isArray(response.data)) {
        return response.data
      }
      return []
    }
    committeeTypes.value = extractData(types)
    committees.value = extractData(comms)
    committeeClassifications.value = extractData(classifications)
    committeeStyles.value = extractData(styles)
    committeeStatuses.value = extractData(statuses)
  } catch (error) {
    console.error('Failed to load lookups:', error)
  } finally {
    loadingLookups.value = false
  }
}

const loadCouncilCommittee = async () => {
  if (!props.councilCommitteeId) return
  try {
    const response = await CouncilCommitteesService.getCouncilCommittee(props.councilCommitteeId)
    const data = response?.data || response
    Object.assign(form.value, {
      id: data.id,
      nameAr: data.nameAr,
      nameEn: data.nameEn,
      code: data.code,
      description: data.description,
      typeId: data.typeId,
      committeeClassificationId: data.committeeClassificationId,
      committeeStyleId: data.committeeStyleId,
      committeeStatusId: data.committeeStatusId,
      parentId: data.parentId,
      active: Boolean(data.isActive ?? data.active ?? true),
      fileName: data.fileName,
      startDate: data.startDate,
      endDate: data.endDate,
      hasAdditionalMembers: data.hasAdditionalMembers,
      additionalMemberName: data.additionalMemberName,
      isInternal: data.isInternal ?? false,
      isPresentationRelated: data.isPresentationRelated ?? false
    })
  } catch (error) {
    console.error('Failed to load council/committee:', error)
  }
}

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    form.value.file = target.files[0]
  }
}

const handleFileDrop = (event: DragEvent) => {
  isDragOver.value = false
  if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
    form.value.file = event.dataTransfer.files[0]
  }
}

const removeFile = () => {
  form.value.file = null
  form.value.fileName = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

const handleSubmit = async () => {
  // Basic validation
  if (!form.value.nameAr || !form.value.nameEn || !form.value.code || !form.value.typeId) {
    return
  }

  if (form.value.hasAdditionalMembers && !form.value.additionalMemberName) {
    return
  }

  saving.value = true
  try {
    // Helper to format date to ISO string (yyyy-MM-dd)
    const formatDate = (date: string | null): string | null => {
      if (!date) return null
      const d = new Date(date)
      return d.toISOString().split('T')[0]
    }

    // Build FormData for the API - field names must match CommitteeDto
    const formData = new FormData()
    // Include ID when editing
    if (props.isEdit && props.councilCommitteeId) {
      formData.append('Id', String(props.councilCommitteeId))
    }
    formData.append('NameAr', form.value.nameAr)
    formData.append('NameEn', form.value.nameEn)
    formData.append('Code', form.value.code)
    formData.append('TypeId', String(form.value.typeId))
    // Ensure Active is properly converted - handle boolean, string, or undefined
    const isActive = form.value.active === true || form.value.active === 'true'
    formData.append('Active', isActive ? 'true' : 'false')
    formData.append('HasAdditionalMembers', form.value.hasAdditionalMembers ? 'true' : 'false')
    formData.append('IsInternal', form.value.isInternal ? 'true' : 'false')
    formData.append('IsPresentationRelated', form.value.isPresentationRelated ? 'true' : 'false')
    if (form.value.description) formData.append('Description', form.value.description)
    if (form.value.committeeClassificationId) formData.append('CommitteeClassificationId', String(form.value.committeeClassificationId))
    if (form.value.committeeStyleId) formData.append('CommitteeStyleId', String(form.value.committeeStyleId))
    if (form.value.committeeStatusId) formData.append('CommitteeStatusId', String(form.value.committeeStatusId))
    // Send ParentId even if null (for committees that should be at root or under a council)
    if (form.value.parentId !== null && form.value.parentId !== undefined && form.value.parentId !== '') {
      formData.append('ParentId', String(form.value.parentId))
    }
    const startDate = formatDate(form.value.startDate)
    const endDate = formatDate(form.value.endDate)
    if (startDate) formData.append('StartDate', startDate)
    if (endDate) formData.append('EndDate', endDate)
    if (form.value.additionalMemberName) formData.append('AdditionalMemberName', form.value.additionalMemberName)
    if (form.value.file) formData.append('File', form.value.file)

    let response
    if (props.isEdit && props.councilCommitteeId) {
      response = await CouncilCommitteesService.updateCouncilCommittee(props.councilCommitteeId, formData)
    } else {
      response = await CouncilCommitteesService.addCouncilCommittee(formData)
    }

    const result = response?.data || response
    const name = locale.value === 'ar' ? form.value.nameAr : form.value.nameEn
    emit('saved', { id: result?.id || props.councilCommitteeId, name })
    emit('update:modelValue', false)
    resetForm()
  } catch (error) {
    console.error('Failed to save council/committee:', error)
  } finally {
    saving.value = false
  }
}

const resetForm = () => {
  form.value = {
    id: null,
    nameAr: '',
    nameEn: '',
    code: '',
    description: '',
    typeId: null,
    committeeClassificationId: null,
    committeeStyleId: null,
    committeeStatusId: null,
    parentId: null,
    active: true,
    file: null,
    fileName: null,
    startDate: null,
    endDate: null,
    hasAdditionalMembers: false,
    additionalMemberName: null,
    isInternal: false,
    isPresentationRelated: false
  }
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

// Lifecycle
onMounted(async () => {
  await loadLookups()
  if (props.isEdit && props.councilCommitteeId) {
    await loadCouncilCommittee()
  }
})
</script>

<style scoped>
/* Modern Toggle Switch */
.toggle-switch {
  @apply flex items-center gap-3 cursor-pointer select-none;
}

.toggle-input {
  @apply sr-only;
}

.toggle-slider {
  @apply relative w-11 h-6 bg-zinc-200 rounded-full transition-colors duration-200 ease-in-out;
  @apply after:content-[''] after:absolute after:top-0.5 after:start-0.5;
  @apply after:bg-white after:rounded-full after:h-5 after:w-5;
  @apply after:transition-all after:duration-200 after:ease-in-out;
  @apply after:shadow-sm;
}

.toggle-input:checked + .toggle-slider {
  @apply bg-primary;
}

.toggle-input:checked + .toggle-slider::after {
  transform: translateX(1.25rem);
}

[dir="rtl"] .toggle-input:checked + .toggle-slider::after {
  transform: translateX(-1.25rem);
}

.toggle-input:focus + .toggle-slider {
  @apply ring-2 ring-primary/30;
}

.toggle-label {
  @apply text-sm font-medium text-zinc-700;
}

/* File Upload Styles */
.file-upload-wrapper {
  @apply w-full;
}

.file-dropzone {
  @apply flex items-center gap-3 px-4 py-3;
  @apply border border-dashed border-zinc-300 rounded-lg;
  @apply bg-zinc-50/50 cursor-pointer transition-all duration-200;
  @apply hover:border-primary hover:bg-primary/5;
}

.file-dropzone.drag-over {
  @apply border-primary bg-primary/10;
}

.dropzone-icon {
  @apply w-6 h-6 text-zinc-400 flex-shrink-0;
}

.file-dropzone:hover .dropzone-icon,
.file-dropzone.drag-over .dropzone-icon {
  @apply text-primary;
}

.dropzone-text {
  @apply text-sm text-zinc-600;
}

.dropzone-hint {
  @apply text-xs text-zinc-400 ms-auto;
}

.file-preview {
  @apply flex items-center justify-between gap-3 px-4 py-2.5;
  @apply bg-primary/5 border border-primary/20 rounded-lg;
}

.file-info {
  @apply flex items-center gap-2 min-w-0;
}

.file-icon {
  @apply w-5 h-5 text-primary flex-shrink-0;
}

.file-name {
  @apply text-sm text-zinc-700 truncate;
}

.file-remove {
  @apply p-1 text-zinc-400 hover:text-error hover:bg-error/10 rounded transition-colors;
}
</style>

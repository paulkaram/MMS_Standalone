<template>
  <div class="space-y-5">
    <!-- Page Header -->
    <PageHeader :title="$t('VotingTypeSettings')" :subtitle="$t('ManageVotingTypes')" :breadcrumbs="[{ label: 'Settings', to: '/settings' }]">
      <template #actions>
        <button class="btn-clean primary" @click="openAddType">
          <Icon icon="mdi:plus" class="w-4 h-4" />
          {{ $t('AddVotingType') }}
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
        <!-- Search Bar -->
        <div class="mg-toolbar">
          <div class="mg-search">
            <Icon icon="mdi:magnify" class="w-4 h-4" style="color: #9ca3af" />
            <input v-model="search" type="text" :placeholder="$t('SearchVotingTypes')" />
          </div>
          <span class="mg-count">{{ filteredTypes.length }} {{ $t('Of') }} {{ votingTypes.length }}</span>
        </div>

        <!-- Table -->
        <template v-if="filteredTypes.length > 0">
          <div class="mg-table-wrap">
            <table class="data-table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>{{ $t('NameAr') }}</th>
                  <th>{{ $t('NameEn') }}</th>
                  <th>{{ $t('Status') }}</th>
                  <th>{{ $t('Order') }}</th>
                  <th>{{ $t('Actions') }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(type, index) in filteredTypes" :key="type.id">
                  <td><span class="mg-id">{{ index + 1 }}</span></td>
                  <td><span class="mg-title">{{ type.nameAr }}</span></td>
                  <td>{{ type.nameEn }}</td>
                  <td>
                    <span :class="['mg-pill', type.active ? 'completed' : 'cancelled']">
                      {{ type.active ? ($t('Active')) : ($t('Inactive')) }}
                    </span>
                  </td>
                  <td>{{ type.displayOrder || 0 }}</td>
                  <td>
                    <div class="mg-actions">
                      <button class="mg-act" :title="$t('VotingOptions')" @click="showTypeOptions(type)">
                        <Icon icon="mdi:format-list-bulleted" class="w-4 h-4" />
                      </button>
                      <button class="mg-act" :title="$t('Edit')" @click="openEditType(type)">
                        <Icon icon="mdi:pencil-outline" class="w-4 h-4" />
                      </button>
                      <button class="mg-act danger" :title="$t('Delete')" @click="confirmDeleteType(type)">
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
          <Icon icon="mdi:vote-outline" class="w-12 h-12" style="color: #ccc" />
          <p>{{ $t('NoVotingTypes') }}</p>
        </div>
      </template>
    </div>

    <!-- Add/Edit Type Dialog -->
    <Modal
      v-model="typeDialog"
      :title="isEdit ? ($t('EditVotingType')) : ($t('AddVotingType'))"
      :icon="isEdit ? 'mdi:pencil' : 'mdi:plus'"
      size="md"
    >
      <form @submit.prevent="saveType" class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-zinc-700 mb-1">{{ $t('NameAr') }}</label>
          <input v-model="formData.nameAr" type="text" dir="rtl" :placeholder="$t('EnterNameAr')" required
            class="w-full px-4 py-2.5 border border-zinc-200 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/10" />
        </div>
        <div>
          <label class="block text-sm font-medium text-zinc-700 mb-1">{{ $t('NameEn') }}</label>
          <input v-model="formData.nameEn" type="text" dir="ltr" :placeholder="$t('EnterNameEn')" required
            class="w-full px-4 py-2.5 border border-zinc-200 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/10" />
        </div>
        <div>
          <label class="block text-sm font-medium text-zinc-700 mb-1">{{ $t('DisplayOrder') }}</label>
          <input v-model.number="formData.displayOrder" type="number" min="0"
            class="w-full px-4 py-2.5 border border-zinc-200 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/10" />
        </div>
        <label class="flex items-center gap-3 cursor-pointer">
          <input type="checkbox" v-model="formData.active" class="sr-only peer" />
          <span class="relative w-11 h-6 bg-zinc-200 rounded-full peer-checked:bg-primary after:content-[''] after:absolute after:top-0.5 after:start-0.5 after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:after:translate-x-5 [dir=rtl]:peer-checked:after:-translate-x-5"></span>
          <span class="text-sm text-zinc-700">{{ $t('Active') }}</span>
        </label>
      </form>

      <template #footer>
        <Button variant="outline" @click="typeDialog = false">{{ $t('Cancel') }}</Button>
        <Button variant="primary" :loading="saving" @click="saveType">
          {{ isEdit ? ($t('SaveChanges')) : ($t('Add')) }}
        </Button>
      </template>
    </Modal>

    <!-- Voting Options Dialog -->
    <Modal
      v-model="optionsDialog"
      :title="($t('VotingOptions')) + (selectedType ? ` - ${selectedType.nameAr}` : '')"
      icon="mdi:format-list-bulleted"
      size="lg"
    >
      <div class="vt-options">
        <!-- Add Option Form -->
        <div class="vt-add-row">
          <input v-model="newOption.nameAr" type="text" dir="rtl" :placeholder="$t('OptionNameAr')"
            class="flex-1 px-3 py-2 border border-zinc-200 rounded-lg text-sm focus:outline-none focus:border-primary" />
          <input v-model="newOption.nameEn" type="text" dir="ltr" :placeholder="$t('OptionNameEn')"
            class="flex-1 px-3 py-2 border border-zinc-200 rounded-lg text-sm focus:outline-none focus:border-primary" />
          <Button variant="primary" icon-left="mdi:plus" :loading="addingOption" @click="addOption">{{ $t('Add') }}</Button>
        </div>

        <!-- Options List -->
        <div v-if="loadingOptions" class="mg-state" style="padding: 40px 0">
          <Icon icon="mdi:loading" class="w-6 h-6 animate-spin" style="color: #008080" />
        </div>
        <div v-else-if="votingOptions.length === 0" class="mg-state" style="padding: 40px 0">
          <Icon icon="mdi:format-list-bulleted-type" class="w-10 h-10" style="color: #ccc" />
          <p>{{ $t('NoOptions') }}</p>
        </div>
        <div v-else class="vt-list">
          <div v-for="option in votingOptions" :key="option.id" class="vt-item">
            <Icon icon="mdi:checkbox-marked-circle" class="w-4 h-4" style="color: #006d4b" />
            <div class="flex-1 min-w-0">
              <span class="block text-sm font-medium text-zinc-800">{{ option.nameAr }}</span>
              <span class="block text-xs text-zinc-500" dir="ltr">{{ option.nameEn }}</span>
            </div>
            <button class="mg-act danger" :title="$t('Delete')" @click="confirmDeleteOption(option)">
              <Icon icon="mdi:trash-can-outline" class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>

      <template #footer>
        <Button variant="outline" @click="optionsDialog = false">{{ $t('Close') }}</Button>
      </template>
    </Modal>

    <!-- Confirm Delete Type Dialog -->
    <Modal v-model="deleteTypeDialog" :title="$t('DeleteVotingType')" icon="mdi:vote-outline" variant="danger" size="sm">
      <div class="text-center py-4">
        <Icon icon="mdi:alert-circle" class="w-12 h-12 text-red-400 mx-auto mb-3" />
        <p class="text-zinc-600">{{ $t('ConfirmDeleteVotingType') }}</p>
        <p v-if="typeToDelete" class="font-semibold text-zinc-800 mt-1">{{ typeToDelete.nameAr }}</p>
      </div>
      <template #footer>
        <Button variant="outline" @click="deleteTypeDialog = false">{{ $t('Cancel') }}</Button>
        <Button variant="danger" :loading="deleting" @click="deleteType">{{ $t('Delete') }}</Button>
      </template>
    </Modal>

    <!-- Confirm Delete Option Dialog -->
    <Modal v-model="deleteOptionDialog" :title="$t('DeleteOption')" icon="mdi:format-list-bulleted" variant="danger" size="sm">
      <div class="text-center py-4">
        <Icon icon="mdi:alert-circle" class="w-12 h-12 text-red-400 mx-auto mb-3" />
        <p class="text-zinc-600">{{ $t('ConfirmDeleteOption') }}</p>
        <p v-if="optionToDelete" class="font-semibold text-zinc-800 mt-1">{{ optionToDelete.nameAr }}</p>
      </div>
      <template #footer>
        <Button variant="outline" @click="deleteOptionDialog = false">{{ $t('Cancel') }}</Button>
        <Button variant="danger" :loading="deletingOption" @click="deleteOption">{{ $t('Delete') }}</Button>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import Modal from '@/components/ui/Modal.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import SettingsService from '@/services/SettingsService'
import { useToast } from '@/composables/useToast'

const { showToast } = useToast()

// Types
interface VotingType {
  id: number
  nameAr: string
  nameEn: string
  active: boolean
  displayOrder: number
}

interface VotingOption {
  id: number
  nameAr: string
  nameEn: string
}

// State
const loading = ref(false)
const saving = ref(false)
const deleting = ref(false)
const loadingOptions = ref(false)
const addingOption = ref(false)
const deletingOption = ref(false)
const votingTypes = ref<VotingType[]>([])
const votingOptions = ref<VotingOption[]>([])
const search = ref('')

// Dialogs
const typeDialog = ref(false)
const optionsDialog = ref(false)
const deleteTypeDialog = ref(false)
const deleteOptionDialog = ref(false)

// Selected items
const isEdit = ref(false)
const selectedType = ref<VotingType | null>(null)
const typeToDelete = ref<VotingType | null>(null)
const optionToDelete = ref<VotingOption | null>(null)

// Form data
const formData = ref({
  nameAr: '',
  nameEn: '',
  active: true,
  displayOrder: 0
})

const newOption = ref({
  nameAr: '',
  nameEn: ''
})

// Computed
const activeTypesCount = computed(() => votingTypes.value.filter(t => t.active).length)

const filteredTypes = computed(() => {
  if (!search.value) return votingTypes.value
  const query = search.value.toLowerCase()
  return votingTypes.value.filter(type =>
    type.nameAr?.toLowerCase().includes(query) ||
    type.nameEn?.toLowerCase().includes(query)
  )
})

// Methods
const loadVotingTypes = async () => {
  loading.value = true
  try {
    const response = await SettingsService.listAllVotingTypes()
    votingTypes.value = response.data || response || []
  } catch (error) {
    console.error('Failed to load voting types:', error)
    showToast('حدث خطأ أثناء تحميل البيانات', 'error')
  } finally {
    loading.value = false
  }
}

const openAddType = () => {
  isEdit.value = false
  selectedType.value = null
  formData.value = { nameAr: '', nameEn: '', active: true, displayOrder: 0 }
  typeDialog.value = true
}

const openEditType = (type: VotingType) => {
  isEdit.value = true
  selectedType.value = type
  formData.value = { ...type }
  typeDialog.value = true
}

const saveType = async () => {
  if (!formData.value.nameAr || !formData.value.nameEn) return

  saving.value = true
  try {
    if (isEdit.value && selectedType.value) {
      await SettingsService.updateVotingType(selectedType.value.id, formData.value)
      showToast('تم تحديث نوع التصويت بنجاح', 'success')
    } else {
      await SettingsService.addVotingType(formData.value)
      showToast('تمت إضافة نوع التصويت بنجاح', 'success')
    }
    typeDialog.value = false
    await loadVotingTypes()
  } catch (error) {
    console.error('Failed to save voting type:', error)
    showToast('حدث خطأ أثناء حفظ البيانات', 'error')
  } finally {
    saving.value = false
  }
}

const confirmDeleteType = (type: VotingType) => {
  typeToDelete.value = type
  deleteTypeDialog.value = true
}

const deleteType = async () => {
  if (!typeToDelete.value) return

  deleting.value = true
  try {
    await SettingsService.deleteVotingType(typeToDelete.value.id)
    showToast('تم حذف نوع التصويت بنجاح', 'success')
    deleteTypeDialog.value = false
    typeToDelete.value = null
    await loadVotingTypes()
  } catch (error) {
    console.error('Failed to delete voting type:', error)
    showToast('فشل الحذف - قد يكون مرتبط ببيانات أخرى', 'error')
  } finally {
    deleting.value = false
  }
}

const showTypeOptions = async (type: VotingType) => {
  selectedType.value = type
  optionsDialog.value = true
  loadingOptions.value = true
  try {
    const response = await SettingsService.listVotingTypeOptions(type.id)
    votingOptions.value = response.data || response || []
  } catch (error) {
    console.error('Failed to load voting options:', error)
    showToast('حدث خطأ أثناء تحميل الخيارات', 'error')
  } finally {
    loadingOptions.value = false
  }
}

const addOption = async () => {
  if (!selectedType.value || !newOption.value.nameAr || !newOption.value.nameEn) {
    showToast('يرجى إدخال جميع الحقول المطلوبة', 'error')
    return
  }

  addingOption.value = true
  try {
    await SettingsService.addVotingTypeOption(selectedType.value.id, newOption.value)
    showToast('تمت إضافة الخيار بنجاح', 'success')
    newOption.value = { nameAr: '', nameEn: '' }
    const response = await SettingsService.listVotingTypeOptions(selectedType.value.id)
    votingOptions.value = response.data || response || []
  } catch (error) {
    console.error('Failed to add option:', error)
    showToast('حدث خطأ أثناء إضافة الخيار', 'error')
  } finally {
    addingOption.value = false
  }
}

const confirmDeleteOption = (option: VotingOption) => {
  optionToDelete.value = option
  deleteOptionDialog.value = true
}

const deleteOption = async () => {
  if (!optionToDelete.value || !selectedType.value) return

  deletingOption.value = true
  try {
    await SettingsService.deleteVotingTypeOption(optionToDelete.value.id)
    showToast('تم حذف الخيار بنجاح', 'success')
    deleteOptionDialog.value = false
    optionToDelete.value = null
    const response = await SettingsService.listVotingTypeOptions(selectedType.value.id)
    votingOptions.value = response.data || response || []
  } catch (error) {
    console.error('Failed to delete option:', error)
    showToast('حدث خطأ أثناء حذف الخيار', 'error')
  } finally {
    deletingOption.value = false
  }
}

// Lifecycle
onMounted(() => {
  loadVotingTypes()
})
</script>

<style scoped>
/* Search toolbar */
.mg-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid #eaeaea;
}

.mg-search {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 1;
  max-width: 320px;
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

.mg-count {
  font-size: 0.8rem;
  color: #6a7a7a;
}

/* Voting options dialog */
.vt-options {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.vt-add-row {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px;
  background: #f8fafc;
  border: 1px solid #e6eaef;
  border-radius: 10px;
}

.vt-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 400px;
  overflow-y: auto;
}

.vt-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  background: #fff;
  border: 1px solid #e6eaef;
  border-radius: 8px;
  transition: border-color 0.15s;
}

.vt-item:hover {
  border-color: #d1d5db;
}

@media (max-width: 640px) {
  .vt-add-row {
    flex-direction: column;
  }
}
</style>

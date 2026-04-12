<template>
  <div class="space-y-6">
    <div class="page-header">
      <h1 class="page-title">{{ $t('TasksConfiguration') }}</h1>
    </div>

    <Card :loading="loading">
      <!-- Add Mapping Form -->
      <div class="p-4 border-b border-zinc-200">
        <form @submit.prevent="addMapping" class="space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4">
            <Select
              v-model="selectedSource"
              :options="connectionOptions"
              :label="$t('SelectDatasource')"
              required
              @update:modelValue="onSourceChange"
            />
            <Select
              v-model="selectedTable"
              :options="tableOptions"
              :label="$t('Tables')"
              :disabled="!tables.length"
              required
              @update:modelValue="onTableChange"
            />
            <Select
              v-model="selectedTextField"
              :options="fieldOptions"
              :label="$t('TextField')"
              :disabled="!fields.length"
              required
            />
            <Select
              v-model="selectedValueField"
              :options="fieldOptions"
              :label="$t('ValueField')"
              :disabled="!fields.length"
              required
            />
            <Input
              v-model="selectedNameAr"
              :label="$t('NameAr')"
              required
            />
            <Input
              v-model="selectedNameEn"
              :label="$t('NameEn')"
              required
            />
          </div>
          <div class="flex items-center justify-between">
            <label class="flex items-center gap-2">
              <input
                v-model="isDocument"
                type="checkbox"
                class="rounded border-zinc-300 text-primary focus:ring-primary"
              />
              {{ $t('IsDocumentMapping') }}
            </label>
            <Button variant="primary" icon-left="mdi:plus" @click="addMapping">
              {{ $t('Add') }}
            </Button>
          </div>
        </form>
      </div>

      <!-- Mappings Table -->
      <div class="p-4">
        <h3 class="text-lg font-semibold mb-4">{{ $t('TablesLinkedToTasks') }}</h3>
        <DataTable
          :columns="columns"
          :data="taskMappings"
          :loading="loading"
        >
          <template #cell-isDocument="{ row }">
            <Icon
              :icon="row.isDocument ? 'mdi:check' : 'mdi:close'"
              :class="row.isDocument ? 'text-success' : 'text-error'"
              class="w-5 h-5"
            />
          </template>

          <template #cell-options="{ row }">
            <Button
              variant="ghost"
              size="sm"
              icon-left="mdi:trash-can-outline"
              class="text-error"
              @click="removeMapping(row.id)"
            />
          </template>
        </DataTable>
      </div>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'
import Button from '@/components/ui/Button.vue'
import Input from '@/components/ui/Input.vue'
import Select from '@/components/ui/Select.vue'
import DataTable from '@/components/ui/DataTable.vue'
import LookupsService, { type DataSourceLookup } from '@/services/LookupsService'
import TaskMappingService, { type TaskMapping } from '@/services/TaskMappingService'
import { useToast } from '@/composables/useToast'
import { useConfirm } from '@/composables/useConfirm'
import { useI18n } from 'vue-i18n'

const { toast } = useToast()
const { confirm } = useConfirm()
const { t } = useI18n()

const loading = ref(false)

const connections = ref<DataSourceLookup[]>([])
const tables = ref<string[]>([])
const fields = ref<string[]>([])
const taskMappings = ref<TaskMapping[]>([])

const selectedSource = ref<number | null>(null)
const selectedTable = ref<string | null>(null)
const selectedTextField = ref<string | null>(null)
const selectedValueField = ref<string | null>(null)
const selectedNameAr = ref('')
const selectedNameEn = ref('')
const isDocument = ref(false)

const columns = [
  { key: 'id', label: t('SourceId') },
  { key: 'tableName', label: t('TableName') },
  { key: 'tableNameAr', label: t('NameAr') },
  { key: 'tableNameEn', label: t('NameEn') },
  { key: 'textField', label: t('TextField') },
  { key: 'valueField', label: t('ValueField') },
  { key: 'isDocument', label: t('Document') },
  { key: 'options', label: t('Options') }
]

const connectionOptions = computed(() =>
  connections.value.map(c => ({ value: c.id, label: c.name }))
)

const tableOptions = computed(() =>
  tables.value.map(t => ({ value: t, label: t }))
)

const fieldOptions = computed(() =>
  fields.value.map(f => ({ value: f, label: f }))
)

const loadConnections = async () => {
  loading.value = true
  try {
    connections.value = await LookupsService.listDatasources()
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const loadMappings = async () => {
  loading.value = true
  try {
    taskMappings.value = await TaskMappingService.listMappings()
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const onSourceChange = async (sourceId: number | null) => {
  tables.value = []
  fields.value = []
  selectedTable.value = null
  selectedTextField.value = null
  selectedValueField.value = null

  if (!sourceId) return

  loading.value = true
  try {
    tables.value = await LookupsService.listDataTables(sourceId)
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const onTableChange = async (tableName: string | null) => {
  fields.value = []
  selectedTextField.value = null
  selectedValueField.value = null

  if (!tableName || !selectedSource.value) return

  loading.value = true
  try {
    fields.value = await LookupsService.listDataFields(selectedSource.value, tableName)
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const addMapping = async () => {
  if (!selectedSource.value || !selectedTable.value || !selectedTextField.value ||
      !selectedValueField.value || !selectedNameAr.value || !selectedNameEn.value) {
    toast.error(t('RequiredField'))
    return
  }

  loading.value = true
  try {
    await TaskMappingService.addMapping({
      dataSourceId: selectedSource.value,
      tableName: selectedTable.value,
      tableNameAr: selectedNameAr.value,
      tableNameEn: selectedNameEn.value,
      textField: selectedTextField.value,
      valueField: selectedValueField.value,
      isDocument: isDocument.value
    })

    // Reset form
    selectedSource.value = null
    selectedTable.value = null
    selectedTextField.value = null
    selectedValueField.value = null
    selectedNameAr.value = ''
    selectedNameEn.value = ''
    isDocument.value = false
    tables.value = []
    fields.value = []

    await loadMappings()
    toast.success(t('AddSuccess'))
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const removeMapping = async (mappingId: number) => {
  const confirmed = await confirm(t('Delete'), t('ConfirmDelete'))
  if (!confirmed) return

  loading.value = true
  try {
    await TaskMappingService.deleteMapping(mappingId)
    await loadMappings()
    toast.success(t('DeleteSuccess'))
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadConnections()
  loadMappings()
})
</script>

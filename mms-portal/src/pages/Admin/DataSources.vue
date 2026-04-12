<template>
  <div class="space-y-6">
    <div class="page-header">
      <h1 class="page-title">{{ $t('DataSources') }}</h1>
    </div>

    <Card :loading="loading">
      <template #header>
        <Button variant="primary" icon-left="mdi:plus" @click="addNew">
          {{ $t('Add') }}
        </Button>
      </template>

      <DataTable
        :columns="columns"
        :data="connections"
        :loading="loading"
      >
        <template #cell-actions="{ row }">
          <div class="flex items-center gap-2">
            <Button
              variant="ghost"
              size="sm"
              icon-left="mdi:pencil"
              class="text-primary"
              @click="editItem(row)"
            />
            <Button
              variant="ghost"
              size="sm"
              icon-left="mdi:trash-can-outline"
              class="text-error"
              @click="deleteDataSource(row.id)"
            />
          </div>
        </template>
      </DataTable>
    </Card>

    <!-- Add/Edit Dialog -->
    <Modal
      v-model="connectionDialog"
      :title="edit ? ($t('Edit')) : ($t('Add'))"
      size="md"
    >
      <form @submit.prevent="edit ? update() : save()" class="space-y-4">
        <Input
          v-model="dataSource.dbName"
          :label="$t('DbName')"
          required
        />
        <Input
          v-model="dataSource.instanceName"
          :label="$t('InstanceName')"
          required
        />
        <Input
          v-model="dataSource.username"
          :label="$t('Username')"
          required
        />
        <Input
          v-model="dataSource.password"
          type="password"
          autocomplete="new-password"
          :label="$t('Password')"
          required
        />
      </form>
      <template #footer>
        <Button variant="secondary" @click="connectionDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button v-if="!edit" variant="primary" @click="save">
          {{ $t('Save') }}
        </Button>
        <Button v-else variant="primary" @click="update">
          {{ $t('Update') }}
        </Button>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Card from '@/components/ui/Card.vue'
import Button from '@/components/ui/Button.vue'
import Input from '@/components/ui/Input.vue'
import Modal from '@/components/ui/Modal.vue'
import DataTable from '@/components/ui/DataTable.vue'
import DataSourcesService, { type DataSource } from '@/services/DataSourcesService'
import { useToast } from '@/composables/useToast'
import { useConfirm } from '@/composables/useConfirm'
import { useI18n } from 'vue-i18n'

const { toast } = useToast()
const { confirm } = useConfirm()
const { t } = useI18n()

const loading = ref(false)
const connectionDialog = ref(false)
const edit = ref(false)
const connections = ref<DataSource[]>([])

const dataSource = ref<Partial<DataSource>>({
  id: undefined,
  dbName: '',
  instanceName: '',
  username: '',
  password: ''
})

const columns = [
  { key: 'id', label: t('Id') },
  { key: 'dbName', label: t('DbName') },
  { key: 'instanceName', label: t('InstanceName') },
  { key: 'username', label: t('Username') },
  { key: 'password', label: t('Password') },
  { key: 'actions', label: t('Actions') }
]

const listDbs = async () => {
  loading.value = true
  try {
    connections.value = await DataSourcesService.listDataSources()
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const addNew = () => {
  edit.value = false
  dataSource.value = {
    id: undefined,
    dbName: '',
    instanceName: '',
    username: '',
    password: ''
  }
  connectionDialog.value = true
}

const editItem = (item: DataSource) => {
  edit.value = true
  dataSource.value = { ...item }
  connectionDialog.value = true
}

const save = async () => {
  if (!dataSource.value.dbName || !dataSource.value.instanceName || !dataSource.value.username || !dataSource.value.password) {
    toast.error(t('RequiredField'))
    return
  }

  loading.value = true
  try {
    await DataSourcesService.addDataSource(dataSource.value)
    connectionDialog.value = false
    await listDbs()
    toast.success(t('AddSuccess'))
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const update = async () => {
  if (!dataSource.value.id || !dataSource.value.dbName || !dataSource.value.instanceName || !dataSource.value.username || !dataSource.value.password) {
    toast.error(t('RequiredField'))
    return
  }

  loading.value = true
  try {
    await DataSourcesService.updateDataSource(dataSource.value.id, dataSource.value)
    connectionDialog.value = false
    await listDbs()
    toast.success(t('UpdateSuccess'))
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const deleteDataSource = async (id: number) => {
  const confirmed = await confirm(t('Delete'), t('ConfirmDelete'))
  if (!confirmed) return

  loading.value = true
  try {
    await DataSourcesService.deleteDataSource(id)
    await listDbs()
    toast.success(t('DeleteSuccess'))
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  listDbs()
})
</script>

<template>
  <div class="space-y-6">
    <div class="page-header">
      <h1 class="page-title">{{ $t('AuditTrail') }}</h1>
    </div>

    <Card :loading="loading">
      <template #header>
        <div class="flex items-center gap-4">
          <Input
            v-model="search"
            :placeholder="$t('SearchTextInput')"
            class="w-64"
          />
          <Button variant="primary" icon-left="mdi:database-search" @click="listAuditTrails()">
            {{ $t('Search') }}
          </Button>
          <Button variant="secondary" icon-left="mdi:refresh" @click="resetSearch">
            {{ $t('Reset') }}
          </Button>
        </div>
      </template>

      <DataTable
        :columns="columns"
        :data="auditLogs"
        :loading="loading"
        :total="totalCount"
        :page-size="10"
        server-side
        expandable
        @page-change="listAuditTrails"
      >
        <template #cell-createdDate="{ row }">
          {{ formatDateTime(row.createdDate) }}
        </template>

        <template #expanded-row="{ row }">
          <div class="p-4 bg-zinc-50 text-sm">
            <p class="text-zinc-700">{{ row.additionalInfo || ($t('NoAdditionalInfo')) }}</p>
          </div>
        </template>
      </DataTable>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Card from '@/components/ui/Card.vue'
import Button from '@/components/ui/Button.vue'
import Input from '@/components/ui/Input.vue'
import DataTable from '@/components/ui/DataTable.vue'
import AuditLogsService, { type AuditLog } from '@/services/AuditLogsService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

const { toast } = useToast()
const { t } = useI18n()

const loading = ref(false)
const search = ref('')
const auditLogs = ref<AuditLog[]>([])
const totalCount = ref(0)

const columns = [
  { key: 'id', label: t('Id') },
  { key: 'username', label: t('Username') },
  { key: 'processInstanceId', label: t('ProcessInstanceId') },
  { key: 'recordId', label: t('RecordId') },
  { key: 'letterId', label: t('LetterId') },
  { key: 'commentId', label: t('CommentId') },
  { key: 'actionName', label: t('ActionName') },
  { key: 'controllerName', label: t('ControllerName') },
  { key: 'description', label: t('Description') },
  { key: 'createdDate', label: t('CreatedDate') }
]

const formatDateTime = (dateString: string) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return new Intl.DateTimeFormat('ar-EG', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date)
}

const listAuditTrails = async (options?: { page: number; itemsPerPage: number }) => {
  const page = options?.page || 1
  const pageSize = options?.itemsPerPage || 10

  loading.value = true
  try {
    const response = await AuditLogsService.listAuditTrails(page, pageSize, search.value)
    auditLogs.value = response.data || []
    totalCount.value = response.total || 0
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const resetSearch = () => {
  search.value = ''
  listAuditTrails()
}

onMounted(() => {
  listAuditTrails()
})
</script>

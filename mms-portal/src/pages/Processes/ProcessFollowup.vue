<template>
  <div class="space-y-6">
    <!-- Search Filters -->
    <Card :loading="loading">
      <template #header>
        <h3 class="text-lg font-semibold">{{ $t('ProcessFollowup') }}</h3>
      </template>

      <div class="p-4 space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Select
            v-model="status"
            :options="statusOptions"
            :label="$t('Status')"
            :loading="controlLoading"
            clearable
          />
          <Input
            v-model="title"
            :label="$t('ProcessTitle')"
            @keydown.enter="search"
          />
          <Input
            v-model="processIdentifier"
            :label="`${$t('ReferenceNumber')}/${$t('ProcessInstanceId')}`"
            @keydown.enter="search"
          />
        </div>
        <div class="flex gap-2">
          <Button variant="secondary" :disabled="loading" @click="() => search()">
            {{ $t('Search') }}
          </Button>
          <Button variant="primary" :disabled="loading" @click="reset">
            {{ $t('Reset') }}
          </Button>
        </div>
      </div>
    </Card>

    <!-- Results Table -->
    <Card v-if="showGrid" :loading="loading">
      <DataTable
        :columns="columns"
        :data="result"
        :loading="loading"
        :total="totalCount"
        :page="currentPage"
        :page-size="pageSize"
        server-side
        @page-change="onPageChange"
      >
        <template #cell-id="{ row }">
          <div class="flex items-center gap-2">
            <router-link
              :to="{ name: 'followup-details', params: { processInstanceId: row.id } }"
              class="text-primary hover:underline"
            >
              {{ row.id }}
            </router-link>
            <router-link
              :to="{ name: 'followup-details', params: { processInstanceId: row.id } }"
              target="_blank"
              class="text-zinc-400 hover:text-primary"
              :title="$t('OpenNewWindow')"
            >
              <Icon icon="mdi:arrow-expand-all" class="w-4 h-4" />
            </router-link>
          </div>
        </template>

        <template #cell-dateStarted="{ row }">
          {{ formatDate(row.dateStarted) }}
        </template>

        <template #cell-status="{ row }">
          <span :class="getStatusClass(row.status)">
            {{ $t(row.status) || row.status }}
          </span>
        </template>

        <template #cell-expand="{ row }">
          <Button
            variant="ghost"
            size="sm"
            :icon-left="expandedRows.includes(row.id) ? 'mdi:chevron-up' : 'mdi:chevron-down'"
            @click="toggleExpand(row)"
          />
        </template>

        <template #expanded-row="{ row }">
          <div v-if="expandedRows.includes(row.id)" class="p-4 bg-zinc-50 border-s-4 border-zinc-600">
            <div v-if="loadingActivities" class="flex justify-center py-4">
              <Icon icon="mdi:loading" class="w-6 h-6 animate-spin text-primary" />
            </div>
            <template v-else-if="row.activities && row.activities.length > 0">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b">
                    <th class="text-start py-2 px-2">{{ $t('Title') }}</th>
                    <th class="text-start py-2 px-2">{{ $t('StartDate') }}</th>
                    <th class="text-start py-2 px-2">{{ $t('DueDate') }}</th>
                    <th class="text-start py-2 px-2">{{ $t('EndDate') }}</th>
                    <th class="text-start py-2 px-2">{{ $t('TaskOwner') }}</th>
                    <th class="text-start py-2 px-2">{{ $t('Status') }}</th>
                    <th class="text-start py-2 px-2">{{ $t('ActivityNumber') }}</th>
                    <th class="text-start py-2 px-2">{{ $t('ParentActivityNumber') }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(activity, i) in row.activities" :key="i" class="border-b">
                    <td class="py-2 px-2">{{ activity.title }}</td>
                    <td class="py-2 px-2">{{ formatDateTime(activity.startDate) }}</td>
                    <td class="py-2 px-2">{{ formatDate(activity.dueDate) }}</td>
                    <td class="py-2 px-2">{{ formatDateTime(activity.endDate) }}</td>
                    <td class="py-2 px-2">{{ activity.owner }}</td>
                    <td class="py-2 px-2" :class="getStatusClass(activity.status)">
                      {{ $t(activity.status) || activity.status }}
                    </td>
                    <td class="py-2 px-2">{{ activity.id }}</td>
                    <td class="py-2 px-2">{{ activity.parentId }}</td>
                  </tr>
                </tbody>
              </table>
            </template>
            <p v-else class="text-zinc-500 py-4">{{ $t('NoItemsToShow') }}</p>
          </div>
        </template>
      </DataTable>
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
import ProcessesService from '@/services/ProcessesService'
import LookupsService, { type LookupItem } from '@/services/LookupsService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

interface ProcessResult {
  id: number
  title: string
  dateStarted?: string
  status: string
  reference?: string
  activities?: any[]
}

const { toast } = useToast()
const { t } = useI18n()

const loading = ref(false)
const controlLoading = ref(false)
const loadingActivities = ref(false)
const showGrid = ref(false)

const status = ref<number | null>(null)
const title = ref('')
const processIdentifier = ref('')
const statuses = ref<LookupItem[]>([])
const result = ref<ProcessResult[]>([])
const totalCount = ref(0)
const currentPage = ref(1)
const pageSize = ref(10)
const expandedRows = ref<number[]>([])

const columns = [
  { field: 'id', header: t('ProcessInstanceId'), key: 'id', label: t('ProcessInstanceId') },
  { field: 'title', header: t('Title'), key: 'title', label: t('Title') },
  { field: 'dateStarted', header: t('StartDate'), key: 'dateStarted', label: t('StartDate') },
  { field: 'status', header: t('Status'), key: 'status', label: t('Status') },
  { field: 'reference', header: t('ReferenceNumber'), key: 'reference', label: t('ReferenceNumber') },
  { field: 'expand', header: '', key: 'expand', label: '', width: '50px' }
]

const statusOptions = computed(() =>
  statuses.value.map(s => ({ value: s.id, label: t(s.name as string) || s.name }))
)

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-'
  try {
    return new Date(dateStr).toLocaleDateString('ar-EG')
  } catch {
    return dateStr
  }
}

const formatDateTime = (dateStr: string) => {
  if (!dateStr) return '-'
  try {
    return new Date(dateStr).toLocaleString('ar-EG')
  } catch {
    return dateStr
  }
}

const getStatusClass = (status: string) => {
  if (status === 'Completed') return 'text-success font-semibold'
  if (status === 'Active') return 'text-warning font-semibold'
  return ''
}

const listProcessStatuses = async () => {
  controlLoading.value = true
  try {
    statuses.value = await LookupsService.listProcessStatuses()
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    controlLoading.value = false
  }
}

const search = async (options?: { page?: number; pageSize?: number }) => {
  showGrid.value = true
  loading.value = true

  const searchObj = {
    text: null,
    statusId: status.value,
    title: title.value || null,
    processIdentifier: processIdentifier.value || null,
    page: options?.page || currentPage.value,
    pageSize: options?.pageSize || pageSize.value,
    sortBy: null,
    sortDesc: null
  }

  try {
    const response = await ProcessesService.list(searchObj)
    result.value = response.data
    totalCount.value = response.total
  } catch {
    result.value = []
    totalCount.value = 0
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const onPageChange = (page: number) => {
  currentPage.value = page
  search({ page })
}

const toggleExpand = async (row: ProcessResult) => {
  const index = expandedRows.value.indexOf(row.id)
  if (index > -1) {
    expandedRows.value.splice(index, 1)
  } else {
    expandedRows.value.push(row.id)
    if (!row.activities) {
      await loadActivities(row)
    }
  }
}

const loadActivities = async (row: ProcessResult) => {
  loadingActivities.value = true
  try {
    const activities = await ProcessesService.listActivities(row.id)
    row.activities = activities
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loadingActivities.value = false
  }
}

const reset = () => {
  status.value = null
  title.value = ''
  processIdentifier.value = ''
  result.value = []
  totalCount.value = 0
  expandedRows.value = []
  showGrid.value = false
}

onMounted(() => {
  listProcessStatuses()
})
</script>

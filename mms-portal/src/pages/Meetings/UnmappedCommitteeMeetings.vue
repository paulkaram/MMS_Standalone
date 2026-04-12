<template>
  <div class="space-y-6">
    <div class="page-header">
      <h1 class="page-title">{{ $t('UnmappedMeetings') }}</h1>
    </div>

    <Card :loading="loading">
      <DataTable
        :columns="columns"
        :data="meetings"
        :loading="loading"
        :total="totalCount"
        :page="currentPage"
        :page-size="pageSize"
        server-side
        @page-change="onPageChange"
      >
        <template #cell-date="{ row }">
          {{ formatDate(row.date) }}
        </template>

        <template #cell-title="{ row }">
          {{ truncateText(row.title, 100) }}
        </template>

        <template #cell-options="{ row }">
          <div class="flex items-center gap-2">
            <router-link
              v-if="isDraft(row.statusId)"
              :to="{ name: 'addMeeting', params: { id: row.id } }"
            >
              <Button variant="ghost" size="sm" icon-left="mdi:pencil" class="text-primary" />
            </router-link>
            <Button
              v-else
              variant="ghost"
              size="sm"
              icon-left="mdi:eye"
              class="text-secondary"
              @click="viewMeeting(row)"
            />
          </div>
        </template>
      </DataTable>
    </Card>

    <!-- Meeting Viewer Modal -->
    <Modal
      v-model="meetingDialogOpened"
      :title="selectedMeeting?.title || ($t('MeetingDetails'))"
      size="xl"
    >
      <div v-if="selectedMeeting" class="p-4 space-y-4">
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="text-sm text-zinc-500">{{ $t('Id') }}</label>
            <p class="font-medium">{{ selectedMeeting.id }}</p>
          </div>
          <div>
            <label class="text-sm text-zinc-500">{{ $t('Status') }}</label>
            <p class="font-medium">{{ selectedMeeting.statusName }}</p>
          </div>
          <div>
            <label class="text-sm text-zinc-500">{{ $t('Date') }}</label>
            <p class="font-medium">{{ formatDate(selectedMeeting.date) }}</p>
          </div>
          <div>
            <label class="text-sm text-zinc-500">{{ $t('MeetingRoom') }}</label>
            <p class="font-medium">{{ selectedMeeting.location || '-' }}</p>
          </div>
          <div>
            <label class="text-sm text-zinc-500">{{ $t('ReferenceNumber') }}</label>
            <p class="font-medium">{{ selectedMeeting.referenceNumber || '-' }}</p>
          </div>
        </div>
        <div>
          <label class="text-sm text-zinc-500">{{ $t('Title') }}</label>
          <p class="font-medium">{{ selectedMeeting.title }}</p>
        </div>
      </div>
      <template #footer>
        <Button variant="secondary" @click="meetingDialogOpened = false">
          {{ $t('Close') }}
        </Button>
        <router-link :to="{ name: 'meetingDashboard', params: { id: selectedMeeting?.id } }">
          <Button variant="primary">
            {{ $t('OpenMeetingDashboard') }}
          </Button>
        </router-link>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Card from '@/components/ui/Card.vue'
import Button from '@/components/ui/Button.vue'
import Modal from '@/components/ui/Modal.vue'
import DataTable from '@/components/ui/DataTable.vue'
import MeetingsService, { type Meeting } from '@/services/MeetingsService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

const { toast } = useToast()
const { t } = useI18n()

// Meeting status enum
const MeetingStatusEnum = {
  Draft: 1
}

interface MeetingRow extends Meeting {
  statusId?: number
  referenceNumber?: string
}

const loading = ref(false)
const meetings = ref<MeetingRow[]>([])
const totalCount = ref(0)
const currentPage = ref(1)
const pageSize = ref(10)
const meetingDialogOpened = ref(false)
const selectedMeeting = ref<MeetingRow | null>(null)

const columns = [
  { key: 'id', label: t('Id') },
  { key: 'title', label: t('Title') },
  { key: 'statusName', label: t('Status') },
  { key: 'location', label: t('MeetingRoom') },
  { key: 'referenceNumber', label: t('ReferenceNumber') },
  { key: 'date', label: t('Date') },
  { key: 'options', label: t('Options') }
]

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-'
  try {
    return new Date(dateStr).toLocaleDateString('ar-EG')
  } catch {
    return dateStr
  }
}

const truncateText = (text: string, maxLength: number) => {
  if (!text) return ''
  if (text.length <= maxLength) return text
  return text.substring(0, maxLength) + '...'
}

const isDraft = (statusId?: number) => {
  return statusId === MeetingStatusEnum.Draft
}

const loadMeetings = async (page: number = 1) => {
  loading.value = true
  try {
    const params = {
      statusId: null,
      committeeId: null,
      meetingId: null,
      fromDate: null,
      toDate: null,
      location: null,
      title: null,
      onlyMyMeetings: false,
      noComitteeRelated: true,
      includeDrafts: true
    }

    const response = await MeetingsService.searchUserMeetings(params, page, pageSize.value)
    meetings.value = response.data
    totalCount.value = response.total
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const onPageChange = (page: number) => {
  currentPage.value = page
  loadMeetings(page)
}

const viewMeeting = (meeting: MeetingRow) => {
  selectedMeeting.value = meeting
  meetingDialogOpened.value = true
}

onMounted(() => {
  loadMeetings()
})
</script>

<template>
  <div class="space-y-5">
    <PageHeader :title="$t('NotRelatedMeetings')" :subtitle="$t('MeetingsNotLinkedToCommittee')" />

    <MeetingGrid
      ref="gridRef"
      :search-params="{ onlyMyMeetings: false, noComitteeRelated: true, includeDrafts: true }"
      :columns="['status', 'reference', 'location']"
      @edit="editMeeting"
      @delete="deleteMeeting"
      @view="viewMeeting"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '@/components/layout/PageHeader.vue'
import MeetingGrid from '@/components/app/meeting/MeetingGrid.vue'
import MeetingsService from '@/services/MeetingsService'
import { useConfirm } from '@/composables/useConfirm'

const router = useRouter()
const { confirm } = useConfirm()
const gridRef = ref<InstanceType<typeof MeetingGrid>>()

function editMeeting(m: any) {
  router.push({ name: 'addMeeting', params: { id: m.id } })
}

function viewMeeting(m: any) {
  router.push({ name: 'meetingRoom', params: { id: m.id } })
}

async function deleteMeeting(m: any) {
  const ok = await confirm({ title: 'Delete', message: `Delete meeting #${m.id}?`, type: 'danger' })
  if (!ok) return
  try {
    await MeetingsService.deleteMeeting(m.id)
    gridRef.value?.reload()
  } catch { /* silent */ }
}
</script>

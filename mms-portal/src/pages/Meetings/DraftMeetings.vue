<template>
  <div class="space-y-5">
    <PageHeader :title="$t('DraftMeetings')" :subtitle="$t('DraftMeetingsDescription')">
      <template #actions>
        <Button variant="primary" icon-left="mdi:plus" @click="$router.push('/addMeeting')">
          {{ $t('NewMeeting') }}
        </Button>
      </template>
    </PageHeader>

    <MeetingGrid
      ref="gridRef"
      :search-params="{ statusId: 1, includeDrafts: true }"
      :columns="['location']"
      @edit="editDraft"
      @delete="deleteDraft"
      @view="editDraft"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import Button from '@/components/ui/Button.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import MeetingGrid from '@/components/app/meeting/MeetingGrid.vue'
import MeetingsService from '@/services/MeetingsService'
import { useConfirm } from '@/composables/useConfirm'

const router = useRouter()
const { confirm } = useConfirm()
const gridRef = ref<InstanceType<typeof MeetingGrid>>()

function editDraft(draft: any) {
  router.push({ name: 'addMeeting', params: { id: draft.id } })
}

async function deleteDraft(draft: any) {
  const ok = await confirm({
    title: 'Delete',
    message: `Delete draft "${draft.title || draft.titleAr || draft.id}"?`,
    type: 'danger'
  })
  if (!ok) return
  try {
    await MeetingsService.deleteMeeting(draft.id)
    gridRef.value?.reload()
  } catch { /* silent */ }
}
</script>

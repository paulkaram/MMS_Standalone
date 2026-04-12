<template>
  <div class="min-h-screen bg-zinc-100">
    <!-- Loading State -->
    <div v-if="loading" class="flex flex-col items-center justify-center min-h-screen">
      <Icon icon="mdi:loading" class="w-16 h-16 animate-spin text-primary" />
      <span class="mt-4 text-zinc-600">{{ loadingText }}</span>
    </div>

    <!-- File Viewer -->
    <FileViewer
      v-else-if="validParams && documentData"
      :query="documentData"
      name="Attachment"
      class="h-screen"
    />

    <!-- No Data State -->
    <div v-else class="flex flex-col items-center justify-center min-h-screen">
      <Icon icon="mdi:file-alert" class="w-16 h-16 text-zinc-400" />
      <p class="mt-4 text-zinc-600">{{ $t('NoData') }}</p>
      <p class="mt-2 text-sm text-zinc-500">
        {{ $t('InvalidAttachmentParams') }}
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import FileViewer from '@/components/ui/FileViewer.vue'
import AttachmentsService, { type AttachmentInfo } from '@/services/AttachmentsService'
import { useUserStore } from '@/stores/user'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

const route = useRoute()
const userStore = useUserStore()
const { toast } = useToast()
const { t } = useI18n()

const loading = ref(false)
const loadingText = ref('')
const documentData = ref<AttachmentInfo | null>(null)

const attachmentId = computed(() => route.query.attachmentId as string)
const token = computed(() => route.query.token as string)
const validParams = computed(() => !!attachmentId.value && !!token.value)

const loadAttachment = async () => {
  if (!validParams.value) return

  loading.value = true
  loadingText.value = t('LoadingAttachment')

  try {
    // Update token in store if provided via query
    if (token.value) {
      userStore.setToken(token.value)
    }

    const response = await AttachmentsService.getAttachment(attachmentId.value)
    documentData.value = response
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  if (validParams.value) {
    loadAttachment()
  }
})
</script>

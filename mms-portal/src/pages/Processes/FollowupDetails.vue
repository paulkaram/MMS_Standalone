<template>
  <div class="space-y-6">
    <Card :loading="loading">
      <template #header>
        <div
          class="flex items-center justify-between cursor-pointer"
          @click="expanded = !expanded"
        >
          <h3 class="text-lg font-semibold">
            {{ $t('ProcessTitle') }}: {{ generalInfo?.processTitle }}
          </h3>
          <Icon
            :icon="expanded ? 'mdi:chevron-up' : 'mdi:chevron-down'"
            class="w-6 h-6"
          />
        </div>
      </template>

      <div v-if="expanded && generalInfo" class="p-4">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div class="space-y-1">
            <label class="text-sm text-zinc-500">{{ $t('ProcessNumber') }}</label>
            <p class="font-medium">{{ generalInfo.processNumber || '-' }}</p>
          </div>
          <div class="space-y-1">
            <label class="text-sm text-zinc-500">{{ $t('ReferenceNumber') }}</label>
            <p class="font-medium">{{ generalInfo.referenceNumber || '-' }}</p>
          </div>
          <div class="space-y-1">
            <label class="text-sm text-zinc-500">{{ $t('Status') }}</label>
            <p class="font-medium text-error">{{ $t(generalInfo.status) || generalInfo.status || '-' }}</p>
          </div>
          <div class="space-y-1">
            <label class="text-sm text-zinc-500">{{ $t('Stage') }}</label>
            <p class="font-medium text-error">{{ $t(generalInfo.stage) || generalInfo.stage || '-' }}</p>
          </div>
          <div class="space-y-1">
            <label class="text-sm text-zinc-500">{{ $t('CreatedDate') }}</label>
            <p class="font-medium">{{ formatDate(generalInfo.createdDate) }}</p>
          </div>
          <div class="space-y-1">
            <label class="text-sm text-zinc-500">{{ $t('ProcessType') }}</label>
            <p class="font-medium">{{ generalInfo.processType || '-' }}</p>
          </div>
        </div>
      </div>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'
import ProcessesService from '@/services/ProcessesService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

const route = useRoute()
const { toast } = useToast()
const { t } = useI18n()

interface GeneralInfo {
  processTitle: string
  processNumber: string
  referenceNumber: string
  status: string
  stage: string
  createdDate: string
  processType: string
}

const loading = ref(false)
const expanded = ref(true)
const generalInfo = ref<GeneralInfo | null>(null)

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-'
  try {
    return new Date(dateStr).toLocaleDateString('ar-EG')
  } catch {
    return dateStr
  }
}

const getProcess = async () => {
  const processId = parseInt(route.params.processInstanceId as string)
  if (!processId) return

  loading.value = true
  try {
    const response = await ProcessesService.getProcess(processId)
    generalInfo.value = response as unknown as GeneralInfo
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  getProcess()
})
</script>

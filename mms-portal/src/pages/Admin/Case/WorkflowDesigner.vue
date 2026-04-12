<template>
  <div class="space-y-6">
    <Card :loading="loading">
      <Stepper v-model="currentStep" :steps="steps">
        <!-- Step 1: Workflow Definition -->
        <template #step-1>
          <div class="p-6">
            <form class="space-y-4">
              <Input
                v-model="workflowObject.workflowDefinition.name"
                :label="$t('Name')"
                required
              />
              <Input
                v-model="workflowObject.workflowDefinition.title"
                :label="$t('Title')"
                required
              />
              <Input
                v-model="workflowObject.workflowDefinition.abbreviation"
                :label="$t('Abbreviation')"
              />
              <Input
                v-model="workflowObject.workflowDefinition.description"
                :label="$t('Description')"
              />
              <Input
                v-model="workflowObject.workflowDefinition.icon"
                :label="$t('Icon')"
              />
              <Input
                v-model="workflowObject.workflowDefinition.store"
                :label="$t('Store')"
              />
              <Input
                v-model="workflowObject.workflowDefinition.titleTemplate"
                :label="$t('TitleTemplate')"
              />
            </form>
          </div>
        </template>

        <!-- Step 2: Workflow Properties -->
        <template #step-2>
          <div class="p-6">
            <div class="text-center py-12 text-zinc-500">
              <Icon icon="mdi:cog" class="w-16 h-16 mx-auto mb-4 text-zinc-300" />
              <p>{{ $t('WorkflowPropertiesConfig') }}</p>
            </div>
          </div>
        </template>

        <!-- Step 3: Form Builder -->
        <template #step-3>
          <div class="p-6">
            <div class="text-center py-12 text-zinc-500">
              <Icon icon="mdi:form-select" class="w-16 h-16 mx-auto mb-4 text-zinc-300" />
              <p>{{ $t('FormBuilderConfig') }}</p>
            </div>
          </div>
        </template>

        <!-- Step 4: Workflow Designer -->
        <template #step-4>
          <div class="p-6">
            <div class="border-2 border-dashed border-zinc-300 rounded-lg p-8 min-h-[400px]">
              <div class="text-center text-zinc-500">
                <Icon icon="mdi:sitemap" class="w-16 h-16 mx-auto mb-4 text-zinc-300" />
                <p class="mb-4">{{ $t('WorkflowDesignerCanvas') }}</p>
                <p class="text-sm">{{ $t('DragAndDropActivities') }}</p>
              </div>
            </div>
          </div>
        </template>

        <!-- Step 5: Publish -->
        <template #step-5>
          <div class="p-6">
            <div class="text-center py-8">
              <Icon icon="mdi:rocket-launch" class="w-20 h-20 mx-auto mb-6 text-primary" />
              <h3 class="text-xl font-semibold mb-4">{{ $t('ReadyToPublish') }}</h3>
              <p class="text-zinc-600 mb-6">
                {{ $t('PublishWorkflowDescription') }}
              </p>

              <div class="flex items-center justify-center gap-4">
                <label class="flex items-center gap-2">
                  <input
                    v-model="publishAsNewVersion"
                    type="checkbox"
                    class="rounded border-zinc-300 text-primary focus:ring-primary"
                  />
                  {{ $t('PublishAsNewVersion') }}
                </label>
              </div>

              <Button
                variant="primary"
                size="lg"
                class="mt-6"
                :loading="publishing"
                @click="publishWorkflow"
              >
                <Icon icon="mdi:publish" class="w-5 h-5 me-2" />
                {{ $t('Publish') }}
              </Button>
            </div>
          </div>
        </template>
      </Stepper>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'
import Button from '@/components/ui/Button.vue'
import Input from '@/components/ui/Input.vue'
import Stepper from '@/components/ui/Stepper.vue'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

const route = useRoute()
const router = useRouter()
const { toast } = useToast()
const { t } = useI18n()

const loading = ref(false)
const publishing = ref(false)
const currentStep = ref(1)
const publishAsNewVersion = ref(false)

const steps = [
  { label: t('WorkflowDefinition') },
  { label: t('WorkflowProperties') },
  { label: t('FormBuilder') },
  { label: t('WorkflowDesigner') },
  { label: t('Publish') }
]

const workflowObject = ref({
  workflowDefinition: {
    name: '',
    title: '',
    abbreviation: '',
    description: '',
    icon: '',
    store: '',
    titleTemplate: ''
  },
  workflowProperties: null,
  formData: null,
  activityDefinitions: null,
  transitions: null
})

const loadProcess = async (processId: string) => {
  loading.value = true
  try {
    // Would load the process from the API
    // const response = await CaseEngineService.getProcess(processId)
    toast.info(t('LoadingProcess'))
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const publishWorkflow = async () => {
  publishing.value = true
  try {
    // Would publish the workflow
    // await CaseEngineService.publish(JSON.stringify(workflowObject.value), route.query.id || 0, publishAsNewVersion.value)
    toast.success(t('WorkflowPublished'))
    router.push({ name: 'apps' })
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    publishing.value = false
  }
}

onMounted(() => {
  if (route.query.id) {
    loadProcess(route.query.id as string)
  }
})
</script>

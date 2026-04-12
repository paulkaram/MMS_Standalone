<template>
  <div class="space-y-6">
    <!-- Dynamic Form -->
    <Card v-if="!isPosted" :loading="loading">
      <template #header>
        <h3 class="text-lg font-semibold text-primary">{{ $t('NewProcess') }}</h3>
      </template>

      <form @submit.prevent="startProcess" class="p-4 space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <template v-for="control in formControls" :key="control.id">
            <!-- Header -->
            <div v-if="control.inputType === 'Header'" class="col-span-full">
              <h4 class="text-lg font-semibold text-primary py-2">{{ control.fieldName }}</h4>
            </div>

            <!-- Text Input -->
            <div v-else-if="control.inputType === 'Text'" :class="getColClass(control.col)">
              <Input
                v-model="control.fieldValue[0]"
                :label="control.fieldName"
                :required="control.required"
                :maxlength="control.maxChar"
                :hint="control.helpText"
                icon-left="mdi:text-shadow"
              />
            </div>

            <!-- Email Input -->
            <div v-else-if="control.inputType === 'Email'" :class="getColClass(control.col)">
              <Input
                v-model="control.fieldValue[0]"
                type="email"
                :label="control.fieldName"
                :required="control.required"
                :maxlength="control.maxChar"
                :hint="control.helpText"
                icon-left="mdi:email"
              />
            </div>

            <!-- Number Input -->
            <div v-else-if="control.inputType === 'Number'" :class="getColClass(control.col)">
              <Input
                v-model="control.fieldValue[0]"
                type="number"
                :label="control.fieldName"
                :required="control.required"
                :maxlength="control.maxChar"
                :hint="control.helpText"
                icon-left="mdi:numeric"
              />
            </div>

            <!-- Dropdown Select -->
            <div v-else-if="control.inputType === 'DropDown'" :class="getColClass(control.col)">
              <Select
                v-model="control.fieldValue[0]"
                :options="mapOptions(control.options)"
                :label="control.fieldName"
                :required="control.required"
                clearable
              />
            </div>

            <!-- TextArea -->
            <div v-else-if="control.inputType === 'TextArea'" :class="getColClass(control.col)">
              <div class="space-y-1">
                <label class="block text-sm font-medium text-zinc-700">
                  {{ control.fieldName }}
                  <span v-if="control.required" class="text-error">*</span>
                </label>
                <textarea
                  v-model="control.fieldValue[0]"
                  :maxlength="control.maxChar"
                  :required="control.required"
                  rows="3"
                  class="w-full px-3 py-2 border border-zinc-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
                />
                <p v-if="control.helpText" class="text-xs text-zinc-500">{{ control.helpText }}</p>
              </div>
            </div>

            <!-- Checkbox Group -->
            <div v-else-if="control.inputType === 'Checkbox'" :class="getColClass(control.col)">
              <div class="space-y-2">
                <label class="block text-sm font-medium text-zinc-700">{{ control.fieldName }}</label>
                <div class="space-y-1">
                  <label
                    v-for="option in control.options"
                    :key="option.value"
                    class="flex items-center gap-2"
                  >
                    <input
                      v-model="control.fieldValue"
                      type="checkbox"
                      :value="option.value"
                      class="rounded border-zinc-300 text-primary focus:ring-primary"
                    />
                    {{ option.label }}
                  </label>
                </div>
              </div>
            </div>

            <!-- Radio Group -->
            <div v-else-if="control.inputType === 'Radio'" :class="getColClass(control.col)">
              <div class="space-y-2">
                <label class="block text-sm font-medium text-zinc-700">{{ control.fieldName }}</label>
                <div :class="(control.options?.length ?? 0) <= 2 ? 'flex gap-4' : 'space-y-1'">
                  <label
                    v-for="option in control.options"
                    :key="option.value"
                    class="flex items-center gap-2"
                  >
                    <input
                      v-model="control.fieldValue[0]"
                      type="radio"
                      :value="option.value"
                      :name="`radio-${control.id}`"
                      class="border-zinc-300 text-primary focus:ring-primary"
                    />
                    {{ option.label }}
                  </label>
                </div>
              </div>
            </div>

            <!-- Boolean Switch -->
            <div v-else-if="control.inputType === 'Boolean'" :class="getColClass(control.col)">
              <label class="flex items-center gap-2">
                <input
                  v-model="control.fieldValue[0]"
                  type="checkbox"
                  class="rounded border-zinc-300 text-primary focus:ring-primary"
                />
                {{ control.fieldName }}
              </label>
            </div>

            <!-- Date Picker -->
            <div v-else-if="control.inputType === 'Date'" :class="getColClass(control.col)">
              <Input
                v-model="control.fieldValue[0]"
                type="date"
                :label="control.fieldName"
                :required="control.required"
              />
            </div>

            <!-- Masters Dropdown -->
            <div v-else-if="control.inputType === 'Masters' && control.controlType === 'dropdown'" :class="getColClass(control.col)">
              <Select
                v-model="control.fieldValue[0]"
                :options="mapOptions(control.options)"
                :label="control.fieldName"
                :required="control.required"
                clearable
              />
            </div>

            <!-- File Upload -->
            <div v-else-if="control.inputType === 'Upload'" :class="getColClass(control.col)">
              <div class="space-y-1">
                <label class="block text-sm font-medium text-zinc-700">
                  {{ control.fieldName }}
                </label>
                <input
                  type="file"
                  :multiple="control.multiple"
                  :accept="control.accept?.toString()"
                  class="block w-full text-sm text-zinc-500 file:me-4 file:py-2 file:px-4 file:rounded file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary hover:file:bg-primary-100"
                  @change="(e) => handleFileChange(e, control)"
                />
                <p v-if="control.helpText" class="text-xs text-zinc-500">{{ control.helpText }}</p>
              </div>
            </div>
          </template>
        </div>

        <div class="flex justify-end pt-4 border-t">
          <Button
            variant="primary"
            size="lg"
            :loading="btnLoading"
            @click="startProcess"
          >
            <Icon icon="mdi:power" class="w-5 h-5 me-2" />
            {{ $t('StartProcess') }}
          </Button>
        </div>
      </form>
    </Card>

    <!-- Success Card -->
    <Card v-else class="max-w-md mx-auto">
      <div class="p-8 text-center">
        <Icon icon="mdi:check-circle" class="w-16 h-16 mx-auto mb-4 text-success" />
        <h2 class="text-xl font-semibold mb-4">{{ $t('resources_success_start_process') }}</h2>
        <div class="space-y-2 text-start mb-6">
          <p>
            <span class="font-medium">{{ $t('process-number') }}:</span>
            {{ response?.processId }}
          </p>
          <p>
            <span class="font-medium">{{ $t('reference-number') }}:</span>
            {{ response?.referenceNumber }}
          </p>
        </div>
        <router-link to="/">
          <Button variant="primary">
            {{ $t('resources_homepage') }}
          </Button>
        </router-link>
      </div>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'
import Button from '@/components/ui/Button.vue'
import Input from '@/components/ui/Input.vue'
import Select from '@/components/ui/Select.vue'
import ProcessesService from '@/services/ProcessesService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

interface FormOption {
  label: string
  value: string | number
}

interface FormControl {
  id: number
  inputType: string
  controlType?: string
  fieldName: string
  fieldValue: any[]
  helpText?: string
  maxChar?: number
  required?: boolean
  col?: number
  options?: FormOption[]
  multiple?: boolean
  accept?: string[]
  attachments?: any[]
}

interface ProcessResponse {
  processId: number
  referenceNumber: string
}

const route = useRoute()
const { toast } = useToast()
const { t } = useI18n()

const loading = ref(false)
const btnLoading = ref(false)
const isPosted = ref(false)
const formControls = ref<FormControl[]>([])
const response = ref<ProcessResponse | null>(null)

const getColClass = (col?: number) => {
  if (!col || col >= 12) return 'col-span-full'
  if (col >= 6) return 'md:col-span-2 lg:col-span-2'
  if (col >= 4) return 'md:col-span-1 lg:col-span-1'
  return ''
}

const mapOptions = (options?: FormOption[]) => {
  if (!options) return []
  return options.map(o => ({ value: o.value, label: o.label }))
}

const handleFileChange = (event: Event, control: FormControl) => {
  const target = event.target as HTMLInputElement
  if (target.files) {
    control.attachments = Array.from(target.files)
  }
}

const listInitiationData = async () => {
  const workflowId = route.params.workflowId as string
  if (!workflowId) return

  loading.value = true
  try {
    const data = await ProcessesService.listInitiationData(parseInt(workflowId))
    formControls.value = data || []
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const startProcess = async () => {
  // Basic validation
  const requiredFields = formControls.value.filter(c => c.required)
  const invalidFields = requiredFields.filter(c => {
    if (!c.fieldValue || c.fieldValue.length === 0) return true
    if (c.inputType === 'Checkbox') return c.fieldValue.length === 0
    return !c.fieldValue[0]
  })

  if (invalidFields.length > 0) {
    toast.error(t('RequiredField'))
    return
  }

  btnLoading.value = true
  try {
    const formData = new FormData()
    formData.append('objectData', JSON.stringify({
      formControls: formControls.value
    }))
    formData.append('processTypeId', route.params.workflowId as string)

    // Append file attachments
    formControls.value.forEach(control => {
      if (control.inputType === 'Upload' && control.attachments) {
        control.attachments.forEach((file: File) => {
          formData.append(`file_${control.id}`, file)
        })
      }
    })

    response.value = await ProcessesService.createProcess(formData)
    isPosted.value = true
    toast.success(t('WorkflowStarted'))
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    btnLoading.value = false
  }
}

onMounted(() => {
  listInitiationData()
})
</script>

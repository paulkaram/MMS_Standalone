<template>
  <div class="space-y-6">
    <!-- Add Agenda Button -->
    <div v-if="!viewMode" class="flex justify-end">
      <button
        type="button"
        class="add-agenda-btn"
        @click="openAddDialog"
      >
        <Icon icon="mdi:plus" class="w-5 h-5" />
        <span>{{ $t('AddAgendaItem') }}</span>
      </button>
    </div>

    <!-- Agenda List -->
    <Card>
      <template #header>
        <h3 class="text-lg font-semibold text-zinc-900">
          {{ $t('AgendaItems') }}
          <span class="text-sm font-normal text-zinc-500">({{ agendaItems.length }})</span>
        </h3>
      </template>

      <!-- Loading -->
      <div v-if="loading" class="space-y-3">
        <div v-for="i in 3" :key="i" class="animate-pulse h-16 bg-zinc-200 rounded"></div>
      </div>

      <!-- Empty State -->
      <div v-else-if="agendaItems.length === 0" class="text-center py-12 text-zinc-500">
        <Icon icon="mdi:format-list-bulleted" class="w-16 h-16 mx-auto mb-4 text-zinc-300" />
        <p>{{ $t('NoAgendaItems') }}</p>
      </div>

      <!-- Agenda Table -->
      <div v-else class="overflow-x-auto">
        <table class="data-table">
          <thead>
            <tr>
              <th class="w-16">#</th>
              <th>{{ $t('Subject') }}</th>
              <th class="w-24">{{ $t('Duration') }}</th>
              <th class="w-32">{{ $t('VotingType') }}</th>
              <th v-if="!viewMode" class="w-24">{{ $t('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(item, index) in agendaItems" :key="item.id || index">
              <td class="font-medium">{{ index + 1 }}</td>
              <td>
                <div>
                  <p class="font-medium">{{ item.title }}</p>
                  <div v-if="item.agendaTopics?.length" class="mt-1">
                    <p
                      v-for="(topic, topicIndex) in item.agendaTopics"
                      :key="topicIndex"
                      class="text-sm text-zinc-500"
                    >
                      • {{ topic.text }}
                    </p>
                  </div>
                </div>
              </td>
              <td>{{ item.duration ? `${item.duration} دقيقة` : '-' }}</td>
              <td>{{ getVotingTypeName(item.votingTypeId) }}</td>
              <td v-if="!viewMode">
                <div class="flex items-center gap-1">
                  <Button
                    variant="ghost"
                    size="sm"
                    icon-left="mdi:pencil"
                    @click="editItem(item)"
                  />
                  <Button
                    variant="ghost"
                    size="sm"
                    icon-left="mdi:delete"
                    class="text-error hover:bg-error-50"
                    @click="deleteItem(item)"
                  />
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </Card>

    <!-- Add/Edit Dialog -->
    <Modal
      v-if="dialogOpen"
      v-model="dialogOpen"
      :title="editingItem ? ($t('EditAgendaItem')) : ($t('AddAgendaItem'))"
      size="lg"
    >
      <div class="space-y-4">
        <!-- Subject -->
        <div>
          <label class="form-label">{{ $t('Subject') }} <span class="text-error">*</span></label>
          <Input
            v-model="formData.title"
            :placeholder="$t('EnterSubject')"
          />
        </div>

        <!-- Duration -->
        <div>
          <label class="form-label">{{ $t('Duration') }}</label>
          <Input
            v-model.number="formData.duration"
            type="number"
            min="0"
            :placeholder="$t('EnterDuration')"
          />
        </div>

        <!-- Voting Type -->
        <div>
          <CustomSelect
            v-model="formData.votingTypeId"
            :options="votingTypes"
            :label="$t('VotingType')"
            :placeholder="$t('NoVoting')"
            value-key="id"
            label-key="name"
            clearable
          />
        </div>

        <!-- Committee Duty (if committee meeting) -->
        <div v-if="isCommittee && committeeDuties.length > 0">
          <CustomSelect
            v-model="formData.committeeDutyId"
            :options="committeeDuties"
            :label="$t('CommitteeDuty')"
            :placeholder="$t('Select')"
            value-key="id"
            label-key="title"
            clearable
          />
        </div>

        <!-- Topics -->
        <div>
          <div class="flex items-center justify-between mb-3">
            <span class="text-sm text-zinc-500">{{ $t('Topics') }}</span>
            <button
              type="button"
              class="add-topic-btn"
              @click="addTopic"
            >
              <span>{{ $t('AddTopic') }}</span>
              <Icon icon="mdi:plus" class="w-4 h-4" />
            </button>
          </div>
          <div class="space-y-2">
            <div
              v-for="(topic, index) in formData.agendaTopics"
              :key="index"
              class="flex items-center gap-2"
            >
              <Input
                v-model="topic.text"
                :placeholder="$t('EnterTopic')"
                class="flex-1"
              />
              <button
                type="button"
                class="remove-topic-btn"
                @click="removeTopic(index)"
              >
                <Icon icon="mdi:close" class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-3">
          <Button variant="outline" @click="closeDialog">
            {{ $t('Cancel') }}
          </Button>
          <Button
            variant="primary"
            :loading="saving"
            :disabled="!formData.title?.trim()"
            @click="saveItem"
          >
            {{ $t('Save') }}
          </Button>
        </div>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, nextTick } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import Modal from '@/components/ui/Modal.vue'
import CustomSelect from '@/components/ui/CustomSelect.vue'
import MeetingsService from '@/services/MeetingsService'
import LookupsService from '@/services/LookupsService'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'

export interface AgendaTopic {
  text: string
}

export interface AgendaItem {
  id?: string | number
  meetingId?: string | number
  title: string
  duration?: number
  votingTypeId?: string | null
  agendaTopics: AgendaTopic[]
  committeeDutyId?: string | null
  order?: number
}

interface Props {
  modelValue: AgendaItem[]
  meetingId?: number | string
  isCommittee?: boolean
  committeeId?: string | null
  viewMode?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  viewMode: false,
  isCommittee: false,
  committeeId: null,
  meetingId: undefined
})

const emit = defineEmits<{
  'update:modelValue': [value: AgendaItem[]]
}>()

// State
const agendaItems = ref<AgendaItem[]>([...(props.modelValue || [])])
const loading = ref(false)

// Flag to prevent infinite loop between watchers
let isUpdatingFromProps = false
const saving = ref(false)
const dialogOpen = ref(false)
const editingItem = ref<AgendaItem | null>(null)
const votingTypes = ref<Array<{ id: string; name: string }>>([])
const committeeDuties = ref<Array<{ id: string; title: string }>>([])

const formData = ref<AgendaItem>({
  title: '',
  duration: undefined,
  votingTypeId: null,
  agendaTopics: [],
  committeeDutyId: null
})

// Watch for external changes
watch(() => props.modelValue, (newVal) => {
  if (JSON.stringify(newVal) !== JSON.stringify(agendaItems.value)) {
    isUpdatingFromProps = true
    agendaItems.value = [...(newVal || [])]
    nextTick(() => {
      isUpdatingFromProps = false
    })
  }
}, { deep: true })

// Watch for local changes and emit
watch(agendaItems, (newVal) => {
  if (!isUpdatingFromProps) {
    emit('update:modelValue', [...newVal])
  }
}, { deep: true })

// Watch committee changes
watch(() => props.committeeId, (newVal) => {
  if (newVal && props.isCommittee) {
    loadCommitteeDuties()
  }
})

// Methods
const getVotingTypeName = (typeId: string | null | undefined): string => {
  if (!typeId) return '-'
  const type = votingTypes.value.find(t => t.id === typeId)
  return type?.name || '-'
}

const openAddDialog = () => {
  editingItem.value = null
  formData.value = {
    title: '',
    duration: undefined,
    votingTypeId: null,
    agendaTopics: [],
    committeeDutyId: null
  }
  dialogOpen.value = true
}

const editItem = (item: AgendaItem) => {
  editingItem.value = item
  formData.value = {
    ...item,
    agendaTopics: [...(item.agendaTopics || [])]
  }
  dialogOpen.value = true
}

const closeDialog = () => {
  dialogOpen.value = false
  editingItem.value = null
}

const addTopic = () => {
  formData.value.agendaTopics.push({ text: '' })
}

const removeTopic = (index: number) => {
  formData.value.agendaTopics.splice(index, 1)
}

const saveItem = async () => {
  if (!formData.value.title?.trim()) return

  saving.value = true
  try {
    // Filter out empty topics
    const cleanedData: AgendaItem = {
      ...formData.value,
      agendaTopics: formData.value.agendaTopics.filter(t => t.text?.trim())
    }

    if (editingItem.value) {
      // Update existing
      if (props.meetingId && editingItem.value.id) {
        const updateData = {
          id: editingItem.value.id,
          meetingId: props.meetingId,
          title: cleanedData.title,
          duration: cleanedData.duration || 0,
          duty: '',
          committeeDutyId: cleanedData.committeeDutyId || null,
          voting: '',
          votingTypeId: cleanedData.votingTypeId || null,
          agendaTopics: cleanedData.agendaTopics || []
        }
        const result = await MeetingsService.updateAgendaItem(props.meetingId, updateData)
        // API returns updated agenda list
        if (result.data) {
          agendaItems.value = result.data
        } else {
          const index = agendaItems.value.findIndex(a => a.id === editingItem.value!.id)
          if (index !== -1) {
            agendaItems.value[index] = { ...editingItem.value, ...cleanedData }
          }
        }
      } else {
        const index = agendaItems.value.findIndex(a => a.id === editingItem.value!.id)
        if (index !== -1) {
          agendaItems.value[index] = { ...editingItem.value, ...cleanedData }
        }
      }
    } else {
      // Add new
      if (props.meetingId) {
        const newItem = {
          id: 0,
          meetingId: props.meetingId,
          title: cleanedData.title,
          duration: cleanedData.duration || 0,
          duty: '',
          committeeDutyId: cleanedData.committeeDutyId || null,
          voting: '',
          votingTypeId: cleanedData.votingTypeId || null,
          agendaTopics: cleanedData.agendaTopics || []
        }
        const result = await MeetingsService.addAgendaItem(props.meetingId, newItem)
        // API returns updated agenda list
        if (result.data) {
          agendaItems.value = result.data
        } else if (result.id) {
          cleanedData.id = result.id
          agendaItems.value.push(cleanedData)
        }
      } else {
        cleanedData.id = Date.now() // Temporary ID
        agendaItems.value.push(cleanedData)
      }
    }

    closeDialog()
  } catch (error) {
    console.error('Failed to save agenda item:', error)
  } finally {
    saving.value = false
  }
}

const deleteItem = async (item: AgendaItem) => {
  // TODO: Add confirmation dialog
  try {
    if (props.meetingId && item.id) {
      const result = await MeetingsService.deleteAgendaItem(props.meetingId, item.id)
      // API returns updated agenda list
      if (result && (result as any).data) {
        agendaItems.value = (result as any).data
      } else {
        agendaItems.value = agendaItems.value.filter(a => a.id !== item.id)
      }
    } else {
      agendaItems.value = agendaItems.value.filter(a => a.id !== item.id)
    }
  } catch (error) {
    console.error('Failed to delete agenda item:', error)
  }
}

const loadVotingTypes = async () => {
  try {
    const response = await LookupsService.getVotingTypes()
    // Handle different response structures and filter out null items
    const data = response?.data || response || []
    votingTypes.value = Array.isArray(data) ? data.filter((t: any) => t && t.id) : []
  } catch (error) {
    console.error('Failed to load voting types:', error)
  }
}

const loadCommitteeDuties = async () => {
  if (!props.committeeId) return
  try {
    const response = await CouncilCommitteesService.listCommitteeDuties(props.committeeId)
    // Handle different response structures and filter out null items
    const data = response?.data || response || []
    committeeDuties.value = Array.isArray(data) ? data.filter((d: any) => d && d.id) : []
  } catch (error) {
    console.error('Failed to load committee duties:', error)
  }
}

// Validation
const validate = (): boolean => {
  return agendaItems.value.length > 0
}

// Expose validate method
defineExpose({ validate })

// Lifecycle
onMounted(() => {
  loadVotingTypes()
  if (props.isCommittee && props.committeeId) {
    loadCommitteeDuties()
  }
})
</script>

<style scoped>
.add-agenda-btn {
  @apply inline-flex items-center gap-2 px-4 py-2.5 rounded-lg font-medium text-sm;
  @apply bg-primary text-white;
  @apply hover:bg-primary-600 active:bg-primary-700;
  @apply transition-all duration-200;
  @apply focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2;
}

.add-topic-btn {
  @apply inline-flex items-center gap-1.5 text-sm text-zinc-600;
  @apply hover:text-primary transition-colors duration-200;
}

.remove-topic-btn {
  @apply p-1.5 rounded-lg text-zinc-400;
  @apply hover:text-error hover:bg-error/10;
  @apply transition-all duration-200;
}
</style>

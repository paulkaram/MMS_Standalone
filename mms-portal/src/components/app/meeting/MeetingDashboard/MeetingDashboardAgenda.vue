<template>
  <div class="space-y-4">
    <!-- Timer Card -->
    <Card class="text-center p-4">
      <p class="text-sm text-zinc-500">{{ $t('TimeRemaining') }}</p>
      <p class="text-2xl font-bold text-primary mt-2">{{ meetingRemainingDuration }}</p>
      <p class="text-xs text-zinc-400">{{ $t('HoursMinutesSeconds') }}</p>
    </Card>

    <!-- Agenda Card -->
    <Card class="h-[calc(100vh-280px)] flex flex-col">
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-sm font-semibold">{{ $t('Agenda') }}</h3>
          <div class="flex items-center gap-1">
            <button
              v-if="showPlayButton"
              type="button"
              class="p-1.5 text-primary hover:bg-primary-50 rounded"
              :disabled="!meetingOwner"
              @click="pauseResumeAgenda"
            >
              <Icon icon="mdi:play-circle" class="w-6 h-6" />
            </button>
            <button
              v-if="showPauseButton"
              type="button"
              class="p-1.5 text-yellow-600 hover:bg-yellow-50 rounded"
              :disabled="!meetingOwner"
              @click="pauseResumeAgenda"
            >
              <Icon icon="mdi:pause-circle" class="w-6 h-6" />
            </button>
            <button
              v-if="showNextButton"
              type="button"
              class="p-1.5 text-error hover:bg-error-50 rounded"
              @click="$emit('start-next-agenda')"
            >
              <Icon icon="mdi:skip-next-circle" class="w-6 h-6" />
            </button>
          </div>
        </div>
      </template>

      <div class="flex-1 overflow-y-auto">
        <div
          v-for="(item, index) in agendaItems"
          :key="item.id"
          class="border-b border-zinc-100 last:border-0"
        >
          <!-- Agenda Item Header -->
          <div class="p-3">
            <div class="flex items-start gap-2">
              <span class="text-sm font-semibold text-zinc-500">{{ index + 1 }}.</span>
              <div class="flex-1">
                <h4 class="text-sm font-medium" :title="item.title">
                  {{ truncate(item.title, 50) }}
                </h4>

                <!-- Voting Options -->
                <div v-if="showVotingOptions(index, item)" class="mt-2 space-y-1">
                  <label
                    v-for="option in item.votingType?.votingOptions"
                    :key="option.id"
                    class="flex items-center gap-2 cursor-pointer"
                  >
                    <input
                      type="radio"
                      :name="`vote-${item.id}`"
                      :value="option.id"
                      class="text-primary focus:ring-primary"
                      @change="onVote(item, option, index)"
                    />
                    <span class="text-sm">{{ option.nameAr }}</span>
                  </label>
                </div>

                <!-- Selected Vote -->
                <div v-else-if="getSelectedVote(item)" class="mt-2">
                  <span class="px-2 py-1 bg-primary-50 text-primary text-xs rounded-full">
                    {{ getSelectedVote(item) }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <!-- Agenda Item Status Bar -->
          <div
            :class="[
              'px-3 py-2 text-xs flex items-center justify-between',
              item.isRunning ? 'bg-primary text-white' : 'bg-zinc-50 text-zinc-600'
            ]"
          >
            <div class="flex items-center gap-2">
              <Icon
                v-if="item.isRunning"
                icon="mdi:clock-outline"
                class="w-4 h-4 text-red-400"
              />
              <span v-if="item.isRunning">{{ formattedCurrentTime }}</span>
              <span v-else-if="index >= runningAgendaIndex">
                {{ formatDuration(item.remainingSeconds || item.duration * 60) }}
              </span>
              <span v-else class="text-red-500 font-medium">
                {{ $t('Finished') }}
              </span>
            </div>
            <div class="flex items-center gap-1">
              <button
                v-if="showAddNoteButton"
                type="button"
                :class="[
                  'p-1 rounded',
                  item.isRunning ? 'hover:bg-white/20' : 'hover:bg-zinc-200'
                ]"
                @click="openAddNote(item.id)"
              >
                <Icon icon="mdi:comment-text" class="w-4 h-4" />
              </button>
              <button
                v-if="showAddRecommendationButton"
                type="button"
                :class="[
                  'p-1 rounded',
                  item.isRunning ? 'hover:bg-white/20' : 'hover:bg-zinc-200'
                ]"
                @click="openAddRecommendation(item.id)"
              >
                <Icon icon="mdi:plus" class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>

        <!-- Empty State -->
        <div v-if="agendaItems.length === 0" class="flex items-center justify-center h-full text-zinc-400">
          <div class="text-center">
            <Icon icon="mdi:calendar-outline" class="w-12 h-12 mx-auto mb-2" />
            <p class="text-sm">{{ $t('NoAgendaItems') }}</p>
          </div>
        </div>
      </div>
    </Card>

    <!-- Add Note Dialog -->
    <Modal v-if="showAddNoteDialog" v-model="showAddNoteDialog" :title="$t('AddNote')" size="md">
      <div>
        <textarea
          v-model="noteText"
          rows="4"
          class="w-full px-4 py-2 border border-zinc-200 rounded-lg focus:outline-none focus:border-primary"
          :placeholder="$t('EnterNote')"
        />
      </div>
      <template #footer>
        <Button variant="outline" @click="showAddNoteDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="primary" :loading="savingNote" @click="saveNote">
          {{ $t('Save') }}
        </Button>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'
import Button from '@/components/ui/Button.vue'
import Modal from '@/components/ui/Modal.vue'
import MeetingsService from '@/services/MeetingsService'
import MeetingAgendaService from '@/services/MeetingAgendaService'
import { useSignalR } from '@/composables/useSignalR'
import { useUserStore } from '@/stores/user'

const MeetingStatusEnum = {
  Started: 4,
  Finished: 5,
  FinalMeetingMinutesSigned: 9
}

const props = defineProps<{
  meetingId: number
  meetingOwner: boolean
  attendees: any[]
  statusId: number
  viewMode: boolean
  agendaList: any[]
}>()

const emit = defineEmits(['start-next-agenda', 'update'])

const userStore = useUserStore()
const { connect, on, off, isConnected } = useSignalR()

// State
const agendaItems = ref<any[]>([])
const secondsRemaining = ref(0)
const isPaused = ref(true)
const runningAgendaIndex = ref(-1)
const meetingEnd = ref(false)
const timer = ref<ReturnType<typeof setInterval> | null>(null)

// Note Dialog
const showAddNoteDialog = ref(false)
const selectedAgendaId = ref<number | null>(null)
const noteText = ref('')
const savingNote = ref(false)

// Computed
const formattedCurrentTime = computed(() => {
  const minutes = Math.floor(secondsRemaining.value / 60)
  const seconds = secondsRemaining.value % 60
  return `${minutes}:${seconds.toString().padStart(2, '0')}`
})

const meetingRemainingDuration = computed(() => {
  if (meetingEnd.value || agendaItems.value.length === 0) return '00:00:00'

  let duration = 0
  for (let i = runningAgendaIndex.value; i < agendaItems.value.length; i++) {
    const item = agendaItems.value[i]
    if (!item.actualStartDate) {
      duration += (item.duration || 0) * 60
    } else if (!item.actualEndDate) {
      duration += secondsRemaining.value
    }
  }

  const hours = Math.floor(duration / 3600)
  const minutes = Math.floor((duration % 3600) / 60)
  const seconds = duration % 60
  return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
})

const showPlayButton = computed(() => isPaused.value && !meetingEnd.value)
const showPauseButton = computed(() => !isPaused.value && !meetingEnd.value)
const showNextButton = computed(() => {
  return !meetingEnd.value && runningAgendaIndex.value >= 0 && props.meetingOwner
})

const showAddNoteButton = computed(() => {
  return !props.viewMode &&
    props.statusId >= MeetingStatusEnum.Started &&
    props.statusId < MeetingStatusEnum.FinalMeetingMinutesSigned
})

const showAddRecommendationButton = computed(() => {
  return !props.viewMode &&
    props.meetingOwner &&
    props.statusId >= MeetingStatusEnum.Started &&
    props.statusId < MeetingStatusEnum.FinalMeetingMinutesSigned
})

// Methods
const formatDuration = (seconds: number) => {
  const mins = Math.floor(seconds / 60)
  const secs = seconds % 60
  return `${mins}:${secs.toString().padStart(2, '0')}`
}

const truncate = (text: string, length: number) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}

const showVotingOptions = (index: number, item: any) => {
  const isRunning = item.id === agendaItems.value[runningAgendaIndex.value]?.id
  const hasOptions = item.votingType?.votingOptions?.length > 0
  const hasVoted = item.meetingUserVotes?.some(
    (vote: any) => vote.userId === userStore.user?.id
  )
  return hasOptions && !hasVoted && isRunning
}

const getSelectedVote = (item: any) => {
  const userVote = item.meetingUserVotes?.find(
    (vote: any) => vote.userId === userStore.user?.id
  )
  if (!userVote) return null
  return item.votingType?.votingOptions?.find(
    (opt: any) => opt.id === userVote.vottingOptionId
  )?.nameAr
}

const onVote = async (agenda: any, option: any, index: number) => {
  try {
    const result = await MeetingAgendaService.sendVote({
      votingOptionId: option.id,
      meetingAgendaId: agenda.id
    })
    if (result.data) {
      agendaItems.value[index].meetingUserVotes = result.data
    }
  } catch (error) {
    console.error('Failed to send vote:', error)
  }
}

const pauseResumeAgenda = async () => {
  try {
    await MeetingsService.pauseOrResume(props.meetingId)
  } catch (error) {
    console.error('Failed to pause/resume agenda:', error)
  }
}

const openAddNote = (agendaId: number) => {
  selectedAgendaId.value = agendaId
  noteText.value = ''
  showAddNoteDialog.value = true
}

const openAddRecommendation = (agendaId: number) => {
  // This would open a recommendation dialog
}

const saveNote = async () => {
  if (!noteText.value.trim() || !selectedAgendaId.value) return

  savingNote.value = true
  try {
    await MeetingAgendaService.addNote(selectedAgendaId.value, noteText.value)
    showAddNoteDialog.value = false
  } catch (error) {
    console.error('Failed to save note:', error)
  } finally {
    savingNote.value = false
  }
}

const startTimer = () => {
  stopTimer()
  timer.value = setInterval(() => {
    if (secondsRemaining.value > 0) {
      secondsRemaining.value--

      if (secondsRemaining.value === 0) {
        runningAgendaIndex.value++
        if (runningAgendaIndex.value >= agendaItems.value.length) {
          meetingEnd.value = true
        }
        if (props.meetingOwner) {
          emit('start-next-agenda')
        }
      }
    } else {
      stopTimer()
    }
  }, 1000)
}

const stopTimer = () => {
  if (timer.value) {
    clearInterval(timer.value)
    timer.value = null
  }
}

const updateAgendaState = () => {
  runningAgendaIndex.value = -1
  secondsRemaining.value = 0

  for (let i = 0; i < agendaItems.value.length; i++) {
    const item = agendaItems.value[i]
    if (item.isRunning) {
      runningAgendaIndex.value = i
      secondsRemaining.value = item.remainingSeconds || 0
      isPaused.value = item.paused || false
      break
    }
  }

  // Check if meeting ended
  if (agendaItems.value.length > 0) {
    const lastItem = agendaItems.value[agendaItems.value.length - 1]
    if (lastItem.actualEndDate) {
      meetingEnd.value = true
      runningAgendaIndex.value = agendaItems.value.length
    }
  }

  // Start/stop timer based on state
  if (!meetingEnd.value && !isPaused.value) {
    startTimer()
  } else {
    stopTimer()
  }
}

const handleAgendaChanges = (newAgendaItems: any[]) => {
  if (newAgendaItems?.length > 0 && newAgendaItems[0].meetingId === props.meetingId) {
    agendaItems.value = newAgendaItems
    emit('update', newAgendaItems)
  }
}

// Watch
watch(() => props.agendaList, (newList) => {
  agendaItems.value = newList
}, { immediate: true, deep: true })

watch(agendaItems, updateAgendaState, { deep: true })

// Lifecycle
onMounted(async () => {
  agendaItems.value = props.agendaList
  updateAgendaState()

  try {
    await connect()
    if (isConnected.value) {
      on(`MeetingAgendaChanges${props.meetingId}`, handleAgendaChanges)
    }
  } catch (error) {
    console.error('SignalR agenda connection failed:', error)
  }
})

onUnmounted(() => {
  stopTimer()
  off(`MeetingAgendaChanges${props.meetingId}`)
})
</script>

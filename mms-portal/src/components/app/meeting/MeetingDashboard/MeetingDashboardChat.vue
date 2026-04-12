<template>
  <Card :class="['flex flex-col transition-all duration-300', expanded ? 'h-80' : 'h-14']">
    <!-- Header -->
    <div class="flex items-center justify-between px-4 py-2 border-b border-zinc-200">
      <h3 class="text-sm font-semibold">{{ $t('Chat') }}</h3>
      <button
        type="button"
        class="p-1 text-primary hover:bg-primary-50 rounded"
        @click="$emit('toggle-expand')"
      >
        <Icon :icon="expanded ? 'mdi:chevron-down' : 'mdi:chevron-up'" class="w-5 h-5" />
      </button>
    </div>

    <template v-if="expanded">
      <!-- Messages -->
      <div
        ref="messagesContainer"
        class="flex-1 overflow-y-auto p-3 space-y-3"
      >
        <div
          v-for="(message, index) in messages"
          :key="index"
          :class="['flex flex-col', message.me ? 'items-start' : 'items-end']"
        >
          <div :class="['flex items-end gap-2', message.me ? 'flex-row-reverse' : '']">
            <div
              :class="[
                'px-3 py-2 rounded-lg max-w-[80%]',
                message.me ? 'bg-primary text-white' : 'bg-zinc-100 text-zinc-800'
              ]"
            >
              <p class="text-sm">{{ message.messageText }}</p>
            </div>
            <div class="w-7 h-7 rounded-full bg-zinc-200 flex items-center justify-center flex-shrink-0">
              <Icon icon="mdi:account" class="w-4 h-4 text-zinc-500" />
            </div>
          </div>
          <span class="text-xs text-zinc-400 mt-1">
            {{ message.userName }} - {{ formatTime(message.sentAt) }}
          </span>
        </div>

        <!-- Empty State -->
        <div v-if="messages.length === 0" class="flex items-center justify-center h-full text-zinc-400">
          <div class="text-center">
            <Icon icon="mdi:chat-outline" class="w-10 h-10 mx-auto mb-2" />
            <p class="text-sm">{{ $t('NoMessages') }}</p>
          </div>
        </div>
      </div>

      <!-- Input -->
      <div v-if="!viewMode" class="p-3 border-t border-zinc-200">
        <div class="flex items-center gap-2">
          <input
            v-model="newMessage"
            type="text"
            :placeholder="$t('TypeMessage')"
            class="flex-1 px-3 py-2 border border-zinc-200 rounded-lg text-sm focus:outline-none focus:border-primary"
            @keyup.enter="sendMessage"
          />
          <button
            type="button"
            :disabled="!newMessage.trim()"
            :class="[
              'p-2 rounded-lg transition-colors',
              newMessage.trim()
                ? 'bg-primary text-white hover:bg-primary-600'
                : 'bg-zinc-100 text-zinc-400 cursor-not-allowed'
            ]"
            @click="sendMessage"
          >
            <Icon icon="mdi:send" class="w-5 h-5 rtl:rotate-180" />
          </button>
        </div>
      </div>
    </template>
  </Card>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'
import ChatsService from '@/services/ChatsService'
import { useSignalR } from '@/composables/useSignalR'
import { useUserStore } from '@/stores/user'

const props = defineProps<{
  meetingId: number
  viewMode: boolean
  expanded: boolean
}>()

defineEmits(['toggle-expand'])

const userStore = useUserStore()
const { connect, on, off, isConnected } = useSignalR()

// State
const messages = ref<any[]>([])
const newMessage = ref('')
const messagesContainer = ref<HTMLElement | null>(null)

// Methods
const loadMessages = async () => {
  try {
    const result = await ChatsService.loadMeetingChatMessages(props.meetingId)
    messages.value = (result.data || result).map((msg: any) => ({
      ...msg,
      me: msg.userId === userStore.user?.id
    }))
    scrollToBottom()
  } catch (error) {
    console.error('Failed to load chat messages:', error)
  }
}

const sendMessage = async () => {
  const text = newMessage.value.trim()
  if (!text) return

  // Optimistically add message
  messages.value.push({
    messageText: text,
    me: true,
    userName: userStore.user?.fullName || '',
    sentAt: new Date().toISOString()
  })
  scrollToBottom()

  const messageContent = text
  newMessage.value = ''

  try {
    await ChatsService.sendMeetingMessage({
      meetingId: props.meetingId,
      messageText: messageContent
    })
  } catch (error) {
    console.error('Failed to send message:', error)
    // Remove optimistically added message on error
    messages.value.pop()
  }
}

const scrollToBottom = () => {
  nextTick(() => {
    if (messagesContainer.value) {
      messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
    }
  })
}

const formatTime = (dateString: string) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' })
}

const setupSignalR = async () => {
  if (props.viewMode) return

  try {
    await connect()
    if (isConnected.value) {
      on(`MeetingChatChanges${props.meetingId}`, loadMessages)
    }
  } catch (error) {
    console.error('SignalR chat connection failed:', error)
  }
}

// Lifecycle
onMounted(async () => {
  if (!props.viewMode) {
    await loadMessages()
    await setupSignalR()
  }
})

onUnmounted(() => {
  off(`MeetingChatChanges${props.meetingId}`)
})

// Watch for expansion to scroll to bottom
watch(() => props.expanded, (expanded) => {
  if (expanded) {
    scrollToBottom()
  }
})
</script>

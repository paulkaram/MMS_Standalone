import { ref, nextTick, type Ref } from 'vue'
import ChatsService from '@/services/ChatsService'
import { useUserStore } from '@/stores/user'
import { useDebounceFn } from '@vueuse/core'
import { DEBOUNCE_DELAY_MS } from '../constants'
import type { ChatMessage } from '../types'

interface UseMeetingChatOptions {
  meetingId: Ref<string | number>
  scrollContainerRef?: Ref<HTMLElement | null>
}

/**
 * Composable for managing meeting chat functionality
 */
export function useMeetingChat(options: UseMeetingChatOptions) {
  const { meetingId, scrollContainerRef } = options
  const userStore = useUserStore()

  // ═══════════════════════════════════════════════════════════════════════════
  // STATE
  // ═══════════════════════════════════════════════════════════════════════════

  const messages = ref<ChatMessage[]>([])
  const inputText = ref('')
  const sending = ref(false)
  const loading = ref(false)

  // ═══════════════════════════════════════════════════════════════════════════
  // METHODS
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Load chat messages from server
   */
  async function loadMessages(): Promise<void> {
    loading.value = true
    try {
      const result: any = await ChatsService.loadMeetingChatMessages(Number(meetingId.value))
      const data = result?.data || result || []

      messages.value = data.map((msg: any) => ({
        ...msg,
        me: msg.userId === userStore.user?.id
      }))

      scrollToBottom()
    } catch (error) {
      console.error('Failed to load chat:', error)
    } finally {
      loading.value = false
    }
  }

  /**
   * Scroll chat container to bottom
   */
  function scrollToBottom(): void {
    nextTick(() => {
      if (scrollContainerRef?.value) {
        scrollContainerRef.value.scrollTop = scrollContainerRef.value.scrollHeight
      }
    })
  }

  /**
   * Internal send message implementation
   */
  async function _sendMessage(): Promise<void> {
    const text = inputText.value.trim()
    if (!text || sending.value) return

    // Optimistically add message to UI
    const optimisticMessage: ChatMessage = {
      meetingId: Number(meetingId.value),
      userId: userStore.user?.id || '',
      messageText: text,
      me: true,
      userName: userStore.user?.fullName || 'You',
      sentAt: new Date().toISOString()
    }

    messages.value.push(optimisticMessage)
    scrollToBottom()
    inputText.value = ''
    sending.value = true

    try {
      await ChatsService.sendMeetingMessage({
        meetingId: Number(meetingId.value),
        messageText: text
      })
    } catch (error) {
      // Remove optimistic message on failure
      const index = messages.value.indexOf(optimisticMessage)
      if (index > -1) {
        messages.value.splice(index, 1)
      }
      console.error('Failed to send message:', error)
    } finally {
      sending.value = false
    }
  }

  /**
   * Debounced send message to prevent spam
   */
  const sendMessage = useDebounceFn(_sendMessage, DEBOUNCE_DELAY_MS)

  /**
   * Add a message received from SignalR
   */
  function addMessage(message: ChatMessage): void {
    // Check if message already exists to avoid duplicates
    const exists = messages.value.some(
      m => m.id === message.id || (
        m.messageText === message.messageText &&
        m.userId === message.userId &&
        m.sentAt === message.sentAt
      )
    )

    if (!exists) {
      messages.value.push({
        ...message,
        me: message.userId === userStore.user?.id
      })
      scrollToBottom()
    }
  }

  /**
   * Clear all messages
   */
  function clearMessages(): void {
    messages.value = []
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // RETURN
  // ═══════════════════════════════════════════════════════════════════════════

  return {
    // State
    messages,
    inputText,
    sending,
    loading,

    // Methods
    loadMessages,
    sendMessage,
    addMessage,
    scrollToBottom,
    clearMessages,
  }
}

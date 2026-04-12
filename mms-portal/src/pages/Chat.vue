<template>
  <div class="chat-page">
    <!-- Sidebar - Chat List -->
    <aside class="chat-sidebar">
      <!-- Header -->
      <header class="sidebar-header">
        <h2>{{ $t('Messages') }}</h2>
        <span v-if="unreadCount > 0" class="unread-badge">{{ unreadCount }}</span>
      </header>

      <!-- Search -->
      <div class="search-box">
        <div class="search-input-wrapper">
          <svg class="search-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="11" cy="11" r="8"/>
            <path d="m21 21-4.35-4.35"/>
          </svg>
          <input
            v-model="searchQuery"
            type="text"
            :placeholder="$t('SearchUsers')"
            class="search-input"
            @input="onSearchInput"
          />
        </div>
        <!-- Search Results Dropdown -->
        <div v-if="showSearchResults && filteredUsers.length > 0" class="search-results">
          <div
            v-for="user in filteredUsers"
            :key="user.id"
            class="search-result-item"
            @click="selectSearchResult(user)"
          >
            <UserAvatar :userId="user.userId" :name="user.name" size="xs" />
            <span class="user-name">{{ user.name }}</span>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loadingChats" class="loading-state">
        <div class="spinner"></div>
      </div>

      <!-- Chat List -->
      <div v-else class="chat-list">
        <div
          v-for="chat in chatsList"
          :key="chat.id"
          class="chat-item"
          :class="{ active: openedChat?.id === chat.id }"
          @click="loadChat(chat)"
        >
          <!-- Avatar with Online Status -->
          <UserAvatar
            :userId="chat.chatMembers?.[0]?.userId"
            :name="chat.name"
            size="md"
            :online="isUserOnline(chat)"
            show-status
          />

          <!-- Chat Info -->
          <div class="chat-info">
            <div class="chat-name-row">
              <span class="chat-name">{{ chat.name }}</span>
              <span v-if="chat.updatedAt" class="chat-time">{{ formatTime(chat.updatedAt) }}</span>
            </div>
            <p class="chat-last-message">{{ chat.lastMessage || '' }}</p>
          </div>

          <!-- Unread Badge -->
          <span v-if="chat.unreadMessages > 0" class="chat-unread-badge">
            {{ chat.unreadMessages > 9 ? '9+' : chat.unreadMessages }}
          </span>
        </div>

        <!-- Empty State -->
        <div v-if="chatsList.length === 0" class="empty-state">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <path d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
          <p>{{ $t('NoChats') }}</p>
        </div>
      </div>
    </aside>

    <!-- Main Chat Area -->
    <main class="chat-main">
      <template v-if="openedChat">
        <!-- Chat Header -->
        <header class="chat-header">
          <div class="chat-header-user">
            <UserAvatar
              :userId="openedChat.chatMembers?.[0]?.userId"
              :name="openedChat.name"
              size="md"
              :online="isUserOnline(openedChat)"
              show-status
            />
            <div class="user-info">
              <h3>{{ openedChat.name }}</h3>
              <span class="user-status" :class="isUserOnline(openedChat) ? 'online' : 'offline'">
                {{ isUserOnline(openedChat) ? ($t('Online')) : ($t('Offline')) }}
              </span>
            </div>
          </div>
        </header>

        <!-- Messages Area -->
        <div ref="messagesContainer" class="messages-area">
          <div v-if="loadingMessages" class="loading-messages">
            <div class="spinner"></div>
          </div>

          <template v-else>
            <div
              v-for="msg in messages"
              :key="msg.id"
              class="message-wrapper"
              :class="{ own: msg.me }"
            >
              <!-- Other user's avatar (for received messages) -->
              <UserAvatar v-if="!msg.me" :userId="msg.userId" :name="msg.userName || ''" size="xs" />

              <div class="message-bubble" :class="{ own: msg.me }">
                <p class="message-text">{{ msg.messageText }}</p>
                <span class="message-time">{{ formatMessageTime(msg.sentAt) }}</span>
              </div>
            </div>

            <!-- Empty messages -->
            <div v-if="messages.length === 0" class="no-messages">
              <p>{{ $t('NoMessages') }}</p>
              <span>{{ $t('StartConversation') }}</span>
            </div>
          </template>
        </div>

        <!-- Message Input -->
        <footer class="message-input-area">
          <div class="input-wrapper">
            <textarea
              v-model="messageContent"
              :placeholder="$t('TypeMessage')"
              rows="1"
              class="message-input"
              @keydown.enter.exact.prevent="sendMessage"
            ></textarea>
            <button
              class="send-btn"
              :disabled="!messageContent.trim() || sending"
              @click="sendMessage"
            >
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M22 2L11 13M22 2l-7 20-4-9-9-4 20-7z" stroke-linecap="round" stroke-linejoin="round"/>
              </svg>
            </button>
          </div>
        </footer>
      </template>

      <!-- No Chat Selected -->
      <div v-else class="no-chat-selected">
        <div class="empty-chat-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <path d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
        <h3>{{ $t('SelectChat') }}</h3>
        <p>{{ $t('SelectChatDescription') }}</p>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useI18n } from 'vue-i18n'
import ChatsService, { type Chat, type ChatMessage, type SearchUser, type OnlineUser } from '@/services/ChatsService'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import { useToast } from '@/composables/useToast'
import { useSignalR } from '@/composables/useSignalR'
import { getDateLocale } from '@/helpers/dateFormat'

const { t } = useI18n()
const toast = useToast()
const { connect, disconnect, on, off, isConnected } = useSignalR()

// State
const chatsList = ref<Chat[]>([])
const messages = ref<ChatMessage[]>([])
const onlineUsers = ref<OnlineUser[]>([])
const searchUsers = ref<SearchUser[]>([])
const openedChat = ref<Chat | null>(null)
const openedChatProfileUrl = ref<string | null>(null)
const searchQuery = ref('')
const messageContent = ref('')
const showSearchResults = ref(false)

// Loading states
const loadingChats = ref(true)
const loadingMessages = ref(false)
const sending = ref(false)

// Refs
const messagesContainer = ref<HTMLElement | null>(null)

// Computed
const unreadCount = computed(() => {
  return chatsList.value.reduce((sum, chat) => sum + (chat.unreadMessages || 0), 0)
})

const filteredUsers = computed(() => {
  if (!searchQuery.value.trim()) return []
  const query = searchQuery.value.toLowerCase()
  return searchUsers.value.filter(user =>
    user.name.toLowerCase().includes(query)
  ).slice(0, 5)
})

// Methods
const getInitials = (name: string) => {
  if (!name) return '?'
  const parts = name.trim().split(/\s+/)
  if (parts.length > 1) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.substring(0, 2).toUpperCase()
}

// Track failed image loads
const failedImages = ref<Set<string>>(new Set())

const getChatAvatar = (chat: Chat, isOpened = false) => {
  // For opened chat, use the stored profile URL first
  if (isOpened && openedChatProfileUrl.value) {
    if (failedImages.value.has(openedChatProfileUrl.value)) return null
    return openedChatProfileUrl.value
  }
  // Check chatMembers for profile picture
  if (chat.chatMembers && chat.chatMembers.length > 0) {
    const member = chat.chatMembers[0]
    // First check if profilePictureUrl is already set
    if (member.profilePictureUrl) {
      if (failedImages.value.has(member.profilePictureUrl)) return null
      return member.profilePictureUrl
    }
    // Otherwise construct from userId if hasProfilePicture is true or not set
    if (member.hasProfilePicture !== false && member.userId) {
      const url = getProfilePictureUrl(member.userId)
      if (url && failedImages.value.has(url)) return null
      return url
    }
  }
  // Check searchUsers for profile picture by matching chatId or name
  const searchUser = searchUsers.value.find(u =>
    u.chatId === chat.id || u.name === chat.name
  )
  if (searchUser) {
    if (searchUser.profilePictureUrl) {
      if (failedImages.value.has(searchUser.profilePictureUrl)) return null
      return searchUser.profilePictureUrl
    }
    // Construct URL from userId
    if (searchUser.hasProfilePicture !== false && searchUser.userId) {
      const url = getProfilePictureUrl(searchUser.userId)
      if (url && failedImages.value.has(url)) return null
      return url
    }
  }
  // Check if chat itself has profile picture (some APIs return it directly)
  if ((chat as any).profilePictureUrl) {
    if (failedImages.value.has((chat as any).profilePictureUrl)) return null
    return (chat as any).profilePictureUrl
  }
  return null
}

const handleImageError = (event: Event) => {
  const img = event.target as HTMLImageElement
  if (img.src) {
    failedImages.value.add(img.src)
  }
  img.style.display = 'none'
}

const isUserOnline = (chat: Chat) => {
  if (!chat.chatMembers || chat.chatMembers.length === 0) return false
  return onlineUsers.value.some(u => u.id === chat.chatMembers[0].userId)
}

const formatTime = (dateStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  const days = Math.floor(diff / (1000 * 60 * 60 * 24))

  if (days === 0) {
    return date.toLocaleTimeString(getDateLocale(), { hour: '2-digit', minute: '2-digit' })
  } else if (days === 1) {
    return t('Yesterday')
  } else if (days < 7) {
    return date.toLocaleDateString(getDateLocale(), { weekday: 'short' })
  } else {
    return date.toLocaleDateString(getDateLocale(), { month: 'short', day: 'numeric' })
  }
}

const formatMessageTime = (dateStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleTimeString(getDateLocale(), { hour: '2-digit', minute: '2-digit' })
}

const scrollToBottom = () => {
  nextTick(() => {
    if (messagesContainer.value) {
      messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
    }
  })
}

const onSearchInput = () => {
  showSearchResults.value = searchQuery.value.trim().length > 0
}

// API Methods
const loadChats = async () => {
  loadingChats.value = true
  try {
    const response: any = await ChatsService.listUserChats()
    chatsList.value = response.data || response || []
  } catch (error) {
    console.error('Failed to load chats:', error)
    toast.error(t('LoadChatsFailed'))
  } finally {
    loadingChats.value = false
  }
}

const loadOnlineUsers = async () => {
  try {
    const response: any = await ChatsService.listOnlineUsers()
    onlineUsers.value = response.data || response || []
  } catch (error) {
    console.error('Failed to load online users:', error)
  }
}

const loadSearchUsers = async () => {
  try {
    const response: any = await ChatsService.searchUsersAndChats()
    searchUsers.value = response.data || response || []
  } catch (error) {
    console.error('Failed to load search users:', error)
  }
}

const loadChat = async (chat: Chat) => {
  if (openedChat.value?.id === chat.id) return

  openedChat.value = chat
  loadingMessages.value = true
  messages.value = []

  try {
    const response: any = await ChatsService.loadChatDetails(chat.id)
    messages.value = response.data || response || []
    chat.unreadMessages = 0
    scrollToBottom()
  } catch (error) {
    console.error('Failed to load chat messages:', error)
    toast.error(t('LoadMessagesFailed'))
  } finally {
    loadingMessages.value = false
  }
}

const selectSearchResult = async (user: SearchUser) => {
  showSearchResults.value = false
  searchQuery.value = ''

  if (user.chatId === 0) {
    // Create new chat
    await createChat(user)
  } else {
    // Open existing chat
    const existingChat = chatsList.value.find(c => c.id === user.chatId)
    if (existingChat) {
      loadChat(existingChat)
    }
  }
}

const createChat = async (user: SearchUser) => {
  try {
    const response: any = await ChatsService.createChat({
      isGroup: false,
      usersIds: [user.userId],
      name: ''
    })

    if (response.success !== false && response.data) {
      chatsList.value.unshift(response.data)
      openedChat.value = response.data
      messages.value = []
    }
  } catch (error) {
    console.error('Failed to create chat:', error)
    toast.error(t('CreateChatFailed'))
  }
}

const sendMessage = async () => {
  if (!messageContent.value.trim() || !openedChat.value || sending.value) return

  const content = messageContent.value
  messageContent.value = ''
  sending.value = true

  // Optimistically add message
  const tempMessage: ChatMessage = {
    id: `temp-${Date.now()}`,
    userId: '',
    userName: '',
    messageText: content,
    sentAt: new Date().toISOString(),
    hasProfilePicture: false,
    me: true
  }
  messages.value.push(tempMessage)
  scrollToBottom()

  try {
    await ChatsService.sendMessage({
      chatId: openedChat.value.id,
      messageText: content
    })
  } catch (error) {
    console.error('Failed to send message:', error)
    toast.error(t('SendMessageFailed'))
    // Remove temp message on error
    messages.value = messages.value.filter(m => m.id !== tempMessage.id)
  } finally {
    sending.value = false
  }
}

// SignalR event handlers
const handleChatChanges = async () => {
  // Reload chat list to get updated last messages and order
  try {
    const response: any = await ChatsService.listUserChats()
    chatsList.value = response.data || response || []
  } catch { /* silent */ }

  // If a chat is open, reload its messages
  if (openedChat.value) {
    try {
      const response: any = await ChatsService.loadChatDetails(openedChat.value.id)
      messages.value = response.data || response || []
      scrollToBottom()
    } catch { /* silent */ }
  }
}

const handleChatMessagesCount = () => {
  // Reload chat list to update unread counts
  loadChats()
}

const handleUserOnline = (userId: string) => {
  if (!onlineUsers.value.some(u => u.id === userId)) {
    onlineUsers.value.push({ id: userId, name: '' })
  }
}

const handleUserOffline = (userId: string) => {
  onlineUsers.value = onlineUsers.value.filter(u => u.id !== userId)
}

const setupSignalR = async () => {
  try {
    await connect()

    // Subscribe to chat events (must match backend hub event names)
    on('ChatChanges', handleChatChanges)
    on('ChatMessagesCountChanges', handleChatMessagesCount)
    on('UserOnline', handleUserOnline)
    on('UserOffline', handleUserOffline)

  } catch (error) {
    console.error('Failed to connect to SignalR:', error)
  }
}

const cleanupSignalR = () => {
  off('ChatChanges', handleChatChanges)
  off('ChatMessagesCountChanges', handleChatMessagesCount)
  off('UserOnline', handleUserOnline)
  off('UserOffline', handleUserOffline)
  disconnect()
}

// Lifecycle
onMounted(() => {
  loadChats()
  loadOnlineUsers()
  loadSearchUsers()
  setupSignalR()
})

onUnmounted(() => {
  cleanupSignalR()
})
</script>

<style scoped>
.chat-page {
  display: flex;
  height: calc(100vh - 100px);
  background: #fff;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

/* ===== Sidebar ===== */
.chat-sidebar {
  width: 360px;
  display: flex;
  flex-direction: column;
  border-inline-end: 1px solid #e2e8f0;
  background: #fafbfc;
}

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px;
  background: #ffffff;
  border-bottom: 1px solid #e4ede8;
}

.sidebar-header h2 {
  font-size: 18px;
  font-weight: 700;
  color: #1a2e25;
  margin: 0;
}

.unread-badge {
  min-width: 24px;
  height: 24px;
  padding: 0 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
  font-size: 12px;
  font-weight: 600;
  border-radius: 12px;
}

/* Search */
.search-box {
  padding: 12px 16px;
  position: relative;
}

.search-input-wrapper {
  position: relative;
}

.search-icon {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  width: 18px;
  height: 18px;
  color: #a1a1aa;
  pointer-events: none;
}

[dir="ltr"] .search-icon {
  right: auto;
  left: 12px;
}

.search-input {
  width: 100%;
  padding: 10px 40px 10px 14px;
  font-size: 14px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  background: #fff;
  transition: all 0.2s;
}

[dir="ltr"] .search-input {
  padding: 10px 14px 10px 40px;
}

.search-input:focus {
  outline: none;
  border-color: #006d4b;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

.search-results {
  position: absolute;
  top: 100%;
  left: 16px;
  right: 16px;
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
  z-index: 100;
  max-height: 250px;
  overflow-y: auto;
}

.search-result-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  cursor: pointer;
  transition: background 0.15s;
}

.search-result-item:hover {
  background: #f4f4f5;
}

.search-result-item .user-avatar.small {
  width: 32px;
  height: 32px;
  font-size: 11px;
}

.search-result-item .user-name {
  font-size: 14px;
  color: #27272a;
}

/* Loading */
.loading-state {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}

.spinner {
  width: 28px;
  height: 28px;
  border: 3px solid #e2e8f0;
  border-top-color: #006d4b;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Chat List */
.chat-list {
  flex: 1;
  overflow-y: auto;
}

.chat-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 16px;
  cursor: pointer;
  transition: background 0.15s, border-color 0.15s;
  border-bottom: 1px solid #f4f4f5;
  border-inline-start: 3px solid transparent;
}

.chat-item:hover {
  background: #fafafa;
}

.chat-item.active {
  background: rgba(0, 109, 75, 0.08);
  border-inline-start: 3px solid #006d4b;
}

/* Avatar Wrapper */
.avatar-wrapper {
  position: relative;
  flex-shrink: 0;
}

/* Avatar */
.chat-avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: #006d4b;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  flex-shrink: 0;
}

.chat-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 50%;
}

.avatar-initials {
  color: #006d4b;
  font-size: 15px;
  font-weight: 700;
  letter-spacing: 0.5px;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: #006d4b;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-weight: 600;
  font-size: 14px;
  overflow: hidden;
}

.user-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.online-indicator {
  position: absolute;
  bottom: 0;
  right: 0;
  width: 14px;
  height: 14px;
  border-radius: 50%;
  border: 2px solid #fff;
  z-index: 1;
}

[dir="rtl"] .online-indicator {
  right: auto;
  left: 0;
}

.online-indicator.online {
  background: #006d4b;
}

.online-indicator.offline {
  background: #ef4444;
}

/* Chat Info */
.chat-info {
  flex: 1;
  min-width: 0;
}

.chat-name-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 4px;
}

.chat-name {
  font-size: 14px;
  font-weight: 600;
  color: #27272a;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.chat-time {
  font-size: 11px;
  color: #a1a1aa;
  flex-shrink: 0;
}

.chat-last-message {
  font-size: 13px;
  color: #71717a;
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.chat-unread-badge {
  min-width: 20px;
  height: 20px;
  padding: 0 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
  font-size: 11px;
  font-weight: 600;
  border-radius: 10px;
  flex-shrink: 0;
}

/* Empty State */
.empty-state {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px;
  text-align: center;
  color: #a1a1aa;
}

.empty-state svg {
  width: 48px;
  height: 48px;
  margin-bottom: 12px;
}

/* ===== Main Chat Area ===== */
.chat-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: #fff;
}

/* Chat Header */
.chat-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid #e2e8f0;
  background: #fff;
}

.chat-header-user {
  display: flex;
  align-items: center;
  gap: 12px;
}

.chat-header .avatar-wrapper .chat-avatar {
  width: 44px;
  height: 44px;
}

.chat-header .avatar-wrapper .avatar-initials {
  font-size: 14px;
}

.chat-header .avatar-wrapper .online-indicator {
  width: 12px;
  height: 12px;
}

.user-info h3 {
  font-size: 15px;
  font-weight: 600;
  color: #27272a;
  margin: 0 0 2px;
}

.user-status {
  font-size: 12px;
  color: #a1a1aa;
}

.user-status.online {
  color: #006d4b;
}

.user-status.offline {
  color: #ef4444;
}

/* Messages Area */
.messages-area {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  background: #fafafa;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.loading-messages {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}

.message-wrapper {
  display: flex;
  align-items: flex-end;
  gap: 8px;
}

.message-wrapper.own {
  flex-direction: row-reverse;
}

.message-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: #006d4b;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #ffffff;
  font-size: 11px;
  font-weight: 700;
  flex-shrink: 0;
  overflow: hidden;
}

.message-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 50%;
}

.message-bubble {
  max-width: 70%;
  padding: 12px 16px;
  border-radius: 16px;
  background: #f8fafc;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
}

.message-bubble.own {
  background: #006d4b;
  color: #fff;
}

.message-text {
  font-size: 14px;
  line-height: 1.5;
  margin: 0 0 4px;
  word-wrap: break-word;
}

.message-time {
  font-size: 11px;
  color: #a1a1aa;
  display: block;
}

.message-bubble.own .message-time {
  color: rgba(255, 255, 255, 0.7);
}

.no-messages {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  color: #a1a1aa;
}

.no-messages p {
  font-size: 15px;
  margin: 0 0 4px;
}

.no-messages span {
  font-size: 13px;
}

/* Message Input */
.message-input-area {
  padding: 16px 20px;
  border-top: 1px solid #e2e8f0;
  background: #fff;
}

.input-wrapper {
  display: flex;
  align-items: flex-end;
  gap: 12px;
  background: #f4f4f5;
  border-radius: 12px;
  padding: 8px 12px;
  border: 1px solid transparent;
  transition: border-color 0.2s, background-color 0.2s;
}

.input-wrapper:focus-within {
  background: #fff;
  border-color: #006d4b;
}

.message-input {
  flex: 1;
  border: none !important;
  background: transparent !important;
  font-size: 14px;
  line-height: 1.5;
  resize: none;
  max-height: 120px;
  padding: 4px 0;
  font-family: inherit;
}

.message-input,
.message-input:focus,
.message-input:focus-visible,
.message-input:active {
  outline: none !important;
  border: none !important;
  box-shadow: 0 0 0 0 transparent !important;
  --tw-ring-shadow: 0 0 0 0 transparent !important;
  --tw-ring-offset-shadow: 0 0 0 0 transparent !important;
  --tw-ring-color: transparent !important;
}

.send-btn {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
  border: none;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s;
  flex-shrink: 0;
}

.send-btn:hover:not(:disabled) {
  background: #004730;
  transform: scale(1.05);
}

.send-btn:disabled {
  background: #d4d4d8;
  cursor: not-allowed;
}

.send-btn svg {
  width: 20px;
  height: 20px;
}

[dir="rtl"] .send-btn svg {
  transform: scaleX(-1);
}

/* No Chat Selected */
.no-chat-selected {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 40px;
  color: #71717a;
}

.empty-chat-icon {
  width: 80px;
  height: 80px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f4f4f5;
  border-radius: 20px;
  margin-bottom: 20px;
}

.empty-chat-icon svg {
  width: 40px;
  height: 40px;
  color: #a1a1aa;
}

.no-chat-selected h3 {
  font-size: 18px;
  font-weight: 600;
  color: #27272a;
  margin: 0 0 8px;
}

.no-chat-selected p {
  font-size: 14px;
  color: #a1a1aa;
  margin: 0;
}
</style>

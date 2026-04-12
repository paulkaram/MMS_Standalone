import { mainApiAxios as axios } from '@/plugins/axios'

// Helper to construct profile picture URL from userId
export const getProfilePictureUrl = (userId: number | string, hasProfilePicture?: boolean): string | null => {
  if (hasProfilePicture === false) return null
  if (!userId) return null
  const baseUrl = (import.meta.env.VITE_MAIN_API || '/api/').replace(/\/$/, '')
  return `${baseUrl}/users/profile-image/${userId}`
}

export interface ChatMember {
  userId: string
  name: string
  hasProfilePicture?: boolean
  profilePictureUrl?: string
}

export interface Chat {
  id: number
  name: string
  isGroup: boolean
  lastMessage?: string
  unreadMessages: number
  updatedAt: string
  chatMembers: ChatMember[]
}

export interface ChatMessage {
  id: string
  chatId?: number
  meetingId?: number
  userId: string
  userName: string
  messageText: string
  sentAt: string
  hasProfilePicture: boolean
  profilePictureUrl?: string
  me?: boolean
}

export interface SearchUser {
  id: string
  userId: string
  chatId: number
  name: string
  hasProfilePicture?: boolean
  profilePictureUrl?: string
}

export interface OnlineUser {
  id: string
  name: string
}

const ChatsService = {
  listOnlineUsers(): Promise<{ data: OnlineUser[] }> {
    return axios.get('chat/online')
  },

  listUserChats(): Promise<{ data: Chat[] }> {
    return axios.get('chat/list')
  },

  loadChatDetails(chatId: number): Promise<{ data: ChatMessage[] }> {
    return axios.get(`chat/details/${chatId}`)
  },

  searchUsersAndChats(): Promise<{ data: SearchUser[] }> {
    return axios.get('chat/search')
  },

  sendMessage(data: { chatId: number; messageText: string }): Promise<{ success: boolean }> {
    return axios.post('chat/chat-message', data)
  },

  createChat(data: { isGroup: boolean; usersIds: string[]; name: string }): Promise<{ success: boolean; data: Chat }> {
    return axios.post('chat', data)
  },

  getUnreadChatMessages(): Promise<{ data: number }> {
    return axios.get('chat/unread-messages-count')
  },

  // Meeting chat methods
  loadMeetingChatMessages(meetingId: number): Promise<{ data: ChatMessage[] }> {
    return axios.get(`chat/meeting-chat-details/${meetingId}`)
  },

  sendMeetingMessage(data: { meetingId: number; messageText: string }): Promise<{ success: boolean }> {
    return axios.post('chat/meeting-message', data)
  }
}

export default ChatsService

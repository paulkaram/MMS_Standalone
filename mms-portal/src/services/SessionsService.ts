import { mainApiAxios as axios } from '@/plugins/axios'

export interface SessionPostData {
  externalReferenceNumber: string
  subject: string
  note?: string
  meetingDate: string
  dueDate: string
  tags?: string
  committeeId: number
  sessionItems: SessionItemPostData[]
}

export interface SessionItemPostData {
  externalId?: string
  subject: string
  itemTypeId: number
  tags?: string
  internalNote?: string
  relatedSessionItemId?: number | null
  order: number
}

export interface SessionDto {
  id: number
  referenceNumber: string
  externalReferenceNumber: string
  subject: string
  note?: string
  meetingDate: string
  dueDate: string
  tags?: string
  committeeId: number
  committeeName?: string
  createdBy: string
  createdDate: string
  sessionItems: SessionItemDto[]
}

export interface SessionItemDto {
  id: number
  externalId?: string
  subject: string
  itemTypeId: number
  itemTypeName?: string
  tags?: string
  internalNote?: string
  relatedSessionItemId?: number | null
  relatedSessionItemSubject?: string
  order: number
}

export interface ListItem {
  id: number
  name: string
}

const SessionsService = {
  createSession(data: SessionPostData) {
    return axios.post('sessions', data)
  },

  getSession(id: number) {
    return axios.get(`sessions/${id}`)
  },

  listCommittees() {
    return axios.get('sessions/committees')
  },

  listItemTypes() {
    return axios.get('sessions/item-types')
  },

  searchSessionItems(search: string) {
    return axios.get(`sessions/search-items?search=${encodeURIComponent(search)}`)
  },

  addAttachment(sessionId: number, formData: FormData, privacyId: number = 1) {
    return axios.post(`sessions/${sessionId}/attachments?privacyId=${privacyId}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  },

  deleteAttachment(sessionId: number, attachmentId: number) {
    return axios.delete(`sessions/${sessionId}/attachments/${attachmentId}`)
  },

  downloadAttachment(sessionId: number, attachmentId: number) {
    return axios.get(`sessions/${sessionId}/attachments/${attachmentId}/download`, {
      responseType: 'blob'
    })
  },

  listAttachments(sessionId: number) {
    return axios.get(`sessions/${sessionId}/attachments`)
  }
}

export default SessionsService

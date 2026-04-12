import { mainApiAxios as axios } from '@/plugins/axios'

export interface Recommendation {
  id: string
  meetingId: string
  meetingReferenceNo: string
  agendaId: string
  text: string
  status: string
  statusId?: number
  statusName?: string
  percentage: number
  owner: string
  ownerName: string
  createdBy: string
  createdByName: string
  createdAt: string
  dueDate: string
  completedAt?: string
  notes?: string
}

export interface RecommendationSearchParams {
  meetingReferenceNo?: string | null
  fromDate?: string | null
  toDate?: string | null
  title?: string | null
  statusId?: string | null
  onlyMyRecommendations?: boolean
}

export interface PaginatedResponse<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
}

export interface RecommendationNote {
  id: string
  recommendationId: string
  text: string
  createdBy: string
  createdByName: string
  createdAt: string
}

export interface RecommendationAttachment {
  id: string
  recommendationId: string
  fileName: string
  fileType: string
  fileSize: number
  uploadedBy: string
  uploadedByName: string
  uploadedAt: string
}

export interface UpdateRecommendationRequest {
  id: number
  progress: number
  statusId: number | null
}

const RecommendationsService = {
  /**
   * Search recommendations with pagination
   */
  searchRecommendations(
    params: RecommendationSearchParams,
    page: number = 1,
    pageSize: number = 10
  ): Promise<PaginatedResponse<Recommendation>> {
    return axios.post(`meetingAgendaRecommendations/search/${page}/${pageSize}`, params)
  },

  /**
   * Get recommendation details by ID
   */
  getRecommendation(id: string): Promise<Recommendation> {
    return axios.get(`meetingAgendaRecommendations/${id}`)
  },

  /**
   * Update recommendation progress and status
   */
  updateRecommendation(id: string, data: UpdateRecommendationRequest): Promise<any> {
    return axios.put('meetingAgendaRecommendations', data)
  },

  // ==================== Notes ====================

  /**
   * Get recommendation notes with pagination
   */
  getNotes(recommendationId: string, page: number = 1, pageSize: number = 10): Promise<RecommendationNote[]> {
    return axios.get(`meetingAgendaRecommendations/${recommendationId}/${page}/${pageSize}/recommendation-notes`)
  },

  /**
   * Add a new note to recommendation
   */
  addNote(data: { recommendationId: number; text: string }): Promise<RecommendationNote> {
    return axios.post('meetingAgendaRecommendations/recommendation-notes', data)
  },

  /**
   * Edit an existing note
   */
  editNote(data: { id: string; text: string }): Promise<RecommendationNote> {
    return axios.put('meetingAgendaRecommendations/recommendation-notes', data)
  },

  /**
   * Delete a note
   */
  deleteNote(noteId: string): Promise<void> {
    return axios.delete(`meetingAgendaRecommendations/${noteId}/recommendation-notes`)
  },

  // ==================== Attachments ====================

  /**
   * Get recommendation attachments with pagination
   */
  getAttachments(recommendationId: string, page: number = 1, pageSize: number = 10): Promise<RecommendationAttachment[]> {
    return axios.get(`meetingAgendaRecommendations/${recommendationId}/${page}/${pageSize}/recommendation-attachments`)
  },

  /**
   * Add attachment to recommendation
   */
  addAttachment(formData: FormData): Promise<RecommendationAttachment> {
    return axios.post('meetingAgendaRecommendations/recommendation-attachments', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  },

  /**
   * Delete an attachment
   */
  deleteAttachment(attachmentId: string): Promise<void> {
    return axios.delete(`meetingAgendaRecommendations/${attachmentId}/recommendation-attachments`)
  },

  /**
   * Download attachment
   */
  downloadAttachment(attachmentId: string): Promise<Blob> {
    return axios.get(`meetingAgendaRecommendations/${attachmentId}/download`, {
      responseType: 'blob'
    })
  }
}

export default RecommendationsService

import { mainApiAxios as axios } from '@/plugins/axios'

export interface MeetingAgenda {
  id: number
  meetingId: number
  title: string
  description?: string
  duration: number
  order: number
  isRunning: boolean
  paused: boolean
  remainingSeconds: number
  actualStartDate?: string
  actualEndDate?: string
  summary?: string
  votingType?: {
    id: number
    nameAr: string
    nameEn: string
    votingOptions: VotingOption[]
  }
  meetingUserVotes: UserVote[]
  attachments: any[]
}

export interface VotingOption {
  id: number
  nameAr: string
  nameEn: string
  active: boolean
}

export interface UserVote {
  id: number
  userId: string
  vottingOptionId: number
  meetingAgendaId: number
}

export interface AgendaNote {
  id: number
  meetingAgendaId: number
  text: string
  isPublic: boolean
  createdBy: string
  createdByName?: string
  createdAt: string
}

export interface AgendaRecommendation {
  id: number
  meetingAgendaId: number
  text: string
  description?: string
  owner: string
  ownerName?: string
  dueDate?: string
  statusId?: number
  statusName?: string
  percentage?: number
  createdBy?: string
  createdByName?: string
  priorityId?: number
  priorityName?: string
  ownerStructureId?: number
  ownerStructureName?: string
}

export interface RecommendationFormData {
  meetingAgendaId: number
  text: string
  description?: string
  owner: string
  dueDate?: string
  priorityId?: number
  ownerStructureId?: number
}

const MeetingAgendaService = {
  // Get agenda items for a meeting
  getAgendaItems(meetingId: number): Promise<MeetingAgenda[]> {
    return axios.get(`meetings/${meetingId}/agendas`)
  },

  // Send vote for an agenda item
  sendVote(data: {
    votingOptionId: number
    meetingAgendaId: number
  }): Promise<UserVote[]> {
    return axios.post('meetingAgenda/meeting-agenda-vote', data)
  },

  // Add note to agenda
  addNote(data: { meetingAgendaId: number; text: string; isPublic: boolean }): Promise<AgendaNote> {
    return axios.post('meetingAgenda/note', data)
  },

  // Get notes for an agenda item
  getNotes(agendaId: number): Promise<AgendaNote[]> {
    return axios.get(`meetingAgenda/${agendaId}/note`)
  },

  // Edit note
  editNote(data: { id: number; meetingAgendaId: number; text: string; isPublic: boolean }): Promise<AgendaNote> {
    return axios.put('meetingAgenda/note', data)
  },

  // Delete note
  deleteNote(noteId: number): Promise<void> {
    return axios.delete(`meetingAgenda/${noteId}/note`)
  },

  // Add recommendation to agenda
  addRecommendation(data: RecommendationFormData): Promise<AgendaRecommendation> {
    return axios.post('meetingAgenda/recommendation', data)
  },

  // Get recommendations for an agenda item
  getRecommendations(agendaId: number): Promise<AgendaRecommendation[]> {
    return axios.get(`meetingAgenda/${agendaId}/recommendation`)
  },

  // Edit recommendation
  editRecommendation(data: RecommendationFormData & { id: number }): Promise<AgendaRecommendation> {
    return axios.put('meetingAgenda/recommendation', data)
  },

  // Delete recommendation
  deleteRecommendation(recommendationId: number): Promise<void> {
    return axios.delete(`meetingAgenda/${recommendationId}/recommendation`)
  },

  // Get voting results for an agenda
  getVotingResults(agendaId: number): Promise<any> {
    return axios.get(`meetingAgenda/${agendaId}/meeting-agenda-vote-results`)
  },

  // Update agenda item
  updateAgenda(
    agendaId: number,
    data: Partial<MeetingAgenda>
  ): Promise<MeetingAgenda> {
    return axios.put(`meetings/agendas/${agendaId}`, data)
  },

  // Delete agenda item
  deleteAgenda(agendaId: number): Promise<void> {
    return axios.delete(`meetings/agendas/${agendaId}`)
  },

  // Reorder agenda items
  reorderAgendas(
    meetingId: number,
    agendaIds: number[]
  ): Promise<void> {
    return axios.put(`meetings/${meetingId}/agendas/reorder`, { agendaIds })
  },

  // Add attachment to agenda
  addAttachment(agendaId: number, formData: FormData): Promise<any> {
    return axios.post(`meetings/agendas/${agendaId}/attachments`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
  },

  // Delete attachment from agenda
  deleteAttachment(agendaId: number, attachmentId: number): Promise<void> {
    return axios.delete(`meetings/agendas/${agendaId}/attachments/${attachmentId}`)
  },

  // Start next agenda item (or first if none running)
  startNextAgenda(meetingId: number): Promise<any> {
    return axios.put(`meetings/${meetingId}/next-agenda`)
  },

  // Pause or resume the current agenda
  pauseResumeAgenda(meetingId: number): Promise<any> {
    return axios.put(`meetings/${meetingId}/pause-resume`)
  },

  // Get agenda summary
  getAgendaSummary(agendaId: number): Promise<string> {
    return axios.get(`meetingAgenda/${agendaId}/summary`)
  },

  // Save agenda summary
  saveAgendaSummary(agendaId: number, summary: string): Promise<void> {
    return axios.post(`meetingAgenda/${agendaId}/summary`, summary, {
      headers: { 'Content-Type': 'application/json' }
    })
  }
}

export default MeetingAgendaService

import { mainApiAxios as axios } from '@/plugins/axios'

export interface Meeting {
  id: string
  title: string
  titleAr: string
  titleEn?: string
  description?: string
  date: string
  startTime: string
  endTime?: string
  location?: string
  locationDetails?: string
  councilId: string
  councilName?: string
  committeeId?: string
  committeeName?: string
  status?: string
  statusId: number
  statusName?: string
  meetingTypeId: string
  meetingTypeName?: string
  referenceNumber?: string
  isVirtual: boolean
  virtualMeetingUrl?: string
  createdBy: string
  createdById?: string
  createdByName?: string
  createdAt: string
  updatedAt?: string
  attendeesCount?: number
  agendaItemsCount?: number
}

export interface MeetingListParams {
  page?: number
  pageSize?: number
  search?: string
  councilId?: string
  committeeId?: string
  status?: string
  fromDate?: string
  toDate?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface Attendee {
  id: string
  meetingId: string
  userId: string
  userName: string
  userFullName: string
  roleId: string
  roleName: string
  attendanceStatus: string
  attendanceStatusName?: string
  isRequired: boolean
  notes?: string
  checkedInAt?: string
  checkedOutAt?: string
}

export interface AgendaItem {
  id: string
  meetingId: string
  title: string
  titleAr: string
  titleEn?: string
  description?: string
  order: number
  duration?: number
  presenterId?: string
  presenterName?: string
  status: string
  statusName?: string
  type: string
  typeName?: string
  parentId?: string
  attachmentsCount?: number
  recommendationsCount?: number
}

export interface MeetingAttachment {
  id: string
  meetingId: string
  agendaId?: string
  fileName: string
  fileType: string
  fileSize: number
  uploadedBy: string
  uploadedByName?: string
  uploadedAt: string
  privacyId: string
  privacyName?: string
}

export interface MeetingUserApproval {
  id: number
  userId: string
  userName: string
  statusId: number
  statusName: string
  approveDate?: string
  comment?: string
  attachmentId?: number
  version?: number
}

// Task status constants matching backend TaskStatusDbEnum
export const TaskStatus = {
  PendingApproval: 1,
  Approved: 2,
  Rejected: 3,
  Cancelled: 4
} as const

const MeetingsService = {
  // Meeting CRUD
  listMeetings(params: MeetingListParams = {}): Promise<PaginatedResponse<Meeting>> {
    const queryParams = new URLSearchParams()
    if (params.page) queryParams.append('page', params.page.toString())
    if (params.pageSize) queryParams.append('pageSize', params.pageSize.toString())
    if (params.search) queryParams.append('search', params.search)
    if (params.councilId) queryParams.append('councilId', params.councilId)
    if (params.committeeId) queryParams.append('committeeId', params.committeeId)
    if (params.status) queryParams.append('status', params.status)
    if (params.fromDate) queryParams.append('fromDate', params.fromDate)
    if (params.toDate) queryParams.append('toDate', params.toDate)
    if (params.sortBy) queryParams.append('sortBy', params.sortBy)
    if (params.sortOrder) queryParams.append('sortOrder', params.sortOrder)

    return axios.get(`meetings?${queryParams.toString()}`)
  },

  // Search meetings for autocomplete
  listMeetingsForAutoComplete(search: string): Promise<any> {
    return axios.get(`meetings?search=${encodeURIComponent(search)}&mode=Autocomplete`)
  },

  getMyMeetings(params: MeetingListParams = {}): Promise<PaginatedResponse<Meeting>> {
    const queryParams = new URLSearchParams()
    if (params.page) queryParams.append('page', params.page.toString())
    if (params.pageSize) queryParams.append('pageSize', params.pageSize.toString())
    if (params.status) queryParams.append('status', params.status)

    return axios.get(`meetings/my-meetings?${queryParams.toString()}`)
  },

  getDraftMeetings(page: number = 1, pageSize: number = 10): Promise<PaginatedResponse<Meeting>> {
    return axios.get(`meetings/user/drafts/${page}/${pageSize}`)
  },

  getMeeting(meetingId: string): Promise<Meeting> {
    return axios.get(`meetings/${meetingId}`)
  },

  createMeeting(meeting: Partial<Meeting>): Promise<Meeting> {
    return axios.post('meetings', meeting)
  },

  updateMeeting(meetingId: string, meeting: Partial<Meeting>): Promise<Meeting> {
    return axios.put(`meetings/${meetingId}`, meeting)
  },

  deleteMeeting(meetingId: string): Promise<void> {
    return axios.delete(`meetings/${meetingId}`)
  },

  // Meeting status actions
  publishMeeting(meetingId: string): Promise<void> {
    return axios.post(`meetings/${meetingId}/publish`)
  },

  cancelMeeting(meetingId: string, reason?: string): Promise<void> {
    return axios.post(`meetings/${meetingId}/cancelMeeting`, { reason })
  },

  startMeeting(meetingId: string | number): Promise<void> {
    // Starting a meeting is done by calling next-agenda which starts the first agenda
    return axios.put(`meetings/${meetingId}/next-agenda`)
  },

  endMeeting(meetingId: string | number): Promise<void> {
    return axios.put(`meetings/${meetingId}/end-meeting`)
  },

  // Attendees
  getAttendees(meetingId: string): Promise<Attendee[]> {
    return axios.get(`meetings/${meetingId}/attendees`)
  },

  addAttendee(meetingId: string | number, attendee: Partial<Attendee>): Promise<any> {
    return axios.post(`meetings/${meetingId}/meeting-attendee`, attendee)
  },

  updateAttendee(meetingId: string, attendeeId: string, attendee: Partial<Attendee>): Promise<Attendee> {
    return axios.put(`meetings/${meetingId}/meeting-attendee`, attendee)
  },

  removeAttendee(meetingId: string, attendeeId: string): Promise<void> {
    return axios.delete(`meetings/${meetingId}/meeting-attendee/${attendeeId}`)
  },

  checkInAttendee(meetingId: string, attendeeId: string): Promise<void> {
    return axios.post(`meetings/${meetingId}/attendees/${attendeeId}/check-in`)
  },

  checkOutAttendee(meetingId: string, attendeeId: string): Promise<void> {
    return axios.post(`meetings/${meetingId}/attendees/${attendeeId}/check-out`)
  },

  // Agenda
  getAgendaItems(meetingId: string): Promise<AgendaItem[]> {
    return axios.get(`meetings/${meetingId}/agendas`)
  },

  addAgendaItem(meetingId: string | number, item: Partial<AgendaItem>): Promise<any> {
    return axios.post(`meetings/${meetingId}/meeting-agenda`, item)
  },

  updateAgendaItem(meetingId: string | number, item: Partial<AgendaItem>): Promise<any> {
    return axios.put(`meetings/${meetingId}/meeting-agenda`, item)
  },

  deleteAgendaItem(meetingId: string | number, itemId: string | number): Promise<any> {
    return axios.delete(`meetings/${meetingId}/meeting-agenda/${itemId}`)
  },

  reorderAgendaItems(meetingId: string, itemIds: string[]): Promise<void> {
    return axios.post(`meetings/${meetingId}/agenda/reorder`, { itemIds })
  },

  // Attachments
  getAttachments(meetingId: string): Promise<MeetingAttachment[]> {
    return axios.get(`meetings/${meetingId}/meeting-attachments`)
  },

  addAttachment(meetingId: string, formData: FormData, privacyId: string, agendaId?: string): Promise<MeetingAttachment> {
    let url = `meetings/${meetingId}/meeting-attachment?privacyId=${privacyId}`
    if (agendaId) url += `&agendaId=${agendaId}`

    return axios.post(url, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
  },

  deleteAttachment(meetingId: string, attachmentId: string): Promise<void> {
    return axios.delete(`meetings/${meetingId}/meeting-attachment/${attachmentId}`)
  },

  downloadAttachment(meetingId: string, attachmentId: string): Promise<Blob> {
    return axios.get(`meetings/${meetingId}/meeting-attachment/${attachmentId}/download`, {
      responseType: 'blob'
    })
  },

  // Minutes
  getMeetingMinutes(meetingId: string): Promise<{ content: string; lastUpdated?: string }> {
    return axios.get(`meetings/${meetingId}/minutes`)
  },

  saveMeetingMinutes(meetingId: string, content: string): Promise<void> {
    return axios.put(`meetings/${meetingId}/minutes`, { content })
  },

  exportMeetingMinutes(meetingId: string, format: 'pdf' | 'docx' = 'pdf'): Promise<Blob> {
    return axios.get(`meetings/${meetingId}/minutes/export?format=${format}`, {
      responseType: 'blob'
    })
  },

  // Invitations
  sendInvitations(meetingId: string, attendeeIds?: string[]): Promise<void> {
    return axios.post(`meetings/${meetingId}/send-invitations`, { attendeeIds })
  },

  // Clone meeting
  cloneMeeting(meetingId: string): Promise<Meeting> {
    return axios.post(`meetings/${meetingId}/clone`)
  },

  // Associated Meetings
  getAssociatedMeetings(meetingId: string): Promise<any> {
    return axios.get(`meetings/${meetingId}/associated-meeting`)
  },

  addAssociatedMeeting(meetingId: string, associatedMeetingId: string): Promise<any> {
    return axios.post(`meetings/${meetingId}/associated-meeting/${associatedMeetingId}`, {})
  },

  removeAssociatedMeeting(meetingId: string, associatedMeetingId: string): Promise<any> {
    return axios.delete(`meetings/${meetingId}/associated-meeting/${associatedMeetingId}`)
  },

  // Meeting Actions
  sendMeeting(meetingId: string): Promise<void> {
    return axios.post(`meetings/${meetingId}/send-meeting`)
  },

  approveMeeting(meetingId: string): Promise<void> {
    return axios.post(`meetings/${meetingId}/approve-meeting`)
  },

  // Save/Update Meeting Info
  saveMeetingInfo(meeting: Partial<Meeting>): Promise<Meeting> {
    return axios.post('meetings/meeting-info', meeting)
  },

  updateMeetingInfo(meeting: Partial<Meeting>): Promise<Meeting> {
    return axios.put('meetings/meeting-info', meeting)
  },

  // Load full meeting
  loadMeeting(meetingId: string): Promise<Meeting & {
    meetingAttendees: any[]
    meetingAgendas: any[]
    associatedMeetings: any[]
    attachments: any[]
  }> {
    return axios.get(`meetings/${meetingId}`)
  },

  // Live Meeting Dashboard
  loadLiveMeeting(meetingId: number): Promise<any> {
    return axios.get(`meetings/${meetingId}/live-meeting`)
  },

  // Agenda Control
  nextAgenda(meetingId: number): Promise<void> {
    return axios.put(`meetings/${meetingId}/next-agenda`)
  },

  pauseOrResume(meetingId: number): Promise<void> {
    return axios.put(`meetings/${meetingId}/pause-resume`)
  },

  // Meeting Minutes
  getInitialMeetingMinutes(meetingId: number): Promise<any> {
    return axios.get(`meetings/${meetingId}/initial-meeting-minutes`)
  },

  getFinalMeetingMinutes(meetingId: number): Promise<any> {
    return axios.get(`meetings/${meetingId}/final-meeting-minutes`)
  },

  getInitialMeetingMinutesVersions(meetingId: number): Promise<any[]> {
    return axios.get(`meetings/${meetingId}/initial-meeting-minutes-versions`)
  },

  approveInitialMeetingMinutes(meetingId: number): Promise<any> {
    return axios.put(`meetings/${meetingId}/approve-initial-meeting-minutes`)
  },

  approveFinalMeetingMinutes(meetingId: number): Promise<any> {
    return axios.put(`meetings/${meetingId}/approve-final-meeting-minutes`)
  },

  generateMeetingMinutes(meetingId: number, type: 'initial' | 'final'): Promise<any> {
    return axios.post(`meetings/${meetingId}/generate-meeting-minutes`, { type })
  },

  sendInitialMeetingMinutesForApproval(meetingId: number, usersIds: string[], dueDate?: number): Promise<void> {
    // Backend expects: MeetingMinutesTaskDto(int MeetingId, List<string> UsersIds, int DueDate)
    // DueDate is stored as an integer (e.g., days from now or 0 for no due date)
    const payload = {
      meetingId,
      usersIds,
      dueDate: dueDate ?? 0
    }
    return axios.post(`meetings/send-initial-Meeting-Minutes`, payload)
  },

  sendFinalMeetingMinutesForApproval(meetingId: number, userIds: string[], dueDateDays?: number, signatureUserIds?: string[]): Promise<any> {
    // Backend expects: MeetingMinutesTaskDto(int MeetingId, List<string> UsersIds, int DueDate)
    const payload: any = {
      meetingId,
      usersIds: userIds,
      dueDate: dueDateDays ?? 0
    }
    if (signatureUserIds && signatureUserIds.length > 0) {
      payload.signatureUserIds = signatureUserIds
    }
    return axios.post(`meetings/send-final-Meeting-Minutes`, payload)
  },

  // Get users who already have pending approval tasks for the current MOM version
  getInitialMeetingMinutesUsers(meetingId: number): Promise<string[]> {
    return axios.get(`meetings/${meetingId}/initial-meeting-minutes-users`)
  },

  // Get approval statuses with comments for a specific attachment
  getMeetingMinutesApprovals(attachmentId: number): Promise<MeetingUserApproval[]> {
    return axios.get(`meetings/${attachmentId}/meeting-minutes-users-approvals`)
  },

  // Get all meeting approvals (for meeting approval workflow)
  getMeetingApprovals(meetingId: number): Promise<MeetingUserApproval[]> {
    return axios.get(`meetings/${meetingId}/meeting-user-approvals`)
  },

  // Generate final MOM
  generateFinalMeetingMinutes(meetingId: number): Promise<any> {
    return axios.post(`meetings/${meetingId}/final-meeting-minutes-generate`)
  },

  // Get Final MOM versions
  getFinalMeetingMinutesVersions(meetingId: number): Promise<any[]> {
    return axios.get(`meetings/${meetingId}/final-meeting-minutes-versions`)
  },

  // Upload Final MOM file
  uploadFinalMeetingMinutes(meetingId: number, file: File): Promise<any> {
    const formData = new FormData()
    formData.append('file', file)
    return axios.post(`meetings/${meetingId}/upload-final-meeting-minutes`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  },

  // Get users who already have pending approval tasks for the Final MOM
  getFinalMeetingMinutesUsers(meetingId: number): Promise<string[]> {
    return axios.get(`meetings/${meetingId}/final-meeting-minutes-users`)
  },

  // Get Final MOM signature statuses (uses meetingId, not attachmentId)
  getFinalMeetingMinutesSignatures(meetingId: number): Promise<MeetingUserApproval[]> {
    return axios.get(`meetings/${meetingId}/meeting-minutes-users-sign`)
  },

  // Meeting Summary
  getMeetingSummary(meetingId: number): Promise<string> {
    return axios.get(`meetings/${meetingId}/meeting-summary`)
  },

  saveMeetingSummary(meetingId: number, summary: string): Promise<void> {
    return axios.post(`meetings/${meetingId}/meeting-summary`, summary, {
      headers: { 'Content-Type': 'application/json' }
    })
  },

  // Audio Transcripts
  uploadAudioRecording(meetingId: number, audioBlob: Blob, options?: { agendaId?: number, language?: string, attendeeName?: string }): Promise<any> {
    const formData = new FormData()
    formData.append('audioFile', audioBlob, 'recording.webm')
    if (options?.agendaId) formData.append('agendaId', options.agendaId.toString())
    if (options?.language) formData.append('language', options.language)
    if (options?.attendeeName) formData.append('attendeeName', options.attendeeName)
    return axios.post(`meetings/${meetingId}/transcripts`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      timeout: 300000
    })
  },
  getTranscripts(meetingId: number): Promise<any> {
    return axios.get(`meetings/${meetingId}/transcripts`)
  },
  getCombinedTranscript(meetingId: number): Promise<any> {
    return axios.get(`meetings/${meetingId}/transcripts/combined`)
  },
  generateCombinedSummary(meetingId: number): Promise<any> {
    return axios.post(`meetings/${meetingId}/transcripts/combined/summarize`)
  },
  generateTranscriptSummary(meetingId: number, transcriptId: number): Promise<any> {
    return axios.post(`meetings/${meetingId}/transcripts/${transcriptId}/summarize`)
  },
  deleteTranscript(meetingId: number, transcriptId: number): Promise<any> {
    return axios.delete(`meetings/${meetingId}/transcripts/${transcriptId}`)
  },

  // Calendar - Get meetings for calendar view
  listMeetingsForCalendar(params: { StartDate: string; EndDate: string }): Promise<any> {
    return axios.post('meetings/calender-search', params)
  },

  // Search meetings with filters
  searchUserMeetings(params: {
    statusId?: number | null
    committeeId?: number | null
    meetingId?: number | null
    fromDate?: string | null
    toDate?: string | null
    location?: string | null
    title?: string | null
    onlyMyMeetings?: boolean
    noComitteeRelated?: boolean
    includeDrafts?: boolean
  }, page: number = 1, pageSize: number = 10): Promise<{ data: Meeting[]; total: number }> {
    return axios.post(`meetings/search/${page}/${pageSize}`, params)
  },

  getAttachmentQuery(attachmentId: number): Promise<string> {
    return axios.get(`attachments/${attachmentId}`)
  },

  // ═══════════════════════════════════════════════════════════════════════════
  // PDF-based Minutes of Meeting
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Gets meeting minutes data for preview (without saving)
   * Use this to show a preview before generating the final PDF
   */
  getMinutesPreview(meetingId: number, includePrivateNotes: boolean = false): Promise<any> {
    return axios.get(`meetings/${meetingId}/minutes-preview?includePrivateNotes=${includePrivateNotes}`)
  },

  /**
   * Generates a PDF-based Minutes of Meeting document
   * This is the new improved method that generates PDFs directly without Word templates
   */
  generatePdfMinutes(meetingId: number, options: {
    includePrivateNotes?: boolean
    includeVoterNames?: boolean
    language?: 'ar' | 'en'
  } = {}): Promise<{
    success: boolean
    attachmentId?: number
    fileName?: string
    version: number
    message?: string
  }> {
    return axios.post(`meetings/${meetingId}/generate-pdf-minutes`, {
      meetingId,
      includePrivateNotes: options.includePrivateNotes ?? false,
      includeVoterNames: options.includeVoterNames ?? true,
      language: options.language ?? 'ar'
    })
  },

  /**
   * Downloads the generated PDF minutes
   */
  downloadPdfMinutes(attachmentId: number): Promise<Blob> {
    return axios.get(`attachments/${attachmentId}/download`, {
      responseType: 'blob'
    })
  }
}

export default MeetingsService

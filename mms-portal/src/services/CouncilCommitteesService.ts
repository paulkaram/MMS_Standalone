import { mainApiAxios as axios } from '@/plugins/axios'

export interface Council {
  id: string
  name: string
  nameAr: string
  nameEn?: string
  type: string
  typeId?: string
  isActive: boolean
  children?: Committee[]
}

export interface Committee {
  id: string
  name: string
  nameAr: string
  nameEn?: string
  councilId: string
  typeId?: string
  isActive: boolean
  parentId?: string
  children?: Committee[]
}

export interface CommitteeUser {
  id: string
  userId: string
  userName: string
  fullName: string
  email?: string
  roleId: string
  roleName?: string
  privacyId?: string
  privacyName?: string
  isActive: boolean
  note?: string
  jobTitle?: string
}

export interface CommitteeDuty {
  id: string
  committeeId: string
  title: string
  description?: string
}

export interface CommitteeActivity {
  id: string
  committeeId: string
  title: string
  description?: string
}

export interface CommitteeAttachment {
  id: string
  committeeId: string
  fileName: string
  fileType: string
  fileSize: number
  privacyId: string
  privacyName?: string
  uploadedAt: string
}

const CouncilCommitteesService = {
  // Organization Structure
  listOrganization(onlyActive: boolean = true): Promise<any> {
    return axios.get(`councilCommittees/organization?onlyActive=${onlyActive}`)
  },

  listCouncilCommittees(): Promise<Council[]> {
    return axios.get('councilCommittees')
  },

  // Council/Committee CRUD
  getCouncilCommittee(councilCommitteeId: string): Promise<any> {
    return axios.get(`councilCommittees/${councilCommitteeId}`)
  },

  addCouncilCommittee(councilCommitteeObj: FormData): Promise<any> {
    return axios.post('councilCommittees', councilCommitteeObj, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
  },

  updateCouncilCommittee(councilCommitteeId: string, councilCommitteeObj: FormData): Promise<any> {
    return axios.put(`councilCommittees/${councilCommitteeId}`, councilCommitteeObj, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
  },

  // User Committees
  listUserCommittees(): Promise<Committee[]> {
    return axios.get('councilCommittees/user-committees')
  },

  listUserCommitteesForMeeting(): Promise<Committee[]> {
    return axios.get('councilCommittees/meeting-user-committees')
  },

  // Committee Users
  listUsersInCouncilCommittee(councilCommitteeId: string): Promise<CommitteeUser[]> {
    return axios.get(`councilCommittees/${councilCommitteeId}/users`)
  },

  listUsersInCouncilCommitteeGeneralInfo(
    councilCommitteeId: string,
    page: number,
    pageSize: number
  ): Promise<{ data: any[]; total: number }> {
    return axios.get(`councilCommittees/${councilCommitteeId}/${page}/${pageSize}/users-general-info`)
  },

  addUserToCouncilCommittee(formData: any): Promise<CommitteeUser> {
    return axios.post('councilCommittees/add-user', formData)
  },

  updateCommitteeUser(councilCommitteeId: string, formData: any): Promise<CommitteeUser> {
    return axios.put(`councilCommittees/${councilCommitteeId}/update-user`, formData)
  },

  removeUserFromCouncilCommittee(councilCommitteeId: string, userId: string): Promise<void> {
    return axios.delete(`councilCommittees/${councilCommitteeId}/user/${userId}`)
  },

  // Committee Duties
  listCommitteeDuties(councilCommitteeId: string): Promise<CommitteeDuty[]> {
    return axios.get(`councilCommittees/${councilCommitteeId}/duties`)
  },

  addCommitteeDuty(councilCommitteeId: string, dutyObj: { title: string; description?: string }): Promise<CommitteeDuty> {
    return axios.post(`councilCommittees/${councilCommitteeId}/duty`, dutyObj)
  },

  updateCommitteeDuty(committeeDutyId: string, dutyObj: { title: string; description?: string }): Promise<CommitteeDuty> {
    return axios.put(`councilCommittees/duty/${committeeDutyId}`, dutyObj)
  },

  removeDuty(dutyId: string): Promise<void> {
    return axios.delete(`councilCommittees/duty/${dutyId}`)
  },

  // Committee Activities
  listCommitteeActivities(councilCommitteeId: string): Promise<CommitteeActivity[]> {
    return axios.get(`councilCommittees/${councilCommitteeId}/Activities`)
  },

  listCommitteeActivitiesGeneralInfo(
    councilCommitteeId: string,
    page: number,
    pageSize: number
  ): Promise<{ data: any[]; total: number }> {
    return axios.get(`councilCommittees/${councilCommitteeId}/${page}/${pageSize}/activities-general-info`)
  },

  addCommitteeActivity(councilCommitteeId: string, activityObj: { title: string; description?: string }): Promise<CommitteeActivity> {
    return axios.post(`councilCommittees/${councilCommitteeId}/activity`, activityObj)
  },

  updateCommitteeActivity(committeeActivityId: string, activityObj: { title: string; description?: string }): Promise<CommitteeActivity> {
    return axios.put(`councilCommittees/activity/${committeeActivityId}`, activityObj)
  },

  removeActivity(activityId: string): Promise<void> {
    return axios.delete(`councilCommittees/activity/${activityId}`)
  },

  // Committee Attachments
  listCommitteeAttachments(councilCommitteeId: string): Promise<CommitteeAttachment[]> {
    return axios.get(`councilCommittees/${councilCommitteeId}/attachments`)
  },

  listCommitteeAttachmentsGeneralInfo(
    councilCommitteeId: string,
    page: number,
    pageSize: number
  ): Promise<{ data: any[]; total: number }> {
    return axios.get(`councilCommittees/${councilCommitteeId}/${page}/${pageSize}/attachments-general-info`)
  },

  addCommitteeAttachment(councilCommitteeId: string, formData: FormData, privacyId: string): Promise<CommitteeAttachment> {
    return axios.post(`councilCommittees/${councilCommitteeId}/attachment?privacyId=${privacyId}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
  },

  deleteCommitteeAttachment(attachmentId: string): Promise<void> {
    return axios.delete(`councilCommittees/${attachmentId}/attachments`)
  },

  // Financial Compensation
  hasFinancialCompensationCommittee(councilCommitteeId: string): Promise<any> {
    return axios.get(`councilCommittees/${councilCommitteeId}/Committee-Financial-Compensation`)
  },

  saveHasFinancialCompensation(councilCommitteeId: string, hasFinancialCompensation: boolean): Promise<void> {
    return axios.post(`councilCommittees/${councilCommitteeId}/Financial-Compensation?hasFinancialCompensation=${hasFinancialCompensation}`)
  },

  // User Permissions
  getUserCommitteePermissions(councilCommitteeId: string, userId: string): Promise<any> {
    return axios.get(`councilCommittees/${councilCommitteeId}/user-permissions/${userId}`)
  },

  getMyCommitteesPermissions(councilCommitteeId: string): Promise<{
    users: boolean
    meetings: boolean
    recommendations: boolean
    committeeActivities: boolean
    committeeAttachments: boolean
    committeeAttachmentButtonAdd: boolean
  }> {
    return axios.get(`councilCommittees/${councilCommitteeId}/my-permissions`)
  },

  updatePermission(councilCommitteeId: string, formData: any): Promise<void> {
    return axios.put(`councilCommittees/${councilCommitteeId}/user-permissions`, formData)
  },

  // General Info Methods
  listUserCouncilsAndCommitteesForGeneralInfo(searchCriteria: {
    committeeTitle?: string
    meetingTitle?: string
    agendaTitle?: string
    agendaTopicTitle?: string
    recommendationTitle?: string
    agendaNote?: string
  }): Promise<any[]> {
    return axios.post('councilCommittees/user-committees/general-info', searchCriteria)
  },

  getCommitteesByCouncilId(councilCommitteeId: string): Promise<any[]> {
    return axios.get(`councilCommittees/${councilCommitteeId}/user-committees`)
  },

  getCommitteesParents(councilCommitteeId: string): Promise<any[]> {
    return axios.get(`councilCommittees/${councilCommitteeId}/parents`)
  },

  // Tasks and Meetings
  listCouncilCommitteeTasks(
    councilCommitteeId: string,
    page: number,
    pageSize: number
  ): Promise<{ data: any[]; total: number }> {
    return axios.get(`councilCommittees/${councilCommitteeId}/${page}/${pageSize}/tasks`)
  },

  listCouncilCommitteeMeetings(
    councilCommitteeId: string,
    page: number,
    pageSize: number
  ): Promise<{ data: any[]; total: number }> {
    return axios.get(`councilCommittees/${councilCommitteeId}/${page}/${pageSize}/meetings`)
  },

  // Self-service endpoints (no permission required, uses JWT)
  listMyOrganization(onlyActive: boolean = true): Promise<any> {
    return axios.get(`councilCommittees/my-organization?onlyActive=${onlyActive}`)
  },

  getMyAdminCommittees(): Promise<any> {
    return axios.get('councilCommittees/my-admin-committees')
  },

  getUserAdminCommittees(userId: string): Promise<any> {
    return axios.get(`councilCommittees/user-admin-committees/${userId}`)
  },

  updateBulkCommitteeAdmin(userId: string, committeeIds: number[]): Promise<any> {
    return axios.post('councilCommittees/update-bulk-admin', { userId, committeeIds })
  }
}

export default CouncilCommitteesService

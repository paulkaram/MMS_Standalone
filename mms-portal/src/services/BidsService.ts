import { mainApiAxios as axios } from '@/plugins/axios'

export enum BidStatus {
  Draft = 1,
  PendingManagerApproval = 2,
  VisionPreparation = 3,
  VisionsCompleted = 4,
  PreparatoryMeeting = 5,
  MinisterialMeeting = 6,
  ExternalMeetingDone = 7,
  AwaitingOpinion = 8,
  FinalMinutes = 9,
  AssignmentsCreated = 10,
  Completed = 11,
  Returned = 12
}

export interface Bid {
  id: number
  committeeId: number
  committeeName?: string | null
  referenceNumber: string
  externalMeetingNumber?: string | null
  subject: string
  description?: string | null
  teamLeaderUserId?: string | null
  teamLeaderName?: string | null
  statusId: number
  statusName?: string | null
  statusStepOrder: number
  startDate: string
  dueDate: string
  meetingId?: number | null
  initialMinutesPath?: string | null
  finalMinutesPath?: string | null
  createdBy: string
  createdByName?: string | null
  createdDate: string
  stakeholdersCount: number
  itemsCount: number
  isOverdue: boolean
}

export interface BidDetail extends Bid {
  stakeholders: BidStakeholder[]
  history: BidStatusHistory[]
  items: any[]
}

export interface BidStakeholder {
  id: number
  bidId: number
  userId?: string | null
  externalMemberId?: number | null
  name: string
  email?: string | null
  isTeamLeader: boolean
  isExternal: boolean
}

export interface BidStatusHistory {
  id: number
  bidId: number
  fromStatusId?: number | null
  fromStatusName?: string | null
  toStatusId: number
  toStatusName: string
  changedBy: string
  changedByName: string
  changedDate: string
  note?: string | null
}

export interface BidPost {
  committeeId: number
  externalMeetingNumber?: string | null
  subject: string
  description?: string | null
  teamLeaderUserId?: string | null
  startDate: string
  dueDate: string
  stakeholders: BidStakeholderPost[]
}

export interface BidStakeholderPost {
  userId?: string | null
  externalMemberId?: number | null
  isTeamLeader: boolean
}

export interface BidStatusDto {
  id: number
  nameAr: string
  nameEn: string
  stepOrder: number
}

const BidsService = {
  listStatuses(): Promise<BidStatusDto[]> {
    return axios.get('bids/statuses')
  },

  listByCommittee(committeeId: number): Promise<Bid[]> {
    return axios.get(`bids/committee/${committeeId}`)
  },

  listMine(): Promise<Bid[]> {
    return axios.get('bids/my')
  },

  getById(id: number): Promise<BidDetail> {
    return axios.get(`bids/${id}`)
  },

  create(dto: BidPost): Promise<Bid> {
    return axios.post('bids', dto)
  },

  update(id: number, dto: BidPost): Promise<Bid> {
    return axios.put(`bids/${id}`, dto)
  },

  delete(id: number): Promise<void> {
    return axios.delete(`bids/${id}`)
  },

  transition(id: number, targetStatusId: number, note?: string): Promise<Bid> {
    return axios.post(`bids/${id}/transition`, { targetStatusId, note: note || null })
  },

  allowedNextStatuses(id: number): Promise<number[]> {
    return axios.get(`bids/${id}/allowed-next-statuses`)
  }
}

export default BidsService

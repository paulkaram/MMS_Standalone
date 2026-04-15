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

  /** Patch only the description field — used by the inline-edit on BidDetail. */
  updateDescription(id: number, description: string | null): Promise<boolean> {
    return axios.patch(`bids/${id}/description`, { description })
  },

  delete(id: number): Promise<void> {
    return axios.delete(`bids/${id}`)
  },

  transition(id: number, targetStatusId: number, note?: string): Promise<Bid> {
    return axios.post(`bids/${id}/transition`, { targetStatusId, note: note || null })
  },

  allowedNextStatuses(id: number): Promise<number[]> {
    return axios.get(`bids/${id}/allowed-next-statuses`)
  },

  // ─────── Committee member picker (scoped to the bid's committee) ───────
  listCommitteeMembersForPicker(committeeId: number): Promise<{ id: string; name: string }[]> {
    return axios.get(`bids/committee/${committeeId}/member-picker`)
  },

  listItemTypes(): Promise<{ id: string; name: string }[]> {
    return axios.get('bids/item-types')
  },

  listRelatableItems(bidId: number): Promise<{ id: string; name: string }[]> {
    return axios.get(`bids/${bidId}/relatable-items`)
  },

  listExternalMembersForPicker(committeeId: number): Promise<{ id: string; name: string }[]> {
    return axios.get(`bids/committee/${committeeId}/external-members-picker`)
  },

  // ─────── Stakeholders ───────
  listStakeholders(bidId: number): Promise<BidStakeholder[]> {
    return axios.get(`bids/${bidId}/stakeholders`)
  },
  addStakeholder(bidId: number, dto: BidStakeholderPost): Promise<BidStakeholder> {
    return axios.post(`bids/${bidId}/stakeholders`, dto)
  },
  removeStakeholder(stakeholderId: number): Promise<boolean> {
    return axios.delete(`bids/stakeholders/${stakeholderId}`)
  },

  // ─────── Bid attachments (§5.6) ───────
  listAttachments(bidId: number): Promise<BidAttachment[]> {
    return axios.get(`bids/${bidId}/attachments`)
  },
  uploadAttachments(bidId: number, files: File[], privacyId = 1): Promise<BidAttachment[]> {
    const fd = new FormData()
    for (const f of files) fd.append('files', f)
    return axios.post(`bids/${bidId}/attachments?privacyId=${privacyId}`, fd, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  },
  deleteAttachment(attachmentId: number): Promise<boolean> {
    return axios.delete(`bids/attachments/${attachmentId}`)
  },

  // ─────── Bid items (no item type — see §5.6/§5.7 split) ───────
  addItem(bidId: number, dto: BidItemPost): Promise<BidItem> {
    return axios.post(`bids/${bidId}/items`, dto)
  },
  updateItem(itemId: number, dto: BidItemPost): Promise<BidItem> {
    return axios.put(`bids/items/${itemId}`, dto)
  },
  deleteItem(itemId: number): Promise<boolean> {
    return axios.delete(`bids/items/${itemId}`)
  }
}

export interface BidItem {
  id: number
  referenceNumber: string
  externalReferenceNumber?: string | null
  content: string
  internalNote?: string | null
  order: number
  dueDate?: string | null
  itemTypeId?: number | null
  itemTypeName?: string | null
  bidItemTypeId?: number | null
  bidItemTypeName?: string | null
  relatedItemId?: number | null
  relatedItemReferenceNumber?: string | null
  tagList?: { id: string; name: string }[]
}

export interface BidItemPost {
  referenceNumber: string
  externalReferenceNumber?: string | null
  content: string
  internalNote?: string | null
  order: number
  dueDate?: string | Date | null
  bidItemTypeId?: number | null     // procurement classification (§5.11)
  itemTypeId?: number | null        // agenda-style type (§5.7 line 223)
  relatedItemId?: number | null     // §5.7 line 224
  tagIds?: number[]                 // §5.7 line 225 — multiple tags
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

export interface BidAttachment {
  id: number
  name: string
  type: string
  size: number
  recordId: number
  recordTypeId: number
  privacyId: number
  privacyName: string
  recordTypeName: string
  version: number
  createdDate?: string | null
}

export default BidsService

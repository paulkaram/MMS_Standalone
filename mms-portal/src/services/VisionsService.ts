import { mainApiAxios as axios } from '@/plugins/axios'

export const VisionStatus = {
  Draft: 1,
  Submitted: 2
} as const

export interface BidItemVision {
  id: number
  bidId: number
  bidItemId: number
  bidItemTitle?: string | null
  stakeholderUserId?: string | null
  externalMemberId?: number | null
  stakeholderName: string
  isExternal: boolean
  comment?: string | null
  statusId: number
  statusName: string
  submittedDate?: string | null
  createdDate: string
}

export interface BidVisionStakeholderProgress {
  userId?: string | null
  externalMemberId?: number | null
  name: string
  isExternal: boolean
  total: number
  submitted: number
  completed: boolean
}

export interface BidVisionsSummary {
  bidId: number
  totalVisions: number
  submittedVisions: number
  stakeholdersCount: number
  itemsCount: number
  allSubmitted: boolean
  byStakeholder: BidVisionStakeholderProgress[]
}

const unwrap = <T>(res: any): T => (res?.data ?? res) as T

const VisionsService = {
  listByBid(bidId: number): Promise<BidItemVision[]> {
    return axios.get(`visions/bid/${bidId}`).then(unwrap<BidItemVision[]>)
  },

  getSummary(bidId: number): Promise<BidVisionsSummary> {
    return axios.get(`visions/bid/${bidId}/summary`).then(unwrap<BidVisionsSummary>)
  },

  listMine(bidId?: number): Promise<BidItemVision[]> {
    const url = bidId ? `visions/my?bidId=${bidId}` : 'visions/my'
    return axios.get(url).then(unwrap<BidItemVision[]>)
  },

  get(id: number): Promise<BidItemVision> {
    return axios.get(`visions/${id}`).then(unwrap<BidItemVision>)
  },

  saveDraft(id: number, comment: string | null): Promise<BidItemVision> {
    return axios.put(`visions/${id}`, { comment }).then(unwrap<BidItemVision>)
  },

  submit(id: number, comment: string | null): Promise<BidItemVision> {
    return axios.post(`visions/${id}/submit`, { comment }).then(unwrap<BidItemVision>)
  }
}

export default VisionsService

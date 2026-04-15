import { mainApiAxios as axios } from '@/plugins/axios'

export const MinutesOpinion = {
  Suitable: 1,
  Unsuitable: 2
} as const

export interface BidMinutesOpinion {
  id: number
  bidId: number
  stakeholderUserId?: string | null
  externalMemberId?: number | null
  stakeholderName: string
  isExternal: boolean
  opinion?: number | null
  opinionName?: string | null
  comment?: string | null
  statusId: number
  submittedDate?: string | null
}

export interface BidMinutesOpinionsSummary {
  bidId: number
  totalOpinions: number
  submittedOpinions: number
  suitableCount: number
  unsuitableCount: number
  allSubmitted: boolean
}

const unwrap = <T>(res: any): T => (res?.data ?? res) as T

const OpinionsService = {
  listByBid: (bidId: number) => axios.get(`opinions/bid/${bidId}`).then(unwrap<BidMinutesOpinion[]>),
  getSummary: (bidId: number) => axios.get(`opinions/bid/${bidId}/summary`).then(unwrap<BidMinutesOpinionsSummary>),
  submit: (id: number, opinion: number, comment?: string) =>
    axios.post(`opinions/${id}/submit`, { opinion, comment: comment || null }).then(unwrap<BidMinutesOpinion>)
}

export default OpinionsService

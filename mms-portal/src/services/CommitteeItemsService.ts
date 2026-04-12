import { mainApiAxios as axios } from '@/plugins/axios'

export interface CommitteeItem {
  id: number
  committeeId: number
  referenceNumber: string
  externalReferenceNumber?: string | null
  content: string
  itemTypeId: number
  itemTypeName?: string | null
  tags?: string | null
  internalNote?: string | null
  relatedItemId?: number | null
  relatedItemReferenceNumber?: string | null
  isPrivate: boolean
  order: number
  createdBy: string
  createdByName?: string | null
  createdDate: string
}

export interface CommitteeItemPost {
  committeeId: number
  externalReferenceNumber?: string | null
  content: string
  itemTypeId: number
  tags?: string | null
  internalNote?: string | null
  relatedItemId?: number | null
  isPrivate: boolean
  order: number
}

export interface ListItem {
  id: number
  name: string
}

const CommitteeItemsService = {
  create(dto: CommitteeItemPost): Promise<CommitteeItem> {
    return axios.post('committee-items', dto)
  },

  getById(id: number): Promise<CommitteeItem> {
    return axios.get(`committee-items/${id}`)
  },

  listByCommittee(committeeId: number): Promise<CommitteeItem[]> {
    return axios.get(`committee-items/committee/${committeeId}`)
  },

  update(id: number, dto: CommitteeItemPost): Promise<CommitteeItem> {
    return axios.put(`committee-items/${id}`, dto)
  },

  delete(id: number): Promise<void> {
    return axios.delete(`committee-items/${id}`)
  },

  listItemTypes(): Promise<ListItem[]> {
    return axios.get('committee-items/types')
  }
}

export default CommitteeItemsService

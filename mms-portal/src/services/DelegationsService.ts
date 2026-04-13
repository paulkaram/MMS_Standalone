import { mainApiAxios as axios } from '@/plugins/axios'

export enum DelegationType {
  General = 1,
  TaskSpecific = 2
}

export interface Delegation {
  id: number
  fromUserId: string
  fromUserName: string
  toUserId: string
  toUserName: string
  typeId: number
  typeName?: string | null
  startDate: string
  endDate: string
  isActive: boolean
  isCurrentlyActive: boolean
  reason?: string | null
  taskIds: number[]
  createdDate: string
}

export interface DelegationPost {
  toUserId: string
  typeId: number
  startDate: string
  endDate: string
  reason?: string | null
  taskIds: number[]
}

const DelegationsService = {
  listOutgoing(): Promise<Delegation[]> {
    return axios.get('delegations/outgoing')
  },

  listIncoming(): Promise<Delegation[]> {
    return axios.get('delegations/incoming')
  },

  getById(id: number): Promise<Delegation> {
    return axios.get(`delegations/${id}`)
  },

  create(dto: DelegationPost): Promise<Delegation> {
    return axios.post('delegations', dto)
  },

  revoke(id: number): Promise<Delegation> {
    return axios.post(`delegations/${id}/revoke`)
  },

  delete(id: number): Promise<void> {
    return axios.delete(`delegations/${id}`)
  }
}

export default DelegationsService

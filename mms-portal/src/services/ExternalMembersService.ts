import { mainApiAxios as axios } from '@/plugins/axios'

export interface ExternalMember {
  id: number
  fullnameAr: string
  fullnameEn: string
  email: string
  mobile?: string | null
  organization?: string | null
  position?: string | null
  isActive: boolean
  createdDate: string
  committeesCount: number
}

export interface ExternalMemberPost {
  fullnameAr: string
  fullnameEn: string
  email: string
  mobile?: string | null
  organization?: string | null
  position?: string | null
  isActive: boolean
}

export interface CommitteeExternalMember {
  id: number
  committeeId: number
  externalMemberId: number
  memberName: string
  email: string
  organization?: string | null
  position?: string | null
  committeeRoleId: number
  committeeRoleName?: string | null
  active: boolean
  note?: string | null
  createdDate: string
}

export interface CommitteeExternalMemberPost {
  committeeId: number
  externalMemberId: number
  committeeRoleId: number
  note?: string | null
}

export interface ListItem {
  id: number
  name: string
}

const ExternalMembersService = {
  listAdmin(): Promise<ExternalMember[]> {
    return axios.get('external-members/admin')
  },

  listAutoComplete(search?: string): Promise<ListItem[]> {
    const q = search ? `?search=${encodeURIComponent(search)}` : ''
    return axios.get(`external-members${q}`)
  },

  create(dto: ExternalMemberPost): Promise<ExternalMember> {
    return axios.post('external-members', dto)
  },

  update(id: number, dto: ExternalMemberPost): Promise<ExternalMember> {
    return axios.put(`external-members/${id}`, dto)
  },

  delete(id: number): Promise<void> {
    return axios.delete(`external-members/${id}`)
  },

  listByCommittee(committeeId: number): Promise<CommitteeExternalMember[]> {
    return axios.get(`external-members/committee/${committeeId}`)
  },

  addToCommittee(dto: CommitteeExternalMemberPost): Promise<CommitteeExternalMember> {
    return axios.post('external-members/committee', dto)
  },

  removeFromCommittee(linkId: number): Promise<void> {
    return axios.delete(`external-members/committee/${linkId}`)
  }
}

export default ExternalMembersService

import { mainApiAxios as axios } from '@/plugins/axios'

export interface Tag {
  id: number
  nameAr: string
  nameEn: string
  color: string
  usageCount: number
  createdDate: string
}

export interface TagPost {
  nameAr: string
  nameEn: string
  color?: string
}

export interface TagListItem {
  id: number
  name: string
}

export interface TagLinkPost {
  entityTypeId: number
  entityId: number
  tagIds: number[]
}

export enum TagEntityType {
  Committee = 1,
  Meeting = 2,
  CommitteeItem = 3,
  Bid = 4
}

const TagsService = {
  list(): Promise<TagListItem[]> {
    return axios.get('tags')
  },

  listAdmin(): Promise<Tag[]> {
    return axios.get('tags/admin')
  },

  create(dto: TagPost): Promise<Tag> {
    return axios.post('tags', dto)
  },

  update(id: number, dto: TagPost): Promise<Tag> {
    return axios.put(`tags/${id}`, dto)
  },

  delete(id: number): Promise<void> {
    return axios.delete(`tags/${id}`)
  },

  listForEntity(entityType: TagEntityType, entityId: number): Promise<TagListItem[]> {
    return axios.get(`tags/entity/${entityType}/${entityId}`)
  },

  setTagsForEntity(dto: TagLinkPost): Promise<void> {
    return axios.post('tags/entity/set', dto)
  }
}

export default TagsService

import { mainApiAxios as axios } from '@/plugins/axios'

export interface DictionaryItem {
  id: string
  keyword: string
  ar: string
  en: string
}

const DictionaryService = {
  listDictionary(): Promise<DictionaryItem[]> {
    return axios.get('dictionary')
  },

  getDictionary(id: string): Promise<DictionaryItem> {
    return axios.get(`dictionary/${id}`)
  },

  addDictionary(item: Partial<DictionaryItem>): Promise<DictionaryItem> {
    return axios.post('dictionary', item)
  },

  updateDictionary(id: string, item: Partial<DictionaryItem>): Promise<DictionaryItem> {
    return axios.put(`dictionary/${id}`, item)
  },

  deleteDictionary(id: string): Promise<void> {
    return axios.delete(`dictionary/${id}`)
  }
}

export default DictionaryService

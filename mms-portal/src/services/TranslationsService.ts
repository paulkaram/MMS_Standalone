import { mainApiAxios as axios } from '@/plugins/axios'

export interface TranslationResponse {
  success: boolean
  data: {
    ar: Record<string, string>
    en: Record<string, string>
  }
  message?: string
}

const TranslationsService = {
  /**
   * Fetch all translations from the API (same as old system)
   */
  listTranslations(): Promise<TranslationResponse> {
    return axios.get(`translations?_t=${Date.now()}`)
  }
}

export default TranslationsService

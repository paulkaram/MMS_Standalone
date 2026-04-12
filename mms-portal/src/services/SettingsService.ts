import { mainApiAxios as axios } from '@/plugins/axios'

export interface ThemeColors {
  primaryColor: string
  secondaryColor: string
  navigationColor: string
}

export interface SettingItem {
  id: string
  name: string
  value: string
  categoryId: string
}

export interface SettingCategory {
  categoryName: string
  items: SettingItem[]
}

export interface VotingType {
  id: number
  nameAr: string
  nameEn: string
  isActive: boolean
  isDefault: boolean
  options?: VotingTypeOption[]
}

export interface VotingTypeOption {
  id: number
  votingTypeId: number
  nameAr: string
  nameEn: string
  value: number
  order: number
  color?: string
}

export interface VotingTypeInput {
  nameAr: string
  nameEn: string
  isActive?: boolean
  isDefault?: boolean
}

export interface VotingTypeOptionInput {
  nameAr: string
  nameEn: string
  value: number
  order?: number
  color?: string
}

export interface SystemTemplate {
  id: number
  name: string
  subject: string
  body: string
  type: string
}

export interface ApiResponse<T = void> {
  success: boolean
  message?: string
  data?: T
}

const SettingsService = {
  listThemeColors(): Promise<ThemeColors> {
    return axios.get('settings/theme-colors')
  },

  updateTheme(colors: ThemeColors): Promise<void> {
    return axios.put('settings/theme-colors', colors)
  },

  listAll(): Promise<SettingCategory[]> {
    return axios.get('settings')
  },

  updateSettings(category: SettingCategory): Promise<void> {
    return axios.put('settings', category)
  },

  getSetting(key: string): Promise<{ value: string }> {
    return axios.get(`settings/${key}`)
  },

  updateSetting(key: string, value: string): Promise<void> {
    return axios.put(`settings/${key}`, { value })
  },

  // Voting Types
  listAllVotingTypes(): Promise<VotingType[]> {
    return axios.get('systemSettings/voting-type')
  },

  addVotingType(votingType: VotingTypeInput): Promise<ApiResponse<VotingType>> {
    return axios.post('systemSettings/voting-type', votingType)
  },

  updateVotingType(id: number, votingType: VotingTypeInput): Promise<void> {
    return axios.put('systemSettings/voting-type', { id, ...votingType })
  },

  deleteVotingType(id: number): Promise<ApiResponse> {
    return axios.delete(`systemSettings/${id}/voting-type`)
  },

  listVotingTypeOptions(votingTypeId: number): Promise<VotingTypeOption[]> {
    return axios.get(`systemSettings/${votingTypeId}/voting-type-option`)
  },

  addVotingTypeOption(votingTypeId: number, option: VotingTypeOptionInput): Promise<ApiResponse<VotingTypeOption>> {
    return axios.post('systemSettings/voting-type-option', { votingTypeId, ...option })
  },

  deleteVotingTypeOption(optionId: number): Promise<ApiResponse> {
    return axios.delete(`systemSettings/${optionId}/voting-type-option`)
  },

  // System Templates
  listSystemTemplates(): Promise<SystemTemplate[]> {
    return axios.get('settings/system-templates')
  },

  updateSystemTemplate(template: SystemTemplate): Promise<void> {
    return axios.put(`settings/system-templates/${template.id}`, template)
  },

  reloadSystemSettings(): Promise<ApiResponse<boolean>> {
    return axios.put('systemSettings/reload-settings')
  },

  // App Settings (system configuration)
  listAppSettings(): Promise<SettingCategory[]> {
    return axios.get('systemSettings')
  },

  updateAppSetting(category: SettingCategory): Promise<ApiResponse<boolean>> {
    return axios.put('systemSettings/setting', category)
  }
}

export default SettingsService

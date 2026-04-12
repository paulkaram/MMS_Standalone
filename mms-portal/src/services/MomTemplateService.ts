import { mainApiAxios as axios } from '@/plugins/axios'
import type {
  MomTemplate,
  MomTemplateListItem,
  MomTemplateCreate,
  MomTemplateUpdate,
  MomTemplateType,
  BranchListItem
} from '@/types/momTemplate'

export interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
}

const MomTemplateService = {
  /**
   * Get all MOM templates, optionally filtered by branch
   */
  list(branchId?: number): Promise<ApiResponse<MomTemplateListItem[]>> {
    const params = branchId ? { branchId } : {}
    return axios.get('mom-templates', { params })
  },

  /**
   * Get a specific MOM template by ID
   */
  getById(id: number): Promise<ApiResponse<MomTemplate>> {
    return axios.get(`mom-templates/${id}`)
  },

  /**
   * Get the default template for a branch and template type
   */
  getDefault(branchId: number | null, templateType: number): Promise<ApiResponse<MomTemplate>> {
    const params: Record<string, any> = { templateType }
    if (branchId !== null) {
      params.branchId = branchId
    }
    return axios.get('mom-templates/default', { params })
  },

  /**
   * Create a new MOM template
   */
  create(template: MomTemplateCreate): Promise<ApiResponse<number>> {
    return axios.post('mom-templates', template)
  },

  /**
   * Update an existing MOM template
   */
  update(id: number, template: MomTemplateUpdate): Promise<ApiResponse<boolean>> {
    return axios.put(`mom-templates/${id}`, template)
  },

  /**
   * Delete a MOM template
   */
  delete(id: number): Promise<ApiResponse<boolean>> {
    return axios.delete(`mom-templates/${id}`)
  },

  /**
   * Get all available template types
   */
  getTemplateTypes(): Promise<ApiResponse<MomTemplateType[]>> {
    return axios.get('mom-templates/template-types')
  },

  /**
   * Get all branches for dropdown
   */
  getBranches(): Promise<ApiResponse<BranchListItem[]>> {
    return axios.get('mom-templates/branches')
  }
}

export default MomTemplateService

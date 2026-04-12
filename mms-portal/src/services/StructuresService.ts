import api from '@/plugins/axios'

export interface Structure {
  id: number | string
  name: string
  nameAr?: string
  nameEn?: string
  parentId?: number | string | null
  children?: Structure[]
  active?: boolean
}

export interface StructureUser {
  id: string
  fullname: string
  email: string
  roleName: string
}

const StructuresService = {
  getStructure(structureId: number): Promise<Structure> {
    return api.get(`structures/${structureId}`)
  },

  listStructures(): Promise<Structure[]> {
    return api.get('structures')
  },

  listStructuresWithAnnualPlan(): Promise<Structure[]> {
    return api.get('structures/with-annual-plans')
  },

  listExternalStructures(): Promise<Structure[]> {
    return api.get('structures/external')
  },

  listOrganization(onlyActive: boolean = true): Promise<Structure[]> {
    return api.get(`structures/organization?onlyActive=${onlyActive}`)
  },

  listIamOrganization(): Promise<any> {
    return api.get('structures/iam-organization')
  },

  listIamDepartmentUsers(departmentId: string): Promise<any> {
    return api.get(`structures/iam-department/${departmentId}/users`)
  },

  addStructure(structureObj: Partial<Structure>): Promise<any> {
    return api.post('structures', structureObj)
  },

  updateStructure(structureId: number, structureObj: Partial<Structure>): Promise<any> {
    return api.put(`structures/${structureId}`, structureObj)
  },

  listUsersInStructure(structureId: number): Promise<StructureUser[]> {
    return api.get(`structures/${structureId}/users`)
  },

  listRoles(structureId: number): Promise<any[]> {
    return api.get(`structures/${structureId}/roles`)
  },

  removeUserFromStructure(structureId: number, userId: string): Promise<any> {
    return api.delete(`structures/${structureId}/user/${userId}`)
  },

  addUserToStructure(structureId: number, userId: string, roleId: number): Promise<any> {
    return api.post(`structures/${structureId}/add-user/${userId}/role/${roleId}`)
  },

  removeStructure(structureId: number): Promise<any> {
    return api.delete(`structures/${structureId}`)
  }
}

export default StructuresService

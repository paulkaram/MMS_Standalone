import { mainApiAxios as axios } from '@/plugins/axios'

export interface Role {
  id: string
  nameAr: string
  nameEn: string
  typeId?: string
  typeName?: string
  isActive?: boolean
  permissions?: string[]
}

export interface CommitteeRole {
  id: string
  nameAr: string
  nameEn: string
  isActive?: boolean
}

const RolesService = {
  // System Roles
  listRoles(): Promise<Role[]> {
    return axios.get('roles')
  },

  getRole(id: string): Promise<Role> {
    return axios.get(`roles/${id}`)
  },

  addRole(role: Partial<Role>): Promise<Role> {
    return axios.post('roles', role)
  },

  updateRole(id: string, role: Partial<Role>): Promise<Role> {
    return axios.put(`roles/${id}`, role)
  },

  deleteRole(id: string): Promise<void> {
    return axios.delete(`roles/${id}`)
  },

  // Role Permissions
  getRolePermissions(roleId: string): Promise<string[]> {
    return axios.get(`roles/${roleId}/permissions`)
  },

  updateRolePermissions(roleId: string, permissions: string[]): Promise<void> {
    return axios.put(`roles/${roleId}/permissions`, { permissions })
  },

  // Committee Roles
  listCommitteeRoles(): Promise<CommitteeRole[]> {
    return axios.get('committee-roles')
  },

  getCommitteeRole(id: string): Promise<CommitteeRole> {
    return axios.get(`committee-roles/${id}`)
  },

  addCommitteeRole(role: Partial<CommitteeRole>): Promise<CommitteeRole> {
    return axios.post('committee-roles', role)
  },

  updateCommitteeRole(id: string, role: Partial<CommitteeRole>): Promise<CommitteeRole> {
    return axios.put(`committee-roles/${id}`, role)
  },

  deleteCommitteeRole(id: string): Promise<void> {
    return axios.delete(`committee-roles/${id}`)
  }
}

export default RolesService

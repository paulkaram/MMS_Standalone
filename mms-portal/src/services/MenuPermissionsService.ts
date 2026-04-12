import api from '@/plugins/axios'

export interface IamGroup {
  id: string
  name: string
  nameAr?: string
  isActive: boolean
}

export interface IamRole {
  id: string
  name: string
  nameAr?: string
  isActive: boolean
}

export interface MenuPermissionItem {
  id: number
  name: string
  groupName?: string
  isAssigned: boolean
}

const MenuPermissionsService = {
  listAvailable(): Promise<any> {
    return api.get('menu-permissions/available')
  },
  listIamGroups(): Promise<any> {
    return api.get('menu-permissions/iam-groups')
  },
  listIamRoles(): Promise<any> {
    return api.get('menu-permissions/iam-roles')
  },
  getGroupPermissions(groupId: string): Promise<any> {
    return api.get(`menu-permissions/group/${groupId}`)
  },
  getRolePermissions(roleName: string): Promise<any> {
    return api.get(`menu-permissions/role/${encodeURIComponent(roleName)}`)
  },
  saveGroupPermissions(groupId: string, data: { displayName?: string; permissionIds: number[] }): Promise<any> {
    return api.post(`menu-permissions/group/${groupId}`, data)
  },
  saveRolePermissions(roleName: string, data: { permissionIds: number[] }): Promise<any> {
    return api.post(`menu-permissions/role/${encodeURIComponent(roleName)}`, data)
  }
}

export default MenuPermissionsService

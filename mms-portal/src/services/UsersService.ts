import { mainApiAxios as axios } from '@/plugins/axios'

export interface User {
  id: string
  username: string
  fullName: string
  email: string
  language: string
  profilePictureUrl: string | null
  hasProfilePicture: boolean
  roles: string[]
  permissions: string[]
  isActive: boolean
  createdAt?: string
  lastLogin?: string
}

export interface UserListParams {
  page?: number
  pageSize?: number
  search?: string
  active?: boolean
  roleId?: string
}

export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface Notification {
  id: string
  title: string
  message: string
  type: string
  isRead: boolean
  createdAt: string
  link?: string
}

const UsersService = {
  /**
   * Get current logged in user info
   */
  getCurrentUser(): Promise<User> {
    return axios.get('users/me')
  },

  /**
   * Get user by ID
   */
  getUser(userId: string): Promise<User> {
    return axios.get(`users/${userId}`)
  },

  /**
   * List users with pagination (admin)
   */
  listUsers(params: UserListParams = {}): Promise<PaginatedResponse<User>> {
    const { page = 1, pageSize = 10, search = '', active } = params
    let url = `users/admin?page=${page}&pageSize=${pageSize}`
    if (search) url += `&search=${encodeURIComponent(search)}`
    if (active !== undefined) url += `&active=${active}`
    return axios.get(url)
  },

  /**
   * List users for autocomplete
   * @param search - Search query
   * @param active - Filter by active status
   * @param signal - Optional AbortSignal for request cancellation
   */
  listUsersForAutoComplete(search: string, active: boolean = true, signal?: AbortSignal): Promise<User[]> {
    return axios.get(`users?search=${encodeURIComponent(search)}&active=${active}&mode=Autocomplete`, { signal })
  },

  /**
   * Add new user
   */
  addUser(userObj: Partial<User>): Promise<User> {
    return axios.post('users', userObj)
  },

  /**
   * Update user
   */
  updateUser(userObj: Partial<User>): Promise<User> {
    return axios.put('users', userObj)
  },

  /**
   * Delete user
   */
  deleteUser(userId: string): Promise<void> {
    return axios.delete(`users/${userId}`)
  },

  /**
   * Activate user
   */
  activateUser(userId: string): Promise<void> {
    return axios.post(`users/${userId}/activate`)
  },

  /**
   * Deactivate user
   */
  deactivateUser(userId: string): Promise<void> {
    return axios.post(`users/${userId}/deactivate`)
  },

  /**
   * Get user notifications
   */
  getNotifications(page: number = 1, pageSize: number = 10): Promise<PaginatedResponse<Notification>> {
    return axios.get(`users/notifications?page=${page}&pageSize=${pageSize}`)
  },

  /**
   * Get unread notifications count
   */
  getUnreadNotificationsCount(): Promise<{ count: number }> {
    return axios.get('users/notifications/unread-count')
  },

  /**
   * Mark notification as read
   */
  markNotificationRead(notificationId: string): Promise<void> {
    return axios.post(`users/notifications/${notificationId}/read`)
  },

  /**
   * Mark all notifications as read
   */
  markAllNotificationsRead(): Promise<void> {
    return axios.post('users/notifications/read-all')
  },

  /**
   * Logout user
   */
  logout(): Promise<void> {
    return axios.post('users/logout', {})
  },

  /**
   * Update user language preference
   */
  updateLanguage(language: string): Promise<void> {
    return axios.post('users/language', JSON.stringify(language), {
      headers: {
        'Content-Type': 'application/json'
      }
    })
  },

  /**
   * Get user permissions
   */
  getUserPermissions(userId: string): Promise<any[]> {
    return axios.get(`users/${userId}/permissions`)
  },

  /**
   * Toggle user permission
   */
  toggleUserPermission(userId: string, permissionId: number, permissionTypeId: number, enabled: boolean): Promise<void> {
    return axios.post(`users/permissions/${userId}/${permissionId}/${permissionTypeId}?enabled=${enabled}`)
  },

  /**
   * Search users (alias for listUsersForAutoComplete)
   * @param search - Search query
   * @param active - Filter by active status
   * @param signal - Optional AbortSignal for request cancellation
   */
  searchUsers(search: string, active: boolean = true, signal?: AbortSignal): Promise<User[]> {
    return axios.get(`users?search=${encodeURIComponent(search)}&active=${active}&mode=Autocomplete`, { signal })
  },

  /**
   * Search users from local database.
   * Returns user IDs and display names.
   */
  async searchLocalUsers(query: string, limit: number = 20, signal?: AbortSignal): Promise<Array<{ id: string; name: string }>> {
    const response: any = await axios.get(`users?search=${encodeURIComponent(query)}&active=true&mode=Autocomplete`, { signal })
    const users = response.data || response || []
    return Array.isArray(users) ? users.map((u: any) => ({ id: String(u.id), name: u.name })) : []
  },

  /**
   * List users with pagination (admin)
   */
  listAdminUsers(page: number = 1, pageSize: number = 10, active: boolean = true): Promise<any> {
    return axios.get(`users/admin?page=${page}&pageSize=${pageSize}&active=${active}`)
  },

  /**
   * Enable/disable SMS notifications for user
   */
  enableSms(userId: string, activated: boolean): Promise<void> {
    return axios.post(`users/${userId}/sms?activated=${activated}`)
  },

  /**
   * Enable/disable email notifications for user
   */
  enableEmail(userId: string, activated: boolean): Promise<void> {
    return axios.post(`users/${userId}/email?activated=${activated}`)
  },

  /**
   * Get user roles and structures
   */
  listUserRolesStructures(userId: string): Promise<any[]> {
    return axios.get(`users/${userId}/roles`)
  },

  /**
   * Get current user's menu permissions
   * Returns list of menu items with hasAccess flag
   */
  listCurrentUserPermissions(): Promise<{ name: string; hasAccess: boolean }[]> {
    return axios.get('users/permissions')
  }
}

export default UsersService

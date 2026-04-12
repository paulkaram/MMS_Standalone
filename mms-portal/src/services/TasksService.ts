import { mainApiAxios as axios } from '@/plugins/axios'

export interface Task {
  id: string
  title: string
  titleAr: string
  titleEn?: string
  description?: string
  status: string
  statusName?: string
  priority: string
  priorityName?: string
  type: string
  typeName?: string
  dueDate?: string
  completedDate?: string
  assignedToId: string
  assignedToName?: string
  assignedById: string
  assignedByName?: string
  meetingId?: string
  meetingTitle?: string
  agendaItemId?: string
  agendaItemTitle?: string
  recommendationId?: string
  createdAt: string
  updatedAt?: string
  completionPercentage?: number
  attachmentsCount?: number
  commentsCount?: number
}

export interface TaskListParams {
  page?: number
  pageSize?: number
  search?: string
  status?: string
  priority?: string
  assignedToId?: string
  meetingId?: string
  fromDate?: string
  toDate?: string
  overdue?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface TaskComment {
  id: string
  taskId: string
  userId: string
  userName: string
  comment: string
  createdAt: string
}

const TasksService = {
  // Task listing
  listTasks(params: TaskListParams = {}): Promise<PaginatedResponse<Task>> {
    const queryParams = new URLSearchParams()
    if (params.page) queryParams.append('page', params.page.toString())
    if (params.pageSize) queryParams.append('pageSize', params.pageSize.toString())
    if (params.search) queryParams.append('search', params.search)
    if (params.status) queryParams.append('status', params.status)
    if (params.priority) queryParams.append('priority', params.priority)
    if (params.assignedToId) queryParams.append('assignedToId', params.assignedToId)
    if (params.meetingId) queryParams.append('meetingId', params.meetingId)
    if (params.fromDate) queryParams.append('fromDate', params.fromDate)
    if (params.toDate) queryParams.append('toDate', params.toDate)
    if (params.overdue !== undefined) queryParams.append('overdue', params.overdue.toString())
    if (params.sortBy) queryParams.append('sortBy', params.sortBy)
    if (params.sortOrder) queryParams.append('sortOrder', params.sortOrder)

    return axios.get(`tasks?${queryParams.toString()}`)
  },

  getMyTasks(params: TaskListParams = {}): Promise<PaginatedResponse<Task>> {
    const queryParams = new URLSearchParams()
    if (params.page) queryParams.append('page', params.page.toString())
    if (params.pageSize) queryParams.append('pageSize', params.pageSize.toString())
    if (params.status) queryParams.append('status', params.status)

    return axios.get(`tasks/my-tasks?${queryParams.toString()}`).then((response: any) => {
      const data = response?.data || response
      const items = data?.items || data?.Items || []
      const totalCount = data?.totalCount ?? data?.TotalCount ?? 0
      return {
        items,
        totalCount,
        page: params.page || 1,
        pageSize: params.pageSize || 10,
        totalPages: Math.ceil(totalCount / (params.pageSize || 10))
      }
    })
  },

  getOverdueTasks(params: TaskListParams = {}): Promise<PaginatedResponse<Task>> {
    const queryParams = new URLSearchParams()
    if (params.page) queryParams.append('page', params.page.toString())
    if (params.pageSize) queryParams.append('pageSize', params.pageSize.toString())

    return axios.get(`tasks/overdue?${queryParams.toString()}`)
  },

  // Task CRUD
  getTask(taskId: string): Promise<Task> {
    return axios.get(`tasks/${taskId}`)
  },

  createTask(task: Partial<Task>): Promise<Task> {
    return axios.post('tasks', task)
  },

  updateTask(taskId: string, task: Partial<Task>): Promise<Task> {
    return axios.put(`tasks/${taskId}`, task)
  },

  deleteTask(taskId: string): Promise<void> {
    return axios.delete(`tasks/${taskId}`)
  },

  // Task status actions
  startTask(taskId: string): Promise<void> {
    return axios.post(`tasks/${taskId}/start`)
  },

  completeTask(taskId: string, notes?: string): Promise<void> {
    return axios.post(`tasks/${taskId}/complete`, { notes })
  },

  reopenTask(taskId: string, reason?: string): Promise<void> {
    return axios.post(`tasks/${taskId}/reopen`, { reason })
  },

  cancelTask(taskId: string, reason?: string): Promise<void> {
    return axios.post(`tasks/${taskId}/cancel`, { reason })
  },

  // Task assignment
  assignTask(taskId: string, userId: string): Promise<void> {
    return axios.post(`tasks/${taskId}/assign`, { userId })
  },

  claimTask(taskId: string): Promise<void> {
    return axios.post(`tasks/${taskId}/claim`)
  },

  // Task progress
  updateProgress(taskId: string, percentage: number): Promise<void> {
    return axios.put(`tasks/${taskId}/progress`, { percentage })
  },

  // Task comments
  getComments(taskId: string): Promise<TaskComment[]> {
    return axios.get(`tasks/${taskId}/comments`)
  },

  addComment(taskId: string, comment: string): Promise<TaskComment> {
    return axios.post(`tasks/${taskId}/comments`, { comment })
  },

  deleteComment(taskId: string, commentId: string): Promise<void> {
    return axios.delete(`tasks/${taskId}/comments/${commentId}`)
  },

  // Task attachments
  getAttachments(taskId: string): Promise<Array<{
    id: string
    fileName: string
    fileType: string
    fileSize: number
    uploadedAt: string
  }>> {
    return axios.get(`tasks/${taskId}/attachments`)
  },

  addAttachment(taskId: string, formData: FormData): Promise<{ id: string; fileName: string }> {
    return axios.post(`tasks/${taskId}/attachments`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
  },

  deleteAttachment(taskId: string, attachmentId: string): Promise<void> {
    return axios.delete(`tasks/${taskId}/attachments/${attachmentId}`)
  },

  downloadAttachment(taskId: string, attachmentId: string): Promise<Blob> {
    return axios.get(`tasks/${taskId}/attachments/${attachmentId}/download`, {
      responseType: 'blob'
    })
  }
}

export default TasksService

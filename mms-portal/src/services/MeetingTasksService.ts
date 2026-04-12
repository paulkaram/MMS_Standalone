import { mainApiAxios as axios } from '@/plugins/axios'

export interface MeetingTask {
  id: string | number
  type: string
  typeId: number
  status: string
  meetingId: string | number
  meetingTitle: string
  meetingReference: string
  claimed: boolean
  isDelayed: boolean
  createdAt?: string
  dueDate?: string
}

export interface MeetingTasksResponse {
  data: MeetingTask[]
  total: number
  page: number
  pageSize: number
}

const MeetingTasksService = {
  /**
   * List meeting tasks for current user
   */
  listTasks(page: number = 1, pageSize: number = 10): Promise<MeetingTasksResponse> {
    return axios.get(`tasks/meeting?page=${page}&pageSize=${pageSize}`)
  },

  /**
   * Claim a meeting task
   */
  claimTask(taskId: string | number): Promise<{ success: boolean }> {
    return axios.post(`tasks/${taskId}/meeting/claim`)
  },

  /**
   * Approve or reject a meeting task
   */
  approveTask(taskId: string | number, approved: boolean, note: string): Promise<{ success: boolean; message?: string }> {
    return axios.post(`tasks/${taskId}/meeting/approve?approved=${approved}`, note)
  },

  /**
   * Get attachment query string for a task - includes sign permissions
   */
  getTaskAttachmentQuery(taskId: string | number, attachmentId: number): Promise<string> {
    return axios.get(`tasks/${taskId}/meeting/attachment/${attachmentId}`)
  }
}

export default MeetingTasksService

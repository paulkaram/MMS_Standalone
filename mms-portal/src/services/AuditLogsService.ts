import api from '@/plugins/axios'

export interface AuditLog {
  id: number
  username: string
  processInstanceId?: number
  recordId?: number
  letterId?: number
  commentId?: number
  actionName: string
  controllerName: string
  description: string
  createdDate: string
  additionalInfo?: string
}

export interface AuditLogsResponse {
  data: AuditLog[]
  total: number
}

const AuditLogsService = {
  listAuditTrails(page: number, pageSize: number, search: string = ''): Promise<AuditLogsResponse> {
    return api.get(`auditLogs?page=${page}&pageSize=${pageSize}&search=${search}`)
  }
}

export default AuditLogsService

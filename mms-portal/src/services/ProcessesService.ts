import api from '@/plugins/axios'

export interface ProcessApp {
  workflowId: number
  workflowDefinitionId: number
  title: string
  icon: string
  runningFlows: number
}

export interface ProcessActivity {
  id: number
  name: string
  status: string
}

export interface ProcessInstance {
  id: number
  title: string
  status: string
  createdDate: string
}

export interface ProcessSearchInput {
  page?: number
  pageSize?: number
  search?: string
  status?: string
  workflowId?: number
  fromDate?: string
  toDate?: string
}

export interface ProcessLetter {
  id: number
  title: string
  content: string
  createdDate: string
}

export interface ProcessComment {
  id: number
  content: string
  createdBy: string
  createdDate: string
}

export interface ProcessInitiation {
  id: number
  fields: Record<string, unknown>
}

export interface ProcessAuditItem {
  id: number
  action: string
  userId: string
  userName: string
  createdDate: string
  details: string
}

export interface ProcessStatusReport {
  pending: number
  inProgress: number
  completed: number
  cancelled: number
}

export interface FinancialProcessReport {
  totalAmount: number
  approvedAmount: number
  pendingAmount: number
}

export interface CommentsReportInput {
  processInstanceId?: number
  fromDate?: string
  toDate?: string
}

export interface ApiResponse<T = void> {
  success: boolean
  message?: string
  data?: T
}

const ProcessesService = {
  list(searchInput: ProcessSearchInput): Promise<{ data: ProcessInstance[]; total: number }> {
    return api.post('processes/search', searchInput)
  },

  createProcess(formData: FormData): Promise<ApiResponse<ProcessInstance>> {
    return api.post('processes', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
  },

  listActivities(processInstanceId: number): Promise<ProcessActivity[]> {
    return api.get(`processes/${processInstanceId}/activities`)
  },

  getProcess(processInstanceId: number): Promise<ProcessInstance> {
    return api.get(`processes/${processInstanceId}`)
  },

  listLetters(processInstanceId: number): Promise<ProcessLetter[]> {
    return api.get(`processes/${processInstanceId}/letters`)
  },

  listCommentsByProcessId(processInstanceId: number): Promise<ProcessComment[]> {
    return api.get(`processes/${processInstanceId}/comments`)
  },

  listProcesses(): Promise<ProcessApp[]> {
    return api.get('processes/apps')
  },

  listInitiationData(processInstanceId: number): Promise<ProcessInitiation> {
    return api.get(`processes/${processInstanceId}/initiation`)
  },

  listCommentsReport(search: CommentsReportInput): Promise<{ data: ProcessComment[]; total: number }> {
    return api.post('processes/comments-report', search)
  },

  getProcessStatusReport(): Promise<ProcessStatusReport> {
    return api.get('processes/reports/process-status-report')
  },

  getFinancialProcessReport(): Promise<FinancialProcessReport> {
    return api.get('processes/reports/financial-process-report')
  },

  listAuditItems(processInstanceId: number): Promise<ProcessAuditItem[]> {
    return api.get(`processes/${processInstanceId}/auditItems`)
  }
}

export default ProcessesService

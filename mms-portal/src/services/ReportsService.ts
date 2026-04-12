import { mainApiAxios as axios } from '@/plugins/axios'

export interface CommitteeSummary {
  id: string
  name: string
  isActive: boolean
  startDate: string
  endDate: string
  meetingsCount: number
  subCommitteesCount: number
  membersCount: number
  recommendationsCount: number
  attendanceRate: number
  statusesMeetingsCount?: Array<{ name: string; meetingsCount: number }>
}

export interface AttendanceItem {
  committeeName: string
  title: string
  meetingId: string
  meetingDate: string
  attended: boolean
  userName: string
}

export interface AttendanceSearchParams {
  meetingReferenceNo?: string | null
  fromDate?: string | null
  toDate?: string | null
  title?: string | null
}

export interface PaginatedResponse<T> {
  data: T[]
  total: number
}

export interface StatusMeetingsCount {
  statusId: string
  statusName: string
  count: number
}

const ReportsService = {
  getCommitteesSummary(
    page: number = 1,
    pageSize: number = 10
  ): Promise<PaginatedResponse<CommitteeSummary>> {
    return axios.get(`reports/comittees-summary/${page}/${pageSize}`)
  },

  getAttendanceReport(
    params: AttendanceSearchParams,
    page: number = 1,
    pageSize: number = 10
  ): Promise<PaginatedResponse<AttendanceItem>> {
    return axios.post(`reports/attendance/${page}/${pageSize}`, params)
  },

  getStatusMeetingsCount(committeeId: string): Promise<StatusMeetingsCount[]> {
    return axios.get(`reports/comittees-meetings-summary/${committeeId}`)
  },

  exportCommitteesSummary(format: 'pdf' | 'excel' = 'excel'): Promise<Blob> {
    return axios.get(`reports/committees-summary/export?format=${format}`, {
      responseType: 'blob'
    })
  },

  exportAttendanceReport(
    params: AttendanceSearchParams,
    format: 'pdf' | 'excel' = 'excel'
  ): Promise<Blob> {
    return axios.post(`reports/attendance/export?format=${format}`, params, {
      responseType: 'blob'
    })
  }
}

export default ReportsService

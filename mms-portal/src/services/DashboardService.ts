import { mainApiAxios as axios } from '@/plugins/axios'

export interface DashboardStats {
  totalMeetings: number
  upcomingMeetings: number
  completedMeetings: number
  pendingTasks: number
  completedTasks: number
  overdueTask: number
  totalRecommendations: number
  pendingRecommendations: number
}

export interface MeetingCount {
  status: string
  count: number
}

export interface TaskCount {
  status: string
  count: number
}

export interface RecentActivity {
  id: number
  type: string
  title: string
  description?: string
  timestamp: string
  userId?: string
  userName?: string
}

export interface UpcomingMeeting {
  id: number
  title: string
  date: string
  time: string
  location?: string
  councilName?: string
  status: string
}

export interface BarChartData {
  labels: string[]
  series: { name: string; data: number[] }[]
}

export interface DoughnutChartData {
  labels: string[]
  values: number[]
}

const DashboardService = {
  /**
   * Get dashboard statistics
   */
  async getStats(): Promise<DashboardStats> {
    const response: any = await axios.get('dashboard/stats')
    // Handle both wrapped { data: {...} } and direct response
    return response?.data || response
  },

  /**
   * Get meetings count by status (returns BarChartData)
   */
  async getMeetingsCount(): Promise<BarChartData> {
    const response: any = await axios.get('dashboard/meetings-count')
    // API returns { data: { labels, series }, success, message }
    // Axios interceptor extracts response.data, so we get { data: { labels, series }, success }
    return response?.data || response
  },

  /**
   * Get tasks count by status (returns DoughnutChartData with percentages)
   */
  async getTasksCount(): Promise<DoughnutChartData> {
    const response: any = await axios.get('dashboard/tasks-count')
    return response.data || response
  },

  /**
   * Get councils and committees count
   */
  async getCouncilsCommitteesCount(): Promise<DoughnutChartData> {
    const response: any = await axios.get('dashboard/councils-comittees-count')
    return response.data || response
  },

  /**
   * Get recommendations count
   */
  async getRecommendationsCount(): Promise<DoughnutChartData> {
    const response: any = await axios.get('dashboard/recommendations-count')
    return response.data || response
  },

  /**
   * Get users count
   */
  async getUsersCount(): Promise<DoughnutChartData> {
    const response: any = await axios.get('dashboard/users-count')
    return response.data || response
  },

  /**
   * Get meeting minutes count
   */
  async getMeetingMinutesCount(): Promise<DoughnutChartData> {
    const response: any = await axios.get('dashboard/meeting-minutes-count')
    return response.data || response
  },

  /**
   * Get recent activities
   */
  async getRecentActivities(count: number = 10): Promise<RecentActivity[]> {
    const response: any = await axios.get(`dashboard/recent-activities?count=${count}`)
    return response.data || response || []
  },

  /**
   * Get upcoming meetings
   */
  async getUpcomingMeetings(count: number = 5): Promise<UpcomingMeeting[]> {
    const response: any = await axios.get(`dashboard/upcoming-meetings?count=${count}`)
    return response.data || response || []
  },

  /**
   * Get user dashboard summary
   */
  async getUserSummary(): Promise<{
    myMeetingsCount: number
    myTasksCount: number
    myPendingTasksCount: number
  }> {
    const response: any = await axios.get('dashboard/user-summary')
    return response.data || response
  }
}

export default DashboardService

import api from '@/plugins/axios'

// Response interface - excludes sensitive data
export interface DataSource {
  id: number
  dbName: string
  instanceName: string
  username: string
  // Note: password is intentionally excluded from responses for security
}

// Input interface for create/update operations
export interface DataSourceInput {
  dbName: string
  instanceName: string
  username: string
  password?: string // Only sent when creating/updating
}

export interface ApiResponse<T = void> {
  success: boolean
  message?: string
  data?: T
}

const DataSourcesService = {
  listDataSources(): Promise<DataSource[]> {
    return api.get('dataSources')
  },

  listSimpleDataSources(): Promise<DataSource[]> {
    return api.get('dataSources/simple-list')
  },

  addDataSource(dataSource: DataSourceInput): Promise<ApiResponse<DataSource>> {
    return api.post('dataSources', dataSource)
  },

  updateDataSource(dataSourceId: number, dataSource: Partial<DataSourceInput>): Promise<ApiResponse<DataSource>> {
    return api.put(`dataSources/${dataSourceId}`, dataSource)
  },

  deleteDataSource(dataSourceId: number): Promise<ApiResponse> {
    return api.delete(`dataSources/${dataSourceId}`)
  }
}

export default DataSourcesService

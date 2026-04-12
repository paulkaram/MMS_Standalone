import api from '@/plugins/axios'

export interface TaskMapping {
  id: number
  dataSourceId: number
  tableName: string
  tableNameAr: string
  tableNameEn: string
  textField: string
  valueField: string
  isDocument: boolean
}

const TaskMappingService = {
  listMappings(): Promise<TaskMapping[]> {
    return api.get('taskMappings')
  },

  addMapping(mapping: Partial<TaskMapping>): Promise<any> {
    return api.post('taskMappings', mapping)
  },

  deleteMapping(mappingId: number): Promise<any> {
    return api.delete(`taskMappings/${mappingId}`)
  }
}

export default TaskMappingService

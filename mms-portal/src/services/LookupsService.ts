import { mainApiAxios as axios } from '@/plugins/axios'

export interface LookupItem {
  id: string | number
  name: string
  nameAr?: string
  nameEn?: string
  code?: string
  isActive?: boolean
  order?: number
}

export interface Lookup {
  id: number
  name: string
  nameAr: string
  nameEn: string
  forSurvey: boolean
  lookupIdRelatedTo: number | null
}

export interface LookupCreate {
  Id?: number
  NameAr: string
  NameEn: string
  ForSurvey: boolean
  LookupIdRelatedTo: number | null
}

export interface LookupItemAdmin {
  recordPK: number
  lookupId: number
  lookupName: string
  id: string
  nameAr: string
  nameEn: string
  itemFromRelatedLookup: number | null
}

export interface LookupItemCreate {
  LookupId: number
  Id: string
  NameAr: string
  NameEn: string
  ItemFromRelatedLookup: number | null
}

export interface DataSourceLookup {
  id: number
  name: string
}

export interface Council {
  id: string
  name: string
  nameAr: string
  nameEn: string
  type: string
  isActive: boolean
}

export interface Committee {
  id: string
  name: string
  nameAr: string
  nameEn: string
  councilId: string
  isActive: boolean
}

const LookupsService = {
  // Meeting related lookups
  getMeetingStatuses(): Promise<LookupItem[]> {
    return axios.get('lookups/meeting-statuses')
  },

  getMeetingTypes(): Promise<LookupItem[]> {
    return axios.get('lookups/meeting-types')
  },

  getMeetingLocations(): Promise<LookupItem[]> {
    return axios.get('lookups/meeting-locations')
  },

  getVotingTypes(): Promise<LookupItem[]> {
    return axios.get('lookups/voting-types')
  },

  getAttendanceStatuses(): Promise<LookupItem[]> {
    return axios.get('lookups/attendance-statuses')
  },

  // Task related lookups
  getTaskStatuses(): Promise<LookupItem[]> {
    return axios.get('lookups/task-statuses')
  },

  getTaskPriorities(): Promise<LookupItem[]> {
    return axios.get('lookups/task-priorities')
  },

  getTaskTypes(): Promise<LookupItem[]> {
    return axios.get('lookups/task-types')
  },

  // Council & Committee lookups
  getCouncils(): Promise<Council[]> {
    return axios.get('lookups/councils')
  },

  getActiveCouncils(): Promise<Council[]> {
    return axios.get('lookups/councils?active=true')
  },

  getCommittees(councilId?: string): Promise<Committee[]> {
    const url = councilId
      ? `lookups/committees?councilId=${councilId}`
      : 'lookups/committees'
    return axios.get(url)
  },

  getActiveCommittees(councilId?: string): Promise<Committee[]> {
    let url = 'lookups/committees?active=true'
    if (councilId) url += `&councilId=${councilId}`
    return axios.get(url)
  },

  // Structure lookups
  listStructureTypes(): Promise<LookupItem[]> {
    return axios.get('lookups/structure-types')
  },

  listStructures(): Promise<LookupItem[]> {
    return axios.get('lookups/structures')
  },

  listBranches(): Promise<LookupItem[]> {
    return axios.get('lookups/branches')
  },

  // User & Role lookups
  getRoles(): Promise<LookupItem[]> {
    return axios.get('lookups/roles')
  },

  getActiveRoles(): Promise<LookupItem[]> {
    return axios.get('lookups/roles?active=true')
  },

  getCommitteeRoles(): Promise<LookupItem[]> {
    return axios.get('lookups/committee-roles')
  },

  // General lookups
  getPrivacyLevels(): Promise<LookupItem[]> {
    return axios.get('lookups/privacies')
  },

  getDocumentTypes(): Promise<LookupItem[]> {
    return axios.get('lookups/document-types')
  },

  getFileCategories(): Promise<LookupItem[]> {
    return axios.get('lookups/file-categories')
  },

  // Recommendation lookups
  getRecommendationStatuses(): Promise<LookupItem[]> {
    return axios.get('lookups/meeting-recommendations-statuses')
  },

  getRecommendationPriorities(): Promise<LookupItem[]> {
    return axios.get('lookups/recommendation-priorities')
  },

  getRecommendationTypes(): Promise<LookupItem[]> {
    return axios.get('lookups/recommendation-types')
  },

  // Agenda lookups
  getAgendaItemTypes(): Promise<LookupItem[]> {
    return axios.get('lookups/agenda-item-types')
  },

  getAgendaItemStatuses(): Promise<LookupItem[]> {
    return axios.get('lookups/agenda-item-statuses')
  },

  // Languages
  getLanguages(): Promise<LookupItem[]> {
    return axios.get('lookups/languages')
  },

  // Nationalities
  getNationalities(): Promise<LookupItem[]> {
    return axios.get('lookups/nationalities')
  },

  // Titles
  getTitles(): Promise<LookupItem[]> {
    return axios.get('lookups/titles')
  },

  // Gender
  getGenders(): Promise<LookupItem[]> {
    return axios.get('lookups/genders')
  },

  // Notification types
  getNotificationTypes(): Promise<LookupItem[]> {
    return axios.get('lookups/notification-types')
  },

  // Countries
  getCountries(): Promise<LookupItem[]> {
    return axios.get('lookups/countries')
  },

  // Cities
  getCities(countryId?: string): Promise<LookupItem[]> {
    const url = countryId
      ? `lookups/cities?countryId=${countryId}`
      : 'lookups/cities'
    return axios.get(url)
  },

  // Council Sessions
  getCouncilSessions(): Promise<LookupItem[]> {
    return axios.get('lookups/council-sessions')
  },

  // Job Titles
  getJobTitles(): Promise<LookupItem[]> {
    return axios.get('lookups/job-titles')
  },

  // Committee Types
  listCommitteeTypes(): Promise<LookupItem[]> {
    return axios.get('lookups/committee-types')
  },

  // Committees (all)
  listCommittees(): Promise<LookupItem[]> {
    return axios.get('lookups/committees')
  },

  // Committee Classifications
  listCommitteeClassifications(): Promise<LookupItem[]> {
    return axios.get('lookups/committee-classification')
  },

  // Committee Styles
  listCommitteeStyles(): Promise<LookupItem[]> {
    return axios.get('lookups/committee-style')
  },

  // Committee Statuses
  listCommitteeStatuses(): Promise<LookupItem[]> {
    return axios.get('lookups/committee-status')
  },

  // Committee Roles
  listCommitteeRoles(): Promise<LookupItem[]> {
    return axios.get('lookups/committee-roles')
  },

  // Privacy Levels
  listPrivacies(): Promise<LookupItem[]> {
    return axios.get('lookups/privacies')
  },

  // Admin Lookup Management
  listLookups(): Promise<Lookup[]> {
    return axios.get('lookups/admin/list')
  },

  addLookup(lookup: LookupCreate): Promise<any> {
    return axios.post('lookups/admin', lookup)
  },

  updateLookup(lookupId: number, lookup: LookupCreate): Promise<any> {
    return axios.put(`lookups/admin/${lookupId}`, lookup)
  },

  deleteLookup(lookupId: number): Promise<any> {
    return axios.delete(`lookups/admin/${lookupId}`)
  },

  // Lookup Items Management
  listLookupItems(lookupId: number): Promise<LookupItemAdmin[]> {
    return axios.get(`lookups/admin/${lookupId}/items`)
  },

  addLookupItem(item: LookupItemCreate): Promise<any> {
    return axios.post('lookups/admin/items', item)
  },

  updateLookupItem(recordPK: number, item: LookupItemCreate): Promise<any> {
    return axios.put(`lookups/admin/items/${recordPK}`, item)
  },

  deleteLookupItem(recordPK: number): Promise<any> {
    return axios.delete(`lookups/admin/items/${recordPK}`)
  },

  // Datasources for TasksConfiguration
  listDatasources(): Promise<DataSourceLookup[]> {
    return axios.get('lookups/datasources')
  },

  listDataTables(connectionId: number): Promise<string[]> {
    return axios.get(`lookups/datasources/${connectionId}/tables`)
  },

  listDataFields(connectionId: number, tableName: string): Promise<string[]> {
    return axios.get(`lookups/datasources/${connectionId}/tables/${tableName}/fields`)
  },

  // Process Statuses
  listProcessStatuses(): Promise<LookupItem[]> {
    return axios.get('lookups/process-statuses')
  },

  // Roles
  listRoles(): Promise<LookupItem[]> {
    return axios.get('lookups/roles')
  }
}

export default LookupsService

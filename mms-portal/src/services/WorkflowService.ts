import { mainApiAxios as axios } from '@/plugins/axios'

// ─────── Enums ───────
export const ActorSourceType = {
  Role: 1,
  Group: 2,
  CommitteeRole: 3,
  Permission: 4,
  Stakeholder: 5,
  TeamLeader: 6,
  Creator: 7,
  SpecificUser: 8
} as const

export const WorkflowActionType = {
  Advance: 1,
  Approve: 2,
  Reject: 3,
  Auto: 4
} as const

export const WorkflowTaskStatus = {
  Pending: 1,
  InProgress: 2,
  Completed: 3,
  Cancelled: 4
} as const

// ─────── Types ───────
export interface ActorTarget { id: string; labelAr: string; labelEn: string }
export interface ActorOptions {
  roles: ActorTarget[]
  groups: ActorTarget[]
  committeeRoles: ActorTarget[]
  permissions: ActorTarget[]
}

export interface WorkflowStep {
  id: number
  templateId: number
  stepOrder: number
  nameAr: string
  nameEn: string
  isTerminal: boolean
  isAutoAdvance: boolean
  actorSourceType: number
  actorSourceTypeName: string
  actorTargetId?: string | null
  actorTargetLabel?: string | null
  taskTitleAr?: string | null
  taskTitleEn?: string | null
  taskBodyAr?: string | null
  taskBodyEn?: string | null
  slaDays?: number | null
  legacyBidStatusId?: number | null
}

export interface WorkflowTransition {
  id: number
  templateId: number
  fromStepId: number
  toStepId: number
  labelAr: string
  labelEn: string
  actionType: number
  actionTypeName: string
  displayOrder: number
}

export interface WorkflowTemplate {
  id: number
  nameAr: string
  nameEn: string
  descriptionAr?: string | null
  descriptionEn?: string | null
  committeeId?: number | null
  committeeName?: string | null
  version: number
  isActive: boolean
  initiatorActorSourceType?: number | null
  initiatorActorTargetId?: string | null
  createdDate: string
  steps: WorkflowStep[]
  transitions: WorkflowTransition[]
}

export interface WorkflowTemplateListItem {
  id: number
  nameAr: string
  nameEn: string
  committeeId?: number | null
  committeeName?: string | null
  isActive: boolean
  version: number
  stepsCount: number
  instancesCount: number
}

export interface WorkflowTemplatePost {
  nameAr: string
  nameEn: string
  descriptionAr?: string | null
  descriptionEn?: string | null
  committeeId?: number | null
  isActive: boolean
  initiatorActorSourceType?: number | null
  initiatorActorTargetId?: string | null
}

export interface WorkflowStepPost {
  stepOrder: number
  nameAr: string
  nameEn: string
  isTerminal: boolean
  isAutoAdvance: boolean
  actorSourceType: number
  actorTargetId?: string | null
  taskTitleAr?: string | null
  taskTitleEn?: string | null
  taskBodyAr?: string | null
  taskBodyEn?: string | null
  slaDays?: number | null
  legacyBidStatusId?: number | null
}

export interface WorkflowTransitionPost {
  fromStepId: number
  toStepId: number
  labelAr: string
  labelEn: string
  actionType: number
  displayOrder: number
}

export interface WorkflowInstance {
  id: number
  templateId: number
  templateName: string
  bidId: number
  currentStepId: number
  currentStepName: string
  currentStepOrder: number
  currentStepIsTerminal: boolean
  startedDate: string
  completedDate?: string | null
  availableTransitions: WorkflowTransition[]
  canCurrentUserAct: boolean
}

export interface WorkflowTask {
  id: number
  instanceId: number
  bidId: number
  bidReferenceNumber?: string | null
  bidSubject?: string | null
  committeeName?: string | null
  stepId: number
  stepName: string
  taskTitle?: string | null
  taskBody?: string | null
  assignedToUserId: string
  statusId: number
  statusName: string
  dueDate?: string | null
  claimedDate?: string | null
  completedDate?: string | null
  createdDate: string
  isDelayed: boolean
}

export interface WorkflowHistoryItem {
  id: number
  fromStepId?: number | null
  fromStepName?: string | null
  toStepId: number
  toStepName: string
  transitionId?: number | null
  transitionLabel?: string | null
  changedBy: string
  changedByName: string
  changedDate: string
  note?: string | null
}

const unwrap = <T>(res: any): T => (res?.data ?? res) as T

// ─────── Designer (admin-only) ───────
export const WorkflowDesignerService = {
  getActorOptions: () => axios.get('workflow-designer/actor-options').then(unwrap<ActorOptions>),
  listTemplates: () => axios.get('workflow-designer/templates').then(unwrap<WorkflowTemplateListItem[]>),
  getTemplate: (id: number) => axios.get(`workflow-designer/templates/${id}`).then(unwrap<WorkflowTemplate>),
  createTemplate: (dto: WorkflowTemplatePost) => axios.post('workflow-designer/templates', dto).then(unwrap<WorkflowTemplate>),
  updateTemplate: (id: number, dto: WorkflowTemplatePost) => axios.put(`workflow-designer/templates/${id}`, dto).then(unwrap<WorkflowTemplate>),
  deleteTemplate: (id: number) => axios.delete(`workflow-designer/templates/${id}`).then(unwrap<boolean>),

  addStep: (templateId: number, dto: WorkflowStepPost) => axios.post(`workflow-designer/templates/${templateId}/steps`, dto).then(unwrap<WorkflowStep>),
  updateStep: (stepId: number, dto: WorkflowStepPost) => axios.put(`workflow-designer/steps/${stepId}`, dto).then(unwrap<WorkflowStep>),
  deleteStep: (stepId: number) => axios.delete(`workflow-designer/steps/${stepId}`).then(unwrap<boolean>),

  addTransition: (templateId: number, dto: WorkflowTransitionPost) => axios.post(`workflow-designer/templates/${templateId}/transitions`, dto).then(unwrap<WorkflowTransition>),
  updateTransition: (transitionId: number, dto: WorkflowTransitionPost) => axios.put(`workflow-designer/transitions/${transitionId}`, dto).then(unwrap<WorkflowTransition>),
  deleteTransition: (transitionId: number) => axios.delete(`workflow-designer/transitions/${transitionId}`).then(unwrap<boolean>)
}

// ─────── Runtime (used by Bid pages and Tasks page) ───────
export const WorkflowService = {
  getInstanceForBid: (bidId: number) => axios.get(`workflow/bid/${bidId}/instance`).then(unwrap<WorkflowInstance | null>),
  getHistory: (instanceId: number) => axios.get(`workflow/instance/${instanceId}/history`).then(unwrap<WorkflowHistoryItem[]>),
  fireTransition: (instanceId: number, transitionId: number, note?: string, linkedMeetingId?: number | null) =>
    axios.post(`workflow/instance/${instanceId}/fire`, {
      transitionId,
      note: note || null,
      linkedMeetingId: linkedMeetingId || null
    }).then(unwrap<boolean>),
  myTasks: (includeCompleted = false) =>
    axios.get(`workflow/my-tasks?includeCompleted=${includeCompleted}`).then(unwrap<WorkflowTask[]>)
}

// Helpers for the designer UI: present user-friendly names for source types
export function actorSourceTypeLabel(type: number, isRtl = false): string {
  const map: Record<number, [string, string]> = {
    1: ['دور', 'Role'],
    2: ['مجموعة', 'Group'],
    3: ['دور لجنة', 'Committee Role'],
    4: ['صلاحية', 'Permission'],
    5: ['أصحاب المصلحة', 'Stakeholders'],
    6: ['قائد الفريق', 'Team Leader'],
    7: ['المنشئ', 'Creator'],
    8: ['مستخدم محدد', 'Specific User']
  }
  const pair = map[type] || ['?', '?']
  return isRtl ? pair[0] : pair[1]
}

export function actionTypeLabel(type: number, isRtl = false): string {
  const map: Record<number, [string, string]> = {
    1: ['تقدم', 'Advance'],
    2: ['موافقة', 'Approve'],
    3: ['رفض', 'Reject'],
    4: ['تلقائي', 'Auto']
  }
  const pair = map[type] || ['?', '?']
  return isRtl ? pair[0] : pair[1]
}

/**
 * TypeScript interfaces for MeetingRoom page
 */

// ═══════════════════════════════════════════════════════════════════════════
// CORE ENTITIES
// ═══════════════════════════════════════════════════════════════════════════

export interface LiveMeeting {
  id: number
  title?: string
  titleAr?: string
  statusId: number
  committeeName?: string
  councilName?: string
  councilId?: number
  committeeId?: number
  referenceNumber?: string
  refNumber?: string
  meetingNumber?: string
  location?: string
  startTime?: string
  fromTime?: string
  start?: string
  endTime?: string
  toTime?: string
  end?: string
  canControl?: boolean
  canManage?: boolean
  isOwner?: boolean
  /**
   * Permission to view Minutes of Meeting (MOM).
   * For council/committee meetings, backend should set this based on member permissions.
   * If not provided, only users with canControl/canManage can view MOMs.
   */
  canViewMom?: boolean
  canViewMinutes?: boolean
  createdby?: string | number
  createdBy?: string | number
  meetingAttendees?: MeetingAttendee[]
  meetingAgendas?: LiveAgenda[]
  attachments?: MeetingAttachment[]
}

export interface LiveAgenda {
  id: number
  title?: string
  titleAr?: string
  description?: string
  duration?: number
  displayOrder?: number
  isRunning?: boolean
  paused?: boolean
  remainingSeconds?: number
  elapsedSeconds?: number
  actualStartDate?: string
  actualEndDate?: string
  status?: string
  votingType?: AgendaVotingType
  votingOptions?: VotingOption[]
  meetingUserVotes?: UserVote[]
  attachments?: MeetingAttachment[]
}

export interface MeetingAttendee {
  id?: number
  odooId?: number
  odoo_id?: number
  meetingId?: number
  userId: string | number
  userFullName?: string
  fullName?: string
  name?: string
  userName?: string
  email?: string
  attended?: boolean
  attendedAt?: string
  imageUrl?: string
  avatar?: string
  profileImage?: string
  isRequired?: boolean
  responseStatus?: string
}

export interface MeetingAttachment {
  id: number
  fileName?: string
  name?: string
  fileSize?: string
  mimeType?: string
  url?: string
}

// ═══════════════════════════════════════════════════════════════════════════
// VOTING
// ═══════════════════════════════════════════════════════════════════════════

export interface AgendaVotingType {
  id: number
  nameAr?: string
  nameEn?: string
  name?: string
  votingOptions?: VotingOption[]
}

export interface VotingOption {
  id: number
  nameAr?: string
  nameEn?: string
  name?: string
  active?: boolean
  weight?: number
  displayOrder?: number
  votingTypeId?: number
}

export interface UserVote {
  id?: number
  userId: string | number
  UserId?: string | number
  meetingAgendaId: number
  votingOptionId?: number
  vottingOptionId?: number // API typo
  VotingOptionId?: number
  VottingOptionId?: number // API typo
  createdAt?: string
  userName?: string
  userFullName?: string
}

// ═══════════════════════════════════════════════════════════════════════════
// NOTES & RECOMMENDATIONS
// ═══════════════════════════════════════════════════════════════════════════

export interface AgendaNote {
  id: number
  meetingAgendaId: number
  text: string
  isPublic: boolean
  createdBy: string | number
  createdByName?: string
  createdAt?: string
  updatedAt?: string
}

export interface AgendaRecommendation {
  id: number
  meetingAgendaId: number
  text: string
  ownerName?: string
  ownerId?: string | number
  dueDate?: string
  status?: string
  createdAt?: string
  updatedAt?: string
}

// ═══════════════════════════════════════════════════════════════════════════
// CHAT
// ═══════════════════════════════════════════════════════════════════════════

export interface ChatMessage {
  id?: number
  meetingId: number
  userId: string | number
  userName?: string
  messageText: string
  sentAt?: string
  createdAt?: string
  timestamp?: string
  me?: boolean
}

// ═══════════════════════════════════════════════════════════════════════════
// AGGREGATED DATA (for summary views)
// ═══════════════════════════════════════════════════════════════════════════

export interface AgendaVotingData {
  id: number
  title: string
  votingOptions: VotingOption[]
  meetingUserVotes: UserVote[]
}

export interface AgendaData {
  id: number
  title: string
  notes: AgendaNote[]
  recommendations: AgendaRecommendation[]
}

// ═══════════════════════════════════════════════════════════════════════════
// API REQUEST/RESPONSE TYPES
// ═══════════════════════════════════════════════════════════════════════════

export interface AddNoteRequest {
  meetingAgendaId: number
  text: string
  isPublic: boolean
}

export interface EditNoteRequest extends AddNoteRequest {
  id: number
}

export interface SendVoteRequest {
  votingOptionId: number
  meetingAgendaId: number
}

export interface SendChatMessageRequest {
  meetingId: number
  messageText: string
}

export interface AddRecommendationRequest {
  meetingAgendaId: number
  text: string
  ownerId?: string | number
  dueDate?: string
}

export interface EditRecommendationRequest extends AddRecommendationRequest {
  id: number
}

// ═══════════════════════════════════════════════════════════════════════════
// COMPONENT PROPS INTERFACES
// ═══════════════════════════════════════════════════════════════════════════

export interface MeetingRoomViewerProps {
  currentAttachment: MeetingAttachment | null
  currentAttachmentUrl: string
  isLightTheme: boolean
}

export interface MeetingRoomControlPanelProps {
  collapsed: boolean
}

export interface MeetingRoomCollaborationProps {
  collapsed: boolean
}

// ═══════════════════════════════════════════════════════════════════════════
// COMPOSABLE RETURN TYPES
// ═══════════════════════════════════════════════════════════════════════════

export interface TimerState {
  remainingTime: number
  elapsedSeconds: number
  isPaused: boolean
}

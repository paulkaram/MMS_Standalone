/**
 * TypeScript interfaces for Minutes of Meeting (MoM) generation
 */

// ═══════════════════════════════════════════════════════════════════════════
// MINUTES DATA STRUCTURE
// ═══════════════════════════════════════════════════════════════════════════

export interface MinutesOfMeeting {
  // Header Info
  meetingId: number
  meetingNumber: string
  referenceNumber: string
  title: string
  titleAr: string

  // Committee/Council Info
  committeeName: string
  councilName: string
  organizationName: string
  organizationLogo?: string

  // Date & Time
  date: string
  hijriDate?: string
  startTime: string
  endTime: string
  actualDuration: string

  // Location
  location: string
  meetingType: 'physical' | 'virtual' | 'hybrid'
  virtualLink?: string

  // Attendees
  attendees: MinutesAttendee[]
  totalAttendees: number
  presentCount: number
  absentCount: number
  quorumMet: boolean

  // Agenda Items
  agendaItems: MinutesAgendaItem[]

  // Action Items / Tasks
  actionItems: MinutesActionItem[]

  // Summary
  meetingSummary: string
  nextMeetingDate?: string
  nextMeetingLocation?: string

  // Signatures
  chairmanName: string
  chairmanTitle: string
  secretaryName: string
  secretaryTitle: string

  // Versioning
  version: number
  versionLabel: string // e.g., "1.0", "1.1", "2.0"
  status: MinutesStatus
  generatedAt: string
  generatedBy: string
  approvedAt?: string
  approvedBy?: string
}

export interface MinutesAttendee {
  id: number
  name: string
  title: string
  role: AttendeeRole
  attended: boolean
  attendedAt?: string
  leftAt?: string
  notes?: string
  signatureRequired: boolean
  signed: boolean
}

export interface MinutesAgendaItem {
  index: number
  id: number
  title: string
  description?: string
  presenter?: string

  // Time tracking
  plannedDuration: number
  actualDuration?: number
  startedAt?: string
  endedAt?: string

  // Content
  summary?: string
  discussionNotes: MinutesNote[]

  // Voting (if applicable)
  hasVoting: boolean
  votingResults?: MinutesVotingResults
  decision?: string

  // Recommendations
  recommendations: MinutesRecommendation[]

  // Attachments discussed
  attachmentsDiscussed: string[]
}

export interface MinutesNote {
  id: number
  text: string
  isPublic: boolean
  authorName: string
  createdAt: string
}

export interface MinutesVotingResults {
  votingType: string
  totalVoters: number
  options: MinutesVotingOption[]
  outcome: string // e.g., "موافقة", "رفض", "تأجيل"
  outcomeDetails?: string
}

export interface MinutesVotingOption {
  id: number
  name: string
  nameAr: string
  voteCount: number
  percentage: number
  voters: string[] // Names of voters for this option
}

export interface MinutesRecommendation {
  id: number
  text: string
  ownerName?: string
  dueDate?: string
  priority?: 'high' | 'medium' | 'low'
}

export interface MinutesActionItem {
  id: number
  description: string
  assignedTo: string
  assignedToId: string
  dueDate: string
  priority: 'high' | 'medium' | 'low'
  status: 'pending' | 'in_progress' | 'completed'
  sourceAgendaId?: number
  sourceAgendaTitle?: string
}

// ═══════════════════════════════════════════════════════════════════════════
// ENUMS
// ═══════════════════════════════════════════════════════════════════════════

export type MinutesStatus =
  | 'draft'           // Initial generation
  | 'pending_review'  // Sent for review
  | 'under_revision'  // Being revised
  | 'pending_approval'// Waiting for approval
  | 'approved'        // Approved by chairman
  | 'published'       // Final and distributed
  | 'archived'        // Old version archived

export type AttendeeRole =
  | 'chairman'        // رئيس الاجتماع
  | 'vice_chairman'   // نائب الرئيس
  | 'secretary'       // أمين السر
  | 'member'          // عضو
  | 'guest'           // ضيف
  | 'observer'        // مراقب

// ═══════════════════════════════════════════════════════════════════════════
// VERSION HISTORY
// ═══════════════════════════════════════════════════════════════════════════

export interface MinutesVersion {
  id: number
  meetingId?: number
  version: number
  versionLabel?: string
  status: MinutesStatus

  // File info
  fileName?: string
  filePath?: string
  fileSize?: number
  mimeType?: string // 'application/pdf'
  attachmentId?: number // For direct attachment access
  createdAt?: string

  // Metadata
  generatedAt?: string
  generatedBy?: string
  generatedByName?: string

  // Changes
  changeNotes?: string
  changedSections?: string[]

  // Approval workflow
  reviewedAt?: string
  reviewedBy?: string
  reviewedByName?: string
  reviewNotes?: string

  approvedAt?: string
  approvedBy?: string
  approvedByName?: string
  approvalNotes?: string

  // Distribution
  publishedAt?: string
  distributedTo?: string[]
}

// ═══════════════════════════════════════════════════════════════════════════
// API REQUEST/RESPONSE
// ═══════════════════════════════════════════════════════════════════════════

export interface GenerateMinutesRequest {
  meetingId: number
  includePrivateNotes?: boolean
  includeVoterNames?: boolean
  templateId?: number
  language?: 'ar' | 'en' | 'both'
}

export interface GenerateMinutesResponse {
  success: boolean
  version: MinutesVersion
  previewUrl?: string
  downloadUrl: string
}

export interface UpdateMinutesRequest {
  versionId: number
  meetingId: number
  changes: Partial<MinutesOfMeeting>
  changeNotes: string
}

export interface ApproveMinutesRequest {
  versionId: number
  meetingId: number
  approvalNotes?: string
  signatureData?: string // Base64 signature image
}

// ═══════════════════════════════════════════════════════════════════════════
// TEMPLATE CONFIGURATION
// ═══════════════════════════════════════════════════════════════════════════

export interface MinutesTemplate {
  id: number
  name: string
  nameAr: string
  description?: string

  // Layout settings
  pageSize: 'A4' | 'Letter'
  orientation: 'portrait' | 'landscape'
  margins: {
    top: number
    bottom: number
    left: number
    right: number
  }

  // Sections to include
  sections: MinutesSection[]

  // Branding
  headerLogo?: string
  footerLogo?: string
  primaryColor: string
  secondaryColor: string

  // Fonts
  titleFont: string
  bodyFont: string

  isDefault: boolean
  organizationId?: number
}

export interface MinutesSection {
  id: string
  name: string
  nameAr: string
  enabled: boolean
  order: number
  required: boolean
}

// Default sections
export const DEFAULT_MINUTES_SECTIONS: MinutesSection[] = [
  { id: 'header', name: 'Header', nameAr: 'الترويسة', enabled: true, order: 1, required: true },
  { id: 'meeting_info', name: 'Meeting Info', nameAr: 'معلومات الاجتماع', enabled: true, order: 2, required: true },
  { id: 'attendees', name: 'Attendees', nameAr: 'الحضور', enabled: true, order: 3, required: true },
  { id: 'agenda_items', name: 'Agenda Items', nameAr: 'بنود جدول الأعمال', enabled: true, order: 4, required: true },
  { id: 'voting_results', name: 'Voting Results', nameAr: 'نتائج التصويت', enabled: true, order: 5, required: false },
  { id: 'recommendations', name: 'Recommendations', nameAr: 'التوصيات', enabled: true, order: 6, required: false },
  { id: 'action_items', name: 'Action Items', nameAr: 'المهام', enabled: true, order: 7, required: false },
  { id: 'summary', name: 'Summary', nameAr: 'الملخص', enabled: true, order: 8, required: false },
  { id: 'signatures', name: 'Signatures', nameAr: 'التوقيعات', enabled: true, order: 9, required: true },
  { id: 'footer', name: 'Footer', nameAr: 'التذييل', enabled: true, order: 10, required: true },
]

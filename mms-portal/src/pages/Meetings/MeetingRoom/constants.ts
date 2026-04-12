/**
 * Constants for MeetingRoom page
 */

// ═══════════════════════════════════════════════════════════════════════════
// MEETING STATUS
// ═══════════════════════════════════════════════════════════════════════════

export const MeetingStatus = {
  DRAFT: 1,
  PENDING_APPROVAL: 2,
  APPROVED: 3,
  STARTED: 4,
  FINISHED: 5,
  CANCELLED: 6,
} as const

export type MeetingStatusType = (typeof MeetingStatus)[keyof typeof MeetingStatus]

// ═══════════════════════════════════════════════════════════════════════════
// VOTING TYPES
// ═══════════════════════════════════════════════════════════════════════════

export const VotingType = {
  NONE: 2, // No voting
  YES_NO: 1,
  APPROVE_REJECT_ABSTAIN: 3,
  CUSTOM: 4,
} as const

export type VotingTypeId = (typeof VotingType)[keyof typeof VotingType]

// ═══════════════════════════════════════════════════════════════════════════
// AGENDA STATUS
// ═══════════════════════════════════════════════════════════════════════════

export const AgendaStatus = {
  PENDING: 'pending',
  RUNNING: 'running',
  PAUSED: 'paused',
  COMPLETED: 'completed',
} as const

// ═══════════════════════════════════════════════════════════════════════════
// SIGNALR EVENTS
// ═══════════════════════════════════════════════════════════════════════════

export const SignalREvents = {
  // Server -> Client events (lowercase as sent by server)
  MEETING_STARTED: 'meetingstarted',
  MEETING_ENDED: 'meetingended',
  MEETING_STATUS_CHANGE: 'meetingstatuschange', // + meetingId
  MEETING_AGENDA_CHANGES: 'meetingagendachanges', // + meetingId
  MEETING_CHAT_CHANGES: 'meetingchatchanges', // + meetingId
  MEETING_AGENDA_NOTES_CHANGES: 'meetingagendanoteschanges', // + meetingId
  NOTIFY_MEETING_ATTENDANCE_CHANGE: 'notifymeetingattendancechange',
  USER_ONLINE: 'useronline',

  // Client -> Server methods
  CHANGE_MEETING_ATTENDANCE_STATUS: 'ChangeMeetingAttendanceStatus',
} as const

// ═══════════════════════════════════════════════════════════════════════════
// UI CONSTANTS
// ═══════════════════════════════════════════════════════════════════════════

export const TIMER_INTERVAL_MS = 1000
export const DEBOUNCE_DELAY_MS = 300
export const CHAT_MAX_LENGTH = 160

// ═══════════════════════════════════════════════════════════════════════════
// LOCALIZED STRINGS (Arabic)
// ═══════════════════════════════════════════════════════════════════════════

// Message keys — values are i18n Dictionary keys, use with t(Messages.X)
export const Messages = {
  // Loading
  LOADING_MEETING: 'MsgLoadingMeeting',

  // Meeting actions
  MEETING_STARTED: 'MsgMeetingStarted',
  MEETING_ENDED: 'MsgMeetingEnded',
  MEETING_START_FAILED: 'MsgMeetingStartFailed',
  MEETING_END_FAILED: 'MsgMeetingEndFailed',
  MEETING_LOAD_ERROR: 'MsgMeetingLoadError',

  // Agenda actions
  AGENDA_COMPLETED: 'MsgAgendaCompleted',
  AGENDA_STATUS_FAILED: 'MsgAgendaStatusFailed',
  AGENDA_COMPLETE_FAILED: 'MsgAgendaCompleteFailed',
  AGENDA_NEXT_FAILED: 'MsgAgendaNextFailed',

  // Attendance
  ATTENDANCE_UPDATE_FAILED: 'MsgAttendanceUpdateFailed',

  // Voting
  VOTE_SUCCESS: 'MsgVoteSuccess',
  VOTE_FAILED: 'MsgVoteFailed',

  // Notes
  NOTE_ADDED: 'MsgNoteAdded',
  NOTE_UPDATED: 'MsgNoteUpdated',
  NOTE_DELETED: 'MsgNoteDeleted',
  NOTE_ADD_FAILED: 'MsgNoteAddFailed',
  NOTE_UPDATE_FAILED: 'MsgNoteUpdateFailed',
  NOTE_DELETE_FAILED: 'MsgNoteDeleteFailed',

  // Recommendations
  RECOMMENDATION_ADDED: 'MsgRecommendationAdded',
  RECOMMENDATION_UPDATED: 'MsgRecommendationUpdated',
  RECOMMENDATION_DELETED: 'MsgRecommendationDeleted',
  RECOMMENDATION_SAVE_FAILED: 'MsgRecommendationSaveFailed',
  RECOMMENDATION_DELETE_FAILED: 'MsgRecommendationDeleteFailed',

  // Summary
  MEETING_SUMMARY_SAVED: 'MsgMeetingSummarySaved',
  MEETING_SUMMARY_FAILED: 'MsgMeetingSummaryFailed',
  AGENDA_SUMMARY_SAVED: 'MsgAgendaSummarySaved',
  AGENDA_SUMMARY_FAILED: 'MsgAgendaSummaryFailed',

  // Attachments
  ATTACHMENT_LOAD_FAILED: 'MsgAttachmentLoadFailed',

  // Agenda status labels
  STATUS_COMPLETED: 'StatusCompleted',
  STATUS_PAUSED: 'StatusPaused',
  STATUS_ACTIVE: 'StatusActive',
  STATUS_UPCOMING: 'StatusUpcoming',
  STATUS_LIVE: 'StatusLive',
  STATUS_DISABLED: 'StatusDisabled',
  STATUS_ENDED: 'StatusEnded',

  // Minutes
  MINUTES_UPLOAD_COMING_SOON: 'MsgMinutesUploadComingSoon',
  MINUTES_GENERATE_COMING_SOON: 'MsgMinutesGenerateComingSoon',
} as const

// Meeting Status Enum
export const MeetingStatusEnum = Object.freeze({
  Draft: 1,
  PendingMeetingApproval: 2,
  Approved: 3,
  Started: 4,
  Finished: 5,
  PendingInitialMeetingMinutesApproval: 6,
  InitialMeetingMinutesApproved: 7,
  PendingFinalMeetingMinutesSign: 8,
  FinalMeetingMinutesSigned: 9,
  Canceled: 10
})

// Meeting Type Enum
export const MeetingTypeEnum = Object.freeze({
  Attend: 2,
  Remote: 3
})

// Agenda Recommendation Status Enum
export const AgendaRecommendationStatusEnum = Object.freeze({
  Draft: 1,
  InProgress: 2,
  Completed: 3
})

// Committee Type Enum
export const CommitteeType = Object.freeze({
  Council: 1,
  Committee: 2
})

// SignalR Status Enum
export const SignalRStatus = Object.freeze({
  Connecting: 0,
  Connected: 1,
  Reconnecting: 2,
  Disconnected: 3
})

// Attachments Record Type Enum
export const AttachmentsRecordType = Object.freeze({
  Meeting: 1,
  MeetingMinutes: 2,
  FinalMeetingMinutes: 3,
  AgendaRecommendation: 4,
  MeetingAgenda: 5
})

// Voting Types Enum
export const VotingTypesEnum = Object.freeze({
  YesOrNo: 1,
  WithoutVoting: 2,
  Evaluation: 3
})

// Meeting Minutes Types Enum
export const MeetingMinutesTypesEnum = Object.freeze({
  Initial: 1,
  Final: 2
})

// Task Type Enum
export const TaskTypeEnum = Object.freeze({
  General: 1,
  Meeting: 2,
  Recommendation: 3
})

// Task Status Enum
export const TaskStatusEnum = Object.freeze({
  New: 1,
  InProgress: 2,
  Completed: 3,
  Cancelled: 4
})

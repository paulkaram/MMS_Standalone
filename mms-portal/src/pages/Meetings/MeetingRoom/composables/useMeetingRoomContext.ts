import { provide, inject, type InjectionKey, type Ref, type ComputedRef } from 'vue'
import type {
  LiveMeeting,
  LiveAgenda,
  MeetingAttendee,
  MeetingAttachment,
  AgendaNote,
  AgendaRecommendation,
  VotingOption,
  AgendaVotingData,
  AgendaData,
  ChatMessage
} from '../types'

// ═══════════════════════════════════════════════════════════════════════════
// CONTEXT INTERFACE
// ═══════════════════════════════════════════════════════════════════════════

export interface MeetingRoomContext {
  // Core meeting data
  meeting: Ref<LiveMeeting | null>
  agendas: Ref<LiveAgenda[]>
  attendees: Ref<MeetingAttendee[]>
  attachments: Ref<MeetingAttachment[]>

  // Current state
  currentAgenda: ComputedRef<LiveAgenda | null>
  currentAgendaIndex: Ref<number>
  currentAttachment: Ref<MeetingAttachment | null>
  currentAttachmentUrl: Ref<string>

  // Status flags
  isLive: ComputedRef<boolean>
  meetingHasEnded: ComputedRef<boolean>
  canControl: ComputedRef<boolean>
  canStartMeeting: ComputedRef<boolean>
  canEndMeeting: ComputedRef<boolean>
  allAgendasCompleted: ComputedRef<boolean>

  // Timers
  meetingElapsedTime: ComputedRef<string>
  remainingTime: Ref<number>
  isPaused: Ref<boolean>

  // Notes
  agendaNotes: Ref<AgendaNote[]>
  noteText: Ref<string>
  noteIsPublic: Ref<boolean>
  editingNoteId: Ref<number | null>
  visibleNotes: ComputedRef<AgendaNote[]>

  // Recommendations
  agendaRecommendations: Ref<AgendaRecommendation[]>
  editingRecommendation: Ref<AgendaRecommendation | null>

  // Voting
  votingOptions: ComputedRef<VotingOption[]>
  votingActive: ComputedRef<boolean>
  canVote: ComputedRef<boolean>
  userVoteOptionId: Ref<number | null>
  votingLoading: Ref<boolean>

  // Summary data
  meetingSummary: Ref<string>
  agendaSummary: Ref<string>
  allAgendasVotingData: ComputedRef<AgendaVotingData[]>
  allAgendasData: ComputedRef<AgendaData[]>

  // Chat
  chatMessages: Ref<ChatMessage[]>
  chatInputText: Ref<string>

  // Attendees status
  presentCount: ComputedRef<number>
  onlineAttendeeIds: Ref<string[]>

  // All attachments combined
  allAttachments: ComputedRef<MeetingAttachment[]>

  // Actions
  selectAgenda: (index: number) => void
  selectAttachment: (attachment: MeetingAttachment) => Promise<void>
  toggleAgendaTimer: (agenda: LiveAgenda, index: number) => Promise<void>
  completeAgenda: (agenda: LiveAgenda, index: number) => Promise<void>
  markAttendance: (attendee: MeetingAttendee, present: boolean) => Promise<void>

  // Notes actions
  saveOrUpdateNote: () => void
  startEditNote: (note: AgendaNote) => void
  deleteAgendaNote: (noteId: number) => Promise<void>
  cancelEditNote: () => void

  // Recommendations actions
  openAddRecommendation: () => void
  editRecommendation: (rec: AgendaRecommendation) => void
  deleteRecommendation: (recId: number) => Promise<void>

  // Voting actions
  submitVote: (votingOptionId: number) => Promise<void>

  // Chat actions
  sendChatMessage: () => void

  // Summary actions
  loadMeetingSummary: () => Promise<void>
  saveMeetingSummary: (summary: string) => Promise<void>
  loadAgendaSummary: (agendaId: number) => Promise<void>
  saveAgendaSummary: (summary: string) => Promise<void>

  // Current user
  currentUserId: ComputedRef<string>

  // Loading states
  loading: Ref<boolean>
  actionLoading: Ref<boolean>

  // Legacy computed for backward compatibility
  voteTimer: ComputedRef<string>
  voteStatus: ComputedRef<string>
}

// ═══════════════════════════════════════════════════════════════════════════
// INJECTION KEY
// ═══════════════════════════════════════════════════════════════════════════

export const MeetingRoomContextKey: InjectionKey<MeetingRoomContext> = Symbol('MeetingRoomContext')

// ═══════════════════════════════════════════════════════════════════════════
// PROVIDER
// ═══════════════════════════════════════════════════════════════════════════

/**
 * Provide the meeting room context to child components
 */
export function provideMeetingRoomContext(context: MeetingRoomContext): void {
  provide(MeetingRoomContextKey, context)
}

// ═══════════════════════════════════════════════════════════════════════════
// CONSUMER
// ═══════════════════════════════════════════════════════════════════════════

/**
 * Inject the meeting room context in child components
 * @throws Error if used outside of MeetingRoom component
 */
export function useMeetingRoomContext(): MeetingRoomContext {
  const context = inject(MeetingRoomContextKey)

  if (!context) {
    throw new Error(
      'useMeetingRoomContext must be used within a component that has MeetingRoomContext provided'
    )
  }

  return context
}

/**
 * Safely inject the meeting room context (returns null if not provided)
 */
export function useMeetingRoomContextOptional(): MeetingRoomContext | null {
  return inject(MeetingRoomContextKey, null)
}

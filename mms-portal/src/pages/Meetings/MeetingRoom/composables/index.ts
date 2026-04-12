/**
 * Composables for MeetingRoom page
 *
 * Re-exports all composables for clean imports:
 * import { useMeetingRoom, useMeetingRoomContext } from './composables'
 */

// Main orchestrator
export { useMeetingRoom } from './useMeetingRoom'

// Context (provide/inject)
export {
  useMeetingRoomContext,
  useMeetingRoomContextOptional,
  provideMeetingRoomContext,
  MeetingRoomContextKey,
  type MeetingRoomContext
} from './useMeetingRoomContext'

// Individual composables
export { useMeetingTimers } from './useMeetingTimers'
export { useMeetingSignalR } from './useMeetingSignalR'
export { useMeetingChat } from './useMeetingChat'
export { useAgendaNotes } from './useAgendaNotes'
export { useAgendaRecommendations } from './useAgendaRecommendations'
export { useMeetingVoting } from './useMeetingVoting'
export { useMinutesGenerator } from './useMinutesGenerator'

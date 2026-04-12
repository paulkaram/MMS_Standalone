/**
 * Centralized Status Colors
 * Use these colors consistently across all components for status indicators
 */

import { MeetingStatusEnum, TaskStatusEnum, AgendaRecommendationStatusEnum } from './enumerations'

// Status color definitions with background and text colors
export interface StatusColor {
  bg: string
  text: string
  border?: string
}

// Meeting Status Colors (using MeetingStatusEnum)
export const MeetingStatusColors: Record<number, StatusColor> = {
  [MeetingStatusEnum.Draft]: { bg: 'rgba(113, 113, 122, 0.12)', text: '#71717a', border: 'rgba(113, 113, 122, 0.2)' },
  [MeetingStatusEnum.PendingMeetingApproval]: { bg: 'rgba(245, 158, 11, 0.12)', text: '#d97706', border: 'rgba(245, 158, 11, 0.2)' },
  [MeetingStatusEnum.Approved]: { bg: 'rgba(0, 174, 141, 0.12)', text: '#006d4b', border: 'rgba(0, 174, 141, 0.2)' },
  [MeetingStatusEnum.Started]: { bg: 'rgba(59, 130, 246, 0.12)', text: '#2563eb', border: 'rgba(59, 130, 246, 0.2)' },
  [MeetingStatusEnum.Finished]: { bg: 'rgba(16, 185, 129, 0.12)', text: '#059669', border: 'rgba(16, 185, 129, 0.2)' },
  [MeetingStatusEnum.PendingInitialMeetingMinutesApproval]: { bg: 'rgba(249, 115, 22, 0.12)', text: '#ea580c', border: 'rgba(249, 115, 22, 0.2)' },
  [MeetingStatusEnum.InitialMeetingMinutesApproved]: { bg: 'rgba(34, 197, 94, 0.12)', text: '#16a34a', border: 'rgba(34, 197, 94, 0.2)' },
  [MeetingStatusEnum.PendingFinalMeetingMinutesSign]: { bg: 'rgba(139, 92, 246, 0.12)', text: '#7c3aed', border: 'rgba(139, 92, 246, 0.2)' },
  [MeetingStatusEnum.FinalMeetingMinutesSigned]: { bg: 'rgba(0, 174, 141, 0.15)', text: '#006d4b', border: 'rgba(0, 174, 141, 0.25)' },
  [MeetingStatusEnum.Canceled]: { bg: 'rgba(239, 68, 68, 0.12)', text: '#dc2626', border: 'rgba(239, 68, 68, 0.2)' },
}

// Task Status Colors (using TaskStatusEnum)
export const TaskStatusColors: Record<number, StatusColor> = {
  [TaskStatusEnum.New]: { bg: 'rgba(59, 130, 246, 0.12)', text: '#2563eb', border: 'rgba(59, 130, 246, 0.2)' },
  [TaskStatusEnum.InProgress]: { bg: 'rgba(245, 158, 11, 0.12)', text: '#d97706', border: 'rgba(245, 158, 11, 0.2)' },
  [TaskStatusEnum.Completed]: { bg: 'rgba(0, 174, 141, 0.12)', text: '#006d4b', border: 'rgba(0, 174, 141, 0.2)' },
  [TaskStatusEnum.Cancelled]: { bg: 'rgba(239, 68, 68, 0.12)', text: '#dc2626', border: 'rgba(239, 68, 68, 0.2)' },
}

// Recommendation Status Colors (using AgendaRecommendationStatusEnum)
export const RecommendationStatusColors: Record<number, StatusColor> = {
  [AgendaRecommendationStatusEnum.Draft]: { bg: 'rgba(113, 113, 122, 0.12)', text: '#71717a', border: 'rgba(113, 113, 122, 0.2)' },
  [AgendaRecommendationStatusEnum.InProgress]: { bg: 'rgba(245, 158, 11, 0.12)', text: '#d97706', border: 'rgba(245, 158, 11, 0.2)' },
  [AgendaRecommendationStatusEnum.Completed]: { bg: 'rgba(0, 174, 141, 0.12)', text: '#006d4b', border: 'rgba(0, 174, 141, 0.2)' },
}

// Generic/Common Status Colors for reuse
export const CommonStatusColors = {
  draft: { bg: 'rgba(113, 113, 122, 0.12)', text: '#71717a', border: 'rgba(113, 113, 122, 0.2)' },
  pending: { bg: 'rgba(245, 158, 11, 0.12)', text: '#d97706', border: 'rgba(245, 158, 11, 0.2)' },
  inProgress: { bg: 'rgba(59, 130, 246, 0.12)', text: '#2563eb', border: 'rgba(59, 130, 246, 0.2)' },
  approved: { bg: 'rgba(0, 174, 141, 0.12)', text: '#006d4b', border: 'rgba(0, 174, 141, 0.2)' },
  completed: { bg: 'rgba(0, 174, 141, 0.12)', text: '#006d4b', border: 'rgba(0, 174, 141, 0.2)' },
  cancelled: { bg: 'rgba(239, 68, 68, 0.12)', text: '#dc2626', border: 'rgba(239, 68, 68, 0.2)' },
  error: { bg: 'rgba(239, 68, 68, 0.12)', text: '#dc2626', border: 'rgba(239, 68, 68, 0.2)' },
  warning: { bg: 'rgba(249, 115, 22, 0.12)', text: '#ea580c', border: 'rgba(249, 115, 22, 0.2)' },
  info: { bg: 'rgba(59, 130, 246, 0.12)', text: '#2563eb', border: 'rgba(59, 130, 246, 0.2)' },
  success: { bg: 'rgba(0, 174, 141, 0.12)', text: '#006d4b', border: 'rgba(0, 174, 141, 0.2)' },
}

// Default fallback color
export const DefaultStatusColor: StatusColor = {
  bg: 'rgba(113, 113, 122, 0.12)',
  text: '#71717a',
  border: 'rgba(113, 113, 122, 0.2)'
}

// Helper function to get meeting status color
export function getMeetingStatusColor(statusId: number): StatusColor {
  return MeetingStatusColors[statusId] || DefaultStatusColor
}

// Helper function to get task status color
export function getTaskStatusColor(statusId: number): StatusColor {
  return TaskStatusColors[statusId] || DefaultStatusColor
}

// Helper function to get recommendation status color
export function getRecommendationStatusColor(statusId: number): StatusColor {
  return RecommendationStatusColors[statusId] || DefaultStatusColor
}

// Helper to generate inline style object from status color
export function getStatusStyle(color: StatusColor): Record<string, string> {
  return {
    backgroundColor: color.bg,
    color: color.text,
    borderColor: color.border || 'transparent'
  }
}

// Helper for progress-based status (0-100%)
export function getProgressStatusColor(percentage: number): StatusColor {
  if (percentage >= 100) return CommonStatusColors.completed
  if (percentage >= 50) return CommonStatusColors.inProgress
  return CommonStatusColors.pending
}

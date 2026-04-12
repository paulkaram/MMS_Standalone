import { ref, computed, type ComputedRef, type Ref } from 'vue'
import MeetingAgendaService from '@/services/MeetingAgendaService'
import { useToast } from '@/composables/useToast'
import { useUserStore } from '@/stores/user'
import { Messages, VotingType } from '../constants'
import type { LiveAgenda, VotingOption, UserVote, AgendaVotingData } from '../types'

interface UseMeetingVotingOptions {
  currentAgenda: ComputedRef<LiveAgenda | null>
  agendas: Ref<LiveAgenda[]>
  isLive: ComputedRef<boolean>
}

/**
 * Composable for managing meeting voting
 *
 * Fixes:
 * - Handle API typo `vottingOptionId` vs `votingOptionId`
 * - Proper type coercion for userId comparison
 */
export function useMeetingVoting(options: UseMeetingVotingOptions) {
  const { currentAgenda, agendas, isLive } = options
  const { toast } = useToast()
  const userStore = useUserStore()

  // ═══════════════════════════════════════════════════════════════════════════
  // STATE
  // ═══════════════════════════════════════════════════════════════════════════

  const userVoteOptionId = ref<number | null>(null)
  const loading = ref(false)
  const showResultsModal = ref(false)

  // ═══════════════════════════════════════════════════════════════════════════
  // COMPUTED
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Whether current agenda allows voting
   */
  const canVote = computed(() => {
    const agenda = currentAgenda.value
    return agenda && agenda.votingType?.id && agenda.votingType.id !== VotingType.NONE && isLive.value
  })

  /**
   * Get voting options for current agenda
   */
  const votingOptions = computed((): VotingOption[] => {
    const agenda = currentAgenda.value
    if (!agenda) return []

    // Check multiple possible paths for voting options
    const options = agenda.votingType?.votingOptions || agenda.votingOptions || []
    if (!Array.isArray(options) || options.length === 0) return []

    // Sort by displayOrder and return all options
    return [...options].sort((a, b) =>
      (a.displayOrder || 0) - (b.displayOrder || 0)
    )
  })

  /**
   * Voting is automatically active when meeting is live and agenda has voting options
   */
  const votingActive = computed(() => {
    return isLive.value && votingOptions.value.length > 0
  })

  /**
   * Get all agendas with their voting data for the all-votes modal
   */
  const allAgendasVotingData = computed((): AgendaVotingData[] => {
    return agendas.value
      .map(agenda => ({
        id: agenda.id,
        title: agenda.title || agenda.titleAr || '',
        votingOptions: agenda.votingType?.votingOptions || agenda.votingOptions || [],
        meetingUserVotes: agenda.meetingUserVotes || []
      }))
      .filter(a => a.votingOptions.length > 0) // Only include agendas with voting
  })

  /**
   * Legacy computed for backward compatibility
   */
  const voteTimer = computed(() => '—')
  const voteStatus = computed(() => votingActive.value ? Messages.STATUS_LIVE : Messages.STATUS_DISABLED)

  // ═══════════════════════════════════════════════════════════════════════════
  // METHODS
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Find user's existing vote for current agenda
   * Handles API typo `vottingOptionId` vs `votingOptionId`
   */
  function findUserVote(): void {
    const agenda = currentAgenda.value
    if (!agenda?.meetingUserVotes || !Array.isArray(agenda.meetingUserVotes)) {
      userVoteOptionId.value = null
      return
    }

    const userId = userStore.user?.id
    if (!userId) {
      userVoteOptionId.value = null
      return
    }

    // Convert userId to string for comparison (API may return as string or number)
    const userIdStr = String(userId)

    // Filter all votes by this user, then get the LAST one (most recent)
    const userVotes = agenda.meetingUserVotes.filter((v: UserVote) => {
      const voteUserId = String(v.userId || v.UserId || '')
      return voteUserId === userIdStr
    })

    // Get the last vote (most recent) - in case there are duplicates
    const userVote = userVotes.length > 0 ? userVotes[userVotes.length - 1] : null

    // Handle both "vottingOptionId" (API typo) and "votingOptionId"
    userVoteOptionId.value = userVote?.vottingOptionId ||
      userVote?.votingOptionId ||
      userVote?.VottingOptionId ||
      userVote?.VotingOptionId ||
      null
  }

  /**
   * Submit vote for current agenda
   */
  async function submitVote(votingOptionId: number): Promise<void> {
    const agenda = currentAgenda.value
    if (!agenda || !votingActive.value) return

    loading.value = true
    try {
      const response: any = await MeetingAgendaService.sendVote({
        votingOptionId,
        meetingAgendaId: agenda.id
      })

      // Update user's vote locally
      userVoteOptionId.value = votingOptionId

      // Update the agenda's meetingUserVotes if response contains updated votes
      const votes = response?.data || response
      if (Array.isArray(votes)) {
        agenda.meetingUserVotes = votes
        // Re-find user vote after updating
        findUserVote()
      }

      toast.success(Messages.VOTE_SUCCESS)
    } catch (error) {
      console.error('Failed to submit vote:', error)
      toast.error(Messages.VOTE_FAILED)
    } finally {
      loading.value = false
    }
  }

  /**
   * Get the label for user's vote
   */
  function getUserVoteLabel(): string {
    if (!userVoteOptionId.value) return ''
    const option = votingOptions.value.find(o => o.id === userVoteOptionId.value)
    return option ? (option.nameAr || option.nameEn || option.name || '') : ''
  }

  /**
   * Check if option is approve type (by name patterns)
   */
  function isApproveOption(option: VotingOption): boolean {
    const name = (option.nameAr || option.nameEn || option.name || '').toLowerCase()
    return name.includes('موافق') || name.includes('approve') || name.includes('نعم') || name.includes('yes')
  }

  /**
   * Check if option is reject type (by name patterns)
   */
  function isRejectOption(option: VotingOption): boolean {
    const name = (option.nameAr || option.nameEn || option.name || '').toLowerCase()
    return name.includes('رفض') || name.includes('reject') || name.includes('لا') || name.includes('no')
  }

  /**
   * Get vote button class based on option type
   */
  function getVoteButtonClass(option: VotingOption): string[] {
    const classes: string[] = []

    if (isApproveOption(option)) {
      classes.push('approve')
    } else if (isRejectOption(option)) {
      classes.push('reject')
    } else {
      classes.push('abstain')
    }

    if (userVoteOptionId.value === option.id) {
      classes.push('voted')
    }

    return classes
  }

  /**
   * Open voting results modal
   */
  function openResultsModal(): void {
    showResultsModal.value = true
  }

  /**
   * Close voting results modal
   */
  function closeResultsModal(): void {
    showResultsModal.value = false
  }

  /**
   * Clear voting state
   */
  function clearState(): void {
    userVoteOptionId.value = null
    closeResultsModal()
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // RETURN
  // ═══════════════════════════════════════════════════════════════════════════

  return {
    // State
    userVoteOptionId,
    loading,
    showResultsModal,

    // Computed
    canVote,
    votingOptions,
    votingActive,
    allAgendasVotingData,
    voteTimer,
    voteStatus,

    // Methods
    findUserVote,
    submitVote,
    getUserVoteLabel,
    isApproveOption,
    isRejectOption,
    getVoteButtonClass,
    openResultsModal,
    closeResultsModal,
    clearState,
  }
}

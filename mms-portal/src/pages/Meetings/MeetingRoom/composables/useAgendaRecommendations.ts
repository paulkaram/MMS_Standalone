import { ref, type ComputedRef } from 'vue'
import MeetingAgendaService from '@/services/MeetingAgendaService'
import { useToast } from '@/composables/useToast'
import { Messages } from '../constants'
import type { AgendaRecommendation, LiveAgenda, AddRecommendationRequest, EditRecommendationRequest } from '../types'

interface UseAgendaRecommendationsOptions {
  currentAgenda: ComputedRef<LiveAgenda | null>
}

/**
 * Composable for managing agenda recommendations
 */
export function useAgendaRecommendations(options: UseAgendaRecommendationsOptions) {
  const { currentAgenda } = options
  const { toast } = useToast()

  // ═══════════════════════════════════════════════════════════════════════════
  // STATE
  // ═══════════════════════════════════════════════════════════════════════════

  const recommendations = ref<AgendaRecommendation[]>([])
  const editingRecommendation = ref<AgendaRecommendation | null>(null)
  const showModal = ref(false)
  const loading = ref(false)

  // Store all agendas recommendations for summary view
  const allAgendasRecommendationsMap = ref<Record<number, AgendaRecommendation[]>>({})

  // ═══════════════════════════════════════════════════════════════════════════
  // METHODS
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Load recommendations for an agenda
   */
  async function loadRecommendations(agendaId: number): Promise<void> {
    loading.value = true
    try {
      const response: any = await MeetingAgendaService.getRecommendations(agendaId)
      const data = response?.data || response || []
      recommendations.value = data
      // Also update the map for all-agendas view
      allAgendasRecommendationsMap.value[agendaId] = data
    } catch (error) {
      console.error('Failed to load agenda recommendations:', error)
      recommendations.value = []
    } finally {
      loading.value = false
    }
  }

  /**
   * Add a new recommendation
   */
  async function addRecommendation(data: AddRecommendationRequest): Promise<void> {
    const agenda = currentAgenda.value
    if (!agenda) return

    try {
      await MeetingAgendaService.addRecommendation(data as any)
      await loadRecommendations(agenda.id)
      closeModal()
      toast.success(Messages.RECOMMENDATION_ADDED)
    } catch (error) {
      console.error('Failed to add recommendation:', error)
      toast.error(Messages.RECOMMENDATION_SAVE_FAILED)
    }
  }

  /**
   * Edit an existing recommendation
   */
  async function editRecommendation(data: EditRecommendationRequest): Promise<void> {
    const agenda = currentAgenda.value
    if (!agenda) return

    try {
      await MeetingAgendaService.editRecommendation(data as any)
      await loadRecommendations(agenda.id)
      closeModal()
      toast.success(Messages.RECOMMENDATION_UPDATED)
    } catch (error) {
      console.error('Failed to edit recommendation:', error)
      toast.error(Messages.RECOMMENDATION_SAVE_FAILED)
    }
  }

  /**
   * Save recommendation (add or edit based on id)
   */
  async function saveRecommendation(data: AddRecommendationRequest | EditRecommendationRequest): Promise<void> {
    if ('id' in data && data.id) {
      await editRecommendation(data as EditRecommendationRequest)
    } else {
      await addRecommendation(data)
    }
  }

  /**
   * Delete a recommendation
   */
  async function deleteRecommendation(recommendationId: number): Promise<void> {
    const agenda = currentAgenda.value
    if (!agenda) return

    try {
      await MeetingAgendaService.deleteRecommendation(recommendationId)
      await loadRecommendations(agenda.id)
      toast.success(Messages.RECOMMENDATION_DELETED)
    } catch (error) {
      console.error('Failed to delete recommendation:', error)
      toast.error(Messages.RECOMMENDATION_DELETE_FAILED)
    }
  }

  /**
   * Open modal for adding new recommendation
   */
  function openAddModal(): void {
    editingRecommendation.value = null
    showModal.value = true
  }

  /**
   * Open modal for editing recommendation
   */
  function openEditModal(recommendation: AgendaRecommendation): void {
    editingRecommendation.value = recommendation
    showModal.value = true
  }

  /**
   * Close modal
   */
  function closeModal(): void {
    showModal.value = false
    editingRecommendation.value = null
  }

  /**
   * Load recommendations for all agendas (for summary view)
   */
  async function loadAllAgendasRecommendations(agendas: LiveAgenda[]): Promise<void> {
    const promises = agendas.map(async (agenda) => {
      try {
        const response: any = await MeetingAgendaService.getRecommendations(agenda.id)
        allAgendasRecommendationsMap.value[agenda.id] = response?.data || response || []
      } catch {
        allAgendasRecommendationsMap.value[agenda.id] = []
      }
    })

    await Promise.all(promises)
  }

  /**
   * Clear all state
   */
  function clearState(): void {
    recommendations.value = []
    closeModal()
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // RETURN
  // ═══════════════════════════════════════════════════════════════════════════

  return {
    // State
    recommendations,
    editingRecommendation,
    showModal,
    loading,
    allAgendasRecommendationsMap,

    // Methods
    loadRecommendations,
    addRecommendation,
    editRecommendation,
    saveRecommendation,
    deleteRecommendation,
    openAddModal,
    openEditModal,
    closeModal,
    loadAllAgendasRecommendations,
    clearState,
  }
}

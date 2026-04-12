import { ref, computed, type ComputedRef } from 'vue'
import MeetingAgendaService from '@/services/MeetingAgendaService'
import { useToast } from '@/composables/useToast'
import { useUserStore } from '@/stores/user'
import { Messages } from '../constants'
import type { AgendaNote, LiveAgenda } from '../types'

interface UseAgendaNotesOptions {
  currentAgenda: ComputedRef<LiveAgenda | null>
}

/**
 * Composable for managing agenda notes
 */
export function useAgendaNotes(options: UseAgendaNotesOptions) {
  const { currentAgenda } = options
  const { toast } = useToast()
  const userStore = useUserStore()

  // ═══════════════════════════════════════════════════════════════════════════
  // STATE
  // ═══════════════════════════════════════════════════════════════════════════

  const notes = ref<AgendaNote[]>([])
  const noteText = ref('')
  const noteIsPublic = ref(true)
  const editingNoteId = ref<number | null>(null)
  const loading = ref(false)

  // Store all agendas notes for summary view
  const allAgendasNotesMap = ref<Record<number, AgendaNote[]>>({})

  // ═══════════════════════════════════════════════════════════════════════════
  // COMPUTED
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Filter notes: show public notes + user's own private notes
   * Uses == for type coercion (createdBy may be int, userId may be string)
   */
  const visibleNotes = computed(() => {
    const userId = userStore.user?.id
    return notes.value.filter(note =>
      note.isPublic || note.createdBy == userId
    )
  })

  /**
   * Current user ID
   */
  const currentUserId = computed(() => userStore.user?.id || '')

  // ═══════════════════════════════════════════════════════════════════════════
  // METHODS
  // ═══════════════════════════════════════════════════════════════════════════

  /**
   * Load notes for an agenda
   */
  async function loadNotes(agendaId: number): Promise<void> {
    loading.value = true
    try {
      const response: any = await MeetingAgendaService.getNotes(agendaId)
      const data = response?.data || response || []
      notes.value = data
      // Also update the map for all-agendas view
      allAgendasNotesMap.value[agendaId] = data
    } catch (error) {
      console.error('Failed to load agenda notes:', error)
      notes.value = []
    } finally {
      loading.value = false
    }
  }

  /**
   * Add a new note
   */
  async function addNote(): Promise<void> {
    const agenda = currentAgenda.value
    if (!agenda || !noteText.value.trim()) return

    try {
      await MeetingAgendaService.addNote({
        meetingAgendaId: agenda.id,
        text: noteText.value.trim(),
        isPublic: noteIsPublic.value
      })

      noteText.value = ''
      noteIsPublic.value = true
      await loadNotes(agenda.id)
      toast.success(Messages.NOTE_ADDED)
    } catch (error) {
      console.error('Failed to add note:', error)
      toast.error(Messages.NOTE_ADD_FAILED)
    }
  }

  /**
   * Edit an existing note
   */
  async function editNote(): Promise<void> {
    const agenda = currentAgenda.value
    if (!agenda || !editingNoteId.value || !noteText.value.trim()) return

    try {
      await MeetingAgendaService.editNote({
        id: editingNoteId.value,
        meetingAgendaId: agenda.id,
        text: noteText.value.trim(),
        isPublic: noteIsPublic.value
      })

      noteText.value = ''
      noteIsPublic.value = true
      editingNoteId.value = null
      await loadNotes(agenda.id)
      toast.success(Messages.NOTE_UPDATED)
    } catch (error) {
      console.error('Failed to edit note:', error)
      toast.error(Messages.NOTE_UPDATE_FAILED)
    }
  }

  /**
   * Delete a note
   */
  async function deleteNote(noteId: number): Promise<void> {
    const agenda = currentAgenda.value
    if (!agenda) return

    try {
      await MeetingAgendaService.deleteNote(noteId)
      await loadNotes(agenda.id)
      toast.success(Messages.NOTE_DELETED)
    } catch (error) {
      console.error('Failed to delete note:', error)
      toast.error(Messages.NOTE_DELETE_FAILED)
    }
  }

  /**
   * Start editing a note
   */
  function startEdit(note: AgendaNote): void {
    editingNoteId.value = note.id
    noteText.value = note.text
    noteIsPublic.value = note.isPublic
  }

  /**
   * Cancel editing
   */
  function cancelEdit(): void {
    editingNoteId.value = null
    noteText.value = ''
    noteIsPublic.value = true
  }

  /**
   * Save or update note based on current state
   */
  function saveOrUpdate(): void {
    if (editingNoteId.value) {
      editNote()
    } else {
      addNote()
    }
  }

  /**
   * Load notes for all agendas (for summary view)
   */
  async function loadAllAgendasNotes(agendas: LiveAgenda[]): Promise<void> {
    const promises = agendas.map(async (agenda) => {
      try {
        const response: any = await MeetingAgendaService.getNotes(agenda.id)
        allAgendasNotesMap.value[agenda.id] = response?.data || response || []
      } catch {
        allAgendasNotesMap.value[agenda.id] = []
      }
    })

    await Promise.all(promises)
  }

  /**
   * Clear all state (for agenda change)
   */
  function clearState(): void {
    cancelEdit()
    notes.value = []
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // RETURN
  // ═══════════════════════════════════════════════════════════════════════════

  return {
    // State
    notes,
    noteText,
    noteIsPublic,
    editingNoteId,
    loading,
    allAgendasNotesMap,

    // Computed
    visibleNotes,
    currentUserId,

    // Methods
    loadNotes,
    addNote,
    editNote,
    deleteNote,
    startEdit,
    cancelEdit,
    saveOrUpdate,
    loadAllAgendasNotes,
    clearState,
  }
}

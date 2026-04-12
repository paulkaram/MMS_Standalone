<template>
  <Modal
    :model-value="show"
    :title="showAllAgendas ? $t('CommentsAllAgendas') : $t('AgendaComments')"
    :description="!showAllAgendas && agendaTitle ? agendaTitle : undefined"
    icon="mdi:chat-outline"
    :size="showAllAgendas ? '3xl' : 'xl'"
    scrollable
    no-padding
    @update:model-value="(val: boolean) => { if (!val) $emit('close') }"
  >

        <!-- Body -->
        <div class="modalBody">
          <!-- ALL AGENDAS MODE -->
          <template v-if="showAllAgendas">
            <div v-if="!allAgendasData?.length" class="emptyState">
              <Icon icon="chat" class="w-12 h-12" />
              <span>{{ $t('NoAgendaItems') }}</span>
            </div>

            <div v-else class="allAgendasList">
              <div
                v-for="(agenda, index) in allAgendasData"
                :key="agenda.id"
                class="agendaSection"
              >
                <div class="agendaSectionHeader">
                  <span class="agendaNumber">{{ index + 1 }}</span>
                  <h4>{{ agenda.title || 'بند ' + (index + 1) }}</h4>
                  <span class="commentCount">{{ agenda.notes?.length || 0 }} {{ $t('Comment') }}</span>
                </div>

                <div v-if="!agenda.notes?.length" class="noItemsMsg">
                  {{ $t('NoCommentsYet') }}
                </div>

                <div v-if="agenda.notes?.length" class="commentsList compact">
                  <div
                    v-for="note in agenda.notes"
                    :key="note.id"
                    class="commentItem"
                  >
                    <div class="commentTop">
                      <div class="commentAuthor">
                        <span class="authorName">{{ note.createdByName || 'مستخدم' }}</span>
                        <span class="commentDate">{{ formatDate(note.createdAt) }}</span>
                      </div>
                      <div class="commentBadges">
                        <span class="visibilityBadge" :class="note.isPublic ? 'public' : 'private'">
                          <Icon v-if="note.isPublic" icon="public" class="w-3 h-3" />
                          <Icon v-else icon="lock" class="w-3 h-3" />
                          {{ note.isPublic ? $t('Public') : $t('Private') }}
                        </span>
                        <template v-if="note.createdBy == currentUserId && !isReadOnly">
                          <button class="iconBtn" @click="startInlineEdit(note, agenda.id)" :title="$t('Edit')">
                            <Icon icon="edit" class="w-3 h-3" />
                          </button>
                          <button class="iconBtn danger" @click="$emit('delete-note', note.id)" :title="$t('Delete')">
                            <Icon icon="delete" class="w-3 h-3" />
                          </button>
                        </template>
                      </div>
                    </div>
                    <div class="commentText">{{ note.text }}</div>
                  </div>
                </div>

                <!-- Inline add/edit form for this agenda -->
                <div v-if="addingForAgendaId === agenda.id && !isReadOnly" class="inlineAddForm">
                  <div class="inlineFormRow">
                    <input
                      ref="inlineInputRef"
                      v-model="inlineNoteText"
                      type="text"
                      class="inlineInput"
                      :placeholder="editingNoteInline ? $t('EditCommentPlaceholder') : $t('AddCommentPlaceholder')"
                      @keyup.enter="submitInlineNote(agenda.id)"
                    />
                    <button
                      class="inlineSendBtn"
                      :class="{ editing: editingNoteInline }"
                      @click="submitInlineNote(agenda.id)"
                      :disabled="!inlineNoteText.trim()"
                    >
                      <Icon :icon="editingNoteInline ? 'check' : 'send'" class="w-3 h-3" />
                    </button>
                  </div>
                  <div class="inlineFormFooter">
                    <div class="visibilitySwitch small">
                      <button
                        type="button"
                        class="visSwitchBtn"
                        :class="{ active: inlineNoteIsPublic }"
                        @click="inlineNoteIsPublic = true"
                      >
                        <Icon icon="public" class="w-2.5 h-2.5" /> {{ $t('Public') }}
                      </button>
                      <button
                        type="button"
                        class="visSwitchBtn private"
                        :class="{ active: !inlineNoteIsPublic }"
                        @click="inlineNoteIsPublic = false"
                      >
                        <Icon icon="lock" class="w-2.5 h-2.5" /> {{ $t('Private') }}
                      </button>
                    </div>
                    <button class="inlineCancelBtn" @click="cancelInlineAdd">
                      <Icon icon="close" class="w-3 h-3" /> {{ $t('Cancel') }}
                    </button>
                  </div>
                </div>

                <!-- Add button per agenda in all agendas mode -->
                <div v-if="!isReadOnly && addingForAgendaId !== agenda.id" class="agendaAddBtn">
                  <button class="addCommentBtn" @click="startInlineAdd(agenda.id)">
                    <Icon icon="add" class="w-3.5 h-3.5" />
                    {{ $t('AddComment') }}
                  </button>
                </div>
              </div>
            </div>
          </template>

          <!-- SINGLE AGENDA MODE -->
          <template v-else>
            <div v-if="notes.length === 0" class="emptyState">
              <Icon icon="chat" class="w-12 h-12" />
              <span>{{ $t('NoCommentsYet') }}</span>
            </div>

            <div v-else class="commentsList">
              <div
                v-for="note in notes"
                :key="note.id"
                class="commentItem"
              >
                <div class="commentTop">
                  <div class="commentAuthor">
                    <span class="authorName">{{ note.createdByName || 'مستخدم' }}</span>
                    <span class="commentDate">{{ formatDate(note.createdAt) }}</span>
                  </div>
                  <div class="commentBadges">
                    <span class="visibilityBadge" :class="note.isPublic ? 'public' : 'private'">
                      <Icon v-if="note.isPublic" icon="public" class="w-3 h-3" />
                      <Icon v-else icon="lock" class="w-3 h-3" />
                      {{ note.isPublic ? $t('Public') : $t('Private') }}
                    </span>
                    <template v-if="note.createdBy == currentUserId && !isReadOnly">
                      <button class="iconBtn" @click="$emit('edit-note', note)" :title="$t('Edit')">
                        <Icon icon="edit" class="w-3 h-3" />
                      </button>
                      <button class="iconBtn danger" @click="$emit('delete-note', note.id)" :title="$t('Delete')">
                        <Icon icon="delete" class="w-3 h-3" />
                      </button>
                    </template>
                  </div>
                </div>
                <div class="commentText">{{ note.text }}</div>
              </div>
            </div>
          </template>
        </div>

        <!-- Footer - Only show form in single agenda mode and not read-only -->
        <div v-if="!showAllAgendas && !isReadOnly" class="modalFooter">
          <div class="footerForm">
            <div class="footerInputRow">
              <input
                type="text"
                class="commentInput"
                :value="noteText"
                @input="$emit('update:noteText', ($event.target as HTMLInputElement).value)"
                :placeholder="editingNoteId ? $t('EditCommentPlaceholder') : $t('AddCommentPlaceholder')"
                @keyup.enter="$emit('save-note')"
              />
              <button
                class="footerSendBtn"
                :class="{ editing: !!editingNoteId }"
                @click="$emit('save-note')"
                :disabled="!noteText.trim()"
              >
                <Icon :icon="editingNoteId ? 'check' : 'send'" class="w-3.5 h-3.5" />
              </button>
            </div>
            <div class="footerOptions">
              <div class="visibilitySwitch">
                <button
                  type="button"
                  class="visSwitchBtn"
                  :class="{ active: noteIsPublic }"
                  @click="$emit('update:noteIsPublic', true)"
                >
                  <Icon icon="public" class="w-3 h-3" /> {{ $t('Public') }}
                </button>
                <button
                  type="button"
                  class="visSwitchBtn private"
                  :class="{ active: !noteIsPublic }"
                  @click="$emit('update:noteIsPublic', false)"
                >
                  <Icon icon="lock" class="w-3 h-3" /> {{ $t('Private') }}
                </button>
              </div>
              <button
                v-if="editingNoteId"
                class="footerCancelBtn"
                @click="$emit('cancel-edit')"
              >
                <Icon icon="close" class="w-3 h-3" /> {{ $t('Cancel') }}
              </button>
            </div>
          </div>
        </div>

        <!-- Footer for all agendas mode -->
        <div v-else class="modalFooter simple">
          <button class="btn ghost" @click="$emit('close')">{{ $t('Close') }}</button>
        </div>
  </Modal>
</template>

<script setup lang="ts">
import { ref, computed, nextTick } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import { formatDateTime } from '@/helpers/dateFormat'

interface AgendaData {
  id: number
  title: string
  notes: any[]
  recommendations: any[]
}

const props = defineProps<{
  show: boolean
  notes: any[]
  agendaTitle: string
  currentUserId: string
  noteText: string
  noteIsPublic: boolean
  editingNoteId: number | null
  showAllAgendas?: boolean
  allAgendasData?: AgendaData[]
  isReadOnly?: boolean
  canControl?: boolean
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'edit-note', note: any): void
  (e: 'delete-note', noteId: number): void
  (e: 'update:noteText', value: string): void
  (e: 'update:noteIsPublic', value: boolean): void
  (e: 'save-note'): void
  (e: 'cancel-edit'): void
  (e: 'add-note-for-agenda', agendaId: number): void
  (e: 'save-note-for-agenda', agendaId: number, text: string, isPublic: boolean): void
  (e: 'edit-note-for-agenda', noteId: number, agendaId: number, text: string, isPublic: boolean): void
}>()

// Inline add/edit form state
const addingForAgendaId = ref<number | null>(null)
const editingNoteInline = ref<any>(null)
const inlineNoteText = ref('')
const inlineNoteIsPublic = ref(true)
const inlineInputRef = ref<HTMLInputElement | HTMLInputElement[] | null>(null)

const totalAllComments = computed(() => {
  if (!props.allAgendasData) return 0
  return props.allAgendasData.reduce((sum, agenda) => sum + (agenda.notes?.length || 0), 0)
})

function startInlineAdd(agendaId: number) {
  editingNoteInline.value = null
  addingForAgendaId.value = agendaId
  inlineNoteText.value = ''
  inlineNoteIsPublic.value = true
  nextTick(() => {
    // When ref is inside v-for, it becomes an array
    const input = Array.isArray(inlineInputRef.value) ? inlineInputRef.value[0] : inlineInputRef.value
    input?.focus()
  })
}

function startInlineEdit(note: any, agendaId: number) {
  addingForAgendaId.value = agendaId
  editingNoteInline.value = note
  inlineNoteText.value = note.text
  inlineNoteIsPublic.value = note.isPublic
  nextTick(() => {
    // When ref is inside v-for, it becomes an array
    const input = Array.isArray(inlineInputRef.value) ? inlineInputRef.value[0] : inlineInputRef.value
    input?.focus()
  })
}

function cancelInlineAdd() {
  addingForAgendaId.value = null
  editingNoteInline.value = null
  inlineNoteText.value = ''
  inlineNoteIsPublic.value = true
}

function submitInlineNote(agendaId: number) {
  if (!inlineNoteText.value.trim()) return
  if (editingNoteInline.value) {
    emit('edit-note-for-agenda', editingNoteInline.value.id, agendaId, inlineNoteText.value.trim(), inlineNoteIsPublic.value)
  } else {
    emit('save-note-for-agenda', agendaId, inlineNoteText.value.trim(), inlineNoteIsPublic.value)
  }
  cancelInlineAdd()
}

const formatDate = (dateStr?: string) => {
  if (!dateStr) return ''
  try {
    return formatDateTime(new Date(dateStr))
  } catch {
    return dateStr
  }
}
</script>

<style scoped>
.commentsModalOverlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10001;
  padding: 20px;
}

.commentsModal {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: 16px;
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.4);
}

.modalHeader {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
  color: #fff;
}

.headerTitle {
  display: flex;
  align-items: center;
  gap: 10px;
}

.headerTitle h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
}

.agendaName,
.agendaCount {
  font-size: 12px;
  background: rgba(255, 255, 255, 0.2);
  padding: 4px 10px;
  border-radius: 4px;
}

.commentsModal.wide {
  max-width: 800px;
}

.commentsModal.wide .modalBody {
  max-height: 500px;
}

.closeBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: none;
  background: rgba(255, 255, 255, 0.15);
  color: #fff;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.closeBtn:hover {
  background: rgba(255, 255, 255, 0.25);
}

.modalBody {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  min-height: 200px;
  max-height: 400px;
  scrollbar-width: thin;
  scrollbar-color: var(--border2) transparent;
}

.modalBody::-webkit-scrollbar {
  width: 6px;
}

.modalBody::-webkit-scrollbar-track {
  background: transparent;
  border-radius: 3px;
}

.modalBody::-webkit-scrollbar-thumb {
  background: var(--border2);
  border-radius: 3px;
  transition: background 0.2s ease;
}

.modalBody::-webkit-scrollbar-thumb:hover {
  background: var(--muted2);
}

.emptyState {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 60px 20px;
  color: var(--muted);
}

.commentsList {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.commentItem {
  background: var(--card2);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 14px;
}

.commentTop {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 10px;
}

.commentAuthor {
  display: flex;
  align-items: center;
  gap: 10px;
}

.authorName {
  font-size: 13px;
  font-weight: 600;
  color: var(--text);
}

.commentDate {
  font-size: 11px;
  color: var(--muted2);
}

.commentBadges {
  display: flex;
  align-items: center;
  gap: 6px;
}

.visibilityBadge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 3px 9px;
  border-radius: 4px;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.02em;
}

.visibilityBadge.public {
  background: #004730;
  color: #ffffff;
}

.visibilityBadge.private {
  background: #92400e;
  color: #ffffff;
}

.iconBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  border: none;
  background: transparent;
  color: var(--muted);
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.iconBtn:hover {
  background: var(--hover-bg);
  color: var(--text);
}

.iconBtn.danger:hover {
  background: rgba(239, 68, 68, 0.15);
  color: #ef4444;
}

.commentText {
  font-size: 14px;
  color: var(--text);
  line-height: 1.6;
}

.modalFooter {
  display: flex;
  align-items: center;
  padding: 16px 20px;
  background: var(--card2);
  border-top: 1px solid var(--border);
}

/* ── Footer form ── */
.footerForm {
  display: flex;
  flex-direction: column;
  gap: 8px;
  width: 100%;
}

.footerInputRow {
  display: flex;
  gap: 8px;
  align-items: center;
}

.commentInput {
  flex: 1;
  min-width: 0;
  padding: 10px 14px;
  border: 1px solid #e2e8f0;
  background: #fff;
  border-radius: 8px;
  color: #0f172a;
  font-size: 13px;
  outline: none;
  font-family: inherit;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.commentInput:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.commentInput::placeholder {
  color: #94a3b8;
}

.footerSendBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  flex-shrink: 0;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  background: linear-gradient(135deg, #004730 0%, #006d4b 100%);
  color: #fff;
  transition: all 0.15s ease;
}

.footerSendBtn:hover:not(:disabled) {
  box-shadow: 0 4px 12px rgba(0, 109, 75, 0.3);
  transform: translateY(-1px);
}

.footerSendBtn.editing {
  background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%);
}

.footerSendBtn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.footerOptions {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* ── Visibility switch (Public / Private segmented control) ── */
.visibilitySwitch {
  display: inline-flex;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid #d1d5db;
  background: #f1f5f9;
}

.visibilitySwitch.small { border-radius: 6px; }

.visSwitchBtn {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 7px 16px;
  font-size: 12px;
  font-weight: 700;
  border: none;
  cursor: pointer;
  background: transparent;
  color: #94a3b8;
  transition: all 0.15s ease;
  font-family: inherit;
  letter-spacing: 0.01em;
}

.visibilitySwitch.small .visSwitchBtn {
  padding: 5px 12px;
  font-size: 11px;
}

.visSwitchBtn.active {
  background: #004730;
  color: #ffffff;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
}

.visSwitchBtn.private.active {
  background: #92400e;
  color: #ffffff;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
}

.visSwitchBtn:not(.active):hover {
  background: rgba(0, 0, 0, 0.05);
  color: #475569;
}

.footerCancelBtn {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 6px 12px;
  border: 1px solid #fecaca;
  border-radius: 6px;
  background: transparent;
  color: #dc2626;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
  font-family: inherit;
}

.footerCancelBtn:hover {
  background: rgba(239, 68, 68, 0.06);
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 10px 16px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
  border: none;
  font-family: inherit;
}

.btn.ghost {
  background: transparent;
  border: 1px solid #e2e8f0;
  color: #64748b;
}

.btn.ghost:hover {
  background: #f1f5f9;
  color: #0f172a;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* All Agendas Mode Styles */
.allAgendasList {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.agendaSection {
  background: var(--card2);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 16px;
}

.agendaSectionHeader {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--border);
}

.agendaNumber {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
  color: #fff;
  font-size: 12px;
  font-weight: 700;
  border-radius: 50%;
  flex-shrink: 0;
}

.agendaSectionHeader h4 {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
  color: var(--text);
  flex: 1;
}

.commentCount {
  font-size: 12px;
  background: rgba(59, 130, 246, 0.15);
  color: #60a5fa;
  padding: 4px 10px;
  border-radius: 4px;
  font-weight: 500;
}

.noItemsMsg {
  text-align: center;
  padding: 16px;
  color: var(--muted2);
  font-size: 13px;
  font-style: italic;
}

/* ── Add comment button per agenda ── */
.agendaAddBtn {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid #f1f5f9;
}

.addCommentBtn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  font-size: 12px;
  font-weight: 600;
  background: linear-gradient(135deg, #004730 0%, #006d4b 100%);
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s ease;
  font-family: inherit;
}

.addCommentBtn:hover {
  box-shadow: 0 4px 12px rgba(0, 109, 75, 0.25);
  transform: translateY(-1px);
}

/* ── Inline add/edit form ── */
.inlineAddForm {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid #f1f5f9;
}

.inlineFormRow {
  display: flex;
  gap: 8px;
  align-items: center;
}

.inlineInput {
  flex: 1;
  min-width: 0;
  padding: 9px 12px;
  border: 1px solid #e2e8f0;
  background: #fff;
  border-radius: 8px;
  color: #0f172a;
  font-size: 13px;
  outline: none;
  font-family: inherit;
  transition: border-color 0.15s, box-shadow 0.15s;
}

.inlineInput:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.inlineInput::placeholder {
  color: #94a3b8;
}

.inlineSendBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  flex-shrink: 0;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  background: linear-gradient(135deg, #004730 0%, #006d4b 100%);
  color: #fff;
  transition: all 0.15s ease;
}

.inlineSendBtn:hover:not(:disabled) {
  box-shadow: 0 4px 12px rgba(0, 109, 75, 0.3);
}

.inlineSendBtn.editing {
  background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%);
}

.inlineSendBtn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.inlineFormFooter {
  display: flex;
  align-items: center;
  gap: 8px;
}


.inlineCancelBtn {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  padding: 4px 10px;
  border: 1px solid #fecaca;
  border-radius: 6px;
  background: transparent;
  color: #dc2626;
  font-size: 11px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
  font-family: inherit;
}

.inlineCancelBtn:hover {
  background: rgba(239, 68, 68, 0.06);
}

.commentsList.compact {
  gap: 8px;
}

.commentsList.compact .commentItem {
  padding: 10px 12px;
}

.modalFooter.simple {
  justify-content: flex-start;
}
</style>

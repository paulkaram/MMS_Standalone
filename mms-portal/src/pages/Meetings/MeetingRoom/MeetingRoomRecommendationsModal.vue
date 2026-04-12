<template>
  <Modal
    :model-value="show"
    :title="showAllAgendas ? $t('Recommendations') : $t('Recommendations')"
    :description="!showAllAgendas && agendaTitle ? agendaTitle : undefined"
    icon="mdi:lightbulb-outline"
    :size="showAllAgendas ? '3xl' : 'xl'"
    scrollable
    no-padding
    @update:model-value="(val: boolean) => { if (!val) $emit('close') }"
  >

        <!-- Body -->
        <div class="modalBody">
          <!-- ALL AGENDAS MODE -->
          <template v-if="showAllAgendas">
            <div v-if="totalAllRecommendations === 0" class="emptyState">
              <Icon icon="assignment" class="w-12 h-12" />
              <span>{{ $t('NoRecommendations') }}</span>
            </div>

            <div v-else class="allAgendasList">
              <div
                v-for="(agenda, index) in allAgendasData"
                :key="agenda.id"
                class="agendaSection"
              >
                <div class="agendaSectionHeader">
                  <span class="agendaNumber">{{ index + 1 }}</span>
                  <h4>{{ agenda.title || $t('AgendaItem') + ' ' + (index + 1) }}</h4>
                  <span class="recCount">{{ agenda.recommendations?.length || 0 }}</span>
                </div>

                <div v-if="!agenda.recommendations?.length" class="noItemsMsg">
                  {{ $t('NoRecommendations') }}
                </div>

                <div v-if="agenda.recommendations?.length" class="recommendationsList compact">
                  <div
                    v-for="rec in agenda.recommendations"
                    :key="rec.id"
                    class="recommendationItem"
                  >
                    <div class="recTop">
                      <div class="recInfo">
                        <span class="recText">{{ rec.text }}</span>
                        <div class="recMeta">
                          <span v-if="rec.ownerName" class="metaItem">
                            <Icon icon="person" class="w-3 h-3" />
                            {{ rec.ownerName }}
                          </span>
                          <span v-if="rec.dueDate" class="metaItem">
                            <Icon icon="calendar_today" class="w-3 h-3" />
                            {{ formatDate(rec.dueDate) }}
                          </span>
                          <span v-if="rec.priorityName" class="priorityBadge" :class="getPriorityClass(rec.priorityId)">
                            {{ rec.priorityName }}
                          </span>
                        </div>
                      </div>
                      <div class="recActions" v-if="(rec.owner == currentUserId || rec.createdBy == currentUserId) && !isReadOnly">
                        <button class="iconBtn" @click="$emit('edit-recommendation', rec)" :title="$t('Edit')">
                          <Icon icon="edit" class="w-3 h-3" />
                        </button>
                        <button class="iconBtn danger" @click="$emit('delete-recommendation', rec.id)" :title="$t('Delete')">
                          <Icon icon="delete" class="w-3 h-3" />
                        </button>
                      </div>
                    </div>
                    <div v-if="rec.description" class="recDescription">{{ rec.description }}</div>
                  </div>
                </div>

                <!-- Add button per agenda in all agendas mode -->
                <div v-if="canControl && !isReadOnly" class="agendaAddBtn">
                  <button class="btn small" @click="$emit('add-recommendation-for-agenda', agenda.id)">
                    <Icon icon="add" class="w-3 h-3" />
                    {{ $t('AddRecommendation') }}
                  </button>
                </div>
              </div>
            </div>
          </template>

          <!-- SINGLE AGENDA MODE -->
          <template v-else>
            <div v-if="recommendations.length === 0" class="emptyState">
              <Icon icon="assignment" class="w-12 h-12" />
              <span>{{ $t('NoRecommendations') }} بعد</span>
            </div>

            <div v-else class="recommendationsList">
              <div
                v-for="rec in recommendations"
                :key="rec.id"
                class="recommendationItem"
              >
                <div class="recTop">
                  <div class="recInfo">
                    <span class="recText">{{ rec.text }}</span>
                    <div class="recMeta">
                      <span v-if="rec.ownerName" class="metaItem">
                        <Icon icon="person" class="w-3 h-3" />
                        {{ rec.ownerName }}
                      </span>
                      <span v-if="rec.dueDate" class="metaItem">
                        <Icon icon="calendar_today" class="w-3 h-3" />
                        {{ formatDate(rec.dueDate) }}
                      </span>
                      <span v-if="rec.priorityName" class="priorityBadge" :class="getPriorityClass(rec.priorityId)">
                        {{ rec.priorityName }}
                      </span>
                    </div>
                  </div>
                  <div class="recActions" v-if="(rec.owner == currentUserId || rec.createdBy == currentUserId) && !isReadOnly">
                    <button class="iconBtn" @click="$emit('edit-recommendation', rec)" :title="$t('Edit')">
                      <Icon icon="edit" class="w-3 h-3" />
                    </button>
                    <button class="iconBtn danger" @click="$emit('delete-recommendation', rec.id)" :title="$t('Delete')">
                      <Icon icon="delete" class="w-3 h-3" />
                    </button>
                  </div>
                </div>
                <div v-if="rec.description" class="recDescription">{{ rec.description }}</div>
                <div v-if="rec.statusName" class="recStatus">
                  <span class="statusBadge" :class="getStatusClass(rec.statusId)">{{ rec.statusName }}</span>
                  <span v-if="rec.percentage !== undefined && rec.percentage !== null" class="progressInfo">
                    {{ rec.percentage }}%
                  </span>
                </div>
              </div>
            </div>
          </template>
        </div>

        <template #footer>
          <button class="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-50" @click="$emit('close')">{{ $t('Close') }}</button>
          <button v-if="canControl && !showAllAgendas && !isReadOnly" class="px-4 py-2 text-sm font-medium text-white rounded-lg flex items-center gap-2" style="background: linear-gradient(135deg, #006d4b 0%, #006d4b 100%)" @click="$emit('open-add-recommendation')">
            <Icon icon="add" class="w-3.5 h-3.5" />
            {{ $t('AddRecommendation') }}
          </button>
        </template>
  </Modal>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'

interface AgendaData {
  id: number
  title: string
  notes: any[]
  recommendations: any[]
}

const props = defineProps<{
  show: boolean
  recommendations: any[]
  agendaTitle: string
  canControl: boolean
  currentUserId: string
  showAllAgendas?: boolean
  allAgendasData?: AgendaData[]
  isReadOnly?: boolean
}>()

const totalAllRecommendations = computed(() => {
  if (!props.allAgendasData) return 0
  return props.allAgendasData.reduce((sum, agenda) => sum + (agenda.recommendations?.length || 0), 0)
})

defineEmits<{
  (e: 'close'): void
  (e: 'edit-recommendation', rec: any): void
  (e: 'delete-recommendation', recId: number): void
  (e: 'open-add-recommendation'): void
  (e: 'add-recommendation-for-agenda', agendaId: number): void
}>()

const formatDate = (dateStr?: string) => {
  if (!dateStr) return ''
  try {
    const date = new Date(dateStr)
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      calendar: 'gregory'
    })
  } catch {
    return dateStr
  }
}

const getPriorityClass = (priorityId?: number) => {
  if (!priorityId) return ''
  // Assuming: 1=High, 2=Medium, 3=Low
  switch (priorityId) {
    case 1: return 'high'
    case 2: return 'medium'
    case 3: return 'low'
    default: return ''
  }
}

const getStatusClass = (statusId?: number) => {
  if (!statusId) return ''
  // Assuming: 1=Draft, 2=InProgress, 3=Completed
  switch (statusId) {
    case 1: return 'draft'
    case 2: return 'inprogress'
    case 3: return 'completed'
    default: return ''
  }
}
</script>

<style scoped>
.recommendationsModalOverlay {
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

.recommendationsModal {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: 16px;
  width: 100%;
  max-width: 700px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.4);
  transition: max-width 0.3s ease;
}

.recommendationsModal.wide {
  max-width: 900px;
}

.agendaCount {
  font-size: 12px;
  background: rgba(255, 255, 255, 0.2);
  padding: 4px 10px;
  border-radius: 4px;
}

.allAgendasList {
  display: flex;
  flex-direction: column;
  gap: 20px;
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
  gap: 10px;
  margin-bottom: 12px;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--border);
}

.agendaNumber {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  color: #fff;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 600;
}

.agendaSectionHeader h4 {
  flex: 1;
  margin: 0;
  font-size: 14px;
  font-weight: 600;
  color: var(--text);
}

.recCount {
  font-size: 11px;
  color: var(--muted);
  background: var(--hover-bg);
  padding: 3px 8px;
  border-radius: 4px;
}

.noItemsMsg {
  font-size: 13px;
  color: var(--muted2);
  text-align: center;
  padding: 16px;
}

.agendaAddBtn {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px dashed var(--border);
  display: flex;
  justify-content: center;
}

.agendaAddBtn .btn.small {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 14px;
  font-size: 12px;
  background: rgba(245, 158, 11, 0.1);
  color: #fbbf24;
  border: 1px dashed rgba(245, 158, 11, 0.3);
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.agendaAddBtn .btn.small:hover {
  background: rgba(245, 158, 11, 0.2);
  border-style: solid;
}

.recommendationsList.compact .recommendationItem {
  padding: 10px 12px;
}

.recommendationsList.compact .recText {
  font-size: 13px;
  margin-bottom: 6px;
}

.modalHeader {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
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

.agendaName {
  font-size: 12px;
  background: rgba(255, 255, 255, 0.2);
  padding: 4px 10px;
  border-radius: 4px;
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
  max-height: 500px;
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

.recommendationsList {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.recommendationItem {
  background: var(--card2);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 14px;
}

.recTop {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.recInfo {
  flex: 1;
}

.recText {
  font-size: 14px;
  font-weight: 600;
  color: var(--text);
  line-height: 1.5;
  display: block;
  margin-bottom: 8px;
}

.recMeta {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 12px;
}

.metaItem {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 11px;
  color: var(--muted);
}

.priorityBadge {
  display: inline-flex;
  align-items: center;
  padding: 3px 9px;
  border-radius: 4px;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.02em;
}

.priorityBadge.high {
  background: #0f172a;
  color: #f87171;
}

.priorityBadge.medium {
  background: #0f172a;
  color: #fbbf24;
}

.priorityBadge.low {
  background: #0f172a;
  color: #4ade80;
}

.recActions {
  display: flex;
  align-items: center;
  gap: 6px;
}

.iconBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
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

.recDescription {
  font-size: 13px;
  color: var(--muted);
  line-height: 1.5;
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px solid var(--border);
}

.recStatus {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px solid var(--border);
}

.statusBadge {
  display: inline-flex;
  align-items: center;
  padding: 3px 10px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 500;
}

.statusBadge.draft {
  background: rgba(156, 163, 175, 0.15);
  color: #a1a1aa;
}

.statusBadge.inprogress {
  background: rgba(59, 130, 246, 0.15);
  color: #60a5fa;
}

.statusBadge.completed {
  background: rgba(74, 222, 128, 0.15);
  color: #4ade80;
}

.progressInfo {
  font-size: 12px;
  color: var(--muted);
}

.modalFooter {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  background: var(--card2);
  border-top: 1px solid var(--border);
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
}

.btn.ghost {
  background: transparent;
  border: 1px solid var(--border2);
  color: var(--muted);
}

.btn.ghost:hover {
  background: var(--hover-bg);
  color: var(--text);
}

.btn.primary {
  background: rgba(245, 158, 11, 0.15);
  color: #fbbf24;
  border: 1px solid rgba(245, 158, 11, 0.3);
}

.btn.primary:hover {
  background: rgba(245, 158, 11, 0.25);
}

@media (max-width: 640px) {
  .recTop {
    flex-direction: column;
  }

  .recActions {
    margin-top: 10px;
  }

  .modalFooter {
    flex-direction: column;
    gap: 10px;
  }

  .modalFooter .btn {
    width: 100%;
  }
}
</style>

<template>
  <Modal
    :model-value="show"
    :title="$t('VotingResultsTitle')"
    icon="mdi:chart-bar"
    size="3xl"
    scrollable
    no-padding
    @update:model-value="(val: boolean) => { if (!val) $emit('close') }"
  >

        <!-- Body -->
        <div class="modalBody">
          <!-- Loading State -->
          <div v-if="loading" class="loadingState">
            <Icon icon="progress_activity" class="w-8 h-8 spin" />
            <span>{{ $t('Loading') }}</span>
          </div>

          <!-- No Agendas with Voting State -->
          <div v-else-if="!allAgendasVotingData.length" class="emptyState">
            <Icon icon="how_to_vote" class="w-12 h-12" />
            <span>{{ $t('AgendasWithVoting') }}</span>
          </div>

          <!-- Results Content - All Agendas -->
          <template v-else>
            <!-- Overall Summary -->
            <div class="summaryCards">
              <div class="summaryCard">
                <span class="cardValue">{{ totalAllVotes }}</span>
                <span class="cardLabel">{{ $t('TotalVotes') }}</span>
              </div>
              <div class="summaryCard">
                <span class="cardValue">{{ allAgendasVotingData.length }}</span>
                <span class="cardLabel">{{ $t('AgendasWithVoting') }}</span>
              </div>
            </div>

            <!-- Each Agenda's Voting Results -->
            <div
              v-for="(agenda, agendaIndex) in allAgendasVotingData"
              :key="agenda.id"
              class="agendaVotingSection"
            >
              <div class="agendaSectionHeader">
                <span class="agendaNumber">{{ agendaIndex + 1 }}</span>
                <h4>{{ agenda.title || 'بند ' + (agendaIndex + 1) }}</h4>
                <span class="votesCount">{{ agenda.meetingUserVotes?.length || 0 }} {{ $t('Vote') }}</span>
              </div>

              <!-- No votes for this agenda -->
              <div v-if="!agenda.meetingUserVotes?.length" class="noVotesMsg">
                {{ $t('NoVotesForAgenda') }}
              </div>

              <!-- Chart for this agenda -->
              <template v-else>
                <div class="agendaChart">
                  <div
                    v-for="option in getAgendaVotingResults(agenda)"
                    :key="option.id"
                    class="barItem"
                  >
                    <div class="barLabel">{{ option.name }}</div>
                    <div class="barWrapper">
                      <div
                        class="barFill"
                        :style="{ width: getAgendaPercentage(agenda, option.count) + '%' }"
                      ></div>
                      <span class="barValue">{{ option.count }} ({{ getAgendaPercentage(agenda, option.count) }}%)</span>
                    </div>
                  </div>
                </div>

                <!-- Expandable votes table -->
                <details class="votesDetails">
                  <summary>{{ $t('ViewVotesDetails') }} ({{ agenda.meetingUserVotes?.length || 0 }})</summary>
                  <div class="tableWrapper">
                    <table class="votesTable">
                      <thead>
                        <tr>
                          <th>#</th>
                          <th>{{ $t('User') }}</th>
                          <th>{{ $t('Option') }}</th>
                          <th>{{ $t('Date') }}</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr v-for="(vote, index) in agenda.meetingUserVotes" :key="vote.id || index">
                          <td>{{ index + 1 }}</td>
                          <td>{{ vote.userName || vote.userFullName || vote.memberName || 'مستخدم' }}</td>
                          <td>
                            <span class="optionBadge">{{ vote.selectedOptionName || getOptionNameFromAgenda(agenda, vote.vottingOptionId || vote.votingOptionId) }}</span>
                          </td>
                          <td>{{ formatDate(vote.createdDate) }}</td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </details>
              </template>
            </div>
          </template>
        </div>

        <!-- Footer -->
        <template #footer>
          <button class="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-50" @click="$emit('close')">{{ $t('Close') }}</button>
          <div class="flex gap-2">
            <button class="px-4 py-2 text-sm font-medium text-white rounded-lg flex items-center gap-2 disabled:opacity-50" style="background:#006d4b" @click="exportAllToExcel" :disabled="!totalAllVotes || !!exporting">
              <Icon icon="table_chart" class="w-3.5 h-3.5" style="color:#006d4b" />
              {{ exporting === 'excel' ? $t('ExportingLabel') : $t('ExportExcel') }}
            </button>
            <button class="px-4 py-2 text-sm font-medium text-white rounded-lg flex items-center gap-2 disabled:opacity-50" style="background:linear-gradient(135deg,#006d4b 0%,#006d4b 100%)" @click="exportAllToPdf" :disabled="!totalAllVotes || !!exporting">
              <Icon icon="description" class="w-3.5 h-3.5" />
              {{ exporting === 'pdf' ? $t('ExportingLabel') : $t('ExportPDF') }}
            </button>
          </div>
        </template>
  </Modal>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Modal from '@/components/ui/Modal.vue'
import Icon from '@/components/ui/Icon.vue'
import { formatDateTime } from '@/helpers/dateFormat'
import * as XLSX from 'xlsx'

interface VotingOption {
  id: number
  nameAr?: string
  nameEn?: string
  name?: string
}

interface VoteRecord {
  id?: number
  userId?: string
  userName?: string
  userFullName?: string
  vottingOptionId?: number
  votingOptionId?: number
  selectedOptionName?: string
  createdDate?: string
}

interface AgendaVotingData {
  id: number
  title: string
  votingOptions: VotingOption[]
  meetingUserVotes: VoteRecord[]
}

const props = defineProps<{
  show: boolean
  agendaId: number
  agendaTitle: string
  votingOptions: VotingOption[]
  meetingUserVotes: VoteRecord[]
  allAgendasVotingData: AgendaVotingData[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
}>()

const loading = ref(false)
const exporting = ref<string | false>(false)

// Total votes across all agendas
const totalAllVotes = computed(() => {
  return props.allAgendasVotingData.reduce((sum, agenda) => {
    return sum + (agenda.meetingUserVotes?.length || 0)
  }, 0)
})

// Get voting results for a specific agenda
const getAgendaVotingResults = (agenda: AgendaVotingData) => {
  const results: { id: number; name: string; count: number }[] = []
  const votes = agenda.meetingUserVotes || []

  // Check if we have selectedOptionName data
  const hasSelectedOptionName = votes.some((v: any) => v.selectedOptionName)

  if (hasSelectedOptionName) {
    // Count by option name
    const optionCounts = new Map<string, number>()
    for (const vote of votes) {
      const optName = (vote as any).selectedOptionName || 'غير محدد'
      optionCounts.set(optName, (optionCounts.get(optName) || 0) + 1)
    }
    let idx = 0
    for (const [name, count] of optionCounts) {
      results.push({ id: idx++, name, count })
    }
  } else {
    // Count by option ID
    for (const option of (agenda.votingOptions || [])) {
      const count = votes.filter(v =>
        (v.vottingOptionId || v.votingOptionId) === option.id
      ).length

      results.push({
        id: option.id,
        name: option.nameAr || option.nameEn || option.name || `خيار ${option.id}`,
        count
      })
    }
  }

  return results.sort((a, b) => b.count - a.count)
}

// Get percentage for agenda votes
const getAgendaPercentage = (agenda: AgendaVotingData, count: number) => {
  const total = agenda.meetingUserVotes?.length || 0
  if (total === 0) return 0
  return Math.round((count / total) * 100)
}

// Get option name from agenda's voting options
const getOptionNameFromAgenda = (agenda: AgendaVotingData, optionId: number) => {
  const option = (agenda.votingOptions || []).find(o => o.id === optionId)
  return option?.nameAr || option?.nameEn || option?.name || `خيار ${optionId}`
}

const formatDate = (dateStr?: string) => {
  if (!dateStr) return '—'
  try {
    return formatDateTime(new Date(dateStr))
  } catch {
    return dateStr
  }
}

const exportAllToExcel = async () => {
  exporting.value = 'excel'
  try {
    // Create workbook
    const wb = XLSX.utils.book_new()

    // Overall summary sheet
    const overallSummary = props.allAgendasVotingData.map((agenda, index) => ({
      '#': index + 1,
      'البند': agenda.title || `بند ${index + 1}`,
      'عدد الأصوات': agenda.meetingUserVotes?.length || 0
    }))
    const summarySheet = XLSX.utils.json_to_sheet(overallSummary)
    XLSX.utils.book_append_sheet(wb, summarySheet, 'ملخص عام')

    // Sheet for each agenda
    props.allAgendasVotingData.forEach((agenda, index) => {
      const votes = agenda.meetingUserVotes || []
      const exportData = votes.map((vote: any, vIndex) => ({
        '#': vIndex + 1,
        'المستخدم': vote.userName || vote.userFullName || vote.memberName || 'مستخدم',
        'الخيار': vote.selectedOptionName || getOptionNameFromAgenda(agenda, vote.vottingOptionId || vote.votingOptionId || 0),
        'التاريخ': formatDate(vote.createdDate)
      }))

      if (exportData.length > 0) {
        const sheetName = `بند ${index + 1}`.substring(0, 31) // Excel sheet name max 31 chars
        const sheet = XLSX.utils.json_to_sheet(exportData)
        XLSX.utils.book_append_sheet(wb, sheet, sheetName)
      }
    })

    // Download file
    const fileName = `نتائج_التصويت_جميع_البنود_${new Date().toISOString().split('T')[0]}.xlsx`
    XLSX.writeFile(wb, fileName)
  } catch (error) {
    console.error('Failed to export to Excel:', error)
  } finally {
    exporting.value = false
  }
}

const exportAllToPdf = async () => {
  exporting.value = 'pdf'
  try {
    // Create printable HTML content for all agendas
    const agendasHtml = props.allAgendasVotingData.map((agenda, index) => {
      const votes = agenda.meetingUserVotes || []
      const results = getAgendaVotingResults(agenda)

      return `
        <div class="agenda-section">
          <h2>${index + 1}. ${agenda.title || 'بند ' + (index + 1)}</h2>
          <p><strong>عدد الأصوات:</strong> ${votes.length}</p>

          ${votes.length > 0 ? `
            <div class="bar-container">
              ${results.map(r => `
                <div class="bar-item">
                  <div class="bar-label">${r.name}</div>
                  <div class="bar-wrapper">
                    <div class="bar-fill" style="width: ${getAgendaPercentage(agenda, r.count)}%"></div>
                    <span class="bar-value">${r.count} (${getAgendaPercentage(agenda, r.count)}%)</span>
                  </div>
                </div>
              `).join('')}
            </div>

            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>المستخدم</th>
                  <th>الخيار</th>
                  <th>التاريخ</th>
                </tr>
              </thead>
              <tbody>
                ${votes.map((vote: any, vIndex) => `
                  <tr>
                    <td>${vIndex + 1}</td>
                    <td>${vote.userName || vote.userFullName || vote.memberName || 'مستخدم'}</td>
                    <td>${vote.selectedOptionName || getOptionNameFromAgenda(agenda, vote.vottingOptionId || vote.votingOptionId || 0)}</td>
                    <td>${formatDate(vote.createdDate)}</td>
                  </tr>
                `).join('')}
              </tbody>
            </table>
          ` : '<p class="no-votes">لا توجد أصوات لهذا البند</p>'}
        </div>
      `
    }).join('')

    const printContent = `
      <!DOCTYPE html>
      <html dir="rtl" lang="ar">
      <head>
        <meta charset="UTF-8">
        <title>نتائج التصويت - جميع البنود</title>
        <style>
          body { font-family: 'Segoe UI', Tahoma, Arial, sans-serif; padding: 20px; direction: rtl; }
          h1 { color: #059669; border-bottom: 2px solid #059669; padding-bottom: 10px; }
          h2 { color: #3f3f46; margin-top: 30px; border-bottom: 1px solid #e4e4e7; padding-bottom: 10px; }
          .summary { display: flex; gap: 20px; margin: 20px 0; }
          .summary-card { background: #f4f4f5; padding: 15px 25px; border-radius: 8px; text-align: center; }
          .summary-card .value { font-size: 24px; font-weight: bold; color: #059669; }
          .summary-card .label { font-size: 12px; color: #71717a; }
          table { width: 100%; border-collapse: collapse; margin-top: 15px; margin-bottom: 30px; }
          th, td { border: 1px solid #e4e4e7; padding: 10px; text-align: right; }
          th { background: #fafafa; font-weight: 600; }
          tr:nth-child(even) { background: #fafafa; }
          .bar-container { margin: 20px 0; }
          .bar-item { margin: 10px 0; }
          .bar-label { font-weight: 500; margin-bottom: 5px; }
          .bar-wrapper { background: #e4e4e7; height: 24px; border-radius: 4px; position: relative; }
          .bar-fill { background: #059669; height: 100%; border-radius: 4px; }
          .bar-value { position: absolute; right: 10px; top: 2px; font-size: 12px; }
          .agenda-section { page-break-inside: avoid; margin-bottom: 40px; }
          .no-votes { color: #a1a1aa; font-style: italic; }
          @media print { body { print-color-adjust: exact; -webkit-print-color-adjust: exact; } }
        </style>
      </head>
      <body>
        <h1>نتائج التصويت - جميع البنود</h1>
        <p><strong>تاريخ التقرير:</strong> ${new Date().toLocaleString('ar-EG')}</p>

        <div class="summary">
          <div class="summary-card">
            <div class="value">${totalAllVotes.value}</div>
            <div class="label">إجمالي الأصوات</div>
          </div>
          <div class="summary-card">
            <div class="value">${props.allAgendasVotingData.length}</div>
            <div class="label">بنود بها تصويت</div>
          </div>
        </div>

        ${agendasHtml}
      </body>
      </html>
    `

    // Open print dialog
    const printWindow = window.open('', '_blank')
    if (printWindow) {
      printWindow.document.write(printContent)
      printWindow.document.close()
      printWindow.focus()
      setTimeout(() => {
        printWindow.print()
        printWindow.close()
      }, 250)
    }
  } catch (error) {
    console.error('Failed to export to PDF:', error)
  } finally {
    exporting.value = false
  }
}
</script>

<style scoped>
.votingResultsOverlay {
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

.votingResultsModal {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: 16px;
  width: 100%;
  max-width: 800px;
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
  background: linear-gradient(135deg, #059669 0%, #047857 100%);
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

.loadingState,
.emptyState {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 60px 20px;
  color: var(--muted);
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.summaryCards {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
  margin-bottom: 24px;
}

.summaryCard {
  background: var(--card2);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 16px;
  text-align: center;
}

.cardValue {
  display: block;
  font-size: 24px;
  font-weight: 700;
  color: var(--text);
  margin-bottom: 4px;
}

.cardValue.highlight {
  color: #4ade80;
  font-size: 16px;
}

.cardLabel {
  font-size: 12px;
  color: var(--muted);
}

.chartSection,
.tableSection {
  background: var(--card2);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 16px;
  margin-bottom: 16px;
}

.chartSection h4,
.tableSection h4 {
  margin: 0 0 16px 0;
  font-size: 14px;
  font-weight: 600;
  color: var(--text);
}

.barChart {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.barItem {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.barLabel {
  font-size: 13px;
  font-weight: 500;
  color: var(--text);
}

.barWrapper {
  position: relative;
  height: 28px;
  background: var(--border);
  border-radius: 6px;
  overflow: hidden;
}

.barFill {
  height: 100%;
  background: linear-gradient(90deg, #059669, #4ade80);
  border-radius: 6px;
  transition: width 0.5s ease;
  min-width: 2px;
}

.barValue {
  position: absolute;
  inset-inline-end: 10px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 12px;
  font-weight: 600;
  color: #fff;
}

.tableWrapper {
  overflow-x: auto;
  max-height: 300px;
  overflow-y: auto;
  scrollbar-width: thin;
  scrollbar-color: var(--border2) transparent;
}

.tableWrapper::-webkit-scrollbar {
  width: 5px;
  height: 5px;
}

.tableWrapper::-webkit-scrollbar-track {
  background: transparent;
}

.tableWrapper::-webkit-scrollbar-thumb {
  background: var(--border2);
  border-radius: 3px;
}

.tableWrapper::-webkit-scrollbar-thumb:hover {
  background: var(--muted2);
}

.votesTable {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.votesTable th,
.votesTable td {
  padding: 10px 12px;
  text-align: start;
  border-bottom: 1px solid var(--border);
}

.votesTable th {
  background: var(--card3);
  font-weight: 600;
  color: var(--muted);
  position: sticky;
  top: 0;
}

.votesTable td {
  color: var(--text);
}

.votesTable tr:hover td {
  background: var(--hover-bg);
}

.optionBadge {
  display: inline-block;
  padding: 4px 10px;
  background: rgba(5, 150, 105, 0.15);
  color: #4ade80;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
}

.modalFooter {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  background: var(--card2);
  border-top: 1px solid var(--border);
}

.exportBtns {
  display: flex;
  gap: 8px;
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

.btn.export {
  background: rgba(5, 150, 105, 0.15);
  color: #4ade80;
  border: 1px solid rgba(5, 150, 105, 0.3);
}

.btn.export:hover:not(:disabled) {
  background: rgba(5, 150, 105, 0.25);
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Agenda Voting Section Styles */
.agendaVotingSection {
  background: var(--card2);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 16px;
  margin-bottom: 16px;
}

.agendaSectionHeader {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--border);
}

.agendaNumber {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  background: linear-gradient(135deg, #059669 0%, #047857 100%);
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

.votesCount {
  font-size: 12px;
  background: rgba(5, 150, 105, 0.15);
  color: #4ade80;
  padding: 4px 10px;
  border-radius: 4px;
  font-weight: 500;
}

.noVotesMsg {
  text-align: center;
  padding: 20px;
  color: var(--muted);
  font-size: 13px;
  font-style: italic;
}

.agendaChart {
  margin-bottom: 12px;
}

.votesDetails {
  margin-top: 12px;
}

.votesDetails summary {
  cursor: pointer;
  font-size: 13px;
  color: var(--muted);
  padding: 8px 12px;
  background: var(--card3);
  border-radius: 6px;
  user-select: none;
}

.votesDetails summary:hover {
  color: var(--text);
  background: var(--hover-bg);
}

.votesDetails[open] summary {
  margin-bottom: 12px;
  border-radius: 6px 6px 0 0;
}

@media (max-width: 640px) {
  .summaryCards {
    grid-template-columns: 1fr;
  }

  .modalFooter {
    flex-direction: column;
    gap: 12px;
  }

  .exportBtns {
    width: 100%;
  }

  .exportBtns .btn {
    flex: 1;
  }
}
</style>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="show" class="minutes-modal-overlay" @click.self="$emit('close')">
        <div class="minutes-modal" :class="{ 'light-theme': isLightTheme }">
          <!-- Modal Header -->
          <div class="modal-header">
            <div class="header-content">
              <div class="header-text">
                <h2>محضر الاجتماع</h2>
                <span class="version-badge" v-if="minutesData">
                  الإصدار {{ minutesData.versionLabel }}
                  <span class="status-dot" :class="minutesData.status"></span>
                </span>
              </div>
            </div>
            <div class="header-actions">
              <button class="action-btn" @click="toggleTheme" title="تغيير السمة">
                <Icon v-if="isLightTheme" icon="light_mode" class="w-4.5 h-4.5" />
                <Icon v-else icon="dark_mode" class="w-4.5 h-4.5" />
              </button>
              <button class="action-btn" @click="printMinutes" title="طباعة">
                <Icon icon="print" class="w-4.5 h-4.5" />
              </button>
              <button class="action-btn primary" @click="$emit('save')" :disabled="saving" title="حفظ PDF">
                <Icon v-if="saving" icon="progress_activity" class="w-4.5 h-4.5 spin" />
                <Icon v-else icon="download" class="w-4.5 h-4.5" />
                <span>حفظ</span>
              </button>
              <button class="close-btn" @click="$emit('close')">
                <Icon icon="close" class="w-5 h-5" />
              </button>
            </div>
          </div>

          <!-- Modal Body - Preview -->
          <div class="modal-body" ref="printArea">
            <div class="minutes-document" v-if="minutesData">
              <!-- Document Header -->
              <header class="document-header">
                <div class="header-top">
                  <div class="org-logo">
                    <div class="logo-placeholder">
                      <Icon icon="business" class="w-10 h-10" />
                    </div>
                  </div>
                  <div class="document-title">
                    <h1>محضر اجتماع</h1>
                    <p class="subtitle">Minutes of Meeting</p>
                  </div>
                  <div class="qr-code">
                    <div class="qr-placeholder">
                      <Icon icon="qr_code" class="w-16 h-16" />
                    </div>
                  </div>
                </div>
                <div class="header-divider">
                  <div class="divider-line"></div>
                  <div class="divider-ornament">
                    <Icon icon="star" class="w-3 h-3" />
                  </div>
                  <div class="divider-line"></div>
                </div>
              </header>

              <!-- Meeting Info Section -->
              <section class="section meeting-info">
                <div class="section-title">
                  <Icon icon="info" class="w-4.5 h-4.5" />
                  <span>معلومات الاجتماع</span>
                </div>
                <div class="info-grid">
                  <div class="info-card">
                    <div class="info-icon"><Icon icon="tag" class="w-4 h-4" /></div>
                    <div class="info-content">
                      <span class="label">رقم المحضر</span>
                      <span class="value">{{ minutesData.meetingNumber }}</span>
                    </div>
                  </div>
                  <div class="info-card">
                    <div class="info-icon"><Icon icon="calendar_today" class="w-4 h-4" /></div>
                    <div class="info-content">
                      <span class="label">التاريخ</span>
                      <span class="value">{{ minutesData.date }}</span>
                    </div>
                  </div>
                  <div class="info-card">
                    <div class="info-icon"><Icon icon="schedule" class="w-4 h-4" /></div>
                    <div class="info-content">
                      <span class="label">الوقت</span>
                      <span class="value">{{ minutesData.startTime }} - {{ minutesData.endTime }}</span>
                    </div>
                  </div>
                  <div class="info-card">
                    <div class="info-icon"><Icon icon="timer" class="w-4 h-4" /></div>
                    <div class="info-content">
                      <span class="label">المدة</span>
                      <span class="value">{{ minutesData.actualDuration }}</span>
                    </div>
                  </div>
                  <div class="info-card wide">
                    <div class="info-icon"><Icon icon="business" class="w-4 h-4" /></div>
                    <div class="info-content">
                      <span class="label">اللجنة / المجلس</span>
                      <span class="value">{{ minutesData.committeeName || minutesData.councilName || 'غير محدد' }}</span>
                    </div>
                  </div>
                  <div class="info-card wide">
                    <div class="info-icon"><Icon icon="location_on" class="w-4 h-4" /></div>
                    <div class="info-content">
                      <span class="label">المكان</span>
                      <span class="value">{{ minutesData.location }}</span>
                    </div>
                  </div>
                </div>
              </section>

              <!-- Attendees Section -->
              <section class="section attendees-section">
                <div class="section-title">
                  <Icon icon="group" class="w-4.5 h-4.5" />
                  <span>الحضور والغياب</span>
                  <span class="count-badge">{{ minutesData.presentCount }}/{{ minutesData.totalAttendees }}</span>
                </div>

                <div class="quorum-status" :class="{ met: minutesData.quorumMet }">
                  <Icon v-if="minutesData.quorumMet" icon="check_circle" class="w-4 h-4" />
                  <Icon v-else icon="error" class="w-4 h-4" />
                  <span>{{ minutesData.quorumMet ? 'النصاب مكتمل' : 'النصاب غير مكتمل' }}</span>
                </div>

                <div class="attendees-table">
                  <table>
                    <thead>
                      <tr>
                        <th class="col-num">#</th>
                        <th class="col-name">الاسم</th>
                        <th class="col-role">الصفة</th>
                        <th class="col-status">الحضور</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="(attendee, idx) in minutesData.attendees" :key="attendee.id" :class="{ absent: !attendee.attended }">
                        <td class="col-num">{{ idx + 1 }}</td>
                        <td class="col-name">
                          <div class="attendee-name">
                            <span class="name">{{ attendee.name }}</span>
                            <span class="title" v-if="attendee.title">{{ attendee.title }}</span>
                          </div>
                        </td>
                        <td class="col-role">
                          <span class="role-badge" :class="attendee.role">
                            {{ getRoleLabel(attendee.role) }}
                          </span>
                        </td>
                        <td class="col-status">
                          <span class="status-badge" :class="attendee.attended ? 'present' : 'absent'">
                            <Icon v-if="attendee.attended" icon="check" class="w-3.5 h-3.5" />
                            <Icon v-else icon="close" class="w-3.5 h-3.5" />
                            {{ attendee.attended ? 'حاضر' : 'غائب' }}
                          </span>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </section>

              <!-- Agenda Items Section -->
              <section class="section agenda-section">
                <div class="section-title">
                  <Icon icon="checklist" class="w-4.5 h-4.5" />
                  <span>بنود جدول الأعمال</span>
                </div>

                <div class="agenda-items">
                  <div
                    v-for="item in minutesData.agendaItems"
                    :key="item.id"
                    class="agenda-item"
                  >
                    <div class="agenda-header">
                      <div class="agenda-number">{{ item.index }}</div>
                      <div class="agenda-title-wrap">
                        <h3 class="agenda-title">{{ item.title }}</h3>
                        <p class="agenda-desc" v-if="item.description">{{ item.description }}</p>
                      </div>
                      <div class="agenda-duration" v-if="item.actualDuration">
                        <Icon icon="schedule" class="w-3.5 h-3.5" />
                        <span>{{ item.actualDuration }} دقيقة</span>
                      </div>
                    </div>

                    <!-- Summary -->
                    <div class="agenda-content" v-if="item.summary">
                      <div class="content-label">
                        <Icon icon="description" class="w-3.5 h-3.5" />
                        <span>الملخص</span>
                      </div>
                      <div class="content-text">{{ item.summary }}</div>
                    </div>

                    <!-- Discussion Notes -->
                    <div class="agenda-content" v-if="item.discussionNotes.length > 0">
                      <div class="content-label">
                        <Icon icon="chat" class="w-3.5 h-3.5" />
                        <span>التعليقات والملاحظات</span>
                      </div>
                      <ul class="notes-list">
                        <li v-for="note in item.discussionNotes" :key="note.id">
                          <span class="note-text">{{ note.text }}</span>
                          <span class="note-author">- {{ note.authorName }}</span>
                        </li>
                      </ul>
                    </div>

                    <!-- Voting Results -->
                    <div class="agenda-content voting-results" v-if="item.hasVoting && item.votingResults">
                      <div class="content-label">
                        <Icon icon="how_to_vote" class="w-3.5 h-3.5" />
                        <span>نتيجة التصويت</span>
                      </div>
                      <div class="voting-summary">
                        <div class="voting-outcome">
                          <span class="outcome-label">القرار:</span>
                          <span class="outcome-value">{{ item.votingResults.outcome }}</span>
                        </div>
                        <div class="voting-breakdown">
                          <div
                            v-for="option in item.votingResults.options"
                            :key="option.id"
                            class="vote-option"
                            :class="getVoteClass(option.nameAr)"
                          >
                            <div class="option-header">
                              <span class="option-name">{{ option.nameAr }}</span>
                              <span class="option-count">{{ option.voteCount }}</span>
                            </div>
                            <div class="option-bar">
                              <div class="bar-fill" :style="{ width: option.percentage + '%' }"></div>
                            </div>
                            <span class="option-percent">{{ option.percentage }}%</span>
                          </div>
                        </div>
                      </div>
                    </div>

                    <!-- Recommendations -->
                    <div class="agenda-content" v-if="item.recommendations.length > 0">
                      <div class="content-label">
                        <Icon icon="lightbulb" class="w-3.5 h-3.5" />
                        <span>التوصيات</span>
                      </div>
                      <ol class="recommendations-list">
                        <li v-for="rec in item.recommendations" :key="rec.id">
                          <span class="rec-text">{{ rec.text }}</span>
                          <div class="rec-meta" v-if="rec.ownerName || rec.dueDate">
                            <span v-if="rec.ownerName">
                              <Icon icon="person" class="w-3 h-3" /> {{ rec.ownerName }}
                            </span>
                            <span v-if="rec.dueDate">
                              <Icon icon="calendar_today" class="w-3 h-3" /> {{ formatDate(rec.dueDate) }}
                            </span>
                          </div>
                        </li>
                      </ol>
                    </div>
                  </div>
                </div>
              </section>

              <!-- Action Items Section -->
              <section class="section action-items-section" v-if="minutesData.actionItems.length > 0">
                <div class="section-title">
                  <Icon icon="assignment" class="w-4.5 h-4.5" />
                  <span>المهام والتكليفات</span>
                </div>

                <div class="action-items-table">
                  <table>
                    <thead>
                      <tr>
                        <th class="col-num">#</th>
                        <th class="col-task">المهمة</th>
                        <th class="col-assignee">المسؤول</th>
                        <th class="col-due">الموعد</th>
                        <th class="col-source">المصدر</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="(action, idx) in minutesData.actionItems" :key="action.id">
                        <td class="col-num">{{ idx + 1 }}</td>
                        <td class="col-task">{{ action.description }}</td>
                        <td class="col-assignee">{{ action.assignedTo }}</td>
                        <td class="col-due">{{ formatDate(action.dueDate) }}</td>
                        <td class="col-source">
                          <span class="source-badge">البند {{ getSourceAgendaIndex(action.sourceAgendaId) }}</span>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </section>

              <!-- Meeting Summary Section -->
              <section class="section summary-section" v-if="minutesData.meetingSummary">
                <div class="section-title">
                  <Icon icon="summarize" class="w-4.5 h-4.5" />
                  <span>ملخص الاجتماع</span>
                </div>
                <div class="summary-content">
                  {{ minutesData.meetingSummary }}
                </div>
              </section>

              <!-- Signatures Section -->
              <section class="section signatures-section">
                <div class="section-title">
                  <Icon icon="draw" class="w-4.5 h-4.5" />
                  <span>التوقيعات</span>
                </div>

                <div class="signatures-grid">
                  <div class="signature-box">
                    <div class="signature-role">رئيس الاجتماع</div>
                    <div class="signature-line"></div>
                    <div class="signature-name">{{ minutesData.chairmanName }}</div>
                    <div class="signature-date">التاريخ: ____________</div>
                  </div>
                  <div class="signature-box">
                    <div class="signature-role">أمين السر</div>
                    <div class="signature-line"></div>
                    <div class="signature-name">{{ minutesData.secretaryName }}</div>
                    <div class="signature-date">التاريخ: ____________</div>
                  </div>
                </div>
              </section>

              <!-- Document Footer -->
              <footer class="document-footer">
                <div class="footer-info">
                  <span>الإصدار: {{ minutesData.versionLabel }}</span>
                  <span class="separator">|</span>
                  <span>تاريخ الإصدار: {{ formatDateTime(minutesData.generatedAt) }}</span>
                  <span class="separator">|</span>
                  <span>أُنشئ بواسطة: {{ minutesData.generatedBy }}</span>
                </div>
                <div class="footer-watermark">
                  تم إنشاء هذا المحضر آلياً بواسطة نظام إدارة الاجتماعات
                </div>
              </footer>
            </div>

            <!-- Empty State -->
            <div class="empty-state" v-else>
              <Icon icon="cancel" class="w-12 h-12" />
              <h3>لا توجد بيانات</h3>
              <p>لم يتم إنشاء بيانات المحضر بعد</p>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import type { MinutesOfMeeting, AttendeeRole } from './types/minutes'

const props = defineProps<{
  show: boolean
  minutesData: MinutesOfMeeting | null
  saving?: boolean
}>()

defineEmits<{
  (e: 'close'): void
  (e: 'save'): void
  (e: 'download'): void
}>()

const isLightTheme = ref(true) // Default to light for printing
const printArea = ref<HTMLElement | null>(null)

function toggleTheme(): void {
  isLightTheme.value = !isLightTheme.value
}

function getRoleLabel(role: AttendeeRole): string {
  const labels: Record<AttendeeRole, string> = {
    chairman: 'رئيس الاجتماع',
    vice_chairman: 'نائب الرئيس',
    secretary: 'أمين السر',
    member: 'عضو',
    guest: 'ضيف',
    observer: 'مراقب'
  }
  return labels[role] || 'عضو'
}

function getVoteClass(optionName: string): string {
  const name = optionName.toLowerCase()
  if (name.includes('موافق') || name.includes('نعم')) return 'approve'
  if (name.includes('رفض') || name.includes('لا')) return 'reject'
  return 'abstain'
}

function getSourceAgendaIndex(agendaId?: number): number {
  if (!agendaId || !props.minutesData) return 0
  const item = props.minutesData.agendaItems.find(a => a.id === agendaId)
  return item?.index || 0
}

function formatDate(dateStr?: string): string {
  if (!dateStr) return '—'
  try {
    return new Date(dateStr).toLocaleDateString('ar-EG')
  } catch {
    return dateStr
  }
}

function formatDateTime(dateStr?: string): string {
  if (!dateStr) return '—'
  try {
    return new Date(dateStr).toLocaleString('ar-EG', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch {
    return dateStr
  }
}

function printMinutes(): void {
  window.print()
}
</script>

<style scoped>
/* ═══════════════════════════════════════════════════════════════════════════ */
/* MODAL OVERLAY & CONTAINER */
/* ═══════════════════════════════════════════════════════════════════════════ */

.minutes-modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 20px;
}

.minutes-modal {
  width: 100%;
  max-width: 900px;
  max-height: 95vh;
  background: #1a1d24;
  border-radius: 16px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 25px 80px rgba(0, 0, 0, 0.5);
}

.minutes-modal.light-theme {
  background: #ffffff;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* MODAL HEADER */
/* ═══════════════════════════════════════════════════════════════════════════ */

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(255, 255, 255, 0.02);
}

.light-theme .modal-header {
  border-bottom-color: rgba(0, 0, 0, 0.08);
  background: #f8fafc;
}

.header-content {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-text h2 {
  font-size: 18px;
  font-weight: 700;
  color: #f1f5f9;
  margin: 0;
}

.light-theme .header-text h2 {
  color: #0f172a;
}

.version-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: #94a3b8;
  margin-top: 2px;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #71717a;
}

.status-dot.draft { background: #f59e0b; }
.status-dot.approved { background: #22c55e; }
.status-dot.published { background: #3b82f6; }

.header-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  border-radius: 8px;
  border: none;
  background: rgba(255, 255, 255, 0.08);
  color: #e2e8f0;
  cursor: pointer;
  font-size: 13px;
  font-weight: 500;
  transition: all 0.15s ease;
}

.action-btn:hover {
  background: rgba(255, 255, 255, 0.15);
}

.action-btn.primary {
  background: linear-gradient(135deg, #00d97e 0%, #059669 100%);
  color: white;
}

.action-btn.primary:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 217, 126, 0.3);
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.light-theme .action-btn {
  background: rgba(15, 23, 42, 0.06);
  color: #475569;
}

.light-theme .action-btn:hover {
  background: rgba(15, 23, 42, 0.1);
}

.close-btn {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  border: none;
  background: transparent;
  color: #94a3b8;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s ease;
}

.close-btn:hover {
  background: rgba(239, 68, 68, 0.15);
  color: #ef4444;
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* MODAL BODY */
/* ═══════════════════════════════════════════════════════════════════════════ */

.modal-body {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
  background: #12141a;
}

.light-theme .modal-body {
  background: #f1f5f9;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* MINUTES DOCUMENT */
/* ═══════════════════════════════════════════════════════════════════════════ */

.minutes-document {
  background: white;
  border-radius: 12px;
  padding: 40px;
  color: #1e293b;
  font-family: 'Cairo', 'Segoe UI', sans-serif;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
}

/* Document Header */
.document-header {
  margin-bottom: 32px;
}

.header-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.org-logo, .qr-code {
  width: 80px;
}

.logo-placeholder, .qr-placeholder {
  width: 80px;
  height: 80px;
  border-radius: 12px;
  background: linear-gradient(135deg, #f1f5f9 0%, #e2e8f0 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #94a3b8;
}

.document-title {
  text-align: center;
  flex: 1;
}

.document-title h1 {
  font-size: 28px;
  font-weight: 800;
  color: #0f172a;
  margin: 0 0 4px 0;
  background: linear-gradient(135deg, #059669 0%, #0d9488 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.document-title .subtitle {
  font-size: 14px;
  color: #64748b;
  margin: 0;
  letter-spacing: 2px;
}

.header-divider {
  display: flex;
  align-items: center;
  gap: 12px;
}

.divider-line {
  flex: 1;
  height: 2px;
  background: linear-gradient(90deg, transparent, #059669, transparent);
}

.divider-ornament {
  color: #059669;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* SECTIONS */
/* ═══════════════════════════════════════════════════════════════════════════ */

.section {
  margin-bottom: 32px;
  page-break-inside: avoid;
}

.section-title {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 16px;
  font-weight: 700;
  color: #059669;
  margin-bottom: 16px;
  padding-bottom: 8px;
  border-bottom: 2px solid #e2e8f0;
}

.section-title .count-badge {
  margin-right: auto;
  padding: 2px 10px;
  border-radius: 12px;
  background: #059669;
  color: white;
  font-size: 12px;
  font-weight: 600;
}

/* Meeting Info Grid */
.info-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
}

.info-card {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  padding: 12px;
  background: #f8fafc;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
}

.info-card.wide {
  grid-column: span 2;
}

.info-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: linear-gradient(135deg, #059669 0%, #0d9488 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  flex-shrink: 0;
}

.info-content {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.info-content .label {
  font-size: 11px;
  color: #64748b;
  margin-bottom: 2px;
}

.info-content .value {
  font-size: 13px;
  font-weight: 600;
  color: #1e293b;
}

/* Quorum Status */
.quorum-status {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 14px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 600;
  margin-bottom: 16px;
  background: #fef2f2;
  color: #dc2626;
}

.quorum-status.met {
  background: #f0fdf4;
  color: #16a34a;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* TABLES */
/* ═══════════════════════════════════════════════════════════════════════════ */

.attendees-table table,
.action-items-table table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.attendees-table th,
.action-items-table th {
  background: #f1f5f9;
  padding: 12px;
  text-align: right;
  font-weight: 700;
  color: #475569;
  border-bottom: 2px solid #e2e8f0;
}

.attendees-table td,
.action-items-table td {
  padding: 12px;
  border-bottom: 1px solid #e2e8f0;
  vertical-align: middle;
}

.attendees-table tr.absent td {
  background: #fef2f2;
}

.col-num { width: 40px; text-align: center; }
.col-status { width: 100px; }
.col-role { width: 120px; }
.col-assignee { width: 150px; }
.col-due { width: 100px; }
.col-source { width: 80px; }

.attendee-name {
  display: flex;
  flex-direction: column;
}

.attendee-name .name {
  font-weight: 600;
  color: #1e293b;
}

.attendee-name .title {
  font-size: 11px;
  color: #64748b;
}

.role-badge {
  display: inline-flex;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
  background: #f1f5f9;
  color: #475569;
}

.role-badge.chairman {
  background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
  color: #92400e;
}

.role-badge.secretary {
  background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%);
  color: #1e40af;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
}

.status-badge.present {
  background: #dcfce7;
  color: #16a34a;
}

.status-badge.absent {
  background: #fee2e2;
  color: #dc2626;
}

.source-badge {
  font-size: 11px;
  color: #64748b;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* AGENDA ITEMS */
/* ═══════════════════════════════════════════════════════════════════════════ */

.agenda-items {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.agenda-item {
  background: #f8fafc;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

.agenda-header {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  padding: 16px;
  background: white;
  border-bottom: 1px solid #e2e8f0;
}

.agenda-number {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  background: linear-gradient(135deg, #059669 0%, #0d9488 100%);
  color: white;
  font-size: 16px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.agenda-title-wrap {
  flex: 1;
  min-width: 0;
}

.agenda-title {
  font-size: 15px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 4px 0;
}

.agenda-desc {
  font-size: 13px;
  color: #64748b;
  margin: 0;
}

.agenda-duration {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: #64748b;
  background: #f1f5f9;
  padding: 4px 10px;
  border-radius: 16px;
}

.agenda-content {
  padding: 16px;
  border-top: 1px solid #e2e8f0;
}

.content-label {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 600;
  color: #475569;
  margin-bottom: 10px;
}

.content-text {
  font-size: 14px;
  line-height: 1.7;
  color: #334155;
}

.notes-list, .recommendations-list {
  margin: 0;
  padding-right: 20px;
}

.notes-list li, .recommendations-list li {
  margin-bottom: 8px;
  line-height: 1.6;
}

.note-text, .rec-text {
  color: #334155;
}

.note-author {
  color: #94a3b8;
  font-size: 12px;
}

.rec-meta {
  display: flex;
  gap: 16px;
  margin-top: 4px;
  font-size: 12px;
  color: #64748b;
}

.rec-meta span {
  display: flex;
  align-items: center;
  gap: 4px;
}

/* Voting Results */
.voting-results .voting-summary {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.voting-outcome {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 16px;
  background: #f0fdf4;
  border-radius: 10px;
  border: 1px solid #bbf7d0;
}

.outcome-label {
  font-weight: 600;
  color: #166534;
}

.outcome-value {
  font-weight: 700;
  color: #15803d;
  font-size: 15px;
}

.voting-breakdown {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 12px;
}

.vote-option {
  padding: 12px;
  border-radius: 10px;
  background: white;
  border: 1px solid #e2e8f0;
}

.vote-option.approve {
  border-color: #bbf7d0;
  background: #f0fdf4;
}

.vote-option.reject {
  border-color: #fecaca;
  background: #fef2f2;
}

.vote-option.abstain {
  border-color: #e4e4e7;
  background: #fafafa;
}

.option-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
}

.option-name {
  font-size: 13px;
  font-weight: 600;
  color: #3f3f46;
}

.option-count {
  font-size: 18px;
  font-weight: 700;
  color: #059669;
}

.option-bar {
  height: 6px;
  background: #e4e4e7;
  border-radius: 3px;
  overflow: hidden;
  margin-bottom: 4px;
}

.bar-fill {
  height: 100%;
  background: linear-gradient(90deg, #059669, #0d9488);
  border-radius: 3px;
  transition: width 0.3s ease;
}

.vote-option.reject .bar-fill {
  background: linear-gradient(90deg, #dc2626, #ef4444);
}

.option-percent {
  font-size: 11px;
  color: #71717a;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* SUMMARY & SIGNATURES */
/* ═══════════════════════════════════════════════════════════════════════════ */

.summary-content {
  font-size: 14px;
  line-height: 1.8;
  color: #334155;
  padding: 16px;
  background: #f8fafc;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
}

.signatures-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 40px;
  margin-top: 40px;
}

.signature-box {
  text-align: center;
}

.signature-role {
  font-size: 14px;
  font-weight: 700;
  color: #475569;
  margin-bottom: 40px;
}

.signature-line {
  height: 1px;
  background: #1e293b;
  margin-bottom: 8px;
}

.signature-name {
  font-size: 14px;
  font-weight: 600;
  color: #0f172a;
  margin-bottom: 4px;
}

.signature-date {
  font-size: 12px;
  color: #64748b;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* DOCUMENT FOOTER */
/* ═══════════════════════════════════════════════════════════════════════════ */

.document-footer {
  margin-top: 40px;
  padding-top: 20px;
  border-top: 2px solid #e2e8f0;
  text-align: center;
}

.footer-info {
  font-size: 11px;
  color: #64748b;
  margin-bottom: 8px;
}

.footer-info .separator {
  margin: 0 8px;
}

.footer-watermark {
  font-size: 10px;
  color: #94a3b8;
  font-style: italic;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* EMPTY STATE */
/* ═══════════════════════════════════════════════════════════════════════════ */

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  color: #64748b;
}

.empty-state h3 {
  margin: 16px 0 8px;
  font-size: 18px;
  color: #94a3b8;
}

.empty-state p {
  margin: 0;
  font-size: 14px;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* TRANSITIONS */
/* ═══════════════════════════════════════════════════════════════════════════ */

.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s ease;
}

.modal-enter-active .minutes-modal,
.modal-leave-active .minutes-modal {
  transition: all 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .minutes-modal,
.modal-leave-to .minutes-modal {
  transform: scale(0.95) translateY(20px);
  opacity: 0;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* PRINT STYLES */
/* ═══════════════════════════════════════════════════════════════════════════ */

@media print {
  .minutes-modal-overlay {
    position: static;
    background: none;
    padding: 0;
  }

  .minutes-modal {
    max-height: none;
    box-shadow: none;
    border-radius: 0;
  }

  .modal-header {
    display: none;
  }

  .modal-body {
    padding: 0;
    overflow: visible;
  }

  .minutes-document {
    box-shadow: none;
    border-radius: 0;
    padding: 20mm;
  }

  .section {
    page-break-inside: avoid;
  }

  .agenda-item {
    page-break-inside: avoid;
  }

  .signatures-section {
    page-break-before: always;
  }
}
</style>

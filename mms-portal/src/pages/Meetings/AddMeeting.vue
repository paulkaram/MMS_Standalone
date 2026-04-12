<template>
  <div class="add-meeting-page">
    <!-- Success Screen -->
    <Transition name="fade-scale">
      <div v-if="isPosted" class="success-overlay">
        <div class="success-card">
          <!-- Decorative Elements -->
          <div class="success-decoration">
            <div class="decoration-circle circle-1"></div>
            <div class="decoration-circle circle-2"></div>
            <div class="decoration-circle circle-3"></div>
          </div>

          <!-- Success Icon -->
          <div class="success-icon-container">
            <div class="success-icon-bg">
              <svg class="checkmark" viewBox="0 0 52 52">
                <circle class="checkmark-circle" cx="26" cy="26" r="25" fill="none"/>
                <path class="checkmark-check" fill="none" d="M14.1 27.2l7.1 7.2 16.7-16.8"/>
              </svg>
            </div>
          </div>

          <!-- Content -->
          <div class="success-content">
            <h2 class="success-title">{{ $t('MeetingSubmitted') }}</h2>
            <p class="success-subtitle">{{ $t('MeetingSubmittedDescription') }}</p>
          </div>

          <!-- Meeting Summary -->
          <div class="success-summary">
            <div class="summary-item">
              <div class="summary-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/><line x1="10" y1="9" x2="8" y2="9"/></svg>
              </div>
              <div class="summary-details">
                <span class="summary-label">{{ $t('Subject') }}</span>
                <span class="summary-value">{{ meeting.title || '-' }}</span>
              </div>
            </div>
            <div class="summary-divider"></div>
            <div class="summary-item">
              <div class="summary-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
              </div>
              <div class="summary-details">
                <span class="summary-label">{{ $t('Date') }}</span>
                <span class="summary-value">{{ formatDate(meeting.date) }}</span>
              </div>
            </div>
            <div class="summary-divider"></div>
            <div class="summary-item">
              <div class="summary-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
              </div>
              <div class="summary-details">
                <span class="summary-label">{{ $t('Time') }}</span>
                <span class="summary-value">{{ meeting.startTime }} - {{ meeting.endTime }}</span>
              </div>
            </div>
            <div class="summary-divider"></div>
            <div class="summary-item">
              <div class="summary-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
              </div>
              <div class="summary-details">
                <span class="summary-label">{{ $t('Attendees') }}</span>
                <span class="summary-value">{{ meeting.meetingAttendees?.length || 0 }} {{ $t('Members') }}</span>
              </div>
            </div>
          </div>

          <!-- Actions -->
          <div class="success-actions">
            <button type="button" class="action-btn secondary" @click="$router.push('/meetings')">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="8" y1="6" x2="21" y2="6"/><line x1="8" y1="12" x2="21" y2="12"/><line x1="8" y1="18" x2="21" y2="18"/><line x1="3" y1="6" x2="3.01" y2="6"/><line x1="3" y1="12" x2="3.01" y2="12"/><line x1="3" y1="18" x2="3.01" y2="18"/></svg>
              <span>{{ $t('ViewMeetings') }}</span>
            </button>
            <button type="button" class="action-btn primary" @click="createAnother">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
              <span>{{ $t('CreateAnother') }}</span>
            </button>
          </div>

          <!-- Footer Note -->
          <p class="success-footer">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>
            {{ $t('AttendeesWillBeNotified') }}
          </p>
        </div>
      </div>
    </Transition>

    <!-- Meeting Form -->
    <template v-if="!isPosted">
      <!-- Main Content -->
      <div class="main-content">
        <!-- Sidebar Steps -->
        <div class="steps-sidebar">
          <!-- Progress Header -->
          <div class="sidebar-progress">
            <div class="progress-header">
              <span class="progress-label">{{ $t('Progress') }}</span>
              <span class="progress-value">{{ progressPercentage }}%</span>
            </div>
            <div class="progress-track">
              <div class="progress-fill" :style="{ width: progressPercentage + '%' }"></div>
            </div>
          </div>

          <div class="steps-list">
            <button
              v-for="(step, index) in steps"
              :key="step.key"
              type="button"
              :class="['step-item', {
                'active': currentStep === index,
                'completed': completedSteps.includes(index),
                'disabled': index > currentStep && !completedSteps.includes(index - 1)
              }]"
              :disabled="index > currentStep && !completedSteps.includes(index - 1)"
              @click="goToStep(index)"
            >
              <div class="step-indicator">
                <div class="step-icon-wrapper">
                  <Icon v-if="completedSteps.includes(index) && currentStep !== index" icon="mdi:check" class="step-check" />
                  <Icon v-else :icon="step.icon" class="step-icon" />
                </div>
                <div v-if="index < steps.length - 1" class="step-connector">
                  <div class="connector-line" :class="{ filled: completedSteps.includes(index) }"></div>
                </div>
              </div>
              <div class="step-content">
                <span class="step-num">{{ $t('Step') }} {{ index + 1 }}</span>
                <span class="step-title">{{ step.title }}</span>
                <span class="step-desc">{{ step.description }}</span>
              </div>
            </button>
          </div>

          <!-- Quick Actions -->
          <div class="quick-actions">
            <button type="button" class="save-draft-btn" @click="saveDraft" :disabled="saving">
              <Icon v-if="saving" icon="mdi:loading" class="w-5 h-5 animate-spin" />
              <Icon v-else icon="mdi:content-save" class="w-5 h-5" />
              <span>{{ $t('SaveDraft') }}</span>
            </button>
          </div>
        </div>

        <!-- Form Content -->
        <div class="form-content">
          <!-- Loading -->
          <div v-if="loading" class="loading-state">
            <div class="loader">
              <div class="loader-ring"></div>
              <Icon icon="mdi:calendar-clock" class="loader-icon" />
            </div>
            <p>{{ $t('LoadingMeeting') }}</p>
          </div>

          <!-- Step Content -->
          <Transition name="slide-fade" mode="out-in">
            <div v-if="!loading" :key="currentStep" class="step-panel">
              <!-- Step Header -->
              <div class="panel-header">
                <div class="panel-icon" :style="{ background: steps[currentStep].color }">
                  <Icon :icon="steps[currentStep].icon" class="w-6 h-6 text-white" />
                </div>
                <div class="panel-info">
                  <h2 class="panel-title">{{ steps[currentStep].title }}</h2>
                  <p class="panel-desc">{{ steps[currentStep].description }}</p>
                </div>
              </div>

              <!-- Step 1: Meeting Info -->
              <div v-show="currentStep === 0" class="panel-body">
                <MeetingMetaData
                  ref="metaDataRef"
                  v-model="meeting"
                  :view-mode="viewMode"
                />
              </div>

              <!-- Step 2: Attendees -->
              <div v-show="currentStep === 1" class="panel-body">
                <MeetingAttendees
                  ref="attendeesRef"
                  v-model="meeting.meetingAttendees"
                  :meeting-id="meeting.id"
                  :view-mode="viewMode"
                />
              </div>

              <!-- Step 3: Agenda -->
              <div v-show="currentStep === 2" class="panel-body">
                <MeetingAgenda
                  ref="agendaRef"
                  v-model="meeting.meetingAgendas"
                  :meeting-id="meeting.id"
                  :is-committee="meeting.isCommittee"
                  :committee-id="meeting.committeeId"
                  :view-mode="viewMode"
                />
              </div>

              <!-- Step 4: Attachments -->
              <div v-show="currentStep === 3" class="panel-body">
                <MeetingAttachments
                  v-model="meeting.attachments"
                  :meeting-id="meeting.id"
                  :meeting-agendas="meeting.meetingAgendas"
                  :view-mode="viewMode"
                />
              </div>

              <!-- Step 5: Review & Submit -->
              <div v-show="currentStep === 4" class="panel-body">
                <div class="review-section">
                  <!-- Summary Cards -->
                  <div class="summary-grid">
                    <div class="summary-card" @click="goToStep(0)">
                      <div class="summary-icon icon-teal">
                        <Icon icon="mdi:calendar-check" class="w-5 h-5" />
                      </div>
                      <div class="summary-content">
                        <span class="summary-label">{{ $t('MeetingInfo') }}</span>
                        <span class="summary-value">{{ meeting.title || '-' }}</span>
                        <span class="summary-meta">{{ formatDate(meeting.date) }} &middot; {{ meeting.startTime }} - {{ meeting.endTime }}</span>
                      </div>
                      <Icon icon="mdi:chevron-right" class="summary-arrow" />
                    </div>

                    <div class="summary-card" @click="goToStep(1)">
                      <div class="summary-icon icon-navy">
                        <Icon icon="mdi:account-group" class="w-5 h-5" />
                      </div>
                      <div class="summary-content">
                        <span class="summary-label">{{ $t('Attendees') }}</span>
                        <span class="summary-value">{{ meeting.meetingAttendees?.length || 0 }} {{ $t('Members') }}</span>
                      </div>
                      <Icon icon="mdi:chevron-right" class="summary-arrow" />
                    </div>

                    <div class="summary-card" @click="goToStep(2)">
                      <div class="summary-icon icon-teal">
                        <Icon icon="mdi:format-list-checks" class="w-5 h-5" />
                      </div>
                      <div class="summary-content">
                        <span class="summary-label">{{ $t('Agenda') }}</span>
                        <span class="summary-value">{{ meeting.meetingAgendas?.length || 0 }} {{ $t('Items') }}</span>
                      </div>
                      <Icon icon="mdi:chevron-right" class="summary-arrow" />
                    </div>

                    <div class="summary-card" @click="goToStep(3)">
                      <div class="summary-icon icon-navy">
                        <Icon icon="mdi:paperclip" class="w-5 h-5" />
                      </div>
                      <div class="summary-content">
                        <span class="summary-label">{{ $t('Attachments') }}</span>
                        <span class="summary-value">{{ meeting.attachments?.length || 0 }} {{ $t('Files') }}</span>
                      </div>
                      <Icon icon="mdi:chevron-right" class="summary-arrow" />
                    </div>
                  </div>

                  <!-- Associated Meetings -->
                  <div class="associated-section">
                    <h3 class="section-title">
                      <Icon icon="mdi:link-variant" class="w-5 h-5" />
                      {{ $t('AssociatedMeetings') }}
                    </h3>
                    <AssociatedMeetings
                      v-model="meeting.associatedMeetings"
                      :meeting-id="meeting.id"
                      :view-mode="viewMode"
                    />
                  </div>
                </div>
              </div>

              <!-- Navigation Footer (inside card) -->
              <div class="panel-footer">
                <!-- Validation Error Message -->
                <Transition name="fade">
                  <div v-if="validationError" class="validation-error">
                    <Icon icon="mdi:alert-circle" class="w-5 h-5 flex-shrink-0" />
                    <span>{{ validationError }}</span>
                  </div>
                </Transition>

                <div class="nav-actions">
                  <div class="nav-start">
                    <Button
                      v-if="currentStep > 0"
                      variant="outline"
                      @click="previousStep"
                    >
                      <Icon :icon="isRTL ? 'mdi:arrow-right' : 'mdi:arrow-left'" class="w-4 h-4" />
                      {{ $t('Previous') }}
                    </Button>
                  </div>

                  <div class="nav-end">
                    <Button
                      v-if="currentStep < steps.length - 1"
                      variant="primary"
                      :loading="saving"
                      @click="nextStep"
                    >
                      {{ $t('SaveAndContinue') }}
                      <Icon :icon="isRTL ? 'mdi:arrow-left' : 'mdi:arrow-right'" class="w-4 h-4" />
                    </Button>

                    <template v-if="currentStep === steps.length - 1 && !viewMode">
                      <button
                        type="button"
                        class="send-btn"
                        :disabled="sending"
                        @click="showSendConfirm = true"
                      >
                        <Icon v-if="sending" icon="mdi:loading" class="w-4 h-4 animate-spin" />
                        <Icon v-else icon="mdi:send" class="w-4 h-4" />
                        <span>{{ $t('Send') }}</span>
                      </button>
                      <button
                        type="button"
                        class="approve-btn"
                        :disabled="approving"
                        @click="showApproveConfirm = true"
                      >
                        <Icon v-if="approving" icon="mdi:loading" class="w-4 h-4 animate-spin" />
                        <Icon v-else icon="mdi:stamp" class="w-4 h-4" />
                        <span>{{ $t('MeetingDirectApproval') }}</span>
                      </button>
                    </template>
                  </div>
                </div>
              </div>
            </div>
          </Transition>
        </div>
      </div>
    </template>

    <!-- Send Confirmation Modal -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showSendConfirm" class="modal-overlay" @click.self="showSendConfirm = false">
          <div class="modal-container">
            <div class="modal-icon send">
              <Icon icon="mdi:send-circle" class="w-12 h-12" />
            </div>
            <h3 class="modal-title">{{ $t('ConfirmSend') }}</h3>
            <p class="modal-text">
              {{ $t('ConfirmSendDescription') }}
            </p>
            <div class="modal-info">
              <div class="info-item">
                <Icon icon="mdi:calendar" class="w-4 h-4" />
                <span>{{ meeting.title || t('Untitled') }}</span>
              </div>
              <div class="info-item">
                <Icon icon="mdi:account-group" class="w-4 h-4" />
                <span>{{ meeting.meetingAttendees?.length || 0 }} {{ $t('Attendees') }}</span>
              </div>
            </div>
            <div class="modal-actions">
              <button type="button" class="modal-btn cancel" @click="showSendConfirm = false">
                <Icon icon="mdi:close" class="w-4 h-4" />
                {{ $t('Cancel') }}
              </button>
              <button type="button" class="modal-btn confirm send" :disabled="sending" @click="confirmSendMeeting">
                <Icon v-if="sending" icon="mdi:loading" class="w-4 h-4 animate-spin" />
                <Icon v-else icon="mdi:send" class="w-4 h-4" />
                {{ $t('ConfirmSendBtn') }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Direct Approve Confirmation Modal -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showApproveConfirm" class="modal-overlay" @click.self="showApproveConfirm = false">
          <div class="modal-container">
            <div class="modal-icon approve">
              <Icon icon="mdi:stamp" class="w-12 h-12" />
            </div>
            <h3 class="modal-title">{{ $t('ConfirmDirectApproval') }}</h3>
            <p class="modal-text">
              {{ $t('ConfirmDirectApprovalDescription') }}
            </p>
            <div class="modal-warning">
              <Icon icon="mdi:alert-circle" class="w-5 h-5" />
              <span>{{ $t('DirectApprovalWarning') }}</span>
            </div>
            <div class="modal-info">
              <div class="info-item">
                <Icon icon="mdi:calendar" class="w-4 h-4" />
                <span>{{ meeting.title || t('Untitled') }}</span>
              </div>
              <div class="info-item">
                <Icon icon="mdi:clock-outline" class="w-4 h-4" />
                <span>{{ formatDate(meeting.date) }} - {{ meeting.startTime }}</span>
              </div>
            </div>
            <div class="modal-actions">
              <button type="button" class="modal-btn cancel" @click="showApproveConfirm = false">
                <Icon icon="mdi:close" class="w-4 h-4" />
                {{ $t('Cancel') }}
              </button>
              <button type="button" class="modal-btn confirm approve" :disabled="approving" @click="confirmApproveMeeting">
                <Icon v-if="approving" icon="mdi:loading" class="w-4 h-4 animate-spin" />
                <Icon v-else icon="mdi:check-decagram" class="w-4 h-4" />
                {{ $t('ConfirmApproveBtn') }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import MeetingMetaData from '@/components/app/meeting/MeetingMetaData.vue'
import MeetingAttendees from '@/components/app/meeting/MeetingAttendees.vue'
import MeetingAgenda from '@/components/app/meeting/MeetingAgenda.vue'
import MeetingAttachments from '@/components/app/meeting/MeetingAttachments.vue'
import AssociatedMeetings from '@/components/app/meeting/AssociatedMeetings.vue'
import MeetingsService from '@/services/MeetingsService'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'
import LookupsService from '@/services/LookupsService'

const { locale } = useI18n()
const isRTL = computed(() => locale.value === 'ar')
const route = useRoute()
const router = useRouter()

// Route params
const meetingId = computed(() => route.params.id as string | undefined)
const isEdit = computed(() => !!meetingId.value)
const viewMode = computed(() => route.query.viewMode === 'true' || meeting.readOnly)

// Refs
const metaDataRef = ref<InstanceType<typeof MeetingMetaData> | null>(null)
const attendeesRef = ref<InstanceType<typeof MeetingAttendees> | null>(null)
const agendaRef = ref<InstanceType<typeof MeetingAgenda> | null>(null)

// State
const loading = ref(false)
const saving = ref(false)
const sending = ref(false)
const approving = ref(false)
const isPosted = ref(false)
const showSendConfirm = ref(false)
const showApproveConfirm = ref(false)
const currentStep = ref(0)
const completedSteps = ref<number[]>([])
const originalMeeting = ref<string>('')
const validationError = ref<string | null>(null)
const skipRouteWatch = ref(false)

const { t } = useI18n()

const steps = computed(() => [
  {
    key: 'data',
    title: t('MeetingInfo'),
    description: t('MeetingInfoDescription'),
    icon: 'mdi:file-document-edit-outline',
    color: 'linear-gradient(135deg, #006d4b 0%, #004730 100%)'
  },
  {
    key: 'attendees',
    title: t('Attendees'),
    description: t('AttendeesDescription'),
    icon: 'mdi:account-group-outline',
    color: 'linear-gradient(135deg, #3B82F6 0%, #2563EB 100%)'
  },
  {
    key: 'agenda',
    title: t('Agenda'),
    description: t('AgendaDescription'),
    icon: 'mdi:format-list-checks',
    color: 'linear-gradient(135deg, #8B5CF6 0%, #7C3AED 100%)'
  },
  {
    key: 'attachments',
    title: t('Attachments'),
    description: t('AttachmentsDescription'),
    icon: 'mdi:paperclip',
    color: 'linear-gradient(135deg, #F59E0B 0%, #D97706 100%)'
  },
  {
    key: 'review',
    title: t('ReviewAndSubmit'),
    description: t('ReviewAndSubmitDescription'),
    icon: 'mdi:send-check',
    color: 'linear-gradient(135deg, #198b5f 0%, #004730 100%)'
  }
])

const meeting = reactive({
  id: undefined as string | undefined,
  title: null as string | null,
  isCommittee: false,
  committeeId: null as string | null,
  committeeName: null as string | null,
  committeeTypeId: null as number | null,
  date: null as string | null,
  startTime: '11:00',
  endTime: '11:30',
  location: null as string | null,
  notes: null as string | null,
  typeId: null as number | null,
  url: null as string | null,
  councilSessionId: null as string | null,
  referenceNumber: null as string | null,
  meetingAttendees: [] as any[],
  meetingAgendas: [] as any[],
  attachments: [] as any[],
  associatedMeetings: [] as any[],
  readOnly: false
})

// Computed
const progressPercentage = computed(() => {
  const totalSteps = steps.value.length
  const completed = completedSteps.value.length
  return Math.round((completed / totalSteps) * 100)
})

// Resolve committee role name from roleId using lookup
const resolveRoleName = (member: any, rolesMap: Map<string, string>): string | null => {
  // Try roleName directly from API
  if (member.roleName) return member.roleName
  // Resolve from committeeRoleId or roleId via lookup
  const roleId = String(member.committeeRoleId || member.roleId || '')
  if (roleId && rolesMap.has(roleId)) return rolesMap.get(roleId)!
  // Fallback to jobTitle
  if (member.jobTitle) return member.jobTitle
  return null
}

// Watch committee selection to auto-populate attendees with roles
watch(() => meeting.committeeId, async (newId, oldId) => {
  // Skip during initial load of existing meeting
  if (loading.value || !newId || newId === oldId) return

  try {
    const [membersResponse, rolesResponse] = await Promise.all([
      CouncilCommitteesService.listUsersInCouncilCommittee(newId.toString()),
      LookupsService.listCommitteeRoles()
    ])
    const members = Array.isArray(membersResponse) ? membersResponse : ((membersResponse as any)?.data || [])
    const roles = Array.isArray(rolesResponse) ? rolesResponse : ((rolesResponse as any)?.data || [])
    const rolesMap = new Map(roles.map((r: any) => [String(r.id), r.name || r.nameAr || r.nameEn]))

    meeting.meetingAttendees = members.map((member: any) => ({
      userId: Number(member.userId || member.id),
      name: member.fullName || member.fullname || member.userName,
      email: member.email || '',
      jobTitle: resolveRoleName(member, rolesMap),
      needsApproval: false
    }))
  } catch (error) {
    console.error('Failed to load committee members:', error)
  }
})

// Watch route changes
watch(() => route.params.id, (newId) => {
  // Skip if we just saved and updated the URL
  if (skipRouteWatch.value) {
    skipRouteWatch.value = false
    return
  }
  if (newId) {
    loadMeeting(newId as string)
  } else {
    resetPage()
  }
})

// Methods
const resetPage = () => {
  currentStep.value = 0
  completedSteps.value = []
  isPosted.value = false
  Object.assign(meeting, {
    id: undefined,
    title: null,
    isCommittee: false,
    committeeId: null,
    committeeName: null,
    committeeTypeId: null,
    date: null,
    startTime: '11:00',
    endTime: '11:30',
    location: null,
    notes: null,
    typeId: null,
    url: null,
    councilSessionId: null,
    referenceNumber: null,
    meetingAttendees: [],
    meetingAgendas: [],
    attachments: [],
    associatedMeetings: [],
    readOnly: false
  })
}

const createAnother = () => {
  resetPage()
  router.push('/addMeeting')
}

const loadMeeting = async (id: string) => {
  loading.value = true
  try {
    const response = await MeetingsService.loadMeeting(id)
    // API returns { data: {...}, success: true }
    const data = response.data || response

    Object.assign(meeting, {
      id: data.id,
      title: data.title,
      isCommittee: data.isCommittee ?? !!data.committeeId,
      committeeId: data.committeeId,
      committeeName: data.committeeName,
      committeeTypeId: data.committeeTypeId,
      date: data.date,
      startTime: data.startTime || '11:00',
      endTime: data.endTime || '11:30',
      location: data.location,
      notes: data.notes,
      typeId: data.typeId,
      url: data.url,
      councilSessionId: data.councilSessionId,
      referenceNumber: data.referenceNumber || data.id?.toString(),
      meetingAttendees: data.meetingAttendees || [],
      meetingAgendas: data.meetingAgendas || [],
      attachments: data.attachments || [],
      associatedMeetings: data.associatedMeetings || [],
      readOnly: data.readOnly || false
    })

    // Backfill missing roles from committee if attendees have empty jobTitle
    const committeeId = data.committeeId
    const attendees = meeting.meetingAttendees
    if (committeeId && attendees.some((a: any) => !a.jobTitle)) {
      try {
        const [membersResponse, rolesResponse] = await Promise.all([
          CouncilCommitteesService.listUsersInCouncilCommittee(committeeId.toString()),
          LookupsService.listCommitteeRoles()
        ])
        const members = Array.isArray(membersResponse) ? membersResponse : ((membersResponse as any)?.data || [])
        const roles = Array.isArray(rolesResponse) ? rolesResponse : ((rolesResponse as any)?.data || [])
        const rolesMap = new Map(roles.map((r: any) => [String(r.id), r.name || r.nameAr || r.nameEn]))
        const memberMap = new Map(members.map((m: any) => [String(m.userId || m.id), m]))

        meeting.meetingAttendees = attendees.map((a: any) => {
          if (!a.jobTitle) {
            const member = memberMap.get(String(a.userId))
            if (member) {
              return { ...a, jobTitle: resolveRoleName(member, rolesMap) }
            }
          }
          return a
        })
      } catch (error) {
        console.error('Failed to backfill committee roles:', error)
      }
    }

    originalMeeting.value = JSON.stringify(meeting)
  } catch (error) {
    console.error('Failed to load meeting:', error)
  } finally {
    loading.value = false
  }
}

const goToStep = (index: number) => {
  if (index <= currentStep.value || completedSteps.value.includes(index - 1)) {
    currentStep.value = index
  }
}

const validateCurrentStep = (): boolean => {
  switch (currentStep.value) {
    case 0:
      return metaDataRef.value?.validate() ?? false
    case 1:
      return attendeesRef.value?.validate() ?? meeting.meetingAttendees.length > 0
    case 2:
      return agendaRef.value?.validate() ?? meeting.meetingAgendas.length > 0
    default:
      return true
  }
}

const nextStep = async () => {
  // Clear any previous validation error
  validationError.value = null

  // Validate and save on step 1 (Meeting Info)
  if (currentStep.value === 0) {
    const saved = await saveMeetingInfo()
    if (!saved) return
  }

  // Validate attendees on step 2
  if (currentStep.value === 1) {
    if (!meeting.meetingAttendees || meeting.meetingAttendees.length === 0) {
      validationError.value = t('AtLeastOneAttendeeRequired')
      return
    }
  }

  if (!completedSteps.value.includes(currentStep.value)) {
    completedSteps.value.push(currentStep.value)
  }

  if (currentStep.value < steps.value.length - 1) {
    currentStep.value++
  }
}

const previousStep = () => {
  validationError.value = null
  if (currentStep.value > 0) {
    currentStep.value--
  }
}

const saveDraft = async () => {
  await saveMeetingInfo()
}

const saveMeetingInfo = async (): Promise<boolean> => {
  if (currentStep.value === 0 && !metaDataRef.value?.validate()) {
    return false
  }

  saving.value = true
  try {
    // Build meeting data matching the old system's structure exactly
    const meetingData: Record<string, any> = {
      id: meeting.id ? Number(meeting.id) : 0,
      title: meeting.title,
      isCommittee: meeting.isCommittee,
      committeeId: meeting.committeeId ? Number(meeting.committeeId) : null,
      date: meeting.date,
      startTime: meeting.startTime || '11:00',
      endTime: meeting.endTime || '11:30',
      location: meeting.location,
      notes: meeting.notes || null,
      typeId: meeting.typeId ? Number(meeting.typeId) : null,
      url: meeting.url || null,
      councilSessionId: meeting.councilSessionId ? Number(meeting.councilSessionId) : null,
      committeeTypeId: meeting.committeeTypeId ? Number(meeting.committeeTypeId) : null
    }

    const isNewMeeting = !meeting.id
    let result
    if (meeting.id) {
      result = await MeetingsService.updateMeetingInfo(meetingData)
    } else {
      result = await MeetingsService.saveMeetingInfo(meetingData)
    }

    // Handle response - API returns data in .data property
    const responseData = result.data || result

    if (responseData && responseData.id) {
      // Update meeting with response data
      for (const key in responseData) {
        if (Object.prototype.hasOwnProperty.call(responseData, key) && key in meeting) {
          (meeting as any)[key] = responseData[key]
        }
      }
      meeting.id = responseData.id
      meeting.referenceNumber = responseData.id?.toString()

      // Update URL if new meeting was created - use history API to avoid triggering watcher
      if (isNewMeeting && responseData.id) {
        window.history.replaceState({}, '', `/addMeeting/${responseData.id}`)
      }
    }

    originalMeeting.value = JSON.stringify(meeting)
    return true
  } catch (error) {
    console.error('Failed to save meeting:', error)
    return false
  } finally {
    saving.value = false
  }
}

const confirmSendMeeting = async () => {
  if (!meeting.id) return

  sending.value = true
  try {
    await MeetingsService.sendMeeting(meeting.id.toString())
    showSendConfirm.value = false
    isPosted.value = true
  } catch (error) {
    console.error('Failed to send meeting:', error)
  } finally {
    sending.value = false
  }
}

const confirmApproveMeeting = async () => {
  if (!meeting.id) return

  approving.value = true
  try {
    await MeetingsService.approveMeeting(meeting.id.toString())
    showApproveConfirm.value = false
    isPosted.value = true
  } catch (error) {
    console.error('Failed to approve meeting:', error)
  } finally {
    approving.value = false
  }
}

const formatDate = (date: string | null) => {
  if (!date) return '-'
  try {
    const loc = userStore.language === 'ar' ? 'ar-EG' : 'en-US'
    return new Date(date).toLocaleDateString(loc, {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      calendar: 'gregory'
    })
  } catch {
    return date
  }
}

// Lifecycle
onMounted(() => {
  if (meetingId.value) {
    loadMeeting(meetingId.value)
  }
})
</script>

<style scoped>
.add-meeting-page {
  /* styles if needed */
}

/* Success Screen */
.success-overlay {
  @apply fixed inset-0 z-50 flex items-center justify-center p-4;
  background: rgba(248, 250, 252, 0.95);
  backdrop-filter: blur(8px);
}

.success-card {
  @apply relative w-full max-w-md bg-white overflow-hidden;
  border-radius: 16px;
  border: 1px solid #e6eaef;
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.08);
  animation: card-appear 0.4s ease-out;
}

@keyframes card-appear {
  0% { opacity: 0; transform: translateY(16px); }
  100% { opacity: 1; transform: translateY(0); }
}

/* Decorative top line */
.success-decoration { display: none; }
.decoration-circle { display: none; }

/* Success Icon */
.success-icon-container {
  display: flex;
  justify-content: center;
  padding-top: 32px;
  margin-bottom: 16px;
}

.success-icon-bg {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: rgba(0, 109, 75, 0.1);
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Animated Checkmark */
.checkmark {
  width: 36px;
  height: 36px;
  stroke: #006d4b;
  stroke-miterlimit: 10;
}

.checkmark-circle {
  stroke-dasharray: 166;
  stroke-dashoffset: 166;
  stroke-width: 2;
  stroke: #006d4b;
  fill: none;
  animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
}

.checkmark-check {
  transform-origin: 50% 50%;
  stroke-dasharray: 48;
  stroke-dashoffset: 48;
  stroke-width: 3;
  animation: stroke 0.3s cubic-bezier(0.65, 0, 0.45, 1) 0.4s forwards;
}

@keyframes stroke {
  100% { stroke-dashoffset: 0; }
}

/* Content */
.success-content {
  text-align: center;
  padding: 0 24px 20px;
}

.success-title {
  font-size: 18px;
  font-weight: 600;
  color: #004730;
  margin: 0 0 4px;
}

.success-subtitle {
  font-size: 13px;
  color: #94a3b8;
  margin: 0;
}

/* Meeting Summary */
.success-summary {
  margin: 0 20px 20px;
  padding: 12px 14px;
  border-radius: 10px;
  background: #f8fafc;
  border: 1px solid #e6eaef;
}

.summary-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 0;
}

.success-summary .summary-icon {
  width: 34px;
  height: 34px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background: #fff;
  border: 1px solid #e6eaef;
}

.success-summary .summary-icon svg {
  color: #006d4b;
  stroke: #006d4b;
  width: 16px;
  height: 16px;
}

.summary-details {
  display: flex;
  flex-direction: column;
  min-width: 0;
  flex: 1;
}

.summary-label {
  font-size: 11px;
  color: #94a3b8;
  font-weight: 500;
}

.summary-value {
  font-size: 13px;
  color: #004730;
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.summary-divider {
  height: 1px;
  background: #e6eaef;
  margin: 2px 0;
}

/* Actions */
.success-actions {
  display: flex;
  gap: 10px;
  padding: 0 20px 16px;
}

.action-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 10px 16px;
  border-radius: 8px;
  font-weight: 500;
  font-size: 13px;
  cursor: pointer;
  border: none;
  transition: all 0.15s;
}

.action-btn.primary {
  color: #fff;
  background: linear-gradient(135deg, #004730 0%, #006d4b 100%);
}

.action-btn.primary:hover {
  opacity: 0.9;
}

.action-btn.secondary {
  background: #f1f5f9;
  color: #334155;
  border: 1px solid #e2e8f0;
}

.action-btn.secondary:hover {
  background: #e2e8f0;
}

/* Footer Note */
.success-footer {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 12px 20px;
  font-size: 11px;
  color: #94a3b8;
  border-top: 1px solid #f1f5f9;
}

/* Sidebar Progress — dark card */
.sidebar-progress {
  background: #fff;
  border: 1px solid #e4ede8;
  border-top: 3px solid #006d4b;
  @apply rounded-xl p-4 mb-4;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.25);
}

.progress-header {
  @apply flex items-center justify-between mb-2;
}

.progress-label {
  @apply text-sm font-semibold;
  color: #5a7a6d;
}

.progress-value {
  @apply text-lg font-bold;
  color: #1a2e25;
}

.progress-track {
  @apply h-2 rounded-full overflow-hidden;
  background: #e4ede8;
}

.sidebar-progress .progress-fill {
  @apply h-full rounded-full transition-all duration-500 ease-out;
  background: linear-gradient(90deg, #006d4b 0%, #63a58f 100%);
}

/* Main Content */
.main-content {
  @apply flex gap-6;
}

/* Steps Sidebar */
.steps-sidebar {
  @apply w-80 flex-shrink-0;
}

.steps-list {
  background: #fff;
  border: 1px solid #e4ede8;
  @apply rounded-2xl p-4 mb-4;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
}

.step-item {
  @apply flex gap-4 p-3 rounded-xl transition-all text-start w-full;
}

.step-item:hover:not(.disabled) {
  background: #f4f8f6;
}

.step-item.active {
  background: #eef5f1;
}

.step-item.disabled {
  @apply cursor-not-allowed;
  opacity: 0.7;
}

.step-indicator {
  @apply flex flex-col items-center;
}

.step-icon-wrapper {
  @apply relative w-12 h-12 rounded-xl flex items-center justify-center transition-all;
  background: #f4f8f6;
  color: #5a7a6d;
}

.step-item.active .step-icon-wrapper {
  @apply bg-primary text-white;
  box-shadow: 0 4px 12px rgba(0, 109, 75, 0.3);
}

.step-item.completed .step-icon-wrapper {
  @apply bg-emerald-500 text-white;
}

.step-icon {
  @apply w-5 h-5;
}

.step-check {
  @apply w-5 h-5;
}

.step-connector {
  @apply flex-1 w-0.5 my-2;
}

.connector-line {
  @apply w-full h-full rounded-full;
  background: #e4ede8;
}

.connector-line.filled {
  @apply bg-emerald-500;
}

.step-content {
  @apply flex-1 min-w-0;
}

.step-num {
  @apply block text-[10px] uppercase tracking-wider font-medium mb-0.5;
  color: #8aa89b;
}

.step-title {
  @apply block text-sm font-bold mb-0.5;
  color: #374a41;
}

.step-item.active .step-title {
  color: #006d4b;
}

.step-item.completed .step-title {
  color: #8aa89b;
}

.step-desc {
  @apply block text-xs line-clamp-1;
  color: #8aa89b;
}

/* Quick Actions & Save Draft Button */
.quick-actions {
  @apply rounded-xl;
}

.save-draft-btn {
  @apply w-full flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl;
  @apply text-sm font-semibold transition-all duration-200;
  background: rgba(0, 109, 75, 0.12);
  color: #006d4b;
  border: 1px solid rgba(0, 109, 75, 0.2);
}

.save-draft-btn:hover:not(:disabled) {
  background: rgba(0, 109, 75, 0.2);
  border-color: rgba(0, 109, 75, 0.4);
  transform: translateY(-1px);
}

.save-draft-btn:active:not(:disabled) {
  transform: translateY(0);
}

.save-draft-btn:disabled {
  @apply opacity-60 cursor-not-allowed;
}

/* Form Content */
.form-content {
  @apply flex-1 min-w-0;
  overflow: hidden;
}

/* Loading State */
.loading-state {
  @apply flex flex-col items-center justify-center py-20 bg-white rounded-2xl border border-zinc-100;
}

.loader {
  @apply relative w-16 h-16 mb-4;
}

.loader-ring {
  @apply absolute inset-0 border-4 border-primary/20 border-t-primary rounded-full animate-spin;
}

.loader-icon {
  @apply absolute inset-0 m-auto w-6 h-6 text-primary;
}

/* Step Panel */
.step-panel {
  @apply bg-white rounded-2xl border border-zinc-100;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.panel-header {
  @apply flex items-center gap-4 p-5 rounded-t-2xl;
  background: #f4f8f6;
  border-bottom: 1px solid #e4ede8;
}

.panel-icon {
  @apply w-12 h-12 rounded-xl flex items-center justify-center;
}

.panel-info {
  @apply flex-1;
}

.panel-title {
  @apply text-lg font-bold;
  color: #1a2e25;
}

.panel-desc {
  @apply text-sm;
  color: #5a7a6d;
}

.panel-body {
  @apply p-5;
}

/* Review Section */
.review-section {
  @apply space-y-6;
}

.summary-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
}

.summary-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 16px;
  background: #fff;
  border: 1px solid #e6eaef;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.15s;
}

.summary-card:hover {
  border-color: rgba(0, 109, 75, 0.3);
  background: #f8fafc;
}

.summary-icon {
  width: 38px;
  height: 38px;
  border-radius: 9px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.summary-icon.icon-teal {
  background: linear-gradient(135deg, #e6f7f3, #ccf0e8);
  color: #007E65;
}

.summary-icon.icon-navy {
  background: linear-gradient(135deg, #e8edf3, #dce3ec);
  color: #004730;
}

.summary-content {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.summary-label {
  font-size: 11px;
  font-weight: 600;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.summary-value {
  font-size: 14px;
  font-weight: 600;
  color: #004730;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.summary-meta {
  font-size: 12px;
  color: #64748b;
}

.summary-arrow {
  color: #cbd5e1;
  width: 18px;
  height: 18px;
  flex-shrink: 0;
  transition: color 0.15s;
}

[dir="rtl"] .summary-arrow {
  transform: scaleX(-1);
}

.summary-card:hover .summary-arrow {
  color: #006d4b;
}

.associated-section {
  @apply pt-4 border-t border-zinc-100;
}

.section-title {
  @apply flex items-center gap-2 text-lg font-bold text-zinc-900 mb-4;
}

/* Panel Footer (inside card) */
.panel-footer {
  @apply flex flex-col gap-3 border-t border-zinc-200 rounded-b-2xl bg-white;
  padding: 1rem;
}

.nav-actions {
  @apply flex items-center justify-between w-full;
}

.nav-start,
.nav-end {
  @apply flex items-center gap-3;
}

/* Validation Error */
.validation-error {
  @apply flex items-center gap-2 px-4 py-3 rounded-lg text-sm font-medium;
  @apply bg-red-50 text-red-700 border border-red-200;
}

/* Fade transition */
/* Send & Approve Buttons */
.send-btn {
  @apply inline-flex items-center gap-2 px-5 py-2.5 rounded-xl font-medium text-sm text-white;
  @apply bg-primary hover:bg-primary/90 active:bg-primary/80;
  @apply disabled:opacity-50 disabled:cursor-not-allowed;
  @apply transition-all duration-200;
  box-shadow: 0 4px 12px rgba(0, 109, 75, 0.25);
}

.approve-btn {
  @apply inline-flex items-center gap-2 px-5 py-2.5 rounded-xl font-medium text-sm text-white;
  background: linear-gradient(135deg, #004730 0%, #003423 100%);
  @apply hover:opacity-90 active:opacity-80;
  @apply disabled:opacity-50 disabled:cursor-not-allowed;
  @apply transition-all duration-200;
  box-shadow: 0 4px 12px rgba(0, 135, 90, 0.25);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Transitions */
.fade-scale-enter-active,
.fade-scale-leave-active {
  transition: all 0.5s ease;
}

.fade-scale-enter-from,
.fade-scale-leave-to {
  opacity: 0;
  transform: scale(0.95);
}

.slide-fade-enter-active {
  transition: opacity 0.25s ease-out, transform 0.25s ease-out;
}

.slide-fade-leave-active {
  transition: opacity 0.15s ease-in, transform 0.15s ease-in;
}

.slide-fade-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.slide-fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* Confirmation Modal */
.modal-overlay {
  @apply fixed inset-0 z-50 flex items-center justify-center p-4;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
}

.modal-container {
  @apply bg-white rounded-2xl p-6 w-full max-w-md text-center;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
}

.modal-icon {
  @apply w-20 h-20 mx-auto mb-4 rounded-full flex items-center justify-center;
}

.modal-icon.send {
  @apply bg-primary/10 text-primary;
}

.modal-icon.approve {
  @apply bg-emerald-100 text-emerald-600;
}

.modal-title {
  @apply text-xl font-bold text-zinc-900 mb-2;
}

.modal-text {
  @apply text-zinc-600 text-sm mb-4 leading-relaxed;
}

.modal-warning {
  @apply flex items-center justify-center gap-2 px-4 py-3 mb-4 rounded-lg;
  @apply bg-amber-50 text-amber-700 text-sm font-medium;
}

.modal-info {
  @apply flex flex-col gap-2 p-4 mb-5 rounded-xl bg-zinc-50;
}

.info-item {
  @apply flex items-center gap-2 text-sm text-zinc-600;
}

.modal-actions {
  @apply flex items-center justify-center gap-3;
}

.modal-btn {
  @apply inline-flex items-center gap-2 px-5 py-2.5 rounded-xl font-medium text-sm transition-all;
}

.modal-btn.cancel {
  @apply bg-zinc-100 text-zinc-700 hover:bg-zinc-200;
}

.modal-btn.confirm {
  @apply text-white;
}

.modal-btn.confirm.send {
  @apply bg-primary hover:bg-primary/90;
  box-shadow: 0 4px 12px rgba(0, 109, 75, 0.25);
}

.modal-btn.confirm.approve {
  @apply bg-emerald-600 hover:bg-emerald-700;
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.25);
}

.modal-btn:disabled {
  @apply opacity-50 cursor-not-allowed;
}

/* Modal Transition */
.modal-fade-enter-active {
  transition: all 0.3s ease-out;
}

.modal-fade-leave-active {
  transition: all 0.2s ease-in;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

.modal-fade-enter-from .modal-container,
.modal-fade-leave-to .modal-container {
  transform: scale(0.95) translateY(10px);
}

/* Responsive */
@media (max-width: 1024px) {
  .main-content {
    @apply flex-col;
  }

  .steps-sidebar {
    @apply w-full;
  }

  .sidebar-progress {
    @apply mb-3;
  }

  .steps-list {
    @apply flex overflow-x-auto gap-2 p-3;
  }

  .step-item {
    @apply flex-col items-center text-center min-w-[100px] p-2;
  }

  .step-indicator {
    @apply flex-row;
  }

  .step-connector {
    @apply hidden;
  }

  .step-num {
    @apply hidden;
  }

  .step-desc {
    @apply hidden;
  }

  .summary-grid {
    @apply grid-cols-1;
  }
}
</style>

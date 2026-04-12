<template>
  <div class="space-y-5">
    <!-- Page Header -->
    <PageHeader :title="$t('OutlookIntegration')" :subtitle="$t('OutlookIntegrationDescription')" :breadcrumbs="[{ label: 'Settings', to: '/settings' }]">
      <template #actions>
        <button
          v-if="hasChanges"
          class="btn-clean primary"
          :disabled="saving"
          @click="saveSettings"
        >
          <Icon icon="mdi:content-save" class="w-4 h-4" />
          {{ $t('SaveChanges') }}
        </button>
      </template>
    </PageHeader>

    <!-- Status Banner -->
    <div :class="['status-banner', settings.enabled ? 'enabled' : 'disabled']">
      <div class="status-content">
        <Icon :icon="settings.enabled ? 'mdi:check-circle' : 'mdi:close-circle'" class="w-6 h-6" />
        <div>
          <h3>{{ settings.enabled ? ($t('IntegrationEnabled')) : ($t('IntegrationDisabled')) }}</h3>
          <p>{{ settings.enabled
            ? ($t('OutlookIntegrationEnabledDesc'))
            : ($t('OutlookIntegrationDisabledDesc')) }}</p>
        </div>
      </div>
      <label class="toggle-switch">
        <input v-model="settings.enabled" type="checkbox" @change="markChanged" />
        <span class="toggle-slider"></span>
      </label>
    </div>

    <div class="content-grid">
      <!-- Settings Section -->
      <div class="settings-section">
        <div class="section-card">
          <div class="section-header">
            <Icon icon="mdi:cog" class="w-5 h-5 text-indigo-500" />
            <h2>{{ $t('IntegrationSettings') }}</h2>
          </div>

          <div class="settings-form">
            <!-- Mode Selection -->
            <div class="form-group">
              <label>{{ $t('IntegrationMode') }}</label>
              <div class="radio-group">
                <label class="radio-option">
                  <input v-model="settings.mode" type="radio" value="SMTP" @change="markChanged" />
                  <div class="radio-content">
                    <Icon icon="mdi:email-fast" class="w-5 h-5" />
                    <div>
                      <span class="radio-title">SMTP</span>
                      <span class="radio-desc">{{ $t('SmtpModeDesc') }}</span>
                    </div>
                  </div>
                </label>
                <label class="radio-option">
                  <input v-model="settings.mode" type="radio" value="Graph" @change="markChanged" />
                  <div class="radio-content">
                    <Icon icon="mdi:microsoft" class="w-5 h-5" />
                    <div>
                      <span class="radio-title">Microsoft Graph API</span>
                      <span class="radio-desc">{{ $t('GraphModeDesc') }}</span>
                    </div>
                  </div>
                </label>
              </div>
            </div>

            <!-- Auto-send Options -->
            <div class="form-group">
              <label>{{ $t('AutoSendOptions') }}</label>
              <div class="opt-list">
                <label class="opt-card">
                  <div class="opt-info">
                    <Icon icon="mdi:calendar-plus" class="w-5 h-5 text-green-500 flex-shrink-0" />
                    <div>
                      <span class="opt-title">{{ $t('SendInviteOnApproval') }}</span>
                      <span class="opt-desc">{{ $t('SendInviteOnApprovalDesc') }}</span>
                    </div>
                  </div>
                  <input v-model="settings.sendInviteOnApproval" type="checkbox" class="sr-only peer" @change="markChanged" />
                  <span class="opt-toggle"></span>
                </label>
                <label class="opt-card">
                  <div class="opt-info">
                    <Icon icon="mdi:calendar-edit" class="w-5 h-5 text-amber-500 flex-shrink-0" />
                    <div>
                      <span class="opt-title">{{ $t('SendUpdateOnChange') }}</span>
                      <span class="opt-desc">{{ $t('SendUpdateOnChangeDesc') }}</span>
                    </div>
                  </div>
                  <input v-model="settings.sendUpdateOnChange" type="checkbox" class="sr-only peer" @change="markChanged" />
                  <span class="opt-toggle"></span>
                </label>
                <label class="opt-card">
                  <div class="opt-info">
                    <Icon icon="mdi:calendar-remove" class="w-5 h-5 text-red-500 flex-shrink-0" />
                    <div>
                      <span class="opt-title">{{ $t('SendCancellationOnCancel') }}</span>
                      <span class="opt-desc">{{ $t('SendCancellationOnCancelDesc') }}</span>
                    </div>
                  </div>
                  <input v-model="settings.sendCancellationOnCancel" type="checkbox" class="sr-only peer" @change="markChanged" />
                  <span class="opt-toggle"></span>
                </label>
              </div>
            </div>
          </div>
        </div>

        <!-- SMTP Settings (shown when SMTP mode) -->
        <div v-if="settings.mode === 'SMTP'" class="section-card">
          <div class="section-header">
            <Icon icon="mdi:email-outline" class="w-5 h-5 text-emerald-500" />
            <h2>{{ $t('SmtpSettings') }}</h2>
            <span class="badge info">{{ $t('RequiredForSending') }}</span>
          </div>

          <div class="settings-form">
            <div class="form-row">
              <div class="form-group">
                <label>{{ $t('SmtpHost') }}</label>
                <input v-model="smtpSettings.host" type="text" :placeholder="$t('SmtpHostPlaceholder')" @input="markChanged" />
              </div>
              <div class="form-group">
                <label>{{ $t('SmtpPort') }}</label>
                <input v-model="smtpSettings.port" type="number" placeholder="587" @input="markChanged" />
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>{{ $t('SmtpUser') }}</label>
                <input v-model="smtpSettings.user" type="text" :placeholder="$t('SmtpUserPlaceholder')" @input="markChanged" />
              </div>
              <div class="form-group">
                <label>{{ $t('SmtpPassword') }}</label>
                <div class="password-input">
                  <input v-model="smtpSettings.password" :type="showPassword ? 'text' : 'password'" @input="markChanged" />
                  <button type="button" @click="showPassword = !showPassword">
                    <Icon :icon="showPassword ? 'mdi:eye-off' : 'mdi:eye'" class="w-5 h-5" />
                  </button>
                </div>
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>{{ $t('SmtpFromEmail') }}</label>
                <input v-model="smtpSettings.fromEmail" type="email" :placeholder="$t('SmtpFromEmailPlaceholder')" @input="markChanged" />
              </div>
              <div class="form-group">
                <label>{{ $t('EnableSSL') }}</label>
                <label class="toggle-switch inline">
                  <input v-model="smtpSettings.enableSSL" type="checkbox" @change="markChanged" />
                  <span class="toggle-slider"></span>
                </label>
              </div>
            </div>
          </div>
        </div>

        <!-- Graph API Settings (shown when Graph mode) -->
        <div v-if="settings.mode === 'Graph'" class="section-card">
          <div class="section-header">
            <Icon icon="mdi:microsoft-azure" class="w-5 h-5 text-blue-500" />
            <h2>{{ $t('GraphApiSettings') }}</h2>
          </div>

          <div class="settings-form">
            <div class="form-group">
              <label>{{ $t('TenantId') }}</label>
              <input v-model="settings.graph.tenantId" type="text" placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" @input="markChanged" />
            </div>
            <div class="form-group">
              <label>{{ $t('ClientId') }}</label>
              <input v-model="settings.graph.clientId" type="text" placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" @input="markChanged" />
            </div>
            <div class="form-group">
              <label>{{ $t('ClientSecret') }}</label>
              <div class="password-input">
                <input v-model="settings.graph.clientSecret" :type="showClientSecret ? 'text' : 'password'" @input="markChanged" />
                <button type="button" @click="showClientSecret = !showClientSecret">
                  <Icon :icon="showClientSecret ? 'mdi:eye-off' : 'mdi:eye'" class="w-5 h-5" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Test Section -->
      <div class="test-section">
        <div class="section-card">
          <div class="section-header">
            <Icon icon="mdi:test-tube" class="w-5 h-5 text-purple-500" />
            <h2>{{ $t('TestIntegration') }}</h2>
          </div>

          <div class="test-content">
            <p class="test-description">
              {{ $t('TestIntegrationDesc') }}
            </p>

            <!-- Meeting Selection -->
            <div class="form-group">
              <label>{{ $t('SelectMeeting') }}</label>
              <div class="meeting-search">
                <Icon icon="mdi:magnify" class="search-icon" />
                <input
                  v-model="meetingSearch"
                  type="text"
                  :placeholder="$t('SearchMeetings')"
                  @input="searchMeetings"
                />
              </div>

              <div v-if="searchingMeetings" class="loading-indicator">
                <Icon icon="mdi:loading" class="w-5 h-5 animate-spin" />
                <span>{{ $t('Searching') }}</span>
              </div>

              <div v-if="searchResults.length > 0" class="meeting-results">
                <button
                  v-for="meeting in searchResults"
                  :key="meeting.id"
                  :class="['meeting-item', { selected: selectedMeeting?.id === meeting.id }]"
                  @click="selectMeeting(meeting)"
                >
                  <div class="meeting-info">
                    <span class="meeting-title">{{ meeting.title }}</span>
                    <span class="meeting-meta">
                      <Icon icon="mdi:calendar" class="w-4 h-4" />
                      {{ formatDate(meeting.date) }}
                      <Icon icon="mdi:account-group" class="w-4 h-4 mr-2" />
                      {{ meeting.attendeesCount }} {{ $t('Attendees') }}
                    </span>
                  </div>
                  <Icon v-if="selectedMeeting?.id === meeting.id" icon="mdi:check-circle" class="w-5 h-5 text-green-500" />
                </button>
              </div>

              <div v-if="meetingSearch && !searchingMeetings && searchResults.length === 0" class="no-results">
                <Icon icon="mdi:calendar-question" class="w-8 h-8 text-zinc-300" />
                <span>{{ $t('NoMeetingsFound') }}</span>
              </div>
            </div>

            <!-- Selected Meeting Preview -->
            <div v-if="selectedMeeting" class="selected-meeting-preview">
              <div class="preview-header">
                <Icon icon="mdi:calendar-check" class="w-5 h-5 text-indigo-500" />
                <span>{{ $t('SelectedMeeting') }}</span>
              </div>
              <div class="preview-content">
                <h4>{{ selectedMeeting.title }}</h4>
                <div class="preview-details">
                  <span><Icon icon="mdi:calendar" class="w-4 h-4" /> {{ formatDate(selectedMeeting.date) }}</span>
                  <span><Icon icon="mdi:clock-outline" class="w-4 h-4" /> {{ selectedMeeting.startTime }} - {{ selectedMeeting.endTime }}</span>
                  <span><Icon icon="mdi:map-marker" class="w-4 h-4" /> {{ selectedMeeting.location || ($t('NoLocation')) }}</span>
                  <span><Icon icon="mdi:account-group" class="w-4 h-4" /> {{ selectedMeeting.attendeesCount }} {{ $t('Attendees') }}</span>
                </div>
              </div>
            </div>

            <!-- Test Button -->
            <Button
              variant="primary"
              icon-left="mdi:send"
              :loading="sendingTest"
              :disabled="!selectedMeeting || !settings.enabled"
              class="test-button"
              @click="sendTestInvite"
            >
              {{ $t('SendTestInvite') }}
            </Button>

            <!-- Test Result -->
            <div v-if="testResult" :class="['test-result', testResult.success ? 'success' : 'error']">
              <Icon :icon="testResult.success ? 'mdi:check-circle' : 'mdi:alert-circle'" class="w-5 h-5" />
              <div>
                <h4>{{ testResult.success ? ($t('TestSuccess')) : ($t('TestFailed')) }}</h4>
                <p>{{ testResult.message }}</p>
              </div>
            </div>

            <!-- Help Text -->
            <div class="help-section">
              <h4><Icon icon="mdi:help-circle" class="w-4 h-4" /> {{ $t('WhatToExpect') }}</h4>
              <ul>
                <li>{{ $t('ExpectEmail') }}</li>
                <li>{{ $t('ExpectOutlookButtons') }}</li>
                <li>{{ $t('ExpectCalendarEvent') }}</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import { formatDate as formatDateHelper } from '@/helpers/dateFormat'
import Button from '@/components/ui/Button.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import { useToast } from '@/composables/useToast'
import { mainApiAxios as axios } from '@/plugins/axios'
import { debounce } from '@/utils/apiUtils'

interface OutlookSettings {
  enabled: boolean
  mode: 'SMTP' | 'Graph'
  sendInviteOnApproval: boolean
  sendUpdateOnChange: boolean
  sendCancellationOnCancel: boolean
  graph: {
    enabled: boolean
    tenantId: string
    clientId: string
    clientSecret: string
  }
}

interface SmtpSettings {
  host: string
  port: number
  user: string
  password: string
  fromEmail: string
  enableSSL: boolean
}

interface Meeting {
  id: number
  title: string
  date: string
  startTime: string
  endTime: string
  location: string
  attendeesCount: number
}

interface TestResult {
  success: boolean
  message: string
}

const { showToast } = useToast()

const loading = ref(false)
const saving = ref(false)
const hasChanges = ref(false)
const showPassword = ref(false)
const showClientSecret = ref(false)

const settings = reactive<OutlookSettings>({
  enabled: false,
  mode: 'SMTP',
  sendInviteOnApproval: true,
  sendUpdateOnChange: true,
  sendCancellationOnCancel: true,
  graph: {
    enabled: false,
    tenantId: '',
    clientId: '',
    clientSecret: ''
  }
})

const smtpSettings = reactive<SmtpSettings>({
  host: '',
  port: 587,
  user: '',
  password: '',
  fromEmail: '',
  enableSSL: true
})

// Test section
const meetingSearch = ref('')
const searchingMeetings = ref(false)
const searchResults = ref<Meeting[]>([])
const selectedMeeting = ref<Meeting | null>(null)
const sendingTest = ref(false)
const testResult = ref<TestResult | null>(null)

function markChanged() {
  hasChanges.value = true
}

async function loadSettings() {
  try {
    loading.value = true
    const response = await axios.get('systemSettings')
    const allSettings = response.data || []

    // Find OutlookIntegration settings
    const outlookCategory = allSettings.find((c: any) => c.categoryName === 'OutlookIntegration')
    if (outlookCategory?.items) {
      for (const item of outlookCategory.items) {
        const key = item.name.replace('OutlookIntegration:', '')
        if (key === 'Enabled') settings.enabled = item.value === 'true'
        else if (key === 'Mode') settings.mode = item.value || 'SMTP'
        else if (key === 'SendInviteOnApproval') settings.sendInviteOnApproval = item.value === 'true'
        else if (key === 'SendUpdateOnChange') settings.sendUpdateOnChange = item.value === 'true'
        else if (key === 'SendCancellationOnCancel') settings.sendCancellationOnCancel = item.value === 'true'
        else if (key === 'Graph:Enabled') settings.graph.enabled = item.value === 'true'
        else if (key === 'Graph:TenantId') settings.graph.tenantId = item.value || ''
        else if (key === 'Graph:ClientId') settings.graph.clientId = item.value || ''
        else if (key === 'Graph:ClientSecret') settings.graph.clientSecret = item.value || ''
      }
    }

    // Find SMTP settings
    const smtpCategory = allSettings.find((c: any) => c.categoryName === 'SMTP')
    if (smtpCategory?.items) {
      for (const item of smtpCategory.items) {
        const key = item.name.replace('Smtp:', '')
        if (key === 'Host') smtpSettings.host = item.value || ''
        else if (key === 'Port') smtpSettings.port = parseInt(item.value) || 587
        else if (key === 'User') smtpSettings.user = item.value || ''
        else if (key === 'Password') smtpSettings.password = item.value || ''
        else if (key === 'FromEmail') smtpSettings.fromEmail = item.value || ''
        else if (key === 'EnableSSL') smtpSettings.enableSSL = item.value === 'true'
      }
    }

    hasChanges.value = false
  } catch (error) {
    console.error('Error loading settings:', error)
    showToast({ type: 'error', message: 'فشل في تحميل الإعدادات' })
  } finally {
    loading.value = false
  }
}

async function saveSettings() {
  try {
    saving.value = true

    // Save OutlookIntegration settings
    const outlookItems = [
      { id: 0, name: 'OutlookIntegration:Enabled', value: settings.enabled.toString() },
      { id: 0, name: 'OutlookIntegration:Mode', value: settings.mode },
      { id: 0, name: 'OutlookIntegration:SendInviteOnApproval', value: settings.sendInviteOnApproval.toString() },
      { id: 0, name: 'OutlookIntegration:SendUpdateOnChange', value: settings.sendUpdateOnChange.toString() },
      { id: 0, name: 'OutlookIntegration:SendCancellationOnCancel', value: settings.sendCancellationOnCancel.toString() },
      { id: 0, name: 'OutlookIntegration:Graph:Enabled', value: settings.graph.enabled.toString() },
      { id: 0, name: 'OutlookIntegration:Graph:TenantId', value: settings.graph.tenantId },
      { id: 0, name: 'OutlookIntegration:Graph:ClientId', value: settings.graph.clientId },
      { id: 0, name: 'OutlookIntegration:Graph:ClientSecret', value: settings.graph.clientSecret }
    ]

    // Get IDs from loaded settings
    const response = await axios.get('systemSettings')
    const allSettings = response.data || []
    const outlookCategory = allSettings.find((c: any) => c.categoryName === 'OutlookIntegration')
    if (outlookCategory?.items) {
      for (const item of outlookItems) {
        const existing = outlookCategory.items.find((i: any) => i.name === item.name)
        if (existing) item.id = existing.id
      }
    }

    await axios.put('systemSettings/setting', {
      categoryName: 'OutlookIntegration',
      items: outlookItems.filter(i => i.id > 0)
    })

    // Save SMTP settings if in SMTP mode
    if (settings.mode === 'SMTP') {
      const smtpItems = [
        { id: 0, name: 'Smtp:Host', value: smtpSettings.host },
        { id: 0, name: 'Smtp:Port', value: smtpSettings.port.toString() },
        { id: 0, name: 'Smtp:User', value: smtpSettings.user },
        { id: 0, name: 'Smtp:Password', value: smtpSettings.password },
        { id: 0, name: 'Smtp:FromEmail', value: smtpSettings.fromEmail },
        { id: 0, name: 'Smtp:EnableSSL', value: smtpSettings.enableSSL.toString() }
      ]

      const smtpCategory = allSettings.find((c: any) => c.categoryName === 'SMTP')
      if (smtpCategory?.items) {
        for (const item of smtpItems) {
          const existing = smtpCategory.items.find((i: any) => i.name === item.name)
          if (existing) item.id = existing.id
        }
      }

      await axios.put('systemSettings/setting', {
        categoryName: 'SMTP',
        items: smtpItems.filter(i => i.id > 0)
      })
    }

    // Reload settings in backend
    await axios.put('systemSettings/reload-settings')

    hasChanges.value = false
    showToast({ type: 'success', message: 'تم حفظ الإعدادات بنجاح' })
  } catch (error) {
    console.error('Error saving settings:', error)
    showToast({ type: 'error', message: 'فشل في حفظ الإعدادات' })
  } finally {
    saving.value = false
  }
}

const searchMeetings = debounce(async () => {
  if (!meetingSearch.value || meetingSearch.value.length < 2) {
    searchResults.value = []
    return
  }

  try {
    searchingMeetings.value = true

    // Build search params - check if search is a number (meeting ID)
    const searchValue = meetingSearch.value.trim()
    const meetingId = /^\d+$/.test(searchValue) ? parseInt(searchValue) : null

    // Use the search endpoint with POST
    const response = await axios.post('meetings/search/1/20', {
      title: meetingId ? null : searchValue,
      meetingId: meetingId,
      includeDrafts: false
    })

    // Handle response structure: { data: { data: [...], total: N }, success: true, message: null }
    // The actual meetings array is at response.data.data
    let meetings: any[] = []

    if (response?.data?.data && Array.isArray(response.data.data)) {
      meetings = response.data.data
    } else if (response?.data?.items) {
      meetings = response.data.items
    } else if (Array.isArray(response?.data)) {
      meetings = response.data
    } else if (response?.items) {
      meetings = response.items
    } else if (Array.isArray(response)) {
      meetings = response
    }

    searchResults.value = meetings.map((m: any) => ({
      id: m.id,
      title: m.title,
      date: m.date,
      startTime: m.startTime,
      endTime: m.endTime,
      location: m.location,
      attendeesCount: m.meetingAttendees?.length || m.attendeesCount || 0
    }))
  } catch (error) {
    console.error('Error searching meetings:', error)
    searchResults.value = []
  } finally {
    searchingMeetings.value = false
  }
}, 300)

function selectMeeting(meeting: Meeting) {
  selectedMeeting.value = meeting
  testResult.value = null
}

function formatDate(date: string): string {
  if (!date) return ''
  return formatDateHelper(new Date(date))
}

async function sendTestInvite() {
  if (!selectedMeeting.value) return

  try {
    sendingTest.value = true
    testResult.value = null

    const response = await axios.post(`meetings/${selectedMeeting.value.id}/test-outlook-invite`)

    testResult.value = {
      success: response.success !== false,
      message: response.message || 'تم إرسال دعوة التقويم بنجاح'
    }
  } catch (error: any) {
    console.error('Error sending test invite:', error)
    testResult.value = {
      success: false,
      message: error.response?.data?.message || error.message || 'فشل في إرسال الدعوة'
    }
  } finally {
    sendingTest.value = false
  }
}

onMounted(() => {
  loadSettings()
})
</script>

<style scoped>
/* Status Banner */
.status-banner {
  display: flex; align-items: center; justify-content: space-between;
  padding: 16px; border-radius: 12px;
}
.status-banner.enabled { background: #e6f9f5; border: 1px solid #b2efe0; }
.status-banner.disabled { background: #f8fafc; border: 1px solid #e6eaef; }
.status-content { display: flex; align-items: center; gap: 12px; }
.status-banner.enabled .status-content { color: #007E65; }
.status-banner.disabled .status-content { color: #64748b; }
.status-content h3 { font-weight: 600; margin: 0; }
.status-content p { font-size: 13px; opacity: 0.8; margin: 0; }

/* Toggle */
.toggle-switch { position: relative; display: inline-flex; align-items: center; cursor: pointer; flex-shrink: 0; }
.toggle-switch.inline { margin-top: 8px; }
.toggle-switch input { position: absolute; opacity: 0; width: 0; height: 0; }
.toggle-slider {
  width: 44px; height: 24px; background: #d1d5db; border-radius: 12px; transition: background 0.2s; position: relative;
}
.toggle-slider::before {
  content: ''; position: absolute; width: 20px; height: 20px; background: #fff; border-radius: 50%;
  top: 2px; inset-inline-start: 2px; transition: transform 0.2s; box-shadow: 0 1px 3px rgba(0,0,0,0.15);
}
.toggle-switch input:checked + .toggle-slider { background: #006d4b; }
.toggle-switch input:checked + .toggle-slider::before { transform: translateX(20px); }
[dir="rtl"] .toggle-switch input:checked + .toggle-slider::before { transform: translateX(-20px); }

/* Content Grid */
.content-grid { display: grid; grid-template-columns: 2fr 1fr; gap: 20px; }
.settings-section { display: flex; flex-direction: column; gap: 20px; }

/* Section Card */
.section-card {
  background: #fff; border-radius: 10px; overflow: hidden;
  border: 1px solid #e6eaef; box-shadow: 0 2px 6px rgba(0,0,0,0.04);
}
.section-header { display: flex; align-items: center; gap: 8px; padding: 14px 16px; border-bottom: 1px solid #e6eaef; }
.section-header h2 { font-size: 14px; font-weight: 600; color: #004730; flex: 1; margin: 0; }
.badge { font-size: 11px; padding: 2px 8px; border-radius: 10px; }
.badge.info { background: #e0f2fe; color: #0369a1; }

/* Form */
.settings-form { padding: 16px; display: flex; flex-direction: column; gap: 18px; }
.form-group { display: flex; flex-direction: column; gap: 6px; }
.form-group > label { font-size: 13px; font-weight: 500; color: #334155; }
.form-group input[type="text"], .form-group input[type="email"],
.form-group input[type="number"], .form-group input[type="password"] {
  width: 100%; padding: 8px 12px; border: 1px solid #e2e8f0; border-radius: 8px; font-size: 13px;
  outline: none; transition: border-color 0.15s;
}
.form-group input:focus { border-color: #006d4b; box-shadow: 0 0 0 3px rgba(0,174,140,0.1); }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
.password-input { position: relative; display: flex; align-items: center; }
.password-input input { flex: 1; padding-inline-start: 36px; }
.password-input button {
  position: absolute; inset-inline-start: 10px; top: 50%; transform: translateY(-50%);
  display: flex; color: #94a3b8; background: none; border: none; cursor: pointer;
}
.password-input button:hover { color: #475569; }

/* Radio Group */
.radio-group { display: flex; flex-direction: column; gap: 8px; }
.radio-option { cursor: pointer; }
.radio-option input { position: absolute; opacity: 0; }
.radio-content {
  display: flex; align-items: center; gap: 12px; padding: 12px; border: 1px solid #e6eaef;
  border-radius: 10px; transition: all 0.15s;
}
.radio-option input:checked + .radio-content { border-color: #006d4b; background: rgba(0,174,140,0.05); }
.radio-content > div { display: flex; flex-direction: column; }
.radio-title { font-size: 13px; font-weight: 600; color: #004730; }
.radio-desc { font-size: 11px; color: #94a3b8; margin-top: 2px; }

/* Modern Toggle Option Cards */
.opt-list { display: flex; flex-direction: column; gap: 8px; }
.opt-card {
  display: flex; align-items: center; justify-content: space-between; gap: 12px;
  padding: 12px 14px; border: 1px solid #e6eaef; border-radius: 10px; cursor: pointer;
  transition: border-color 0.15s, background 0.15s;
}
.opt-card:hover { border-color: #d1d5db; }
.opt-card:has(input:checked) { border-color: #006d4b; background: rgba(0,174,140,0.04); }
.opt-info { display: flex; align-items: flex-start; gap: 10px; flex: 1; min-width: 0; }
.opt-title { display: block; font-size: 13px; font-weight: 500; color: #334155; }
.opt-desc { display: block; font-size: 11px; color: #94a3b8; margin-top: 2px; }
.opt-toggle {
  position: relative; width: 44px; height: 24px; background: #d1d5db; border-radius: 12px;
  flex-shrink: 0; transition: background 0.2s;
}
.opt-toggle::after {
  content: ''; position: absolute; top: 2px; inset-inline-start: 2px; width: 20px; height: 20px;
  background: #fff; border-radius: 50%; box-shadow: 0 1px 3px rgba(0,0,0,0.15); transition: transform 0.2s;
}
.opt-card:has(input:checked) .opt-toggle { background: #006d4b; }
.opt-card:has(input:checked) .opt-toggle::after { transform: translateX(20px); }
[dir="rtl"] .opt-card:has(input:checked) .opt-toggle::after { transform: translateX(-20px); }

/* Test Section */
.test-content { padding: 16px; display: flex; flex-direction: column; gap: 14px; }
.test-description { font-size: 13px; color: #64748b; }
.meeting-search { position: relative; }
.meeting-search .search-icon {
  position: absolute; inset-inline-start: 10px; top: 50%; transform: translateY(-50%);
  width: 18px; height: 18px; color: #94a3b8; pointer-events: none;
}
.meeting-search input[type="text"] {
  width: 100% !important;
  padding: 8px 12px 8px 34px !important;
  border: 1px solid #e2e8f0; border-radius: 8px; font-size: 13px; outline: none;
}
[dir="rtl"] .meeting-search input[type="text"] {
  padding: 8px 34px 8px 12px !important;
}
.meeting-search input:focus { border-color: #006d4b; box-shadow: 0 0 0 3px rgba(0,174,140,0.1); }
.loading-indicator { display: flex; align-items: center; gap: 8px; font-size: 13px; color: #94a3b8; padding: 8px 0; }
.meeting-results { margin-top: 8px; border: 1px solid #e6eaef; border-radius: 8px; overflow: hidden; max-height: 240px; overflow-y: auto; }
.meeting-item {
  width: 100%; display: flex; align-items: center; justify-content: space-between;
  padding: 10px 12px; text-align: start; border-bottom: 1px solid #f1f5f9; cursor: pointer;
  background: none; border-left: none; border-right: none; border-top: none; transition: background 0.1s;
}
.meeting-item:last-child { border-bottom: none; }
.meeting-item:hover { background: #f8fafc; }
.meeting-item.selected { background: rgba(0,174,140,0.06); }
.meeting-info { display: flex; flex-direction: column; gap: 2px; }
.meeting-title { font-size: 13px; font-weight: 500; color: #1e293b; }
.meeting-meta { display: flex; align-items: center; gap: 4px; font-size: 11px; color: #94a3b8; }
.no-results { display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 32px 0; color: #94a3b8; }
.selected-meeting-preview { border: 1px solid #b2efe0; border-radius: 10px; overflow: hidden; background: #e6f9f5; }
.preview-header { display: flex; align-items: center; gap: 6px; padding: 8px 12px; background: rgba(0,174,140,0.12); color: #007E65; font-size: 13px; font-weight: 500; }
.preview-content { padding: 12px; }
.preview-content h4 { font-size: 14px; font-weight: 600; color: #004730; margin: 0 0 8px 0; }
.preview-details { display: flex; flex-wrap: wrap; gap: 12px; font-size: 11px; color: #64748b; }
.preview-details span { display: flex; align-items: center; gap: 4px; }
.test-button { width: 100%; }
.test-result { display: flex; align-items: flex-start; gap: 10px; padding: 12px; border-radius: 8px; }
.test-result.success { background: #e6f9f5; color: #007E65; }
.test-result.error { background: #fef2f2; color: #dc2626; }
.test-result h4 { font-weight: 600; margin: 0; }
.test-result p { font-size: 12px; opacity: 0.8; margin: 2px 0 0; }
.help-section { margin-top: 14px; padding-top: 14px; border-top: 1px solid #e6eaef; }
.help-section h4 { display: flex; align-items: center; gap: 4px; font-size: 13px; font-weight: 500; color: #334155; margin: 0 0 8px; }
.help-section ul { font-size: 12px; color: #94a3b8; list-style: disc; padding-inline-start: 18px; margin: 0; }
.help-section li { margin-bottom: 4px; }

@media (max-width: 1024px) {
  .content-grid { grid-template-columns: 1fr; }
}
</style>

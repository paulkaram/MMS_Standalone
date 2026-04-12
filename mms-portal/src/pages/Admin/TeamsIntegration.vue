<template>
  <div class="space-y-5">
    <!-- Page Header -->
    <PageHeader :title="$t('TeamsIntegration')" :subtitle="$t('TeamsIntegrationDescription')" :breadcrumbs="[{ label: 'Settings', to: '/settings' }]">
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
          <h3>{{ settings.enabled ? ($t('TeamsIntegrationEnabled')) : ($t('TeamsIntegrationDisabled')) }}</h3>
          <p>{{ settings.enabled
            ? ($t('TeamsIntegrationEnabledDesc'))
            : ($t('TeamsIntegrationDisabledDesc')) }}</p>
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
        <!-- Azure AD Settings -->
        <div class="section-card">
          <div class="section-header">
            <Icon icon="mdi:microsoft-azure" class="w-5 h-5 text-blue-500" />
            <h2>{{ $t('AzureAdSettings') }}</h2>
          </div>

          <div class="settings-form">
            <div class="form-group">
              <label>{{ $t('TenantId') }}</label>
              <input
                v-model="settings.tenantId"
                type="text"
                placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                @input="markChanged"
              />
              <small class="form-hint">{{ $t('TenantIdDesc') }}</small>
            </div>

            <div class="form-group">
              <label>{{ $t('ClientId') }}</label>
              <input
                v-model="settings.clientId"
                type="text"
                placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                @input="markChanged"
              />
              <small class="form-hint">{{ $t('ClientIdDesc') }}</small>
            </div>

            <div class="form-group">
              <label>{{ $t('ClientSecret') }}</label>
              <div class="password-input">
                <input
                  v-model="settings.clientSecret"
                  :type="showClientSecret ? 'text' : 'password'"
                  @input="markChanged"
                />
                <button type="button" @click="showClientSecret = !showClientSecret">
                  <Icon :icon="showClientSecret ? 'mdi:eye-off' : 'mdi:eye'" class="w-5 h-5" />
                </button>
              </div>
              <small class="form-hint">{{ $t('ClientSecretDesc') }}</small>
            </div>
          </div>
        </div>

        <!-- Organizer Settings -->
        <div class="section-card">
          <div class="section-header">
            <Icon icon="mdi:account-cog" class="w-5 h-5 text-purple-500" />
            <h2>{{ $t('OrganizerSettings') }}</h2>
          </div>

          <div class="settings-form">
            <div class="form-group">
              <label>{{ $t('OrganizerEmail') }}</label>
              <input
                v-model="settings.organizerEmail"
                type="email"
                placeholder="organizer@domain.com"
                @input="markChanged"
              />
              <small class="form-hint">{{ $t('OrganizerEmailDesc') }}</small>
            </div>

            <label class="opt-card">
              <div class="opt-info">
                <Icon icon="mdi:calendar-check" class="w-5 h-5 text-green-500 flex-shrink-0" />
                <div>
                  <span class="opt-title">{{ $t('AutoCreateOnApproval') }}</span>
                  <span class="opt-desc">{{ $t('AutoCreateOnApprovalDesc') }}</span>
                </div>
              </div>
              <input v-model="settings.autoCreateOnApproval" type="checkbox" class="sr-only peer" @change="markChanged" />
              <span class="opt-toggle"></span>
            </label>
          </div>
        </div>

        <!-- Required Permissions Info -->
        <div class="section-card info-card">
          <div class="section-header">
            <Icon icon="mdi:shield-check" class="w-5 h-5 text-amber-500" />
            <h2>{{ $t('RequiredPermissions') }}</h2>
          </div>

          <div class="permissions-info">
            <p>{{ $t('PermissionsInfo') }}</p>
            <ul>
              <li>
                <code>OnlineMeetings.ReadWrite.All</code>
                <span>{{ $t('OnlineMeetingsReadWriteAll') }}</span>
              </li>
              <li>
                <code>User.Read.All</code>
                <span>{{ $t('UserReadAll') }}</span>
              </li>
            </ul>
            <div class="permission-note">
              <Icon icon="mdi:information" class="w-4 h-4" />
              <span>{{ $t('AdminConsentRequired') }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Test Section -->
      <div class="test-section">
        <div class="section-card">
          <div class="section-header">
            <Icon icon="mdi:connection" class="w-5 h-5 text-green-500" />
            <h2>{{ $t('TestTeamsConnection') }}</h2>
          </div>

          <div class="test-content">
            <p class="test-description">
              {{ $t('TestTeamsConnectionDesc') }}
            </p>

            <Button
              variant="primary"
              icon-left="mdi:play"
              :loading="testing"
              :disabled="!canTest"
              class="test-button"
              @click="testConnection"
            >
              {{ $t('TestTeamsConnection') }}
            </Button>

            <!-- Test Result -->
            <div v-if="testResult" :class="['test-result', testResult.success ? 'success' : 'error']">
              <Icon :icon="testResult.success ? 'mdi:check-circle' : 'mdi:alert-circle'" class="w-5 h-5" />
              <div>
                <h4>{{ testResult.success ? ($t('ConnectionSuccessful')) : ($t('ConnectionFailed')) }}</h4>
                <p>{{ testResult.message }}</p>
              </div>
            </div>

            <!-- Setup Guide -->
            <div class="setup-guide">
              <h4><Icon icon="mdi:book-open-variant" class="w-4 h-4" /> {{ $t('SetupGuide') }}</h4>
              <ol>
                <li>{{ $t('SetupStep1') }}</li>
                <li>{{ $t('SetupStep2') }}</li>
                <li>{{ $t('SetupStep3') }}</li>
                <li>{{ $t('SetupStep4') }}</li>
                <li>{{ $t('SetupStep5') }}</li>
                <li>{{ $t('SetupStep6') }}</li>
              </ol>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import { useToast } from '@/composables/useToast'
import { mainApiAxios as axios } from '@/plugins/axios'

interface TeamsSettings {
  enabled: boolean
  tenantId: string
  clientId: string
  clientSecret: string
  organizerEmail: string
  autoCreateOnApproval: boolean
}

interface TestResult {
  success: boolean
  message: string
}

const { showToast } = useToast()

const loading = ref(false)
const saving = ref(false)
const testing = ref(false)
const hasChanges = ref(false)
const showClientSecret = ref(false)
const testResult = ref<TestResult | null>(null)

const settings = reactive<TeamsSettings>({
  enabled: false,
  tenantId: '',
  clientId: '',
  clientSecret: '',
  organizerEmail: '',
  autoCreateOnApproval: true
})

const canTest = computed(() => {
  return settings.tenantId && settings.clientId && settings.clientSecret
})

function markChanged() {
  hasChanges.value = true
  testResult.value = null
}

async function loadSettings() {
  try {
    loading.value = true
    const response = await axios.get('systemSettings')
    const allSettings = response.data || []

    // Find TeamsIntegration settings
    const teamsCategory = allSettings.find((c: any) => c.categoryName === 'TeamsIntegration')
    if (teamsCategory?.items) {
      for (const item of teamsCategory.items) {
        const key = item.name.replace('TeamsIntegration:', '')
        if (key === 'Enabled') settings.enabled = item.value === 'true'
        else if (key === 'TenantId') settings.tenantId = item.value || ''
        else if (key === 'ClientId') settings.clientId = item.value || ''
        else if (key === 'ClientSecret') settings.clientSecret = item.value || ''
        else if (key === 'OrganizerEmail') settings.organizerEmail = item.value || ''
        else if (key === 'AutoCreateOnApproval') settings.autoCreateOnApproval = item.value !== 'false'
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

    // Get current settings to preserve IDs
    const response = await axios.get('systemSettings')
    const allSettings = response.data || []
    const teamsCategory = allSettings.find((c: any) => c.categoryName === 'TeamsIntegration')

    const teamsItems = [
      { id: 0, name: 'TeamsIntegration:Enabled', value: settings.enabled.toString() },
      { id: 0, name: 'TeamsIntegration:TenantId', value: settings.tenantId },
      { id: 0, name: 'TeamsIntegration:ClientId', value: settings.clientId },
      { id: 0, name: 'TeamsIntegration:ClientSecret', value: settings.clientSecret },
      { id: 0, name: 'TeamsIntegration:OrganizerEmail', value: settings.organizerEmail },
      { id: 0, name: 'TeamsIntegration:AutoCreateOnApproval', value: settings.autoCreateOnApproval.toString() }
    ]

    // Get IDs from existing settings
    if (teamsCategory?.items) {
      for (const item of teamsItems) {
        const existing = teamsCategory.items.find((i: any) => i.name === item.name)
        if (existing) item.id = existing.id
      }
    }

    await axios.put('systemSettings/setting', {
      categoryName: 'TeamsIntegration',
      items: teamsItems.filter(i => i.id > 0)
    })

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

async function testConnection() {
  try {
    testing.value = true
    testResult.value = null

    // Test by getting an access token from Azure AD
    const tokenEndpoint = `https://login.microsoftonline.com/${settings.tenantId}/oauth2/v2.0/token`

    // Note: This should be done server-side for security
    // For now, we'll call a backend endpoint to test
    const response = await axios.post('systemSettings/test-teams-connection', {
      tenantId: settings.tenantId,
      clientId: settings.clientId,
      clientSecret: settings.clientSecret
    })

    testResult.value = {
      success: response.data?.success || response.success,
      message: response.data?.message || response.message || 'تم الاتصال بنجاح'
    }
  } catch (error: any) {
    testResult.value = {
      success: false,
      message: error.response?.data?.message || error.message || 'فشل في الاتصال بـ Microsoft Graph API'
    }
  } finally {
    testing.value = false
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
.section-card.info-card { background: #fffbeb; border-color: #fde68a; }
.section-header { display: flex; align-items: center; gap: 8px; padding: 14px 16px; border-bottom: 1px solid #e6eaef; }
.info-card .section-header { border-bottom-color: #fde68a; }
.section-header h2 { font-size: 14px; font-weight: 600; color: #004730; flex: 1; margin: 0; }

/* Form */
.settings-form { padding: 16px; display: flex; flex-direction: column; gap: 18px; }
.form-group { display: flex; flex-direction: column; gap: 6px; }
.form-group > label { font-size: 13px; font-weight: 500; color: #334155; }
.form-group input[type="text"], .form-group input[type="email"], .form-group input[type="password"] {
  width: 100%; padding: 8px 12px; border: 1px solid #e2e8f0; border-radius: 8px; font-size: 13px;
  outline: none; transition: border-color 0.15s;
}
.form-group input:focus { border-color: #006d4b; box-shadow: 0 0 0 3px rgba(0,174,140,0.1); }
.form-hint { font-size: 11px; color: #94a3b8; }
.password-input { position: relative; display: flex; align-items: center; }
.password-input input { flex: 1; padding-inline-start: 36px; }
.password-input button {
  position: absolute; inset-inline-start: 10px; top: 50%; transform: translateY(-50%);
  display: flex; color: #94a3b8; background: none; border: none; cursor: pointer;
}
.password-input button:hover { color: #475569; }

/* Modern Toggle Option Card */
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

/* Permissions Info */
.permissions-info { padding: 16px; }
.permissions-info p { font-size: 13px; color: #92400e; margin-bottom: 12px; }
.permissions-info ul { display: flex; flex-direction: column; gap: 8px; }
.permissions-info li { display: flex; align-items: center; gap: 8px; font-size: 13px; }
.permissions-info code { background: #fef3c7; padding: 2px 8px; border-radius: 4px; color: #78350f; font-family: monospace; font-size: 12px; }
.permissions-info li span { color: #92400e; }
.permission-note {
  display: flex; align-items: center; gap: 6px; margin-top: 12px; padding-top: 10px;
  border-top: 1px solid #fde68a; font-size: 12px; color: #92400e;
}

/* Test Section */
.test-content { padding: 16px; display: flex; flex-direction: column; gap: 14px; }
.test-description { font-size: 13px; color: #64748b; }
.test-button { width: 100%; }
.test-result { display: flex; align-items: flex-start; gap: 10px; padding: 12px; border-radius: 8px; }
.test-result.success { background: #e6f9f5; color: #007E65; }
.test-result.error { background: #fef2f2; color: #dc2626; }
.test-result h4 { font-weight: 600; margin: 0; }
.test-result p { font-size: 12px; opacity: 0.8; margin: 2px 0 0; }

/* Setup Guide */
.setup-guide { margin-top: 14px; padding-top: 14px; border-top: 1px solid #e6eaef; }
.setup-guide h4 { display: flex; align-items: center; gap: 4px; font-size: 13px; font-weight: 500; color: #334155; margin: 0 0 10px; }
.setup-guide ol { font-size: 12px; color: #64748b; list-style: decimal; padding-inline-start: 18px; margin: 0; }
.setup-guide li { margin-bottom: 6px; }
.setup-guide a { color: #006d4b; text-decoration: none; }
.setup-guide a:hover { text-decoration: underline; }

@media (max-width: 1024px) {
  .content-grid { grid-template-columns: 1fr; }
}
</style>

<template>
  <div class="space-y-5">
    <!-- Page Header -->
    <PageHeader :title="$t('AppSettings')" :subtitle="$t('ManageAppSettings')" :breadcrumbs="[{ label: 'Settings', to: '/settings' }]">
      <template #actions>
        <button class="btn-clean secondary" :disabled="reloading" @click="reloadSettings">
          <Icon icon="mdi:refresh" class="w-4 h-4" />
          {{ $t('ReloadSettings') }}
        </button>
      </template>
    </PageHeader>

    <!-- Stats Cards -->
    <div class="stats-row">
      <div class="stat-card">
        <div class="stat-icon total">
          <Icon icon="mdi:cog" class="w-5 h-5" />
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ totalSettings }}</span>
          <span class="stat-label">{{ $t('TotalSettings') }}</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon categories">
          <Icon icon="mdi:folder-multiple" class="w-5 h-5" />
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ settings.length }}</span>
          <span class="stat-label">{{ $t('Categories') }}</span>
        </div>
      </div>
    </div>

    <!-- Category Filter Tabs -->
    <div class="category-tabs">
      <button
        :class="['tab-btn', { active: selectedCategory === null }]"
        @click="selectedCategory = null"
      >
        <Icon icon="mdi:view-grid" class="w-4 h-4" />
        {{ $t('AllCategories') }}
        <span class="tab-count">{{ totalSettings }}</span>
      </button>
      <button
        v-for="category in settings"
        :key="category.categoryName"
        :class="['tab-btn', { active: selectedCategory === category.categoryName }]"
        @click="selectedCategory = category.categoryName"
      >
        <Icon :icon="getCategoryIcon(category.categoryName)" class="w-4 h-4" />
        {{ getCategoryDisplayName(category.categoryName) }}
        <span class="tab-count">{{ category.items?.length || 0 }}</span>
      </button>
    </div>

    <!-- Settings Cards -->
    <div class="settings-container" :class="{ 'is-loading': loading }">
      <div v-if="loading" class="loading-overlay">
        <div class="loader">
          <div class="loader-ring"></div>
          <Icon icon="mdi:cog" class="loader-icon" />
        </div>
      </div>

      <div v-for="category in filteredSettings" :key="category.categoryName" class="category-card">
        <div class="category-header">
          <div class="category-title">
            <div :class="['category-icon', getCategoryColor(category.categoryName)]">
              <Icon :icon="getCategoryIcon(category.categoryName)" class="w-5 h-5" />
            </div>
            <div>
              <h3>{{ getCategoryDisplayName(category.categoryName) }}</h3>
              <p class="text-xs text-zinc-400">{{ category.items?.length || 0 }} {{ $t('Settings') }}</p>
            </div>
          </div>
          <div class="category-actions">
            <button
              v-if="editingCategory !== category.categoryName"
              class="edit-btn"
              @click="startEditing(category)"
            >
              <Icon icon="mdi:pencil" class="w-4 h-4" />
              {{ $t('Edit') }}
            </button>
            <template v-else>
              <Button
                variant="primary"
                size="sm"
                icon-left="mdi:check"
                :loading="saving"
                @click="saveCategory(category)"
              >
                {{ $t('Save') }}
              </Button>
              <Button
                variant="ghost"
                size="sm"
                icon-left="mdi:close"
                @click="cancelEditing"
              >
                {{ $t('Cancel') }}
              </Button>
            </template>
          </div>
        </div>

        <div class="settings-grid">
          <div
            v-for="item in category.items"
            :key="item.id"
            class="setting-card"
          >
            <div class="setting-card-header">
              <div class="setting-name-wrapper">
                <span :class="['category-dot', getCategoryDotColor(category.categoryName)]"></span>
                <label :for="`setting-${item.id}`" class="setting-name">
                  {{ getSettingDisplayName(item.name) }}
                </label>
              </div>
              <div v-if="isBooleanSetting(item.name) && editingCategory !== category.categoryName" :class="['status-dot', item.value === 'true' ? 'active' : '']"></div>
            </div>
            <div class="setting-card-body">
              <template v-if="editingCategory === category.categoryName">
                <!-- Boolean settings -->
                <div v-if="isBooleanSetting(item.name)" class="toggle-wrapper">
                  <label class="toggle-switch">
                    <input
                      type="checkbox"
                      :checked="editedValues[item.id] === 'true'"
                      @change="editedValues[item.id] = ($event.target as HTMLInputElement).checked ? 'true' : 'false'"
                    />
                    <span class="toggle-slider"></span>
                  </label>
                  <span class="toggle-label">{{ editedValues[item.id] === 'true' ? ($t('Enabled')) : ($t('Disabled')) }}</span>
                </div>
                <!-- Password fields -->
                <div v-else-if="isPasswordSetting(item.name)" class="input-wrapper">
                  <input
                    :id="`setting-${item.id}`"
                    v-model="editedValues[item.id]"
                    :type="showPassword[item.id] ? 'text' : 'password'"
                    class="setting-input"
                    :placeholder="$t('EnterValue')"
                  />
                  <button
                    type="button"
                    class="input-icon"
                    @click="showPassword[item.id] = !showPassword[item.id]"
                  >
                    <Icon :icon="showPassword[item.id] ? 'mdi:eye-off' : 'mdi:eye'" class="w-4 h-4" />
                  </button>
                </div>
                <!-- Number settings -->
                <div v-else-if="isNumberSetting(item.name)" class="input-wrapper">
                  <input
                    :id="`setting-${item.id}`"
                    v-model="editedValues[item.id]"
                    type="number"
                    class="setting-input"
                    :placeholder="$t('EnterValue')"
                  />
                  <Icon icon="mdi:numeric" class="input-icon-static w-4 h-4" />
                </div>
                <!-- Text settings -->
                <div v-else class="input-wrapper">
                  <input
                    :id="`setting-${item.id}`"
                    v-model="editedValues[item.id]"
                    type="text"
                    class="setting-input"
                    :placeholder="$t('EnterValue')"
                  />
                  <Icon icon="mdi:text" class="input-icon-static w-4 h-4" />
                </div>
              </template>
              <template v-else>
                <div v-if="isPasswordSetting(item.name)" class="value-display password">
                  <Icon icon="mdi:lock" class="w-4 h-4 text-zinc-400" />
                  <span>{{ item.value ? '••••••••' : '-' }}</span>
                </div>
                <div v-else-if="isBooleanSetting(item.name)" class="value-display">
                  <span :class="['status-badge', item.value === 'true' ? 'enabled' : 'disabled']">
                    {{ item.value === 'true' ? ($t('Enabled')) : ($t('Disabled')) }}
                  </span>
                </div>
                <div v-else class="value-display text">
                  <span>{{ item.value || '-' }}</span>
                </div>
              </template>
            </div>
          </div>
        </div>
      </div>

      <div v-if="!loading && filteredSettings.length === 0" class="empty-state">
        <Icon icon="mdi:cog-off" class="w-16 h-16 text-zinc-300" />
        <p class="text-zinc-500 mt-4">{{ $t('NoSettingsFound') }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import { useToast } from '@/composables/useToast'
import SettingsService from '@/services/SettingsService'

const { t } = useI18n()

interface SettingItem {
  id: number
  name: string
  value: string | null
  description?: string
}

interface SettingsCategory {
  categoryName: string
  items: SettingItem[]
}

const { showToast } = useToast()

const loading = ref(false)
const saving = ref(false)
const reloading = ref(false)
const settings = ref<SettingsCategory[]>([])
const selectedCategory = ref<string | null>(null)
const editingCategory = ref<string | null>(null)
const editedValues = ref<Record<number, string>>({})
const showPassword = ref<Record<number, boolean>>({})

// Category display names — use translation keys
const categoryDisplayKeyMap: Record<string, string> = {
  'LDAP': 'CategoryLDAP',
  'SMTP': 'CategorySMTP',
  'SMS': 'CategorySMS',
  'JWT': 'CategoryJWT',
  'STORAGE': 'CategoryStorage',
  'APP': 'CategoryApp',
  'CONNECTIONS': 'CategoryConnections',
  'Encryption': 'CategoryEncryption',
  'MoiJWT': 'CategoryMoiJWT',
  'Oidc': 'CategoryOidc',
  'OutlookIntegration': 'CategoryOutlook',
  'PINCODE': 'CategoryPincode',
  'Redis': 'CategoryRedis',
  'MMS': 'CategoryMMS',
  'Chat': 'CategoryChat',
  'AuditTrail': 'CategoryAuditTrail',
  'Viewer': 'CategoryViewer',
  'TeamsIntegration': 'CategoryTeams',
  'NULL': 'CategoryOther'
}

// Category icons mapping
const categoryIcons: Record<string, string> = {
  'LDAP': 'mdi:folder-network',
  'SMTP': 'mdi:email-outline',
  'SMS': 'mdi:message-text',
  'JWT': 'mdi:key',
  'STORAGE': 'mdi:database',
  'APP': 'mdi:application',
  'CONNECTIONS': 'mdi:connection',
  'Encryption': 'mdi:shield-lock',
  'MoiJWT': 'mdi:key-variant',
  'Oidc': 'mdi:login',
  'OutlookIntegration': 'mdi:microsoft-outlook',
  'PINCODE': 'mdi:numeric',
  'Redis': 'mdi:memory',
  'MMS': 'mdi:calendar-check',
  'Chat': 'mdi:chat',
  'AuditTrail': 'mdi:history',
  'Viewer': 'mdi:file-eye',
  'TeamsIntegration': 'mdi:microsoft-teams',
  'NULL': 'mdi:cog'
}

// Category colors mapping
const categoryColors: Record<string, string> = {
  'LDAP': 'bg-blue',
  'SMTP': 'bg-emerald',
  'SMS': 'bg-purple',
  'JWT': 'bg-amber',
  'STORAGE': 'bg-cyan',
  'APP': 'bg-indigo',
  'CONNECTIONS': 'bg-teal',
  'Encryption': 'bg-red',
  'MoiJWT': 'bg-orange',
  'Oidc': 'bg-violet',
  'OutlookIntegration': 'bg-sky',
  'PINCODE': 'bg-lime',
  'Redis': 'bg-rose',
  'MMS': 'bg-indigo',
  'Chat': 'bg-emerald',
  'AuditTrail': 'bg-amber',
  'Viewer': 'bg-cyan',
  'TeamsIntegration': 'bg-purple',
  'NULL': 'bg-slate'
}

const totalSettings = computed(() => {
  return settings.value.reduce((total, cat) => total + (cat.items?.length || 0), 0)
})

const filteredSettings = computed(() => {
  if (!selectedCategory.value) {
    return settings.value
  }
  return settings.value.filter(s => s.categoryName === selectedCategory.value)
})

function getCategoryDisplayName(category: string | null): string {
  const key = categoryDisplayKeyMap[category || 'NULL'] || category || 'CategoryOther'
  const translated = t(key)
  return translated !== key ? translated : (category || 'Other')
}

function getCategoryIcon(category: string | null): string {
  if (!category) return categoryIcons['NULL']
  return categoryIcons[category] || 'mdi:cog'
}

function getCategoryColor(category: string | null): string {
  if (!category) return categoryColors['NULL']
  return categoryColors[category] || 'bg-slate'
}

function getCategoryDotColor(category: string | null): string {
  if (!category) return 'dot-slate'
  const colorMap: Record<string, string> = {
    'LDAP': 'dot-blue',
    'SMTP': 'dot-emerald',
    'SMS': 'dot-purple',
    'JWT': 'dot-amber',
    'STORAGE': 'dot-cyan',
    'APP': 'dot-indigo',
    'CONNECTIONS': 'dot-teal',
    'Encryption': 'dot-red',
    'MoiJWT': 'dot-orange',
    'Oidc': 'dot-violet',
    'OutlookIntegration': 'dot-sky',
    'PINCODE': 'dot-lime',
    'Redis': 'dot-rose',
    'MMS': 'dot-indigo',
    'Chat': 'dot-emerald',
    'AuditTrail': 'dot-amber',
    'Viewer': 'dot-cyan',
    'TeamsIntegration': 'dot-purple',
    'NULL': 'dot-slate'
  }
  return colorMap[category] || 'dot-slate'
}

function getSettingDisplayName(name: string): string {
  // Remove category prefix (e.g., "Ldap:Path" -> "Path")
  const parts = name.split(':')
  if (parts.length > 1) {
    return parts.slice(1).join(':')
  }
  return name
}

function isBooleanSetting(name: string): boolean {
  const lowerName = name.toLowerCase()
  return lowerName.includes('enabled') ||
         lowerName.includes('enable') ||
         lowerName.includes('ssl') ||
         lowerName.includes('active') ||
         lowerName.includes('httpsonly')
}

function isPasswordSetting(name: string): boolean {
  const lowerName = name.toLowerCase()
  return lowerName.includes('password') ||
         lowerName.includes('secret') ||
         lowerName.includes('key') ||
         lowerName.includes('bearer') ||
         lowerName.includes('clientsecret')
}

function isNumberSetting(name: string): boolean {
  const lowerName = name.toLowerCase()
  return lowerName.includes('port') ||
         lowerName.includes('minutes') ||
         lowerName.includes('timeout') ||
         lowerName.includes('expiry') ||
         lowerName.includes('count') ||
         lowerName.includes('max') ||
         lowerName.includes('size')
}

function startEditing(category: SettingsCategory) {
  editingCategory.value = category.categoryName
  editedValues.value = {}
  category.items?.forEach(item => {
    editedValues.value[item.id] = item.value || ''
  })
}

function cancelEditing() {
  editingCategory.value = null
  editedValues.value = {}
}

async function saveCategory(category: SettingsCategory) {
  try {
    saving.value = true

    const items = category.items?.map(item => ({
      id: item.id,
      name: item.name,
      value: editedValues.value[item.id] || null
    })) || []

    await SettingsService.updateAppSetting({
      categoryName: category.categoryName,
      items
    })

    // Update local state
    category.items?.forEach(item => {
      item.value = editedValues.value[item.id] || null
    })

    editingCategory.value = null
    editedValues.value = {}

    showToast({
      type: 'success',
      message: 'تم حفظ الإعدادات بنجاح'
    })
  } catch (error) {
    console.error('Error saving settings:', error)
    showToast({
      type: 'error',
      message: 'فشل في حفظ الإعدادات'
    })
  } finally {
    saving.value = false
  }
}

async function loadSettings() {
  try {
    loading.value = true
    const response = await SettingsService.listAppSettings()
    settings.value = (response as any)?.data || response || []

    // Sort categories alphabetically, but put null/empty at the end
    settings.value.sort((a, b) => {
      if (!a.categoryName) return 1
      if (!b.categoryName) return -1
      return a.categoryName.localeCompare(b.categoryName)
    })
  } catch (error) {
    console.error('Error loading settings:', error)
    showToast({
      type: 'error',
      message: 'فشل في تحميل الإعدادات'
    })
  } finally {
    loading.value = false
  }
}

async function reloadSettings() {
  try {
    reloading.value = true
    await SettingsService.reloadSystemSettings()
    await loadSettings()
    showToast({
      type: 'success',
      message: 'تم إعادة تحميل الإعدادات بنجاح'
    })
  } catch (error) {
    console.error('Error reloading settings:', error)
    showToast({
      type: 'error',
      message: 'فشل في إعادة تحميل الإعدادات'
    })
  } finally {
    reloading.value = false
  }
}

onMounted(() => {
  loadSettings()
})
</script>

<style scoped>
/* removed max-width constraint */

/* Stats Row */
.stats-row {
  @apply grid grid-cols-2 md:grid-cols-4 gap-4 mb-6;
}

.stat-card {
  @apply bg-white rounded-xl p-4 flex items-center gap-3;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  border: 1px solid rgba(0, 0, 0, 0.04);
}

.stat-icon {
  @apply w-10 h-10 rounded-lg flex items-center justify-center text-white;
}

.stat-icon.total {
  background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%);
}

.stat-icon.categories {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
}

.stat-info {
  @apply flex flex-col;
}

.stat-value {
  @apply text-xl font-bold text-zinc-800;
}

.stat-label {
  @apply text-xs text-zinc-500;
}

/* Category Tabs */
.category-tabs {
  @apply flex flex-wrap gap-2 mb-6 bg-white rounded-xl p-3;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  border: 1px solid rgba(0, 0, 0, 0.04);
}

.tab-btn {
  @apply flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium text-zinc-600 transition-all;
}

.tab-btn:hover {
  @apply bg-zinc-100;
}

.tab-btn.active {
  @apply bg-indigo-50 text-indigo-600;
}

.tab-count {
  @apply text-xs px-1.5 py-0.5 rounded-full bg-zinc-100 text-zinc-500;
}

.tab-btn.active .tab-count {
  @apply bg-indigo-100 text-indigo-600;
}

/* Settings Container */
.settings-container {
  @apply relative space-y-4;
}

.settings-container.is-loading {
  @apply min-h-[300px];
}

.loading-overlay {
  @apply absolute inset-0 bg-white/80 backdrop-blur-sm flex items-center justify-center z-10 rounded-xl;
}

.loader {
  @apply relative w-16 h-16 flex items-center justify-center;
}

.loader-ring {
  @apply absolute inset-0 border-4 border-indigo-100 border-t-indigo-500 rounded-full;
  animation: spin 1s linear infinite;
}

.loader-icon {
  @apply w-6 h-6 text-indigo-500;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Category Card */
.category-card {
  @apply bg-white rounded-xl overflow-hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  border: 1px solid rgba(0, 0, 0, 0.04);
}

.category-header {
  @apply flex items-center justify-between p-4 border-b border-zinc-100;
}

.category-title {
  @apply flex items-center gap-3;
}

.category-title h3 {
  @apply text-base font-semibold text-zinc-800;
}

.category-icon {
  @apply w-10 h-10 rounded-lg flex items-center justify-center text-white;
}

.category-icon.bg-blue { background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%); }
.category-icon.bg-emerald { background: linear-gradient(135deg, #10b981 0%, #059669 100%); }
.category-icon.bg-purple { background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%); }
.category-icon.bg-amber { background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); }
.category-icon.bg-cyan { background: linear-gradient(135deg, #06b6d4 0%, #0891b2 100%); }
.category-icon.bg-indigo { background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%); }
.category-icon.bg-pink { background: linear-gradient(135deg, #ec4899 0%, #db2777 100%); }
.category-icon.bg-teal { background: linear-gradient(135deg, #14b8a6 0%, #0d9488 100%); }
.category-icon.bg-red { background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%); }
.category-icon.bg-orange { background: linear-gradient(135deg, #f97316 0%, #ea580c 100%); }
.category-icon.bg-violet { background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%); }
.category-icon.bg-sky { background: linear-gradient(135deg, #0ea5e9 0%, #0284c7 100%); }
.category-icon.bg-lime { background: linear-gradient(135deg, #84cc16 0%, #65a30d 100%); }
.category-icon.bg-rose { background: linear-gradient(135deg, #f43f5e 0%, #e11d48 100%); }
.category-icon.bg-slate { background: linear-gradient(135deg, #64748b 0%, #475569 100%); }

.category-actions {
  @apply flex items-center gap-2;
}

.edit-btn {
  @apply flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium;
  @apply bg-indigo-50 text-indigo-600 border border-indigo-100;
  @apply hover:bg-indigo-100 hover:border-indigo-200;
  @apply transition-all duration-200;
}

.edit-btn:hover {
  @apply shadow-sm;
}

/* Settings Grid - 2 Column Layout */
.settings-grid {
  @apply grid grid-cols-1 md:grid-cols-2 gap-3 p-4;
}

.setting-card {
  @apply bg-zinc-50 rounded-xl p-4 border border-zinc-100 transition-all;
}

.setting-card:hover {
  @apply bg-zinc-100/70 border-zinc-200;
}

.setting-card-header {
  @apply flex items-center justify-between mb-3;
}

.setting-name-wrapper {
  @apply flex items-center gap-2;
}

.category-dot {
  @apply w-2.5 h-2.5 rounded-full flex-shrink-0;
  box-shadow: 0 0 0 2px rgba(255, 255, 255, 0.8);
}

.category-dot.dot-blue { background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%); }
.category-dot.dot-emerald { background: linear-gradient(135deg, #10b981 0%, #059669 100%); }
.category-dot.dot-purple { background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%); }
.category-dot.dot-amber { background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); }
.category-dot.dot-cyan { background: linear-gradient(135deg, #06b6d4 0%, #0891b2 100%); }
.category-dot.dot-indigo { background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%); }
.category-dot.dot-pink { background: linear-gradient(135deg, #ec4899 0%, #db2777 100%); }
.category-dot.dot-teal { background: linear-gradient(135deg, #14b8a6 0%, #0d9488 100%); }
.category-dot.dot-red { background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%); }
.category-dot.dot-orange { background: linear-gradient(135deg, #f97316 0%, #ea580c 100%); }
.category-dot.dot-violet { background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%); }
.category-dot.dot-sky { background: linear-gradient(135deg, #0ea5e9 0%, #0284c7 100%); }
.category-dot.dot-lime { background: linear-gradient(135deg, #84cc16 0%, #65a30d 100%); }
.category-dot.dot-rose { background: linear-gradient(135deg, #f43f5e 0%, #e11d48 100%); }
.category-dot.dot-slate { background: linear-gradient(135deg, #64748b 0%, #475569 100%); }

.setting-name {
  @apply text-sm font-semibold text-zinc-700;
}

.status-dot {
  @apply w-2.5 h-2.5 rounded-full bg-zinc-300;
}

.status-dot.active {
  @apply bg-green-500;
}

.setting-card-body {
  @apply min-h-[40px];
}

/* Value Display Styles */
.value-display {
  @apply flex items-center gap-2 text-sm min-h-[40px];
}

.value-display.password {
  @apply text-zinc-400 bg-zinc-100 rounded-lg px-3 py-2;
}

.value-display.password span {
  @apply tracking-wider;
}

.value-display.text {
  @apply bg-zinc-100 rounded-lg px-3 py-2;
}

.value-display.text span {
  @apply text-zinc-600 break-all;
}

.status-badge {
  @apply px-3 py-1 rounded-full text-xs font-medium;
}

.status-badge.enabled {
  @apply bg-green-100 text-green-700;
}

.status-badge.disabled {
  @apply bg-zinc-200 text-zinc-500;
}

/* Input Wrapper */
.input-wrapper {
  @apply relative;
}

.setting-input {
  @apply w-full px-3 py-2.5 text-sm bg-white border border-zinc-200 rounded-lg;
  @apply focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent;
  @apply placeholder:text-zinc-400;
  padding-left: 2.5rem;
}

[dir="rtl"] .setting-input {
  padding-left: 0.75rem;
  padding-right: 2.5rem;
}

.input-icon {
  @apply absolute top-1/2 -translate-y-1/2 text-zinc-400 hover:text-zinc-600 cursor-pointer z-10;
  left: 0.75rem;
}

[dir="rtl"] .input-icon {
  left: auto;
  right: 0.75rem;
}

.input-icon-static {
  @apply absolute top-1/2 -translate-y-1/2 text-zinc-300 pointer-events-none;
  left: 0.75rem;
}

[dir="rtl"] .input-icon-static {
  left: auto;
  right: 0.75rem;
}

/* Toggle Switch */
.toggle-wrapper {
  @apply flex items-center gap-3;
}

.toggle-switch {
  @apply relative inline-flex items-center cursor-pointer;
}

.toggle-switch input {
  @apply sr-only;
}

.toggle-slider {
  @apply w-11 h-6 bg-zinc-300 rounded-full transition-colors relative;
}

.toggle-slider::before {
  content: '';
  @apply absolute w-5 h-5 bg-white rounded-full shadow transition-transform;
  top: 2px;
  left: 2px;
}

.toggle-switch input:checked + .toggle-slider {
  @apply bg-green-500;
}

.toggle-switch input:checked + .toggle-slider::before {
  transform: translateX(20px);
}

[dir="rtl"] .toggle-slider::before {
  left: auto;
  right: 2px;
}

[dir="rtl"] .toggle-switch input:checked + .toggle-slider::before {
  transform: translateX(-20px);
}

.toggle-label {
  @apply text-sm text-zinc-600;
}

/* Empty State */
.empty-state {
  @apply flex flex-col items-center justify-center py-16 bg-white rounded-xl;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  border: 1px solid rgba(0, 0, 0, 0.04);
}

/* Responsive */
@media (max-width: 768px) {
  .settings-grid {
    @apply grid-cols-1;
  }

  .category-tabs {
    @apply overflow-x-auto -mx-4 px-4;
    scrollbar-width: none;
  }

  .category-tabs::-webkit-scrollbar {
    display: none;
  }
}

@media (min-width: 1280px) {
  .settings-grid {
    @apply grid-cols-3;
  }
}
</style>

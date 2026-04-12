<template>
  <div class="councils-page">
    <!-- Page Header -->
    <PageHeader :title="$t('CouncilsAndCommittees')" :subtitle="$t('ManageCouncilsAndCommittees')" :breadcrumbs="[{ label: 'Settings', to: '/settings' }]">
      <template #actions>
        <button
          v-if="isFullAdmin"
          class="btn-clean primary"
          @click="openAddCouncilCommittee"
        >
          <Icon icon="mdi:plus" class="w-4 h-4" />
          {{ $t('AddNew') }}
        </button>
      </template>
    </PageHeader>

    <!-- Main Layout -->
    <div v-if="permissionsLoaded" class="main-layout">
      <!-- Left Sidebar - Tree Navigation -->
      <OrganizationTree
        ref="organizationTreeRef"
        :selected-id="selectedId"
        :use-my-organization="!isFullAdmin && isCommitteeAdmin"
        @select="handleNodeSelect"
      />

      <!-- Right Content Panel -->
      <main class="content-panel">
        <template v-if="selectedItem">
          <!-- Selected Item Header Card -->
          <div class="selected-header">
            <div class="selected-info">
              <div class="selected-icon" :class="selectedItem.typeId === 1 ? 'council' : 'committee'">
                <Icon :icon="selectedItem.typeId === 1 ? 'mdi:domain' : 'mdi:account-multiple'" class="w-6 h-6" />
              </div>
              <div class="selected-details">
                <span class="selected-type">
                  {{ selectedItem.typeId === 1 ? ($t('Council')) : ($t('Committee')) }}
                </span>
                <h2 class="selected-name">{{ selectedItem.name }}</h2>
              </div>
            </div>
            <div v-if="isFullAdmin" class="selected-actions">
              <label class="toggle-switch">
                <input
                  type="checkbox"
                  v-model="hasFinancialCompensation"
                  class="toggle-input"
                  @change="saveFinancialCompensation"
                />
                <span class="toggle-slider"></span>
                <span class="toggle-text">
                  <Icon icon="mdi:cash" class="w-4 h-4" />
                  {{ $t('FinancialCompensation') }}
                </span>
              </label>
              <Button
                variant="outline"
                size="sm"
                icon-left="mdi:pencil"
                @click="openEditCouncilCommittee"
              >
                {{ $t('Edit') }}
              </Button>
            </div>
          </div>

          <!-- Stats Cards -->
          <div class="stats-grid">
            <div class="dark-stat-card-sm" :class="{ active: activeTab === 0 }" @click="activeTab = 0">
              <svg class="stat-wave" viewBox="0 0 200 60" preserveAspectRatio="none">
                <path d="M0 35 C40 20, 80 45, 120 30 S200 40 200 40 L200 60 L0 60Z" fill="rgba(0,174,140,0.18)" />
                <path d="M0 42 C50 30, 90 50, 140 38 S200 48 200 48 L200 60 L0 60Z" fill="rgba(0,174,140,0.10)" />
              </svg>
              <div class="stat-top">
                <Icon icon="mdi:account-group" class="w-4 h-4 stat-icon-sm" />
                <span class="stat-label-sm">{{ $t('Members') }}</span>
              </div>
              <span class="stat-value-sm">{{ membersCount }}</span>
            </div>
            <div class="dark-stat-card-sm" :class="{ active: activeTab === 1 }" @click="activeTab = 1">
              <svg class="stat-wave" viewBox="0 0 200 60" preserveAspectRatio="none">
                <path d="M0 35 C40 20, 80 45, 120 30 S200 40 200 40 L200 60 L0 60Z" fill="rgba(0,174,140,0.18)" />
                <path d="M0 42 C50 30, 90 50, 140 38 S200 48 200 48 L200 60 L0 60Z" fill="rgba(0,174,140,0.10)" />
              </svg>
              <div class="stat-top">
                <Icon icon="mdi:clipboard-list" class="w-4 h-4 stat-icon-sm" />
                <span class="stat-label-sm">{{ $t('Tasks') }}</span>
              </div>
              <span class="stat-value-sm">{{ tasksCount }}</span>
            </div>
            <div class="dark-stat-card-sm" :class="{ active: activeTab === 3 }" @click="activeTab = 3">
              <svg class="stat-wave" viewBox="0 0 200 60" preserveAspectRatio="none">
                <path d="M0 35 C40 20, 80 45, 120 30 S200 40 200 40 L200 60 L0 60Z" fill="rgba(0,174,140,0.18)" />
                <path d="M0 42 C50 30, 90 50, 140 38 S200 48 200 48 L200 60 L0 60Z" fill="rgba(0,174,140,0.10)" />
              </svg>
              <div class="stat-top">
                <Icon icon="mdi:calendar-check" class="w-4 h-4 stat-icon-sm" />
                <span class="stat-label-sm">{{ $t('Meetings') }}</span>
              </div>
              <span class="stat-value-sm">{{ meetingsCount }}</span>
            </div>
            <div class="dark-stat-card-sm" :class="{ active: activeTab === 4 }" @click="activeTab = 4">
              <svg class="stat-wave" viewBox="0 0 200 60" preserveAspectRatio="none">
                <path d="M0 35 C40 20, 80 45, 120 30 S200 40 200 40 L200 60 L0 60Z" fill="rgba(0,174,140,0.18)" />
                <path d="M0 42 C50 30, 90 50, 140 38 S200 48 200 48 L200 60 L0 60Z" fill="rgba(0,174,140,0.10)" />
              </svg>
              <div class="stat-top">
                <Icon icon="mdi:paperclip" class="w-4 h-4 stat-icon-sm" />
                <span class="stat-label-sm">{{ $t('Attachments') }}</span>
              </div>
              <span class="stat-value-sm">{{ attachmentsCount }}</span>
            </div>
          </div>

          <!-- Tabs Content -->
          <div class="tabs-container">
            <!-- Tab Headers -->
            <div class="tabs-header">
              <button
                v-for="(tab, index) in tabs"
                :key="tab.key"
                :class="['tab-btn', { active: activeTab === index }]"
                @click="activeTab = index"
              >
                <Icon :icon="tab.icon" class="w-5 h-5" />
                <span>{{ tab.label }}</span>
              </button>
            </div>

            <!-- Tab Content -->
            <div class="tab-content">
              <!-- Members Tab -->
              <MembersTab
                v-show="activeTab === 0"
                ref="membersTabRef"
                :committee-id="selectedId"
                @update:count="membersCount = $event"
              />

              <!-- Tasks Tab -->
              <TasksTab
                v-show="activeTab === 1"
                ref="tasksTabRef"
                :committee-id="selectedId"
                @update:count="tasksCount = $event"
              />

              <!-- Activities Tab -->
              <ActivitiesTab
                v-show="activeTab === 2"
                ref="activitiesTabRef"
                :committee-id="selectedId"
              />

              <!-- Meetings Tab -->
              <MeetingsTab
                v-show="activeTab === 3"
                ref="meetingsTabRef"
                :committee-id="selectedId"
                @update:count="meetingsCount = $event"
              />

              <!-- Attachments Tab -->
              <AttachmentsTab
                v-show="activeTab === 4"
                ref="attachmentsTabRef"
                :committee-id="selectedId"
                @update:count="attachmentsCount = $event"
              />
            </div>
          </div>
        </template>

        <!-- Empty State -->
        <div v-else class="content-empty">
          <div class="empty-icon-ring">
            <div class="empty-icon-inner">
              <svg class="w-7 h-7" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
                <path d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-4 0a1 1 0 01-1-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 01-1 1h-2"/>
              </svg>
            </div>
          </div>
          <h3 class="empty-title">{{ $t('SelectItem') }}</h3>
          <p class="empty-desc">{{ $t('SelectItemDesc') }}</p>
        </div>
      </main>
    </div>

    <!-- Council/Committee Form Dialog -->
    <CouncilCommitteeForm
      v-if="councilCommitteeDialog"
      v-model="councilCommitteeDialog"
      :council-committee-id="selectedId"
      :is-edit="isEditCouncilCommittee"
      @saved="handleCouncilCommitteeSaved"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import OrganizationTree from '@/components/app/admin/councils/OrganizationTree.vue'
import MembersTab from '@/components/app/admin/councils/MembersTab.vue'
import TasksTab from '@/components/app/admin/councils/TasksTab.vue'
import ActivitiesTab from '@/components/app/admin/councils/ActivitiesTab.vue'
import MeetingsTab from '@/components/app/admin/councils/MeetingsTab.vue'
import AttachmentsTab from '@/components/app/admin/councils/AttachmentsTab.vue'
import CouncilCommitteeForm from '@/components/app/admin/CouncilCommitteeForm.vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'
import UsersService from '@/services/UsersService'

// Types
interface TreeItem {
  id: string
  name: string
  typeId: number
  children?: TreeItem[]
}

const { t } = useI18n()

// Component refs
const organizationTreeRef = ref<InstanceType<typeof OrganizationTree> | null>(null)
const membersTabRef = ref<InstanceType<typeof MembersTab> | null>(null)
const tasksTabRef = ref<InstanceType<typeof TasksTab> | null>(null)
const activitiesTabRef = ref<InstanceType<typeof ActivitiesTab> | null>(null)
const meetingsTabRef = ref<InstanceType<typeof MeetingsTab> | null>(null)
const attachmentsTabRef = ref<InstanceType<typeof AttachmentsTab> | null>(null)

// State
const selectedId = ref<string | null>(null)
const selectedItem = ref<TreeItem | null>(null)
const hasFinancialCompensation = ref(false)
const activeTab = ref(0)

// Counts for stats
const membersCount = ref(0)
const tasksCount = ref(0)
const meetingsCount = ref(0)
const attachmentsCount = ref(0)

// Council/Committee form
const councilCommitteeDialog = ref(false)
const isEditCouncilCommittee = ref(false)

// Admin type detection
const isFullAdmin = ref(false)
const isCommitteeAdmin = ref(false)
const permissionsLoaded = ref(false)

onMounted(async () => {
  try {
    const [permissionsResponse, adminCommitteesResponse] = await Promise.all([
      UsersService.listCurrentUserPermissions(),
      CouncilCommitteesService.getMyAdminCommittees()
    ])

    const permissions = (permissionsResponse as any)?.data ?? permissionsResponse
    const adminData = (adminCommitteesResponse as any)?.data ?? adminCommitteesResponse
    const committees = adminData?.committeeIds ?? adminData

    // Full admin has CouncilsAndCommittees permission
    isFullAdmin.value = Array.isArray(permissions) &&
      permissions.some((p: any) => p.name === 'CouncilsAndCommittees' && p.hasAccess)

    isCommitteeAdmin.value = Array.isArray(committees) && committees.length > 0
  } catch {
    isFullAdmin.value = false
    isCommitteeAdmin.value = false
  } finally {
    permissionsLoaded.value = true
  }
})

// Tabs
const tabs = computed(() => [
  { key: 'users', label: t('Members'), icon: 'mdi:account-group' },
  { key: 'duties', label: t('Tasks'), icon: 'mdi:clipboard-list' },
  { key: 'activities', label: t('Activities'), icon: 'mdi:lightning-bolt' },
  { key: 'meetings', label: t('Meetings'), icon: 'mdi:calendar-check' },
  { key: 'attachments', label: t('Attachments'), icon: 'mdi:paperclip' }
])

// Methods
const handleNodeSelect = async (item: TreeItem) => {
  selectedId.value = item.id
  selectedItem.value = item
  activeTab.value = 0

  // Reset counts
  membersCount.value = 0
  tasksCount.value = 0
  meetingsCount.value = 0
  attachmentsCount.value = 0

  // Load financial compensation
  await loadFinancialCompensation()
}

// Financial Compensation
const loadFinancialCompensation = async () => {
  if (!selectedId.value) return
  try {
    const result = await CouncilCommitteesService.hasFinancialCompensationCommittee(selectedId.value)
    // API returns boolean directly or in data property
    hasFinancialCompensation.value = result.data ?? result ?? false
  } catch (error) {
    hasFinancialCompensation.value = false
    console.error('Failed to load financial compensation:', error)
  }
}

const saveFinancialCompensation = async () => {
  if (!selectedId.value) return
  try {
    await CouncilCommitteesService.saveHasFinancialCompensation(selectedId.value, hasFinancialCompensation.value)
  } catch (error) {
    console.error('Failed to save financial compensation:', error)
  }
}

// Council/Committee Form
const openAddCouncilCommittee = () => {
  isEditCouncilCommittee.value = false
  councilCommitteeDialog.value = true
}

const openEditCouncilCommittee = () => {
  isEditCouncilCommittee.value = true
  councilCommitteeDialog.value = true
}

const handleCouncilCommitteeSaved = (data: { id: string; name: string }) => {
  if (data.name && selectedItem.value) {
    selectedItem.value = { ...selectedItem.value, name: data.name }
  }
  organizationTreeRef.value?.refresh()
}
</script>

<style scoped>
.councils-page {
  @apply h-full flex flex-col;
}

/* Main Layout */
.main-layout {
  @apply flex gap-6 flex-1 min-h-0;
}

/* Content Panel */
.content-panel {
  @apply flex-1 min-w-0 flex flex-col;
}

/* Selected Header */
.selected-header {
  @apply flex items-center justify-between p-4 bg-white rounded-2xl border border-gray-200 mb-4;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}

.selected-info {
  @apply flex items-center gap-4;
}

.selected-icon {
  @apply w-12 h-12 rounded-xl flex items-center justify-center text-white;
}

.selected-icon.council {
  background: linear-gradient(135deg, #006d4b 0%, #007a4a 100%);
}

.selected-icon.committee {
  background: linear-gradient(135deg, #003423 0%, #00472b 100%);
}

.selected-type {
  @apply text-xs text-zinc-500 uppercase tracking-wide;
}

.selected-name {
  @apply text-lg font-bold text-zinc-900;
}

.selected-actions {
  @apply flex items-center gap-4;
}

/* Modern Toggle Switch */
.toggle-switch {
  @apply flex items-center gap-3 cursor-pointer select-none;
}

.toggle-input {
  @apply sr-only;
}

.toggle-slider {
  @apply relative w-11 h-6 bg-zinc-200 rounded-full transition-colors duration-200 ease-in-out;
  @apply after:content-[''] after:absolute after:top-0.5 after:start-0.5;
  @apply after:bg-white after:rounded-full after:h-5 after:w-5;
  @apply after:transition-all after:duration-200 after:ease-in-out;
  @apply after:shadow-sm;
}

.toggle-input:checked + .toggle-slider {
  @apply bg-primary;
}

.toggle-input:checked + .toggle-slider::after {
  transform: translateX(1.25rem);
}

[dir="rtl"] .toggle-input:checked + .toggle-slider::after {
  transform: translateX(-1.25rem);
}

.toggle-input:focus + .toggle-slider {
  @apply ring-2 ring-primary/30;
}

.toggle-text {
  @apply flex items-center gap-1.5 text-sm text-zinc-600 font-medium;
}

/* Stats Grid */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 10px;
  margin-bottom: 16px;
}

.dark-stat-card-sm {
  position: relative;
  background: #fff;
  border-radius: 10px;
  padding: 12px 14px;
  color: #1a2e25;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.25s ease;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
  border: 1px solid #e4ede8;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.dark-stat-card-sm:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 14px rgba(0, 109, 75, 0.1);
  border-color: #c8ddd3;
}

.dark-stat-card-sm.active {
  border-color: #006d4b;
  box-shadow: 0 4px 14px rgba(0, 109, 75, 0.15);
  background: #f0f7f4;
}

.stat-wave {
  position: absolute;
  left: 0;
  bottom: 0;
  width: 100%;
  height: 40px;
  pointer-events: none;
}

.stat-top {
  display: flex;
  align-items: center;
  gap: 6px;
  z-index: 1;
  position: relative;
}

.stat-icon-sm {
  color: #006d4b;
}

.stat-label-sm {
  font-size: 11px;
  font-weight: 600;
  color: #6b8a7d;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.stat-value-sm {
  font-size: 22px;
  font-weight: 800;
  color: #1a2e25;
  line-height: 1;
  z-index: 1;
  position: relative;
}

/* Tabs */
.tabs-container {
  @apply flex-1 bg-white rounded-2xl border border-gray-200 overflow-hidden flex flex-col;
}

.tabs-header {
  @apply flex border-b border-gray-200 bg-zinc-50/50 overflow-x-auto;
}

.tab-btn {
  @apply flex items-center gap-2 px-5 py-3 text-sm font-medium text-zinc-500 border-b-2 border-transparent transition-colors whitespace-nowrap;
  @apply hover:text-zinc-700 hover:bg-zinc-100/50;
}

.tab-btn.active {
  background: #006d4b;
  box-shadow: none;
  @apply text-white border-transparent;
}

.tab-content {
  @apply flex-1 overflow-y-auto;
}

/* Empty States */
.content-empty {
  @apply flex-1 flex flex-col items-center justify-center bg-white rounded-2xl;
  border: 1px solid #e6eaef;
}

.empty-icon-ring {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: rgba(0, 109, 75, 0.06);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 20px;
}

.empty-icon-inner {
  width: 52px;
  height: 52px;
  border-radius: 50%;
  background: rgba(0, 109, 75, 0.1);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #006d4b;
}

.empty-title {
  font-size: 16px;
  font-weight: 600;
  color: #004730;
  margin: 0 0 6px 0;
}

.empty-desc {
  font-size: 13px;
  color: #94a3b8;
  margin: 0;
}

/* Responsive */
@media (max-width: 1024px) {
  .main-layout {
    @apply flex-col;
  }

  .stats-grid {
    @apply grid-cols-2;
  }
}
</style>

<template>
  <div class="space-y-5">
    <PageHeader :title="$t('MenuPermissions')" :subtitle="$t('ManageMenuPermissions')" :breadcrumbs="[{ label: 'Settings', to: '/settings' }]">
      <template #actions>
        <Button v-if="selectedItem" variant="primary" icon-left="save" :loading="saving" @click="savePermissions">
          {{ $t('Save') }}
        </Button>
      </template>
    </PageHeader>

    <!-- Tabs -->
    <div class="tabs-bar">
      <button :class="['tab-btn', { active: activeTab === 'groups' }]" @click="activeTab = 'groups'; selectedItem = null">
        <Icon icon="group" class="w-4 h-4" />
        {{ $t('Groups') }}
      </button>
      <button :class="['tab-btn', { active: activeTab === 'roles' }]" @click="activeTab = 'roles'; selectedItem = null">
        <Icon icon="shield_person" class="w-4 h-4" />
        {{ $t('Roles') }}
      </button>
    </div>

    <!-- Content -->
    <div class="content-grid">
      <!-- Left: List -->
      <div class="list-panel">
        <div class="list-header">
          <input v-model="searchQuery" type="text" class="search-input" :placeholder="$t('Search') + '...'" />
        </div>
        <div class="list-body">
          <div v-if="loadingList" class="list-loading">
            <Icon icon="progress_activity" class="w-6 h-6 animate-spin" />
          </div>
          <div v-else-if="filteredItems.length === 0" class="list-empty">
            {{ $t('NoData') }}
          </div>
          <div
            v-for="item in filteredItems"
            :key="item.id"
            class="list-item"
            :class="{ active: selectedItem?.id === item.id }"
            @click="selectItem(item)"
          >
            <div class="item-icon">
              <Icon :icon="activeTab === 'groups' ? 'group' : 'shield_person'" class="w-4 h-4" />
            </div>
            <div class="item-info">
              <span class="item-name">{{ item.name }}</span>
              <span v-if="item.nameAr" class="item-name-ar">{{ item.nameAr }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Right: Permissions -->
      <div class="perms-panel">
        <div v-if="!selectedItem" class="perms-empty">
          <Icon icon="touch_app" class="w-10 h-10" />
          <p>{{ $t('SelectGroupOrRole') }}</p>
        </div>
        <div v-else-if="loadingPerms" class="perms-empty">
          <Icon icon="progress_activity" class="w-6 h-6 animate-spin" />
        </div>
        <div v-else>
          <div class="perms-header">
            <h3>{{ selectedItem.name }}</h3>
            <span class="perms-count">{{ assignedCount }} / {{ availablePermissions.length }}</span>
          </div>
          <div class="perms-list">
            <label
              v-for="perm in availablePermissions"
              :key="perm.id"
              class="perm-item"
              :class="{ checked: selectedPermissionIds.includes(perm.id) }"
            >
              <input
                type="checkbox"
                :checked="selectedPermissionIds.includes(perm.id)"
                @change="togglePermission(perm.id)"
                class="sr-only"
              />
              <span class="perm-check">
                <Icon v-if="selectedPermissionIds.includes(perm.id)" icon="check" class="w-3 h-3" />
              </span>
              <Icon :icon="getPermIcon(perm.name)" class="w-4 h-4 perm-icon" />
              <span class="perm-name">{{ $t(perm.name) || perm.name }}</span>
              <span v-if="perm.groupName" class="perm-group">{{ perm.groupName }}</span>
            </label>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import MenuPermissionsService from '@/services/MenuPermissionsService'

const activeTab = ref<'groups' | 'roles'>('groups')
const searchQuery = ref('')
const loadingList = ref(false)
const loadingPerms = ref(false)
const saving = ref(false)

const groups = ref<any[]>([])
const roles = ref<any[]>([])
const availablePermissions = ref<any[]>([])
const selectedItem = ref<any>(null)
const selectedPermissionIds = ref<number[]>([])

const filteredItems = computed(() => {
  const items = activeTab.value === 'groups' ? groups.value : roles.value
  if (!searchQuery.value) return items
  const q = searchQuery.value.toLowerCase()
  return items.filter((i: any) =>
    i.name?.toLowerCase().includes(q) || i.nameAr?.includes(q)
  )
})

const assignedCount = computed(() => selectedPermissionIds.value.length)

async function loadData() {
  loadingList.value = true
  try {
    const [groupsRes, rolesRes, permsRes] = await Promise.all([
      MenuPermissionsService.listIamGroups(),
      MenuPermissionsService.listIamRoles(),
      MenuPermissionsService.listAvailable()
    ])
    groups.value = groupsRes?.data?.value || groupsRes?.data || groupsRes?.value || []
    roles.value = rolesRes?.data?.value || rolesRes?.data || rolesRes?.value || []
    // Flatten paginated roles if needed
    if (roles.value?.items) roles.value = roles.value.items
    availablePermissions.value = permsRes?.data || permsRes || []
  } catch (err) {
    console.error('Failed to load data:', err)
  } finally {
    loadingList.value = false
  }
}

async function selectItem(item: any) {
  selectedItem.value = item
  loadingPerms.value = true
  try {
    let res
    if (activeTab.value === 'groups') {
      res = await MenuPermissionsService.getGroupPermissions(item.id)
    } else {
      res = await MenuPermissionsService.getRolePermissions(item.name)
    }
    const data = res?.data || res || []
    selectedPermissionIds.value = Array.isArray(data) ? data : (data.permissionIds || [])
  } catch {
    selectedPermissionIds.value = []
  } finally {
    loadingPerms.value = false
  }
}

function togglePermission(permId: number) {
  const idx = selectedPermissionIds.value.indexOf(permId)
  if (idx >= 0) {
    selectedPermissionIds.value.splice(idx, 1)
  } else {
    selectedPermissionIds.value.push(permId)
  }
}

async function savePermissions() {
  if (!selectedItem.value) return
  saving.value = true
  try {
    if (activeTab.value === 'groups') {
      await MenuPermissionsService.saveGroupPermissions(selectedItem.value.id, {
        displayName: selectedItem.value.name,
        permissionIds: selectedPermissionIds.value
      })
    } else {
      await MenuPermissionsService.saveRolePermissions(selectedItem.value.name, {
        permissionIds: selectedPermissionIds.value
      })
    }
  } catch (err) {
    console.error('Failed to save:', err)
  } finally {
    saving.value = false
  }
}

function getPermIcon(name: string): string {
  const icons: Record<string, string> = {
    Home: 'home', Dashboard: 'dashboard', Meetings: 'calendar_month',
    DraftMeetings: 'edit_note', AddMeeting: 'event', MMSTasks: 'check_circle',
    Recommendations: 'lightbulb', Chat: 'chat', CouncilsAndCommittees: 'group',
    CouncilsAndCommitteesGeneralInfo: 'info', ComitteesSummaryReport: 'insert_chart',
    AttendanceReport: 'how_to_reg', Settings: 'settings', SessionRelated: 'co_present',
    NotLinkedMeeting: 'event_busy', SystemSettings: 'settings', Roles: 'shield',
    Dictionary: 'book', ManageOrganization: 'apartment',
    VotingTypesSettings: 'how_to_vote',
    Bids: 'work_outline'
  }
  return icons[name] || 'menu'
}

onMounted(() => loadData())
</script>

<style scoped>
.tabs-bar {
  display: flex;
  gap: 4px;
  background: #f1f5f9;
  padding: 4px;
  border-radius: 10px;
  width: fit-content;
}

.tab-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 20px;
  border: none;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  color: #64748b;
  background: transparent;
  transition: all 0.15s;
}

.tab-btn.active {
  background: #006d4b;
  color: #fff;
}

.tab-btn:hover:not(.active) {
  background: #e2e8f0;
}

.content-grid {
  display: grid;
  grid-template-columns: 320px 1fr;
  gap: 20px;
  min-height: 500px;
}

/* Left panel */
.list-panel {
  background: #fff;
  border: 1px solid #e0e0e0;
  border-radius: 10px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.list-header {
  padding: 12px;
  border-bottom: 1px solid #f1f5f9;
}

.search-input {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  font-size: 13px;
  outline: none;
}

.search-input:focus {
  border-color: #006d4b;
  box-shadow: 0 0 0 2px rgba(0, 109, 75, 0.1);
}

.list-body {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
}

.list-loading, .list-empty {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
  color: #94a3b8;
}

.list-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s;
  margin-bottom: 2px;
}

.list-item:hover {
  background: #f8faf9;
}

.list-item.active {
  background: #f0faf7;
  border-inline-start: 3px solid #006d4b;
}

.item-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.item-info {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.item-name {
  font-size: 13px;
  font-weight: 600;
  color: #1a1a1a;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.item-name-ar {
  font-size: 11px;
  color: #94a3b8;
}

/* Right panel */
.perms-panel {
  background: #fff;
  border: 1px solid #e0e0e0;
  border-radius: 10px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.perms-empty {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  color: #94a3b8;
  padding: 60px;
}

.perms-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 20px;
  background: #006d4b;
  color: #fff;
  border-bottom: 2px solid #006d4b;
}

.perms-header h3 {
  font-size: 14px;
  font-weight: 700;
  margin: 0;
}

.perms-count {
  font-size: 12px;
  color: #006d4b;
  font-weight: 600;
}

.perms-list {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
}

.perm-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s;
  margin-bottom: 2px;
}

.perm-item:hover {
  background: #f8faf9;
}

.perm-item.checked {
  background: #f0faf7;
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  border: 0;
}

.perm-check {
  width: 20px;
  height: 20px;
  border: 2px solid #d1d5db;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  color: #fff;
  transition: all 0.15s;
}

.perm-item.checked .perm-check {
  background: #006d4b;
  border-color: #006d4b;
}

.perm-icon {
  color: #64748b;
  flex-shrink: 0;
}

.perm-item.checked .perm-icon {
  color: #006d4b;
}

.perm-name {
  font-size: 13px;
  font-weight: 500;
  color: #1a1a1a;
  flex: 1;
}

.perm-group {
  font-size: 11px;
  color: #94a3b8;
  background: #f1f5f9;
  padding: 2px 8px;
  border-radius: 10px;
}

@media (max-width: 768px) {
  .content-grid {
    grid-template-columns: 1fr;
  }
}
</style>

<template>
  <div class="sidebar-nav">
    <!-- Loading State -->
    <div v-if="loading" class="nav-loading">
      <div class="spinner"></div>
    </div>

    <!-- Navigation Items -->
    <template v-else>
      <template
        v-for="item in filteredNavigation"
        :key="item.name"
      >
        <!-- Section header (expanded) -->
        <div
          v-if="item.type === 'header' && !collapsed"
          class="section-header"
        >
          <span>{{ $t(item.label) || item.label }}</span>
        </div>

        <!-- Section divider (collapsed) -->
        <div
          v-else-if="item.type === 'header' && collapsed"
          class="nav-section-divider"
        ></div>

        <!-- Regular navigation item -->
        <RouterLink
          v-else-if="!item.type"
          :to="item.to || '/'"
          :class="[
            'nav-item',
            isActive(item.to) ? 'active' : '',
            collapsed ? 'collapsed' : ''
          ]"
          :title="collapsed ? ($t(item.label) || item.label) : undefined"
        >
          <span :class="['nav-icon', isActive(item.to) ? 'active' : '']">
            <svg v-if="item.svgIcon" class="w-5 h-5" viewBox="0 0 24 24" fill="currentColor" v-html="item.svgIcon"></svg>
            <Icon v-else :icon="item.icon" class="w-5 h-5" />
          </span>
          <span v-if="!collapsed" :class="['nav-label', isActive(item.to) ? 'active' : '']">
            {{ $t(item.label) || item.label }}
          </span>
          <span
            v-if="item.badge && !collapsed"
            :class="['nav-badge', item.badgeVariant || '']"
          >
            {{ item.badge }}
          </span>
        </RouterLink>
      </template>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import UsersService from '@/services/UsersService'

interface NavigationItem {
  name: string
  type?: 'header'
  label: string
  icon: string
  svgIcon?: string
  to?: string
  badge?: string | number
  badgeVariant?: 'primary' | 'error' | 'warning'
}

// Props
interface Props {
  collapsed?: boolean
}

withDefaults(defineProps<Props>(), {
  collapsed: false
})

// Route
const route = useRoute()

// State
const loading = ref(true)
const userPermissions = ref<{ name: string; hasAccess: boolean }[]>([])

// All navigation items - flat structure with sections
const allNavigationItems: NavigationItem[] = [
  // Main items (no section header)
  {
    name: 'Home',
    label: 'Home',
    icon: 'home',
    to: '/home'
  },
  {
    name: 'Dashboard',
    label: 'Dashboard',
    icon: 'dashboard',
    to: '/dashboard'
  },
  // Meetings Section
  {
    name: 'header-meetings',
    type: 'header',
    label: 'MeetingManagement',
    icon: ''
  },
  {
    name: 'Meetings',
    label: 'Meetings',
    icon: 'calendar_month',
    to: '/meetings'
  },
  {
    name: 'DraftMeetings',
    label: 'DraftMeetings',
    icon: 'edit_note',
    to: '/draftMeetings'
  },
  {
    name: 'CreateMeeting',
    label: 'AddMeeting',
    icon: 'event',
    to: '/addMeeting'
  },
  {
    name: 'NotLinkedMeeting',
    label: 'NotRelatedMeetings',
    icon: 'event_busy',
    to: '/not-related-meetings'
  },
  {
    name: 'Bids',
    label: 'Bids',
    icon: 'work_outline',
    to: '/bids'
  },
  // Work Section
  {
    name: 'header-work',
    type: 'header',
    label: 'WorkManagement',
    icon: ''
  },
  {
    name: 'MMSTasks',
    label: 'Tasks',
    icon: 'check_circle',
    to: '/tasks'
  },
  {
    name: 'Recommendations',
    label: 'Recommendations',
    icon: 'lightbulb',
    to: '/recommendations'
  },
  {
    name: 'Chat',
    label: 'Chat',
    icon: 'chat',
    to: '/chat'
  },
  // Data Section
  {
    name: 'header-data',
    type: 'header',
    label: 'DataAndReports',
    icon: ''
  },
  {
    name: 'CouncilsAndCommitteesGeneralInfo',
    label: 'CouncilsAndCommitteesGeneralInfo',
    icon: 'group',
    to: '/council-committee-general-info'
  },
  {
    name: 'ComitteesSummaryReport',
    label: 'CommitteesSummary',
    icon: 'insert_chart',
    to: '/reports/committees-summary'
  },
  {
    name: 'AttendanceReport',
    label: 'AttendanceReport',
    icon: 'how_to_reg',
    to: '/reports/attendance'
  },
  // Administration Section
  {
    name: 'header-admin',
    type: 'header',
    label: 'Administration',
    icon: ''
  },
  {
    name: 'Settings',
    label: 'Settings',
    icon: 'settings',
    to: '/settings'
  },
]

// Static items that always show (not filtered by permissions)
const staticItems = ['Home', 'Dashboard']

// Filter navigation based on permissions
const filteredNavigation = computed<NavigationItem[]>(() => {
  const perms = Array.isArray(userPermissions.value) ? userPermissions.value : []

  // No permissions returned — show only static items (Home, Dashboard)
  if (perms.length === 0) {
    return allNavigationItems.filter(item => staticItems.includes(item.name))
  }

  const permissionMap = new Map(
    perms.map(p => [p.name, p.hasAccess])
  )

  const result: NavigationItem[] = []

  for (const item of allNavigationItems) {
    // Always include static items (Home, Main header)
    if (staticItems.includes(item.name)) {
      result.push(item)
      continue
    }

    if (item.type === 'header') {
      result.push(item)
      continue
    }

    // Settings visible if user has any admin-level permission
    if (item.name === 'Settings') {
      const adminPerms = ['SystemSettings', 'Roles', 'Dictionary', 'ManageOrganization', 'VotingTypesSettings', 'CouncilsAndCommittees', 'SuperAdmin']
      if (adminPerms.some(p => permissionMap.get(p) === true)) {
        result.push(item)
      }
      continue
    }

    if (permissionMap.get(item.name) === true) {
      result.push(item)
    }
  }

  // Clean up orphan headers
  return cleanupNavigation(result)
})

function cleanupNavigation(items: NavigationItem[]): NavigationItem[] {
  const cleaned: NavigationItem[] = []

  for (let i = 0; i < items.length; i++) {
    const item = items[i]
    const nextItem = items[i + 1]

    // Skip header if next item is also header or end of list
    if (item.type === 'header') {
      if (!nextItem || nextItem.type === 'header') {
        continue
      }
    }

    cleaned.push(item)
  }

  return cleaned
}

function isActive(to?: string): boolean {
  if (!to) return false
  return route.path === to || route.path.startsWith(to + '/')
}

// Load permissions on mount
onMounted(async () => {
  try {
    const response: any = await UsersService.listCurrentUserPermissions()
    const permissions = response?.data ?? response
    userPermissions.value = Array.isArray(permissions) ? permissions : []
  } catch (error) {
    console.error('Failed to load user permissions:', error)
    // On error, show only static items — don't grant full access
    userPermissions.value = []
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
/* ===== Container ===== */
.sidebar-nav {
  padding: 16px 16px;
  display: flex;
  flex-direction: column;
  gap: 0;
}

/* ===== Loading State ===== */
.nav-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px 0;
}

.spinner {
  width: 24px;
  height: 24px;
  border: 2px solid rgba(255, 255, 255, 0.1);
  border-top-color: #006d4b;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* ===== Section Header (matching IAM .section-header exactly) ===== */
.section-header {
  display: flex;
  align-items: center;
  justify-content: flex-start;
  padding: 0 14px 8px;
  margin: 0;
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.12em;
  color: rgba(255, 255, 255, 0.4);
  background: none;
  border-radius: 0;
  cursor: default;
  user-select: none;
  margin-top: 8px;
}

.section-header:first-child {
  margin-top: 0;
}

/* Collapsed section divider (matching IAM .collapsed-section-divider) */
.nav-section-divider {
  height: 1px;
  background: rgba(255, 255, 255, 0.06);
  margin: 8px 8px;
}

/* ===== Nav Items (matching IAM .nav-item exactly) ===== */
.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  color: #d4d4d8;
  font-size: 13px;
  font-weight: 400;
  border-inline-start: 2px solid transparent;
  overflow: hidden;
  -webkit-font-smoothing: antialiased;
  letter-spacing: 0.01em;
  margin-bottom: 2px;
  text-decoration: none;
}

.nav-item:hover {
  background: linear-gradient(90deg, rgba(255, 255, 255, 0.08) 0%, rgba(255, 255, 255, 0.03) 100%);
  color: #ffffff;
  border-inline-start-color: rgba(0, 109, 75, 0.5);
}

.nav-item.active {
  background: linear-gradient(90deg, rgba(0, 109, 75, 0.18) 0%, rgba(0, 109, 75, 0.06) 100%);
  color: #ffffff;
  font-weight: 500;
  border-inline-start-color: #006d4b;
  box-shadow: inset 0 0 20px rgba(0, 109, 75, 0.08), 0 0 12px rgba(0, 109, 75, 0.04);
}

.nav-item.active::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: radial-gradient(ellipse at left center, rgba(0, 109, 75, 0.15) 0%, transparent 70%);
  pointer-events: none;
}

[dir="rtl"] .nav-item.active::before {
  background: radial-gradient(ellipse at right center, rgba(0, 109, 75, 0.15) 0%, transparent 70%);
}

.nav-item.collapsed {
  width: 44px;
  height: 44px;
  padding: 0;
  margin: 4px auto;
  justify-content: center;
  border-inline-start: none;
}

/* ===== Navigation Icon ===== */
.nav-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  color: #a1a1aa;
  flex-shrink: 0;
  transition: color 0.25s cubic-bezier(0.4, 0, 0.2, 1);
}

.nav-item:hover .nav-icon {
  color: #ffffff;
}

.nav-icon.active {
  color: #ffffff;
}

/* ===== Navigation Label ===== */
.nav-label {
  flex: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  text-align: start;
  transition: opacity 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.nav-label.active {
  color: #ffffff;
}

/* ===== Badge ===== */
.nav-badge {
  padding: 2px 8px;
  font-size: 11px;
  font-weight: 600;
  border-radius: 10px;
  background: rgba(0, 109, 75, 0.2);
  color: #ffffff;
}

.nav-badge.error {
  background: rgba(239, 68, 68, 0.15);
  color: #f87171;
}

.nav-badge.warning {
  background: rgba(245, 158, 11, 0.15);
  color: #fbbf24;
}
</style>

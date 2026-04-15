<template>
  <div class="settings-page">
    <!-- Page Header -->
    <PageHeader :title="$t('Settings')" :subtitle="$t('SettingsDescription')" />

    <!-- Loading State -->
    <div v-if="loading" class="settings-loading">
      <div class="loader-ring"></div>
    </div>

    <!-- Settings Cards Grid -->
    <div v-else class="settings-content">
      <div class="settings-grid">
        <div
          v-for="item in settingsItems"
          :key="item.name"
          class="setting-card"
          @click="$router.push(item.to)"
        >
          <div class="card-body">
            <div :class="['card-icon', item.iconClass]">
              <Icon :icon="item.icon" class="w-5 h-5" />
            </div>
            <h3 class="card-title">{{ $t(item.label) || item.fallback }}</h3>
            <p class="card-description">{{ $t(item.descKey) || item.descFallback }}</p>
          </div>
          <div class="card-footer">
            <svg class="card-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M9 18l6-6-6-6"/>
            </svg>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import Icon from '@/components/ui/Icon.vue'
import UsersService from '@/services/UsersService'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'

interface SettingsItem {
  name: string
  label: string
  fallback: string
  descKey: string
  descFallback: string
  icon: string
  iconClass: string
  to: string
  permissionName?: string
}

const loading = ref(true)
const menuPermissions = ref<{ name: string; hasAccess: boolean }[]>([])
const isCommitteeAdmin = ref(false)

const allSettingsItems: SettingsItem[] = [
  {
    name: 'councils-committees',
    label: 'CouncilsAndCommittees',
    fallback: 'المجالس واللجان',
    descKey: 'CouncilsAndCommitteesDesc',
    descFallback: 'إدارة المجالس واللجان والأعضاء',
    icon: 'mdi:account-group',
    iconClass: 'icon-teal',
    to: '/councils-committees',
    permissionName: 'CouncilsAndCommittees'
  },
  {
    name: 'roles',
    label: 'Roles',
    fallback: 'الأدوار',
    descKey: 'RolesDesc',
    descFallback: 'إدارة أدوار النظام',
    icon: 'mdi:shield-account',
    iconClass: 'icon-navy',
    to: '/roles',
    permissionName: 'Roles'
  },
  {
    name: 'menu-permissions',
    label: 'MenuPermissions',
    fallback: 'صلاحيات القائمة',
    descKey: 'MenuPermissionsDesc',
    descFallback: 'إدارة صلاحيات القائمة حسب الأدوار والمجموعات',
    icon: 'mdi:menu-open',
    iconClass: 'icon-teal',
    to: '/menu-permissions',
    permissionName: 'Roles'
  },
  {
    name: 'users',
    label: 'Users',
    fallback: 'المستخدمين',
    descKey: 'UsersDesc',
    descFallback: 'عرض المستخدمين وإدارة الصلاحيات',
    icon: 'mdi:account-multiple',
    iconClass: 'icon-navy',
    to: '/users',
    permissionName: 'ManageOrganization'
  },
  {
    name: 'departments',
    label: 'Departments',
    fallback: 'الأقسام',
    descKey: 'DepartmentsDesc',
    descFallback: 'إدارة الهيكل التنظيمي',
    icon: 'mdi:sitemap',
    iconClass: 'icon-teal',
    to: '/departments',
    permissionName: 'ManageOrganization'
  },
  {
    name: 'dictionary',
    label: 'Dictionary',
    fallback: 'القاموس',
    descKey: 'DictionaryDesc',
    descFallback: 'إدارة ترجمات النظام',
    icon: 'mdi:translate',
    iconClass: 'icon-navy',
    to: '/dictionary',
    permissionName: 'Dictionary'
  },
  {
    name: 'voting-types',
    label: 'VotingTypes',
    fallback: 'أنواع التصويت',
    descKey: 'VotingTypesDesc',
    descFallback: 'إدارة أنواع وخيارات التصويت',
    icon: 'mdi:vote',
    iconClass: 'icon-teal',
    to: '/voting-types',
    permissionName: 'VotingTypesSettings'
  },
  {
    name: 'committee-item-types',
    label: 'CommitteeItemTypes',
    fallback: 'أنواع بنود اللجان',
    descKey: 'CommitteeItemTypesDesc',
    descFallback: 'إدارة أنواع بنود اللجان',
    icon: 'mdi:format-list-bulleted-type',
    iconClass: 'icon-navy',
    to: '/committee-item-types',
    permissionName: 'SystemSettings'
  },
  {
    name: 'tags',
    label: 'Tags',
    fallback: 'الوسوم',
    descKey: 'TagDesc',
    descFallback: 'إدارة وسوم اللجان والاجتماعات والبنود',
    icon: 'mdi:tag-multiple',
    iconClass: 'icon-teal',
    to: '/tags',
    permissionName: 'SystemSettings'
  },
  {
    name: 'external-members',
    label: 'ExternalMembers',
    fallback: 'الأعضاء الخارجيون',
    descKey: 'ExternalMembersDesc',
    descFallback: 'إدارة الأعضاء الخارجيين بدون حسابات',
    icon: 'mdi:account-multiple-outline',
    iconClass: 'icon-navy',
    to: '/external-members',
    permissionName: 'ManageOrganization'
  },
  {
    name: 'delegations',
    label: 'Delegations',
    fallback: 'التفويضات',
    descKey: 'DelegationsDesc',
    descFallback: 'إدارة تفويض الصلاحيات والمهام',
    icon: 'mdi:handshake-outline',
    iconClass: 'icon-teal',
    to: '/delegations',
    permissionName: ''
  },
  {
    name: 'mom-templates',
    label: 'MomTemplateSettings',
    fallback: 'قوالب المحضر',
    descKey: 'MomTemplateDesc',
    descFallback: 'إدارة قوالب محاضر الاجتماعات',
    icon: 'mdi:file-document-edit',
    iconClass: 'icon-navy',
    to: '/mom-templates',
    permissionName: 'SystemSettings'
  },
  {
    name: 'app-settings',
    label: 'AppSettings',
    fallback: 'إعدادات النظام',
    descKey: 'AppSettingsDesc',
    descFallback: 'إعدادات النظام العامة',
    icon: 'mdi:cog',
    iconClass: 'icon-teal',
    to: '/app-settings',
    permissionName: 'SystemSettings'
  },
  {
    name: 'bid-workflows',
    label: 'BidWorkflows',
    fallback: 'سير عمل العروض',
    descKey: 'WorkflowDesignerDesc',
    descFallback: 'اضبط من ينفذ كل خطوة من خطوات العرض',
    icon: 'mdi:transit-connection-variant',
    iconClass: 'icon-navy',
    to: '/admin/bid-workflows',
    permissionName: 'ManageOrganization'
  },
  {
    name: 'outlook-integration',
    label: 'OutlookIntegration',
    fallback: 'تكامل Outlook',
    descKey: 'OutlookIntegrationDesc',
    descFallback: 'إعدادات تكامل Microsoft Outlook',
    icon: 'mdi:microsoft-outlook',
    iconClass: 'icon-navy',
    to: '/outlook-integration',
    permissionName: 'SystemSettings'
  },
  {
    name: 'teams-integration',
    label: 'TeamsIntegration',
    fallback: 'تكامل Teams',
    descKey: 'TeamsIntegrationDesc',
    descFallback: 'إعدادات تكامل Microsoft Teams',
    icon: 'mdi:microsoft-teams',
    iconClass: 'icon-teal',
    to: '/teams-integration',
    permissionName: 'SystemSettings'
  },
  {
    name: 'email-templates',
    label: 'EmailTemplates',
    fallback: 'قوالب البريد الإلكتروني',
    descKey: 'EmailTemplatesDesc',
    descFallback: 'إدارة قوالب البريد الإلكتروني',
    icon: 'mdi:email-edit-outline',
    iconClass: 'icon-navy',
    to: '/email-templates',
    permissionName: 'SystemSettings'
  }
]

const settingsItems = computed(() => {
  if (loading.value) return []

  return allSettingsItems.filter(item => {
    if (item.name === 'councils-committees' && isCommitteeAdmin.value) {
      return true
    }
    if (!item.permissionName) return true
    return menuPermissions.value.some(p => p.name === item.permissionName && p.hasAccess)
  })
})

onMounted(async () => {
  try {
    const [permissionsResponse, adminCommitteesResponse] = await Promise.all([
      UsersService.listCurrentUserPermissions(),
      CouncilCommitteesService.getMyAdminCommittees()
    ])

    const permissions = (permissionsResponse as any)?.data ?? permissionsResponse
    const adminData = (adminCommitteesResponse as any)?.data ?? adminCommitteesResponse
    const committees = adminData?.committeeIds ?? adminData

    menuPermissions.value = Array.isArray(permissions) ? permissions : []
    isCommitteeAdmin.value = Array.isArray(committees) && committees.length > 0
  } catch {
    menuPermissions.value = []
    isCommitteeAdmin.value = false
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.settings-page {
  min-height: 100%;
  display: flex;
  flex-direction: column;
  background: #f8fafc;
}

.settings-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 80px 0;
}

.loader-ring {
  width: 36px;
  height: 36px;
  border: 3px solid #e5e7eb;
  border-top-color: #006d4b;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.settings-content {
  padding: 0;
  flex: 1;
}

/* Grid */
.settings-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 15px;
}

/* Card */
.setting-card {
  position: relative;
  background: #ffffff;
  border-radius: 9px;
  border: 1px solid #e6eaef;
  padding: 12px 12px 8px;
  display: flex;
  flex-direction: column;
  cursor: pointer;
  transition: all 0.22s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.04);
  overflow: hidden;
}

/* Wave decoration on top */
.setting-card::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 20px;
  pointer-events: none;
  z-index: 0;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 300 20' preserveAspectRatio='none'%3E%3Cdefs%3E%3ClinearGradient id='wg' x1='0' y1='0' x2='1' y2='0'%3E%3Cstop offset='0%25' stop-color='%23112d48'/%3E%3Cstop offset='100%25' stop-color='%2300ae8c'/%3E%3C/linearGradient%3E%3C/defs%3E%3Cpath d='M0,0 L300,0 L300,10 C240,17 180,6 120,14 C60,20 20,8 0,16 Z' fill='url(%23wg)'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-size: 100% 100%;
  border-radius: 9px 9px 0 0;
}

.setting-card:hover {
  transform: translateY(-2px);
  border-color: rgba(0, 109, 75, 0.35);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
}

.setting-card:active {
  transform: translateY(-1px);
}

.card-body {
  display: flex;
  flex-direction: column;
  flex: 1;
  position: relative;
  z-index: 1;
}

.card-icon {
  width: 34px;
  height: 34px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: 22px;
  margin-bottom: 8px;
  transition: transform 0.22s ease;
}

.setting-card:hover .card-icon {
  transform: scale(1.06);
}

/* Icon variants — alternating teal and navy */
.icon-teal { background: linear-gradient(135deg, #e6f7f3 0%, #ccf0e8 100%); color: #007E65; }
.icon-navy { background: linear-gradient(135deg, #e8edf3 0%, #dce3ec 100%); color: #004730; }

.card-title {
  font-size: 12px;
  font-weight: 700;
  color: #004730;
  margin: 0 0 2px 0;
  line-height: 1.3;
}

.card-description {
  font-size: 10.5px;
  color: #64748b;
  margin: 0;
  line-height: 1.35;
}

.card-footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  margin-top: 4px;
  padding: 0;
  border: none;
  background: none;
  position: relative;
  z-index: 1;
}

[dir="rtl"] .card-footer {
  justify-content: flex-start;
}

.card-arrow {
  width: 14px;
  height: 14px;
  color: #cbd5e1;
  transition: all 0.22s ease;
}

[dir="rtl"] .card-arrow {
  transform: scaleX(-1);
}

.setting-card:hover .card-arrow {
  color: #006d4b;
  transform: translateX(2px);
}

[dir="rtl"] .setting-card:hover .card-arrow {
  transform: scaleX(-1) translateX(2px);
}

/* Responsive */
@media (max-width: 640px) {
  .settings-content {
    padding: 14px;
  }

  .settings-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 8px;
  }

  .setting-card {
    padding: 10px 10px 7px;
  }

  .card-icon {
    width: 30px;
    height: 30px;
    margin-bottom: 6px;
  }

  .card-title {
    font-size: 11px;
  }

  .card-description {
    font-size: 10px;
  }
}

@media (min-width: 641px) and (max-width: 900px) {
  .settings-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (min-width: 901px) and (max-width: 1200px) {
  .settings-grid {
    grid-template-columns: repeat(4, 1fr);
  }
}

@media (min-width: 1201px) {
  .settings-grid {
    grid-template-columns: repeat(5, 1fr);
  }
}
</style>

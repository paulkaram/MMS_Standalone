import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useAppStore } from '@/stores/app'
import { loadTranslation } from '@/plugins/i18n'
import UsersService from '@/services/UsersService'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'

// Cached permission data (fetched once per session)
let cachedMenuPermissions: { name: string; hasAccess: boolean }[] | null = null
let cachedIsCommitteeAdmin: boolean | null = null

async function loadPermissionData(): Promise<void> {
  if (cachedMenuPermissions !== null && cachedIsCommitteeAdmin !== null) return

  try {
    const [permissions, adminCommittees] = await Promise.all([
      cachedMenuPermissions ? Promise.resolve(cachedMenuPermissions) : UsersService.listCurrentUserPermissions(),
      cachedIsCommitteeAdmin !== null ? Promise.resolve(null) : CouncilCommitteesService.getMyAdminCommittees()
    ])

    if (!cachedMenuPermissions && permissions) {
      cachedMenuPermissions = permissions as any
      // Unwrap ApiResponseDto if needed
      if ((permissions as any)?.data) {
        cachedMenuPermissions = (permissions as any).data
      }
    }

    if (cachedIsCommitteeAdmin === null) {
      const adminData = adminCommittees?.data ?? adminCommittees
      const committees = adminData?.committeeIds ?? adminData
      cachedIsCommitteeAdmin = Array.isArray(committees) && committees.length > 0
    }
  } catch (error) {
    console.error('Failed to load permission data:', error)
    cachedMenuPermissions = cachedMenuPermissions || []
    cachedIsCommitteeAdmin = cachedIsCommitteeAdmin ?? false
  }
}

// Admin permission names that grant access to admin routes
const ADMIN_PERMISSION_NAMES = [
  'SystemSettings', 'Roles', 'Dictionary', 'ManageOrganization',
  'VotingTypesSettings', 'CouncilsAndCommittees', 'SuperAdmin'
]

function hasAnyAdminMenuPermission(): boolean {
  if (!cachedMenuPermissions) return false
  return cachedMenuPermissions.some(p =>
    p.hasAccess && ADMIN_PERMISSION_NAMES.includes(p.name)
  )
}

function hasSettingsAccess(): boolean {
  return hasAnyAdminMenuPermission() || cachedIsCommitteeAdmin === true
}

// Reset cache on logout
export function resetPermissionCache() {
  cachedMenuPermissions = null
  cachedIsCommitteeAdmin = null
}

// Lazy-loaded route components
const Login = () => import('@/pages/Login.vue')
const OidcLogin = () => import('@/pages/Oidc/SignInOidc.vue')
const TwoFA = () => import('@/pages/TwoFA.vue')
const Logout = () => import('@/pages/Logout.vue')
const Dashboard = () => import('@/pages/Dashboard/Dashboard.vue')
const Chat = () => import('@/pages/Chat.vue')
const PageNotFound = () => import('@/pages/PageNotFound.vue')

// Settings page
const Settings = () => import('@/pages/Settings.vue')

// Admin pages
const Dictionary = () => import('@/pages/Admin/Dictionary.vue')
const ManageOrganization = () => import('@/pages/Admin/ManageOrganization.vue')
const Users = () => import('@/pages/Admin/Users.vue')
const Departments = () => import('@/pages/Admin/Departments.vue')
const ManagePrivileges = () => import('@/pages/Admin/ManagePrivileges.vue')
const DataSources = () => import('@/pages/Admin/DataSources.vue')
const WorkflowDesigner = () => import('@/pages/Admin/Case/WorkflowDesigner.vue')
const Apps = () => import('@/pages/Admin/Case/Apps.vue')
const Roles = () => import('@/pages/Admin/Roles.vue')
const AuditTrail = () => import('@/pages/Admin/AuditTrail.vue')
const CouncilsAndCommittees = () => import('@/pages/Admin/CouncilsAndCommittees.vue')
const VotingTypeSettings = () => import('@/pages/Admin/VotingTypeSettings.vue')
const CommitteeItemTypeSettings = () => import('@/pages/Admin/CommitteeItemTypeSettings.vue')
const MomTemplateSettings = () => import('@/pages/Admin/MomTemplateSettings.vue')
const MenuPermissions = () => import('@/pages/Admin/MenuPermissions.vue')
const TasksConfiguration = () => import('@/pages/Admin/TasksConfiguration.vue')
const AppSettings = () => import('@/pages/Admin/AppSettings.vue')
const OutlookIntegration = () => import('@/pages/Admin/OutlookIntegration.vue')
const TeamsIntegration = () => import('@/pages/Admin/TeamsIntegration.vue')
const EmailTemplates = () => import('@/pages/Admin/EmailTemplates.vue')

// Meeting pages
const AddMeeting = () => import('@/pages/Meetings/AddMeeting.vue')
const DraftMeetings = () => import('@/pages/Meetings/DraftMeetings.vue')
const MyMeetings = () => import('@/pages/Meetings/MyMeetings.vue')
const NotRelatedMeetings = () => import('@/pages/Meetings/NotRelatedMeetings.vue')
const MeetingRoom = () => import('@/pages/Meetings/MeetingRoom/MeetingRoom.vue')
const MeetingDashboard = () => import('@/components/app/meeting/MeetingDashboard/MeetingDashboard.vue')

// Recommendation pages
const Recommendations = () => import('@/pages/Recommendations/Recommendations.vue')
const RecommendationDetails = () => import('@/pages/Recommendations/RecommendationDetails.vue')

// General Info pages
const CouncilsAndCommitteesData = () => import('@/pages/GeneralInfo/CouncilsAndCommitteesGeneralInfo.vue')
const CommitteeDetails = () => import('@/pages/GeneralInfo/CommitteeDetails.vue')

// Tasks
const Tasks = () => import('@/pages/Tasks.vue')

// Calendar
const Calendar = () => import('@/pages/Calendar.vue')

// Sessions
const StartSession = () => import('@/pages/StartSession.vue')

// Reports
const CommitteesSummaryReport = () => import('@/pages/Reports/CommitteesSummaryReport.vue')
const AttendanceReport = () => import('@/pages/Reports/AttendanceReport.vue')

// Mobile
const MobileFileViewer = () => import('@/pages/MobileSupport/MobileFileViewer.vue')

// Processes
const ProcessFollowup = () => import('@/pages/Processes/ProcessFollowup.vue')
const FollowupDetails = () => import('@/pages/Processes/FollowupDetails.vue')
const InitiateProcess = () => import('@/pages/Processes/InitiateProcess.vue')

// Route definitions
const routes: RouteRecordRaw[] = [
  // Auth routes (public)
  {
    name: 'login',
    path: '/login',
    component: Login,
    meta: { secure: false, layout: 'blank' }
  },
  {
    name: 'signIn-oidc',
    path: '/signIn-oidc',
    component: OidcLogin,
    meta: { secure: false, layout: 'blank' }
  },
  {
    name: '2FA',
    path: '/2FA',
    component: TwoFA,
    meta: { secure: false, layout: 'blank' }
  },
  {
    name: 'logout',
    path: '/logout',
    component: Logout,
    meta: { secure: true }
  },

  // Main routes (protected)
  {
    name: 'home',
    path: '/home',
    component: Calendar,
    meta: { secure: true }
  },
  {
    name: 'dashboard',
    path: '/dashboard',
    component: Dashboard,
    meta: { secure: true }
  },
  {
    name: 'chat',
    path: '/chat',
    component: Chat,
    meta: { secure: true }
  },
  {
    name: 'calendar',
    path: '/calendar',
    component: Calendar,
    meta: { secure: true }
  },

  // General Info routes
  {
    name: 'council-committee-general-info',
    path: '/council-committee-general-info/:councilId?',
    component: CouncilsAndCommitteesData,
    meta: { secure: true }
  },
  {
    name: 'committee-details',
    path: '/committee-details/:committeeId',
    component: CommitteeDetails,
    meta: { secure: true }
  },

  // Settings page
  {
    name: 'settings',
    path: '/settings',
    component: Settings,
    meta: { secure: true, permission: 'settings' }
  },

  // Admin routes
  {
    name: 'manage-organization',
    path: '/manage-organization',
    component: ManageOrganization,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'users',
    path: '/users',
    component: Users,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'departments',
    path: '/departments',
    component: Departments,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'app-settings',
    path: '/app-settings',
    component: AppSettings,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'outlook-integration',
    path: '/outlook-integration',
    component: OutlookIntegration,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'teams-integration',
    path: '/teams-integration',
    component: TeamsIntegration,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'email-templates',
    path: '/email-templates',
    component: EmailTemplates,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'dictionary',
    path: '/dictionary',
    component: Dictionary,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'voting-types',
    path: '/voting-types',
    component: VotingTypeSettings,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'committee-item-types',
    path: '/committee-item-types',
    component: CommitteeItemTypeSettings,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'mom-templates',
    path: '/mom-templates',
    component: MomTemplateSettings,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'manage-privileges',
    path: '/manage-privileges',
    component: ManagePrivileges,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'data-sources',
    path: '/data-sources',
    component: DataSources,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'audit-trail',
    path: '/audit-trail',
    component: AuditTrail,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'councils-committees',
    path: '/councils-committees',
    component: CouncilsAndCommittees,
    meta: { secure: true, permission: 'settings' }
  },
  {
    name: 'workflow-designer',
    path: '/workflow-designer',
    component: WorkflowDesigner,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'apps',
    path: '/apps',
    component: Apps,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'roles',
    path: '/roles',
    component: Roles,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'menu-permissions',
    path: '/menu-permissions',
    component: MenuPermissions,
    meta: { secure: true, permission: 'admin' }
  },
  {
    name: 'tasks-configuration',
    path: '/tasks-configuration',
    component: TasksConfiguration,
    meta: { secure: true, permission: 'admin' }
  },

  // Meeting routes
  {
    name: 'addMeeting',
    path: '/addMeeting/:id?',
    component: AddMeeting,
    meta: { secure: true }
  },
  {
    name: 'draftMeetings',
    path: '/draftMeetings',
    component: DraftMeetings,
    meta: { secure: true }
  },
  {
    name: 'meetings',
    path: '/meetings',
    component: MyMeetings,
    meta: { secure: true }
  },
  {
    name: 'not-related-meetings',
    path: '/not-related-meetings',
    component: NotRelatedMeetings,
    meta: { secure: true }
  },
  {
    name: 'meetingRoom',
    path: '/meetingRoom/:id',
    component: MeetingRoom,
    meta: { secure: true }
  },
  {
    name: 'meetingDashboard',
    path: '/meeting-dashboard/:id',
    component: MeetingDashboard,
    meta: { secure: true }
  },

  // Recommendations
  {
    name: 'recommendations',
    path: '/recommendations',
    component: Recommendations,
    meta: { secure: true }
  },
  {
    name: 'recommendation-details',
    path: '/recommendation-details/:id',
    component: RecommendationDetails,
    meta: { secure: true }
  },

  // Tasks
  {
    name: 'tasks',
    path: '/tasks',
    component: Tasks,
    meta: { secure: true }
  },

  // Processes
  {
    name: 'process-followup',
    path: '/process-followup',
    component: ProcessFollowup,
    meta: { secure: true }
  },
  {
    name: 'followup-details',
    path: '/followup-details/:processInstanceId',
    component: FollowupDetails,
    meta: { secure: true }
  },
  {
    name: 'initiate-process',
    path: '/initiate-process/:workflowId',
    component: InitiateProcess,
    meta: { secure: true }
  },

  // Sessions
  {
    name: 'start-session',
    path: '/start-session',
    component: StartSession,
    meta: { secure: true }
  },

  // Reports
  {
    name: 'committees-summary-report',
    path: '/reports/committees-summary',
    component: CommitteesSummaryReport,
    meta: { secure: true }
  },
  {
    name: 'attendance-report',
    path: '/reports/attendance',
    component: AttendanceReport,
    meta: { secure: true }
  },

  // Mobile
  {
    name: 'mobile-file-viewer',
    path: '/mobile-file-viewer',
    component: MobileFileViewer,
    meta: { secure: false, layout: 'blank' }
  },

  // Catch-all redirect
  {
    path: '/',
    redirect: '/login'
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: PageNotFound,
    meta: { layout: 'blank' }
  }
]

// Create router instance
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  scrollBehavior(_to, _from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    }
    return { top: 0 }
  }
})

// Navigation guards
router.beforeEach(async (to, _from, next) => {
  const userStore = useUserStore()
  const appStore = useAppStore()

  // Reset translation failed status
  appStore.setTranslationFailed(false)

  try {
    // Load translations if not already loaded
    await loadTranslation()

    const isAuthenticated = userStore.isAuthenticated
    const isSecureRoute = to.meta.secure === true

    // If authenticated user navigates to login page, redirect to home
    // Never block signIn-oidc — it always processes the OIDC code (handles user switching)
    if (isAuthenticated && to.name === 'login') {
      next({ name: 'home' })
      return
    }

    // If route requires authentication and user is not authenticated
    if (isSecureRoute && !isAuthenticated) {
      next({ name: 'login' })
      return
    }

    // Permission check for protected routes
    if (isAuthenticated && to.meta.permission) {
      await loadPermissionData()

      if (to.meta.permission === 'settings') {
        // Settings and councils-committees: accessible to admins and committee admins
        if (!hasSettingsAccess()) {
          next({ name: 'home' })
          return
        }
      } else if (to.meta.permission === 'admin') {
        // Admin routes: only accessible to users with admin menu permissions
        if (!hasAnyAdminMenuPermission()) {
          next({ name: 'home' })
          return
        }
      }
    }

    next()
  } catch (error) {
    console.error('Failed to load translations:', error)
    appStore.setTranslationFailed(true)
    next()
  }
})

// After each navigation, update document direction based on language
router.afterEach(() => {
  const userStore = useUserStore()
  const dir = userStore.isRtl ? 'rtl' : 'ltr'
  const lang = userStore.language

  document.documentElement.setAttribute('dir', dir)
  document.documentElement.setAttribute('lang', lang)
})

export default router

// Type augmentation for route meta
declare module 'vue-router' {
  interface RouteMeta {
    secure?: boolean
    layout?: 'default' | 'blank'
    permission?: string
  }
}

<template>
  <div class="space-y-5">
    <!-- Page Header -->
    <PageHeader :title="$t('Users')" :subtitle="$t('ManageSystemUsers')" :breadcrumbs="[{ label: 'Settings', to: '/settings' }]" />

    <!-- Grid Container -->
    <div class="mg-container">
      <!-- Loading -->
      <div v-if="loading" class="mg-state">
        <Icon icon="mdi:loading" class="w-6 h-6 animate-spin" style="color: #008080" />
      </div>

      <!-- Search Bar (always visible) -->
      <div v-if="!loading" class="mg-toolbar">
          <div class="mg-search">
            <Icon icon="mdi:magnify" class="w-4 h-4" style="color: #9ca3af" />
            <input v-model="search" type="text" :placeholder="$t('SearchUsers')" @keyup.enter="handleSearch" />
            <button v-if="search" class="mg-search-clear" @click="clearSearch">
              <Icon icon="mdi:close" class="w-3.5 h-3.5" />
            </button>
          </div>
          <div class="mg-toolbar-end">
            <Button variant="primary" size="sm" icon-left="mdi:magnify" @click="handleSearch">{{ $t('Search') }}</Button>
            <Button variant="outline" size="sm" icon-left="mdi:refresh" @click="resetSearch">{{ $t('Reset') }}</Button>
            <span class="mg-iam-badge">IAM</span>
          </div>
        </div>

      <template v-if="users.length > 0">
        <div class="mg-table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>#</th>
                <th>{{ $t('FullName') }}</th>
                <th>{{ $t('Username') }}</th>
                <th>{{ $t('Email') }}</th>
                <th>{{ $t('Status') }}</th>
                <th>{{ $t('Actions') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(user, index) in users" :key="user.id">
                <td><span class="mg-id">{{ (currentPage - 1) * pageSize + index + 1 }}</span></td>
                <td>
                  <div class="user-cell">
                    <UserAvatar :userId="user.id" :name="user.displayName || user.username || ''" size="sm" />
                    <div>
                      <span class="mg-title">{{ user.displayName || '-' }}</span>
                      <span v-if="user.phoneNumber" class="user-cell-phone">{{ user.phoneNumber }}</span>
                    </div>
                  </div>
                </td>
                <td>{{ user.username || user.email || '-' }}</td>
                <td>{{ user.email || '-' }}</td>
                <td>
                  <span :class="['mg-pill', user.isActive ? 'completed' : 'cancelled']">
                    {{ user.isActive ? ($t('Active')) : ($t('Inactive')) }}
                  </span>
                </td>
                <td>
                  <div class="mg-actions">
                    <button class="mg-act" :title="$t('Permissions')" @click="openPermissions(user)">
                      <Icon icon="mdi:shield-account" class="w-4 h-4" />
                    </button>
                    <button class="mg-act" :title="$t('CommitteeAdmin')" @click="openCommitteeAdmin(user)">
                      <Icon icon="mdi:shield-key" class="w-4 h-4" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div v-if="totalCount > pageSize" class="mg-pag">
          <span class="mg-pag-info">
            {{ $t('Showing') }} <strong>{{ (currentPage - 1) * pageSize + 1 }}</strong>
            {{ $t('To') }} <strong>{{ Math.min(currentPage * pageSize, totalCount) }}</strong>
            {{ $t('Of') }} <strong>{{ totalCount }}</strong>
          </span>
          <div class="mg-pag-btns">
            <button class="mg-pg" :disabled="currentPage === 1" @click="goToPage(1)">
              <Icon icon="mdi:chevron-double-left" class="w-4 h-4" />
            </button>
            <button class="mg-pg" :disabled="currentPage === 1" @click="goToPage(currentPage - 1)">
              <Icon icon="mdi:chevron-left" class="w-4 h-4" />
            </button>
            <button v-for="p in visiblePages" :key="p" class="mg-pg num" :class="{ active: p === currentPage }" @click="goToPage(p)">{{ p }}</button>
            <button class="mg-pg" :disabled="currentPage === totalPages" @click="goToPage(currentPage + 1)">
              <Icon icon="mdi:chevron-right" class="w-4 h-4" />
            </button>
            <button class="mg-pg" :disabled="currentPage === totalPages" @click="goToPage(totalPages)">
              <Icon icon="mdi:chevron-double-right" class="w-4 h-4" />
            </button>
          </div>
        </div>
      </template>

      <!-- Empty (after search) -->
      <div v-else-if="!loading && users.length === 0" class="mg-state">
        <Icon icon="mdi:account-off-outline" class="w-12 h-12" style="color: #ccc" />
        <p>{{ $t('NoUsers') }}</p>
      </div>
    </div>

    <!-- Add/Edit User Dialog -->
    <Modal
      v-model="userDialog"
      :title="isEditMode ? ($t('EditUser')) : ($t('AddUser'))"
      :icon="isEditMode ? 'mdi:account-edit' : 'mdi:account-plus'"
      size="lg"
    >
      <div class="user-form">
        <div class="form-grid">
          <!-- Full Name Arabic -->
          <div class="form-group">
            <label class="form-label required">
              <Icon icon="mdi:account" class="w-4 h-4" />
              {{ $t('FullNameAr') }}
            </label>
            <input
              v-model="userForm.fullNameAr"
              type="text"
              class="form-input"
              :placeholder="$t('EnterFullNameAr')"
              required
            />
          </div>

          <!-- Full Name English -->
          <div class="form-group">
            <label class="form-label required">
              <Icon icon="mdi:account" class="w-4 h-4" />
              {{ $t('FullNameEn') }}
            </label>
            <input
              v-model="userForm.fullNameEn"
              type="text"
              class="form-input"
              dir="ltr"
              :placeholder="$t('EnterFullNameEn')"
              required
            />
          </div>

          <!-- Username -->
          <div class="form-group">
            <label class="form-label required">
              <Icon icon="mdi:at" class="w-4 h-4" />
              {{ $t('Username') }}
            </label>
            <input
              v-model="userForm.username"
              type="text"
              class="form-input"
              dir="ltr"
              :placeholder="$t('EnterUsername')"
              :disabled="isEditMode"
              required
            />
          </div>

          <!-- Email -->
          <div class="form-group">
            <label class="form-label required">
              <Icon icon="mdi:email" class="w-4 h-4" />
              {{ $t('Email') }}
            </label>
            <input
              v-model="userForm.email"
              type="email"
              class="form-input"
              dir="ltr"
              :placeholder="$t('EnterEmail')"
              required
            />
          </div>

          <!-- Mobile -->
          <div class="form-group">
            <label class="form-label">
              <Icon icon="mdi:phone" class="w-4 h-4" />
              {{ $t('Mobile') }}
            </label>
            <input
              v-model="userForm.mobile"
              type="tel"
              class="form-input"
              dir="ltr"
              :placeholder="$t('EnterMobile')"
            />
          </div>

          <!-- National ID -->
          <div class="form-group">
            <label class="form-label">
              <Icon icon="mdi:card-account-details" class="w-4 h-4" />
              {{ $t('NationalId') }}
            </label>
            <input
              v-model="userForm.nationalId"
              type="text"
              class="form-input"
              dir="ltr"
              :placeholder="$t('EnterNationalId')"
            />
          </div>

          <!-- Language -->
          <div class="form-group">
            <label class="form-label required">
              <Icon icon="mdi:translate" class="w-4 h-4" />
              {{ $t('DefaultLanguage') }}
            </label>
            <CustomSelect
              v-model="userForm.defaultLanguageId"
              :options="languages"
              value-key="id"
              label-key="name"
              :placeholder="$t('SelectLanguage')"
            />
          </div>

          <!-- Status -->
          <div class="form-group">
            <label class="form-label">
              <Icon icon="mdi:check-circle" class="w-4 h-4" />
              {{ $t('Status') }}
            </label>
            <label class="toggle-switch-lg">
              <input v-model="userForm.approved" type="checkbox" />
              <span class="toggle-slider-lg"></span>
              <span class="toggle-text">{{ userForm.approved ? ($t('Approved')) : ($t('Pending')) }}</span>
            </label>
          </div>
        </div>
      </div>
      <template #footer>
        <Button variant="outline" @click="userDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="primary" :loading="saving" @click="saveUser">
          <Icon v-if="!saving" icon="mdi:check" class="w-4 h-4" />
          {{ isEditMode ? ($t('Update')) : ($t('Add')) }}
        </Button>
      </template>
    </Modal>

    <!-- Permissions Dialog -->
    <Modal
      v-model="permissionsDialog"
      :title="($t('UserPermissions')) + ' - ' + (selectedUser?.displayName || selectedUser?.username || '')"
      icon="mdi:shield-account"
      size="xl"
    >
      <div class="permissions-container">
        <!-- Loading State -->
        <div v-if="loadingPermissions" class="permissions-loading">
          <div class="loading-spinner">
            <Icon icon="mdi:loading" class="w-8 h-8 animate-spin text-primary" />
          </div>
          <span>{{ $t('LoadingPermissions') }}</span>
        </div>

        <!-- Permissions Content -->
        <div v-else-if="userPermissions.length" class="permissions-content">
          <!-- Permission Type Tabs -->
          <div class="permission-types">
            <button
              v-for="permType in userPermissions"
              :key="permType.id"
              type="button"
              class="permission-type-tab"
              :class="{ active: activePermissionType === permType.id }"
              @click="activePermissionType = permType.id"
            >
              <Icon :icon="getPermissionTypeIcon(permType.typeName)" class="w-4 h-4" />
              <span>{{ translateKey(permType.typeName) }}</span>
            </button>
          </div>

          <!-- Permission Groups -->
          <div class="permission-groups">
            <template v-for="permType in userPermissions" :key="permType.id">
              <div v-if="activePermissionType === permType.id" class="groups-container">
                <div
                  v-for="group in permType.items"
                  :key="group.groupName"
                  class="permission-group"
                >
                  <div class="group-header">
                    <Icon icon="mdi:folder-key" class="w-5 h-5 text-primary" />
                    <span class="group-name">{{ translateKey(group.groupName) }}</span>
                    <span class="group-count">{{ group.items?.length || 0 }} {{ $t('Permissions') }}</span>
                  </div>
                  <div class="group-permissions">
                    <div
                      v-for="perm in group.items"
                      :key="`${perm.id}-${perm.levelId}`"
                      class="permission-item"
                      :class="{ enabled: perm.hasAccess }"
                    >
                      <div class="permission-info">
                        <Icon
                          :icon="perm.hasAccess ? 'mdi:shield-check' : 'mdi:shield-off'"
                          class="w-5 h-5"
                          :class="perm.hasAccess ? 'text-emerald-500' : 'text-zinc-300'"
                        />
                        <span class="permission-name">{{ translateKey(perm.name) }}</span>
                      </div>
                      <label class="permission-toggle">
                        <input
                          type="checkbox"
                          :checked="perm.hasAccess"
                          @change="togglePermission(perm, $event)"
                        />
                        <span class="toggle-track"></span>
                      </label>
                    </div>
                  </div>
                </div>
              </div>
            </template>
          </div>
        </div>

        <!-- Empty State -->
        <div v-else class="permissions-empty">
          <Icon icon="mdi:shield-off-outline" class="w-16 h-16 text-zinc-200" />
          <p>{{ $t('NoPermissionsFound') }}</p>
        </div>
      </div>
      <template #footer>
        <Button variant="outline" @click="permissionsDialog = false">
          {{ $t('Close') }}
        </Button>
      </template>
    </Modal>

    <!-- Committee Admin Dialog -->
    <Modal
      v-model="committeeAdminDialog"
      :title="($t('CommitteeAdmin')) + ' - ' + (selectedUser?.displayName || selectedUser?.username || '')"
      icon="mdi:shield-key"
      size="lg"
    >
      <div class="committee-admin-content">
        <div v-if="loadingCommitteeAdmin" class="committee-admin-loading">
          <Icon icon="mdi:loading" class="w-8 h-8 animate-spin text-primary" />
          <span>{{ $t('Loading') }}</span>
        </div>
        <div v-else class="committee-admin-tree">
          <OrganizationTree
            :checkbox-mode="true"
            v-model:checked-ids="committeeAdminCheckedIds"
          />
        </div>
      </div>
      <template #footer>
        <Button variant="outline" @click="committeeAdminDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="primary" :loading="savingCommitteeAdmin" @click="saveCommitteeAdmin">
          <Icon v-if="!savingCommitteeAdmin" icon="mdi:check" class="w-4 h-4" />
          {{ $t('Save') }}
        </Button>
      </template>
    </Modal>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import Icon from '@/components/ui/Icon.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import Button from '@/components/ui/Button.vue'
import Modal from '@/components/ui/Modal.vue'
import CustomSelect from '@/components/ui/CustomSelect.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import UsersService from '@/services/UsersService'
import LookupsService from '@/services/LookupsService'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'
import OrganizationTree from '@/components/app/admin/councils/OrganizationTree.vue'
import { useToast } from '@/composables/useToast'
const { t, te } = useI18n()
const { toast } = useToast()

// Page size options
// Types
interface AdminUser {
  id: string
  username: string
  displayName: string
  firstName?: string
  lastName?: string
  email: string
  phoneNumber?: string
  isActive: boolean
  // MMS-specific (resolved on demand)
  mmsUserId?: number
  fullnameAr?: string
  fullnameEn?: string
  structuresRoles?: any[]
}

interface UserFormData {
  id: number | null
  fullNameAr: string
  fullNameEn: string
  username: string
  email: string
  mobile: string
  nationalId: string
  approved: boolean
  defaultLanguageId: number | null
}

// State
const loading = ref(false)
const loadingRoles = ref(false)
const saving = ref(false)
const users = ref<AdminUser[]>([])
const totalCount = ref(0)
const search = ref('')
const expandedRows = ref<string[]>([])

// Languages
const languages = ref<{ id: number; name: string }[]>([])

// Pagination
const currentPage = ref(1)
const pageSize = ref(10)

// Profile pictures - track failed image loads
const failedImages = ref<Record<string, boolean>>({})

// Dialogs
const userDialog = ref(false)
const permissionsDialog = ref(false)
const selectedUser = ref<AdminUser | null>(null)
const isEditMode = ref(false)

// Committee Admin
const committeeAdminDialog = ref(false)
const loadingCommitteeAdmin = ref(false)
const savingCommitteeAdmin = ref(false)
const committeeAdminCheckedIds = ref<number[]>([])

// Permissions
const loadingPermissions = ref(false)
const userPermissions = ref<any[]>([])
const activePermissionType = ref<number | null>(null)

// User form
const defaultUserForm = (): UserFormData => ({
  id: null,
  fullNameAr: '',
  fullNameEn: '',
  username: '',
  email: '',
  mobile: '',
  nationalId: '',
  approved: true,
  defaultLanguageId: 1
})

const userForm = ref<UserFormData>(defaultUserForm())

// Computed
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value) || 1)

const activeUsersCount = computed(() => users.value.filter(u => u.isActive).length)


const visiblePages = computed(() => {
  const pages: number[] = []
  const total = totalPages.value
  const current = currentPage.value

  let start = Math.max(1, current - 2)
  let end = Math.min(total, current + 2)

  if (end - start < 4) {
    if (start === 1) {
      end = Math.min(total, start + 4)
    } else {
      start = Math.max(1, end - 4)
    }
  }

  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  return pages
})

// Methods
const loadUsers = async () => {
  loading.value = true
  expandedRows.value = []
  try {
    const searchQuery = search.value || undefined
    const response = await UsersService.listAdminUsers(currentPage.value, pageSize.value, searchQuery)
    // Handle API wrapper format
    const data = response.data || response
    users.value = data.data || data || []
    totalCount.value = data.total || 0
  } catch (error) {
    console.error('Failed to load users:', error)
    users.value = []
    totalCount.value = 0
  } finally {
    loading.value = false
  }
}

const goToPage = (page: number) => {
  currentPage.value = page
  loadUsers()
}

const handleSearch = async () => {
  currentPage.value = 1
  loading.value = true
  try {
    const searchQuery = search.value || undefined
    const response = await UsersService.listAdminUsers(1, pageSize.value, searchQuery)
    const data = response.data || response
    users.value = data.data || data || []
    totalCount.value = data.total || 0
  } catch (error) {
    users.value = []
    totalCount.value = 0
  } finally {
    loading.value = false
  }
}

const clearSearch = () => {
  search.value = ''
}

const resetSearch = () => {
  search.value = ''
  currentPage.value = 1
  loadUsers()
}

const toggleExpand = async (user: AdminUser) => {
  const index = expandedRows.value.indexOf(user.id)
  if (index > -1) {
    expandedRows.value.splice(index, 1)
  } else {
    expandedRows.value.push(user.id)
    if (!user.structuresRoles) {
      loadingRoles.value = true
      try {
        const roles = await UsersService.listUserRolesStructures(user.id)
        user.structuresRoles = roles || []
      } catch (error) {
        console.error('Failed to load user roles:', error)
        user.structuresRoles = []
      } finally {
        loadingRoles.value = false
      }
    }
  }
}

const toggleSms = async (user: AdminUser) => {
  try {
    await UsersService.enableSms(user.id, user.smsEnabled)
  } catch (error) {
    console.error('Failed to toggle SMS:', error)
    user.smsEnabled = !user.smsEnabled
  }
}

const toggleEmail = async (user: AdminUser) => {
  try {
    await UsersService.enableEmail(user.id, user.emailNotificationEnabled)
  } catch (error) {
    console.error('Failed to toggle email:', error)
    user.emailNotificationEnabled = !user.emailNotificationEnabled
  }
}

const loadLanguages = async () => {
  try {
    const response = await LookupsService.getLanguages()
    languages.value = (response as any)?.data || response || []
  } catch (error) {
    console.error('Failed to load languages:', error)
    // Default languages if API fails
    languages.value = [
      { id: 1, name: 'العربية' },
      { id: 2, name: 'English' }
    ]
  }
}

const openAddDialog = () => {
  isEditMode.value = false
  userForm.value = defaultUserForm()
  userDialog.value = true
}

const editUser = (user: AdminUser) => {
  isEditMode.value = true
  userForm.value = {
    id: parseInt(user.id),
    fullNameAr: user.fullnameAr || '',
    fullNameEn: user.fullnameEn || '',
    username: user.username || '',
    email: user.email || '',
    mobile: user.mobile || '',
    nationalId: user.nationalId || '',
    approved: user.approved,
    defaultLanguageId: user.defaultLanguageId || 1
  }
  userDialog.value = true
}

const saveUser = async () => {
  // Validate required fields
  if (!userForm.value.fullNameAr || !userForm.value.fullNameEn || !userForm.value.username || !userForm.value.email) {
    return
  }

  saving.value = true
  try {
    const payload = {
      id: userForm.value.id,
      fullNameAr: userForm.value.fullNameAr,
      fullNameEn: userForm.value.fullNameEn,
      username: userForm.value.username,
      email: userForm.value.email,
      mobile: userForm.value.mobile || null,
      nationalId: userForm.value.nationalId || null,
      approved: userForm.value.approved,
      defaultLanguageId: userForm.value.defaultLanguageId || 1
    }

    if (isEditMode.value) {
      await UsersService.updateUser(payload as any)
    } else {
      await UsersService.addUser(payload as any)
    }

    userDialog.value = false
    loadUsers()
  } catch (error) {
    console.error('Failed to save user:', error)
  } finally {
    saving.value = false
  }
}

// Returns the user's ID directly (users are now local, no IAM resolution needed)
const resolveUserMmsId = async (user: AdminUser): Promise<string> => {
  return user.id
}

const openPermissions = async (user: AdminUser) => {
  selectedUser.value = user
  userPermissions.value = []
  activePermissionType.value = null
  permissionsDialog.value = true
  loadingPermissions.value = true

  try {
    const mmsId = await resolveUserMmsId(user)
    const response = await UsersService.getUserPermissions(String(mmsId))
    const data = (response as any)?.data || response || []
    userPermissions.value = data

    // Set first permission type as active
    if (data.length > 0) {
      activePermissionType.value = data[0].id
    }
  } catch (error) {
    console.error('Failed to load permissions:', error)
    userPermissions.value = []
  } finally {
    loadingPermissions.value = false
  }
}

const togglePermission = async (perm: any, event: Event) => {
  const checkbox = event.target as HTMLInputElement
  const enabled = checkbox.checked

  try {
    await UsersService.toggleUserPermission(
      String(selectedUser.value!.mmsUserId || selectedUser.value!.id),
      perm.id,
      perm.levelId,
      enabled
    )
    // Update local state
    perm.hasAccess = enabled
  } catch (error) {
    console.error('Failed to toggle permission:', error)
    // Revert checkbox state on error
    checkbox.checked = !enabled
  }
}

const getPermissionTypeIcon = (typeName: string): string => {
  const lowerName = typeName?.toLowerCase() || ''
  if (lowerName.includes('read') || lowerName.includes('قراءة')) {
    return 'mdi:eye'
  }
  if (lowerName.includes('write') || lowerName.includes('كتابة')) {
    return 'mdi:pencil'
  }
  if (lowerName.includes('delete') || lowerName.includes('حذف')) {
    return 'mdi:delete'
  }
  if (lowerName.includes('admin') || lowerName.includes('إدارة')) {
    return 'mdi:shield-crown'
  }
  return 'mdi:shield'
}

const translateKey = (key: string | null | undefined): string => {
  if (!key) return ''
  // Check if translation exists, otherwise return the key as-is
  return te(key) ? t(key) : key
}

// Committee Admin
const openCommitteeAdmin = async (user: AdminUser) => {
  selectedUser.value = user
  committeeAdminCheckedIds.value = []
  committeeAdminDialog.value = true
  loadingCommitteeAdmin.value = true

  try {
    const mmsId = await resolveUserMmsId(user)
    const response = await CouncilCommitteesService.getUserAdminCommittees(mmsId)
    const data = (response as any)?.data || response
    committeeAdminCheckedIds.value = data?.committeeIds || []
  } catch (error) {
    console.error('Failed to load user admin committees:', error)
    toast.error(t('ErrorLoadingData'))
  } finally {
    loadingCommitteeAdmin.value = false
  }
}

const saveCommitteeAdmin = async () => {
  if (!selectedUser.value) return

  savingCommitteeAdmin.value = true
  try {
    await CouncilCommitteesService.updateBulkCommitteeAdmin(
      selectedUser.value.mmsUserId || parseInt(selectedUser.value.id),
      committeeAdminCheckedIds.value
    )
    toast.success(t('SavedSuccessfully'))
    committeeAdminDialog.value = false
  } catch (error) {
    console.error('Failed to save committee admin:', error)
    toast.error(t('ErrorSavingData'))
  } finally {
    savingCommitteeAdmin.value = false
  }
}

// Avatar colors for users without profile pictures
const avatarColors = ['#8B5CF6', '#F59E0B', '#10B981', '#3B82F6', '#EC4899', '#6366F1', '#14B8A6', '#F97316', '#006d4b', '#003423']

const getAvatarColor = (index: number): string => {
  return avatarColors[index % avatarColors.length]
}

const getInitials = (name: string) => {
  if (!name) return '?'
  const parts = name.split(' ')
  return parts.length > 1 ? parts[0][0] + parts[1][0] : name.substring(0, 2)
}

const getProfilePicture = (userId: string) => {
  const baseUrl = import.meta.env.VITE_MAIN_API || '/api/'
  return `${baseUrl}users/profile-image/${userId}`
}

const handleImageError = (event: Event, userId: string) => {
  failedImages.value[userId] = true
}

// Lifecycle
onMounted(() => {
  loadUsers()
  loadLanguages()
})
</script>

<style scoped>
/* Search toolbar */
.mg-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid #eaeaea;
  flex-wrap: wrap;
  gap: 10px;
}

.mg-search {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 1;
  max-width: 320px;
  position: relative;
}

.mg-search input {
  flex: 1;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 6px 28px 6px 10px;
  font-size: 13px;
  outline: none;
  transition: border-color 0.15s;
}

.mg-search input:focus {
  border-color: #006d4b;
  box-shadow: 0 0 0 2px rgba(0, 109, 75, 0.1);
}

.mg-search-clear {
  position: absolute;
  right: 6px;
  top: 50%;
  transform: translateY(-50%);
  color: #9ca3af;
  cursor: pointer;
  border: none;
  background: none;
  padding: 2px;
}

.mg-search-clear:hover { color: #6b7280; }

[dir="rtl"] .mg-search-clear {
  right: auto;
  left: 6px;
}

.mg-toolbar-end {
  display: flex;
  align-items: center;
  gap: 8px;
}

.mg-iam-badge {
  font-size: 10px;
  font-weight: 700;
  color: #006d4b;
  background: rgba(0, 109, 75, 0.08);
  padding: 3px 8px;
  border-radius: 4px;
  letter-spacing: 0.05em;
}

/* User cell with avatar */
.user-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}

.user-cell-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  color: #fff;
  font-size: 11px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  text-transform: uppercase;
}

.user-cell-phone {
  display: block;
  font-size: 11px;
  color: #94a3b8;
}

/* Permissions Dialog */
.permissions-container {
  @apply min-h-[400px];
}

.permissions-loading {
  @apply flex flex-col items-center justify-center gap-3 py-16 text-zinc-500;
}

.loading-spinner {
  @apply w-16 h-16 flex items-center justify-center;
}

.permissions-content {
  @apply space-y-4;
}

.permission-types {
  @apply flex flex-wrap gap-2 p-3 bg-zinc-50 rounded-xl border border-gray-200;
}

.permission-type-tab {
  @apply flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all;
  @apply bg-white text-zinc-600 border border-gray-200;
  @apply hover:border-primary/30 hover:text-primary;
}

.permission-type-tab.active {
  background: #006d4b;
  box-shadow: none;
  @apply text-white border-transparent;
}

.permission-groups {
  @apply max-h-[450px] overflow-y-auto;
}

.groups-container {
  @apply space-y-4;
}

.permission-group {
  @apply bg-white rounded-xl border border-gray-200 overflow-hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.group-header {
  @apply flex items-center gap-3 px-4 py-3 bg-gradient-to-l from-gray-50 to-white border-b border-gray-200;
}

.group-name {
  @apply font-bold text-zinc-800 flex-1;
}

.group-count {
  @apply text-xs text-zinc-400 bg-zinc-100 px-2 py-1 rounded-full;
}

.group-permissions {
  @apply divide-y divide-zinc-50;
}

.permission-item {
  @apply flex items-center justify-between px-4 py-3 transition-colors;
  @apply hover:bg-zinc-50;
}

.permission-item.enabled {
  @apply bg-emerald-50/50;
}

.permission-info {
  @apply flex items-center gap-3;
}

.permission-name {
  @apply text-sm text-zinc-700;
}

.permission-toggle {
  @apply relative inline-flex cursor-pointer;
}

.permission-toggle input {
  @apply sr-only;
}

.toggle-track {
  @apply w-11 h-6 bg-zinc-200 rounded-full transition-colors;
  @apply after:content-[''] after:absolute after:top-[2px] after:start-[2px];
  @apply after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all;
  @apply after:shadow-sm;
}

.permission-toggle input:checked + .toggle-track {
  @apply bg-emerald-500;
}

.permission-toggle input:checked + .toggle-track::after {
  transform: translateX(20px);
}

[dir="rtl"] .permission-toggle input:checked + .toggle-track::after {
  transform: translateX(-20px);
}

.permissions-empty {
  @apply flex flex-col items-center justify-center gap-3 py-16 text-zinc-400;
}

/* Committee Admin Dialog */
.committee-admin-content {
  @apply min-h-[400px];
}

.committee-admin-loading {
  @apply flex flex-col items-center justify-center gap-3 py-16 text-zinc-500;
}

.committee-admin-tree {
  @apply flex justify-center;
}

.committee-admin-tree :deep(.sidebar-panel) {
  @apply w-full border-0 shadow-none;
}

/* Responsive */
@media (max-width: 768px) {
  .toolbar {
    @apply flex-col items-stretch;
  }

  .toolbar-start, .toolbar-end {
    @apply flex-col items-stretch gap-2;
  }

  .search-input {
    @apply w-full;
  }

  .pagination {
    @apply flex-col;
  }

  .pagination-controls {
    @apply order-first;
  }
}
</style>

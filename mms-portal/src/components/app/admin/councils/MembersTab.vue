<template>
  <div class="tab-panel">
    <div class="panel-header">
      <h3 class="panel-title">{{ $t('Members') }}</h3>
      <Button
        variant="primary"
        size="sm"
        icon-left="mdi:account-plus"
        @click="openAddUser"
      >
        {{ $t('AddMember') }}
      </Button>
    </div>
    <div class="users-grid">
      <div
        v-for="user in users"
        :key="user.id"
        class="user-card"
      >
        <UserAvatar :userId="user.id" :name="user.fullname || ''" size="sm" />
        <div class="user-info">
          <h4 class="user-name">{{ user.fullname }}</h4>
          <p class="user-role">{{ user.roleName }}</p>
          <p class="user-email">{{ user.email }}</p>
        </div>
        <div class="user-status" :class="{ active: user.active }">
          {{ user.active ? ($t('Active')) : ($t('Inactive')) }}
        </div>
        <div class="user-actions">
          <button class="action-btn edit" @click="openEditUser(user)" :title="$t('Edit')">
            <Icon icon="mdi:pencil" class="w-4 h-4" />
          </button>
          <button class="action-btn permissions" @click="openUserPermissions(user)" :title="$t('Permissions')">
            <Icon icon="mdi:shield-account" class="w-4 h-4" />
          </button>
          <button class="action-btn delete" @click="removeUser(user)" :title="$t('Delete')">
            <Icon icon="mdi:delete" class="w-4 h-4" />
          </button>
        </div>
      </div>
      <div v-if="users.length === 0 && !loading" class="empty-state">
        <Icon icon="mdi:account-group-outline" class="w-16 h-16 text-zinc-200" />
        <p class="text-zinc-400">{{ $t('NoMembers') }}</p>
      </div>
    </div>

    <!-- Add/Edit User Dialog -->
    <Modal
      v-model="userDialog"
      :title="isEditUser ? ($t('EditMember')) : ($t('AddMember'))"
      size="md"
    >
      <form @submit.prevent="saveUser" class="space-y-4">
        <label class="toggle-switch">
          <input
            type="checkbox"
            v-model="userForm.active"
            class="toggle-input"
          />
          <span class="toggle-slider"></span>
          <span class="toggle-label">{{ $t('Active') }}</span>
        </label>

        <!-- Show user name when editing, combobox when adding -->
        <div v-if="isEditUser">
          <label class="block text-sm font-medium text-zinc-700 mb-1">
            {{ $t('Member') }}
          </label>
          <div class="px-4 py-2.5 bg-zinc-100 rounded-lg text-zinc-700 border border-zinc-200">
            {{ editingUserName }}
          </div>
        </div>
        <Combobox
          v-else
          v-model="userForm.userId"
          :options="userOptions"
          item-text="label"
          item-value="value"
          :label="$t('SearchUser')"
          :placeholder="$t('TypeToSearch')"
          :min-search-length="1"
          required
          @search="searchUsers"
        />

        <Select
          v-model="userForm.roleId"
          :options="roles"
          item-text="name"
          item-value="id"
          :label="$t('Role')"
          required
        />

        <Select
          v-model="userForm.privacyId"
          :options="privacies"
          item-text="name"
          item-value="id"
          :label="$t('PrivacyLevel')"
          required
        />

        <Input
          v-model="userForm.note"
          :label="$t('Notes')"
        />
      </form>

      <template #footer>
        <Button variant="outline" @click="userDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="primary" :loading="savingUser" @click="saveUser">
          {{ isEditUser ? ($t('Update')) : ($t('Add')) }}
        </Button>
      </template>
    </Modal>

    <!-- User Permissions Dialog -->
    <UserCommitteePermissions
      v-if="permissionsDialog"
      v-model="permissionsDialog"
      :committee-id="committeeId"
      :user-id="selectedUserId"
    />

    <!-- Confirm Delete Dialog -->
    <Modal
      v-model="confirmDialog"
      title="حذف العضو"
      icon="mdi:account-remove"
      variant="danger"
      size="sm"
    >
      <div class="text-center py-4">
        <Icon icon="mdi:alert-circle" class="w-16 h-16 text-error mx-auto mb-4" />
        <p class="text-zinc-600">{{ $t('ConfirmDeleteMember') }}</p>
      </div>
      <template #footer>
        <Button variant="outline" @click="confirmDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="danger" :loading="deleting" @click="confirmDelete">
          {{ $t('Delete') }}
        </Button>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from 'vue'
import { useI18n } from 'vue-i18n'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import Input from '@/components/ui/Input.vue'
import Select from '@/components/ui/Select.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import Combobox from '@/components/ui/Combobox.vue'
import Modal from '@/components/ui/Modal.vue'
import UserCommitteePermissions from '@/components/app/admin/UserCommitteePermissions.vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'
import LookupsService from '@/services/LookupsService'
import UsersService from '@/services/UsersService'

// Types
interface User {
  id: string
  fullname: string
  email: string
  roleName: string
  committeeRoleId: number
  privacyId: number
  active: boolean
  note?: string
}

// Props
const props = defineProps<{
  committeeId: string | null
}>()

// Emits
const emit = defineEmits<{
  (e: 'update:count', count: number): void
}>()

const { t } = useI18n()

// State
const loading = ref(false)
const users = ref<User[]>([])
const userDialog = ref(false)
const isEditUser = ref(false)
const savingUser = ref(false)
const userForm = ref<{ userId: string | number; roleId: string | number; privacyId: string | number; active: boolean; note: string }>({ userId: '', roleId: '', privacyId: '', active: true, note: '' })
const editingUserName = ref('')
const userOptions = ref<any[]>([])
const roles = ref<any[]>([])
const privacies = ref<any[]>([])

// Permissions
const permissionsDialog = ref(false)
const selectedUserId = ref<string | null>(null)

// Delete confirmation
const confirmDialog = ref(false)
const userToDelete = ref<User | null>(null)
const deleting = ref(false)

// Profile pictures - track failed image loads
const failedImages = ref<Record<string, boolean>>({})

// Methods
const loadUsers = async () => {
  if (!props.committeeId) return
  loading.value = true
  try {
    const response = await CouncilCommitteesService.listUsersInCouncilCommittee(props.committeeId)
    // API returns { data, success, message } wrapper
    users.value = response.data || response || []
    emit('update:count', users.value.length)
  } catch (error) {
    console.error('Failed to load users:', error)
  } finally {
    loading.value = false
  }
}

const loadRoles = async () => {
  if (roles.value.length > 0) return
  try {
    const response = await LookupsService.listCommitteeRoles()
    roles.value = response.data || response || []
  } catch (error) {
    console.error('Failed to load roles:', error)
  }
}

const loadPrivacies = async () => {
  if (privacies.value.length > 0) return
  try {
    const response = await LookupsService.listPrivacies()
    privacies.value = response.data || response || []
  } catch (error) {
    console.error('Failed to load privacies:', error)
  }
}

const searchUsers = async (query: string) => {
  if (!query || query.length < 1) {
    userOptions.value = []
    return
  }
  try {
    const results = await UsersService.searchLocalUsers(query)
    userOptions.value = results.map((u: any) => ({
      value: u.id,
      label: u.name
    }))
  } catch (error) {
    console.error('Failed to search users:', error)
  }
}

const openAddUser = async () => {
  isEditUser.value = false
  editingUserName.value = ''
  userForm.value = { userId: '', roleId: '', privacyId: '', active: true, note: '' }
  await Promise.all([loadRoles(), loadPrivacies()])
  userDialog.value = true
}

const openEditUser = async (user: User) => {
  isEditUser.value = true
  editingUserName.value = user.fullname
  await Promise.all([loadRoles(), loadPrivacies()])
  // Set form after lookups are loaded - keep IDs as numbers to match Select options
  // Use String() to match ListItemDto.Id which is string
  userForm.value = {
    userId: user.id,
    roleId: String(user.committeeRoleId),
    privacyId: String(user.privacyId),
    active: user.active,
    note: user.note || ''
  }
  userDialog.value = true
}

const saveUser = async () => {
  if (!props.committeeId || !userForm.value.userId || !userForm.value.roleId) return

  savingUser.value = true
  try {
    if (isEditUser.value) {
      await CouncilCommitteesService.updateCommitteeUser(props.committeeId, {
        UserId: String(userForm.value.userId),
        CommitteeRoleId: Number(userForm.value.roleId),
        PrivacyId: Number(userForm.value.privacyId),
        Active: userForm.value.active,
        Note: userForm.value.note || null
      })
    } else {
      await CouncilCommitteesService.addUserToCouncilCommittee({
        CommitteeId: Number(props.committeeId),
        UserId: String(userForm.value.userId),
        CommitteeRoleId: Number(userForm.value.roleId),
        PrivacyId: Number(userForm.value.privacyId),
        Active: userForm.value.active,
        Note: userForm.value.note || null
      })
    }
    userDialog.value = false
    await loadUsers()
  } catch (error) {
    console.error('Failed to save user:', error)
  } finally {
    savingUser.value = false
  }
}

const removeUser = (user: User) => {
  userToDelete.value = user
  confirmDialog.value = true
}

const confirmDelete = async () => {
  if (!props.committeeId || !userToDelete.value) return
  deleting.value = true
  try {
    await CouncilCommitteesService.removeUserFromCouncilCommittee(props.committeeId, userToDelete.value.id)
    confirmDialog.value = false
    await loadUsers()
  } catch (error) {
    console.error('Failed to remove user:', error)
  } finally {
    deleting.value = false
  }
}

const openUserPermissions = (user: User) => {
  selectedUserId.value = user.id
  permissionsDialog.value = true
}

const getInitials = (name: string) => {
  if (!name) return '?'
  const parts = name.split(' ')
  return parts.length > 1 ? parts[0][0] + parts[1][0] : name.substring(0, 2)
}

const getProfilePicture = (userId: string) => {
  return `/api/users/profile-image/${userId}`
}

const handleImageError = (event: Event, userId: string) => {
  failedImages.value[userId] = true
}

// Watch for committeeId changes
watch(() => props.committeeId, () => {
  if (props.committeeId) {
    loadUsers()
  }
}, { immediate: true })

// Expose refresh method
defineExpose({ refresh: loadUsers })
</script>

<style scoped>
.tab-panel {
  @apply p-4;
}

.panel-header {
  @apply flex items-center justify-between mb-4;
}

.panel-title {
  @apply font-semibold text-zinc-900;
}

/* Users Grid */
.users-grid {
  @apply grid grid-cols-1 lg:grid-cols-2 gap-4;
}

.user-card {
  @apply flex items-center gap-4 p-4 bg-zinc-50 rounded-xl border border-zinc-100 transition-all;
  @apply hover:border-primary/20 hover:shadow-sm;
}

.user-avatar {
  @apply w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center text-primary font-bold text-sm flex-shrink-0 overflow-hidden;
}

.avatar-img {
  @apply w-full h-full object-cover;
}

.avatar-initials {
  @apply uppercase;
}

.user-info {
  @apply flex-1 min-w-0;
}

.user-name {
  @apply font-semibold text-zinc-900 truncate;
}

.user-role {
  @apply text-sm text-primary;
}

.user-email {
  @apply text-xs text-zinc-400 truncate;
}

.user-status {
  @apply px-2 py-1 text-xs rounded-full bg-zinc-200 text-zinc-600;
}

.user-status.active {
  @apply bg-success/10 text-success;
}

.user-actions {
  @apply flex items-center gap-1;
}

.action-btn {
  @apply p-2 rounded-lg transition-colors;
}

.action-btn.edit { @apply text-zinc-400 hover:text-primary hover:bg-primary/10; }
.action-btn.permissions { @apply text-zinc-400 hover:text-info hover:bg-info/10; }
.action-btn.delete { @apply text-zinc-400 hover:text-error hover:bg-error/10; }

/* Empty State */
.empty-state {
  @apply flex flex-col items-center justify-center py-12 col-span-full;
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

.toggle-label {
  @apply text-sm font-medium text-zinc-700;
}

/* Responsive */
@media (max-width: 1024px) {
  .users-grid {
    @apply grid-cols-1;
  }
}
</style>

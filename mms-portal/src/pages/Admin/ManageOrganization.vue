<template>
  <div class="space-y-6">
    <div class="page-header">
      <h1 class="page-title">{{ $t('ManageOrganization') }}</h1>
    </div>

    <Card :loading="loading">
      <Tabs v-model="activeTab" :tabs="tabs">
        <!-- Structures Tab -->
        <template #structures>
          <div class="grid grid-cols-12 gap-6 mt-6">
            <!-- Tree Panel -->
            <div class="col-span-4 border-e border-zinc-200 pe-4">
              <div class="flex items-center gap-2 mb-4">
                <Button
                  variant="primary"
                  size="sm"
                  icon-left="mdi:folder-plus"
                  @click="addNewStructure"
                >
                  {{ $t('Add') }}
                </Button>
                <label class="flex items-center gap-2 text-sm">
                  <input
                    v-model="onlyActive"
                    type="checkbox"
                    class="rounded border-zinc-300 text-primary focus:ring-primary"
                    @change="listOrganization"
                  />
                  {{ $t('ShowDeactivatedStructures') }}
                </label>
              </div>

              <Input
                v-model="search"
                :placeholder="$t('Search')"
                icon-left="mdi:magnify"
                class="mb-4"
              />

              <div class="border border-zinc-200 rounded-lg p-4 max-h-96 overflow-y-auto">
                <TreeView
                  :items="filteredStructures"
                  item-key="id"
                  item-text="name"
                  @select="viewStructureDetails"
                />
              </div>
            </div>

            <!-- Details Panel -->
            <div class="col-span-8">
              <template v-if="selectedStructure">
                <div class="flex items-center justify-between mb-6">
                  <h3 class="text-lg font-semibold">{{ selectedStructure.name }}</h3>
                  <div class="flex gap-2">
                    <Button
                      variant="danger"
                      size="sm"
                      icon-left="mdi:trash-can-outline"
                      @click="deleteStructure"
                    >
                      {{ $t('Delete') }}
                    </Button>
                    <Button
                      variant="primary"
                      size="sm"
                      icon-left="mdi:pencil"
                      @click="editStructure"
                    >
                      {{ $t('Edit') }}
                    </Button>
                  </div>
                </div>

                <Card>
                  <template #header>
                    <div class="flex items-center justify-between">
                      <h4 class="font-semibold">{{ $t('UsersInStructure') }}</h4>
                      <Button
                        variant="primary"
                        size="sm"
                        icon-left="mdi:account-plus"
                        @click="openUserAddDialog"
                      >
                        {{ $t('AddUser') }}
                      </Button>
                    </div>
                  </template>

                  <DataTable
                    :columns="userColumns"
                    :data="users"
                    :loading="loadingUsers"
                  >
                    <template #cell-options="{ row }">
                      <Button
                        variant="ghost"
                        size="sm"
                        icon-left="mdi:trash-can-outline"
                        class="text-error"
                        @click="removeUserFromStructure(row.id)"
                      />
                    </template>
                  </DataTable>
                </Card>
              </template>

              <div v-else class="flex items-center justify-center h-64 text-zinc-500">
                <div class="text-center">
                  <Icon icon="mdi:folder-outline" class="w-16 h-16 mx-auto mb-4 text-zinc-300" />
                  <p>{{ $t('NoItemsToShow') }}</p>
                </div>
              </div>
            </div>
          </div>
        </template>

        <!-- Users Tab -->
        <template #users>
          <div class="mt-6">
            <div class="flex items-center gap-4 mb-6">
              <Combobox
                v-model="selectedUserId"
                :options="userOptions"
                :placeholder="$t('SelectUserSearch')"
                class="w-80"
              />
              <Button variant="primary" icon-left="mdi:account-search" @click="searchUser">
                {{ $t('Search') }}
              </Button>
              <Button variant="secondary" icon-left="mdi:refresh" @click="resetUserSearch">
                {{ $t('Reset') }}
              </Button>
              <div class="flex-1"></div>
              <label class="flex items-center gap-2 text-sm">
                <input
                  v-model="onlyActiveUsers"
                  type="checkbox"
                  class="rounded border-zinc-300 text-primary focus:ring-primary"
                  @change="listAdminUsers"
                />
                {{ $t('InactiveUsers') }}
              </label>
            </div>

            <Card>
              <template #header>
                <Button
                  variant="primary"
                  size="sm"
                  icon-left="mdi:account-plus"
                  @click="openAddUserDialog"
                >
                  {{ $t('AddUser') }}
                </Button>
              </template>

              <DataTable
                :columns="adminUserColumns"
                :data="adminUsers"
                :loading="loadingAdminUsers"
                :total="totalCount"
                :page-size="10"
                server-side
                expandable
                @page-change="listAdminUsers"
                @row-expand="expandUser"
              >
                <template #cell-approved="{ row }">
                  <span
                    :class="[
                      'inline-flex items-center px-2 py-1 rounded-full text-xs',
                      row.approved ? 'bg-success-50 text-success' : 'bg-error-50 text-error'
                    ]"
                  >
                    <Icon :icon="row.approved ? 'mdi:account-check-outline' : 'mdi:account-remove-outline'" class="w-4 h-4" />
                  </span>
                </template>

                <template #cell-smsNotification="{ row }">
                  <label class="relative inline-flex items-center cursor-pointer">
                    <input
                      v-model="row.smsEnabled"
                      type="checkbox"
                      class="sr-only peer"
                      @change="enableSms(row)"
                    />
                    <div class="w-9 h-5 bg-zinc-200 peer-focus:ring-2 peer-focus:ring-primary rounded-full peer peer-checked:after:translate-x-full peer-checked:bg-primary after:content-[''] after:absolute after:top-[2px] after:start-[2px] after:bg-white after:rounded-full after:h-4 after:w-4 after:transition-all rtl:peer-checked:after:-translate-x-full"></div>
                  </label>
                </template>

                <template #cell-emailNotification="{ row }">
                  <label class="relative inline-flex items-center cursor-pointer">
                    <input
                      v-model="row.emailNotificationEnabled"
                      type="checkbox"
                      class="sr-only peer"
                      @change="enableEmail(row)"
                    />
                    <div class="w-9 h-5 bg-zinc-200 peer-focus:ring-2 peer-focus:ring-primary rounded-full peer peer-checked:after:translate-x-full peer-checked:bg-primary after:content-[''] after:absolute after:top-[2px] after:start-[2px] after:bg-white after:rounded-full after:h-4 after:w-4 after:transition-all rtl:peer-checked:after:-translate-x-full"></div>
                  </label>
                </template>

                <template #cell-options="{ row }">
                  <div class="flex items-center gap-2">
                    <Button
                      variant="ghost"
                      size="sm"
                      icon-left="mdi:pencil"
                      class="text-primary"
                      @click="editUser(row)"
                    />
                    <Button
                      variant="ghost"
                      size="sm"
                      icon-left="mdi:shield-half-full"
                      class="text-warning"
                      @click="openUserPermissions(row)"
                    />
                    <Button
                      variant="ghost"
                      size="sm"
                      icon-left="mdi:lock-reset"
                      class="text-secondary"
                      @click="openResetPassword(row)"
                    />
                  </div>
                </template>

                <template #expanded-row="{ row }">
                  <div class="p-4 bg-zinc-50">
                    <div v-if="loadingRoles" class="flex items-center justify-center py-4">
                      <Icon icon="mdi:loading" class="w-6 h-6 animate-spin text-primary" />
                    </div>
                    <table v-else-if="row.structuresRoles?.length" class="w-full text-sm">
                      <thead>
                        <tr class="border-b">
                          <th class="text-start py-2">{{ $t('Role') }}</th>
                          <th class="text-start py-2">{{ $t('Structure') }}</th>
                          <th class="text-start py-2">{{ $t('IsPrimary') }}</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr v-for="(structure, idx) in row.structuresRoles" :key="idx" class="border-b">
                          <td class="py-2">{{ structure.roleName }}</td>
                          <td class="py-2">{{ structure.structureName }}</td>
                          <td class="py-2">
                            <Icon
                              :icon="structure.isPrimary ? 'mdi:check' : 'mdi:close'"
                              :class="structure.isPrimary ? 'text-success' : 'text-error'"
                              class="w-5 h-5"
                            />
                          </td>
                        </tr>
                      </tbody>
                    </table>
                    <p v-else class="text-zinc-500 text-center py-4">
                      {{ $t('NoItemsToShow') }}
                    </p>
                  </div>
                </template>
              </DataTable>
            </Card>
          </div>
        </template>
      </Tabs>
    </Card>

    <!-- Add User to Structure Dialog -->
    <Modal
      v-model="userAddDialog"
      :title="$t('AddUser')"
      size="md"
    >
      <div class="space-y-4">
        <Combobox
          v-model="addUserToStructureData.userId"
          :options="userOptions"
          :label="$t('SelectUserSearch')"
          required
        />
        <Select
          v-model="addUserToStructureData.roleId"
          :options="roleOptions"
          :label="$t('SelectRole')"
          required
        />
      </div>
      <template #footer>
        <Button variant="secondary" @click="userAddDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="primary" @click="saveUserToStructure">
          {{ $t('Add') }}
        </Button>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'
import Button from '@/components/ui/Button.vue'
import Input from '@/components/ui/Input.vue'
import Modal from '@/components/ui/Modal.vue'
import Tabs from '@/components/ui/Tabs.vue'
import DataTable from '@/components/ui/DataTable.vue'
import TreeView from '@/components/ui/TreeView.vue'
import Select from '@/components/ui/Select.vue'
import Combobox from '@/components/ui/Combobox.vue'
import StructuresService, { type Structure, type StructureUser } from '@/services/StructuresService'
import UsersService from '@/services/UsersService'
import LookupsService from '@/services/LookupsService'
import { useToast } from '@/composables/useToast'
import { useConfirm } from '@/composables/useConfirm'
import { useI18n } from 'vue-i18n'

const { toast } = useToast()
const { confirm } = useConfirm()
const { t } = useI18n()

const loading = ref(false)
const loadingUsers = ref(false)
const loadingAdminUsers = ref(false)
const loadingRoles = ref(false)
const activeTab = ref('structures')

const tabs = [
  { key: 'structures', label: t('Structures'), icon: 'mdi:sitemap' },
  { key: 'users', label: t('Users'), icon: 'mdi:account' }
]

// Structures tab state
const onlyActive = ref(true)
const search = ref('')
const structures = ref<Structure[]>([])
const selectedStructure = ref<Structure | null>(null)
const users = ref<StructureUser[]>([])
const roles = ref<any[]>([])
const userAddDialog = ref(false)
const addUserToStructureData = ref({
  userId: null as string | null,
  roleId: null as number | null
})

// Users tab state
const onlyActiveUsers = ref(true)
const selectedUserId = ref<number | null>(null)
const adminUsers = ref<any[]>([])
const totalCount = ref(0)
const userOptions = ref<any[]>([])

const userColumns = [
  { key: 'id', label: t('Id') },
  { key: 'fullname', label: t('FullName') },
  { key: 'roleName', label: t('Role') },
  { key: 'email', label: t('Email') },
  { key: 'options', label: t('Options') }
]

const adminUserColumns = [
  { key: 'id', label: t('Id') },
  { key: 'fullnameAr', label: t('FullNameAr') },
  { key: 'fullnameEn', label: t('FullNameEn') },
  { key: 'username', label: t('Username') },
  { key: 'email', label: t('Email') },
  { key: 'smsNotification', label: t('EnableSms') },
  { key: 'emailNotification', label: t('EnableEmail') },
  { key: 'approved', label: t('Approved') },
  { key: 'options', label: t('Options') }
]

const filteredStructures = computed(() => {
  if (!search.value) return structures.value
  const searchLower = search.value.toLowerCase()
  return filterTree(structures.value, searchLower)
})

const roleOptions = computed(() =>
  roles.value.map(r => ({ value: r.id, label: r.name }))
)

function filterTree(items: Structure[], search: string): Structure[] {
  return items
    .filter(item => {
      const matches = item.name.toLowerCase().includes(search)
      const hasMatchingChildren = item.children?.length && filterTree(item.children, search).length
      return matches || hasMatchingChildren
    })
    .map(item => ({
      ...item,
      children: item.children ? filterTree(item.children, search) : []
    }))
}

const listOrganization = async () => {
  loading.value = true
  try {
    structures.value = await StructuresService.listOrganization(!onlyActive.value)
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const viewStructureDetails = async (item: Structure) => {
  selectedStructure.value = item
  loadingUsers.value = true
  try {
    users.value = await StructuresService.listUsersInStructure(item.id)
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loadingUsers.value = false
  }
}

const addNewStructure = () => {
  // Open structure form dialog (to be implemented)
  toast.info(t('FeatureComingSoon'))
}

const editStructure = () => {
  // Open structure form dialog for editing (to be implemented)
  toast.info(t('FeatureComingSoon'))
}

const deleteStructure = async () => {
  if (!selectedStructure.value) return

  const confirmed = await confirm(t('Delete'), t('ConfirmDelete'))
  if (!confirmed) return

  loading.value = true
  try {
    await StructuresService.removeStructure(selectedStructure.value.id)
    selectedStructure.value = null
    await listOrganization()
    toast.success(t('DeleteSuccess'))
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const openUserAddDialog = async () => {
  addUserToStructureData.value = { userId: null, roleId: null }
  try {
    roles.value = await LookupsService.listRoles()
  } catch {
    // ignore
  }
  userAddDialog.value = true
}

const saveUserToStructure = async () => {
  if (!selectedStructure.value || !addUserToStructureData.value.userId || !addUserToStructureData.value.roleId) {
    toast.error(t('RequiredField'))
    return
  }

  try {
    await StructuresService.addUserToStructure(
      selectedStructure.value.id,
      addUserToStructureData.value.userId,
      addUserToStructureData.value.roleId
    )
    userAddDialog.value = false
    await viewStructureDetails(selectedStructure.value)
    toast.success(t('AddSuccess'))
  } catch {
    toast.error(t('ErrorOccurred'))
  }
}

const removeUserFromStructure = async (userId: string) => {
  if (!selectedStructure.value) return

  const confirmed = await confirm(t('Delete'), t('ConfirmDelete'))
  if (!confirmed) return

  try {
    await StructuresService.removeUserFromStructure(selectedStructure.value.id, userId)
    await viewStructureDetails(selectedStructure.value)
    toast.success(t('DeleteSuccess'))
  } catch {
    toast.error(t('ErrorOccurred'))
  }
}

const listAdminUsers = async (options?: { page: number; itemsPerPage: number }) => {
  const page = options?.page || 1
  const pageSize = options?.itemsPerPage || 10

  loadingAdminUsers.value = true
  try {
    const response = await UsersService.listAdminUsers(page, pageSize, onlyActiveUsers.value)
    adminUsers.value = response.data || []
    totalCount.value = response.total || 0
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loadingAdminUsers.value = false
  }
}

const expandUser = async (data: { item: any; value: boolean }) => {
  if (data.value) {
    loadingRoles.value = true
    try {
      const roles = await UsersService.listUserRolesStructures(data.item.id)
      data.item.structuresRoles = roles
    } catch {
      toast.error(t('ErrorOccurred'))
    } finally {
      loadingRoles.value = false
    }
  }
}

const searchUser = async () => {
  if (!selectedUserId.value) {
    toast.error(t('SelectUser'))
    return
  }

  loadingAdminUsers.value = true
  try {
    const user = await UsersService.getUser(selectedUserId.value)
    adminUsers.value = user ? [user] : []
    totalCount.value = adminUsers.value.length
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loadingAdminUsers.value = false
  }
}

const resetUserSearch = () => {
  selectedUserId.value = null
  listAdminUsers()
}

const enableSms = async (user: any) => {
  try {
    await UsersService.enableSms(user.id, user.smsEnabled)
    toast.success(user.smsEnabled ? (t('SmsEnabled')) : (t('SmsDisabled')))
  } catch {
    toast.error(t('ErrorOccurred'))
  }
}

const enableEmail = async (user: any) => {
  try {
    await UsersService.enableEmail(user.id, user.emailNotificationEnabled)
    toast.success(user.emailNotificationEnabled ? (t('emailEnabled')) : (t('emailDisabled')))
  } catch {
    toast.error(t('ErrorOccurred'))
  }
}


const openAddUserDialog = () => {
  toast.info(t('FeatureComingSoon'))
}

const editUser = (user: any) => {
  toast.info(t('FeatureComingSoon'))
}

const openUserPermissions = (user: any) => {
  toast.info(t('FeatureComingSoon'))
}

const openResetPassword = (user: any) => {
  toast.info(t('FeatureComingSoon'))
}

onMounted(() => {
  listOrganization()
})
</script>

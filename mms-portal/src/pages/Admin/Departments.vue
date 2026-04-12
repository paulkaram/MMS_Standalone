<template>
  <div class="departments-page">
    <!-- Page Header -->
    <PageHeader :title="$t('Departments')" :subtitle="$t('ManageDepartments')" :breadcrumbs="[{ label: 'Settings', to: '/settings' }]" />

    <!-- Main Content -->
    <div class="content-card">
      <div class="split-layout">
        <!-- Left Panel - Tree -->
        <div class="tree-panel">
          <div class="panel-header">
            <h3 class="panel-title">
              <Icon icon="mdi:file-tree" class="w-5 h-5" />
              {{ $t('OrganizationStructure') }}
            </h3>
            <label class="filter-toggle">
              <input
                v-model="showInactive"
                type="checkbox"
                class="toggle-input"
                @change="loadDepartments"
              />
              <span class="toggle-label">{{ $t('ShowInactive') }}</span>
            </label>
          </div>

          <div class="search-box">
            <Icon icon="mdi:magnify" class="search-icon" />
            <input
              v-model="treeSearch"
              type="text"
              class="search-input"
              :placeholder="$t('SearchDepartments')"
            />
          </div>

          <div class="tree-container" :class="{ 'is-loading': loadingTree }">
            <div v-if="loadingTree" class="tree-loading">
              <Icon icon="mdi:loading" class="w-6 h-6 animate-spin text-primary" />
            </div>
            <div v-else-if="filteredDepartments.length === 0" class="tree-empty">
              <Icon icon="mdi:folder-off-outline" class="w-12 h-12 text-zinc-200" />
              <p>{{ $t('NoDepartments') }}</p>
            </div>
            <div v-else class="tree-content">
              <TreeNode
                v-for="dept in filteredDepartments"
                :key="dept.id"
                :node="dept"
                :selected-id="selectedDepartment?.id"
                :expanded-ids="expandedIds"
                @select="selectDepartment"
                @toggle="toggleNode"
              />
            </div>
          </div>
        </div>

        <!-- Right Panel - Details -->
        <div class="details-panel">
          <template v-if="selectedDepartment">
            <div class="details-header">
              <div class="dept-info">
                <span class="dept-type-badge">
                  <Icon icon="mdi:domain" class="w-4 h-4" />
                  {{ $t('Department') }}
                </span>
                <h3 class="dept-name">{{ selectedDepartment.name }}</h3>
                <span class="dept-members-count">({{ departmentUsers.length }} {{ $t('Members') }})</span>
              </div>
              <span class="text-xs text-zinc-400 bg-zinc-100 px-2 py-1 rounded">IAM</span>
            </div>

            <!-- Users in Department -->
            <div class="users-section">
              <div class="section-header">
                <h4 class="section-title">
                  <Icon icon="mdi:account-group" class="w-5 h-5" />
                  {{ $t('UsersInDepartment') }}
                </h4>
              </div>

              <div class="users-table-container" :class="{ 'is-loading': loadingUsers }">
                <div v-if="loadingUsers" class="users-loading">
                  <Icon icon="mdi:loading" class="w-6 h-6 animate-spin text-primary" />
                </div>
                <table v-else-if="departmentUsers.length > 0" class="users-table">
                  <thead>
                    <tr>
                      <th>#</th>
                      <th>{{ $t('FullName') }}</th>
                      <th>{{ $t('Role') }}</th>
                      <th>{{ $t('Email') }}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(user, index) in departmentUsers" :key="user.id">
                      <td>
                        <span class="id-badge">{{ index + 1 }}</span>
                      </td>
                      <td>
                        <div class="user-cell">
                          <UserAvatar :userId="user.id" :name="user.fullname || ''" size="sm" />
                          <span>{{ user.fullname }}</span>
                        </div>
                      </td>
                      <td>
                        <span class="role-badge">{{ user.roleName }}</span>
                      </td>
                      <td>{{ user.email || '-' }}</td>
                    </tr>
                  </tbody>
                </table>
                <div v-else class="users-empty">
                  <Icon icon="mdi:account-group-outline" class="w-12 h-12 text-zinc-200" />
                  <p>{{ $t('NoUsersInDepartment') }}</p>
                </div>
              </div>
            </div>
          </template>

          <div v-else class="no-selection">
            <Icon icon="mdi:folder-open-outline" class="w-20 h-20 text-zinc-200" />
            <p class="no-selection-text">{{ $t('SelectDepartment') }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Add/Edit Department Dialog -->
    <Modal
      v-model="deptDialog"
      :title="isEdit ? ($t('EditDepartment')) : ($t('AddDepartment'))"
      :icon="isEdit ? 'mdi:folder-edit' : 'mdi:folder-plus'"
      size="lg"
    >
      <form @submit.prevent="saveDepartment" class="form-content">
        <!-- Active Switch -->
        <div class="form-group">
          <div class="toggle-group">
            <label class="form-label">
              <Icon icon="mdi:check-circle" class="w-4 h-4" />
              {{ $t('Status') }}
            </label>
            <label class="toggle-switch-modern">
              <input v-model="deptForm.active" type="checkbox" />
              <span class="toggle-track-modern"></span>
              <span class="toggle-status">{{ deptForm.active ? ($t('Active')) : ($t('Inactive')) }}</span>
            </label>
          </div>
        </div>

        <!-- Names Row -->
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">
              <Icon icon="mdi:abjad-arabic" class="w-4 h-4" />
              {{ $t('NameAr') }} <span class="required">*</span>
            </label>
            <input
              v-model="deptForm.nameAr"
              type="text"
              class="form-input"
              dir="rtl"
              required
            />
          </div>

          <div class="form-group">
            <label class="form-label">
              <Icon icon="mdi:alphabetical" class="w-4 h-4" />
              {{ $t('NameEn') }} <span class="required">*</span>
            </label>
            <input
              v-model="deptForm.nameEn"
              type="text"
              class="form-input"
              dir="ltr"
              required
            />
          </div>
        </div>

        <!-- Code and Type Row -->
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">
              <Icon icon="mdi:identifier" class="w-4 h-4" />
              {{ $t('Code') }} <span class="required">*</span>
            </label>
            <input
              v-model="deptForm.code"
              type="text"
              class="form-input"
              required
            />
          </div>

          <div class="form-group">
            <CustomSelect
              v-model="deptForm.typeId"
              :options="structureTypes"
              :label="$t('Type')"
              :placeholder="$t('SelectType')"
              value-key="id"
              label-key="name"
              clearable
            />
          </div>
        </div>

        <!-- Parent and Branch Row -->
        <div class="form-row">
          <div class="form-group">
            <CustomSelect
              v-model="deptForm.parentId"
              :options="structuresList"
              :label="$t('ParentDepartment')"
              :placeholder="$t('NoParent')"
              value-key="id"
              label-key="name"
              clearable
              searchable
            />
          </div>

          <div class="form-group">
            <CustomSelect
              v-model="deptForm.branchId"
              :options="branches"
              :label="$t('Branch')"
              :placeholder="$t('SelectBranch')"
              value-key="id"
              label-key="name"
              clearable
            />
          </div>
        </div>

        <!-- Description -->
        <div class="form-group">
          <label class="form-label">
            <Icon icon="mdi:text" class="w-4 h-4" />
            {{ $t('Description') }}
          </label>
          <textarea
            v-model="deptForm.description"
            class="form-input form-textarea"
            rows="3"
          ></textarea>
        </div>
      </form>

      <template #footer>
        <Button variant="outline" @click="deptDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="primary" :loading="saving" @click="saveDepartment">
          <Icon v-if="!saving" icon="mdi:check" class="w-4 h-4" />
          {{ isEdit ? ($t('SaveChanges')) : ($t('Add')) }}
        </Button>
      </template>
    </Modal>

    <!-- Add User to Department Dialog -->
    <Modal
      v-model="addUserDialog"
      :title="$t('AddUserToDepartment')"
      icon="mdi:account-plus"
      size="md"
    >
      <form @submit.prevent="saveUserToDepartment" class="form-content">
        <div class="form-group">
          <label class="form-label">
            <Icon icon="mdi:account-search" class="w-4 h-4" />
            {{ $t('SelectUser') }}
          </label>
          <Combobox
            v-model="userForm.userId"
            :options="userOptions"
            :placeholder="$t('SearchUser')"
            :min-search-length="2"
            item-text="label"
            item-value="value"
            @search="searchUsers"
          />
        </div>

        <div class="form-group">
          <CustomSelect
            v-model="userForm.roleId"
            :options="roles"
            :label="$t('SelectRole')"
            :placeholder="$t('SelectRole')"
            value-key="id"
            label-key="name"
          />
        </div>
      </form>

      <template #footer>
        <Button variant="outline" @click="addUserDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="primary" :loading="savingUser" @click="saveUserToDepartment">
          <Icon v-if="!savingUser" icon="mdi:check" class="w-4 h-4" />
          {{ $t('Add') }}
        </Button>
      </template>
    </Modal>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, h, defineComponent } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import Modal from '@/components/ui/Modal.vue'
import Combobox from '@/components/ui/Combobox.vue'
import CustomSelect from '@/components/ui/CustomSelect.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import StructuresService, { type Structure, type StructureUser } from '@/services/StructuresService'
import { useUserStore } from '@/stores/user'

const userStore = useUserStore()
const isArabic = computed(() => userStore.language === 'ar')
import LookupsService from '@/services/LookupsService'
import RolesService from '@/services/RolesService'
import UsersService from '@/services/UsersService'
import { useConfirm } from '@/composables/useConfirm'

const { confirm } = useConfirm()

// Tree Node Component
const TreeNode = defineComponent({
  name: 'TreeNode',
  props: {
    node: { type: Object as () => Structure, required: true },
    selectedId: { type: [Number, String], default: null },
    expandedIds: { type: Array as () => any[], default: () => [] },
    level: { type: Number, default: 0 }
  },
  emits: ['select', 'toggle'],
  setup(props, { emit }) {
    const hasChildren = computed(() => props.node.children && props.node.children.length > 0)
    const isExpanded = computed(() => props.expandedIds.includes(props.node.id))
    const isSelected = computed(() => props.selectedId === props.node.id)

    return () => h('div', { class: 'tree-node' }, [
      h('div', {
        class: ['tree-item', { selected: isSelected.value }],
        style: { paddingInlineStart: `${props.level * 20 + 8}px` },
        onClick: () => emit('select', props.node)
      }, [
        hasChildren.value ? h('button', {
          class: 'expand-icon',
          onClick: (e: Event) => { e.stopPropagation(); emit('toggle', props.node.id) }
        }, [
          h(Icon, { icon: isExpanded.value ? 'mdi:chevron-down' : (document.documentElement.dir === 'rtl' ? 'mdi:chevron-left' : 'mdi:chevron-right'), class: 'w-4 h-4' })
        ]) : h('span', { class: 'expand-placeholder' }),
        h(Icon, { icon: isExpanded.value ? 'mdi:folder-open' : 'mdi:folder', class: 'folder-icon w-5 h-5' }),
        h('span', { class: 'node-label' }, props.node.name)
      ]),
      hasChildren.value && isExpanded.value ? h('div', { class: 'tree-children' },
        props.node.children!.map(child =>
          h(TreeNode, {
            key: child.id,
            node: child,
            selectedId: props.selectedId,
            expandedIds: props.expandedIds,
            level: props.level + 1,
            onSelect: (n: Structure) => emit('select', n),
            onToggle: (id: number) => emit('toggle', id)
          })
        )
      ) : null
    ])
  }
})

// Types
interface DepartmentForm {
  nameAr: string
  nameEn: string
  code: string
  description: string
  typeId: number | null
  parentId: number | null
  branchId: number | null
  active: boolean
}

// State
const loadingTree = ref(false)
const loadingUsers = ref(false)
const saving = ref(false)
const savingUser = ref(false)
const deleting = ref(false)

const departments = ref<Structure[]>([])
const selectedDepartment = ref<Structure | null>(null)
const departmentUsers = ref<StructureUser[]>([])
const roles = ref<any[]>([])
const structureTypes = ref<any[]>([])
const structuresList = ref<any[]>([])
const branches = ref<any[]>([])
const expandedIds = ref<any[]>([])
const treeSearch = ref('')
const showInactive = ref(false)

// Dialogs
const deptDialog = ref(false)
const isEdit = ref(false)
const addUserDialog = ref(false)

const deptForm = ref<DepartmentForm>({
  nameAr: '',
  nameEn: '',
  code: '',
  description: '',
  typeId: null,
  parentId: null,
  branchId: null,
  active: true
})

const userForm = ref({
  userId: null as string | null,
  roleId: null as number | null
})

const userOptions = ref<any[]>([])

// Computed
const totalDepartments = computed(() => countDepartments(departments.value))
const usersInSelectedDept = computed(() => departmentUsers.value.length)

const filteredDepartments = computed(() => {
  if (!treeSearch.value) return departments.value
  return filterTree(departments.value, treeSearch.value.toLowerCase())
})

const flatDepartments = computed(() => flattenTree(departments.value))

// Methods
function countDepartments(items: Structure[]): number {
  let count = items.length
  for (const item of items) {
    if (item.children) {
      count += countDepartments(item.children)
    }
  }
  return count
}

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

function flattenTree(items: Structure[], result: Structure[] = []): Structure[] {
  for (const item of items) {
    result.push(item)
    if (item.children) {
      flattenTree(item.children, result)
    }
  }
  return result
}

function getDepartmentPath(dept: Structure): string {
  // Simple implementation - could be improved with parent lookup
  return ''
}

const loadDepartments = async () => {
  loadingTree.value = true
  try {
    const response: any = await StructuresService.listIamOrganization()
    const data = response?.data || response
    const flat: any[] = Array.isArray(data) ? data : (data?.items || data?.value || [])
    // Normalize IAM fields and build tree from flat array
    departments.value = buildTree(flat)
    // Auto-expand all nodes
    expandedIds.value = flat.map((d: any) => d.id ?? d.Id)
  } catch (error) {
    console.error('Failed to load departments from IAM:', error)
    try {
      const fallback = await StructuresService.listOrganization(!showInactive.value)
      departments.value = Array.isArray(fallback) ? fallback : (fallback?.data || [])
    } catch {
      departments.value = []
    }
  } finally {
    loadingTree.value = false
  }
}

// Build tree from flat IAM department array
function buildTree(flat: any[]): Structure[] {
  const map = new Map<any, Structure>()
  const roots: Structure[] = []

  // First pass: create nodes
  for (const item of flat) {
    const id = item.id ?? item.Id
    const parentId = item.parentId ?? item.ParentId ?? item.parentDepartmentId ?? null
    const name = item.name ?? item.Name ?? item.nameAr ?? item.NameAr ?? ''
    map.set(id, { id, name, nameAr: item.nameAr ?? item.NameAr ?? name, nameEn: item.nameEn ?? item.NameEn ?? name, parentId, children: [], active: item.isActive ?? item.IsActive ?? true })
  }

  // Second pass: link parents
  for (const [id, node] of map) {
    if (node.parentId && map.has(node.parentId)) {
      map.get(node.parentId)!.children!.push(node)
    } else {
      roots.push(node)
    }
  }

  return roots
}

const loadRoles = async () => {
  try {
    const response = await RolesService.listRoles()
    const data = Array.isArray(response) ? response : (response?.data?.data || response?.data || [])
    // Map to ensure name field exists based on language
    roles.value = data.map((item: any) => ({
      ...item,
      name: item.name || (isArabic.value ? (item.nameAr || item.nameEn) : (item.nameEn || item.nameAr)) || item.label
    }))
  } catch (error) {
    console.error('Failed to load roles:', error)
  }
}

const loadLookups = async () => {
  try {
    const [typesRes, structuresRes, branchesRes] = await Promise.all([
      LookupsService.listStructureTypes(),
      LookupsService.listStructures(),
      LookupsService.listBranches()
    ])

    const extractData = (res: any) => {
      const data = Array.isArray(res) ? res : (res?.data?.data || res?.data || [])
      // Ensure each item has a 'name' field based on language
      return data.map((item: any) => ({
        ...item,
        name: item.name || (isArabic.value ? (item.nameAr || item.nameEn) : (item.nameEn || item.nameAr)) || item.label
      }))
    }

    structureTypes.value = extractData(typesRes)
    structuresList.value = extractData(structuresRes)
    branches.value = extractData(branchesRes)
  } catch (error) {
    console.error('Failed to load lookups:', error)
  }
}

const selectDepartment = async (dept: Structure) => {
  selectedDepartment.value = dept
  loadingUsers.value = true
  try {
    // Use IAM endpoint (department ID is a GUID string)
    const response: any = await StructuresService.listIamDepartmentUsers(String(dept.id))
    const data = response?.data || response
    const users = Array.isArray(data) ? data : (data?.items || data?.value || [])
    // Normalize IAM user fields to match expected StructureUser shape
    departmentUsers.value = users.map((u: any) => ({
      id: u.id ?? u.Id,
      fullname: u.displayName ?? u.fullName ?? u.name ?? `${u.firstName || ''} ${u.lastName || ''}`.trim(),
      email: u.email ?? u.Email ?? '',
      roleName: u.roleName ?? u.role ?? u.jobTitle ?? '-'
    }))
  } catch (error) {
    console.error('Failed to load users from IAM:', error)
    departmentUsers.value = []
  } finally {
    loadingUsers.value = false
  }
}

const toggleNode = (id: number) => {
  const index = expandedIds.value.indexOf(id)
  if (index > -1) {
    expandedIds.value.splice(index, 1)
  } else {
    expandedIds.value.push(id)
  }
}

const openAddDialog = () => {
  isEdit.value = false
  deptForm.value = {
    nameAr: '',
    nameEn: '',
    code: '',
    description: '',
    typeId: null,
    parentId: null,
    branchId: null,
    active: true
  }
  deptDialog.value = true
}

const editDepartment = async () => {
  if (!selectedDepartment.value) return

  // Fetch full department details
  try {
    const response = await StructuresService.getStructure(selectedDepartment.value.id)
    const dept = (response as any)?.data || response

    isEdit.value = true
    deptForm.value = {
      nameAr: dept.nameAr || dept.name || '',
      nameEn: dept.nameEn || '',
      code: dept.code || '',
      description: dept.description || '',
      typeId: dept.typeId || dept.structureTypeId || null,
      parentId: dept.parentId || null,
      branchId: dept.branchId || null,
      active: dept.active !== false
    }
    deptDialog.value = true
  } catch (error) {
    console.error('Failed to load department details:', error)
  }
}

const saveDepartment = async () => {
  if (!deptForm.value.nameAr) return

  saving.value = true
  try {
    const data = {
      nameAr: deptForm.value.nameAr,
      nameEn: deptForm.value.nameEn,
      code: deptForm.value.code,
      description: deptForm.value.description,
      typeId: deptForm.value.typeId,
      parentId: deptForm.value.parentId,
      branchId: deptForm.value.branchId,
      active: deptForm.value.active
    }

    if (isEdit.value && selectedDepartment.value) {
      await StructuresService.updateStructure(selectedDepartment.value.id, data)
    } else {
      await StructuresService.addStructure(data)
    }

    deptDialog.value = false
    await loadDepartments()
  } catch (error) {
    console.error('Failed to save department:', error)
  } finally {
    saving.value = false
  }
}

const deleteDepartment = async () => {
  if (!selectedDepartment.value) return

  const confirmed = await confirm({
    title: 'حذف القسم',
    message: `هل أنت متأكد من حذف "${selectedDepartment.value.name}"؟`,
    type: 'danger',
    confirmText: 'حذف',
    cancelText: 'إلغاء'
  })

  if (!confirmed) return

  deleting.value = true
  try {
    await StructuresService.removeStructure(selectedDepartment.value.id)
    selectedDepartment.value = null
    departmentUsers.value = []
    await loadDepartments()
  } catch (error) {
    console.error('Failed to delete department:', error)
  } finally {
    deleting.value = false
  }
}

const openAddUserDialog = async () => {
  userForm.value = { userId: null, roleId: null }
  await loadRoles()
  addUserDialog.value = true
}

const searchUsers = async (query: string) => {
  if (!query || query.length < 2) {
    userOptions.value = []
    return
  }
  try {
    const response = await UsersService.searchUsers(query)
    const result = (response as any)?.data || response || []
    userOptions.value = result.map((u: any) => ({
      value: u.id,
      label: u.name || u.fullName || u.userName || u.username
    }))
  } catch (error) {
    console.error('Failed to search users:', error)
    userOptions.value = []
  }
}

const saveUserToDepartment = async () => {
  if (!selectedDepartment.value || !userForm.value.userId || !userForm.value.roleId) return

  savingUser.value = true
  try {
    await StructuresService.addUserToStructure(
      selectedDepartment.value.id,
      parseInt(userForm.value.userId),
      userForm.value.roleId
    )
    addUserDialog.value = false
    await selectDepartment(selectedDepartment.value)
  } catch (error) {
    console.error('Failed to add user:', error)
  } finally {
    savingUser.value = false
  }
}

const removeUserFromDepartment = async (user: StructureUser) => {
  if (!selectedDepartment.value) return

  const confirmed = await confirm({
    title: 'إزالة المستخدم',
    message: `هل أنت متأكد من إزالة "${user.fullname}" من القسم؟`,
    type: 'danger',
    confirmText: 'إزالة',
    cancelText: 'إلغاء'
  })

  if (!confirmed) return

  try {
    await StructuresService.removeUserFromStructure(selectedDepartment.value.id, user.id)
    await selectDepartment(selectedDepartment.value)
  } catch (error) {
    console.error('Failed to remove user:', error)
  }
}

// Lifecycle
onMounted(() => {
  loadDepartments()
  loadLookups()
  loadRoles()
})
</script>

<style scoped>
.departments-page {
  @apply space-y-6;
}

/* Stats Row */
.stats-row {
  @apply grid grid-cols-1 md:grid-cols-2 gap-4;
}

.stat-card {
  @apply flex items-center gap-4 p-4 bg-white rounded-xl border border-gray-200;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}

.stat-icon {
  @apply w-12 h-12 rounded-xl flex items-center justify-center;
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
}

.stat-icon.users { background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%); color: #fff; }

.stat-info {
  @apply flex flex-col;
}

.stat-value {
  @apply text-2xl font-bold text-zinc-900;
}

.stat-label {
  @apply text-sm text-zinc-500;
}

/* Content Card */
.content-card {
  @apply bg-white rounded-2xl border border-gray-200 overflow-hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}

/* Split Layout */
.split-layout {
  @apply flex min-h-[600px];
}

/* Tree Panel */
.tree-panel {
  @apply w-80 flex-shrink-0 border-e border-gray-200 flex flex-col;
}

.panel-header {
  @apply flex items-center justify-between p-4 border-b border-gray-200 bg-zinc-50/50;
}

.panel-title {
  @apply flex items-center gap-2 font-semibold text-zinc-800;
}

.filter-toggle {
  @apply flex items-center gap-2 cursor-pointer;
}

.toggle-input {
  @apply w-4 h-4 rounded border-zinc-300 text-primary focus:ring-primary;
}

.toggle-label {
  @apply text-xs text-zinc-500;
}

.search-box {
  @apply relative p-3 border-b border-gray-200;
}

.search-icon {
  @apply absolute start-6 top-1/2 -translate-y-1/2 w-4 h-4 text-zinc-400;
}

.search-input {
  @apply w-full ps-9 pe-3 py-2 bg-zinc-100 border-0 rounded-lg text-sm;
  @apply focus:outline-none focus:ring-2 focus:ring-primary/20 focus:bg-white;
  @apply placeholder:text-zinc-400;
}

.tree-container {
  @apply flex-1 overflow-y-auto p-2;
}

.tree-loading, .tree-empty {
  @apply flex flex-col items-center justify-center h-full text-zinc-400 gap-2;
}

.tree-content {
  @apply space-y-1;
}

/* Tree Node Styles */
:deep(.tree-node) {
  @apply select-none;
}

:deep(.tree-item) {
  @apply flex items-center gap-2 px-2 py-2 rounded-lg cursor-pointer;
  @apply hover:bg-zinc-100 transition-colors;
}

:deep(.tree-item.selected) {
  @apply bg-primary/10 text-primary;
}

:deep(.expand-icon) {
  @apply p-0.5 rounded hover:bg-zinc-200 text-zinc-400;
}

:deep(.expand-placeholder) {
  @apply w-5;
}

:deep(.folder-icon) {
  @apply text-primary;
}

:deep(.node-label) {
  @apply text-sm font-medium;
}

/* Details Panel */
.details-panel {
  @apply flex-1 flex flex-col;
}

.details-header {
  @apply flex items-center gap-3 p-4 border-b border-gray-200;
}

.dept-info {
  @apply flex items-center gap-3;
}

.dept-type-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 3px 10px;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 600;
  background: rgba(0, 109, 75, 0.1);
  color: #007E65;
  border: 1px solid rgba(0, 109, 75, 0.2);
}

.dept-name {
  @apply font-bold text-zinc-900 text-lg;
  margin: 0;
}

.dept-members-count {
  @apply text-sm text-zinc-400;
}

/* Users Section */
.users-section {
  @apply flex-1 flex flex-col p-4;
}

.section-header {
  @apply flex items-center justify-between mb-4;
}

.section-title {
  @apply flex items-center gap-2 font-semibold text-zinc-800;
}

.users-table-container {
  @apply flex-1 border border-gray-200 rounded-xl overflow-hidden;
}

.users-table-container.is-loading {
  @apply min-h-[200px];
}

.users-loading {
  @apply flex items-center justify-center h-full;
}

.users-table {
  @apply w-full text-sm;
}

.users-table th {
  @apply px-5 py-4 text-start text-xs font-semibold uppercase tracking-wider border-b;
  background-color: #006d4b;
  color: #d4d4d8;
}

.users-table td {
  @apply px-4 py-3 border-b border-gray-200;
}

.users-table tbody tr:hover {
  background-color: rgba(0, 109, 75, 0.05);
}

.id-badge {
  @apply inline-flex items-center justify-center w-7 h-7 bg-zinc-100 text-zinc-600 rounded-lg text-xs font-medium;
}

.user-cell {
  @apply flex items-center gap-2;
}

.role-badge {
  @apply inline-flex px-2 py-1 text-xs font-medium rounded-lg bg-indigo-50 text-indigo-700;
}

.action-btn {
  @apply p-2 rounded-lg transition-all duration-200;
}

.action-btn.delete {
  @apply text-zinc-400 hover:text-error hover:bg-error/10;
}

.users-empty {
  @apply flex flex-col items-center justify-center h-full py-12 text-zinc-400 gap-2;
}

/* No Selection */
.no-selection {
  @apply flex-1 flex flex-col items-center justify-center text-zinc-400;
}

.no-selection-text {
  @apply mt-4 text-lg;
}

/* Form Styles */
.form-content {
  @apply space-y-4;
}

.form-row {
  @apply grid grid-cols-1 md:grid-cols-2 gap-4;
}

.form-group {
  @apply space-y-1.5;
}

.form-label {
  @apply flex items-center gap-2 text-sm font-medium text-zinc-700;
}

.form-label .required {
  @apply text-red-500;
}

.form-input {
  @apply w-full px-4 py-2.5 border border-gray-200 rounded-xl text-sm;
  @apply focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary;
  @apply placeholder:text-zinc-400;
}

.form-textarea {
  @apply resize-none;
}

/* Modern Toggle Switch */
.toggle-group {
  @apply flex items-center justify-between p-3 bg-zinc-50 rounded-xl;
}

.toggle-switch-modern {
  @apply relative inline-flex items-center gap-3 cursor-pointer;
}

.toggle-switch-modern input {
  @apply sr-only;
}

.toggle-track-modern {
  @apply relative w-12 h-6 bg-zinc-300 rounded-full transition-colors duration-200;
  @apply after:content-[''] after:absolute after:top-[3px] after:start-[3px];
  @apply after:bg-white after:rounded-full after:h-[18px] after:w-[18px] after:transition-all after:duration-200;
  @apply after:shadow-sm;
}

.toggle-switch-modern input:checked + .toggle-track-modern {
  @apply bg-primary;
}

.toggle-switch-modern input:checked + .toggle-track-modern::after {
  transform: translateX(24px);
}

[dir="rtl"] .toggle-switch-modern input:checked + .toggle-track-modern::after {
  transform: translateX(-24px);
}

.toggle-status {
  @apply text-sm font-medium text-zinc-600 min-w-[60px];
}

.toggle-switch-modern input:checked ~ .toggle-status {
  @apply text-primary;
}


/* Responsive */
@media (max-width: 1024px) {
  .split-layout {
    @apply flex-col;
  }

  .tree-panel {
    @apply w-full border-e-0 border-b;
    max-height: 300px;
  }
}

@media (max-width: 768px) {
  .details-header {
    @apply flex-col items-start gap-4;
  }

  .details-actions {
    @apply w-full justify-end;
  }
}
</style>

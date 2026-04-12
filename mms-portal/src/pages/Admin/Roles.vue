<template>
  <div class="space-y-5">
    <!-- Page Header -->
    <PageHeader :title="$t('CommitteeRoles')" :subtitle="$t('ManageCommitteeRoles')" :breadcrumbs="[{ label: 'Settings', to: '/settings' }]">
      <template #actions>
        <button class="btn-clean primary" @click="openAddDialog">
          <Icon icon="mdi:plus" class="w-4 h-4" />
          {{ $t('AddRole') }}
        </button>
      </template>
    </PageHeader>

    <!-- Grid Container -->
    <div class="mg-container">
      <!-- Loading -->
      <div v-if="loading" class="mg-state">
        <Icon icon="mdi:loading" class="w-6 h-6 animate-spin" style="color: #008080" />
      </div>

      <!-- Empty -->
      <div v-else-if="filteredRoles.length === 0" class="mg-state">
        <Icon icon="mdi:shield-off-outline" class="w-12 h-12" style="color: #ccc" />
        <p>{{ $t('NoRoles') }}</p>
      </div>

      <!-- Table -->
      <template v-else>
        <!-- Search Bar -->
        <div class="mg-toolbar">
          <div class="mg-search">
            <Icon icon="mdi:magnify" class="w-4 h-4" style="color: #9ca3af" />
            <input v-model="search" type="text" :placeholder="$t('SearchRoles')" />
          </div>
          <span class="mg-count">{{ filteredRoles.length }} {{ $t('Of') }} {{ roles.length }}</span>
        </div>

        <div class="mg-table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>#</th>
                <th>{{ $t('NameAr') }}</th>
                <th>{{ $t('NameEn') }}</th>
                <th>{{ $t('Actions') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(role, index) in paginatedRoles" :key="role.id">
                <td><span class="mg-id">{{ (currentPage - 1) * pageSize + index + 1 }}</span></td>
                <td><span class="mg-title">{{ role.nameAr }}</span></td>
                <td>{{ role.nameEn }}</td>
                <td>
                  <div class="mg-actions">
                    <button class="mg-act" :title="$t('Edit')" @click="editRole(role)">
                      <Icon icon="mdi:pencil-outline" class="w-4 h-4" />
                    </button>
                    <button class="mg-act danger" :title="$t('Delete')" @click="deleteRole(role.id)">
                      <Icon icon="mdi:trash-can-outline" class="w-4 h-4" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div v-if="filteredRoles.length > pageSize" class="mg-pag">
          <span class="mg-pag-info">
            {{ $t('Showing') }} <strong>{{ (currentPage - 1) * pageSize + 1 }}</strong>
            {{ $t('To') }} <strong>{{ Math.min(currentPage * pageSize, filteredRoles.length) }}</strong>
            {{ $t('Of') }} <strong>{{ filteredRoles.length }}</strong>
          </span>
          <div class="mg-pag-btns">
            <button class="mg-pg" :disabled="currentPage === 1" @click="currentPage = 1">
              <Icon icon="mdi:chevron-double-left" class="w-4 h-4" />
            </button>
            <button class="mg-pg" :disabled="currentPage === 1" @click="currentPage--">
              <Icon icon="mdi:chevron-left" class="w-4 h-4" />
            </button>
            <button v-for="p in visiblePages" :key="p" class="mg-pg num" :class="{ active: p === currentPage }" @click="currentPage = p">{{ p }}</button>
            <button class="mg-pg" :disabled="currentPage === totalPages" @click="currentPage++">
              <Icon icon="mdi:chevron-right" class="w-4 h-4" />
            </button>
            <button class="mg-pg" :disabled="currentPage === totalPages" @click="currentPage = totalPages">
              <Icon icon="mdi:chevron-double-right" class="w-4 h-4" />
            </button>
          </div>
        </div>
      </template>
    </div>

    <!-- Add/Edit Dialog -->
    <Modal
      v-model="dialog"
      :title="isEdit ? ($t('EditRole')) : ($t('AddRole'))"
      :icon="isEdit ? 'mdi:pencil' : 'mdi:plus'"
      size="md"
    >
      <form @submit.prevent="save" class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-zinc-700 mb-1">{{ $t('NameAr') }}</label>
          <input v-model="form.nameAr" type="text" dir="rtl" :placeholder="$t('EnterNameAr')" required
            class="w-full px-4 py-2.5 border border-zinc-200 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/10" />
        </div>
        <div>
          <label class="block text-sm font-medium text-zinc-700 mb-1">{{ $t('NameEn') }}</label>
          <input v-model="form.nameEn" type="text" dir="ltr" :placeholder="$t('EnterNameEn')" required
            class="w-full px-4 py-2.5 border border-zinc-200 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/10" />
        </div>
      </form>

      <template #footer>
        <Button variant="outline" @click="dialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="primary" :loading="saving" @click="save">
          {{ isEdit ? ($t('SaveChanges')) : ($t('Add')) }}
        </Button>
      </template>
    </Modal>

    <!-- Confirm Delete Dialog -->
    <Modal v-model="confirmDialog" :title="$t('DeleteRole')" icon="mdi:shield-remove" variant="danger" size="sm">
      <div class="text-center py-4">
        <Icon icon="mdi:alert-circle" class="w-12 h-12 text-red-400 mx-auto mb-3" />
        <p class="text-zinc-600">{{ $t('ConfirmDelete') }}</p>
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
import { ref, computed, onMounted, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import Modal from '@/components/ui/Modal.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import RolesService from '@/services/RolesService'

// Types
interface Role {
  id: string
  nameAr: string
  nameEn: string
  isActive?: boolean
}

// State
const loading = ref(false)
const saving = ref(false)
const deleting = ref(false)
const roles = ref<Role[]>([])
const search = ref('')
const dialog = ref(false)
const confirmDialog = ref(false)
const isEdit = ref(false)
const selectedId = ref<string | null>(null)
const roleToDelete = ref<string | null>(null)

// Pagination
const currentPage = ref(1)
const pageSize = ref(10)

const form = ref({
  nameAr: '',
  nameEn: ''
})

// Computed
const activeRolesCount = computed(() => roles.value.filter(r => r.isActive !== false).length)

const filteredRoles = computed(() => {
  if (!search.value) return roles.value
  const query = search.value.toLowerCase()
  return roles.value.filter(role =>
    role.nameAr?.toLowerCase().includes(query) ||
    role.nameEn?.toLowerCase().includes(query)
  )
})

const totalPages = computed(() => Math.ceil(filteredRoles.value.length / pageSize.value) || 1)

const paginatedRoles = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredRoles.value.slice(start, end)
})

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

// Reset page when search changes
watch(search, () => {
  currentPage.value = 1
})

// Methods
const loadRoles = async () => {
  loading.value = true
  try {
    const response = await RolesService.listRoles()
    // Handle API wrapper format
    roles.value = response.data || response || []
  } catch (error) {
    console.error('Failed to load roles:', error)
  } finally {
    loading.value = false
  }
}

const openAddDialog = () => {
  isEdit.value = false
  selectedId.value = null
  form.value = { nameAr: '', nameEn: '' }
  dialog.value = true
}

const editRole = (role: Role) => {
  isEdit.value = true
  selectedId.value = role.id
  form.value = {
    nameAr: role.nameAr,
    nameEn: role.nameEn
  }
  dialog.value = true
}

const save = async () => {
  if (!form.value.nameAr || !form.value.nameEn) return

  saving.value = true
  try {
    if (isEdit.value && selectedId.value) {
      await RolesService.updateRole(selectedId.value, form.value)
    } else {
      await RolesService.addRole(form.value)
    }
    dialog.value = false
    await loadRoles()
  } catch (error) {
    console.error('Failed to save role:', error)
  } finally {
    saving.value = false
  }
}

const deleteRole = (id: string) => {
  roleToDelete.value = id
  confirmDialog.value = true
}

const confirmDelete = async () => {
  if (!roleToDelete.value) return

  deleting.value = true
  try {
    await RolesService.deleteRole(roleToDelete.value)
    confirmDialog.value = false
    roleToDelete.value = null
    await loadRoles()
  } catch (error) {
    console.error('Failed to delete role:', error)
  } finally {
    deleting.value = false
  }
}

// Lifecycle
onMounted(() => {
  loadRoles()
})
</script>

<style scoped>
/* Search toolbar inside grid container */
.mg-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid #eaeaea;
}

.mg-search {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 1;
  max-width: 320px;
}

.mg-search input {
  flex: 1;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 6px 10px;
  font-size: 13px;
  outline: none;
  transition: border-color 0.15s;
}

.mg-search input:focus {
  border-color: #006d4b;
  box-shadow: 0 0 0 2px rgba(0, 109, 75, 0.1);
}

.mg-count {
  font-size: 0.8rem;
  color: #6a7a7a;
}

/* Reuse MeetingGrid global styles: .mg-container, .mg-state, .mg-table-wrap,
   .data-table (global), .mg-id, .mg-title, .mg-actions, .mg-act,
   .mg-pag, .mg-pag-info, .mg-pag-btns, .mg-pg */
</style>

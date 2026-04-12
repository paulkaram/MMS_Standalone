<template>
  <Modal
    :model-value="modelValue"
    :title="$t('Permissions')"
    size="2xl"
    @update:model-value="$emit('update:modelValue', $event)"
  >
    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-16">
      <div class="loading-spinner">
        <Icon icon="mdi:loading" class="w-10 h-10 text-primary animate-spin" />
        <p class="mt-3 text-sm text-zinc-500">{{ $t('LoadingPermissions') }}</p>
      </div>
    </div>

    <!-- Permissions Content -->
    <div v-else-if="permissionGroups.length > 0" class="permissions-container">
      <div
        v-for="group in permissionGroups"
        :key="group.groupName"
        class="permission-group"
      >
        <!-- Group Header -->
        <div class="group-header">
          <Icon :icon="getGroupIcon(group.groupName)" class="group-icon" />
          <span class="group-title">{{ $t(group.groupName) || group.groupName }}</span>
          <span class="group-count">{{ getCheckedCount(group.items) }}/{{ group.items.length }}</span>
        </div>

        <!-- Permissions Grid -->
        <div class="permissions-grid">
          <label
            v-for="item in group.items"
            :key="item.id"
            class="permission-item"
            :class="{ 'is-checked': item.hasAccess }"
          >
            <div class="permission-checkbox">
              <input
                type="checkbox"
                v-model="item.hasAccess"
                class="checkbox-input"
                @change="updatePermission(item, item.hasAccess)"
              >
              <div class="checkbox-custom">
                <Icon v-if="item.hasAccess" icon="mdi:check" class="check-icon" />
              </div>
            </div>
            <span class="permission-label">{{ $t(item.name) || item.name }}</span>
            <div v-if="updatingIds.includes(item.id)" class="permission-loading">
              <Icon icon="mdi:loading" class="w-4 h-4 animate-spin text-primary" />
            </div>
          </label>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <div class="empty-icon-wrapper">
        <Icon icon="mdi:shield-off-outline" class="empty-icon" />
      </div>
      <h4 class="empty-title">{{ $t('NoPermissions') }}</h4>
      <p class="empty-description">{{ $t('NoPermissionsDescription') }}</p>
    </div>

    <template #footer>
      <div class="footer-info">
        <Icon icon="mdi:information-outline" class="w-4 h-4 text-zinc-400" />
        <span class="text-xs text-zinc-500">{{ $t('PermissionsAutoSave') }}</span>
      </div>
      <Button variant="primary" @click="$emit('update:modelValue', false)">
        {{ $t('Done') }}
      </Button>
    </template>
  </Modal>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import Button from '@/components/ui/Button.vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'
import { useToast } from '@/composables/useToast'

const { t } = useI18n()

interface PermissionItem {
  id: number
  name: string
  hasAccess: boolean
  levelId?: number
  groupName?: string
}

interface PermissionGroup {
  groupName: string
  items: PermissionItem[]
}

interface Props {
  modelValue: boolean
  committeeId: string | number | null
  userId: string | number | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

const { showToast } = useToast()

// State
const loading = ref(false)
const permissionGroups = ref<PermissionGroup[]>([])
const updatingIds = ref<number[]>([])

// Icon mapping for groups
const groupIcons: Record<string, string> = {
  'Meetings': 'mdi:calendar-multiple',
  'Recommendations': 'mdi:clipboard-check',
  'Tasks': 'mdi:checkbox-marked-circle-outline',
  'Members': 'mdi:account-group',
  'Attachments': 'mdi:paperclip',
  'Activities': 'mdi:chart-timeline-variant',
  'Reports': 'mdi:file-chart',
  'Settings': 'mdi:cog',
  'default': 'mdi:shield-check'
}

const getGroupIcon = (groupName: string): string => {
  return groupIcons[groupName] || groupIcons['default']
}

const getCheckedCount = (items: PermissionItem[]): number => {
  return items.filter(item => item.hasAccess).length
}

// Methods
const loadPermissions = async () => {
  if (!props.committeeId || !props.userId) return

  loading.value = true
  try {
    const result = await CouncilCommitteesService.getUserCommitteePermissions(
      String(props.committeeId),
      String(props.userId)
    )
    // API returns { data: { items: SecondLevelPermissionDto[] } }
    const data = result?.data || result
    const items = data?.items || []

    // Map to our structure
    permissionGroups.value = items.map((group: any) => ({
      groupName: group.groupName,
      items: (group.items || []).map((item: any) => ({
        id: item.id,
        name: item.name,
        hasAccess: item.hasAccess,
        levelId: item.levelId,
        groupName: item.groupName
      }))
    }))
  } catch (error) {
    console.error('Failed to load permissions:', error)
    permissionGroups.value = []
    showToast(t('ErrorLoadingPermissions'), 'error')
  } finally {
    loading.value = false
  }
}

const updatePermission = async (item: PermissionItem, enabled: boolean) => {
  if (!props.committeeId || !props.userId) return

  // v-model already updated item.hasAccess, store previous state for rollback
  const previousState = !enabled
  updatingIds.value.push(item.id)

  try {
    // Use camelCase to match old system
    await CouncilCommitteesService.updatePermission(String(props.committeeId), {
      userId: String(props.userId),
      permissionId: item.id,
      enabled: enabled
    })
    // Success - no need to update, v-model already did
  } catch (error) {
    console.error('Failed to update permission:', error)
    // Revert on error - set back to previous state
    item.hasAccess = previousState
    showToast(t('ErrorUpdatingPermission'), 'error')
  } finally {
    updatingIds.value = updatingIds.value.filter(id => id !== item.id)
  }
}

// Lifecycle
onMounted(() => {
  loadPermissions()
})
</script>

<style scoped>
.permissions-container {
  @apply space-y-4 max-h-[60vh] overflow-y-auto pe-2;
}

.permission-group {
  @apply bg-white border border-zinc-200 rounded-xl overflow-hidden;
}

.group-header {
  @apply flex items-center gap-3 px-4 py-3 bg-gradient-to-r from-primary/10 to-primary/5 border-b border-primary/10;
}

.group-icon {
  @apply w-5 h-5 text-primary;
}

.group-title {
  @apply font-semibold text-zinc-800 flex-1;
}

.group-count {
  @apply text-xs font-medium px-2 py-1 bg-primary/20 text-primary rounded-full;
}

.permissions-grid {
  @apply grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2 p-4;
}

.permission-item {
  @apply flex items-center gap-3 p-3 rounded-lg cursor-pointer transition-all duration-200;
  @apply hover:bg-zinc-50 border border-transparent;
}

.permission-item.is-checked {
  @apply bg-primary/5 border-primary/20;
}

.permission-checkbox {
  @apply relative flex-shrink-0;
}

.checkbox-input {
  @apply sr-only;
}

.checkbox-custom {
  @apply w-5 h-5 rounded border-2 border-zinc-300 flex items-center justify-center transition-all duration-200;
  @apply bg-white;
}

.permission-item.is-checked .checkbox-custom {
  @apply bg-primary border-primary;
}

.check-icon {
  @apply w-3.5 h-3.5 text-white;
}

.permission-label {
  @apply text-sm text-zinc-700 flex-1;
}

.permission-item.is-checked .permission-label {
  @apply text-zinc-900 font-medium;
}

.permission-loading {
  @apply flex-shrink-0;
}

/* Loading State */
.loading-spinner {
  @apply flex flex-col items-center;
}

/* Empty State */
.empty-state {
  @apply flex flex-col items-center justify-center py-16 text-center;
}

.empty-icon-wrapper {
  @apply w-20 h-20 rounded-full bg-zinc-100 flex items-center justify-center mb-4;
}

.empty-icon {
  @apply w-10 h-10 text-zinc-400;
}

.empty-title {
  @apply text-lg font-semibold text-zinc-700 mb-2;
}

.empty-description {
  @apply text-sm text-zinc-500 max-w-xs;
}

/* Footer */
.footer-info {
  @apply flex items-center gap-2 me-auto;
}

/* Scrollbar Styling */
.permissions-container::-webkit-scrollbar {
  @apply w-1.5;
}

.permissions-container::-webkit-scrollbar-track {
  @apply bg-zinc-100 rounded-full;
}

.permissions-container::-webkit-scrollbar-thumb {
  @apply bg-zinc-300 rounded-full hover:bg-zinc-400;
}
</style>

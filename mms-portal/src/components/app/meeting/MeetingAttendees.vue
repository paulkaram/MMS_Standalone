<template>
  <div class="attendees-section">
    <!-- Add Attendee Form -->
    <div v-if="!viewMode" class="add-attendee-form">
      <div class="form-grid">
        <div class="form-field">
          <label class="form-label">{{ $t('User') }} <span class="text-error">*</span></label>
          <Combobox
            v-model="newAttendee.userId"
            :options="userOptions"
            item-value="id"
            item-text="name"
            :placeholder="$t('SearchUser')"
            :loading="searchingUsers"
            :min-search-length="1"
            @search="searchUsers"
            @update:model-value="onUserSelect"
          />
        </div>

        <div class="form-field">
          <label class="form-label">{{ $t('Role') }} <span class="text-error">*</span></label>
          <CustomSelect
            v-model="newAttendee.jobTitle"
            :options="committeeRoles"
            value-key="name"
            label-key="name"
            :placeholder="$t('SelectRole')"
            size="medium"
          />
        </div>

        <div class="form-field checkbox-field">
          <label class="checkbox-wrapper">
            <input
              v-model="newAttendee.needsApproval"
              type="checkbox"
              class="checkbox-input"
            >
            <span class="checkbox-label-text">{{ $t('Required') }}</span>
          </label>
        </div>

        <div class="form-field btn-field">
          <Button
            variant="primary"
            icon-left="mdi:plus"
            :loading="adding"
            :disabled="!canAddAttendee"
            @click="addAttendee"
          >
            {{ $t('Add') }}
          </Button>
        </div>
      </div>
    </div>

    <!-- Attendees List -->
    <div class="attendees-list">
      <!-- Loading -->
      <div v-if="loading" class="loading-state">
        <div v-for="i in 3" :key="i" class="skeleton-row">
          <div class="skeleton-avatar"></div>
          <div class="skeleton-content">
            <div class="skeleton-line w-40"></div>
            <div class="skeleton-line w-32"></div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else-if="attendees.length === 0" class="empty-state">
        <Icon icon="mdi:account-group-outline" class="empty-icon" />
        <p>{{ $t('NoAttendees') }}</p>
      </div>

      <!-- Attendees Cards -->
      <template v-else>
        <div
          v-for="(attendee, index) in attendees"
          :key="attendee.userId"
          class="attendee-row"
        >
          <!-- User Info -->
          <div class="attendee-user">
            <UserAvatar :userId="attendee.userId" :name="attendee.name || ''" size="sm" />
            <div class="attendee-info">
              <span class="attendee-name">{{ attendee.name }}</span>
              <span v-if="attendee.email" class="attendee-email">{{ attendee.email }}</span>
            </div>
          </div>

          <!-- Controls -->
          <div class="attendee-actions">
            <!-- Role -->
            <div class="action-item role-action">
              <CustomSelect
                v-if="!viewMode"
                :model-value="attendee.jobTitle"
                :options="committeeRoles"
                value-key="name"
                label-key="name"
                :placeholder="$t('SelectRole')"
                size="small"
                @update:model-value="(val: any) => { attendee.jobTitle = val; updateAttendee(attendee) }"
              />
              <span v-else class="role-text">{{ attendee.jobTitle }}</span>
            </div>

            <!-- Required -->
            <label class="action-item required-toggle">
              <input
                v-model="attendee.needsApproval"
                type="checkbox"
                class="checkbox-input"
                :disabled="viewMode"
                @change="updateAttendee(attendee)"
              >
              <span>{{ $t('Required') }}</span>
            </label>

            <!-- Delete -->
            <button
              v-if="!viewMode"
              type="button"
              class="delete-btn"
              @click="removeAttendee(attendee)"
              :title="$t('Delete')"
            >
              <Icon icon="mdi:close" class="w-4 h-4" />
            </button>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import Combobox from '@/components/ui/Combobox.vue'
import CustomSelect from '@/components/ui/CustomSelect.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import MeetingsService from '@/services/MeetingsService'
import UsersService from '@/services/UsersService'
import LookupsService from '@/services/LookupsService'

export interface Attendee {
  userId: string
  name?: string
  email?: string
  jobTitle: string | null
  needsApproval: boolean
}

// Committee roles loaded from API
const committeeRoles = ref<Array<{ id: string | number; name: string }>>([])

const loadCommitteeRoles = async () => {
  try {
    const response = await LookupsService.listCommitteeRoles()
    const data = Array.isArray(response) ? response : ((response as any)?.data || [])
    committeeRoles.value = data.map((r: any) => ({ id: r.id, name: r.name || r.nameAr || r.nameEn }))
  } catch (error) {
    console.error('Failed to load committee roles:', error)
  }
}


interface Props {
  modelValue: Attendee[]
  meetingId?: number | string
  viewMode?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  viewMode: false,
  meetingId: undefined
})

const emit = defineEmits<{
  'update:modelValue': [value: Attendee[]]
}>()

// State
const attendees = ref<Attendee[]>([...props.modelValue])
const loading = ref(false)
const adding = ref(false)
const searchingUsers = ref(false)
const userOptions = ref<Array<{ id: string; name: string; email?: string }>>([])
const newAttendee = ref({
  userId: null as string | null,
  name: '',
  email: '',
  jobTitle: null as string | null,
  needsApproval: false
})

// Computed
const canAddAttendee = computed(() => {
  return newAttendee.value.userId && newAttendee.value.jobTitle
})

// Flag to prevent infinite loop
let isUpdatingFromProps = false

// Watch for external changes
watch(() => props.modelValue, (newVal) => {
  if (JSON.stringify(newVal) !== JSON.stringify(attendees.value)) {
    isUpdatingFromProps = true
    attendees.value = [...newVal]
    isUpdatingFromProps = false
  }
}, { deep: true })

// Watch for local changes and emit
watch(attendees, (newVal) => {
  if (!isUpdatingFromProps) {
    emit('update:modelValue', [...newVal])
  }
}, { deep: true })

// Methods
const searchUsers = async (query: string) => {
  if (!query || query.length < 1) {
    userOptions.value = []
    return
  }

  searchingUsers.value = true
  try {
    userOptions.value = await UsersService.searchLocalUsers(query, 20)
  } catch (error) {
    console.error('Failed to search users:', error)
    userOptions.value = []
  } finally {
    searchingUsers.value = false
  }
}

const onUserSelect = (userId: string | number) => {
  const user = userOptions.value.find(u => String(u.id) === String(userId))
  if (user) {
    newAttendee.value.name = user.name
    newAttendee.value.email = user.email || ''
  }
}

const addAttendee = async () => {
  if (!canAddAttendee.value) return

  // Check for duplicates
  if (attendees.value.some(a => Number(a.userId) === Number(newAttendee.value.userId))) {
    console.warn('User already added')
    return
  }

  adding.value = true
  try {
    if (props.meetingId) {
      const response = await MeetingsService.addAttendee(
        Number(props.meetingId),
        {
          userId: newAttendee.value.userId,
          needsApproval: newAttendee.value.needsApproval,
          jobTitle: newAttendee.value.jobTitle,
          name: ''
        }
      )
      const data = response.data || response
      if (data && Array.isArray(data)) {
        attendees.value = data
      }
      resetForm()
    } else {
      attendees.value.push({
        userId: newAttendee.value.userId!,
        name: newAttendee.value.name,
        email: newAttendee.value.email,
        jobTitle: newAttendee.value.jobTitle,
        needsApproval: newAttendee.value.needsApproval
      })
      resetForm()
    }
  } catch (error) {
    console.error('Failed to add attendee:', error)
  } finally {
    adding.value = false
  }
}

const resetForm = () => {
  newAttendee.value.userId = null
  newAttendee.value.name = ''
  newAttendee.value.email = ''
  newAttendee.value.jobTitle = null
  newAttendee.value.needsApproval = false
}

const updateAttendee = async (attendee: Attendee) => {
  if (!props.meetingId) return

  try {
    const response = await MeetingsService.updateAttendee(
      props.meetingId.toString(),
      attendee.userId,
      {
        userId: attendee.userId,
        jobTitle: attendee.jobTitle,
        needsApproval: attendee.needsApproval,
        name: ''
      }
    )
    const data = response.data || response
    if (Array.isArray(data)) {
      attendees.value = data
    }
  } catch (error) {
    console.error('Failed to update attendee:', error)
  }
}

const removeAttendee = async (attendee: Attendee) => {
  try {
    if (props.meetingId) {
      const response = await MeetingsService.removeAttendee(props.meetingId.toString(), attendee.userId)
      const data = response.data || response
      if (Array.isArray(data)) {
        attendees.value = data
      } else {
        attendees.value = attendees.value.filter(a => String(a.userId) !== String(attendee.userId))
      }
    } else {
      attendees.value = attendees.value.filter(a => String(a.userId) !== String(attendee.userId))
    }
  } catch (error) {
    console.error('Failed to remove attendee:', error)
  }
}

// Validation
const validate = (): boolean => {
  return attendees.value.length > 0
}

defineExpose({ validate })

onMounted(() => {
  loadCommitteeRoles()
})
</script>

<style scoped>
.attendees-section {
  background: white;
  border-radius: 12px;
  border: 1px solid #e4e4e7;
  overflow: visible; /* Allow dropdown to overflow */
}

/* Add Form */
.add-attendee-form {
  padding: 20px 24px;
  background: #f8fafc;
  border-bottom: 1px solid #e4e4e7;
  border-radius: 12px 12px 0 0;
  position: relative;
  z-index: 10; /* Ensure dropdown appears above list */
}

.form-grid {
  display: flex;
  align-items: flex-end;
  gap: 20px;
  flex-wrap: wrap;
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-field:first-child {
  flex: 2;
  min-width: 280px;
}

.form-field:nth-child(2) {
  flex: 1;
  min-width: 220px;
}

.checkbox-field {
  justify-content: flex-end;
  padding-bottom: 8px;
}

.btn-field {
  justify-content: flex-end;
}

.form-label {
  font-size: 13px;
  font-weight: 500;
  color: #3f3f46;
}


/* Checkbox */
.checkbox-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}

.checkbox-input {
  appearance: none;
  width: 18px;
  height: 18px;
  border: 2px solid #d4d4d8;
  border-radius: 4px;
  background: white;
  cursor: pointer;
  position: relative;
  flex-shrink: 0;
}

.checkbox-input:checked {
  background: #006d4b;
  border-color: #006d4b;
}

.checkbox-input:checked::after {
  content: '';
  position: absolute;
  top: 2px;
  left: 5px;
  width: 4px;
  height: 8px;
  border: solid white;
  border-width: 0 2px 2px 0;
  transform: rotate(45deg);
}

.checkbox-input:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.checkbox-label-text {
  font-size: 14px;
  color: #3f3f46;
}

/* Attendees List */
.attendees-list {
  min-height: 60px;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.loading-state {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.skeleton-row {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 16px 20px;
  background: #f4f4f5;
  border-radius: 10px;
}

.skeleton-avatar {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  background: #e4e4e7;
}

.skeleton-content {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.skeleton-line {
  height: 12px;
  background: #e4e4e7;
  border-radius: 4px;
}

.skeleton-line.w-40 { width: 160px; }
.skeleton-line.w-32 { width: 128px; }

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 32px 16px;
  color: #a1a1aa;
  gap: 12px;
}

.empty-icon {
  width: 48px;
  height: 48px;
}

.empty-state p {
  font-size: 14px;
}

/* Attendee Row - Card Style */
.attendee-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 14px;
  background: #f8fafc;
  border-radius: 8px;
  border: 1px solid #e6eaef;
  transition: all 0.15s ease;
}

.attendee-row:hover {
  background: #f1f5f9;
}

.attendee-user {
  display: flex;
  align-items: center;
  gap: 10px;
  flex: 1;
  min-width: 0;
}

.attendee-info {
  display: flex;
  flex-direction: column;
  gap: 1px;
  min-width: 0;
  flex: 1;
}

.attendee-name {
  font-size: 13px;
  font-weight: 600;
  color: #004730;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.attendee-email {
  font-size: 11px;
  color: #94a3b8;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* Actions */
.attendee-actions {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-shrink: 0;
}

.action-item {
  display: flex;
  align-items: center;
  flex-shrink: 0;
}

.action-item.role-action {
  min-width: 200px;
}

.role-text {
  font-size: 13px;
  color: #3f3f46;
  padding: 6px 12px;
  background: #f4f4f5;
  border-radius: 6px;
}

.required-toggle {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  font-size: 13px;
  color: #71717a;
  white-space: nowrap;
}

.delete-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  color: #a1a1aa;
  background: transparent;
  border: none;
  cursor: pointer;
  transition: all 0.15s;
}

.delete-btn:hover {
  color: #ef4444;
  background: #fef2f2;
}

/* Responsive */
@media (max-width: 768px) {
  .add-attendee-form {
    padding: 16px;
  }

  .form-grid {
    flex-direction: column;
    align-items: stretch;
    gap: 16px;
  }

  .form-field:first-child,
  .form-field:nth-child(2) {
    min-width: 100%;
    flex: 1;
  }

  .attendees-list {
    padding: 12px;
    gap: 10px;
  }

  .attendee-row {
    flex-direction: column;
    align-items: flex-start;
    gap: 14px;
    padding: 14px 16px;
  }

  .attendee-user {
    width: 100%;
  }

  .attendee-actions {
    width: 100%;
    justify-content: flex-start;
    flex-wrap: wrap;
    gap: 12px;
  }

  .action-item.role-action {
    flex: 1;
    min-width: 160px;
  }
}
</style>

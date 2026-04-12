<template>
  <div class="tab-panel">
    <div class="panel-header">
      <h3 class="panel-title">{{ $t('Activities') }}</h3>
      <Button
        variant="primary"
        size="sm"
        icon-left="mdi:plus"
        @click="openAddActivity"
      >
        {{ $t('Add') }}
      </Button>
    </div>
    <div class="items-list">
      <div
        v-for="activity in activities"
        :key="activity.id"
        class="list-item"
      >
        <div class="item-icon activity">
          <Icon icon="mdi:lightning-bolt" class="w-5 h-5" />
        </div>
        <div class="item-content">
          <h4 class="item-title">{{ activity.title }}</h4>
          <p v-if="activity.description" class="item-desc">{{ activity.description }}</p>
        </div>
        <div class="item-actions">
          <button class="action-btn edit" @click="openEditActivity(activity)">
            <Icon icon="mdi:pencil" class="w-4 h-4" />
          </button>
          <button class="action-btn delete" @click="removeActivity(activity)">
            <Icon icon="mdi:delete" class="w-4 h-4" />
          </button>
        </div>
      </div>
      <div v-if="activities.length === 0 && !loading" class="empty-state">
        <Icon icon="mdi:lightning-bolt-outline" class="w-16 h-16 text-zinc-200" />
        <p class="text-zinc-400">{{ $t('NoActivities') }}</p>
      </div>
    </div>

    <!-- Add/Edit Activity Dialog -->
    <Modal
      v-model="activityDialog"
      :title="isEditActivity ? ($t('EditActivity')) : ($t('AddActivity'))"
      size="md"
    >
      <form @submit.prevent="saveActivity" class="space-y-4">
        <Input
          v-model="activityForm.title"
          :label="$t('Title')"
          required
        />
        <div>
          <label class="block text-sm font-medium text-zinc-700 mb-1">
            {{ $t('Description') }}
          </label>
          <textarea
            v-model="activityForm.description"
            rows="3"
            class="w-full px-4 py-2 border border-zinc-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
          />
        </div>
      </form>

      <template #footer>
        <Button variant="outline" @click="activityDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="primary" :loading="savingActivity" @click="saveActivity">
          {{ isEditActivity ? ($t('Update')) : ($t('Add')) }}
        </Button>
      </template>
    </Modal>

    <!-- Confirm Delete Dialog -->
    <Modal
      v-model="confirmDialog"
      title="حذف النشاط"
      icon="mdi:lightning-bolt-outline"
      variant="danger"
      size="sm"
    >
      <div class="text-center py-4">
        <Icon icon="mdi:alert-circle" class="w-16 h-16 text-error mx-auto mb-4" />
        <p class="text-zinc-600">{{ $t('ConfirmDeleteActivity') }}</p>
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
import { ref, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import Input from '@/components/ui/Input.vue'
import Modal from '@/components/ui/Modal.vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'

// Types
interface Activity {
  id: string
  title: string
  description?: string
}

// Props
const props = defineProps<{
  committeeId: string | null
}>()

// State
const loading = ref(false)
const activities = ref<Activity[]>([])
const activityDialog = ref(false)
const isEditActivity = ref(false)
const savingActivity = ref(false)
const selectedActivityId = ref<string | null>(null)
const activityForm = ref({ title: '', description: '' })

// Delete confirmation
const confirmDialog = ref(false)
const activityToDelete = ref<Activity | null>(null)
const deleting = ref(false)

// Methods
const loadActivities = async () => {
  if (!props.committeeId) return
  loading.value = true
  try {
    const response = await CouncilCommitteesService.listCommitteeActivities(props.committeeId)
    // API returns { data, success, message } wrapper
    activities.value = response.data || response || []
  } catch (error) {
    console.error('Failed to load activities:', error)
  } finally {
    loading.value = false
  }
}

const openAddActivity = () => {
  isEditActivity.value = false
  selectedActivityId.value = null
  activityForm.value = { title: '', description: '' }
  activityDialog.value = true
}

const openEditActivity = (activity: Activity) => {
  isEditActivity.value = true
  selectedActivityId.value = activity.id
  activityForm.value = { title: activity.title, description: activity.description || '' }
  activityDialog.value = true
}

const saveActivity = async () => {
  if (!props.committeeId || !activityForm.value.title) return

  savingActivity.value = true
  try {
    if (isEditActivity.value && selectedActivityId.value) {
      await CouncilCommitteesService.updateCommitteeActivity(selectedActivityId.value, activityForm.value)
    } else {
      await CouncilCommitteesService.addCommitteeActivity(props.committeeId, activityForm.value)
    }
    activityDialog.value = false
    await loadActivities()
  } catch (error) {
    console.error('Failed to save activity:', error)
  } finally {
    savingActivity.value = false
  }
}

const removeActivity = (activity: Activity) => {
  activityToDelete.value = activity
  confirmDialog.value = true
}

const confirmDelete = async () => {
  if (!activityToDelete.value) return
  deleting.value = true
  try {
    await CouncilCommitteesService.removeActivity(activityToDelete.value.id)
    confirmDialog.value = false
    await loadActivities()
  } catch (error) {
    console.error('Failed to remove activity:', error)
  } finally {
    deleting.value = false
  }
}

// Watch for committeeId changes
watch(() => props.committeeId, () => {
  if (props.committeeId) {
    loadActivities()
  }
}, { immediate: true })

// Expose refresh method
defineExpose({ refresh: loadActivities })
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

/* Items List */
.items-list {
  @apply space-y-3;
}

.list-item {
  @apply flex items-center gap-4 p-4 bg-zinc-50 rounded-xl border border-zinc-100;
}

.item-icon {
  @apply w-10 h-10 rounded-lg bg-primary/10 text-primary flex items-center justify-center flex-shrink-0;
}

.item-icon.activity {
  @apply bg-warning/10 text-warning;
}

.item-content {
  @apply flex-1 min-w-0;
}

.item-title {
  @apply font-medium text-zinc-900;
}

.item-desc {
  @apply text-sm text-zinc-500 line-clamp-1;
}

.item-actions {
  @apply flex items-center gap-1;
}

.action-btn {
  @apply p-2 rounded-lg transition-colors;
}

.action-btn.edit { @apply text-zinc-400 hover:text-primary hover:bg-primary/10; }
.action-btn.delete { @apply text-zinc-400 hover:text-error hover:bg-error/10; }

/* Empty State */
.empty-state {
  @apply flex flex-col items-center justify-center py-12;
}
</style>

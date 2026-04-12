<template>
  <div class="tab-panel">
    <div class="panel-header">
      <h3 class="panel-title">{{ $t('CommitteeTasks') }}</h3>
      <Button
        variant="primary"
        size="sm"
        icon-left="mdi:plus"
        @click="openAddDuty"
      >
        {{ $t('Add') }}
      </Button>
    </div>
    <div class="items-list">
      <div
        v-for="duty in duties"
        :key="duty.id"
        class="list-item"
      >
        <div class="item-icon">
          <Icon icon="mdi:clipboard-check" class="w-5 h-5" />
        </div>
        <div class="item-content">
          <h4 class="item-title">{{ duty.title }}</h4>
          <p v-if="duty.description" class="item-desc">{{ duty.description }}</p>
        </div>
        <div class="item-actions">
          <button class="action-btn edit" @click="openEditDuty(duty)">
            <Icon icon="mdi:pencil" class="w-4 h-4" />
          </button>
          <button class="action-btn delete" @click="removeDuty(duty)">
            <Icon icon="mdi:delete" class="w-4 h-4" />
          </button>
        </div>
      </div>
      <div v-if="duties.length === 0 && !loading" class="empty-state">
        <Icon icon="mdi:clipboard-text-outline" class="w-16 h-16 text-zinc-200" />
        <p class="text-zinc-400">{{ $t('NoTasks') }}</p>
      </div>
    </div>

    <!-- Add/Edit Duty Dialog -->
    <Modal
      v-model="dutyDialog"
      :title="isEditDuty ? ($t('EditTask')) : ($t('AddTask'))"
      size="md"
    >
      <form @submit.prevent="saveDuty" class="space-y-4">
        <Input
          v-model="dutyForm.title"
          :label="$t('Title')"
          required
        />
        <div>
          <label class="block text-sm font-medium text-zinc-700 mb-1">
            {{ $t('Description') }}
          </label>
          <textarea
            v-model="dutyForm.description"
            rows="3"
            class="w-full px-4 py-2 border border-zinc-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
          />
        </div>
      </form>

      <template #footer>
        <Button variant="outline" @click="dutyDialog = false">
          {{ $t('Cancel') }}
        </Button>
        <Button variant="primary" :loading="savingDuty" @click="saveDuty">
          {{ isEditDuty ? ($t('Update')) : ($t('Add')) }}
        </Button>
      </template>
    </Modal>

    <!-- Confirm Delete Dialog -->
    <Modal
      v-model="confirmDialog"
      title="حذف المهمة"
      icon="mdi:clipboard-remove"
      variant="danger"
      size="sm"
    >
      <div class="text-center py-4">
        <Icon icon="mdi:alert-circle" class="w-16 h-16 text-error mx-auto mb-4" />
        <p class="text-zinc-600">{{ $t('ConfirmDeleteTask') }}</p>
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
interface Duty {
  id: string
  title: string
  description?: string
}

// Props
const props = defineProps<{
  committeeId: string | null
}>()

// Emits
const emit = defineEmits<{
  (e: 'update:count', count: number): void
}>()

// State
const loading = ref(false)
const duties = ref<Duty[]>([])
const dutyDialog = ref(false)
const isEditDuty = ref(false)
const savingDuty = ref(false)
const selectedDutyId = ref<string | null>(null)
const dutyForm = ref({ title: '', description: '' })

// Delete confirmation
const confirmDialog = ref(false)
const dutyToDelete = ref<Duty | null>(null)
const deleting = ref(false)

// Methods
const loadDuties = async () => {
  if (!props.committeeId) return
  loading.value = true
  try {
    const response = await CouncilCommitteesService.listCommitteeDuties(props.committeeId)
    // API returns { data, success, message } wrapper
    duties.value = response.data || response || []
    emit('update:count', duties.value.length)
  } catch (error) {
    console.error('Failed to load duties:', error)
  } finally {
    loading.value = false
  }
}

const openAddDuty = () => {
  isEditDuty.value = false
  selectedDutyId.value = null
  dutyForm.value = { title: '', description: '' }
  dutyDialog.value = true
}

const openEditDuty = (duty: Duty) => {
  isEditDuty.value = true
  selectedDutyId.value = duty.id
  dutyForm.value = { title: duty.title, description: duty.description || '' }
  dutyDialog.value = true
}

const saveDuty = async () => {
  if (!props.committeeId || !dutyForm.value.title) return

  savingDuty.value = true
  try {
    if (isEditDuty.value && selectedDutyId.value) {
      await CouncilCommitteesService.updateCommitteeDuty(selectedDutyId.value, dutyForm.value)
    } else {
      await CouncilCommitteesService.addCommitteeDuty(props.committeeId, dutyForm.value)
    }
    dutyDialog.value = false
    await loadDuties()
  } catch (error) {
    console.error('Failed to save duty:', error)
  } finally {
    savingDuty.value = false
  }
}

const removeDuty = (duty: Duty) => {
  dutyToDelete.value = duty
  confirmDialog.value = true
}

const confirmDelete = async () => {
  if (!dutyToDelete.value) return
  deleting.value = true
  try {
    await CouncilCommitteesService.removeDuty(dutyToDelete.value.id)
    confirmDialog.value = false
    await loadDuties()
  } catch (error) {
    console.error('Failed to remove duty:', error)
  } finally {
    deleting.value = false
  }
}

// Watch for committeeId changes
watch(() => props.committeeId, () => {
  if (props.committeeId) {
    loadDuties()
  }
}, { immediate: true })

// Expose refresh method
defineExpose({ refresh: loadDuties })
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

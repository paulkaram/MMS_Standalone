<template>
  <div class="items-section">
    <!-- Header actions -->
    <div class="items-toolbar">
      <button class="btn-add" @click="openAddModal">
        <Icon icon="mdi:plus" class="w-4 h-4" />
        {{ $t('AddItem') }}
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading && items.length === 0" class="loading-state">
      <div class="spinner"></div>
    </div>

    <!-- Items list -->
    <div v-else-if="items.length > 0" class="items-list">
      <div v-for="item in items" :key="item.id" class="item-card">
        <div class="item-header">
          <div class="item-refs">
            <span class="ref-tag internal">
              <Icon icon="mdi:pound" class="w-3 h-3" />
              {{ item.referenceNumber }}
            </span>
            <span v-if="item.externalReferenceNumber" class="ref-tag external">
              <Icon icon="mdi:link-variant" class="w-3 h-3" />
              {{ item.externalReferenceNumber }}
            </span>
            <span class="type-tag">{{ item.itemTypeName }}</span>
            <span v-if="item.isPrivate" class="private-tag">
              <Icon icon="mdi:lock" class="w-3 h-3" />
              {{ $t('IsPrivate') }}
            </span>
          </div>
          <div class="item-actions">
            <button class="icon-btn" :title="$t('EditItem')" @click="openEditModal(item)">
              <Icon icon="mdi:pencil" class="w-4 h-4" />
            </button>
            <button class="icon-btn danger" :title="$t('DeleteItem')" @click="confirmDelete(item)">
              <Icon icon="mdi:delete" class="w-4 h-4" />
            </button>
          </div>
        </div>

        <div class="item-content" v-html="item.content"></div>

        <div v-if="item.tags" class="item-tags">
          <span v-for="tag in item.tags.split(',')" :key="tag" class="tag-chip">
            {{ tag.trim() }}
          </span>
        </div>

        <div class="item-footer">
          <div class="owner-info" v-if="item.createdByName">
            <UserAvatar :userId="item.createdBy" :name="item.createdByName" size="xs" />
            <span class="owner-name">{{ item.createdByName }}</span>
          </div>
          <span class="date-tag">{{ formatDate(item.createdDate) }}</span>
          <span v-if="item.relatedItemReferenceNumber" class="related-tag">
            <Icon icon="mdi:link" class="w-3 h-3" />
            {{ item.relatedItemReferenceNumber }}
          </span>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-else class="empty-state">
      <Icon icon="mdi:format-list-bulleted" class="w-10 h-10" />
      <p>{{ $t('NoItems') }}</p>
    </div>

    <!-- Add/Edit Modal -->
    <Modal v-model="modalOpen" :title="editingItem ? $t('EditItem') : $t('AddItem')" size="lg">
      <form class="item-form" @submit.prevent="saveItem">
        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('ExternalReferenceNumber') }}</label>
            <input v-model="form.externalReferenceNumber" type="text" class="form-input" />
          </div>
          <div class="form-field">
            <label>{{ $t('ItemType') }} <span class="required">*</span></label>
            <select v-model="form.itemTypeId" class="form-input" required>
              <option :value="0" disabled>{{ $t('Select') }}</option>
              <option v-for="t in itemTypes" :key="t.id" :value="t.id">{{ t.name }}</option>
            </select>
          </div>
        </div>

        <div class="form-field">
          <label>{{ $t('ItemContent') }} <span class="required">*</span></label>
          <textarea v-model="form.content" rows="6" class="form-input" required></textarea>
        </div>

        <div class="form-row">
          <div class="form-field">
            <label>{{ $t('Tags') }}</label>
            <input v-model="form.tags" type="text" class="form-input" :placeholder="$t('Tags')" />
          </div>
          <div class="form-field">
            <label>{{ $t('Order') }}</label>
            <input v-model.number="form.order" type="number" min="0" class="form-input" />
          </div>
        </div>

        <div class="form-field">
          <label>{{ $t('RelatedItem') }}</label>
          <select v-model="form.relatedItemId" class="form-input">
            <option :value="null">-</option>
            <option v-for="i in relatedItemsOptions" :key="i.id" :value="i.id">
              {{ i.referenceNumber }}
            </option>
          </select>
        </div>

        <div class="form-field">
          <label>{{ $t('InternalNote') }}</label>
          <textarea v-model="form.internalNote" rows="3" class="form-input"></textarea>
        </div>

        <div class="form-field checkbox-field">
          <label class="checkbox-label">
            <input v-model="form.isPrivate" type="checkbox" />
            <span>{{ $t('IsPrivate') }}</span>
          </label>
          <small>{{ $t('IsPrivateHint') }}</small>
        </div>

        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="modalOpen = false">
            {{ $t('Cancel') }}
          </button>
          <button type="submit" class="btn-primary" :disabled="saving">
            <span v-if="saving" class="spinner-sm"></span>
            {{ $t('Save') }}
          </button>
        </div>
      </form>
    </Modal>

    <!-- Delete confirmation -->
    <ConfirmModal
      v-model="confirmOpen"
      :title="$t('DeleteItem')"
      :message="$t('ConfirmDeleteItem')"
      :confirm-text="$t('Delete')"
      :cancel-text="$t('Cancel')"
      variant="danger"
      @confirm="deleteItem"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import ConfirmModal from '@/components/ui/ConfirmModal.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import CommitteeItemsService, { type CommitteeItem, type CommitteeItemPost, type ListItem } from '@/services/CommitteeItemsService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

interface Props {
  committeeId: string | number
}
const props = defineProps<Props>()
const emit = defineEmits<{ 'update:count': [value: number] }>()

const { t } = useI18n()
const { toast } = useToast()

const items = ref<CommitteeItem[]>([])
const itemTypes = ref<ListItem[]>([])
const loading = ref(false)
const saving = ref(false)
const modalOpen = ref(false)
const confirmOpen = ref(false)
const editingItem = ref<CommitteeItem | null>(null)
const pendingDeleteId = ref<number | null>(null)

const committeeIdNum = computed(() => Number(props.committeeId))

const relatedItemsOptions = computed(() =>
  items.value.filter(i => !editingItem.value || i.id !== editingItem.value.id)
)

const emptyForm = (): CommitteeItemPost => ({
  committeeId: committeeIdNum.value,
  externalReferenceNumber: null,
  content: '',
  itemTypeId: 0,
  tags: null,
  internalNote: null,
  relatedItemId: null,
  isPrivate: false,
  order: 0
})

const form = reactive<CommitteeItemPost>(emptyForm())

const loadItems = async () => {
  if (!committeeIdNum.value) return
  loading.value = true
  try {
    const res: any = await CommitteeItemsService.listByCommittee(committeeIdNum.value)
    items.value = res?.data ?? res ?? []
    emit('update:count', items.value.length)
  } catch (err) {
    console.error('Failed to load items:', err)
  } finally {
    loading.value = false
  }
}

const loadTypes = async () => {
  try {
    const res: any = await CommitteeItemsService.listItemTypes()
    itemTypes.value = res?.data ?? res ?? []
  } catch (err) {
    console.error('Failed to load item types:', err)
  }
}

const openAddModal = () => {
  editingItem.value = null
  Object.assign(form, emptyForm())
  modalOpen.value = true
}

const openEditModal = (item: CommitteeItem) => {
  editingItem.value = item
  Object.assign(form, {
    committeeId: committeeIdNum.value,
    externalReferenceNumber: item.externalReferenceNumber,
    content: item.content,
    itemTypeId: item.itemTypeId,
    tags: item.tags,
    internalNote: item.internalNote,
    relatedItemId: item.relatedItemId,
    isPrivate: item.isPrivate,
    order: item.order
  })
  modalOpen.value = true
}

const saveItem = async () => {
  if (!form.content.trim() || !form.itemTypeId) return
  saving.value = true
  try {
    if (editingItem.value) {
      await CommitteeItemsService.update(editingItem.value.id, form)
      toast.success(t('ItemUpdatedSuccessfully'))
    } else {
      await CommitteeItemsService.create(form)
      toast.success(t('ItemCreatedSuccessfully'))
    }
    modalOpen.value = false
    await loadItems()
  } catch (err: any) {
    const msg = err?.response?.data?.message || t('ErrorOccured')
    toast.error(msg)
  } finally {
    saving.value = false
  }
}

const confirmDelete = (item: CommitteeItem) => {
  pendingDeleteId.value = item.id
  confirmOpen.value = true
}

const deleteItem = async () => {
  if (!pendingDeleteId.value) return
  try {
    await CommitteeItemsService.delete(pendingDeleteId.value)
    toast.success(t('ItemDeletedSuccessfully'))
    await loadItems()
  } catch (err: any) {
    const msg = err?.response?.data?.message || t('ErrorOccured')
    toast.error(msg)
  } finally {
    pendingDeleteId.value = null
    confirmOpen.value = false
  }
}

const formatDate = (iso: string): string => {
  if (!iso) return ''
  const d = new Date(iso)
  return d.toLocaleDateString()
}

watch(() => props.committeeId, loadItems)

onMounted(() => {
  loadTypes()
  loadItems()
})
</script>

<style scoped>
.items-section { display: flex; flex-direction: column; gap: 14px; }

.items-toolbar {
  display: flex;
  justify-content: flex-end;
}

.btn-add {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 14px;
  background: #006d4b;
  color: #fff;
  border: none;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
  font-family: inherit;
}
.btn-add:hover { background: #005a3e; }

.loading-state {
  display: flex; justify-content: center;
  padding: 32px 0;
}

.spinner {
  width: 28px; height: 28px;
  border: 3px solid #e2e8f0;
  border-top-color: #006d4b;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

.items-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.item-card {
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 10px;
  padding: 14px 16px;
  transition: border-color 0.2s, box-shadow 0.2s;
}
.item-card:hover {
  border-color: #c8ddd3;
  box-shadow: 0 2px 8px rgba(0, 109, 75, 0.06);
}

.item-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 8px;
}

.item-refs {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 6px;
  flex: 1;
}

.ref-tag {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  padding: 2px 8px;
  font-size: 11px;
  font-weight: 600;
  border-radius: 10px;
  background: #e8f5ef;
  color: #006d4b;
  border: 1px solid #c8ddd3;
}
.ref-tag.external {
  background: #eef2ff;
  color: #3b4cca;
  border-color: #c5cbf5;
}

.type-tag {
  padding: 2px 8px;
  font-size: 11px;
  font-weight: 600;
  border-radius: 10px;
  background: #fff7e8;
  color: #b45309;
  border: 1px solid #fde2b6;
}

.private-tag {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  padding: 2px 8px;
  font-size: 11px;
  font-weight: 600;
  border-radius: 10px;
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
}

.item-actions {
  display: flex;
  gap: 4px;
  flex-shrink: 0;
}

.icon-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 30px; height: 30px;
  border: 1px solid #e4ede8;
  background: #fff;
  border-radius: 6px;
  color: #5a7a6d;
  cursor: pointer;
  transition: all 0.15s;
}
.icon-btn:hover {
  background: #f4f8f6;
  border-color: #006d4b;
  color: #006d4b;
}
.icon-btn.danger:hover {
  background: #fef2f2;
  border-color: #ef4444;
  color: #ef4444;
}

.item-content {
  font-size: 14px;
  color: #1a2e25;
  line-height: 1.6;
  margin: 4px 0 10px;
}
.item-content :deep(p) { margin: 4px 0; }

.item-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  margin-bottom: 8px;
}

.tag-chip {
  padding: 2px 8px;
  font-size: 11px;
  background: #f4f8f6;
  color: #5a7a6d;
  border-radius: 10px;
  border: 1px solid #e4ede8;
}

.item-footer {
  display: flex;
  align-items: center;
  gap: 10px;
  padding-top: 8px;
  border-top: 1px solid #f0f4f2;
  font-size: 11px;
  color: #6b8a7d;
}

.owner-info {
  display: flex;
  align-items: center;
  gap: 4px;
}
.owner-name { font-weight: 500; }

.date-tag { color: #6b8a7d; }

.related-tag {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  padding: 2px 7px;
  font-size: 10px;
  border-radius: 8px;
  background: #eef5f1;
  color: #006d4b;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 40px 20px;
  color: #93afa4;
  gap: 10px;
}
.empty-state p { font-size: 13px; margin: 0; }

/* Form */
.item-form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.form-field label {
  font-size: 12px;
  font-weight: 600;
  color: #374a41;
}
.form-field .required { color: #dc2626; }
.form-field small {
  font-size: 11px;
  color: #6b8a7d;
  margin-top: 2px;
}

.form-input {
  padding: 10px 12px;
  border: 1.5px solid #d4e0da;
  border-radius: 8px;
  font-size: 14px;
  font-family: inherit;
  background: #f7faf8;
  color: #1a2e25;
  transition: border-color 0.2s, box-shadow 0.2s, background 0.2s;
}
.form-input:hover { border-color: #a3c4b7; background: #fff; }
.form-input:focus {
  outline: none;
  border-color: #006d4b;
  background: #fff;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

.checkbox-field { gap: 2px; }
.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: #1a2e25;
  cursor: pointer;
  font-weight: 500;
}
.checkbox-label input[type="checkbox"] {
  width: 16px; height: 16px;
  accent-color: #006d4b;
  cursor: pointer;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 6px;
  padding-top: 12px;
  border-top: 1px solid #e4ede8;
}

.btn-primary, .btn-secondary {
  padding: 9px 18px;
  font-size: 13px;
  font-weight: 600;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  font-family: inherit;
  display: inline-flex;
  align-items: center;
  gap: 6px;
}
.btn-primary {
  background: #006d4b;
  color: #fff;
  border: 1px solid #006d4b;
}
.btn-primary:hover:not(:disabled) { background: #005a3e; }
.btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
.btn-secondary {
  background: #fff;
  color: #374a41;
  border: 1px solid #d4e0da;
}
.btn-secondary:hover { background: #f4f8f6; }

.spinner-sm {
  width: 12px; height: 12px;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}
</style>

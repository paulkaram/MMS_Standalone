<template>
  <div class="space-y-5">
    <PageHeader
      :title="$t('Tags')"
      :subtitle="$t('TagDesc')"
      :breadcrumbs="[{ label: $t('Settings'), to: '/settings' }]"
    >
      <template #actions>
        <button class="btn-clean primary" @click="openAdd">
          <Icon icon="mdi:plus" class="w-4 h-4" />
          {{ $t('AddTag') }}
        </button>
      </template>
    </PageHeader>

    <div class="mg-container">
      <div v-if="loading" class="mg-state">
        <Icon icon="mdi:loading" class="w-6 h-6 animate-spin" style="color: #006d4b" />
      </div>

      <template v-else>
        <div class="mg-toolbar">
          <div class="mg-search">
            <Icon icon="mdi:magnify" class="w-4 h-4" style="color: #9ca3af" />
            <input v-model="search" type="text" :placeholder="$t('SearchTags')" />
          </div>
          <span class="mg-count">{{ filtered.length }} {{ $t('Of') }} {{ tags.length }}</span>
        </div>

        <div v-if="filtered.length > 0" class="mg-table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>#</th>
                <th>{{ $t('NameAr') }}</th>
                <th>{{ $t('NameEn') }}</th>
                <th>{{ $t('Color') }}</th>
                <th>{{ $t('UsageCount') }}</th>
                <th>{{ $t('Actions') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(tag, i) in filtered" :key="tag.id">
                <td><span class="mg-id">{{ i + 1 }}</span></td>
                <td>
                  <span
                    class="tag-preview"
                    :style="{ background: tag.color + '18', color: tag.color, borderColor: tag.color + '55' }"
                  >
                    {{ tag.nameAr }}
                  </span>
                </td>
                <td>
                  <span
                    class="tag-preview"
                    :style="{ background: tag.color + '18', color: tag.color, borderColor: tag.color + '55' }"
                  >
                    {{ tag.nameEn }}
                  </span>
                </td>
                <td>
                  <div class="color-cell">
                    <span class="color-dot" :style="{ background: tag.color }"></span>
                    <code>{{ tag.color }}</code>
                  </div>
                </td>
                <td>
                  <span class="usage-badge" :class="{ zero: tag.usageCount === 0 }">
                    {{ tag.usageCount }}
                  </span>
                </td>
                <td>
                  <div class="mg-actions">
                    <button class="mg-act" :title="$t('Edit')" @click="openEdit(tag)">
                      <Icon icon="mdi:pencil" class="w-4 h-4" />
                    </button>
                    <button class="mg-act danger" :title="$t('Delete')" @click="confirmDelete(tag)">
                      <Icon icon="mdi:delete" class="w-4 h-4" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else class="empty-state">
          <Icon icon="mdi:tag-outline" class="w-10 h-10" />
          <p>{{ $t('NoTags') }}</p>
        </div>
      </template>
    </div>

    <Modal v-model="modalOpen" :title="editingTag ? $t('EditTag') : $t('AddTag')" size="sm">
      <form class="tag-form" @submit.prevent="save">
        <div class="form-field">
          <label>{{ $t('NameAr') }} <span class="required">*</span></label>
          <input v-model="form.nameAr" type="text" class="form-input" required />
        </div>
        <div class="form-field">
          <label>{{ $t('NameEn') }} <span class="required">*</span></label>
          <input v-model="form.nameEn" type="text" class="form-input" required />
        </div>
        <div class="form-field">
          <label>{{ $t('Color') }}</label>
          <div class="color-picker">
            <input v-model="form.color" type="color" class="color-input" />
            <input v-model="form.color" type="text" class="form-input color-text" />
          </div>
        </div>
        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="modalOpen = false">{{ $t('Cancel') }}</button>
          <button type="submit" class="btn-primary" :disabled="saving">{{ $t('Save') }}</button>
        </div>
      </form>
    </Modal>

    <ConfirmModal
      v-model="confirmOpen"
      :title="$t('DeleteTag')"
      :message="$t('ConfirmDeleteTag')"
      :confirm-text="$t('Delete')"
      :cancel-text="$t('Cancel')"
      variant="danger"
      @confirm="performDelete"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import ConfirmModal from '@/components/ui/ConfirmModal.vue'
import TagsService, { type Tag } from '@/services/TagsService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const { toast } = useToast()

const tags = ref<Tag[]>([])
const search = ref('')
const loading = ref(false)
const saving = ref(false)
const modalOpen = ref(false)
const confirmOpen = ref(false)
const editingTag = ref<Tag | null>(null)
const pendingDeleteId = ref<number | null>(null)

const form = reactive({ nameAr: '', nameEn: '', color: '#006d4b' })

const filtered = computed(() => {
  if (!search.value) return tags.value
  const q = search.value.toLowerCase()
  return tags.value.filter(t => t.nameAr.includes(q) || t.nameEn.toLowerCase().includes(q))
})

const load = async () => {
  loading.value = true
  try {
    const res: any = await TagsService.listAdmin()
    tags.value = res?.data ?? res ?? []
  } catch (err) {
    console.error('Failed to load tags:', err)
  } finally {
    loading.value = false
  }
}

const openAdd = () => {
  editingTag.value = null
  form.nameAr = ''
  form.nameEn = ''
  form.color = '#006d4b'
  modalOpen.value = true
}

const openEdit = (tag: Tag) => {
  editingTag.value = tag
  form.nameAr = tag.nameAr
  form.nameEn = tag.nameEn
  form.color = tag.color
  modalOpen.value = true
}

const save = async () => {
  if (!form.nameAr.trim() || !form.nameEn.trim()) return
  saving.value = true
  try {
    if (editingTag.value) {
      await TagsService.update(editingTag.value.id, form)
      toast.success(t('TagUpdatedSuccessfully'))
    } else {
      await TagsService.create(form)
      toast.success(t('TagCreatedSuccessfully'))
    }
    modalOpen.value = false
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    saving.value = false
  }
}

const confirmDelete = (tag: Tag) => {
  pendingDeleteId.value = tag.id
  confirmOpen.value = true
}

const performDelete = async () => {
  if (!pendingDeleteId.value) return
  try {
    await TagsService.delete(pendingDeleteId.value)
    toast.success(t('TagDeletedSuccessfully'))
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    pendingDeleteId.value = null
    confirmOpen.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.space-y-5 > * + * { margin-top: 20px; }

.btn-clean {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 8px 14px; border-radius: 8px; border: none;
  font-size: 13px; font-weight: 600; cursor: pointer;
  font-family: inherit; transition: all 0.2s;
}
.btn-clean.primary { background: #006d4b; color: #fff; }
.btn-clean.primary:hover { background: #005a3e; }

.mg-container {
  background: #fff;
  border: 1px solid #e4ede8;
  border-radius: 12px;
  overflow: hidden;
}

.mg-state {
  display: flex; justify-content: center; padding: 60px 0;
}

.mg-toolbar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 12px 16px; border-bottom: 1px solid #e4ede8;
  background: #f7faf8;
}

.mg-search {
  display: flex; align-items: center; gap: 6px;
  padding: 6px 12px;
  background: #fff;
  border: 1px solid #d4e0da;
  border-radius: 8px;
  flex: 1;
  max-width: 320px;
}
.mg-search input {
  border: none; outline: none; background: transparent;
  font-size: 13px; color: #1a2e25; flex: 1;
  font-family: inherit;
}
.mg-search input::placeholder { color: #a3b5ad; }

.mg-count {
  font-size: 12px; color: #6b8a7d; font-weight: 500;
}

.mg-table-wrap { overflow-x: auto; }

.data-table { width: 100%; border-collapse: collapse; }
.data-table thead { background-color: #006d4b; }
.data-table th {
  color: #fff; font-weight: 600; font-size: 12px;
  padding: 12px 16px; text-align: start;
}
.data-table tbody tr { border-bottom: 1px solid #f0f4f2; transition: background 0.15s; }
.data-table tbody tr:hover { background: rgba(0, 109, 75, 0.04); }
.data-table td { padding: 12px 16px; font-size: 13px; color: #374151; }

.mg-id {
  display: inline-block; min-width: 20px;
  font-weight: 700; color: #006d4b;
}

.tag-preview {
  display: inline-block;
  padding: 3px 10px;
  font-size: 12px;
  font-weight: 600;
  border-radius: 12px;
  border: 1px solid;
}

.color-cell { display: flex; align-items: center; gap: 8px; }
.color-dot {
  width: 18px; height: 18px;
  border-radius: 50%;
  border: 1px solid #e4ede8;
}
.color-cell code {
  font-family: 'Consolas', monospace;
  font-size: 11px;
  color: #5a7a6d;
}

.usage-badge {
  display: inline-block; padding: 2px 8px;
  background: #e8f5ef; color: #006d4b;
  border: 1px solid #c8ddd3;
  border-radius: 10px;
  font-size: 11px; font-weight: 600;
}
.usage-badge.zero { background: #f5f5f5; color: #94a3b8; border-color: #e5e7eb; }

.mg-actions { display: flex; gap: 4px; }
.mg-act {
  display: inline-flex; align-items: center; justify-content: center;
  width: 30px; height: 30px;
  border: 1px solid #e4ede8; background: #fff;
  border-radius: 6px; color: #5a7a6d;
  cursor: pointer; transition: all 0.15s;
}
.mg-act:hover {
  background: #f4f8f6; border-color: #006d4b; color: #006d4b;
}
.mg-act.danger:hover {
  background: #fef2f2; border-color: #ef4444; color: #ef4444;
}

.empty-state {
  display: flex; flex-direction: column; align-items: center;
  padding: 48px 20px; color: #93afa4; gap: 10px;
}
.empty-state p { font-size: 13px; margin: 0; }

.tag-form { display: flex; flex-direction: column; gap: 14px; }
.form-field { display: flex; flex-direction: column; gap: 4px; }
.form-field label { font-size: 12px; font-weight: 600; color: #374a41; }
.form-field .required { color: #dc2626; }

.form-input {
  padding: 10px 12px;
  border: 1.5px solid #d4e0da;
  border-radius: 8px;
  font-size: 14px; background: #f7faf8;
  color: #1a2e25;
  font-family: inherit;
}
.form-input:focus {
  outline: none; border-color: #006d4b;
  background: #fff;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

.color-picker { display: flex; gap: 8px; align-items: stretch; }
.color-input {
  width: 44px; height: 40px;
  padding: 0; border: 1.5px solid #d4e0da;
  border-radius: 8px; cursor: pointer;
  background: #fff;
}
.color-text { flex: 1; font-family: 'Consolas', monospace; }

.form-actions {
  display: flex; justify-content: flex-end; gap: 8px;
  padding-top: 8px; border-top: 1px solid #e4ede8;
  margin-top: 6px;
}

.btn-primary, .btn-secondary {
  padding: 9px 18px; font-size: 13px; font-weight: 600;
  border-radius: 8px; cursor: pointer;
  transition: all 0.2s; font-family: inherit;
}
.btn-primary {
  background: #006d4b; color: #fff; border: 1px solid #006d4b;
}
.btn-primary:hover:not(:disabled) { background: #005a3e; }
.btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
.btn-secondary {
  background: #fff; color: #374a41;
  border: 1px solid #d4e0da;
}
.btn-secondary:hover { background: #f4f8f6; }
</style>

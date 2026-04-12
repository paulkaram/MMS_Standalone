<template>
  <div class="space-y-5">
    <!-- Page Header -->
    <PageHeader :title="$t('Dictionary')" :subtitle="$t('ManageDictionary')" :breadcrumbs="[{ label: 'Settings', to: '/settings' }]">
      <template #actions>
        <button class="btn-clean primary" @click="openAddDialog">
          <Icon icon="mdi:plus" class="w-4 h-4" />
          {{ $t('AddNew') }}
        </button>
      </template>
    </PageHeader>

    <!-- Grid Container -->
    <div class="mg-container">
      <!-- Loading -->
      <div v-if="loading" class="mg-state">
        <Icon icon="mdi:loading" class="w-6 h-6 animate-spin" style="color: #008080" />
      </div>

      <template v-else>
        <!-- Search Bar -->
        <div class="mg-toolbar">
          <div class="mg-search">
            <Icon icon="mdi:magnify" class="w-4 h-4" style="color: #9ca3af" />
            <input v-model="search" type="text" :placeholder="$t('SearchDictionary')" />
          </div>
          <span class="mg-count">{{ filteredItems.length }} {{ $t('Of') }} {{ items.length }}</span>
        </div>

        <!-- Table -->
        <template v-if="filteredItems.length > 0">
          <div class="mg-table-wrap">
            <table class="data-table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>{{ $t('Keyword') }}</th>
                  <th>{{ $t('Arabic') }}</th>
                  <th>{{ $t('English') }}</th>
                  <th>{{ $t('Actions') }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(item, index) in paginatedItems" :key="item.id">
                  <td><span class="mg-id">{{ (currentPage - 1) * pageSize + index + 1 }}</span></td>
                  <td><code class="dict-keyword">{{ item.keyword }}</code></td>
                  <td dir="rtl">{{ item.ar || '-' }}</td>
                  <td dir="ltr">{{ item.en || '-' }}</td>
                  <td>
                    <div class="mg-actions">
                      <button class="mg-act" :title="$t('Edit')" @click="editItem(item)">
                        <Icon icon="mdi:pencil-outline" class="w-4 h-4" />
                      </button>
                      <button class="mg-act danger" :title="$t('Delete')" @click="deleteItem(item.id)">
                        <Icon icon="mdi:trash-can-outline" class="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Pagination -->
          <div v-if="filteredItems.length > pageSize" class="mg-pag">
            <span class="mg-pag-info">
              {{ $t('Showing') }} <strong>{{ (currentPage - 1) * pageSize + 1 }}</strong>
              {{ $t('To') }} <strong>{{ Math.min(currentPage * pageSize, filteredItems.length) }}</strong>
              {{ $t('Of') }} <strong>{{ filteredItems.length }}</strong>
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

        <!-- Empty -->
        <div v-else class="mg-state">
          <Icon icon="mdi:book-search-outline" class="w-12 h-12" style="color: #ccc" />
          <p>{{ $t('NoResults') }}</p>
        </div>
      </template>
    </div>

    <!-- Add/Edit Dialog -->
    <Modal
      v-model="dialog"
      :title="isEdit ? ($t('EditEntry')) : ($t('AddEntry'))"
      :icon="isEdit ? 'mdi:pencil' : 'mdi:plus'"
      size="md"
    >
      <form @submit.prevent="save" class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-zinc-700 mb-1">{{ $t('Keyword') }}</label>
          <input v-model="form.keyword" type="text" :placeholder="$t('EnterKeyword')" required
            class="w-full px-4 py-2.5 border border-zinc-200 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/10" />
          <p class="text-xs text-zinc-400 mt-1">{{ $t('KeywordHint') }}</p>
        </div>
        <div>
          <label class="block text-sm font-medium text-zinc-700 mb-1">{{ $t('ArabicTranslation') }}</label>
          <input v-model="form.ar" type="text" dir="rtl" :placeholder="$t('EnterArabic')" required
            class="w-full px-4 py-2.5 border border-zinc-200 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/10" />
        </div>
        <div>
          <label class="block text-sm font-medium text-zinc-700 mb-1">{{ $t('EnglishTranslation') }}</label>
          <input v-model="form.en" type="text" dir="ltr" :placeholder="$t('EnterEnglish')" required
            class="w-full px-4 py-2.5 border border-zinc-200 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/10" />
        </div>
      </form>

      <template #footer>
        <Button variant="outline" @click="dialog = false">{{ $t('Cancel') }}</Button>
        <Button variant="primary" :loading="saving" @click="save">
          {{ isEdit ? ($t('SaveChanges')) : ($t('AddEntry')) }}
        </Button>
      </template>
    </Modal>

    <!-- Confirm Delete Dialog -->
    <Modal v-model="confirmDialog" :title="$t('DeleteEntry')" icon="mdi:trash-can" variant="danger" size="sm">
      <div class="text-center py-4">
        <Icon icon="mdi:alert-circle" class="w-12 h-12 text-red-400 mx-auto mb-3" />
        <p class="text-zinc-600">{{ $t('ConfirmDelete') }}</p>
      </div>
      <template #footer>
        <Button variant="outline" @click="confirmDialog = false">{{ $t('Cancel') }}</Button>
        <Button variant="danger" :loading="deleting" @click="confirmDelete">{{ $t('Delete') }}</Button>
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
import DictionaryService from '@/services/DictionaryService'

// Page size options
const pageSizeOptions = [
  { id: 10, name: '10' },
  { id: 25, name: '25' },
  { id: 50, name: '50' },
  { id: 100, name: '100' }
]

// Types
interface DictionaryItem {
  id: string
  keyword: string
  ar: string
  en: string
}

// State
const loading = ref(false)
const saving = ref(false)
const deleting = ref(false)
const items = ref<DictionaryItem[]>([])
const search = ref('')
const dialog = ref(false)
const confirmDialog = ref(false)
const isEdit = ref(false)
const selectedId = ref<string | null>(null)
const itemToDelete = ref<string | null>(null)

// Pagination
const currentPage = ref(1)
const pageSize = ref(10)

const form = ref({
  keyword: '',
  ar: '',
  en: ''
})

// Computed
const arabicCount = computed(() => items.value.filter(i => i.ar).length)
const englishCount = computed(() => items.value.filter(i => i.en).length)

const filteredItems = computed(() => {
  if (!search.value) return items.value
  const query = search.value.toLowerCase()
  return items.value.filter(item =>
    item.keyword?.toLowerCase().includes(query) ||
    item.ar?.toLowerCase().includes(query) ||
    item.en?.toLowerCase().includes(query)
  )
})

const totalPages = computed(() => Math.ceil(filteredItems.value.length / pageSize.value) || 1)

const paginatedItems = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredItems.value.slice(start, end)
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
const loadDictionary = async () => {
  loading.value = true
  try {
    const response = await DictionaryService.listDictionary()
    // Handle API wrapper format
    items.value = (response as any).data || response || []
  } catch (error) {
    console.error('Failed to load dictionary:', error)
  } finally {
    loading.value = false
  }
}

const openAddDialog = () => {
  isEdit.value = false
  selectedId.value = null
  form.value = { keyword: '', ar: '', en: '' }
  dialog.value = true
}

const editItem = (item: DictionaryItem) => {
  isEdit.value = true
  selectedId.value = item.id
  form.value = {
    keyword: item.keyword,
    ar: item.ar,
    en: item.en
  }
  dialog.value = true
}

const save = async () => {
  if (!form.value.keyword || !form.value.ar || !form.value.en) return

  saving.value = true
  try {
    if (isEdit.value && selectedId.value) {
      await DictionaryService.updateDictionary(selectedId.value, form.value)
    } else {
      await DictionaryService.addDictionary(form.value)
    }
    dialog.value = false
    await loadDictionary()
  } catch (error) {
    console.error('Failed to save:', error)
  } finally {
    saving.value = false
  }
}

const deleteItem = (id: string) => {
  itemToDelete.value = id
  confirmDialog.value = true
}

const confirmDelete = async () => {
  if (!itemToDelete.value) return

  deleting.value = true
  try {
    await DictionaryService.deleteDictionary(itemToDelete.value)
    confirmDialog.value = false
    itemToDelete.value = null
    await loadDictionary()
  } catch (error) {
    console.error('Failed to delete:', error)
  } finally {
    deleting.value = false
  }
}

// Lifecycle
onMounted(() => {
  loadDictionary()
})
</script>

<style scoped>
/* Search toolbar */
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

/* Keyword code badge */
.dict-keyword {
  display: inline-block;
  padding: 3px 8px;
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
  border-radius: 6px;
  font-size: 0.78em;
  font-weight: 600;
  font-family: monospace;
}
</style>

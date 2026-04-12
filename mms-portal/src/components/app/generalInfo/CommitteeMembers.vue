<template>
  <div class="members-section">
    <!-- Loading -->
    <div v-if="loading && members.length === 0" class="loading-state">
      <div class="spinner"></div>
    </div>

    <!-- Members Grid -->
    <div v-else-if="members.length > 0" class="members-grid">
      <div
        v-for="member in members"
        :key="member.id"
        class="member-card"
      >
        <UserAvatar :userId="member.userId || member.id" :name="member.fullname || ''" size="md" />
        <div class="member-info">
          <h4 class="member-name">{{ member.fullname }}</h4>
          <span class="member-role">{{ member.roleName }}</span>
        </div>
        <div class="member-meta">
          <div v-if="member.email" class="meta-item">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/>
              <polyline points="22,6 12,13 2,6"/>
            </svg>
            <span>{{ member.email }}</span>
          </div>
          <div v-if="member.privacyName" class="privacy-badge">
            {{ member.privacyName }}
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
        <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z"/>
      </svg>
      <p>{{ $t('NoMembers') }}</p>
    </div>

    <!-- Pagination -->
    <div v-if="totalCount > pageSize" class="pagination">
      <button
        class="page-btn"
        :disabled="page === 1"
        @click="goToPage(page - 1)"
      >
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M15 18l-6-6 6-6" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </button>
      <span class="page-info">{{ page }} / {{ totalPages }}</span>
      <button
        class="page-btn"
        :disabled="page >= totalPages"
        @click="goToPage(page + 1)"
      >
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M9 18l6-6-6-6" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'

const props = defineProps<{
  committeeId: string
}>()

const emit = defineEmits<{
  'update:count': [count: number]
}>()

// State
const loading = ref(false)
const members = ref<any[]>([])
const totalCount = ref(0)
const page = ref(1)
const pageSize = ref(6)
// Computed
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

// Methods
const loadMembers = async () => {
  loading.value = true
  try {
    const result = await CouncilCommitteesService.listUsersInCouncilCommitteeGeneralInfo(
      props.committeeId,
      page.value,
      pageSize.value
    )
    // API returns { success, data: { data: [], total } }
    members.value = result.data?.data || result.data || []
    totalCount.value = result.data?.total || result.total || 0
    emit('update:count', totalCount.value)
  } catch (error) {
    console.error('Failed to load members:', error)
  } finally {
    loading.value = false
  }
}

const goToPage = (newPage: number) => {
  page.value = newPage
  loadMembers()
}

// Watch for committeeId changes
watch(() => props.committeeId, () => {
  page.value = 1
  loadMembers()
})

// Lifecycle
onMounted(() => {
  loadMembers()
})
</script>

<style scoped>
.members-section {
  /* No wrapper styling - parent card provides it */
}

/* Loading */
.loading-state {
  display: flex;
  justify-content: center;
  padding: 40px;
}

.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid #e4e4e7;
  border-top-color: #006d4b;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Members Grid */
.members-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 16px;
}

.member-card {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  padding: 16px;
  background: #fafafa;
  border-radius: 12px;
  transition: all 0.2s;
}

.member-card:hover {
  background: #f4f4f5;
  transform: translateY(-1px);
}

.member-info {
  flex: 1;
  min-width: 0;
}

.member-name {
  font-size: 15px;
  font-weight: 600;
  color: #27272a;
  margin: 0 0 4px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.member-role {
  font-size: 13px;
  color: #006d4b;
  font-weight: 500;
}

.member-meta {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 6px;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: #71717a;
}

.meta-item svg {
  width: 14px;
  height: 14px;
}

.privacy-badge {
  font-size: 11px;
  padding: 2px 8px;
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
  border-radius: 10px;
  font-weight: 500;
}

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px;
  color: #a1a1aa;
}

.empty-state svg {
  width: 48px;
  height: 48px;
  margin-bottom: 12px;
}

.empty-state p {
  font-size: 14px;
  margin: 0;
}

/* Pagination */
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #e4e4e7;
}

.page-btn {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fff;
  border: 1px solid #e4e4e7;
  border-radius: 8px;
  color: #71717a;
  cursor: pointer;
  transition: all 0.2s;
}

.page-btn:hover:not(:disabled) {
  background: #fafafa;
  border-color: #d4d4d8;
  color: #3f3f46;
}

.page-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-btn svg {
  width: 18px;
  height: 18px;
}

[dir="rtl"] .page-btn svg {
  transform: scaleX(-1);
}

.page-info {
  font-size: 14px;
  color: #71717a;
}
</style>

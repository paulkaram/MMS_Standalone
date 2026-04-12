<template>
  <div class="activities-section">
    <!-- Loading -->
    <div v-if="loading && activities.length === 0" class="loading-state">
      <div class="spinner"></div>
    </div>

    <!-- Activities List -->
    <div v-else-if="activities.length > 0" class="activities-list">
      <div
        v-for="activity in activities"
        :key="activity.id"
        class="activity-card"
      >
        <div class="activity-id">#{{ activity.id }}</div>
        <div class="activity-content">
          <h4 class="activity-title">{{ activity.title }}</h4>
          <p v-if="activity.description" class="activity-description">{{ activity.description }}</p>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
        <polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/>
      </svg>
      <p>{{ $t('NoActivities') }}</p>
    </div>

    <!-- Pagination -->
    <div v-if="totalCount > pageSize" class="pagination">
      <button class="page-btn" :disabled="page === 1" @click="goToPage(page - 1)">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M15 18l-6-6 6-6" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </button>
      <span class="page-info">{{ page }} / {{ totalPages }}</span>
      <button class="page-btn" :disabled="page >= totalPages" @click="goToPage(page + 1)">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M9 18l6-6-6-6" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'

const props = defineProps<{
  committeeId: string
}>()

const emit = defineEmits<{
  'update:count': [count: number]
}>()

// State
const loading = ref(false)
const activities = ref<any[]>([])
const totalCount = ref(0)
const page = ref(1)
const pageSize = ref(5)

// Computed
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

// Methods
const loadActivities = async () => {
  loading.value = true
  try {
    const result = await CouncilCommitteesService.listCommitteeActivitiesGeneralInfo(
      props.committeeId,
      page.value,
      pageSize.value
    )
    // API returns { success, data: { data: [], total } }
    activities.value = result.data?.data || result.data || []
    totalCount.value = result.data?.total || result.total || 0
    emit('update:count', totalCount.value)
  } catch (error) {
    console.error('Failed to load activities:', error)
  } finally {
    loading.value = false
  }
}

const goToPage = (newPage: number) => {
  page.value = newPage
  loadActivities()
}

// Watch
watch(() => props.committeeId, () => {
  page.value = 1
  loadActivities()
})

// Lifecycle
onMounted(() => {
  loadActivities()
})
</script>

<style scoped>
.activities-section {
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

/* Activities List */
.activities-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.activity-card {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  padding: 14px 16px;
  background: #fafafa;
  border-radius: 10px;
  transition: all 0.2s;
}

.activity-card:hover {
  background: #f4f4f5;
}

.activity-id {
  flex-shrink: 0;
  font-size: 12px;
  font-weight: 600;
  color: #f97316;
  background: rgba(249, 115, 22, 0.1);
  padding: 4px 10px;
  border-radius: 6px;
}

.activity-content {
  flex: 1;
  min-width: 0;
}

.activity-title {
  font-size: 14px;
  font-weight: 600;
  color: #27272a;
  margin: 0 0 4px;
}

.activity-description {
  font-size: 13px;
  color: #71717a;
  margin: 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
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

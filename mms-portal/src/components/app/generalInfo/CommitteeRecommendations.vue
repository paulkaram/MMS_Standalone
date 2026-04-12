<template>
  <div class="recommendations-section">
    <!-- Loading -->
    <div v-if="loading && recommendations.length === 0" class="loading-state">
      <div class="spinner"></div>
    </div>

    <!-- Recommendations Grid -->
    <div v-else-if="recommendations.length > 0" class="recommendations-grid">
      <div
        v-for="rec in recommendations"
        :key="rec.id"
        class="rec-card"
        @click="openRecommendation(rec)"
      >
        <!-- Header: Title & Status -->
        <div class="rec-header">
          <h4 class="rec-title">{{ rec.text }}</h4>
          <span class="status-badge" :style="getStatusStyle(getProgressStatusColor(rec.percentage || 0))">
            {{ getStatusText(rec.percentage) }}
          </span>
        </div>

        <!-- Progress Bar -->
        <div class="progress-section">
          <div class="progress-bar">
            <div
              class="progress-fill"
              :style="{ width: `${rec.percentage || 0}%`, backgroundColor: getProgressColor(rec.percentage) }"
            ></div>
          </div>
          <span class="progress-text" :style="{ color: getProgressColor(rec.percentage) }">
            {{ rec.percentage || 0 }}%
          </span>
        </div>

        <!-- Footer: Meta Info -->
        <div class="rec-footer">
          <!-- Owner with Profile Picture -->
          <div class="owner-info" v-if="rec.ownerName">
            <UserAvatar :userId="rec.owner" :name="rec.ownerName || ''" size="xs" />
            <span class="owner-name">{{ rec.ownerName }}</span>
          </div>

          <!-- Meta Items -->
          <div class="meta-items">
            <span v-if="rec.meetingReferenceNo" class="meta-tag ref-tag">
              #{{ rec.meetingReferenceNo }}
            </span>
            <span v-if="rec.dueDate" class="meta-tag" :class="{ 'overdue-tag': isOverdue(rec.dueDate) }">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
                <line x1="16" y1="2" x2="16" y2="6"/>
                <line x1="8" y1="2" x2="8" y2="6"/>
                <line x1="3" y1="10" x2="21" y2="10"/>
              </svg>
              {{ formatDate(rec.dueDate) }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
        <path d="M9 11l3 3L22 4M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11"/>
      </svg>
      <p>{{ $t('NoRecommendations') }}</p>
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
import { useRouter } from 'vue-router'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'
import { formatDate as formatDateHelper } from '@/helpers/dateFormat'
import { getProgressStatusColor, getStatusStyle } from '@/helpers/statusColors'

const props = defineProps<{
  committeeId: string
}>()

const emit = defineEmits<{
  'update:count': [count: number]
  'update:progress': [progress: number]
}>()

const router = useRouter()

// State
const loading = ref(false)
const recommendations = ref<any[]>([])
const totalCount = ref(0)
const page = ref(1)
const pageSize = ref(6)
// Computed
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

const avgProgress = computed(() => {
  if (recommendations.value.length === 0) return 0
  const total = recommendations.value.reduce((sum, r) => sum + (r.percentage || 0), 0)
  return Math.round(total / recommendations.value.length)
})

// Methods
const loadRecommendations = async () => {
  loading.value = true
  try {
    const result = await CouncilCommitteesService.listCouncilCommitteeTasks(
      props.committeeId,
      page.value,
      pageSize.value
    )
    // API returns { success, data: { data: [], total } }
    recommendations.value = result.data?.data || result.data || []
    totalCount.value = result.data?.total || result.total || 0
    emit('update:count', totalCount.value)
    emit('update:progress', avgProgress.value)
  } catch (error) {
    console.error('Failed to load recommendations:', error)
  } finally {
    loading.value = false
  }
}

const goToPage = (newPage: number) => {
  page.value = newPage
  loadRecommendations()
}

const openRecommendation = (rec: any) => {
  router.push({
    name: 'recommendation-details',
    params: { id: rec.id },
    query: { viewMode: 'true' }
  })
}

const getProgressColor = (percentage: number) => {
  return getProgressStatusColor(percentage).text
}

const getStatusText = (percentage: number) => {
  if (percentage >= 100) return 'مكتمل'
  if (percentage >= 50) return 'قيد التنفيذ'
  return 'قيد الانتظار'
}

const isOverdue = (dueDate: string) => {
  if (!dueDate) return false
  return new Date(dueDate) < new Date()
}

const formatDate = (date: string) => {
  if (!date) return '-'
  return formatDateHelper(new Date(date))
}

// Watch
watch(() => props.committeeId, () => {
  page.value = 1
  loadRecommendations()
})

// Lifecycle
onMounted(() => {
  loadRecommendations()
})
</script>

<style scoped>
.recommendations-section {
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

/* Recommendations Grid - 2 columns */
.recommendations-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 14px;
}

@media (max-width: 768px) {
  .recommendations-grid {
    grid-template-columns: 1fr;
  }
}

/* Recommendation Card */
.rec-card {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 14px;
  background: #fafafa;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.rec-card:hover {
  background: #fff;
  border-color: #e4e4e7;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

/* Header */
.rec-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 10px;
}

.rec-title {
  flex: 1;
  font-size: 14px;
  font-weight: 600;
  color: #27272a;
  margin: 0;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.status-badge {
  flex-shrink: 0;
  font-size: 10px;
  padding: 4px 10px;
  border-radius: 6px;
  font-weight: 600;
  white-space: nowrap;
  border: 1px solid;
}

/* Progress Section */
.progress-section {
  display: flex;
  align-items: center;
  gap: 10px;
}

.progress-bar {
  flex: 1;
  height: 6px;
  background: #e4e4e7;
  border-radius: 3px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  border-radius: 3px;
  transition: width 0.3s ease;
}

.progress-text {
  font-size: 12px;
  font-weight: 700;
  min-width: 36px;
  text-align: end;
}

/* Footer */
.rec-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}

/* Owner Info */
.owner-info {
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 0;
}

.owner-name {
  font-size: 12px;
  color: #71717a;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* Meta Items */
.meta-items {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-shrink: 0;
}

.meta-tag {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 10px;
  padding: 3px 8px;
  background: #f4f4f5;
  color: #71717a;
  border-radius: 10px;
  font-weight: 500;
}

.meta-tag svg {
  width: 10px;
  height: 10px;
}

.ref-tag {
  background: rgba(99, 102, 241, 0.1);
  color: #6366f1;
}

.overdue-tag {
  background: rgba(239, 68, 68, 0.1);
  color: #ef4444;
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

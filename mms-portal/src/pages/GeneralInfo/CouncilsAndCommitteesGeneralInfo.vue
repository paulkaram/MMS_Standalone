<template>
  <div class="councils-committees-page">
    <!-- Page Header -->
    <PageHeader
      :title="$t('CouncilsAndCommittees')"
      :subtitle="$t('ManageCouncilsAndCommittees')"
    />

    <!-- Breadcrumb -->
    <nav v-if="parents.length > 0" class="breadcrumb-nav">
        <button class="breadcrumb-back" @click="goBack">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M19 12H5M12 19l-7-7 7-7" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </button>
        <div class="breadcrumb-trail">
          <button class="breadcrumb-home" @click="navigateToRoot">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>
              <polyline points="9 22 9 12 15 12 15 22"/>
            </svg>
          </button>
          <template v-for="(parent, index) in parents" :key="parent.id">
            <svg class="breadcrumb-separator" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M9 18l6-6-6-6" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
            <span
              class="breadcrumb-item"
              :class="{ active: index === parents.length - 1, clickable: index < parents.length - 1 }"
              @click="index < parents.length - 1 && navigateToBreadcrumb(parent)"
            >
              {{ parent.name }}
            </span>
          </template>
        </div>
      </nav>

      <div class="content-grid" :class="{ 'has-sidebar': !councilId }">
        <!-- Search Sidebar -->
        <aside v-if="!councilId" class="search-sidebar">
          <div class="search-card">
            <div class="search-header">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="11" cy="11" r="8"/>
                <path d="m21 21-4.35-4.35"/>
              </svg>
              <h3>{{ $t('AdvancedSearch') }}</h3>
            </div>

            <div class="search-fields">
              <div class="field-group">
                <label>{{ $t('Committee') }}</label>
                <input
                  type="text"
                  v-model="searchCriteria.committeeTitle"
                  :placeholder="$t('SearchByCommittee')"
                  @keydown.enter="loadCommittees"
                >
              </div>

              <div class="field-group">
                <label>{{ $t('MeetingSubject') }}</label>
                <input
                  type="text"
                  v-model="searchCriteria.meetingTitle"
                  :placeholder="$t('SearchByMeeting')"
                  @keydown.enter="loadCommittees"
                >
              </div>

              <div class="field-group">
                <label>{{ $t('Agenda') }}</label>
                <input
                  type="text"
                  v-model="searchCriteria.agendaTitle"
                  :placeholder="$t('SearchByAgenda')"
                  @keydown.enter="loadCommittees"
                >
              </div>

              <div class="field-group">
                <label>{{ $t('Topics') }}</label>
                <input
                  type="text"
                  v-model="searchCriteria.agendaTopicTitle"
                  :placeholder="$t('SearchByTopic')"
                  @keydown.enter="loadCommittees"
                >
              </div>

              <div class="field-group">
                <label>{{ $t('AgendaNotes') }}</label>
                <input
                  type="text"
                  v-model="searchCriteria.agendaNote"
                  :placeholder="$t('SearchByNotes')"
                  @keydown.enter="loadCommittees"
                >
              </div>

              <div class="field-group">
                <label>{{ $t('Recommendations') }}</label>
                <input
                  type="text"
                  v-model="searchCriteria.recommendationTitle"
                  :placeholder="$t('SearchByRecommendation')"
                  @keydown.enter="loadCommittees"
                >
              </div>
            </div>

            <div class="search-actions">
              <button class="btn-search" :disabled="loading" @click="loadCommittees">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <circle cx="11" cy="11" r="8"/>
                  <path d="m21 21-4.35-4.35"/>
                </svg>
                {{ $t('Search') }}
              </button>
              <button class="btn-reset" :disabled="loading" @click="reset">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8"/>
                  <path d="M21 3v5h-5"/>
                  <path d="M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16"/>
                  <path d="M8 16H3v5"/>
                </svg>
                {{ $t('Reset') }}
              </button>
            </div>
          </div>
        </aside>

        <!-- Main Content -->
        <main class="main-content">
          <!-- Stats Summary -->
          <div class="stats-row" v-if="!loading && committees.length > 0">
            <StatCard :title="$t('Councils')" :value="councilsCount" icon="mdi:sitemap" color="primary" />
            <StatCard :title="$t('Committees')" :value="committeesCount" icon="mdi:account-group" color="info" />
            <StatCard :title="$t('Total')" :value="committees.length" icon="mdi:view-grid" color="warning" />
          </div>

          <!-- Loading -->
          <div v-if="loading" class="loading-state">
            <div class="loading-spinner"></div>
            <p>{{ $t('Loading') }}</p>
          </div>

          <!-- Committees List -->
          <div v-else-if="filteredCommittees.length > 0" class="committees-list">
            <div
              v-for="committee in filteredCommittees"
              :key="committee.id"
              class="committee-card"
            >
              <!-- Icon -->
              <div class="card-icon" :class="committee.typeId === 1 ? 'council' : 'committee'">
                <svg v-if="committee.typeId === 1" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                  <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/>
                </svg>
                <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                  <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                  <circle cx="9" cy="7" r="4"/>
                  <path d="M23 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75"/>
                </svg>
              </div>

              <!-- Info -->
              <div class="card-info">
                <h3 class="card-title">{{ committee.name }}</h3>
                <div class="card-meta">
                  <span class="meta-type" :class="committee.typeId === 1 ? 'council' : 'committee'">
                    {{ committee.typeId === 1 ? ($t('Council')) : ($t('Committee')) }}
                  </span>
                  <span class="meta-code">{{ committee.code }}</span>
                </div>
              </div>

              <!-- Actions -->
              <div class="card-actions">
                <button
                  v-if="committee.hasChilds"
                  class="btn-sub"
                  @click="navigateToCommittee(committee)"
                >
                  {{ $t('SubCommittees') }}
                  <span class="count">{{ committee.childernsCount }}</span>
                </button>
                <router-link
                  v-if="committee.showDetails"
                  :to="{ name: 'committee-details', params: { committeeId: committee.id } }"
                  class="btn-view"
                >
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M5 12h14M12 5l7 7-7 7" stroke-linecap="round" stroke-linejoin="round"/>
                  </svg>
                </router-link>
              </div>
            </div>
          </div>

          <!-- Empty State -->
          <div v-else class="empty-state">
            <div class="empty-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                <circle cx="9" cy="7" r="4"/>
                <line x1="17" y1="11" x2="23" y2="11"/>
              </svg>
            </div>
            <h3>{{ $t('NoResults') }}</h3>
            <p>{{ $t('NoCouncilsOrCommitteesFound') }}</p>
            <button v-if="hasSearchCriteria" class="btn-clear-search" @click="reset">
              {{ $t('ClearSearch') }}
            </button>
          </div>
        </main>
      </div>
  </div>
</template>

<script setup lang="ts">
import PageHeader from '@/components/layout/PageHeader.vue'
import StatCard from '@/components/app/dashboard/StatCard.vue'
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'

const route = useRoute()
const router = useRouter()

// State
const loading = ref(false)
const committees = ref<any[]>([])
const parents = ref<any[]>([])

const searchCriteria = ref({
  committeeTitle: '',
  meetingTitle: '',
  agendaTitle: '',
  agendaTopicTitle: '',
  recommendationTitle: '',
  agendaNote: ''
})

// Computed
const councilId = computed(() => route.params.councilId as string | undefined)

const filteredCommittees = computed(() => {
  return committees.value || []
})

const councilsCount = computed(() => {
  return committees.value.filter(c => c.typeId === 1).length
})

const committeesCount = computed(() => {
  return committees.value.filter(c => c.typeId !== 1).length
})

const hasSearchCriteria = computed(() => {
  return Object.values(searchCriteria.value).some(v => v.trim() !== '')
})

// Methods
const loadCommittees = async () => {
  loading.value = true

  try {
    if (!councilId.value) {
      // Load all user committees with search
      const searchObj = {
        committeeTitle: searchCriteria.value.committeeTitle || '',
        meetingTitle: searchCriteria.value.meetingTitle || '',
        agendaTitle: searchCriteria.value.agendaTitle || '',
        agendaTopicTitle: searchCriteria.value.agendaTopicTitle || '',
        recommendationTitle: searchCriteria.value.recommendationTitle || '',
        agendaNote: searchCriteria.value.agendaNote || ''
      }
      const result = await CouncilCommitteesService.listUserCouncilsAndCommitteesForGeneralInfo(searchObj)
      committees.value = result.data || result
      parents.value = []
    } else {
      // Load sub-committees for a specific council
      const [councilData, committeesList] = await Promise.all([
        CouncilCommitteesService.getCouncil(councilId.value),
        CouncilCommitteesService.getCommitteesByCouncilId(councilId.value)
      ])

      const committeesData = committeesList.data || committeesList
      committees.value = committeesData

      if (committeesData.length > 0 && committeesData[0].parents) {
        parents.value = committeesData[0].parents
      }
    }
  } catch (error) {
    console.error('Failed to load committees:', error)
    committees.value = []
  } finally {
    loading.value = false
  }
}

const navigateToCommittee = (committee: any) => {
  parents.value = committee.parents || []
  router.push({
    name: 'council-committees-general-info',
    params: { councilId: committee.id }
  })
}

const navigateToBreadcrumb = (parent: any) => {
  router.push({
    name: 'council-committees-general-info',
    params: { councilId: parent.id }
  })
}

const navigateToRoot = () => {
  router.push({ name: 'council-committees-general-info' })
}

const goBack = () => {
  router.back()
}

const reset = () => {
  searchCriteria.value = {
    committeeTitle: '',
    meetingTitle: '',
    agendaTitle: '',
    agendaTopicTitle: '',
    recommendationTitle: '',
    agendaNote: ''
  }
  loadCommittees()
}

// Watch for route changes
watch(
  () => route.params.councilId,
  () => {
    parents.value = []
    loadCommittees()
  }
)

// Lifecycle
onMounted(() => {
  loadCommittees()
})
</script>

<style scoped>
.councils-committees-page {
  @apply space-y-6;
}

/* Breadcrumb */
.breadcrumb-nav {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px 20px;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  border: 1px solid #e4e4e7;
}

.breadcrumb-back {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f4f4f5;
  border: none;
  border-radius: 8px;
  color: #71717a;
  cursor: pointer;
  transition: all 0.2s;
  flex-shrink: 0;
}

.breadcrumb-back:hover {
  background: #e4e4e7;
  color: #3f3f46;
}

.breadcrumb-back svg {
  width: 18px;
  height: 18px;
}

[dir="rtl"] .breadcrumb-back svg {
  transform: scaleX(-1);
}

.breadcrumb-trail {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  font-size: 14px;
}

.breadcrumb-home {
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: none;
  color: #71717a;
  cursor: pointer;
  transition: all 0.2s;
  border-radius: 6px;
}

.breadcrumb-home:hover {
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
}

.breadcrumb-home svg {
  width: 16px;
  height: 16px;
}

.breadcrumb-separator {
  width: 14px;
  height: 14px;
  color: #d4d4d8;
  flex-shrink: 0;
}

[dir="rtl"] .breadcrumb-separator {
  transform: scaleX(-1);
}

.breadcrumb-item {
  padding: 4px 8px;
  border-radius: 6px;
  color: #71717a;
  transition: all 0.2s;
}

.breadcrumb-item.clickable {
  cursor: pointer;
}

.breadcrumb-item.clickable:hover {
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
}

.breadcrumb-item.active {
  color: #27272a;
  font-weight: 600;
}

/* Content Grid */
.content-grid {
  display: grid;
  gap: 24px;
}

.content-grid.has-sidebar {
  grid-template-columns: 1fr;
}

@media (min-width: 1024px) {
  .content-grid.has-sidebar {
    grid-template-columns: 320px 1fr;
  }
}

/* Search Sidebar */
.search-sidebar {
  order: 2;
}

@media (min-width: 1024px) {
  .search-sidebar {
    order: 1;
  }
}

.search-card {
  background: #fff;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  border: 1px solid #e4e4e7;
  position: sticky;
  top: 80px;
}

.search-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 1px solid #e4e4e7;
}

.search-header svg {
  width: 24px;
  height: 24px;
  color: #006d4b;
}

.search-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: #27272a;
  margin: 0;
}

.search-fields {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-bottom: 24px;
}

.field-group label {
  display: block;
  font-size: 13px;
  font-weight: 500;
  color: #3f3f46;
  margin-bottom: 6px;
}

.field-group input {
  width: 100%;
  padding: 10px 14px;
  font-size: 14px;
  border: 1px solid #e4e4e7;
  border-radius: 8px;
  background: #fafafa;
  color: #27272a;
  transition: all 0.2s;
}

.field-group input:focus {
  outline: none;
  border-color: #006d4b;
  background: #fff;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

.field-group input::placeholder {
  color: #a1a1aa;
}

.search-actions {
  display: flex;
  gap: 12px;
}

.btn-search,
.btn-reset {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px 16px;
  font-size: 14px;
  font-weight: 500;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-search {
  background: #006d4b;
  color: #fff;
  border: none;
}

.btn-search:hover:not(:disabled) {
  background: #009959;
}

.btn-search:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-search svg,
.btn-reset svg {
  width: 18px;
  height: 18px;
}

.btn-reset {
  background: #fff;
  color: #3f3f46;
  border: 1px solid #e4e4e7;
}

.btn-reset:hover:not(:disabled) {
  background: #fafafa;
  border-color: #d4d4d8;
}

/* Main Content */
.main-content {
  order: 1;
}

@media (min-width: 1024px) {
  .main-content {
    order: 2;
  }
}

/* Stats Row */
.stats-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  margin-bottom: 24px;
}

/* Loading State */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px;
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.loading-spinner {
  width: 48px;
  height: 48px;
  border: 3px solid #e4e4e7;
  border-top-color: #006d4b;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.loading-state p {
  margin-top: 16px;
  color: #71717a;
  font-size: 14px;
}

/* Committees List */
.committees-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.committee-card {
  display: flex;
  align-items: center;
  gap: 14px;
  background: #fff;
  border-radius: 10px;
  padding: 14px 16px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.04);
  border: 1px solid #e4e4e7;
  transition: all 0.2s;
}

.committee-card:hover {
  border-color: #d4d4d8;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

/* Card Icon */
.card-icon {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 10px;
  flex-shrink: 0;
}

.card-icon svg {
  width: 20px;
  height: 20px;
}

.card-icon.council {
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
}

.card-icon.committee {
  background: rgba(59, 130, 246, 0.1);
  color: #3b82f6;
}

/* Card Info */
.card-info {
  flex: 1;
  min-width: 0;
}

.card-title {
  font-size: 14px;
  font-weight: 600;
  color: #27272a;
  margin: 0 0 4px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.card-meta {
  display: flex;
  align-items: center;
  gap: 8px;
}

.meta-type {
  font-size: 11px;
  font-weight: 500;
  padding: 2px 8px;
  border-radius: 4px;
}

.meta-type.council {
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
}

.meta-type.committee {
  background: rgba(59, 130, 246, 0.1);
  color: #3b82f6;
}

.meta-code {
  font-size: 12px;
  color: #a1a1aa;
}

/* Card Actions */
.card-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
}

.btn-sub {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  font-size: 12px;
  font-weight: 500;
  background: #1e40af;
  color: #fff;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-sub:hover {
  background: #1e3a8a;
}

.btn-sub .count {
  background: rgba(255, 255, 255, 0.2);
  padding: 1px 6px;
  border-radius: 4px;
  font-size: 11px;
}

.btn-view {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #006d4b;
  color: #fff;
  border-radius: 6px;
  transition: all 0.2s;
}

.btn-view:hover {
  background: #009959;
}

.btn-view svg {
  width: 16px;
  height: 16px;
}

[dir="rtl"] .btn-view svg {
  transform: scaleX(-1);
}

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 40px;
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  text-align: center;
}

.empty-icon {
  width: 80px;
  height: 80px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f4f4f5;
  border-radius: 20px;
  margin-bottom: 24px;
}

.empty-icon svg {
  width: 40px;
  height: 40px;
  color: #a1a1aa;
}

.empty-state h3 {
  font-size: 18px;
  font-weight: 600;
  color: #27272a;
  margin: 0 0 8px;
}

.empty-state p {
  font-size: 14px;
  color: #71717a;
  margin: 0 0 20px;
}

.btn-clear-search {
  padding: 10px 20px;
  font-size: 14px;
  font-weight: 500;
  background: #006d4b;
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-clear-search:hover {
  background: #009959;
}
</style>

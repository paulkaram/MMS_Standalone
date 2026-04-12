<template>
  <div class="cd-page">
    <!-- Loading -->
    <div v-if="loading" class="cd-loading">
      <div class="cd-spinner"></div>
    </div>

    <template v-else-if="hasAnyPermission">
      <!-- Page Header -->
      <PageHeader :title="committeeName" :subtitle="$t('CommitteeOverview360')">
        <template #actions>
          <button class="cd-back-btn" @click="goBack">
            <Icon icon="mdi:arrow-left" class="w-4 h-4" />
            {{ $t('Back') }}
          </button>
        </template>
      </PageHeader>

      <!-- Stats Row -->
      <div class="cd-stats">
        <StatCard v-if="permissions.users" :title="$t('Members')" :value="stats.members" icon="mdi:account-group" color="primary" />
        <StatCard v-if="permissions.meetings" :title="$t('Meetings')" :value="stats.meetings" icon="mdi:calendar" color="info" />
        <StatCard v-if="permissions.recommendations" :title="$t('Recommendations')" :value="stats.recommendations" icon="mdi:lightbulb" color="warning" />
        <StatCard v-if="permissions.committeeActivities" :title="$t('Activities')" :value="stats.activities" icon="mdi:chart-timeline" color="success" />
        <StatCard v-if="permissions.committeeAttachments" :title="$t('Attachments')" :value="stats.attachments" icon="mdi:attachment" color="secondary" />
      </div>

      <!-- Dashboard Grid -->
      <div class="cd-grid">
        <!-- Members -->
        <section v-if="permissions.users" class="cd-card cd-members">
          <div class="cd-card-header">
            <div class="cd-card-icon"><Icon icon="mdi:account-group" class="w-4 h-4" /></div>
            <h3>{{ $t('Members') }}</h3>
            <span class="cd-card-badge">{{ stats.members }}</span>
          </div>
          <div class="cd-card-body">
            <CommitteeMembers :committee-id="committeeId" @update:count="stats.members = $event" />
          </div>
        </section>

        <!-- Meetings -->
        <section v-if="permissions.meetings" class="cd-card cd-meetings">
          <div class="cd-card-header">
            <div class="cd-card-icon meetings"><Icon icon="mdi:calendar" class="w-4 h-4" /></div>
            <h3>{{ $t('Meetings') }}</h3>
            <span class="cd-card-badge">{{ stats.meetings }}</span>
          </div>
          <div class="cd-card-body">
            <CommitteeMeetings :committee-id="committeeId" :user-permissions="permissions" @update:count="stats.meetings = $event" />
          </div>
        </section>

        <!-- Recommendations -->
        <section v-if="permissions.recommendations" class="cd-card cd-recommendations">
          <div class="cd-card-header">
            <div class="cd-card-icon recommendations"><Icon icon="mdi:lightbulb" class="w-4 h-4" /></div>
            <h3>{{ $t('Recommendations') }}</h3>
            <span class="cd-card-badge">{{ stats.recommendations }}</span>
          </div>
          <div class="cd-card-body">
            <CommitteeRecommendations :committee-id="committeeId" @update:count="stats.recommendations = $event" @update:progress="stats.recommendationsProgress = $event" />
          </div>
        </section>

        <!-- Activities -->
        <section v-if="permissions.committeeActivities" class="cd-card cd-activities">
          <div class="cd-card-header">
            <div class="cd-card-icon activities"><Icon icon="mdi:chart-timeline" class="w-4 h-4" /></div>
            <h3>{{ $t('Activities') }}</h3>
            <span class="cd-card-badge">{{ stats.activities }}</span>
          </div>
          <div class="cd-card-body">
            <CommitteeActivities :committee-id="committeeId" @update:count="stats.activities = $event" />
          </div>
        </section>

        <!-- Attachments -->
        <section v-if="permissions.committeeAttachments" class="cd-card cd-attachments">
          <div class="cd-card-header">
            <div class="cd-card-icon attachments"><Icon icon="mdi:attachment" class="w-4 h-4" /></div>
            <h3>{{ $t('Attachments') }}</h3>
            <span class="cd-card-badge">{{ stats.attachments }}</span>
          </div>
          <div class="cd-card-body">
            <CommitteeAttachments :committee-id="committeeId" :permission-add="permissions.committeeAttachmentButtonAdd" @update:count="stats.attachments = $event" />
          </div>
        </section>
      </div>
    </template>

    <!-- No Permissions -->
    <div v-else class="cd-no-access">
      <div class="cd-no-access-icon">
        <Icon icon="mdi:lock" class="w-10 h-10" />
      </div>
      <h2>{{ $t('NoPermissions') }}</h2>
      <p>{{ $t('NoPermissionsDescription') }}</p>
      <button class="cd-no-access-btn" @click="goBack">{{ $t('GoBack') }}</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageHeader from '@/components/layout/PageHeader.vue'
import Icon from '@/components/ui/Icon.vue'
import StatCard from '@/components/app/dashboard/StatCard.vue'
import CommitteeMembers from '@/components/app/generalInfo/CommitteeMembers.vue'
import CommitteeMeetings from '@/components/app/generalInfo/CommitteeMeetings.vue'
import CommitteeRecommendations from '@/components/app/generalInfo/CommitteeRecommendations.vue'
import CommitteeActivities from '@/components/app/generalInfo/CommitteeActivities.vue'
import CommitteeAttachments from '@/components/app/generalInfo/CommitteeAttachments.vue'
import CouncilCommitteesService from '@/services/CouncilCommitteesService'

const route = useRoute()
const router = useRouter()

// State
const loading = ref(true)
const committeeName = ref('')
const committeeType = ref(2)
const permissions = ref({
  users: false,
  meetings: false,
  recommendations: false,
  committeeActivities: false,
  committeeAttachments: false,
  committeeAttachmentButtonAdd: false
})
const stats = ref({
  members: 0,
  meetings: 0,
  recommendations: 0,
  recommendationsProgress: 0,
  activities: 0,
  attachments: 0
})

// Computed
const committeeId = computed(() => route.params.committeeId as string)

const hasAnyPermission = computed(() => {
  return (
    permissions.value.users ||
    permissions.value.meetings ||
    permissions.value.recommendations ||
    permissions.value.committeeActivities ||
    permissions.value.committeeAttachments
  )
})

// Methods
const goBack = () => {
  router.back()
}

const loadData = async () => {
  if (!committeeId.value) return

  loading.value = true
  try {
    const [parentsData, permissionsData] = await Promise.all([
      CouncilCommitteesService.getCommitteesParents(committeeId.value),
      CouncilCommitteesService.getMyCommitteesPermissions(committeeId.value)
    ])

    const parentsArr = parentsData.data || parentsData || []

    if (parentsArr.length > 0) {
      const current = parentsArr[parentsArr.length - 1]
      committeeName.value = current.name || current.nameAr || ''
      committeeType.value = current.typeId || 2
    }

    permissions.value = permissionsData.data || permissionsData

  } catch (error) {
    console.error('Failed to load committee data:', error)
  } finally {
    loading.value = false
  }
}

// Lifecycle
onMounted(() => {
  loadData()
})
</script>

<style scoped>
.cd-page { display: flex; flex-direction: column; gap: 20px; }

/* Loading */
.cd-loading { display: flex; justify-content: center; padding: 80px 0; }
.cd-spinner {
  width: 36px; height: 36px;
  border: 3px solid #e2e8f0; border-top-color: #006d4b;
  border-radius: 50%; animation: cd-spin 0.7s linear infinite;
}
@keyframes cd-spin { to { transform: rotate(360deg); } }

/* Back Button */
.cd-back-btn {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 8px 16px; border-radius: 8px;
  background: #fff; border: 1px solid #e2e8f0;
  color: #475569; font-size: 13px; font-weight: 500;
  cursor: pointer; transition: all 0.15s; font-family: inherit;
}
.cd-back-btn:hover { background: #f8fafc; border-color: #cbd5e1; }

/* Stats Row */
.cd-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 16px;
}

/* Dashboard Grid */
.cd-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 20px;
}

@media (min-width: 1024px) {
  .cd-grid { grid-template-columns: repeat(2, 1fr); }
  .cd-recommendations { grid-column: span 2; }
}

/* Card */
.cd-card {
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}

/* Card Header — dark strip */
.cd-card-header {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 14px 20px;
  background: #006d4b;
  color: #fff;
}

.cd-card-icon {
  width: 30px; height: 30px;
  display: flex; align-items: center; justify-content: center;
  border-radius: 8px;
  background: rgba(0, 109, 75, 0.15);
  color: #006d4b;
  flex-shrink: 0;
}

.cd-card-icon.meetings { background: rgba(59, 130, 246, 0.15); color: #60a5fa; }
.cd-card-icon.recommendations { background: rgba(251, 191, 36, 0.15); color: #fbbf24; }
.cd-card-icon.activities { background: rgba(34, 197, 94, 0.15); color: #4ade80; }
.cd-card-icon.attachments { background: rgba(148, 163, 184, 0.15); color: #94a3b8; }

.cd-card-header h3 {
  flex: 1;
  font-size: 14px;
  font-weight: 600;
  margin: 0;
  color: #f1f5f9;
}

.cd-card-badge {
  padding: 2px 10px;
  background: rgba(0, 109, 75, 0.15);
  color: #006d4b;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 700;
}

/* Card Body */
.cd-card-body {
  padding: 16px;
  max-height: 420px;
  overflow-y: auto;
}

.cd-card-body::-webkit-scrollbar { width: 5px; }
.cd-card-body::-webkit-scrollbar-track { background: transparent; }
.cd-card-body::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 3px; }

/* No Access */
.cd-no-access {
  display: flex; flex-direction: column;
  align-items: center; justify-content: center;
  padding: 80px 20px; text-align: center;
}

.cd-no-access-icon {
  width: 72px; height: 72px;
  display: flex; align-items: center; justify-content: center;
  background: rgba(0, 109, 75, 0.1); color: #006d4b;
  border-radius: 16px; margin-bottom: 20px;
}

.cd-no-access h2 {
  font-size: 18px; font-weight: 700;
  color: #0f172a; margin: 0 0 6px;
}

.cd-no-access p {
  font-size: 14px; color: #94a3b8;
  margin: 0 0 20px;
}

.cd-no-access-btn {
  padding: 10px 24px; border-radius: 8px;
  background: rgba(0, 109, 75, 0.1); color: #006d4b;
  border: 1px solid rgba(0, 109, 75, 0.3);
  font-size: 14px; font-weight: 600;
  cursor: pointer; transition: all 0.15s; font-family: inherit;
}

.cd-no-access-btn:hover {
  border-color: #006d4b;
  box-shadow: 0 2px 8px rgba(0, 109, 75, 0.2);
}
</style>

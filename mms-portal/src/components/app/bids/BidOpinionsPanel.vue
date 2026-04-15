<template>
  <section v-if="opinions.length > 0" class="bop">
    <!-- Header + overall tally -->
    <header class="bop-head">
      <h3>
        <Icon icon="mdi:thumbs-up-down" class="w-4 h-4" />
        {{ $t('MinutesOpinions') || 'Minutes Opinions' }}
        <span class="bop-count">{{ summary.submittedOpinions }} / {{ summary.totalOpinions }}</span>
      </h3>
      <div class="bop-tallies">
        <span class="bop-tally bop-tally-suitable">
          <Icon icon="mdi:thumb-up-outline" class="w-3 h-3" />
          {{ summary.suitableCount }} {{ $t('Suitable') || 'Suitable' }}
        </span>
        <span class="bop-tally bop-tally-unsuitable">
          <Icon icon="mdi:thumb-down-outline" class="w-3 h-3" />
          {{ summary.unsuitableCount }} {{ $t('Unsuitable') || 'Unsuitable' }}
        </span>
      </div>
    </header>

    <!-- Progress bar -->
    <div class="bop-progress-wrap">
      <div class="bop-progress-track">
        <div
          class="bop-progress-fill"
          :style="{ width: overallPct + '%' }"
          :class="{ complete: summary.allSubmitted }"
        ></div>
      </div>
      <span class="bop-progress-label">{{ overallPct }}%</span>
    </div>

    <!-- Per-stakeholder rows -->
    <div class="bop-list">
      <div
        v-for="o in opinions"
        :key="o.id"
        class="bop-row"
        :class="{
          submitted: o.statusId === 2,
          suitable: o.opinion === MinutesOpinion.Suitable,
          unsuitable: o.opinion === MinutesOpinion.Unsuitable
        }"
      >
        <UserAvatar :userId="o.stakeholderUserId || ''" :name="o.stakeholderName" size="sm" />
        <div class="bop-info">
          <div class="bop-name">
            {{ o.stakeholderName }}
            <span v-if="o.isExternal" class="bop-ext">{{ $t('ExternalMember') }}</span>
          </div>
          <div v-if="o.statusId === 2" class="bop-verdict">
            <Icon
              :icon="o.opinion === MinutesOpinion.Suitable ? 'mdi:thumb-up' : 'mdi:thumb-down'"
              class="w-3 h-3"
            />
            {{ o.opinionName }}
            <span v-if="o.comment" class="bop-comment">— {{ o.comment }}</span>
          </div>
          <div v-else class="bop-pending">{{ $t('AwaitingOpinionShort') || 'Awaiting opinion' }}</div>
        </div>
        <Icon v-if="o.statusId === 2" icon="mdi:check-circle" class="w-5 h-5 bop-check" />
      </div>
    </div>

    <!-- Submit form for the current user's opinion -->
    <button
      v-if="myOpinion && myOpinion.statusId !== 2"
      class="btn-primary full-width"
      @click="submitModalOpen = true"
    >
      <Icon icon="mdi:thumbs-up-down" class="w-4 h-4" />
      {{ $t('SubmitMyOpinion') || 'Submit My Opinion' }}
    </button>

    <Modal v-model="submitModalOpen" :title="$t('SubmitMyOpinion') || 'Submit My Opinion'" size="md">
      <form class="bop-form" @submit.prevent="submit">
        <div class="form-field">
          <label>{{ $t('Verdict') || 'Verdict' }} *</label>
          <div class="bop-verdict-toggle">
            <button
              type="button"
              class="verdict-btn suitable"
              :class="{ active: draft.opinion === MinutesOpinion.Suitable }"
              @click="draft.opinion = MinutesOpinion.Suitable"
            >
              <Icon icon="mdi:thumb-up" class="w-4 h-4" />
              {{ $t('Suitable') || 'Suitable' }}
            </button>
            <button
              type="button"
              class="verdict-btn unsuitable"
              :class="{ active: draft.opinion === MinutesOpinion.Unsuitable }"
              @click="draft.opinion = MinutesOpinion.Unsuitable"
            >
              <Icon icon="mdi:thumb-down" class="w-4 h-4" />
              {{ $t('Unsuitable') || 'Unsuitable' }}
            </button>
          </div>
        </div>

        <div class="form-field">
          <label>{{ $t('CommentOptional') || 'Comment (optional)' }}</label>
          <textarea
            v-model="draft.comment"
            rows="4"
            class="form-input"
            :placeholder="$t('OpinionCommentPlaceholder') || 'Reasoning for your verdict'"
          ></textarea>
        </div>

        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="submitModalOpen = false">{{ $t('Cancel') }}</button>
          <button type="submit" class="btn-primary" :disabled="saving || draft.opinion === 0">
            {{ $t('Submit') }}
          </button>
        </div>
      </form>
    </Modal>
  </section>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import UserAvatar from '@/components/ui/UserAvatar.vue'
import OpinionsService, {
  MinutesOpinion,
  type BidMinutesOpinion,
  type BidMinutesOpinionsSummary
} from '@/services/OpinionsService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'
import { useUserStore } from '@/stores/user'

interface Props { bidId: number }
const props = defineProps<Props>()
// Tells the parent whether there are any opinions to render (so the parent
// can hide its wrapping card and avoid an empty-card visual artifact).
const emit = defineEmits<{ 'has-data': [hasData: boolean] }>()

const { t } = useI18n()
const { toast } = useToast()
const userStore = useUserStore()

const opinions = ref<BidMinutesOpinion[]>([])
const summary = ref<BidMinutesOpinionsSummary>({
  bidId: 0, totalOpinions: 0, submittedOpinions: 0,
  suitableCount: 0, unsuitableCount: 0, allSubmitted: false
})
const submitModalOpen = ref(false)
const saving = ref(false)
const draft = ref<{ opinion: number; comment: string }>({ opinion: 0, comment: '' })

const overallPct = computed(() => {
  if (!summary.value || summary.value.totalOpinions === 0) return 0
  return Math.round((summary.value.submittedOpinions / summary.value.totalOpinions) * 100)
})

const myOpinion = computed(() =>
  opinions.value.find(o => o.stakeholderUserId === userStore.loggedInUser?.id) || null
)

async function load() {
  try {
    const [list, summ]: any = await Promise.all([
      OpinionsService.listByBid(props.bidId).catch(() => []),
      OpinionsService.getSummary(props.bidId).catch(() => null)
    ])
    opinions.value = list || []
    if (summ) summary.value = summ
    if (myOpinion.value) {
      draft.value = {
        opinion: myOpinion.value.opinion || 0,
        comment: myOpinion.value.comment || ''
      }
    }
  } catch {
    opinions.value = []
  } finally {
    emit('has-data', opinions.value.length > 0)
  }
}

async function submit() {
  if (!myOpinion.value || draft.value.opinion === 0) return
  saving.value = true
  try {
    await OpinionsService.submit(myOpinion.value.id, draft.value.opinion, draft.value.comment)
    toast.success(t('OpinionSubmitted') || 'Opinion submitted')
    submitModalOpen.value = false
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message || t('ErrorOccured'))
  } finally {
    saving.value = false
  }
}

watch(() => props.bidId, () => { if (props.bidId) load() })
onMounted(load)
</script>

<style scoped>
.bop { display: flex; flex-direction: column; gap: 12px; }
.bop-head { display: flex; justify-content: space-between; align-items: flex-start; gap: 12px; flex-wrap: wrap; }
.bop-head h3 {
  margin: 0; display: flex; align-items: center; gap: 6px;
  font-size: 14px; font-weight: 700; color: #1a2e25;
}
.bop-count {
  display: inline-flex; align-items: center; justify-content: center;
  min-width: 22px; height: 20px; padding: 0 7px;
  background: rgba(0, 109, 75, 0.1); color: #006d4b;
  border-radius: 10px; font-size: 11px; font-weight: 700;
}

.bop-tallies { display: flex; gap: 8px; }
.bop-tally {
  display: inline-flex; align-items: center; gap: 4px;
  padding: 3px 10px; border-radius: 10px;
  font-size: 11px; font-weight: 700;
}
.bop-tally-suitable   { background: #dcfce7; color: #15803d; border: 1px solid #86efac; }
.bop-tally-unsuitable { background: #fee2e2; color: #b91c1c; border: 1px solid #fca5a5; }

/* Progress */
.bop-progress-wrap { display: flex; align-items: center; gap: 10px; }
.bop-progress-track {
  flex: 1; height: 8px; background: #e4ede8;
  border-radius: 6px; overflow: hidden;
}
.bop-progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #63a58f, #006d4b);
  transition: width 0.3s ease;
}
.bop-progress-fill.complete { background: linear-gradient(90deg, #22c55e, #15803d); }
.bop-progress-label {
  font-size: 11px; font-weight: 700; color: #006d4b;
  min-width: 36px; text-align: end;
}

/* Rows */
.bop-list { display: flex; flex-direction: column; gap: 8px; }
.bop-row {
  display: flex; align-items: center; gap: 10px;
  padding: 10px 12px; background: #f7faf8;
  border: 1px solid #e4ede8; border-radius: 10px;
  transition: all 0.15s;
}
.bop-row.suitable   { background: #f0fdf4; border-color: #bbf7d0; }
.bop-row.unsuitable { background: #fef2f2; border-color: #fecaca; }

.bop-info { flex: 1; min-width: 0; }
.bop-name {
  font-size: 12.5px; font-weight: 600; color: #1a2e25;
  display: flex; align-items: center; gap: 6px;
}
.bop-ext {
  font-size: 10px; font-weight: 700; padding: 1px 6px;
  background: #eef2ff; color: #3b4cca; border: 1px solid #c5cbf5;
  border-radius: 6px; text-transform: uppercase;
}
.bop-verdict {
  display: flex; align-items: center; gap: 5px; margin-top: 3px;
  font-size: 12px; color: #1a2e25;
}
.bop-row.suitable   .bop-verdict { color: #15803d; font-weight: 600; }
.bop-row.unsuitable .bop-verdict { color: #b91c1c; font-weight: 600; }
.bop-comment { color: #6b8a7d; font-weight: 400; font-style: italic; }
.bop-pending {
  font-size: 11.5px; color: #92400e; margin-top: 3px;
  font-weight: 500;
}
.bop-check { color: #22c55e; flex-shrink: 0; }

.btn-primary, .btn-secondary {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 9px 16px; border-radius: 8px; font-size: 13px; font-weight: 600;
  cursor: pointer; font-family: inherit; transition: all 0.15s; border: 1px solid transparent;
}
.btn-primary { background: #006d4b; color: #fff; }
.btn-primary:hover:not(:disabled) { background: #005339; }
.btn-primary:disabled { opacity: 0.55; cursor: not-allowed; }
.btn-primary.full-width { width: 100%; justify-content: center; }
.btn-secondary { background: #fff; color: #374a41; border-color: #d4e0da; }
.btn-secondary:hover { background: #f4f8f6; }

/* Modal form */
.bop-form { display: flex; flex-direction: column; gap: 12px; }
.form-field { display: flex; flex-direction: column; gap: 4px; }
.form-field label { font-size: 12px; font-weight: 600; color: #374a41; }
.form-input {
  padding: 10px 12px; border: 1.5px solid #d4e0da; border-radius: 8px;
  font-size: 14px; background: #f7faf8; color: #1a2e25; font-family: inherit;
  resize: vertical;
}
.form-input:focus { outline: none; border-color: #006d4b; background: #fff; box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1); }

.bop-verdict-toggle { display: flex; gap: 8px; }
.verdict-btn {
  flex: 1; display: inline-flex; align-items: center; justify-content: center; gap: 6px;
  padding: 14px; border: 2px solid #d4e0da; border-radius: 10px;
  font-size: 14px; font-weight: 700; color: #475569;
  background: #f7faf8; cursor: pointer; font-family: inherit; transition: all 0.2s;
}
.verdict-btn:hover { transform: translateY(-1px); }
.verdict-btn.suitable.active {
  background: #dcfce7; border-color: #15803d; color: #15803d;
  box-shadow: 0 4px 12px rgba(21, 128, 61, 0.15);
}
.verdict-btn.unsuitable.active {
  background: #fee2e2; border-color: #b91c1c; color: #b91c1c;
  box-shadow: 0 4px 12px rgba(185, 28, 28, 0.15);
}

.form-actions { display: flex; justify-content: flex-end; gap: 8px; padding-top: 8px; border-top: 1px solid #e4ede8; margin-top: 6px; }
</style>

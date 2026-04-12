<template>
  <section class="panel" :class="{ collapsed }">
    <!-- Collapsed State -->
    <div v-if="collapsed" class="collapsedPanel">
      <button class="expandBtn" @click="$emit('toggle-collapse')" :title="$t('ExpandPanel')">
        <Icon :icon="isRTL ? 'chevron_left' : 'chevron_right'" style="font-size: 18px;" />
      </button>
      <div class="collapsedIcons">
        <div class="collapsedIcon" :title="$t('Attachments')">
          <Icon icon="attach_file" style="font-size: 16px;" />
          <span class="badge">{{ attachments.length }}</span>
        </div>
        <div class="collapsedIcon" :title="$t('Agenda')">
          <Icon icon="checklist" style="font-size: 16px;" />
          <span class="badge">{{ agendas.length }}</span>
        </div>
      </div>
    </div>

    <!-- Expanded State -->
    <template v-else>
    <div class="panelHeader">
      <div class="titleRow">
        <div class="badge">
          <Icon icon="dashboard" style="font-size: 18px;" />
        </div>
        <div>
          <h3>{{ $t('MeetingControl') }}</h3>
          <div class="sub">{{ $t('AttachmentsAndAgenda') }}</div>
        </div>
      </div>
      <button class="collapseBtn" @click="$emit('toggle-collapse')" :title="$t('CollapsePanel')">
        <Icon :icon="isRTL ? 'chevron_right' : 'chevron_left'" style="font-size: 16px;" />
      </button>
    </div>

    <div class="panelBody">
      <!-- Attachments Card -->
      <div class="card" style="flex: 0 0 44%;">
        <div class="cardHeader">
          <div class="label"><Icon icon="attach_file" style="font-size: 14px;" /> {{ $t('Attachments') }}</div>
          <div class="sub">{{ attachments.length }} {{ $t('Files') }}</div>
        </div>
        <div class="cardBody">
          <div class="list">
            <div
              v-for="att in attachments"
              :key="att.id"
              class="item"
              :class="{
                active: att.isMom
                  ? (att.momType === 'initial' && activeViewType === 'initialMom') || (att.momType === 'final' && activeViewType === 'finalMom')
                  : currentAttachment?.id === att.id && activeViewType === 'attachment',
                'mom-item': att.isMom,
                'initial-mom': att.isMom && att.momType === 'initial',
                'final-mom': att.isMom && att.momType === 'final'
              }"
              @click="$emit('select-attachment', att)"
            >
              <div class="itemLeft">
                <div class="fileIcon" :class="att.isMom ? (att.momType === 'final' ? 'final-mom-icon' : 'initial-mom-icon') : getFileTypeClass(att.fileName || att.name)">
                  <Icon :icon="att.isMom ? (att.momType === 'final' ? 'task_alt' : 'article') : getFileIconName(att.fileName || att.name)" style="font-size: 16px;" />
                </div>
                <div class="itemText">
                  <div class="name">
                    <span v-if="att.isMom" class="mom-badge" :class="att.momType">
                      {{ att.momType === 'final' ? $t('FinalMOM') : $t('InitialMOM') }}
                    </span>
                    {{ att.isMom ? $t('MinutesOfMeeting') : (att.fileName || att.name) }}
                  </div>
                  <div class="meta">{{ att.isMom ? (att.momType === 'final' ? $t('ApprovedFinalMOM') : $t('InitialMOM')) : (getFileType(att.fileName || att.name) + ' • ' + (att.fileSize || '—')) }}</div>
                </div>
              </div>
              <Icon :icon="isRTL ? 'chevron_left' : 'chevron_right'" class="chevron" style="font-size: 14px;" />
            </div>
            <div v-if="!attachments.length" class="emptyState">
              <Icon icon="attach_file" style="font-size: 24px;" />
              <span>{{ $t('NoAttachmentsAvailable') }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Agenda Card - Redesigned -->
      <div class="card agendaCard" style="flex: 1 1 auto;">
        <div class="cardHeader">
          <div class="label"><Icon icon="checklist" style="font-size: 16px;" /> {{ $t('Agenda') }}</div>
          <div class="sub">{{ agendas.length }} {{ $t('AgendaItem') }}</div>
        </div>
        <div class="cardBody">
          <div class="agendaList">
            <div
              v-for="(agenda, index) in agendas"
              :key="agenda.id"
              class="agendaItem"
              :class="{
                active: currentAgendaIndex === index,
                completed: isAgendaCompleted(agenda, index),
                running: agenda.isRunning && !agenda.paused
              }"
              @click="$emit('select-agenda', index)"
            >
              <!-- Running indicator bar -->
              <div v-if="agenda.isRunning && !agenda.paused && !isAgendaCompleted(agenda, index)" class="agendaRunBar"></div>

              <div class="agendaRow">
                <span class="agendaNumber">{{ index + 1 }}</span>
                <div class="agendaBody">
                  <h4 class="agendaTitle">{{ agenda.title || agenda.titleAr }}</h4>
                  <span v-if="agenda.duration" class="agendaDuration">
                    <Icon icon="schedule" style="font-size: 10px;" /> {{ agenda.duration }} {{ $t('Minutes') }}
                  </span>
                </div>
                <Icon v-if="isAgendaCompleted(agenda, index)" icon="check_circle" class="completedIcon" style="font-size: 16px;" />
              </div>

              <!-- Controls row: only during live meeting -->
              <div v-if="isLive && (currentAgendaIndex === index || agenda.isRunning) && !isAgendaCompleted(agenda, index)" class="agendaControls">
                <span class="timerText mono" :class="{ active: agenda.isRunning }">
                  <Icon icon="timer" style="font-size: 11px;" />
                  {{ formatAgendaTime(agenda) }}
                </span>
                <div v-if="canControl" class="agendaActions">
                  <button
                    class="agendaActBtn"
                    :class="agenda.isRunning && !agenda.paused ? 'pause' : 'play'"
                    @click.stop="$emit('toggle-agenda-timer', agenda, index)"
                    :title="agenda.isRunning && !agenda.paused ? $t('PauseTimer') : $t('PlayTimer')"
                  >
                    <Icon v-if="agenda.isRunning && !agenda.paused" icon="pause" style="font-size: 11px;" />
                    <Icon v-else icon="play_arrow" style="font-size: 11px;" />
                  </button>
                  <button
                    class="agendaActBtn complete"
                    @click.stop="$emit('complete-agenda', agenda, index)"
                    :title="$t('EndTopic')"
                  >
                    <Icon icon="check" style="font-size: 11px;" />
                    {{ $t('EndTopic') }}
                  </button>
                </div>
              </div>
            </div>
            <div v-if="!agendas.length" class="emptyState">
              <div class="emptyIcon">
                <Icon icon="checklist" style="font-size: 32px;" />
              </div>
              <span>{{ $t('NoAgendaItems') }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
    </template>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import { getLocale } from '@/plugins/i18n'

const isRTL = computed(() => getLocale() === 'ar')

const props = defineProps<{
  attachments: any[]
  agendas: any[]
  currentAttachment: any
  currentAgendaIndex: number
  canControl: boolean
  isLive: boolean
  collapsed: boolean
  activeViewType?: 'attachment' | 'initialMom' | 'finalMom'
}>()

defineEmits<{
  (e: 'select-attachment', att: any): void
  (e: 'select-agenda', index: number): void
  (e: 'toggle-agenda-timer', agenda: any, index: number): void
  (e: 'complete-agenda', agenda: any, index: number): void
  (e: 'toggle-collapse'): void
}>()

// Helper functions
const getFileType = (filename: string) => {
  if (!filename) return '—'
  const ext = filename.split('.').pop()?.toLowerCase()
  const types: Record<string, string> = {
    pdf: 'PDF', doc: 'DOC', docx: 'DOC', xls: 'XLS', xlsx: 'XLS',
    ppt: 'PPT', pptx: 'PPT', png: 'IMG', jpg: 'IMG', jpeg: 'IMG', gif: 'IMG'
  }
  return types[ext || ''] || 'FILE'
}

const getFileIconName = (filename: string): string => {
  const type = getFileType(filename)
  const icons: Record<string, string> = {
    PDF: 'picture_as_pdf', DOC: 'description', XLS: 'table',
    PPT: 'slideshow', IMG: 'image', FILE: 'draft'
  }
  return icons[type] || 'draft'
}

const getFileTypeClass = (filename: string): string => {
  const type = getFileType(filename)
  const classes: Record<string, string> = {
    PDF: 'pdf', DOC: 'doc', XLS: 'xls',
    PPT: 'ppt', IMG: 'img', FILE: 'file'
  }
  return classes[type] || 'file'
}

const formatSeconds = (seconds: number) => {
  const m = Math.floor(Math.abs(seconds) / 60)
  const s = Math.abs(seconds) % 60
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
}

const formatAgendaTime = (agenda: any) => {
  if (agenda.elapsedSeconds !== undefined) {
    return formatSeconds(agenda.elapsedSeconds)
  }
  if (agenda.remainingSeconds !== undefined) {
    return formatSeconds(agenda.remainingSeconds)
  }
  return '00:00'
}

const isAgendaCompleted = (agenda: any, _index: number) => {
  return !!agenda.actualEndDate || agenda.status === 'completed'
}

const getAgendaStatus = (agenda: any, index: number) => {
  if (isAgendaCompleted(agenda, index)) return 'completed'
  if (agenda.isRunning && agenda.paused) return 'paused'
  if (agenda.isRunning) return 'active'
  if (props.currentAgendaIndex === index) return 'active'
  return 'upcoming'
}

const getTimerButtonClass = (agenda: any) => {
  if (agenda.isRunning && !agenda.paused) {
    // Running - show pause button in red
    return 'danger'
  } else if (agenda.isRunning && agenda.paused) {
    // Paused - show play button in orange/warning
    return 'warning'
  }
  // Not started - show play button in green
  return 'success'
}

const getStatusClass = (agenda: any, index: number) => {
  if (isAgendaCompleted(agenda, index)) return 'completed'
  if (agenda.isRunning && agenda.paused) return 'paused'
  if (agenda.isRunning) return 'running'
  if (props.currentAgendaIndex === index) return 'current'
  return 'pending'
}
</script>

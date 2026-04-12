<template>
  <section class="panel centerPanel">
    <!-- Unified Header -->
    <div class="panelHeader compact">
      <!-- Right side in RTL: Meeting info -->
      <div class="headerRight">
        <div class="meetingMeta">
          <div class="badge">
            <Icon icon="group" class="w-4 h-4" />
          </div>
          <div class="meetingName" :title="meeting.title || meeting.titleAr">
            <strong>{{ meeting.title || meeting.titleAr }}</strong>
            <span class="sub">{{ meeting.committeeName || meeting.councilName }}</span>
          </div>
        </div>

        <!-- Meeting Details Button with Dropdown -->
        <div class="detailsDropdown" v-if="hasAnyDetails">
          <button class="detailsBtn" @click="showDetailsDropdown = !showDetailsDropdown">
            <Icon icon="info" class="w-3.5 h-3.5" />
            <span>{{ $t('Details') }}</span>
            <Icon icon="expand_more" class="w-3 h-3" :class="{ rotated: showDetailsDropdown }" />
          </button>
          <Transition name="dropdown">
            <div v-if="showDetailsDropdown" class="dropdownContent detailsCard" @click.stop>
              <div v-if="meeting.referenceNumber || meeting.refNumber || meeting.meetingNumber" class="detailRow">
                <div class="detailIcon ref"><Icon icon="tag" class="w-3.5 h-3.5" /></div>
                <div class="detailText">
                  <span class="detailLabel">{{ $t('ReferenceNumberLabel') }}</span>
                  <span class="detailValue">{{ meeting.referenceNumber || meeting.refNumber || meeting.meetingNumber }}</span>
                </div>
              </div>
              <div v-if="meeting.location" class="detailRow">
                <div class="detailIcon loc"><Icon icon="location_on" class="w-3.5 h-3.5" /></div>
                <div class="detailText">
                  <span class="detailLabel">{{ $t('LocationLabel') }}</span>
                  <span class="detailValue">{{ meeting.location }}</span>
                </div>
              </div>
              <div v-if="meeting.startTime || meeting.fromTime || meeting.start" class="detailRow">
                <div class="detailIcon time"><Icon icon="schedule" class="w-3.5 h-3.5" /></div>
                <div class="detailText">
                  <span class="detailLabel">{{ $t('StartTimeLabel') }}</span>
                  <span class="detailValue">{{ formatTime(meeting.startTime || meeting.fromTime || meeting.start) }}</span>
                </div>
              </div>
              <div v-if="meeting.endTime || meeting.toTime || meeting.end" class="detailRow">
                <div class="detailIcon time"><Icon icon="timer" class="w-3.5 h-3.5" /></div>
                <div class="detailText">
                  <span class="detailLabel">{{ $t('EndTimeLabel') }}</span>
                  <span class="detailValue">{{ formatTime(meeting.endTime || meeting.toTime || meeting.end) }}</span>
                </div>
              </div>
            </div>
          </Transition>
        </div>

        <!-- Elapsed Timer -->
        <div class="pill timer">
          <Icon icon="schedule" class="w-3 h-3" />
          <span class="mono">{{ meetingElapsedTime }}</span>
        </div>
      </div>

      <!-- Actions -->
      <div class="headerActions">
        <button type="button" class="headerBtn commentsBtn" :disabled="!isMeetingStartedOrFinished" @click="showCommentsModal = true" :title="$t('Comments')">
          <Icon icon="chat_bubble_outline" class="w-3.5 h-3.5" /> {{ $t('Comments') }}
          <span v-if="totalCommentsCount" class="btnBadge">{{ totalCommentsCount }}</span>
        </button>
        <button v-if="canControl" type="button" class="headerBtn recommendationsBtn" :disabled="!isMeetingStartedOrFinished" @click="showRecommendationsModal = true" :title="$t('Recommendations')">
          <Icon icon="lightbulb" class="w-3.5 h-3.5" /> {{ $t('Recommendations') }}
          <span v-if="totalRecommendationsCount" class="btnBadge recBadge">{{ totalRecommendationsCount }}</span>
        </button>
        <button v-if="allAgendasCompleted" type="button" class="headerBtn votingBtn" :disabled="!isMeetingStartedOrFinished" @click="$emit('view-voting-results')" :title="$t('VotingLabel')">
          <Icon icon="how_to_vote" class="w-3.5 h-3.5" /> {{ $t('VotingLabel') }}
          <span v-if="totalVotesCount" class="btnBadge voteBadge">{{ totalVotesCount }}</span>
        </button>
        <button v-if="canControl" type="button" class="headerBtn summaryBtn" :disabled="!isMeetingStartedOrFinished" @click="openSummaryModal" :title="$t('MeetingSummary')">
          <Icon icon="summarize" class="w-3.5 h-3.5" /> {{ $t('MeetingSummary') }}
        </button>
        <button
          v-if="isMeetingStartedOrFinished && !meetingEnded"
          type="button"
          class="headerBtn recordBtn"
          :class="{ recording: isRecording, uploading: isUploadingRecording }"
          :disabled="isUploadingRecording"
          @click="$emit(isRecording ? 'stop-recording' : 'start-recording')"
          :title="isRecording ? $t('StopRecording') : $t('StartRecording')"
        >
          <span v-if="isUploadingRecording" class="recordingSpinner"></span>
          <Icon v-else-if="isRecording" icon="stop_circle" class="w-4 h-4" />
          <Icon v-else icon="mic" class="w-3.5 h-3.5" />
          <span v-if="isRecording && recordingDuration" class="recDuration">{{ formatRecDuration(recordingDuration || 0) }}</span>
          <span v-else-if="isUploadingRecording">{{ $t('Transcribing') }}...</span>
          <span v-else>{{ $t('Record') }}</span>
        </button>
        <button v-if="canStartMeeting" type="button" class="headerBtn startBtn" @click="$emit('confirm-start')" :disabled="actionLoading">
          <Icon icon="play_circle" class="w-4 h-4" /> {{ $t('StartMeeting') }}
        </button>
        <button v-if="canEndMeeting" type="button" class="headerBtn endBtn" @click="$emit('confirm-end')" :disabled="actionLoading">
          <Icon icon="stop_circle" class="w-4 h-4" /> {{ $t('EndMeeting') }}
        </button>
        <button type="button" class="headerBtn themeBtn" @click="$emit('toggle-theme')" :title="isLightTheme ? $t('DarkMode') : $t('LightMode')">
          <Icon v-if="isLightTheme" icon="dark_mode" class="w-4 h-4" />
          <Icon v-else icon="light_mode" class="w-4 h-4" />
        </button>
        <button type="button" class="headerBtn closeBtn" @click="$emit('go-back')" :title="$t('Close')">
          <Icon icon="close" class="w-4 h-4" />
        </button>
      </div>
    </div>

    <!-- Meeting Summary Modal -->
    <Modal v-model="showSummaryModal" :title="$t('MeetingSummary')" icon="mdi:text-box-outline" size="4xl" scrollable>
      <!-- Per-Attendee Transcripts -->
      <div v-if="completedTranscripts.length > 0" class="transcript-section">
        <div class="transcript-header">
          <Icon icon="mic" class="w-4 h-4" style="color: #ef4444;" />
          <span class="transcript-label">{{ $t('AudioTranscript') }}</span>
          <span class="transcript-badge">{{ completedTranscripts.length }}</span>
        </div>
        <div class="transcript-dialog">
          <div v-for="tr in completedTranscripts" :key="tr.id" class="transcript-message">
            <div class="transcript-speaker">{{ tr.attendeeName || $t('Attendee') }}</div>
            <div class="transcript-bubble">{{ tr.transcriptText }}</div>
          </div>
        </div>
        <div class="transcript-footer">
          <button v-if="canControl" class="transcript-action-btn fill" @click="$emit('generate-combined-summary')">
            <Icon icon="auto_awesome" class="w-3.5 h-3.5" /> {{ $t('GenerateCombinedSummary') }}
          </button>
          <button v-if="combinedSummaryText && canControl" class="transcript-action-btn fill" @click="localSummaryText = combinedSummaryText">
            <Icon icon="content_paste" class="w-3.5 h-3.5" /> {{ $t('UseAsSummary') }}
          </button>
        </div>
        <div v-if="combinedSummaryText" class="transcript-combined-summary">
          <div class="transcript-combined-label">
            <Icon icon="auto_awesome" class="w-3.5 h-3.5" /> {{ $t('AISummary') }}
          </div>
          <div class="transcript-combined-text">{{ combinedSummaryText }}</div>
        </div>
      </div>

      <textarea
        v-model="localSummaryText"
        class="w-full min-h-[200px] p-3 border border-gray-200 rounded-lg text-sm resize-y focus:outline-none focus:border-[#006d4b] focus:ring-2 focus:ring-[#006d4b]/10"
        :placeholder="$t('WriteMeetingSummary')"
        rows="10"
      ></textarea>
      <template #footer>
        <button class="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-50" @click="showSummaryModal = false">{{ $t('Cancel') }}</button>
        <button class="px-4 py-2 text-sm font-medium text-white rounded-lg" style="background: linear-gradient(135deg, #006d4b 0%, #006d4b 100%)" @click="saveSummary" :disabled="savingSummary">
          <span class="flex items-center gap-2">
            <Icon icon="mdi:content-save" class="w-4 h-4" />
            {{ savingSummary ? $t('SavingSummary') : $t('SaveSummary') }}
          </span>
        </button>
      </template>
    </Modal>

    <!-- Agenda Summary Modal -->
    <Modal v-model="showAgendaSummaryModal" :title="$t('AgendaSummaryLabel')" :description="currentAgenda?.title || currentAgenda?.titleAr" icon="mdi:text-box-outline" size="lg" scrollable>
      <textarea
        v-model="localAgendaSummaryText"
        class="w-full min-h-[200px] p-3 border border-gray-200 rounded-lg text-sm resize-y focus:outline-none focus:border-[#006d4b] focus:ring-2 focus:ring-[#006d4b]/10"
        :placeholder="$t('WriteAgendaSummary')"
        rows="10"
        :readonly="!canControl"
      ></textarea>
      <template #footer>
        <button class="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-50" @click="showAgendaSummaryModal = false">{{ $t('Cancel') }}</button>
        <button v-if="canControl" class="px-4 py-2 text-sm font-medium text-white rounded-lg" style="background: linear-gradient(135deg, #006d4b 0%, #006d4b 100%)" @click="saveAgendaSummary" :disabled="savingAgendaSummary">
          <span class="flex items-center gap-2">
            <Icon icon="mdi:content-save" class="w-4 h-4" />
            {{ savingAgendaSummary ? $t('SavingSummary') : $t('SaveSummary') }}
          </span>
        </button>
      </template>
    </Modal>

    <!-- Comments Modal Component -->
    <MeetingRoomCommentsModal
      :show="showCommentsModal"
      :notes="visibleNotes"
      :agenda-title="currentAgenda?.title || currentAgenda?.titleAr || ''"
      :current-user-id="currentUserId"
      :note-text="noteText"
      :note-is-public="noteIsPublic"
      :editing-note-id="editingNoteId"
      :show-all-agendas="allAgendasCompleted"
      :all-agendas-data="allAgendasData"
      :is-read-only="isInitialMomReadOnly"
      :can-control="canControl"
      @close="showCommentsModal = false"
      @edit-note="(note) => $emit('edit-note', note)"
      @delete-note="(noteId) => $emit('delete-note', noteId)"
      @update:noteText="(val) => $emit('update:noteText', val)"
      @update:noteIsPublic="(val) => $emit('update:noteIsPublic', val)"
      @save-note="$emit('save-note')"
      @cancel-edit="$emit('cancel-edit')"
      @add-note-for-agenda="(agendaId) => { showCommentsModal = false; $emit('add-note-for-agenda', agendaId) }"
      @save-note-for-agenda="(agendaId, text, isPublic) => $emit('save-note-for-agenda', agendaId, text, isPublic)"
      @edit-note-for-agenda="(noteId, agendaId, text, isPublic) => $emit('edit-note-for-agenda', noteId, agendaId, text, isPublic)"
    />

    <!-- Recommendations Modal Component -->
    <MeetingRoomRecommendationsModal
      :show="showRecommendationsModal"
      :recommendations="agendaRecommendations"
      :agenda-title="currentAgenda?.title || currentAgenda?.titleAr || ''"
      :can-control="canControl"
      :current-user-id="currentUserId"
      :show-all-agendas="allAgendasCompleted"
      :all-agendas-data="allAgendasData"
      :is-read-only="isInitialMomReadOnly"
      @close="showRecommendationsModal = false"
      @edit-recommendation="(rec) => { showRecommendationsModal = false; $emit('edit-recommendation', rec) }"
      @delete-recommendation="(recId) => $emit('delete-recommendation', recId)"
      @open-add-recommendation="showRecommendationsModal = false; $emit('open-add-recommendation')"
      @add-recommendation-for-agenda="(agendaId) => { showRecommendationsModal = false; $emit('add-recommendation-for-agenda', agendaId) }"
    />

    <div class="panelBody">
      <!-- Document Viewer -->
      <div class="viewerWrap">
        <div class="viewer">
          <div class="viewerCanvasWrap">
            <!-- Final MOM PDF - Show when activeViewType is finalMom -->
            <div v-if="activeViewType === 'finalMom' && savedFinalMinutesUrl && canViewMom" class="savedMinutesState">
              <PdfViewer
                :src="savedFinalMinutesUrl"
                :file-name="savedFinalMinutesFileName || $t('FinalMinutesFile')"
                class="pdfViewerFull"
              >
                <!-- Status badge in toolbar -->
                <template #status-badge>
                  <span v-if="isMeetingFinalized" class="toolbarStatusBadge finalized">
                    <Icon icon="check_circle" class="w-3 h-3" />
                    {{ $t('Approved') }}
                  </span>
                  <span v-else class="toolbarStatusBadge pending">{{ $t('UnderReview') }}</span>
                </template>
              </PdfViewer>
            </div>
            <!-- Initial MOM PDF - Show when activeViewType is initialMom -->
            <div v-else-if="activeViewType === 'initialMom' && savedMinutesUrl && canViewMom" class="savedMinutesState">
              <PdfViewer
                :src="savedMinutesUrl"
                :file-name="savedMinutesFileName || $t('InitialMinutesFile')"
                class="pdfViewerFull"
              >
                <!-- Status badge -->
                <template #status-badge>
                  <span v-if="isInitialMomReadOnly" class="toolbarStatusBadge finalized">
                    <Icon icon="check_circle" class="w-3 h-3" />
                    {{ $t('Approved') }}
                  </span>
                  <span v-else class="toolbarStatusBadge pending">{{ $t('Draft') }}</span>
                </template>
              </PdfViewer>
            </div>
            <!-- Final MOM Options - Show when Initial MOM is approved (status 7) and no Final MOM yet -->
            <div v-else-if="showFinalMomButtons" class="minutesOptionsState finalMomState">
              <div class="minutesOptionsCard finalMomCard">
                <div class="minutesIcon finalIcon">
                  <Icon icon="task_alt" class="w-10 h-10" />
                </div>
                <h3>{{ $t('InitialMOMApproved') }}</h3>
                <p>{{ $t('CanNowCreateFinalMinutes') }}</p>

                <div class="minutesActions">
                  <button class="minutesBtn upload final" @click="$emit('upload-final-minutes')">
                    <Icon icon="upload" class="w-6 h-6" />
                    <span>{{ $t('UploadFinalMinutesManually') }}</span>
                    <small>{{ $t('UploadPreparedFinalFile') }}</small>
                  </button>

                  <button class="minutesBtn generate final" @click="$emit('generate-final-minutes')">
                    <Icon icon="auto_awesome" class="w-6 h-6" />
                    <span>{{ $t('GenerateFinalMinutesAuto') }}</span>
                    <small>{{ $t('GenerateFinalFromData') }}</small>
                  </button>
                </div>
              </div>
            </div>
            <!-- Minutes Generation Loading State -->
            <div v-else-if="minutesGenerating" class="minutesLoadingState">
              <div class="minutesLoadingCard">
                <div class="loadingSpinner">
                  <Icon icon="progress_activity" class="w-12 h-12 spinnerIcon" />
                </div>
                <h3>{{ $t('GeneratingMinutes') }}</h3>
                <p>{{ $t('PleaseWaitGeneratingMinutes') }}</p>
              </div>
            </div>
            <!-- Final MOM Generation Loading State -->
            <div v-else-if="finalMinutesGenerating" class="minutesLoadingState finalLoadingState">
              <div class="minutesLoadingCard">
                <div class="loadingSpinner finalSpinner">
                  <Icon icon="progress_activity" class="w-12 h-12 spinnerIcon" />
                </div>
                <h3>{{ $t('GeneratingFinalMinutes') }}</h3>
                <p>{{ $t('PleaseWaitGenerating') }}</p>
              </div>
            </div>
            <!-- Minutes Generation Options - Show when all agendas completed, meeting started (4), and no MOM generated -->
            <div v-else-if="showMomButtons" class="minutesOptionsState">
              <div class="minutesOptionsCard">
                <div class="minutesIcon">
                  <Icon icon="description" class="w-10 h-10" />
                </div>
                <h3>{{ $t('AllAgendasCompleted') }}</h3>
                <p>{{ $t('CanNowCreateMinutes') }}</p>

                <div class="minutesActions">
                  <button class="minutesBtn upload" @click="$emit('upload-minutes')">
                    <Icon icon="upload" class="w-6 h-6" />
                    <span>{{ $t('UploadMinutesManually') }}</span>
                    <small>{{ $t('UploadPreparedFile') }}</small>
                  </button>

                  <button class="minutesBtn generate" @click="$emit('generate-minutes')">
                    <Icon icon="auto_awesome" class="w-6 h-6" />
                    <span>{{ $t('GenerateMinutesAuto') }}</span>
                    <small>{{ $t('GenerateFromData') }}</small>
                  </button>
                </div>
              </div>
            </div>
            <!-- Regular Attachment (PDF/DOC/Images/PowerPoint - PowerPoint is converted to PDF on backend) -->
            <PdfViewer
              v-else-if="activeViewType === 'attachment' && currentAttachment && currentAttachmentUrl"
              :src="currentAttachmentUrl"
              :file-name="currentAttachment?.fileName || currentAttachment?.name || ''"
              class="pdfViewerFull"
            />
            <!-- Empty state -->
            <div v-else class="emptyState">
              <Icon icon="document_scanner" class="w-12 h-12 emptyIcon" />
              <h3>{{ $t('DocumentViewer') }}</h3>
              <p>{{ $t('SelectAttachment') }}</p>
            </div>
          </div>
          <!-- Initial MOM Action Bar - Always visible when Initial MOM exists -->
          <MeetingRoomMinutesActionBar
            v-if="canControl && savedMinutesUrl && !isInitialMomApproved && !hasFinalMomGenerated"
            :versions="minutesVersions"
            :current-version-id="currentMinutesVersionId"
            :loading-versions="loadingMinutesVersions"
            :regenerating="regeneratingMinutes"
            :sending-for-approval="sendingMinutesForApproval"
            :creating-final="creatingFinalMinutes"
            :approvals-count="minutesApprovals.length"
            :hide-create-final="isInitialMomApproved || hasFinalMomGenerated"
            :is-read-only="isInitialMomReadOnly"
            @regenerate="$emit('regenerate-minutes')"
            @open-approval-modal="$emit('open-approval-modal')"
            @select-version="selectInitialVersionById"
            @create-final="$emit('create-final-minutes')"
            @view-approvals="$emit('view-approvals')"
          />
          <!-- Final MOM Action Bar - Always visible when Final MOM exists -->
          <MeetingRoomMinutesActionBar
            v-if="canControl && savedFinalMinutesUrl"
            :versions="finalMinutesVersions"
            :current-version-id="currentFinalMinutesVersionId"
            :loading-versions="loadingFinalMinutesVersions"
            :regenerating="finalMinutesGenerating"
            :sending-for-approval="sendingFinalMinutesForApproval"
            :creating-final="false"
            :approvals-count="finalApprovalsCount"
            :hide-create-final="true"
            :show-approve-final="allFinalApprovalsComplete && !isMeetingFinalized"
            :approving-final="approvingFinalMom"
            :is-read-only="isMeetingFinalized"
            @regenerate="$emit('generate-final-minutes')"
            @open-approval-modal="$emit('open-final-approval-modal')"
            @select-version="selectFinalVersionById"
            @view-approvals="$emit('view-final-approvals')"
            @approve-final="$emit('approve-final-mom')"
          />
        </div>
      </div>

      <!-- Collapse Toggle Bar - Hide when all agendas completed -->
      <div v-if="!allAgendasCompleted" class="collapseToggleBar" @click="isDetailsCollapsed = !isDetailsCollapsed">
        <div class="toggleLine"></div>
        <div class="toggleArrow">
          <Icon v-if="!isDetailsCollapsed" icon="expand_more" class="w-4 h-4" />
          <Icon v-else icon="expand_less" class="w-4 h-4" />
        </div>
        <div class="toggleLine"></div>
      </div>

      <!-- Active Agenda Workspace - Hide when all agendas completed -->
      <div v-show="!isDetailsCollapsed && !allAgendasCompleted" class="detailsBar">
        <div class="detailsCard">
          <div class="detailsHead">
            <div class="h"><Icon icon="space_dashboard" class="w-3.5 h-3.5" /> <span class="agendaHeadTitle">{{ currentAgenda?.title || currentAgenda?.titleAr || $t('ActiveAgendaWorkspace') }}</span></div>
            <div v-if="canControl" class="controls">
              <!-- Single toggle button like control panel -->
              <button
                v-if="isMeetingLive && currentAgenda && !isCurrentAgendaCompleted"
                class="btn small"
                :class="getTimerButtonClass"
                @click="$emit('toggle-agenda-timer')"
              >
                <Icon v-if="currentAgenda.isRunning && !currentAgenda.paused" icon="pause" class="w-3.5 h-3.5" />
                <Icon v-else icon="play_arrow" class="w-3.5 h-3.5" />
                {{ currentAgenda.isRunning && !currentAgenda.paused ? $t('PauseTimer') : $t('PlayTimer') }}
              </button>
              <button
                v-if="isMeetingLive"
                class="btn small"
                :class="isCurrentAgendaCompleted ? 'success' : 'ghost'"
                :disabled="!currentAgenda || isCurrentAgendaCompleted"
                @click="$emit('complete-agenda')"
              >
                <Icon icon="check_circle" class="w-3.5 h-3.5" /> {{ isCurrentAgendaCompleted ? $t('Completed') : $t('EndTopic') }}
              </button>
              <button
                v-if="isMeetingStartedOrFinished"
                class="btn small agendaSummaryBtn"
                @click="openAgendaSummaryModal"
                :disabled="!currentAgenda"
              >
                <Icon icon="description" class="w-3.5 h-3.5" /> {{ $t('AgendaSummaryLabel') }}
              </button>
            </div>
          </div>

          <div class="detailsBody" :class="canControl ? 'threeColLayout' : 'twoColLayout'">
            <!-- Comments Section -->
            <div class="workspaceSection commentsSection">
              <div class="sectionHeader">
                <Icon icon="chat_bubble_outline" class="w-3.5 h-3.5" />
                <span style="flex:1">{{ $t('Comments') }}</span>
                <span class="sectionCount">{{ visibleNotes.length }}</span>
                <button class="sectionExpandBtn" @click="showCommentsModal = true" :title="$t('Expand')">
                  <Icon icon="open_in_full" class="w-3 h-3" />
                </button>
              </div>

              <div class="sectionList">
                <div v-if="visibleNotes.length === 0" class="emptySection">
                  {{ $t('NoCommentsYet') }}
                </div>
                <div
                  v-for="note in visibleNotes"
                  :key="note.id"
                  class="listItem commentItem"
                  :class="{ editing: editingNoteId === note.id }"
                >
                  <div class="itemContent">
                    <span class="itemText">{{ note.text }}</span>
                    <div class="commentMeta">
                      <span class="itemMeta">{{ note.createdByName || $t('User') }}</span>
                      <span v-if="note.isPublic" class="commentBadge public">
                        <Icon icon="language" class="w-[9px] h-[9px]" /> {{ $t('Public') }}
                      </span>
                      <span v-else class="commentBadge private">
                        <Icon icon="lock" class="w-[9px] h-[9px]" /> {{ $t('Private') }}
                      </span>
                    </div>
                  </div>
                  <div class="itemActions" v-if="!isInitialMomReadOnly && note.createdBy == currentUserId">
                    <button class="iconBtn" @click="$emit('edit-note', note); showCommentsModal = true" :title="$t('Edit')">
                      <Icon icon="edit" class="w-[11px] h-[11px]" />
                    </button>
                    <button class="iconBtn danger" @click="$emit('delete-note', note.id)" :title="$t('Delete')">
                      <Icon icon="delete" class="w-[11px] h-[11px]" />
                    </button>
                  </div>
                </div>
              </div>

              <div class="sectionForm" v-if="canControl && !isInitialMomReadOnly" style="padding: 8px 12px;">
                <button class="addCommentBtnCompact" @click="showCommentsModal = true">
                  <Icon icon="add" class="w-3.5 h-3.5" /> {{ $t('AddComment') }}
                </button>
              </div>
            </div>

            <!-- Recommendations Section - Only visible to organizer -->
            <div v-if="canControl" class="workspaceSection recommendationsSection">
              <div class="sectionHeader">
                <Icon icon="lightbulb" class="w-3.5 h-3.5" />
                <span style="flex:1">{{ $t('Recommendations') }}</span>
                <span class="sectionCount recCount">{{ agendaRecommendations.length }}</span>
                <button class="sectionExpandBtn" @click="showRecommendationsModal = true" :title="$t('Expand')">
                  <Icon icon="open_in_full" class="w-3 h-3" />
                </button>
              </div>

              <div class="sectionList">
                <div v-if="agendaRecommendations.length === 0" class="emptySection">
                  {{ $t('NoRecommendations') }}
                </div>
                <div
                  v-for="rec in agendaRecommendations"
                  :key="rec.id"
                  class="listItem recItem"
                >
                  <div class="itemContent">
                    <span class="itemText">{{ rec.text }}</span>
                    <span class="itemMeta">{{ rec.ownerName || $t('Unspecified') }}</span>
                  </div>
                  <div class="itemActions" v-if="rec.owner == currentUserId || rec.createdBy == currentUserId">
                    <button class="iconBtn" @click="$emit('edit-recommendation', rec)" :title="$t('Edit')">
                      <Icon icon="edit" class="w-[11px] h-[11px]" />
                    </button>
                    <button class="iconBtn danger" @click="$emit('delete-recommendation', rec.id)" :title="$t('Delete')">
                      <Icon icon="delete" class="w-[11px] h-[11px]" />
                    </button>
                  </div>
                </div>
              </div>

              <div class="sectionForm" v-if="canControl && !isInitialMomReadOnly" style="padding: 8px 12px;">
                <button class="addRecBtnCompact" @click="$emit('open-add-recommendation')">
                  <Icon icon="add" class="w-3.5 h-3.5" /> {{ $t('AddRecommendation') }}
                </button>
              </div>
            </div>

            <!-- Voting Section -->
            <div class="workspaceSection votingSection">
              <div class="sectionHeader">
                <Icon icon="how_to_vote" class="w-3.5 h-3.5" />
                <span>{{ $t('VotingLabel') }}</span>
                <button
                  v-if="canControl && votingOptions.length > 0"
                  class="resultsBtn"
                  @click="$emit('view-voting-results')"
                  :title="$t('ViewVotesDetails')"
                >
                  <Icon icon="bar_chart" class="w-3 h-3" />
                </button>
              </div>

              <div class="votingContent">
                <div class="voteOptionsList">
                  <button
                    v-for="option in votingOptions"
                    :key="option.id"
                    class="voteItem"
                    :class="{ active: userVoteOptionId === option.id }"
                    :disabled="!votingActive || votingLoading"
                    @click="$emit('submit-vote', option.id)"
                  >
                    <span class="voteItemText">{{ votingOptionLabel(option) }}</span>
                    <div class="voteIndicator">
                      <Icon icon="check" class="voteIndicatorCheck" />
                    </div>
                  </button>
                </div>

                <!-- Show message if no voting options available -->
                <div v-if="!votingOptions.length && canVote" class="noVotingOptions">
                  {{ $t('NoVotesForAgenda') }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { MeetingStatusEnum } from '@/helpers/enumerations'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import PdfViewer from '@/components/ui/PdfViewer.vue'
import MeetingRoomMinutesActionBar from './MeetingRoomMinutesActionBar.vue'
import MeetingRoomCommentsModal from './MeetingRoomCommentsModal.vue'
import MeetingRoomRecommendationsModal from './MeetingRoomRecommendationsModal.vue'
import type { MinutesVersion } from './types/minutes'

const { locale } = useI18n()
const isArabic = computed(() => locale.value === 'ar')

// Click-outside directive for dropdowns
const vClickOutside = {
  mounted(el: HTMLElement & { _clickOutside?: (e: MouseEvent) => void }, binding: any) {
    el._clickOutside = (event: MouseEvent) => {
      if (!(el === event.target || el.contains(event.target as Node))) {
        binding.value(event)
      }
    }
    document.addEventListener('click', el._clickOutside)
  },
  unmounted(el: HTMLElement & { _clickOutside?: (e: MouseEvent) => void }) {
    if (el._clickOutside) {
      document.removeEventListener('click', el._clickOutside)
    }
  }
}

// Collapse state
const isDetailsCollapsed = ref(false)

// Details dropdown state
const showDetailsDropdown = ref(false)

// Modal states
const showSummaryModal = ref(false)
const showCommentsModal = ref(false)
const showAgendaSummaryModal = ref(false)
const showRecommendationsModal = ref(false)
const localSummaryText = ref('')
const localAgendaSummaryText = ref('')
const savingSummary = ref(false)
const savingAgendaSummary = ref(false)

// Inline version dropdown states
const showInitialVersionDropdown = ref(false)
const showFinalVersionDropdown = ref(false)

interface VotingOption {
  id: number
  nameAr?: string
  nameEn?: string
  name?: string
  active?: boolean
  weight?: number
  displayOrder?: number
  votingTypeId?: number
}

interface AgendaVotingData {
  id: number
  title: string
  votingOptions: VotingOption[]
  meetingUserVotes: any[]
}

interface AgendaData {
  id: number
  title: string
  notes: any[]
  recommendations: any[]
}

const props = defineProps<{
  meeting: any
  currentAttachment: any
  currentAttachmentUrl: string
  currentAgenda: any
  meetingElapsedTime: string
  canStartMeeting: boolean
  canEndMeeting: boolean
  actionLoading: boolean
  agendaRecommendations: any[]
  agendaSummary: string
  agendaNotes: any[]
  noteText: string
  noteIsPublic: boolean
  editingNoteId: number | null
  currentUserId: string
  voteTimer: string
  voteStatus: string
  votingActive: boolean
  canVote: boolean
  canControl: boolean
  meetingSummary: string
  isLightTheme: boolean
  votingOptions: VotingOption[]
  userVoteOptionId: number | null
  votingLoading: boolean
  allAgendasCompleted: boolean
  allAgendasVotingData: AgendaVotingData[]
  allAgendasData: AgendaData[]
  activeViewType?: 'attachment' | 'initialMom' | 'finalMom'
  savedMinutesUrl: string
  savedMinutesFileName: string
  minutesGenerating: boolean
  // Minutes versioning props
  minutesVersions: MinutesVersion[]
  currentMinutesVersionId: number | null
  loadingMinutesVersions: boolean
  regeneratingMinutes: boolean
  sendingMinutesForApproval: boolean
  creatingFinalMinutes: boolean
  minutesApprovals: any[]
  // Final MOM props
  savedFinalMinutesUrl?: string
  savedFinalMinutesFileName?: string
  finalMinutesGenerating?: boolean
  // Final MOM versioning props
  finalMinutesVersions?: MinutesVersion[]
  currentFinalMinutesVersionId?: number | null
  loadingFinalMinutesVersions?: boolean
  sendingFinalMinutesForApproval?: boolean
  // Final MOM approval status
  finalApprovalsCount?: number
  allFinalApprovalsComplete?: boolean
  approvingFinalMom?: boolean
  // MOM visibility permission
  canViewMom?: boolean
  // Audio recording
  isRecording?: boolean
  isPausedRecording?: boolean
  recordingDuration?: number
  isUploadingRecording?: boolean
  transcripts?: any[]
}>()


// Filter notes: show public notes + user's own private notes
// Use == for type coercion (createdBy may be int, currentUserId may be string)
const visibleNotes = computed(() => {
  return props.agendaNotes.filter(note =>
    note.isPublic || note.createdBy == props.currentUserId
  )
})

// Total recommendations count (all agendas when completed, or current agenda)
const totalRecommendationsCount = computed(() => {
  if (props.allAgendasCompleted && props.allAgendasData) {
    return props.allAgendasData.reduce((sum, agenda) => sum + (agenda.recommendations?.length || 0), 0)
  }
  return props.agendaRecommendations.length
})

// Total votes count (all agendas when completed)
const totalVotesCount = computed(() => {
  if (props.allAgendasCompleted && props.allAgendasVotingData) {
    return props.allAgendasVotingData.filter(a => a.votingOptions.length > 0).length
  }
  return 0
})

// Total comments count (all agendas when completed, or visible notes)
const totalCommentsCount = computed(() => {
  if (props.allAgendasCompleted && props.allAgendasData) {
    return props.allAgendasData.reduce((sum, agenda) => sum + (agenda.notes?.length || 0), 0)
  }
  return visibleNotes.value.length
})

// Check if there are any meeting details to show
const hasAnyDetails = computed(() => {
  return !!(
    props.meeting.referenceNumber || props.meeting.refNumber || props.meeting.meetingNumber ||
    props.meeting.location ||
    props.meeting.startTime || props.meeting.fromTime || props.meeting.start ||
    props.meeting.endTime || props.meeting.toTime || props.meeting.end
  )
})

// Check if minutes have been generated (status >= PendingInitialMeetingMinutesApproval means MOM has been generated)
const hasMinutesGenerated = computed(() => {
  const statusId = Number(props.meeting?.statusId || 0)
  return statusId >= MeetingStatusEnum.PendingInitialMeetingMinutesApproval
})

// Check if meeting is in Started or Finished status - MOM buttons should show in these statuses
const isMeetingStartedOrFinished = computed(() => {
  const statusId = Number(props.meeting?.statusId || 0)
  return statusId >= MeetingStatusEnum.Started
})

const meetingEnded = computed(() => {
  const statusId = Number(props.meeting?.statusId || 0)
  return statusId > MeetingStatusEnum.Started
})

const completedTranscripts = computed(() => {
  return (props.transcripts || []).filter((t: any) => t.status === 'Completed' || t.status === 'Summarized')
})

const combinedSummaryText = computed(() => {
  const summarized = (props.transcripts || []).find((t: any) => t.summaryText)
  return summarized?.summaryText || null
})

// Check if meeting is currently live (in progress) - for enabling comments/chat
const isMeetingLive = computed(() => {
  const statusId = Number(props.meeting?.statusId || 0)
  return statusId === MeetingStatusEnum.Started
})

// Show MOM buttons when all agendas completed, user can control, meeting is started/finished, and no MOM generated yet
const showMomButtons = computed(() => {
  const result = props.allAgendasCompleted &&
         props.canControl &&
         isMeetingStartedOrFinished.value &&
         !hasMinutesGenerated.value &&
         !props.savedMinutesUrl

  // Debug logging
  console.log('[MeetingRoomViewer] showMomButtons conditions:', {
    allAgendasCompleted: props.allAgendasCompleted,
    canControl: props.canControl,
    isMeetingStartedOrFinished: isMeetingStartedOrFinished.value,
    hasMinutesGenerated: hasMinutesGenerated.value,
    savedMinutesUrl: !!props.savedMinutesUrl,
    meetingStatusId: props.meeting?.statusId,
    result
  })

  return result
})

// Check if Initial MOM has been approved (status = 7) - show Final MOM options
const isInitialMomApproved = computed(() => {
  const statusId = Number(props.meeting?.statusId || 0)
  return statusId === MeetingStatusEnum.InitialMeetingMinutesApproved
})

// Check if Final MOM is pending or signed (status >= 8)
const hasFinalMomGenerated = computed(() => {
  const statusId = Number(props.meeting?.statusId || 0)
  return statusId >= MeetingStatusEnum.PendingFinalMeetingMinutesSign
})

// Check if meeting is finalized (status = 9 - FinalMeetingMinutesSigned)
const isMeetingFinalized = computed(() => {
  const statusId = Number(props.meeting?.statusId || 0)
  return statusId === MeetingStatusEnum.FinalMeetingMinutesSigned
})

// Check if Initial MOM should be read-only (status >= 7 means Initial MOM was approved)
const isInitialMomReadOnly = computed(() => {
  const statusId = Number(props.meeting?.statusId || 0)
  return statusId >= MeetingStatusEnum.InitialMeetingMinutesApproved
})

// Show Final MOM buttons when initial MOM approved and no final MOM generated yet
const showFinalMomButtons = computed(() => {
  const result = props.canControl &&
         isInitialMomApproved.value &&
         !hasFinalMomGenerated.value &&
         !props.savedFinalMinutesUrl

  // Debug logging
  console.log('[MeetingRoomViewer] showFinalMomButtons conditions:', {
    canControl: props.canControl,
    isInitialMomApproved: isInitialMomApproved.value,
    hasFinalMomGenerated: hasFinalMomGenerated.value,
    savedFinalMinutesUrl: !!props.savedFinalMinutesUrl,
    meetingStatusId: props.meeting?.statusId,
    result
  })

  return result
})

// Version labels for inline toolbar
const initialVersionLabel = computed(() => {
  if (props.loadingMinutesVersions) return 'v...'
  const version = props.minutesVersions?.find((v: any) => v.id === props.currentMinutesVersionId)
  return version ? `v${version.version}.0` : 'v1.0'
})

const finalVersionLabel = computed(() => {
  if (props.loadingFinalMinutesVersions) return 'v...'
  const version = props.finalMinutesVersions?.find((v: any) => v.id === props.currentFinalMinutesVersionId)
  return version ? `v${version.version}.0` : 'v1.0'
})

// Version selection functions
function selectInitialVersion(version: any) {
  emit('select-minutes-version', version.id)
  showInitialVersionDropdown.value = false
}

function selectFinalVersion(version: any) {
  emit('select-final-minutes-version', version.id)
  showFinalVersionDropdown.value = false
}

// Version selection by ID (for action bar component)
function selectInitialVersionById(versionId: number) {
  emit('select-minutes-version', versionId)
}

function selectFinalVersionById(versionId: number) {
  emit('select-final-minutes-version', versionId)
}

// Close dropdown when clicking outside
const handleClickOutside = (event: MouseEvent) => {
  const target = event.target as HTMLElement
  if (!target.closest('.detailsDropdown')) {
    showDetailsDropdown.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})

const emit = defineEmits<{
  (e: 'confirm-start'): void
  (e: 'confirm-end'): void
  (e: 'go-back'): void
  (e: 'toggle-agenda-timer'): void
  (e: 'complete-agenda'): void
  (e: 'enable-voting'): void
  (e: 'submit-vote', votingOptionId: number): void
  (e: 'update:noteText', value: string): void
  (e: 'update:noteIsPublic', value: boolean): void
  (e: 'save-note'): void
  (e: 'edit-note', note: any): void
  (e: 'delete-note', noteId: number): void
  (e: 'cancel-edit'): void
  (e: 'open-summary-modal'): void
  (e: 'save-summary', summary: string): void
  (e: 'open-agenda-summary-modal'): void
  (e: 'save-agenda-summary', summary: string): void
  (e: 'edit-recommendation', rec: any): void
  (e: 'delete-recommendation', recId: number): void
  (e: 'open-add-recommendation'): void
  (e: 'add-recommendation-for-agenda', agendaId: number): void
  (e: 'add-note-for-agenda', agendaId: number): void
  (e: 'save-note-for-agenda', agendaId: number, text: string, isPublic: boolean): void
  (e: 'edit-note-for-agenda', noteId: number, agendaId: number, text: string, isPublic: boolean): void
  (e: 'toggle-theme'): void
  (e: 'view-voting-results'): void
  (e: 'upload-minutes'): void
  (e: 'generate-minutes'): void
  (e: 'regenerate-minutes'): void
  (e: 'open-approval-modal'): void
  (e: 'select-minutes-version', versionId: number): void
  (e: 'create-final-minutes'): void
  (e: 'view-approvals'): void
  // Final MOM events
  (e: 'upload-final-minutes'): void
  (e: 'generate-final-minutes'): void
  (e: 'open-final-approval-modal'): void
  (e: 'select-final-minutes-version', versionId: number): void
  (e: 'view-final-approvals'): void
  (e: 'approve-final-mom'): void
  (e: 'start-recording'): void
  (e: 'stop-recording'): void
  (e: 'generate-transcript-summary', transcriptId: number): void
  (e: 'generate-combined-summary'): void
}>()

const formatRecDuration = (seconds: number) => {
  const m = Math.floor(seconds / 60).toString().padStart(2, '0')
  const s = (seconds % 60).toString().padStart(2, '0')
  return `${m}:${s}`
}

// Check if current agenda is completed
const isCurrentAgendaCompleted = computed(() => {
  if (!props.currentAgenda) return false
  return props.currentAgenda.actualEndDate != null
})

// Get button class based on agenda state (same logic as control panel)
const getTimerButtonClass = computed(() => {
  if (!props.currentAgenda) return 'primary'
  if (props.currentAgenda.isRunning && !props.currentAgenda.paused) return 'danger'  // Running - red
  if (props.currentAgenda.isRunning && props.currentAgenda.paused) return 'warning'  // Paused - orange
  return 'primary'  // Not started - gradient
})

// Open summary modal and request load from parent
const openSummaryModal = () => {
  showSummaryModal.value = true
  localSummaryText.value = props.meetingSummary || ''
  emit('open-summary-modal')
}

// Watch for summary prop changes (when parent loads it)
watch(() => props.meetingSummary, (newVal) => {
  localSummaryText.value = newVal || ''
}, { immediate: false })

// Watch for agenda summary prop changes
watch(() => props.agendaSummary, (newVal) => {
  localAgendaSummaryText.value = newVal || ''
}, { immediate: false })

// Save summary
const saveSummary = async () => {
  savingSummary.value = true
  emit('save-summary', localSummaryText.value)
  // Close modal after a short delay to show loading state
  setTimeout(() => {
    savingSummary.value = false
    showSummaryModal.value = false
  }, 500)
}

// Open agenda summary modal
const openAgendaSummaryModal = () => {
  showAgendaSummaryModal.value = true
  localAgendaSummaryText.value = props.agendaSummary || ''
  emit('open-agenda-summary-modal')
}

// Save agenda summary
const saveAgendaSummary = async () => {
  savingAgendaSummary.value = true
  emit('save-agenda-summary', localAgendaSummaryText.value)
  // Close modal after a short delay to show loading state
  setTimeout(() => {
    savingAgendaSummary.value = false
    showAgendaSummaryModal.value = false
  }, 500)
}

// Helper: Format time from date string or time string
const formatTime = (timeStr: string) => {
  if (!timeStr) return ''
  try {
    // If it's already a time format like "14:30", return as is
    if (/^\d{1,2}:\d{2}(:\d{2})?$/.test(timeStr)) {
      return timeStr.substring(0, 5)
    }
    // Otherwise parse as date
    const date = new Date(timeStr)
    return date.toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit', hour12: false })
  } catch {
    return timeStr
  }
}

// ═══════════════════════════════════════════════════════════════════════════
// VOTING HELPERS
// ═══════════════════════════════════════════════════════════════════════════

// Check if option is approve type (by name patterns)
const isApproveOption = (option: VotingOption) => {
  const name = (option.nameAr || option.nameEn || option.name || '').toLowerCase()
  return name.includes('موافق') || name.includes('approve') || name.includes('نعم') || name.includes('yes')
}

// Check if option is reject type (by name patterns)
const isRejectOption = (option: VotingOption) => {
  const name = (option.nameAr || option.nameEn || option.name || '').toLowerCase()
  return name.includes('رفض') || name.includes('reject') || name.includes('لا') || name.includes('no')
}

// Get vote button class based on option type and voted state
const getVoteButtonClass = (option: VotingOption) => {
  const isVoted = props.userVoteOptionId === option.id
  const classes: string[] = []

  if (isApproveOption(option)) {
    classes.push('approve')
  } else if (isRejectOption(option)) {
    classes.push('reject')
  } else {
    classes.push('abstain')
  }

  if (isVoted) {
    classes.push('voted')
  }

  return classes
}

// Voting option label: Arabic in RTL, English in LTR
const votingOptionLabel = (option: VotingOption) => {
  if (isArabic.value) {
    return option.nameAr || option.nameEn || option.name || ''
  }
  return option.nameEn || option.name || option.nameAr || ''
}

// Computed: label for user's vote
const userVoteLabel = computed(() => {
  if (!props.userVoteOptionId) return ''
  const option = props.votingOptions.find(o => o.id === props.userVoteOptionId)
  return option ? votingOptionLabel(option) : ''
})
</script>

<style scoped>
/* Transcript section in Summary modal */
.transcript-section {
  margin-bottom: 16px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  overflow: hidden;
}
.transcript-header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 14px;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
  font-size: 12px;
  font-weight: 700;
  color: #334155;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}
.transcript-label { flex: 1; }
.transcript-badge {
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
  font-size: 10px;
  padding: 2px 8px;
  border-radius: 10px;
  font-weight: 700;
}

/* Chat-like dialog */
.transcript-dialog {
  padding: 14px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  background: #fafbfc;
}
.transcript-message {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.transcript-speaker {
  font-size: 11px;
  font-weight: 700;
  color: #006d4b;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  padding-inline-start: 2px;
}
.transcript-bubble {
  font-size: 13px;
  color: #334155;
  line-height: 1.65;
  padding: 10px 14px;
  background: #fff;
  border-radius: 0 10px 10px 10px;
  border: 1px solid #e8ecf0;
  box-shadow: 0 1px 3px rgba(0,0,0,0.04);
}

/* Footer with action buttons */
.transcript-footer {
  display: flex;
  gap: 8px;
  padding: 10px 14px;
  border-top: 1px solid #e2e8f0;
  background: #f8fafc;
}

/* Combined AI Summary */
.transcript-combined-summary {
  padding: 12px 14px;
  border-top: 1px solid #e2e8f0;
  background: #f0f7ff;
}
.transcript-combined-label {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 11px;
  font-weight: 700;
  color: #2563eb;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  margin-bottom: 8px;
}
.transcript-combined-text {
  font-size: 13px;
  color: #334155;
  line-height: 1.65;
  padding: 10px 14px;
  background: #fff;
  border-radius: 8px;
  border: 1px solid #bfdbfe;
  border-inline-start: 3px solid #3b82f6;
}

/* Action buttons */
.transcript-action-btn {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 6px 12px;
  font-size: 11px;
  font-weight: 600;
  border-radius: 6px;
  border: 1px solid #e2e8f0;
  background: #fff;
  color: #475569;
  cursor: pointer;
  transition: all 0.15s;
  font-family: inherit;
}
.transcript-action-btn:hover {
  border-color: #006d4b;
  color: #006d4b;
}
.transcript-action-btn.fill {
  background: linear-gradient(135deg, #004730 0%, #006d4b 100%);
  color: #fff;
  border: none;
}
.transcript-action-btn.fill:hover {
  box-shadow: 0 2px 8px rgba(0, 109, 75, 0.3);
}

/* Compact unified header */
.panelHeader.compact {
  display: flex !important;
  align-items: center !important;
  justify-content: space-between !important;
  padding: 10px 16px !important;
  background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%) !important;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1) !important;
  min-height: 52px !important;
}

.headerRight {
  display: flex !important;
  align-items: center !important;
  gap: 16px !important;
  flex: 1 !important;
  min-width: 0 !important;
  overflow: visible !important;
}

.headerActions {
  display: flex !important;
  align-items: center !important;
  gap: 10px !important;
  flex-shrink: 0 !important;
  margin-inline-start: 16px !important;
}

.panelHeader.compact .meetingMeta {
  display: flex !important;
  align-items: center !important;
  gap: 10px !important;
  min-width: 0 !important;
  overflow: hidden !important;
}

.panelHeader.compact .badge {
  width: 36px !important;
  height: 36px !important;
  min-width: 36px !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  background: rgba(59, 130, 246, 0.2) !important;
  border-radius: 8px !important;
  color: #60a5fa !important;
  flex-shrink: 0 !important;
}

.panelHeader.compact .meetingName {
  display: flex !important;
  flex-direction: column !important;
  gap: 2px !important;
  min-width: 0 !important;
  overflow: hidden !important;
}

.panelHeader.compact .meetingName strong {
  color: #f1f5f9 !important;
  font-size: 14px !important;
  font-weight: 600 !important;
  white-space: nowrap !important;
  overflow: hidden !important;
  text-overflow: ellipsis !important;
  max-width: 300px !important;
}

.panelHeader.compact .meetingName .sub {
  color: #94a3b8 !important;
  font-size: 11px !important;
  white-space: nowrap !important;
  overflow: hidden !important;
  text-overflow: ellipsis !important;
}

/* Details Dropdown */
.detailsDropdown {
  position: relative;
}

.detailsBtn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  background: rgba(255, 255, 255, 0.06);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  color: #94a3b8;
  font-size: 12px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.detailsBtn:hover {
  background: rgba(255, 255, 255, 0.1);
  color: #e2e8f0;
  border-color: rgba(255, 255, 255, 0.2);
}

.detailsBtn svg.rotated {
  transform: rotate(180deg);
}

.detailsBtn svg {
  transition: transform 0.2s ease;
}

.dropdownContent {
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  min-width: 280px;
  background: #1e2128;
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.4);
  padding: 12px;
  z-index: 100;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

[dir="rtl"] .dropdownContent {
  left: auto;
  right: 0;
}

.detailRow {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 8px;
  transition: background 0.15s ease;
}

.detailRow:hover {
  background: rgba(255, 255, 255, 0.06);
}

.detailRow svg {
  color: #64748b;
  flex-shrink: 0;
}

.detailLabel {
  font-size: 11px;
  color: #64748b;
  min-width: 80px;
}

.detailValue {
  font-size: 13px;
  color: #e2e8f0;
  font-weight: 500;
  margin-right: auto;
}

/* Dropdown transition */
.dropdown-enter-active,
.dropdown-leave-active {
  transition: all 0.2s ease;
}

.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

/* Light theme - Details button */
.meeting-room.light .detailsBtn {
  background: #f1f5f9;
  border: none;
  color: #475569;
  border-radius: 8px;
  font-weight: 600;
}

.meeting-room.light .detailsBtn:hover {
  background: #e2e8f0;
  color: #1a1a1a;
}

/* Light theme - Timer pill */
.meeting-room.light .pill.timer {
  background: #006d4b !important;
  border: none !important;
  border-radius: 8px !important;
  padding: 6px 14px !important;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15) !important;
}

.meeting-room.light .pill.timer .mono {
  color: #ffffff !important;
  font-weight: 700 !important;
  font-size: 14px !important;
  letter-spacing: 0.05em !important;
}

.meeting-room.light .pill.timer svg,
.meeting-room.light .pill.timer .w-3 {
  color: #ffffff !important;
}

.meeting-room.light .dropdownContent.detailsCard {
  background: #ffffff;
  border: none;
  border-radius: 12px;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
  padding: 8px;
  min-width: 300px;
}

.meeting-room.light .detailRow {
  background: transparent;
  border-radius: 8px;
  padding: 10px 12px;
  gap: 12px;
}

.meeting-room.light .detailRow:hover {
  background: #f8faf9;
}

.meeting-room.light .detailIcon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.meeting-room.light .detailIcon.ref { background: #eff6ff; color: #3b82f6; }
.meeting-room.light .detailIcon.loc { background: #fef2f2; color: #ef4444; }
.meeting-room.light .detailIcon.time { background: #f0faf7; color: #006d4b; }

.meeting-room.light .detailText {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.meeting-room.light .detailLabel {
  color: #94a3b8;
  font-size: 11px;
  font-weight: 500;
}

.meeting-room.light .detailValue {
  color: #1a1a1a;
  font-size: 13px;
  font-weight: 600;
}

.pill.timer {
  display: flex !important;
  align-items: center !important;
  gap: 6px !important;
  padding: 6px 12px !important;
  background: var(--card2) !important;
  border-radius: 20px !important;
  color: var(--text) !important;
  font-size: 13px !important;
}

.pill.timer .mono {
  font-family: 'SF Mono', Monaco, monospace !important;
  color: #60a5fa !important;
  font-weight: 500 !important;
}

/* Header action buttons - unique class names to avoid conflicts */
.headerBtn {
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
  gap: 6px !important;
  padding: 8px 14px !important;
  font-size: 13px !important;
  font-weight: 500 !important;
  border-radius: 6px !important;
  border: none !important;
  cursor: pointer !important;
  transition: all 0.15s ease !important;
  white-space: nowrap !important;
}

.headerBtn.startBtn {
  background: #22c55e !important;
  color: #fff !important;
}

.headerBtn.startBtn:hover {
  background: #16a34a !important;
}

.headerBtn.endBtn {
  background: #ef4444 !important;
  color: #fff !important;
}

.headerBtn.endBtn:hover {
  background: #dc2626 !important;
}

.headerBtn.themeBtn {
  width: 36px !important;
  height: 36px !important;
  padding: 0 !important;
  background: rgba(251, 191, 36, 0.15) !important;
  color: #fbbf24 !important;
  border-radius: 8px !important;
  border: 1px solid rgba(251, 191, 36, 0.3) !important;
}

.headerBtn.themeBtn:hover {
  background: rgba(251, 191, 36, 0.25) !important;
  border-color: rgba(251, 191, 36, 0.5) !important;
}

.headerBtn.closeBtn {
  width: 34px !important;
  height: 34px !important;
  padding: 0 !important;
  background: rgba(239, 68, 68, 0.12) !important;
  color: #f87171 !important;
  border: 1px solid rgba(239, 68, 68, 0.25) !important;
  border-radius: 8px !important;
}

.headerBtn.closeBtn:hover {
  background: rgba(239, 68, 68, 0.25) !important;
  color: #fca5a5 !important;
  border-color: rgba(239, 68, 68, 0.5) !important;
}

.headerBtn:disabled {
  opacity: 0.5 !important;
  cursor: not-allowed !important;
}

/* RTL icon flip */
.rtl-flip {
  transform: scaleX(-1) !important;
}

/* Empty state */
.emptyState {
  position: absolute !important;
  top: 0 !important;
  left: 0 !important;
  right: 0 !important;
  bottom: 0 !important;
  display: flex !important;
  flex-direction: column !important;
  align-items: center !important;
  justify-content: center !important;
  color: #64748b !important;
  text-align: center !important;
  gap: 12px !important;
}

.emptyState .emptyIcon {
  color: #475569 !important;
  opacity: 0.5 !important;
}

.emptyState h3 {
  font-size: 18px !important;
  font-weight: 600 !important;
  color: #475569 !important;
  margin: 0 !important;
}

.emptyState p {
  font-size: 14px !important;
  color: #64748b !important;
  margin: 0 !important;
}

/* Notes Layout */
.detailsBody.notesLayout {
  display: flex !important;
  gap: 12px !important;
  grid-template-columns: none !important;
}

.notesSection {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;
  background: var(--card2, #22252c);
  border: 1px solid var(--border, rgba(255,255,255,0.1));
  border-radius: 6px;
  padding: 10px;
}

.rightSection {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;
}

.notesHeader {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: var(--muted, #b0b0b0);
  padding-bottom: 6px;
  border-bottom: 1px solid var(--border, rgba(255,255,255,0.1));
}

.noteCount {
  background: rgba(0, 217, 126, 0.2);
  color: var(--accent, #00d97e);
  padding: 2px 6px;
  border-radius: 10px;
  font-size: 10px;
  font-weight: 600;
}

.notesList {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 120px;
  min-height: 60px;
}

.notesList::-webkit-scrollbar { width: 4px; }
.notesList::-webkit-scrollbar-track { background: transparent; }
.notesList::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.15); border-radius: 2px; }

.noNotes {
  color: var(--muted2, #909090);
  font-size: 11px;
  text-align: center;
  padding: 16px 8px;
}

.noteItem {
  background: var(--card, #1c1e24);
  border: 1px solid var(--border, rgba(255,255,255,0.1));
  border-radius: 6px;
  padding: 8px;
  transition: all 0.15s ease;
}

.noteItem.editing {
  border-color: var(--accent, #00d97e);
  background: rgba(0, 217, 126, 0.05);
}

.noteTop {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 4px;
}

.noteAuthor {
  display: flex;
  align-items: center;
  gap: 6px;
  min-width: 0;
}

.authorName {
  font-size: 11px;
  font-weight: 600;
  color: var(--text, #fff);
}

.noteDate {
  font-size: 10px;
  color: var(--muted2, #909090);
}

.noteBadges {
  display: flex;
  align-items: center;
  gap: 4px;
}

.visibilityBadge {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 9px;
  font-weight: 600;
}

.visibilityBadge.public {
  background: rgba(0, 217, 126, 0.15);
  color: var(--accent, #00d97e);
}

.visibilityBadge.private {
  background: rgba(245, 158, 11, 0.15);
  color: #fbbf24;
}

.iconBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 22px;
  border: none;
  background: transparent;
  color: var(--muted, #b0b0b0);
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.iconBtn:hover {
  background: var(--card3, #2a2d35);
  color: var(--text, #fff);
}

.iconBtn.danger:hover {
  background: rgba(255, 107, 107, 0.15);
  color: var(--danger, #ff6b6b);
}

.noteText {
  font-size: 12px;
  color: var(--text2, #e0e0e0);
  line-height: 1.4;
  word-wrap: break-word;
}

.noteForm {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding-top: 8px;
  border-top: 1px solid var(--border, rgba(255,255,255,0.1));
}

.noteInputRow {
  display: flex;
  gap: 8px;
  align-items: center;
}

.noteInput {
  flex: 1;
  padding: 8px 10px;
  border: 1px solid var(--border, rgba(255,255,255,0.1));
  background: var(--card, #1c1e24);
  border-radius: 6px;
  color: var(--text, #fff);
  font-size: 12px;
  outline: none;
  font-family: inherit;
}

.noteInput:focus {
  border-color: var(--accent, #00d97e);
}

.noteInput::placeholder {
  color: var(--muted2, #909090);
}

.publicToggle {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 6px 10px;
  border: 1px solid var(--border, rgba(255,255,255,0.1));
  border-radius: 6px;
  background: var(--card, #1c1e24);
  cursor: pointer;
  font-size: 11px;
  color: var(--muted, #b0b0b0);
  transition: all 0.15s ease;
  white-space: nowrap;
}

.publicToggle:has(input:checked) {
  border-color: var(--accent, #00d97e);
  color: var(--accent, #00d97e);
  background: rgba(0, 217, 126, 0.1);
}

.publicToggle input {
  display: none;
}

.noteActions {
  display: flex;
  gap: 6px;
  justify-content: flex-end;
}

/* Collapse Toggle Bar */
.collapseToggleBar {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 12px;
  padding: 6px 16px;
  cursor: pointer;
  flex-shrink: 0;
  transition: all 0.15s ease;
}

.collapseToggleBar:hover {
  background: rgba(255, 255, 255, 0.02);
}

.toggleLine {
  flex: 1;
  height: 1px;
  background: linear-gradient(90deg, transparent, var(--border, rgba(255,255,255,0.15)), transparent);
  max-width: 120px;
}

.toggleArrow {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  color: var(--muted2, #909090);
  transition: all 0.2s ease;
}

.collapseToggleBar:hover .toggleArrow {
  color: var(--accent, #00d97e);
  transform: scale(1.1);
}

.collapseToggleBar:hover .toggleLine {
  background: linear-gradient(90deg, transparent, var(--accent, #00d97e), transparent);
}

/* Light theme - toggle bar */
.meeting-room.light .collapseToggleBar {
  padding: 4px 16px;
  margin: 2px 0;
}

.meeting-room.light .collapseToggleBar:hover {
  background: #f8faf9;
}

.meeting-room.light .toggleLine {
  background: linear-gradient(90deg, transparent, #d1d5db, transparent);
  max-width: 140px;
}

.meeting-room.light .toggleArrow {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: #f1f5f9;
  color: #64748b;
  border: 1px solid #e0e0e0;
}

.meeting-room.light .collapseToggleBar:hover .toggleArrow {
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
  border-color: #006d4b;
}

.meeting-room.light .collapseToggleBar:hover .toggleLine {
  background: linear-gradient(90deg, transparent, #006d4b, transparent);
}

/* Summary Button */
.headerBtn.summaryBtn {
  background: rgba(139, 92, 246, 0.15) !important;
  color: #a78bfa !important;
  border: 1px solid rgba(139, 92, 246, 0.3) !important;
}

.headerBtn.summaryBtn:hover {
  background: rgba(139, 92, 246, 0.25) !important;
  border-color: rgba(139, 92, 246, 0.5) !important;
}

/* Summary Modal */
.summaryModalOverlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10002;
  padding: 20px;
}

.summaryModal {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: 16px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.4);
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.summaryModalHeader {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  background: var(--header-bg);
  color: #fff;
}

.summaryTitle {
  display: flex;
  align-items: center;
  gap: 10px;
  color: #fff;
}

.summaryTitle h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #fff;
}

.closeModalBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: none;
  background: rgba(255, 255, 255, 0.15);
  color: #fff;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.closeModalBtn:hover {
  background: rgba(255, 255, 255, 0.25);
}

.summaryModalBody {
  padding: 20px;
  flex: 1;
  overflow-y: auto;
  scrollbar-width: thin;
  scrollbar-color: var(--border) transparent;
}

.summaryModalBody::-webkit-scrollbar {
  width: 6px;
}

.summaryModalBody::-webkit-scrollbar-track {
  background: transparent;
}

.summaryModalBody::-webkit-scrollbar-thumb {
  background: var(--border);
  border-radius: 3px;
}

.summaryModalBody::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.25);
}

.summaryTextarea {
  width: 100%;
  min-height: 200px;
  padding: 14px;
  border: 1px solid var(--border);
  background: var(--input-bg);
  border-radius: 8px;
  color: var(--text);
  font-size: 14px;
  line-height: 1.6;
  resize: vertical;
  outline: none;
  font-family: inherit;
}

.summaryTextarea:focus {
  border-color: var(--accent);
}

.summaryTextarea::placeholder {
  color: var(--muted);
}

.summaryModalFooter {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 10px;
  padding: 16px 20px;
  border-top: 1px solid var(--border);
  background: var(--card2);
}

.summaryModalFooter .btn {
  padding: 10px 18px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.summaryModalFooter .btn.ghost {
  background: transparent;
  border: 1px solid var(--border);
  color: var(--muted);
}

.summaryModalFooter .btn.ghost:hover {
  background: rgba(255, 255, 255, 0.05);
  color: #f1f5f9;
}

.summaryModalFooter .btn.primary {
  background: #8b5cf6;
  border: none;
  color: #fff;
}

.summaryModalFooter .btn.primary:hover {
  background: #7c3aed;
}

.summaryModalFooter .btn.primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Comments Button */
.headerBtn.commentsBtn {
  background: rgba(59, 130, 246, 0.15) !important;
  color: #60a5fa !important;
  border: 1px solid rgba(59, 130, 246, 0.3) !important;
  position: relative;
}

.headerBtn.commentsBtn:hover {
  background: rgba(59, 130, 246, 0.25) !important;
  border-color: rgba(59, 130, 246, 0.5) !important;
}

.btnBadge {
  background: #3b82f6;
  color: #fff;
  padding: 2px 6px;
  border-radius: 10px;
  font-size: 10px;
  font-weight: 700;
  margin-right: 4px;
}

/* Expand button for inline comments */
.expandBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 22px;
  border: none;
  background: rgba(255, 255, 255, 0.05);
  color: var(--muted, #b0b0b0);
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.15s ease;
  margin-right: auto;
}

.expandBtn:hover {
  background: rgba(59, 130, 246, 0.15);
  color: #60a5fa;
}

/* Agenda Summary Button in controls */
.btn.agendaSummaryBtn {
  background: rgba(34, 197, 94, 0.15) !important;
  color: #4ade80 !important;
  border: 1px solid rgba(34, 197, 94, 0.3) !important;
}

.btn.agendaSummaryBtn:hover:not(:disabled) {
  background: rgba(34, 197, 94, 0.25) !important;
  border-color: rgba(34, 197, 94, 0.5) !important;
}

.btn.agendaSummaryBtn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

/* Agenda Summary Modal variations */
.agendaSummaryModal .summaryModalHeader {
  background: linear-gradient(135deg, #14532d 0%, #052e16 100%);
}

.agendaSummaryTitle {
  color: #4ade80 !important;
}

.agendaSummaryTitle h3 {
  color: #f1f5f9 !important;
}

.agendaName {
  font-size: 12px;
  color: #86efac;
  background: rgba(34, 197, 94, 0.15);
  padding: 4px 10px;
  border-radius: 4px;
  margin-right: 10px;
}

.summaryModalFooter .btn.primary.agenda {
  background: #22c55e !important;
}

.summaryModalFooter .btn.primary.agenda:hover {
  background: #16a34a !important;
}

/* Recommendations Button in header - ORANGE to match modal */
.headerBtn.recommendationsBtn {
  background: rgba(245, 158, 11, 0.15) !important;
  color: #fbbf24 !important;
  border: 1px solid rgba(245, 158, 11, 0.3) !important;
  position: relative;
}

.headerBtn.recommendationsBtn:hover {
  background: rgba(245, 158, 11, 0.25) !important;
  border-color: rgba(245, 158, 11, 0.5) !important;
}

.btnBadge.recBadge {
  background: #f59e0b;
}

/* Voting Button in header - GREEN to match modal */
.headerBtn.votingBtn {
  background: rgba(34, 197, 94, 0.15) !important;
  color: #4ade80 !important;
  border: 1px solid rgba(34, 197, 94, 0.3) !important;
  position: relative;
}

.headerBtn.votingBtn:hover {
  background: rgba(34, 197, 94, 0.25) !important;
  border-color: rgba(34, 197, 94, 0.5) !important;
}

.btnBadge.voteBadge {
  background: #22c55e;
}

/* 3-Column Workspace Layout */
.detailsBody.threeColLayout {
  display: grid !important;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 12px;
  padding: 12px;
}

/* 2-Column Workspace Layout (for non-organizers) */
.detailsBody.twoColLayout {
  display: grid !important;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
  padding: 12px;
}

.workspaceSection {
  display: flex;
  flex-direction: column;
  background: var(--card2, #22252c);
  border: 1px solid var(--border, rgba(255,255,255,0.1));
  border-radius: 8px;
  overflow: hidden;
  min-height: 200px;
  max-height: 280px;
}

.sectionHeader {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 10px 12px;
  font-size: 12px;
  font-weight: 600;
  color: var(--text, #fff);
  border-bottom: 1px solid var(--border, rgba(255,255,255,0.1));
  background: rgba(255,255,255,0.02);
}

.sectionExpandBtn {
  width: 24px;
  height: 24px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  cursor: pointer;
  background: rgba(255,255,255,0.08);
  color: var(--muted2, #909090);
  transition: all 0.15s;
  flex-shrink: 0;
}

.sectionExpandBtn:hover {
  background: rgba(255,255,255,0.15);
  color: var(--accent, #006d4b);
}

.sectionCount {
  background: rgba(59, 130, 246, 0.2);
  color: #60a5fa;
  padding: 2px 7px;
  border-radius: 10px;
  font-size: 10px;
  font-weight: 700;
}

.sectionCount.recCount {
  background: rgba(245, 158, 11, 0.2);
  color: #fbbf24;
}

.sectionList {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.sectionList::-webkit-scrollbar { width: 4px; }
.sectionList::-webkit-scrollbar-track { background: transparent; }
.sectionList::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.15); border-radius: 2px; }

.emptySection {
  color: var(--muted2, #909090);
  font-size: 11px;
  text-align: center;
  padding: 20px 8px;
}

.listItem {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 8px;
  background: var(--card, #1c1e24);
  border: 1px solid var(--border, rgba(255,255,255,0.08));
  border-radius: 6px;
  padding: 8px 10px;
  transition: all 0.15s ease;
}

.listItem.editing {
  border-color: var(--accent, #00d97e);
  background: rgba(0, 217, 126, 0.05);
}

.listItem.recItem {
  border-right: 3px solid #22c55e;
}

.itemContent {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.itemText {
  font-size: 12px;
  color: var(--text2, #e0e0e0);
  line-height: 1.4;
  word-wrap: break-word;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.itemMeta {
  font-size: 10px;
  color: var(--muted2, #909090);
}

.itemActions {
  display: flex;
  gap: 4px;
  flex-shrink: 0;
}

.sectionForm {
  padding: 8px;
  border-top: 1px solid var(--border, rgba(255,255,255,0.1));
  background: rgba(0,0,0,0.15);
}

.sectionForm.disabled {
  opacity: 0.6;
}

.sectionForm.disabled .sectionInput {
  cursor: not-allowed;
  background: rgba(0,0,0,0.1);
}

.sectionForm.disabled .btn {
  cursor: not-allowed;
  opacity: 0.5;
}

.publicToggle.compact.disabled {
  cursor: not-allowed;
  opacity: 0.5;
}

.sectionInput {
  width: 100%;
  padding: 8px 10px;
  border: 1px solid var(--border, rgba(255,255,255,0.1));
  background: var(--card, #1c1e24);
  border-radius: 6px;
  color: var(--text, #fff);
  font-size: 12px;
  outline: none;
  font-family: inherit;
}

.sectionInput:focus {
  border-color: var(--accent, #00d97e);
}

.sectionInput::placeholder {
  color: var(--muted2, #909090);
}

.formActions {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 6px;
}

.publicToggle.compact {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 26px;
  height: 26px;
  padding: 0;
  border: 1px solid var(--border, rgba(255,255,255,0.1));
  border-radius: 4px;
  background: var(--card, #1c1e24);
  cursor: pointer;
  color: var(--muted, #b0b0b0);
  transition: all 0.15s ease;
}

.publicToggle.compact:has(input:checked) {
  border-color: var(--accent, #00d97e);
  color: var(--accent, #00d97e);
  background: rgba(0, 217, 126, 0.1);
}

.publicToggle.compact input {
  display: none;
}

.btn.tiny {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 26px;
  height: 26px;
  padding: 0;
  border-radius: 4px;
  border: none;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn.tiny.success {
  background: rgba(34, 197, 94, 0.2);
  color: #4ade80;
}

.btn.tiny.success:hover:not(:disabled) {
  background: rgba(34, 197, 94, 0.3);
}

.btn.tiny.warning {
  background: rgba(245, 158, 11, 0.2);
  color: #fbbf24;
}

.btn.tiny.warning:hover:not(:disabled) {
  background: rgba(245, 158, 11, 0.3);
}

.btn.tiny.ghost {
  background: rgba(255,255,255,0.05);
  color: var(--muted, #b0b0b0);
}

.btn.tiny.ghost:hover {
  background: rgba(255,255,255,0.1);
  color: var(--text, #fff);
}

.btn.tiny:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

/* ── Comment items — blue accent to match comments section ── */
.listItem.commentItem {
  border-inline-start: 3px solid #3b82f6;
}

.listItem.commentItem.editing {
  border-inline-start-color: #3b82f6;
  background: rgba(59, 130, 246, 0.05);
}

.commentMeta {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 2px;
}

.commentBadge {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  padding: 2px 7px;
  border-radius: 4px;
  font-size: 9px;
  font-weight: 700;
  letter-spacing: 0.02em;
}

.commentBadge.public {
  background: #004730;
  color: #ffffff;
}

.commentBadge.private {
  background: #92400e;
  color: #ffffff;
}

/* ── Comment form ── */
.commentForm {
  padding: 10px 12px;
  border-top: 1px solid var(--border, rgba(255,255,255,0.1));
  background: rgba(0,0,0,0.12);
}

.commentForm.disabled {
  opacity: 0.5;
}

.commentInputRow {
  display: flex;
  gap: 6px;
  align-items: center;
}

.commentInput {
  flex: 1;
  min-width: 0;
}

.commentSendBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 30px;
  height: 30px;
  flex-shrink: 0;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  background: rgba(59, 130, 246, 0.2);
  color: #60a5fa;
  transition: all 0.15s ease;
}

.commentSendBtn:hover:not(:disabled) {
  background: rgba(59, 130, 246, 0.35);
  color: #93c5fd;
}

.commentSendBtn.editing {
  background: rgba(245, 158, 11, 0.2);
  color: #fbbf24;
}

.commentSendBtn.editing:hover:not(:disabled) {
  background: rgba(245, 158, 11, 0.35);
  color: #fcd34d;
}

.commentSendBtn:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.commentFormFooter {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 6px;
}

.commentVisibilityToggle {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 3px 8px;
  border-radius: 4px;
  font-size: 10px;
  font-weight: 600;
  cursor: pointer;
  border: 1px solid var(--border, rgba(255,255,255,0.1));
  background: transparent;
  color: var(--muted, #b0b0b0);
  transition: all 0.15s ease;
}

.commentVisibilityToggle input { display: none; }

.commentVisibilityToggle.checked {
  border-color: rgba(59, 130, 246, 0.3);
  background: rgba(59, 130, 246, 0.1);
  color: #60a5fa;
}

.commentVisibilityToggle.disabled {
  cursor: not-allowed;
  opacity: 0.5;
}

.commentCancelBtn {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  padding: 3px 8px;
  border: 1px solid rgba(239, 68, 68, 0.25);
  border-radius: 4px;
  background: transparent;
  color: #f87171;
  font-size: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
  font-family: inherit;
}

.commentCancelBtn:hover {
  background: rgba(239, 68, 68, 0.1);
  border-color: rgba(239, 68, 68, 0.4);
}

/* Recommendations add button — matches recommendation card amber accent */
.addRecBtnCompact {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 5px 12px;
  border-radius: 6px;
  border: 1px solid rgba(245, 158, 11, 0.25);
  font-size: 11px;
  font-weight: 600;
  cursor: pointer;
  background: rgba(245, 158, 11, 0.1);
  color: #fbbf24;
  transition: all 0.2s ease;
}

.addRecBtnCompact:hover {
  background: rgba(245, 158, 11, 0.2);
  border-color: rgba(245, 158, 11, 0.45);
  color: #fcd34d;
}

/* Add comment button — blue accent to match comments section */
.addCommentBtnCompact {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 5px 12px;
  border-radius: 6px;
  border: 1px solid rgba(59, 130, 246, 0.25);
  font-size: 11px;
  font-weight: 600;
  cursor: pointer;
  background: rgba(59, 130, 246, 0.1);
  color: #60a5fa;
  transition: all 0.2s ease;
  font-family: inherit;
}

.addCommentBtnCompact:hover {
  background: rgba(59, 130, 246, 0.2);
  border-color: rgba(59, 130, 246, 0.45);
  color: #93c5fd;
}

/* LEGACY - keep for dark theme */
.addRecBtn {
  width: 100%;
  justify-content: center;
  background: rgba(245, 158, 11, 0.15) !important;
  color: #fbbf24 !important;
  border: 1px dashed rgba(245, 158, 11, 0.4) !important;
}

.addRecBtn:hover {
  background: rgba(245, 158, 11, 0.25) !important;
  border-color: rgba(245, 158, 11, 0.6) !important;
}

/* Recommendations Section - ORANGE to match recommendations theme */
.recommendationsSection .sectionHeader {
  color: #fbbf24;
}

/* Voting Section */
.votingSection .sectionHeader {
  color: #006d4b;
}

.resultsBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  border: none;
  background: rgba(0, 109, 75, 0.15);
  color: #006d4b;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.15s ease;
  margin-inline-start: auto;
}

.resultsBtn:hover {
  background: rgba(0, 109, 75, 0.25);
  color: #63a58f;
}

.votingContent {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 10px;
  gap: 8px;
  overflow: hidden;
}

/* ── Compact Vote Card (PK ONE design) ── */
.voteOptionsList {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 6px;
  max-height: 160px;
  overflow-y: auto;
}

.voteOptionsList::-webkit-scrollbar { width: 4px; }
.voteOptionsList::-webkit-scrollbar-track { background: transparent; }
.voteOptionsList::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.15); border-radius: 2px; }

/* When only 1 or 2 options, let them span full width */
.voteOptionsList:has(> :nth-child(2):last-child) { grid-template-columns: repeat(2, 1fr); }
.voteOptionsList:has(> :first-child:last-child) { grid-template-columns: 1fr; }

.voteItem {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 9px 12px;
  border-radius: 8px;
  border: 1px solid rgba(255,255,255,0.1);
  background: var(--card, #1c1e24);
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  font-family: inherit;
  width: 100%;
  text-align: start;
  min-width: 0;
}

.voteItem:hover:not(:disabled):not(.active) {
  border-color: #006d4b;
  background: rgba(0, 109, 75, 0.06);
}

.voteItem:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

/* Selected state — deep dark with teal glow */
.voteItem.active {
  background: #0d151c;
  border-color: #0d151c;
  box-shadow: 0 4px 12px rgba(0, 109, 75, 0.2);
}

.voteItemText {
  font-size: 12px;
  font-weight: 700;
  color: var(--text, #e4e4e7);
  transition: color 0.2s;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  min-width: 0;
}

.voteItem.active .voteItemText {
  color: #ffffff;
}

/* Circular indicator */
.voteIndicator {
  width: 18px;
  height: 18px;
  min-width: 18px;
  border-radius: 50%;
  border: 2px solid rgba(255,255,255,0.15);
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
  overflow: hidden;
}

.voteIndicatorCheck {
  font-size: 12px !important;
  width: 12px !important;
  height: 12px !important;
  color: transparent;
  transition: color 0.2s;
}

.voteItem.active .voteIndicator {
  border-color: #006d4b;
  background: #006d4b;
}

.voteItem.active .voteIndicatorCheck {
  color: #ffffff;
}

.noVotingOptions {
  text-align: center;
  padding: 16px 8px;
  color: var(--muted2, #909090);
  font-size: 11px;
}

.enableVoteBtn {
  width: 100%;
  justify-content: center;
  background: rgba(245, 158, 11, 0.15) !important;
  color: #fbbf24 !important;
  border: 1px solid rgba(245, 158, 11, 0.3) !important;
  margin-top: auto;
}

.enableVoteBtn:hover:not(:disabled) {
  background: rgba(245, 158, 11, 0.25) !important;
  border-color: rgba(245, 158, 11, 0.5) !important;
}

.enableVoteBtn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

/* Responsive: Stack on smaller screens */
@media (max-width: 1200px) {
  .detailsBody.threeColLayout {
    grid-template-columns: 1fr 1fr;
  }

  .detailsBody.threeColLayout .votingSection {
    grid-column: span 2;
  }
}

@media (max-width: 900px) {
  .detailsBody.threeColLayout,
  .detailsBody.twoColLayout {
    grid-template-columns: 1fr;
  }

  .detailsBody.threeColLayout .votingSection,
  .detailsBody.twoColLayout .votingSection {
    grid-column: span 1;
  }
}

/* Saved Minutes State */
.savedMinutesState {
  position: absolute !important;
  top: 0 !important;
  left: 0 !important;
  right: 0 !important;
  bottom: 0 !important;
  display: flex !important;
  flex-direction: column !important;
  z-index: 10 !important;
  background: transparent !important;
}

.savedMinutesState .pdfViewerFull {
  flex: 1 !important;
  width: 100% !important;
  min-height: 0 !important;
  border-radius: 12px !important;
  overflow: hidden !important;
}

/* Light theme - savedMinutesState */
.meeting-room.light .savedMinutesState {
  background: #f1f5f9 !important;
}

.meeting-room.light .savedMinutesState .pdfViewerFull {
  border: 1px solid #e4e4e7 !important;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08) !important;
}

/* Meeting Finalized Banner */
.meetingFinalizedBanner {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 12px 20px;
  background: linear-gradient(135deg, rgba(34, 197, 94, 0.15), rgba(34, 197, 94, 0.08));
  border-bottom: 1px solid rgba(34, 197, 94, 0.3);
  color: #4ade80;
  font-size: 14px;
  font-weight: 600;
}

.meetingFinalizedBanner svg {
  color: #22c55e;
}

/* Minutes Loading State */
.minutesLoadingState {
  position: absolute !important;
  top: 0 !important;
  left: 0 !important;
  right: 0 !important;
  bottom: 0 !important;
  display: flex !important;
  flex-direction: column !important;
  align-items: center !important;
  justify-content: center !important;
  padding: 40px !important;
  background: linear-gradient(135deg, rgba(59, 130, 246, 0.05) 0%, transparent 100%) !important;
  z-index: 10 !important;
}

.minutesLoadingCard {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  width: 100%;
  max-width: 400px;
  padding: 48px;
  background: #ffffff;
  border: 1px solid #e0e0e0;
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}

.loadingSpinner {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 64px;
  height: 64px;
  background: rgba(0, 109, 75, 0.1);
  border: none;
  border-radius: 16px;
  margin-bottom: 24px;
  color: #006d4b;
}

.spinnerIcon {
  animation: spin 1.5s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.minutesLoadingCard h3 {
  font-size: 18px;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0 0 8px 0;
}

.minutesLoadingCard p {
  font-size: 13px;
  color: #6b7280;
  margin: 0;
}

/* Minutes Generation Options */
.minutesOptionsState {
  position: absolute !important;
  top: 0 !important;
  left: 0 !important;
  right: 0 !important;
  bottom: 0 !important;
  display: flex !important;
  flex-direction: column !important;
  align-items: center !important;
  justify-content: center !important;
  padding: 40px !important;
  background: linear-gradient(135deg, rgba(16, 185, 129, 0.05) 0%, transparent 100%) !important;
  z-index: 10 !important;
}

.minutesOptionsCard {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  width: 100%;
  max-width: 600px;
  padding: 40px 48px;
  background: #ffffff;
  border: 1px solid #e0e0e0;
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}

.minutesIcon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 64px;
  height: 64px;
  background: rgba(0, 109, 75, 0.1);
  border: none;
  border-radius: 16px;
  margin-bottom: 20px;
  color: #006d4b;
}

.minutesOptionsCard h3 {
  font-size: 20px;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0 0 8px 0;
}

.minutesOptionsCard p {
  font-size: 14px;
  color: #6b7280;
  margin: 0 0 32px 0;
}

.minutesActions {
  display: flex;
  flex-direction: row;
  gap: 16px;
  width: 100%;
}

.minutesBtn {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  flex: 1;
  padding: 24px 20px;
  border-radius: 16px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: all 0.25s ease;
  text-align: center;
  min-height: 120px;
}

.minutesBtn span {
  font-size: 14px;
  font-weight: 600;
}

.minutesBtn small {
  font-size: 11px;
  opacity: 0.7;
  line-height: 1.3;
}

.minutesBtn.upload {
  background: #f8fafc;
  border: 1px solid #e0e0e0;
  color: #475569;
}

.minutesBtn.upload:hover {
  background: #fff;
  border-color: #3b82f6;
  color: #2563eb;
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(59, 130, 246, 0.15);
}

.minutesBtn.upload .w-6 { color: #3b82f6; }

.minutesBtn.generate {
  background: linear-gradient(135deg, #006d4b 0%, #006d4b 100%);
  border: none;
  color: #fff;
}

.minutesBtn.generate:hover {
  opacity: 0.9;
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 109, 75, 0.25);
}

@media (max-width: 600px) {
  .minutesActions {
    flex-direction: column;
  }

  .minutesBtn {
    min-height: auto;
    padding: 16px;
  }
}

/* Final MOM Styles */
.finalMomState {
  background: linear-gradient(135deg, rgba(139, 92, 246, 0.05) 0%, transparent 100%) !important;
}

.finalMomCard {
  border-color: rgba(139, 92, 246, 0.2) !important;
}

.finalIcon {
  background: linear-gradient(135deg, rgba(139, 92, 246, 0.2) 0%, rgba(124, 58, 237, 0.1) 100%) !important;
  border-color: rgba(139, 92, 246, 0.3) !important;
  color: #a78bfa !important;
}

.finalLoadingState {
  background: linear-gradient(135deg, rgba(139, 92, 246, 0.05) 0%, transparent 100%) !important;
}

.finalSpinner {
  background: linear-gradient(135deg, rgba(139, 92, 246, 0.2) 0%, rgba(124, 58, 237, 0.1) 100%) !important;
  border-color: rgba(139, 92, 246, 0.3) !important;
  color: #a78bfa !important;
}

.minutesBtn.upload.final {
  background: rgba(139, 92, 246, 0.08);
  border-color: rgba(139, 92, 246, 0.25);
  color: #a78bfa;
}

.minutesBtn.upload.final:hover {
  background: rgba(139, 92, 246, 0.15);
  border-color: rgba(139, 92, 246, 0.5);
  transform: translateY(-3px);
  box-shadow: 0 12px 32px rgba(139, 92, 246, 0.2);
}

.minutesBtn.generate.final {
  background: rgba(236, 72, 153, 0.08);
  border-color: rgba(236, 72, 153, 0.25);
  color: #f472b6;
}

.minutesBtn.generate.final:hover {
  background: rgba(236, 72, 153, 0.15);
  border-color: rgba(236, 72, 153, 0.5);
  transform: translateY(-3px);
  box-shadow: 0 12px 32px rgba(236, 72, 153, 0.2);
}

/* Final MOM Action Bar (below initial MOM PDF) */
.finalMomActionBar {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 16px 20px;
  background: linear-gradient(135deg, rgba(139, 92, 246, 0.1) 0%, rgba(124, 58, 237, 0.05) 100%);
  border-top: 1px solid rgba(139, 92, 246, 0.2);
  flex-shrink: 0;
}

.finalMomHeader {
  display: flex;
  align-items: center;
  gap: 10px;
  color: #a78bfa;
  font-size: 14px;
  font-weight: 600;
}

.finalMomHeader svg {
  color: #a78bfa;
}

.finalMomActions {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.finalMomBtn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 12px 20px;
  border-radius: 10px;
  border: 2px solid transparent;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.finalMomBtn span {
  white-space: nowrap;
}

.finalMomBtn.upload {
  background: rgba(139, 92, 246, 0.1);
  border-color: rgba(139, 92, 246, 0.3);
  color: #a78bfa;
}

.finalMomBtn.upload:hover {
  background: rgba(139, 92, 246, 0.2);
  border-color: rgba(139, 92, 246, 0.5);
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(139, 92, 246, 0.2);
}

.finalMomBtn.generate {
  background: rgba(236, 72, 153, 0.1);
  border-color: rgba(236, 72, 153, 0.3);
  color: #f472b6;
}

.finalMomBtn.generate:hover {
  background: rgba(236, 72, 153, 0.2);
  border-color: rgba(236, 72, 153, 0.5);
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(236, 72, 153, 0.2);
}

.finalMomBtn.regenerate {
  background: rgba(59, 130, 246, 0.1);
  border-color: rgba(59, 130, 246, 0.3);
  color: #60a5fa;
}

.finalMomBtn.regenerate:hover:not(:disabled) {
  background: rgba(59, 130, 246, 0.2);
  border-color: rgba(59, 130, 246, 0.5);
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(59, 130, 246, 0.2);
}

.finalMomBtn.regenerate:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.finalMomBtn.approval {
  background: rgba(16, 185, 129, 0.1);
  border-color: rgba(16, 185, 129, 0.3);
  color: #34d399;
}

.finalMomBtn.approval:hover {
  background: rgba(16, 185, 129, 0.2);
  border-color: rgba(16, 185, 129, 0.5);
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(16, 185, 129, 0.2);
}

.spinIcon {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

@media (max-width: 600px) {
  .finalMomActionBar {
    padding: 12px;
  }

  .finalMomHeader {
    font-size: 12px;
    flex-wrap: wrap;
  }

  .finalMomActions {
    flex-direction: column;
  }

  .finalMomBtn {
    width: 100%;
    justify-content: center;
  }
}

/* ═══════════════════════════════════════════════════════════════════════════
   INLINE TOOLBAR ACTIONS (in PDF toolbar)
   ═══════════════════════════════════════════════════════════════════════════ */

.toolbarStatusBadge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 3px 10px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
  margin-inline-start: 10px;
}

.toolbarStatusBadge.finalized {
  background: linear-gradient(135deg, #22c55e 0%, #16a34a 100%);
  color: #fff;
  box-shadow: 0 2px 8px rgba(34, 197, 94, 0.3);
}

.toolbarStatusBadge.pending {
  background: rgba(245, 158, 11, 0.15);
  color: #fbbf24;
  border: 1px solid rgba(245, 158, 11, 0.3);
}

.toolbarActionGroup {
  display: flex;
  align-items: center;
  gap: 6px;
}

.inlineVersionSelector {
  position: relative;
}

.inlineVersionBtn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 5px 10px;
  background: rgba(255, 255, 255, 0.08);
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-radius: 6px;
  color: #e2e8f0;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.inlineVersionBtn:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.12);
  border-color: rgba(255, 255, 255, 0.25);
}

.inlineVersionBtn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.inlineVersionBtn svg {
  transition: transform 0.2s ease;
}

.inlineVersionBtn svg.rotated {
  transform: rotate(180deg);
}

.inlineVersionDropdown {
  position: absolute;
  bottom: calc(100% + 4px);
  right: 0;
  min-width: 120px;
  background: #1e2128;
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-radius: 8px;
  box-shadow: 0 -8px 24px rgba(0, 0, 0, 0.4);
  z-index: 100;
  padding: 4px;
}

[dir="rtl"] .inlineVersionDropdown {
  right: auto;
  left: 0;
}

.versionOption {
  display: block;
  width: 100%;
  padding: 6px 10px;
  background: transparent;
  border: none;
  border-radius: 4px;
  color: #e2e8f0;
  font-size: 12px;
  text-align: start;
  cursor: pointer;
  transition: all 0.15s ease;
}

.versionOption:hover {
  background: rgba(255, 255, 255, 0.08);
}

.versionOption.active {
  background: rgba(59, 130, 246, 0.2);
  color: #60a5fa;
}

.inlineActionBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s ease;
  position: relative;
}

.inlineActionBtn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.inlineActionBtn.regenerate {
  background: rgba(139, 92, 246, 0.15);
  color: #a78bfa;
}

.inlineActionBtn.regenerate:hover:not(:disabled) {
  background: rgba(139, 92, 246, 0.25);
}

.inlineActionBtn.send {
  background: rgba(34, 197, 94, 0.15);
  color: #4ade80;
}

.inlineActionBtn.send:hover:not(:disabled) {
  background: rgba(34, 197, 94, 0.25);
}

.inlineActionBtn.approve {
  background: rgba(59, 130, 246, 0.15);
  color: #60a5fa;
}

.inlineActionBtn.approve:hover:not(:disabled) {
  background: rgba(59, 130, 246, 0.25);
}

.inlineActionBtn.view {
  background: rgba(100, 116, 139, 0.15);
  color: #94a3b8;
}

.inlineActionBtn.view:hover:not(:disabled) {
  background: rgba(100, 116, 139, 0.25);
  color: #e2e8f0;
}

.inlineActionBtn .countBadge {
  position: absolute;
  top: -4px;
  right: -4px;
  min-width: 14px;
  height: 14px;
  padding: 0 4px;
  background: #3b82f6;
  color: #fff;
  border-radius: 7px;
  font-size: 9px;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
}

[dir="rtl"] .inlineActionBtn .countBadge {
  right: auto;
  left: -4px;
}

.inlineActionBtn .spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* Light theme styles for inline toolbar */
.meeting-room.light .toolbarStatusBadge.finalized {
  background: #dcfce7;
  color: #166534;
  border: 1px solid #86efac;
}

.meeting-room.light .toolbarStatusBadge.pending {
  background: #fef3c7;
  color: #92400e;
  border: 1px solid #fcd34d;
}

.meeting-room.light .inlineVersionBtn {
  background: #ffffff;
  border: 1px solid #d4d4d8;
  color: #3f3f46;
}

.meeting-room.light .inlineVersionBtn:hover:not(:disabled) {
  background: #f4f4f5;
  border-color: #a1a1aa;
}

.meeting-room.light .inlineVersionDropdown {
  background: #fff;
  border-color: rgba(0, 0, 0, 0.1);
  box-shadow: 0 -8px 24px rgba(0, 0, 0, 0.15);
}

.meeting-room.light .versionOption {
  color: #3f3f46;
}

.meeting-room.light .versionOption:hover {
  background: rgba(0, 0, 0, 0.05);
}

.meeting-room.light .versionOption.active {
  background: rgba(0, 109, 75, 0.1);
  color: #006d4b;
}

.meeting-room.light .inlineActionBtn {
  background: #ffffff;
  border: 1px solid #d4d4d8;
  color: #71717a;
}

.meeting-room.light .inlineActionBtn:hover:not(:disabled) {
  background: #f4f4f5;
  border-color: #a1a1aa;
  color: #3f3f46;
}

.meeting-room.light .inlineActionBtn.regenerate,
.meeting-room.light .inlineActionBtn.send,
.meeting-room.light .inlineActionBtn.approve,
.meeting-room.light .inlineActionBtn.view {
  background: #ffffff;
  border: 1px solid #d4d4d8;
  color: #71717a;
}

.meeting-room.light .inlineActionBtn.regenerate:hover:not(:disabled),
.meeting-room.light .inlineActionBtn.send:hover:not(:disabled),
.meeting-room.light .inlineActionBtn.approve:hover:not(:disabled),
.meeting-room.light .inlineActionBtn.view:hover:not(:disabled) {
  background: #f4f4f5;
  border-color: #a1a1aa;
  color: #3f3f46;
}
</style>

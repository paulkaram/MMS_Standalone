<template>
  <div class="start-session-page">
    <!-- Success Screen -->
    <Transition name="fade-scale">
      <div v-if="isCreated" class="success-overlay">
        <div class="success-card">
          <!-- Decorative Elements -->
          <div class="success-decoration">
            <div class="decoration-circle circle-1"></div>
            <div class="decoration-circle circle-2"></div>
            <div class="decoration-circle circle-3"></div>
          </div>

          <!-- Success Icon -->
          <div class="success-icon-container">
            <div class="success-icon-bg">
              <svg class="checkmark" viewBox="0 0 52 52">
                <circle class="checkmark-circle" cx="26" cy="26" r="25" fill="none"/>
                <path class="checkmark-check" fill="none" d="M14.1 27.2l7.1 7.2 16.7-16.8"/>
              </svg>
            </div>
          </div>

          <!-- Content -->
          <div class="success-content">
            <h2 class="success-title">{{ $t('SessionCreatedSuccessfully') }}</h2>
            <p class="success-subtitle">{{ $t('SessionCreatedDescription') }}</p>
          </div>

          <!-- Session Summary -->
          <div class="success-summary">
            <div class="summary-item">
              <div class="summary-icon-box">
                <Icon icon="mdi:pound" class="w-5 h-5" />
              </div>
              <div class="summary-details">
                <span class="summary-label">{{ $t('ReferenceNumber') }}</span>
                <span class="summary-value">{{ savedSession?.referenceNumber || '-' }}</span>
              </div>
            </div>
            <div class="summary-divider"></div>
            <div class="summary-item">
              <div class="summary-icon-box">
                <Icon icon="mdi:file-document-outline" class="w-5 h-5" />
              </div>
              <div class="summary-details">
                <span class="summary-label">{{ $t('Subject') }}</span>
                <span class="summary-value">{{ form.subject || '-' }}</span>
              </div>
            </div>
            <div class="summary-divider"></div>
            <div class="summary-item">
              <div class="summary-icon-box">
                <Icon icon="mdi:calendar" class="w-5 h-5" />
              </div>
              <div class="summary-details">
                <span class="summary-label">{{ $t('MeetingDate') }}</span>
                <span class="summary-value">{{ formatDate(form.meetingDate) }}</span>
              </div>
            </div>
            <div class="summary-divider"></div>
            <div class="summary-item">
              <div class="summary-icon-box">
                <Icon icon="mdi:format-list-checks" class="w-5 h-5" />
              </div>
              <div class="summary-details">
                <span class="summary-label">{{ $t('SessionItems') }}</span>
                <span class="summary-value">{{ form.sessionItems.length }} {{ $t('Items') }}</span>
              </div>
            </div>
          </div>

          <!-- Attachments Section -->
          <div class="success-attachments">
            <h3 class="attachments-title">
              <Icon icon="mdi:paperclip" class="w-5 h-5" />
              {{ $t('Attachments') }}
            </h3>
            <div
              class="upload-dropzone"
              :class="{ 'active': isDragging }"
              @click="triggerFileInput"
              @dragover.prevent="isDragging = true"
              @dragleave="isDragging = false"
              @drop.prevent="handleFileDrop"
            >
              <input ref="fileInput" type="file" multiple class="hidden" @change="handleFileSelect" />
              <Icon icon="mdi:cloud-upload" class="w-8 h-8 text-zinc-400" />
              <p class="text-sm text-zinc-500">{{ $t('DragDropFiles') }}</p>
            </div>
            <div v-if="uploadingAttachment" class="flex items-center gap-2 text-sm text-zinc-500 mt-3">
              <Icon icon="mdi:loading" class="w-4 h-4 animate-spin" />
              {{ $t('Uploading') }}
            </div>
            <div v-if="attachments.length > 0" class="attachment-list">
              <div v-for="att in attachments" :key="att.id" class="attachment-row">
                <div class="attachment-info">
                  <Icon icon="mdi:file-document-outline" class="w-4 h-4 text-zinc-400" />
                  <span class="attachment-name">{{ att.name }}</span>
                  <span class="attachment-size">{{ formatFileSize(att.size) }}</span>
                </div>
                <div class="attachment-actions">
                  <button type="button" class="att-btn download" @click="downloadAttachment(att)">
                    <Icon icon="mdi:download" class="w-4 h-4" />
                  </button>
                  <button type="button" class="att-btn delete" @click="deleteAttachment(att)">
                    <Icon icon="mdi:delete" class="w-4 h-4" />
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Actions -->
          <div class="success-actions">
            <button type="button" class="action-btn secondary" @click="$router.push('/')">
              <Icon icon="mdi:home" class="w-4 h-4" />
              <span>{{ $t('BackToHome') }}</span>
            </button>
            <button type="button" class="action-btn primary" @click="createAnother">
              <Icon icon="mdi:plus" class="w-4 h-4" />
              <span>{{ $t('CreateAnother') }}</span>
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Wizard Form -->
    <template v-if="!isCreated">
      <div class="main-content">
        <!-- Sidebar Steps -->
        <div class="steps-sidebar">
          <!-- Progress Header -->
          <div class="sidebar-progress">
            <div class="progress-header">
              <span class="progress-label">{{ $t('Progress') }}</span>
              <span class="progress-value">{{ progressPercentage }}%</span>
            </div>
            <div class="progress-track">
              <div class="progress-fill" :style="{ width: progressPercentage + '%' }"></div>
            </div>
          </div>

          <div class="steps-list">
            <button
              v-for="(step, index) in steps"
              :key="step.key"
              type="button"
              :class="['step-item', {
                'active': currentStep === index,
                'completed': completedSteps.includes(index),
                'disabled': index > currentStep && !completedSteps.includes(index - 1)
              }]"
              :disabled="index > currentStep && !completedSteps.includes(index - 1)"
              @click="goToStep(index)"
            >
              <div class="step-indicator">
                <div class="step-icon-wrapper">
                  <Icon v-if="completedSteps.includes(index) && currentStep !== index" icon="mdi:check" class="step-check" />
                  <Icon v-else :icon="step.icon" class="step-icon" />
                </div>
                <div v-if="index < steps.length - 1" class="step-connector">
                  <div class="connector-line" :class="{ filled: completedSteps.includes(index) }"></div>
                </div>
              </div>
              <div class="step-content">
                <span class="step-num">{{ $t('Step') }} {{ index + 1 }}</span>
                <span class="step-title">{{ step.title }}</span>
                <span class="step-desc">{{ step.description }}</span>
              </div>
            </button>
          </div>
        </div>

        <!-- Form Content -->
        <div class="form-content">
          <!-- Loading -->
          <div v-if="loading" class="loading-state">
            <div class="loader">
              <div class="loader-ring"></div>
              <Icon icon="mdi:presentation" class="loader-icon" />
            </div>
            <p>{{ $t('Loading') }}</p>
          </div>

          <!-- Step Content -->
          <Transition name="slide-fade" mode="out-in">
            <div v-if="!loading" :key="currentStep" class="step-panel">
              <!-- Step 1: Session Info -->
              <div v-show="currentStep === 0" class="panel-body">
                <div class="form-grid">
                  <!-- Committee -->
                  <div class="form-field">
                    <Combobox
                      v-model="form.committeeId"
                      :options="committeeOptions"
                      item-text="label"
                      item-value="value"
                      :label="$t('CommitteeCouncil')"
                      :placeholder="$t('SelectCommittee')"
                      :required="true"
                      :loading="loadingCommittees"
                    />
                  </div>

                  <!-- External Reference Number -->
                  <div class="form-field">
                    <Input
                      v-model="form.externalReferenceNumber"
                      :label="$t('ExternalReferenceNumber')"
                      :placeholder="$t('EnterExternalReferenceNumber')"
                      :required="true"
                    />
                  </div>

                  <!-- Subject -->
                  <div class="form-field full-width">
                    <Input
                      v-model="form.subject"
                      :label="$t('Subject')"
                      :placeholder="$t('EnterSubject')"
                      :required="true"
                    />
                  </div>

                  <!-- Note -->
                  <div class="form-field full-width">
                    <label class="field-label">{{ $t('Note') }}</label>
                    <textarea
                      v-model="form.note"
                      class="field-textarea"
                      :placeholder="$t('EnterNote')"
                      rows="3"
                    ></textarea>
                  </div>

                  <!-- Meeting Date -->
                  <div class="form-field">
                    <DatePicker
                      v-model="form.meetingDate"
                      :label="$t('MeetingDate')"
                      :required="true"
                    />
                  </div>

                  <!-- Due Date -->
                  <div class="form-field">
                    <DatePicker
                      v-model="form.dueDate"
                      :label="$t('DueDate')"
                      :required="true"
                    />
                  </div>

                  <!-- Tags -->
                  <div class="form-field full-width">
                    <label class="field-label">
                      <Icon icon="mdi:tag" class="w-4 h-4 inline-block" />
                      {{ $t('Tags') }}
                    </label>
                    <div class="tags-input" :class="{ focused: tagInputFocused }" @click="focusTagInput">
                      <span v-for="(tag, i) in tags" :key="i" class="tag-chip">
                        <span class="tag-text">{{ tag }}</span>
                        <button type="button" class="tag-remove" @click.stop="removeTag(i)">
                          <Icon icon="mdi:close" class="w-3 h-3" />
                        </button>
                      </span>
                      <input
                        ref="tagInputRef"
                        v-model="tagInput"
                        class="tag-field"
                        :placeholder="tags.length === 0 ? ($t('EnterTags')) : ''"
                        @keydown.enter.prevent="addTag"
                        @keydown.,.prevent="addTag"
                        @keydown.delete="onTagBackspace"
                        @focus="tagInputFocused = true"
                        @blur="tagInputFocused = false; addTag()"
                      />
                    </div>
                  </div>
                </div>
              </div>

              <!-- Step 2: Session Items -->
              <div v-show="currentStep === 1" class="panel-body">
                <!-- Add Item Button -->
                <div class="items-toolbar">
                  <span class="items-count" v-if="form.sessionItems.length > 0">
                    {{ form.sessionItems.length }} {{ $t('Items') }}
                  </span>
                  <button type="button" class="add-item-btn" @click="openAddItemDialog">
                    <Icon icon="mdi:plus" class="w-4 h-4" />
                    {{ $t('AddItem') }}
                  </button>
                </div>

                <!-- Empty State -->
                <div v-if="form.sessionItems.length === 0" class="empty-state">
                  <div class="empty-icon">
                    <Icon icon="mdi:format-list-checks" class="w-12 h-12" />
                  </div>
                  <h3>{{ $t('NoSessionItems') }}</h3>
                  <p>{{ $t('AddAtLeastOneItem') }}</p>
                  <button type="button" class="empty-action-btn" @click="openAddItemDialog">
                    <Icon icon="mdi:plus" class="w-4 h-4" />
                    {{ $t('AddFirstItem') }}
                  </button>
                </div>

                <!-- Items Table -->
                <div v-else class="items-table-container">
                  <table class="items-table">
                    <thead>
                      <tr>
                        <th class="col-num">#</th>
                        <th class="col-subject">{{ $t('Subject') }}</th>
                        <th class="col-type">{{ $t('ItemType') }}</th>
                        <th class="col-external">{{ $t('ExternalId') }}</th>
                        <th class="col-related">{{ $t('RelatedItem') }}</th>
                        <th class="col-actions">{{ $t('Actions') }}</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="(item, index) in form.sessionItems" :key="index">
                        <td class="col-num">
                          <span class="row-number">{{ index + 1 }}</span>
                        </td>
                        <td class="col-subject">
                          <div class="item-subject">
                            <span class="subject-text">{{ item.subject }}</span>
                            <span v-if="item.tags" class="item-tags-preview">
                              <Icon icon="mdi:tag" class="w-3 h-3" />
                              {{ item.tags }}
                            </span>
                          </div>
                        </td>
                        <td class="col-type">
                          <span class="type-badge">{{ getItemTypeName(item.itemTypeId) }}</span>
                        </td>
                        <td class="col-external">{{ item.externalId || '-' }}</td>
                        <td class="col-related">
                          <span v-if="item.relatedSessionItemSubject" class="related-badge">
                            <Icon icon="mdi:link-variant" class="w-3 h-3" />
                            {{ item.relatedSessionItemSubject }}
                          </span>
                          <span v-else class="text-zinc-400">-</span>
                        </td>
                        <td class="col-actions">
                          <div class="action-buttons">
                            <button type="button" class="row-action-btn edit" @click="editItem(index)" :title="$t('Edit')">
                              <Icon icon="mdi:pencil" class="w-4 h-4" />
                            </button>
                            <button type="button" class="row-action-btn delete" @click="removeItem(index)" :title="$t('Delete')">
                              <Icon icon="mdi:delete" class="w-4 h-4" />
                            </button>
                          </div>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>

              <!-- Step 3: Review & Create -->
              <div v-show="currentStep === 2" class="panel-body">
                <div class="review-section">
                  <!-- Summary Cards -->
                  <div class="summary-grid">
                    <div class="summary-card">
                      <div class="sc-icon info">
                        <Icon icon="mdi:information-outline" class="w-6 h-6" />
                      </div>
                      <div class="sc-content">
                        <h4>{{ $t('SessionInfo') }}</h4>
                        <p>{{ form.subject || '-' }}</p>
                        <span class="sc-meta">
                          <Icon icon="mdi:calendar" class="w-4 h-4" />
                          {{ formatDate(form.meetingDate) }}
                        </span>
                      </div>
                      <button type="button" class="sc-edit" @click="goToStep(0)">
                        <Icon icon="mdi:pencil" class="w-4 h-4" />
                      </button>
                    </div>

                    <div class="summary-card">
                      <div class="sc-icon items">
                        <Icon icon="mdi:format-list-checks" class="w-6 h-6" />
                      </div>
                      <div class="sc-content">
                        <h4>{{ $t('SessionItems') }}</h4>
                        <p>{{ form.sessionItems.length }} {{ $t('Items') }}</p>
                      </div>
                      <button type="button" class="sc-edit" @click="goToStep(1)">
                        <Icon icon="mdi:pencil" class="w-4 h-4" />
                      </button>
                    </div>

                    <div class="summary-card">
                      <div class="sc-icon committee">
                        <Icon icon="mdi:account-group" class="w-6 h-6" />
                      </div>
                      <div class="sc-content">
                        <h4>{{ $t('CommitteeCouncil') }}</h4>
                        <p>{{ getCommitteeName(form.committeeId) }}</p>
                      </div>
                    </div>

                    <div class="summary-card">
                      <div class="sc-icon dates">
                        <Icon icon="mdi:clock-outline" class="w-6 h-6" />
                      </div>
                      <div class="sc-content">
                        <h4>{{ $t('DueDate') }}</h4>
                        <p>{{ formatDate(form.dueDate) }}</p>
                      </div>
                    </div>
                  </div>

                  <!-- Tags Preview -->
                  <div v-if="tags.length > 0" class="review-tags">
                    <h4 class="review-tags-label">
                      <Icon icon="mdi:tag" class="w-4 h-4" />
                      {{ $t('Tags') }}
                    </h4>
                    <div class="review-tags-list">
                      <span v-for="(tag, i) in tags" :key="i" class="review-tag-chip">{{ tag }}</span>
                    </div>
                  </div>

                  <!-- Items Preview -->
                  <div class="review-items">
                    <h4 class="review-items-label">
                      <Icon icon="mdi:format-list-checks" class="w-4 h-4" />
                      {{ $t('SessionItems') }}
                    </h4>
                    <div class="review-items-list">
                      <div v-for="(item, index) in form.sessionItems" :key="index" class="review-item-row">
                        <span class="review-item-num">{{ index + 1 }}</span>
                        <div class="review-item-info">
                          <span class="review-item-subject">{{ item.subject }}</span>
                          <span class="review-item-type">{{ getItemTypeName(item.itemTypeId) }}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Navigation Footer -->
              <div class="panel-footer">
                <Transition name="fade">
                  <div v-if="validationError" class="validation-error">
                    <Icon icon="mdi:alert-circle" class="w-5 h-5 flex-shrink-0" />
                    <span>{{ validationError }}</span>
                  </div>
                </Transition>

                <div class="nav-actions">
                  <div class="nav-start">
                    <Button v-if="currentStep > 0" variant="outline" @click="previousStep">
                      <Icon icon="mdi:arrow-right" class="w-4 h-4" />
                      {{ $t('Previous') }}
                    </Button>
                  </div>

                  <div class="nav-end">
                    <Button
                      v-if="currentStep < steps.length - 1"
                      variant="primary"
                      @click="nextStep"
                    >
                      {{ $t('Next') }}
                      <Icon icon="mdi:arrow-left" class="w-4 h-4" />
                    </Button>

                    <button
                      v-if="currentStep === steps.length - 1"
                      type="button"
                      class="create-btn"
                      :disabled="submitting"
                      @click="submitSession"
                    >
                      <Icon v-if="submitting" icon="mdi:loading" class="w-4 h-4 animate-spin" />
                      <Icon v-else icon="mdi:check" class="w-4 h-4" />
                      <span>{{ $t('CreateSession') }}</span>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </Transition>
        </div>
      </div>
    </template>

    <!-- Add/Edit Item Modal -->
    <Modal v-model="showItemDialog" :title="editingItemIndex !== null ? ($t('EditItem')) : ($t('AddItem'))" size="lg">
      <div class="p-6 space-y-4">
        <Input
          v-model="itemForm.subject"
          :label="$t('Subject')"
          :placeholder="$t('EnterItemSubject')"
          :required="true"
        />
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Select
            v-model="itemForm.itemTypeId"
            :options="itemTypeOptions"
            item-text="label"
            item-value="value"
            :label="$t('ItemType')"
            :placeholder="$t('SelectItemType')"
            :required="true"
          />
          <Input
            v-model="itemForm.externalId"
            :label="$t('ExternalId')"
            :placeholder="$t('EnterExternalId')"
          />
        </div>

        <!-- Tags for item -->
        <div>
          <label class="field-label">
            <Icon icon="mdi:tag" class="w-4 h-4 inline-block" />
            {{ $t('Tags') }}
          </label>
          <div class="tags-input" :class="{ focused: itemTagInputFocused }" @click="focusItemTagInput">
            <span v-for="(tag, i) in itemTags" :key="i" class="tag-chip">
              <span class="tag-text">{{ tag }}</span>
              <button type="button" class="tag-remove" @click.stop="removeItemTag(i)">
                <Icon icon="mdi:close" class="w-3 h-3" />
              </button>
            </span>
            <input
              ref="itemTagInputRef"
              v-model="itemTagInput"
              class="tag-field"
              :placeholder="itemTags.length === 0 ? ($t('EnterTags')) : ''"
              @keydown.enter.prevent="addItemTag"
              @keydown.,.prevent="addItemTag"
              @keydown.delete="onItemTagBackspace"
              @focus="itemTagInputFocused = true"
              @blur="itemTagInputFocused = false; addItemTag()"
            />
          </div>
        </div>

        <div>
          <label class="field-label">{{ $t('InternalNote') }}</label>
          <textarea
            v-model="itemForm.internalNote"
            class="field-textarea"
            :placeholder="$t('EnterInternalNote')"
            rows="3"
          ></textarea>
        </div>

        <!-- Related Item -->
        <div>
          <label class="field-label">{{ $t('RelatedItem') }}</label>
          <div class="related-item-field">
            <div class="related-item-display">
              <Icon icon="mdi:link-variant" class="w-4 h-4 text-zinc-400" />
              <span>{{ itemForm.relatedSessionItemSubject || ($t('NoRelatedItem')) }}</span>
            </div>
            <button type="button" class="related-item-btn" @click="openRelatedItemSearch">
              <Icon icon="mdi:magnify" class="w-4 h-4" />
            </button>
            <button v-if="itemForm.relatedSessionItemId" type="button" class="related-item-btn danger" @click="clearRelatedItem">
              <Icon icon="mdi:close" class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
      <template #footer>
        <div class="flex justify-end gap-3 p-4 border-t">
          <button type="button" class="btn btn-outline" @click="showItemDialog = false">
            {{ $t('Cancel') }}
          </button>
          <button type="button" class="btn btn-primary" @click="saveItem" :disabled="!itemForm.subject || !itemForm.itemTypeId">
            {{ $t('Save') }}
          </button>
        </div>
      </template>
    </Modal>

    <!-- Related Item Search Modal -->
    <Modal v-model="showRelatedSearch" :title="$t('SearchRelatedItems')" size="lg">
      <div class="p-6">
        <Input
          v-model="relatedSearchText"
          :placeholder="$t('SearchBySubjectOrId')"
          @update:model-value="searchRelatedItems"
        />
        <div v-if="searchingRelated" class="flex items-center justify-center py-8">
          <Icon icon="mdi:loading" class="w-6 h-6 animate-spin text-primary" />
        </div>
        <div v-else-if="relatedSearchResults.length > 0" class="mt-4 max-h-[300px] overflow-y-auto">
          <div
            v-for="result in relatedSearchResults"
            :key="result.id"
            class="related-result-row"
            @click="selectRelatedItem(result)"
          >
            <div class="related-result-info">
              <span class="related-result-subject">{{ result.subject }}</span>
              <span v-if="result.externalId" class="related-result-id">{{ result.externalId }}</span>
            </div>
            <button type="button" class="related-select-btn">
              {{ $t('Select') }}
            </button>
          </div>
        </div>
        <div v-else-if="relatedSearchText && relatedSearchText.length >= 2" class="text-center py-8 text-zinc-400">
          <Icon icon="mdi:magnify" class="w-8 h-8 mx-auto mb-2 opacity-40" />
          {{ $t('NoResults') }}
        </div>
      </div>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
import Input from '@/components/ui/Input.vue'
import Combobox from '@/components/ui/Combobox.vue'
import Select from '@/components/ui/Select.vue'
import DatePicker from '@/components/ui/DatePicker.vue'
import Modal from '@/components/ui/Modal.vue'
import { useToast } from '@/composables/useToast'
import SessionsService, { type SessionDto, type SessionItemDto } from '@/services/SessionsService'

const router = useRouter()
const { toast } = useToast()

// State
const loading = ref(false)
const submitting = ref(false)
const uploadingAttachment = ref(false)
const isDragging = ref(false)
const searchingRelated = ref(false)
const loadingCommittees = ref(false)
const isCreated = ref(false)
const currentStep = ref(0)
const completedSteps = ref<number[]>([])
const validationError = ref<string | null>(null)

// Data
const committees = ref<{ id: number; name: string }[]>([])
const itemTypes = ref<{ id: number; name: string }[]>([])
const savedSession = ref<SessionDto | null>(null)
const attachments = ref<any[]>([])
const fileInput = ref<HTMLInputElement | null>(null)

// Steps configuration
const steps = [
  {
    key: 'info',
    title: 'معلومات الجلسة',
    description: 'البيانات الأساسية للجلسة',
    icon: 'mdi:file-document-edit-outline',
    color: 'linear-gradient(135deg, #006d4b 0%, #004730 100%)'
  },
  {
    key: 'items',
    title: 'بنود الجلسة',
    description: 'إضافة وإدارة البنود',
    icon: 'mdi:format-list-checks',
    color: 'linear-gradient(135deg, #3B82F6 0%, #2563EB 100%)'
  },
  {
    key: 'review',
    title: 'المراجعة والإنشاء',
    description: 'مراجعة البيانات وإنشاء الجلسة',
    icon: 'mdi:check-circle-outline',
    color: 'linear-gradient(135deg, #198b5f 0%, #004730 100%)'
  }
]

// Form
const form = reactive({
  committeeId: null as number | null,
  externalReferenceNumber: '',
  subject: '',
  note: '',
  meetingDate: null as Date | string | null,
  dueDate: null as Date | string | null,
  sessionItems: [] as Array<{
    externalId?: string
    subject: string
    itemTypeId: number
    tags?: string
    internalNote?: string
    relatedSessionItemId?: number | null
    relatedSessionItemSubject?: string
    order: number
  }>
})

// Tags (session-level)
const tags = ref<string[]>([])
const tagInput = ref('')
const tagInputRef = ref<HTMLInputElement | null>(null)
const tagInputFocused = ref(false)

// Tags (item-level)
const itemTags = ref<string[]>([])
const itemTagInput = ref('')
const itemTagInputRef = ref<HTMLInputElement | null>(null)
const itemTagInputFocused = ref(false)

// Item dialog
const showItemDialog = ref(false)
const editingItemIndex = ref<number | null>(null)
const itemForm = reactive({
  externalId: '',
  subject: '',
  itemTypeId: null as number | null,
  internalNote: '',
  relatedSessionItemId: null as number | null,
  relatedSessionItemSubject: ''
})

// Related item search
const showRelatedSearch = ref(false)
const relatedSearchText = ref('')
const relatedSearchResults = ref<SessionItemDto[]>([])
let searchTimeout: ReturnType<typeof setTimeout> | null = null

// Computed
const committeeOptions = computed(() =>
  committees.value.map(c => ({ label: c.name, value: c.id }))
)

const itemTypeOptions = computed(() =>
  itemTypes.value.map(t => ({ label: t.name, value: t.id }))
)

const progressPercentage = computed(() => {
  const totalSteps = steps.length
  const completed = completedSteps.value.length
  return Math.round((completed / totalSteps) * 100)
})

// Tag methods (session-level)
const focusTagInput = () => {
  tagInputRef.value?.focus()
}

const addTag = () => {
  const val = tagInput.value.trim().replace(/,/g, '')
  if (val && !tags.value.includes(val)) {
    tags.value.push(val)
  }
  tagInput.value = ''
}

const removeTag = (index: number) => {
  tags.value.splice(index, 1)
}

const onTagBackspace = () => {
  if (!tagInput.value && tags.value.length > 0) {
    tags.value.pop()
  }
}

// Tag methods (item-level)
const focusItemTagInput = () => {
  itemTagInputRef.value?.focus()
}

const addItemTag = () => {
  const val = itemTagInput.value.trim().replace(/,/g, '')
  if (val && !itemTags.value.includes(val)) {
    itemTags.value.push(val)
  }
  itemTagInput.value = ''
}

const removeItemTag = (index: number) => {
  itemTags.value.splice(index, 1)
}

const onItemTagBackspace = () => {
  if (!itemTagInput.value && itemTags.value.length > 0) {
    itemTags.value.pop()
  }
}

// Data loading
const loadCommittees = async () => {
  loadingCommittees.value = true
  try {
    const response = await SessionsService.listCommittees()
    const data = response.data?.data || response.data || []
    committees.value = Array.isArray(data) ? data : []
  } catch (error) {
    console.error('Failed to load committees:', error)
  } finally {
    loadingCommittees.value = false
  }
}

const loadItemTypes = async () => {
  try {
    const response = await SessionsService.listItemTypes()
    const data = response.data?.data || response.data || []
    itemTypes.value = Array.isArray(data) ? data : []
  } catch (error) {
    console.error('Failed to load item types:', error)
  }
}

const getItemTypeName = (typeId: number) => {
  return itemTypes.value.find(t => t.id === typeId)?.name || '-'
}

const getCommitteeName = (id: number | null) => {
  if (!id) return '-'
  return committees.value.find(c => c.id === id)?.name || '-'
}

// Navigation
const goToStep = (index: number) => {
  if (index <= currentStep.value || completedSteps.value.includes(index - 1)) {
    currentStep.value = index
    validationError.value = null
  }
}

const nextStep = () => {
  validationError.value = null

  // Validate current step
  if (currentStep.value === 0) {
    if (!form.committeeId) {
      validationError.value = 'يرجى اختيار المجلس أو اللجنة'
      return
    }
    if (!form.externalReferenceNumber) {
      validationError.value = 'يرجى إدخال الرقم المرجعي الخارجي'
      return
    }
    if (!form.subject) {
      validationError.value = 'يرجى إدخال الموضوع'
      return
    }
    if (!form.meetingDate) {
      validationError.value = 'يرجى تحديد تاريخ الجلسة'
      return
    }
    if (!form.dueDate) {
      validationError.value = 'يرجى تحديد تاريخ الاستحقاق'
      return
    }
  }

  if (currentStep.value === 1) {
    if (form.sessionItems.length === 0) {
      validationError.value = 'يرجى إضافة بند واحد على الأقل'
      return
    }
  }

  if (!completedSteps.value.includes(currentStep.value)) {
    completedSteps.value.push(currentStep.value)
  }

  if (currentStep.value < steps.length - 1) {
    currentStep.value++
  }
}

const previousStep = () => {
  validationError.value = null
  if (currentStep.value > 0) {
    currentStep.value--
  }
}

// Item management
const openAddItemDialog = () => {
  editingItemIndex.value = null
  Object.assign(itemForm, {
    externalId: '',
    subject: '',
    itemTypeId: null,
    internalNote: '',
    relatedSessionItemId: null,
    relatedSessionItemSubject: ''
  })
  itemTags.value = []
  itemTagInput.value = ''
  showItemDialog.value = true
}

const editItem = (index: number) => {
  editingItemIndex.value = index
  const item = form.sessionItems[index]
  Object.assign(itemForm, {
    externalId: item.externalId || '',
    subject: item.subject,
    itemTypeId: item.itemTypeId,
    internalNote: item.internalNote || '',
    relatedSessionItemId: item.relatedSessionItemId || null,
    relatedSessionItemSubject: item.relatedSessionItemSubject || ''
  })
  itemTags.value = item.tags ? item.tags.split(',').map(t => t.trim()).filter(Boolean) : []
  itemTagInput.value = ''
  showItemDialog.value = true
}

const saveItem = () => {
  if (!itemForm.subject || !itemForm.itemTypeId) return

  const item = {
    externalId: itemForm.externalId || undefined,
    subject: itemForm.subject,
    itemTypeId: itemForm.itemTypeId!,
    tags: itemTags.value.length > 0 ? itemTags.value.join(', ') : undefined,
    internalNote: itemForm.internalNote || undefined,
    relatedSessionItemId: itemForm.relatedSessionItemId,
    relatedSessionItemSubject: itemForm.relatedSessionItemSubject || undefined,
    order: editingItemIndex.value !== null ? form.sessionItems[editingItemIndex.value].order : form.sessionItems.length
  }

  if (editingItemIndex.value !== null) {
    form.sessionItems[editingItemIndex.value] = item
  } else {
    form.sessionItems.push(item)
  }

  showItemDialog.value = false
}

const removeItem = (index: number) => {
  form.sessionItems.splice(index, 1)
  form.sessionItems.forEach((item, i) => { item.order = i })
}

// Related item search
const openRelatedItemSearch = () => {
  relatedSearchText.value = ''
  relatedSearchResults.value = []
  showRelatedSearch.value = true
}

const searchRelatedItems = (query: string) => {
  if (searchTimeout) clearTimeout(searchTimeout)
  if (!query || query.length < 2) {
    relatedSearchResults.value = []
    return
  }
  searchTimeout = setTimeout(async () => {
    searchingRelated.value = true
    try {
      const response = await SessionsService.searchSessionItems(query)
      const data = response.data?.data || response.data || []
      relatedSearchResults.value = Array.isArray(data) ? data : []
    } catch (error) {
      console.error('Failed to search items:', error)
    } finally {
      searchingRelated.value = false
    }
  }, 300)
}

const selectRelatedItem = (item: SessionItemDto) => {
  itemForm.relatedSessionItemId = item.id
  itemForm.relatedSessionItemSubject = item.subject
  showRelatedSearch.value = false
}

const clearRelatedItem = () => {
  itemForm.relatedSessionItemId = null
  itemForm.relatedSessionItemSubject = ''
}

// Submit
const submitSession = async () => {
  validationError.value = null
  submitting.value = true
  try {
    const postData = {
      committeeId: form.committeeId!,
      externalReferenceNumber: form.externalReferenceNumber,
      subject: form.subject,
      note: form.note || undefined,
      meetingDate: new Date(form.meetingDate!).toISOString(),
      dueDate: new Date(form.dueDate!).toISOString(),
      tags: tags.value.length > 0 ? tags.value.join(', ') : undefined,
      sessionItems: form.sessionItems.map(item => ({
        externalId: item.externalId,
        subject: item.subject,
        itemTypeId: item.itemTypeId,
        tags: item.tags,
        internalNote: item.internalNote,
        relatedSessionItemId: item.relatedSessionItemId,
        order: item.order
      }))
    }

    const response = await SessionsService.createSession(postData)
    const data = response.data?.data || response.data
    savedSession.value = data
    toast.success('تم إنشاء الجلسة بنجاح')
    isCreated.value = true
  } catch (error) {
    console.error('Failed to create session:', error)
    toast.error('فشل في إنشاء الجلسة')
  } finally {
    submitting.value = false
  }
}

// Attachments
const triggerFileInput = () => {
  fileInput.value?.click()
}

const handleFileSelect = async (event: Event) => {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    await uploadFiles(target.files)
    target.value = ''
  }
}

const handleFileDrop = async (event: DragEvent) => {
  isDragging.value = false
  if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
    await uploadFiles(event.dataTransfer.files)
  }
}

const uploadFiles = async (files: FileList) => {
  if (!savedSession.value) return
  uploadingAttachment.value = true
  try {
    const formData = new FormData()
    for (let i = 0; i < files.length; i++) {
      formData.append('files', files[i])
    }
    const response = await SessionsService.addAttachment(savedSession.value.id, formData)
    const data = response.data?.data || response.data || []
    attachments.value = Array.isArray(data) ? data : []
    toast.success('تم رفع المرفقات بنجاح')
  } catch (error) {
    console.error('Failed to upload:', error)
    toast.error('فشل في رفع المرفقات')
  } finally {
    uploadingAttachment.value = false
  }
}

const deleteAttachment = async (att: any) => {
  if (!savedSession.value) return
  try {
    const response = await SessionsService.deleteAttachment(savedSession.value.id, att.id)
    const data = response.data?.data || response.data || []
    attachments.value = Array.isArray(data) ? data : []
    toast.success('تم حذف المرفق')
  } catch (error) {
    console.error('Failed to delete attachment:', error)
    toast.error('فشل في حذف المرفق')
  }
}

const downloadAttachment = async (att: any) => {
  if (!savedSession.value) return
  try {
    const response = await SessionsService.downloadAttachment(savedSession.value.id, att.id)
    const blob = new Blob([response.data])
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = att.name || 'attachment'
    link.click()
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('Failed to download:', error)
    toast.error('فشل في تحميل المرفق')
  }
}

const formatFileSize = (bytes: number) => {
  if (!bytes) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i]
}

const formatDate = (date: Date | string | null) => {
  if (!date) return '-'
  try {
    return new Date(date).toLocaleDateString('ar-EG', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    })
  } catch {
    return String(date)
  }
}

const createAnother = () => {
  isCreated.value = false
  savedSession.value = null
  attachments.value = []
  currentStep.value = 0
  completedSteps.value = []
  validationError.value = null
  tags.value = []
  tagInput.value = ''
  Object.assign(form, {
    committeeId: null,
    externalReferenceNumber: '',
    subject: '',
    note: '',
    meetingDate: null,
    dueDate: null,
    sessionItems: []
  })
}

// Init
onMounted(async () => {
  await Promise.all([loadCommittees(), loadItemTypes()])
})
</script>

<style scoped>
/* ===== Layout ===== */
.main-content {
  @apply flex gap-6;
}

.steps-sidebar {
  @apply w-80 flex-shrink-0;
}

.form-content {
  @apply flex-1 min-w-0;
  overflow: hidden;
}

/* ===== Sidebar Progress ===== */
.sidebar-progress {
  @apply bg-white rounded-xl border border-zinc-100 p-4 mb-4;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.progress-header {
  @apply flex items-center justify-between mb-2;
}

.progress-label {
  @apply text-sm font-semibold text-zinc-700;
}

.progress-value {
  @apply text-lg font-bold;
  background: linear-gradient(135deg, #006d4b 0%, #003423 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.progress-track {
  @apply h-2 bg-zinc-100 rounded-full overflow-hidden;
}

.sidebar-progress .progress-fill {
  @apply h-full rounded-full transition-all duration-500 ease-out;
  background: linear-gradient(90deg, #006d4b 0%, #003423 100%);
}

/* ===== Steps List ===== */
.steps-list {
  @apply bg-white rounded-2xl border border-zinc-100 p-4;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.step-item {
  @apply flex gap-4 p-3 rounded-xl transition-all text-start w-full;
}

.step-item:hover:not(.disabled) {
  @apply bg-zinc-50;
}

.step-item.active {
  @apply bg-primary/5;
}

.step-item.disabled {
  @apply opacity-50 cursor-not-allowed;
}

.step-indicator {
  @apply flex flex-col items-center;
}

.step-icon-wrapper {
  @apply relative w-12 h-12 rounded-xl flex items-center justify-center transition-all;
  @apply bg-zinc-100 text-zinc-400;
}

.step-item.active .step-icon-wrapper {
  @apply bg-primary text-white;
  box-shadow: 0 4px 12px rgba(0, 109, 75, 0.3);
}

.step-item.completed .step-icon-wrapper {
  @apply bg-emerald-500 text-white;
}

.step-icon, .step-check {
  @apply w-5 h-5;
}

.step-connector {
  @apply flex-1 w-0.5 my-2;
}

.connector-line {
  @apply w-full h-full bg-zinc-200 rounded-full;
}

.connector-line.filled {
  @apply bg-emerald-500;
}

.step-content {
  @apply flex-1 min-w-0;
}

.step-num {
  @apply block text-[10px] uppercase tracking-wider text-zinc-400 font-medium mb-0.5;
}

.step-title {
  @apply block text-sm font-bold text-zinc-900 mb-0.5;
}

.step-item.active .step-title {
  @apply text-primary;
}

.step-desc {
  @apply block text-xs text-zinc-500 line-clamp-1;
}

/* ===== Loading ===== */
.loading-state {
  @apply flex flex-col items-center justify-center py-20 bg-white rounded-2xl border border-zinc-100;
}

.loader {
  @apply relative w-16 h-16 mb-4;
}

.loader-ring {
  @apply absolute inset-0 border-4 border-primary/20 border-t-primary rounded-full animate-spin;
}

.loader-icon {
  @apply absolute inset-0 m-auto w-6 h-6 text-primary;
}

/* ===== Step Panel ===== */
.step-panel {
  @apply bg-white rounded-2xl border border-zinc-100;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.panel-body {
  @apply p-5;
}

/* ===== Form Grid ===== */
.form-grid {
  @apply grid grid-cols-1 md:grid-cols-2 gap-4;
}

.form-field.full-width {
  @apply md:col-span-2;
}

.field-label {
  @apply flex items-center gap-1.5 text-sm font-medium text-zinc-700 mb-1.5;
}

.field-textarea {
  @apply w-full rounded-lg border border-zinc-300 p-3 text-sm resize-none transition-colors;
  @apply focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none;
}

/* ===== Tags Input ===== */
.tags-input {
  @apply flex flex-wrap items-center gap-1.5 min-h-[42px] px-3 py-2 rounded-lg border border-zinc-300 bg-white cursor-text transition-colors;
}

.tags-input.focused {
  @apply border-primary ring-1 ring-primary;
}

.tag-chip {
  @apply inline-flex items-center gap-1 px-2.5 py-1 rounded-md text-xs font-medium;
  background: linear-gradient(135deg, rgba(0, 109, 75, 0.08) 0%, rgba(0, 109, 75, 0.15) 100%);
  color: #004730;
  border: 1px solid rgba(0, 109, 75, 0.2);
}

.tag-text {
  @apply max-w-[120px] truncate;
}

.tag-remove {
  @apply flex items-center justify-center w-4 h-4 rounded-full hover:bg-red-100 hover:text-red-600 transition-colors;
}

.tag-field {
  @apply flex-1 min-w-[80px] text-sm py-0.5;
  border: none !important;
  outline: none !important;
  box-shadow: none !important;
  background: transparent !important;
  height: auto !important;
  padding: 0 !important;
  min-height: unset !important;
}

/* ===== Items Step ===== */
.items-toolbar {
  @apply flex items-center justify-between mb-4;
}

.items-count {
  @apply text-sm font-medium text-zinc-500;
}

.add-item-btn {
  @apply inline-flex items-center gap-1.5 px-4 py-2 rounded-lg text-sm font-medium text-white;
  @apply transition-all duration-200;
  background: linear-gradient(135deg, #006d4b 0%, #004730 100%);
  box-shadow: 0 2px 8px rgba(0, 109, 75, 0.25);
}

.add-item-btn:hover {
  box-shadow: 0 4px 14px rgba(0, 109, 75, 0.4);
  transform: translateY(-1px);
}

/* Empty State */
.empty-state {
  @apply flex flex-col items-center justify-center py-12 text-center;
}

.empty-icon {
  @apply w-20 h-20 rounded-2xl bg-zinc-100 flex items-center justify-center text-zinc-300 mb-4;
}

.empty-state h3 {
  @apply text-lg font-bold text-zinc-700 mb-1;
}

.empty-state p {
  @apply text-sm text-zinc-400 mb-4;
}

.empty-action-btn {
  @apply inline-flex items-center gap-1.5 px-5 py-2.5 rounded-lg text-sm font-medium;
  @apply text-primary border-2 border-dashed border-primary/30 hover:bg-primary/5 transition-colors;
}

/* Items Table */
.items-table-container {
  @apply overflow-x-auto rounded-lg border border-zinc-200;
}

.items-table {
  @apply w-full text-sm;
}

.items-table th {
  @apply px-5 py-4 text-start text-xs font-semibold uppercase tracking-wider border-b border-zinc-200;
  background-color: #006d4b;
  color: #d4d4d8;
}

.items-table td {
  @apply px-4 py-3 border-b border-zinc-100;
}

.items-table tr:last-child td {
  @apply border-b-0;
}

.items-table tbody tr:hover {
  background-color: rgba(0, 109, 75, 0.05);
}

.row-number {
  @apply inline-flex items-center justify-center w-7 h-7 rounded-lg bg-zinc-100 text-zinc-500 text-xs font-bold;
}

.item-subject {
  @apply flex flex-col gap-0.5;
}

.subject-text {
  @apply font-medium text-zinc-800;
}

.item-tags-preview {
  @apply flex items-center gap-1 text-xs text-zinc-400;
}

.type-badge {
  @apply inline-block px-2.5 py-1 rounded-md text-xs font-medium bg-blue-50 text-blue-700;
}

.related-badge {
  @apply inline-flex items-center gap-1 text-xs text-primary;
}

.action-buttons {
  @apply flex items-center gap-1;
}

.row-action-btn {
  @apply w-8 h-8 rounded-lg flex items-center justify-center transition-colors;
}

.row-action-btn.edit {
  @apply text-zinc-400 hover:text-primary hover:bg-primary/10;
}

.row-action-btn.delete {
  @apply text-zinc-400 hover:text-red-500 hover:bg-red-50;
}

/* ===== Review Step ===== */
.review-section {
  @apply space-y-5;
}

.summary-grid {
  @apply grid grid-cols-2 gap-4;
}

.summary-card {
  @apply relative flex items-start gap-3 p-4 bg-zinc-50 rounded-xl;
}

.sc-icon {
  @apply w-11 h-11 rounded-xl flex items-center justify-center text-white flex-shrink-0;
}

.sc-icon.info {
  background: linear-gradient(135deg, #006d4b 0%, #004730 100%);
}

.sc-icon.items {
  background: linear-gradient(135deg, #3B82F6 0%, #2563EB 100%);
}

.sc-icon.committee {
  background: linear-gradient(135deg, #8B5CF6 0%, #7C3AED 100%);
}

.sc-icon.dates {
  background: linear-gradient(135deg, #F59E0B 0%, #D97706 100%);
}

.sc-content {
  @apply flex-1 min-w-0;
}

.sc-content h4 {
  @apply text-xs font-bold text-zinc-500 mb-0.5;
}

.sc-content p {
  @apply text-sm text-zinc-800 font-medium truncate;
}

.sc-meta {
  @apply flex items-center gap-1 text-xs text-zinc-400 mt-1;
}

.sc-edit {
  @apply absolute top-3 left-3 w-7 h-7 rounded-lg bg-white text-zinc-400 flex items-center justify-center;
  @apply hover:text-primary hover:bg-primary/10 transition-colors;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.review-tags {
  @apply p-4 bg-zinc-50 rounded-xl;
}

.review-tags-label {
  @apply flex items-center gap-1.5 text-sm font-bold text-zinc-600 mb-2;
}

.review-tags-list {
  @apply flex flex-wrap gap-2;
}

.review-tag-chip {
  @apply inline-block px-3 py-1 rounded-full text-xs font-medium bg-primary/10 text-primary;
}

.review-items {
  @apply p-4 bg-zinc-50 rounded-xl;
}

.review-items-label {
  @apply flex items-center gap-1.5 text-sm font-bold text-zinc-600 mb-3;
}

.review-items-list {
  @apply space-y-2;
}

.review-item-row {
  @apply flex items-center gap-3 p-3 bg-white rounded-lg;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.review-item-num {
  @apply w-7 h-7 rounded-lg bg-primary/10 text-primary flex items-center justify-center text-xs font-bold flex-shrink-0;
}

.review-item-info {
  @apply flex-1 min-w-0;
}

.review-item-subject {
  @apply block text-sm font-medium text-zinc-800 truncate;
}

.review-item-type {
  @apply block text-xs text-zinc-400;
}

/* ===== Panel Footer ===== */
.panel-footer {
  @apply flex flex-col gap-3 border-t border-zinc-200 rounded-b-2xl bg-white;
  padding: 1rem;
}

.nav-actions {
  @apply flex items-center justify-between w-full;
}

.nav-start, .nav-end {
  @apply flex items-center gap-3;
}

.validation-error {
  @apply flex items-center gap-2 px-4 py-3 rounded-lg text-sm font-medium;
  @apply bg-red-50 text-red-700 border border-red-200;
}

.create-btn {
  @apply inline-flex items-center gap-2 px-6 py-2.5 rounded-xl font-medium text-sm text-white;
  @apply disabled:opacity-50 disabled:cursor-not-allowed;
  @apply transition-all duration-200;
  background: linear-gradient(135deg, #006d4b 0%, #004730 100%);
  box-shadow: 0 4px 14px rgba(0, 109, 75, 0.3);
}

.create-btn:hover:not(:disabled) {
  box-shadow: 0 6px 20px rgba(0, 109, 75, 0.5);
  transform: translateY(-1px);
}

/* ===== Related Item Field (in modal) ===== */
.related-item-field {
  @apply flex items-center gap-2;
}

.related-item-display {
  @apply flex-1 flex items-center gap-2 text-sm text-zinc-500 border border-zinc-200 rounded-lg px-3 py-2.5 min-h-[42px] bg-zinc-50;
}

.related-item-btn {
  @apply w-10 h-10 rounded-lg border border-zinc-200 flex items-center justify-center text-zinc-500;
  @apply hover:bg-zinc-50 transition-colors;
}

.related-item-btn.danger {
  @apply text-red-400 hover:bg-red-50 hover:text-red-600;
}

/* Related search results */
.related-result-row {
  @apply flex items-center justify-between p-3 rounded-lg cursor-pointer transition-colors;
  @apply hover:bg-primary/5 border border-transparent hover:border-primary/20;
}

.related-result-row + .related-result-row {
  @apply mt-2;
}

.related-result-info {
  @apply flex flex-col gap-0.5;
}

.related-result-subject {
  @apply text-sm font-medium text-zinc-800;
}

.related-result-id {
  @apply text-xs text-zinc-400;
}

.related-select-btn {
  @apply px-3 py-1.5 rounded-lg text-xs font-medium text-primary bg-primary/10;
  @apply hover:bg-primary/20 transition-colors;
}

/* ===== Success Screen ===== */
.success-overlay {
  @apply fixed inset-0 z-50 flex items-center justify-center p-4;
  background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
  overflow-y: auto;
}

.success-card {
  @apply relative w-full max-w-lg bg-white rounded-3xl overflow-hidden my-auto;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  animation: card-appear 0.5s ease-out;
}

@keyframes card-appear {
  0% { opacity: 0; transform: scale(0.9) translateY(20px); }
  100% { opacity: 1; transform: scale(1) translateY(0); }
}

.success-decoration {
  @apply absolute top-0 left-0 right-0 h-32 overflow-hidden;
  background: linear-gradient(135deg, var(--color-primary, #006d4b) 0%, #004730 100%);
}

.decoration-circle {
  @apply absolute rounded-full;
  background: rgba(255, 255, 255, 0.1);
}

.circle-1 { width: 200px; height: 200px; top: -100px; right: -50px; }
.circle-2 { width: 150px; height: 150px; top: -30px; left: -40px; }
.circle-3 { width: 80px; height: 80px; bottom: -20px; right: 30%; }

.success-icon-container {
  @apply relative flex justify-center;
  margin-top: 4rem;
  margin-bottom: 1.5rem;
}

.success-icon-bg {
  @apply w-24 h-24 rounded-full bg-white flex items-center justify-center;
  box-shadow: 0 10px 40px rgba(0, 109, 75, 0.3);
}

.checkmark {
  width: 56px; height: 56px; border-radius: 50%;
  stroke-width: 2; stroke: var(--color-primary, #006d4b); stroke-miterlimit: 10;
}

.checkmark-circle {
  stroke-dasharray: 166; stroke-dashoffset: 166;
  stroke-width: 2; stroke-miterlimit: 10;
  stroke: var(--color-primary, #006d4b); fill: none;
  animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
}

.checkmark-check {
  transform-origin: 50% 50%;
  stroke-dasharray: 48; stroke-dashoffset: 48; stroke-width: 3;
  animation: stroke 0.3s cubic-bezier(0.65, 0, 0.45, 1) 0.4s forwards;
}

@keyframes stroke { 100% { stroke-dashoffset: 0; } }

.success-content {
  @apply text-center px-6 mb-4;
}

.success-title {
  @apply text-2xl font-bold text-zinc-900 mb-2;
}

.success-subtitle {
  @apply text-zinc-500 text-sm;
}

.success-summary {
  @apply mx-6 mb-4 p-4 rounded-2xl;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
}

.summary-item {
  @apply flex items-center gap-3 py-2;
}

.summary-icon-box {
  @apply w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0;
  background: white;
  color: var(--color-primary, #006d4b);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.summary-details {
  @apply flex flex-col min-w-0 flex-1;
}

.summary-label {
  @apply text-xs text-zinc-400 font-medium;
}

.summary-value {
  @apply text-sm text-zinc-800 font-medium truncate;
}

.summary-divider {
  @apply h-px bg-zinc-200 my-1;
}

/* Success Attachments */
.success-attachments {
  @apply mx-6 mb-4;
}

.attachments-title {
  @apply flex items-center gap-2 text-sm font-bold text-zinc-700 mb-3;
}

.upload-dropzone {
  @apply flex flex-col items-center justify-center gap-2 p-5 rounded-xl border-2 border-dashed border-zinc-300;
  @apply cursor-pointer transition-colors hover:border-primary/50 hover:bg-primary/5;
}

.upload-dropzone.active {
  @apply border-primary bg-primary/5;
}

.attachment-list {
  @apply mt-3 space-y-2;
}

.attachment-row {
  @apply flex items-center justify-between p-3 rounded-lg bg-zinc-50;
}

.attachment-info {
  @apply flex items-center gap-2 min-w-0 flex-1;
}

.attachment-name {
  @apply text-sm text-zinc-700 font-medium truncate;
}

.attachment-size {
  @apply text-xs text-zinc-400 flex-shrink-0;
}

.attachment-actions {
  @apply flex items-center gap-1;
}

.att-btn {
  @apply w-7 h-7 rounded-md flex items-center justify-center transition-colors;
}

.att-btn.download {
  @apply text-zinc-400 hover:text-primary hover:bg-primary/10;
}

.att-btn.delete {
  @apply text-zinc-400 hover:text-red-500 hover:bg-red-50;
}

/* Success Actions */
.success-actions {
  @apply flex gap-3 px-6 mb-6;
}

.action-btn {
  @apply flex-1 flex items-center justify-center gap-2 py-3 px-4 rounded-xl font-medium text-sm transition-all duration-200;
}

.action-btn.primary {
  @apply text-white;
  background: linear-gradient(135deg, var(--color-primary, #006d4b) 0%, #004730 100%);
  box-shadow: 0 4px 14px rgba(0, 109, 75, 0.4);
}

.action-btn.primary:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 20px rgba(0, 109, 75, 0.5);
}

.action-btn.secondary {
  @apply bg-zinc-100 text-zinc-700 hover:bg-zinc-200;
}

/* ===== Transitions ===== */
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

.fade-scale-enter-active, .fade-scale-leave-active {
  transition: all 0.5s ease;
}
.fade-scale-enter-from, .fade-scale-leave-to {
  opacity: 0;
  transform: scale(0.95);
}

.slide-fade-enter-active {
  transition: opacity 0.25s ease-out, transform 0.25s ease-out;
}
.slide-fade-leave-active {
  transition: opacity 0.15s ease-in, transform 0.15s ease-in;
}
.slide-fade-enter-from {
  opacity: 0;
  transform: translateY(10px);
}
.slide-fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* ===== Responsive ===== */
@media (max-width: 1024px) {
  .main-content {
    @apply flex-col;
  }

  .steps-sidebar {
    @apply w-full;
  }

  .steps-list {
    @apply flex overflow-x-auto gap-2 p-3;
  }

  .step-item {
    @apply flex-col items-center text-center min-w-[100px] p-2;
  }

  .step-indicator {
    @apply flex-row;
  }

  .step-connector {
    @apply hidden;
  }

  .step-num, .step-desc {
    @apply hidden;
  }

  .summary-grid {
    @apply grid-cols-1;
  }
}
</style>

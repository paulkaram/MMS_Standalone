<template>
  <div class="space-y-5">
    <PageHeader :title="$t('EmailTemplates')" :subtitle="$t('EmailTemplatesDesc')" :breadcrumbs="[{ label: 'Settings', to: '/settings' }]" />

    <!-- Container -->
    <div class="et-container">
      <!-- Loading -->
      <div v-if="loading" class="et-state">
        <Icon icon="mdi:loading" class="w-6 h-6 animate-spin" style="color: #006d4b" />
      </div>

      <template v-else>
        <!-- Toolbar -->
        <div class="et-toolbar">
          <div class="et-search">
            <Icon icon="mdi:magnify" class="w-4 h-4" style="color: #9ca3af" />
            <input v-model="searchQuery" type="text" :placeholder="$t('SearchTemplates') || 'Search templates...'" />
          </div>
          <span class="et-count">{{ filteredTemplates.length }}/{{ templates.length }}</span>
        </div>

        <!-- Table -->
        <template v-if="filteredTemplates.length > 0">
          <div class="et-table-wrap">
            <table class="data-table">
              <thead>
                <tr>
                  <th style="width: 50px">#</th>
                  <th>{{ $t('TemplateName') || 'Template Name' }}</th>
                  <th>{{ $t('AppCode') || 'App' }}</th>
                  <th>{{ $t('Subject') || 'Subject' }}</th>
                  <th>{{ $t('SendTo') || 'Send To' }}</th>
                  <th style="width: 80px; text-align: center">{{ $t('Actions') || 'Actions' }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(tmpl, idx) in filteredTemplates" :key="tmpl.id">
                  <td class="et-row-num">{{ idx + 1 }}</td>
                  <td>
                    <div class="et-name-cell">
                      <Icon icon="mdi:email-outline" class="w-4 h-4" style="color: #006d4b" />
                      <span class="et-name">{{ tmpl.name }}</span>
                    </div>
                  </td>
                  <td>
                    <span class="et-badge" :class="tmpl.appCode === 'MMS' ? 'et-badge-teal' : 'et-badge-navy'">
                      {{ tmpl.appCode }}
                    </span>
                  </td>
                  <td class="et-subject">{{ tmpl.subject || '—' }}</td>
                  <td class="et-sendto">{{ tmpl.sendTo || '—' }}</td>
                  <td class="et-actions">
                    <button class="et-action-btn" @click="editTemplate(tmpl)" :title="$t('Edit') || 'Edit'">
                      <Icon icon="mdi:pencil" class="w-4 h-4" />
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>

        <!-- Empty -->
        <div v-else class="et-state">
          <Icon icon="mdi:email-off-outline" class="w-10 h-10" style="color: #cbd5e1" />
          <p>{{ $t('NoEmailTemplates') || 'No email templates found' }}</p>
        </div>
      </template>
    </div>

    <!-- Edit Modal -->
    <Modal
      v-model="showModal"
      :title="$t('EditEmailTemplate') || 'Edit Email Template'"
      :description="editingTemplate?.name"
      icon="mdi:email-edit-outline"
      size="3xl"
      scrollable
    >
      <div class="et-form" v-if="editingTemplate">
        <!-- Subject -->
        <div class="et-field">
          <label class="et-label">{{ $t('Subject') || 'Subject' }}</label>
          <input
            v-model="form.subject"
            type="text"
            class="et-input"
            :placeholder="$t('EnterSubject') || 'Enter email subject...'"
          />
        </div>

        <!-- Send To -->
        <div class="et-field">
          <label class="et-label">{{ $t('SendTo') || 'Send To' }}</label>
          <input
            v-model="form.sendTo"
            type="text"
            class="et-input"
            :placeholder="$t('EnterSendTo') || 'Comma-separated emails or leave empty for dynamic...'"
          />
          <p class="et-hint">{{ $t('SendToHint') || 'Leave empty if recipients are determined dynamically' }}</p>
        </div>

        <!-- Body (HTML) -->
        <div class="et-field">
          <label class="et-label">{{ $t('EmailBody') || 'Email Body' }} <span class="et-label-badge">HTML</span></label>
          <div class="et-tabs-row">
            <button
              :class="['et-tab-btn', { active: bodyTab === 'editor' }]"
              @click="bodyTab = 'editor'"
            >
              <Icon icon="mdi:code-tags" class="w-4 h-4" />
              {{ $t('SourceCode') || 'Source' }}
            </button>
            <button
              :class="['et-tab-btn', { active: bodyTab === 'preview' }]"
              @click="bodyTab = 'preview'"
            >
              <Icon icon="mdi:eye" class="w-4 h-4" />
              {{ $t('Preview') || 'Preview' }}
            </button>
          </div>
          <textarea
            v-if="bodyTab === 'editor'"
            v-model="form.body"
            class="et-textarea"
            rows="14"
            :placeholder="$t('EnterEmailBody') || 'Enter HTML email body...'"
            dir="ltr"
          />
          <div
            v-else
            class="et-preview"
            v-html="form.body"
          />
        </div>

        <!-- Placeholders hint -->
        <div class="et-placeholders">
          <Icon icon="mdi:information-outline" class="w-4 h-4" style="color: #006d4b; flex-shrink: 0" />
          <p>{{ $t('PlaceholdersHint') || 'Available placeholders: {createdByName}, {meetingSubject}, {ReferenceNumber}, {meetingNotes}, {startTime}, {endTime}, {meetingDate}, {taskUrl}, {meetingHall}' }}</p>
        </div>
      </div>

      <template #footer>
        <button class="btn-clean" @click="showModal = false">{{ $t('Cancel') || 'Cancel' }}</button>
        <button class="btn-clean primary" :disabled="saving" @click="saveTemplate">
          <Icon v-if="saving" icon="mdi:loading" class="w-4 h-4 animate-spin" />
          {{ $t('Save') || 'Save' }}
        </button>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import Icon from '@/components/ui/Icon.vue'
import Modal from '@/components/ui/Modal.vue'
import EmailTemplatesService, { type EmailTemplate, type UpdateEmailTemplateDto } from '@/services/EmailTemplatesService'
import { useToast } from '@/composables/useToast'

const { success: toastSuccess, error: toastError } = useToast()

const loading = ref(true)
const saving = ref(false)
const templates = ref<EmailTemplate[]>([])
const searchQuery = ref('')
const showModal = ref(false)
const editingTemplate = ref<EmailTemplate | null>(null)
const bodyTab = ref<'editor' | 'preview'>('editor')

const form = ref<UpdateEmailTemplateDto>({
  subject: null,
  body: '',
  sendTo: null
})

const filteredTemplates = computed(() => {
  if (!searchQuery.value) return templates.value
  const q = searchQuery.value.toLowerCase()
  return templates.value.filter(t =>
    t.name.toLowerCase().includes(q) ||
    t.appCode.toLowerCase().includes(q) ||
    (t.subject?.toLowerCase().includes(q))
  )
})

const loadTemplates = async () => {
  try {
    loading.value = true
    const res = await EmailTemplatesService.list()
    templates.value = res.data ?? res
  } catch {
    toastError('Failed to load email templates')
  } finally {
    loading.value = false
  }
}

const editTemplate = (tmpl: EmailTemplate) => {
  editingTemplate.value = tmpl
  form.value = {
    subject: tmpl.subject,
    body: tmpl.body,
    sendTo: tmpl.sendTo
  }
  bodyTab.value = 'editor'
  showModal.value = true
}

const saveTemplate = async () => {
  if (!editingTemplate.value) return
  try {
    saving.value = true
    await EmailTemplatesService.update(editingTemplate.value.id, form.value)
    toastSuccess('Email template updated')
    showModal.value = false
    await loadTemplates()
  } catch {
    toastError('Failed to update template')
  } finally {
    saving.value = false
  }
}

onMounted(loadTemplates)
</script>

<style scoped>
.et-container {
  background: #fff;
  border: 1px solid #e6eaef;
  border-radius: 10px;
  overflow: hidden;
}

.et-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 60px 20px;
  color: #94a3b8;
  font-size: 13px;
}

/* Toolbar */
.et-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid #e6eaef;
  background: #f8fafc;
}

.et-search {
  display: flex;
  align-items: center;
  gap: 8px;
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 6px 10px;
  width: 260px;
}

.et-search input {
  border: none;
  outline: none;
  font-size: 13px;
  width: 100%;
  background: transparent;
}

.et-count {
  font-size: 12px;
  color: #94a3b8;
  font-weight: 500;
}

/* Table */
.et-table-wrap {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.data-table thead {
  background: #f8fafc;
}

.data-table th {
  padding: 10px 14px;
  font-weight: 600;
  color: #475569;
  text-align: start;
  border-bottom: 1px solid #e6eaef;
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.data-table td {
  padding: 10px 14px;
  border-bottom: 1px solid #f1f5f9;
  color: #334155;
}

.data-table tbody tr:hover {
  background: #f8fafc;
}

.et-row-num {
  color: #94a3b8;
  font-size: 12px;
  font-weight: 500;
}

.et-name-cell {
  display: flex;
  align-items: center;
  gap: 8px;
}

.et-name {
  font-weight: 600;
  color: #004730;
}

.et-badge {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.3px;
}

.et-badge-teal {
  background: #e6f9f5;
  color: #007E65;
}

.et-badge-navy {
  background: #e8edf3;
  color: #004730;
}

.et-subject {
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.et-sendto {
  color: #64748b;
  font-size: 12px;
}

.et-actions {
  text-align: center;
}

.et-action-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 30px;
  height: 30px;
  border-radius: 6px;
  border: 1px solid #e2e8f0;
  background: #fff;
  color: #64748b;
  cursor: pointer;
  transition: all 0.15s ease;
}

.et-action-btn:hover {
  border-color: #006d4b;
  color: #006d4b;
  background: #f0fdf9;
}

/* Form */
.et-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 4px 0;
}

.et-field {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.et-label {
  font-size: 13px;
  font-weight: 600;
  color: #334155;
  display: flex;
  align-items: center;
  gap: 6px;
}

.et-label-badge {
  font-size: 10px;
  font-weight: 600;
  background: #004730;
  color: #fff;
  padding: 1px 5px;
  border-radius: 3px;
  letter-spacing: 0.5px;
}

.et-input {
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 8px 12px;
  font-size: 13px;
  outline: none;
  transition: border-color 0.15s, box-shadow 0.15s;
}

.et-input:focus {
  border-color: #006d4b;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

.et-hint {
  font-size: 11px;
  color: #94a3b8;
  margin: 0;
}

/* Tabs */
.et-tabs-row {
  display: flex;
  gap: 4px;
}

.et-tab-btn {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 5px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 5px 5px 0 0;
  background: #f8fafc;
  color: #64748b;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s;
  border-bottom: none;
}

.et-tab-btn.active {
  background: #fff;
  color: #004730;
  border-color: #e2e8f0;
  position: relative;
}

.et-tab-btn.active::after {
  content: '';
  position: absolute;
  bottom: -1px;
  left: 0;
  right: 0;
  height: 1px;
  background: #fff;
}

.et-textarea {
  border: 1px solid #e2e8f0;
  border-radius: 0 6px 6px 6px;
  padding: 10px 12px;
  font-size: 12px;
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  line-height: 1.6;
  outline: none;
  resize: vertical;
  transition: border-color 0.15s, box-shadow 0.15s;
}

.et-textarea:focus {
  border-color: #006d4b;
  box-shadow: 0 0 0 3px rgba(0, 109, 75, 0.1);
}

.et-preview {
  border: 1px solid #e2e8f0;
  border-radius: 0 6px 6px 6px;
  padding: 16px;
  min-height: 200px;
  max-height: 400px;
  overflow-y: auto;
  background: #fff;
  font-size: 13px;
  line-height: 1.6;
}

/* Placeholders hint */
.et-placeholders {
  display: flex;
  gap: 8px;
  align-items: flex-start;
  background: #f0fdf9;
  border: 1px solid #d1fae5;
  border-radius: 6px;
  padding: 10px 12px;
}

.et-placeholders p {
  font-size: 11px;
  color: #475569;
  margin: 0;
  line-height: 1.5;
}

/* Button overrides */
.btn-clean {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 7px 16px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  border: 1px solid #e2e8f0;
  background: #fff;
  color: #475569;
  transition: all 0.15s;
}

.btn-clean:hover {
  background: #f8fafc;
}

.btn-clean.primary {
  background: linear-gradient(90deg, #004730 0%, #006d4b 100%);
  color: #fff;
  border: none;
}

.btn-clean.primary:hover {
  opacity: 0.9;
}

.btn-clean.primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>

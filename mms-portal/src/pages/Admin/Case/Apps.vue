<template>
  <div class="space-y-6">
    <div class="page-header">
      <h1 class="page-title">{{ $t('Apps') }}</h1>
    </div>

    <Card :loading="loading">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 p-4">
        <div
          v-for="app in apps"
          :key="app.workflowId"
          class="border border-zinc-200 rounded-lg p-4 hover:shadow-lg transition-shadow"
        >
          <div class="flex items-start justify-between">
            <div class="flex-1">
              <h3 class="font-semibold text-zinc-900 mb-2">{{ app.title }}</h3>
              <div class="flex items-center gap-2 text-sm text-zinc-600">
                <span>{{ $t('InitiatedWorkflows') }}</span>
                <span class="px-2 py-0.5 bg-secondary-50 text-secondary rounded text-xs font-medium">
                  {{ app.runningFlows }}
                </span>
              </div>
            </div>
            <div class="w-12 h-12 bg-primary-50 rounded-lg flex items-center justify-center">
              <Icon :icon="app.icon || 'mdi:apps'" class="w-6 h-6 text-primary" />
            </div>
          </div>

          <div class="flex items-center gap-2 mt-4 pt-4 border-t border-zinc-100">
            <router-link
              :to="{ name: 'workflow-designer', query: { id: app.workflowDefinitionId } }"
            >
              <Button variant="ghost" size="sm" icon-left="mdi:lead-pencil" class="text-error">
                {{ $t('Edit') }}
              </Button>
            </router-link>
            <Button
              variant="ghost"
              size="sm"
              icon-left="mdi:shield-account"
              class="text-warning"
              @click="setPermissions(app.workflowId)"
            >
              {{ $t('Permissions') }}
            </Button>
          </div>
        </div>

        <div v-if="!apps.length && !loading" class="col-span-full text-center py-12 text-zinc-500">
          <Icon icon="mdi:apps" class="w-16 h-16 mx-auto mb-4 text-zinc-300" />
          <p>{{ $t('NoAppsFound') }}</p>
        </div>
      </div>
    </Card>

    <!-- Permissions Modal -->
    <Modal
      v-model="permissionDialog"
      :title="$t('Permissions')"
      size="lg"
    >
      <div class="p-4">
        <p class="text-zinc-600 mb-4">
          {{ $t('ManagePermissionsFor') }}: {{ selectedApp?.title }}
        </p>
        <!-- Generic Permissions component would go here -->
        <div class="text-center py-8 text-zinc-500">
          <Icon icon="mdi:shield-account" class="w-12 h-12 mx-auto mb-2 text-zinc-300" />
          <p>{{ $t('PermissionsConfigurationComingSoon') }}</p>
        </div>
      </div>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'
import Button from '@/components/ui/Button.vue'
import Modal from '@/components/ui/Modal.vue'
import ProcessesService, { type ProcessApp } from '@/services/ProcessesService'
import { useToast } from '@/composables/useToast'
import { useI18n } from 'vue-i18n'

const { toast } = useToast()
const { t } = useI18n()

const loading = ref(false)
const apps = ref<ProcessApp[]>([])
const permissionDialog = ref(false)
const selectedApp = ref<ProcessApp | null>(null)

const listWorkflows = async () => {
  loading.value = true
  try {
    apps.value = await ProcessesService.listProcesses()
  } catch {
    toast.error(t('ErrorOccurred'))
  } finally {
    loading.value = false
  }
}

const setPermissions = (workflowId: number) => {
  selectedApp.value = apps.value.find(a => a.workflowId === workflowId) || null
  permissionDialog.value = true
}

onMounted(() => {
  listWorkflows()
})
</script>

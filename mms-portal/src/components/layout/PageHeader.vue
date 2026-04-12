<template>
  <div class="dashboard-header">
    <div class="header-main">
      <div class="header-content">
        <h1 class="dashboard-title">{{ title }}</h1>
        <p v-if="subtitle" class="dashboard-subtitle">{{ subtitle }}</p>
      </div>
      <div v-if="$slots.actions" class="header-actions">
        <slot name="actions" />
      </div>
    </div>
  </div>

  <!-- Breadcrumb (outside header container) -->
  <nav v-if="breadcrumbs && breadcrumbs.length" class="header-breadcrumb">
    <template v-for="(crumb, index) in breadcrumbs" :key="index">
      <RouterLink :to="crumb.to" class="breadcrumb-link">
        {{ $t(crumb.label) || crumb.label }}
      </RouterLink>
      <span class="breadcrumb-sep">
        <Icon icon="mdi:chevron-left" class="w-3.5 h-3.5" />
      </span>
    </template>
    <span class="breadcrumb-current">{{ $t(title) || title }}</span>
  </nav>
</template>

<script setup lang="ts">
import Icon from '@/components/ui/Icon.vue'

interface BreadcrumbItem {
  label: string
  to: string
}

defineProps<{
  title: string
  subtitle?: string
  breadcrumbs?: BreadcrumbItem[]
}>()
</script>

<style scoped>
/* Page Header — matching IAM .dashboard-header exactly */
.dashboard-header {
  position: relative;
  background: #ffffff;
  margin: -20px -24px 20px -24px;
  padding: 20px 24px;
}

.dashboard-header::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: #e5e7eb;
}

/* Breadcrumb — sits outside the header container */
.header-breadcrumb {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  padding: 10px 0;
  margin-bottom: 4px;
}

.breadcrumb-link {
  color: #006d4b;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.2s;
}

.breadcrumb-link:hover {
  color: #007E65;
  text-decoration: underline;
}

.breadcrumb-sep {
  color: #9ca3af;
  display: flex;
  align-items: center;
}

[dir="ltr"] .breadcrumb-sep {
  transform: rotate(180deg);
}

.breadcrumb-current {
  color: #6b7280;
  font-weight: 400;
}

.header-main {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 24px;
}

.header-content {
  flex: 1;
  min-width: 0;
}

.dashboard-title {
  font-size: 24px;
  font-weight: 700;
  color: #006d4b;
  margin: 0;
  letter-spacing: -0.3px;
}

.dashboard-subtitle {
  color: #6b7280;
  margin: 6px 0 0 0;
  font-size: 14px;
  font-weight: 400;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-shrink: 0;
}

/* Action button styles — matching IAM .btn-clean */
.header-actions :deep(.btn-clean) {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
  padding: 0.5rem 0.875rem;
  border-radius: 6px;
  font-weight: 500;
  font-size: 14px;
  line-height: 1.25rem;
  transition: all 0.2s;
  cursor: pointer;
  border: 1px solid transparent;
  text-decoration: none;
}

.header-actions :deep(.btn-clean.primary) {
  background: linear-gradient(135deg, rgb(15 23 42) 0%, rgb(0 174 141) 100%);
  color: white;
  border: none;
  box-shadow: 0 4px 12px rgba(0, 109, 75, 0.25), inset 0 1px 0 rgba(255, 255, 255, 0.1);
}

.header-actions :deep(.btn-clean.primary:hover) {
  background: linear-gradient(135deg, rgb(15 23 42) 10%, rgb(0 174 141) 90%);
  transform: translateY(-1px);
  box-shadow: 0 6px 16px rgba(0, 109, 75, 0.35), inset 0 1px 0 rgba(255, 255, 255, 0.1);
}

.header-actions :deep(.btn-clean.secondary) {
  background: white;
  color: #64748b;
  border-color: #cbd5e1;
}

.header-actions :deep(.btn-clean.secondary:hover) {
  background: #f8fafc;
  color: #475569;
  border-color: #94a3b8;
}

@media (max-width: 1023px) {
  .dashboard-header {
    margin: -16px -16px 16px -16px;
    padding: 16px;
  }
}

@media (max-width: 480px) {
  .dashboard-header {
    margin: -12px -12px 12px -12px;
    padding: 12px;
  }

  .header-main {
    flex-direction: column;
    gap: 12px;
  }

  .header-breadcrumb {
    font-size: 12px;
  }
}
</style>

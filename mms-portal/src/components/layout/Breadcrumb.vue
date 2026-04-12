<template>
  <nav class="settings-breadcrumb">
    <template v-for="(crumb, index) in items" :key="index">
      <RouterLink :to="crumb.to" class="breadcrumb-link">
        {{ $t(crumb.label) || crumb.label }}
      </RouterLink>
      <span class="breadcrumb-separator">
        <Icon icon="mdi:chevron-left" class="w-3.5 h-3.5" />
      </span>
    </template>
    <span class="breadcrumb-current">{{ $t(current) || current }}</span>
  </nav>
</template>

<script setup lang="ts">
import Icon from '@/components/ui/Icon.vue'

interface BreadcrumbItem {
  label: string
  to: string
}

defineProps<{
  items: BreadcrumbItem[]
  current: string
}>()
</script>

<style scoped>
.settings-breadcrumb {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  padding: 12px 32px;
  background: transparent;
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

.breadcrumb-separator {
  color: #9ca3af;
  display: flex;
  align-items: center;
}

[dir="ltr"] .breadcrumb-separator {
  transform: rotate(180deg);
}

.breadcrumb-current {
  color: #6b7280;
  font-weight: 400;
}

@media (max-width: 640px) {
  .settings-breadcrumb {
    padding: 10px 16px;
    font-size: 12px;
  }
}
</style>

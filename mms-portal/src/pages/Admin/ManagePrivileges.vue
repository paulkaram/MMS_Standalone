<template>
  <div class="space-y-6">
    <div class="page-header">
      <h1 class="page-title">{{ $t('ManagePrivileges') }}</h1>
    </div>
    <Card>
      <div class="text-center py-12 text-zinc-500">
        <Icon icon="mdi:shield-key" class="w-16 h-16 mx-auto mb-4 text-zinc-300" />
        <p>سيتم عرض إدارة الصلاحيات هنا</p>
      </div>
    </Card>
  </div>
</template>

<script setup lang="ts">
import Icon from '@/components/ui/Icon.vue'
import Card from '@/components/ui/Card.vue'
</script>

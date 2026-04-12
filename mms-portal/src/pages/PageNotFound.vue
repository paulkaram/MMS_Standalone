<template>
  <div class="min-h-screen flex items-center justify-center bg-zinc-50 py-12 px-4">
    <div class="text-center">
      <!-- 404 Icon -->
      <div class="w-32 h-32 mx-auto bg-primary/10 rounded-full flex items-center justify-center mb-8">
        <Icon
          icon="mdi:file-question-outline"
          class="w-16 h-16 text-primary"
        />
      </div>

      <!-- Error code -->
      <h1 class="text-6xl font-bold text-zinc-900 mb-4">404</h1>

      <!-- Message -->
      <h2 class="text-2xl font-semibold text-zinc-700 mb-2">
        {{ $t('PageNotFound') }}
      </h2>
      <p class="text-zinc-500 mb-8">
        {{ $t('PageNotFoundMessage') }}
      </p>

      <!-- Actions -->
      <div class="flex items-center justify-center gap-4">
        <Button
          variant="primary"
          icon-left="mdi:home"
          @click="$router.push('/home')"
        >
          {{ $t('GoHome') }}
        </Button>
        <Button
          variant="outline"
          icon-left="mdi:arrow-right"
          @click="$router.back()"
        >
          {{ $t('GoBack') }}
        </Button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import Icon from '@/components/ui/Icon.vue'
import Button from '@/components/ui/Button.vue'
</script>

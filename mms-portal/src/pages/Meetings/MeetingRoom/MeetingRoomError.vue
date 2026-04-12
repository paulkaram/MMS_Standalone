<template>
  <div class="errorState">
    <Icon icon="warning" class="errorIcon" style="font-size: 48px;" />
    <h2>{{ $t('MeetingNotFound') }}</h2>
    <p>{{ $t('FailedToLoadMeeting') }}</p>
    <button class="btn primary" @click="$emit('go-back')">{{ $t('GoBack') }}</button>
  </div>
</template>

<script setup lang="ts">
import Icon from '@/components/ui/Icon.vue'

defineEmits<{
  (e: 'go-back'): void
}>()
</script>

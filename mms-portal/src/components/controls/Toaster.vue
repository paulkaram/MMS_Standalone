<template>
  <!-- Toast container is handled by vue-toastification -->
  <!-- This component serves as a wrapper and provides custom toast methods -->
  <div />
</template>

<script setup lang="ts">
// Toast notifications are handled by vue-toastification
// This component provides a convenient API for showing toasts

import { useToast } from '@/composables/useToast'

const { toast } = useToast()

// Expose methods
defineExpose({
  success: toast.success,
  error: toast.error,
  warning: toast.warning,
  info: toast.info,
  clear: toast.clear,
  toast
})
</script>

<template>
  <div class="flex items-center justify-center min-h-[200px]">
    <div class="text-center">
      <!-- Spinner -->
      <div class="relative w-12 h-12 mx-auto mb-4">
        <div class="absolute inset-0 border-4 border-zinc-200 rounded-full" />
        <div class="absolute inset-0 border-4 border-primary rounded-full animate-spin border-t-transparent" />
      </div>

      <!-- Text -->
      <p
        v-if="message"
        class="text-zinc-500 text-sm"
      >
        {{ message }}
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
// Props
interface Props {
  message?: string
}

withDefaults(defineProps<Props>(), {
  message: undefined
})
</script>

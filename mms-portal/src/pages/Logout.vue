<template>
  <div class="min-h-screen flex items-center justify-center bg-zinc-50">
    <div class="text-center">
      <div class="relative w-16 h-16 mx-auto mb-6">
        <div class="absolute inset-0 border-4 border-zinc-200 rounded-full" />
        <div class="absolute inset-0 border-4 border-primary rounded-full animate-spin border-t-transparent" />
      </div>
      <p class="text-zinc-600">جاري تسجيل الخروج...</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { clearTranslation } from '@/plugins/i18n'
import { resetPermissionCache } from '@/router'
import AuthService from '@/services/AuthService'

const router = useRouter()
const userStore = useUserStore()

onMounted(async () => {
  // Call backend to clear Redis cache + refresh tokens
  try {
    await AuthService.logout()
  } catch {
    // Continue logout even if backend call fails
  }

  // Clear user session
  userStore.removeUserSession()

  // Clear cached permissions
  resetPermissionCache()

  // Clear translations
  clearTranslation()

  // Redirect to login
  await router.push('/login')
})
</script>

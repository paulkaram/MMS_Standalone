<template>
  <div class="min-h-screen flex items-center justify-center bg-zinc-50">
    <div class="text-center">
      <div
        v-if="!error"
        class="relative w-16 h-16 mx-auto mb-6"
      >
        <div class="absolute inset-0 border-4 border-zinc-200 rounded-full" />
        <div class="absolute inset-0 border-4 border-primary rounded-full animate-spin border-t-transparent" />
      </div>

      <div
        v-if="error"
        class="w-16 h-16 mx-auto mb-6 bg-error/10 rounded-full flex items-center justify-center"
      >
        <Icon
          icon="mdi:alert-circle"
          class="w-8 h-8 text-error"
        />
      </div>

      <p
        v-if="!error"
        class="text-zinc-600"
      >
        {{ $t('ProcessingLogin') }}
      </p>

      <template v-else>
        <p class="text-error mb-4">{{ error }}</p>
        <Button
          variant="primary"
          @click="$router.push('/login')"
        >
          {{ $t('ReturnToLogin') }}
        </Button>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import { useUserStore } from '@/stores/user'
import AuthService from '@/services/AuthService'
import Button from '@/components/ui/Button.vue'

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()

const error = ref('')
let exchanged = false

onMounted(async () => {
  // Guard against double-mount (HMR / StrictMode / router re-navigation)
  if (exchanged) return
  exchanged = true

  const code = route.query.code as string
  const state = route.query.state as string

  if (!code) {
    error.value = 'لم يتم العثور على رمز التفويض'
    return
  }

  try {
    // Exchange authorization code for tokens via MMS backend
    // redirect_uri is configured on backend (IamOAuth:RedirectUri) — no need to send from frontend
    // Retry up to 3 times to handle cold API starts (IIS app pool warming up)
    let response: any = null
    const maxRetries = 3
    for (let attempt = 0; attempt <= maxRetries; attempt++) {
      try {
        response = await AuthService.exchangeOidcCode(code)
        break // success — exit retry loop
      } catch (retryErr: any) {
        const status = retryErr.response?.status
        const isTransient = !status || status === 502 || status === 503 || status === 504 || retryErr.code === 'ECONNABORTED'
        if (isTransient && attempt < maxRetries) {
          // Wait with exponential backoff: 2s, 4s, 8s
          await new Promise(r => setTimeout(r, 2000 * Math.pow(2, attempt)))
          continue
        }
        throw retryErr // not transient or exhausted retries
      }
    }

    // Unwrap ApiResponseDto wrapper (backend returns { data: {...}, success, message })
    const responseData = response.data || response

    // Check for locked account
    if (responseData.statusCode === 423) {
      error.value = responseData.message || 'الحساب مقفل'
      return
    }

    // Check if 2FA is required
    if (responseData.statusCode === 206) {
      const userInfo = responseData.userInfo
      userStore.setUser2FA({
        userId: userInfo?.id || userInfo?.userId || '',
        method: userInfo?.method || 'sms',
        verified: false
      })
      await router.push({ name: '2FA' })
      return
    }

    const token = responseData.token || responseData.accessToken
    const refreshToken = responseData.refreshToken || responseData.RefreshToken
    const iamToken = responseData.iamAccessToken || null
    const userData = responseData.user || responseData.userInfo

    if (!token || !userData) {
      error.value = response.message || 'فشل تسجيل الدخول'
      console.error('Invalid OIDC response:', response)
      return
    }

    // Build profile picture URL from IAM (same pattern as Case Portal)
    const iamAuthority = import.meta.env.VITE_IAM_AUTHORITY || ''
    let iamUserId = ''
    let hasProfilePicture = userData.hasProfilePicture || false
    if (iamToken) {
      try {
        const payload = JSON.parse(atob(iamToken.split('.')[1]))
        iamUserId = payload.sub || payload.UserId || payload.userId || ''
        if (payload.HasProfileImage === 'true' || payload.hasProfileImage === 'true') {
          hasProfilePicture = true
        }
      } catch { /* ignore decode error */ }
    }
    const profilePictureUrl = hasProfilePicture && iamUserId && iamAuthority
      ? `${iamAuthority}/api/user/${iamUserId}/profile-image?t=${Date.now()}`
      : null
    const userIdStr = userData.id?.toString() || ''

    // Store user session
    userStore.setUserSession({
      user: {
        id: userIdStr,
        username: userData.username || userData.email || '',
        fullName: userData.fullnameAr || userData.fullnameEn || userData.fullName || '',
        email: userData.email || '',
        language: userData.language || 'ar',
        profilePictureUrl,
        hasProfilePicture,
        roles: userData.roles || [],
        permissions: userData.permissions || []
      },
      token,
      refreshToken,
      iamToken
    })

    // Redirect to home or intended page (iam-sso is a marker, not a route)
    // Use replace so back-navigation won't re-trigger the code exchange
    const returnUrl = (!state || state === 'iam-sso') ? '/home' : decodeURIComponent(state)
    await router.replace(returnUrl)
  } catch (err: any) {
    console.error('OIDC login error:', err)
    error.value = err.response?.data?.message || err.message || 'فشل تسجيل الدخول'
  }
})
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-zinc-50 py-12 px-4">
    <div class="max-w-md w-full">
      <Card>
        <div class="text-center mb-6">
          <div class="w-16 h-16 mx-auto bg-primary/10 rounded-full flex items-center justify-center mb-4">
            <Icon
              icon="mdi:shield-check-outline"
              class="w-8 h-8 text-primary"
            />
          </div>
          <h2 class="text-xl font-bold text-zinc-900">
            {{ $t('TwoFactorAuth') }}
          </h2>
          <p class="text-sm text-zinc-500 mt-2">
            {{ $t('TwoFactorAuthMessage') }}
          </p>
        </div>

        <form
          class="space-y-6"
          @submit.prevent="handleVerify"
        >
          <!-- OTP Input -->
          <div class="flex justify-center gap-2">
            <input
              v-for="(_, index) in 6"
              :key="index"
              ref="otpInputs"
              v-model="otpDigits[index]"
              type="text"
              maxlength="1"
              class="w-12 h-14 text-center text-xl font-bold border border-zinc-300 rounded-lg focus:border-primary focus:ring-2 focus:ring-primary/20 focus:outline-none"
              @input="handleOtpInput(index)"
              @keydown="handleOtpKeydown($event, index)"
            >
          </div>

          <!-- Error -->
          <p
            v-if="error"
            class="text-sm text-error text-center"
          >
            {{ error }}
          </p>

          <!-- Submit -->
          <Button
            type="submit"
            variant="primary"
            block
            :loading="isLoading"
          >
            {{ $t('Verify') }}
          </Button>

          <!-- Resend -->
          <p class="text-center text-sm text-zinc-500">
            {{ $t('DidntReceiveCode') }}
            <button
              type="button"
              class="text-primary hover:underline"
              :disabled="resendCountdown > 0"
              @click="handleResend"
            >
              {{ resendCountdown > 0 ? `إعادة الإرسال (${resendCountdown})` : 'إعادة الإرسال' }}
            </button>
          </p>
        </form>
      </Card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import { useUserStore } from '@/stores/user'
import AuthService from '@/services/AuthService'
import Card from '@/components/ui/Card.vue'
import Button from '@/components/ui/Button.vue'
import { useToast } from '@/composables/useToast'

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()
const { toast } = useToast()

const otpDigits = ref(['', '', '', '', '', ''])
const otpInputs = ref<HTMLInputElement[]>([])
const isLoading = ref(false)
const isResending = ref(false)
const error = ref('')
const resendCountdown = ref(0)

// Get user info from route state or store
const userId = ref(route.query.userId as string || userStore.twoFAUserId || '')
const twoFAMethod = ref(route.query.method as string || userStore.twoFAMethod || 'sms')

let countdownInterval: number | null = null

function handleOtpInput(index: number) {
  const value = otpDigits.value[index]
  if (value && index < 5) {
    otpInputs.value[index + 1]?.focus()
  }
}

function handleOtpKeydown(event: KeyboardEvent, index: number) {
  if (event.key === 'Backspace' && !otpDigits.value[index] && index > 0) {
    otpInputs.value[index - 1]?.focus()
  }
}

async function handleVerify() {
  const otp = otpDigits.value.join('')
  if (otp.length !== 6) {
    error.value = 'الرجاء إدخال الرمز كاملاً'
    return
  }

  isLoading.value = true
  error.value = ''

  try {
    const response = await AuthService.checkVerificationCode({
      userId: userId.value,
      method: twoFAMethod.value,
      code: otp
    })

    // Construct profile picture URL if user has one
    const userData = response.user
    const userIdStr = userData.id?.toString() || ''
    const hasProfilePicture = userData.hasProfilePicture || false
    const baseUrl = import.meta.env.VITE_API_URL || ''
    const profilePictureUrl = hasProfilePicture
      ? `${baseUrl}/api/users/profile-image/${userIdStr}?t=${Date.now()}`
      : null

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
      token: response.token,
      refreshToken: response.refreshToken
    })

    // Clear 2FA state
    userStore.clearTwoFAState()

    toast.success('تم التحقق بنجاح')
    await router.push('/home')
  } catch (err: any) {
    error.value = err.response?.data?.message || err.message || 'رمز التحقق غير صحيح'
    // Clear OTP inputs on error
    otpDigits.value = ['', '', '', '', '', '']
    otpInputs.value[0]?.focus()
  } finally {
    isLoading.value = false
  }
}

async function handleResend() {
  if (resendCountdown.value > 0 || isResending.value) return

  isResending.value = true
  error.value = ''

  try {
    await AuthService.requestVerificationCode({
      userId: userId.value,
      method: twoFAMethod.value
    })

    toast.success('تم إعادة إرسال الرمز')
    resendCountdown.value = 60
  } catch (err: any) {
    error.value = err.response?.data?.message || 'فشل في إعادة إرسال الرمز'
  } finally {
    isResending.value = false
  }
}

function startCountdown() {
  countdownInterval = window.setInterval(() => {
    if (resendCountdown.value > 0) {
      resendCountdown.value--
    }
  }, 1000)
}

onMounted(() => {
  // Redirect to login if no user ID
  if (!userId.value) {
    router.push('/login')
    return
  }

  startCountdown()
  resendCountdown.value = 60

  // Focus first input
  setTimeout(() => {
    otpInputs.value[0]?.focus()
  }, 100)
})

onUnmounted(() => {
  if (countdownInterval) {
    clearInterval(countdownInterval)
  }
})
</script>

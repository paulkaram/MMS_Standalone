<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useUserStore } from '@/stores/user'
import AuthService from '@/services/AuthService'
import { encryptPassword } from '@/helpers/encryption'
import { setLocale } from '@/plugins/i18n'
import Icon from '@/components/ui/Icon.vue'
import misaLogo from '@/assets/misa_logo.svg'

const { t } = useI18n()
const router = useRouter()
const userStore = useUserStore()

const isRtl = computed(() => userStore.isRtl)
const mounted = ref(false)

onMounted(() => {
  setTimeout(() => { mounted.value = true }, 50)
})

function toggleLanguage() {
  const newLang = userStore.language === 'ar' ? 'en' : 'ar'
  userStore.setLanguage(newLang)
  setLocale(newLang as 'ar' | 'en')
}

const form = reactive({
  username: '',
  password: ''
})

const errors = reactive({
  username: '',
  password: ''
})

const isLoading = ref(false)
const loginError = ref('')
const showPassword = ref(false)

async function handleLogin() {
  errors.username = ''
  errors.password = ''
  loginError.value = ''

  if (!form.username) {
    errors.username = t('UsernameRequired')
    return
  }
  if (!form.password) {
    errors.password = t('PasswordRequired')
    return
  }

  isLoading.value = true

  try {
    const encryptedPassword = encryptPassword(form.password)
    const response = await AuthService.login(form.username, encryptedPassword)

    if (response.statusCode === 206) {
      const userInfo = (response as any).userInfo
      userStore.setUser2FA({
        userId: userInfo?.id || userInfo?.userId || '',
        method: userInfo?.method || 'sms',
        verified: false
      })
      await router.push({ name: '2FA' })
      return
    }

    if (response.statusCode === 423) {
      loginError.value = t('AccountLocked')
      return
    }

    const responseData = (response as any).data || response
    const token = responseData.token || responseData.accessToken
    const refreshToken = responseData.refreshToken || responseData.RefreshToken
    const userData = responseData.user || responseData.userInfo

    if (!token || !userData) {
      loginError.value = t('ServerError')
      return
    }

    const userId = userData.id?.toString() || ''
    const hasProfilePicture = userData.hasProfilePicture || false
    const baseUrl = import.meta.env.VITE_API_URL || ''
    const profilePictureUrl = hasProfilePicture
      ? `${baseUrl}/api/users/profile-image/${userId}?t=${Date.now()}`
      : null

    const user = {
      id: userId,
      username: userData.username || userData.email || '',
      fullName: userData.fullnameAr || userData.fullnameEn || userData.fullName || '',
      email: userData.email || '',
      language: userData.language || 'ar',
      profilePictureUrl,
      hasProfilePicture,
      roles: userData.roles || [],
      permissions: userData.permissions || []
    }

    userStore.setUserSession({ user, token, refreshToken })
    await router.replace('/home')
  } catch (error: any) {
    const errorData = error.response?.data
    if (typeof errorData === 'string') loginError.value = errorData
    else if (errorData?.message) loginError.value = errorData.message
    else loginError.value = t('InvalidCredentials')
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <div class="misa-login" :dir="isRtl ? 'rtl' : 'ltr'" :class="{ 'is-mounted': mounted }">

    <!-- ========== FORM HALF ========== -->
    <section class="half half--form">
      <!-- subtle geometric accent top-corner -->
      <svg class="corner-geo" viewBox="0 0 320 320" fill="none" aria-hidden="true">
        <path d="M0 0H160V160H0z" fill="#006d4b" opacity=".03"/>
        <path d="M160 0C160 88.37 88.37 160 0 160V0h160z" fill="#63a58f" opacity=".05"/>
        <path d="M160 0h160v160H160z" fill="#006d4b" opacity=".02"/>
        <circle cx="240" cy="80" r="40" fill="#63a58f" opacity=".04"/>
        <path d="M0 160h160v160H0z" fill="#63a58f" opacity=".025"/>
        <path d="M0 320V160l80 80L0 320z" fill="#006d4b" opacity=".03"/>
      </svg>

      <div class="form-wrap">
        <!-- Language toggle -->
        <button class="lang-toggle" @click="toggleLanguage" type="button">
          <span class="lang-toggle__label">{{ isRtl ? 'EN' : 'عربي' }}</span>
        </button>

        <!-- Logo -->
        <div class="logo-block">
          <img :src="misaLogo" alt="MISA" class="logo-img" />
        </div>

        <!-- Divider accent -->
        <div class="accent-rule" aria-hidden="true">
          <span class="accent-rule__seg accent-rule__seg--short"></span>
          <span class="accent-rule__seg accent-rule__seg--long"></span>
          <span class="accent-rule__seg accent-rule__seg--short"></span>
        </div>

        <!-- Title -->
        <h1 class="form-title">{{ $t('ApplicationName') }}</h1>
        <p class="form-subtitle">{{ $t('Login') }}</p>

        <!-- Error banner -->
        <Transition name="err-slide">
          <div v-if="loginError" class="err-banner" role="alert">
            <Icon icon="mdi:alert-circle-outline" class="w-[18px] h-[18px]" />
            <span>{{ loginError }}</span>
          </div>
        </Transition>

        <!-- Form -->
        <form @submit.prevent="handleLogin" class="login-form" autocomplete="on">
          <!-- Username -->
          <div class="fld">
            <label class="fld__label">{{ $t('Username') }}</label>
            <div class="fld__box" :class="{ 'fld__box--err': errors.username }">
              <span class="fld__ico">
                <Icon icon="mdi:account-outline" class="w-5 h-5" />
              </span>
              <input
                v-model="form.username"
                type="text"
                class="fld__input"
                :placeholder="$t('EnterUsername')"
                autocomplete="username"
              />
            </div>
            <Transition name="err-slide">
              <span v-if="errors.username" class="fld__err">{{ errors.username }}</span>
            </Transition>
          </div>

          <!-- Password -->
          <div class="fld">
            <label class="fld__label">{{ $t('Password') }}</label>
            <div class="fld__box" :class="{ 'fld__box--err': errors.password }">
              <span class="fld__ico">
                <Icon icon="mdi:lock-outline" class="w-5 h-5" />
              </span>
              <input
                v-model="form.password"
                :type="showPassword ? 'text' : 'password'"
                class="fld__input"
                :placeholder="$t('EnterPassword')"
                autocomplete="current-password"
              />
              <button type="button" class="fld__eye" @click="showPassword = !showPassword" tabindex="-1">
                <Icon :icon="showPassword ? 'mdi:eye-off-outline' : 'mdi:eye-outline'" class="w-[18px] h-[18px]" />
              </button>
            </div>
            <Transition name="err-slide">
              <span v-if="errors.password" class="fld__err">{{ errors.password }}</span>
            </Transition>
          </div>

          <!-- Submit -->
          <button type="submit" class="btn-login" :disabled="isLoading">
            <span v-if="isLoading" class="btn-login__spinner"></span>
            <span>{{ isLoading ? $t('LoggingIn') : $t('Login') }}</span>
            <Icon v-if="!isLoading" :icon="isRtl ? 'mdi:arrow-left' : 'mdi:arrow-right'" class="w-5 h-5" />
          </button>
        </form>

        <!-- Footer -->
        <footer class="form-footer">
          &copy; {{ new Date().getFullYear() }}&ensp;{{ isRtl ? 'وزارة الاستثمار' : 'Ministry of Investment' }}
        </footer>
      </div>
    </section>

    <!-- ========== MOSAIC HALF ========== -->
    <section class="half half--mosaic">
      <!-- Layer 1: Full-bleed pattern at strong opacity (the star of the show) -->
      <svg class="mosaic mosaic--bg" viewBox="0 0 783 320" fill="none" xmlns="http://www.w3.org/2000/svg" preserveAspectRatio="xMidYMid slice" aria-hidden="true">
        <rect width="160" height="152" transform="translate(631 320) rotate(-90)" fill="#e0ede7"/>
        <path d="M707 160L707 240C748.974 240 783 204.183 783 160L707 160Z" fill="#63a58f"/>
        <path d="M631 240C631 195.817 665.026 160 707 160L707 240L631 240Z" fill="#005e3f"/>
        <rect width="160" height="152" transform="translate(479 160) rotate(-90)" fill="#e0ede7"/>
        <path d="M555 80L555 0L631 80L555 80Z" fill="#63a58f"/>
        <path d="M479 80L555 80L555 160L479 80Z" fill="#63a58f"/>
        <rect width="160" height="152" transform="translate(631 160) rotate(-90)" fill="#8fc4b0"/>
        <path d="M783 80C783 124.183 748.974 160 707 160C665.026 160 631 124.183 631 80L783 80Z" fill="#005e3f"/>
        <ellipse cx="707" cy="80" rx="40" ry="38" transform="rotate(-90 707 80)" fill="#e0ede7"/>
        <rect width="160" height="152" transform="translate(479 320) rotate(-90)" fill="#8fc4b0"/>
        <path d="M479 160L631 160L631 320L479 160Z" fill="#005e3f"/>
        <ellipse cx="555" cy="240" rx="40" ry="38" transform="rotate(-90 555 240)" fill="#e0ede7"/>
        <rect width="160" height="152" transform="translate(480 158) rotate(-180)" fill="#8fc4b0"/>
        <path d="M480 82L400 82C400 40.0264 435.817 6 480 6L480 82Z" fill="#e0ede7"/>
        <path d="M400 158C355.817 158 320 123.974 320 82L400 82L400 158Z" fill="#005e3f"/>
        <rect width="160" height="152" transform="translate(320 158) rotate(-180)" fill="#e0ede7"/>
        <path d="M160 5.99999C204.183 6 240 40.0264 240 82C240 123.974 204.183 158 160 158L160 5.99999Z" fill="#005e3f"/>
        <ellipse cx="280" cy="82" rx="40" ry="38" transform="rotate(-180 280 82)" fill="#8fc4b0"/>
        <rect width="160" height="152" transform="translate(160 158) rotate(-180)" fill="#8fc4b0"/>
        <path d="M160 6C115.817 6 80 40.0263 80 82C80 123.974 115.817 158 160 158L160 6Z" fill="#005e3f"/>
        <path d="M0 158L0 6L80 82L0 158Z" fill="#e0ede7"/>
        <rect width="160" height="152" transform="translate(479 311) rotate(-180)" fill="#e0ede7"/>
        <path d="M319 235L399 235C399 193.026 363.183 159 319 159L319 235Z" fill="#63a58f"/>
        <path d="M399 311C354.817 311 319 276.974 319 235L399 235L399 311Z" fill="#005e3f"/>
        <rect width="160" height="152" transform="translate(319 311) rotate(-180)" fill="#8fc4b0"/>
        <path d="M239 159C283.183 159 319 193.026 319 235C319 276.974 283.183 311 239 311L239 159Z" fill="#005e3f"/>
        <ellipse cx="239" cy="235" rx="40" ry="38" transform="rotate(-180 239 235)" fill="#e0ede7"/>
      </svg>

      <!-- Layer 2: Second copy, offset + rotated for richer coverage -->
      <svg class="mosaic mosaic--layer2" viewBox="0 0 783 320" fill="none" xmlns="http://www.w3.org/2000/svg" preserveAspectRatio="xMidYMid slice" aria-hidden="true">
        <rect width="160" height="152" transform="translate(631 320) rotate(-90)" fill="#d5e8df"/>
        <path d="M707 160L707 240C748.974 240 783 204.183 783 160L707 160Z" fill="#4f9a80"/>
        <path d="M631 240C631 195.817 665.026 160 707 160L707 240L631 240Z" fill="#006d4b"/>
        <rect width="160" height="152" transform="translate(479 160) rotate(-90)" fill="#d5e8df"/>
        <path d="M555 80L555 0L631 80L555 80Z" fill="#4f9a80"/>
        <path d="M479 80L555 80L555 160L479 80Z" fill="#4f9a80"/>
        <rect width="160" height="152" transform="translate(631 160) rotate(-90)" fill="#82b9a4"/>
        <path d="M783 80C783 124.183 748.974 160 707 160C665.026 160 631 124.183 631 80L783 80Z" fill="#006d4b"/>
        <ellipse cx="707" cy="80" rx="40" ry="38" transform="rotate(-90 707 80)" fill="#d5e8df"/>
        <rect width="160" height="152" transform="translate(479 320) rotate(-90)" fill="#82b9a4"/>
        <path d="M479 160L631 160L631 320L479 160Z" fill="#006d4b"/>
        <ellipse cx="555" cy="240" rx="40" ry="38" transform="rotate(-90 555 240)" fill="#d5e8df"/>
        <rect width="160" height="152" transform="translate(480 158) rotate(-180)" fill="#82b9a4"/>
        <path d="M480 82L400 82C400 40.0264 435.817 6 480 6L480 82Z" fill="#d5e8df"/>
        <path d="M400 158C355.817 158 320 123.974 320 82L400 82L400 158Z" fill="#006d4b"/>
        <rect width="160" height="152" transform="translate(320 158) rotate(-180)" fill="#d5e8df"/>
        <path d="M160 6C204.183 6 240 40.0264 240 82C240 123.974 204.183 158 160 158L160 6Z" fill="#006d4b"/>
        <ellipse cx="280" cy="82" rx="40" ry="38" transform="rotate(-180 280 82)" fill="#82b9a4"/>
        <rect width="160" height="152" transform="translate(160 158) rotate(-180)" fill="#82b9a4"/>
        <path d="M160 6C115.817 6 80 40.0263 80 82C80 123.974 115.817 158 160 158L160 6Z" fill="#006d4b"/>
        <path d="M0 158L0 6L80 82L0 158Z" fill="#d5e8df"/>
      </svg>

      <!-- Scrim for text readability -->
      <div class="mosaic-scrim"></div>

      <!-- Center text overlay -->
      <div class="mosaic-content">
        <div class="mosaic-badge">
          <svg viewBox="0 0 32 32" fill="none" stroke="currentColor" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round">
            <path d="M16 3L3 10l13 7 13-7-13-7z"/>
            <path d="M3 22l13 7 13-7"/>
            <path d="M3 16l13 7 13-7"/>
          </svg>
        </div>
        <h2 class="mosaic-title">
          {{ isRtl ? 'نظام إدارة الاجتماعات' : 'Meeting Management' }}
          <span>{{ isRtl ? '' : 'System' }}</span>
        </h2>
        <div class="mosaic-rule"></div>
        <p class="mosaic-ministry">{{ isRtl ? 'وزارة الاستثمار' : 'Ministry of Investment' }}</p>
      </div>
    </section>
  </div>
</template>

<style scoped>
/* ================================================================
   MISA LOGIN — "Institutional Mosaic"
   ================================================================ */

/* --- Variables --- */
.misa-login {
  --misa-primary: #006d4b;
  --misa-primary-dark: #005339;
  --misa-secondary: #63a58f;
  --misa-cream: #f4f8f6;
  --misa-sage: #e4ede8;
  --misa-text: #1c3028;
  --misa-text-muted: #6b8a7d;
  --misa-border: #d0ddd6;
  --misa-error: #c0392b;

  display: flex;
  min-height: 100vh;
  font-family: 'Tajawal', sans-serif;
  background: #fff;
}

/* --- Reveal orchestration --- */
.half--form .form-wrap { opacity: 0; transform: translateY(18px); transition: opacity .55s ease, transform .55s ease; }
.half--mosaic .mosaic-content { opacity: 0; transform: translateY(24px); transition: opacity .6s ease .25s, transform .6s ease .25s; }
.half--mosaic .mosaic--bg { opacity: 0; transition: opacity .8s ease .1s; }
.half--mosaic .mosaic--layer2 { opacity: 0; transition: opacity .9s ease .3s; }

.is-mounted .half--form .form-wrap { opacity: 1; transform: translateY(0); }
.is-mounted .half--mosaic .mosaic-content { opacity: 1; transform: translateY(0); }
.is-mounted .half--mosaic .mosaic--bg { opacity: .65; }
.is-mounted .half--mosaic .mosaic--layer2 { opacity: .35; }

/* ================================================================
   LAYOUT HALVES
   ================================================================ */
.half { min-height: 100vh; }
.half--form {
  flex: 0 0 46%;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  z-index: 2;
  background: #fff;
  overflow: hidden;
}
.half--mosaic {
  flex: 1;
  position: relative;
  overflow: hidden;
  background: var(--misa-primary);
}

/* ================================================================
   CORNER GEOMETRY on the form side (subtle)
   ================================================================ */
.corner-geo {
  position: absolute;
  inset-inline-end: 0;
  top: 0;
  width: 320px;
  height: 320px;
  pointer-events: none;
}

/* ================================================================
   FORM PANEL
   ================================================================ */
.form-wrap {
  width: 100%;
  max-width: 370px;
  padding: 2rem;
  position: relative;
}

/* --- Language toggle --- */
.lang-toggle {
  position: absolute;
  top: 0;
  inset-inline-end: 0;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 6px 14px;
  border: 1.5px solid var(--misa-border);
  border-radius: 8px;
  background: var(--misa-cream);
  color: var(--misa-primary);
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  transition: all .2s;
}
.lang-toggle:hover {
  background: var(--misa-sage);
  border-color: var(--misa-secondary);
}

/* --- Logo --- */
.logo-block {
  margin: 0 0 2rem;
  text-align: center;
}
.logo-img {
  height: 62px;
  margin: 0 auto;
  display: block;
  object-fit: contain;
}

/* --- Accent rule --- */
.accent-rule {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 5px;
  margin-bottom: 1.75rem;
}
.accent-rule__seg {
  height: 3px;
  border-radius: 2px;
}
.accent-rule__seg--short {
  width: 10px;
  background: var(--misa-secondary);
  opacity: .45;
}
.accent-rule__seg--long {
  width: 36px;
  background: var(--misa-primary);
}

/* --- Title --- */
.form-title {
  text-align: center;
  font-size: 20px;
  font-weight: 700;
  color: var(--misa-text);
  margin: 0 0 4px;
  line-height: 1.4;
}
.form-subtitle {
  text-align: center;
  font-size: 14px;
  color: var(--misa-text-muted);
  font-weight: 500;
  margin: 0 0 1.75rem;
}

/* --- Error banner --- */
.err-banner {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  padding: 10px 14px;
  margin-bottom: 1.25rem;
  background: #fdf2f2;
  border-inline-start: 3px solid var(--misa-error);
  border-radius: 0 8px 8px 0;
  color: var(--misa-error);
  font-size: 13px;
  line-height: 1.5;
}

.err-slide-enter-active,
.err-slide-leave-active { transition: all .25s cubic-bezier(.4,0,.2,1); }
.err-slide-enter-from,
.err-slide-leave-to { opacity: 0; transform: translateY(-6px); }

/* --- Form fields --- */
.login-form { display: flex; flex-direction: column; gap: 1.125rem; }

.fld__label {
  display: block;
  font-size: 13px;
  font-weight: 600;
  color: var(--misa-text);
  margin-bottom: 6px;
}

.fld__box {
  display: flex;
  align-items: center;
  border: 1.5px solid var(--misa-border);
  border-radius: 10px;
  background: var(--misa-cream);
  transition: border-color .2s, box-shadow .2s, background .2s;
}
.fld__box:hover {
  border-color: var(--misa-secondary);
  background: #fff;
}
.fld__box:focus-within {
  border-color: var(--misa-primary);
  background: #fff;
  box-shadow: 0 0 0 3px rgba(0,109,75,.1);
}
.fld__box--err {
  border-color: var(--misa-error) !important;
}
.fld__box--err:focus-within {
  box-shadow: 0 0 0 3px rgba(192,57,43,.1);
}

.fld__ico {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 46px;
  flex-shrink: 0;
  color: var(--misa-text-muted);
  opacity: .6;
  transition: color .2s, opacity .2s;
}
.fld__box:focus-within .fld__ico {
  color: var(--misa-primary);
  opacity: 1;
}

.fld__input {
  flex: 1;
  border: none;
  background: transparent;
  padding: 12px 12px 12px 0;
  font-size: 14px;
  color: var(--misa-text);
  font-family: inherit;
  outline: none;
  min-width: 0;
}
[dir="rtl"] .fld__input { padding: 12px 0 12px 12px; }
.fld__input::placeholder { color: #a5b8ae; }

.fld__eye {
  width: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: none;
  border: none;
  color: var(--misa-text-muted);
  opacity: .5;
  cursor: pointer;
  transition: opacity .2s, color .2s;
  flex-shrink: 0;
}
.fld__eye:hover { opacity: 1; color: var(--misa-primary); }

.fld__err {
  display: block;
  font-size: 11.5px;
  color: var(--misa-error);
  margin-top: 4px;
  padding-inline-start: 2px;
}

/* --- Submit button --- */
.btn-login {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 13px 28px;
  margin-top: .375rem;
  background: var(--misa-primary);
  color: #fff;
  border: none;
  border-radius: 10px;
  font-size: 15px;
  font-weight: 700;
  font-family: inherit;
  cursor: pointer;
  position: relative;
  transition: background .2s, box-shadow .25s, transform .15s;
  letter-spacing: .3px;
}
.btn-login:hover:not(:disabled) {
  background: var(--misa-primary-dark);
  box-shadow: 0 8px 28px rgba(0,109,75,.28);
  transform: translateY(-1px);
}
.btn-login:active:not(:disabled) { transform: translateY(0); box-shadow: none; }
.btn-login:disabled { opacity: .55; cursor: not-allowed; }

.btn-login__spinner {
  width: 18px;
  height: 18px;
  border: 2.5px solid rgba(255,255,255,.25);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin .65s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

/* --- Footer --- */
.form-footer {
  text-align: center;
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid var(--misa-sage);
  font-size: 12px;
  color: var(--misa-text-muted);
  letter-spacing: .2px;
}

/* ================================================================
   MOSAIC PANEL (right)
   ================================================================ */

/* Large vivid pattern — the hero element */
.mosaic {
  position: absolute;
  pointer-events: none;
}

.mosaic--bg {
  inset: -15%;
  width: 130%;
  height: 130%;
}

.mosaic--layer2 {
  inset: 5% -20% -10% 10%;
  width: 130%;
  height: 120%;
  transform: rotate(180deg);
}

/* Dark vignette for text readability */
.mosaic-scrim {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(ellipse at 50% 50%, rgba(0,53,35,.55) 0%, transparent 75%),
    linear-gradient(180deg, rgba(0,83,57,.15) 0%, rgba(0,53,35,.4) 100%);
  pointer-events: none;
}

/* --- Center branding overlay --- */
.mosaic-content {
  position: relative;
  z-index: 3;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  padding: 3rem;
  text-align: center;
}

.mosaic-badge {
  width: 60px;
  height: 60px;
  border-radius: 16px;
  background: rgba(255,255,255,.12);
  border: 1px solid rgba(255,255,255,.18);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 1.5rem;
  backdrop-filter: blur(6px);
}
.mosaic-badge svg {
  width: 28px;
  height: 28px;
  color: rgba(255,255,255,.9);
}

.mosaic-title {
  font-size: 32px;
  font-weight: 800;
  color: #fff;
  line-height: 1.35;
  margin: 0 0 1rem;
  max-width: 300px;
  text-shadow: 0 2px 16px rgba(0,0,0,.2);
}
.mosaic-title span {
  display: block;
}

.mosaic-rule {
  width: 48px;
  height: 3px;
  border-radius: 2px;
  background: rgba(255,255,255,.35);
  margin: 0 auto 1rem;
}

.mosaic-ministry {
  font-size: 15px;
  color: rgba(255,255,255,.6);
  font-weight: 500;
  margin: 0;
  letter-spacing: .4px;
}

/* ================================================================
   RESPONSIVE
   ================================================================ */
@media (max-width: 960px) {
  .misa-login { flex-direction: column-reverse; }
  .half { min-height: auto; }

  .half--mosaic {
    height: 240px;
    flex: none;
  }
  .half--form {
    flex: 1;
    padding: 0;
  }

  .mosaic-title { font-size: 22px; }
  .mosaic-badge { width: 44px; height: 44px; border-radius: 12px; margin-bottom: 1rem; }
  .mosaic-badge svg { width: 22px; height: 22px; }
  .mosaic-ministry { font-size: 13px; }
  .mosaic-rule { margin-bottom: .75rem; }

  .form-wrap { max-width: 420px; padding: 2rem 1.5rem; }
  .logo-img { height: 52px; }

  .lang-toggle { position: fixed; top: 16px; inset-inline-end: 16px; z-index: 50;
    background: rgba(255,255,255,.85); backdrop-filter: blur(8px);
    border-color: rgba(255,255,255,.4); }

  .corner-geo { display: none; }
}

@media (max-width: 480px) {
  .half--mosaic { height: 200px; }
  .mosaic-title { font-size: 18px; }
  .form-wrap { padding: 1.5rem 1.25rem; }
  .logo-img { height: 46px; }
  .form-title { font-size: 18px; }
  .form-subtitle { font-size: 13px; }
}
</style>

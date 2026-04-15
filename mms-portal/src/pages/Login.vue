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
import misaPattern from '@/assets/misa.svg'

const { t } = useI18n()
const router = useRouter()
const userStore = useUserStore()

const isRtl = computed(() => userStore.isRtl)
const mounted = ref(false)

onMounted(() => {
  requestAnimationFrame(() => { mounted.value = true })
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

    <!-- ══════════════════════ FORM PANE (LEFT) ══════════════════════ -->
    <section class="pane pane--form">
      <!-- subtle top banding -->
      <div class="form-topband" aria-hidden="true">
        <span></span><span></span><span></span>
      </div>

      <div class="form-wrap">
        <!-- Language toggle -->
        <button class="lang-toggle" @click="toggleLanguage" type="button">
          <Icon icon="mdi:translate" class="w-4 h-4" />
          <span>{{ isRtl ? 'English' : 'العربية' }}</span>
        </button>

        <!-- Logo -->
        <div class="brand">
          <img :src="misaLogo" alt="MISA" class="brand__logo" />
          <div class="brand__rule" aria-hidden="true"></div>
        </div>

        <!-- Titles -->
        <div class="heading">
          <span class="heading__eyebrow">{{ isRtl ? 'أهلاً بك' : 'Welcome' }}</span>
          <h1 class="heading__title">{{ $t('ApplicationName') }}</h1>
          <p class="heading__sub">
            {{ isRtl ? 'سجّل الدخول للوصول إلى لوحة إدارة الاجتماعات' : 'Sign in to access the meeting management dashboard' }}
          </p>
        </div>

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
            <span class="btn-login__label">{{ isLoading ? $t('LoggingIn') : $t('Login') }}</span>
            <Icon v-if="!isLoading" :icon="isRtl ? 'mdi:arrow-left' : 'mdi:arrow-right'" class="w-5 h-5 btn-login__arrow" />
          </button>
        </form>

        <!-- Footer -->
        <footer class="form-footer">
          <span>&copy; {{ new Date().getFullYear() }}&ensp;{{ isRtl ? 'وزارة الاستثمار — المملكة العربية السعودية' : 'Ministry of Investment — Kingdom of Saudi Arabia' }}</span>
        </footer>
      </div>
    </section>

    <!-- ══════════════════════ PATTERN PANE (RIGHT) ══════════════════════ -->
    <section class="pane pane--mosaic">
      <!-- HERO pattern, full vivid opacity, bleeds off edges -->
      <img :src="misaPattern" alt="" class="mosaic-hero" aria-hidden="true" />
      <img :src="misaPattern" alt="" class="mosaic-hero mosaic-hero--2" aria-hidden="true" />

      <!-- A single dark green plaque anchored to the bottom — holds the branding -->
      <div class="plaque">
        <div class="plaque__accent" aria-hidden="true">
          <span></span><span></span><span></span>
        </div>

        <p class="plaque__eyebrow">{{ isRtl ? 'المملكة العربية السعودية' : 'Kingdom of Saudi Arabia' }}</p>
        <h2 class="plaque__title">
          {{ isRtl ? 'نظام إدارة الاجتماعات' : 'Meeting Management' }}
          <span v-if="!isRtl">System</span>
        </h2>
        <p class="plaque__ministry">{{ isRtl ? 'وزارة الاستثمار' : 'Ministry of Investment' }}</p>

        <div class="plaque__stats">
          <div class="plaque__stat">
            <Icon icon="mdi:shield-check-outline" class="w-5 h-5" />
            <span>{{ isRtl ? 'بيئة آمنة' : 'Secure Environment' }}</span>
          </div>
          <div class="plaque__stat">
            <Icon icon="mdi:check-decagram-outline" class="w-5 h-5" />
            <span>{{ isRtl ? 'معتمد حكومياً' : 'Government Certified' }}</span>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
/* ╔══════════════════════════════════════════════════════════════════╗
   ║  MISA LOGIN — Mosaic Hero                                        ║
   ║  The geometric tile pattern is the hero. Everything else serves  ║
   ║  as a quiet frame for it.                                        ║
   ╚══════════════════════════════════════════════════════════════════╝ */

.misa-login {
  --misa-primary: #006d4b;
  --misa-primary-dark: #004f37;
  --misa-primary-darker: #003524;
  --misa-secondary: #63a58f;
  --misa-secondary-light: #8fc4b0;
  --misa-cream: #f4f8f6;
  --misa-sage: #e6f1ed;
  --misa-sage-deep: #d5e8df;
  --misa-text: #0f2920;
  --misa-text-muted: #6b8a7d;
  --misa-border: #d5e0db;
  --misa-error: #b83227;

  display: flex;
  min-height: 100vh;
  font-family: 'Tajawal', sans-serif;
  background: #fff;
  color: var(--misa-text);
  position: relative;
  overflow: hidden;
}

/* ────────────────── PANES ────────────────── */
.pane { min-height: 100vh; position: relative; }

.pane--form {
  flex: 0 0 44%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fff;
  z-index: 2;
  overflow: hidden;
}

.pane--mosaic {
  flex: 1;
  position: relative;
  overflow: hidden;
  /* Cream background — pattern's cream tiles blend, green tiles pop */
  background:
    radial-gradient(ellipse at top right, #f0f8f4 0%, #e0ede7 100%);
}

/* ────────────────── FORM TOP BAND ──────────────────
   Three thin bars of decreasing green depth at the very top of the
   form panel — a quiet echo of the mosaic. */
.form-topband {
  position: absolute;
  top: 0; left: 0; right: 0;
  display: flex;
  height: 4px;
  z-index: 1;
}
.form-topband span { flex: 1; }
.form-topband span:nth-child(1) { background: var(--misa-primary); flex: 0 0 30%; }
.form-topband span:nth-child(2) { background: var(--misa-secondary); flex: 0 0 15%; }
.form-topband span:nth-child(3) { background: var(--misa-sage); }

/* ────────────────── FORM WRAP ────────────────── */
.form-wrap {
  width: 100%;
  max-width: 400px;
  padding: 3rem 2rem;
  position: relative;
}

/* --- Language toggle --- */
.lang-toggle {
  position: absolute;
  top: 1.25rem;
  inset-inline-end: 1.25rem;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 7px 14px;
  border: 1px solid var(--misa-border);
  border-radius: 999px;
  background: #fff;
  color: var(--misa-primary);
  font-size: 12.5px;
  font-weight: 600;
  cursor: pointer;
  font-family: inherit;
  transition: all .22s ease;
  letter-spacing: .2px;
}
.lang-toggle:hover {
  background: var(--misa-sage);
  border-color: var(--misa-secondary);
  transform: translateY(-1px);
  box-shadow: 0 4px 14px rgba(0,109,75,.10);
}

/* ────────────────── BRAND BLOCK ────────────────── */
.brand {
  margin-bottom: 2rem;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 14px;
}
[dir="rtl"] .brand { align-items: flex-end; }

.brand__logo {
  height: 58px;
  object-fit: contain;
  display: block;
}

.brand__rule {
  width: 64px;
  height: 3px;
  background: linear-gradient(90deg, var(--misa-primary) 0%, var(--misa-secondary) 100%);
  border-radius: 2px;
}

/* ────────────────── HEADING ────────────────── */
.heading { margin-bottom: 1.75rem; }
[dir="rtl"] .heading { text-align: right; }

.heading__eyebrow {
  display: inline-block;
  font-size: 11.5px;
  font-weight: 700;
  letter-spacing: 1.5px;
  text-transform: uppercase;
  color: var(--misa-secondary);
  margin-bottom: 8px;
}
[dir="rtl"] .heading__eyebrow { letter-spacing: 0; }

.heading__title {
  font-size: 26px;
  font-weight: 800;
  color: var(--misa-text);
  margin: 0 0 6px;
  line-height: 1.25;
  letter-spacing: -.3px;
}

.heading__sub {
  font-size: 14px;
  color: var(--misa-text-muted);
  margin: 0;
  line-height: 1.55;
  max-width: 340px;
}

/* ────────────────── ERROR BANNER ────────────────── */
.err-banner {
  display: flex;
  align-items: flex-start;
  gap: 9px;
  padding: 11px 14px;
  margin-bottom: 1.25rem;
  background: #fdeeeb;
  border-inline-start: 3px solid var(--misa-error);
  border-radius: 0 8px 8px 0;
  color: var(--misa-error);
  font-size: 13px;
  line-height: 1.5;
}
[dir="rtl"] .err-banner { border-radius: 8px 0 0 8px; }

.err-slide-enter-active,
.err-slide-leave-active { transition: all .25s cubic-bezier(.4,0,.2,1); }
.err-slide-enter-from,
.err-slide-leave-to { opacity: 0; transform: translateY(-6px); }

/* ────────────────── FORM FIELDS ────────────────── */
.login-form { display: flex; flex-direction: column; gap: 1rem; }

.fld__label {
  display: block;
  font-size: 12.5px;
  font-weight: 600;
  color: var(--misa-text);
  margin-bottom: 7px;
  letter-spacing: .2px;
}

.fld__box {
  display: flex;
  align-items: center;
  border: 1.5px solid var(--misa-border);
  border-radius: 10px;
  background: #fafcfb;
  transition: border-color .2s, box-shadow .22s, background .2s;
}
.fld__box:hover {
  border-color: var(--misa-secondary);
  background: #fff;
}
.fld__box:focus-within {
  border-color: var(--misa-primary);
  background: #fff;
  box-shadow: 0 0 0 4px rgba(0,109,75,.10);
}
.fld__box--err { border-color: var(--misa-error) !important; }
.fld__box--err:focus-within { box-shadow: 0 0 0 4px rgba(184,50,39,.10); }

.fld__ico {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 46px;
  flex-shrink: 0;
  color: var(--misa-text-muted);
  opacity: .55;
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
  padding: 13px 12px 13px 0;
  font-size: 14px;
  color: var(--misa-text);
  font-family: inherit;
  outline: none;
  min-width: 0;
}
[dir="rtl"] .fld__input { padding: 13px 0 13px 12px; }
.fld__input::placeholder { color: #a5b8ae; }

.fld__eye {
  width: 42px;
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
  margin-top: 5px;
  padding-inline-start: 2px;
}

/* ────────────────── SUBMIT BUTTON ────────────────── */
.btn-login {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 14px 28px;
  margin-top: .5rem;
  background: linear-gradient(135deg, var(--misa-primary) 0%, var(--misa-primary-dark) 100%);
  color: #fff;
  border: none;
  border-radius: 10px;
  font-size: 15px;
  font-weight: 700;
  font-family: inherit;
  cursor: pointer;
  position: relative;
  transition: transform .18s ease, box-shadow .25s ease, filter .2s;
  letter-spacing: .3px;
  box-shadow: 0 6px 20px rgba(0,109,75,.25);
  overflow: hidden;
}
.btn-login::before {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(90deg, transparent 0%, rgba(255,255,255,.15) 50%, transparent 100%);
  transform: translateX(-100%);
  transition: transform .55s ease;
}
.btn-login:hover:not(:disabled)::before { transform: translateX(100%); }
.btn-login:hover:not(:disabled) {
  transform: translateY(-1.5px);
  box-shadow: 0 10px 28px rgba(0,109,75,.32);
}
.btn-login:active:not(:disabled) { transform: translateY(0); }
.btn-login:disabled { opacity: .6; cursor: not-allowed; }

.btn-login__arrow { transition: transform .2s ease; }
.btn-login:hover:not(:disabled) .btn-login__arrow { transform: translateX(3px); }
[dir="rtl"] .btn-login:hover:not(:disabled) .btn-login__arrow { transform: translateX(-3px); }

.btn-login__spinner {
  width: 18px;
  height: 18px;
  border: 2.5px solid rgba(255,255,255,.28);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin .65s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

/* ────────────────── FOOTER ────────────────── */
.form-footer {
  margin-top: 2.25rem;
  padding-top: 1.25rem;
  border-top: 1px solid var(--misa-sage);
  font-size: 11.5px;
  color: var(--misa-text-muted);
  letter-spacing: .3px;
  text-align: center;
}

/* ╔══════════════════════════════════════════════════════════════════╗
   ║  MOSAIC PANE — pattern is the hero                               ║
   ╚══════════════════════════════════════════════════════════════════╝ */

.mosaic-hero {
  position: absolute;
  pointer-events: none;
  user-select: none;
  /* Full vivid opacity — this is the star */
  opacity: 1;
}

/* Primary tile — big, positioned to the upper area, bleeds right */
.mosaic-hero:not(.mosaic-hero--2) {
  top: -8%;
  inset-inline-start: -5%;
  width: 115%;
  height: auto;
  filter: drop-shadow(0 20px 40px rgba(0,113,77,.12));
}

/* Second tile — flipped, anchored lower, slight transparency so the
   overlap feels intentional rather than noisy */
.mosaic-hero--2 {
  bottom: -10%;
  inset-inline-end: -15%;
  width: 100%;
  height: auto;
  transform: scaleX(-1) scaleY(-1);
  opacity: .72;
  mix-blend-mode: multiply;
}
[dir="rtl"] .mosaic-hero--2 { transform: scaleX(1) scaleY(-1); }

/* ────────────────── PLAQUE (branding card) ──────────────────
   A dark green floating panel anchored to the bottom. It sits ON TOP
   of the pattern so the pattern wraps around it like a frame. */
.plaque {
  position: absolute;
  bottom: 2.5rem;
  inset-inline-start: 2.5rem;
  inset-inline-end: 2.5rem;
  max-width: 520px;
  margin-inline: auto;
  background: linear-gradient(135deg, rgba(0,83,57,.96) 0%, rgba(0,53,36,.98) 100%);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  border: 1px solid rgba(255,255,255,.08);
  border-radius: 18px;
  padding: 2rem 2.25rem;
  color: #fff;
  box-shadow:
    0 30px 60px -20px rgba(0,53,36,.4),
    0 0 0 1px rgba(255,255,255,.04) inset;
  z-index: 3;
}

.plaque__accent {
  display: flex;
  gap: 5px;
  margin-bottom: 1.125rem;
}
.plaque__accent span {
  height: 3px;
  border-radius: 2px;
}
.plaque__accent span:nth-child(1) { width: 28px; background: var(--misa-secondary-light); }
.plaque__accent span:nth-child(2) { width: 14px; background: var(--misa-secondary); opacity: .7; }
.plaque__accent span:nth-child(3) { width: 6px;  background: rgba(255,255,255,.4); }

.plaque__eyebrow {
  font-size: 11.5px;
  font-weight: 600;
  letter-spacing: 2px;
  text-transform: uppercase;
  color: rgba(143,196,176,.85);
  margin: 0 0 .5rem;
}
[dir="rtl"] .plaque__eyebrow { letter-spacing: 0; }

.plaque__title {
  font-size: 28px;
  font-weight: 800;
  color: #fff;
  margin: 0 0 .375rem;
  line-height: 1.25;
  letter-spacing: -.4px;
}
.plaque__title span { display: inline-block; }

.plaque__ministry {
  font-size: 15px;
  color: rgba(255,255,255,.65);
  margin: 0 0 1.5rem;
  font-weight: 500;
  letter-spacing: .2px;
}

.plaque__stats {
  display: flex;
  gap: 1.75rem;
  padding-top: 1.25rem;
  border-top: 1px solid rgba(255,255,255,.1);
  flex-wrap: wrap;
}

.plaque__stat {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-size: 12.5px;
  font-weight: 500;
  color: rgba(255,255,255,.78);
}
.plaque__stat :deep(svg),
.plaque__stat :deep(.material-symbols-outlined) {
  color: var(--misa-secondary-light);
  flex-shrink: 0;
}

/* ╔══════════════════════════════════════════════════════════════════╗
   ║  REVEAL ORCHESTRATION                                            ║
   ╚══════════════════════════════════════════════════════════════════╝ */
.form-wrap > * { opacity: 0; transform: translateY(14px); transition: opacity .55s ease, transform .55s ease; }
.is-mounted .form-wrap > * { opacity: 1; transform: translateY(0); }
.is-mounted .form-wrap > .lang-toggle { transition-delay: .05s; }
.is-mounted .form-wrap > .brand        { transition-delay: .08s; }
.is-mounted .form-wrap > .heading      { transition-delay: .16s; }
.is-mounted .form-wrap > .login-form   { transition-delay: .24s; }
.is-mounted .form-wrap > .form-footer  { transition-delay: .32s; }

.mosaic-hero { opacity: 0 !important; transition: opacity .9s ease .1s, transform 1.1s cubic-bezier(.2,.8,.2,1) .1s; transform: scale(1.05); }
.mosaic-hero--2 { transition-delay: .25s; }
.is-mounted .mosaic-hero { opacity: 1 !important; transform: scale(1); }
.is-mounted .mosaic-hero--2 { opacity: .72 !important; }

.plaque { opacity: 0; transform: translateY(28px); transition: opacity .6s ease .45s, transform .6s ease .45s; }
.is-mounted .plaque { opacity: 1; transform: translateY(0); }

/* ╔══════════════════════════════════════════════════════════════════╗
   ║  RESPONSIVE                                                      ║
   ╚══════════════════════════════════════════════════════════════════╝ */
@media (max-width: 1100px) {
  .pane--form { flex: 0 0 48%; }
  .plaque { inset-inline-start: 1.75rem; inset-inline-end: 1.75rem; padding: 1.5rem 1.75rem; }
  .plaque__title { font-size: 24px; }
}

@media (max-width: 960px) {
  .misa-login { flex-direction: column-reverse; }
  .pane { min-height: auto; }

  .pane--mosaic {
    height: 320px;
    flex: none;
  }
  .pane--form { flex: 1; padding: 0; }

  .mosaic-hero:not(.mosaic-hero--2) {
    top: -15%;
    inset-inline-start: -10%;
    width: 130%;
  }
  .mosaic-hero--2 { display: none; }

  .plaque {
    bottom: 1rem;
    inset-inline-start: 1rem;
    inset-inline-end: 1rem;
    padding: 1.125rem 1.25rem;
    border-radius: 14px;
  }
  .plaque__eyebrow { font-size: 10.5px; margin-bottom: .25rem; }
  .plaque__title { font-size: 18px; margin-bottom: .2rem; }
  .plaque__ministry { font-size: 13px; margin-bottom: .75rem; }
  .plaque__stats { gap: 1rem; padding-top: .625rem; }
  .plaque__stat { font-size: 11px; }
  .plaque__accent { margin-bottom: .5rem; }

  .form-wrap { max-width: 440px; padding: 2.5rem 1.5rem 2rem; }
  .brand__logo { height: 50px; }
  .heading__title { font-size: 22px; }

  .lang-toggle { top: 1rem; inset-inline-end: 1rem; }
}

@media (max-width: 480px) {
  .pane--mosaic { height: 260px; }
  .plaque__title { font-size: 16px; }
  .plaque__stat span { display: none; }
  .plaque__stats { justify-content: center; }
  .form-wrap { padding: 2rem 1.25rem 1.5rem; }
  .brand__logo { height: 44px; }
  .heading__title { font-size: 20px; }
  .heading__sub { font-size: 13px; }
}

/* Reduce motion */
@media (prefers-reduced-motion: reduce) {
  .form-wrap > *,
  .mosaic-hero,
  .plaque,
  .btn-login,
  .btn-login::before,
  .btn-login__arrow {
    transition: none !important;
    animation: none !important;
  }
}
</style>

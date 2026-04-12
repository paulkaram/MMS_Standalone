<template>
  <header
    class="hdr"
    :class="[
      sidebarCollapsed ? 'header-collapsed' : 'header-expanded'
    ]"
  >
    <div class="hdr-inner">
      <!-- Left: Mobile menu + Welcome -->
      <div class="hdr-left">
        <!-- Mobile menu toggle only -->
        <button
          type="button"
          class="hdr-menu-btn"
          @click="toggleMobileSidebar"
        >
          <Icon icon="mdi:menu" class="w-5 h-5" />
        </button>

        <div class="hdr-welcome">
          <span class="hdr-welcome-label">{{ $t('WelcomeBack') }}</span>
          <span class="hdr-welcome-name">{{ user?.fullName }}</span>
        </div>
      </div>

      <!-- Right: Language, Notifications, Divider, Profile -->
      <div class="hdr-right">
        <!-- Language -->
        <div class="hdr-dropdown-container" ref="langContainer">
          <button
            type="button"
            class="hdr-ctrl-btn hdr-ctrl-btn--lang"
            :class="{ 'hdr-ctrl-btn--active': langDropdownOpen }"
            @click.stop="langDropdownOpen = !langDropdownOpen"
          >
            <Icon icon="mdi:translate" class="w-[18px] h-[18px]" />
            <span class="hdr-lang-code">{{ language === 'ar' ? 'AR' : 'EN' }}</span>
          </button>
          <Transition name="hdr-dd">
            <div v-if="langDropdownOpen" class="hdr-dropdown hdr-dropdown--lang">
              <button
                v-for="lang in languages"
                :key="lang.code"
                class="hdr-dropdown-item"
                :class="{ 'hdr-dropdown-item--active': language === lang.code }"
                @click.stop="selectLanguage(lang.code)"
              >
                <span>{{ lang.label }}</span>
                <Icon v-if="language === lang.code" icon="mdi:check-circle" class="w-4 h-4 text-[#006d4b]" />
              </button>
            </div>
          </Transition>
        </div>

        <!-- Unified Notification Center (IAM) -->
        <IntalioNotificationBell
          :token="mmsToken"
          :language="currentLanguage"
        />

        <div class="hdr-divider"></div>

        <!-- Profile -->
        <div class="hdr-dropdown-container" ref="profileContainer">
          <button
            type="button"
            class="hdr-profile"
            :class="{ 'hdr-profile--active': profileDropdownOpen }"
            @click.stop="profileDropdownOpen = !profileDropdownOpen"
          >
            <div class="hdr-avatar">
              <img
                v-if="profilePictureUrl"
                :src="profilePictureUrl"
                :alt="user?.fullName"
                class="hdr-avatar-img"
                @error="handleProfileImageError"
              />
              <template v-else>{{ userInitials }}</template>
            </div>
            <div class="hdr-profile-text">
              <span class="hdr-profile-name">{{ user?.fullName }}</span>
              <span class="hdr-profile-role">{{ $t('Welcome') }}</span>
            </div>
            <Icon icon="mdi:chevron-down" class="w-4 h-4 hdr-chevron" :class="{ 'hdr-chevron--open': profileDropdownOpen }" />
          </button>

          <Transition name="hdr-dd">
            <div v-if="profileDropdownOpen" class="hdr-dropdown hdr-dropdown--profile">
              <!-- Profile card -->
              <div class="hdr-profile-card">
                <div class="hdr-profile-card-avatar">
                  <img
                    v-if="profilePictureUrl"
                    :src="profilePictureUrl"
                    :alt="user?.fullName"
                    class="hdr-avatar-img"
                    @error="handleProfileImageError"
                  />
                  <template v-else>{{ userInitials }}</template>
                </div>
                <div>
                  <p class="hdr-profile-card-name">{{ user?.fullName }}</p>
                  <p class="hdr-profile-card-email">{{ user?.email }}</p>
                </div>
              </div>

              <!-- Menu items -->
              <div class="hdr-profile-menu">
                <button class="hdr-profile-menu-item" @click="navigateTo('/profile')">
                  <Icon icon="mdi:account-outline" class="w-[17px] h-[17px]" />
                  <span>{{ $t('Profile') }}</span>
                </button>
                <button class="hdr-profile-menu-item" @click="navigateTo('/system-settings')">
                  <Icon icon="mdi:cog-outline" class="w-[17px] h-[17px]" />
                  <span>{{ $t('Settings') }}</span>
                </button>
              </div>

              <!-- Logout -->
              <div class="hdr-profile-logout-wrap">
                <button class="hdr-profile-logout" @click="handleLogout">
                  <Icon icon="mdi:logout" class="w-[17px] h-[17px]" />
                  <span>{{ $t('Logout') }}</span>
                </button>
              </div>
            </div>
          </Transition>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import Icon from '@/components/ui/Icon.vue'
import IntalioNotificationBell from '@/components/IntalioNotificationBell.vue'
import { useUserStore } from '@/stores/user'
import { useAppStore } from '@/stores/app'
import AuthService from '@/services/AuthService'

// Stores
const router = useRouter()
const userStore = useUserStore()
const appStore = useAppStore()

// State
const profileImageError = ref(false)
const langDropdownOpen = ref(false)
const profileDropdownOpen = ref(false)
const langContainer = ref<HTMLElement | null>(null)
const profileContainer = ref<HTMLElement | null>(null)

// Language options
const languages = [
  { code: 'en', label: 'English' },
  { code: 'ar', label: 'العربية' }
]

// Click-outside handler
function handleClickOutside(e: MouseEvent) {
  if (langDropdownOpen.value && langContainer.value && !langContainer.value.contains(e.target as Node)) {
    langDropdownOpen.value = false
  }
  if (profileDropdownOpen.value && profileContainer.value && !profileContainer.value.contains(e.target as Node)) {
    profileDropdownOpen.value = false
  }
}

onMounted(() => document.addEventListener('click', handleClickOutside))
onBeforeUnmount(() => document.removeEventListener('click', handleClickOutside))

// Computed
const user = computed(() => userStore.loggedInUser)
const language = computed(() => userStore.language)
const sidebarCollapsed = computed(() => appStore.isSidebarCollapsed)
// Unified token — same IAM token used across all portals
const mmsToken = computed(() => localStorage.getItem('pickOne_token') || '')
const currentLanguage = computed(() => userStore.language || 'en')

// Profile picture
const profilePictureUrl = computed(() => {
  if (profileImageError.value) return null
  if (user.value?.profilePictureUrl) return user.value.profilePictureUrl
  if (user.value?.hasProfilePicture && user.value?.id) {
    const baseUrl = import.meta.env.VITE_API_URL || ''
    return `${baseUrl}/api/users/profile-image/${user.value.id}`
  }
  return null
})

const handleProfileImageError = () => {
  profileImageError.value = true
}

const userInitials = computed(() => {
  const name = user.value?.fullName || ''
  const parts = name.split(' ')
  if (parts.length >= 2) {
    return parts[0][0] + parts[1][0]
  }
  return name.substring(0, 2).toUpperCase()
})

// Methods
function toggleMobileSidebar() {
  appStore.toggleMobileSidebar()
}

async function selectLanguage(code: string) {
  if (code === language.value) {
    langDropdownOpen.value = false
    return
  }

  userStore.setLanguage(code)
  langDropdownOpen.value = false

  if (userStore.isAuthenticated) {
    try {
      // Update language in DB first, then refresh token so JWT has the new language claim
      await AuthService.updateLanguage(code)

      const rt = userStore.refreshToken
      if (rt) {
        const res = await AuthService.refreshToken(rt)
        const newToken = res?.data?.token || res?.token
        const newRt = res?.data?.refreshToken || res?.refreshToken
        if (newToken) {
          userStore.updateToken(newToken, newRt)
        }
      }
    } catch (e) {
      // Language saved locally, token refresh failed — reload will still use correct Accept-Language header
    }
  }

  localStorage.setItem('language_transition', 'true')
  document.body.style.opacity = '0'
  document.body.style.transition = 'opacity 0.2s ease-out'
  setTimeout(() => window.location.reload(), 200)
}

function navigateTo(path: string) {
  profileDropdownOpen.value = false
  router.push(path)
}

async function handleLogout() {
  profileDropdownOpen.value = false
  try {
    await AuthService.logout()
  } catch {
    // Continue logout even if backend call fails
  }
  userStore.removeUserSession()
  await router.push('/login')
}
</script>

<style scoped>
/* ===== Header bar (matching IAM .hdr exactly) ===== */
.hdr {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  display: flex;
  align-items: center;
  height: 60px;
  background: #fff;
  border-bottom: none;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  z-index: 40;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.hdr::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: #e5e7eb;
}

/* Desktop: Header starts after sidebar */
@media (min-width: 1024px) {
  [dir="ltr"] .header-expanded {
    left: var(--sidebar-width, 260px);
    right: 0;
  }

  [dir="rtl"] .header-expanded {
    right: var(--sidebar-width, 260px);
    left: 0;
  }

  [dir="ltr"] .header-collapsed {
    left: var(--sidebar-collapsed-width, 64px);
    right: 0;
  }

  [dir="rtl"] .header-collapsed {
    right: var(--sidebar-collapsed-width, 64px);
    left: 0;
  }
}

.hdr-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 0 24px;
  height: 100%;
}

/* ===== Left section ===== */
.hdr-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.hdr-menu-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 8px;
  border: none;
  background: transparent;
  color: #64748b;
  cursor: pointer;
  transition: all 0.15s;
}

.hdr-menu-btn:hover {
  background: #f1f5f9;
  color: #0f172a;
}

@media (min-width: 1024px) {
  .hdr-menu-btn {
    display: none;
  }
}

.hdr-welcome {
  display: flex;
  align-items: baseline;
  gap: 5px;
}

.hdr-welcome-label {
  font-size: 15px;
  color: #64748b;
  font-weight: 400;
}

.hdr-welcome-name {
  font-size: 14px;
  font-weight: 700;
  color: #006d4b;
  letter-spacing: -0.2px;
}

/* ===== Right section ===== */
.hdr-right {
  display: flex;
  align-items: center;
  gap: 4px;
}

/* ===== Control Buttons ===== */
.hdr-dropdown-container {
  position: relative;
}

.hdr-ctrl-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 5px;
  height: 36px;
  padding: 0 12px;
  border-radius: 8px;
  border: none;
  background: transparent;
  color: #64748b;
  cursor: pointer;
  transition: all 0.15s ease;
}

.hdr-ctrl-btn:hover {
  color: #0f172a;
  background: #f1f5f9;
}

.hdr-ctrl-btn--active {
  color: #006d4b;
  background: rgba(0, 109, 75, 0.06);
}

.hdr-ctrl-btn--notif {
  padding: 0 10px;
  height: 38px;
  position: relative;
}

.hdr-lang-code {
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.3px;
}

.hdr-notif-chip {
  min-width: 20px;
  height: 20px;
  padding: 0 6px;
  border-radius: 10px;
  background: #ef4444;
  color: #fff;
  font-size: 11px;
  font-weight: 700;
  line-height: 20px;
  text-align: center;
}

/* ===== Divider ===== */
.hdr-divider {
  width: 1px;
  height: 24px;
  background: #e2e8f0;
  margin: 0 8px;
  flex-shrink: 0;
}

/* ===== Profile Button ===== */
.hdr-profile {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 5px 10px 5px 5px;
  border-radius: 12px;
  border: none;
  background: transparent;
  cursor: pointer;
  transition: all 0.15s ease;
}

.hdr-profile:hover {
  background: #f1f5f9;
}

.hdr-avatar {
  width: 34px;
  height: 34px;
  border-radius: 10px;
  background: linear-gradient(135deg, #004730 0%, #006d4b 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 12px;
  font-weight: 600;
  flex-shrink: 0;
  overflow: hidden;
}

.hdr-avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.hdr-profile-text {
  display: flex;
  flex-direction: column;
  text-align: start;
}

.hdr-profile-name {
  font-size: 13px;
  font-weight: 600;
  color: #006d4b;
  line-height: 1.2;
  white-space: nowrap;
  max-width: 140px;
  overflow: hidden;
  text-overflow: ellipsis;
}

.hdr-profile-role {
  font-size: 11px;
  color: #94a3b8;
  font-weight: 400;
  line-height: 1.2;
  margin-top: 1px;
}

.hdr-profile--active {
  background: #f1f5f9;
}

.hdr-chevron {
  color: #94a3b8;
  transition: transform 0.2s ease;
}

.hdr-chevron--open {
  transform: rotate(180deg);
}

/* ===== Dropdowns ===== */
.hdr-dropdown {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  background: #fff;
  border: 1px solid #e8ecf1;
  border-radius: 12px;
  box-shadow: 0 12px 36px rgba(0, 0, 0, 0.08), 0 2px 8px rgba(0, 0, 0, 0.04);
  z-index: 200;
  overflow: hidden;
}

.hdr-dropdown--lang {
  width: 160px;
}

.hdr-dropdown--profile {
  width: 240px;
}

.hdr-dropdown-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 10px 16px;
  border: none;
  background: transparent;
  font-size: 13px;
  color: #475569;
  cursor: pointer;
  text-align: start;
  transition: background 0.15s;
}

.hdr-dropdown-item:hover {
  background: #f1f5f9;
}

.hdr-dropdown-item--active {
  color: #006d4b;
  font-weight: 600;
  background: rgba(0, 109, 75, 0.05);
}

/* ===== Profile Dropdown ===== */
.hdr-profile-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  border-bottom: 1px solid #f1f5f9;
}

.hdr-profile-card-avatar {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  background: linear-gradient(135deg, #004730 0%, #006d4b 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 13px;
  font-weight: 600;
  flex-shrink: 0;
  overflow: hidden;
}

.hdr-profile-card-name {
  font-size: 14px;
  font-weight: 600;
  color: #006d4b;
  margin: 0;
  line-height: 1.3;
}

.hdr-profile-card-email {
  font-size: 12px;
  color: #94a3b8;
  margin: 2px 0 0 0;
  line-height: 1.3;
}

.hdr-profile-menu {
  padding: 6px 0;
}

.hdr-profile-menu-item {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 9px 16px;
  border: none;
  background: transparent;
  font-size: 13px;
  font-weight: 500;
  color: #475569;
  cursor: pointer;
  transition: all 0.15s;
  text-align: start;
}

.hdr-profile-menu-item span:first-child {
  color: #94a3b8;
  transition: color 0.15s;
}

.hdr-profile-menu-item:hover {
  background: #f8fafc;
  color: #006d4b;
}

.hdr-profile-menu-item:hover span:first-child {
  color: #006d4b;
}

.hdr-profile-logout-wrap {
  border-top: 1px solid #f1f5f9;
  padding: 6px 0;
}

.hdr-profile-logout {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 9px 16px;
  border: none;
  background: transparent;
  font-size: 13px;
  font-weight: 500;
  color: #ef4444;
  cursor: pointer;
  transition: all 0.15s;
  text-align: start;
}

.hdr-profile-logout:hover {
  background: rgba(239, 68, 68, 0.06);
}

.hdr-profile-logout span:first-child {
  color: #ef4444;
}

/* ===== Dropdown transition ===== */
.hdr-dd-enter-active {
  transition: all 200ms ease-out;
}

.hdr-dd-leave-active {
  transition: all 150ms ease-in;
}

.hdr-dd-enter-from,
.hdr-dd-leave-to {
  opacity: 0;
  transform: scale(0.95) translateY(-4px);
}

.hdr-dd-enter-to,
.hdr-dd-leave-from {
  opacity: 1;
  transform: scale(1) translateY(0);
}

/* ===== RTL ===== */
[dir="rtl"] .hdr-dropdown {
  right: auto;
  left: 0;
}

/* ===== Responsive ===== */
@media (max-width: 640px) {
  .hdr-lang-code {
    display: none;
  }

  .hdr-profile-text {
    display: none;
  }

  .hdr-chevron {
    display: none;
  }

  .hdr-welcome-label {
    display: none;
  }
}
</style>

<template>
  <!-- Mobile sidebar -->
  <TransitionRoot :show="mobileSidebarOpen" as="template">
    <Dialog as="div" class="relative z-sidebar lg:hidden" @close="closeMobileSidebar">
      <TransitionChild
        as="template"
        enter="transition-opacity ease-linear duration-200"
        enter-from="opacity-0"
        enter-to="opacity-100"
        leave="transition-opacity ease-linear duration-150"
        leave-from="opacity-100"
        leave-to="opacity-0"
      >
        <div class="fixed inset-0 bg-black/50" />
      </TransitionChild>

      <div class="fixed inset-0 flex">
        <TransitionChild
          as="template"
          enter="transition ease-in-out duration-200 transform"
          :enter-from="isRtl ? 'translate-x-full' : '-translate-x-full'"
          enter-to="translate-x-0"
          leave="transition ease-in-out duration-150 transform"
          leave-from="translate-x-0"
          :leave-to="isRtl ? 'translate-x-full' : '-translate-x-full'"
        >
          <DialogPanel
            :class="['sidebar-mobile relative flex flex-col h-full', isRtl ? 'ms-auto' : '']"
          >
            <!-- Close button -->
            <div class="absolute top-3 end-3 z-10">
              <button
                type="button"
                class="mobile-close-btn"
                @click="closeMobileSidebar"
              >
                <Icon icon="mdi:close" class="w-5 h-5" />
              </button>
            </div>

            <!-- Brand Header -->
            <div class="sidebar-brand">
              <div class="brand-content">
                <RouterLink to="/home" class="brand-link">
                  <img :src="isRtl ? logoAr : logoEn" alt="MMS" class="brand-logo" />
                </RouterLink>
              </div>
            </div>

            <!-- Navigation -->
            <nav class="flex-1 overflow-y-auto sidebar-scroll">
              <SidebarNavigation :collapsed="false" />
            </nav>

            <!-- Bottom / User Section -->
            <div class="sidebar-bottom">
              <div class="user-item">
                <div class="user-avatar">
                  {{ userInitials }}
                  <div class="user-status-dot"></div>
                </div>
                <div class="user-info">
                  <span class="user-name-text">{{ userName }}</span>
                  <span class="user-email">{{ userEmail }}</span>
                </div>
                <button @click="handleLogout" class="logout-btn" :title="$t('Logout')">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
                  </svg>
                </button>
              </div>
            </div>
          </DialogPanel>
        </TransitionChild>
      </div>
    </Dialog>
  </TransitionRoot>

  <!-- Desktop sidebar -->
  <aside
    :class="[
      'sidebar-desktop fixed top-0 bottom-0 z-sidebar',
      'hidden lg:flex flex-col transition-all duration-300 ease-in-out',
      sidebarCollapsed ? 'sidebar-width-collapsed' : 'sidebar-width-expanded',
      isRtl ? 'right-0' : 'left-0'
    ]"
  >
    <!-- Brand Header -->
    <div class="sidebar-brand">
      <div class="brand-content">
        <!-- Expanded: logo + collapse btn -->
        <RouterLink v-if="!sidebarCollapsed" to="/home" class="brand-link">
          <img :src="isRtl ? logoAr : logoEn" alt="MMS" class="brand-logo" />
        </RouterLink>

        <!-- Collapsed: just icon, click to expand -->
        <button v-else class="brand-link brand-link-collapsed" @click="toggleCollapse" title="Expand">
          <div class="logo-mark">
            <svg class="w-[18px] h-[18px] text-white" fill="currentColor" viewBox="0 0 24 24">
              <path d="M12,1L3,5V11C3,16.55 6.84,21.74 12,23C17.16,21.74 21,16.55 21,11V5L12,1M10,17L6,13L7.41,11.59L10,14.17L16.59,7.58L18,9L10,17Z"/>
            </svg>
          </div>
        </button>

        <!-- Collapse button (only when expanded) -->
        <button
          v-if="!sidebarCollapsed"
          class="brand-collapse-btn"
          @click="toggleCollapse"
          title="Collapse"
        >
          <svg class="w-[18px] h-[18px]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
          </svg>
        </button>
      </div>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 overflow-y-auto sidebar-scroll">
      <SidebarNavigation :collapsed="sidebarCollapsed" />
    </nav>

    <!-- Bottom / User Section (matching IAM .sidebar-bottom) -->
    <div :class="['sidebar-bottom', sidebarCollapsed ? 'sidebar-bottom-collapsed' : '']">
      <div
        class="user-item"
        :class="{ 'collapsed-user': sidebarCollapsed }"
      >
        <div class="user-avatar">
          {{ userInitials }}
          <div class="user-status-dot"></div>
        </div>
        <template v-if="!sidebarCollapsed">
          <div class="user-info">
            <span class="user-name-text">{{ userName }}</span>
            <span class="user-email">{{ userEmail }}</span>
          </div>
          <button @click="handleLogout" class="logout-btn" :title="$t('Logout')">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
            </svg>
          </button>
        </template>
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import {
  Dialog,
  DialogPanel,
  TransitionRoot,
  TransitionChild
} from '@headlessui/vue'
import Icon from '@/components/ui/Icon.vue'
import { useUserStore } from '@/stores/user'
import { useAppStore } from '@/stores/app'
import SidebarNavigation from './SidebarNavigation.vue'
import logoEn from '@/assets/misa_logo.svg'
import logoAr from '@/assets/misa_logo.svg'

// Stores & Router
const router = useRouter()
const userStore = useUserStore()
const appStore = useAppStore()

// Computed
const isRtl = computed(() => userStore.isRtl)
const sidebarCollapsed = computed(() => appStore.isSidebarCollapsed)
const mobileSidebarOpen = computed(() => appStore.isSidebarMobileOpen)

// User info for profile section
const userName = computed(() => userStore.loggedInUser?.fullName || '')
const userEmail = computed(() => userStore.loggedInUser?.email || '')
const userInitials = computed(() => {
  const name = userName.value
  if (!name) return ''
  const parts = name.trim().split(' ')
  if (parts.length >= 2) return parts[0][0] + parts[1][0]
  return name.substring(0, 2).toUpperCase()
})

// Methods
function toggleCollapse() {
  appStore.toggleSidebar()
}

function closeMobileSidebar() {
  appStore.closeMobileSidebar()
}

async function handleLogout() {
  userStore.removeUserSession()
  await router.push('/login')
}
</script>

<style scoped>
/* ===== Sidebar (matching IAM .modern-sidebar exactly) ===== */
.sidebar-width-expanded {
  width: 260px;
}

.sidebar-width-collapsed {
  width: 64px;
}

.sidebar-desktop {
  background: linear-gradient(180deg, #1a3c30 0%, #1e4637 50%, #162e25 100%);
  border-right: none;
  height: 100vh;
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-rendering: optimizeLegibility;
  overflow: hidden;
}

[dir="ltr"] .sidebar-desktop {
  border-radius: 0 16px 16px 0;
  box-shadow: 4px 0 24px rgba(0, 0, 0, 0.25), 1px 0 8px rgba(0, 0, 0, 0.15);
}

[dir="rtl"] .sidebar-desktop {
  border-radius: 16px 0 0 16px;
  box-shadow: -4px 0 24px rgba(0, 0, 0, 0.25), -1px 0 8px rgba(0, 0, 0, 0.15);
}

/* ===== Mobile Sidebar ===== */
.sidebar-mobile {
  width: 260px;
  background: linear-gradient(180deg, #1a3c30 0%, #1e4637 50%, #162e25 100%);
}

[dir="ltr"] .sidebar-mobile {
  border-radius: 0 16px 16px 0;
}

[dir="rtl"] .sidebar-mobile {
  border-radius: 16px 0 0 16px;
}

.mobile-close-btn {
  padding: 8px;
  border-radius: 8px;
  color: #a1a1aa;
  transition: all 0.15s;
}

.mobile-close-btn:hover {
  background: rgba(255, 255, 255, 0.08);
  color: #ffffff;
}

/* ===== Brand Header (matching IAM .sidebar-brand exactly) ===== */
.sidebar-brand {
  position: relative;
  height: 64px;
  flex-shrink: 0;
  overflow: hidden;
  background: linear-gradient(to right, #162e25, #1e4637, #162e25);
}

/* Teal hint overlay */
.sidebar-brand::before {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, rgba(0, 109, 75, 0.05), transparent, rgba(0, 109, 75, 0.03));
  pointer-events: none;
}

/* Accent line at bottom */
.sidebar-brand::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: linear-gradient(to right, transparent, #006d4b, transparent);
  opacity: 0.6;
  pointer-events: none;
}

.brand-content {
  position: relative;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  transition: padding 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.sidebar-width-collapsed .brand-content {
  padding: 0;
  justify-content: center;
}

.brand-link {
  display: flex;
  align-items: center;
  gap: 12px;
  text-decoration: none;
  min-width: 0;
  flex: 1;
  transition: gap 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.brand-link-collapsed {
  flex: 1;
  justify-content: center;
  background: none;
  border: none;
  cursor: pointer;
  padding: 0;
  gap: 0;
}

.logo-mark {
  position: relative;
  flex-shrink: 0;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  background: linear-gradient(135deg, #2a6b53, #1e5240);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.sidebar-width-collapsed .logo-mark {
  width: 44px;
  height: 44px;
}

.brand-link:hover .logo-mark {
  box-shadow: 0 4px 20px rgba(0, 109, 75, 0.6);
}

.brand-logo {
  height: 32px;
  width: auto;
  object-fit: contain;
  filter: brightness(0) invert(1);
}

/* ===== Collapse Button (matching IAM .brand-collapse-btn) ===== */
.brand-collapse-btn {
  flex-shrink: 0;
  width: 28px;
  height: 28px;
  border-radius: 7px;
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  display: flex;
  align-items: center;
  justify-content: center;
  color: rgba(255, 255, 255, 0.4);
  cursor: pointer;
  transition: background 0.2s cubic-bezier(0.4, 0, 0.2, 1), color 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.brand-collapse-btn:hover {
  background: rgba(0, 109, 75, 0.2);
  color: #006d4b;
  border-color: rgba(0, 109, 75, 0.3);
}

/* RTL: flip the chevron */
[dir="rtl"] .brand-collapse-btn svg {
  transform: rotate(180deg);
}

/* ===== Scrollbar ===== */
.sidebar-scroll {
  scrollbar-width: thin;
  scrollbar-color: rgba(0, 109, 75, 0.3) transparent;
}

.sidebar-scroll::-webkit-scrollbar {
  width: 4px;
}

.sidebar-scroll::-webkit-scrollbar-track {
  background: transparent;
}

.sidebar-scroll::-webkit-scrollbar-thumb {
  background: linear-gradient(180deg, #006d4b, #004730);
  border-radius: 2px;
}

.sidebar-scroll::-webkit-scrollbar-thumb:hover {
  background: linear-gradient(180deg, #198b5f, #006d4b);
}

/* ===== Sidebar Bottom / User Section (matching IAM .sidebar-bottom) ===== */
.sidebar-bottom {
  padding: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
  background: #004730;
  flex-shrink: 0;
  position: relative;
}

.sidebar-bottom::before {
  content: '';
  position: absolute;
  top: 0;
  inset-inline-start: 16px;
  inset-inline-end: 16px;
  height: 1px;
  background: linear-gradient(to right, transparent, rgba(0, 109, 75, 0.4), transparent);
}

.sidebar-bottom-collapsed {
  padding: 12px 8px;
}

.user-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 12px;
  border-radius: 8px;
  cursor: default;
  border-inline-start: none;
  transition: background 0.15s;
}

.user-item:hover {
  background: rgba(255, 255, 255, 0.04);
}

.collapsed-user {
  justify-content: center;
  padding: 10px;
}

.user-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: linear-gradient(135deg, #004730 0%, #006d4b 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 11px;
  font-weight: 600;
  flex-shrink: 0;
  position: relative;
  overflow: hidden;
}

.user-status-dot {
  position: absolute;
  bottom: 0;
  right: 0;
  width: 8px;
  height: 8px;
  background: #22c55e;
  border: 2px solid #006d4b;
  border-radius: 50%;
}

.user-info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.user-name-text {
  font-size: 13px;
  font-weight: 600;
  color: #ffffff;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  line-height: 1.3;
}

.user-email {
  font-size: 11px;
  color: #94a3b8;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  line-height: 1.3;
}

.logout-btn {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: none;
  border: none;
  color: #94a3b8;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  transition: all 0.15s ease;
}

.logout-btn:hover {
  color: #ef4444;
  background: rgba(239, 68, 68, 0.1);
}
</style>

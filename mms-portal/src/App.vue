<template>
  <div
    :dir="isRtl ? 'rtl' : 'ltr'"
    :class="[
      'min-h-screen app-background',
      { 'dark': isDarkMode }
    ]"
  >
    <!-- App is ready and user is authenticated -->
    <template v-if="isAppReady">
      <template v-if="isLoggedIn">
        <!-- Header -->
        <Header />

        <!-- Sidebar -->
        <Sidebar />

        <!-- Main content -->
        <main
          :class="[
            'main-content transition-all duration-300 pt-header min-h-screen',
            sidebarCollapsed
              ? 'ltr:ml-sidebar-collapsed rtl:mr-sidebar-collapsed'
              : 'ltr:ml-sidebar rtl:mr-sidebar',
            'lg:block',
            !showLayout ? 'z-[10000]' : ''
          ]"
        >
          <div class="content-wrapper">
            <RouterView v-slot="{ Component, route }">
              <Transition
                name="page"
                mode="out-in"
              >
                <Suspense>
                  <template #default>
                    <div :key="`${locale}-${route.path}`">
                      <component :is="Component" />
                    </div>
                  </template>
                  <template #fallback>
                    <PageLoader />
                  </template>
                </Suspense>
              </Transition>
            </RouterView>
          </div>
        </main>

        <!-- Mobile sidebar overlay -->
        <Transition name="fade">
          <div
            v-if="mobileSidebarOpen"
            class="fixed inset-0 bg-black/50 z-30 lg:hidden"
            @click="closeMobileSidebar"
          />
        </Transition>
      </template>

      <!-- User is not authenticated (login pages) -->
      <template v-else>
        <RouterView />
      </template>
    </template>

    <!-- App is not ready (loading translations) -->
    <Transition name="overlay-fade">
      <SiteOverlay v-if="!isAppReady" />
    </Transition>

    <!-- Global components -->
    <Toaster />
    <ConfirmDialog />

    <!-- In-progress meeting notification banner -->
    <InProgressMeetingBanner v-if="isLoggedIn" />
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useAppStore } from '@/stores/app'
import { isReady } from '@/plugins/i18n'
import { useI18n } from 'vue-i18n'

// Layout components
import Header from '@/components/layout/Header.vue'
import Sidebar from '@/components/layout/Sidebar.vue'
import SiteOverlay from '@/components/controls/SiteOverlay.vue'
import PageLoader from '@/components/controls/PageLoader.vue'
import ConfirmDialog from '@/components/controls/ConfirmDialog.vue'
import Toaster from '@/components/controls/Toaster.vue'
import InProgressMeetingBanner from '@/components/app/meeting/InProgressMeetingBanner.vue'

// Composables
import { useGlobalMeetingNotifications } from '@/composables/useGlobalMeetingNotifications'

// Stores
const userStore = useUserStore()
const appStore = useAppStore()
const route = useRoute()
const { locale } = useI18n()

// Global meeting notifications - auto-initializes via internal watcher when user is authenticated
useGlobalMeetingNotifications()

// Computed properties
const isRtl = computed(() => userStore.isRtl)
const isDarkMode = computed(() => false) // TODO: Implement dark mode

const isLoggedIn = computed(() => {
  return userStore.isAuthenticated && route.name !== 'login'
})

// Check if we should show the layout (header/sidebar) - hide on meeting room
const showLayout = computed(() => {
  return route.name !== 'meetingRoom'
})

const isAppReady = computed(() => {
  return isReady() || appStore.isTranslationFailed
})

const sidebarCollapsed = computed(() => appStore.isSidebarCollapsed)
const mobileSidebarOpen = computed(() => appStore.isSidebarMobileOpen)

// Methods
function closeMobileSidebar() {
  appStore.closeMobileSidebar()
}

// Prevent iframe embedding (security)
onMounted(() => {
  if (window.top !== window.self) {
    window.top!.location = window.location
  }
})

// Watch for language changes and update document attributes
watch(
  () => userStore.language,
  (newLang) => {
    document.documentElement.setAttribute('lang', newLang)
    document.documentElement.setAttribute('dir', newLang === 'ar' ? 'rtl' : 'ltr')
  },
  { immediate: true }
)
</script>

<style scoped>
/* Page transition - smooth slide and fade */
.page-enter-active {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.page-leave-active {
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.page-enter-from {
  opacity: 0;
  transform: translateY(12px);
}

.page-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

/* Fade transition (kept for other uses) */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Overlay fade out transition - smooth exit */
.overlay-fade-leave-active {
  transition: opacity 0.6s cubic-bezier(0.4, 0, 0.2, 1);
}

.overlay-fade-leave-to {
  opacity: 0;
}

/* Spacing variables matching Tailwind config */
.pt-header {
  padding-top: var(--header-height, 60px);
}

.ml-sidebar {
  margin-left: var(--sidebar-width, 260px);
}

.mr-sidebar {
  margin-right: var(--sidebar-width, 260px);
}

.ml-sidebar-collapsed {
  margin-left: var(--sidebar-collapsed-width, 64px);
}

.mr-sidebar-collapsed {
  margin-right: var(--sidebar-collapsed-width, 64px);
}

/* Mobile responsive: no sidebar margin on small screens */
@media (max-width: 1023px) {
  .ml-sidebar,
  .mr-sidebar,
  .ml-sidebar-collapsed,
  .mr-sidebar-collapsed {
    margin-left: 0;
    margin-right: 0;
  }
}

/* App background - matching IAM content-area */
.app-background {
  background-color: #f8fafc;
}

/* Dark mode background */
.dark .app-background {
  background-color: #0f172a;
  background-image: linear-gradient(180deg, #1e293b 0%, #0f172a 100%);
}

/* Main content area */
.main-content {
  position: relative;
  overflow-x: hidden;
}

/* Content area — matching IAM .content-area padding */
.content-wrapper {
  position: relative;
  z-index: 1;
  padding: 20px 24px;
}

@media (max-width: 1023px) {
  .content-wrapper {
    padding: 16px;
  }
}

@media (max-width: 480px) {
  .content-wrapper {
    padding: 12px;
  }
}

</style>

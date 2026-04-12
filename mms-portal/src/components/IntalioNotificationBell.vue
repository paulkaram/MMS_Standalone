<template>
  <div class="ntf-bell-wrap" ref="bellRef">
    <button class="ntf-bell-btn" @click="toggleDropdown" :title="unreadCount > 0 ? `${unreadCount} unread` : 'Notifications'">
      <span class="material-symbols-outlined" :class="{ 'ntf-bell-shake': hasNewNotification }">notifications</span>
      <span v-if="unreadCount > 0" class="ntf-badge" :class="{ 'ntf-badge-pop': hasNewNotification }">{{ unreadCount > 99 ? '99+' : unreadCount }}</span>
    </button>

    <Transition name="ntf-dd">
      <div v-if="open" class="ntf-dropdown">
        <div class="ntf-header">
          <span class="ntf-header-title">Notifications</span>
          <button v-if="unreadCount > 0" class="ntf-mark-all" @click="markAllRead">Mark all read</button>
        </div>

        <div v-if="loading" class="ntf-loading">
          <span class="material-symbols-outlined ntf-spin">progress_activity</span>
        </div>

        <div v-else-if="notifications.length === 0" class="ntf-empty">
          <span class="material-symbols-outlined">notifications_none</span>
          <span>No notifications</span>
        </div>

        <div v-else class="ntf-list">
          <div
            v-for="n in notifications"
            :key="n.id"
            class="ntf-item"
            :class="{ 'ntf-item--unread': !n.isRead }"
            @click="handleClick(n)"
          >
            <div class="ntf-item-dot" :class="getSourceClass(n.sourceApp)"></div>
            <div class="ntf-item-body">
              <div class="ntf-item-title">{{ isAr ? (n.titleAr || n.titleEn) : (n.titleEn || n.titleAr) }}</div>
              <div class="ntf-item-msg">{{ isAr ? (n.messageAr || n.messageEn) : (n.messageEn || n.messageAr) }}</div>
              <div class="ntf-item-meta">
                <span class="ntf-item-source">{{ n.sourceApp || 'IAM' }}</span>
                <span>{{ formatTime(n.createdAt) }}</span>
              </div>
            </div>
            <button class="ntf-item-del" @click.stop="deleteNotification(n)" title="Delete">
              <span class="material-symbols-outlined">close</span>
            </button>
          </div>
        </div>

        <div v-if="notifications.length > 0 || totalCount > 0" class="ntf-footer">
          <a class="ntf-footer-link" :href="iamPortalUrl + '/notifications'" target="_blank">
            View all notifications
            <span class="material-symbols-outlined">open_in_new</span>
          </a>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import * as signalR from '@microsoft/signalr'
import axios from 'axios'

interface Props {
  token: string
  iamApiUrl?: string
  iamHubUrl?: string
  iamPortalUrl?: string
  language?: string
}

const props = withDefaults(defineProps<Props>(), {
  iamApiUrl: 'http://localhost:5100/api',
  iamHubUrl: 'http://localhost:5100/hubs/iam',
  iamPortalUrl: 'http://localhost:4500',
  language: 'en'
})

interface Notification {
  id: string
  sourceApp: string | null
  type: string
  titleEn: string | null
  titleAr: string | null
  messageEn: string | null
  messageAr: string | null
  priority: number
  actionUrl: string | null
  isRead: boolean
  createdAt: string
}

const open = ref(false)
const loading = ref(false)
const notifications = ref<Notification[]>([])
const unreadCount = ref(0)
const totalCount = ref(0)
const hasNewNotification = ref(false)
const bellRef = ref<HTMLElement>()
const isAr = ref(props.language === 'ar')

let connection: signalR.HubConnection | null = null

watch(() => props.language, (val) => { isAr.value = val === 'ar' })

function toggleDropdown() {
  open.value = !open.value
  if (open.value && notifications.value.length === 0) {
    loadNotifications()
  }
}

function handleClickOutside(e: MouseEvent) {
  if (bellRef.value && !bellRef.value.contains(e.target as Node)) {
    open.value = false
  }
}

async function loadNotifications() {
  loading.value = true
  try {
    const res = await axios.get(`${props.iamApiUrl}/notifications?page=1&pageSize=15`, {
      headers: { Authorization: `Bearer ${props.token}` }
    })
    const data = res.data?.data || res.data
    notifications.value = data?.notifications || []
    unreadCount.value = data?.unreadCount ?? 0
    totalCount.value = data?.totalCount ?? 0
  } catch (err) {
    console.error('[NotificationBell] Failed to load:', err)
  } finally {
    loading.value = false
  }
}

async function loadUnreadCount() {
  try {
    const res = await axios.get(`${props.iamApiUrl}/notifications/unread-count`, {
      headers: { Authorization: `Bearer ${props.token}` }
    })
    const data = res.data?.data || res.data
    unreadCount.value = data?.unreadCount ?? 0
  } catch { /* silent */ }
}

async function markAsRead(n: Notification) {
  if (n.isRead) return
  try {
    await axios.post(`${props.iamApiUrl}/notifications/${n.id}/mark-read`, null, {
      headers: { Authorization: `Bearer ${props.token}` }
    })
    n.isRead = true
    unreadCount.value = Math.max(0, unreadCount.value - 1)
  } catch { /* silent */ }
}

async function markAllRead() {
  try {
    await axios.post(`${props.iamApiUrl}/notifications/mark-all-read`, null, {
      headers: { Authorization: `Bearer ${props.token}` }
    })
    notifications.value.forEach(n => n.isRead = true)
    unreadCount.value = 0
  } catch { /* silent */ }
}

async function deleteNotification(n: Notification) {
  try {
    await axios.delete(`${props.iamApiUrl}/notifications/${n.id}`, {
      headers: { Authorization: `Bearer ${props.token}` }
    })
    notifications.value = notifications.value.filter(x => x.id !== n.id)
    if (!n.isRead) unreadCount.value = Math.max(0, unreadCount.value - 1)
    totalCount.value = Math.max(0, totalCount.value - 1)
  } catch { /* silent */ }
}

function handleClick(n: Notification) {
  markAsRead(n)
  if (n.actionUrl) {
    window.open(n.actionUrl, '_blank')
  }
}

function getSourceClass(source: string | null): string {
  const s = (source || '').toLowerCase()
  if (s.includes('mms') || s.includes('meeting')) return 'ntf-dot--mms'
  if (s.includes('case')) return 'ntf-dot--case'
  return 'ntf-dot--iam'
}

function formatTime(dateStr: string): string {
  try {
    const d = new Date(dateStr)
    const now = new Date()
    const diffH = Math.floor((now.getTime() - d.getTime()) / 3600000)
    if (diffH < 1) return isAr.value ? 'الآن' : 'Just now'
    if (diffH < 24) return isAr.value ? `منذ ${diffH} ساعة` : `${diffH}h ago`
    if (diffH < 48) return isAr.value ? 'أمس' : 'Yesterday'
    return d.toLocaleDateString(isAr.value ? 'ar-SA' : 'en-US', { month: 'short', day: 'numeric' })
  } catch { return '' }
}

async function connectHub() {
  if (!props.token) return

  connection = new signalR.HubConnectionBuilder()
    .withUrl(props.iamHubUrl, {
      accessTokenFactory: () => props.token
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  connection.on('ReceiveNotification', (payload: any) => {
    // Add to top of list
    notifications.value.unshift({
      id: payload.id,
      sourceApp: payload.sourceApp,
      type: payload.type,
      titleEn: payload.titleEn,
      titleAr: payload.titleAr,
      messageEn: payload.messageEn,
      messageAr: payload.messageAr,
      priority: payload.priority,
      actionUrl: payload.actionUrl,
      isRead: false,
      createdAt: payload.createdAt
    })
    unreadCount.value = payload.unreadCount ?? (unreadCount.value + 1)
    hasNewNotification.value = true
    setTimeout(() => { hasNewNotification.value = false }, 3000)
  })

  try {
    await connection.start()
  } catch (err) {
    console.error('[NotificationBell] Hub connection failed:', err)
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
  loadUnreadCount()
  connectHub()
})

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
  connection?.stop()
})
</script>

<style scoped>
.ntf-bell-wrap {
  position: relative;
}

.ntf-bell-btn {
  position: relative;
  background: none;
  border: none;
  cursor: pointer;
  padding: 6px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s;
  color: inherit;
}

.ntf-bell-btn:hover { background: rgba(0,0,0,0.05); }

.ntf-bell-btn .material-symbols-outlined { font-size: 22px; }

.ntf-badge {
  position: absolute;
  top: 2px; right: 0;
  min-width: 16px; height: 16px;
  border-radius: 8px;
  background: #ef4444;
  color: #fff;
  font-size: 10px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 4px;
  line-height: 1;
  transition: transform 0.2s;
}

.ntf-badge-pop {
  animation: ntf-pop 0.5s cubic-bezier(0.36, 0.07, 0.19, 0.97);
}

@keyframes ntf-pop {
  0% { transform: scale(1); }
  30% { transform: scale(1.5); }
  60% { transform: scale(0.9); }
  100% { transform: scale(1); }
}

.ntf-bell-shake {
  animation: ntf-shake 0.6s cubic-bezier(0.36, 0.07, 0.19, 0.97);
  transform-origin: top center;
}

@keyframes ntf-shake {
  0% { transform: rotate(0); }
  15% { transform: rotate(14deg); }
  30% { transform: rotate(-12deg); }
  45% { transform: rotate(10deg); }
  60% { transform: rotate(-8deg); }
  75% { transform: rotate(4deg); }
  100% { transform: rotate(0); }
}

/* Dropdown */
.ntf-dropdown {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  width: 360px;
  max-height: 480px;
  background: #fff;
  border: 1px solid #d1d5db;
  border-radius: 12px;
  box-shadow: 0 8px 30px rgba(0,0,0,0.15);
  z-index: 1050;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

[dir="rtl"] .ntf-dropdown { right: auto; left: 0; }

.ntf-dd-enter-active, .ntf-dd-leave-active { transition: all 0.2s ease; }
.ntf-dd-enter-from, .ntf-dd-leave-to { opacity: 0; transform: translateY(-6px) scale(0.97); }

/* Header */
.ntf-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 16px;
  border-bottom: 1px solid #f1f5f9;
}

.ntf-header-title {
  font-size: 14px;
  font-weight: 700;
  color: #0f172a;
}

.ntf-mark-all {
  background: none;
  border: none;
  font-size: 12px;
  font-weight: 600;
  color: #006d4b;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 6px;
  transition: background 0.15s;
  font-family: inherit;
}

.ntf-mark-all:hover { background: rgba(0,174,140,0.08); }

/* List */
.ntf-list {
  overflow-y: auto;
  flex: 1;
  max-height: 380px;
}

.ntf-item {
  display: flex;
  gap: 10px;
  padding: 12px 16px;
  border-bottom: 1px solid #f8fafc;
  cursor: pointer;
  transition: background 0.15s;
}

.ntf-item:last-child { border-bottom: none; }
.ntf-item:hover { background: #f8fafc; }

.ntf-item--unread {
  background: rgba(0,174,140,0.03);
}

.ntf-item--unread .ntf-item-title { color: #0f172a; }

.ntf-item-dot {
  width: 8px; height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
  margin-top: 5px;
}

.ntf-dot--mms { background: #006d4b; }
.ntf-dot--case { background: #006d4b; }
.ntf-dot--iam { background: #006d4b; }

.ntf-item-body { flex: 1; min-width: 0; }

.ntf-item-title {
  font-size: 13px;
  font-weight: 600;
  color: #64748b;
  line-height: 1.3;
}

.ntf-item-msg {
  font-size: 12px;
  color: #94a3b8;
  margin-top: 2px;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.ntf-item-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 4px;
  font-size: 11px;
  color: #cbd5e1;
}

.ntf-item-source {
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  font-size: 10px;
}

/* Loading / Empty */
.ntf-loading, .ntf-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 40px 20px;
  color: #cbd5e1;
  font-size: 13px;
}

.ntf-loading .material-symbols-outlined,
.ntf-empty .material-symbols-outlined { font-size: 28px; }

.ntf-spin { animation: ntf-spin-anim 1s linear infinite; }
@keyframes ntf-spin-anim { to { transform: rotate(360deg); } }

/* Delete button */
.ntf-item-del {
  background: none;
  border: none;
  cursor: pointer;
  width: 28px; height: 28px;
  border-radius: 50%;
  opacity: 0;
  transition: all 0.2s;
  flex-shrink: 0;
  color: #cbd5e1;
  display: flex;
  align-items: center;
  justify-content: center;
  align-self: center;
}

.ntf-item-del .material-symbols-outlined { font-size: 14px; }
.ntf-item:hover .ntf-item-del { opacity: 1; }
.ntf-item-del:hover { background: #fef2f2; color: #ef4444; transform: scale(1.1); }

/* Footer */
.ntf-footer {
  padding: 10px 16px;
  border-top: 1px solid #f1f5f9;
  text-align: center;
}

.ntf-footer-link {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  font-weight: 600;
  color: #006d4b;
  text-decoration: none;
  padding: 4px 8px;
  border-radius: 6px;
  transition: background 0.15s;
}

.ntf-footer-link:hover { background: rgba(0,174,140,0.08); }
.ntf-footer-link .material-symbols-outlined { font-size: 14px; }
</style>

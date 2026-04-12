<template>
  <div class="ua" :class="sizeClass" :title="name">
    <img
      v-if="imgSrc && imgFailed < 2"
      :src="imgSrc"
      :alt="name"
      class="ua-img"
      @error="onImgError"
    />
    <span v-else class="ua-initials">{{ initials }}</span>
    <span v-if="showStatus" class="ua-status" :class="online ? 'online' : 'offline'"></span>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'

const props = withDefaults(defineProps<{
  userId?: string | number | null
  name?: string
  profilePictureUrl?: string | null
  hasProfilePicture?: boolean
  size?: 'xs' | 'sm' | 'md' | 'lg'
  online?: boolean
  showStatus?: boolean
}>(), {
  userId: null,
  name: '',
  profilePictureUrl: null,
  hasProfilePicture: undefined,
  size: 'md',
  online: false,
  showStatus: false
})

const imgFailed = ref(0) // 0 = no fail, 1 = primary failed, 2 = fallback failed

// Reset failed state when userId or profilePictureUrl changes
watch(() => [props.userId, props.profilePictureUrl], () => {
  imgFailed.value = 0
})

const sizeClass = computed(() => `ua-${props.size}`)

// Try IAM profile image first, then MMS fallback
const iamAuthority = import.meta.env.VITE_IAM_AUTHORITY || ''
const mmsBaseUrl = (import.meta.env.VITE_MAIN_API || '/api/').replace(/\/$/, '')

const imgSrc = computed(() => {
  if (props.hasProfilePicture === false) return null
  if (props.profilePictureUrl) return props.profilePictureUrl
  if (!props.userId) return null

  if (imgFailed.value === 0 && iamAuthority) {
    // Try IAM profile image first
    return `${iamAuthority}/api/user/${props.userId}/profile-image`
  }
  if (imgFailed.value <= 1) {
    // Fallback to MMS profile image
    return `${mmsBaseUrl}/users/profile-image/${props.userId}`
  }
  return null // Both failed
})

const onImgError = () => {
  imgFailed.value++
}

const initials = computed(() => {
  if (!props.name) return '?'
  const parts = props.name.trim().split(/\s+/)
  if (parts.length > 1) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return props.name.substring(0, 2).toUpperCase()
})
</script>

<style scoped>
.ua {
  position: relative;
  border-radius: 50%;
  background: #006d4b;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: visible;
  flex-shrink: 0;
}

.ua-initials {
  color: #ffffff;
}

.ua-xs { width: 28px; height: 28px; }
.ua-sm { width: 36px; height: 36px; }
.ua-md { width: 44px; height: 44px; }
.ua-lg { width: 56px; height: 56px; }

.ua-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 50%;
}

.ua-initials {
  color: #ffffff;
  font-weight: 700;
  letter-spacing: 0.3px;
  user-select: none;
}

.ua-xs .ua-initials { font-size: 10px; }
.ua-sm .ua-initials { font-size: 12px; }
.ua-md .ua-initials { font-size: 14px; }
.ua-lg .ua-initials { font-size: 18px; }

.ua-status {
  position: absolute;
  bottom: 1px;
  inset-inline-end: 1px;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  border: 2px solid #fff;
}

.ua-status.online { background: #006d4b; }
.ua-status.offline { background: #94a3b8; }
</style>

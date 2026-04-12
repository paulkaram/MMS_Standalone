<template>
  <div class="stat-card">
    <!-- Top: icon + label -->
    <div class="stat-top">
      <div class="stat-icon-wrap" :style="{ background: iconBg }">
        <Icon :icon="icon" class="stat-icon" :style="{ color: accentColor }" />
      </div>
      <span class="stat-label">{{ title }}</span>
    </div>

    <!-- Bottom: value + trend -->
    <div class="stat-bottom">
      <span class="stat-value">{{ value }}</span>
      <span v-if="trend || change" :class="['stat-trend', trendClass]">
        {{ trend || change }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'

interface Props {
  title: string
  value: string | number
  icon: string
  trend?: string
  trendType?: 'positive' | 'negative' | 'neutral'
  color?: 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info'
  change?: string
}

const props = withDefaults(defineProps<Props>(), {
  trend: undefined,
  trendType: 'neutral',
  color: 'primary',
  change: undefined
})

const colorMap: Record<string, string> = {
  primary: '#006d4b',
  secondary: '#63a58f',
  success: '#22c55e',
  warning: '#f59e0b',
  error: '#ef4444',
  info: '#3b82f6'
}

const accentColor = computed(() => colorMap[props.color] || colorMap.primary)

const iconBg = computed(() => {
  const c = accentColor.value
  const r = parseInt(c.slice(1, 3), 16)
  const g = parseInt(c.slice(3, 5), 16)
  const b = parseInt(c.slice(5, 7), 16)
  return `rgba(${r},${g},${b},0.08)`
})

const trendClass = computed(() => {
  if (props.trendType === 'positive') return 'trend-positive'
  if (props.trendType === 'negative') return 'trend-negative'
  return ''
})
</script>

<style scoped>
.stat-card {
  position: relative;
  background: #fff;
  border-radius: 12px;
  padding: 22px 24px;
  color: #1a2e25;
  overflow: hidden;
  transition: all 0.3s ease;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
  border: 1px solid #e4ede8;
  border-top: 3px solid #006d4b;
  min-height: 130px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(0, 109, 75, 0.08);
  border-color: #c8ddd3;
  border-top-color: #006d4b;
}

.stat-top {
  display: flex;
  align-items: center;
  gap: 10px;
}

.stat-icon-wrap {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-icon {
  font-size: 20px;
  width: 20px;
  height: 20px;
}

.stat-label {
  font-size: 13px;
  font-weight: 600;
  color: #5a7a6d;
  letter-spacing: 0.01em;
}

.stat-bottom {
  margin-top: auto;
  padding-top: 8px;
}

.stat-value {
  font-size: 32px;
  font-weight: 800;
  color: #1a2e25;
  line-height: 1;
  display: block;
}

.stat-trend {
  font-size: 13px;
  font-weight: 600;
  color: #5a7a6d;
  margin-top: 6px;
  display: flex;
  align-items: center;
  gap: 4px;
}

.stat-trend.trend-positive {
  color: #22c55e;
}

.stat-trend.trend-negative {
  color: #ef4444;
}
</style>

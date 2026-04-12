<template>
  <div ref="chartRef" class="pie-chart"></div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import ApexCharts from 'apexcharts'

interface Props {
  labels: string[]
  series: number[]
  colors?: string[]
  height?: string
  showLegend?: boolean
  legendPosition?: 'top' | 'bottom' | 'left' | 'right'
  donut?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  colors: () => ['#F59E0B', '#006d4b', '#EF4444'],
  height: '300',
  showLegend: true,
  legendPosition: 'bottom',
  donut: false
})

const chartRef = ref<HTMLElement | null>(null)
let chart: ApexCharts | null = null

const buildOptions = () => {
  return {
    chart: {
      type: props.donut ? 'donut' : 'pie',
      height: parseInt(props.height) || 300,
      fontFamily: 'Tajawal, sans-serif'
    },
    labels: props.labels,
    series: props.series,
    colors: props.colors,
    legend: {
      show: props.showLegend,
      position: props.legendPosition,
      fontFamily: 'Tajawal, sans-serif'
    },
    dataLabels: {
      enabled: true,
      formatter: (val: number) => `${val.toFixed(1)}%`
    },
    responsive: [
      {
        breakpoint: 480,
        options: {
          legend: { position: 'bottom' }
        }
      }
    ]
  }
}

const initChart = () => {
  if (!chartRef.value) return
  if (chart) {
    chart.destroy()
    chart = null
  }

  if (!props.labels?.length || !props.series?.length) return
  if (!props.series.some(v => v > 0)) return

  try {
    const options = buildOptions()
    chart = new ApexCharts(chartRef.value, options)
    chart.render()
  } catch (err) {
    console.error('PieChart render error:', err)
  }
}

watch(
  () => [props.labels, props.series],
  () => initChart(),
  { deep: true }
)

onMounted(() => {
  setTimeout(initChart, 300)
})

onUnmounted(() => {
  if (chart) {
    chart.destroy()
    chart = null
  }
})
</script>

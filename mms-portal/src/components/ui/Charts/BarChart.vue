<template>
  <div ref="chartRef" class="bar-chart"></div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import ApexCharts from 'apexcharts'

interface ChartSeries {
  name: string
  data: number[]
}

interface Props {
  categories: string[]
  series: ChartSeries[]
  colors?: string[]
  height?: string
  horizontal?: boolean
  stacked?: boolean
  showLegend?: boolean
  distributed?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  colors: () => ['#006d4b'],
  height: '300',
  horizontal: false,
  stacked: false,
  showLegend: false,
  distributed: false
})

const chartRef = ref<HTMLElement | null>(null)
let chart: ApexCharts | null = null

const buildOptions = () => {
  const isRTL = document.dir === 'rtl' || document.documentElement.dir === 'rtl'

  return {
    chart: {
      type: 'bar',
      height: parseInt(props.height) || 300,
      toolbar: { show: false },
      fontFamily: 'Tajawal, sans-serif'
    },
    plotOptions: {
      bar: {
        horizontal: props.horizontal,
        borderRadius: 4,
        columnWidth: '55%',
        distributed: props.distributed
      }
    },
    dataLabels: { enabled: false },
    series: props.series,
    xaxis: {
      categories: props.categories,
      labels: {
        show: false
      },
      axisBorder: { show: false },
      axisTicks: { show: false }
    },
    yaxis: {
      labels: {
        style: { fontFamily: 'Tajawal, sans-serif' }
      },
      opposite: isRTL
    },
    colors: props.colors,
    legend: {
      show: props.showLegend,
      position: 'bottom',
      horizontalAlign: 'center',
      fontFamily: 'Tajawal, sans-serif',
      fontSize: '12px',
      inverseOrder: isRTL,
      markers: {
        size: 8,
        offsetX: isRTL ? 4 : -4
      },
      itemMargin: {
        horizontal: 10,
        vertical: 8
      }
    },
    tooltip: {
      enabled: true,
      style: {
        fontFamily: 'Tajawal, sans-serif'
      },
      y: {
        formatter: function(val: number) {
          return val.toString()
        }
      }
    },
    grid: {
      borderColor: '#f1f1f1',
      padding: {
        right: isRTL ? 0 : 10,
        left: isRTL ? 10 : 0
      }
    }
  }
}

const initChart = () => {
  if (!chartRef.value) return
  if (chart) {
    chart.destroy()
    chart = null
  }

  if (!props.categories?.length || !props.series?.length) return

  try {
    const options = buildOptions()
    chart = new ApexCharts(chartRef.value, options)
    chart.render()
  } catch (err) {
    console.error('BarChart render error:', err)
  }
}

watch(
  () => [props.categories, props.series],
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

<style scoped>
.bar-chart :deep(.apexcharts-legend) {
  justify-content: center !important;
  flex-wrap: wrap;
  gap: 6px;
}

.bar-chart :deep(.apexcharts-legend-series) {
  display: inline-flex !important;
  align-items: center;
  margin: 3px 4px !important;
  padding: 3px 10px;
  background: #f8f9fa;
  border-radius: 16px;
}

.bar-chart :deep(.apexcharts-legend-text) {
  padding-inline-start: 0 !important;
  padding-inline-end: 8px !important;
  margin: 0 !important;
  font-size: 11px;
}

.bar-chart :deep(.apexcharts-legend-marker) {
  margin: 0 !important;
}
</style>

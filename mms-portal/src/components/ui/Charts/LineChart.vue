<template>
  <div ref="chartRef" class="line-chart" :style="{ height: height }"></div>
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
  showLegend?: boolean
  title?: string
  xAxisTitle?: string
  yAxisTitle?: string
  curve?: 'smooth' | 'straight' | 'stepline'
  showDataLabels?: boolean
  showArea?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  colors: () => ['#520773', '#C05A7C', '#2CA69A', '#456191', '#F2A365', '#845EC2'],
  height: '350px',
  showLegend: true,
  curve: 'smooth',
  showDataLabels: false,
  showArea: false
})

const chartRef = ref<HTMLElement | null>(null)
let chart: ApexCharts | null = null

const getOptions = () => ({
  chart: {
    type: props.showArea ? 'area' : 'line',
    fontFamily: 'Tajawal, sans-serif',
    toolbar: {
      show: false
    },
    zoom: {
      enabled: false
    }
  },
  stroke: {
    curve: props.curve,
    width: 3
  },
  fill: props.showArea
    ? {
        type: 'gradient',
        gradient: {
          shadeIntensity: 1,
          opacityFrom: 0.4,
          opacityTo: 0.1,
          stops: [0, 90, 100]
        }
      }
    : undefined,
  series: props.series,
  colors: props.colors,
  xaxis: {
    categories: props.categories,
    title: props.xAxisTitle
      ? {
          text: props.xAxisTitle,
          style: {
            fontFamily: 'Tajawal, sans-serif'
          }
        }
      : undefined
  },
  yaxis: {
    title: props.yAxisTitle
      ? {
          text: props.yAxisTitle,
          style: {
            fontFamily: 'Tajawal, sans-serif'
          }
        }
      : undefined
  },
  title: props.title
    ? {
        text: props.title,
        align: 'center' as const,
        style: {
          fontSize: '16px',
          fontWeight: 'bold',
          fontFamily: 'Tajawal, sans-serif'
        }
      }
    : undefined,
  legend: {
    show: props.showLegend,
    fontFamily: 'Tajawal, sans-serif'
  },
  dataLabels: {
    enabled: props.showDataLabels
  },
  grid: {
    borderColor: '#e7e7e7'
  },
  markers: {
    size: 4,
    hover: {
      size: 6
    }
  },
  tooltip: {
    shared: true,
    intersect: false
  }
})

const renderChart = () => {
  if (chart) {
    chart.updateOptions(getOptions())
  } else if (chartRef.value) {
    chart = new ApexCharts(chartRef.value, getOptions())
    chart.render()
  }
}

watch(
  () => [props.categories, props.series, props.colors],
  () => {
    renderChart()
  },
  { deep: true }
)

onMounted(() => {
  renderChart()
})

onUnmounted(() => {
  chart?.destroy()
})
</script>

<template>
  <div class="stepper">
    <!-- Stepper Header -->
    <div class="stepper-header">
      <div class="flex items-center justify-between relative">
        <!-- Progress line -->
        <div class="absolute top-5 inset-x-0 h-0.5 bg-zinc-200 mx-12"></div>
        <div
          class="absolute top-5 h-0.5 bg-primary transition-all duration-300 mx-12"
          :style="{ width: progressWidth }"
        ></div>

        <!-- Steps -->
        <div
          v-for="(step, index) in steps"
          :key="index"
          class="flex flex-col items-center relative z-10 cursor-pointer"
          :class="{ 'pointer-events-none': !allowStepClick }"
          @click="handleStepClick(index + 1)"
        >
          <!-- Step Circle -->
          <div
            :class="[
              'w-10 h-10 rounded-full flex items-center justify-center text-sm font-semibold transition-all duration-200',
              getStepClass(index + 1)
            ]"
          >
            <Icon
              v-if="isStepComplete(index + 1)"
              icon="mdi:check"
              class="w-5 h-5"
            />
            <span v-else>{{ index + 1 }}</span>
          </div>

          <!-- Step Label -->
          <span
            :class="[
              'mt-2 text-sm font-medium transition-colors whitespace-nowrap',
              modelValue === index + 1 ? 'text-primary' :
              isStepComplete(index + 1) ? 'text-success' : 'text-zinc-500'
            ]"
          >
            {{ step.title }}
          </span>
        </div>
      </div>
    </div>

    <!-- Stepper Content -->
    <div class="stepper-content mt-8">
      <slot :name="`step-${modelValue}`"></slot>
    </div>

    <!-- Stepper Actions -->
    <div class="stepper-actions mt-6 flex items-center justify-between border-t border-gray-200 pt-4">
      <div>
        <Button
          v-if="modelValue > 1"
          variant="outline"
          icon-left="mdi:arrow-right"
          @click="previousStep"
        >
          {{ previousLabel }}
        </Button>
      </div>

      <div class="flex items-center gap-3">
        <slot name="actions"></slot>

        <Button
          v-if="showSave"
          variant="outline"
          :loading="saving"
          @click="$emit('save')"
        >
          {{ saveLabel }}
        </Button>

        <Button
          v-if="modelValue < steps.length"
          variant="primary"
          icon-right="mdi:arrow-left"
          :loading="loading"
          :disabled="!canProceed"
          @click="nextStep"
        >
          {{ nextLabel }}
        </Button>

        <Button
          v-if="modelValue === steps.length && showComplete"
          variant="primary"
          :loading="loading"
          @click="$emit('complete')"
        >
          {{ completeLabel }}
        </Button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import Icon from '@/components/ui/Icon.vue'
import Button from './Button.vue'

interface Step {
  title: string
  subtitle?: string
  complete?: boolean
}

interface Props {
  modelValue: number
  steps: Step[]
  loading?: boolean
  saving?: boolean
  canProceed?: boolean
  allowStepClick?: boolean
  showSave?: boolean
  showComplete?: boolean
  previousLabel?: string
  nextLabel?: string
  saveLabel?: string
  completeLabel?: string
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  saving: false,
  canProceed: true,
  allowStepClick: false,
  showSave: true,
  showComplete: true,
  previousLabel: 'السابق',
  nextLabel: 'التالي',
  saveLabel: 'حفظ',
  completeLabel: 'إنهاء'
})

const emit = defineEmits<{
  'update:modelValue': [value: number]
  'step-change': [step: number]
  'save': []
  'complete': []
}>()

// Computed
const progressWidth = computed(() => {
  const percentage = ((props.modelValue - 1) / (props.steps.length - 1)) * 100
  return `calc(${percentage}% - 3rem)`
})

// Methods
const isStepComplete = (step: number): boolean => {
  return props.steps[step - 1]?.complete ?? step < props.modelValue
}

const getStepClass = (step: number): string => {
  if (props.modelValue === step) {
    return 'bg-primary text-white shadow-lg ring-4 ring-primary-100'
  }
  if (isStepComplete(step)) {
    return 'bg-success text-white'
  }
  return 'bg-white border-2 border-zinc-300 text-zinc-500'
}

const nextStep = () => {
  if (props.modelValue < props.steps.length) {
    const newStep = props.modelValue + 1
    emit('update:modelValue', newStep)
    emit('step-change', newStep)
  }
}

const previousStep = () => {
  if (props.modelValue > 1) {
    const newStep = props.modelValue - 1
    emit('update:modelValue', newStep)
    emit('step-change', newStep)
  }
}

const handleStepClick = (step: number) => {
  if (props.allowStepClick && step <= props.modelValue) {
    emit('update:modelValue', step)
    emit('step-change', step)
  }
}
</script>

<style scoped>
.stepper-header {
  padding: 0 1rem;
}

[dir="rtl"] .stepper-actions .mdi-arrow-right {
  transform: scaleX(-1);
}

[dir="rtl"] .stepper-actions .mdi-arrow-left {
  transform: scaleX(-1);
}
</style>

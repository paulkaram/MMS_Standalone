<template>
  <nav class="flex items-center gap-1 text-sm">
    <button
      type="button"
      :class="[
        'hover:underline transition-colors',
        parents.length === 0 ? 'text-primary font-semibold' : 'text-zinc-600 hover:text-primary'
      ]"
      @click="goToRoot"
    >
      {{ $t('CouncilsAndCommitteesData') }}
    </button>

    <template v-for="(committee, index) in parents" :key="committee.id">
      <Icon icon="mdi:chevron-left" class="w-4 h-4 text-zinc-400 rtl:rotate-180" />
      <button
        type="button"
        :class="[
          'hover:underline transition-colors',
          index === parents.length - 1 ? 'text-primary font-semibold' : 'text-zinc-600 hover:text-primary'
        ]"
        @click="goToCommittee(committee, index)"
      >
        {{ committee.name }}
      </button>
    </template>
  </nav>
</template>

<script setup lang="ts">
import Icon from '@/components/ui/Icon.vue'
import { useRouter, useRoute } from 'vue-router'

interface Parent {
  id: string
  name: string
}

defineProps<{
  parents: Parent[]
}>()

const router = useRouter()
const route = useRoute()

const goToRoot = () => {
  if (route.path !== '/council-committees-general-info') {
    router.push({ name: 'council-committees-general-info' })
  }
}

const goToCommittee = (committee: Parent, index: number) => {
  // Don't navigate if clicking on the last item (current page)
  if (index === (route.params.councilId ? -1 : 0)) return

  if (route.path !== `/council-committees-general-info/${committee.id}`) {
    router.push({
      name: 'council-committees-general-info',
      params: { councilId: committee.id }
    })
  }
}
</script>

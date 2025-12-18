<template>
  <div class="bg-white shadow rounded-lg">
    <div class="px-4 py-5 sm:p-6">
      <h3 class="text-lg font-medium text-gray-900 mb-4">Quick Actions</h3>

      <div class="grid grid-cols-1 gap-4">
        <BaseButton
          variant="primary"
          size="lg"
          full-width
          @click="$router.push('/projects/new')"
        >
          <PlusIcon class="h-5 w-5 mr-2" />
          Create New Project
        </BaseButton>

        <BaseButton
          variant="outline"
          size="lg"
          full-width
          @click="$router.push('/tasks')"
        >
          <CheckCircleIcon class="h-5 w-5 mr-2" />
          View All Tasks
        </BaseButton>

        <BaseButton
          variant="outline"
          size="lg"
          full-width
          @click="$router.push('/projects')"
        >
          <FolderIcon class="h-5 w-5 mr-2" />
          Browse Projects
        </BaseButton>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import BaseButton from '@/components/ui/BaseButton.vue'
import {
  FolderIcon,
  CheckCircleIcon,
  PlusIcon
} from '@heroicons/vue/24/outline'
</script>
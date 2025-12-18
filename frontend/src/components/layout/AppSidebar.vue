<template>
  <div>
    <!-- Mobile sidebar overlay -->
    <div v-if="open" class="fixed inset-0 flex z-40 md:hidden">
      <div
        class="fixed inset-0 bg-gray-600 bg-opacity-75"
        @click="$emit('close')"
      ></div>

      <div class="relative flex-1 flex flex-col max-w-xs w-full pt-5 pb-4 bg-white">
        <div class="absolute top-0 right-0 -mr-12 pt-2">
          <button
            type="button"
            class="ml-1 flex items-center justify-center h-10 w-10 rounded-full focus:outline-none focus:ring-2 focus:ring-inset focus:ring-white"
            @click="$emit('close')"
          >
            <span class="sr-only">Close sidebar</span>
            <XMarkIcon class="h-6 w-6 text-white" />
          </button>
        </div>

        <SidebarContent />
      </div>

      <div class="flex-shrink-0 w-14"></div>
    </div>

    <!-- Desktop sidebar -->
    <div class="hidden md:flex md:w-64 md:flex-col md:fixed md:inset-y-0">
      <div class="flex flex-col flex-grow pt-5 bg-white overflow-y-auto border-r border-gray-200">
        <SidebarContent />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { XMarkIcon } from '@heroicons/vue/24/outline'
import SidebarContent from './SidebarContent.vue'

interface Props {
  open: boolean
}

defineProps<Props>()

defineEmits<{
  close: []
}>()
</script>

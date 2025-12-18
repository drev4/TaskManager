<template>
  <div class="bg-white shadow rounded-lg">
    <div class="px-4 py-5 sm:p-6">
      <h3 class="text-lg font-medium text-gray-900 mb-4">Recent Activity</h3>

      <div v-if="recentActivity.length > 0" class="flow-root">
        <ul role="list" class="-mb-8">
          <li
            v-for="(activity, index) in recentActivity"
            :key="activity.id"
          >
            <div class="relative pb-8">
              <span
                v-if="index !== recentActivity.length - 1"
                class="absolute top-4 left-4 -ml-px h-full w-0.5 bg-gray-200"
              ></span>
              <div class="relative flex space-x-3">
                <div>
                  <span :class="[
                    getActivityIconBackground(activity.type),
                    'h-8 w-8 rounded-full flex items-center justify-center ring-8 ring-white'
                  ]">
                    <component
                      :is="getActivityIcon(activity.type)"
                      class="h-5 w-5 text-white"
                    />
                  </span>
                </div>
                <div class="min-w-0 flex-1 pt-1.5 flex justify-between space-x-4">
                  <div>
                    <p class="text-sm text-gray-500">
                      {{ activity.description }}
                    </p>
                  </div>
                  <div class="text-right text-sm whitespace-nowrap text-gray-500">
                    {{ formatRelativeTime(activity.timestamp) }}
                  </div>
                </div>
              </div>
            </div>
          </li>
        </ul>
      </div>

      <div v-else class="text-center py-6">
        <ClockIcon class="mx-auto h-12 w-12 text-gray-400" />
        <h3 class="mt-2 text-sm font-medium text-gray-900">No recent activity</h3>
        <p class="mt-1 text-sm text-gray-500">
          Get started by creating your first project.
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { formatDistanceToNow } from 'date-fns'
import { ClockIcon } from '@heroicons/vue/24/outline'
import { getActivityIcon, getActivityIconBackground } from '@/composables/useActivityIcons'
import type { RecentActivity } from '@/types'

interface Props {
  recentActivity: RecentActivity[]
}

defineProps<Props>()

const formatRelativeTime = (date: Date) => {
  return formatDistanceToNow(new Date(date), { addSuffix: true })
}
</script>
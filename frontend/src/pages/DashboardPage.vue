<template>
  <div>
    <!-- Page header -->
    <div class="mb-8">
      <h1 class="text-2xl font-bold text-gray-900">Dashboard</h1>
      <p class="mt-1 text-sm text-gray-500">
        Welcome back! Here's what's happening with your projects.
      </p>
    </div>

    <!-- Stats cards -->
    <div class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4 mb-8">
      <div
        v-for="stat in stats"
        :key="stat.name"
        class="relative bg-white pt-5 px-4 pb-12 sm:pt-6 sm:px-6 shadow rounded-lg overflow-hidden"
      >
        <dt>
          <div :class="[stat.iconBackground, 'absolute rounded-md p-3']">
            <component :is="getIconComponent(stat.icon)" class="h-6 w-6 text-white" />
          </div>
          <p class="ml-16 text-sm font-medium text-gray-500 truncate">
            {{ stat.name }}
          </p>
        </dt>
        <dd class="ml-16 pb-6 flex items-baseline sm:pb-7">
          <p class="text-2xl font-semibold text-gray-900">
            {{ stat.value }}
          </p>
          <p
            v-if="stat.change"
            :class="[
              stat.changeType === 'increase' ? 'text-green-600' : 'text-red-600',
              'ml-2 flex items-baseline text-sm font-semibold'
            ]"
          >
            <component
              :is="stat.changeType === 'increase' ? ArrowUpIcon : ArrowDownIcon"
              class="self-center flex-shrink-0 h-5 w-5"
            />
            <span class="sr-only">
              {{ stat.changeType === 'increase' ? 'Increased' : 'Decreased' }} by
            </span>
            {{ stat.change }}
          </p>
        </dd>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <RecentActivity :recent-activity="recentActivity" />
      <QuickActions />
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import {
  ArrowUpIcon,
  ArrowDownIcon,
  FolderIcon,
  CheckCircleIcon,
  ExclamationTriangleIcon
} from '@heroicons/vue/24/outline'
import RecentActivity from '@/components/dashboard/RecentActivity.vue'
import QuickActions from '@/components/dashboard/QuickActions.vue'
import { useDashboardStats, useRecentActivity } from '@/composables/useDashboard'

const { stats, loadStats } = useDashboardStats()
const { recentActivity, loadActivity } = useRecentActivity()

const iconComponents = {
  FolderIcon,
  CheckCircleIcon,
  ExclamationTriangleIcon
} as const

const getIconComponent = (iconName: string) => {
  return iconComponents[iconName as keyof typeof iconComponents] || FolderIcon
}

const loadDashboardData = async () => {
  await Promise.allSettled([
    loadStats(),
    loadActivity()
  ])
}

onMounted(() => {
  loadDashboardData()
})
</script>

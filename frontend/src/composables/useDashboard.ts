import { ref, computed } from 'vue'
import { useDashboardApi, useApiCall } from '@/composables/useApi'
import type { DashboardStats, RecentActivity } from '@/types'

interface StatConfig {
  name: string
  key: keyof DashboardStats
  icon: string
  iconBackground: string
}

interface StatItem extends StatConfig {
  value: string
  change: string | null
  changeType: 'increase' | 'decrease' | null
}

export function useDashboardStats() {
  const dashboardApi = useDashboardApi()
  const { execute: executeStatsCall } = useApiCall<DashboardStats>()

  const statsConfig: StatConfig[] = [
    {
      name: 'Total Projects',
      key: 'totalProjects',
      icon: 'FolderIcon',
      iconBackground: 'bg-blue-500'
    },
    {
      name: 'Active Tasks',
      key: 'pendingTasks',
      icon: 'CheckCircleIcon',
      iconBackground: 'bg-green-500'
    },
    {
      name: 'Completed Tasks',
      key: 'completedTasks',
      icon: 'CheckCircleIcon',
      iconBackground: 'bg-indigo-500'
    },
    {
      name: 'Overdue Tasks',
      key: 'overdueTasks',
      icon: 'ExclamationTriangleIcon',
      iconBackground: 'bg-red-500'
    }
  ]

  const stats = ref<StatItem[]>(statsConfig.map(stat => ({
    ...stat,
    value: '0',
    change: null,
    changeType: null
  })))

  const loadStats = async () => {
    const statsData = await executeStatsCall(
      () => dashboardApi.getDashboardStats(),
      { showErrorNotification: false }
    )

    if (statsData) {
      stats.value.forEach((stat: StatItem) => {
        stat.value = statsData[stat.key]?.toString() || '0'
      })
    }
  }

  return {
    stats: computed(() => stats.value),
    loadStats
  }
}

export function useRecentActivity() {
  const dashboardApi = useDashboardApi()
  const { execute: executeActivityCall } = useApiCall<RecentActivity[]>()

  const recentActivity = ref<RecentActivity[]>([])

  const loadActivity = async () => {
    const activityData = await executeActivityCall(
      () => dashboardApi.getRecentActivity(),
      { showErrorNotification: false }
    )

    if (activityData) {
      recentActivity.value = activityData
    }
  }

  return {
    recentActivity: computed(() => recentActivity.value),
    loadActivity
  }
}
import {
  FolderIcon as FolderIconSolid,
  CheckCircleIcon as CheckCircleIconSolid,
  ClockIcon as ClockIconSolid,
  PlusIcon as PlusIconSolid
} from '@heroicons/vue/24/solid'

export const activityIcons = {
  project_created: FolderIconSolid,
  task_completed: CheckCircleIconSolid,
  task_created: PlusIconSolid,
  default: ClockIconSolid
} as const

export const activityIconBackgrounds = {
  project_created: 'bg-blue-500',
  task_completed: 'bg-green-500',
  task_created: 'bg-indigo-500',
  default: 'bg-gray-500'
} as const

export type ActivityType = keyof typeof activityIcons

export function getActivityIcon(type: string) {
  return activityIcons[type as ActivityType] || activityIcons.default
}

export function getActivityIconBackground(type: string) {
  return activityIconBackgrounds[type as ActivityType] || activityIconBackgrounds.default
}
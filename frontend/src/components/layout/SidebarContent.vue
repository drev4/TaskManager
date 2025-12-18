<template>
  <div class="flex flex-col h-full">
    <!-- Logo -->
    <div class="flex items-center flex-shrink-0 px-4">
      <div class="flex items-center">
        <div class="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center">
          <span class="text-white font-bold text-sm">TM</span>
        </div>
        <h1 class="ml-2 text-xl font-bold text-gray-900">TaskManager</h1>
      </div>
    </div>

    <!-- Navigation -->
    <div class="mt-5 flex-grow flex flex-col">
      <nav class="flex-1 px-2 space-y-1">
        <template v-for="item in navigation" :key="item.name">
          <RouterLink
            v-if="!item.children"
            :to="item.href"
            :class="[
              isCurrentRoute(item.href) 
                ? 'bg-blue-100 text-blue-900' 
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900',
              'group flex items-center px-2 py-2 text-sm font-medium rounded-md'
            ]"
          >
            <component
              :is="item.icon"
              :class="[
                isCurrentRoute(item.href) 
                  ? 'text-blue-500' 
                  : 'text-gray-400 group-hover:text-gray-500',
                'mr-3 flex-shrink-0 h-6 w-6'
              ]"
            />
            {{ item.name }}
          </RouterLink>

          <!-- Expandable section for children -->
          <div v-else>
            <button
              type="button"
              :class="[
                hasActiveChild(item) 
                  ? 'bg-gray-100 text-gray-900' 
                  : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900',
                'group w-full flex items-center px-2 py-2 text-left text-sm font-medium rounded-md'
              ]"
              @click="toggleSection(item.name)"
            >
              <component
                :is="item.icon"
                :class="[
                  hasActiveChild(item) 
                    ? 'text-gray-500' 
                    : 'text-gray-400 group-hover:text-gray-500',
                  'mr-3 flex-shrink-0 h-6 w-6'
                ]"
              />
              <span class="flex-1">{{ item.name }}</span>
              <ChevronRightIcon
                :class="[
                  expandedSections.includes(item.name) ? 'rotate-90' : '',
                  'ml-3 flex-shrink-0 h-5 w-5 text-gray-400 group-hover:text-gray-500 transform transition-transform'
                ]"
              />
            </button>

            <div 
              v-if="expandedSections.includes(item.name)"
              class="space-y-1"
            >
              <RouterLink
                v-for="subItem in item.children"
                :key="subItem.name"
                :to="subItem.href"
                :class="[
                  isCurrentRoute(subItem.href)
                    ? 'bg-blue-100 text-blue-900'
                    : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900',
                  'group flex items-center pl-11 pr-2 py-2 text-sm font-medium rounded-md'
                ]"
              >
                {{ subItem.name }}
              </RouterLink>
            </div>
          </div>
        </template>
      </nav>
    </div>

    <!-- User info -->
    <div class="flex-shrink-0 flex border-t border-gray-200 p-4">
      <div class="flex items-center">
        <div class="h-9 w-9 rounded-full bg-blue-600 flex items-center justify-center">
          <span class="text-white text-sm font-medium">
            {{ userInitials }}
          </span>
        </div>
        <div class="ml-3">
          <p class="text-sm font-medium text-gray-700 group-hover:text-gray-900">
            {{ authStore.currentUser?.displayName }}
          </p>
          <p class="text-xs font-medium text-gray-500 group-hover:text-gray-700">
            {{ authStore.currentUser?.email }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  HomeIcon,
  FolderIcon,
  CheckCircleIcon,
  ChartBarIcon,
  CogIcon,
  ChevronRightIcon
} from '@heroicons/vue/24/outline'

const route = useRoute()
const authStore = useAuthStore()

const expandedSections = ref<string[]>(['Projects'])

const navigation = [
  { 
    name: 'Dashboard', 
    href: '/dashboard', 
    icon: HomeIcon 
  },
  { 
    name: 'Projects', 
    href: '/projects', 
    icon: FolderIcon,
    children: [
      { name: 'All Projects', href: '/projects' },
      { name: 'Create New', href: '/projects/new' }
    ]
  },
  { 
    name: 'Tasks', 
    href: '/tasks', 
    icon: CheckCircleIcon 
  },
  { 
    name: 'Analytics', 
    href: '/analytics', 
    icon: ChartBarIcon 
  },
  { 
    name: 'Settings', 
    href: '/settings', 
    icon: CogIcon 
  }
]

const userInitials = computed(() => {
  const user = authStore.currentUser
  if (!user?.displayName) return 'U'
  
  const names = user.displayName.split(' ')
  if (names.length >= 2) {
    return `${names[0][0]}${names[1][0]}`.toUpperCase()
  }
  return user.displayName[0].toUpperCase()
})

const isCurrentRoute = (href: string) => {
  return route.path === href || route.path.startsWith(href + '/')
}

const hasActiveChild = (item: any) => {
  if (!item.children) return false
  return item.children.some((child: any) => isCurrentRoute(child.href))
}

const toggleSection = (sectionName: string) => {
  const index = expandedSections.value.indexOf(sectionName)
  if (index > -1) {
    expandedSections.value.splice(index, 1)
  } else {
    expandedSections.value.push(sectionName)
  }
}
</script>

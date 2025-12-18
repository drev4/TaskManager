<template>
  <nav class="flex" aria-label="Breadcrumb">
    <ol role="list" class="flex items-center space-x-4">
      <li>
        <div>
          <RouterLink to="/dashboard" class="text-gray-400 hover:text-gray-500">
            <HomeIcon class="flex-shrink-0 h-5 w-5" />
            <span class="sr-only">Home</span>
          </RouterLink>
        </div>
      </li>
      <li v-for="(page, index) in breadcrumbs" :key="page.name">
        <div class="flex items-center">
          <ChevronRightIcon class="flex-shrink-0 h-5 w-5 text-gray-400" />
          <RouterLink
            v-if="page.href && index < breadcrumbs.length - 1"
            :to="page.href"
            class="ml-4 text-sm font-medium text-gray-500 hover:text-gray-700"
          >
            {{ page.name }}
          </RouterLink>
          <span
            v-else
            class="ml-4 text-sm font-medium text-gray-900"
          >
            {{ page.name }}
          </span>
        </div>
      </li>
    </ol>
  </nav>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { HomeIcon, ChevronRightIcon } from '@heroicons/vue/24/solid'

const route = useRoute()

interface BreadcrumbItem {
  name: string
  href?: string
}

const breadcrumbs = computed<BreadcrumbItem[]>(() => {
  const pathSegments = route.path.split('/').filter(Boolean)
  const breadcrumbs: BreadcrumbItem[] = []
  
  // Build breadcrumbs based on route
  for (let i = 0; i < pathSegments.length; i++) {
    const segment = pathSegments[i]
    const href = '/' + pathSegments.slice(0, i + 1).join('/')
    
    // Customize breadcrumb names based on route
    let name = segment.charAt(0).toUpperCase() + segment.slice(1)
    
    switch (segment) {
      case 'dashboard':
        name = 'Dashboard'
        break
      case 'projects':
        name = 'Projects'
        break
      case 'tasks':
        name = 'Tasks'
        break
      case 'profile':
        name = 'Profile'
        break
      case 'settings':
        name = 'Settings'
        break
      case 'analytics':
        name = 'Analytics'
        break
      default:
        // If it's a UUID or ID, try to get a better name from route meta or params
        if (route.params.id === segment) {
          name = route.meta?.breadcrumbName as string || `Item ${segment.slice(0, 8)}...`
        }
    }
    
    breadcrumbs.push({
      name,
      href: i < pathSegments.length - 1 ? href : undefined
    })
  }
  
  return breadcrumbs
})
</script>

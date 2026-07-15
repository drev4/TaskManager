<template>
  <div class="h-screen flex overflow-hidden bg-gray-100">
    <!-- Sidebar -->
    <AppSidebar 
      :open="sidebarOpen" 
      @close="sidebarOpen = false" 
    />

    <!-- Main content -->
    <div class="flex flex-col w-0 flex-1 overflow-hidden md:pl-64">
      <!-- Top nav -->
      <AppHeader @toggle-sidebar="sidebarOpen = !sidebarOpen" />

      <!-- Page content -->
      <main class="flex-1 relative overflow-y-auto focus:outline-none">
        <div class="py-6">
          <div class="max-w-7xl mx-auto px-4 sm:px-6 md:px-8">
            <!-- Page header -->
            <AppBreadcrumb />
            
            <!-- Page content -->
            <div class="py-4">
              <RouterView />
            </div>
          </div>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import AppSidebar from '@/components/layout/AppSidebar.vue'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppBreadcrumb from '@/components/layout/AppBreadcrumb.vue'

const sidebarOpen = ref(false)
</script>

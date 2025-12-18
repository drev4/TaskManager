<template>
  <div class="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
    <div class="sm:mx-auto sm:w-full sm:max-w-md">
      <div class="flex justify-center">
        <div class="flex items-center">
          <!-- Logo placeholder -->
          <div class="w-12 h-12 bg-blue-600 rounded-lg flex items-center justify-center">
            <span class="text-white font-bold text-xl">TM</span>
          </div>
          <h1 class="ml-3 text-2xl font-bold text-gray-900">TaskManager</h1>
        </div>
      </div>
    </div>

    <div class="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
      <div class="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
        <RouterView />
      </div>
    </div>

    <div class="mt-8 text-center">
      <p class="text-sm text-gray-600">
        Built with Vue 3, TypeScript & Azure B2C
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
// Auth layout for login and other authentication pages
</script>

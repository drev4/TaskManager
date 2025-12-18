<template>
  <div id="app">
    <RouterView />
  </div>
</template>

<script setup lang="ts">
// TODO: Enable authentication when Azure AD B2C is configured
// import { onMounted } from 'vue'
// import { useAuthStore } from '@/stores/auth'

// const authStore = useAuthStore()

// onMounted(async () => {
//   // Initialize authentication when app starts
//   await authStore.initialize()
// })
</script>

<style>
/* Global styles are imported in main.ts */
</style>

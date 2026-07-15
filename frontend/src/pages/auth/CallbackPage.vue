<template>
  <div class="text-center">
    <div class="inline-flex items-center px-4 py-2 font-semibold leading-6 text-sm shadow rounded-md text-blue-500 bg-blue-100">
      <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
      </svg>
      Completing sign in...
    </div>
    
    <p class="mt-4 text-sm text-gray-600">
      Please wait while we complete your authentication.
    </p>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useUsersApi } from '@/composables/useApi'

const router = useRouter()
const authStore = useAuthStore()
const { initializeUser } = useUsersApi()

onMounted(async () => {
  try {
    // Handle the redirect callback
    const success = await authStore.handleRedirectCallback()

    if (success) {
      // Ensure an internal User row exists for this account before anything
      // (SignalR group membership, task ownership) depends on it.
      await initializeUser()

      // Redirect to dashboard on successful authentication
      router.push('/dashboard')
    } else {
      // Redirect back to login if authentication failed
      router.push('/auth/login')
    }
  } catch (error) {
    console.error('Callback error:', error)
    router.push('/auth/login')
  }
})
</script>

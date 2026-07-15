<template>
  <div id="app">
    <ErrorBoundary @error="handleGlobalError">
      <RouterView />
    </ErrorBoundary>
  </div>
</template>

<script setup lang="ts">
import { onMounted, watch } from 'vue'
import { useSignalR } from './composables/useSignalR'
import { useNotification } from './composables/useNotification'
import { useAuthStore } from './stores/auth'
import { useNotificationsStore } from './stores/notifications'
import { useUsersApi } from './composables/useApi'
import { logger } from './services/logger'
import { config } from './services/config'
import ErrorBoundary from './components/ErrorBoundary.vue'

const { startConnection, stopConnection, on, isConnected } = useSignalR()
const { showNotification } = useNotification()
const authStore = useAuthStore()
const notificationsStore = useNotificationsStore()
const { initializeUser } = useUsersApi()

// Ensures the internal User row exists for this account before anything that
// depends on it (task ownership, SignalR group membership) runs. Idempotent on
// the backend, and needed here (not just in CallbackPage.vue) because MSAL can
// resolve an authenticated session purely from cached SSO state without ever
// navigating through /auth/callback again.
const ensureUserInitialized = async () => {
  try {
    await initializeUser()
  } catch (error) {
    logger.error('Failed to initialize user profile:', error)
  }
}

const handleGlobalError = (error: Error, errorInfo: any) => {
  logger.error('Global application error:', {
    error: error.message,
    stack: error.stack,
    component: errorInfo.info
  })

  showNotification({
    title: 'Application Error',
    message: 'Something went wrong. Please try refreshing the page.',
    type: 'error'
  })
}

const connectRealtime = async () => {
  if (!config.get('features').realTimeUpdates) return

  const tokenProvider = config.isAuthEnabled()
    ? () => authStore.getAccessToken()
    : undefined

  await startConnection(tokenProvider)

  on('TaskCreated', (task) => {
    logger.info('New Task Created via SignalR:', { task })

    const title = 'New Task Created'
    const message = `Task "${task.title}" has been created.`

    showNotification({ title, message, type: 'success' })
    notificationsStore.add({ title, message, type: 'success' })
  })

  logger.info('SignalR event handlers registered')
}

onMounted(async () => {
  try {
    logger.info('Application starting...')

    if (!config.isAuthEnabled()) {
      // Dev mode: connect immediately, no token needed (mock user on the API side)
      await connectRealtime()
      return
    }

    if (authStore.isAuthenticated) {
      await ensureUserInitialized()
      await connectRealtime()
    }

    // Reactively (re)connect on login and disconnect on logout
    watch(() => authStore.isAuthenticated, async (isAuth) => {
      if (isAuth) {
        await ensureUserInitialized()
        await connectRealtime()
      } else {
        await stopConnection()
      }
    })
  } catch (error) {
    logger.error('Failed to initialize application:', error)

    showNotification({
      title: 'Initialization Error',
      message: 'Failed to initialize some features. The application will continue with limited functionality.',
      type: 'warning'
    })
  }
})
</script>

<style>
/* Global styles are imported in main.ts */
</style>

<template>
  <div id="app">
    <ErrorBoundary @error="handleGlobalError">
      <RouterView />
    </ErrorBoundary>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useSignalR } from './composables/useSignalR'
import { useNotification } from './composables/useNotification'
import { logger } from './services/logger'
import { config } from './services/config'
import ErrorBoundary from './components/ErrorBoundary.vue'

const { startConnection, on, isConnected } = useSignalR()
const { showNotification } = useNotification()

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

onMounted(async () => {
  try {
    logger.info('Application starting...')
    
    if (config.get('features').realTimeUpdates) {
      // Start SignalR connection
      await startConnection()
      
      // Listen for events
      on('TaskCreated', (task) => {
        logger.info('New Task Created via SignalR:', { task })
        
        showNotification({
          title: 'New Task Created',
          message: `Task "${task.title}" has been created.`,
          type: 'success'
        })
      })
      
      logger.info('SignalR event handlers registered')
    }
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

<template>
  <div v-if="hasError" class="error-boundary">
    <div class="error-boundary-content">
      <div class="error-icon">
        <svg class="w-16 h-16 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
        </svg>
      </div>
      
      <div class="error-details">
        <h2 class="error-title">Something went wrong</h2>
        <p class="error-message">
          {{ errorMessage || 'An unexpected error occurred while rendering this component.' }}
        </p>
        
        <div v-if="isDevelopment && errorDetails" class="error-technical">
          <details class="error-details-toggle">
            <summary>Technical Details</summary>
            <pre class="error-stack">{{ errorDetails }}</pre>
          </details>
        </div>
      </div>
      
      <div class="error-actions">
        <button @click="retry" class="btn btn-primary">
          Try Again
        </button>
        <button @click="reportError" class="btn btn-secondary">
          Report Issue
        </button>
      </div>
    </div>
  </div>
  
  <slot v-else />
</template>

<script setup lang="ts">
import { ref, computed, onErrorCaptured, onMounted } from 'vue'
import { logger } from '@/services/logger'
import { config } from '@/services/config'
import { useNotification } from '@/composables/useNotification'

interface Props {
  fallback?: string
  showRetry?: boolean
  onError?: (error: Error, errorInfo: any) => void
}

const props = withDefaults(defineProps<Props>(), {
  fallback: '',
  showRetry: true
})

const emit = defineEmits<{
  error: [error: Error, errorInfo: any]
  retry: []
}>()

const hasError = ref(false)
const error = ref<Error | null>(null)
const errorInfo = ref<any>(null)

const { showNotification } = useNotification()
const isDevelopment = computed(() => config.isDevelopment())

const errorMessage = computed(() => {
  if (props.fallback) return props.fallback
  if (error.value?.message) return error.value.message
  return null
})

const errorDetails = computed(() => {
  if (!error.value || !isDevelopment.value) return null
  
  return {
    message: error.value.message,
    stack: error.value.stack,
    component: errorInfo.value,
    timestamp: new Date().toISOString()
  }
})

const retry = () => {
  logger.info('User requested retry after error')
  
  hasError.value = false
  error.value = null
  errorInfo.value = null
  
  emit('retry')
  showNotification({
    title: 'Retrying',
    message: 'Attempting to recover from error...',
    type: 'info',
    timeout: 2000
  })
}

const reportError = () => {
  if (!error.value) return
  
  logger.error('User reported error:', {
    error: error.value.message,
    stack: error.value.stack,
    component: errorInfo.value
  })
  
  showNotification({
    title: 'Error Reported',
    message: 'Thank you for helping us improve the application.',
    type: 'success'
  })
}

onErrorCaptured((err: Error, instance, info) => {
  logger.error('ErrorBoundary caught an error:', {
    error: err.message,
    stack: err.stack,
    component: info,
    instance: instance?.$options?.name || 'Unknown'
  })
  
  hasError.value = true
  error.value = err
  errorInfo.value = { info, instance: instance?.$options?.name }
  
  emit('error', err, { info, instance })
  
  // Call custom error handler if provided
  if (props.onError) {
    props.onError(err, { info, instance })
  }
  
  // Prevent error from propagating to parent
  return false
})

onMounted(() => {
  logger.debug('ErrorBoundary mounted')
})
</script>

<style scoped>
.error-boundary {
  @apply flex items-center justify-center min-h-[400px] p-4 bg-red-50 border border-red-200 rounded-lg;
}

.error-boundary-content {
  @apply max-w-md text-center;
}

.error-icon {
  @apply flex justify-center mb-4;
}

.error-title {
  @apply text-xl font-semibold text-red-800 mb-2;
}

.error-message {
  @apply text-red-600 mb-4;
}

.error-technical {
  @apply mb-4 text-left;
}

.error-details-toggle {
  @apply cursor-pointer;
}

.error-details-toggle summary {
  @apply text-sm text-red-500 hover:text-red-700 mb-2;
}

.error-stack {
  @apply text-xs bg-red-100 p-2 rounded border border-red-200 overflow-auto max-h-40;
  white-space: pre-wrap;
  word-break: break-word;
}

.error-actions {
  @apply flex gap-2 justify-center;
}

.btn {
  @apply px-4 py-2 rounded text-sm font-medium transition-colors;
}

.btn-primary {
  @apply bg-red-600 text-white hover:bg-red-700;
}

.btn-secondary {
  @apply bg-gray-200 text-gray-700 hover:bg-gray-300;
}
</style>
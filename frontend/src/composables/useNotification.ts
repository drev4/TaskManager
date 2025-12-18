import { useToast } from 'vue-toastification'
import type { NotificationOptions } from '@/types'

export function useNotification() {
  const toast = useToast()
  
  const showNotification = (options: NotificationOptions) => {
    const { title, message, type, duration = 5000 } = options
    
    const content = title ? `${title}: ${message}` : message
    
    switch (type) {
      case 'success':
        toast.success(content, { timeout: duration })
        break
      case 'error':
        toast.error(content, { timeout: duration })
        break
      case 'warning':
        toast.warning(content, { timeout: duration })
        break
      case 'info':
        toast.info(content, { timeout: duration })
        break
      default:
        toast(content, { timeout: duration })
    }
  }
  
  const showSuccess = (message: string, title?: string) => {
    showNotification({ title: title || 'Success', message, type: 'success' })
  }
  
  const showError = (message: string, title?: string) => {
    showNotification({ title: title || 'Error', message, type: 'error' })
  }
  
  const showWarning = (message: string, title?: string) => {
    showNotification({ title: title || 'Warning', message, type: 'warning' })
  }
  
  const showInfo = (message: string, title?: string) => {
    showNotification({ title: title || 'Info', message, type: 'info' })
  }
  
  return {
    showNotification,
    showSuccess,
    showError,
    showWarning,
    showInfo
  }
}

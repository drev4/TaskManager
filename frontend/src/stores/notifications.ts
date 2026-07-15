import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface AppNotification {
  id: string
  title: string
  message: string
  type: 'success' | 'error' | 'warning' | 'info'
  timestamp: Date
  read: boolean
}

const MAX_NOTIFICATIONS = 50

export const useNotificationsStore = defineStore('notifications', () => {
  const items = ref<AppNotification[]>([])

  const unreadCount = computed(() => items.value.filter(n => !n.read).length)

  const add = (notification: Pick<AppNotification, 'title' | 'message' | 'type'>) => {
    items.value.unshift({
      ...notification,
      id: crypto.randomUUID(),
      timestamp: new Date(),
      read: false
    })

    if (items.value.length > MAX_NOTIFICATIONS) {
      items.value = items.value.slice(0, MAX_NOTIFICATIONS)
    }
  }

  const markAsRead = (id: string) => {
    const item = items.value.find(n => n.id === id)
    if (item) item.read = true
  }

  const markAllAsRead = () => {
    items.value.forEach(n => { n.read = true })
  }

  const clear = () => {
    items.value = []
  }

  return {
    items,
    unreadCount,
    add,
    markAsRead,
    markAllAsRead,
    clear
  }
})

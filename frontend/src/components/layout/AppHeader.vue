<template>
  <div class="relative z-10 flex-shrink-0 flex h-16 bg-white shadow">
    <!-- Mobile menu button -->
    <button
      type="button"
      class="px-4 border-r border-gray-200 text-gray-500 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-blue-500 md:hidden"
      @click="$emit('toggle-sidebar')"
    >
      <span class="sr-only">Open sidebar</span>
      <Bars3Icon class="h-6 w-6" />
    </button>

    <div class="flex-1 px-4 flex justify-between">
      <!-- Search -->
      <div class="flex-1 flex">
        <div class="w-full flex md:ml-0">
          <label for="search-field" class="sr-only">Search</label>
          <div class="relative w-full text-gray-400 focus-within:text-gray-600">
            <div class="absolute inset-y-0 left-0 flex items-center pointer-events-none">
              <MagnifyingGlassIcon class="h-5 w-5" />
            </div>
            <input
              id="search-field"
              class="block w-full h-full pl-8 pr-3 py-2 border-transparent text-gray-900 placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-0 focus:border-transparent sm:text-sm"
              placeholder="Search tasks, projects..."
              type="search"
              name="search"
            />
          </div>
        </div>
      </div>

      <!-- Right side -->
      <div class="ml-4 flex items-center md:ml-6">
        <!-- Notifications -->
        <Popover as="div" class="relative">
          <PopoverButton
            class="relative bg-white p-1 rounded-full text-gray-400 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            @click="notificationsStore.markAllAsRead()"
          >
            <span class="sr-only">View notifications</span>
            <BellIcon class="h-6 w-6" />
            <span
              v-if="notificationsStore.unreadCount > 0"
              class="absolute top-0 right-0 block h-2 w-2 rounded-full bg-red-500 ring-2 ring-white"
            />
          </PopoverButton>

          <transition
            enter-active-class="transition ease-out duration-100"
            enter-from-class="transform opacity-0 scale-95"
            enter-to-class="transform opacity-100 scale-100"
            leave-active-class="transition ease-in duration-75"
            leave-from-class="transform opacity-100 scale-100"
            leave-to-class="transform opacity-0 scale-95"
          >
            <PopoverPanel class="origin-top-right absolute right-0 mt-2 w-80 rounded-md shadow-lg bg-white ring-1 ring-black ring-opacity-5 focus:outline-none">
              <div class="flex items-center justify-between px-4 py-2 border-b border-gray-200">
                <h3 class="text-sm font-medium text-gray-900">Notifications</h3>
                <button
                  v-if="notificationsStore.items.length > 0"
                  type="button"
                  class="text-xs text-gray-500 hover:text-gray-700"
                  @click="notificationsStore.clear()"
                >
                  Clear all
                </button>
              </div>

              <div class="max-h-80 overflow-y-auto">
                <p v-if="notificationsStore.items.length === 0" class="px-4 py-6 text-sm text-gray-500 text-center">
                  No notifications yet
                </p>
                <div
                  v-for="notification in notificationsStore.items"
                  :key="notification.id"
                  class="px-4 py-3 border-b border-gray-100 last:border-b-0"
                >
                  <p class="text-sm font-medium text-gray-900">{{ notification.title }}</p>
                  <p class="text-sm text-gray-500">{{ notification.message }}</p>
                  <p class="text-xs text-gray-400 mt-1">{{ formatTimestamp(notification.timestamp) }}</p>
                </div>
              </div>
            </PopoverPanel>
          </transition>
        </Popover>

        <!-- Profile dropdown -->
        <Menu as="div" class="ml-3 relative">
          <div>
            <MenuButton class="max-w-xs bg-white flex items-center text-sm rounded-full focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
              <span class="sr-only">Open user menu</span>
              <div class="h-8 w-8 rounded-full bg-blue-600 flex items-center justify-center">
                <span class="text-white text-sm font-medium">
                  {{ userInitials }}
                </span>
              </div>
            </MenuButton>
          </div>
          <transition
            enter-active-class="transition ease-out duration-100"
            enter-from-class="transform opacity-0 scale-95"
            enter-to-class="transform opacity-100 scale-100"
            leave-active-class="transition ease-in duration-75"
            leave-from-class="transform opacity-100 scale-100"
            leave-to-class="transform opacity-0 scale-95"
          >
            <MenuItems class="origin-top-right absolute right-0 mt-2 w-48 rounded-md shadow-lg py-1 bg-white ring-1 ring-black ring-opacity-5 focus:outline-none">
              <MenuItem v-slot="{ active }">
                <RouterLink
                  to="/profile"
                  :class="[
                    active ? 'bg-gray-100' : '',
                    'block px-4 py-2 text-sm text-gray-700'
                  ]"
                >
                  Your Profile
                </RouterLink>
              </MenuItem>
              <MenuItem v-slot="{ active }">
                <button
                  :class="[
                    active ? 'bg-gray-100' : '',
                    'block w-full text-left px-4 py-2 text-sm text-gray-700'
                  ]"
                  @click="handleSignOut"
                >
                  Sign out
                </button>
              </MenuItem>
            </MenuItems>
          </transition>
        </Menu>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { format } from 'date-fns'
import { Menu, MenuButton, MenuItem, MenuItems, Popover, PopoverButton, PopoverPanel } from '@headlessui/vue'
import {
  Bars3Icon,
  BellIcon,
  MagnifyingGlassIcon
} from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'
import { useNotificationsStore } from '@/stores/notifications'

defineEmits<{
  'toggle-sidebar': []
}>()

const authStore = useAuthStore()
const notificationsStore = useNotificationsStore()

const formatTimestamp = (date: Date) => format(new Date(date), 'MMM d, HH:mm')

const userInitials = computed(() => {
  const user = authStore.currentUser
  if (!user?.displayName) return 'U'
  
  const names = user.displayName.split(' ')
  if (names.length >= 2) {
    return `${names[0][0]}${names[1][0]}`.toUpperCase()
  }
  return user.displayName[0].toUpperCase()
})

const handleSignOut = () => {
  authStore.logout()
}
</script>

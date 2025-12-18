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
        <button
          type="button"
          class="bg-white p-1 rounded-full text-gray-400 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          <span class="sr-only">View notifications</span>
          <BellIcon class="h-6 w-6" />
        </button>

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
import { Menu, MenuButton, MenuItem, MenuItems } from '@headlessui/vue'
import { 
  Bars3Icon, 
  BellIcon, 
  MagnifyingGlassIcon 
} from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'

defineEmits<{
  'toggle-sidebar': []
}>()

const authStore = useAuthStore()

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

<template>
  <div>
    <!-- Page header -->
    <div class="mb-8">
      <h1 class="text-2xl font-bold text-gray-900">Profile</h1>
      <p class="mt-1 text-sm text-gray-500">
        Manage your account settings and preferences.
      </p>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Profile Card -->
      <div class="lg:col-span-1">
        <div class="bg-white shadow rounded-lg">
          <div class="px-6 py-4">
            <div class="flex items-center">
              <div class="h-20 w-20 rounded-full bg-blue-600 flex items-center justify-center">
                <span class="text-white text-2xl font-bold">
                  {{ userInitials }}
                </span>
              </div>
              <div class="ml-4">
                <h3 class="text-lg font-medium text-gray-900">
                  {{ authStore.currentUser?.displayName }}
                </h3>
                <p class="text-sm text-gray-500">
                  {{ authStore.currentUser?.email }}
                </p>
              </div>
            </div>
          </div>
          
          <div class="border-t border-gray-200 px-6 py-4">
            <dl class="grid grid-cols-1 gap-x-4 gap-y-4">
              <div>
                <dt class="text-sm font-medium text-gray-500">Member since</dt>
                <dd class="text-sm text-gray-900">January 2024</dd>
              </div>
              <div>
                <dt class="text-sm font-medium text-gray-500">Projects</dt>
                <dd class="text-sm text-gray-900">{{ stats.totalProjects }}</dd>
              </div>
              <div>
                <dt class="text-sm font-medium text-gray-500">Completed tasks</dt>
                <dd class="text-sm text-gray-900">{{ stats.completedTasks }}</dd>
              </div>
            </dl>
          </div>
        </div>
      </div>

      <!-- Settings -->
      <div class="lg:col-span-2 space-y-6">
        <!-- Account Information -->
        <div class="bg-white shadow rounded-lg">
          <div class="px-6 py-4 border-b border-gray-200">
            <h3 class="text-lg font-medium text-gray-900">Account Information</h3>
            <p class="mt-1 text-sm text-gray-500">
              Your account is managed through Microsoft Azure AD B2C.
            </p>
          </div>
          
          <div class="px-6 py-4 space-y-4">
            <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div>
                <label class="block text-sm font-medium text-gray-700">Display Name</label>
                <div class="mt-1 text-sm text-gray-900 bg-gray-50 rounded-md px-3 py-2">
                  {{ authStore.currentUser?.displayName }}
                </div>
              </div>
              
              <div>
                <label class="block text-sm font-medium text-gray-700">Email Address</label>
                <div class="mt-1 text-sm text-gray-900 bg-gray-50 rounded-md px-3 py-2">
                  {{ authStore.currentUser?.email }}
                </div>
              </div>
            </div>
            
            <div class="bg-blue-50 rounded-md p-4">
              <div class="flex">
                <div class="flex-shrink-0">
                  <InformationCircleIcon class="h-5 w-5 text-blue-400" />
                </div>
                <div class="ml-3">
                  <h3 class="text-sm font-medium text-blue-800">
                    Account Settings
                  </h3>
                  <div class="mt-2 text-sm text-blue-700">
                    <p>
                      To update your personal information, please visit your Microsoft account settings.
                      Changes made there will be reflected here after signing in again.
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Preferences -->
        <div class="bg-white shadow rounded-lg">
          <div class="px-6 py-4 border-b border-gray-200">
            <h3 class="text-lg font-medium text-gray-900">Preferences</h3>
            <p class="mt-1 text-sm text-gray-500">
              Customize your TaskManager experience.
            </p>
          </div>
          
          <div class="px-6 py-4 space-y-4">
            <div class="flex items-center justify-between">
              <div>
                <label class="text-sm font-medium text-gray-700">
                  Email Notifications
                </label>
                <p class="text-sm text-gray-500">
                  Receive email updates about your tasks and projects.
                </p>
              </div>
              <button
                type="button"
                :class="[
                  preferences.emailNotifications ? 'bg-blue-600' : 'bg-gray-200',
                  'relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2'
                ]"
                @click="preferences.emailNotifications = !preferences.emailNotifications"
              >
                <span
                  :class="[
                    preferences.emailNotifications ? 'translate-x-5' : 'translate-x-0',
                    'pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out'
                  ]"
                />
              </button>
            </div>
            
            <div class="flex items-center justify-between">
              <div>
                <label class="text-sm font-medium text-gray-700">
                  Desktop Notifications
                </label>
                <p class="text-sm text-gray-500">
                  Show browser notifications for important updates.
                </p>
              </div>
              <button
                type="button"
                :class="[
                  preferences.desktopNotifications ? 'bg-blue-600' : 'bg-gray-200',
                  'relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2'
                ]"
                @click="preferences.desktopNotifications = !preferences.desktopNotifications"
              >
                <span
                  :class="[
                    preferences.desktopNotifications ? 'translate-x-5' : 'translate-x-0',
                    'pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out'
                  ]"
                />
              </button>
            </div>
            
            <div class="pt-4 border-t border-gray-200">
              <BaseButton
                variant="primary"
                @click="savePreferences"
              >
                Save Preferences
              </BaseButton>
            </div>
          </div>
        </div>

        <!-- Danger Zone -->
        <div class="bg-white shadow rounded-lg">
          <div class="px-6 py-4 border-b border-gray-200">
            <h3 class="text-lg font-medium text-gray-900">Account Actions</h3>
          </div>
          
          <div class="px-6 py-4">
            <BaseButton
              variant="danger"
              @click="authStore.logout()"
            >
              Sign Out
            </BaseButton>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { InformationCircleIcon } from '@heroicons/vue/24/solid'
import BaseButton from '@/components/ui/BaseButton.vue'
import { useAuthStore } from '@/stores/auth'
import { useNotification } from '@/composables/useNotification'

const authStore = useAuthStore()
const { showSuccess } = useNotification()

const stats = ref({
  totalProjects: 0,
  completedTasks: 0
})

const preferences = ref({
  emailNotifications: true,
  desktopNotifications: false
})

const userInitials = computed(() => {
  const user = authStore.currentUser
  if (!user?.displayName) return 'U'
  
  const names = user.displayName.split(' ')
  if (names.length >= 2) {
    return `${names[0][0]}${names[1][0]}`.toUpperCase()
  }
  return user.displayName[0].toUpperCase()
})

const savePreferences = () => {
  // Here you would save preferences to the API
  showSuccess('Preferences saved successfully!')
}

const loadUserStats = () => {
  // Mock data - replace with API call
  stats.value = {
    totalProjects: 3,
    completedTasks: 27
  }
}

onMounted(() => {
  loadUserStats()
})
</script>

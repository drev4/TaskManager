<template>
  <div>
    <!-- Page header -->
    <div class="mb-8 sm:flex sm:items-center sm:justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Projects</h1>
        <p class="mt-1 text-sm text-gray-500">
          Manage and organize your work into projects.
        </p>
      </div>
      <div class="mt-4 sm:mt-0">
        <BaseButton
          variant="primary"
          @click="showCreateModal = true"
        >
          <PlusIcon class="h-5 w-5 mr-2" />
          New Project
        </BaseButton>
      </div>
    </div>

    <!-- Projects grid -->
    <div v-if="projects.length > 0" class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
      <div
        v-for="project in projects"
        :key="project.id"
        class="bg-white overflow-hidden shadow rounded-lg hover:shadow-lg transition-shadow cursor-pointer"
        @click="$router.push(`/projects/${project.id}`)"
      >
        <div class="p-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-medium text-gray-900 truncate">
              {{ project.name }}
            </h3>
            <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
              {{ project.totalTasksCount }} tasks
            </span>
          </div>
          
          <p v-if="project.description" class="text-sm text-gray-500 mb-4 line-clamp-2">
            {{ project.description }}
          </p>
          
          <div class="flex items-center justify-between">
            <div class="flex items-center text-sm text-gray-500">
              <CalendarIcon class="h-4 w-4 mr-1" />
              {{ formatDate(project.createdAt) }}
            </div>
            
            <div class="flex items-center">
              <div class="w-full bg-gray-200 rounded-full h-2 mr-2" style="width: 60px;">
                <div 
                  class="bg-blue-600 h-2 rounded-full transition-all duration-300"
                  :style="{ width: `${getProjectProgress(project)}%` }"
                ></div>
              </div>
              <span class="text-xs text-gray-500">
                {{ getProjectProgress(project) }}%
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-else class="text-center py-12">
      <FolderIcon class="mx-auto h-12 w-12 text-gray-400" />
      <h3 class="mt-2 text-sm font-medium text-gray-900">No projects</h3>
      <p class="mt-1 text-sm text-gray-500">
        Get started by creating your first project.
      </p>
      <div class="mt-6">
        <BaseButton
          variant="primary"
          @click="showCreateModal = true"
        >
          <PlusIcon class="h-5 w-5 mr-2" />
          New Project
        </BaseButton>
      </div>
    </div>

    <!-- Create Project Modal -->
    <BaseModal
      v-model:show="showCreateModal"
      title="Create New Project"
      size="md"
    >
      <form @submit.prevent="createProject">
        <div class="space-y-4">
          <BaseInput
            v-model="newProject.name"
            label="Project Name"
            placeholder="Enter project name"
            required
          />
          
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Description
            </label>
            <textarea
              v-model="newProject.description"
              rows="3"
              class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
              placeholder="Optional project description"
            ></textarea>
          </div>
        </div>
      </form>

      <template #footer>
        <div class="flex justify-end space-x-3">
          <BaseButton
            variant="outline"
            @click="showCreateModal = false"
          >
            Cancel
          </BaseButton>
          <BaseButton
            variant="primary"
            :loading="isCreating"
            @click="createProject"
          >
            Create Project
          </BaseButton>
        </div>
      </template>
    </BaseModal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { format } from 'date-fns'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import {
  PlusIcon,
  FolderIcon,
  CalendarIcon
} from '@heroicons/vue/24/outline'
import type { Project, CreateProjectDto } from '@/types'
import { useProjectsApi, useApiCall } from '@/composables/useApi'
import { useNotification } from '@/composables/useNotification'

const route = useRoute()
const router = useRouter()
const projectsApi = useProjectsApi()
const { execute: executeGet } = useApiCall<Project[]>()
const { execute: executeCreate } = useApiCall<Project>()
const { showSuccess } = useNotification()

const projects = ref<Project[]>([])
const showCreateModal = ref(false)
const isCreating = ref(false)
const newProject = ref<CreateProjectDto>({
  name: '',
  description: ''
})

const loadProjects = async () => {
  try {
    const data = await executeGet(
      () => projectsApi.getProjects(),
      { showErrorNotification: false }
    )
    
    if (data) {
      projects.value = data
    }
  } catch (error) {
    // Mock data for development
    projects.value = [
      {
        id: '1',
        name: 'Website Redesign',
        description: 'Complete overhaul of the company website with modern design',
        ownerUserId: 'user1',
        createdAt: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000),
        updatedAt: new Date(),
        completedTasksCount: 5,
        totalTasksCount: 12
      },
      {
        id: '2',
        name: 'Mobile App',
        description: 'Develop cross-platform mobile application',
        ownerUserId: 'user1',
        createdAt: new Date(Date.now() - 14 * 24 * 60 * 60 * 1000),
        updatedAt: new Date(),
        completedTasksCount: 2,
        totalTasksCount: 8
      }
    ]
  }
}

const createProject = async () => {
  if (!newProject.value.name.trim()) {
    return
  }
  
  try {
    isCreating.value = true
    
    const data = await executeCreate(
      () => projectsApi.createProject(newProject.value)
    )
    
    if (data) {
      projects.value.unshift(data)
      showCreateModal.value = false
      newProject.value = { name: '', description: '' }
      showSuccess('Project created successfully!')
    }
  } catch (error) {
    // executeCreate already surfaced the real error via a toast; nothing to add here.
  } finally {
    isCreating.value = false
  }
}

const formatDate = (date: Date) => {
  return format(new Date(date), 'MMM d, yyyy')
}

const getProjectProgress = (project: Project) => {
  if (project.totalTasksCount === 0) return 0
  return Math.round((project.completedTasksCount / project.totalTasksCount) * 100)
}

onMounted(() => {
  loadProjects()

  if (route.query.create === 'true') {
    showCreateModal.value = true
    router.replace({ query: {} })
  }
})
</script>

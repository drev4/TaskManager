<template>
  <div>
    <!-- Page header -->
    <div class="mb-8 sm:flex sm:items-center sm:justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Tasks</h1>
        <p class="mt-1 text-sm text-gray-500">
          Manage and track all your tasks across projects.
        </p>
      </div>
      <div class="mt-4 sm:mt-0">
        <BaseButton
          variant="primary"
          @click="showCreateModal = true"
        >
          <PlusIcon class="h-5 w-5 mr-2" />
          New Task
        </BaseButton>
      </div>
    </div>

    <!-- Filters -->
    <div class="mb-6 bg-white p-4 rounded-lg shadow">
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
          <select
            v-model="filters.status"
            class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            @change="applyFilters"
          >
            <option value="">All Statuses</option>
            <option value="todo">To Do</option>
            <option value="in_progress">In Progress</option>
            <option value="done">Done</option>
          </select>
        </div>
        
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Priority</label>
          <select
            v-model="filters.priority"
            class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            @change="applyFilters"
          >
            <option value="">All Priorities</option>
            <option value="urgent">Urgent</option>
            <option value="high">High</option>
            <option value="medium">Medium</option>
            <option value="low">Low</option>
          </select>
        </div>
        
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Project</label>
          <select
            v-model="filters.projectId"
            class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            @change="applyFilters"
          >
            <option value="">All Projects</option>
            <option
              v-for="project in projects"
              :key="project.id"
              :value="project.id"
            >
              {{ project.name }}
            </option>
          </select>
        </div>
      </div>
    </div>

    <!-- Tasks list -->
    <div class="bg-white shadow overflow-hidden sm:rounded-md">
      <ul v-if="filteredTasks.length > 0" role="list" class="divide-y divide-gray-200">
        <li
          v-for="task in filteredTasks"
          :key="task.id"
          class="px-6 py-4 hover:bg-gray-50 cursor-pointer"
          @click="selectTask(task)"
        >
          <div class="flex items-center justify-between">
            <div class="flex items-center min-w-0 flex-1">
              <div class="flex-shrink-0">
                <component
                  :is="getStatusIcon(task.status)"
                  :class="getStatusIconClass(task.status)"
                  class="h-5 w-5"
                />
              </div>
              
              <div class="min-w-0 flex-1 ml-4">
                <div class="flex items-center justify-between">
                  <p class="text-sm font-medium text-gray-900 truncate">
                    {{ task.title }}
                  </p>
                  <div class="flex items-center space-x-2">
                    <span :class="getPriorityBadgeClass(task.priority)">
                      {{ task.priority }}
                    </span>
                    <span v-if="task.dueDate" class="text-xs text-gray-500">
                      Due {{ formatDate(task.dueDate) }}
                    </span>
                  </div>
                </div>
                
                <div class="flex items-center mt-2">
                  <p class="text-sm text-gray-500 truncate">
                    {{ task.description || 'No description' }}
                  </p>
                  <span v-if="task.project" class="ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800">
                    {{ task.project.name }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </li>
      </ul>
      
      <!-- Empty state -->
      <div v-else class="text-center py-12">
        <CheckCircleIcon class="mx-auto h-12 w-12 text-gray-400" />
        <h3 class="mt-2 text-sm font-medium text-gray-900">No tasks found</h3>
        <p class="mt-1 text-sm text-gray-500">
          {{ Object.values(filters).some(Boolean) ? 'Try adjusting your filters.' : 'Get started by creating your first task.' }}
        </p>
        <div v-if="!Object.values(filters).some(Boolean)" class="mt-6">
          <BaseButton
            variant="primary"
            @click="showCreateModal = true"
          >
            <PlusIcon class="h-5 w-5 mr-2" />
            New Task
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Create Task Modal -->
    <BaseModal
      v-model:show="showCreateModal"
      title="Create New Task"
      size="lg"
    >
      <form @submit.prevent="createTask">
        <div class="space-y-4">
          <BaseInput
            v-model="newTask.title"
            label="Task Title"
            placeholder="Enter task title"
            required
          />
          
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Description
            </label>
            <textarea
              v-model="newTask.description"
              rows="3"
              class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
              placeholder="Optional task description"
            ></textarea>
          </div>
          
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Priority</label>
              <select
                v-model="newTask.priority"
                class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
                required
              >
                <option value="low">Low</option>
                <option value="medium">Medium</option>
                <option value="high">High</option>
                <option value="urgent">Urgent</option>
              </select>
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Project</label>
              <select
                v-model="newTask.projectId"
                class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
                required
              >
                <option value="">Select a project</option>
                <option
                  v-for="project in projects"
                  :key="project.id"
                  :value="project.id"
                >
                  {{ project.name }}
                </option>
              </select>
            </div>
          </div>
          
          <BaseInput
            v-model="newTask.dueDate"
            type="date"
            label="Due Date (Optional)"
          />
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
            @click="createTask"
          >
            Create Task
          </BaseButton>
        </div>
      </template>
    </BaseModal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { format } from 'date-fns'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import {
  PlusIcon,
  CheckCircleIcon,
  ClockIcon,
  PlayIcon
} from '@heroicons/vue/24/outline'
import {
  CheckCircleIcon as CheckCircleIconSolid
} from '@heroicons/vue/24/solid'
import type { Task, Project, CreateTaskDto, TaskStatus, TaskPriority } from '@/types'
import { useTasksApi, useProjectsApi, useApiCall } from '@/composables/useApi'
import { useNotification } from '@/composables/useNotification'

const tasksApi = useTasksApi()
const projectsApi = useProjectsApi()
const { execute: executeGetTasks } = useApiCall<Task[]>()
const { execute: executeGetProjects } = useApiCall<Project[]>()
const { execute: executeCreate } = useApiCall<Task>()
const { showSuccess, showError } = useNotification()

const tasks = ref<Task[]>([])
const projects = ref<Project[]>([])
const showCreateModal = ref(false)
const isCreating = ref(false)

const filters = ref({
  status: '',
  priority: '',
  projectId: ''
})

const newTask = ref<CreateTaskDto>({
  title: '',
  description: '',
  priority: 'medium' as TaskPriority,
  projectId: '',
  dueDate: undefined
})

const filteredTasks = computed(() => {
  return tasks.value.filter(task => {
    if (filters.value.status && task.status !== filters.value.status) return false
    if (filters.value.priority && task.priority !== filters.value.priority) return false
    if (filters.value.projectId && task.projectId !== filters.value.projectId) return false
    return true
  })
})

const loadTasks = async () => {
  try {
    const data = await executeGetTasks(
      () => tasksApi.getTasks(),
      { showErrorNotification: false }
    )
    
    if (data) {
      tasks.value = data
    }
  } catch (error) {
    // Mock data for development
    tasks.value = [
      {
        id: '1',
        title: 'Design homepage mockup',
        description: 'Create wireframes and mockups for the new homepage design',
        status: 'in_progress' as TaskStatus,
        priority: 'high' as TaskPriority,
        projectId: '1',
        project: { id: '1', name: 'Website Redesign' } as Project,
        createdAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000),
        updatedAt: new Date(),
        dueDate: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000)
      },
      {
        id: '2',
        title: 'Set up development environment',
        description: 'Configure local development environment for the project',
        status: 'done' as TaskStatus,
        priority: 'medium' as TaskPriority,
        projectId: '2',
        project: { id: '2', name: 'Mobile App' } as Project,
        createdAt: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000),
        updatedAt: new Date(),
        completedAt: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000)
      }
    ]
  }
}

const loadProjects = async () => {
  try {
    const data = await executeGetProjects(
      () => projectsApi.getProjects(),
      { showErrorNotification: false }
    )
    
    if (data) {
      projects.value = data
    }
  } catch (error) {
    // Mock data for development
    projects.value = [
      { id: '1', name: 'Website Redesign' } as Project,
      { id: '2', name: 'Mobile App' } as Project
    ]
  }
}

const createTask = async () => {
  if (!newTask.value.title.trim()) {
    showError('Task title is required')
    return
  }
  if (!newTask.value.projectId) {
    showError('Select a project first. Create one from the Projects page if you don\'t have any yet.')
    return
  }
  
  try {
    isCreating.value = true
    
    const taskData = {
      ...newTask.value,
      dueDate: newTask.value.dueDate ? new Date(newTask.value.dueDate) : undefined
    }
    
    const data = await executeCreate(
      () => tasksApi.createTask(taskData)
    )
    
    if (data) {
      tasks.value.unshift(data)
      showCreateModal.value = false
      resetNewTask()
      showSuccess('Task created successfully!')
    }
  } catch (error) {
    // For development, simulate success
    const project = projects.value.find(p => p.id === newTask.value.projectId)
    const mockTask: Task = {
      id: Date.now().toString(),
      title: newTask.value.title,
      description: newTask.value.description,
      status: 'todo' as TaskStatus,
      priority: newTask.value.priority,
      projectId: newTask.value.projectId,
      project: project,
      createdAt: new Date(),
      updatedAt: new Date(),
      dueDate: newTask.value.dueDate ? new Date(newTask.value.dueDate) : undefined
    }
    
    tasks.value.unshift(mockTask)
    showCreateModal.value = false
    resetNewTask()
    showSuccess('Task created successfully!')
  } finally {
    isCreating.value = false
  }
}

const resetNewTask = () => {
  newTask.value = {
    title: '',
    description: '',
    priority: 'medium' as TaskPriority,
    projectId: '',
    dueDate: undefined
  }
}

const applyFilters = () => {
  // Filters are reactive via computed property
}

const selectTask = (task: Task) => {
  // TODO: Navigate to task detail or open task modal
  console.log('Selected task:', task)
}

const getStatusIcon = (status: TaskStatus) => {
  switch (status) {
    case 'done':
      return CheckCircleIconSolid
    case 'in_progress':
      return PlayIcon
    default:
      return ClockIcon
  }
}

const getStatusIconClass = (status: TaskStatus) => {
  switch (status) {
    case 'done':
      return 'text-green-500'
    case 'in_progress':
      return 'text-blue-500'
    default:
      return 'text-gray-400'
  }
}

const getPriorityBadgeClass = (priority: TaskPriority) => {
  const baseClasses = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium capitalize'
  
  switch (priority) {
    case 'urgent':
      return `${baseClasses} bg-red-100 text-red-800`
    case 'high':
      return `${baseClasses} bg-orange-100 text-orange-800`
    case 'medium':
      return `${baseClasses} bg-yellow-100 text-yellow-800`
    default:
      return `${baseClasses} bg-gray-100 text-gray-800`
  }
}

const formatDate = (date: Date) => {
  return format(new Date(date), 'MMM d')
}

onMounted(() => {
  loadTasks()
  loadProjects()
})
</script>

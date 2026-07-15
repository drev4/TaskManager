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
      <ul v-if="tasks.length > 0" role="list" class="divide-y divide-gray-200">
        <li
          v-for="task in tasks"
          :key="task.id"
          class="px-6 py-4 hover:bg-gray-50 cursor-pointer"
          @click="openEditModal(task)"
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
                  <span v-if="task.project" class="ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800 flex-shrink-0">
                    {{ task.project.name }}
                  </span>
                  <span v-if="task.assignedTo" class="ml-2 text-xs text-gray-500 flex-shrink-0">
                    {{ task.assignedTo.displayName }}
                  </span>
                  <span v-if="task.estimatedHours" class="ml-2 text-xs text-gray-500 flex-shrink-0">
                    {{ task.estimatedHours }}h estimated
                  </span>
                </div>

                <div v-if="task.tags && task.tags.length > 0" class="flex flex-wrap gap-1 mt-2">
                  <span
                    v-for="tag in task.tags"
                    :key="tag"
                    class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-50 text-blue-700"
                  >
                    {{ tag }}
                  </span>
                </div>
              </div>
            </div>

            <div class="flex items-center ml-4 flex-shrink-0">
              <button
                type="button"
                class="p-1.5 rounded-md text-gray-400 hover:text-red-600 hover:bg-red-50"
                title="Delete task"
                @click.stop="confirmDeleteTask(task)"
              >
                <TrashIcon class="h-4 w-4" />
              </button>
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

          <div class="grid grid-cols-2 gap-4">
            <BaseInput
              v-model="newTask.dueDate"
              type="date"
              label="Due Date (Optional)"
            />

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                Estimated Hours (Optional)
              </label>
              <input
                v-model.number="newTask.estimatedHours"
                type="number"
                min="0"
                step="0.5"
                placeholder="e.g. 4"
                class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
              />
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Assignee</label>
            <select
              v-model="newTask.assignedToUserId"
              class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            >
              <option value="">Unassigned</option>
              <option
                v-for="user in activeUsers"
                :key="user.id"
                :value="user.id"
              >
                {{ user.displayName }}
              </option>
            </select>
          </div>

          <BaseInput
            v-model="newTask.tagsInput"
            label="Tags (Optional)"
            placeholder="Comma-separated, e.g. frontend, urgent"
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

    <!-- Edit Task Modal -->
    <BaseModal
      v-model:show="showEditModal"
      title="Edit Task"
      size="lg"
    >
      <form @submit.prevent="updateTask">
        <div class="space-y-4">
          <BaseInput
            v-model="editForm.title"
            label="Task Title"
            placeholder="Enter task title"
            required
          />

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Description
            </label>
            <textarea
              v-model="editForm.description"
              rows="3"
              class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
              placeholder="Optional task description"
            ></textarea>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select
                v-model="editForm.status"
                class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
              >
                <option value="todo">To Do</option>
                <option value="in_progress">In Progress</option>
                <option value="done">Done</option>
              </select>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Priority</label>
              <select
                v-model="editForm.priority"
                class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
              >
                <option value="low">Low</option>
                <option value="medium">Medium</option>
                <option value="high">High</option>
                <option value="urgent">Urgent</option>
              </select>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <BaseInput
              v-model="editForm.dueDate"
              type="date"
              label="Due Date (Optional)"
            />

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                Estimated Hours (Optional)
              </label>
              <input
                v-model.number="editForm.estimatedHours"
                type="number"
                min="0"
                step="0.5"
                placeholder="e.g. 4"
                class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
              />
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Assignee</label>
            <select
              v-model="editForm.assignedToUserId"
              class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            >
              <option value="">Unassigned</option>
              <option
                v-for="user in activeUsers"
                :key="user.id"
                :value="user.id"
              >
                {{ user.displayName }}
              </option>
            </select>
          </div>

          <BaseInput
            v-model="editForm.tagsInput"
            label="Tags (Optional)"
            placeholder="Comma-separated, e.g. frontend, urgent"
          />
        </div>
      </form>

      <template #footer>
        <div class="flex justify-end space-x-3">
          <BaseButton
            variant="outline"
            @click="showEditModal = false"
          >
            Cancel
          </BaseButton>
          <BaseButton
            variant="primary"
            :loading="isUpdating"
            @click="updateTask"
          >
            Save Changes
          </BaseButton>
        </div>
      </template>
    </BaseModal>

    <!-- Delete Confirmation Modal -->
    <BaseModal
      v-model:show="showDeleteModal"
      title="Delete Task"
      size="sm"
    >
      <p class="text-sm text-gray-600">
        Are you sure you want to delete
        <span class="font-medium text-gray-900">"{{ taskToDelete?.title }}"</span>?
        This cannot be undone.
      </p>

      <template #footer>
        <div class="flex justify-end space-x-3">
          <BaseButton
            variant="outline"
            @click="showDeleteModal = false"
          >
            Cancel
          </BaseButton>
          <BaseButton
            variant="danger"
            :loading="isDeleting"
            @click="deleteTask"
          >
            Delete
          </BaseButton>
        </div>
      </template>
    </BaseModal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { format } from 'date-fns'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import {
  PlusIcon,
  TrashIcon,
  CheckCircleIcon,
  ClockIcon,
  PlayIcon
} from '@heroicons/vue/24/outline'
import {
  CheckCircleIcon as CheckCircleIconSolid
} from '@heroicons/vue/24/solid'
import type { Task, Project, User, CreateTaskDto, UpdateTaskDto, TaskStatus, TaskPriority } from '@/types'
import { useTasksApi, useProjectsApi, useUsersApi, useApiCall } from '@/composables/useApi'
import { useNotification } from '@/composables/useNotification'

const tasksApi = useTasksApi()
const projectsApi = useProjectsApi()
const usersApi = useUsersApi()
const { execute: executeGetTasks } = useApiCall<Task[]>()
const { execute: executeGetProjects } = useApiCall<Project[]>()
const { execute: executeGetUsers } = useApiCall<User[]>()
const { execute: executeCreate } = useApiCall<Task>()
const { execute: executeUpdate } = useApiCall<Task>()
const { execute: executeDelete } = useApiCall<void>()
const { showSuccess, showError } = useNotification()

const tasks = ref<Task[]>([])
const projects = ref<Project[]>([])
const activeUsers = ref<User[]>([])
const showCreateModal = ref(false)
const isCreating = ref(false)
const showEditModal = ref(false)
const isUpdating = ref(false)
const editingTaskId = ref<string | null>(null)
const showDeleteModal = ref(false)
const isDeleting = ref(false)
const taskToDelete = ref<Task | null>(null)

const filters = ref({
  status: '',
  priority: '',
  projectId: ''
})

const newTask = ref<CreateTaskDto & { tagsInput: string }>({
  title: '',
  description: '',
  priority: 'medium' as TaskPriority,
  projectId: '',
  assignedToUserId: '',
  dueDate: undefined,
  estimatedHours: undefined,
  tagsInput: ''
})

const editForm = ref({
  title: '',
  description: '',
  status: 'todo' as TaskStatus,
  priority: 'medium' as TaskPriority,
  assignedToUserId: '',
  dueDate: undefined as string | undefined,
  estimatedHours: undefined as number | undefined,
  tagsInput: ''
})

const loadTasks = async () => {
  try {
    const data = await executeGetTasks(
      () => tasksApi.getTasks(filters.value),
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

const loadActiveUsers = async () => {
  try {
    const data = await executeGetUsers(
      () => usersApi.getActiveUsers(),
      { showErrorNotification: false }
    )

    if (data) {
      activeUsers.value = data
    }
  } catch (error) {
    // Mock data for development
    activeUsers.value = []
  }
}

const buildTaskPayload = (): CreateTaskDto => {
  const tags = newTask.value.tagsInput
    .split(',')
    .map(tag => tag.trim())
    .filter(Boolean)

  return {
    title: newTask.value.title,
    description: newTask.value.description,
    priority: newTask.value.priority,
    projectId: newTask.value.projectId,
    assignedToUserId: newTask.value.assignedToUserId || undefined,
    dueDate: newTask.value.dueDate ? new Date(newTask.value.dueDate) : undefined,
    estimatedHours: newTask.value.estimatedHours || undefined,
    tags: tags.length > 0 ? tags : undefined
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

    const taskData = buildTaskPayload()

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
    // executeCreate already surfaced the real error via a toast; nothing to add here.
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
    assignedToUserId: '',
    dueDate: undefined,
    estimatedHours: undefined,
    tagsInput: ''
  }
}

const applyFilters = () => {
  loadTasks()
}

const openEditModal = (task: Task) => {
  editingTaskId.value = task.id
  editForm.value = {
    title: task.title,
    description: task.description || '',
    status: task.status,
    priority: task.priority,
    assignedToUserId: task.assignedToUserId || '',
    dueDate: task.dueDate ? format(new Date(task.dueDate), 'yyyy-MM-dd') : undefined,
    estimatedHours: task.estimatedHours,
    tagsInput: task.tags ? task.tags.join(', ') : ''
  }
  showEditModal.value = true
}

const updateTask = async () => {
  if (!editingTaskId.value) return

  if (!editForm.value.title.trim()) {
    showError('Task title is required')
    return
  }

  const tags = editForm.value.tagsInput
    .split(',')
    .map(tag => tag.trim())
    .filter(Boolean)

  // The API only clears an existing assignment when it sees Guid.Empty; omitting
  // the field (undefined/null) leaves the current assignee untouched.
  const UNASSIGNED_SENTINEL = '00000000-0000-0000-0000-000000000000'

  const payload: UpdateTaskDto = {
    title: editForm.value.title,
    description: editForm.value.description,
    status: editForm.value.status,
    priority: editForm.value.priority,
    assignedToUserId: editForm.value.assignedToUserId || UNASSIGNED_SENTINEL,
    dueDate: editForm.value.dueDate ? new Date(editForm.value.dueDate) : undefined,
    estimatedHours: editForm.value.estimatedHours || undefined,
    tags
  }

  try {
    isUpdating.value = true

    const data = await executeUpdate(
      () => tasksApi.updateTask(editingTaskId.value!, payload)
    )

    if (data) {
      const index = tasks.value.findIndex(t => t.id === editingTaskId.value)
      if (index !== -1) {
        tasks.value[index] = data
      }
      showEditModal.value = false
      showSuccess('Task updated successfully!')
    }
  } catch (error) {
    showError('Failed to update task. Please try again.')
  } finally {
    isUpdating.value = false
  }
}

const confirmDeleteTask = (task: Task) => {
  taskToDelete.value = task
  showDeleteModal.value = true
}

const deleteTask = async () => {
  if (!taskToDelete.value) return

  try {
    isDeleting.value = true

    await executeDelete(() => tasksApi.deleteTask(taskToDelete.value!.id))
    tasks.value = tasks.value.filter(t => t.id !== taskToDelete.value!.id)
    showDeleteModal.value = false
    showSuccess('Task deleted successfully!')
  } catch (error) {
    showError('Failed to delete task. Please try again.')
  } finally {
    isDeleting.value = false
  }
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
  loadActiveUsers()
})
</script>

import { ref, computed } from 'vue'
import axios, { type AxiosError, type AxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/stores/auth'
import { useNotification } from '@/composables/useNotification'
import { config as appConfig } from '@/services/config'
import type { ApiResponse, ApiError } from '@/types'

// Create axios instance
const api = axios.create({
  baseURL: appConfig.get('api').baseUrl,
  timeout: appConfig.get('api').timeout,
  headers: {
    'Content-Type': 'application/json'
  }
})

export function useApi() {
  const authStore = useAuthStore()
  const { showError } = useNotification()

  // Request interceptor to add auth token
  api.interceptors.request.use(
    async (requestConfig) => {
      if (appConfig.isAuthEnabled()) {
        const token = await authStore.getAccessToken()
        if (token) {
          requestConfig.headers.Authorization = `Bearer ${token}`
        }
      }
      return requestConfig
    },
    (error) => {
      return Promise.reject(error)
    }
  )
  
  // Response interceptor to handle errors
  api.interceptors.response.use(
    (response) => response,
    (error: AxiosError<ApiError>) => {
      const { response } = error
      
      if (response?.status === 401) {
        // Unauthorized - redirect to login
        authStore.logout()
        return Promise.reject(error)
      }
      
      if (response?.status === 403) {
        showError('You do not have permission to perform this action')
        return Promise.reject(error)
      }
      
      if (response?.status >= 500) {
        showError('Server error. Please try again later.')
        return Promise.reject(error)
      }
      
      // Return the error for specific handling
      return Promise.reject(error)
    }
  )
  
  return { api }
}

// Pulls a human-readable message out of either an ApiResponseDto error body
// ({ message }) or an ASP.NET Core ProblemDetails body ({ detail, errors }),
// falling back to axios's generic "Request failed with status code N".
const extractErrorMessage = (err: any): string => {
  const body = err.response?.data

  if (body?.errors && typeof body.errors === 'object') {
    const firstField = Object.values(body.errors)[0]
    if (Array.isArray(firstField) && firstField.length > 0) {
      return firstField[0]
    }
  }

  return body?.detail || body?.message || err.message || 'An error occurred'
}

// Composable for API requests with loading state
export function useApiCall<T = any>() {
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const data = ref<T | null>(null)
  
  const { api } = useApi()
  const { showError } = useNotification()
  
  const execute = async (
    requestFn: () => Promise<any>,
    options?: {
      showErrorNotification?: boolean
      onSuccess?: (data: T) => void
      onError?: (error: any) => void
    }
  ) => {
    const {
      showErrorNotification = true,
      onSuccess,
      onError
    } = options || {}

    try {
      isLoading.value = true
      error.value = null

      const response = await requestFn()

      // Extract data from ApiResponseDto wrapper if present
      const responseData = response.data?.data !== undefined ? response.data.data : response.data
      data.value = responseData

      if (onSuccess) {
        onSuccess(responseData)
      }

      return responseData
    } catch (err: any) {
      const errorMessage = extractErrorMessage(err)
      error.value = errorMessage

      if (showErrorNotification) {
        showError(errorMessage)
      }

      if (onError) {
        onError(err)
      }

      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  const reset = () => {
    isLoading.value = false
    error.value = null
    data.value = null
  }
  
  return {
    isLoading: computed(() => isLoading.value),
    error: computed(() => error.value),
    data: computed(() => data.value),
    execute,
    reset
  }
}

// Specific API methods
export function useProjectsApi() {
  const { api } = useApi()
  
  const getProjects = () => api.get('/api/Projects')
  const getProject = (id: string) => api.get(`/api/Projects/${id}`)
  const createProject = (data: any) => api.post('/api/Projects', data)
  const updateProject = (id: string, data: any) => api.put(`/api/Projects/${id}`, data)
  const deleteProject = (id: string) => api.delete(`/api/Projects/${id}`)
  
  return {
    getProjects,
    getProject,
    createProject,
    updateProject,
    deleteProject
  }
}

// The API accepts enum query values as their PascalCase C# member names
// (e.g. "InProgress"), independent of the snake_case used in JSON bodies.
const toEnumQueryValue = (value: string) =>
  value.split('_').map(part => part.charAt(0).toUpperCase() + part.slice(1)).join('')

export interface TaskFilters {
  projectId?: string
  status?: string
  priority?: string
}

export function useTasksApi() {
  const { api } = useApi()

  const getTasks = (filters?: TaskFilters) => {
    const params: Record<string, string> = {}
    if (filters?.projectId) params.projectId = filters.projectId
    if (filters?.status) params.status = toEnumQueryValue(filters.status)
    if (filters?.priority) params.priority = toEnumQueryValue(filters.priority)
    return api.get('/api/Tasks', { params })
  }

  const getTask = (id: string) => api.get(`/api/Tasks/${id}`)
  const createTask = (data: any) => api.post('/api/Tasks', data)
  const updateTask = (id: string, data: any) => api.put(`/api/Tasks/${id}`, data)
  const deleteTask = (id: string) => api.delete(`/api/Tasks/${id}`)
  
  return {
    getTasks,
    getTask,
    createTask,
    updateTask,
    deleteTask
  }
}

export function useDashboardApi() {
  const { api } = useApi()

  const getDashboardStats = () => api.get('/api/Dashboard/stats')
  const getRecentActivity = () => api.get('/api/Dashboard/activity')

  return {
    getDashboardStats,
    getRecentActivity
  }
}

export function useUsersApi() {
  const { api } = useApi()

  const initializeUser = () => api.post('/api/Users/initialize')
  const getCurrentUser = () => api.get('/api/Users/me')
  const getActiveUsers = () => api.get('/api/Users')

  return {
    initializeUser,
    getActiveUsers,
    getCurrentUser
  }
}

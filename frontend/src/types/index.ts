// User types
export interface User {
  id: string
  email: string
  displayName: string
  avatar?: string
  createdAt: Date
  updatedAt: Date
}

// Project types
export interface Project {
  id: string
  name: string
  description?: string
  ownerUserId: string
  owner?: User
  tasks?: Task[]
  createdAt: Date
  updatedAt: Date
  completedTasksCount: number
  totalTasksCount: number
}

export interface CreateProjectDto {
  name: string
  description?: string
}

export interface UpdateProjectDto {
  name?: string
  description?: string
}

// Task types
export enum TaskStatus {
  TODO = 'todo',
  IN_PROGRESS = 'in_progress',
  DONE = 'done'
}

export enum TaskPriority {
  LOW = 'low',
  MEDIUM = 'medium',
  HIGH = 'high',
  URGENT = 'urgent'
}

export interface Task {
  id: string
  title: string
  description?: string
  status: TaskStatus
  priority: TaskPriority
  projectId: string
  project?: Project
  assignedToUserId?: string
  assignedTo?: User
  dueDate?: Date
  createdAt: Date
  updatedAt: Date
  completedAt?: Date
}

export interface CreateTaskDto {
  title: string
  description?: string
  priority: TaskPriority
  projectId: string
  assignedToUserId?: string
  dueDate?: Date
}

export interface UpdateTaskDto {
  title?: string
  description?: string
  status?: TaskStatus
  priority?: TaskPriority
  assignedToUserId?: string
  dueDate?: Date
}

// API Response types
export interface ApiResponse<T = any> {
  data: T
  message?: string
  success: boolean
}

export interface PaginatedResponse<T> extends ApiResponse<T[]> {
  pagination: {
    page: number
    limit: number
    total: number
    totalPages: number
  }
}

export interface ApiError {
  message: string
  errors?: Record<string, string[]>
  statusCode: number
}

// Auth types
export interface AuthUser {
  id: string
  email: string
  displayName: string
  avatar?: string
}

export interface LoginCredentials {
  email: string
  password: string
}

// UI Component types
export interface SelectOption {
  label: string
  value: string | number
  disabled?: boolean
}

export interface NotificationOptions {
  title: string
  message: string
  type: 'success' | 'error' | 'warning' | 'info'
  duration?: number
}

// Form validation types
export interface ValidationError {
  field: string
  message: string
}

// Dashboard types
export interface DashboardStats {
  totalProjects: number
  totalTasks: number
  completedTasks: number
  pendingTasks: number
  overdueTasks: number
}

export interface RecentActivity {
  id: string
  type: 'task_created' | 'task_completed' | 'project_created' | 'task_assigned'
  description: string
  timestamp: Date
  user: User
  relatedItem?: {
    id: string
    name: string
    type: 'task' | 'project'
  }
}

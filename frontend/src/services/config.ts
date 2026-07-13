interface AppConfig {
  api: {
    baseUrl: string
    timeout: number
  }
  signalR: {
    hubUrl: string
    retryDelay: number
    maxRetries: number
  }
  auth: {
    enabled: boolean
    mockUser: {
      id: string
      email: string
      displayName: string
    }
  }
  features: {
    notifications: boolean
    realTimeUpdates: boolean
    analytics: boolean
  }
  environment: 'development' | 'staging' | 'production'
}

class ConfigService {
  private static instance: ConfigService
  private config: AppConfig

  private constructor() {
    this.config = this.loadConfig()
  }

  static getInstance(): ConfigService {
    if (!ConfigService.instance) {
      ConfigService.instance = new ConfigService()
    }
    return ConfigService.instance
  }

  private loadConfig(): AppConfig {
    const environment = (import.meta.env.MODE || 'development') as 'development' | 'staging' | 'production'
    
    return {
      api: {
        baseUrl: this.getApiBaseUrl(),
        timeout: 10000
      },
      signalR: {
        hubUrl: `${this.getApiBaseUrl()}/hubs/task`,
        retryDelay: 3000,
        maxRetries: 5
      },
      auth: {
        enabled: environment === 'production',
        mockUser: {
          id: '3',
          email: 'bob.wilson@example.com',
          displayName: 'Bob Wilson'
        }
      },
      features: {
        notifications: true,
        realTimeUpdates: true,
        analytics: environment === 'production'
      },
      environment
    }
  }

  private getApiBaseUrl(): string {
    // In production, don't use fallback URLs
    if (import.meta.env.PROD) {
      const baseUrl = import.meta.env.VITE_API_BASE_URL
      if (!baseUrl) {
        throw new Error('VITE_API_BASE_URL is required in production')
      }
      return baseUrl
    }

    // In development, use fallback
    return import.meta.env.VITE_API_BASE_URL || 'http://localhost:65454'
  }

  get<K extends keyof AppConfig>(key: K): AppConfig[K] {
    return this.config[key]
  }

  getAll(): AppConfig {
    return { ...this.config }
  }

  isDevelopment(): boolean {
    return this.config.environment === 'development'
  }

  isProduction(): boolean {
    return this.config.environment === 'production'
  }

  isAuthEnabled(): boolean {
    return this.config.auth.enabled
  }

  getMockUser() {
    return this.config.auth.mockUser
  }
}

export const config = ConfigService.getInstance()
export type { AppConfig }
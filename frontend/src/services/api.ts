import axios, { AxiosInstance, AxiosError, AxiosRequestConfig, AxiosResponse } from 'axios';
import { logger } from './logger';
import { config } from './config';
import { useAuthStore } from '@/stores/auth';
import { useNotification } from '@/composables/useNotification';

class ApiService {
  private api: AxiosInstance;

  constructor() {
    const apiConfig = config.get('api');
    
    this.api = axios.create({
      baseURL: apiConfig.baseUrl,
      timeout: apiConfig.timeout,
      headers: {
        'Content-Type': 'application/json',
      }
    });

    this.setupInterceptors();
  }

  private setupInterceptors() {
    // Request interceptor
    this.api.interceptors.request.use(
      (config: AxiosRequestConfig) => {
        logger.debug(`API Request: ${config.method?.toUpperCase()} ${config.url}`, {
          data: config.data,
          params: config.params
        });

        // Add authentication header if available
        const authStore = useAuthStore();
        if (authStore.isAuthenticated) {
          // This would be implemented when auth is fully enabled
          // const token = await authStore.getAccessToken();
          // if (token && config.headers) {
          //   config.headers.Authorization = `Bearer ${token}`;
          // }
        }

        // Add request timestamp
        if (config.headers) {
          config.headers['X-Request-Time'] = new Date().toISOString();
        }

        return config;
      },
      (error: AxiosError) => {
        logger.error('API Request Error:', error);
        return Promise.reject(error);
      }
    );

    // Response interceptor
    this.api.interceptors.response.use(
      (response: AxiosResponse) => {
        const duration = Date.now() - new Date(response.config.headers?.['X-Request-Time'] as string).getTime();
        
        logger.debug(`API Response: ${response.config.method?.toUpperCase()} ${response.config.url}`, {
          status: response.status,
          duration: `${duration}ms`
        });

        return response;
      },
      async (error: AxiosError) => {
        const { showNotification } = useNotification();
        
        const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };
        
        logger.error('API Response Error:', {
          url: originalRequest?.url,
          method: originalRequest?.method,
          status: error.response?.status,
          message: error.message,
          data: error.response?.data
        });

        // Handle specific error cases
        if (error.response) {
          const status = error.response.status;
          const errorData = error.response.data as any;

          switch (status) {
            case 401:
              // Unauthorized - handle token refresh or redirect to login
              if (!originalRequest._retry) {
                originalRequest._retry = true;
                
                // This would be implemented when auth is fully enabled
                // const authStore = useAuthStore();
                // const refreshed = await authStore.refreshToken();
                // if (refreshed) {
                //   return this.api(originalRequest);
                // }
              }

              showNotification({
                title: 'Authentication Required',
                message: 'Please sign in to continue.',
                type: 'error'
              });
              
              // Redirect to login (would be implemented when auth is enabled)
              // router.push('/auth/login');
              break;

            case 403:
              showNotification({
                title: 'Access Denied',
                message: 'You do not have permission to perform this action.',
                type: 'error'
              });
              break;

            case 404:
              showNotification({
                title: 'Not Found',
                message: 'The requested resource was not found.',
                type: 'warning'
              });
              break;

            case 422:
              // Validation errors
              const validationErrors = errorData?.errors || errorData?.message || 'Validation failed';
              showNotification({
                title: 'Validation Error',
                message: Array.isArray(validationErrors) ? validationErrors.join(', ') : validationErrors,
                type: 'error'
              });
              break;

            case 429:
              showNotification({
                title: 'Rate Limited',
                message: 'Too many requests. Please try again later.',
                type: 'warning'
              });
              break;

            case 500:
              showNotification({
                title: 'Server Error',
                message: 'An unexpected error occurred. Please try again.',
                type: 'error'
              });
              break;

            default:
              showNotification({
                title: 'Request Failed',
                message: errorData?.message || 'An error occurred while processing your request.',
                type: 'error'
              });
          }
        } else if (error.request) {
          // Network error
          showNotification({
            title: 'Network Error',
            message: 'Unable to connect to the server. Please check your internet connection.',
            type: 'error'
          });
        } else {
          // Other error
          showNotification({
            title: 'Error',
            message: 'An unexpected error occurred.',
            type: 'error'
          });
        }

        return Promise.reject(error);
      }
    );
  }

  // HTTP methods
  async get<T = any>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.api.get<T>(url, config);
    return response.data;
  }

  async post<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.api.post<T>(url, data, config);
    return response.data;
  }

  async put<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.api.put<T>(url, data, config);
    return response.data;
  }

  async patch<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.api.patch<T>(url, data, config);
    return response.data;
  }

  async delete<T = any>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.api.delete<T>(url, config);
    return response.data;
  }

  // Get the raw axios instance for custom requests
  getAxiosInstance(): AxiosInstance {
    return this.api;
  }
}

// Export singleton instance
export const apiService = new ApiService();

// Also export default for backward compatibility
export default apiService;
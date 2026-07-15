import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { msalInstance, initializeMsal } from '@/auth/msal'
import type { AuthUser } from '@/types'
import { useNotification } from '@/composables/useNotification'

const apiScope = import.meta.env.VITE_AUTH_API_SCOPE as string | undefined

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<AuthUser | null>(null)
  const isInitialized = ref(false)
  const isLoading = ref(false)

  // Notification helper
  const { showNotification } = useNotification()
  
  // Getters
  const isAuthenticated = computed(() => !!user.value)
  const currentUser = computed(() => user.value)
  
  // Actions
  const initialize = async () => {
    try {
      isLoading.value = true

      // Initialize MSAL first
      await initializeMsal()

      // Handle redirect response
      await msalInstance.handleRedirectPromise()

      // Check if user is already logged in
      const currentAccounts = msalInstance.getAllAccounts()
      if (currentAccounts.length > 0) {
        const account = currentAccounts[0]
        await setUserFromAccount(account)
      }

      isInitialized.value = true
    } catch (error) {
      console.error('Error initializing auth:', error)
      showNotification({
        title: 'Authentication Error',
        message: 'Failed to initialize authentication',
        type: 'error'
      })
    } finally {
      isLoading.value = false
    }
  }
  
  const login = async () => {
    try {
      isLoading.value = true
      
      const loginRequest = {
        scopes: ['openid', 'profile', 'email'],
        redirectUri: window.location.origin + '/auth/callback'
      }
      
      await msalInstance.loginRedirect(loginRequest)
    } catch (error) {
      console.error('Login error:', error)
      showNotification({
        title: 'Login Failed',
        message: 'Unable to sign in. Please try again.',
        type: 'error'
      })
      isLoading.value = false
    }
  }
  
  const logout = async () => {
    try {
      isLoading.value = true
      
      const logoutRequest = {
        account: msalInstance.getActiveAccount(),
        postLogoutRedirectUri: window.location.origin + '/auth/login'
      }
      
      await msalInstance.logoutRedirect(logoutRequest)
      
      // Clear user state
      user.value = null
      
      showNotification({
        title: 'Signed Out',
        message: 'You have been successfully signed out',
        type: 'success'
      })
    } catch (error) {
      console.error('Logout error:', error)
      showNotification({
        title: 'Logout Error',
        message: 'Failed to sign out properly',
        type: 'error'
      })
    } finally {
      isLoading.value = false
    }
  }
  
  const getAccessToken = async (): Promise<string | null> => {
    try {
      const account = msalInstance.getActiveAccount()
      if (!account) return null
      
      const tokenRequest = {
        scopes: apiScope ? [apiScope] : [],
        account: account
      }

      const response = await msalInstance.acquireTokenSilent(tokenRequest)
      return response.accessToken
    } catch (error) {
      console.error('Error getting access token:', error)

      try {
        // Try to get token via popup if silent request fails
        const tokenRequest = {
          scopes: apiScope ? [apiScope] : [],
          account: msalInstance.getActiveAccount()
        }
        
        const response = await msalInstance.acquireTokenPopup(tokenRequest)
        return response.accessToken
      } catch (popupError) {
        console.error('Error getting token via popup:', popupError)
        return null
      }
    }
  }
  
  const setUserFromAccount = async (account: any) => {
    try {
      user.value = {
        id: account.localAccountId,
        email: account.username,
        displayName: account.name || account.username,
        avatar: undefined // Could fetch from Microsoft Graph API
      }
      
      msalInstance.setActiveAccount(account)
    } catch (error) {
      console.error('Error setting user from account:', error)
    }
  }
  
  const refreshUser = async () => {
    try {
      const account = msalInstance.getActiveAccount()
      if (account) {
        await setUserFromAccount(account)
      }
    } catch (error) {
      console.error('Error refreshing user:', error)
    }
  }
  
  const handleRedirectCallback = async () => {
    try {
      const response = await msalInstance.handleRedirectPromise()
      
      if (response && response.account) {
        await setUserFromAccount(response.account)
        return true
      }
      
      return false
    } catch (error) {
      console.error('Error handling redirect callback:', error)
      showNotification({
        title: 'Authentication Error',
        message: 'Failed to complete sign in',
        type: 'error'
      })
      return false
    }
  }
  
  return {
    // State
    user: readonly(user),
    isInitialized: readonly(isInitialized),
    isLoading: readonly(isLoading),
    
    // Getters
    isAuthenticated,
    currentUser,
    
    // Actions
    initialize,
    login,
    logout,
    getAccessToken,
    refreshUser,
    handleRedirectCallback
  }
})

// Helper to make reactive properties readonly
function readonly<T>(ref: any): T {
  return ref
}

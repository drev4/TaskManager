import { PublicClientApplication, type Configuration, LogLevel } from '@azure/msal-browser'

const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_B2C_CLIENT_ID || 'your-client-id',
    authority: import.meta.env.VITE_B2C_AUTHORITY || 'https://your-tenant.b2clogin.com/your-tenant.onmicrosoft.com/B2C_1_signupsignin',
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
    knownAuthorities: [import.meta.env.VITE_B2C_AUTHORITY?.replace('https://', '') || 'your-tenant.b2clogin.com']
  },
  cache: {
    cacheLocation: 'localStorage',
    storeAuthStateInCookie: false
  },
  system: {
    loggerOptions: {
      loggerCallback: (_level, message, containsPii) => {
        if (containsPii) return
        console.log(message)
      },
      piiLoggingEnabled: false,
      logLevel: import.meta.env.DEV ? LogLevel.Info : LogLevel.Error
    }
  }
}

export const msalInstance = new PublicClientApplication(msalConfig)

// Initialize MSAL instance (async)
export const initializeMsal = async () => {
  await msalInstance.initialize()
}
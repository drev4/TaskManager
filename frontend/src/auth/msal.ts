import { PublicClientApplication, type Configuration, LogLevel } from '@azure/msal-browser'

const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AUTH_CLIENT_ID || 'your-client-id',
    // Microsoft Entra External ID authority: https://<tenant-subdomain>.ciamlogin.com/
    // (no user-flow/policy name in the URL - the user flow is associated with the
    // app registration in the Entra admin center instead). If a custom URL domain
    // is configured, add it here via knownAuthorities.
    authority: import.meta.env.VITE_AUTH_AUTHORITY || 'https://your-tenant.ciamlogin.com/',
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin
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
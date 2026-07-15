import { createApp } from 'vue'
import { createPinia } from 'pinia'
import Toast from 'vue-toastification'
import 'vue-toastification/dist/index.css'
import 'nprogress/nprogress.css'

import App from './App.vue'
import router from './router'
import { config } from './services/config'
import { useAuthStore } from './stores/auth'
import './assets/styles/main.css'

async function bootstrap() {
  const app = createApp(App)
  const pinia = createPinia()

  // Setup Pinia store
  app.use(pinia)

  // Setup router
  app.use(router)

  // Setup toast notifications
  app.use(Toast, {
    position: 'top-right',
    timeout: 5000,
    closeOnClick: true,
    pauseOnFocusLoss: true,
    pauseOnHover: true,
    draggable: true,
    draggablePercent: 0.6,
    showCloseButtonOnHover: false,
    hideProgressBar: false,
    closeButton: 'button',
    icon: true,
    rtl: false
  })

  // Resolve any signed-in account before the router's first navigation guard
  // and App.vue's onMounted run, so both see the correct auth state immediately.
  if (config.isAuthEnabled()) {
    await useAuthStore(pinia).initialize()
  }

  app.mount('#app')
}

bootstrap()
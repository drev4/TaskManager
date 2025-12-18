import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import NProgress from 'nprogress'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/dashboard'
    },
    {
      path: '/auth',
      component: () => import('@/layouts/AuthLayout.vue'),
      children: [
        {
          path: 'login',
          name: 'login',
          component: () => import('@/pages/auth/LoginPage.vue'),
          meta: { requiresGuest: true }
        },
        {
          path: 'callback',
          name: 'auth-callback',
          component: () => import('@/pages/auth/CallbackPage.vue')
        }
      ]
    },
    {
      path: '/',
      component: () => import('@/layouts/AppLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        {
          path: 'dashboard',
          name: 'dashboard',
          component: () => import('@/pages/DashboardPage.vue')
        },
        {
          path: 'projects',
          name: 'projects',
          component: () => import('@/pages/ProjectsPage.vue')
        },
        {
          path: 'projects/:id',
          name: 'project-detail',
          component: () => import('@/pages/ProjectDetailPage.vue'),
          props: true
        },
        {
          path: 'tasks',
          name: 'tasks',
          component: () => import('@/pages/TasksPage.vue')
        },
        {
          path: 'profile',
          name: 'profile',
          component: () => import('@/pages/ProfilePage.vue')
        }
      ]
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/pages/NotFoundPage.vue')
    }
  ]
})

// Navigation guards
router.beforeEach(async (to, from, next) => {
  NProgress.start()

  // TODO: Enable authentication when Azure AD B2C is configured
  // const authStore = useAuthStore()

  // // Initialize authentication if not already done
  // if (!authStore.isInitialized) {
  //   await authStore.initialize()
  // }

  // const requiresAuth = to.matched.some(record => record.meta.requiresAuth)
  // const requiresGuest = to.matched.some(record => record.meta.requiresGuest)

  // if (requiresAuth && !authStore.isAuthenticated) {
  //   next({ name: 'login' })
  // } else if (requiresGuest && authStore.isAuthenticated) {
  //   next({ name: 'dashboard' })
  // } else {
  //   next()
  // }

  // Temporary: Skip authentication for development
  next()
})

router.afterEach(() => {
  NProgress.done()
})

export default router

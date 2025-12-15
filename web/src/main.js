import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { vMaska } from "maska/vue" 
import axios from 'axios'

import App from './App.vue'
import router from './router'
import { useAuthStore } from '@/stores/auth'

import './assets/main.css'

const app = createApp(App)
const pinia = createPinia()

app.directive("maska", vMaska)
app.use(pinia)
app.use(router)

const authStore = useAuthStore();

axios.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response && error.response.status === 401) {
            authStore.logout();
            router.push('/login');
        }
        return Promise.reject(error);
    }
);

authStore.initialize();

app.mount('#app')
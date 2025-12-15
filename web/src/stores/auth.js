import { defineStore } from 'pinia';
import axios from 'axios';
import router from '@/router';

export const useAuthStore = defineStore('auth', {
    state: () => ({
        token: sessionStorage.getItem('token') || null,
        user: JSON.parse(sessionStorage.getItem('user')) || null,
        returnUrl: null,
        roles: []
    }),
    actions: {
        async login(email, password) {
            try {
                const response = await axios.post('http://localhost:5000/api/auth/login', {
                    email,
                    password
                });

                const token = response.data.data;
                const base64Url = token.split('.')[1];
                const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
                const payload = JSON.parse(window.atob(base64));
                
                const roleClaim = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload["role"];
                const userRoles = Array.isArray(roleClaim) ? roleClaim : [roleClaim];

                this.token = token;
                this.user = { email };
                this.roles = userRoles || [];

                sessionStorage.setItem('token', token);
                sessionStorage.setItem('user', JSON.stringify(this.user));
                sessionStorage.setItem('roles', JSON.stringify(this.roles));
                
                axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;

                router.push(this.returnUrl || '/dashboard'); 
            } catch (error) {
                throw error;
            }
        },
        logout() {
            this.token = null;
            this.user = null;
            this.roles = [];
            
            sessionStorage.removeItem('token');
            sessionStorage.removeItem('user');
            sessionStorage.removeItem('roles');
            
            delete axios.defaults.headers.common['Authorization'];
            router.push('/login');
        },
        initialize() {
            const token = sessionStorage.getItem('token');
            if (token) {
                this.token = token;
                this.user = JSON.parse(sessionStorage.getItem('user'));
                this.roles = JSON.parse(sessionStorage.getItem('roles') || '[]');
                axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
            }
        },
        isAdmin() {
            return this.roles && this.roles.includes('Admin');
        }
    }
});
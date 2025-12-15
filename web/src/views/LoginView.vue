<script setup>
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth';
import logoLogin from '@/assets/img/odontologo.png';

const authStore = useAuthStore();

const email = ref('');
const password = ref('');
const errorMessage = ref('');
const loading = ref(false);

async function handleSubmit() {
    loading.value = true;
    errorMessage.value = '';
    
    try {
        await authStore.login(email.value, password.value);
    } catch (error) {
        if (error.response) {
            if (error.response.status === 401) {
                errorMessage.value = "E-mail ou senha inválidos.";
            } 
            else if (error.response.status === 403) {
                errorMessage.value = error.response.data?.message || "Acesso negado.";
            }
            else {
                errorMessage.value = error.response.data?.message || "Ocorreu um erro no servidor.";
            }
        } else {
            errorMessage.value = "Erro ao conectar com o servidor. Verifique sua conexão.";
        }
    } finally {
        loading.value = false;
    }
}
</script>

<template>
    <div class="login-container">
        <div class="card">
            <div class="logo-area">
                <img :src="logoLogin" alt="Logo OdontoSys" class="logo-img" />
            </div>
            
            <form @submit.prevent="handleSubmit">
                <div class="form-group">
                    <label>E-mail</label>
                    <div class="input-wrapper">
                        <i class="bi bi-envelope input-icon"></i>
                        <input type="email" v-model="email" required placeholder="ex: admin@odonto.com" />
                    </div>
                </div>

                <div class="form-group">
                    <label>Senha</label>
                    <div class="input-wrapper">
                        <i class="bi bi-lock input-icon"></i>
                        <input type="password" v-model="password" required placeholder="******" />
                    </div>
                </div>

                <div v-if="errorMessage" class="error-alert">
                    <i class="bi bi-exclamation-circle-fill"></i> {{ errorMessage }}
                </div>

                <button type="submit" :disabled="loading" class="btn-login">
                    {{ loading ? 'Entrando...' : 'Acessar Sistema' }} <i class="bi bi-arrow-right"></i>
                </button>
            </form>
        </div>
    </div>
</template>

<style scoped>
.login-container {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    background-color: var(--bg-primary);
    transition: background-color 0.3s;
}
.card {
    background: var(--bg-secondary);
    padding: 2.5rem;
    border-radius: 12px;
    box-shadow: 0 10px 30px rgba(0,0,0,0.1);
    width: 100%;
    max-width: 400px;
    text-align: center;
    border: 1px solid var(--border-color);
}

.logo-area { display: flex; align-items: center; justify-content: center; margin-bottom: 2rem; }

.logo-img {
    width: 150px;
    height: auto;
    object-fit: contain;
}

.form-group { margin-bottom: 1.2rem; text-align: left; }
label { display: block; margin-bottom: 0.5rem; color: var(--text-primary); font-size: 0.9rem; font-weight: 500; }

.input-wrapper { position: relative; }
.input-icon { position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: var(--text-secondary); }

input {
    width: 100%;
    padding: 0.8rem 0.8rem 0.8rem 2.5rem;
    border: 1px solid var(--border-color);
    background-color: var(--bg-primary);
    color: var(--text-primary);
    border-radius: 6px;
    font-size: 1rem;
    transition: border-color 0.2s;
    box-sizing: border-box;
}
input:focus { border-color: var(--accent-color); outline: none; }

.btn-login {
    width: 100%;
    padding: 0.8rem;
    background-color: var(--accent-color);
    color: white;
    border: none;
    border-radius: 6px;
    font-size: 1rem;
    font-weight: bold;
    cursor: pointer;
    transition: filter 0.2s;
    margin-top: 1rem;
    display: flex; align-items: center; justify-content: center; gap: 0.5rem;
}
.btn-login:hover { filter: brightness(1.1); }
.btn-login:disabled { background-color: var(--text-secondary); cursor: not-allowed; }

.error-alert {
    background-color: rgba(231, 76, 60, 0.1);
    color: var(--danger-color);
    padding: 0.8rem;
    margin-bottom: 1rem;
    border-radius: 6px;
    font-size: 0.9rem;
    display: flex; align-items: center; gap: 0.5rem;
    justify-content: center;
}
</style>
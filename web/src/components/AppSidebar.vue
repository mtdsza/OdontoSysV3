<script setup>
import { ref, computed } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useThemeStore } from '@/stores/theme';
import { useRouter } from 'vue-router';
import ChangePasswordModal from '@/components/ui/ChangePasswordModal.vue';
import logoSidebar from '@/assets/img/odontologosmall.png';

const authStore = useAuthStore();
const themeStore = useThemeStore();
const router = useRouter();
const showPasswordModal = ref(false);

const isDentista = computed(() => {
  return authStore.roles.includes('Dentista') || authStore.roles.includes('Admin');
});

const handleLogout = () => {
  authStore.logout();
  router.push('/login');
};
</script>

<template>
  <aside class="sidebar">
    <div class="logo-area">
      <img :src="logoSidebar" alt="OdontoSys" class="sidebar-logo-img" />
    </div>

    <nav class="nav-links">
      <router-link to="/dashboard" class="nav-item" active-class="active">
        <i class="bi bi-speedometer2"></i> Dashboard
      </router-link>
      
      <router-link to="/pacientes" class="nav-item" active-class="active">
        <i class="bi bi-people-fill"></i> Pacientes
      </router-link>

      <router-link to="/procedimentos" class="nav-item" active-class="active">
        <i class="bi bi-bandaid-fill"></i> Procedimentos
      </router-link>

      <router-link to="/orcamentos" class="nav-item" active-class="active">
        <i class="bi bi-file-earmark-text-fill"></i> Orçamentos
      </router-link>

      <router-link to="/receber" class="nav-item" active-class="active">
        <i class="bi bi-cash-coin"></i> A Receber
      </router-link>

      <router-link to="/financeiro" class="nav-item" active-class="active">
        <i class="bi bi-graph-up-arrow"></i> Finanças
      </router-link>

      <router-link to="/agenda" class="nav-item" active-class="active">
        <i class="bi bi-calendar-week-fill"></i> Agenda
      </router-link>

      <router-link v-if="isDentista" to="/meus-atendimentos" class="nav-item" active-class="active">
        <i class="bi bi-clipboard2-pulse-fill"></i> Atendimentos
      </router-link>

      <router-link to="/estoque" class="nav-item" active-class="active">
        <i class="bi bi-box-seam-fill"></i> Estoque
      </router-link>

      <div v-if="authStore.isAdmin()" class="admin-section">
        <div class="divider"></div>
        <span class="menu-label">ADMINISTRAÇÃO</span>
        
        <router-link to="/funcionarios" class="nav-item" active-class="active">
          <i class="bi bi-person-badge-fill"></i> Funcionários
        </router-link>

        <router-link to="/acessos" class="nav-item" active-class="active">
          <i class="bi bi-shield-lock-fill"></i> Usuários
        </router-link>
      </div>
    </nav>

    <div class="user-area">
      <div class="user-info">
        <small>Logado como:</small>
        <strong>{{ authStore.user?.email }}</strong>
      </div>
      
      <div class="actions">
        <button @click="themeStore.toggleTheme" class="btn-icon-sidebar" title="Alternar Tema">
          <i class="bi" :class="themeStore.currentTheme === 'light' ? 'bi-moon-stars-fill' : 'bi-sun-fill'"></i>
        </button>
        <button @click="showPasswordModal = true" class="btn-icon-sidebar" title="Alterar Senha">
          <i class="bi bi-key-fill"></i>
        </button>
        <button @click="handleLogout" class="btn-icon-sidebar btn-logout" title="Sair">
          <i class="bi bi-box-arrow-right"></i>
        </button>
      </div>
    </div>

    <ChangePasswordModal :isOpen="showPasswordModal" @close="showPasswordModal = false" />
  </aside>
</template>

<style scoped>
.sidebar {
  width: 250px;
  height: 100vh;
  background-color: var(--bg-sidebar);
  color: var(--text-sidebar);
  display: flex;
  flex-direction: column;
  position: fixed;
  left: 0;
  top: 0;
  transition: width 0.3s;
  box-shadow: 2px 0 5px rgba(0,0,0,0.1);
  z-index: 100;
}

.logo-area {
  padding: 1.5rem;
  display: flex;
  justify-content: center;
  align-items: center;
  border-bottom: 1px solid rgba(255,255,255,0.1);
}

.sidebar-logo-img {
  max-width: 80%;
  max-height: 50px;
  object-fit: contain;
}

.nav-links {
  flex: 1;
  padding: 1rem 0;
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  overflow-y: auto;
}

.nav-item {
  padding: 0.8rem 1.5rem;
  color: rgba(255, 255, 255, 0.7); 
  text-decoration: none;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 1rem; 
  transition: all 0.2s;
  border-left: 4px solid transparent;
}

.nav-item i {
  font-size: 1.1rem;
}

.nav-item:hover {
  background-color: rgba(255,255,255,0.05);
  color: var(--text-sidebar);
}

.nav-item.active {
  background-color: rgba(52, 152, 219, 0.1); 
  color: var(--accent-color); 
  border-left-color: var(--accent-color);
}

.divider { 
  border-top: 1px solid rgba(255,255,255,0.1); 
  margin: 0.5rem 1.5rem 0.5rem 1.5rem; 
}

.menu-label { 
  padding-left: 1.5rem; 
  font-size: 0.7rem; 
  color: rgba(255, 255, 255, 0.4); 
  letter-spacing: 1px; 
  font-weight: bold; 
  margin-bottom: 0.5rem; 
  margin-top: 0.5rem;
  display: block; 
  text-transform: uppercase;
}

.user-area {
  padding: 1rem;
  background-color: rgba(0,0,0,0.2);
  border-top: 1px solid rgba(255,255,255,0.1);
}

.user-info {
  display: flex;
  flex-direction: column;
  margin-bottom: 1rem;
  font-size: 0.85rem;
}

.user-info small { opacity: 0.6; }

.actions { display: flex; justify-content: space-between; gap: 0.5rem; }

.btn-icon-sidebar {
  flex: 1;
  background-color: rgba(255,255,255,0.1);
  border: none;
  padding: 0.5rem;
  border-radius: 4px;
  cursor: pointer;
  color: var(--text-sidebar);
  font-size: 1rem;
  transition: background 0.2s, color 0.2s;
  display: flex;
  justify-content: center;
  align-items: center;
}

.btn-icon-sidebar:hover { background-color: rgba(255,255,255,0.2); color: white; }
.btn-logout:hover { background-color: var(--danger-color); color: white; }
</style>
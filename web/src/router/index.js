import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

import MainLayout from '@/layouts/MainLayout.vue'
import LoginView from '@/views/LoginView.vue'
import DashboardView from '@/views/DashboardView.vue'
import PacientesView from '@/views/PacientesView.vue'
import ProcedimentosView from '@/views/ProcedimentosView.vue'
import OrcamentosView from '@/views/OrcamentosView.vue'
import ContasReceberView from '@/views/ContasReceberView.vue'
import FinanceiroView from '@/views/FinanceiroView.vue'
import AgendaView from '@/views/AgendaView.vue'
import EstoqueView from '@/views/EstoqueView.vue'
import MinhaAgendaView from '@/views/MinhaAgendaView.vue'
import RealizarAtendimentoView from '@/views/RealizarAtendimentoView.vue'

import FuncionariosView from '@/views/FuncionariosView.vue'
import AcessoView from '@/views/AcessoView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView
    },
    {
      path: '/',
      component: MainLayout,
      children: [
        { path: '', redirect: '/dashboard' },
        { path: 'dashboard', name: 'dashboard', component: DashboardView },
        { path: 'pacientes', name: 'pacientes', component: PacientesView },
        { path: 'procedimentos', name: 'procedimentos', component: ProcedimentosView },
        { path: 'orcamentos', name: 'orcamentos', component: OrcamentosView },
        { path: 'receber', name: 'receber', component: ContasReceberView },
        { path: 'financeiro', name: 'financeiro', component: FinanceiroView },
        { path: 'agenda', name: 'agenda', component: AgendaView },
        { path: 'estoque', name: 'estoque', component: EstoqueView },
        
        { 
          path: 'meus-atendimentos', 
          name: 'meus-atendimentos', 
          component: MinhaAgendaView 
        },
        
        {
          path: 'atendimento/:idConsulta',
          name: 'realizar-atendimento',
          component: RealizarAtendimentoView
        },

        { 
          path: 'funcionarios', 
          name: 'funcionarios', 
          component: FuncionariosView, 
          meta: { requiresAdmin: true } 
        },
        { 
          path: 'acessos', 
          name: 'acessos', 
          component: AcessoView, 
          meta: { requiresAdmin: true } 
        }
      ]
    }
  ]
})

router.beforeEach(async (to) => {
  const publicPages = ['/login'];
  const authRequired = !publicPages.includes(to.path);
  const auth = useAuthStore();

  if (authRequired && !auth.token) {
    auth.returnUrl = to.fullPath;
    return '/login';
  }
  
  if (to.meta.requiresAdmin && !auth.isAdmin()) {
      alert("Acesso Negado: Área restrita a administradores.");
      return '/dashboard';
  }

  if (auth.token && to.path === '/login') {
      return '/dashboard';
  }
})

export default router
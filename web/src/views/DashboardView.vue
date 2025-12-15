<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import { useAuthStore } from '@/stores/auth';

const authStore = useAuthStore();
const loading = ref(true);

const dados = ref({
    nomeUsuario: '',
    totalPacientes: 0,
    consultasHoje: 0,
    proximasConsultas: [],
    faturamentoMes: 0,
    despesasMes: 0,
    saldoMes: 0,
    estoqueBaixo: []
});

const getDataLocal = () => {
    const d = new Date();
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
};

onMounted(async () => {
    await carregarTudo();
});

const carregarTudo = async () => {
    try {
        const emailLogado = authStore.user?.email;
        const mesAtualStr = getDataLocal().slice(0, 7); 

        const [resDash, resFin, resEst, resFunc] = await Promise.all([
            axios.get('http://localhost:5000/api/dashboard/resumo'),
            axios.get('http://localhost:5000/api/financeiro/fluxo-caixa'),
            axios.get('http://localhost:5000/api/estoque/listar'),
            axios.get('http://localhost:5000/api/funcionario/buscartodos')
        ]);

        const dashData = resDash.data.data;
        dados.value.totalPacientes = dashData.totalPacientes;
        dados.value.consultasHoje = dashData.consultasHoje;
        
        dados.value.proximasConsultas = dashData.proximasConsultas.map(c => ({
            ...c,
            data: c.data.endsWith('Z') ? c.data : c.data + 'Z'
        }));

        const movs = resFin.data.data;
        const movsMes = movs.filter(m => m.dataMovimentacao.startsWith(mesAtualStr));
        
        dados.value.faturamentoMes = movsMes.filter(m => m.tipo === 0).reduce((acc, m) => acc + m.valor, 0);
        dados.value.despesasMes = movsMes.filter(m => m.tipo === 1).reduce((acc, m) => acc + m.valor, 0);
        dados.value.saldoMes = dados.value.faturamentoMes - dados.value.despesasMes;

        const itens = resEst.data.data;
        dados.value.estoqueBaixo = itens.filter(i => i.quantidade <= i.estoqueMin);

        const funcionario = resFunc.data.data.find(f => f.email === emailLogado);
        dados.value.nomeUsuario = funcionario ? funcionario.nome.split(' ')[0] : (emailLogado || 'Visitante');

    } catch (e) {
        console.error("Erro ao montar dashboard", e);
    } finally {
        loading.value = false;
    }
};

const formatMoney = (v) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v);
const formatHora = (d) => new Date(d).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
const formatData = (d) => new Date(d).toLocaleDateString('pt-BR');
</script>

<template>
    <div class="page-container">
        <header class="page-header">
            <div class="header-content">
                <h2>Olá, {{ dados.nomeUsuario }}</h2>
                <p class="subtitle">Visão geral do consultório.</p>
            </div>
            <div class="date-badge">
                <i class="bi bi-calendar-event"></i>
                {{ new Date().toLocaleDateString('pt-BR', { weekday: 'long', day: 'numeric', month: 'long' }) }}
            </div>
        </header>

        <div v-if="loading" class="loading-state">
            Carregando indicadores...
        </div>

        <div v-else>
            
            <div class="stats-row">
                
                <div class="stat-card blue">
                    <div class="icon"><i class="bi bi-calendar-check-fill"></i></div>
                    <div class="info">
                        <h3>{{ dados.consultasHoje }}</h3>
                        <p>Consultas Hoje</p>
                    </div>
                </div>

                <div class="stat-card green">
                    <div class="icon"><i class="bi bi-graph-up-arrow"></i></div>
                    <div class="info">
                        <h3>{{ formatMoney(dados.faturamentoMes) }}</h3>
                        <p>Entradas (Mês)</p>
                    </div>
                </div>

                <div class="stat-card red">
                    <div class="icon"><i class="bi bi-graph-down-arrow"></i></div>
                    <div class="info">
                        <h3>{{ formatMoney(dados.despesasMes) }}</h3>
                        <p>Saídas (Mês)</p>
                    </div>
                </div>

                <div class="stat-card" :class="dados.saldoMes >= 0 ? 'teal' : 'orange'">
                    <div class="icon"><i class="bi bi-wallet2"></i></div>
                    <div class="info">
                        <h3>{{ formatMoney(dados.saldoMes) }}</h3>
                        <p>Saldo Líquido</p>
                    </div>
                </div>

            </div>

            <div class="panels-row">
                
                <div class="card-panel agenda-panel">
                    <div class="panel-header">
                        <h3><i class="bi bi-clock"></i> Próximos Atendimentos</h3>
                        <router-link to="/agenda" class="link-more">
                            Agenda Completa <i class="bi bi-arrow-right"></i>
                        </router-link>
                    </div>
                    
                    <table class="tabela-dash">
                        <thead>
                            <tr>
                                <th>Horário</th>
                                <th>Data</th>
                                <th>Paciente</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="c in dados.proximasConsultas" :key="c.idConsulta">
                                <td class="bold">{{ formatHora(c.data) }}</td>
                                <td>{{ formatData(c.data) }}</td>
                                <td>{{ c.paciente }}</td>
                            </tr>
                            <tr v-if="dados.proximasConsultas.length === 0">
                                <td colspan="3" class="text-center">Nenhuma consulta futura agendada.</td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <div class="card-panel stock-panel">
                    <div class="panel-header">
                        <h3><i class="bi bi-box-seam"></i> Alerta de Estoque</h3>
                        <router-link to="/estoque" class="link-more">Gerenciar</router-link>
                    </div>

                    <ul class="stock-list" v-if="dados.estoqueBaixo.length > 0">
                        <li v-for="(item, i) in dados.estoqueBaixo" :key="i">
                            <span class="item-name">{{ item.descricao }}</span>
                            <span class="item-qtd critico">{{ item.quantidade }} un</span>
                        </li>
                    </ul>
                    <div v-else class="empty-stock">
                        <i class="bi bi-check-circle"></i> Tudo certo no estoque!
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.page-container { max-width: 1200px; margin: 0 auto; }

.page-header { 
    display: flex; justify-content: space-between; align-items: center; 
    margin-bottom: 2rem; padding-bottom: 1rem; border-bottom: 1px solid var(--border-color); 
}
h2 { margin: 0; color: var(--text-primary); font-size: 1.5rem; }
.subtitle { margin: 0.2rem 0 0; color: var(--text-secondary); }
.date-badge { 
    background: var(--bg-secondary); padding: 0.5rem 1rem; 
    border-radius: 20px; font-weight: 500; color: var(--text-secondary); 
    border: 1px solid var(--border-color); text-transform: capitalize;
    display: flex; align-items: center; gap: 0.5rem;
}

.stats-row {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 1.5rem;
    margin-bottom: 2rem;
}

.stat-card {
    background: var(--bg-secondary); padding: 1.5rem; border-radius: 12px;
    display: flex; align-items: center; gap: 1rem;
    box-shadow: var(--shadow-sm); border: 1px solid var(--border-color);
    transition: transform 0.2s;
    min-width: 0;
}
.stat-card:hover { transform: translateY(-3px); }

.stat-card .icon { 
    width: 50px; height: 50px; border-radius: 10px; flex-shrink: 0;
    display: flex; justify-content: center; align-items: center; 
    font-size: 1.5rem; 
}
.stat-card.blue .icon { background: #e3f2fd; color: #3498db; }
.stat-card.green .icon { background: #e8f5e9; color: #27ae60; }
.stat-card.red .icon { background: #ffebee; color: #c0392b; }
.stat-card.teal .icon { background: #e0f2f1; color: #009688; }
.stat-card.orange .icon { background: #fff3e0; color: #e67e22; }

.info { overflow: hidden; }
.info h3 { 
    margin: 0; font-size: 1.3rem; color: var(--text-primary); 
    white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}
.info p { margin: 0; color: var(--text-secondary); font-size: 0.9rem; margin-top: 0.2rem; }

.panels-row { 
    display: grid; grid-template-columns: 2fr 1fr; gap: 1.5rem; 
}

.card-panel {
    background: var(--bg-secondary); border-radius: 12px;
    border: 1px solid var(--border-color); box-shadow: var(--shadow-sm);
    padding: 1.5rem;
    height: 100%;
}
.panel-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
.panel-header h3 { margin: 0; color: var(--text-primary); font-size: 1.1rem; display: flex; align-items: center; gap: 0.5rem; }
.link-more { color: var(--accent-color); text-decoration: none; font-weight: 500; font-size: 0.9rem; display: flex; align-items: center; gap: 0.3rem; }
.link-more:hover { text-decoration: underline; }

.tabela-dash { width: 100%; border-collapse: collapse; }
.tabela-dash th { text-align: left; padding: 0.8rem; color: var(--text-secondary); font-size: 0.85rem; border-bottom: 2px solid var(--border-color); }
.tabela-dash td { padding: 0.8rem; color: var(--text-primary); border-bottom: 1px solid var(--border-color); }
.bold { font-weight: bold; color: var(--accent-color); }
.text-center { text-align: center; color: var(--text-secondary); font-style: italic; }

.stock-list { list-style: none; padding: 0; margin: 0; }
.stock-list li { 
    display: flex; justify-content: space-between; align-items: center; 
    padding: 0.8rem 0; border-bottom: 1px solid var(--border-color); 
}
.stock-list li:last-child { border-bottom: none; }
.item-qtd.critico { 
    background-color: #ffebee; color: #c0392b; 
    padding: 2px 8px; border-radius: 12px; font-size: 0.85rem; font-weight: bold; 
}
.empty-stock { 
    text-align: center; color: var(--success-color); padding: 2rem 0; 
    display: flex; flex-direction: column; align-items: center; gap: 0.5rem; font-weight: 500;
}
.empty-stock i { font-size: 1.5rem; }

.loading-state { text-align: center; padding: 3rem; color: var(--text-secondary); font-size: 1.2rem; }

@media (max-width: 1000px) {
    .stats-row { grid-template-columns: 1fr 1fr; }
    .panels-row { grid-template-columns: 1fr; }
}

@media (max-width: 600px) {
    .page-header { flex-direction: column; align-items: flex-start; gap: 1rem; }
    .stats-row { grid-template-columns: 1fr; }
}
</style>
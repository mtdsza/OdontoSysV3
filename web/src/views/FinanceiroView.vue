<script setup>
import { ref, onMounted, computed, watch } from 'vue';
import axios from 'axios';

const movimentacoes = ref([]);
const loading = ref(false);
const error = ref('');
const showModal = ref(false);

const dataAtual = ref(new Date()); 
const filtroMes = ref(dataAtual.value.toISOString().slice(0, 7));

const formDespesa = ref({
    descricao: '',
    valor: 0
});

onMounted(() => {
    carregarFluxo();
});

watch(dataAtual, (novaData) => {
    filtroMes.value = novaData.toISOString().slice(0, 7);
});

const navegarMes = (delta) => {
    const nova = new Date(dataAtual.value);
    nova.setMonth(nova.getMonth() + delta);
    dataAtual.value = nova;
};

const carregarFluxo = async () => {
    loading.value = true;
    try {
        const response = await axios.get('http://localhost:5000/api/financeiro/fluxo-caixa');
        movimentacoes.value = response.data.data;
    } catch (err) {
        error.value = "Erro ao carregar fluxo.";
    } finally {
        loading.value = false;
    }
};

const registrarDespesa = async () => {
    if (!formDespesa.value.descricao || formDespesa.value.valor <= 0) return;

    try {
        const dto = {
            descricao: formDespesa.value.descricao,
            valor: formDespesa.value.valor,
            tipo: 1
        };
        await axios.post('http://localhost:5000/api/financeiro/registrar-despesa', dto);
        alert("Despesa registrada!");
        showModal.value = false;
        formDespesa.value = { descricao: '', valor: 0 };
        carregarFluxo();
    } catch (err) { alert("Erro ao registrar."); }
};

const imprimirRelatorio = () => { window.print(); };

const mesExtenso = computed(() => {
    return dataAtual.value.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
});

const movimentacoesFiltradas = computed(() => {
    if (!filtroMes.value) return movimentacoes.value;
    return movimentacoes.value.filter(m => m.dataMovimentacao.startsWith(filtroMes.value));
});

const totalEntradas = computed(() => 
    movimentacoesFiltradas.value.filter(m => m.tipo === 0).reduce((acc, m) => acc + m.valor, 0)
);

const totalSaidas = computed(() => 
    movimentacoesFiltradas.value.filter(m => m.tipo === 1).reduce((acc, m) => acc + m.valor, 0)
);

const saldo = computed(() => totalEntradas.value - totalSaidas.value);

const formatCurrency = (val) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val);

const formatDate = (dateStr) => {
    const iso = dateStr.endsWith('Z') ? dateStr : dateStr + 'Z';
    return new Date(iso).toLocaleDateString('pt-BR') + ' ' + new Date(iso).toLocaleTimeString('pt-BR', {hour: '2-digit', minute:'2-digit'});
};
</script>

<template>
    <div class="page-container">
        
        <header class="page-header">
            <div class="header-title">
                <h2>Financeiro</h2>
                <p class="subtitle">Gestão de Caixa</p>
            </div>
            
            <div class="month-navigator">
                <button class="btn-nav" @click="navegarMes(-1)">
                    <i class="bi bi-chevron-left"></i>
                </button>
                <span class="current-month">{{ mesExtenso }}</span>
                <button class="btn-nav" @click="navegarMes(1)">
                    <i class="bi bi-chevron-right"></i>
                </button>
            </div>

            <div class="header-actions">
                <button class="btn-print" @click="imprimirRelatorio" title="Imprimir Relatório">
                    <i class="bi bi-printer-fill"></i>
                </button>
                <button class="btn-expense" @click="showModal = true">
                    <i class="bi bi-dash-circle"></i> Despesa
                </button>
            </div>
        </header>

        <div class="print-only-header">
            <h1>Relatório Financeiro</h1>
            <p class="periodo">Competência: <strong>{{ mesExtenso }}</strong></p>
            <hr>
        </div>

        <div class="cards-grid">
            <div class="card card-in">
                <h3><i class="bi bi-arrow-down-circle"></i> Entradas</h3>
                <p>{{ formatCurrency(totalEntradas) }}</p>
            </div>
            <div class="card card-out">
                <h3><i class="bi bi-arrow-up-circle"></i> Saídas</h3>
                <p>{{ formatCurrency(totalSaidas) }}</p>
            </div>
            <div class="card card-balance" :class="saldo >= 0 ? 'positive' : 'negative'">
                <h3><i class="bi bi-wallet2"></i> Saldo</h3>
                <p>{{ formatCurrency(saldo) }}</p>
            </div>
        </div>

        <div v-if="loading" class="loading-state">Atualizando dados...</div>

        <div v-if="!loading" class="table-responsive">
            <table class="tabela">
                <thead>
                    <tr>
                        <th>Data</th>
                        <th>Descrição</th>
                        <th>Tipo</th>
                        <th style="text-align: right;">Valor</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="m in movimentacoesFiltradas" :key="m.idMovimentacao">
                        <td>{{ formatDate(m.dataMovimentacao) }}</td>
                        <td>{{ m.descricao }}</td>
                        <td>
                            <span :class="m.tipo === 0 ? 'badge-in' : 'badge-out'">
                                <i class="bi" :class="m.tipo === 0 ? 'bi-arrow-down' : 'bi-arrow-up'"></i>
                                {{ m.tipo === 0 ? 'Entrada' : 'Saída' }}
                            </span>
                        </td>
                        <td class="valor" :class="m.tipo === 0 ? 'text-green' : 'text-red'">
                            {{ m.tipo === 0 ? '+ ' : '- ' }}
                            {{ formatCurrency(m.valor) }}
                        </td>
                    </tr>
                    <tr v-if="movimentacoesFiltradas.length === 0">
                        <td colspan="4" class="text-center">
                            Nenhuma movimentação em <strong>{{ mesExtenso }}</strong>.
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div v-if="showModal" class="modal-overlay">
            <div class="modal">
                <h3>Registrar Saída (Despesa)</h3>
                <form @submit.prevent="registrarDespesa">
                    <label>Descrição</label>
                    <input v-model="formDespesa.descricao" placeholder="Ex: Conta de Luz" required />
                    
                    <label>Valor (R$)</label>
                    <input v-model="formDespesa.valor" type="number" step="0.01" min="0.01" required />

                    <div class="modal-footer">
                        <button type="button" class="btn-sec" @click="showModal = false">Cancelar</button>
                        <button type="submit" class="btn-danger">Confirmar Saída</button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>

<style scoped>
.page-container { max-width: 1000px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; flex-wrap: wrap; gap: 1rem; }
.header-actions { display: flex; gap: 0.8rem; align-items: center; }

h2 { margin: 0; color: var(--text-primary); }
.subtitle { margin: 0.2rem 0 0; color: var(--text-secondary); font-size: 0.9rem; }

.month-navigator {
    display: flex; align-items: center; gap: 1rem;
    background: var(--bg-secondary);
    padding: 0.5rem 1rem;
    border-radius: 8px;
    border: 1px solid var(--border-color);
    box-shadow: var(--shadow-sm);
}
.current-month {
    font-weight: bold; font-size: 1.1rem; text-transform: capitalize; color: var(--text-primary);
    min-width: 140px; text-align: center;
}
.btn-nav {
    background: none; border: none; cursor: pointer; color: var(--text-secondary);
    font-size: 1.1rem; padding: 4px 8px; border-radius: 4px; transition: background 0.2s;
}
.btn-nav:hover { background-color: var(--bg-primary); color: var(--accent-color); }

.print-only-header { display: none; margin-bottom: 2rem; text-align: center; }

.cards-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1.5rem; margin-bottom: 2rem; }
.card { background: var(--bg-secondary); padding: 1.5rem; border-radius: 8px; border: 1px solid var(--border-color); box-shadow: var(--shadow-sm); text-align: center; }
.card h3 { margin: 0 0 0.5rem 0; font-size: 1rem; color: var(--text-secondary); display: flex; align-items: center; justify-content: center; gap: 0.5rem; }
.card p { margin: 0; font-size: 1.8rem; font-weight: bold; }

.card-in p { color: var(--success-color); }
.card-out p { color: var(--danger-color); }
.card-balance.positive p { color: var(--accent-color); }
.card-balance.negative p { color: var(--danger-color); }

.table-responsive { background: var(--bg-secondary); border-radius: 8px; border: 1px solid var(--border-color); box-shadow: var(--shadow-sm); }
.tabela { width: 100%; border-collapse: collapse; }
.tabela th { background: var(--bg-primary); color: var(--text-secondary); padding: 1rem; text-align: left; border-bottom: 2px solid var(--border-color); }
.tabela td { padding: 1rem; border-bottom: 1px solid var(--border-color); color: var(--text-primary); }
.valor { font-family: monospace; font-weight: bold; text-align: right; }
.text-green { color: var(--success-color); }
.text-red { color: var(--danger-color); }
.text-center { text-align: center; padding: 2rem; color: var(--text-secondary); }

.badge-in { background: rgba(39, 174, 96, 0.15); color: var(--success-color); padding: 4px 8px; border-radius: 4px; font-weight: bold; font-size: 0.8rem; display: inline-flex; align-items: center; gap: 4px; }
.badge-out { background: rgba(231, 76, 60, 0.15); color: var(--danger-color); padding: 4px 8px; border-radius: 4px; font-weight: bold; font-size: 0.8rem; display: inline-flex; align-items: center; gap: 4px; }

.btn-expense { background-color: var(--danger-color); color: white; border: none; padding: 0.7rem 1.2rem; border-radius: 6px; cursor: pointer; font-weight: bold; display: flex; align-items: center; gap: 0.5rem; }
.btn-expense:hover { filter: brightness(0.9); }

.btn-print { background-color: var(--bg-secondary); border: 1px solid var(--border-color); color: var(--text-primary); padding: 0.7rem 1.2rem; border-radius: 6px; cursor: pointer; font-weight: bold; }
.btn-print:hover { background-color: var(--bg-primary); }

.modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); display: flex; justify-content: center; align-items: center; z-index: 1000; }
.modal { background: var(--bg-secondary); padding: 2rem; border-radius: 8px; width: 400px; border: 1px solid var(--border-color); color: var(--text-primary); }
input { width: 100%; padding: 0.7rem; background: var(--bg-primary); border: 1px solid var(--border-color); color: var(--text-primary); border-radius: 4px; margin-bottom: 1rem; }
label { display: block; margin-bottom: 0.5rem; }
.modal-footer { display: flex; justify-content: flex-end; gap: 1rem; }
.btn-danger { background: var(--danger-color); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; }
.btn-sec { background: var(--text-secondary); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; }

@media (max-width: 650px) {
    .page-header { flex-direction: column; align-items: stretch; gap: 1rem; }
    .header-actions { justify-content: space-between; }
    .month-navigator { order: 3; width: 100%; justify-content: space-between; }
}
</style>

<style>
@media print {
    aside, .sidebar, .btn-print, .btn-expense, .modal-overlay, .month-navigator {
        display: none !important;
    }

    .content-area {
        margin-left: 0 !important;
        padding: 0 !important;
        background: white !important;
        color: black !important;
    }

    .page-container {
        max-width: 100% !important;
    }

    .print-only-header {
        display: block !important;
        text-align: center;
        margin-bottom: 2rem;
    }
    .print-only-header h1 { font-size: 1.5rem; margin-bottom: 0.5rem; }
    .print-only-header .periodo { font-size: 1.1rem; text-transform: capitalize; }

    .table-responsive {
        box-shadow: none !important;
        border: 1px solid #ddd !important;
    }
    
    .tabela th { 
        background-color: #f0f0f0 !important; 
        color: black !important;
        -webkit-print-color-adjust: exact;
    }

    .card {
        box-shadow: none !important;
        border: 1px solid #000 !important;
    }
}
</style>
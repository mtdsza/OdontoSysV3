<script setup>
import { ref, watch, onMounted } from 'vue';
import axios from 'axios';

const pacientes = ref([]);
const orcamentosAprovados = ref([]);
const parcelas = ref([]);
const loading = ref(false);
const pacienteSelecionado = ref('');
const orcamentoSelecionado = ref(null);
const showModal = ref(false);
const formGerar = ref({
    quantidadeParcelas: 1,
    primeiroVencimento: new Date().toISOString().split('T')[0]
});

onMounted(() => {
    carregarPacientes();
});

const carregarPacientes = async () => {
    try {
        const res = await axios.get('http://localhost:5000/api/paciente/buscartodos');
        pacientes.value = res.data.data;
    } catch (e) { console.error("Erro ao carregar pacientes"); }
};

watch(pacienteSelecionado, async (newId) => {
    orcamentoSelecionado.value = null;
    parcelas.value = [];
    if (!newId) {
        orcamentosAprovados.value = [];
        return;
    }
    
    loading.value = true;
    try {
        const res = await axios.get(`http://localhost:5000/api/orcamento/por-paciente/${newId}`);
        orcamentosAprovados.value = res.data.data.filter(o => o.aprovado);
    } catch (e) { alert("Erro ao buscar orçamentos"); }
    finally { loading.value = false; }
});

const selecionarOrcamento = async (orcamento) => {
    orcamentoSelecionado.value = orcamento;
    carregarParcelas();
};

const carregarParcelas = async () => {
    if(!orcamentoSelecionado.value) return;
    
    loading.value = true;
    try {
        const res = await axios.get(`http://localhost:5000/api/financeiro/parcelas/${orcamentoSelecionado.value.idOrcamento}`);
        parcelas.value = res.data.data;
    } catch (e) { alert("Erro ao carregar parcelas"); }
    finally { loading.value = false; }
};

const abrirGerarParcelas = () => {
    formGerar.value = {
        quantidadeParcelas: 1,
        primeiroVencimento: new Date().toISOString().split('T')[0]
    };
    showModal.value = true;
};

const confirmarGeracao = async () => {
    if(formGerar.value.quantidadeParcelas < 1) return;

    try {
        const dto = {
            idOrcamento: orcamentoSelecionado.value.idOrcamento,
            quantidadeParcelas: formGerar.value.quantidadeParcelas,
            primeiroVencimento: formGerar.value.primeiroVencimento
        };
        await axios.post('http://localhost:5000/api/financeiro/gerar-parcelas', dto);
        alert("Parcelas geradas com sucesso!");
        showModal.value = false;
        carregarParcelas();
    } catch (err) {
        alert("Erro: " + (err.response?.data?.message || "Falha ao gerar"));
    }
};

const pagarParcela = async (idParcela) => {
    if(!confirm("Confirmar recebimento desta parcela? O valor entrará no caixa.")) return;

    try {
        await axios.post(`http://localhost:5000/api/financeiro/pagar-parcela/${idParcela}`);
        alert("Pagamento registrado!");
        carregarParcelas();
    } catch (err) {
        alert("Erro ao registrar pagamento.");
    }
};

const formatMoney = (v) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v);
const formatDate = (d) => {
    if(!d) return '-';
    const data = new Date(d + 'T00:00:00');
    return data.toLocaleDateString('pt-BR');
};
</script>

<template>
    <div class="page-container">
        <header class="page-header">
            <div class="header-title">
                <h2>Contas a Receber</h2>
                <p class="subtitle">Gestão de Parcelas e Pagamentos</p>
            </div>
        </header>

        <div class="search-card">
            <label>Selecione o Paciente:</label>
            <select v-model="pacienteSelecionado">
                <option value="">-- Buscar Paciente --</option>
                <option v-for="p in pacientes" :key="p.idPaciente" :value="p.idPaciente">
                    {{ p.nome }} (CPF: {{ p.cpf }})
                </option>
            </select>
        </div>

        <div v-if="loading" class="loading-state">Carregando dados...</div>

        <div class="split-view" v-if="pacienteSelecionado && !loading">
            <div class="panel-left">
                <h3>Orçamentos Aprovados</h3>
                <div v-if="orcamentosAprovados.length === 0" class="empty-list">
                    Nenhum orçamento aprovado encontrado para este paciente.
                </div>
                
                <div 
                    v-for="orc in orcamentosAprovados" 
                    :key="orc.idOrcamento"
                    class="orcamento-item"
                    :class="{ active: orcamentoSelecionado?.idOrcamento === orc.idOrcamento }"
                    @click="selecionarOrcamento(orc)"
                >
                    <div class="orc-header">
                        <span class="orc-id">Orçamento #{{ orc.idOrcamento }}</span>
                        <span class="orc-date">{{ formatDate(orc.dataEmissao) }}</span>
                    </div>
                    <div class="orc-total">{{ formatMoney(orc.valorTotal) }}</div>
                    <small>Clique para ver as parcelas</small>
                </div>
            </div>

            <div class="panel-right">
                <div v-if="!orcamentoSelecionado" class="empty-state">
                    <i class="bi bi-arrow-left"></i> Selecione um orçamento ao lado para gerenciar o pagamento.
                </div>

                <div v-else>
                    <div class="panel-header">
                        <h3>Parcelas do Orçamento #{{ orcamentoSelecionado.idOrcamento }}</h3>
                        
                        <button v-if="parcelas.length === 0" class="btn-generate" @click="abrirGerarParcelas">
                            <i class="bi bi-gear-fill"></i> Gerar Carnê/Parcelas
                        </button>
                    </div>

                    <div v-if="parcelas.length > 0" class="table-responsive">
                        <table class="tabela">
                            <thead>
                                <tr>
                                    <th>Descrição</th>
                                    <th>Vencimento</th>
                                    <th>Valor</th>
                                    <th>Status</th>
                                    <th>Ação</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="par in parcelas" :key="par.idParcela">
                                    <td>{{ par.descricao }}</td>
                                    <td>{{ formatDate(par.dataVencimento) }}</td>
                                    <td class="valor">{{ formatMoney(par.valor) }}</td>
                                    <td>
                                        <span class="tag" :class="{
                                            'tag-paid': par.status === 1,
                                            'tag-pending': par.status === 0,
                                            'tag-overdue': par.status === 2
                                        }">
                                            {{ par.status === 1 ? 'Pago' : (par.status === 2 ? 'Vencido' : 'Pendente') }}
                                        </span>
                                    </td>
                                    <td>
                                        <button 
                                            v-if="par.status !== 1" 
                                            class="btn-pay" 
                                            @click="pagarParcela(par.idParcela)"
                                            title="Receber Pagamento"
                                        >
                                            <i class="bi bi-cash-coin"></i> Receber
                                        </button>
                                        <span v-else class="text-muted">
                                            <i class="bi bi-check-lg"></i>
                                        </span>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                    <div v-else-if="orcamentoSelecionado" class="alert-box">
                        Este orçamento ainda não possui parcelas geradas. Clique no botão acima para configurar as condições de pagamento.
                    </div>
                </div>
            </div>
        </div>

        <div v-if="showModal" class="modal-overlay">
            <div class="modal">
                <h3>Gerar Condições de Pagamento</h3>
                <form @submit.prevent="confirmarGeracao">
                    <label>Número de Parcelas</label>
                    <select v-model="formGerar.quantidadeParcelas">
                        <option v-for="n in 12" :key="n" :value="n">{{ n }}x</option>
                        <option value="18">18x</option>
                        <option value="24">24x</option>
                    </select>

                    <label>Vencimento da 1ª Parcela</label>
                    <input type="date" v-model="formGerar.primeiroVencimento" required />

                    <div class="modal-footer">
                        <button type="button" class="btn-sec" @click="showModal = false">Cancelar</button>
                        <button type="submit" class="btn-pri">Gerar Parcelas</button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>

<style scoped>
.page-container { max-width: 1200px; margin: 0 auto; }
.page-header { margin-bottom: 1.5rem; }
h2 { margin: 0; color: var(--text-primary); }
.subtitle { margin: 0.2rem 0 0; color: var(--text-secondary); font-size: 0.9rem; }

.search-card { background: var(--bg-secondary); padding: 1.5rem; border-radius: 8px; border: 1px solid var(--border-color); margin-bottom: 1.5rem; }
select, input { width: 100%; padding: 0.7rem; background: var(--bg-primary); border: 1px solid var(--border-color); color: var(--text-primary); border-radius: 4px; }

.split-view { display: grid; grid-template-columns: 320px 1fr; gap: 1.5rem; align-items: start; }

.panel-left { background: var(--bg-secondary); border-radius: 8px; border: 1px solid var(--border-color); padding: 1rem; max-height: 600px; overflow-y: auto; }
.panel-left h3 { font-size: 1rem; color: var(--text-secondary); margin-top: 0; margin-bottom: 1rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem; }

.orcamento-item { padding: 1rem; border: 1px solid var(--border-color); border-radius: 6px; margin-bottom: 0.8rem; cursor: pointer; transition: all 0.2s; background: var(--bg-primary); }
.orcamento-item:hover { transform: translateX(3px); border-color: var(--accent-color); }
.orcamento-item.active { background-color: var(--accent-color); color: white; border-color: var(--accent-color); }
.orcamento-item.active .orc-total, .orcamento-item.active .orc-id, .orcamento-item.active small { color: rgba(255,255,255,0.9); }

.orc-header { display: flex; justify-content: space-between; font-size: 0.85rem; margin-bottom: 0.5rem; }
.orc-total { font-size: 1.2rem; font-weight: bold; color: var(--success-color); }
small { display: block; font-size: 0.75rem; margin-top: 0.5rem; color: var(--text-secondary); }

.panel-right { background: var(--bg-secondary); border-radius: 8px; border: 1px solid var(--border-color); padding: 1.5rem; min-height: 300px; }
.panel-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
.panel-header h3 { margin: 0; color: var(--text-primary); }

.btn-generate { background-color: var(--accent-color); color: white; border: none; padding: 0.6rem 1.2rem; border-radius: 6px; cursor: pointer; font-weight: bold; }
.btn-generate:hover { filter: brightness(1.1); }

.empty-state { text-align: center; color: var(--text-secondary); padding: 3rem; border: 2px dashed var(--border-color); border-radius: 8px; font-size: 1.1rem; }
.alert-box { background: rgba(243, 156, 18, 0.1); color: #e67e22; padding: 2rem; border-radius: 6px; text-align: center; border: 1px solid rgba(243, 156, 18, 0.3); }

.table-responsive { overflow-x: auto; }
.tabela { width: 100%; border-collapse: collapse; }
.tabela th { text-align: left; padding: 0.8rem; border-bottom: 2px solid var(--border-color); color: var(--text-secondary); }
.tabela td { padding: 0.8rem; border-bottom: 1px solid var(--border-color); color: var(--text-primary); }
.valor { font-family: monospace; font-weight: bold; }

.tag { padding: 4px 8px; border-radius: 4px; font-size: 0.8rem; font-weight: bold; }
.tag-paid { background: rgba(39, 174, 96, 0.15); color: var(--success-color); }
.tag-pending { background: rgba(243, 156, 18, 0.15); color: #f39c12; }
.tag-overdue { background: rgba(231, 76, 60, 0.15); color: var(--danger-color); }

.btn-pay { background: var(--success-color); color: white; border: none; padding: 0.4rem 0.8rem; border-radius: 4px; cursor: pointer; font-size: 0.85rem; }
.btn-pay:hover { filter: brightness(1.1); }
.text-muted { color: var(--success-color); font-weight: bold; font-size: 1.2rem; }

.modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); display: flex; justify-content: center; align-items: center; z-index: 1000; }
.modal { background: var(--bg-secondary); padding: 2rem; border-radius: 8px; width: 350px; border: 1px solid var(--border-color); color: var(--text-primary); }
label { display: block; margin-bottom: 0.5rem; margin-top: 1rem; }
.modal-footer { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1.5rem; }
.btn-pri { background: var(--success-color); color: white; border: none; padding: 0.6rem 1.2rem; border-radius: 4px; cursor: pointer; font-weight: bold; }
.btn-sec { background: var(--text-secondary); color: white; border: none; padding: 0.6rem 1.2rem; border-radius: 4px; cursor: pointer; }

@media (max-width: 900px) {
    .split-view { grid-template-columns: 1fr; }
    .panel-left { max-height: 250px; margin-bottom: 1rem; }
}
</style>
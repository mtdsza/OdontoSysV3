<script setup>
import { ref, onMounted, computed, watch } from 'vue';
import axios from 'axios';

const pacientes = ref([]);
const dentistas = ref([]);
const procedimentos = ref([]);
const orcamentosDoPaciente = ref([]);

const loading = ref(false);
const pacienteSelecionado = ref('');
const showModal = ref(false);
const error = ref('');

const form = ref({
    idPaciente: '',
    idDentista: '',
    itens: []
});

const itemAtual = ref({
    idProcedimento: '',
    quantidade: 1
});

onMounted(() => {
    carregarDadosIniciais();
});

const carregarDadosIniciais = async () => {
    loading.value = true;
    try {
        const [resPacientes, resDentistas, resProcs] = await Promise.all([
            axios.get('http://localhost:5000/api/paciente/buscartodos'),
            axios.get('http://localhost:5000/api/dentista/buscartodos'),
            axios.get('http://localhost:5000/api/procedimento/buscartodos')
        ]);
        
        pacientes.value = resPacientes.data.data;
        dentistas.value = resDentistas.data.data;
        procedimentos.value = resProcs.data.data;
    } catch (err) {
        error.value = "Erro ao carregar cadastros básicos.";
    } finally {
        loading.value = false;
    }
};

watch(pacienteSelecionado, async (novoId) => {
    if (novoId) {
        await carregarOrcamentos(novoId);
    } else {
        orcamentosDoPaciente.value = [];
    }
});

const carregarOrcamentos = async (idPaciente) => {
    loading.value = true;
    try {
        const response = await axios.get(`http://localhost:5000/api/orcamento/por-paciente/${idPaciente}`);
        orcamentosDoPaciente.value = response.data.data;
    } catch (err) {
        error.value = "Erro ao buscar orçamentos do paciente.";
    } finally {
        loading.value = false;
    }
};


const abrirNovoOrcamento = () => {
    if (!pacienteSelecionado.value) {
        alert("Selecione um paciente na lista acima primeiro.");
        return;
    }
    
    form.value = {
        idPaciente: pacienteSelecionado.value,
        idDentista: '',
        itens: []
    };
    itemAtual.value = { idProcedimento: '', quantidade: 1 };
    showModal.value = true;
};

const adicionarItem = () => {
    if (!itemAtual.value.idProcedimento || itemAtual.value.quantidade < 1) return;

    const proc = procedimentos.value.find(p => p.idProcedimento === itemAtual.value.idProcedimento);
    if (!proc) return;

    form.value.itens.push({
        idProcedimento: proc.idProcedimento,
        nome: proc.nome,
        valorUnitario: proc.valorPadrao,
        quantidade: itemAtual.value.quantidade,
        total: proc.valorPadrao * itemAtual.value.quantidade
    });

    itemAtual.value = { idProcedimento: '', quantidade: 1 };
};

const removerItem = (index) => {
    form.value.itens.splice(index, 1);
};

const atualizarTotalItem = (item) => {
    if(item.quantidade < 1) item.quantidade = 1;
    if(item.valorUnitario < 0) item.valorUnitario = 0;
    item.total = item.valorUnitario * item.quantidade;
};

const totalOrcamento = computed(() => {
    return form.value.itens.reduce((acc, item) => acc + item.total, 0);
});

const salvarOrcamento = async () => {
    if (!form.value.idDentista) {
        alert("Selecione o dentista responsável.");
        return;
    }
    if (form.value.itens.length === 0) {
        alert("Adicione pelo menos um procedimento.");
        return;
    }

    try {
        const dto = {
            idPaciente: form.value.idPaciente,
            idDentista: form.value.idDentista,
            itens: form.value.itens.map(i => ({
                idProcedimento: i.idProcedimento,
                quantidade: i.quantidade,
                valorUnitario: i.valorUnitario 
            }))
        };

        await axios.post('http://localhost:5000/api/orcamento/criar', dto);
        alert("Orçamento criado com sucesso!");
        showModal.value = false;
        carregarOrcamentos(form.value.idPaciente);
    } catch (err) {
        alert("Erro ao criar orçamento: " + (err.response?.data?.message || err.message));
    }
};

const aprovarOrcamento = async (id) => {
    if (!confirm("Deseja aprovar este orçamento? Isso pode gerar parcelas financeiras futuramente.")) return;
    
    try {
        await axios.put(`http://localhost:5000/api/orcamento/aprovar/${id}`);
        alert("Orçamento Aprovado!");
        carregarOrcamentos(pacienteSelecionado.value);
    } catch (err) {
        alert("Erro ao aprovar: " + (err.response?.data?.message || err.message));
    }
};

const excluirOrcamento = async (id) => {
    if (!confirm("Tem certeza que deseja excluir?")) return;
    try {
        await axios.delete(`http://localhost:5000/api/orcamento/${id}`);
        carregarOrcamentos(pacienteSelecionado.value);
    } catch (err) {
        alert("Erro ao excluir. (Orçamentos aprovados não podem ser excluídos)");
    }
};

const formatCurrency = (value) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
};

const formatDate = (dateStr) => {
    if(!dateStr) return '-';
    const data = new Date(dateStr + 'T00:00:00'); 
    return data.toLocaleDateString('pt-BR');
};
</script>

<template>
    <div class="page-container">
        <header class="page-header">
            <div class="header-title">
                <h2>Orçamentos</h2>
                <p class="subtitle">Emissão e Aprovação de Tratamentos</p>
            </div>
            <button class="btn-primary" @click="abrirNovoOrcamento" :disabled="!pacienteSelecionado">
                <i class="bi bi-plus-lg"></i> Novo Orçamento
            </button>
        </header>

        <div class="filter-card">
            <label>Selecione o Paciente para ver os orçamentos:</label>
            <select v-model="pacienteSelecionado">
                <option value="">-- Selecione --</option>
                <option v-for="p in pacientes" :key="p.idPaciente" :value="p.idPaciente">
                    {{ p.nome }} (CPF: {{ p.cpf }})
                </option>
            </select>
        </div>

        <div v-if="loading" class="loading-state">Carregando...</div>
        <div v-if="error" class="error-state">{{ error }}</div>

        <div v-if="pacienteSelecionado && !loading" class="table-responsive">
            <table class="tabela">
                <thead>
                    <tr>
                        <th>Nº</th>
                        <th>Data Emissão</th>
                        <th>Valor Total</th>
                        <th>Status</th>
                        <th style="text-align: right;">Ações</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="orc in orcamentosDoPaciente" :key="orc.idOrcamento">
                        <td>#{{ orc.idOrcamento }}</td>
                        <td>{{ formatDate(orc.dataEmissao) }}</td>
                        <td class="valor">{{ formatCurrency(orc.valorTotal) }}</td>
                        <td>
                            <span :class="orc.aprovado ? 'tag-success' : 'tag-warning'">
                                {{ orc.aprovado ? 'Aprovado' : 'Pendente' }}
                            </span>
                        </td>
                        <td class="actions-cell">
                            <button v-if="!orc.aprovado" class="btn-icon btn-check" @click="aprovarOrcamento(orc.idOrcamento)" title="Aprovar">
                                <i class="bi bi-check-lg"></i>
                            </button>
                            <button class="btn-icon btn-del" @click="excluirOrcamento(orc.idOrcamento)" title="Excluir">
                                <i class="bi bi-trash3"></i>
                            </button>
                        </td>
                    </tr>
                    <tr v-if="orcamentosDoPaciente.length === 0">
                        <td colspan="5" class="text-center">Nenhum orçamento encontrado para este paciente.</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div v-if="!pacienteSelecionado" class="empty-state">
            Selecione um paciente acima para começar.
        </div>

        <div v-if="showModal" class="modal-overlay">
            <div class="modal large-modal">
                <div class="modal-header">
                    <h3>Novo Orçamento</h3>
                    <button class="btn-close" @click="showModal = false">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>
                
                <div class="modal-body">
                    <div class="form-row">
                        <label>Dentista Responsável</label>
                        <select v-model="form.idDentista">
                            <option value="">-- Selecione --</option>
                            <option v-for="d in dentistas" :key="d.idDentista" :value="d.idDentista">
                                {{ d.nome }} (CRO: {{ d.cro }})
                            </option>
                        </select>
                    </div>

                    <hr class="divider"/>

                    <div class="add-items-row">
                        <div class="flex-grow">
                            <label>Adicionar Procedimento</label>
                            <select v-model="itemAtual.idProcedimento">
                                <option value="">-- Selecione o serviço --</option>
                                <option v-for="proc in procedimentos" :key="proc.idProcedimento" :value="proc.idProcedimento">
                                    {{ proc.nome }} - {{ formatCurrency(proc.valorPadrao) }}
                                </option>
                            </select>
                        </div>
                        <div class="w-100">
                            <label>Qtd</label>
                            <input type="number" v-model="itemAtual.quantidade" min="1" />
                        </div>
                        <div class="btn-align">
                            <button type="button" class="btn-secondary" @click="adicionarItem">
                                <i class="bi bi-plus-lg"></i> Adicionar
                            </button>
                        </div>
                    </div>

                    <table class="tabela-itens">
                        <thead>
                            <tr>
                                <th width="40%">Serviço</th>
                                <th width="15%">Qtd</th>
                                <th width="20%">Valor Unit. (R$)</th>
                                <th width="20%">Total</th>
                                <th width="5%"></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="(item, idx) in form.itens" :key="idx">
                                <td>{{ item.nome }}</td>
                                <td>
                                    <input 
                                        type="number" 
                                        v-model="item.quantidade" 
                                        min="1" 
                                        class="input-table"
                                        @input="atualizarTotalItem(item)"
                                    />
                                </td>
                                <td>
                                    <input 
                                        type="number" 
                                        v-model="item.valorUnitario" 
                                        step="0.01" 
                                        class="input-table"
                                        @input="atualizarTotalItem(item)"
                                    />
                                </td>
                                <td>{{ formatCurrency(item.total) }}</td>
                                <td>
                                    <button class="btn-icon-small" @click="removerItem(idx)">
                                        <i class="bi bi-trash"></i>
                                    </button>
                                </td>
                            </tr>
                            <tr v-if="form.itens.length === 0">
                                <td colspan="5" class="text-center-small">Nenhum item adicionado.</td>
                            </tr>
                        </tbody>
                    </table>

                    <div class="total-row">
                        <span>Total do Orçamento:</span>
                        <span class="total-value">{{ formatCurrency(totalOrcamento) }}</span>
                    </div>
                </div>

                <div class="modal-footer">
                    <button class="btn-cancel" @click="showModal = false">Cancelar</button>
                    <button class="btn-save" @click="salvarOrcamento">
                        <i class="bi bi-floppy"></i> Salvar Orçamento
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.page-container { max-width: 1000px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
h2 { margin: 0; color: var(--text-primary); }
.subtitle { margin: 0.2rem 0 0; color: var(--text-secondary); font-size: 0.9rem; }

.filter-card {
    background: var(--bg-secondary);
    padding: 1.5rem;
    border-radius: 8px;
    border: 1px solid var(--border-color);
    margin-bottom: 2rem;
    box-shadow: var(--shadow-sm);
}
.filter-card select { margin-top: 0.5rem; margin-bottom: 0; }

.table-responsive {
    background: var(--bg-secondary);
    border-radius: 8px;
    box-shadow: var(--shadow-sm);
    border: 1px solid var(--border-color);
}
.tabela { width: 100%; border-collapse: collapse; }
.tabela th {
    background-color: var(--bg-primary);
    color: var(--text-secondary);
    padding: 1rem;
    text-align: left;
    border-bottom: 2px solid var(--border-color);
}
.tabela td { padding: 1rem; border-bottom: 1px solid var(--border-color); color: var(--text-primary); }
.valor { font-family: monospace; font-weight: bold; }
.actions-cell { text-align: right; }

.tag-success { background: rgba(39, 174, 96, 0.15); color: var(--success-color); padding: 4px 8px; border-radius: 4px; font-weight: bold; font-size: 0.85rem; }
.tag-warning { background: rgba(243, 156, 18, 0.15); color: #f39c12; padding: 4px 8px; border-radius: 4px; font-weight: bold; font-size: 0.85rem; }

.btn-primary { background-color: var(--accent-color); color: white; border: none; padding: 0.8rem 1.5rem; border-radius: 6px; cursor: pointer; font-weight: bold; }
.btn-primary:disabled { background-color: var(--border-color); cursor: not-allowed; }
.btn-icon { background: none; border: none; cursor: pointer; font-size: 1.2rem; transition: transform 0.1s; }
.btn-icon:hover { transform: scale(1.1); }

.empty-state { text-align: center; padding: 3rem; color: var(--text-secondary); font-size: 1.2rem; border: 2px dashed var(--border-color); border-radius: 8px; margin-top: 2rem; }

.modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); display: flex; justify-content: center; align-items: center; z-index: 1000; }
.modal { background: var(--bg-secondary); border-radius: 8px; box-shadow: 0 10px 25px rgba(0,0,0,0.3); border: 1px solid var(--border-color); color: var(--text-primary); display: flex; flex-direction: column; max-height: 90vh; }
.large-modal { width: 750px; }

.modal-header { padding: 1.5rem; border-bottom: 1px solid var(--border-color); display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; }
.btn-close { background: none; border: none; color: var(--text-secondary); cursor: pointer; font-size: 1.2rem; }

.modal-body { padding: 1.5rem; overflow-y: auto; }

.divider { border: 0; border-top: 1px solid var(--border-color); margin: 1.5rem 0; }

label { display: block; margin-bottom: 0.5rem; color: var(--text-primary); }
select, input { width: 100%; padding: 0.7rem; background-color: var(--bg-primary); border: 1px solid var(--border-color); color: var(--text-primary); border-radius: 4px; }

.input-table { 
    padding: 0.4rem; font-size: 0.9rem; margin-bottom: 0; 
    border: 1px solid var(--border-color); border-radius: 4px; 
    width: 100%; max-width: 100px;
}

.add-items-row { display: flex; gap: 1rem; align-items: flex-end; margin-bottom: 1rem; }
.flex-grow { flex: 1; }
.w-100 { width: 100px; }
.btn-align { padding-bottom: 2px; }

.btn-secondary { background: transparent; border: 1px solid var(--accent-color); color: var(--accent-color); padding: 0.6rem 1rem; border-radius: 4px; cursor: pointer; }
.btn-secondary:hover { background: var(--accent-color); color: white; }

.tabela-itens { width: 100%; border-collapse: collapse; font-size: 0.9rem; margin-bottom: 1.5rem; }
.tabela-itens th { text-align: left; border-bottom: 1px solid var(--border-color); padding: 0.5rem; color: var(--text-secondary); }
.tabela-itens td { padding: 0.5rem; border-bottom: 1px solid var(--border-color); vertical-align: middle; }
.btn-icon-small { background: none; border: none; cursor: pointer; color: var(--danger-color); font-size: 1rem; }

.total-row { display: flex; justify-content: space-between; align-items: center; font-size: 1.2rem; font-weight: bold; padding-top: 1rem; border-top: 2px solid var(--border-color); }
.total-value { color: var(--success-color); }

.modal-footer { padding: 1.5rem; border-top: 1px solid var(--border-color); display: flex; justify-content: flex-end; gap: 1rem; }
.btn-save { background-color: var(--success-color); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; font-weight: bold; }
.btn-cancel { background-color: var(--text-secondary); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; }

.loading-state, .error-state { text-align: center; padding: 2rem; color: var(--text-secondary); }
.error-state { color: var(--danger-color); }
.text-center-small { text-align: center; color: var(--text-secondary); padding: 1rem; font-style: italic; }
</style>
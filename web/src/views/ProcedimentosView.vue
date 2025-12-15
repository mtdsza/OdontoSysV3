<script setup>
import { ref, onMounted, computed } from 'vue';
import axios from 'axios';

const procedimentos = ref([]);
const loading = ref(false);
const error = ref('');
const showModal = ref(false);
const isEditing = ref(false);

const form = ref({
    idProcedimento: 0,
    nome: '',
    valorPadrao: 0
});

onMounted(() => {
    fetchProcedimentos();
});

const fetchProcedimentos = async () => {
    loading.value = true;
    try {
        const response = await axios.get('http://localhost:5000/api/procedimento/buscartodos');
        procedimentos.value = response.data.data;
    } catch (err) {
        error.value = "Erro ao carregar procedimentos.";
    } finally {
        loading.value = false;
    }
};

const salvar = async () => {
    try {
        if (!form.value.nome || form.value.valorPadrao < 0) {
            alert("Nome obrigatório e valor não pode ser negativo.");
            return;
        }

        if (isEditing.value) {
            await axios.put('http://localhost:5000/api/procedimento/atualizar', form.value);
            alert('Procedimento atualizado!');
        } else {
            await axios.post('http://localhost:5000/api/procedimento/inserir', form.value);
            alert('Procedimento criado!');
        }
        closeModal();
        fetchProcedimentos();
    } catch (err) {
        alert("Erro ao salvar: " + (err.response?.data?.message || err.message));
    }
};

const excluir = async (id) => {
    if (!confirm("Tem certeza? Isso pode afetar orçamentos existentes.")) return;
    try {
        await axios.delete(`http://localhost:5000/api/procedimento/excluir/${id}`);
        fetchProcedimentos();
    } catch (err) {
        alert("Erro ao excluir. O procedimento pode estar em uso.");
    }
};

const openModal = (proc = null) => {
    if (proc) {
        isEditing.value = true;
        form.value = { ...proc };
    } else {
        isEditing.value = false;
        form.value = { idProcedimento: 0, nome: '', valorPadrao: 0 };
    }
    showModal.value = true;
};

const closeModal = () => {
    showModal.value = false;
};

const formatCurrency = (value) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
};
</script>

<template>
    <div class="page-container">
        <header class="page-header">
            <div class="header-title">
                <h2>Procedimentos</h2>
                <p class="subtitle">Tabela de preços e serviços</p>
            </div>
            <button class="btn-primary" @click="openModal()">
                <i class="bi bi-plus-lg"></i> Novo Serviço
            </button>
        </header>

        <div v-if="loading" class="loading-state">Carregando...</div>
        
        <div v-if="!loading" class="table-responsive">
            <table class="tabela">
                <thead>
                    <tr>
                        <th>Nome do Procedimento</th>
                        <th>Valor Padrão</th>
                        <th style="text-align: right;">Ações</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="p in procedimentos" :key="p.idProcedimento">
                        <td>{{ p.nome }}</td>
                        <td class="valor">{{ formatCurrency(p.valorPadrao) }}</td>
                        <td class="actions-cell">
                            <button class="btn-icon btn-edit" @click="openModal(p)" title="Editar">
                                <i class="bi bi-pencil-square"></i>
                            </button>
                            <button class="btn-icon btn-del" @click="excluir(p.idProcedimento)" title="Excluir">
                                <i class="bi bi-trash3"></i>
                            </button>
                        </td>
                    </tr>
                    <tr v-if="procedimentos.length === 0">
                        <td colspan="3" class="text-center">Nenhum procedimento cadastrado.</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
            <div class="modal">
                <h3>{{ isEditing ? 'Editar Procedimento' : 'Novo Procedimento' }}</h3>
                <form @submit.prevent="salvar">
                    <label>Nome do Serviço</label>
                    <input v-model="form.nome" required placeholder="Ex: Limpeza Simples" />
                    
                    <label>Valor (R$)</label>
                    <input v-model="form.valorPadrao" type="number" step="0.01" min="0" required />

                    <div class="modal-actions">
                        <button type="button" class="btn-cancel" @click="closeModal">Cancelar</button>
                        <button type="submit" class="btn-save">
                            <i class="bi bi-check-lg"></i> Salvar
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>

<style scoped>
.page-container { max-width: 800px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
h2 { margin: 0; color: var(--text-primary); }
.subtitle { margin: 0.2rem 0 0; color: var(--text-secondary); font-size: 0.9rem; }

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
.valor { font-family: monospace; font-weight: bold; color: var(--success-color); }

.btn-primary { background-color: var(--accent-color); color: white; border: none; padding: 0.8rem 1.5rem; border-radius: 6px; cursor: pointer; font-weight: bold; }
.btn-primary:hover { background-color: var(--accent-hover); }

.actions-cell { text-align: right; }
.text-center { text-align: center; color: var(--text-secondary); padding: 2rem; }

.modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); display: flex; justify-content: center; align-items: center; z-index: 1000; }
.modal { background: var(--bg-secondary); padding: 2rem; border-radius: 8px; width: 400px; box-shadow: 0 10px 25px rgba(0,0,0,0.3); border: 1px solid var(--border-color); color: var(--text-primary); }
.modal h3 { margin-top: 0; margin-bottom: 1.5rem; }

input { width: 100%; padding: 0.7rem; background-color: var(--bg-primary); border: 1px solid var(--border-color); color: var(--text-primary); border-radius: 4px; margin-bottom: 1rem; }
label { display: block; margin-bottom: 0.5rem; color: var(--text-primary); }

.modal-actions { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1rem; }
.btn-save { background-color: var(--success-color); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; }
.btn-cancel { background-color: var(--text-secondary); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; }
</style>
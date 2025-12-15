<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';

const itens = ref([]);
const loading = ref(false);
const showModal = ref(false);
const showMovModal = ref(false);
const itemSelecionado = ref(null);

const form = ref({ descricao: '', estoqueMin: 5 });

const formMov = ref({ 
    quantidade: 1, 
    tipo: 0, 
    justificativa: '', 
    valor: 0 
}); 

onMounted(() => fetchEstoque());

const fetchEstoque = async () => {
    loading.value = true;
    try {
        const res = await axios.get('http://localhost:5000/api/estoque/listar');
        itens.value = res.data.data;
    } catch (e) { alert('Erro ao carregar estoque'); } 
    finally { loading.value = false; }
};

const criarItem = async () => {
    try {
        await axios.post('http://localhost:5000/api/estoque/criar-item', form.value);
        fetchEstoque();
        showModal.value = false;
        form.value = { descricao: '', estoqueMin: 5 };
    } catch(e) { alert('Erro ao criar item'); }
};

const abrirMovimentacao = (item) => {
    itemSelecionado.value = item;
    formMov.value = { quantidade: 1, tipo: 0, justificativa: '', valor: 0 };
    showMovModal.value = true;
};

const salvarMovimentacao = async () => {
    loading.value = true;
    try {
        const dtoEstoque = {
            idItemEstoque: itemSelecionado.value.idItemEstoque,
            quantidade: formMov.value.quantidade,
            tipo: parseInt(formMov.value.tipo),
            justificativa: formMov.value.justificativa
        };
        await axios.post('http://localhost:5000/api/estoque/movimentacao', dtoEstoque);

        if (formMov.value.tipo == 0 && formMov.value.valor > 0) {
            const dtoFinanceiro = {
                descricao: `Compra Estoque: ${itemSelecionado.value.descricao} (Qtd: ${formMov.value.quantidade})`,
                valor: formMov.value.valor,
                tipo: 1
            };
            
            try {
                await axios.post('http://localhost:5000/api/financeiro/registrar-despesa', dtoFinanceiro);
            } catch (errFin) {
                console.error("Erro ao lançar financeiro", errFin);
                alert("Estoque atualizado, mas houve um erro ao lançar a despesa no financeiro.");
            }
        }

        alert("Movimentação registrada com sucesso!");
        fetchEstoque();
        showMovModal.value = false;
    } catch(e) { 
        alert('Erro ao registrar movimentação de estoque.'); 
    } finally {
        loading.value = false;
    }
};

const excluirItem = async (id) => {
    if(!confirm("Excluir item?")) return;
    try {
        await axios.delete(`http://localhost:5000/api/estoque/excluir-item/${id}`);
        fetchEstoque();
    } catch(e) { alert('Erro ao excluir (item pode ter movimentações)'); }
};
</script>

<template>
    <div class="page-container">
        <header class="page-header">
            <div class="header-title">
                <h2>Controle de Estoque</h2>
                <p class="subtitle">Materiais e Consumo</p>
            </div>
            <button class="btn-primary" @click="showModal = true">
                <i class="bi bi-plus-lg"></i> Novo Material
            </button>
        </header>

        <div v-if="!loading" class="table-responsive">
            <table class="tabela">
                <thead>
                    <tr>
                        <th>Material</th>
                        <th>Qtd Atual</th>
                        <th>Mínimo</th>
                        <th>Status</th>
                        <th style="text-align: right;">Ações</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="i in itens" :key="i.idItemEstoque">
                        <td>{{ i.descricao }}</td>
                        <td class="bold">{{ i.quantidade }}</td>
                        <td>{{ i.estoqueMin }}</td>
                        <td>
                            <span v-if="i.quantidade <= i.estoqueMin" class="tag-low">
                                <i class="bi bi-exclamation-circle"></i> Baixo
                            </span>
                            <span v-else class="tag-ok">OK</span>
                        </td>
                        <td class="actions-cell">
                            <button class="btn-icon btn-mov" @click="abrirMovimentacao(i)" title="Movimentar">
                                <i class="bi bi-arrow-left-right"></i>
                            </button>
                            <button class="btn-icon btn-del" @click="excluirItem(i.idItemEstoque)" title="Excluir">
                                <i class="bi bi-trash3"></i>
                            </button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div v-if="showModal" class="modal-overlay">
            <div class="modal">
                <h3>Novo Material</h3>
                <form @submit.prevent="criarItem">
                    <label>Nome do Material</label>
                    <input v-model="form.descricao" required />
                    <label>Estoque Mínimo (Alerta)</label>
                    <input type="number" v-model="form.estoqueMin" required />
                    <div class="modal-footer">
                        <button type="button" class="btn-sec" @click="showModal = false">Cancelar</button>
                        <button type="submit" class="btn-pri">
                            <i class="bi bi-check-lg"></i> Salvar
                        </button>
                    </div>
                </form>
            </div>
        </div>

        <div v-if="showMovModal" class="modal-overlay">
            <div class="modal">
                <h3>Movimentar: {{ itemSelecionado?.descricao }}</h3>
                <form @submit.prevent="salvarMovimentacao">
                    
                    <label>Tipo de Movimento</label>
                    <select v-model="formMov.tipo">
                        <option :value="0">Entrada (Compra)</option>
                        <option :value="1">Perda/Quebra</option>
                        <option :value="2">Ajuste Manual</option>
                    </select>

                    <div class="row-inputs">
                        <div class="col">
                            <label>Quantidade</label>
                            <input type="number" v-model="formMov.quantidade" min="0.1" step="0.1" required />
                        </div>
                        
                        <div class="col" v-if="formMov.tipo == 0">
                            <label>Custo Total (R$)</label>
                            <input 
                                type="number" 
                                v-model="formMov.valor" 
                                min="0" 
                                step="0.01" 
                                placeholder="0,00" 
                            />
                        </div>
                    </div>

                    <label>Observação</label>
                    <input v-model="formMov.justificativa" placeholder="Ex: Compra Nota Fiscal 123" />
                    
                    <div class="modal-footer">
                        <button type="button" class="btn-sec" @click="showMovModal = false">Cancelar</button>
                        <button type="submit" class="btn-pri" :disabled="loading">
                            {{ loading ? 'Salvando...' : 'Confirmar' }}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>

<style scoped>
.page-container { max-width: 900px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
h2 { margin: 0; color: var(--text-primary); }
.subtitle { margin: 0.2rem 0 0; color: var(--text-secondary); font-size: 0.9rem; }
.btn-primary { background: var(--accent-color); color: white; border: none; padding: 0.8rem 1.5rem; border-radius: 6px; cursor: pointer; font-weight: bold; }

.table-responsive { background: var(--bg-secondary); border-radius: 8px; border: 1px solid var(--border-color); }
.tabela { width: 100%; border-collapse: collapse; }
.tabela th { background: var(--bg-primary); color: var(--text-secondary); padding: 1rem; text-align: left; border-bottom: 2px solid var(--border-color); }
.tabela td { padding: 1rem; border-bottom: 1px solid var(--border-color); color: var(--text-primary); }
.bold { font-weight: bold; font-size: 1.1rem; }
.tag-low { color: var(--danger-color); font-weight: bold; }
.tag-ok { color: var(--success-color); font-weight: bold; }

.btn-icon { background: none; border: none; cursor: pointer; font-size: 1.1rem; transition: transform 0.2s; }
.btn-icon:hover { transform: scale(1.1); }
.actions-cell { text-align: right; }

.modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); display: flex; justify-content: center; align-items: center; z-index: 1000; }
.modal { background: var(--bg-secondary); padding: 2rem; border-radius: 8px; width: 450px; border: 1px solid var(--border-color); color: var(--text-primary); }

input, select { width: 100%; padding: 0.7rem; background: var(--bg-primary); border: 1px solid var(--border-color); color: var(--text-primary); border-radius: 4px; margin-bottom: 1rem; }
label { display: block; margin-bottom: 0.5rem; font-weight: 500; font-size: 0.9rem; }
.row-inputs { display: flex; gap: 1rem; }
.col { flex: 1; }

.modal-footer { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 0.5rem; }
.btn-pri { background: var(--success-color); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; font-weight: bold; }
.btn-pri:disabled { opacity: 0.7; cursor: not-allowed; }
.btn-sec { background: var(--text-secondary); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; }
</style>
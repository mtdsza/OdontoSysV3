<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import BaseModal from '@/components/ui/BaseModal.vue';

const funcionarios = ref([]);
const loading = ref(false);
const showModal = ref(false);
const isEditing = ref(false);

const form = ref({
    idFuncionario: 0,
    nome: '',
    email: '',
    telefone: '',
    tipo: 1,
    ativo: true,
    salarioBase: 0,
    dataContratacao: '',
    cro: ''
});

onMounted(() => fetchFuncionarios());

const fetchFuncionarios = async () => {
    loading.value = true;
    try {
        const res = await axios.get('http://localhost:5000/api/funcionario/buscartodos');
        funcionarios.value = res.data.data;
    } catch (e) { alert("Erro ao carregar funcionários."); }
    finally { loading.value = false; }
};

const abrirModal = (func = null) => {
    if (func) {
        isEditing.value = true;
        form.value = { ...func };
    } else {
        isEditing.value = false;
        form.value = { 
            idFuncionario: 0, nome: '', email: '', telefone: '', 
            tipo: 1, ativo: true, salarioBase: 0, dataContratacao: '', cro: '' 
        };
    }
    showModal.value = true;
};

const salvar = async () => {
    try {
        if (!form.value.nome || !form.value.email) return alert("Nome e Email de contato são obrigatórios.");
        
        const payload = { ...form.value };

        if (payload.telefone) {
            payload.telefone = payload.telefone.replace(/\D/g, '');
        }

        delete payload.senha;

        if (parseInt(form.value.tipo) === 0 && !isEditing.value) {
            if (!form.value.cro) return alert("Para cadastrar Dentista, o CRO é obrigatório.");
            await axios.post('http://localhost:5000/api/dentista/inserir', payload);
        } else {
            const url = isEditing.value 
                ? 'http://localhost:5000/api/funcionario/atualizar' 
                : 'http://localhost:5000/api/funcionario/inserir';
            const method = isEditing.value ? axios.put : axios.post;
            
            await method(url, payload);
        }

        alert("Dados salvos com sucesso!");
        showModal.value = false;
        fetchFuncionarios();
    } catch (e) {
        const msg = e.response?.data?.message || e.message;
        const errors = e.response?.data?.errors ? JSON.stringify(e.response.data.errors) : '';
        alert(`Erro ao salvar: ${msg} ${errors}`);
    }
};

const desativarFuncionario = async (func) => {
    const acao = func.ativo ? "desativar" : "reativar";
    if (!confirm(`Deseja realmente ${acao} o funcionário ${func.nome}?`)) return;

    try {
        const payload = { ...func, ativo: !func.ativo };
        
        delete payload.senha; 

        await axios.put('http://localhost:5000/api/funcionario/atualizar', payload);
        fetchFuncionarios();
    } catch (e) {
        alert("Erro ao atualizar status.");
    }
};

const pagarSalario = async (func) => {
    if (!func.salarioBase) return alert("Defina um salário base primeiro.");
    if (!confirm(`Lançar pagamento de salário para ${func.nome}?`)) return;
    try {
        await axios.post('http://localhost:5000/api/financeiro/registrar-despesa', {
            descricao: `Salário - ${func.nome}`,
            valor: func.salarioBase,
            tipo: 1
        });
        alert("Lançado no financeiro!");
    } catch (e) { alert("Erro ao lançar."); }
};

const getCargo = (t) => {
    const map = { 0: 'Dentista', 1: 'Secretária', 2: 'Gerente/Admin', 3: 'Auxiliar', 4: 'Outros' };
    return map[t] || 'Outro';
};
const money = (v) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v || 0);
</script>

<template>
    <div class="page-container">
        <header class="page-header">
            <div class="header-title">
                <h2>Recursos Humanos</h2>
                <p class="subtitle">Quadro de Funcionários e Dentistas</p>
            </div>
            <button class="btn-primary" @click="abrirModal()">
                <i class="bi bi-plus-lg"></i> Novo Funcionário
            </button>
        </header>

        <div v-if="!loading" class="table-responsive">
            <table class="tabela">
                <thead>
                    <tr>
                        <th>Nome</th>
                        <th>Cargo</th>
                        <th>Contato</th>
                        <th>Salário Base</th>
                        <th>Status</th>
                        <th style="text-align: right;">Ações</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="f in funcionarios" :key="f.idFuncionario">
                        <td :class="{ 'text-muted': !f.ativo }">{{ f.nome }}</td>
                        <td><span :class="['badge', 'role-'+f.tipo]">{{ getCargo(f.tipo) }}</span></td>
                        <td>
                            <div class="contato-col">
                                <span>{{ f.email }}</span>
                                <small>{{ f.telefone }}</small>
                            </div>
                        </td>
                        <td class="mono">{{ money(f.salarioBase) }}</td>
                        <td>
                            <span :class="f.ativo ? 'tag-ok' : 'tag-bad'">{{ f.ativo ? 'Ativo' : 'Inativo' }}</span>
                        </td>
                        <td class="actions-cell">
                            <button class="btn-icon btn-check" @click="pagarSalario(f)" title="Pagar Salário" :disabled="!f.ativo">
                                <i class="bi bi-cash-coin"></i>
                            </button>
                            <button class="btn-icon btn-edit" @click="abrirModal(f)" title="Editar">
                                <i class="bi bi-pencil-square"></i>
                            </button>
                            
                            <button 
                                class="btn-icon" 
                                :class="f.ativo ? 'btn-del' : 'btn-activate'"
                                @click="desativarFuncionario(f)" 
                                :title="f.ativo ? 'Desativar' : 'Reativar'"
                            >
                                <i class="bi" :class="f.ativo ? 'bi-ban' : 'bi-check-circle'"></i>
                            </button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <BaseModal :isOpen="showModal" :title="isEditing ? 'Editar Funcionário' : 'Contratar Funcionário'" @close="showModal = false">
            <form @submit.prevent="salvar" class="form-grid">
                <div class="col-2">
                    <label>Nome Completo</label>
                    <input v-model="form.nome" required />
                </div>
                <div>
                    <label>Cargo / Função</label>
                    <select v-model="form.tipo" :disabled="isEditing">
                        <option :value="0">Dentista</option>
                        <option :value="1">Secretária</option>
                        <option :value="3">Auxiliar</option>
                        <option :value="4">Limpeza/Outros</option>
                        <option :value="2">Gerente/Admin</option>
                    </select>
                </div>
                <div v-if="parseInt(form.tipo) === 0">
                    <label>CRO (Registro)</label>
                    <input v-model="form.cro" placeholder="12345-UF" :disabled="isEditing" />
                </div>
                
                <div>
                    <label>Email (Contato)</label>
                    <input type="email" v-model="form.email" required />
                </div>
                <div>
                    <label>Telefone</label>
                    <input v-model="form.telefone" v-maska data-maska="['(##) ####-####', '(##) #####-####']" />
                </div>
                <div>
                    <label>Salário Base (R$)</label>
                    <input type="number" v-model="form.salarioBase" step="0.01" />
                </div>
                <div>
                    <label>Data Admissão</label>
                    <input type="date" v-model="form.dataContratacao" />
                </div>
                <div class="col-2 chk">
                    <label><input type="checkbox" v-model="form.ativo" /> Funcionário Ativo?</label>
                </div>
                <div class="col-2 footer-btns">
                    <button type="button" class="btn-sec" @click="showModal = false">Cancelar</button>
                    <button type="submit" class="btn-pri">
                        <i class="bi bi-check-lg"></i> Salvar Ficha
                    </button>
                </div>
            </form>
        </BaseModal>
    </div>
</template>

<style scoped>
.page-container { max-width: 1100px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
h2 { margin: 0; color: var(--text-primary); }
.subtitle { margin: 0.2rem 0 0; color: var(--text-secondary); font-size: 0.9rem; }

.table-responsive { background: var(--bg-secondary); border-radius: 8px; border: 1px solid var(--border-color); }
.tabela { width: 100%; border-collapse: collapse; }
.tabela th { background: var(--bg-primary); padding: 1rem; text-align: left; color: var(--text-secondary); border-bottom: 1px solid var(--border-color); }
.tabela td { padding: 1rem; border-bottom: 1px solid var(--border-color); color: var(--text-primary); }

.contato-col { display: flex; flex-direction: column; font-size: 0.9rem; }
.contato-col small { color: var(--text-secondary); }
.mono { font-family: monospace; font-weight: bold; }
.text-muted { opacity: 0.6; }

.badge { padding: 0.2rem 0.6rem; border-radius: 12px; font-size: 0.8rem; font-weight: bold; }
.role-0 { background: #e3f2fd; color: #1976d2; }
.role-1 { background: #fff3e0; color: #f57c00; }
.role-2 { background: #f3e5f5; color: #7b1fa2; }
.role-3 { background: #e8f5e9; color: #388e3c; }

.tag-ok { color: var(--success-color); font-weight: bold; }
.tag-bad { color: var(--danger-color); font-weight: bold; }

.btn-primary { background: var(--accent-color); color: white; border: none; padding: 0.8rem 1.5rem; border-radius: 6px; font-weight: bold; cursor: pointer; }
.btn-icon { background: none; border: none; cursor: pointer; font-size: 1.1rem; transition: transform 0.1s; }
.btn-icon:hover { transform: scale(1.1); }
.btn-icon.btn-del:hover { color: var(--danger-color); }
.btn-icon.btn-activate { color: var(--success-color); }
.btn-icon.btn-activate:hover { background-color: #e8f5e9; }

.actions-cell { text-align: right; white-space: nowrap; }

.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
.col-2 { grid-column: span 2; }
input, select { width: 100%; padding: 0.7rem; border-radius: 4px; border: 1px solid var(--border-color); background: var(--bg-primary); color: var(--text-primary); }
label { display: block; margin-bottom: 0.4rem; font-weight: 500; font-size: 0.9rem; color: var(--text-primary); }
.chk { margin-top: 0.5rem; }
.footer-btns { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1rem; }
.btn-pri { background: var(--success-color); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; font-weight: bold; }
.btn-sec { background: var(--text-secondary); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; }
</style>
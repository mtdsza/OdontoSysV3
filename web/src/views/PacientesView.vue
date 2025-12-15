<script setup>
import { ref, onMounted, computed } from 'vue';
import axios from 'axios';
import PacienteForm from '@/components/pacientes/PacienteForm.vue';
import PacienteFicha from '@/components/pacientes/PacienteFicha.vue';

const pacientes = ref([]);
const loading = ref(false);
const error = ref('');
const termoBusca = ref('');

const showModalForm = ref(false);
const showModalFicha = ref(false);
const pacienteIdSelecionado = ref(0);

onMounted(() => {
    fetchPacientes();
});

const fetchPacientes = async () => {
    loading.value = true;
    try {
        const response = await axios.get('http://localhost:5000/api/paciente/buscartodos');
        pacientes.value = response.data.data;
    } catch (err) {
        error.value = "Erro ao carregar pacientes.";
    } finally {
        loading.value = false;
    }
};

const abrirNovo = () => {
    pacienteIdSelecionado.value = 0;
    showModalForm.value = true;
};

const abrirEdicao = (id) => {
    pacienteIdSelecionado.value = id;
    showModalForm.value = true;
};

const abrirProntuario = (id) => {
    pacienteIdSelecionado.value = id;
    showModalFicha.value = true;
};

const aoSalvar = () => {
    fetchPacientes();
};

const desativarPaciente = async (paciente) => {
    if (!paciente.ativo) return;
    if (!confirm(`Deseja desativar o paciente ${paciente.nome}?`)) return;
    
    try {
        const payload = { ...paciente, ativo: false };
        await axios.put('http://localhost:5000/api/paciente/atualizar', payload);
        fetchPacientes();
    } catch (err) {
        alert("Erro ao desativar paciente.");
    }
};

const formatarCpf = (cpf) => cpf ? cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, "$1.$2.$3-$4") : '-';
const formatarData = (dataStr) => {
    if (!dataStr) return '-';
    const [ano, mes, dia] = dataStr.split('T')[0].split('-');
    return `${dia}/${mes}/${ano}`;
};

const pacientesFiltrados = computed(() => {
    if (!termoBusca.value) return pacientes.value;
    const termo = termoBusca.value.toLowerCase();
    return pacientes.value.filter(p => 
        p.nome.toLowerCase().includes(termo) || 
        p.cpf.includes(termo)
    );
});
</script>

<template>
    <div class="page-container">
        <header class="page-header">
            <div class="header-title">
                <h2>Pacientes</h2>
                <p class="subtitle">Gestão de prontuários e cadastros</p>
            </div>
            <button class="btn-primary" @click="abrirNovo">
                <i class="bi bi-plus-lg"></i> Novo Paciente
            </button>
        </header>

        <div class="search-bar">
            <input v-model="termoBusca" placeholder="Buscar por nome ou CPF..." />
        </div>

        <div v-if="loading" class="loading-state">Carregando...</div>

        <div v-if="!loading" class="table-responsive">
            <table class="tabela">
                <thead>
                    <tr>
                        <th>Nome</th>
                        <th>CPF</th>
                        <th>Nascimento</th>
                        <th>Telefone</th>
                        <th>Status</th>
                        <th style="text-align: right;">Ações</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="p in pacientesFiltrados" :key="p.idPaciente">
                        <td :class="{'text-inativo': !p.ativo}">{{ p.nome }}</td>
                        <td>{{ formatarCpf(p.cpf) }}</td>
                        <td>{{ formatarData(p.nascimento) }}</td>
                        <td>{{ p.telefone || '-' }}</td>
                        <td>
                            <span :class="p.ativo ? 'tag-active' : 'tag-inactive'">
                                {{ p.ativo ? 'Ativo' : 'Inativo' }}
                            </span>
                        </td>
                        <td class="actions-cell">
                            <button class="btn-icon btn-view" @click="abrirProntuario(p.idPaciente)" title="Abrir Prontuário">
                                <i class="bi bi-journal-medical"></i>
                            </button>
                            <button class="btn-icon btn-edit" @click="abrirEdicao(p.idPaciente)" title="Editar Cadastro">
                                <i class="bi bi-pencil-square"></i>
                            </button>
                            
                            <button 
                                v-if="p.ativo"
                                class="btn-icon btn-del" 
                                @click="desativarPaciente(p)" 
                                title="Desativar Paciente"
                            >
                                <i class="bi bi-person-x"></i>
                            </button>
                        </td>
                    </tr>
                    <tr v-if="pacientesFiltrados.length === 0">
                        <td colspan="6" class="text-center">Nenhum paciente encontrado.</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <PacienteForm 
            :isOpen="showModalForm" 
            :pacienteId="pacienteIdSelecionado"
            @close="showModalForm = false"
            @saved="aoSalvar"
        />

        <PacienteFicha 
            :isOpen="showModalFicha" 
            :pacienteId="pacienteIdSelecionado"
            @close="showModalFicha = false"
        />
    </div>
</template>

<style scoped>
.page-container { max-width: 1200px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
h2 { margin: 0; color: var(--text-primary); }
.subtitle { margin: 0.2rem 0 0; color: var(--text-secondary); font-size: 0.9rem; }

.search-bar input {
    width: 100%; padding: 0.8rem; border-radius: 6px;
    border: 1px solid var(--border-color);
    background-color: var(--bg-secondary); color: var(--text-primary);
    margin-bottom: 1.5rem;
}

.table-responsive {
    background: var(--bg-secondary); border-radius: 8px;
    box-shadow: var(--shadow-sm); overflow-x: auto;
    border: 1px solid var(--border-color);
}
.tabela { width: 100%; border-collapse: collapse; }
.tabela th { background-color: var(--bg-primary); color: var(--text-secondary); padding: 1rem; text-align: left; border-bottom: 2px solid var(--border-color); white-space: nowrap; }
.tabela td { padding: 1rem; border-bottom: 1px solid var(--border-color); color: var(--text-primary); }
.text-center { text-align: center; padding: 2rem; color: var(--text-secondary); }
.text-inativo { opacity: 0.5; text-decoration: line-through; }

.tag-active { color: var(--success-color); font-weight: bold; font-size: 0.85rem; }
.tag-inactive { color: var(--danger-color); font-weight: bold; font-size: 0.85rem; }

.btn-primary { background-color: var(--accent-color); color: white; border: none; padding: 0.8rem 1.5rem; border-radius: 6px; cursor: pointer; font-weight: bold; }
.btn-primary:hover { filter: brightness(1.1); }

.actions-cell { text-align: right; white-space: nowrap; }
</style>
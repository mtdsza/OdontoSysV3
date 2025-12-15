<script setup>
import { ref, watch, computed } from 'vue';
import axios from 'axios';
import BaseModal from '@/components/ui/BaseModal.vue';

const props = defineProps({
    isOpen: Boolean,
    pacienteId: Number 
});

const emit = defineEmits(['close', 'saved']);

const abaAtiva = ref('pessoal');
const tratamentosExternos = ref([]);

const form = ref({
    idPaciente: 0, nome: '', cpf: '', nascimento: '', telefone: '', email: '',
    ativo: true, cep: '', logradouro: '', numero: '', complemento: '',
    bairro: '', cidade: '', estado: '', condicoesMedicas: '', observacoesGerais: ''
});

const novoLab = ref({ descricao: '', local: '', dataRealizacao: '', observacoes: '' });

watch(() => props.isOpen, async (val) => {
    if (val) {
        abaAtiva.value = 'pessoal';
        if (props.pacienteId) {
            await carregarPaciente(props.pacienteId);
        } else {
            resetForm();
        }
    }
});

const resetForm = () => {
    form.value = {
        idPaciente: 0, nome: '', cpf: '', nascimento: '', telefone: '', email: '',
        ativo: true, cep: '', logradouro: '', numero: '', complemento: '',
        bairro: '', cidade: '', estado: '', condicoesMedicas: '', observacoesGerais: ''
    };
    tratamentosExternos.value = [];
};

const carregarPaciente = async (id) => {
    try {
        const res = await axios.get(`http://localhost:5000/api/paciente/${id}`);
        form.value = res.data.data;
        fetchLabs(id);
    } catch (e) { alert("Erro ao carregar dados."); }
};

const salvar = async () => {
    try {
        if (!form.value.nome || !form.value.cpf || !form.value.nascimento) {
            alert("Preencha os campos obrigatórios (Nome, CPF e Nascimento).");
            return;
        }

        const payload = { ...form.value };

        payload.cpf = payload.cpf.replace(/\D/g, ''); 
        
        if (payload.telefone) {
            payload.telefone = payload.telefone.replace(/\D/g, '');
        }
        
        if (payload.cep) {
            payload.cep = payload.cep.replace(/\D/g, '');
        }

        if (form.value.idPaciente) {
            await axios.put('http://localhost:5000/api/paciente/atualizar', payload);
            alert("Atualizado com sucesso!");
        } else {
            const res = await axios.post('http://localhost:5000/api/paciente/inserir', payload);
            form.value.idPaciente = res.data.data;
            alert("Criado com sucesso! Agora você pode adicionar registros de laboratório.");
        }
        
        emit('saved');
        
        if (!props.pacienteId) return; 
        
        emit('close');
    } catch (e) {
        const msg = e.response?.data?.message || e.message;
        const errors = e.response?.data?.errors ? JSON.stringify(e.response.data.errors) : '';
        alert(`Erro ao salvar: ${msg}\n${errors}`);
    }
};

const fetchLabs = async (id) => {
    try {
        const res = await axios.get(`http://localhost:5000/api/tratamentoexterno/buscar-por-paciente/${id}`);
        tratamentosExternos.value = res.data.data;
    } catch (e) { tratamentosExternos.value = []; }
};

const addLab = async () => {
    if (!form.value.idPaciente) return alert("Salve o paciente primeiro.");
    try {
        await axios.post('http://localhost:5000/api/tratamentoexterno/inserir', {
            ...novoLab.value, idPaciente: form.value.idPaciente
        });
        novoLab.value = { descricao: '', local: '', dataRealizacao: '', observacoes: '' };
        fetchLabs(form.value.idPaciente);
    } catch (e) { alert("Erro ao adicionar lab."); }
};

const delLab = async (id) => {
    if (!confirm("Remover?")) return;
    try {
        await axios.delete(`http://localhost:5000/api/tratamentoexterno/excluir/${id}`);
        fetchLabs(form.value.idPaciente);
    } catch (e) { alert("Erro ao remover."); }
};

const isEditing = computed(() => !!form.value.idPaciente);
</script>

<template>
    <BaseModal 
        :isOpen="isOpen" 
        :title="isEditing ? 'Editar Prontuário' : 'Novo Paciente'" 
        maxWidth="800px"
        @close="emit('close')"
    >
        <div class="tabs">
            <button :class="{ active: abaAtiva === 'pessoal' }" @click="abaAtiva = 'pessoal'">
                <i class="bi bi-person"></i> Dados Pessoais
            </button>
            <button :class="{ active: abaAtiva === 'endereco' }" @click="abaAtiva = 'endereco'">
                <i class="bi bi-geo-alt"></i> Endereço
            </button>
            <button :class="{ active: abaAtiva === 'clinico' }" @click="abaAtiva = 'clinico'">
                <i class="bi bi-clipboard2-pulse"></i> Anamnese
            </button>
            <button :class="{ active: abaAtiva === 'laboratorio' }" @click="abaAtiva = 'laboratorio'" :disabled="!isEditing">
                <i class="bi bi-eyedropper"></i> Histórico {{ !isEditing ? '(Salve 1º)' : '' }}
            </button>
        </div>

        <form @submit.prevent="salvar" class="form-content">
            
            <div v-show="abaAtiva === 'pessoal'" class="grid-form">
                <div class="col-2">
                    <label>Nome Completo *</label>
                    <input v-model="form.nome" required placeholder="Nome do paciente" />
                </div>
                <div>
                    <label>CPF *</label>
                    <input v-model="form.cpf" v-maska data-maska="###.###.###-##" required placeholder="000.000.000-00" />
                </div>
                <div>
                    <label>Nascimento *</label>
                    <input type="date" v-model="form.nascimento" required />
                </div>
                <div>
                    <label>Telefone</label>
                    <input v-model="form.telefone" v-maska data-maska="['(##) ####-####', '(##) #####-####']" placeholder="(00) 00000-0000" />
                </div>
                <div>
                    <label>E-mail</label>
                    <input type="email" v-model="form.email" placeholder="email@exemplo.com" />
                </div>
                <div class="col-2 chk-wrapper">
                    <label class="chk-label">
                        <input type="checkbox" v-model="form.ativo" /> Paciente Ativo no Sistema
                    </label>
                </div>
            </div>

            <div v-show="abaAtiva === 'endereco'" class="grid-form">
                <div>
                    <label>CEP</label>
                    <input v-model="form.cep" v-maska data-maska="#####-###" placeholder="00000-000" />
                </div>
                <div>
                    <label>Estado (UF)</label>
                    <input v-model="form.estado" v-maska data-maska="@@" placeholder="SP" style="text-transform: uppercase;" />
                </div>
                <div class="col-2">
                    <label>Logradouro</label>
                    <input v-model="form.logradouro" placeholder="Rua, Avenida..." />
                </div>
                <div>
                    <label>Número</label>
                    <input v-model="form.numero" />
                </div>
                <div>
                    <label>Bairro</label>
                    <input v-model="form.bairro" />
                </div>
                <div class="col-2">
                    <label>Complemento</label>
                    <input v-model="form.complemento" placeholder="Apto, Bloco..." />
                </div>
                <div class="col-2">
                    <label>Cidade</label>
                    <input v-model="form.cidade" />
                </div>
            </div>

            <div v-show="abaAtiva === 'clinico'" class="grid-form">
                <div class="col-2">
                    <label>Condições Médicas / Alergias</label>
                    <textarea v-model="form.condicoesMedicas" rows="4" placeholder="Ex: Diabético, Alérgico a Penicilina..."></textarea>
                </div>
                <div class="col-2">
                    <label>Observações Gerais</label>
                    <textarea v-model="form.observacoesGerais" rows="4" placeholder="Anotações internas..."></textarea>
                </div>
            </div>

            <div v-show="abaAtiva === 'laboratorio'" class="lab-wrapper">
                <div class="lab-add-card">
                    <h4>Novo Pedido Externo</h4>
                    <div class="grid-lab">
                        <input v-model="novoLab.descricao" placeholder="Serviço (ex: Prótese)" class="inp-desc" />
                        <input v-model="novoLab.local" placeholder="Laboratório" />
                        <input type="date" v-model="novoLab.dataRealizacao" />
                        <button type="button" class="btn-mini-add" @click="addLab">Adicionar</button>
                    </div>
                    <input v-model="novoLab.observacoes" placeholder="Obs: Cor A3, Urgente..." class="inp-full" />
                </div>

                <div class="lab-list">
                    <div v-for="lab in tratamentosExternos" :key="lab.idTratamentoExterno" class="lab-item">
                        <div class="lab-info">
                            <strong>{{ lab.descricao }}</strong>
                            <span class="lab-sub">{{ lab.local }} • {{ new Date(lab.dataRealizacao).toLocaleDateString('pt-BR') }}</span>
                            <p v-if="lab.observacoes" class="lab-obs">Obs: {{ lab.observacoes }}</p>
                        </div>
                        <button type="button" class="btn-icon btn-del" @click="delLab(lab.idTratamentoExterno)">
                            <i class="bi bi-trash3"></i>
                        </button>
                    </div>
                    <p v-if="tratamentosExternos.length === 0" class="empty-lab">Nenhum registro encontrado.</p>
                </div>
            </div>

        </form>

        <template #footer>
            <button class="btn-cancel" @click="emit('close')">Cancelar</button>
            <button class="btn-save" @click="salvar">
                <i class="bi bi-floppy"></i> Salvar Dados
            </button>
        </template>
    </BaseModal>
</template>

<style scoped>
.tabs { display: flex; gap: 1rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem; margin-bottom: 1.5rem; overflow-x: auto; }
.tabs button {
    background: none; border: none; padding: 0.5rem 1rem; cursor: pointer;
    color: var(--text-secondary); font-weight: 600; border-radius: 4px; white-space: nowrap;
    display: flex; align-items: center; gap: 0.5rem;
}
.tabs button.active { background-color: var(--accent-color); color: white; }
.tabs button:disabled { opacity: 0.5; cursor: not-allowed; }

.grid-form { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
.col-2 { grid-column: span 2; }

label { display: block; margin-bottom: 0.3rem; color: var(--text-primary); font-size: 0.9rem; font-weight: 500; }
input, textarea {
    width: 100%; padding: 0.6rem; border-radius: 4px;
    border: 1px solid var(--border-color);
    background: var(--bg-primary); color: var(--text-primary);
    font-family: inherit;
}
input:focus, textarea:focus { border-color: var(--accent-color); outline: none; }

.chk-wrapper { display: flex; align-items: center; padding-top: 0.5rem; }
.chk-label { display: flex; align-items: center; gap: 0.5rem; cursor: pointer; }
.chk-label input { width: auto; }

.lab-wrapper { display: flex; flex-direction: column; gap: 1.5rem; }
.lab-add-card { background: var(--bg-primary); padding: 1rem; border-radius: 6px; border: 1px solid var(--border-color); }
.lab-add-card h4 { margin: 0 0 0.8rem 0; font-size: 0.9rem; color: var(--text-secondary); text-transform: uppercase; }
.grid-lab { display: grid; grid-template-columns: 2fr 1fr 150px auto; gap: 0.5rem; margin-bottom: 0.5rem; }
.inp-full { width: 100%; margin-top: 0.5rem; }
.btn-mini-add { background: var(--success-color); color: white; border: none; border-radius: 4px; cursor: pointer; font-weight: bold; }

.lab-list { max-height: 250px; overflow-y: auto; }
.lab-item { 
    display: flex; justify-content: space-between; align-items: center;
    padding: 0.8rem; border-bottom: 1px solid var(--border-color);
}
.lab-info { display: flex; flex-direction: column; gap: 0.2rem; }
.lab-sub { font-size: 0.85rem; color: var(--text-secondary); }
.lab-obs { font-size: 0.85rem; color: var(--text-primary); font-style: italic; margin: 0; }
.empty-lab { text-align: center; padding: 1rem; color: var(--text-secondary); font-style: italic; }

.btn-save { background: var(--success-color); color: white; border: none; padding: 0.6rem 1.5rem; border-radius: 4px; cursor: pointer; font-weight: bold; }
.btn-save:hover { filter: brightness(1.1); }
.btn-cancel { background: transparent; border: 1px solid var(--border-color); color: var(--text-secondary); padding: 0.6rem 1.5rem; border-radius: 4px; cursor: pointer; }
.btn-cancel:hover { border-color: var(--text-primary); color: var(--text-primary); }

@media (max-width: 700px) {
    .grid-form, .grid-lab { grid-template-columns: 1fr; }
    .col-2 { grid-column: span 1; }
}
</style>
<script setup>
import { ref, watch } from 'vue';
import axios from 'axios';
import BaseModal from '@/components/ui/BaseModal.vue';
import ConsultaFicha from '@/components/consultas/ConsultaFicha.vue';

const props = defineProps({
    isOpen: Boolean,
    pacienteId: Number
});

const emit = defineEmits(['close']);

const paciente = ref(null);
const historicoExterno = ref([]);
const historicoInterno = ref([]);
const loading = ref(false);
const tabAtiva = ref('evolucao');

const consultaSelecionadaId = ref(0);
const showDetalhesConsulta = ref(false);

watch(() => props.isOpen, async (val) => {
    if (val && props.pacienteId) {
        await carregarDados(props.pacienteId);
    } else {
        paciente.value = null;
        historicoExterno.value = [];
        historicoInterno.value = [];
    }
});

const carregarDados = async (id) => {
    loading.value = true;
    try {
        const resPac = await axios.get(`http://localhost:5000/api/paciente/${id}`);
        paciente.value = resPac.data.data;

        const resExt = await axios.get(`http://localhost:5000/api/tratamentoexterno/buscar-por-paciente/${id}`);
        historicoExterno.value = resExt.data.data;

        const resInt = await axios.get('http://localhost:5000/api/consulta/buscartodos');
        historicoInterno.value = resInt.data.data
            .filter(c => c.idPaciente === id && c.situacao === 1)
            .sort((a, b) => new Date(b.dataInicio) - new Date(a.dataInicio));

    } catch (e) {
        console.error("Erro ao carregar prontuário", e);
    } finally {
        loading.value = false;
    }
};

const abrirDetalhesConsulta = (idConsulta) => {
    consultaSelecionadaId.value = idConsulta;
    showDetalhesConsulta.value = true;
};

const calcularIdade = (dataNasc) => {
    if (!dataNasc) return '-';
    const hoje = new Date();
    const nasc = new Date(dataNasc);
    let idade = hoje.getFullYear() - nasc.getFullYear();
    const m = hoje.getMonth() - nasc.getMonth();
    if (m < 0 || (m === 0 && hoje.getDate() < nasc.getDate())) {
        idade--;
    }
    return idade;
};

const formatData = (d) => d ? new Date(d).toLocaleDateString('pt-BR') : '-';
</script>

<template>
    <BaseModal 
        :isOpen="isOpen" 
        title="Prontuário do Paciente" 
        maxWidth="750px"
        @close="emit('close')"
    >
        <div v-if="loading" class="loading">Carregando ficha...</div>

        <div v-else-if="paciente" class="ficha-container">
            
            <section class="header-section">
                <div class="avatar-placeholder">{{ paciente.nome.charAt(0) }}</div>
                <div class="header-info">
                    <h2>{{ paciente.nome }}</h2>
                    <p class="sub-info">
                        {{ calcularIdade(paciente.nascimento) }} anos • CPF: {{ paciente.cpf }}
                    </p>
                    <div class="badges">
                        <span v-if="paciente.ativo" class="tag-active">Ativo</span>
                        <span class="tag-contact">
                            <i class="bi bi-telephone-fill"></i> {{ paciente.telefone || 'Sem telefone' }}
                        </span>
                    </div>
                </div>
            </section>

            <div class="alert-box" v-if="paciente.condicoesMedicas">
                <i class="bi bi-exclamation-triangle-fill"></i> <strong>Alerta:</strong> {{ paciente.condicoesMedicas }}
            </div>

            <div class="tabs-ficha">
                <button :class="{ active: tabAtiva === 'evolucao' }" @click="tabAtiva = 'evolucao'">
                    <i class="bi bi-file-medical-fill"></i> Evolução Clínica
                </button>
                <button :class="{ active: tabAtiva === 'externo' }" @click="tabAtiva = 'externo'">
                    <i class="bi bi-box-seam"></i> Histórico Externo
                </button>
            </div>

            <section v-if="tabAtiva === 'evolucao'" class="history-section">
                <div v-if="historicoInterno.length > 0" class="timeline">
                    <div 
                        v-for="c in historicoInterno" 
                        :key="c.idConsulta" 
                        class="timeline-item clickable"
                        @click="abrirDetalhesConsulta(c.idConsulta)"
                        title="Clique para ver detalhes completos"
                    >
                        <div class="timeline-date">{{ formatData(c.dataInicio) }}</div>
                        <div class="timeline-content">
                            <strong>{{ c.descricao }}</strong>
                            <div v-if="c.diagnostico" class="detail-row">
                                <span class="label">Diag:</span> {{ c.diagnostico }}
                            </div>
                            <i class="bi bi-eye-fill icon-view"></i>
                        </div>
                    </div>
                </div>
                <div v-else class="empty-text">
                    Nenhum atendimento clínico registrado no sistema.
                </div>
            </section>

            <section v-if="tabAtiva === 'externo'" class="history-section">
                <div v-if="historicoExterno.length > 0" class="timeline">
                    <div v-for="item in historicoExterno" :key="item.idTratamentoExterno" class="timeline-item">
                        <div class="timeline-date">{{ formatData(item.dataRealizacao) }}</div>
                        <div class="timeline-content">
                            <strong>{{ item.descricao }}</strong>
                            <span class="local-tag">{{ item.local }}</span>
                            <p v-if="item.observacoes">{{ item.observacoes }}</p>
                        </div>
                    </div>
                </div>
                <div v-else class="empty-text">
                    Nenhum registro de laboratório/externo encontrado.
                </div>
            </section>

        </div>

        <template #footer>
            <button class="btn-close-modal" @click="emit('close')">Fechar Ficha</button>
        </template>

        <ConsultaFicha 
            :isOpen="showDetalhesConsulta" 
            :consultaId="consultaSelecionadaId"
            @close="showDetalhesConsulta = false" 
        />
    </BaseModal>
</template>

<style scoped>
.loading { text-align: center; padding: 2rem; color: var(--text-secondary); }

.header-section { display: flex; align-items: center; gap: 1rem; margin-bottom: 1rem; }
.avatar-placeholder {
    width: 60px; height: 60px; background-color: var(--accent-color); color: white;
    border-radius: 50%; display: flex; justify-content: center; align-items: center;
    font-size: 1.8rem; font-weight: bold;
}
.header-info h2 { margin: 0; color: var(--text-primary); }
.sub-info { margin: 0.2rem 0 0.5rem 0; color: var(--text-secondary); }
.badges { display: flex; gap: 0.5rem; }
.tag-active { background: #e8f5e9; color: #2ecc71; padding: 2px 8px; border-radius: 4px; font-size: 0.8rem; font-weight: bold; }
.tag-contact { background: #f4f6f8; color: var(--text-secondary); padding: 2px 8px; border-radius: 4px; font-size: 0.8rem; border: 1px solid var(--border-color); display: flex; align-items: center; gap: 0.3rem; }

.alert-box { background-color: rgba(231, 76, 60, 0.1); border: 1px solid rgba(231, 76, 60, 0.3); padding: 0.8rem; border-radius: 6px; margin-bottom: 1rem; color: #c0392b; display: flex; align-items: center; gap: 0.5rem; }

.tabs-ficha { display: flex; gap: 1rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem; margin-bottom: 1rem; }
.tabs-ficha button { background: none; border: none; padding: 0.5rem 1rem; cursor: pointer; color: var(--text-secondary); font-weight: 600; display: flex; gap: 0.5rem; }
.tabs-ficha button.active { color: var(--accent-color); border-bottom: 2px solid var(--accent-color); }

.timeline { display: flex; flex-direction: column; gap: 1rem; border-left: 2px solid var(--border-color); padding-left: 1.5rem; margin-left: 0.5rem; }
.timeline-item { position: relative; }
.timeline-item::before {
    content: ''; position: absolute; left: -1.95rem; top: 5px;
    width: 12px; height: 12px; background: var(--accent-color); border-radius: 50%; border: 2px solid var(--bg-secondary);
}
.timeline-date { font-size: 0.8rem; color: var(--text-secondary); margin-bottom: 0.2rem; }
.timeline-content { background: var(--bg-primary); padding: 0.8rem; border-radius: 6px; border: 1px solid var(--border-color); position: relative; transition: transform 0.2s; }

.clickable .timeline-content { cursor: pointer; }
.clickable .timeline-content:hover { border-color: var(--accent-color); background-color: white; box-shadow: 0 2px 5px rgba(0,0,0,0.05); }
.icon-view { position: absolute; top: 10px; right: 10px; color: var(--accent-color); opacity: 0.5; }
.clickable:hover .icon-view { opacity: 1; }

.local-tag { float: right; font-size: 0.75rem; background: var(--bg-secondary); padding: 2px 6px; border-radius: 4px; border: 1px solid var(--border-color); }
.timeline-content p { margin: 0.5rem 0 0 0; font-size: 0.9rem; font-style: italic; color: var(--text-secondary); }

.detail-row { font-size: 0.9rem; margin-top: 0.3rem; }
.label { font-weight: bold; color: var(--text-secondary); font-size: 0.8rem; text-transform: uppercase; }

.empty-text { color: var(--text-secondary); font-style: italic; padding: 0.5rem; }

.btn-close-modal { background-color: var(--text-secondary); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; font-weight: bold; }
</style>
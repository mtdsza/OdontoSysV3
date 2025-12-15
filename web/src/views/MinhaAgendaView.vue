<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import axios from 'axios';
import PacienteFicha from '@/components/pacientes/PacienteFicha.vue';

const router = useRouter();
const consultas = ref([]);
const loading = ref(true);
const error = ref('');
const dentistaInfo = ref(null);

const showProntuario = ref(false);
const pacienteEmAtendimento = ref(0); 

const getDataLocal = () => {
    const d = new Date();
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
};

const dataHoje = getDataLocal();

onMounted(() => {
    identificarDentista();
});

const identificarDentista = async () => {
    try {
        const resMe = await axios.get('http://localhost:5000/api/dentista/perfil');
        dentistaInfo.value = resMe.data.data;
        carregarAgenda(dentistaInfo.value.idDentista);
    } catch (e) {
        error.value = "Você não possui um perfil de dentista vinculado a este usuário.";
        loading.value = false;
    }
};

const carregarAgenda = async (idDentista) => {
    try {
        const res = await axios.get(`http://localhost:5000/api/consulta/buscar-por-dentista/${idDentista}`);
        
        const todas = res.data.data.map(c => ({
            ...c,
            dataInicio: c.dataInicio.endsWith('Z') ? c.dataInicio : c.dataInicio + 'Z'
        }));

        consultas.value = todas
            .filter(c => {
                const d = new Date(c.dataInicio);
                const ano = d.getFullYear();
                const mes = String(d.getMonth() + 1).padStart(2, '0');
                const dia = String(d.getDate()).padStart(2, '0');
                return `${ano}-${mes}-${dia}` === dataHoje && c.situacao !== 2;
            })
            .sort((a, b) => new Date(a.dataInicio) - new Date(b.dataInicio));
    } catch (e) {
        error.value = "Erro ao carregar agenda.";
    } finally {
        loading.value = false;
    }
};

const abrirFicha = (idPaciente) => {
    pacienteEmAtendimento.value = idPaciente;
    showProntuario.value = true;
};

const irParaAtendimento = (idConsulta) => {
    router.push(`/atendimento/${idConsulta}`);
};

const formatHora = (d) => new Date(d).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
</script>

<template>
    <div class="page-container">
        <header class="page-header">
            <div class="header-title">
                <h2>Painel Clínico</h2>
                <p v-if="dentistaInfo" class="subtitle">Dr(a). {{ dentistaInfo.nome }} • Agenda de Hoje</p>
            </div>
            <div class="date-display">
                <i class="bi bi-calendar-day"></i> 
                {{ new Date().toLocaleDateString('pt-BR', { weekday: 'long', day:'numeric', month:'long' }) }}
            </div>
        </header>

        <div v-if="loading" class="loading-state">Carregando perfil...</div>
        <div v-if="error" class="error-state">{{ error }}</div>

        <div v-if="!loading && !error" class="timeline">
            
            <div v-if="consultas.length === 0" class="empty-state">
                <i class="bi bi-cup-hot" style="font-size: 2rem; display: block; margin-bottom: 1rem;"></i>
                Nenhuma consulta agendada para hoje. Aproveite o descanso!
            </div>

            <div 
                v-for="c in consultas" 
                :key="c.idConsulta" 
                class="card-consulta"
                :class="{ 'realizada': c.situacao === 1 }"
            >
                <div class="time-col">
                    <span class="hora">{{ formatHora(c.dataInicio) }}</span>
                    <span class="status-dot" :class="c.situacao === 1 ? 'green' : 'blue'"></span>
                </div>
                
                <div class="info-col">
                    <h3>{{ c.nomePaciente }}</h3>
                    <p class="motivo">{{ c.descricao || 'Consulta de Rotina' }}</p>
                </div>

                <div class="actions-col">
                    <span v-if="c.situacao === 1" class="badge-done">
                        <i class="bi bi-check-circle-fill"></i> Atendido
                    </span>
                    
                    <div v-else class="btn-group">
                        <button class="btn-prontuario" @click="abrirFicha(c.idPaciente)">
                            <i class="bi bi-file-earmark-medical"></i> Ficha
                        </button>
                        <button class="btn-check" @click="irParaAtendimento(c.idConsulta)">
                            <i class="bi bi-play-circle-fill"></i> Atender
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <PacienteFicha 
            :isOpen="showProntuario" 
            :pacienteId="pacienteEmAtendimento"
            @close="showProntuario = false"
        />
    </div>
</template>

<style scoped>
.page-container { max-width: 900px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; border-bottom: 1px solid var(--border-color); padding-bottom: 1rem; }
h2 { margin: 0; color: var(--text-primary); }
.subtitle { margin: 0.2rem 0 0; color: var(--text-secondary); font-size: 1rem; font-weight: 500; }
.date-display { background: var(--bg-secondary); padding: 0.5rem 1rem; border-radius: 20px; font-weight: bold; text-transform: capitalize; border: 1px solid var(--border-color); color: var(--text-primary); display: flex; align-items: center; gap: 0.5rem; }

.timeline { display: flex; flex-direction: column; gap: 1.5rem; }

.card-consulta {
    display: flex; align-items: center;
    background: var(--bg-secondary);
    border-radius: 12px;
    padding: 1.5rem;
    box-shadow: var(--shadow-sm);
    border-left: 5px solid var(--accent-color);
    transition: transform 0.2s;
}
.card-consulta:hover { transform: translateY(-2px); }
.card-consulta.realizada { border-left-color: var(--success-color); opacity: 0.8; background: var(--bg-primary); }

.time-col { display: flex; flex-direction: column; align-items: center; margin-right: 1.5rem; min-width: 60px; }
.hora { font-size: 1.4rem; font-weight: bold; color: var(--text-primary); }
.status-dot { width: 10px; height: 10px; border-radius: 50%; margin-top: 0.5rem; }
.blue { background: var(--accent-color); }
.green { background: var(--success-color); }

.info-col { flex: 1; }
.info-col h3 { margin: 0 0 0.3rem 0; font-size: 1.2rem; color: var(--text-primary); }
.motivo { margin: 0; color: var(--text-secondary); }

.actions-col { min-width: 200px; display: flex; justify-content: flex-end; }
.btn-group { display: flex; gap: 0.8rem; }

.btn-prontuario {
    background: var(--bg-primary); color: var(--text-primary); border: 1px solid var(--border-color);
    padding: 0.6rem 1rem; border-radius: 6px; cursor: pointer; font-weight: bold;
    display: flex; align-items: center; gap: 0.5rem;
}
.btn-prontuario:hover { background: var(--border-color); }

.btn-check {
    background: var(--accent-color); color: white; border: none;
    padding: 0.6rem 1.2rem; border-radius: 6px; cursor: pointer; font-weight: bold;
    display: flex; align-items: center; gap: 0.5rem;
}
.btn-check:hover { filter: brightness(1.1); }

.badge-done { color: var(--success-color); font-weight: bold; border: 1px solid var(--success-color); padding: 0.4rem 1rem; border-radius: 20px; display: flex; align-items: center; gap: 0.5rem; }

.empty-state { text-align: center; padding: 4rem; color: var(--text-secondary); font-size: 1.2rem; background: var(--bg-secondary); border-radius: 8px; border: 1px dashed var(--border-color); }
.error-state { color: var(--danger-color); text-align: center; padding: 2rem; }
</style>
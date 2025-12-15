<script setup>
import { ref, onMounted, watch, computed } from 'vue';
import axios from 'axios';
import ConsultaFicha from '@/components/consultas/ConsultaFicha.vue';

const dentistas = ref([]);
const pacientes = ref([]);
const consultasDoMes = ref([]); 
const consultasDoDia = ref([]); 

const loading = ref(false);
const showModal = ref(false);
const error = ref('');

const showDetalhes = ref(false);
const consultaIdSelecionada = ref(0);

const filtroDentista = ref('');

const getDataLocal = () => {
    const d = new Date();
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
};

const dataSelecionada = ref(getDataLocal()); 
const mesAtual = ref(new Date()); 

const modoVisualizacao = ref('calendario'); 

const form = ref({
    idPaciente: '',
    idDentista: '',
    dataInicio: '', 
    horaInicio: '', 
    duracaoMinutos: 30,
    descricao: ''
});

onMounted(() => {
    carregarDadosBasicos();
});

const carregarDadosBasicos = async () => {
    try {
        const [resDentistas, resPacientes] = await Promise.all([
            axios.get('http://localhost:5000/api/dentista/buscartodos'),
            axios.get('http://localhost:5000/api/paciente/buscartodos')
        ]);
        dentistas.value = resDentistas.data.data;
        pacientes.value = resPacientes.data.data;
    } catch (err) {
        error.value = "Erro ao carregar dados.";
    }
};

watch([filtroDentista, mesAtual], async () => {
    if (filtroDentista.value) {
        await carregarConsultasDoMes();
        filtrarConsultasDoDia();
    } else {
        consultasDoMes.value = [];
        consultasDoDia.value = [];
    }
});

watch(dataSelecionada, () => {
    filtrarConsultasDoDia();
});

const carregarConsultasDoMes = async () => {
    loading.value = true;
    try {
        const response = await axios.get(`http://localhost:5000/api/consulta/buscar-por-dentista/${filtroDentista.value}`);
        
        consultasDoMes.value = response.data.data.map(c => ({
            ...c,
            dataInicio: c.dataInicio.endsWith('Z') ? c.dataInicio : c.dataInicio + 'Z'
        }));

    } catch (err) {
        console.error(err);
    } finally {
        loading.value = false;
    }
};

const filtrarConsultasDoDia = () => {
    if (!consultasDoMes.value.length) {
        consultasDoDia.value = [];
        return;
    }
    
    consultasDoDia.value = consultasDoMes.value
        .filter(c => {
            const dataItem = new Date(c.dataInicio);
            const ano = dataItem.getFullYear();
            const mes = String(dataItem.getMonth() + 1).padStart(2, '0');
            const dia = String(dataItem.getDate()).padStart(2, '0');
            const dataItemStr = `${ano}-${mes}-${dia}`;
            
            return dataItemStr === dataSelecionada.value;
        })
        .sort((a, b) => new Date(a.dataInicio) - new Date(b.dataInicio));
};

const nomeMesAno = computed(() => {
    return mesAtual.value.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
});

const diasCalendario = computed(() => {
    const ano = mesAtual.value.getFullYear();
    const mes = mesAtual.value.getMonth();
    
    const primeiroDiaSemana = new Date(ano, mes, 1).getDay(); 
    const ultimoDiaMes = new Date(ano, mes + 1, 0).getDate();
    
    const dias = [];

    for (let i = 0; i < primeiroDiaSemana; i++) {
        dias.push({ dia: '', data: null });
    }

    for (let i = 1; i <= ultimoDiaMes; i++) {
        const dataFull = `${ano}-${String(mes + 1).padStart(2, '0')}-${String(i).padStart(2, '0')}`;
        
        const temConsulta = consultasDoMes.value.some(c => {
            if (c.situacao === 2) return false;
            const d = new Date(c.dataInicio);
            const diaC = d.getDate();
            const mesC = d.getMonth();
            const anoC = d.getFullYear();
            return diaC === i && mesC === mes && anoC === ano;
        });

        dias.push({
            dia: i,
            data: dataFull,
            temConsulta: temConsulta,
            selecionado: dataFull === dataSelecionada.value
        });
    }

    return dias;
});

const mudarMes = (delta) => {
    const novaData = new Date(mesAtual.value);
    novaData.setMonth(novaData.getMonth() + delta);
    mesAtual.value = novaData;
};

const selecionarDia = (dia) => {
    if (!dia.data) return;
    dataSelecionada.value = dia.data;
};

const abrirAgendamento = () => {
    if (!filtroDentista.value) return alert("Selecione um dentista.");
    
    form.value = {
        idPaciente: '',
        idDentista: filtroDentista.value,
        dataInicio: dataSelecionada.value, 
        horaInicio: '08:00',
        duracaoMinutos: 30,
        descricao: ''
    };
    showModal.value = true;
};

const salvarConsulta = async () => {
    if (!form.value.idPaciente || !form.value.horaInicio) return alert("Preencha dados.");

    const dataLocal = new Date(`${form.value.dataInicio}T${form.value.horaInicio}:00`);
    const dataIsoUtc = dataLocal.toISOString();

    const dto = {
        idPaciente: form.value.idPaciente,
        idDentista: form.value.idDentista,
        dataInicio: dataIsoUtc,
        descricao: form.value.descricao,
        situacao: 0 
    };

    try {
        await axios.post('http://localhost:5000/api/consulta/agendar', dto);
        alert("Consulta agendada!");
        showModal.value = false;
        await carregarConsultasDoMes(); 
        filtrarConsultasDoDia();
    } catch (err) {
        alert(err.response?.data?.message || "Erro ao agendar.");
    }
};

const cancelarConsulta = async (id) => {
    if (!confirm("Cancelar?")) return;
    try {
        await axios.delete(`http://localhost:5000/api/consulta/cancelar/${id}`);
        await carregarConsultasDoMes();
        filtrarConsultasDoDia();
    } catch (err) { alert("Erro ao cancelar."); }
};

const verDetalhes = (id) => {
    consultaIdSelecionada.value = id;
    showDetalhes.value = true;
};

const formatHora = (dataStr) => {
    const iso = dataStr.endsWith('Z') ? dataStr : dataStr + 'Z';
    return new Date(iso).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
};

const formatDataBr = (dataIso) => {
    if(!dataIso) return '';
    const [ano, mes, dia] = dataIso.split('-');
    return `${dia}/${mes}/${ano}`;
};
</script>

<template>
    <div class="page-container">
        <header class="page-header">
            <div class="header-title">
                <h2>Agenda</h2>
                <p class="subtitle">Gestão de Consultas e Horários</p>
            </div>
            <div class="header-actions">
                <button class="btn-primary" @click="abrirAgendamento" :disabled="!filtroDentista">
                    <i class="bi bi-plus-lg"></i> Agendar
                </button>
            </div>
        </header>

        <div class="filters-bar">
            <div class="filter-group grow">
                <label><i class="bi bi-person-badge"></i> Dentista:</label>
                <select v-model="filtroDentista">
                    <option value="">-- Selecione o Profissional --</option>
                    <option v-for="d in dentistas" :key="d.idDentista" :value="d.idDentista">
                        {{ d.nome }}
                    </option>
                </select>
            </div>
            
            <div class="view-toggle">
                <button :class="{ active: modoVisualizacao === 'calendario' }" @click="modoVisualizacao = 'calendario'">
                    <i class="bi bi-calendar-month"></i>
                </button>
                <button :class="{ active: modoVisualizacao === 'lista' }" @click="modoVisualizacao = 'lista'">
                    <i class="bi bi-list-ul"></i>
                </button>
            </div>
        </div>

        <div v-if="loading" class="loading-state">Carregando agenda...</div>

        <div v-if="!filtroDentista" class="empty-state">
            <i class="bi bi-arrow-up-circle"></i> Selecione um dentista acima para visualizar a agenda.
        </div>

        <div v-if="filtroDentista && !loading" class="agenda-layout">
            
            <div class="calendar-panel" v-if="modoVisualizacao === 'calendario'">
                <div class="calendar-header">
                    <button @click="mudarMes(-1)" class="btn-nav"><i class="bi bi-chevron-left"></i></button>
                    <span class="month-label">{{ nomeMesAno }}</span>
                    <button @click="mudarMes(1)" class="btn-nav"><i class="bi bi-chevron-right"></i></button>
                </div>
                
                <div class="calendar-grid">
                    <div class="day-name" v-for="d in ['Dom','Seg','Ter','Qua','Qui','Sex','Sáb']" :key="d">{{ d }}</div>

                    <div 
                        v-for="(dia, index) in diasCalendario" 
                        :key="index"
                        class="day-cell"
                        :class="{ 
                            'empty': !dia.dia, 
                            'selected': dia.selecionado,
                            'has-event': dia.temConsulta 
                        }"
                        @click="selecionarDia(dia)"
                    >
                        <span v-if="dia.dia">{{ dia.dia }}</span>
                        <div v-if="dia.temConsulta" class="event-dot"></div>
                    </div>
                </div>
            </div>

            <div class="list-panel" :class="{ 'full-width': modoVisualizacao === 'lista' }">
                <div class="list-header">
                    <h3>Agenda de {{ formatDataBr(dataSelecionada) }}</h3>
                    <input type="date" v-model="dataSelecionada" class="date-input-small" />
                </div>

                <div v-if="consultasDoDia.length === 0" class="empty-day">
                    <p>Nenhum agendamento para este dia.</p>
                    <button class="btn-link" @click="abrirAgendamento">Agendar agora</button>
                </div>

                <div class="lista-consultas">
                    <div 
                        v-for="c in consultasDoDia" 
                        :key="c.idConsulta" 
                        class="consulta-card" 
                        :class="{
                            'cancelada': c.situacao === 2,
                            'realizada': c.situacao === 1
                        }"
                    >
                        <div class="time-box">
                            <span class="hora">{{ formatHora(c.dataInicio) }}</span>
                        </div>
                        <div class="info-box">
                            <h4>{{ c.nomePaciente }}</h4>
                            <p class="desc">{{ c.descricao || 'Sem descrição' }}</p>
                            <span class="status-badge badge-cancel" v-if="c.situacao === 2">Cancelada</span>
                            <span class="status-badge badge-done" v-if="c.situacao === 1">✔ Realizada</span>
                        </div>
                        <div class="action-box">
                            <button v-if="c.situacao === 0" class="btn-cancel" @click="cancelarConsulta(c.idConsulta)" title="Cancelar">
                                <i class="bi bi-x-circle"></i>
                            </button>
                            <button v-if="c.situacao === 1" class="btn-view" @click="verDetalhes(c.idConsulta)" title="Ver Detalhes">
                                <i class="bi bi-eye-fill"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div v-if="showModal" class="modal-overlay">
            <div class="modal">
                <h3>Novo Agendamento</h3>
                <form @submit.prevent="salvarConsulta">
                    <label>Paciente</label>
                    <select v-model="form.idPaciente" required>
                        <option value="">-- Selecione --</option>
                        <option v-for="p in pacientes" :key="p.idPaciente" :value="p.idPaciente">
                            {{ p.nome }}
                        </option>
                    </select>

                    <div class="row">
                        <div class="col">
                            <label>Data</label>
                            <input type="date" v-model="form.dataInicio" required />
                        </div>
                        <div class="col">
                            <label>Horário</label>
                            <input type="time" v-model="form.horaInicio" required />
                        </div>
                    </div>

                    <label>Observação / Procedimento</label>
                    <input v-model="form.descricao" placeholder="Ex: Avaliação inicial" />

                    <div class="modal-footer">
                        <button type="button" class="btn-sec" @click="showModal = false">Cancelar</button>
                        <button type="submit" class="btn-pri">Confirmar</button>
                    </div>
                </form>
            </div>
        </div>

        <ConsultaFicha 
            :isOpen="showDetalhes" 
            :consultaId="consultaIdSelecionada" 
            @close="showDetalhes = false" 
        />
    </div>
</template>

<style scoped>
.page-container { max-width: 1100px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
h2 { margin: 0; color: var(--text-primary); }
.subtitle { margin: 0.2rem 0 0; color: var(--text-secondary); font-size: 0.9rem; }

.filters-bar { 
    display: flex; gap: 1rem; margin-bottom: 1.5rem; 
    background: var(--bg-secondary); padding: 1rem; 
    border-radius: 8px; border: 1px solid var(--border-color);
    align-items: flex-end;
}
.filter-group { display: flex; flex-direction: column; gap: 0.5rem; }
.grow { flex: 1; }
.filter-group label { font-weight: bold; color: var(--text-primary); font-size: 0.9rem; }
select { width: 100%; padding: 0.7rem; background: var(--bg-primary); border: 1px solid var(--border-color); color: var(--text-primary); border-radius: 4px; }

.view-toggle { display: flex; border: 1px solid var(--border-color); border-radius: 6px; overflow: hidden; }
.view-toggle button {
    background: var(--bg-primary); border: none; padding: 0.6rem 1rem; cursor: pointer; color: var(--text-secondary);
    border-right: 1px solid var(--border-color);
}
.view-toggle button:last-child { border-right: none; }
.view-toggle button.active { background: var(--accent-color); color: white; }

.agenda-layout { display: flex; gap: 1.5rem; flex-wrap: wrap; }

.calendar-panel { 
    flex: 1; min-width: 300px; 
    background: var(--bg-secondary); border-radius: 8px; 
    border: 1px solid var(--border-color); padding: 1rem; 
    box-shadow: var(--shadow-sm);
}
.calendar-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
.month-label { font-weight: bold; text-transform: capitalize; color: var(--text-primary); }
.btn-nav { background: none; border: none; cursor: pointer; color: var(--text-secondary); font-size: 1.2rem; }
.btn-nav:hover { color: var(--accent-color); }

.calendar-grid { display: grid; grid-template-columns: repeat(7, 1fr); gap: 5px; text-align: center; }
.day-name { font-size: 0.8rem; font-weight: bold; color: var(--text-secondary); padding-bottom: 0.5rem; }
.day-cell { 
    padding: 0.8rem 0; border-radius: 6px; cursor: pointer; 
    background: var(--bg-primary); color: var(--text-primary);
    position: relative; transition: all 0.2s;
}
.day-cell:hover { background: rgba(52, 152, 219, 0.1); }
.day-cell.empty { background: transparent; cursor: default; }
.day-cell.selected { background: var(--accent-color); color: white; font-weight: bold; }
.event-dot { 
    width: 6px; height: 6px; background-color: var(--success-color); 
    border-radius: 50%; position: absolute; bottom: 4px; left: 50%; transform: translateX(-50%); 
}
.day-cell.selected .event-dot { background-color: white; }

.list-panel { 
    flex: 1; min-width: 300px; 
    background: var(--bg-secondary); border-radius: 8px; 
    border: 1px solid var(--border-color); padding: 1.5rem; 
    box-shadow: var(--shadow-sm); display: flex; flex-direction: column;
}
.list-panel.full-width { flex: 100%; }

.list-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
.list-header h3 { margin: 0; font-size: 1.1rem; color: var(--text-primary); }
.date-input-small { padding: 0.4rem; border: 1px solid var(--border-color); border-radius: 4px; background: var(--bg-primary); color: var(--text-primary); }

.lista-consultas { display: flex; flex-direction: column; gap: 0.8rem; overflow-y: auto; max-height: 500px; }

.consulta-card {
    display: flex; align-items: center;
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: 8px;
    padding: 0.8rem;
    border-left: 4px solid var(--accent-color);
    transition: transform 0.2s;
}
.consulta-card:hover { transform: translateX(3px); }

.time-box { font-weight: bold; color: var(--accent-color); margin-right: 1rem; font-size: 1.1rem; }
.info-box { flex: 1; }
.info-box h4 { margin: 0 0 0.2rem 0; color: var(--text-primary); font-size: 1rem; }
.desc { margin: 0; color: var(--text-secondary); font-size: 0.85rem; }

.action-box { margin-left: 0.5rem; }
.btn-cancel { background: transparent; border: none; color: var(--danger-color); font-size: 1.2rem; cursor: pointer; opacity: 0.7; }
.btn-cancel:hover { opacity: 1; transform: scale(1.1); }
.btn-view { background: transparent; border: none; color: var(--accent-color); font-size: 1.2rem; cursor: pointer; }
.btn-view:hover { transform: scale(1.1); }

.consulta-card.cancelada { opacity: 0.6; border-left-color: var(--danger-color); }
.consulta-card.realizada { border-left-color: var(--success-color); background: rgba(39, 174, 96, 0.05); }
.status-badge { padding: 2px 6px; border-radius: 4px; font-size: 0.7rem; font-weight: bold; display: inline-block; margin-top: 0.2rem; }
.badge-cancel { background: var(--danger-color); color: white; }
.badge-done { background: var(--success-color); color: white; }

.empty-state { text-align: center; padding: 3rem; color: var(--text-secondary); background: rgba(0,0,0,0.02); border-radius: 8px; border: 1px dashed var(--border-color); }
.empty-day { text-align: center; padding: 2rem; color: var(--text-secondary); }
.btn-link { background: none; border: none; color: var(--accent-color); cursor: pointer; text-decoration: underline; font-weight: bold; }

.modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); display: flex; justify-content: center; align-items: center; z-index: 1000; }
.modal { background: var(--bg-secondary); padding: 2rem; border-radius: 8px; width: 450px; border: 1px solid var(--border-color); color: var(--text-primary); }
.row { display: flex; gap: 1rem; }
.col { flex: 1; }
input { width: 100%; padding: 0.7rem; background: var(--bg-primary); border: 1px solid var(--border-color); color: var(--text-primary); border-radius: 4px; margin-bottom: 1rem; }
label { display: block; margin-bottom: 0.5rem; color: var(--text-primary); }
.modal-footer { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1rem; }
.btn-pri { background: var(--success-color); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; }
.btn-sec { background: var(--text-secondary); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; }
.btn-primary { background: var(--accent-color); color: white; border: none; padding: 0.8rem 1.5rem; border-radius: 6px; font-weight: bold; cursor: pointer; }
.btn-primary:disabled { background: var(--border-color); cursor: not-allowed; }

@media (max-width: 800px) {
    .agenda-layout { flex-direction: column; }
    .calendar-panel, .list-panel { width: 100%; }
}
</style>
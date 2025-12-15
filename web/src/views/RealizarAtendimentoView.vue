<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import axios from 'axios';

const route = useRoute();
const router = useRouter();

const consulta = ref(null);
const paciente = ref(null);
const historico = ref([]);
const estoque = ref([]);
const consumo = ref([]); 

const loading = ref(true);
const processing = ref(false);

const form = ref({
    diagnostico: '',
    prescricao: '',
    observacoes: ''
});

const itemSelecionado = ref('');
const qtdSelecionada = ref(1);

onMounted(async () => {
    const id = route.params.idConsulta;
    if (id) await carregarDados(id);
});

const carregarDados = async (idConsulta) => {
    try {
        const resConsulta = await axios.get(`http://localhost:5000/api/consulta/${idConsulta}`);
        consulta.value = resConsulta.data.data;
        
        form.value.diagnostico = consulta.value.diagnostico || '';
        form.value.prescricao = consulta.value.prescricao || '';
        form.value.observacoes = consulta.value.observacoesDaConsulta || consulta.value.descricao || '';

        const [resPac, resEstoque, resTodasConsultas] = await Promise.all([
            axios.get(`http://localhost:5000/api/paciente/${consulta.value.idPaciente}`),
            axios.get('http://localhost:5000/api/estoque/listar'),
            axios.get('http://localhost:5000/api/consulta/buscartodos') 
        ]);

        paciente.value = resPac.data.data;
        estoque.value = resEstoque.data.data;

        historico.value = resTodasConsultas.data.data
            .filter(c => c.idPaciente === consulta.value.idPaciente && c.situacao === 1 && c.idConsulta !== consulta.value.idConsulta)
            .sort((a, b) => new Date(b.dataInicio) - new Date(a.dataInicio));

    } catch (e) {
        alert("Erro ao carregar atendimento.");
        router.push('/meus-atendimentos');
    } finally {
        loading.value = false;
    }
};

const adicionarMaterial = () => {
    if (!itemSelecionado.value || qtdSelecionada.value <= 0) return;
    
    const material = estoque.value.find(i => i.idItemEstoque === itemSelecionado.value);
    
    consumo.value.push({
        idItemEstoque: material.idItemEstoque,
        descricao: material.descricao,
        quantidade: qtdSelecionada.value
    });

    itemSelecionado.value = '';
    qtdSelecionada.value = 1;
};

const removerMaterial = (index) => {
    consumo.value.splice(index, 1);
};

const finalizarAtendimento = async () => {
    if (!confirm("Confirmar finalização e baixa no estoque?")) return;
    
    processing.value = true;
    try {
        const dtoConsulta = {
            ...consulta.value,
            situacao: 1, 
            diagnostico: form.value.diagnostico,
            prescricao: form.value.prescricao,
            descricao: form.value.observacoes 
        };
        await axios.put('http://localhost:5000/api/consulta/atualizar', dtoConsulta);

        for (const item of consumo.value) {
            await axios.post('http://localhost:5000/api/estoque/registrar-uso', {
                idConsulta: consulta.value.idConsulta,
                idItemEstoque: item.idItemEstoque,
                quantidade: item.quantidade
            });
        }

        alert("Atendimento finalizado com sucesso!");
        router.push('/meus-atendimentos');
    } catch (e) {
        alert("Erro ao salvar dados. Tente novamente.");
    } finally {
        processing.value = false;
    }
};

const formatData = (d) => new Date(d).toLocaleDateString('pt-BR');
</script>

<template>
    <div class="page-container" v-if="!loading">
        <header class="atendimento-header">
            <div class="header-info">
                <h2><i class="bi bi-clipboard2-pulse-fill"></i> Atendimento em Andamento</h2>
                <div class="paciente-info" v-if="paciente">
                    <strong>{{ paciente.nome }}</strong>
                    <span class="divider">•</span>
                    <span>{{ new Date().getFullYear() - new Date(paciente.nascimento).getFullYear() }} anos</span>
                </div>
            </div>
            <button class="btn-finish" @click="finalizarAtendimento" :disabled="processing">
                <i class="bi bi-check-circle-fill"></i> {{ processing ? 'Salvando...' : 'Concluir Atendimento' }}
            </button>
        </header>

        <div v-if="paciente?.condicoesMedicas" class="alerta-medico">
            <i class="bi bi-exclamation-triangle-fill"></i> <strong>Alerta Médico:</strong> {{ paciente.condicoesMedicas }}
        </div>

        <div class="grid-layout">
            
            <div class="main-panel">
                <div class="section-card">
                    <h3><i class="bi bi-pencil-square"></i> Evolução Clínica</h3>
                    
                    <div class="form-group">
                        <label>Queixa / Procedimento Realizado</label>
                        <textarea v-model="form.observacoes" rows="3" placeholder="Relato do procedimento..."></textarea>
                    </div>

                    <div class="form-group">
                        <label>Diagnóstico</label>
                        <input v-model="form.diagnostico" placeholder="CID ou diagnóstico descritivo" />
                    </div>

                    <div class="form-group">
                        <label>Prescrição / Receita</label>
                        <textarea v-model="form.prescricao" rows="3" placeholder="Medicamentos e orientações..."></textarea>
                    </div>
                </div>

                <div class="section-card">
                    <h3><i class="bi bi-box-seam"></i> Materiais Utilizados (Baixa de Estoque)</h3>
                    <div class="stock-input-row">
                        <select v-model="itemSelecionado" class="grow">
                            <option value="">-- Selecione o Material --</option>
                            <option v-for="item in estoque" :key="item.idItemEstoque" :value="item.idItemEstoque">
                                {{ item.descricao }} (Saldo: {{ item.quantidade }})
                            </option>
                        </select>
                        <input type="number" v-model="qtdSelecionada" min="0.1" step="0.1" class="w-small" />
                        <button class="btn-add" @click="adicionarMaterial">
                            <i class="bi bi-plus-lg"></i>
                        </button>
                    </div>

                    <ul class="stock-list">
                        <li v-for="(use, idx) in consumo" :key="idx">
                            <span>{{ use.quantidade }}x {{ use.descricao }}</span>
                            <button class="btn-icon-small" @click="removerMaterial(idx)">
                                <i class="bi bi-trash"></i>
                            </button>
                        </li>
                        <li v-if="consumo.length === 0" class="empty-stock">Nenhum material lançado.</li>
                    </ul>
                </div>
            </div>

            <div class="side-panel">
                <h3><i class="bi bi-clock-history"></i> Histórico Recente</h3>
                <div class="timeline">
                    <div v-for="h in historico" :key="h.idConsulta" class="timeline-item">
                        <div class="timeline-date">{{ formatData(h.dataInicio) }}</div>
                        <div class="timeline-content">
                            <strong>{{ h.descricao }}</strong>
                            <p v-if="h.diagnostico">Diag: {{ h.diagnostico }}</p>
                        </div>
                    </div>
                    <div v-if="historico.length === 0" class="empty-hist">
                        Primeira consulta registrada.
                    </div>
                </div>
            </div>

        </div>
    </div>
    <div v-else class="loading-full">Carregando ambiente...</div>
</template>

<style scoped>
.page-container { max-width: 1200px; margin: 0 auto; height: 90vh; display: flex; flex-direction: column; }

.atendimento-header { 
    display: flex; justify-content: space-between; align-items: center; 
    background: var(--bg-secondary); padding: 1rem 1.5rem; 
    border-radius: 8px; border: 1px solid var(--border-color);
    margin-bottom: 1rem;
}
.header-info h2 { margin: 0; font-size: 1.4rem; color: var(--accent-color); }
.paciente-info { margin-top: 0.3rem; font-size: 1.1rem; color: var(--text-primary); }
.divider { margin: 0 0.5rem; color: var(--text-secondary); }

.btn-finish { 
    background-color: var(--success-color); color: white; border: none; 
    padding: 0.8rem 1.5rem; border-radius: 6px; font-weight: bold; 
    cursor: pointer; font-size: 1rem; display: flex; align-items: center; gap: 0.5rem;
}
.btn-finish:hover { filter: brightness(1.1); }
.btn-finish:disabled { opacity: 0.7; cursor: not-allowed; }

.alerta-medico {
    background-color: #ffebee; color: #c62828; border: 1px solid #ef9a9a;
    padding: 0.8rem 1rem; border-radius: 6px; margin-bottom: 1rem;
    display: flex; align-items: center; gap: 0.5rem;
}

.grid-layout { display: grid; grid-template-columns: 2fr 1fr; gap: 1.5rem; flex: 1; overflow: hidden; }

.main-panel { display: flex; flex-direction: column; gap: 1.5rem; overflow-y: auto; padding-right: 0.5rem; }
.section-card { 
    background: var(--bg-secondary); padding: 1.5rem; 
    border-radius: 8px; border: 1px solid var(--border-color); 
}
.section-card h3 { margin-top: 0; font-size: 1.1rem; color: var(--text-primary); border-bottom: 1px solid var(--border-color); padding-bottom: 0.8rem; margin-bottom: 1rem; }

.form-group { margin-bottom: 1rem; }
label { display: block; margin-bottom: 0.4rem; font-weight: 500; color: var(--text-secondary); }
input, textarea, select { 
    width: 100%; padding: 0.8rem; border-radius: 4px; 
    border: 1px solid var(--border-color); background: var(--bg-primary); 
    color: var(--text-primary); font-family: inherit;
}

.stock-input-row { display: flex; gap: 0.5rem; margin-bottom: 1rem; }
.grow { flex: 1; }
.w-small { width: 80px; }
.btn-add { background: var(--accent-color); color: white; border: none; width: 40px; border-radius: 4px; cursor: pointer; }

.stock-list { list-style: none; padding: 0; margin: 0; }
.stock-list li { 
    display: flex; justify-content: space-between; align-items: center; 
    padding: 0.6rem; border-bottom: 1px solid var(--border-color); 
}
.btn-icon-small { background: none; border: none; color: var(--danger-color); cursor: pointer; }
.empty-stock { color: var(--text-secondary); font-style: italic; text-align: center; padding: 1rem; }

.side-panel { 
    background: var(--bg-secondary); padding: 1.5rem; 
    border-radius: 8px; border: 1px solid var(--border-color); 
    overflow-y: auto; 
}
.side-panel h3 { margin-top: 0; font-size: 1.1rem; color: var(--text-primary); margin-bottom: 1rem; }

.timeline { display: flex; flex-direction: column; gap: 1rem; border-left: 2px solid var(--border-color); padding-left: 1rem; }
.timeline-item { position: relative; }
.timeline-item::before {
    content: ''; position: absolute; left: -1.35rem; top: 6px;
    width: 10px; height: 10px; background: var(--accent-color); border-radius: 50%;
}
.timeline-date { font-size: 0.8rem; color: var(--text-secondary); margin-bottom: 0.2rem; }
.timeline-content { background: var(--bg-primary); padding: 0.8rem; border-radius: 6px; font-size: 0.9rem; }
.empty-hist { color: var(--text-secondary); font-style: italic; }

.loading-full { display: flex; justify-content: center; align-items: center; height: 80vh; font-size: 1.5rem; color: var(--text-secondary); }
</style>
<script setup>
import { ref, watch } from 'vue';
import axios from 'axios';
import BaseModal from '@/components/ui/BaseModal.vue';

const props = defineProps({
    isOpen: Boolean,
    consultaId: Number
});

const emit = defineEmits(['close']);

const consulta = ref(null);
const loading = ref(false);

watch(() => props.isOpen, async (val) => {
    if (val && props.consultaId) {
        await carregarDetalhes(props.consultaId);
    } else {
        consulta.value = null;
    }
});

const carregarDetalhes = async (id) => {
    loading.value = true;
    try {
        const res = await axios.get(`http://localhost:5000/api/consulta/${id}`);
        consulta.value = res.data.data;
    } catch (e) {
        console.error("Erro ao carregar consulta", e);
    } finally {
        loading.value = false;
    }
};

const formatDataHora = (d) => {
    if (!d) return '-';
    return new Date(d).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' });
};
</script>

<template>
    <BaseModal 
        :isOpen="isOpen" 
        title="Detalhes do Atendimento" 
        maxWidth="600px"
        @close="emit('close')"
    >
        <div v-if="loading" class="loading">Carregando detalhes...</div>

        <div v-else-if="consulta" class="detalhes-container">
            <div class="header-info">
                <span class="data-badge">
                    <i class="bi bi-calendar-event"></i> {{ formatDataHora(consulta.dataInicio) }}
                </span>
                <span class="status-badge" :class="consulta.situacao === 1 ? 'ok' : 'pending'">
                    {{ consulta.situacao === 1 ? 'Realizada' : (consulta.situacao === 2 ? 'Cancelada' : 'Agendada') }}
                </span>
            </div>

            <div class="field-group">
                <label>Queixa / Procedimento Relatado</label>
                <div class="box-content">{{ consulta.descricao || 'Não informado.' }}</div>
            </div>

            <div class="field-group">
                <label>Diagnóstico</label>
                <div class="box-content">{{ consulta.diagnostico || 'Não informado.' }}</div>
            </div>

            <div class="field-group">
                <label>Prescrição / Receita</label>
                <div class="box-content highlight">{{ consulta.prescricao || 'Nenhuma prescrição.' }}</div>
            </div>
            
            <div class="footer-info">
                <small>Dentista: {{ consulta.nomeDentista }}</small>
                <small>Paciente: {{ consulta.nomePaciente }}</small>
            </div>
        </div>

        <template #footer>
            <button class="btn-close-modal" @click="emit('close')">Fechar</button>
        </template>
    </BaseModal>
</template>

<style scoped>
.loading { text-align: center; padding: 2rem; color: var(--text-secondary); }

.header-info { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
.data-badge { font-size: 1.1rem; font-weight: bold; color: var(--text-primary); }
.status-badge { padding: 4px 10px; border-radius: 12px; font-size: 0.8rem; font-weight: bold; text-transform: uppercase; }
.status-badge.ok { background: #e8f5e9; color: var(--success-color); border: 1px solid var(--success-color); }
.status-badge.pending { background: #fff3e0; color: #f39c12; border: 1px solid #f39c12; }

.field-group { margin-bottom: 1.2rem; }
label { display: block; font-weight: 600; color: var(--text-secondary); font-size: 0.85rem; margin-bottom: 0.3rem; text-transform: uppercase; letter-spacing: 0.5px; }
.box-content { background: var(--bg-primary); padding: 1rem; border-radius: 6px; border: 1px solid var(--border-color); color: var(--text-primary); line-height: 1.5; white-space: pre-wrap; }
.box-content.highlight { background: #f4f6f8; border-left: 4px solid var(--accent-color); }

.footer-info { margin-top: 2rem; border-top: 1px solid var(--border-color); padding-top: 1rem; display: flex; justify-content: space-between; color: var(--text-secondary); }

.btn-close-modal { background-color: var(--text-secondary); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; font-weight: bold; }
</style>
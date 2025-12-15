<script setup>
import { ref } from 'vue';
import axios from 'axios';
import BaseModal from '@/components/ui/BaseModal.vue';

const props = defineProps({
    isOpen: Boolean
});

const emit = defineEmits(['close']);

const form = ref({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
});

const loading = ref(false);

const resetForm = () => {
    form.value = { currentPassword: '', newPassword: '', confirmPassword: '' };
};

const handleChange = async () => {
    if (form.value.newPassword !== form.value.confirmPassword) {
        alert("A nova senha e a confirmação não coincidem.");
        return;
    }
    if (form.value.newPassword.length < 6) {
        alert("A nova senha deve ter no mínimo 6 caracteres.");
        return;
    }

    loading.value = true;
    try {
        await axios.post('http://localhost:5000/api/auth/alterar-senha', {
            currentPassword: form.value.currentPassword,
            newPassword: form.value.newPassword
        });
        alert("Senha alterada com sucesso!");
        emit('close');
        resetForm();
    } catch (e) {
        const msg = e.response?.data?.message || "Erro ao alterar senha. Verifique sua senha atual.";
        alert(msg);
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <BaseModal 
        :isOpen="isOpen" 
        title="Alterar Minha Senha" 
        maxWidth="400px"
        @close="emit('close')"
    >
        <form @submit.prevent="handleChange" class="form-pass">
            <div class="field">
                <label>Senha Atual</label>
                <input type="password" v-model="form.currentPassword" required />
            </div>
            
            <div class="field">
                <label>Nova Senha</label>
                <input type="password" v-model="form.newPassword" required minlength="6" />
            </div>

            <div class="field">
                <label>Confirmar Nova Senha</label>
                <input type="password" v-model="form.confirmPassword" required minlength="6" />
            </div>

            <div class="actions">
                <button type="button" class="btn-cancel" @click="emit('close')">Cancelar</button>
                <button type="submit" class="btn-save" :disabled="loading">
                    {{ loading ? 'Salvando...' : 'Alterar Senha' }}
                </button>
            </div>
        </form>
    </BaseModal>
</template>

<style scoped>
.form-pass { display: flex; flex-direction: column; gap: 1rem; }
.field label { display: block; margin-bottom: 0.4rem; color: var(--text-primary); font-size: 0.9rem; font-weight: 500; }
.field input { width: 100%; padding: 0.7rem; border-radius: 4px; border: 1px solid var(--border-color); background: var(--bg-primary); color: var(--text-primary); }
.actions { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1rem; }
.btn-save { background: var(--accent-color); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; font-weight: bold; }
.btn-save:hover { filter: brightness(1.1); }
.btn-save:disabled { opacity: 0.7; cursor: not-allowed; }
.btn-cancel { background: transparent; border: 1px solid var(--border-color); color: var(--text-secondary); padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; }
</style>
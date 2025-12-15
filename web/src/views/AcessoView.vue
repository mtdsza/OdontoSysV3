<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import BaseModal from '@/components/ui/BaseModal.vue';
import { useAuthStore } from '@/stores/auth';

const authStore = useAuthStore();
const usuarios = ref([]);
const funcionarios = ref([]);
const loading = ref(false);

const showModalCriar = ref(false);
const showModalSenha = ref(false);

const form = ref({
    modo: 'vinculado',
    idFuncionario: '',
    emailLogin: '',
    senha: '',
    perfil: 'Secretaria'
});

const formReset = ref({
    email: '',
    novaSenha: ''
});

onMounted(() => {
    fetchUsuarios();
    fetchFuncionarios();
});

const fetchUsuarios = async () => {
    loading.value = true;
    try {
        const res = await axios.get('http://localhost:5000/api/usuario/listar');
        usuarios.value = res.data.data;
    } catch (e) { 
        alert("Erro ao carregar usuários."); 
    } finally { 
        loading.value = false; 
    }
};

const fetchFuncionarios = async () => {
    try {
        const res = await axios.get('http://localhost:5000/api/funcionario/buscartodos');
        funcionarios.value = res.data.data;
    } catch (e) { 
        console.error("Erro ao carregar lista para vínculo."); 
    }
};

const abrirCriar = () => {
    form.value = { modo: 'vinculado', idFuncionario: '', emailLogin: '', senha: '', perfil: 'Secretaria' };
    showModalCriar.value = true;
};

const salvar = async () => {
    try {
        if (!form.value.emailLogin || !form.value.senha) return alert("Email e Senha são obrigatórios.");
        
        if (form.value.modo === 'admin') {
            await axios.post('http://localhost:5000/api/usuario/criar-admin-sistema', {
                email: form.value.emailLogin,
                password: form.value.senha,
                confirmPassword: form.value.senha
            });
        } else {
            if (!form.value.idFuncionario) return alert("Selecione um funcionário para vincular.");
            
            await axios.post('http://localhost:5000/api/usuario/criar-vinculado', {
                idFuncionario: form.value.idFuncionario,
                emailLogin: form.value.emailLogin,
                senha: form.value.senha,
                perfil: form.value.perfil
            });
        }
        alert("Acesso criado com sucesso!");
        showModalCriar.value = false;
        fetchUsuarios();
    } catch (e) {
        alert("Erro: " + (e.response?.data?.message || e.message));
    }
};

const removerUsuario = async (id) => {
    if (!confirm("Remover este acesso? O funcionário continuará existindo, mas não poderá mais logar.")) return;
    try {
        await axios.delete(`http://localhost:5000/api/usuario/${id}`);
        fetchUsuarios();
    } catch (e) { alert("Erro ao remover usuário."); }
};

const abrirResetSenha = (usuario) => {
    formReset.value = {
        email: usuario.email,
        novaSenha: '' 
    };
    showModalSenha.value = true;
};

const confirmarResetSenha = async () => {
    if (formReset.value.novaSenha.length < 6) {
        alert("A senha deve ter no mínimo 6 caracteres.");
        return;
    }

    const dto = {
        email: formReset.value.email,
        novaSenha: formReset.value.novaSenha 
    };

    try {
        await axios.post('http://localhost:5000/api/usuario/reset-senha-admin', dto);
        alert(`Senha de ${formReset.value.email} redefinida com sucesso!`);
        showModalSenha.value = false;
    } catch (err) {
        const msg = err.response?.data?.message || "Erro ao redefinir senha.";
        alert(msg);
    }
};
</script>

<template>
    <div class="page-container">
        <header class="page-header">
            <div class="header-title">
                <h2>Controle de Acessos</h2>
                <p class="subtitle">Gerenciamento de Logins e Segurança</p>
            </div>
            <button class="btn-primary" @click="abrirCriar">
                <i class="bi bi-person-plus-fill"></i> Novo Acesso
            </button>
        </header>

        <div v-if="!loading" class="table-responsive">
            <table class="tabela">
                <thead>
                    <tr>
                        <th>Login (Email)</th>
                        <th>Perfil (Role)</th>
                        <th>Vinculado a</th>
                        <th style="text-align: right;">Ações</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="u in usuarios" :key="u.id">
                        <td class="bold">{{ u.email }}</td>
                        <td>
                            <span class="badge">{{ u.roles[0] || 'User' }}</span>
                        </td>
                        <td>
                            <span v-if="u.vinculadoA" class="tag-link">
                                <i class="bi bi-link-45deg"></i> {{ u.vinculadoA.nome }}
                            </span>
                            <span v-else class="tag-admin">
                                <i class="bi bi-shield-check"></i> Administrador
                            </span>
                        </td>
                        <td class="actions-cell">
                            <button 
                                class="btn-icon btn-key" 
                                @click="abrirResetSenha(u)" 
                                title="Alterar Senha"
                            >
                                <i class="bi bi-key-fill"></i>
                            </button>

                            <button 
                                class="btn-icon btn-del" 
                                @click="removerUsuario(u.id)" 
                                :disabled="u.email === authStore.user?.email"
                                :title="u.email === authStore.user?.email ? 'Você não pode revogar seu próprio acesso' : 'Revogar Acesso'"
                                :style="u.email === authStore.user?.email ? { opacity: 0.3, cursor: 'not-allowed' } : {}"
                            >
                                <i class="bi bi-person-x-fill"></i>
                            </button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <BaseModal :isOpen="showModalCriar" title="Criar Novo Acesso" @close="showModalCriar = false">
            <form @submit.prevent="salvar" class="form-col">
                
                <div class="toggle-type">
                    <label>Tipo de Acesso:</label>
                    <div class="radios">
                        <label><input type="radio" v-model="form.modo" value="vinculado"> Funcionário</label>
                        <label><input type="radio" v-model="form.modo" value="admin"> Suporte</label>
                    </div>
                </div>

                <div v-if="form.modo === 'vinculado'" class="fade-in">
                    <label>Selecione o Funcionário</label>
                    <select v-model="form.idFuncionario" required>
                        <option value="">-- Selecione --</option>
                        <option v-for="f in funcionarios" :key="f.idFuncionario" :value="f.idFuncionario">
                            {{ f.nome }} ({{ f.email }})
                        </option>
                    </select>
                </div>

                <div>
                    <label>Email de Login</label>
                    <input type="email" v-model="form.emailLogin" required placeholder="usuario@sistema.com" />
                </div>

                <div>
                    <label>Senha Provisória</label>
                    <input type="password" v-model="form.senha" required minlength="6" />
                </div>

                <div v-if="form.modo === 'vinculado'">
                    <label>Perfil de Permissão</label>
                    <select v-model="form.perfil">
                        <option value="Secretaria">Secretaria</option>
                        <option value="Dentista">Dentista</option>
                        <option value="Admin">Administrador</option>
                    </select>
                </div>

                <div class="footer-btns">
                    <button type="button" class="btn-sec" @click="showModalCriar = false">Cancelar</button>
                    <button type="submit" class="btn-pri">Criar Acesso</button>
                </div>
            </form>
        </BaseModal>

        <BaseModal :isOpen="showModalSenha" title="Redefinir Senha do Usuário" @close="showModalSenha = false" maxWidth="400px">
            <form @submit.prevent="confirmarResetSenha" class="form-col">
                <p>Defina uma nova senha para <strong>{{ formReset.email }}</strong>.</p>
                
                <div>
                    <label>Nova Senha</label>
                    <input type="password" v-model="formReset.novaSenha" required minlength="6" placeholder="Mínimo 6 caracteres" />
                </div>

                <div class="footer-btns">
                    <button type="button" class="btn-sec" @click="showModalSenha = false">Cancelar</button>
                    <button type="submit" class="btn-warning">Salvar Nova Senha</button>
                </div>
            </form>
        </BaseModal>
    </div>
</template>

<style scoped>
.page-container { max-width: 1000px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
h2 { margin: 0; color: var(--text-primary); }
.subtitle { margin: 0.2rem 0 0; color: var(--text-secondary); font-size: 0.9rem; }

.table-responsive { background: var(--bg-secondary); border-radius: 8px; border: 1px solid var(--border-color); }
.tabela { width: 100%; border-collapse: collapse; }
.tabela th { background: var(--bg-primary); padding: 1rem; text-align: left; color: var(--text-secondary); border-bottom: 1px solid var(--border-color); }
.tabela td { padding: 1rem; border-bottom: 1px solid var(--border-color); color: var(--text-primary); }

.bold { font-weight: bold; }
.badge { background: var(--bg-primary); padding: 2px 8px; border-radius: 4px; font-size: 0.85rem; border: 1px solid var(--border-color); }
.tag-link { color: var(--success-color); font-weight: 500; display: inline-flex; align-items: center; gap: 0.3rem; }
.tag-admin { color: var(--accent-color); font-weight: 500; display: inline-flex; align-items: center; gap: 0.3rem; }

.btn-primary { background: var(--accent-color); color: white; border: none; padding: 0.8rem 1.5rem; border-radius: 6px; font-weight: bold; cursor: pointer; }
.btn-icon { background: none; border: none; cursor: pointer; font-size: 1.2rem; padding: 0.5rem; transition: transform 0.2s; }
.btn-icon:hover { transform: scale(1.1); background-color: var(--bg-primary); border-radius: 50%; }
.btn-del { color: var(--danger-color); }
.btn-key { color: #f1c40f; margin-right: 0.5rem; }

.actions-cell { text-align: right; }

.form-col { display: flex; flex-direction: column; gap: 1rem; }
.toggle-type { background: var(--bg-primary); padding: 1rem; border-radius: 6px; border: 1px solid var(--border-color); }
.radios { display: flex; gap: 1.5rem; margin-top: 0.5rem; }
.radios label { display: flex; align-items: center; gap: 0.5rem; cursor: pointer; font-weight: normal; }

label { display: block; margin-bottom: 0.4rem; font-weight: 500; color: var(--text-primary); font-size: 0.9rem; }
input, select { width: 100%; padding: 0.7rem; border-radius: 4px; border: 1px solid var(--border-color); background: var(--bg-secondary); color: var(--text-primary); }
small { display: block; margin-top: 0.3rem; color: var(--text-secondary); font-size: 0.8rem; }

.footer-btns { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1rem; }
.btn-pri { background: var(--success-color); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; font-weight: bold; }
.btn-sec { background: var(--text-secondary); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; }
.btn-warning { background: #e67e22; color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 4px; cursor: pointer; font-weight: bold; }

.fade-in { animation: fadeIn 0.3s; }
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
</style>
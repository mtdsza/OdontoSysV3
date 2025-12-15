<script setup>
defineProps({
    title: { type: String, default: 'Título' },
    isOpen: { type: Boolean, required: true },
    maxWidth: { type: String, default: '600px' }
});

const emit = defineEmits(['close']);
</script>

<template>
    <div v-if="isOpen" class="modal-overlay" @click.self="emit('close')">
        <div class="modal-card" :style="{ maxWidth: maxWidth }">
            <header class="modal-header">
                <h3>{{ title }}</h3>
                <button class="btn-close" @click="emit('close')" title="Fechar">
                    <i class="bi bi-x-lg"></i>
                </button>
            </header>
            
            <div class="modal-body">
                <slot></slot>
            </div>

            <footer class="modal-footer" v-if="$slots.footer">
                <slot name="footer"></slot>
            </footer>
        </div>
    </div>
</template>

<style scoped>
.modal-overlay {
    position: fixed; top: 0; left: 0; width: 100%; height: 100%;
    background: rgba(0,0,0,0.6);
    display: flex; justify-content: center; align-items: center;
    z-index: 9999;
    padding: 1rem;
    backdrop-filter: blur(2px);
}

.modal-card {
    background: var(--bg-secondary);
    border-radius: 8px;
    width: 100%;
    max-height: 90vh;
    display: flex; flex-direction: column;
    box-shadow: 0 10px 25px rgba(0,0,0,0.3);
    border: 1px solid var(--border-color);
    animation: fadeIn 0.2s ease-out;
}

.modal-header {
    padding: 1.2rem 1.5rem;
    border-bottom: 1px solid var(--border-color);
    display: flex; justify-content: space-between; align-items: center;
}

.modal-header h3 { margin: 0; color: var(--text-primary); font-size: 1.2rem; }

.btn-close {
    background: none; border: none; font-size: 1.2rem;
    cursor: pointer; color: var(--text-secondary);
    padding: 0.2rem 0.5rem; border-radius: 4px;
    transition: color 0.2s;
}
.btn-close:hover { color: var(--danger-color); }

.modal-body { padding: 1.5rem; overflow-y: auto; }

.modal-footer {
    padding: 1.2rem 1.5rem;
    border-top: 1px solid var(--border-color);
    display: flex; justify-content: flex-end; gap: 1rem;
    background-color: var(--bg-primary);
    border-bottom-left-radius: 8px; border-bottom-right-radius: 8px;
}

@keyframes fadeIn {
    from { opacity: 0; transform: translateY(-10px); }
    to { opacity: 1; transform: translateY(0); }
}
</style>
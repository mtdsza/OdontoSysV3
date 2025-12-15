<script setup>
import { RouterView } from 'vue-router';
import AppSidebar from '@/components/AppSidebar.vue';
</script>

<template>
  <div class="app-layout">
    <AppSidebar />
    
    <main class="content-area">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
.app-layout {
  display: flex;
  min-height: 100vh;
}

.content-area {
  flex: 1;
  margin-left: 250px;
  padding: 2rem;
  background-color: var(--bg-primary);
  color: var(--text-primary);
  transition: margin-left 0.3s, background-color 0.3s, color 0.3s;
}
</style>
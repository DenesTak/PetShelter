import { createApp } from 'vue';
import { createRouter, createWebHistory } from 'vue-router';
import App from './App.vue';
import Shelters from './components/Shelters/shelters-component.vue';
import Pets from './components/Pets/pets-component.vue';

const routes = [
    { path: '/shelters', component: Shelters },
    { path: '/pets', component: Pets },
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

createApp(App).use(router).mount('#app');
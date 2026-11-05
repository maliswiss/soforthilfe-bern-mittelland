import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';
import router from './router';
import { i18n } from './i18n';
// Schriften werden mit der Anwendung ausgeliefert (kein Abruf bei Drittanbietern).
import '@fontsource/archivo/latin-500.css';
import '@fontsource/archivo/latin-600.css';
import '@fontsource/archivo/latin-700.css';
import '@fontsource/source-sans-3/latin-400.css';
import '@fontsource/source-sans-3/latin-500.css';
import '@fontsource/source-sans-3/latin-600.css';
import './style.css';

const anwendung = createApp(App);
anwendung.use(createPinia());
anwendung.use(i18n);
anwendung.use(router);
anwendung.mount('#app');

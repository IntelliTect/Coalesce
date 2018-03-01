import './css/site.scss';

// import "babel-polyfill";
import Vue from 'vue';

// Vuetify imports
import 'vuetify/dist/vuetify.min.css';
import Vuetify from 'vuetify';
Vue.use(Vuetify);

import App from './components/app.vue';
import Admin from './components/admin-table-page.vue'

import VueRouter from 'vue-router';
Vue.use(VueRouter);

const routes = [
    { path: '/', component: Admin },
];

new Vue({
    el: '#app-root',
    router: new VueRouter({ mode: 'history', routes: routes }),
    render: h => h(App)
});

import './css/site.scss';

import "babel-polyfill";
import Vue from 'vue';

// Vuetify imports
import 'vuetify/dist/vuetify.min.css';
import Vuetify from 'vuetify';
Vue.use(Vuetify);

import App from './components/app.vue';
import Home from './components/home.vue';
import Counter from './components/counter.vue';
import FetchData from './components/fetchdata.vue';
import Event from './components/event.vue';
import Admin from './components/admin-table-page.vue'

import VueRouter from 'vue-router';
Vue.use(VueRouter);

const routes = [
    { path: '/', component: Admin },
    { path: '/counter', component: Counter },
    { path: '/fetchdata', component: FetchData },
    { path: '/event', component: Event }
];

new Vue({
    el: '#app-root',
    router: new VueRouter({ mode: 'history', routes: routes }),
    render: h => h(App)
});

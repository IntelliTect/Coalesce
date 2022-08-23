import './css/site.scss';

// import "babel-polyfill";
import Vue from 'vue';

import 'typeface-roboto';
import 'typeface-roboto-mono';
import '@fortawesome/fontawesome-free/css/all.css'
//import 'coalesce-vue-vuetify/dist/coalesce-vue-vuetify.css'

// Vuetify imports
import Vuetify from 'vuetify'
import 'vuetify/dist/vuetify.min.css'
Vue.use(Vuetify);

import App from './components/app.vue';
import { AxiosClient } from 'coalesce-vue'

import VueRouter from 'vue-router';
Vue.use(VueRouter);

import $metadata from '@/metadata.g';

// viewmodels.g has sideeffects - it populates the global lookup on ViewModel and ListViewModel.
import '@/viewmodels.g';

import CoalesceVuetify, { CAdminTablePage, CAdminEditorPage } from 'coalesce-vue-vuetify';
Vue.use(CoalesceVuetify, {
  metadata: $metadata
});

// @ts-ignore
const components: any = Vue.options.components;
components.VInput.options.props.dense.default = true
components.VTextField.options.props.dense.default = true
components.VTextField.options.props.outlined.default = true

Vue.config.productionTip = false;

AxiosClient.defaults.baseURL = '/api'
AxiosClient.defaults.withCredentials = true

const router = new VueRouter({ mode: 'history', routes: [
  
  { path: '/', redirect: '/admin/Person' },
  {
    path: '/test',
    component: () => import("./components/test.vue"),  },

  { path: '/admin/:type', 
    name: 'coalesce-admin-list', 
    component: CAdminTablePage, 
    props: r => ({
      type: r.params.type
    }) 
  },
  { path: '/admin/:type/edit/:id?', 
    name: 'coalesce-admin-item', 
    component: CAdminEditorPage, 
    props: r => ({
      type: r.params.type, 
      id: r.params.id
    }) 
  },
]});

new Vue({
    el: '#app',
    router,
    vuetify: new Vuetify({
      icons: {
        iconfont: 'fa', // 'mdi' || 'mdiSvg' || 'md' || 'fa' || 'fa4'
      },
      customProperties: true,
      theme: { 
        options: {
          customProperties: true,
        },
        themes: {
          light: {
            // primary: "#9ccc6f",
            // secondary: "#4d97bc",
            // accent: "#e98f07",
            error: '#df323b', // This is the default error color with darken-1
          }
        }
      }
    }),
    render: h => h(App)
});

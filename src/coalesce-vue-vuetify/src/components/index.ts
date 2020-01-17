
import _Vue from "vue";

import CDisplay from './c-display'
import CEditorDialog from './c-editor-dialog.vue';
import CInput from './c-input';
import CInputPropsProvider from './c-input-props-provider.vue';
import CSelect from './c-select.vue';
import CSelectStringValue from './c-select-string-value.vue';
import CTable from './c-table.vue';
import CMethods from './c-methods.vue';
import CMethodsDialog from './c-methods-dialog.vue';
import CDatetimePicker from './c-datetime-picker.vue';

import CAdminTableToolbar from './c-admin-table-toolbar.vue';
import CPagination from './c-pagination.vue';
import CPaginationPage from './c-pagination-page.vue';

export default function install(Vue: typeof _Vue) {
  Vue.component('c-admin-table-toolbar', CAdminTableToolbar);
  Vue.component('c-pagination', CPagination);
  Vue.component('c-pagination-page', CPaginationPage);

  Vue.component('c-display', CDisplay);

  Vue.component('c-input-props-provider', CInputPropsProvider);

  Vue.component('c-input', CInput);
  Vue.component('c-select', CSelect);
  Vue.component('c-select-string-value', CSelectStringValue);
  Vue.component('c-datetime-picker', CDatetimePicker);

  Vue.component('c-editor-dialog', CEditorDialog);
  Vue.component('c-table', CTable);
  Vue.component('c-methods', CMethods);
  Vue.component('c-methods-dialog', CMethodsDialog);
}
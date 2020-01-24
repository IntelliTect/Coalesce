
import _Vue from "vue";

// In alphabetical order:
import CAdminDisplay from './admin/c-admin-display';
import CAdminEditorPage from './admin/c-admin-editor-page.vue';
import CAdminEditor from './admin/c-admin-editor.vue';
import CAdminMethod from './admin/c-admin-method.vue';
import CAdminMethods from './admin/c-admin-methods.vue';
import CAdminTablePage from './admin/c-admin-table-page.vue';
import CAdminTableToolbar from './admin/c-admin-table-toolbar.vue';
import CAdminTable from './admin/c-admin-table.vue';
import CDatetimePicker from './input/c-datetime-picker.vue';
import CDisplay from './display/c-display'
// import CEditorDialog from './admin/c-editor-dialog.vue';
import CInputPropsProvider from './input/c-input-props-provider.vue';
import CInput from './input/c-input';
import CLoaderStatus from './display/c-loader-status.vue';
// import CMethodsDialog from './admin/c-methods-dialog.vue';
import CListPage from './input/c-list-page.vue';
import CListPagination from './input/c-list-pagination.vue';
import CSelectManyToMany from './input/c-select-many-to-many.vue';
import CSelectStringValue from './input/c-select-string-value.vue';
import CSelect from './input/c-select.vue';
import CTable from './display/c-table.vue';


export {
  CAdminDisplay,
  CAdminEditorPage,
  CAdminEditor,
  CAdminMethod,
  CAdminMethods,
  CAdminTablePage,
  CAdminTableToolbar,
  CAdminTable,
  CDatetimePicker,
  CDisplay,
  // CEditorDialog,
  CInputPropsProvider,
  CInput,
  CLoaderStatus,
  // CMethodsDialog,
  CListPage,
  CListPagination,
  CSelectManyToMany,
  CSelectStringValue,
  CSelect,
  CTable,
};

export default function install(Vue: typeof _Vue) {
  // In alphabetical order:
  Vue.component('c-admin-display', CAdminDisplay);
  Vue.component('c-admin-editor-page', CAdminEditorPage);
  Vue.component('c-admin-editor', CAdminEditor);
  Vue.component('c-admin-method', CAdminMethod);
  Vue.component('c-admin-methods', CAdminMethods);
  Vue.component('c-admin-table-page', CAdminTablePage);
  Vue.component('c-admin-table-toolbar', CAdminTableToolbar);
  Vue.component('c-admin-table', CAdminTable);
  Vue.component('c-datetime-picker', CDatetimePicker);
  Vue.component('c-display', CDisplay);
  // Vue.component('c-editor-dialog', CEditorDialog);
  Vue.component('c-input-props-provider', CInputPropsProvider);
  Vue.component('c-input', CInput);
  Vue.component('c-loader-status', CLoaderStatus);
  // Vue.component('c-methods-dialog', CMethodsDialog);
  Vue.component('c-list-page', CListPage);
  Vue.component('c-list-pagination', CListPagination);
  Vue.component('c-select-many-to-many', CSelectManyToMany);
  Vue.component('c-select-string-value', CSelectStringValue);
  Vue.component('c-select', CSelect);
  Vue.component('c-table', CTable);
}
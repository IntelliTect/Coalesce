
import _Vue from "vue";

// In alphabetical order:
import CAdminTablePage from './c-admin-table-page.vue';
import CAdminTableToolbar from './c-admin-table-toolbar.vue';
import CAdminTable from './c-admin-table.vue';
import CDatetimePicker from './c-datetime-picker.vue';
import CDisplay from './c-display'
import CEditorDialog from './c-editor-dialog.vue';
import CEditor from './c-editor.vue';
import CInputPropsProvider from './c-input-props-provider.vue';
import CInput from './c-input';
import CLoaderStatus from './c-loader-status.vue';
import CMetadataComponent from './c-metadata-component'
import CMethod from './c-method.vue';
import CMethods from './c-methods.vue';
import CMethodsDialog from './c-methods-dialog.vue';
import CPaginationPage from './c-pagination-page.vue';
import CPagination from './c-pagination.vue';
import CSelectManyToMany from './c-select-many-to-many.vue';
import CSelectStringValue from './c-select-string-value.vue';
import CSelect from './c-select.vue';
import CTable from './c-table.vue';

// import CTable from './c-table-body.vue';
// import CTable from './c-table-row.vue';

export {
  CAdminTablePage,
  CAdminTableToolbar,
  CAdminTable,
  CDatetimePicker,
  CDisplay,
  CEditorDialog,
  CEditor,
  CInputPropsProvider,
  CInput,
  CLoaderStatus,
  CMetadataComponent,
  CMethod,
  CMethods,
  CMethodsDialog,
  CPaginationPage,
  CPagination,
  CSelectManyToMany,
  CSelectStringValue,
  CSelect,
  CTable,
};

export default function install(Vue: typeof _Vue) {
  // In alphabetical order:
  Vue.component('c-admin-table-page', CAdminTablePage);
  Vue.component('c-admin-table-toolbar', CAdminTableToolbar);
  Vue.component('c-admin-table', CAdminTable);
  Vue.component('c-datetime-picker', CDatetimePicker);
  Vue.component('c-display', CDisplay);
  Vue.component('c-editor-dialog', CEditorDialog);
  Vue.component('c-editor', CEditor);
  Vue.component('c-input-props-provider', CInputPropsProvider);
  Vue.component('c-input', CInput);
  Vue.component('c-loader-status', CLoaderStatus);
  Vue.component('c-metadata-component', CMetadataComponent);
  Vue.component('c-method', CMethod);
  Vue.component('c-methods', CMethods);
  Vue.component('c-methods-dialog', CMethodsDialog);
  Vue.component('c-pagination-page', CPaginationPage);
  Vue.component('c-pagination', CPagination);
  Vue.component('c-select-many-to-many', CSelectManyToMany);
  Vue.component('c-select-string-value', CSelectStringValue);
  Vue.component('c-select', CSelect);
  Vue.component('c-table', CTable);
}
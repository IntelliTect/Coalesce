


<script lang="ts">

// *** DEPRECATED ***
// This was made to provide the same props to all c-input components throughout an application.
// A better way to do this is to just modify the defaults of the Vuetify components at startup, like so:

/*
  // @ts-ignore
  const components: any = Vue.options.components;
  components.VInput.options.props.dense.default = true;
  components.VTextField.options.props.dense.default = true;
  components.VTextField.options.props.outlined.default = true;
*/

import Vue from 'vue'
export default Vue.extend({
  name: 'c-input-props-provider',
  provide() {
    return {
      'c-input-props': this.$attrs
    }
  },
  render(h) {
    return this.$scopedSlots.default!({}) as any
  }
})
</script>
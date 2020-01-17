<script lang="ts">
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
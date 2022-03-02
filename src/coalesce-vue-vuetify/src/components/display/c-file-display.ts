import Vue, { PropOptions, VNode } from "vue";

export default Vue.extend({
  name: "c-file-display",
  props: {
    value: <PropOptions<File>>{ required: true }
  },
  data: function() {
    return {
      blobUrl: null as string | null,
    }
  },
  watch: {
    value: {
      handler: function(v, old) {
        if (this.blobUrl) {
          URL.revokeObjectURL(this.blobUrl);
        }
        this.blobUrl = URL.createObjectURL(v);
      },
      immediate: true
    }
  },

  beforeDestroy() {
    if (this.blobUrl) {
      URL.revokeObjectURL(this.blobUrl);
    }
  },

	render(h): VNode {
    const blobUrl = this.blobUrl;
    if (!blobUrl) {
      return h("span");
    }

    if (this.value.type.indexOf("image") >= 0) {
      return h("img", {
        attrs: {
          src: blobUrl,
        },
      })
    }

    return h("v-btn", {
      props: {
        href: blobUrl,
        color: 'primary',
      },
      attrs: {
        download: this.value.name,
      }
    }, [
      h('v-icon', {attrs: {left: true}}, "fa fa-download"),
      "Download"
    ])
  }
});

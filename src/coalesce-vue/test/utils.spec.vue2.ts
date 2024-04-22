import { bindToQueryString } from "../src";
import Vue from "vue";

describe("VueInstance", () => {
  test("is assignable from Vue class component", async () => {
    class MyComponent extends Vue {
      created() {
        bindToQueryString(this, { a: 1 }, "a");
      }
    }
  });
});

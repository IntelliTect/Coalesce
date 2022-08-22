// Importing @vue/test-utils will turn off the Vue devtools and productionTip warnings.
// Normally we could do this ourselves, but in order for this project to be a vue2/vue3 polyglot,
// we have to check that we're running vue2 before we try to turn these flags off.
// If we use `IsVue2` from our own util.ts file, this then makes there be a second async import
// after we import from 'vue', which gives the setTimeout(..., 0) function in Vue that prints
// these warnings time to run before we're able to actually turn the warnings off.
// So, to make things easy, we just let the default functionality in @vue/test-utils turn them off.
import "@vue/test-utils";

import { version } from "vue";

declare const TEST_EXPECTED_VUE_VERSION: number;

expect(TEST_EXPECTED_VUE_VERSION).toBe(+version.charAt(0));

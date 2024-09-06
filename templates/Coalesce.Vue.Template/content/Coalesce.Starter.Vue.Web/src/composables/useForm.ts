import { VForm } from "vuetify/components";

/** Returns a ref suitable for holding an instance of a Vuetify VForm.
 *
 * @param eagerValidate If true (default), the form will be eagerly validated,
 * showing error messages immediately instead of waiting for submission
 * or an explicit validate() call.
 */
export function useForm(eagerValidate = true) {
  const form = ref<VForm>();

  // Eagerly validate as new controls are added or removed.
  if (eagerValidate) {
    watch(
      () => JSON.stringify(form.value?.items.map((i) => i.id)),
      () => {
        form.value?.validate();
      },
    );
  }

  return form;
}

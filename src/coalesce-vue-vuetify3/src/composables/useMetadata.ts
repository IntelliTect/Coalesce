import { inject } from "vue";
import { Domain } from "coalesce-vue";

export const metadataKey = Symbol("coalesce metadata");

export function useMetadata() {
  return inject(metadataKey) as Domain;
}

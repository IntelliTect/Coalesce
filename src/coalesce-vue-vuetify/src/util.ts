import { Property } from 'coalesce-vue';

export function isPropReadOnly(p: Property) {
  return p.dontSerialize 
    && p.role !== "referenceNavigation" 
    && p.role !== "collectionNavigation"
}
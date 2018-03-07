
import { Vue, Component, Watch, Prop } from 'vue-property-decorator';
import { IHaveMetadata, Property } from 'coalesce-vue/lib/metadata'

export default abstract class extends Vue {

  @Prop({required: true, type: Object})
  public item?: IHaveMetadata;

  @Prop({required: false, type: [String, Object]})
  public prop?: string | Property;

  get propMeta(): Property {
    if (this.item == null) {
      throw `Item is required`;
    }

    if (this.item.$metadata == null) {
      throw `Item ${this.item} has no $metadata`;
    }

    let itemProps = this.item.$metadata.props;
    if (this.prop == null) {
      // No prop specified - just an item. Display the display prop of the item.
      const propMetadata = this.item.$metadata.displayProp;
      if (propMetadata == null) {
        // TODO: this isn't very good. We should maybe have "virtual computed" properties, or something like that, for doing things like jsonifying objects that have no display property.
        throw `Prop ${this.prop} has no display property`
      }
      return propMetadata;
    } else if (typeof this.prop == "string") {
      // Prop string name. Display that prop's value.
      let propMetadata = itemProps[this.prop];
      if (propMetadata == null){
        throw `Prop ${this.prop} doesn't exist on item ${this.item.$metadata.name}`
      }
      return propMetadata;
    } else {
      // Prop metadata. Display that prop's value.
      // Get the metadata fresh from the item's metadata to ensure we haven't been fed metadata that doesn't actually belong to this.item.
      let propMetadata = itemProps[this.prop.name];
      if (propMetadata == null || propMetadata !== this.prop) {
        throw `Prop ${this.prop.name} doesn't exist on item ${this.item.$metadata.name}`
      }
      return propMetadata;
    }
  }
}
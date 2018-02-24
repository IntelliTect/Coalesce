
import { CreateElement } from 'vue';
import { Vue, Component, Watch, Prop } from 'vue-property-decorator';

import MetadataComponent from './c-metadata-component'
import { IHaveMetadata, Property, ExternalType, ModelType, CollectableType } from '../core'

@Component({
  name: 'c-display',
})
export default class extends MetadataComponent {
  // @Prop() public value?: any;

  @Prop({default: 'span', type: String}) 
  public element!: string;

  @Prop({default: 'L LT', type: String}) 
  public dateFormat!: string;

  getDisplayForModel(modelMeta: ExternalType | ModelType, object: any): string | null {
    if (modelMeta.displayProp)
      return this.getPropDisplay(object, modelMeta.displayProp);
    else {
      // https://stackoverflow.com/a/46908358 - stringify only first-level properties.
      try {
        return JSON.stringify(object, function (k, v) { return k ? "" + v : v; });
      } catch {
        return object.toString();
      }
    }
  }

  getDisplayForType(type: CollectableType, value: any): string | null {
    if (value == null) return value;

    switch (type) {
      case "date":
        return value.toLocaleString()
      case "number":
      case "boolean":
      case "string":
        return value
    }
    switch (type.type) {
      case "enum":
        const enumData = type.valueLookup[value];
        if (!enumData) return null;
        return enumData.displayName;
      case "model":
      case "object":
        return this.getDisplayForModel(type, value);
    }
  }
  
  getPropDisplay(parentItem: any, prop: Property): string | null {
    var value: any = (parentItem as any)[prop.name];

    switch (prop.type) {
      case "enum":
      case "model":
      case "object":
        return this.getDisplayForType(prop.typeDef, value);
      case "collection":
        if (!value) {
          value = [];
        }
        if (!Array.isArray(value)){
          throw `Value for collection ${prop.name} was not an array`
        }

        // Is this what we want? I think so - its the cleanest option.
        // Perhaps an prop that controls this would be best.
        if (value.length == 0) return "";
        // TODO: a prop that controls this number would also be good.
        if (value.length <= 5) {
          let collectedType = prop.typeDef
          if (collectedType) {
            return (value)
              .map<string>(childItem => {
                var display = this.getDisplayForType(collectedType, childItem);
                if (display === null) display = '???' // TODO: what should this be for un-displayable members of a collection?
                return display;
              })
              .join(", ")
          } else {

          }
        }
        return value.length.toLocaleString();
      default:
        return this.getDisplayForType(prop.type, value);
    }
  }

  render(h: CreateElement) {
    const value = this.getPropDisplay(this.item, this.propMeta);
    if (value == null) {
      return null;
    }
    
    return h(this.element, value)
  }
}

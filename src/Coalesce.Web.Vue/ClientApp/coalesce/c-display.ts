
import { Vue, Component, Watch, Prop } from 'vue-property-decorator';
import { IHaveMetadata, Property, ExternalType, ModelType, CollectableType } from './metadata'
import { CreateElement } from 'vue';

@Component({
    name: 'c-display',
})
export default class extends Vue {
    // @Prop() public value?: any;

    @Prop({required: true, type: Object})
    public item?: IHaveMetadata;

    @Prop({required: false, type: [String, Object]}) 
    public prop?: string | Property;

    @Prop({default: 'span', type: String}) 
    public element: string;

    @Prop({default: 'L LT', type: String}) 
    public dateFormat?: string;

    get propMeta(): Property {
        if (this.item != null) {
            if (this.item.$metadata == null) {
                throw `Item ${this.item} has no metadata`;
            }
            const itemProps = this.item.$metadata.props;
            if (this.prop == null) {
                // No prop specified - just an item. Display the display prop of the item.
                const propMetadata = this.item.$metadata.displayProp;
                if (propMetadata == null) {
                    throw `Prop ${this.prop} has no display property`
                }
                return propMetadata;
            } else if (typeof this.prop == "string") {
                // Prop string name. Display that prop's value.
                const propMetadata = itemProps[this.prop];
                if (propMetadata == null){
                    throw `Prop ${this.prop} doesn't exist on item ${this.item.$metadata.name}`
                }
                return propMetadata;
            } else {
                // Prop metadata. Display that prop's value.
                // Get the metadata fresh from the item's metadata to ensure we haven't been fed metadata that doesn't actually belong to this.item.
                const propMetadata = itemProps[this.prop.name];
                if (propMetadata == null || propMetadata !== this.prop) {
                    throw `Prop ${this.prop.name} doesn't exist on item ${this.item.$metadata.name}`
                }
                return propMetadata;
            }
        } else {
            throw "One day we'll handle specifying an actualy value and some metadata here. For now, we don't."
        }
    }

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

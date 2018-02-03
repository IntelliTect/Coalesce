
import { Vue, Component, Watch, Prop } from 'vue-property-decorator';
import { IHaveMetadata, PropertyMetadata, EnumPropertyMetadata, ModelPropertyMetadata } from './metadata'
import { CreateElement } from 'vue';

@Component({
    name: 'c-display',
})
export default class extends Vue {
    // @Prop() public value?: any;

    @Prop({required: true, type: Object})
    public item?: IHaveMetadata;

    @Prop({required: false, type: [String, Object]}) 
    public prop?: string | PropertyMetadata;

    @Prop({default: 'span', type: String}) 
    public element: string;

    @Prop({default: 'L LT', type: String}) 
    public dateFormat?: string;

    get propValue(): { metadata: PropertyMetadata, value: any } {
        if (this.item != null) {
            if (this.item.$metadata == null) {
                throw `Item ${this.item} has no metadata`;
            }
            let itemProps = this.item.$metadata.props;
            if (this.prop == null) {
                // No prop specified - just an item. Display the display prop of the item.
                let propMetadata = this.item.$metadata.displayProp;
                return { metadata: propMetadata, value: (this.item as any)[propMetadata.name] };
            } else if (typeof this.prop == "string") {
                // Prop string name. Display that prop's value.
                let propMetadata = itemProps[this.prop];
                if (propMetadata == null){
                    throw `Prop ${this.prop} doesn't exist on item ${this.item.$metadata.name}`
                }
                return { metadata: propMetadata, value: (this.item as any)[propMetadata.name] };
            } else {
                // Prop metadata. Display that prop's value.
                // Get the metadata fresh from the item's metadata to ensure we haven't been fed metadata that doesn't actually belong to this.item.
                let propMetadata = itemProps[this.prop.name];
                if (propMetadata == null || propMetadata !== this.prop) {
                    throw `Prop ${this.prop.name} doesn't exist on item ${this.item.$metadata.name}`
                }
                return { metadata: this.prop, value: (this.item as any)[this.prop.name] };
            }
        } else {
            throw "One day we'll handle specifying an actualy value and some metadata here. For now, we don't."
        }
    }

    getPropDisplay(propMeta: PropertyMetadata, value: any): string {
        switch (propMeta.type) {
            case "date":
                return value.format(this.dateFormat)
            case "enum":
                return propMeta.values[value].strValue // TODO: should be a "displayName" prop or similar.
            case "model":
            case "object":
                let model = propMeta.model;
                return this.getPropDisplay(model.displayProp, value[model.displayProp.name]);
            case "collection":
                if (!value) {
                    value = [];
                }
                if (!Array.isArray(value)){
                    throw `Value for collection ${propMeta.name} was not an array`
                }

                // Is this what we want? I think so - its the cleanest option.
                // Perhaps an prop that controls this would be best.
                if (value.length == 0) return "";
                // TODO: a prop that controls this number would also be good.
                if (value.length <= 5) {
                    let model = propMeta.model
                    if (model) {
                        return (value as any[])
                            .map(item => model ? item[model.displayProp.name] : '')
                            .join(", ")
                    }
                }
                return value.length.toLocaleString();
            case "number":
            case "string":
                return value
        }
    }

    render(h: CreateElement) {
        const propValue = this.propValue;

        if (propValue.value == null) {
            return null;
        }
        
        return h(this.element, this.getPropDisplay(propValue.metadata, propValue.value))
    }
}

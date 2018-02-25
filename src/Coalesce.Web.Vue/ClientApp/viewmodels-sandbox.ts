
import * as metadata from './metadata.g'
import * as models from './models.g'
import { PropNames, mapToDto, ApiClient, ClassType, ModelType } from './coalesce';
import Vue from 'vue';
import { Model } from './coalesce/core/model';

abstract class ViewModel<TMeta extends ModelType, TModel extends Model<TMeta>> implements Model<TMeta> {
    abstract readonly $metadata: TMeta;

    public $apiClient: ApiClient<this>;

    public $startAutoSave(vue: Vue) {
        vue.$watch(this.$getModel.bind(this), (n, o) => {
            // TODO: this needs a lot more work.
            // Autosave needs to be disablable, we need to handle responses, etc...
            this.$apiClient.save(this);
        });
    }

    public $setModel(data: Partial<TModel>){
        for (const key in data) {
            if (key in this.$metadata.props) {
                var propName = key as PropNames<TMeta>
                (this as any)[propName] = data && (data as any)[propName] || null;
            }
        }
    }

    public $getModel(): TModel {
        const model = {};

        for (const propName in this.$metadata.props) {
            (model as any)[propName] = (this as any)[propName];
        }

        return model as TModel;
    }

    constructor(initialData: TModel | undefined, $metadata: TMeta) {
        this.$apiClient = new ApiClient<this>($metadata);

        for (const propName in $metadata.props) {
            // Ensure we always set to null when the initial data is missing - no undefined so Vue reactivity always works.
            (this as any)[propName] = initialData && (initialData as any)[propName] || null;
        }
    }
}

export class PersonViewModel extends ViewModel<typeof metadata.Person, models.Person> implements models.Person {
    readonly $metadata = metadata.Person

    personId!: number | null
    title!:  models.Titles | null
    firstName!: string | null
    lastName!: string | null
    birthDate!: Date | null
    email!: string | null;
    gender!: models.Genders | null;
    lastBath!: Date | null;
    nextUpgrade!: Date | null;
    name!: string | null;
    companyId!: number | null;

    // Would be typed as other ViewModel classes.
    personStats!: any | null
    casesAssigned!: any[] | null
    casesReported!: any[] | null
    company!: any | null

    constructor(initialData?: models.Person) {
        super(initialData, metadata.Person);
    }
}
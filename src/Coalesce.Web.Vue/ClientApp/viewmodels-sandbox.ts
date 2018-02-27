
import * as metadata from './metadata.g'
import * as models from './models.g'
import { PropNames, mapToDto, ApiClient, ClassType, ModelType, ApiResult, ItemResult, ListResult } from './coalesce';
import Vue from 'vue';
import { Model } from './coalesce/core/model';
import { AxiosResponse, AxiosError } from 'axios';


abstract class ViewModel<TMeta extends ModelType, TModel extends Model<TMeta>> implements Model<TMeta> {
    abstract readonly $metadata: TMeta
    public $data!: TModel

    public $apiClient: ApiClient<TModel>

    public $save = this.$apiClient.makeApiCaller("item",
        c => () => c.save(this.$data),
        p => p.then(resp => {
            const obj = resp.result;
            if (obj) this.$data = obj; // TODO: merge in the result, don't replace the existing one.
            return resp;
        }))
        
    public $delete = this.$apiClient.makeApiCaller("item",
        c => () => c.delete((this as any)[this.$metadata.keyProp.name]))

    public $load = this.$apiClient.makeApiCaller("item",
        c => (id?: string | number) => c.get(id != null ? id : (this as any)[this.$metadata.keyProp.name]), 
        p => p.then(resp => {
            const obj = resp.result;
            if (obj) this.$data = obj; // TODO: merge in the result, don't replace the existing one.
            return resp;
        })
    )

    public $startAutoSave(vue: Vue) {
        vue.$watch((() => mapToDto(this.$data)), (n, o) => {
            // TODO: this needs a lot more work.
            // Autosave needs to be disablable, we need to handle responses, etc...
            this.$save()
        })
    }

    constructor($metadata: TMeta) {
        this.$apiClient = new ApiClient<TModel>($metadata)

        // for (const propName in $metadata.props) {
        //     // Ensure we always set to null when the initial data is missing - no undefined so Vue reactivity always works.
        //     (this as any)[propName] = initialData && (initialData as any)[propName] || null;
        // }
    }
}

export class PersonViewModel extends ViewModel<typeof metadata.Person, models.Person> implements models.Person {
    readonly $metadata = metadata.Person

    get personId() { return this.$data.personId }
    set personId(val) { this.$data.personId = val }
    get title() { return this.$data.title }
    set title(val) { this.$data.title = val }
    get firstName() { return this.$data.firstName }
    set firstName(val) { this.$data.firstName = val }
    get lastName() { return this.$data.lastName }
    set lastName(val) { this.$data.lastName = val }
    get birthDate() { return this.$data.birthDate }
    set birthDate(val) { this.$data.birthDate = val }
    get email() { return this.$data.email }
    set email(val) { this.$data.email = val }
    get gender() { return this.$data.gender }
    set gender(val) { this.$data.gender = val }
    get lastBath() { return this.$data.lastBath }
    set lastBath(val) { this.$data.lastBath = val }
    get nextUpgrade() { return this.$data.nextUpgrade }
    set nextUpgrade(val) { this.$data.nextUpgrade = val }
    get name() { return this.$data.name }
    set name(val) { this.$data.name = val }
    get companyId() { return this.$data.companyId }
    set companyId(val) { this.$data.companyId = val }
    get personStats() { return this.$data.personStats }
    set personStats(val) { this.$data.personStats = val }
    get casesAssigned() { return this.$data.casesAssigned }
    set casesAssigned(val) { this.$data.casesAssigned = val }
    get casesReported() { return this.$data.casesReported }
    set casesReported(val) { this.$data.casesReported = val }
    get company() { return this.$data.company }
    set company(val) { this.$data.company = val }

    constructor(initialData: models.Person) {
        super(metadata.Person)

        this.$data = initialData
    }
}
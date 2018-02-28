
import * as metadata from './metadata.g'
import * as models from './models.g'
import * as api from './coalesce/core/api-client'
import { mapToDto, ApiClient, ClassType, ModelType, ApiResult, ItemResult, ListResult, hydrateModel } from './coalesce';
import Vue from 'vue';
import { Model } from './coalesce/core/model';
import { AxiosResponse, AxiosError } from 'axios';
import * as _ from 'underscore';


abstract class ViewModel<TMeta extends ModelType, TModel extends Model<TMeta>> implements Model<TMeta> {
    abstract readonly $metadata: TMeta
    public $data: TModel

    private _pristineDto: any;
    /**
     * Returns true if the values of the savable data properties of this ViewModel 
     * have changed since the last load, save, or the last time $isDirty was set to false.
     */
    public get $isDirty() { return JSON.stringify(mapToDto(this.$data)) != JSON.stringify(this._pristineDto)}
    public set $isDirty(val) { if (val) throw "Can't set $isDirty to true manually"; this._pristineDto = mapToDto(this.$data) }

    /**
     * Instance of an API client for the model through which direct, stateless API requests may be made.
     */
    // Metadata will actually be undefined here. It will be late-initialized in the ctor.
    public $apiClient: ApiClient<TModel> = new ApiClient<TModel>(this.$metadata)

    /**
     * A function for invoking the /get endpoint, and a set of properties about the state of the last call.
     */
    public $load = this.$apiClient.$caller("item",
        c => (id?: string | number) => c.get(id != null ? id : (this as any)[this.$metadata.keyProp.name]))
        // TODO: merge in the result, don't replace the existing one.
        .onFulfilled(() => { this.$data = this.$load.result || this.$data; this.$isDirty = false; })

    /**
     * A function for invoking the /save endpoint, and a set of properties about the state of the last call.
     */
    public $save = this.$apiClient.$caller("item", 
        c => () => {
            // Before we make the save call, set isDirty = false.
            // This lets us detect changes that happen to the model while our save request is pending.
            // If the model is dirty when the request completes, we'll not load the response from the server.
            this.$isDirty = false;
            return c.save(this.$data);
        })
        .onFulfilled(() => { 
            if (!this.$isDirty){
                // Only load the save response if the data hasn't changed since we sent it.
                // If the data has changed, loading the response would overwrite users' changes.
                // TODO: merge in the result, don't replace the existing one.
                this.$data = this.$save.result || this.$data; 

                // Set the new state of our data as being clean (since we just made a change to it)
                this.$isDirty = false;
            }
        })
        
    /**
     * A function for invoking the /delete endpoint, and a set of properties about the state of the last call.
     */
    public $delete = this.$apiClient.$caller("item",
        c => () => c.delete((this as any)[this.$metadata.keyProp.name]))

    // Internal autosave state - seal to prevent unnessecary reactivity
    private _autoSaveState = Object.seal<{
        on: boolean,
        watcher: (() => void) | null
        debouncer: _.Cancelable | null
    }>({ on: false, watcher: null, debouncer: null })

    /**
     * Starts auto-saving of the instance's data properties when changes occur. 
     * Only properties which will be sent in save requests are watched - 
     * navigation properties are not considered.
     * @param vue A Vue instance through which the lifecycle of the watcher will be managed.
     * @param wait Time in milliseconds to debounce saves for
     */
    public $startAutoSave(vue: Vue, wait: number = 1000) {
        this.$stopAutoSave()

        const enqueueSave = _.debounce(() => {
            if (this.$save.isLoading) {
                // Save already in progress. Enqueue another attempt.
                enqueueSave();
            } else if (this.$isDirty) {
                // No saves in progress - go ahead and save now.
                this.$save()
            }
        }, wait)

        this._autoSaveState.debouncer = enqueueSave;
        this._autoSaveState.watcher = vue.$watch(() => this.$isDirty, enqueueSave)
        this._autoSaveState.on = true;
    }

    /** Stops auto-saving if it is currently enabled. */
    public $stopAutoSave() {
        if (!this._autoSaveState.on) return;
        this._autoSaveState.on = false
        this._autoSaveState.watcher!()
        this._autoSaveState.debouncer!.cancel()
    }

    constructor($metadata: TMeta, initialData?: TModel) {
        // Late-initialize the metadata of the api client, 
        // since it isn't actually available in the field initializer.
        this.$apiClient.$metadata = $metadata

        if (initialData) {
            if (!initialData.$metadata) {
                throw `Initial data must have a $metadata property.`
            } else if (initialData.$metadata != $metadata) {
                throw `Initial data must have a $metadata value for type ${$metadata.name}.`
            } else {
                this.$data = initialData
            }
        }
        else {
            this.$data = hydrateModel({}, $metadata);
        }

        this.$isDirty = false;
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

    constructor(initialData?: models.Person) {
        super(metadata.Person, initialData)
    }
}
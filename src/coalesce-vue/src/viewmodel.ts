import Vue from "vue";

import {
  ModelType,
  CollectionProperty,
  PropertyOrName,
  resolvePropMeta,
  PropNames,
  Method,
  ClassType,
  CollectionValue,
  ModelCollectionNavigationProperty,
  ModelCollectionValue,
  Service,
  Property
} from "./metadata.js";
import {
  ModelApiClient,
  ListParameters,
  DataSourceParameters,
  ParamsObject,
  ListApiState,
  ItemApiState,
  ItemResultPromise,
  ListResultPromise,
  ServiceApiClient
} from "./api-client.js";
import {
  Model,
  modelDisplay,
  propDisplay,
  mapToDto,
  convertToModel,
  mapToModel
} from "./model.js";
import { Indexable } from "./util.js";
import { debounce } from "lodash-es";
import type { Cancelable, DebounceSettings } from "lodash";

export { DeepPartial } from "./util.js";

// These imports allow TypeScript to correctly name types in the generated declarations.
// Without them, it will generate some horrible, huge relative paths that won't work on any other machine.
// For example: import("../../../../Coalesce/src/coalesce-vue/src/api-client").ItemResult<TModel>
import type * as apiClient from "./api-client.js";
import type * as axios from "axios";

/*
DESIGN NOTES
    - ViewModel deliberately does not have TMeta as a type parameter.
        The type of the metadata is always accessed off of TModel as TModel["$metadata"].
        This makes the intellisense in IDEs quite nice. If TMeta is a type param,
        we end up with the type of implemented classes taking several pages of the intellisense tooltip.
        With this, we can still strongly type off of known information of TMeta (like PropNames<TModel["$metadata"]>),
        but without cluttering up tooltips with the entire type structure of the metadata.
*/


let nextStableId = 1;

export abstract class ViewModel<
  TModel extends Model<ModelType> = any,
  TApi extends ModelApiClient<TModel> = any,
  TPrimaryKey extends string | number = any
> implements Model<TModel["$metadata"]> {
  /** Static lookup of all generated ViewModel types. */
  public static typeLookup: ViewModelTypeLookup | null = null;

  // TODO: Do $parent and $parentCollection need to be Set<>s in order to handle objects being in multiple collections or parented to multiple parents? Could be useful.
  private $parent: any = null;
  private $parentCollection: this[] | null = null;

  // Underlying object which will hold the backing values
  // of the custom getters/setters. Not for external use.
  // Must exist in order to for Vue to pick it up and add reactivity.
  private $data: TModel & {[propName: string]: any} = convertToModel({}, this.$metadata);

  /** 
   * A number unique among all ViewModel instances that will be unchanged for the instance's lifetime. 
   * Can be used to uniquely identify objects with `:key` in Vue components.
   * 
   * Especially useful with `transition-group` where some elements of iteration may be unsaved 
   * and therefore not yet have a `$primaryKey`.
  */
  public readonly $stableId!: number;

  /**
   * Gets or sets the primary key of the ViewModel's data.
   */
  public get $primaryKey() {
    return ((this as any) as Indexable<TModel>)[
      this.$metadata.keyProp.name
    ] as TPrimaryKey;
  }
  public set $primaryKey(val) {
    (this as any)[this.$metadata.keyProp.name] = val;
  }

  // This bool could be computed from the size of the set,
  // but this wouldn't trigger reactivity because Vue 2 doesn't have reactivity
  // for types like Set and Map.
  private _isDirty = false;
  private _dirtyProps = new Set<string>();
  /**
   * Returns true if the values of the savable data properties of this ViewModel
   * have changed since the last load, save, or the last time $isDirty was set to false.
   */
  public get $isDirty() {
    return this._isDirty
  }
  public set $isDirty(val) {
    if (!val) {
      this._dirtyProps.clear();
      this._isDirty = false;
    } else {
      // When explicitly setting the whole model dirty, 
      // we don't know which specific prop might be dirty,
      // so just set them all.
      for (const propName in this.$metadata.props) {
        this.$setPropDirty(propName as any)
      }
    }
  }

  public $setPropDirty(propName: PropNames<TModel["$metadata"]>, dirty = true, triggerAutosave = true) {
    const propMeta = this.$metadata.props[propName];
    if (propMeta.dontSerialize) {
      return;
    }
    
    if (dirty) {
      this._dirtyProps.add(propName);
      this._isDirty = true;
      
      if (triggerAutosave && this._autoSaveState?.active) {
        // If dirty, and autosave is enabled, queue an evaluation of autosave.
        this._autoSaveState.trigger?.();
      }
    } else {
      this._dirtyProps.delete(propName);
      if (!this._dirtyProps.size) {
        this._isDirty = false;
      }
    }
  }
  public $getPropDirty(propName: PropNames<TModel["$metadata"]>) {
    return this._dirtyProps.has(propName);
  }

  /** The parameters that will be passed to `/get`, `/save`, and `/delete` calls. */
  public $params = new DataSourceParameters();

  /** Wrapper for `$params.dataSource` */
  public get $dataSource() {
    return this.$params.dataSource;
  }
  public set $dataSource(val) {
    this.$params.dataSource = val;
  }

  /** Wrapper for `$params.includes` */
  public get $includes() {
    return this.$params.includes;
  }
  public set $includes(val) {
    this.$params.includes = val;
  }

  /** An object whose values are booleans that specify whether a given validation rule should be ignored,
   * and whose keys are one of the following formats:
   *  - `"propName.ruleName"` - ignores the given rule on the given property.
   *  - `"propName.*"` - ignores rules of the given propName on all properties
   */
  private $ignoredRules: { [identifier: string]: boolean } | null = null;

  /** 
   * A two-layer map, the first layer being property names and the second layer being rule identifiers,
   * of custom validation rules that should be applied to this viewmodel instance.
   */
  private $customRules: { [propName: string]: {
    [identifier: string]: (val: any) => true | string
  } } | null = null;

  /**
   * Cached set of the effective rules for each property on the model.
   * Will be invalidated when custom rules and/or ignores are changed.
   */
  private _effectiveRules: { [propName: string]: undefined | Array<(val: any) => true | string> } | null = null;
  private get $effectiveRules() {
    let effectiveRules = this._effectiveRules;
    if (effectiveRules) return effectiveRules;

    effectiveRules = {};

    for (const propName in this.$metadata.props) {
      const prop = this.$metadata.props[propName];
      const custom = this.$customRules?.[propName];
      const propRules = [];

      if ("rules" in prop) {
        const ignored = this.$ignoredRules;
        const rules: any = prop.rules!;

        // Process metadata-provided rules
        for (const ruleName in rules!) {

          // Process ignores
          if (ignored) {
            if (ignored[`${propName}.${ruleName}`]
            || ignored[`${propName}.*`]
            ) {
              continue;
            }
          }

          // If the prop has custom rules, and there is a custom rule
          // with the same identifier as a metadata-provided rule,
          // do not process the metadata-provided rule.
          if (custom && custom[ruleName]) {
            continue;
          }

          propRules.push(rules[ruleName]);
        }
      }

      // Process custom rules.
      if (custom) {
        for (const ruleName in custom) {
          propRules.push(custom[ruleName]);
        }
      }

      if (propRules.length) {
        effectiveRules[propName] = propRules;
      }
    }

    return this._effectiveRules = effectiveRules;
  }

  public $addRule(prop: string | Property, identifier: string, rule: (val: any) => true | string) {
    const propName = typeof prop == 'string' ? prop : prop.name;

    // Add this as a custom rule. 
    // Because rules are keyed by propName and identifier,
    // the rule will not be duplicated.
    this.$customRules = {
      ...this.$customRules,
      [propName]: {
        ...this.$customRules?.[propName],
        [identifier]: rule
      }
    }

    // Remove any ignore for this rule, if there is one:
    const ignoreSpec = `${propName}.${identifier}`;
    if (this.$ignoredRules && ignoreSpec in this.$ignoredRules) {
      let { [ignoreSpec]: removed, ...remainder } = this.$ignoredRules as any;
      this.$ignoredRules = remainder
    }

    // Force recompute of effective rules.
    this._effectiveRules = null;
  }
  
  public $removeRule(prop: string | Property, identifier: string) {
    const propName = typeof prop == 'string' ? prop : prop.name;

    // Remove any custom rule that may match this spec.
    const propRules = this.$customRules?.[propName] as any;
    if (propRules) {
      let { [identifier]: removed, ...remainder } = propRules;
      this.$customRules = {
        ...this.$customRules,
        [propName]: remainder
      }
    }

    // Add an ignore to override any rules that are coming from metadata.
    this.$ignoredRules = {
      ...this.$ignoredRules,
      [`${propName}.${identifier}`]: true
    }

    // Force recompute of effective rules.
    this._effectiveRules = null;
  }

  /** Returns an array of all applicable rules for the given property
    in accordance with:
    - The rules defined for the model's properties' metadata, 
    - Custom rules added by calling `$addRule`
    - Any rules that where ignored by calling `this.$removeRule`,*/
  public $getRules(prop: string | Property) {
    return this.$effectiveRules[typeof prop == 'string' ? prop : prop.name]
  }

  /** Returns a generator that provides all error messages for the current model
    in accordance with:
    - The rules defined for the model's properties' metadata, 
    - Custom rules added by calling `$addRule`
    - Any rules that where ignored by calling `this.$removeRule`,
  */
  public *$getErrors(prop?: string | Property): Generator<string, void, unknown> {
    if (prop) {
      const propName = typeof prop == 'string' ? prop : prop.name

      const effectiveRules = this.$effectiveRules[propName]
      if (!effectiveRules) return;

      for (const rule of effectiveRules){
        const result = rule(this.$data[propName]);
        if (result !== true) yield result;
      }
    } else {
      for (const propName in this.$effectiveRules) {
        yield* this.$getErrors(propName);
      }
    }
  }

  /** True if `this.$getErrors()` is returning at least one error. */
  public get $hasError() {
    return !!this.$getErrors().next().value
  }

  /**
   * A function for invoking the `/get` endpoint, and a set of properties about the state of the last call.
   */
  get $load() {
    const $load = this.$apiClient
      .$makeCaller("item", (c, id?: TPrimaryKey) =>
        c.get(id != null ? id : this.$primaryKey, this.$params)
      )
      .onFulfilled(() => {
        if (this.$load.result) {
          this.$loadCleanData(this.$load.result);
        }
      });

    // Lazy getter technique - don't create the caller until/unless it is needed,
    // since creation of api callers is a little expensive.
    Object.defineProperty(this, "$load", { value: $load });

    return $load;
  }

  /** Whether or not to reload the ViewModel's `$data` with the response received from the server after a call to .$save(). */
  public $loadResponseFromSaves = true;

  /** Whether to save the whole object, or only properties which are dirty.
   * - `surgical` - save only properties that are flagged as dirty (default).
   * - `whole` - save all properties on the object.
   */
  public $saveMode: "surgical" | "whole" = "surgical";

  /**
   * A function for invoking the `/save` endpoint, and a set of properties about the state of the last call.
   */
  get $save() {
    const $save = this.$apiClient
      .$makeCaller("item", async function(this: ViewModel, c) {

        if (this.$hasError) {
          throw Error("Cannot save - validation failed: " + [...this.$getErrors()].join(", "))
        }

        // Capture the dirty props before we set $isDirty = false;
        const dirtyProps = [...this._dirtyProps];

        // Save only the dirty props if we're doing surgical saves
        // and the item already has a PK.
        const propsToSave = this.$saveMode == "surgical" && !!this.$primaryKey
          ? [
              // Add the PK if it isn't already marked dirty.
              ...(this.$getPropDirty(this.$metadata.keyProp.name) 
                ? [] 
                : [this.$metadata.keyProp.name]
              ), 
              ...dirtyProps
            ]
          : null;

        // Before we make the save call, set isDirty = false.
        // This lets us detect changes that happen to the model while our save request is pending.
        // If the model is dirty when the request completes, we'll not load the response from the server.
        this.$isDirty = false;
        try {
          return await c.save((this as any) as TModel, { fields: propsToSave, ...this.$params });
        } catch (e) {
          for (const prop of dirtyProps) {
            this.$setPropDirty(
              prop, 
              true, 
              // Don't re-trigger autosave on save failure. 
              // Wait for next prop change to trigger it again.
              // Otherwise, if the wait timeout is zero,
              // the save will keep triggering as fast as possible.
              // Note that this could be a candidate for a user-customizable option
              // in the future.
              false
            );
          }
          throw e;
        }
      })
      .onFulfilled(function(this: ViewModel) {
        if (!this.$save.result) {
          // Can't do anything useful if the save returned no data.
          return;
        }

        if (!this.$loadResponseFromSaves) {
          // The PK *MUST* be loaded so that the PK returned by a creation save call
          // will be used by subsequent update calls.
          this.$primaryKey = (this.$save.result as Indexable<TModel>)[
            this.$metadata.keyProp.name
          ];
        } else {
          this.$loadCleanData(this.$save.result);
          this.updatedRelatedForeignKeysWithCurrentPrimaryKey();
        }

      });

    // Lazy getter technique - don't create the caller until/unless it is needed,
    // since creation of api callers is a little expensive.
    Object.defineProperty(this, "$save", { value: $save });

    return $save;
  }

  private updatedRelatedForeignKeysWithCurrentPrimaryKey() {
    const pkValue = this.$primaryKey;

    // Look through collection navigations.
    // Set the corresponding foreign key of all entities to the current PK value.
    for (const prop of Object.values(this.$metadata.props)) {
      if (prop.role != "collectionNavigation") continue;
      
      const value = (this as any)[prop.name]
      if (value?.length) {
        const fk = prop.foreignKey.name
        for (const child of value) {
          child[fk] = pkValue
        }
      }
    }

    if (this.$parent?.$metadata?.props) {
      const parent = this.$parent
      for (const prop of Object.values(parent.$metadata.props) as Property[]) {
        if (prop.role != "referenceNavigation") continue;
        
        // The prop on the parent is a reference navigation. 
        // If the value of the navigation is ourself, 
        // update the foreign key on the $parent with our new PK.
        const value = parent[prop.name]
        if (value === this) {
          parent[prop.foreignKey.name] = pkValue;
        }
      }
    }
  }

  /** 
   * Loads data from the provided model into the current ViewModel, 
   * and then clears the $isDirty flag if `isCleanData` is not given as `false`.
   * 
   * Data is loaded in a surgical fashion that will preserve existing instances
   * of objects and arrays found on navigation properties.
   */
  private $loadFromModel(source: {} | TModel, isCleanData: boolean = true) {
    if (this.$isDirty && this._autoSaveState?.active) {
      updateViewModelFromModel(this as any, source, true, isCleanData);
    } else {
      updateViewModelFromModel(this as any, source, false, isCleanData);
      if (isCleanData) {
        this.$isDirty = false;
      }
    }
  }

  /** 
   * Loads data from the provided model into the current ViewModel, and then clears $isDirty flags.
   * 
   * Data is loaded in a surgical fashion that will preserve existing instances
   * of objects and arrays found on navigation properties.
   */
  public $loadCleanData(source: {} | TModel) {
    return this.$loadFromModel(source, true);
  }

  /** 
   * Loads data from the provided model into the current ViewModel.
   * Does not clear $isDirty flags.
   * 
   * Data is loaded in a surgical fashion that will preserve existing instances
   * of objects and arrays found on navigation properties.
   */
  public $loadDirtyData(source: {} | TModel) {
    return this.$loadFromModel(source, false);
  }

  /**
   * A function for invoking the `/delete` endpoint, and a set of properties about the state of the last call.
   */
  get $delete() {
    const $delete = this.$apiClient
      .$makeCaller("item", function(this: ViewModel, c) {
        if (this.$primaryKey) {
          return c.delete(this.$primaryKey, this.$params);
        } else if (this.$parentCollection) {
          this.$parentCollection.splice(
            this.$parentCollection.indexOf(this),
            1
          );
        }
      })
      .onFulfilled(function(this: ViewModel) {
        if (this.$parentCollection) {
          this.$parentCollection.splice(
            this.$parentCollection.indexOf(this),
            1
          );
        }
      });

    // Lazy getter technique - don't create the caller until/unless it is needed,
    // since creation of api callers is a little expensive.
    Object.defineProperty(this, "$delete", { value: $delete });

    return $delete;
  }

  // Internal autosave state. We must use <any> instead of <this> here because reasons.
  private _autoSaveState?: AutoCallState<AutoSaveOptions<any>>;

  /**
   * Starts auto-saving of the instance when changes to its savable data properties occur.
   * @param vue A Vue instance through which the lifecycle of the watcher will be managed.
   * @param options Options to control how the auto-saving is performed.
   */
  public $startAutoSave(vue: Vue, options: AutoSaveOptions<this> = {}) {
    
    let state = this._autoSaveState;

    if (state?.active && state.options === options) {
      // If already active using the exact same options object, don't restart.
      // This prevents infinite recursion when setting up deep autosaves.
      return;
    }

    const { wait = 1000, predicate = undefined, debounce: debounceOptions } = options;

    this.$stopAutoSave();

    state = this._autoSaveState = new AutoCallState<AutoSaveOptions<any>>()
    state.options = options;

    let ranOnce = false
    
    const enqueueSave = debounce(() => {
      if (!state?.active) return;

      /*
        Try and save if:
          1) The model is dirty
          2) The model lacks a primary key, and we haven't yet tried to save it
            since autosave was enabled. We should only ever try to do this once
            in case the Behaviors on the server are for some reason not configured
            to return responses from saves.
      */
      if (this.$isDirty || (!ranOnce && !this.$primaryKey)) {
        if (this.$hasError || (predicate && !predicate(this))) {
          // There are validation errors, or a user-defined predicate failed. 
          // Do nothing, and don't enqueue another attempt.
          // The next attempt will be enqueued the next time a change is made to the data.
          return;
        }

        if (this.$save.isLoading || this.$load.isLoading) {
          // Save already in progress, or model is currently loading. 
          // Enqueue another attempt.
          enqueueSave();
          return;
        }

        // Everything should be good to go. Go forth and save!
        ranOnce = true;
        this.$save()
          // After the save finishes, attempt another autosave.
          // If the model has become dirty since the last save,
          // we need to save again.
          // This will happen if the state of the model changes while the save
          // is in-flight.
          .then(enqueueSave)
          // We need a catch block so all of this is testable.
          // Otherwise, jest will fail tests as soon as it sees an unhandled promise rejection.
          .catch(() => {  });
      }
    }, Math.max(1, wait), debounceOptions);

    let isPending = false;
    state.trigger = function() {
      if (isPending) return;
      isPending = true;
      // This MUST happen on next tick in case $isDirty was set to true automatically
      // and is about to be manually (or by $loadFromModel) set to false.
      vue.$nextTick(() => {
        isPending = false;
        enqueueSave();
      })
    }

    startAutoCall(state, vue, undefined, enqueueSave);
    state.trigger();

    if (options.deep) {
      for (const propName in this.$metadata.props) {
        const prop = this.$metadata.props[propName];
        if (prop.role == "collectionNavigation") {
          if ((this as any)[propName]) {
            for (const child of ((this as any)[propName] as ViewModel[])) {
              child.$startAutoSave(vue, options)
            }
          }
        } else if (prop.role == "referenceNavigation") {
          ((this as any)[propName] as ViewModel)?.$startAutoSave(vue, options)
        }
      }
    }
  }

  /** Stops auto-saving if it is currently enabled. */
  public $stopAutoSave() {
    if (this._autoSaveState) {
      stopAutoCall(this._autoSaveState);
      this._autoSaveState = undefined;
    }
  }

  /**
   * Returns a string representation of the object, or one of its properties, suitable for display.
   * @param prop If provided, specifies a property whose value will be displayed.
   * If omitted, the whole object will be represented.
   */
  public $display(prop?: PropertyOrName<TModel["$metadata"]>) {
    if (!prop) return modelDisplay(this);
    return propDisplay(this, prop);
  }

  /**
   * Creates a new instance of an item for the specified child model collection,
   * adds it to that collection, and returns the item.
   * @param prop The name of the collection property, or the metadata representing it.
   */
  public $addChild(
    prop:
      | ModelCollectionNavigationProperty
      | PropNames<TModel["$metadata"], ModelCollectionNavigationProperty>
  ) {
    const propMeta = resolvePropMeta<ModelCollectionNavigationProperty>(
      this.$metadata,
      prop
    );
    var collection: Array<any> | undefined = ((this as any) as Indexable<
      TModel
    >)[propMeta.name];

    if (!Array.isArray(collection)) {
      (this as any)[propMeta.name] = [];
      collection = (this as any)[propMeta.name] as any[];
    }

    if (propMeta.role == "collectionNavigation") {
      const newModel: any = mapToModel({}, propMeta.itemType.typeDef);
      const foreignKey = propMeta.foreignKey;
      newModel[foreignKey.name] = this.$primaryKey;

      // The ViewModelCollection will handle create a new ViewModel,
      // and setting $parent, $parentCollection.
      // TODO: Should it also handle setting of the foreign key?
      const newViewModel = collection[collection.push(newModel) - 1] as ViewModel;
      newViewModel.$setPropDirty(foreignKey.name);
      return newViewModel;
    } else {
      throw "$addChild only adds to collections of model properties.";
    }
  }

  constructor(
    // The following MUST be declared in the constructor so its value will be available to property initializers.

    /** The metadata representing the type of data that this ViewModel handles. */
    public readonly $metadata: TModel["$metadata"],

    /** Instance of an API client for the model through which direct, stateless API requests may be made. */
    public readonly $apiClient: TApi,

    initialDirtyData?: {} | TModel | null
  ) {
    Object.defineProperty(this, '$stableId', {
      enumerable: false,
      configurable: false,
      value: nextStableId++,
      writable: false
    })

    if (initialDirtyData) {
      this.$loadDirtyData(initialDirtyData);
    }
  }
}

export abstract class ListViewModel<
  TModel extends Model<ModelType> = Model<ModelType>,
  TApi extends ModelApiClient<TModel> = ModelApiClient<TModel>,
  TItem extends ViewModel = ViewModel<TModel, TApi>
> {
  /** Static lookup of all generated ListViewModel types. */
  public static typeLookup: ListViewModelTypeLookup | null = null;

  /** The parameters that will be passed to `/list` and `/count` calls. */
  public $params = new ListParameters();

  /** Wrapper for `$params.dataSource` */
  public get $dataSource() {
    return this.$params.dataSource;
  }
  public set $dataSource(val) {
    this.$params.dataSource = val;
  }

  /** Wrapper for `$params.includes` */
  public get $includes() {
    return this.$params.includes;
  }
  public set $includes(val) {
    this.$params.includes = val;
  }
  
  /**
   * The current set of items that have been loaded into this ListViewModel.
   */
  private _items = new ViewModelCollection(this.$metadata, this);
  public get $items(): TItem[] {
    return (this._items as unknown) as TItem[];
  }
  public set $items(val: TItem[]) {
    if ((this._items as any) === val) return;

    const vmc = new ViewModelCollection(this.$metadata, this);
    if (val) vmc.push(...val);
    this._items = vmc;
  }

  /** True if the page set in $params.page is greater than 1 */
  public get $hasPreviousPage() {
    return (this.$params.page || 1) > 1;
  }
  /** True if the count retrieved from the last load indicates that there may be pages after the page set in $params.page */
  public get $hasNextPage() {
    // -1 is used to signal an unknown number of pages.
    if (this.$load.pageCount === -1) return true;

    return (this.$params.page || 1) < (this.$load.pageCount || 0);
  }

  /** Decrement the page parameter by 1 if there is a previous page. */
  public $previousPage() {
    if (this.$hasPreviousPage) this.$params.page = (this.$params.page || 1) - 1;
  }
  /** Increment the page parameter by 1 if there is a next page. */
  public $nextPage() {
    if (this.$hasNextPage) this.$params.page = (this.$params.page || 1) + 1;
  }

  public get $page() {
    return this.$params.page || 1;
  }
  public set $page(val) {
    this.$params.page = Number(val);
  }

  public get $pageSize() {
    return this.$params.pageSize || 1;
  }
  public set $pageSize(val) {
    this.$params.pageSize = Number(val);
  }

  public get $pageCount() {
    return this.$load.pageCount;
  }

  /**
   * A function for invoking the `/load` endpoint, and a set of properties about the state of the last call.
   */
  public $load = this.$apiClient
    .$makeCaller("list", c => c.list(this.$params))
    .onFulfilled(state => {
      if (state.result) {
        this.$items = rebuildModelCollectionForViewModelCollection(
          this.$metadata,
          this.$items,
          state.result,
          true
        );
      }
    });

  /**
   * A function for invoking the `/count` endpoint, and a set of properties about the state of the last call.
   */
  public $count = this.$apiClient.$makeCaller("item", c =>
    c.count(this.$params)
  );

  // Internal autoload state
  private _autoLoadState = new AutoCallState();

  /**
   * Starts auto-loading of the list as changes to its parameters occur.
   * @param vue A Vue instance through which the lifecycle of the watcher will be managed.
   * @param options Options that control the auto-load behavior.
   */
  public $startAutoLoad(vue: Vue, options: AutoLoadOptions<this> = {}) {
    const { wait = 1000, predicate = undefined, debounce: debounceOptions } = options;
    this.$stopAutoLoad();

    const enqueueLoad = debounce(() => {
      if (!this._autoLoadState.active) return;

      // Check the predicate again incase its state has changed while we were waiting for the debouncing timer.
      if (predicate && !predicate(this)) {
        return;
      }

      if (this.$load.isLoading && this.$load.concurrencyMode != "cancel") {
        // Load already in progress. Enqueue another attempt.
        enqueueLoad();
      } else {
        // No loads in progress, or concurrency is set to cancel - go for it.
        this.$load();
      }
    }, wait, debounceOptions);

    const onChange = () => {
      if (predicate && !predicate(this)) {
        return;
      }
      enqueueLoad();
    };

    const watcher = vue.$watch(() => this.$params, onChange, { deep: true });
    startAutoCall(this._autoLoadState, vue, watcher, enqueueLoad);
  }

  /** Stops auto-loading if it is currently enabled. */
  public $stopAutoLoad() {
    stopAutoCall(this._autoLoadState);
  }

  constructor(
    // The following MUST be declared in the constructor so its value will be available to property initializers.

    /** The metadata representing the type of data that this ViewModel handles. */
    public readonly $metadata: TModel["$metadata"],

    /** Instance of an API client for the model through which direct, stateless API requests may be made. */
    public readonly $apiClient: TApi
  ) {}
}

export class ServiceViewModel<
  TMeta extends Service = Service,
  TApi extends ServiceApiClient<TMeta> = ServiceApiClient<TMeta>
> {
  /** Static lookup of all generated ServiceViewModel types. */
  public static typeLookup: ServiceViewModelTypeLookup | null = null;

  constructor(
    /** The metadata representing the type of data that this ViewModel handles. */
    public readonly $metadata: TMeta,

    /** Instance of an API client for the model through which direct, stateless API requests may be made. */
    public readonly $apiClient: TApi
  ) {}
}

/** Factory for creating new ViewModels from some initial data.
 *
 * For all ViewModels created recursively as a result of creating the root ViewModel,
 * the same ViewModel instance will be returned whenever the exact same `initialData` object is encountered.
 */
export class ViewModelFactory {
  private static current: ViewModelFactory | null = null;

  private map = new Map<any, ViewModel>();

  /** Ask the factory for a ViewModel for the given type and initial data.
   * The instance may be a brand new one, or may be already existing
   * if the same initialData has already been seen.
   */
  get(typeName: string, initialData: any) {
    const map = this.map;

    if (map.has(initialData)) {
      return map.get(initialData)!;
    }
    const vmCtor = ViewModel.typeLookup![typeName];
    const vm = (new vmCtor() as unknown) as ViewModel;
    map.set(initialData, vm);

    // @ts-ignore
    vm.$loadFromModel(initialData, this.isCleanData);

    return vm;
  }

  /** Provide a pre-existing instance to the factory. */
  set(initialData: any, vm: ViewModel) {
    this.map.set(initialData, vm);
  }

  public static scope<TRet>(action: (factory: ViewModelFactory) => TRet, isCleanData: boolean) {
    if (!ViewModelFactory.current) {
      // There is no current factory. Make a new one.
      ViewModelFactory.current = new ViewModelFactory(isCleanData);
      try {
        // Perform the action, and when we're done, destory the factory.
        return action(ViewModelFactory.current);
      } finally {
        ViewModelFactory.current = null;
      }
    } else {
      // Perform the action using the already existing factory.
      return action(ViewModelFactory.current);
    }
  }

  public static get(typeName: string, initialData: any, isCleanData: boolean) {
    return ViewModelFactory.scope(function(factory) {
      return factory.get(typeName, initialData);
    }, isCleanData);
  }

  private constructor(private isCleanData = true) {}
}

/** Gets a human-friendly description for ViewModelCollection error messages. */
function viewModelCollectionName($metadata: ModelCollectionValue | ModelType) {
  const collectedTypeMeta =
    $metadata.type == "model" ? $metadata : $metadata.itemType.typeDef;

  const str = `a collection of ${collectedTypeMeta.name}`;

  return $metadata.type == "model" ? str : `${$metadata.name} (${str})`;
}

function viewModelCollectionMapItems<T extends ViewModel>(
  items: T[],
  vmc: ViewModelCollection<T>,
  isCleanData: boolean
) {
  const collectedTypeMeta =
    vmc.$metadata.type == "model"
      ? vmc.$metadata
      : vmc.$metadata.itemType.typeDef;

  return items.map(val => {
    if (val == null) {
      throw Error(`Cannot push null to a collection of ViewModels.`);
    }

    if (typeof val !== "object") {
      throw Error(
        `Cannot push a non-object to ${viewModelCollectionName(vmc.$metadata)}`
      );
    } 
    // Sanity check. Probably not crucial if this ends up causing issues. A warning would probably suffice too.
    else if ("$metadata" in val && val.$metadata != collectedTypeMeta) {
      throw Error(
        `Type mismatch - attempted to assign a ${
          val.$metadata.name
        } to ${viewModelCollectionName(vmc.$metadata)}`
      );
    }

    if (val instanceof ViewModel) {
      // Already a viewmodel. Do nothing
    } else {
      // Incoming is a Model. Make a ViewModel from it.
      if (ViewModel.typeLookup === null) {
        throw Error(
          "Static `ViewModel.typeLookup` is not defined. It should get defined in viewmodels.g.ts."
        );
      }
      val = (ViewModelFactory.get(collectedTypeMeta.name, val, isCleanData) as unknown) as T;
    }

    // $parent and $parentCollection are intentionally protected -
    // they're just for internal tracking of stuff
    // and probably shouldn't be used in custom code.
    // So, we'll cast to `any` so we can set them here.
    (val as any).$parent = vmc.$parent;
    (val as any).$parentCollection = vmc;

    // If deep autosave is active, propagate it to the ViewModel instance being attached to the object graph.
    const autoSaveState: AutoCallState<AutoSaveOptions<any>> = vmc.$parent._autoSaveState;
    if (autoSaveState?.active && autoSaveState.options?.deep) {
      val.$startAutoSave(autoSaveState.vue!, autoSaveState.options);
    }
    
    return val;
  });
}

function resolveProto(obj: ViewModelCollection<any>): Array<any> {
  // Babel does some stupid nonsense where it will wrap our proto
  // in another proto. This breaks things if coalesce-vue is imported from source,
  // or if we were to at some point in the future emit a esnext version of coalesce-vue.
  const proto = Object.getPrototypeOf(obj);
  if (obj.push == proto.push) {
    // `proto` is the wrapper because it contains our own `push` (and other methods).
    // Go up one more level.
    return Object.getPrototypeOf(proto);
  }
  return proto;
}

export class ViewModelCollection<T extends ViewModel> extends Array<T> {
  readonly $metadata!: ModelCollectionValue | ModelType;
  readonly $parent!: any;
  $hasLoaded!: boolean;


  override push(...items: T[]): number {
    // MUST evaluate the .map() before grabbing the .push()
    // method from the proto. See test "newly loaded additional items are reactive".
    const viewModelItems = viewModelCollectionMapItems(items, this, true);

    return resolveProto(this).push.apply(this, viewModelItems);
  }

  override splice(start: number, deleteCount?: number, ...items: T[]): T[] {
    // MUST evaluate the .map() before grabbing the .push()
    // method from the proto. See test "newly loaded additional items are reactive".
    const viewModelItems: any[] = items
      ? viewModelCollectionMapItems(items, this, true)
      : items;

    return resolveProto(this).splice.call(
      this,
      start, deleteCount as any, ...viewModelItems
    );
  }

  constructor($metadata: ModelCollectionValue | ModelType, $parent: any) {
    super();
    Object.defineProperties(this, {
      // These properties need to be non-enumerable to avoid them from being looped over
      // during iteration of the array if `for ... in ` is used.
      // We also don't want Vue to bother making them reactive because it won't be beneficial.
      $metadata: {
        value: $metadata,
        enumerable: false,
        writable: false,
        configurable: false
      },
      $parent: {
        value: $parent,
        enumerable: false,
        writable: false,
        configurable: false
      },
      $hasLoaded: {
        value: false,
        enumerable: false,
        writable: true,
        configurable: false
      },
      push: {
        value: ViewModelCollection.prototype.push,
        enumerable: false,
        writable: false,
        configurable: false
      },
      splice: {
        value: ViewModelCollection.prototype.splice,
        enumerable: false,
        writable: false,
        configurable: false
      }
    });
  }
}

type DebounceOptions = {
  /** Time, in milliseconds, to delay. Passed as the second parameter to lodash's `debounce` function. */
  wait?: number;
  /** Additional options to pass to the third parameter of lodash's `debounce` function. */
  debounce?: DebounceSettings;
}

type AutoLoadOptions<TThis> = DebounceOptions & {
  /** A function that will be called before autoloading that can return false to prevent a load. */
  predicate?: (viewModel: TThis) => boolean;
}

type AutoSaveOptions<TThis> = DebounceOptions & ({
  /** A function that will be called before autosaving that can return false to prevent a save. */
  predicate?: (viewModel: TThis) => boolean;

  /** If true, auto-saving will also be enabled for all view models that are
   * reachable from the navigation properties & collections of the current view model. */
  deep?: false;
} | {
  /** A function that will be called before autosaving that can return false to prevent a save. */
  predicate?: (viewModel: ViewModel) => boolean;

  /** If true, auto-saving will also be enabled for all view models that are
   * reachable from the navigation properties & collections of the current view model. */
  deep: true;
})

/**
 * Dynamically adds gettter/setter properties to a class. These properties wrap the properties in its instances' $data objects.
 * @param ctor The class to add wrapper properties to
 * @param metadata The metadata describing the properties to add.
 */
export function defineProps<T extends new () => ViewModel<any, any>>(
  ctor: T,
  metadata: ModelType
) {
  const props = Object.values(metadata.props);
  const descriptors = {} as PropertyDescriptorMap;
  for (let i = 0; i < props.length; i++) {
    const prop = props[i];
    const propName = prop.name;

    descriptors[propName] = {
      enumerable: true,
      configurable: true,
      get: function(this: InstanceType<T>) {
        return (this as any).$data[propName];
      },
      set:
        prop.type == "model"
          ? function(this: InstanceType<T>, incomingValue: any) {
              if (incomingValue != null) {
                if (typeof incomingValue !== "object") {
                  throw Error(
                    `Cannot assign a non-object to ${metadata.name}.${propName}`
                  );
                } else if ("$metadata" in incomingValue) {
                  if (incomingValue.$metadata.name != prop.typeDef.name) {
                    throw Error(
                      `Type mismatch - attempted to assign a ${incomingValue.$metadata.name} to ${metadata.name}.${propName}`
                    );
                  }
                }

                if (incomingValue instanceof ViewModel) {
                  // Already a viewmodel. Do nothing
                } else {
                  // Incoming is a Model. Make a ViewModel from it.
                  // This won't technically be valid according to the types of the properties
                  // on our generated ViewModels, but we should handle it
                  // so that input components work really nicely if a component sets the navigation prop.
                  incomingValue = ViewModelFactory.get(
                    prop.typeDef.name,
                    incomingValue,
                    true // Assume the data is clean.
                  );
                }

                // If deep autosave is active, propagate it to the new ViewModel instance.
                const autoSaveState: AutoCallState<AutoSaveOptions<any>> = (this as any)._autoSaveState;
                if (autoSaveState?.active && autoSaveState.options?.deep) {
                  incomingValue.$startAutoSave(autoSaveState.vue!, autoSaveState.options);
                }

                incomingValue.$parent = this;
              }

              (this as any).$data[propName] = incomingValue;

              // Set the foreign key using the PK of the incoming object.
              // This may end up being redundant if we're in the process
              // of copying the props of an object that already has the FK,
              // but it will help ensure 100% correctness.
              if (prop.role == "referenceNavigation") {
                // When setting an object, fix up the foreign key using the value pulled from the object
                // if it has a value.
                const incomingPk = incomingValue
                  ? incomingValue[prop.principalKey.name]
                  : null;

                // Set on `this`, not `$data`, in order to trigger $isDirty in the
                // setter function for the FK prop.
                (this as any)[prop.foreignKey.name] = incomingPk;
              }
            }

          : prop.type == "collection" && prop.itemType.type == 'model'
          ? function(this: InstanceType<T>, incomingValue: any) {
              let hasLoaded = false;
              if (incomingValue == null) {
                // Usability niceness - make an empty array if the incoming is null.
                // This shouldn't have any adverse effects that I can think of.
                // This will cause the viewmodel collections to always be initialized with empty arrays
                incomingValue = [];
              } else if (!Array.isArray(incomingValue)) {
                throw Error(
                  `Cannot assign a non-array to ${metadata.name}.${propName}`
                );
              } else {
                hasLoaded = true;
              }

              const $data = (this as any).$data;
              const old = $data[propName];

              if (old === incomingValue) {
                // Setting same value. Do nothing.
                return;
              }

              const vmc = new ViewModelCollection(prop as ModelCollectionValue, this);
              // Mark the collection so that we can determine that it has been loaded,
              // versus a collection that was never loaded because the server never populated it.
              // This lets us tell if the collection is truly empty, or empty because it isn't loaded.
              vmc.$hasLoaded = hasLoaded
              vmc.push(...incomingValue);
              $data[propName] = vmc;
            }


          // : prop.role =="primaryKey"
          // ? function(this: InstanceType<T>, incomingValue: any) {
          //     const $data = (this as any).$data;

          //     // Do nothing if the value is unchanged.
          //     if ($data[propName] === incomingValue) {
          //       return;
          //     }

          //     $data[propName] = incomingValue;
            
          //     // TODO: Implement $emit?
          //     // this.$emit('valueChanged', prop, value, val);
          //   }


          : function(this: InstanceType<T>, incomingValue: any) {
            const $data = (this as any).$data;
            const old = $data[propName];

            // First, check strict equality. This will handle the 90% most common case.
            if (old === incomingValue) {
              return;
            }

            if (prop.type == "object" && incomingValue != null && !("$metadata" in incomingValue)) {
              convertToModel(incomingValue, prop.typeDef);
            }

            // If strict equality fails, try to use valueOf() to compare.
            // valueOf() helps with Date instances that represent the same time value.
            // If either side is null, it is ok to set $isDirty, since we
            // know that if we got this var, BOTH sides aren't both null.
            if (old?.valueOf() !== incomingValue?.valueOf()) {
              $data[propName] = incomingValue;
            
              // TODO: Implement $emit?
              // this.$emit('valueChanged', prop, value, val);

              this.$setPropDirty(propName);

              if (prop.role == "foreignKey" && prop.navigationProp) {
                /*
                  If there's a navigation property for this FK,
                  we need to null it out if the current value of the 
                  navigation prop is non-null and the incoming value of the FK does not agree with the  PK on the value of the navigation prop.
                */
                const currentObject = $data[prop.navigationProp.name];
                if (
                  currentObject != null &&
                  incomingValue != currentObject[prop.principalKey.name]
                ) {
                  // Set on `$data`, not `this`.
                  // We don't want to trigger the "model" setter
                  // since it basically does nothing when the value is null,
                  // and it would also attempt to perform fixup of the FK prop,
                  // but we're already doing just that.
                  $data[prop.navigationProp.name] = null;
                }
              }
            }
          }
    };
  }

  Object.defineProperties(ctor.prototype, descriptors);
}

export interface ViewModelTypeLookup {
  [name: string]: new (initialData?: any) => ViewModel;
}
export interface ListViewModelTypeLookup {
  [name: string]: new () => ListViewModel<any, any, any>;
}
export interface ServiceViewModelTypeLookup {
  [name: string]: new () => ServiceViewModel;
}

export type ModelOf<T> = T extends ViewModel<infer TModel> ? TModel : never;

/** Do not export.
 *
 * Doesn't strictly return collections of ViewModels,
 * but instead expects the receiving setter of the array to be a ViewModelCollection.
 */
function rebuildModelCollectionForViewModelCollection(
  type: ModelType,
  currentValue: any[],
  incomingValue: any[],
  isCleanData: boolean
) {
  if (!Array.isArray(currentValue)) {
    currentValue = [];
  }

  let incomingLength = incomingValue.length;
  let currentLength = currentValue.length;

  // There are existing items. We need to surgically merge in the incoming items,
  // keeping existing ViewModels the same based on keys.
  const pkName = type.keyProp.name;
  const existingItemsMap = new Map();
  const existingItemsWithoutPk = [];
  for (let i = 0; i < currentLength; i++) {
    const item = currentValue[i];
    const itemPk = item[pkName];
    if (itemPk) {
      existingItemsMap.set(itemPk, item);
    } else {
      existingItemsWithoutPk.push(item)
    }
  }

  // Rebuild the currentValue array, using existing items when they exist,
  // otherwise using the incoming items.

  for (let i = 0; i < incomingLength; i++) {
    const incomingItem = incomingValue[i];
    const incomingItemPk = incomingItem[pkName];
    const existingItem = existingItemsMap.get(incomingItemPk);
    if (existingItem) {
      existingItem.$loadFromModel(incomingItem, isCleanData);

      if (currentValue[i] === existingItem) {
        // The existing item is not moving position. Do nothing.
      } else {
        // Replace the item currently at this position with the existing item.
        currentValue.splice(i, 1, existingItem);
      }
    } else {
      // No need to $loadFromModel on the incoming item.
      // The setter for the collection will transform its contents into ViewModels for us.

      if (currentValue[i]) {
        // There is something else already in the array at this position. Replace it.
        currentValue.splice(i, 1, incomingItem);
      } else {
        // Nothing in the current array at this position. Just stick it in.
        currentValue.push(incomingItem);
      }
    }
  }

  if (existingItemsWithoutPk.length) {
    // Add to the end of the collection any existing items that do not have primary keys.
    // This behavior exists to prevent losing items on the client
    // that may not yet be saved in the event that the parent of the collection
    // get reloaded from a save.
    // If this behavior is undesirable in a specific circumstance,
    // it is trivial to manually remove unsaved items after a .$save() is peformed.

    const existingItemsLength = existingItemsWithoutPk.length
    for (let i = 0; i < existingItemsLength; i++) {
      let realIndex = incomingLength + i;

      const existingItem = existingItemsWithoutPk[i];
      const currentItem = currentValue[realIndex];

      if (existingItem === currentItem) {
        // The existing item is not moving position. Do nothing.
      } else {
        // Replace the item currently at this position with the existing item.
        currentValue.splice(realIndex, 1, existingItem);
      }
    }

    incomingLength += existingItemsLength;
  }

  // If the new collection is shorter than the existing length,
  // remove the extra items.
  if (currentLength > incomingLength) {
    currentValue.splice(incomingLength, currentLength - incomingLength);
  }

  // Let the receiving ViewModelCollection handle the conversion of the contents
  // into ViewModel instances.
  return currentValue;
}

/**
 * Updates the target model with values from the source model.
 * @param target The viewmodel to be updated.
 * @param source The model whose values will be used to perform the update.
 * @param skipDirty If true, only non-dirty props, and related objects, will be updated. 
 * Basic properties on target that are dirty will be skipped.
 */
export function updateViewModelFromModel<
  TViewModel extends ViewModel<Model<ModelType>>
>(
  target: TViewModel, 
  source: Indexable<{}>, 
  skipDirty = false,
  isCleanData = true
) {
  ViewModelFactory.scope(function(factory) {
    // Add the root ViewModel to the factory
    // so that when existing ViewModels are being updated,
    // duplicate VM instances won't be created needlessly.

    // "Existing ViewModels" includes user-instantiated instances
    // (i.e. those not instantiated by code in this file).

    factory.set(source, target);

    const metadata = target.$metadata;

    // Sanity check. Probably not crucial if this ends up causing issues. A warning would probably suffice too.
    if ("$metadata" in source && source.$metadata != metadata) {
      throw Error(
        `Attempted to load a ${metadata.name} ViewModel with a ${source.$metadata.name} object.`
      );
    }

    for (const prop of Object.values(metadata.props)) {
      const propName = prop.name as keyof typeof target & string;
      // Hit $data directly since this is internal code.
      // Avoids a redundant function call against the ViewModel's prop getter.
      const currentValue = (target as any).$data[propName] as any;
      let incomingValue = (source as any)[propName];

      // Sanitize incomingValue to not be undefined (to not break Vue's reactivity),
      // since `source` isn't guaranteed to be a model and thus isn't guaranteed that
      // all properties are defined.
      if (incomingValue === undefined) incomingValue = null;

      switch (prop.role) {
        case "referenceNavigation":
          if (incomingValue) {
            if (
              currentValue == null ||
              currentValue[prop.typeDef.keyProp.name] !==
                incomingValue[prop.typeDef.keyProp.name]
            ) {
              // If the current value is null,
              // or if the current value has a different PK than the incoming value,
              // we should create a brand new object.

              // The setter on the viewmodel will handle the conversion to a ViewModel.
              target[propName] = incomingValue;
              if (!isCleanData && !(incomingValue instanceof ViewModel)) {
                // The setter on the viewmodel will assume clean by default.
                // If the data isn't clean, and the incoming value wasn't already
                // a ViewModel that is capable of tracking its own dirty state,
                // explicitly dirty the model.

                // This might be redundant with the fact that the newly created
                // ViewModel would have been created in our current ViewModelFactory scope,
                // which itself will flag the incoming data as dirty.
                // TODO: Test if this is redundant, and remove it if it is.
                (target[propName] as any as ViewModel).$isDirty = true;
              }
            } else {
              // `currentValue` is guaranteed to be a ViewModel by virtue of the
              // implementations of the setters for referenceNavigation properties on ViewModel instances.
              currentValue.$loadFromModel(incomingValue, isCleanData);
            }

            // TODO (maybe): Check if target.$parent is the expected value of this navigation prop,
            // and if it is, update $parent? This is what the KO stack did. Maybe we want to do the same?
          } else {
            // We allow the existing value of the navigation prop to stick around
            // if the server didn't send it back.
            // The setter handling for the foreign key will handle
            // clearing out the current object if it doesn't match the incoming FK.
            // This allows us to keep the existing navigation object
            // even if the server didn't respond with one.
            // However, if the FK has changed to a different value that no longer
            // agrees with the existing navigation object,
            // the FK setter will null the navigation to prevent an inconsistent data model.
          }
          break;
        case "collectionNavigation":
          if (incomingValue == null) {
            if (currentValue) {
              // No incoming collection was provided. Allow the existing collection to stick around.
              // Note that this case is different from the incoming value being an empty array,
              // which should be used to explicitly clear our the existing collection.
            } else {
              // Pass null directly through to the VM's setter. 
              // The VM's setter will then populate the VM's data with an empty array rather than null.
              target[propName] = null as any;
            }
            break;
          }
          
          if (!Array.isArray(incomingValue)) {
            throw `Expected array for incoming value for ${metadata.name}.${prop.name}`;
          }

          target[propName] = rebuildModelCollectionForViewModelCollection(
            prop.itemType.typeDef,
            currentValue,
            incomingValue,
            isCleanData
          ) as any;
          break;

        case "primaryKey":
          // Always update the PK, even if skipSelf is true.
          if (currentValue !== incomingValue) {
            target[propName] = incomingValue;
          }
          break;

        default:
          // We check against currentValue here for a minor perf increase - 
          // Even though this is redundant with the ViewModel's setters,
          // it avoids calling into the setter if we don't have to,
          // and handles the vast majority of cases.
          if ((!skipDirty || !target.$getPropDirty(propName)) && currentValue !== incomingValue) {
            target[propName] = incomingValue;
          }
          break;
      }
    }
  }, isCleanData);
}

/* Internal members/helpers */

class AutoCallState<TOptions = any> {
  active: boolean = false;
  cleanup: Function | null = null;
  vue: Vue | null = null;
  options: TOptions | null = null;
  trigger: (() => void) | null = null;

  constructor() {
    // Seal to prevent unnessecary reactivity
    return Object.seal(this);
  }
}

function startAutoCall(
  state: AutoCallState,
  vue: Vue,
  watcher?: () => void,
  debouncer?: Cancelable
) {
  const destroyHook = () => stopAutoCall(state);

  vue.$on("hook:beforeDestroy", destroyHook);
  state.vue = vue;
  state.cleanup = () => {
    if (!state.active) return;
    // Destroy the watcher
    watcher?.();
    // Cancel the debouncing timer if there is one.
    if (debouncer) debouncer.cancel();
    // Cleanup the hook, in case we're not responding to beforeDestroy but instead to a direct call to stopAutoCall.
    // If we didn't do this, autosave could later get disabled when the original component is destroyed,
    // even though if was later attached to a different component that is still alive.
    vue.$off("hook:beforeDestroy", destroyHook);
  };
  state.active = true;
}

function stopAutoCall(state: AutoCallState) {
  if (!state.active) return;
  state.cleanup!();
  state.active = false;
}

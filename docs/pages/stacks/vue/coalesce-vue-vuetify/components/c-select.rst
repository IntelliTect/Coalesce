.. _c-select:

c-select
========

.. MARKER:summary
    
A dropdown component that allows for selecting values fetched from the generated ``/list`` API endpoints. 

Used both for selecting values for foreign key and navigation properties, and for selecting arbitrary objects or primary keys independent of a parent or owning object.

.. MARKER:summary-end

.. contents:: Contents
    :local:

Examples
--------

Binding to a navigation property or foreign key of a model:

.. code-block:: sfc

  <c-input :model="person" for="company" />
  <!-- OR: -->
  <c-input :model="person" for="companyId" />

Binding an arbitrary primary key value or an arbitrary object:

.. code-block:: sfc

  <!-- Binding a key: -->
  <c-input for="Person" :key-value.sync="selectedPersonId" />

  <!-- Binding an object: -->
  <c-input for="Person" :object-value.sync="selectedPerson" />
  <c-input for="Person" v-model="selectedPerson" />

Examples of other props:

.. code-block:: sfc

  <c-input 
    for="Person" 
    v-model="selectedPerson"
    :clearable="false"
    preselect-first
    :params="{ pageSize: 42, filter: { isActive: true } }"
    :create="createMethods"
    dense
    outlined
    color="pink"
  />
  <!-- `createMethods` is defined in the docs of `create` below -->


Props
-----


:ts:`for: string | ForeignKeyProperty | ModelReferenceNavigationProperty | ModelType`

    A metadata specifier for the value being bound. One of:

    - The name of a foreign key or reference navigation property belonging to :ts:`model`. 
    - The name of a model type.
    - A direct reference to a metadata object.
    - A string in dot-notation that starts with a type name that resolves to a foreign key or reference navigation property.

    .. tip:: 

      When binding by a key value, if the corresponding object cannot be found (e.g. there is no navigation property, or the navigation property is null), c-select will automatically attempt to load the object from the server so it can be displayed in the UI.

:ts:`model?: Model`
    An object owning the value that was specified by the :ts:`for` prop. If provided, the input will be bound to the corresponding property on the :ts:`model` object.

    If :ts:`for` specifies a foreign key or reference navigation property, both the foreign key and the navigation property of the :ts:`model` will be updated when the selected value is changed.

:ts:`value?: any`
    When binding the component with ``v-model``, accepts the ``value`` part of ``v-model``. If :ts:`for` was specified as a foreign key, this will expect a key; likewise, if :ts:`for` was specified as a type or as a navigation property, this will expect an object.

:ts:`keyValue?: any`
    When bound with :html:`:key-value.sync="keyValue"`, allows binding the primary key of the selected object explicitly.

:ts:`objectValue?: any`
    When bound with :html:`:object-value.sync="objectValue"`, allows binding the selected object explicitly.

:ts:`clearable?: boolean`
    Whether the selection can be cleared or not, emitting :ts:`null` as the input value.

    If not specified and the component is bound to a foreign key or reference navigation property, defaults to whether or not the foreign key has a ``required`` validation rule defined in its :ref:`Metadata <VueMetadata>`.

:ts:`preselectFirst?: boolean = false`
    If true, then when the first list results for the component are received by the client just after the component is created, c-select will emit the first item in the list as the selected value.

:ts:`preselectSingle?: boolean = false`
    If true, then when the first list results for the component are received by the client just after the component is created, if the results contained exactly one item, c-select will emit that only item as the selected value.

:ts:`params?: ListParameters`
    An optional set of :ref:`Data Source Standard Parameters <DataSourceStandardParameters>` to pass to API calls made to the server.

:ts:`create?`

  A object containing a pair of methods that allowing users to create new items from directly within the c-select if a matching object is not found. 

  The object must contain the following two methods. You should define these in your component's ``script`` section - don't try to define them inline in your component.

  :ts:`getLabel: (search: string, items: TModel[]) => string | false,`

    A function that will be called with the user's current search term, as well as the collection of currently loaded items being presented to the user as valid selection options.

    It should return either a :ts:`string` that will be presented to the user as an option in the dropdown that can be clicked to invoke the :ts:`getItem` function below, or it should return :ts:`false` to prevent such an option from being shown to the user.

  :ts:`getItem: (search: string, label: string) => Promise<TModel>`

    A function that will be invoked when the user clicks the option in the dropdown list described by :ts:`getLabel`. It will be given the user's current search term as well as the value of the label returned from :ts:`getLabel` as parameters. It must perform the necessary operations to create the new object on the server and then return a reference to that object.
  
  For example:

  .. code-block:: vue

    createMethods = {
      getLabel(search: string, items: Person[]) {
        const searchLower = search.toLowerCase();
        if (items.some(a => a.name?.toLowerCase().indexOf(searchLower) == 0)) {
          return false;
        }
        return search;
      },
      async getItem(search: string, label: string) {
        const client = new PersonApiClient();
        return (await client.addPersonByName(label)).data.object!;
      }
    }

Slots
-----

:html:`#item="{ item }"` - Slot used to customize the text of both items inside the list, as well as the text of selected items. By default, items are rendered with :ref:`c-display`. Slot is passed a single parameter :ts:`item` containing a :ref:`model instance <VueModels>`.
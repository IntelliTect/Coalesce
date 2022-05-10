.. _c-loader-status:

c-loader-status
===============

.. MARKER:summary
    
A component for displaying progress and error information for one or more [API Callers](/stacks/vue/layers/api-clients.md).

.. tip:: 
    It is highly recommended that all [API Callers](/stacks/vue/layers/api-clients.md) utilized by your application that don't have any other kind of error handling should be represented by a [c-loader-status](/stacks/vue/coalesce-vue-vuetify/components/c-loader-status.md) so that users can be aware of any errors that occur.

.. MARKER:summary-end

Progress is indicated with a [Vuetify](https://vuetifyjs.com/) [v-progress-linear](https://vuetifyjs.com/en/components/progress-linear) component, and errors are displayed in a [v-alert](https://vuetifyjs.com/en/components/alerts/). [Transitions](https://vuetifyjs.com/en/styles/transitions/) are applied to smoothly fade between the different states the the caller can be in.

.. note:: 
    This component uses the legacy term "loader" to refer to [API Callers](/stacks/vue/layers/api-clients.md). A new ``c-caller-status`` component may be coming in the future with a few usability improvements - if that happens, `c-loader-status` will be preserved for backwards compatibility.

[[toc]]

Examples
--------

Wrap contents of a details/edit page:

``` vue-html

    <h1>Person Details</h1>
    <c-loader-status
        :loaders="{ 
            'no-initial-content no-error-content': [person.$load],
            '': [person.$save, person.$delete],
        }"
        #default
    >
        First Name: {{ person.firstName }}
        Last Name: {{ person.lastName }}
        Employer: {{ person.company.name }}
    </c-loader-status>
    

```

|

Use ``c-loader-status`` to render a progress bar and any error messages, but don't use it to control content:

``` vue-html

    <c-loader-status :loaders="{'': [list.$load]}" />


```

|

Wrap a save/submit button:

``` vue-html

    <c-loader-status
        :loaders="{ 'no-loading-content': [person.$save] }"
    >
        <button> Save </button>
    </c-loader-status>
    

```

|

Hides the table before the first load has completed, or if loading the list encountered an error. Don't show the progress bar after we've already loaded the list for the first time (useful for loads that occur without user interaction, e.g. `setInterval`).:

``` vue-html

    <c-loader-status
        :loaders="{
            'no-secondary-progress no-initial-content no-error-content': [list.$load]
        }"
        #default
    >
        <table>
            <tr v-for="item in list.$items"> ... </tr>
        </table>
    </c-loader-status>


```

Props
-----

`loaders: { [flags: string]: ApiCaller | ApiCaller[] }`
    A dictionary object with entries mapping zero or more flags to one or more [API Callers](/stacks/vue/layers/api-clients.md). Multiple entries of flags/caller pairs may be specified in the dictionary to give different behavior to different API callers.
    
    The available flags are as follows. All flags may be prefixed with ``no-`` to set the flag to ``false`` instead of ``true``. Multiple flags may be specified at once by delimiting them with spaces.

    - ``loading-content`` - default ``true`` — Controls whether the default slot is rendered while any API caller is loading (i.e. when  `caller.isLoading === true`).

    - ``error-content`` - default ``true`` — Controls whether the default slot is rendered while any API Caller is in an error state (i.e. when  `caller.wasSuccessful === false`).

    - ``initial-content`` - default ``true`` — Controls whether the default slot is rendered while any API Caller has yet to receive a response for the first time (i.e. when `caller.wasSuccessful === null`).

    - ``initial-progress`` - default ``true`` — Controls whether the progress indicator is shown when an API Caller is loading for the very first time (i.e. when  `caller.wasSuccessful === null`).

    - ``secondary-progress`` - default ``true`` — Controls whether the progress indicator is shown when an API Caller is loading any time after its first invocation (i.e. when  `caller.wasSuccessful !== null`).

`progressPlaceholder: boolean = true`
    Specify if space should be reserved for the progress indicator. If set to false, the content in the default slot may jump up and down slightly as the progress indicator shows and hides.

`height: number = 10`
    Specifies the height in pixels of the [v-progress-linear](https://vuetifyjs.com/en/components/progress-linear) used to indicate progress.

Slots
-----

``default``
    Accepts the content whose visibility is controlled by the state of the supplied [API Callers](/stacks/vue/layers/api-clients.md). It will be shown or hidden according to the flags defined for each caller.

    .. important:: 
    
        Define the default slot as a [scoped slot](https://vuejs.org/v2/guide/components-slots.html#Scoped-Slots) (e.g. with ``#default`` or ``v-slot:default`` on the ``c-loader-status``) to prevent the VNode tree from being created when the content should be hidden. This improves performances and helps avoid null reference errors that can be caused when trying to render objects that haven't been loaded yet.

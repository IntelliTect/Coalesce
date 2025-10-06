# c-loader-status

<!-- MARKER:summary -->
    
A component for displaying progress and error information for one or more [API Callers](/stacks/vue/layers/api-clients.md#api-callers).

::: tip
It is highly recommended that all [API Callers](/stacks/vue/layers/api-clients.md#api-callers) utilized by your application that don't have any other kind of error handling should be represented by a [c-loader-status](/stacks/vue/coalesce-vue-vuetify/components/c-loader-status.md) so that users can be aware of any errors that occur.
::: 

<!-- MARKER:summary-end -->

Progress is indicated with a [Vuetify](https://vuetifyjs.com/) [v-progress-linear](https://vuetifyjs.com/en/components/progress-linear) component, and errors are displayed in a [v-alert](https://vuetifyjs.com/en/components/alerts/). [Transitions](https://vuetifyjs.com/en/styles/transitions/) are applied to smoothly fade between the different states the caller can be in.


## Examples

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


Use ``c-loader-status`` to render a progress bar and any error messages, but don't use it to control content:
``` vue-html
<c-loader-status :loaders="list.$load" />
```


Wrap a save/submit button:
``` vue-html
<c-loader-status :loaders="[person.$save, person.$delete]" no-loading-content>
  <button> Save </button>
  <button> Delete </button>
</c-loader-status>
```

Show success alerts when operations complete successfully:
``` vue-html
<c-loader-status :loaders="person.$save" show-success>
  <button> Save </button>
</c-loader-status>
```

Show a retry button when operations fail (note: only use this with parameterless loaders, or those that can be retried with `invokeWithArgs`.):
``` vue-html
<c-loader-status :loaders="person.$save" show-retry>
  <button> Save </button>
</c-loader-status>
```

Customize alert appearance with prepend and append slots:
``` vue-html
<c-loader-status :loaders="person.$save" show-success>
  <template #prepend>
    <v-icon>fa fa-user</v-icon>
  </template>
  <template #append>
    <v-btn 
      size="small" 
      variant="text" 
      @click="person.$save()"
      :disabled="person.$save.isLoading"
    >
      Retry
    </v-btn>
  </template>
  <button> Save </button>
</c-loader-status>
```

Hides the table before the first load has completed, or if loading the list encountered an error. Don't show the progress bar after we've already loaded the list for the first time (useful for loads that occur without user interaction, e.g. `setInterval`):

``` vue-html
<c-loader-status
  :loaders="list.$load"
  no-initial-content 
  no-error-content
  no-secondary-progress 
>
  <table>
    <tr v-for="item in list.$items"> ... </tr>
  </table>
</c-loader-status>
```

## Props

<Prop def="loaders: 
  // Flags per component:
  | ApiCaller 
  | ApiCaller[]
  // Flags per caller:
  | { [flags: string]: ApiCaller | ApiCaller[] } " lang="ts" />

<div>

This prop has multiple options that support simple or complex usage scenarios:

#### Flags Per Component
A single instance, or array of [API Callers](/stacks/vue/layers/api-clients.md#api-callers), whose status will be represented by the component. The [flags](#flags) for these objects will be determined from the component-level [flag props](#flags-props).

``` vue-html
<c-loader-status
  :loaders="[product.$load, person.$load]"
  no-initial-content
  no-error-content
/>
```

#### Flags Per Caller
A more advanced usage allows passing different flags for different callers. Provide a dictionary object with entries mapping zero or more [flags](#flags) to one or more [API Callers](/stacks/vue/layers/api-clients.md#api-callers). Multiple entries of flags/caller pairs may be specified in the dictionary to give different behavior to different API callers. These flags are layered on top of the base [flag props](#flags-props). 

``` vue-html
<c-loader-status
  :loaders="{ 
    'no-initial-content no-error-content': [person.$load],
    'no-loading-content': [person.$save, person.$delete],
  }"
/>
```

</div>
<br>


<Prop def="progressPlaceholder: boolean = true" lang="ts" />

Specify if space should be reserved for the progress indicator. If set to false, the content in the default slot may jump up and down slightly as the progress indicator shows and hides.

<Prop def="progressAbsolute: boolean = false" lang="ts" />

Positions the progress bar absolutely. This can be useful in compact interfaces where extra space for the progress bar is undesirable, allowing the progress bar to potentially overlap content while active.

<Prop def="height: number = 10" lang="ts" />

Specifies the height in pixels of the [v-progress-linear](https://vuetifyjs.com/en/components/progress-linear) used to indicate progress.

<Prop def="title: string" lang="ts" />

Specifies the title to display in the [v-alert](https://vuetifyjs.com/en/components/alerts/) used for error and success messages.

<Prop def="density: 'compact' | 'comfortable' | 'default'" lang="ts" />

Controls the density of the [v-alert](https://vuetifyjs.com/en/components/alerts/) used for error and success messages. Use `compact` for tighter spacing, `comfortable` for medium spacing, or `default` for standard spacing.

<Prop def="
no-loading-content?: boolean;
no-error-content?: boolean;
no-initial-content?: boolean;
no-progress?: boolean;
no-initial-progress?: boolean;
no-secondary-progress?: boolean;
show-success?: boolean;
show-retry?: boolean;" lang="ts" id="flags-props" />

Component level [flags](#flags) options that control behavior when the simple form of `loaders` (single instance or array) is used, as well as provide baseline defaults that can be overridden by the advanced form of `loaders` (object map) .

## Flags

The available flags are as follows, all of which default to `true` except for `show-success` and `show-retry` which default to `false`. In the object literal syntax for `loaders`, the `no-` prefix may be omitted to set the flag to `true`.

| <div style="width:160px">Flag</div> | Description |
| - | - |
| `no-loading-content` | Controls whether the default slot is rendered while any API caller is loading (i.e. when  `caller.isLoading === true`). |
| `no-error-content` | Controls whether the default slot is rendered while any API Caller is in an error state (i.e. when  `caller.wasSuccessful === false`). |
| `no-initial-content` | Controls whether the default slot is rendered while any API Caller has yet to receive a response for the first time (i.e. when `caller.wasSuccessful === null`). |
| `no-progress` | Master toggle for whether the progress indicator is shown in any scenario. |
| `no-initial-progress` | Controls whether the progress indicator is shown when an API Caller is loading for the very first time (i.e. when  `caller.wasSuccessful === null`). |
| `no-secondary-progress` | Controls whether the progress indicator is shown when an API Caller is loading any time after its first invocation (i.e. when  `caller.wasSuccessful !== null`). |
| `show-success` | Controls whether success alerts are shown when API calls complete successfully (i.e. when `caller.wasSuccessful === true`). |
| `show-retry` | Controls whether a retry button is shown when API calls fail. The retry button will call `invokeWithArgs()` if available, otherwise `invoke()`. |

## Slots

``default`` - Accepts the content whose visibility is controlled by the state of the supplied [API Callers](/stacks/vue/layers/api-clients.md#api-callers). It will be shown or hidden according to the flags defined for each caller.

``prepend`` - Content to be placed in the `prepend` slot of the [v-alert](https://vuetifyjs.com/en/components/alerts/) used to display error and success messages.

``append`` - Content to be placed in the `append` slot of the [v-alert](https://vuetifyjs.com/en/components/alerts/) used to display error and success messages.


# c-app-update-alert

<!-- MARKER:summary -->

A component that displays a notification when a new version of the application has been deployed, prompting the user to refresh the page.

<!-- MARKER:summary-end -->

This component monitors the `X-App-Build` response header on API responses (emitted by `app.UseAppVersionHeader()` on the server). When the header value changes from the initially observed value, a [v-snackbar](https://vuetifyjs.com/en/components/snackbars/) is displayed with a refresh button.

## Examples

Place in your root `App.vue`, inside the `<v-app>` element:
``` vue-html
<v-app>
  ...
  <CAppUpdateAlert />
</v-app>
```

## Props

<Prop def="axiosInstance?: AxiosInstance" />

The Axios instance to monitor for version changes. Defaults to the Coalesce `AxiosClient`.

## Slots

<Prop def="#default" />

Override the default message text ("A new version of this application is available.").

<Prop def="#actions" />

Override the action buttons. Receives a `reload` function as a slot prop that triggers `window.location.reload()`.

``` vue-html
<CAppUpdateAlert>
  <template #actions="{ reload }">
    <v-btn @click="reload">Update Now</v-btn>
  </template>
</CAppUpdateAlert>
```

## Server Setup

The server must emit an `X-App-Build` response header for this component to function. Add the following to your ASP.NET Core middleware pipeline:

``` cs
app.UseAppVersionHeader();
```

See [UseAppVersionHeader](/topics/startup.md#member-useappversionheader) for details.

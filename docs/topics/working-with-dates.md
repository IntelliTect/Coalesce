# Working with Dates

Coalesce provides comprehensive support for working with different date and time types in both C# and TypeScript. This guide covers the various date types, their differences, and how to effectively display and format dates on the client side.

## Date Types Overview

Coalesce supports all the major .NET date and time types, each with specific behaviors and use cases:

### DateTimeOffset  
- **C# Type**: `System.DateTimeOffset`
- **Timezone Behavior**: Includes timezone offset information
- **Client Behavior**: Automatically converted to and from the user's current timezone for input and display, unless overridden
- **Use Case**: Absolute points in time that need timezone awareness (e.g., log timestamps, scheduled events across timezones), dates that would ever need to be compared relative to "now". When in doubt, prefer using DateTimeOffset.

### DateTime
- **C# Type**: `System.DateTime`
- **Timezone Behavior**: No timezone information
- **Client Behavior**: No automatic timezone conversion applied
- **Use Case**: Local dates/times where timezone doesn't matter (e.g. appointment times in a single timezone). Avoid if these dates need to be compared to "now", because "now" is timezone-dependent.

### DateOnly
- **C# Type**: `System.DateOnly`
- **Timezone Behavior**: No timezone information
- **Client Behavior**: Displays as date only, no timezone conversion
- **Use Case**: Dates without time significance (e.g., birth dates, due dates)

### TimeOnly
- **C# Type**: `System.TimeOnly`
- **Timezone Behavior**: No timezone information
- **Client Behavior**: Displays as time only
- **Use Case**: Times without date significance (e.g., daily recurring events, business hours)

## Displaying Dates

### c-display Component

The [c-display](/stacks/vue/coalesce-vue-vuetify/components/c-display) component automatically formats dates based on their type and metadata:

```vue
<template>
  <!-- Basic date display -->
  <c-display :model="event" for="eventDate" />
  
  <!-- Custom format -->
  <c-display :model="event" for="createdAt" format="yyyy-MM-dd HH:mm:ss" />
  
  <!-- Relative time display -->
  <c-display :model="event" for="createdAt" :format="{ distance: true }" />
  
  <!-- Timezone-specific display -->
  <c-display 
    :model="event" 
    for="createdAt" 
    :format="{ format: 'yyyy-MM-dd HH:mm:ss zzz', timeZone: 'America/New_York' }" 
  />
</template>

<script lang="ts" setup>
  import { EventViewModel } from '@/viewmodels.g'
  const event = new EventViewModel();
</script>
```

You can also use directly with a JavaScript `Date` object:

```vue
<template>
  <!-- Basic format -->
  <c-display :value="dateValue" format="yyyy-MM-dd HH:mm:ss" />
  
  <!-- Relative time display -->
  <c-display :value="dateValue" :format="{ distance: true }" />
  
  <!-- Timezone-specific display -->
  <c-display 
    :value="dateValue"
    :format="{ format: 'yyyy-MM-dd HH:mm:ss zzz', timeZone: 'America/New_York' }" 
  />
</template>

<script lang="ts" setup>
  const dateValue = new Date();
</script>
```

### Display Functions

The [modelDisplay](/stacks/vue/layers/models.md#modeldisplay), [propDisplay](/stacks/vue/layers/models.md#propdisplay), and [valueDisplay](/stacks/vue/layers/models.md#valuedisplay) functions from the model layer also support the same date handling options as the `c-display` component if you need to format a date on a model programmatically.

You can also use [date-fns `format` function](https://date-fns.org/docs/format) directly.

### Format Options

The `format` prop accepts several types of values:

#### String Format

String formats use [date-fns `format` function](https://date-fns.org/docs/format).

```typescript
"yyyy-MM-dd"           // 2023-12-25
"M/d/yyyy h:mm a"      // 12/25/2023 3:30 PM
"EEEE, MMMM do, yyyy" // Monday, December 25th, 2023
```

#### Object Format with Options
```typescript
// With timezone conversion
{ 
  format: 'yyyy-MM-dd HH:mm:ss zzz', 
  timeZone: 'America/New_York' 
}

// Relative time display
{ distance: true }                   // "2 hours ago"
{ distance: true, addSuffix: false } // "2 hours"
```

## Inputting Dates

The [c-datetime-picker](/stacks/vue/coalesce-vue-vuetify/components/c-datetime-picker) component automatically adapts based on the date type when bound to a `model`. It can also be used standalone without a model instance.

``` vue-html
<c-datetime-picker :model="person" for="birthDate" />

<c-datetime-picker 
  v-model="standaloneDateTime" 
  format="yyyy-MM-dd HH:mm:ss" />

<c-datetime-picker 
  v-model="standaloneTime" 
  date-kind="time"
/>
```

## Manipulating Dates

Coalesce includes [date-fns](https://date-fns.org/) and [date-fns-tz](https://github.com/marnusw/date-fns-tz) for powerful date formatting and manipulation. 

Avoid using built-in JavaScript Date formatting and manipulation functions, which are fraught with peril.

For example, here some common Date operations with date-fns:

```typescript
import { format, parseISO, addDays, subWeeks, isAfter } from 'date-fns';
import { formatInTimeZone } from 'date-fns-tz';

// Format dates
const formatted = format(new Date(), 'yyyy-MM-dd HH:mm:ss');

// Parse ISO strings safely
const date = parseISO('2023-12-25T15:30:00Z');

// Date arithmetic
const nextWeek = addDays(new Date(), 7);
const lastMonth = subMonths(new Date(), 1);

// Comparisons
const isLater = isAfter(date1, date2);

// Timezone operations
const nyTime = formatInTimeZone(date, 'America/New_York', 'yyyy-MM-dd HH:mm:ss zzz');
```

## Default Timezone Configuration

Coalesce supports timezone configuration at both the server and client levels for different purposes.

### Server-Side Configuration

You can configure a server-side timezone for handling date operations where a timezone must be assumed. Specifically, this includes search and filter operations against DateTimeOffset properties where the zoneless user input must be parsed into a DateTimeOffset. This is configured in your `Program.cs`:

```csharp
// In your Program.cs
services.AddCoalesce(builder => builder
    .AddContext<AppDbContext>()
    .UseTimeZone(TimeZoneInfo.FindSystemTimeZoneById("America/New_York"))
);

// Or use a dynamic timezone resolver
services.AddCoalesce(builder => builder
    .AddContext<AppDbContext>()
    .UseTimeZone<ITimeZoneResolver>()
);
```

### Client-Side Configuration

You can configure a default timezone for your client application to control how `DateTimeOffset` values are displayed in the UI. By default, this uses the client computer's timezone and does not need to be configured unless you want to override this. See [Time Zones](/stacks/vue/layers/models.md#time-zones) for more details.

You can override this on a per-component or per-operation basis using the `timeZone` format option described above.

```typescript
// In your main.ts file
import { setDefaultTimeZone } from 'coalesce-vue';

// Set default timezone for client-side date display
setDefaultTimeZone('America/New_York');

// Or clear the default to use browser's local timezone
setDefaultTimeZone(null);
```

When a client-side default timezone is set:
- `DateTimeOffset` values automatically convert to this timezone for display in `c-display` components
- `DateTimeOffset` values in `c-datetime-picker` components use this timezone by default
- `DateTime` properties (no offset) are **not** converted - they remain timezone-agnostic

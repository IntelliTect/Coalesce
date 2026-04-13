<template>
  <h1>Two autofocus c-selects on the same page</h1>
  <p>
    Only one should receive focus (it is probably random-ish which one wins).
  </p>
  <v-row>
    <v-col>
      <c-select for="Person" autofocus label="First (autofocus)" clearable />
    </v-col>
    <v-col>
      <c-select for="Person" autofocus label="Second (autofocus)" clearable />
    </v-col>
  </v-row>

  <h1>Autofocus should not steal focus</h1>
  <p>
    Click in the text field below. The autofocus c-selects above should not
    steal focus away from it.
  </p>
  <v-row>
    <v-col>
      <v-text-field label="Type here - focus should not be stolen" />
    </v-col>
  </v-row>

  <h1>Autofocus in a dialog</h1>
  <v-row>
    <v-col>
      <v-btn @click="dialogOpen = true">Open Dialog</v-btn>
      <v-dialog v-model="dialogOpen" max-width="500">
        <v-card title="Dialog with autofocus c-select">
          <v-card-text>
            <c-select
              for="Person"
              autofocus
              label="Autofocus in dialog"
              clearable
            />
          </v-card-text>
          <v-card-actions>
            <v-spacer />
            <v-btn @click="dialogOpen = false">Close</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </v-col>
  </v-row>

  <h1>Non-autofocus (control)</h1>
  <v-row>
    <v-col>
      <c-select for="Person" label="No autofocus" clearable />
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { ref } from "vue";

const dialogOpen = ref(false);
</script>

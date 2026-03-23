<template>
  <c-loader-status :loaders="{ '': [person.$load] }" no-initial-content>
    <h1>JSON Complex Property (CurrentWeather)</h1>
    <v-row>
      <v-col>
        <v-card class="pa-4" v-if="person.currentWeather">
          <c-input :model="person.currentWeather" for="tempFahrenheit" />
          <c-input :model="person.currentWeather" for="humidity" />
          <c-input
            :model="person.currentWeather.location"
            for="city"
            class="mt-2"
          />
          <c-input
            :model="person.currentWeather.location"
            for="state"
            class="mt-2"
          />
          <c-input
            :model="person.currentWeather.location"
            for="zip"
            class="mt-2"
          />
        </v-card>
      </v-col>
      <v-col>
        <div><strong>Dirty:</strong> {{ person.$isDirty }}</div>
        <div>
          <strong>Saving props:</strong>
          {{ Object.keys(person.$savingProps) }}
        </div>
      </v-col>
    </v-row>

    <h1 class="mt-6">JSON Complex Collection (WeatherHistory)</h1>
    <v-row>
      <v-col>
        <v-card
          v-for="(item, i) in person.weatherHistory"
          :key="i"
          class="pa-4 mb-4"
        >
          <div class="d-flex align-center mb-2">
            <strong>Entry {{ i + 1 }}</strong>
            <v-spacer />
            <v-btn
              size="small"
              color="error"
              variant="text"
              @click="person.weatherHistory!.splice(i, 1)"
            >
              Remove
            </v-btn>
          </div>
          <c-input :model="item" for="tempFahrenheit" />
          <c-input :model="item" for="humidity" />
          <c-input :model="item.location" for="city" class="mt-2" />
          <c-input :model="item.location" for="state" class="mt-2" />
        </v-card>
        <v-btn color="primary" @click="addWeatherEntry"> Add Entry </v-btn>
      </v-col>
      <v-col>
        <div><strong>Dirty:</strong> {{ person.$isDirty }}</div>
        <div><strong>Dirty Props:</strong> {{ person._dirtyProps }}</div>
        <div>
          <strong>Count:</strong> {{ person.weatherHistory?.length ?? 0 }}
        </div>
      </v-col>
    </v-row>

    <v-row class="mt-4">
      <v-col>
        <v-btn
          color="success"
          :loading="person.$save.isLoading"
          @click="person.$save()"
        >
          Save
        </v-btn>
        <c-loader-status
          :loaders="{ '': [person.$save] }"
          class="mt-2"
          show-success
        />
      </v-col>
    </v-row>
  </c-loader-status>
</template>

<script setup lang="ts">
import { PersonViewModel } from "@/viewmodels.g";
import { WeatherData } from "@/models.g";

const person = new PersonViewModel();
person.$load(15).then(() => {
  person.currentWeather ??= new WeatherData({
    location: { city: "", state: "" },
  });
});

function addWeatherEntry() {
  if (!person.weatherHistory) {
    person.weatherHistory = [];
  }
  person.weatherHistory.push(
    new WeatherData({
      tempFahrenheit: 72,
      humidity: 50,
      location: { city: "", state: "" },
    }),
  );
}
</script>

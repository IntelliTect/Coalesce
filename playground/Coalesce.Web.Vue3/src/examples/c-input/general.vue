<template>
  <h1>Date</h1>
  <v-row>
    <v-col>
      <c-input :model="person" for="lastBath" />
    </v-col>
    <v-col>
      <c-display :model="person" for="lastBath" />
    </v-col>
  </v-row>

  <h1>Enum</h1>
  <v-row>
    <v-col>
      <c-input :model="caseVm" for="status" variant="outlined">
        <template #item="{ item, props }">
          <v-list-item v-bind="props" :title="undefined">
            {{ item.raw.displayName }}: {{ item.value }}
          </v-list-item>
        </template>
        <template #selection="{ item }">
          {{ item.title }}: {{ item.value }}
        </template>
      </c-input>
    </v-col>
    <v-col>
      <c-display :model="caseVm" for="status" />
    </v-col>
  </v-row>

  <h1>Multiline string</h1>
  <v-row>
    <v-col>
      <c-input :model="caseVm" for="description" />
    </v-col>
    <v-col>
      <c-display :model="caseVm" for="description" />
    </v-col>
  </v-row>

  <h1>Color</h1>
  <v-row>
    <v-col>
      <c-input :model="caseVm" for="severity" />
    </v-col>
    <v-col>
      <c-display :model="caseVm" for="severity" />
    </v-col>
  </v-row>

  <h1>Reference nav</h1>
  <v-row>
    <v-col>
      <c-input :model="person" for="company" />
    </v-col>
    <v-col>
      <c-display :model="person" for="company" />
    </v-col>
  </v-row>

  <h1>Phone</h1>
  <v-row>
    <v-col>
      <c-input :model="company" for="phone" />
    </v-col>
    <v-col>
      <c-display :model="company" for="phone" />
    </v-col>
  </v-row>

  <h1>URL</h1>
  <v-row>
    <v-col>
      <c-input :model="company" for="websiteUrl" />
    </v-col>
    <v-col>
      <c-display :model="company" for="websiteUrl" />
    </v-col>
  </v-row>

  <h1>URL image</h1>
  <v-row>
    <v-col>
      <c-input :model="company" for="logoUrl" />
    </v-col>
    <v-col>
      <c-display :model="company" for="logoUrl" />
    </v-col>
  </v-row>

  <h1>Email</h1>
  <v-row>
    <v-col>
      <c-input :model="person" for="email" />
    </v-col>
    <v-col>
      <c-display :model="person" for="email" />
    </v-col>
  </v-row>

  <h1>Password</h1>
  <v-row>
    <v-col>
      <c-input :model="product" for="uniqueId" />
    </v-col>
    <v-col>
      <c-display :model="product" for="uniqueId" />
    </v-col>
  </v-row>

  <h1>strings</h1>
  <v-row>
    <v-col>
      <c-input :model="person" for="arbitraryCollectionOfStrings" />
    </v-col>
    <v-col>
      <c-display :model="person" for="arbitraryCollectionOfStrings" />
    </v-col>
  </v-row>

  <h1>Many-to-many</h1>
  <v-row>
    <v-col>
      <c-input :model="caseVm" for="caseProducts" />
    </v-col>
    <v-col>
      <c-display :model="caseVm" for="caseProducts" />
    </v-col>
  </v-row>

  <h1>File</h1>
  <v-row>
    <v-col>
      <c-input :model="caseVm.uploadImage" for="file" />
    </v-col>
    <v-col>
      <c-display :model="caseVm.uploadImage" for="file" />
    </v-col>
  </v-row>

  <h1>Files</h1>
  <v-row>
    <v-col>
      <c-input :model="caseVm.uploadImages" for="files" />
    </v-col>
    <v-col>
      <c-display :model="caseVm.uploadImages" for="files" />
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import {
  CaseViewModel,
  CompanyListViewModel,
  CompanyViewModel,
  PersonViewModel,
  ProductViewModel,
} from "@/viewmodels.g";

const person = new PersonViewModel();
person.$load(15);

const caseVm = new CaseViewModel();
caseVm.$load(10);

const product = new ProductViewModel();
product.$load(10);

const company = new CompanyViewModel();
company.$load(1);
</script>

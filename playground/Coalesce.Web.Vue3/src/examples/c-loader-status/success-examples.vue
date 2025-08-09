<template>
  <v-container>
    <h1>c-loader-status show-success Demo</h1>
    
    <v-alert type="info" class="mb-4">
      This demonstrates the new <code>show-success</code> prop for c-loader-status.
      Unlike other flags, this defaults to false so success alerts are hidden by default.
    </v-alert>

    <v-row>
      <v-col cols="6">
        <h3>Success alerts enabled</h3>
        <c-loader-status :loaders="person1.$save" show-success>
          <v-btn 
            color="primary" 
            @click="simulateSuccessWithMessage"
            :loading="person1.$save.isLoading"
          >
            Save Person (with message)
          </v-btn>
        </c-loader-status>
        <p class="text-caption mt-2">
          Success alert will appear with custom message.
        </p>
      </v-col>

      <v-col cols="6">
        <h3>Success alerts disabled (default)</h3>
        <c-loader-status :loaders="person2.$save">
          <v-btn 
            color="primary" 
            @click="simulateSuccessDefault"
            :loading="person2.$save.isLoading"
          >
            Save Person (hidden)
          </v-btn>
        </c-loader-status>
        <p class="text-caption mt-2">
          No success alert will appear (default behavior).
        </p>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="6">
        <h3>Success with default message</h3>
        <c-loader-status :loaders="person3.$save" show-success>
          <v-btn 
            color="primary" 
            @click="simulateSuccessNoMessage"
            :loading="person3.$save.isLoading"
          >
            Save (default message)
          </v-btn>
        </c-loader-status>
        <p class="text-caption mt-2">
          Success alert will show "Success" when no message provided.
        </p>
      </v-col>

      <v-col cols="6">
        <h3>Error for comparison</h3>
        <c-loader-status :loaders="person4.$save">
          <v-btn 
            color="error" 
            @click="simulateError"
            :loading="person4.$save.isLoading"
          >
            Trigger Error
          </v-btn>
        </c-loader-status>
        <p class="text-caption mt-2">
          Error alerts always show (existing behavior).
        </p>
      </v-col>
    </v-row>

    <v-row>
      <v-col>
        <h3>Using flags string syntax</h3>
        <c-loader-status :loaders="{ 'show-success': [person5.$save] }">
          <v-btn 
            color="primary" 
            @click="simulateSuccessFlags"
            :loading="person5.$save.isLoading"
          >
            Save with flags string
          </v-btn>
        </c-loader-status>
        <p class="text-caption mt-2">
          Using the object syntax: <code>:loaders="{ 'show-success': [person.$save] }"</code>
        </p>
      </v-col>
    </v-row>

    <v-row>
      <v-col>
        <v-btn color="secondary" @click="resetAll" variant="outlined">
          Reset All States
        </v-btn>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { PersonViewModel } from "@/viewmodels.g";

// Create real view models for testing
const person1 = new PersonViewModel();
const person2 = new PersonViewModel();
const person3 = new PersonViewModel();
const person4 = new PersonViewModel();
const person5 = new PersonViewModel();

async function simulateSuccessWithMessage() {
  person1.$save.isLoading = true;
  person1.$save.wasSuccessful = null;
  
  // Simulate API delay
  await new Promise(resolve => setTimeout(resolve, 1000));
  
  person1.$save.isLoading = false;
  person1.$save.wasSuccessful = true;
  person1.$save.message = "Person saved successfully with custom message!";
}

async function simulateSuccessDefault() {
  person2.$save.isLoading = true;
  person2.$save.wasSuccessful = null;
  
  await new Promise(resolve => setTimeout(resolve, 1000));
  
  person2.$save.isLoading = false;
  person2.$save.wasSuccessful = true;
  person2.$save.message = "This success message should not be shown";
}

async function simulateSuccessNoMessage() {
  person3.$save.isLoading = true;
  person3.$save.wasSuccessful = null;
  
  await new Promise(resolve => setTimeout(resolve, 1000));
  
  person3.$save.isLoading = false;
  person3.$save.wasSuccessful = true;
  person3.$save.message = null; // No message - should show "Success"
}

async function simulateError() {
  person4.$save.isLoading = true;
  person4.$save.wasSuccessful = null;
  
  await new Promise(resolve => setTimeout(resolve, 1000));
  
  person4.$save.isLoading = false;
  person4.$save.wasSuccessful = false;
  person4.$save.message = "Something went wrong during the operation!";
}

async function simulateSuccessFlags() {
  person5.$save.isLoading = true;
  person5.$save.wasSuccessful = null;
  
  await new Promise(resolve => setTimeout(resolve, 1000));
  
  person5.$save.isLoading = false;
  person5.$save.wasSuccessful = true;
  person5.$save.message = "Success using flags string syntax!";
}

function resetAll() {
  [person1, person2, person3, person4, person5].forEach(person => {
    person.$save.isLoading = false;
    person.$save.wasSuccessful = null;
    person.$save.message = null;
  });
}
</script>
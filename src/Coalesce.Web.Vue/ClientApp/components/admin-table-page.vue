<template>
  <v-container grid-list-lg>
    <v-layout v-if="person != null" >
      <v-flex xs12>
        <v-card >
          <v-card-actions>
            <v-container >
              <v-layout  wrap >
                <v-flex sm6 md4 xl3 v-for="prop in showProps" :key="prop.name" class="py-0">
                  <c-input :item="person" :prop="prop"/>
                </v-flex>
              </v-layout>
            </v-container>
          </v-card-actions>
          <v-card-actions right>
            <v-spacer/>
            <v-btn flat >
              <v-icon left >cancel</v-icon>
              Cancel
            </v-btn>
            {{person.$save.isLoading}}
            <v-btn color="primary"  
              @click.native="person.$save()" 
              :loading="person.$save.isLoading"
              :disabled="person.$save.isLoading"
            >
              <v-icon left >save</v-icon>
              Save 
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-flex>
      
    </v-layout>
    <v-layout >

      <v-flex xs12>
        <v-data-table
          :headers="headers"
          :items="items"
          :search="search"
          :pagination.sync="pagination"
          :total-items="count"
          :loading="isLoading"
          class="elevation-1"
          >
          <template slot="items" slot-scope="props">
            <td v-for="prop in showProps" :key="prop.name" :class="'prop-' + prop.name" >
              <c-display :item="props.item" :prop="prop.name"/>
            </td>
          </template>
        </v-data-table>
      </v-flex>
    </v-layout>
    
  </v-container>


</template>

<script lang="ts" src="./admin-table-page.ts"/>


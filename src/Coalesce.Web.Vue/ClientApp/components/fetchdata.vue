<template>
    <div>
        <h1>Tasks</h1>


        <button @click="events.nextPage()">Next Page</button>
        <table v-if="events.items.length" class="table">
            <thead>
                <tr>
                    <th>Date</th>
                    <th>Temp. (C)</th>
                    <th>Temp. (F)</th>
                    <th>Summary</th>
                </tr>
            </thead>    
            <tbody>
                <EventRow v-for="event in events.items" :event="event" :key="event.eventId"></EventRow>
            </tbody>
        </table>

        <p v-else><em>Loading...</em></p>
    </div>
</template>

<script lang="ts">
    import Vue from 'vue';
    import { Component } from 'vue-property-decorator';
    import { EventList } from '../gen/services/event.service'

    @Component({
        name: 'FetchData',
        components: {
            EventRow: require('./counter.vue')
        }
    })
    export default class extends Vue {
        events = new EventList();

        mounted() {
            this.events.pageSize = 1;
            this.events.load();
        }
    }
</script>

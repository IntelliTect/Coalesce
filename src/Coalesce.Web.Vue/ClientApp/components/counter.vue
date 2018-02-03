<template>
    <tr>
        <td>asdsdfds{{ event.eventId }}</td>
        <td>{{ event.title }}</td>
                    
        <td>
            <ul>
                <li v-for="note in event.notes" :key="note.eventNoteId">{{note.eventNoteId}}: {{note.text}}</li>
            </ul>
        </td>
        <td>
            <ul>
                <li v-for="eventApp in event.eventApplications" :key="eventApp.eventApplicationId" :class="eventApp.application.name">{{eventApp.application.name}}</li>
            </ul>
        </td>
    </tr>
</template>

<script lang="ts">
    import Vue from 'vue';
    import { Component } from 'vue-property-decorator';
    import { Event } from '../gen/models';

    @Component({
        name: 'EventRow',
        props: ['event'],
    })
    export default class extends Vue {
        event: Event;

        mounted() {
            console.log(this.event);
        }
    }
</script>

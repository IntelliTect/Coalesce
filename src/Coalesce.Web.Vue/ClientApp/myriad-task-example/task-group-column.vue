<template>
  <div class="task-group-column" data-bind="style: {width: group ? netColumnWidth() / group.netColumnWidth() * 100 + '%' : '100%'}">

        <div style="min-height: 35px">

            <template v-for="item in items" >
                <EventTask v-if="item.eventTaskId !== undefined" :task="item" :key="item.eventTaskId">
                </EventTask>
                <EventTaskGroup v-if="item.eventTaskGroupId !== undefined" :event="event" :group="item" :key="item.eventTaskGroupId">
                </EventTaskGroup>
            </template>
        </div>
    </div>
</template>


<script lang="ts">
    import Vue from 'vue';
    import { Component } from 'vue-property-decorator';
    import EventTask from './task-item.vue';
    import EventTaskGroup from './task-group.vue';

    @Component({
        name: 'EventTaskColumn',
        props: ['event', 'items'],
        components: {
            EventTask, EventTaskGroup
        }
    })
    export default class extends Vue {
        items: any;
        event: any;
        mounted() {
            
        }
    }
</script>


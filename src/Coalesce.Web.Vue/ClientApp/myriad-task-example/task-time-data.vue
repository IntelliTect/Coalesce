
// TODO

<template>
  
</template>

<script lang="tsx">
    import Vue from 'vue';
    import { Component } from 'vue-property-decorator';
    import { EventList } from '../gen/services/event.service'

    @Component({
        name: 'EventTaskItemTimeData',
        props: ['item'],
        components: {
            UserAvatar: require('./user-avatar.vue')
            // EventRow: require('./counter.vue')
        }
    })
    export default class extends Vue {
        item: any;

    }
</script>

<style>

</style>

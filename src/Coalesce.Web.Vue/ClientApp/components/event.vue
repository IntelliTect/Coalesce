<template>
    <div v-if="event">
        <h4>
            {{event.title}}
        </h4>


        <button @click="reload()">
                Reload
        </button>

        <div >
            <EventTaskColumn :items="rootTaskItems()" :event="event">
            </EventTaskColumn>
        </div>
        
    </div>
</template>

<script lang="ts">
    import Vue from 'vue';
    import { Component } from 'vue-property-decorator';
    import EventTask from './task-item.vue';
    import EventTaskColumn from './task-group-column.vue';

    @Component({
        name: 'Event',
        components: {
            EventTask,
            EventTaskColumn,
        }
    })
    export default class extends Vue {
        event: any = null;
        
        rootTaskItems() {
            return [
                ...this.event.taskGroups,
                ...this.event.tasks
            ]
                .filter(t => !t.parentGroupId)
                .sort((a, b) => a.order - b.order)
        }

        reload() {

            fetch(`/Event/Get?eventId=6375`)
                .then(response => response.json())
                .then(event => this.event = event);
        }

        mounted() {
            this.reload();
        }
    }
</script>

<style lang="scss">

    .validationMessage {
        display: none;
    }

    .task-group {
        padding: 15px 0;
        padding-bottom: 10px;

        &.editing {
            padding: 5px 5px 10px 5px;
        }

        > h4 {
            transition: padding ease-in-out 0.2s;
            margin: 0 0 3px 0;
            font-size: 20px;


            .task-time-slip {
                font-size: 75%;
            }
        }

        &.has-dropdown > h4:hover {
            padding-left: 30px;

            .dropdown {
                visibility: visible;
                opacity: 1;
                transition-delay: 0s;
            }
        }

        .dropdown {
            visibility: hidden;
            opacity: 0;
            transition: visibility 0s linear 0.2s, opacity ease-in-out 0.2s;
            position: absolute;
            top: 12px;
            left: 3px;
            font-size: 14px;
        }
    }


    .task-group, .task-item {
        &.editing {
            border: 1px solid rgb(51, 122, 183) !important;
            margin: 10px 0;
            padding-right: 5px;
            min-width: 260px;
            box-shadow: rgba(0, 0, 0, 0.42) 1px 1px 10px 0px;
            background-color: white;

            &:hover {
                background-color: white;
            }
        }
    }

    .task-group-columns {
        $group-border: hsl(0, 0%, 80%);
        $group-border-left: lighten($group-border, 5%);

        display: table;
        table-layout: fixed;
        width: 100%;
        margin-bottom: -1px;
        border-top: 1px solid $group-border;
        border-left: 1px solid transparent;

        &:hover {
            border-left-color: $group-border-left;

            /* You can do this left border with a gradient like so. Don't do this - it murders the performance. 
                Just use an actual border-left, and change the color based on hover.
            */

            /*background: linear-gradient(to right, $group-border-left 0px, $group-border-left 1px, transparent 1px);*/
            /*background-position-x: 14px;*/
        }
    }

    .task-group-column {
        vertical-align: top;
        display: table-cell;
        width: 100%;
        padding-left: 15px;
    }

    .task-time .duration {
        &::before {
            content: " \00b7 ";
            padding-right: 5px;
            color: black;
            font-weight: bold;
            white-space: initial;
        }

        padding-left: 5px;
    }
    .task-time:not(.time-slip-current) {
        white-space: nowrap;
    }



    .task-time-slip {
        padding-right: 7px;
        font-weight: 300;

        &:not(:last-child)::after {
            content: " \00b7 ";
            padding-left: 5px;
            color: black;
            font-weight: bold;
            white-space: initial;
        }
    }

    .time-slip-past {
        font-size: small;

        &.time-slip-lost {
            color: hsl(2, 56%, 74%);
        }

        &.time-slip-gained {
            color: hsl(120, 30%, 60%);
        }
    }

    .time-slip-current {
        font-weight: bold;

        &.time-slip-lost {
            color: hsl(2, 79%, 58%);
        }

        &.time-slip-gained {
            color: hsl(120, 63%, 44%);
        }
    }



    .task-item {
        &:not(:first-child), .dndPlaceholder:not(:first-child) {
            border-top: 1px solid hsl(0, 0%, 95%);
        }

        display: flex;
        transition: background-color 150ms linear;

        &:hover {
            background-color: #f4f4f4;
        }

        .strong {
            font-weight: bold;
        }

        .padded-flex label {
            /* Remove label bottom padding in editor */
            margin-top: 5px;
            margin-bottom: 0px;
        }

        .avatars {
            .avatar-container {
                font-size: 2em;
                margin: 5px;
                margin-bottom: -5px;

                .fa {
                    color: white;
                    font-size: 0.7em;
                    vertical-align: top;
                    margin-top: 0.2em;
                }
            }

            .hover-container {
                display: none;

                .avatar-circle {
                    border: 2px solid #333;

                    .fa {
                        color: #333;
                    }
                }
            }

            .avatar-container-started {
                .avatar-circle {
                    background-color: #44879d;
                }

                .avatar-circle .fa {
                    font-size: 0.6em;
                    margin-top: 0.35em;
                    margin-left: 2px;
                }
            }

            .avatar-container-finished {
                .avatar-circle {
                    background-color: #449d44;
                }

                .avatar-circle .fa {
                    margin-top: 0.3em;
                    margin-left: -1px;
                }
            }
        }

        &:hover .avatars .avatar-container:not(.hover-container) {
            display: none;
        }

        &:hover .avatars .hover-container {
            display: inline-block;
        }
    }


    .task-item, .dndPlaceholder {
        margin: 0px 0px;
        margin-bottom: -1px;
        margin-top: 1px;
    }

    .dndDraggingSource {
        display: none;
    }

    .dndPlaceholder {
        height: 40px;
        box-shadow: inset 0px 0px 10px 0px rgba(148, 148, 148, 0.50);
    }

    .task-item, .task-group {
        position: relative;
    }


    * > .hover-button {
        display: none;
    }

    *:hover > .hover-button {
        display: block;
    }

    .padded-flex {
        display: flex;
        flex-flow: row wrap;
    }

    .padded-flex > * {
        padding: 3px;
    }

</style>

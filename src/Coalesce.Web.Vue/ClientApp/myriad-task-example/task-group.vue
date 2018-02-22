<template>
  <div class="task-group">
                        
        <!-- ko if: isSaving -->
            <h4 v-if="isSaving"
                 class="pull-left" style="margin-right: 10px;">
                <i class="fa fa-pulse fa-spinner"></i>
            </h4>
        <!-- /ko -->
        
        <template v-if="!isEditing">

            <span v-if="group.scheduledStart" style="padding-left: 20px; max-width: 50%;" class="pull-right">
                <span class="thin">start:</span>
                <strong style="color: #333">
                    {{group.scheduledStart}}
                </strong>
            </span>


            <h4 @dblclick="isEditing = !isEditing">
                <span >
                    <i v-if="group.isParallel" class="fa fa-bars fa-rotate-90" ></i>
                    {{group.name}}
                    <span data-bind="taskTimeData: $data, includeDuration: true" style="padding-left: 10px">
                        task time data
                    </span>
                </span>
            
                <div class="dropdown">
                    <button class="btn btn-xs btn-default" data-toggle="dropdown" style="font-size: 100%"
                            aria-haspopup="true" aria-expanded="false" @click="isExpanded=true">
                        <i class="fa fa-ellipsis-h"></i>
                    </button>

                    <ul v-if="isExpanded" class="dropdown-menu">
                        <li>
                            <a data-bind="click: toggleIsEditing">
                                <i class="fa fa-fw fa-pencil"></i> Edit
                            </a>
                        </li>
                        <li data-bind="css: {disabled: !eventTaskGroupId()}">
                            <a data-bind="click: $root.addGroupTask">
                                <i class="fa fa-fw fa-plus"></i> Add Task
                            </a>
                        </li>
                        <li>
                            <a data-bind="click: $root.addGroupColumn, disable: isSaving">
                                <i class="fa fa-fw fa-plus"></i> Add Column
                            </a>
                        </li>
                        <li role="separator" class="divider"></li>
                        <!-- ko if: childItems.length -->
                            <li class="disabled">
                                <a>
                                    <i class="fa fa-fw fa-trash"></i> Group Not Empty
                                </a>
                            </li>
                        <!-- /ko -->
                        <!-- ko ifnot: childItems.length -->
                            <li>
                                <a data-bind="click: deleteItem">
                                    <i class="fa fa-fw fa-trash"></i> Delete
                                </a>
                            </li>
                        <!-- /ko -->
                    </ul>
                </div>
            </h4>
        </template>

        <template v-else>
            <div class="padded-flex" @dblclick.capture="isEditing=!isEditing">
                <div style="flex: 1; min-width: 200px">
                    <label>Group Name</label>
                    <input type="text" class="form-control" v-model="name" placeholder="group name">
                </div>
            </div>
        </template>

        <div class="task-group-columns">
            <EventTaskColumn v-for="column in columnsIterable()" :items="column.items" :event="event" :key="column.column">

            </EventTaskColumn>
        </div>
    </div>
</template>


<script lang="ts">
    import Vue from 'vue';
    import { Component } from 'vue-property-decorator';
    import { EventList } from '../gen/services/event.service'
    import EventTaskColumn from './task-group-column.vue';

    @Component({
        name: 'EventTaskGroup',
        props: ['event', 'group'],
        components: {
            EventTaskColumn
        },
    })
    export default class extends Vue {
        event: any;
        group: any;
        isSaving = false;
        isEditing = false;
        isExpanded = false;

        beforeCreate() {
            // https://vuejs.org/v2/guide/components.html#Circular-References-Between-Components
            if (!this.$options.components) return;
            this.$options.components.EventTaskColumn = (require('./task-group-column.vue') as any).default
        }

        // netColumnWidth() {
        //     return this.columnsIterable().reduce((prev, colData) => prev + colData.netColumnWidth(), 0)
        // }

        columnsIterable() {
            var columns = [];

            var children = this.childItems();
            var colCount = this.group.columns;
            for (var i = 0; i < colCount; i++) {
                var columnItems = children
                    .filter(t => t.parentColumn == i + 1)
                    .sort((a, b) => a.order - b.order);
                columns[i] = {
                    column: i + 1,
                    group: this.group,
                    items: columnItems
                }
            }

            return columns;
        }

        childItems() {
            var event = this.event;
            var eventTaskGroupId = this.group.eventTaskGroupId;

            if (!eventTaskGroupId) return [];

            return [
                ...event.taskGroups,
                ...event.tasks
            ].filter((g: any) => g.parentGroupId == eventTaskGroupId);
        }

        mounted() {
            
        }
    }
</script>

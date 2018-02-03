<template>
  <div class="task-item">
                        
        <div class="avatars"
            :css="isSaving ? 'cursor-pointer' : ''"
            @click="isExpanded = true"
            aria-haspopup="true"
            aria-expanded="false"
            data-toggle="dropdown" >
                
            <div v-if="isSaving"
                class="avatar-container"
                style="display: inline-block">
                <div class="avatar-circle">
                    <i class="fa fa-pulse fa-spinner" style="color: black"></i>
                </div>
            </div>
            <div v-else>
                <div class="avatar-container hover-container">
                    <div class="avatar-circle">
                        <i class="fa fa-ellipsis-h"></i>
                    </div>
                </div>

                <div v-if="task.state == 1"
                    class="avatar-container avatar-container-started">
                    <div class="avatar-circle">
                        <i class="fa fa-play"></i>
                    </div>
                </div>

                <div v-else-if="task.state == 2"
                    class="avatar-container avatar-container-finished">
                    <div class="avatar-circle">
                        <i class="fa fa-check"></i>
                    </div>
                </div>

                <div v-else>
                    <div v-if="task.assignedTo"  class="avatar-container avatar-container-new">
                    <UserAvatar :user="task.assignedTo" ></UserAvatar>
                    </div>
                    <div v-else
                            class="avatar-container avatar-container-new">
                        <div class="avatar-circle" style="background-color: #ddd"></div>
                    </div>
                </div>
            </div>
        </div>


        <ul v-if="isExpanded"
            class="dropdown-menu" style="top: 25px; left: 25px" >

            <li v-if="task.state != 1">
                <a data-bind="click: $root.taskEditor.setTaskState.bind(this, $data, @((int)EventTaskState.Started))">
                    <i class="fa fa-fw fa-play"></i> Mark as Started
                </a>
            </li>

            <li v-if="task.state != 2">
                <a data-bind="click: $root.taskEditor.setTaskState.bind(this, $data, @((int)EventTaskState.Finished))">
                    <i class="fa fa-fw fa-check"></i> Mark as Finished
                </a>
            </li>

            <li v-if="task.state != 0 ">
                <a data-bind="click: $root.taskEditor.setTaskState.bind(this, $data, @((int)EventTaskState.New))">
                    <i class="fa fa-fw fa-asterisk"></i> Mark as New
                </a>
            </li>

            <!-- @if (User.IsInRole(Roles.EnvironmentManager))
            {
                <li role="separator" class="divider"></li>
                <li>
                    <a data-bind="click: $root.taskEditor.toggleIsEditing">
                        <i class="fa fa-fw fa-pencil"></i> Edit
                    </a>
                </li>

                if (ViewBag.AdvancedEditing)
                {
                    <li data-bind="css: {disabled: !eventTaskId()}">
                        <a data-bind="click: $root.taskEditor.addGrouping">
                            <i class="fa fa-fw fa-object-group"></i> Create Grouping
                        </a>
                    </li>
                    <li role="separator" class="divider"></li>
                    <li>
                        <a data-bind="click: function(){$data.deleteItemWithConfirmation(function(){$root.load()})}">
                            <i class="fa fa-fw fa-trash"></i> Delete
                        </a>
                    </li>
                }
            } -->
        </ul>

        <div class="clearfix" style="flex: 1;">
                <!-- <div v-if="isEditing" data-bind="if: isDirty() && errors().length" style="text-align: center; padding-top: 10px; font-size: larger">
                    <span class="validation-summary-errors strong" data-bind="text: errors"></span>
                </div>
                <div class="padded-flex" >
                    <div style="flex: 1;min-width: 200px;">
                        <label>Task Name</label>
                        <textarea type="text" class="form-control" rows="1"
                                    data-bind="value: name" placeholder="task name"></textarea>
                    </div>
                    <div >
                        <label>Hours</label>
                        <input class="form-control" placeholder="hours" data-bind="value: durationHours"
                                style="width: 70px; vertical-align: top" />
                    </div>
                </div>

                <div class="padded-flex">
                    <div style="flex: 1;min-width: 200px;">
                        <label>Description</label>
                        <textarea type="text" class="form-control" rows="3"
                                    data-bind="value: description" placeholder="task notes"></textarea>
                    </div>
                </div>

                <div class="padded-flex">
                    <div style="flex: 1; min-width: 200px;">
                        <label>Assigned To</label>
                        @(Knockout.SelectForObject<EventTask>(f => f.AssignedTo, placeholder: "Assigned To"))
                    </div>
                        
                    <div style="flex: 1; min-width: 200px" >
                        <label>Scheduled Start</label>
                        <div class="input-group date">
                            <input data-bind="datePicker: scheduledStart, format: 'M/D/YYYY h:mm a',
                                    preserveTime: false, preserveDate: false, sideBySide: true, keyBinds: {left: null, right: null}"
                                    type="text" class="form-control" placeholder="scheduled start" />
                            <span class="input-group-addon">
                                <span class="fa fa-calendar"></span>
                            </span>
                        </div>
                    </div>
                </div>

                <div class="padded-flex">
                    <div style="flex: 1; min-width: 200px" >
                        <label>Actual Start</label>
                        <div class="input-group date">
                            <input data-bind="datePicker: actualStart,
                                    enable: state() && state() != @((int)EventTaskState.New), format: 'M/D/YYYY h:mm a',
                                    preserveTime: false, preserveDate: false, sideBySide: true, keyBinds: {left: null, right: null}"
                                    type="text" class="form-control" placeholder="actual start" />
                            <span class="input-group-addon">
                                <span class="fa fa-calendar"></span>
                            </span>
                        </div>
                    </div>

                    <div style="flex: 1; min-width: 200px" >
                        <label>Actual End</label>
                        <div class="input-group date">
                            <input data-bind="datePicker: actualEnd,
                                    enable: state() == @((int)EventTaskState.Finished), format: 'M/D/YYYY h:mm a',
                                    preserveTime: false, preserveDate: false, sideBySide: true, keyBinds: {left: null, right: null}"
                                    type="text" class="form-control" placeholder="actual end" />
                            <span class="input-group-addon">
                                <span class="fa fa-calendar"></span>
                            </span>
                        </div>
                    </div>
                </div>

                <div style="margin-top: 3px; margin-bottom: 7px; padding: 3px;">
                    <div class="clearfix">
                        <button class="btn btn-xs btn-success pull-right" 
                                data-bind="click: $root.taskEditor.saveItem, disable: !name() || errors().length || isSaving() ">
                                
                            <i class="fa fa-floppy-o"></i>
                            Save
                        </button>
                            
                        @if (ViewBag.AdvancedEditing)
                        {
                            <button class="btn btn-xs btn-danger" data-bind="click: deleteItemWithConfirmation">
                                <i class="fa fa-trash-o"></i> Delete
                            </button>
                        }
                            
                    </div>
                </div> -->
                
            <div v-if="!isEditing">
                <span class="pull-right">
                    <strong v-if="task.durationHours">
                        {{task.durationHours}}
                    </strong>
                    <strong v-if="task.scheduledStart">
                        <!-- data-bind="moment: scheduledStart, format: '[@@] M/D h:mm a'" -->
                        {{task.scheduledStart}}
                    </strong>
                </span>

                <div>
                    <span :style="{strong: task.state == 1}">
                        {{task.name}}
                    </span>
                    <i v-if="task.description"
                        :title="task.description"
                        @click="showDescriptionModal()"
                        class="fa fa-file-text-o task-description" ></i>
                </div>

                <span class="pull-right" data-bind="taskTimeData: $data">task time data</span>
                <small class="faded" >
                    {{task.assignedTo && task.assignedTo.fullName || 'nobody'}}
                </small>
            </div>
        </div>
    </div>
</template>


<script lang="ts">
    import Vue from 'vue';
    import Component from 'vue-class-component';
    import { EventList } from '../gen/services/event.service'
    import UserAvatar from './user-avatar.vue';
    
    @Component({
        name: 'EventTaskItem',
        props: ['task'],
        components: {
            UserAvatar
        }
    })
    export default class EventTaskItem extends Vue {
        task: any;
        isEditing = false;
        isSaving = false;
        isExpanded = false;

        showDescriptionModal() {
            console.log('desc modal '+ this.task.description)
        }
        mounted() {
            
        }
    }
</script>


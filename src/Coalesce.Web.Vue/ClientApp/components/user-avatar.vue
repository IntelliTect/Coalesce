<template>
  <div class="avatar-circle" :style="{'background-color': user.color + '!important'}">
      <span class="initials">{{initials()}}</span>
  </div>
</template>


<script lang="ts">
    import Vue from 'vue';
    import { Component } from 'vue-property-decorator';

    @Component({
        name: 'EventTaskItem',
        props: ['user'],
    })
    export default class extends Vue {
        user: any;
        initials() {
            var user = this.user;
            if (!user.fullName) return "";

            var firstInitialMatch = /, (.)/.exec(user.fullName);
            if (firstInitialMatch) {
                return firstInitialMatch[1] + user.fullName.charAt(0)
            } else {
                var lastInitialMatch = / ([^ ])[^ ]+$/.exec(user.fullName);
                if (lastInitialMatch) {
                    return user.fullName.charAt(0) + lastInitialMatch[1];
                } else {
                    return user.fullName.substr(0, 2);
                }
            }
        }
    }
</script>

<style lang="scss">

.avatar-container {
    padding-right: 5px;
    display: inline-block;
    line-height: 1em;
    .avatar-circle {
        width: 1em;
        height: 1em;
        text-align: center;
        display: inline-block;
        border-radius: 50%;
        .initials {
            position: relative;
            top: 50%;
            font-size: 0.45em;
            line-height: 0;
            color: #fff  !important;
            vertical-align: top;
            font-family: 'Open Sans', sans-serif;
            font-weight: bold;
        }
    }
}
</style>

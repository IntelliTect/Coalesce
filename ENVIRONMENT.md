Debugging & Environment Configuration
=====================================

## Debugging

### To Debug Code Generation
1. Run the `gulp` task called `coalesce:debug`. This will compile and launch the CLI tool.
2. Once the CLI tool starts, it will wait for 60 seconds for you to attach the Visual Studio debugger.
3. Attach the debugger to the PID specified in the `gulp` task output.
4. Depending on what is being diagnosed, the default Exception Settings may have to be changed to break on a specific or all CLR exceptions in order to find the issue.

### To Debug TypeScript
* Generally, you will have the most success with Chrome
* The [KnockoutJS](https://chrome.google.com/webstore/detail/knockoutjs-context-debugg/oddcpmchholgcjgjdnfjmildmlielhof) Context Debugger is very helpful for seeing what's happening at runtime with your Knockout bindings.

### To Debug controller run-time behavior
* Set breakpoints in either generated controllers, or in your own custom controllers.

## Environment Configuration
* Visual Studio 2017 15.5.* (any edition).  Get it [here](https://www.visualstudio.com/downloads/).
* .NET Core 2.0.* SDK.  Get it [here](https://www.microsoft.com/net/download/windows).
* .NET Framework 4.6.2.  Get it [here](https://www.microsoft.com/net/download/windows) (if you didn't install it with Visual Studio).
* An internet browser (Chrome, Edge, Firefox -- all are known to work).
* You can use the Git source control client built into Visual Studio.

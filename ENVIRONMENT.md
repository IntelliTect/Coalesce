Debugging & Environment Configuration
=====================================

## Debugging

### To Debug Code Generation
1. Run the `gulp` task called `coalesce:debug`. This will compile and launch the CLI tool.
2. Once the CLI tool starts, it will wait for 60 seconds for you to attach the Visual Studio debugger.
3. Attach the debugger to the PID specified in the `gulp` task output.
4. Depending on what is being diagnosed, the default Exception Settings may have to be changed to break on a specific or all CLR exceptions in order to find the issue.

### To Debug TypeScript
* TBD

### To Debug controller run-time behavior
* TBD

## Environment Configuration
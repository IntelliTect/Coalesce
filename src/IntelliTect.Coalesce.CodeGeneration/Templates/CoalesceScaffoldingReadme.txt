Directions for generating web code from a model

Command prompt with DNX suppport

Change directory to the target project (ie x.y.Web)
dnx gen scripts -dc <ContextName>
	-validateOnly to not actually generate
	-filesOnly to not install npm, etc.

Debugging
use --debug after dnx to debug. You must then attach the debugger to the process.

IntelliTect Internal Testing
	dnx gen scripts -dc DbContext

EF Migration commands
	dnx ef migrations add <MigrationName> -c DbContext -p Coalesce.Domain

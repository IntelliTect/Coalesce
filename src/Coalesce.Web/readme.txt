# To generate code based on the model use the following command from the web folder
dnx gen scripts -dc DbContext
-validateOnly to not actually generate
-filesOnly to not install npm, etc.

# Debugging
use --debug after dnx to debug. You must then attach the debugger to the process.

# Push package to feed
# run from Coalesce folder
src\Coalesce.Web\bower_components\eonasdan-bootstrap-datetimepicker\src\nuget\nuget push artifacts\bin\Intellitect.ComponentModel\debug\Intellitect.ComponentModel.1.0.0-alpha3.nupkg 536300da-5e23-433c-8f45-f84e9a225b4b -Source https://www.myget.org/F/intellitect-public/api/v2/package
src\Coalesce.Web\bower_components\eonasdan-bootstrap-datetimepicker\src\nuget\nuget push artifacts\bin\Intellitect.Extensions.CodeGenerators.Mvc\debug\Intellitect.Extensions.CodeGenerators.Mvc.1.0.0-alpha3.nupkg 536300da-5e23-433c-8f45-f84e9a225b4b -Source https://www.myget.org/F/intellitect-public/api/v2/package

#run from .web folder
bower_components\eonasdan-bootstrap-datetimepicker\src\nuget\nuget push ../../artifacts\bin\Intellitect.ComponentModel\debug\Intellitect.ComponentModel.1.0.0-alpha4.nupkg 536300da-5e23-433c-8f45-f84e9a225b4b -Source https://www.myget.org/F/intellitect-public/api/v2/package
bower_components\eonasdan-bootstrap-datetimepicker\src\nuget\nuget push ../..artifacts\bin\Intellitect.Extensions.CodeGenerators.Mvc\debug\Intellitect.Extensions.CodeGenerators.Mvc.1.0.0-alpha4.nupkg 536300da-5e23-433c-8f45-f84e9a225b4b -Source https://www.myget.org/F/intellitect-public/api/v2/package

# EF migrations
dnx ef migrations add Dates -c DbContext -p Coalesce.Domain
dnx ef migrations remove -p Coalesce.Domain

# How to debug from another project:
# http://stackoverflow.com/questions/27912558/can-i-debug-into-mvc-6-source-code
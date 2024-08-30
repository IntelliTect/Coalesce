$dir = $PSScriptRoot

dotnet new uninstall IntelliTect.Coalesce.Vue.Template
Remove-Item -Path "$dir/IntelliTect.Coalesce.Vue.*.nupkg"

dotnet pack -o $dir/.

dotnet new install $(Get-ChildItem -Path "IntelliTect.Coalesce.Vue.*.nupkg").Name

rm $dir/Test.Template.Instance/* -Recurse
dotnet new coalescevue -o $dir/Test.Template.Instance
dotnet build $dir/Test.Template.Instance
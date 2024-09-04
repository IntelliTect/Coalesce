$dir = $PSScriptRoot

dotnet new uninstall IntelliTect.Coalesce.Vue.Template
Remove-Item -Path "$dir/IntelliTect.Coalesce.Vue.*.nupkg"

dotnet pack -o $dir/.

dotnet new install $(Get-ChildItem -Path "IntelliTect.Coalesce.Vue.*.nupkg").Name

rm $dir/Test.Template.Instance/* -Recurse
dotnet new coalescevue --help
dotnet new coalescevue -o $dir/Test.Template.Instance

Push-Location $dir/Test.Template.Instance/*.Web
try {
    dotnet restore
    dotnet coalesce
    npm ci
    npm run build
    dotnet build
} finally {
    Pop-Location
}
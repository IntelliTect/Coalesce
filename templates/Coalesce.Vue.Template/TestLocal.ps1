param (
    [Parameter(Position=0)]
    [string[]]$testCases
)

$dir = $PSScriptRoot
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $false

dotnet new uninstall IntelliTect.Coalesce.Vue.Template
Remove-Item -Path "$dir/IntelliTect.Coalesce.Vue.Template.*.nupkg" -ErrorAction SilentlyContinue

$PSNativeCommandUseErrorActionPreference = $true

dotnet pack $(Get-ChildItem -Path "$dir/*.csproj") -o $dir/.

dotnet new install $(Get-ChildItem -Path "$dir/IntelliTect.Coalesce.Vue.Template.*.nupkg").FullName

dotnet new coalescevue --help

foreach ($testCase in $testCases) {
    Write-Output "-------TEST CASE------"
    if (-not $testCase) {
        Write-Output "<no options enabled>"
    } else {
        Write-Output $testCase
    }
    Write-Output "----------------------"
    Write-Output ""

    Remove-Item $dir/Test.Template.Instance/* -Recurse -Force -ErrorAction SilentlyContinue
    Invoke-Expression "dotnet new coalescevue -o $dir/Test.Template.Instance $testcase"

    Push-Location $dir/Test.Template.Instance/*.Web
    try {
        dotnet restore
        dotnet coalesce
        npm ci
        npm run build
        dotnet build
    }
    finally {
        Pop-Location
    }
}
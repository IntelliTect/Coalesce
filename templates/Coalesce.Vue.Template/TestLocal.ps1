$dir = $PSScriptRoot
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $false

dotnet new uninstall IntelliTect.Coalesce.Vue.Template
Remove-Item -Path "$dir/IntelliTect.Coalesce.Vue.Template.*.nupkg" -ErrorAction SilentlyContinue

$PSNativeCommandUseErrorActionPreference = $true

dotnet pack $(Get-ChildItem -Path "$dir/*.csproj") -o $dir/.

dotnet new install $(Get-ChildItem -Path "$dir/IntelliTect.Coalesce.Vue.Template.*.nupkg").FullName

dotnet new coalescevue --help

$testCases = 
# Nothing:
"",
# Everything:
"--Identity --MicrosoftAuth --GoogleAuth --UserPictures --AuditLogs --ExampleModel --DarkMode --TrackingBase --AppInsights --OpenAPI",
# Assorted partial variants:
"--Identity --UserPictures --TrackingBase"

foreach ($testCase in $testCases) {
    Write-Output "----------------------"
    Write-Output "-------TEST CASE------"
    Write-Output (!$testCase ? "<no options enabled>" : $testCase);
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
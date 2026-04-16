#!/usr/bin/env pwsh
# Updates the Coalesce dependency versions in the template's
# Directory.Build.props and package.json to the specified version.

param(
    [Parameter(Mandatory)]
    [string]$Version,

    [string]$TemplateContentDir = "$PSScriptRoot/content"
)

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

# Update Directory.Build.props
$propsPath = "$TemplateContentDir/Directory.Build.props"
$newVersion = "<CoalesceVersion>$Version</CoalesceVersion>"
(Get-Content $propsPath) -replace '<CoalesceVersion>.*?</CoalesceVersion>', $newVersion | Set-Content $propsPath

Write-Host "Updated Directory.Build.props:"
Get-Content $propsPath

# Update package.json
$packageJsonPath = "$TemplateContentDir/Coalesce.Starter.Vue.Web/package.json"
$packageJson = Get-Content $packageJsonPath | ConvertFrom-Json
$packageJson.dependencies.'coalesce-vue' = $Version
$packageJson.dependencies.'coalesce-vue-vuetify3' = $Version
$packageJson.devDependencies.'eslint-plugin-coalesce' = $Version
$packageJson | ConvertTo-Json -Depth 10 | Set-Content $packageJsonPath

Write-Host "Updated package.json:"
Get-Content $packageJsonPath

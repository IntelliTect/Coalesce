$ErrorActionPreference = 'Stop'

$EFDefaultParameterValues = @{
    ProjectName = ''
    ContextTypeName = ''
}

#
# Use-DbContext
#

Register-TabExpansion Use-DbContext @{
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
    Project = { GetProjects }
    StartupProject = { GetProjects }
}

<#
.SYNOPSIS
    Sets the default DbContext to use.

.DESCRIPTION
    Sets the default DbContext to use.

.PARAMETER Context
    Specifies the DbContext to use.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.PARAMETER StartupProject
    Specifies the startup project to use. If omitted, the solution's startup project is used.

.PARAMETER Environment
    Specifies the environment to use. If omitted, "Development" is used.

.LINK
    about_EntityFramework
#>
function Use-DbContext {
    [CmdletBinding(PositionalBinding = $false)]
    param ([Parameter(Position = 0, Mandatory = $true)] [string] $Context, [string] $Project, [string] $StartupProject, [string] $Environment)

    $dteProject = GetProject $Project
    $dteStartupProject = GetStartupProject $StartupProject $dteProject
    $contextTypeName = InvokeOperation $dteStartupProject $Environment $dteProject GetContextType @{ name = $Context }

    $EFDefaultParameterValues.ContextTypeName = $contextTypeName
    $EFDefaultParameterValues.ProjectName = $dteProject.ProjectName
}

#
# Add-Migration
#

Register-TabExpansion Add-Migration @{
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
    Project = { GetProjects }
    StartupProject = { GetProjects }
}

<#
.SYNOPSIS
    Adds a new migration.

.DESCRIPTION
    Adds a new migration.

.PARAMETER Name
    Specifies the name of the migration.

.PARAMETER OutputDir
    The directory (and sub-namespace) to use. If omitted, "Migrations" is used.

.PARAMETER Context
    Specifies the DbContext to use. If omitted, the default DbContext is used.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.PARAMETER StartupProject
    Specifies the startup project to use. If omitted, the solution's startup project is used.

.PARAMETER Environment
    Specifies the environment to use. If omitted, "Development" is used.

.LINK
    Remove-Migration
    Update-Database
    about_EntityFramework
#>
function Add-Migration {
    [CmdletBinding(PositionalBinding = $false)]
    param (
        [Parameter(Position = 0, Mandatory = $true)]
        [string] $Name,
        [string] $OutputDir,
        [string] $Context,
        [string] $Project,
        [string] $StartupProject,
        [string] $Environment)

    $values = ProcessCommonParameters $StartupProject $Project $Context
    $dteStartupProject = $values.StartupProject
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName

    $artifacts = InvokeOperation $dteStartupProject $Environment $dteProject AddMigration @{
        name = $Name
        outputDir = $OutputDir
        contextType = $contextTypeName
    }

    $artifacts | %{ $dteProject.ProjectItems.AddFromFile($_) | Out-Null }
    $DTE.ItemOperations.OpenFile($artifacts[0]) | Out-Null
    ShowConsole

    Write-Output 'To undo this action, use Remove-Migration.'
}

#
# Update-Database
#

Register-TabExpansion Update-Database @{
    Migration = { param ($tabExpansionContext) GetMigrations $tabExpansionContext.Context $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
    Project = { GetProjects }
    StartupProject = { GetProjects }
}

<#
.SYNOPSIS
    Updates the database to a specified migration.

.DESCRIPTION
    Updates the database to a specified migration.

.PARAMETER Migration
    Specifies the target migration. If '0', all migrations will be reverted. If omitted, all pending migrations will be applied.

.PARAMETER Context
    Specifies the DbContext to use. If omitted, the default DbContext is used.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.PARAMETER StartupProject
    Specifies the startup project to use. If omitted, the solution's startup project is used.

.PARAMETER Environment
    Specifies the environment to use. If omitted, "Development" is used.

.LINK
    Script-Migration
    about_EntityFramework
#>
function Update-Database {
    [CmdletBinding(PositionalBinding = $false)]
    param (
        [Parameter(Position = 0)]
        [string] $Migration,
        [string] $Context,
        [string] $Project,
        [string] $StartupProject,
        [string] $Environment)

    $values = ProcessCommonParameters $StartupProject $Project $Context
    $dteStartupProject = $values.StartupProject
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName

    $targetFrameworkMoniker = GetProperty $dteProject.Properties TargetFrameworkMoniker
    $frameworkName = New-Object System.Runtime.Versioning.FrameworkName $targetFrameworkMoniker
    if ($frameworkName.Identifier -eq '.NETCore') {
        throw 'Update-Database should not be used with Universal Windows apps. Instead, call DbContext.Database.Migrate() at runtime.'
    }

    InvokeOperation $dteStartupProject $Environment $dteProject UpdateDatabase @{
        targetMigration = $Migration
        contextType = $contextTypeName
    }
}

#
# Apply-Migration (Obsolete)
#

function Apply-Migration {
    # TODO: Remove before RTM
    throw 'Apply-Migration has been removed. Use Update-Database instead.'
}

#
# Script-Migration
#

Register-TabExpansion Script-Migration @{
    From = { param ($tabExpansionContext) GetMigrations $tabExpansionContext.Context $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
    To = { param ($tabExpansionContext) GetMigrations $tabExpansionContext.Context $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
    Project = { GetProjects }
    StartupProject = { GetProjects }
}

<#
.SYNOPSIS
    Generates a SQL script from migrations.

.DESCRIPTION
    Generates a SQL script from migrations.

.PARAMETER From
    Specifies the starting migration. If omitted, '0' (the initial database) is used.

.PARAMETER To
    Specifies the ending migration. If omitted, the last migration is used.

.PARAMETER Idempotent
    Generates an idempotent script that can used on a database at any migration.

.PARAMETER Context
    Specifies the DbContext to use. If omitted, the default DbContext is used.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.PARAMETER StartupProject
    Specifies the startup project to use. If omitted, the solution's startup project is used.

.PARAMETER Environment
    Specifies the environment to use. If omitted, "Development" is used.

.LINK
    Update-Database
    about_EntityFramework
#>
function Script-Migration {
    [CmdletBinding(PositionalBinding = $false)]
    param (
        [Parameter(ParameterSetName = 'WithoutTo')]
        [Parameter(ParameterSetName = 'WithTo', Mandatory = $true)]
        [string] $From,
        [Parameter(ParameterSetName = 'WithTo', Mandatory = $true)]
        [string] $To,
        [switch] $Idempotent,
        [string] $Context,
        [string] $Project,
        [string] $StartupProject,
        [string] $Environment)

    $values = ProcessCommonParameters $StartupProject $Project $Context
    $dteStartupProject = $values.StartupProject
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName

    $script = InvokeOperation $dteStartupProject $Environment $dteProject ScriptMigration @{
        fromMigration = $From
        toMigration = $To
        idempotent = [bool]$Idempotent
        contextType = $contextTypeName
    }

    try {
        # NOTE: Certain SKUs cannot create new SQL files
        $window = $DTE.ItemOperations.NewFile('General\Sql File')
        $textDocument = $window.Document.Object('TextDocument')
        $editPoint = $textDocument.StartPoint.CreateEditPoint()
        $editPoint.Insert($script)
    }
    catch {
        $fullPath = GetProperty $dteProject.Properties FullPath
        $intermediatePath = GetProperty $dteProject.ConfigurationManager.ActiveConfiguration.Properties IntermediatePath
        $fullIntermediatePath = Join-Path $fullPath $intermediatePath
        $fileName = [IO.Path]::GetRandomFileName()
        $fileName = [IO.Path]::ChangeExtension($fileName, '.sql')
        $scriptFile = Join-Path $fullIntermediatePath $fileName
        $script | Out-File $scriptFile
        $DTE.ItemOperations.OpenFile($scriptFile) | Out-Null
    }

    ShowConsole
}

#
# Remove-Migration
#

Register-TabExpansion Remove-Migration @{
    Context = { param ($tabExpansionContext) GetContextTypes $tabExpansionContext.Project $tabExpansionContext.StartupProject $tabExpansionContext.Environment }
    Project = { GetProjects }
    StartupProject = { GetProjects }
}

<#
.SYNOPSIS
    Removes the last migration.

.DESCRIPTION
    Removes the last migration.

.PARAMETER Context
    Specifies the DbContext to use. If omitted, the default DbContext is used.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.PARAMETER StartupProject
    Specifies the startup project to use. If omitted, the solution's startup project is used.

.PARAMETER Environment
    Specifies the environment to use. If omitted, "Development" is used.

.LINK
    Add-Migration
    about_EntityFramework
#>
function Remove-Migration {
    [CmdletBinding(PositionalBinding = $false)]
    param ([string] $Context, [string] $Project, [string] $StartupProject, [string] $Environment)

    $values = ProcessCommonParameters $StartupProject $Project $Context
    $dteProject = $values.Project
    $contextTypeName = $values.ContextTypeName
    $dteStartupProject = $values.StartupProject

    $filesToRemove = InvokeOperation $dteStartupProject $Environment $dteProject RemoveMigration @{
        contextType = $contextTypeName
    }

    $filesToRemove | %{
        $projectItem = GetProjectItem $dteProject $_
        if ($projectItem) {
            $projectItem.Remove()
        }
    }
}

#
# Scaffold-DbContext
#

Register-TabExpansion Scaffold-DbContext @{
    Provider = { param ($tabExpansionContext) GetProviders $tabExpansionContext.Project }
    Project = { GetProjects }
    StartupProject = { GetProjects }
}

<#
.SYNOPSIS
    Scaffolds a DbContext and entity type classes for a specified database.

.DESCRIPTION
    Scaffolds a DbContext and entity type classes for a specified database.

.PARAMETER Connection
    Specifies the connection string of the database.

.PARAMETER Provider
    Specifies the provider to use. For example, EntityFramework.MicrosoftSqlServer.

.PARAMETER OutputDirectory
    Specifies the directory to use to output the classes. If omitted, the top-level project directory is used.

.PARAMETER Context
    Specifies the name of the generated DbContext class.

.PARAMETER Schemas
    Specifies the schemas for which to generate classes.

.PARAMETER Tables
    Specifies the tables for which to generate classes.

.PARAMETER DataAnnotations
    Use DataAnnotation attributes to configure the model where possible. If omitted, the output code will use only the fluent API.

.PARAMETER Force
    Force scaffolding to overwrite existing files. Otherwise, the code will only proceed if no output files would be overwritten.

.PARAMETER Project
    Specifies the project to use. If omitted, the default project is used.

.PARAMETER StartupProject
    Specifies the startup project to use. If omitted, the solution's startup project is used.

.PARAMETER Environment
    Specifies the environment to use. If omitted, "Development" is used.

.LINK
    about_EntityFramework
#>
function Scaffold-DbContext {
    [CmdletBinding(PositionalBinding = $false)]
    param (
        [Parameter(Position = 0, Mandatory = $true)]
        [string] $Connection,
        [Parameter(Position = 1, Mandatory = $true)]
        [string] $Provider,
        [string] $OutputDirectory,
        [string] $ContextClassName,
        [string[]] $Schemas,
        [string[]] $Tables,
        [switch] $DataAnnotations,
        [switch] $Force,
        [string] $Project,
        [string] $StartupProject,
        [string] $Environment)

    $values = ProcessCommonParameters $StartupProject $Project
    $dteStartupProject = $values.StartupProject
    $dteProject = $values.Project

    $artifacts = InvokeOperation $dteStartupProject $Environment $dteProject ReverseEngineer @{
        connectionString = $Connection
        provider = $Provider
        outputDir = $OutputDirectory
        dbContextClassName = $ContextClassName
        schemaFilters = $Schemas
        tableFilters = $Tables
        useDataAnnotations = [bool]$DataAnnotations
        overwriteFiles = [bool]$Force
    }

    $artifacts | %{ $dteProject.ProjectItems.AddFromFile($_) | Out-Null }
    $DTE.ItemOperations.OpenFile($artifacts[0]) | Out-Null
    ShowConsole
}

#
# Enable-Migrations (Obsolete)
#

function Enable-Migrations {
    # TODO: Link to some docs on the changes to Migrations
    Write-Warning 'Enable-Migrations is obsolete. Use Add-Migration to start using Migrations.'
}

#
# (Private Helpers)
#

function GetProjects {
    $projects = Get-Project -All
    $groups = $projects | group Name

    return $projects | %{
        if ($groups | ? Name -eq $_.Name | ? Count -eq 1) {
            return $_.Name
        }

        return $_.ProjectName
    }
}

function GetContextTypes($projectName, $startupProjectName, $environment) {
    $values = ProcessCommonParameters $startupProjectName $projectName
    $startupProject = $values.StartupProject
    $project = $values.Project

    $contextTypes = InvokeOperation $startupProject $environment $project GetContextTypes -skipBuild

    return $contextTypes | %{ $_.SafeName }
}

function GetMigrations($contextTypeName, $projectName, $startupProjectName, $environment) {
    $values = ProcessCommonParameters $startupProjectName $projectName $contextTypeName
    $startupProject = $values.StartupProject
    $project = $values.Project
    $contextTypeName = $values.ContextTypeName

    $migrations = InvokeOperation $startupProject $environment $project GetMigrations @{ contextTypeName = $contextTypeName } -skipBuild

    return $migrations | %{ $_.SafeName }
}

function ProcessCommonParameters($startupProjectName, $projectName, $contextTypeName) {
    $project = GetProject $projectName

    if (!$contextTypeName -and $project.ProjectName -eq $EFDefaultParameterValues.ProjectName) {
        $contextTypeName = $EFDefaultParameterValues.ContextTypeName
    }

    $startupProject = GetStartupProject $startupProjectName $project

    return @{
        Project = $project
        ContextTypeName = $contextTypeName
        StartupProject = $startupProject
    }
}

function GetProject($projectName) {
    if ($projectName) {
        return Get-Project $projectName
    }

    return Get-Project
}

function ShowConsole {
    $componentModel = Get-VSComponentModel
    $powerConsoleWindow = $componentModel.GetService([NuGetConsole.IPowerConsoleWindow])
    $powerConsoleWindow.Show()
}

function InvokeOperation($startupProject, $environment, $project, $operation, $arguments = @{}, [switch] $skipBuild) {
    $startupProjectName = $startupProject.ProjectName

    Write-Verbose "Using startup project '$startupProjectName'."

    $projectName = $project.ProjectName

    Write-Verbose "Using project '$projectName'"

    $package = Get-Package -ProjectName $startupProjectName | ? Id -eq EntityFramework.Commands
    if (!($package)) {
        throw "Cannot execute this command because EntityFramework.Commands is not installed in the startup project '$startupProjectName'."
    }

    if (!$skipBuild) {
        Write-Verbose 'Build started...'

        # TODO: Only build required project. Don't use BuildProject, you can't specify platform
        $solutionBuild = $DTE.Solution.SolutionBuild
        $solutionBuild.Build($true)
        if ($solutionBuild.LastBuildInfo) {
            throw "Build failed."
        }

        Write-Verbose 'Build succeeded.'
    }

    if (![Type]::GetType('Microsoft.Data.Entity.Design.OperationResultHandler')) {
        Add-Type -Path (Join-Path $PSScriptRoot OperationHandlers.cs) -CompilerParameters (
            New-Object CodeDom.Compiler.CompilerParameters -Property @{
                CompilerOptions = '/d:ENABLE_HANDLERS'
            })
    }

    $logHandler = New-Object Microsoft.Data.Entity.Design.OperationLogHandler @(
        { param ($message) Write-Error $message }
        { param ($message) Write-Warning $message }
        { param ($message) Write-Host $message }
        { param ($message) Write-Verbose $message }
        { param ($message) Write-Debug $message }
    )

    $properties = $project.Properties
    $fullPath = GetProperty $properties FullPath

    $startupOutputPath = GetProperty $startupProject.ConfigurationManager.ActiveConfiguration.Properties OutputPath
    $startupProperties = $startupProject.Properties
    $startupFullPath = GetProperty $startupProperties FullPath
    $startupTargetDir = Join-Path $startupFullPath $startupOutputPath

    $webConfig = GetProjectItem $startupProject 'Web.Config'
    $appConfig = GetProjectItem $startupProject 'App.Config'

    if ($webConfig) {
        $configurationFile = GetProperty $webConfig.Properties FullPath
        $dataDirectory = Join-Path $startupFullPath 'App_Data'
    }
    elseif ($appConfig) {
        $configurationFile = GetProperty $appConfig.Properties FullPath
    }

    Write-Verbose "Using application base '$startupTargetDir'."

    $info = New-Object AppDomainSetup -Property @{
        ApplicationBase = $startupTargetDir
        ShadowCopyFiles = 'true'
    }

    if ($configurationFile) {
        Write-Verbose "Using application configuration '$configurationFile'"
        $info.ConfigurationFile = $configurationFile
    }
    else {
        Write-Verbose 'No configuration file found.'
    }

    $domain = [AppDomain]::CreateDomain('EntityFrameworkDesignDomain', $null, $info)
    if ($dataDirectory) {
        Write-Verbose "Using data directory '$dataDirectory'"
        $domain.SetData('DataDirectory', $dataDirectory)
    }
    try {
        $commandsAssembly = 'EntityFramework.Commands'
        $operationExecutorTypeName = 'Microsoft.Data.Entity.Design.OperationExecutor'
        $targetAssemblyName = GetProperty $properties AssemblyName
        $startupAssemblyName = GetProperty $startupProperties AssemblyName
        $rootNamespace = GetProperty $properties RootNamespace

        $executor = $domain.CreateInstanceAndUnwrap(
            $commandsAssembly,
            $operationExecutorTypeName,
            $false,
            0,
            $null,
            @(
                [MarshalByRefObject]$logHandler,
                @{
                    startupTargetName = $startupAssemblyName
                    targetName = $targetAssemblyName
                    environment = $environment
                    projectDir = $fullPath
                    rootNamespace = $rootNamespace
                }
            ),
            $null,
            $null)

        $resultHandler = New-Object Microsoft.Data.Entity.Design.OperationResultHandler
        $currentDirectory = [IO.Directory]::GetCurrentDirectory()

        Write-Verbose "Using current directory '$startupTargetDir'."

        [IO.Directory]::SetCurrentDirectory($startupTargetDir)
        try {
            $domain.CreateInstance(
                $commandsAssembly,
                "$operationExecutorTypeName+$operation",
                $false,
                0,
                $null,
                ($executor, [MarshalByRefObject]$resultHandler, $arguments),
                $null,
                $null) | Out-Null
        }
        finally {
            [IO.Directory]::SetCurrentDirectory($currentDirectory)
        }
    }
    finally {
        [AppDomain]::Unload($domain)
    }

    if ($resultHandler.ErrorType) {
        if ($resultHandler.ErrorType -eq 'Microsoft.Data.Entity.Design.OperationException') {
            Write-Verbose $resultHandler.ErrorStackTrace
        }
        else {
            Write-Host $resultHandler.ErrorStackTrace
        }

        throw $resultHandler.ErrorMessage
    }
    if ($resultHandler.HasResult) {
        return $resultHandler.Result
    }
}

function GetProperty($properties, $propertyName) {
    $property = $properties.Item($propertyName)
    if (!$property) {
        return $null
    }

    return $property.Value
}

function GetProjectItem($project, $path) {
    $fullPath = GetProperty $project.Properties FullPath

    if (Split-Path $path -IsAbsolute) {
        $path = $path.Substring($fullPath.Length)
    }

    $itemDirectory = (Split-Path $path -Parent)

    $projectItems = $project.ProjectItems
    if ($itemDirectory) {
        $directories = $itemDirectory.Split('\')
        $directories | %{
            $projectItems = $projectItems.Item($_).ProjectItems
        }
    }

    $itemName = Split-Path $path -Leaf

    try {
        return $projectItems.Item($itemName)
    }
    catch [Exception] {
    }

    return $null
}

function GetStartUpProject($name, $fallbackProject) {
    if ($name) {
        return Get-Project $name
    }

    $startupProjectPaths = $DTE.Solution.SolutionBuild.StartupProjects
    if ($startupProjectPaths) {
        if ($startupProjectPaths.Length -eq 1) {
            $startupProjectPath = $startupProjectPaths[0]
            if (!(Split-Path -IsAbsolute $startupProjectPath)) {
                $solutionPath = Split-Path (GetProperty $DTE.Solution.Properties Path)
                $startupProjectPath = Join-Path $solutionPath $startupProjectPath -Resolve
            }

            $startupProject = GetSolutionProjects | ?{
                try {
                    $fullName = $_.FullName
                }
                catch [NotImplementedException] {
                    return $false
                }

                if ($fullName -and $fullName.EndsWith('\')) {
                    $fullName = $fullName.Substring(0, $fullName.Length - 1)
                }

                return $fullName -eq $startupProjectPath
            }
            if ($startupProject) {
                return $startupProject
            }

            Write-Warning "Unable to resolve startup project '$startupProjectPath'."
        }
        else {
            Write-Verbose 'More than one startup project found.'
        }
    }
    else {
        Write-Verbose 'No startup project found.'
    }

    return $fallbackProject
}

function GetSolutionProjects() {
    $projects = New-Object System.Collections.Stack

    $DTE.Solution.Projects | %{
        $projects.Push($_)
    }

    while ($projects.Count -ne 0) {
        $project = $projects.Pop();

        # NOTE: This line is similar to doing a "yield return" in C#
        $project

        if ($project.ProjectItems) {
            $project.ProjectItems | ?{ $_.SubProject } | %{
                $projects.Push($_.SubProject)
            }
        }
    }
}

function GetProviders($projectName) {
    if (!($projectName)) {
        $projectName = (Get-Project).ProjectName
    }

    return Get-Package -ProjectName $projectName | select -ExpandProperty Id
}


# SIG # Begin signature block
# MIIkCwYJKoZIhvcNAQcCoIIj/DCCI/gCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCB78t0UbcHu07y/
# tEMZQnpJFKgILVZTFZ5/DHA+bNdSb6CCDZIwggYQMIID+KADAgECAhMzAAAAZEeE
# lIbbQRk4AAAAAABkMA0GCSqGSIb3DQEBCwUAMH4xCzAJBgNVBAYTAlVTMRMwEQYD
# VQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25p
# bmcgUENBIDIwMTEwHhcNMTUxMDI4MjAzMTQ2WhcNMTcwMTI4MjAzMTQ2WjCBgzEL
# MAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1v
# bmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjENMAsGA1UECxMETU9Q
# UjEeMBwGA1UEAxMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMIIBIjANBgkqhkiG9w0B
# AQEFAAOCAQ8AMIIBCgKCAQEAky7a2OY+mNkbD2RfTahYTRQ793qE/DwRMTrvicJK
# LUGlSF3dEp7vq2YoNNV9KlV7TE2K8sDxstNSFYu2swi4i1AL3X/7agmg3GcExPHf
# vHUYIEC+eCyZVt3u9S7dPkL5Wh8wrgEUirCCtVGg4m1l/vcYCo0wbU06p8XzNi3u
# XyygkgCxHEziy/f/JCV/14/A3ZduzrIXtsccRKckyn6B5uYxuRbZXT7RaO6+zUjQ
# hiyu3A4hwcCKw+4bk1kT9sY7gHIYiFP7q78wPqB3vVKIv3rY6LCTraEbjNR+phBQ
# EL7hyBxk+ocu+8RHZhbAhHs2r1+6hURsAg8t4LAOG6I+JQIDAQABo4IBfzCCAXsw
# HwYDVR0lBBgwFgYIKwYBBQUHAwMGCisGAQQBgjdMCAEwHQYDVR0OBBYEFFhWcQTw
# vbsz9YNozOeARvdXr9IiMFEGA1UdEQRKMEikRjBEMQ0wCwYDVQQLEwRNT1BSMTMw
# MQYDVQQFEyozMTY0Mis0OWU4YzNmMy0yMzU5LTQ3ZjYtYTNiZS02YzhjNDc1MWM0
# YjYwHwYDVR0jBBgwFoAUSG5k5VAF04KqFzc3IrVtqMp1ApUwVAYDVR0fBE0wSzBJ
# oEegRYZDaHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3BraW9wcy9jcmwvTWljQ29k
# U2lnUENBMjAxMV8yMDExLTA3LTA4LmNybDBhBggrBgEFBQcBAQRVMFMwUQYIKwYB
# BQUHMAKGRWh0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2lvcHMvY2VydHMvTWlj
# Q29kU2lnUENBMjAxMV8yMDExLTA3LTA4LmNydDAMBgNVHRMBAf8EAjAAMA0GCSqG
# SIb3DQEBCwUAA4ICAQCI4gxkQx3dXK6MO4UktZ1A1r1mrFtXNdn06DrARZkQTdu0
# kOTLdlGBCfCzk0309RLkvUgnFKpvLddrg9TGp3n80yUbRsp2AogyrlBU+gP5ggHF
# i7NjGEpj5bH+FDsMw9PygLg8JelgsvBVudw1SgUt625nY7w1vrwk+cDd58TvAyJQ
# FAW1zJ+0ySgB9lu2vwg0NKetOyL7dxe3KoRLaztUcqXoYW5CkI+Mv3m8HOeqlhyf
# FTYxPB5YXyQJPKQJYh8zC9b90JXLT7raM7mQ94ygDuFmlaiZ+QSUR3XVupdEngrm
# ZgUB5jX13M+Pl2Vv7PPFU3xlo3Uhj1wtupNC81epoxGhJ0tRuLdEajD/dCZ0xIni
# esRXCKSC4HCL3BMnSwVXtIoj/QFymFYwD5+sAZuvRSgkKyD1rDA7MPcEI2i/Bh5O
# MAo9App4sR0Gp049oSkXNhvRi/au7QG6NJBTSBbNBGJG8Qp+5QThKoQUk8mj0ugr
# 4yWRsA9JTbmqVw7u9suB5OKYBMUN4hL/yI+aFVsE/KJInvnxSzXJ1YHka45ADYMK
# AMl+fLdIqm3nx6rIN0RkoDAbvTAAXGehUCsIod049A1T3IJyUJXt3OsTd3WabhIB
# XICYfxMg10naaWcyUePgW3+VwP0XLKu4O1+8ZeGyaDSi33GnzmmyYacX3BTqMDCC
# B3owggVioAMCAQICCmEOkNIAAAAAAAMwDQYJKoZIhvcNAQELBQAwgYgxCzAJBgNV
# BAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4w
# HAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xMjAwBgNVBAMTKU1pY3Jvc29m
# dCBSb290IENlcnRpZmljYXRlIEF1dGhvcml0eSAyMDExMB4XDTExMDcwODIwNTkw
# OVoXDTI2MDcwODIxMDkwOVowfjELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hp
# bmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
# b3JhdGlvbjEoMCYGA1UEAxMfTWljcm9zb2Z0IENvZGUgU2lnbmluZyBQQ0EgMjAx
# MTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAKvw+nIQHC6t2G6qghBN
# NLrytlghn0IbKmvpWlCquAY4GgRJun/DDB7dN2vGEtgL8DjCmQawyDnVARQxQtOJ
# DXlkh36UYCRsr55JnOloXtLfm1OyCizDr9mpK656Ca/XllnKYBoF6WZ26DJSJhIv
# 56sIUM+zRLdd2MQuA3WraPPLbfM6XKEW9Ea64DhkrG5kNXimoGMPLdNAk/jj3gcN
# 1Vx5pUkp5w2+oBN3vpQ97/vjK1oQH01WKKJ6cuASOrdJXtjt7UORg9l7snuGG9k+
# sYxd6IlPhBryoS9Z5JA7La4zWMW3Pv4y07MDPbGyr5I4ftKdgCz1TlaRITUlwzlu
# ZH9TupwPrRkjhMv0ugOGjfdf8NBSv4yUh7zAIXQlXxgotswnKDglmDlKNs98sZKu
# HCOnqWbsYR9q4ShJnV+I4iVd0yFLPlLEtVc/JAPw0XpbL9Uj43BdD1FGd7P4AOG8
# rAKCX9vAFbO9G9RVS+c5oQ/pI0m8GLhEfEXkwcNyeuBy5yTfv0aZxe/CHFfbg43s
# TUkwp6uO3+xbn6/83bBm4sGXgXvt1u1L50kppxMopqd9Z4DmimJ4X7IvhNdXnFy/
# dygo8e1twyiPLI9AN0/B4YVEicQJTMXUpUMvdJX3bvh4IFgsE11glZo+TzOE2rCI
# F96eTvSWsLxGoGyY0uDWiIwLAgMBAAGjggHtMIIB6TAQBgkrBgEEAYI3FQEEAwIB
# ADAdBgNVHQ4EFgQUSG5k5VAF04KqFzc3IrVtqMp1ApUwGQYJKwYBBAGCNxQCBAwe
# CgBTAHUAYgBDAEEwCwYDVR0PBAQDAgGGMA8GA1UdEwEB/wQFMAMBAf8wHwYDVR0j
# BBgwFoAUci06AjGQQ7kUBU7h6qfHMdEjiTQwWgYDVR0fBFMwUTBPoE2gS4ZJaHR0
# cDovL2NybC5taWNyb3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWljUm9vQ2Vy
# QXV0MjAxMV8yMDExXzAzXzIyLmNybDBeBggrBgEFBQcBAQRSMFAwTgYIKwYBBQUH
# MAKGQmh0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljUm9vQ2Vy
# QXV0MjAxMV8yMDExXzAzXzIyLmNydDCBnwYDVR0gBIGXMIGUMIGRBgkrBgEEAYI3
# LgMwgYMwPwYIKwYBBQUHAgEWM2h0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2lv
# cHMvZG9jcy9wcmltYXJ5Y3BzLmh0bTBABggrBgEFBQcCAjA0HjIgHQBMAGUAZwBh
# AGwAXwBwAG8AbABpAGMAeQBfAHMAdABhAHQAZQBtAGUAbgB0AC4gHTANBgkqhkiG
# 9w0BAQsFAAOCAgEAZ/KGpZjgVHkaLtPYdGcimwuWEeFjkplCln3SeQyQwWVfLiw+
# +MNy0W2D/r4/6ArKO79HqaPzadtjvyI1pZddZYSQfYtGUFXYDJJ80hpLHPM8QotS
# 0LD9a+M+By4pm+Y9G6XUtR13lDni6WTJRD14eiPzE32mkHSDjfTLJgJGKsKKELuk
# qQUMm+1o+mgulaAqPyprWEljHwlpblqYluSD9MCP80Yr3vw70L01724lruWvJ+3Q
# 3fMOr5kol5hNDj0L8giJ1h/DMhji8MUtzluetEk5CsYKwsatruWy2dsViFFFWDgy
# cScaf7H0J/jeLDogaZiyWYlobm+nt3TDQAUGpgEqKD6CPxNNZgvAs0314Y9/HG8V
# fUWnduVAKmWjw11SYobDHWM2l4bf2vP48hahmifhzaWX0O5dY0HjWwechz4GdwbR
# BrF1HxS+YWG18NzGGwS+30HHDiju3mUv7Jf2oVyW2ADWoUa9WfOXpQlLSBCZgB/Q
# ACnFsZulP0V3HjXG0qKin3p6IvpIlR+r+0cjgPWe+L9rt0uX4ut1eBrs6jeZeRhL
# /9azI2h15q/6/IvrC4DqaTuv/DDtBEyO3991bWORPdGdVk5Pv4BXIqF4ETIheu9B
# CrE/+6jMpF3BoYibV3FWTkhFwELJm3ZbCoBIa/15n8G9bW1qyVJzEw16UM0xghXP
# MIIVywIBATCBlTB+MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQ
# MA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9u
# MSgwJgYDVQQDEx9NaWNyb3NvZnQgQ29kZSBTaWduaW5nIFBDQSAyMDExAhMzAAAA
# ZEeElIbbQRk4AAAAAABkMA0GCWCGSAFlAwQCAQUAoIG6MBkGCSqGSIb3DQEJAzEM
# BgorBgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgorBgEEAYI3AgEVMC8GCSqG
# SIb3DQEJBDEiBCA7AURO7nDVoD3i/HiumiV8GKAMe0+GxCsaGkhFkOILYTBOBgor
# BgEEAYI3AgEMMUAwPqAkgCIATQBpAGMAcgBvAHMAbwBmAHQAIABBAFMAUAAuAE4A
# RQBUoRaAFGh0dHA6Ly93d3cuYXNwLm5ldC8gMA0GCSqGSIb3DQEBAQUABIIBAAD9
# czs5HVSmniGlZmR0LAUHS2nMVuAGl+u/HzghcEcG0gY0zuDNtibp0YhVKE2U31EP
# m0ubVNOWbb1SuZ27qFmd5Qb2AywBL7QZe0lYTi0gRjnhg0l6PV2nFXmi7O1SVg+w
# YUgl3SrBOyS/NTwlGep64Hj1CC7Kv16HmKFYUGUXGx2Z+XFpvX5FyoM/WyLwYAE8
# kAnDOy8qrQE+KFWKKSfSL4lavCnZ07fB2ELTnObZ794NauLV4oOj/r/2KkVJ/Y5B
# xOCBE1UxlkOI8KyeKm6pHH9jbBUo6/gfKog0kCBAR6lAwN5Hijh3jD/8qjhxgbEu
# 8rJUYOr8fek7Ivv70PahghNNMIITSQYKKwYBBAGCNwMDATGCEzkwghM1BgkqhkiG
# 9w0BBwKgghMmMIITIgIBAzEPMA0GCWCGSAFlAwQCAQUAMIIBPQYLKoZIhvcNAQkQ
# AQSgggEsBIIBKDCCASQCAQEGCisGAQQBhFkKAwEwMTANBglghkgBZQMEAgEFAAQg
# /TafPNSo01+fSAbaem1g3H2gtA+UnHaarpBqwgydgN4CBlZy+DBQkRgTMjAxNTEy
# MjkwNTEzMDUuNDQyWjAHAgEBgAIB9KCBuaSBtjCBszELMAkGA1UEBhMCVVMxEzAR
# BgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1p
# Y3Jvc29mdCBDb3Jwb3JhdGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMebkNp
# cGhlciBEU0UgRVNOOkY1MjgtMzc3Ny04QTc2MSUwIwYDVQQDExxNaWNyb3NvZnQg
# VGltZS1TdGFtcCBTZXJ2aWNloIIO0DCCBnEwggRZoAMCAQICCmEJgSoAAAAAAAIw
# DQYJKoZIhvcNAQELBQAwgYgxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5n
# dG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9y
# YXRpb24xMjAwBgNVBAMTKU1pY3Jvc29mdCBSb290IENlcnRpZmljYXRlIEF1dGhv
# cml0eSAyMDEwMB4XDTEwMDcwMTIxMzY1NVoXDTI1MDcwMTIxNDY1NVowfDELMAkG
# A1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQx
# HjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEmMCQGA1UEAxMdTWljcm9z
# b2Z0IFRpbWUtU3RhbXAgUENBIDIwMTAwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAw
# ggEKAoIBAQCpHQ28dxGKOiDs/BOX9fp/aZRrdFQQ1aUKAIKF++18aEssX8XD5WHC
# drc+Zitb8BVTJwQxH0EbGpUdzgkTjnxhMFmxMEQP8WCIhFRDDNdNuDgIs0Ldk6zW
# czBXJoKjRQ3Q6vVHgc2/JGAyWGBG8lhHhjKEHnRhZ5FfgVSxz5NMksHEpl3RYRNu
# KMYa+YaAu99h/EbBJx0kZxJyGiGKr0tkiVBisV39dx898Fd1rL2KQk1AUdEPnAY+
# Z3/1ZsADlkR+79BL/W7lmsqxqPJ6Kgox8NpOBpG2iAg16HgcsOmZzTznL0S6p/Tc
# ZL2kAcEgCZN4zfy8wMlEXV4WnAEFTyJNAgMBAAGjggHmMIIB4jAQBgkrBgEEAYI3
# FQEEAwIBADAdBgNVHQ4EFgQU1WM6XIoxkPNDe3xGG8UzaFqFbVUwGQYJKwYBBAGC
# NxQCBAweCgBTAHUAYgBDAEEwCwYDVR0PBAQDAgGGMA8GA1UdEwEB/wQFMAMBAf8w
# HwYDVR0jBBgwFoAU1fZWy4/oolxiaNE9lJBb186aGMQwVgYDVR0fBE8wTTBLoEmg
# R4ZFaHR0cDovL2NybC5taWNyb3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWlj
# Um9vQ2VyQXV0XzIwMTAtMDYtMjMuY3JsMFoGCCsGAQUFBwEBBE4wTDBKBggrBgEF
# BQcwAoY+aHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3BraS9jZXJ0cy9NaWNSb29D
# ZXJBdXRfMjAxMC0wNi0yMy5jcnQwgaAGA1UdIAEB/wSBlTCBkjCBjwYJKwYBBAGC
# Ny4DMIGBMD0GCCsGAQUFBwIBFjFodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vUEtJ
# L2RvY3MvQ1BTL2RlZmF1bHQuaHRtMEAGCCsGAQUFBwICMDQeMiAdAEwAZQBnAGEA
# bABfAFAAbwBsAGkAYwB5AF8AUwB0AGEAdABlAG0AZQBuAHQALiAdMA0GCSqGSIb3
# DQEBCwUAA4ICAQAH5ohRDeLG4Jg/gXEDPZ2joSFvs+umzPUxvs8F4qn++ldtGTCz
# wsVmyWrf9efweL3HqJ4l4/m87WtUVwgrUYJEEvu5U4zM9GASinbMQEBBm9xcF/9c
# +V4XNZgkVkt070IQyK+/f8Z/8jd9Wj8c8pl5SpFSAK84Dxf1L3mBZdmptWvkx872
# ynoAb0swRCQiPM/tA6WWj1kpvLb9BOFwnzJKJ/1Vry/+tuWOM7tiX5rbV0Dp8c6Z
# ZpCM/2pif93FSguRJuI57BlKcWOdeyFtw5yjojz6f32WapB4pm3S4Zz5Hfw42JT0
# xqUKloakvZ4argRCg7i1gJsiOCC1JeVk7Pf0v35jWSUPei45V3aicaoGig+JFrph
# pxHLmtgOR5qAxdDNp9DvfYPw4TtxCd9ddJgiCGHasFAeb73x4QDf5zEHpJM692VH
# eOj4qEir995yfmFrb3epgcunCaw5u+zGy9iCtHLNHfS4hQEegPsbiSpUObJb2sgN
# VZl6h3M7COaYLeqN4DMuEin1wC9UJyH3yKxO2ii4sanblrKnQqLJzxlBTeCG+Sqa
# oxFmMNO7dDJL32N79ZmKLxvHIa9Zta7cRDyXUHHXodLFVeNp3lfB0d4wwP3M5k37
# Db9dT+mdHhk4L7zPWAUu7w2gUDXa7wknHNWzfjUeCLraNtvTX4/edIhJEjCCBNow
# ggPCoAMCAQICEzMAAABytngu0A9Uj4gAAAAAAHIwDQYJKoZIhvcNAQELBQAwfDEL
# MAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1v
# bmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEmMCQGA1UEAxMdTWlj
# cm9zb2Z0IFRpbWUtU3RhbXAgUENBIDIwMTAwHhcNMTUxMDA3MTgxNzM4WhcNMTcw
# MTA3MTgxNzM4WjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24x
# EDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlv
# bjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNOOkY1Mjgt
# Mzc3Ny04QTc2MSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBTZXJ2aWNl
# MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArwySfFp/b/s9/KTMw0U0
# VHqcOaMh7UnTdnA8YWZ/v1uGGFeFpZGALzpHTr3th9AOqbqDrDklNI83BSXOrt4+
# dbuYa2dVdqrZj1GWNGrHozzYZUUbPgeCDHsslEgDh4hVNKQGdu1BGUHqALy07r0g
# 54CGCpxosbrsdGVyVqFypP2o/fVEbpeWiqZSC0P4YzpsMPJHu03MzKW278/YfI+r
# VHxWw0RArDjl9hbFyK1DVb+/19Wu2NPZsfss4DDEhQwphPYV4gLpYbZBG7Wn5Y65
# uqhv8BHB3R0qbuAGALSJqbuVwGdI7Km3StUz46noERx1GQZyjtOAdkAXzCn39Rkh
# DwIDAQABo4IBGzCCARcwHQYDVR0OBBYEFPZwiokNdc0lLz6rnRGCBIF9ZThEMB8G
# A1UdIwQYMBaAFNVjOlyKMZDzQ3t8RhvFM2hahW1VMFYGA1UdHwRPME0wS6BJoEeG
# RWh0dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3RzL01pY1Rp
# bVN0YVBDQV8yMDEwLTA3LTAxLmNybDBaBggrBgEFBQcBAQROMEwwSgYIKwYBBQUH
# MAKGPmh0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljVGltU3Rh
# UENBXzIwMTAtMDctMDEuY3J0MAwGA1UdEwEB/wQCMAAwEwYDVR0lBAwwCgYIKwYB
# BQUHAwgwDQYJKoZIhvcNAQELBQADggEBAIZWcjha2TG3r5aFMOMCNNMEN98O+sIb
# JsUsd2mJcpfbJL96QkC+7PPJD4lQPeqwxyF6sGdTnFrmXlJgI4EVm92bRXraZafb
# 4a6ij71czSuVepTfH4oq4Xk68ijHP9JsV8NRBJqjRa/cNftkMphaTjDGe2fkv4iI
# ydzyCoWokb1uyBG0JgOE0gneqevEtl27MPc1nVcSrQ6P1lRPYfHIKpxNlzxJ+gCV
# 0d4A2Iuap2cZdZl5TEdFHMev9CNL2SlOQpvZE6o1Na2Yfe1cwCXPLoH0lCzsAYOR
# /pP96Ppk+Y5qejPN4m0jiVvY7P2XtsAx2wiYkGPAyOfrAf5KORFHcDehggN5MIIC
# YQIBATCB46GBuaSBtjCBszELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0
# b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3Jh
# dGlvbjENMAsGA1UECxMETU9QUjEnMCUGA1UECxMebkNpcGhlciBEU0UgRVNOOkY1
# MjgtMzc3Ny04QTc2MSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBTZXJ2
# aWNloiUKAQEwCQYFKw4DAhoFAAMVADoxxGInbGLLojAX1xE9CMdkuZw3oIHCMIG/
# pIG8MIG5MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UE
# BxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMQ0wCwYD
# VQQLEwRNT1BSMScwJQYDVQQLEx5uQ2lwaGVyIE5UUyBFU046NTdGNi1DMUUwLTU1
# NEMxKzApBgNVBAMTIk1pY3Jvc29mdCBUaW1lIFNvdXJjZSBNYXN0ZXIgQ2xvY2sw
# DQYJKoZIhvcNAQEFBQACBQDaLFCIMCIYDzIwMTUxMjI5MDAyMTI4WhgPMjAxNTEy
# MzAwMDIxMjhaMHcwPQYKKwYBBAGEWQoEATEvMC0wCgIFANosUIgCAQAwCgIBAAIC
# BnoCAf8wBwIBAAICF2QwCgIFANotoggCAQAwNgYKKwYBBAGEWQoEAjEoMCYwDAYK
# KwYBBAGEWQoDAaAKMAgCAQACAxbjYKEKMAgCAQACAwehIDANBgkqhkiG9w0BAQUF
# AAOCAQEAhUPcPk+Af4y7yXFWIS8Qzt9GKNx0UsjWMvPbMod98A2LD+adULCt57q1
# PtT7weEMak+9Wd8eSTYPxqkjEm+vUh+n3Xhuth2N8HuIel3xGSPodmvOYeu+M13I
# B3hiEaxrt93cbY9zzrxuX+SHt84YLnmvNk0SXPYQSpMPhKgPDJC97OwQtByxkjh3
# yehsyJEj8My63i9+NvwJ21pGmw4WyEmM09bIuGd8uSD5+WTTYrZjOj+5OEhNxYRk
# TB/BbVtRdkiHzquo4lE+0JJy+2ymaZTCgOAiOI4GGs4aT1K+c/XR5c8W/eitQH4o
# eFWX1YMkkKreZ+mCq9SSMV8xhsYV6DGCAvUwggLxAgEBMIGTMHwxCzAJBgNVBAYT
# AlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYD
# VQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xJjAkBgNVBAMTHU1pY3Jvc29mdCBU
# aW1lLVN0YW1wIFBDQSAyMDEwAhMzAAAAcrZ4LtAPVI+IAAAAAAByMA0GCWCGSAFl
# AwQCAQUAoIIBMjAaBgkqhkiG9w0BCQMxDQYLKoZIhvcNAQkQAQQwLwYJKoZIhvcN
# AQkEMSIEIAol6LryzTQtDiVk1aMqko94WgOJHMv74TbBHAD3+cozMIHiBgsqhkiG
# 9w0BCRACDDGB0jCBzzCBzDCBsQQUOjHEYidsYsuiMBfXET0Ix2S5nDcwgZgwgYCk
# fjB8MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMH
# UmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSYwJAYDVQQD
# Ex1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EgMjAxMAITMwAAAHK2eC7QD1SPiAAA
# AAAAcjAWBBQVEhhFG4M+z1DT22i8ZsydDm8trDANBgkqhkiG9w0BAQsFAASCAQAG
# oe/Zx64ZnIEjf3gLUbcS3nRVc+HYTIaHTA+58kpepBsAxw/ASUtg5shMa9fzTiTT
# p/MJF2DaoDAasUZH8YleXbDfJp8SNXqeA+2npAvtm6VxI9+/V0OUXuyVPWCQFY1T
# yQmSe16XS66Z3Z9T5J2rpQQyCAd3ua862VzOP5HPTpcX13fZ+TtkG9281LQWcnZD
# bg2npSltjH6TkrHsmb9L3PR3P4qQw5EXKCk4+hhCchZw87JE7SheFawVGL5WTGnz
# ZPovCA3PG/sqe3LZQyfByVPJpyzXKtWV1aOrDm4Bpm6/NY5vSJYF+bWBezfMTg+g
# EWHOlVmquKeGT8LDPtAE
# SIG # End signature block

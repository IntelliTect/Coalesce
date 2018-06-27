$env:coalesce_version = "$env:coalesce_version-rc-$env:appveyor_build_number"
if ($env:APPVEYOR_PULL_REQUEST_NUMBER) {
  $env:coalesce_version = "$env:coalesce_version-pr-$env:APPVEYOR_PULL_REQUEST_NUMBER"
}
Update-AppveyorBuild -Version "$env:coalesce_version"
if ($env:APPVEYOR_PULL_REQUEST_NUMBER) {
  $env:coalesce_version = "$env:coalesce_version-release-pr-$env:APPVEYOR_PULL_REQUEST_NUMBER-$env:appveyor_build_number"
}
Update-AppveyorBuild -Version "$env:coalesce_version"
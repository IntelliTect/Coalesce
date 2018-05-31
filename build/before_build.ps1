Set-PSDebug -Trace 1
Install-Product node 9
if ($env:APPVEYOR_PULL_REQUEST_NUMBER -ne $null) {
  "//registry.npmjs.org/:_authToken=$env:NPM_TOKEN`n" | out-file "$env:userprofile\.npmrc" -Encoding ASCII
  npm whoami
  if($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode )  }
}
node --version 
npm --version
dotnet restore
pushd .\src\coalesce-vue
yarn
if($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode )  }
if ($env:APPVEYOR_BUILD_VERSION -ne $null) {
  yarn version --no-git-tag-version --new-version $env:APPVEYOR_BUILD_VERSION
  if($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode )  }
}
yarn build
if($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode )  }
popd
pushd .\src\Coalesce.Web
yarn
if($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode )  }
popd
pushd .\src\Coalesce.Web.Vue
yarn
if($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode )  }
yarn global add gulp-cli
if($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode )  }
<#
Currently, our coalesce scripts for vue is in the knockout web project's gulpfile. 
I don't want to add gulp to Coalesce.Web.Vue
#>
popd
pushd .\src\Coalesce.Web
gulp coalesce-ko
if($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode )  }
popd
pushd .\src\Coalesce.Web
gulp coalesce-vue
if($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode )  }
      
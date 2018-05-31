Install-Product node 9
if ($env:NPM_TOKEN -ne $null) {
  "//registry.npmjs.org/:_authToken=$env:NPM_TOKEN`n" | out-file "$env:userprofile\.npmrc" -Encoding ASCII
  npm whoami
}
Write-Host "Node version:"
node --version
Write-Host "Npm Version:"
npm --version
dotnet restore
pushd .\src\coalesce-vue
yarn
if ($env:APPVEYOR_BUILD_VERSION -ne $null) {
  yarn version --no-git-tag-version --new-version $env:APPVEYOR_BUILD_VERSION
}
yarn build

popd
pushd .\src\Coalesce.Web
yarn

popd
pushd .\src\Coalesce.Web.Vue
yarn
yarn global add gulp-cli

<#
Currently, our coalesce scripts for vue is in the knockout web project's gulpfile. 
I don't want to add gulp to Coalesce.Web.Vue
#>

popd
pushd .\src\Coalesce.Web
gulp coalesce-ko

popd
pushd .\src\Coalesce.Web
gulp coalesce-vue
      
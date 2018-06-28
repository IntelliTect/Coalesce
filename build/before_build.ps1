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
Push-Location .\src\coalesce-vue
yarn
if ($env:APPVEYOR_BUILD_VERSION -ne $null) {
  yarn version --no-git-tag-version --new-version $env:APPVEYOR_BUILD_VERSION
}
yarn build
yarn test

Pop-Location
Push-Location .\src\Coalesce.Web
yarn

Pop-Location
Push-Location .\src\Coalesce.Web.Vue
yarn
yarn global add gulp-cli

<#
Currently, our coalesce scripts for vue is in the knockout web project's gulpfile. 
I don't want to add gulp to Coalesce.Web.Vue
#>

Pop-Location
Push-Location .\src\Coalesce.Web
yarn gulp coalesce-ko

Pop-Location
Push-Location .\src\Coalesce.Web
yarn gulp coalesce-vue

Pop-Location
Push-Location .\src\Coalesce.Domain
dotnet ef database update --framework netcoreapp2.1
      
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
if ($env:coalesce_version -ne $null) {
  yarn version --no-git-tag-version --new-version $env:coalesce_version
}
yarn build
yarn test --coverage --reporters="default" --reporters="jest-junit"

# upload results to AppVeyor
$wc = New-Object 'System.Net.WebClient'
$wc.UploadFile("https://ci.appveyor.com/api/testresults/junit/$($env:APPVEYOR_JOB_ID)", (Resolve-Path .\junit.xml))

Pop-Location
Push-Location .\src\Coalesce.Web.Vue
yarn

<#
Currently, our coalesce scripts for vue is in the knockout web project's gulpfile. 
I don't want to add gulp to Coalesce.Web.Vue. So, hence the `yarn gulp coalesce-vue` in Coalesce.Web.
#>

Pop-Location
Push-Location .\src\Coalesce.Web
yarn
yarn gulp coalesce-ko
yarn gulp coalesce-vue
yarn gulp build # Compile the TS/SCSS/etc for Coalesce.Web

Pop-Location
Push-Location .\src\Coalesce.Domain
dotnet ef database update --framework netcoreapp2.1
Pop-Location
      
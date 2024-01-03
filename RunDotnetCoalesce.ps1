$directories = @("Coalesce.Web.Ko", "Coalesce.Web.Vue2", "Coalesce.Web.Vue3")

foreach ($dir in $directories) {
    Set-Location -Path ".\playground\$dir"
    dotnet coalesce
    Set-Location -Path "..\.."
}

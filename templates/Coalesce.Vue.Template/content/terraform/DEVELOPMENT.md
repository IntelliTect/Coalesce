# Terraform Template Development

This README is for developers working on the Coalesce project template itself.
It is excluded from template instantiation and will not appear in generated projects.

For end-user instructions, see [README.md](README.md).

## Developer Setup

To test changes to the Terraform configuration, you can deploy it to your own
Azure subscription. 

1. Instantiate the template:

``` pwsh
Coalesce\templates\Coalesce.Vue.Template\TestLocal.ps1 -- "--Terraform --GithubActions"
cd (Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath "Coalesce.Template.TestInstance")
```

2. Follow the instructions in terraform/README.md in the new template instance.

3. Push the template instance to an actual repo and test there. Otherwise, to apply locally, apply the root with the bootstrap tfvars:

   ```bash
   cd ..  # back to terraform/
   terraform init
   terraform apply -var-file="bootstrap/terraform.tfvars"
   ```

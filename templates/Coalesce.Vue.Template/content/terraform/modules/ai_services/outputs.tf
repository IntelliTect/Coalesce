output "id" {
  description = "The ID of the AI Services account."
  value       = azurerm_cognitive_account.this.id
}

output "endpoint" {
  description = "The endpoint of the Azure OpenAI service."
  value       = azurerm_cognitive_account.this.endpoint
}

output "connection_string" {
  description = "The connection string for the Azure OpenAI service, for use with .NET Aspire's AddAzureOpenAIClient."
  value       = "Endpoint=${azurerm_cognitive_account.this.endpoint}"
}

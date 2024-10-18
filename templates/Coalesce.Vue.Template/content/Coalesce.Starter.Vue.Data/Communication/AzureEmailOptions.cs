using Azure.Identity;

namespace Coalesce.Starter.Vue.Data.Communication;

public class AzureEmailOptions
{
    /// <summary>
    /// The ACS resource endpoint, e.g. "https://my-acs-resource.unitedstates.communication.azure.com".
    /// This code is configured to use managed RBAC authentication via <see cref="DefaultAzureCredential"/>
    /// and so does not use a connection string or API keys. Assign the Contributor role to allow email sending.
    /// </summary>
    public string? Endpoint { get; set; }

    public string? SenderEmail { get; set; }
}

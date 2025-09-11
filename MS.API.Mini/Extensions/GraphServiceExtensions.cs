using Azure.Identity;
// using Microsoft.Graph;

namespace MS.API.Mini.Extensions;

public static class GraphServiceExtensions
{
    public static IServiceCollection AddMicrosoftGraph(this IServiceCollection services, IConfiguration configuration)
    {
        var graphConfig = configuration.GetSection("MicrosoftGraph");
        
        var scopes = graphConfig["GraphScopes"]!.Split(",");
        
        // Client credentials flow for app-only access
        // services.AddSingleton<GraphServiceClient>(provider =>
        // {
        //     var clientId = graphConfig["ClientId"];
        //     var clientSecret = graphConfig["ClientSecret"];
        //     var tenantId = graphConfig["TenantId"];
        //
        //     if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(tenantId))
        //     {
        //         throw new InvalidOperationException("Microsoft Graph configuration is missing required values");
        //     }
        //
        //     var options = new ClientSecretCredentialOptions
        //     {
        //         AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
        //     };
        //
        //     var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);
        //     
        //     return new GraphServiceClient(clientSecretCredential, scopes);
        // });

        return services;
    }
}
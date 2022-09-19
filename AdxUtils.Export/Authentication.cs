using AdxUtils.Options;
using Kusto.Data;

namespace AdxUtils.Export;

/// <summary>
/// Provides methods for handling authentication with Azure Data Explorer.
/// </summary>
internal static class Authentication
{
    /// <summary>
    /// Create a <see cref="KustoConnectionStringBuilder"/> instance based on the user provided authentication mechanism details.
    /// </summary>
    /// <param name="authenticationOptions">The authentication options provided by the client.</param>
    /// <param name="endpointOptions">Endpoint configuration options provided by the client.</param>
    /// <returns>An instance of a <see cref="KustoConnectionStringBuilder"/>.</returns>
    internal static KustoConnectionStringBuilder GetConnectionStringBuilder(IAuthenticationOptions authenticationOptions, IEndpointOptions endpointOptions)
    {
        KustoConnectionStringBuilder connectionStringBuilder;

        if (authenticationOptions.UseAzureCli)
        {
            connectionStringBuilder = new KustoConnectionStringBuilder(endpointOptions.Endpoint, endpointOptions.DatabaseName)
                .WithAadAzCliAuthentication();
        }
        else
        {
            connectionStringBuilder = new KustoConnectionStringBuilder(endpointOptions.Endpoint, endpointOptions.DatabaseName)
                .WithAadApplicationKeyAuthentication(
                    authenticationOptions.ClientId,
                    authenticationOptions.ClientSecret,
                    authenticationOptions.Authority);
        }

        return connectionStringBuilder;
    }
}
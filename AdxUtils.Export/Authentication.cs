using AdxUtils.Options;
using Kusto.Data;

namespace AdxUtils.Export;

/// <summary>
/// Provides methods for handling authentication with Azure Data Explorer.
/// </summary>
public static class Authentication
{
    /// <summary>
    /// Create a <see cref="KustoConnectionStringBuilder"/> instance based on the user provided authentication mechanism details.
    /// </summary>
    /// <param name="authenticationOptions">The authentication options provided by the client.</param>
    /// <returns>An instance of a <see cref="KustoConnectionStringBuilder"/>.</returns>
    public static KustoConnectionStringBuilder GetConnectionStringBuilder(IAuthenticationOptions authenticationOptions)
    {
        KustoConnectionStringBuilder connectionStringBuilder;

        if (authenticationOptions.UseAzureCli)
        {
            connectionStringBuilder = new KustoConnectionStringBuilder(authenticationOptions.Endpoint, authenticationOptions.DatabaseName)
                .WithAadAzCliAuthentication();
        }
        else
        {
            connectionStringBuilder = new KustoConnectionStringBuilder(authenticationOptions.Endpoint, authenticationOptions.DatabaseName)
                .WithAadApplicationKeyAuthentication(
                    authenticationOptions.ClientId,
                    authenticationOptions.ClientSecret,
                    authenticationOptions.Authority);
        }

        return connectionStringBuilder; 
    }
}
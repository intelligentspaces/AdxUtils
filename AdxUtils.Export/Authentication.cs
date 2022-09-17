using AdxUtils.Options;
using Kusto.Data;

namespace AdxUtils.Export;

internal static class Authentication
{
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
using AdxUtils.Options;

namespace AdxUtils.Export.Tests;

public class AuthenticationTests
{
    [Fact]
    public void WhenRequestingConnectionString_IfUsingCli_ThenCorrectObjectIsReturned()
    {
        var options = new TestAuthenticationOptions
        {
            Endpoint = "https://testendpoint",
            DatabaseName = "db01",
            UseAzureCli = true
        };

        var connectionStringBuilder = Authentication.GetConnectionStringBuilder(options);

        connectionStringBuilder.EnableAzCliAuthentication.Should().BeTrue();
        connectionStringBuilder.ConnectionString.Should()
            .StartWith("Data Source=https://testendpoint;Initial Catalog=db01");
    }
    
    [Fact]
    public void WhenRequestingConnectionString_IfUsingClientSecret_ThenCorrectObjectIsReturned()
    {
        var options = new TestAuthenticationOptions
        {
            Endpoint = "https://testendpoint",
            DatabaseName = "db01",
            UseAzureCli = false,
            ClientId = "client-id-abc",
            ClientSecret = "test-secret",
            Authority = "example.com"
        };

        var connectionStringBuilder = Authentication.GetConnectionStringBuilder(options);

        connectionStringBuilder.EnableAzCliAuthentication.Should().BeFalse();
        connectionStringBuilder.ConnectionString.Should()
            .StartWith("Data Source=https://testendpoint;Initial Catalog=db01")
            .And.Contain("Application Client Id=client-id-abc")
            .And.Contain("Application Key=test-secret")
            .And.Contain("Authority Id=example.com");
    }
}

public class TestAuthenticationOptions : IAuthenticationOptions
{
    public string Endpoint { get; set; } = string.Empty;
    
    public string DatabaseName { get; set; } = string.Empty;
    
    public bool UseAzureCli { get; set; }
    
    public string ClientId { get; set; } = string.Empty;
    
    public string ClientSecret { get; set; } = string.Empty;
    
    public string Authority { get; set; } = string.Empty;
}
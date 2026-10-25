using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace TaskManager.Api.IntegrationTests.Fixtures;

public sealed class CustomWebApplicationFactory(string connectionString, FakeTimeProvider time)
    : WebApplicationFactory<Program>
{
    public const string JwtKey = "integration-tests-signing-key-0123456789-abcdef";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not Development, so the demo seed does not run.
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", connectionString);
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Jwt:Issuer", "TaskManager.Tests");
        builder.UseSetting("Jwt:Audience", "TaskManager.Tests.Web");
        builder.UseSetting("RateLimiting:LoginPermitLimit", "1000");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(time);
        });
    }
}

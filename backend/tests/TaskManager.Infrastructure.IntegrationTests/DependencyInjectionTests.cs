using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaskManager.Application.Abstractions;
using TaskManager.Infrastructure.Authentication;
using TaskManager.Infrastructure.Persistence.Repositories;
using TaskManager.Infrastructure.Persistence.Seed;

namespace TaskManager.Infrastructure.IntegrationTests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_WithValidSettings_ResolvesPortsAndServices()
    {
        // Arrange
        using var provider = BuildProvider(jwtKey: new string('k', 32));
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        // Act
        var taskRepository = services.GetRequiredService<ITaskRepository>();
        var userRepository = services.GetRequiredService<IUserRepository>();
        var tokenService = services.GetRequiredService<ITokenService>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var initializer = services.GetService<DatabaseInitializer>();
        var timeProvider = services.GetService<TimeProvider>();

        // Assert
        taskRepository.ShouldBeOfType<TaskRepository>();
        userRepository.ShouldBeOfType<UserRepository>();
        tokenService.ShouldBeOfType<JwtTokenService>();
        passwordHasher.ShouldBeOfType<PasswordHasher>();
        initializer.ShouldNotBeNull();
        timeProvider.ShouldBe(TimeProvider.System);
    }

    [Fact]
    public void AddInfrastructure_WithKeyShorterThan32Chars_FailsValidation()
    {
        // Arrange
        using var provider = BuildProvider(jwtKey: new string('k', 31));

        // Act
        var act = () => provider.GetRequiredService<IOptions<JwtOptions>>().Value;

        // Assert
        act.ShouldThrow<OptionsValidationException>();
    }

    private static ServiceProvider BuildProvider(string jwtKey)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Host=localhost;Database=taskmanager",
                ["Jwt:Issuer"] = "TaskManager",
                ["Jwt:Audience"] = "TaskManager.Web",
                ["Jwt:Key"] = jwtKey,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider(validateScopes: true);
    }
}

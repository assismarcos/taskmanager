using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Auth;

namespace TaskManager.Application.UnitTests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersAuthServiceAndValidators()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();

        // Assert
        services.ShouldContain(d =>
            d.ServiceType == typeof(IAuthService)
            && d.ImplementationType == typeof(AuthService)
            && d.Lifetime == ServiceLifetime.Scoped);
        services.ShouldContain(d => d.ServiceType == typeof(IValidator<RegisterRequest>));
        services.ShouldContain(d => d.ServiceType == typeof(IValidator<LoginRequest>));
    }
}

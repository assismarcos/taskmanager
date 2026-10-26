using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Auth;
using TaskManager.Application.Tasks;

namespace TaskManager.Application.UnitTests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersServicesAndValidators()
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
        services.ShouldContain(d =>
            d.ServiceType == typeof(ITaskService)
            && d.ImplementationType == typeof(TaskService)
            && d.Lifetime == ServiceLifetime.Scoped);
        services.ShouldContain(d => d.ServiceType == typeof(IValidator<RegisterRequest>));
        services.ShouldContain(d => d.ServiceType == typeof(IValidator<LoginRequest>));
    }
}

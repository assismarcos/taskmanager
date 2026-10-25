using System.Reflection;
using NetArchTest.Rules;
using TaskManager.Domain.Tasks;

namespace TaskManager.ArchitectureTests;

public sealed class DependencyRulesTests
{
    private const string ApiAssemblyName = "TaskManager.Api";
    private const string EntityFrameworkCoreNamespace = "Microsoft.EntityFrameworkCore";
    private const string AspNetCoreNamespace = "Microsoft.AspNetCore";

    private static readonly Assembly _domainAssembly = typeof(TaskItem).Assembly;
    private static readonly Assembly _applicationAssembly = typeof(Application.DependencyInjection).Assembly;
    private static readonly Assembly _infrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly _apiAssembly = Assembly.Load(ApiAssemblyName);

    // Each project's root namespace equals its assembly name
    private static readonly string _domainNamespace = _domainAssembly.GetName().Name!;
    private static readonly string _applicationNamespace = _applicationAssembly.GetName().Name!;
    private static readonly string _infrastructureNamespace = _infrastructureAssembly.GetName().Name!;
    private static readonly string _apiNamespace = _apiAssembly.GetName().Name!;

    [Fact]
    public void Domain_ShouldNotDependOnAnyOtherProject()
    {
        // Act
        var result = Types.InAssembly(_domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(_applicationNamespace, _infrastructureNamespace, _apiNamespace)
            .GetResult();

        // Assert
        AssertNoViolations(result);
    }

    [Fact]
    public void Domain_ShouldNotDependOnEntityFrameworkCore()
    {
        // Act
        var result = Types.InAssembly(_domainAssembly)
            .ShouldNot()
            .HaveDependencyOn(EntityFrameworkCoreNamespace)
            .GetResult();

        // Assert
        AssertNoViolations(result);
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureOrApi()
    {
        // Act
        var result = Types.InAssembly(_applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(_infrastructureNamespace, _apiNamespace)
            .GetResult();

        // Assert
        AssertNoViolations(result);
    }

    [Fact]
    public void Application_ShouldNotDependOnEntityFrameworkCoreOrAspNetCore()
    {
        // Act
        var result = Types.InAssembly(_applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(EntityFrameworkCoreNamespace, AspNetCoreNamespace)
            .GetResult();

        // Assert
        AssertNoViolations(result);
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnApi()
    {
        // Act
        var result = Types.InAssembly(_infrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(_apiNamespace)
            .GetResult();

        // Assert
        AssertNoViolations(result);
    }

    [Fact]
    public void Controllers_ShouldNotDependOnInfrastructure()
    {
        // Act
        var result = Types.InAssembly(_apiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .ShouldNot()
            .HaveDependencyOn(_infrastructureNamespace)
            .GetResult();

        // Assert
        AssertNoViolations(result);
    }

    private static void AssertNoViolations(NetArchTest.Rules.TestResult result)
    {
        var offenders = string.Join(", ", result.FailingTypeNames ?? []);
        result.IsSuccessful.ShouldBeTrue($"Forbidden dependencies found in: {offenders}");
    }
}

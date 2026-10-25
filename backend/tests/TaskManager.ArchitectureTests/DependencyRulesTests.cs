using System.Reflection;
using NetArchTest.Rules;
using TaskManager.Application.Common;
using TaskManager.Domain.Tasks;

namespace TaskManager.ArchitectureTests;

public class DependencyRulesTests
{
    private const string ApplicationNamespace = "TaskManager.Application";
    private const string InfrastructureNamespace = "TaskManager.Infrastructure";
    private const string ApiNamespace = "TaskManager.Api";
    private const string ControllersNamespace = "TaskManager.Api.Controllers";
    private const string EntityFrameworkCore = "Microsoft.EntityFrameworkCore";
    private const string AspNetCore = "Microsoft.AspNetCore";

    private static readonly Assembly DomainAssembly = typeof(TaskItem).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Result).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly ApiAssembly = Assembly.Load(ApiNamespace);

    [Fact]
    public void Domain_ShouldNotDependOnAnyOtherProject()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        AssertNoViolations(result);
    }

    [Fact]
    public void Domain_ShouldNotDependOnEntityFrameworkCore()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(EntityFrameworkCore)
            .GetResult();

        AssertNoViolations(result);
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        AssertNoViolations(result);
    }

    [Fact]
    public void Application_ShouldNotDependOnEntityFrameworkCoreOrAspNetCore()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(EntityFrameworkCore, AspNetCore)
            .GetResult();

        AssertNoViolations(result);
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        AssertNoViolations(result);
    }

    // Passes trivially until the controllers exist
    [Fact]
    public void Controllers_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .ResideInNamespace(ControllersNamespace)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        AssertNoViolations(result);
    }

    private static void AssertNoViolations(NetArchTest.Rules.TestResult result)
    {
        var offenders = string.Join(", ", result.FailingTypeNames ?? []);
        result.IsSuccessful.ShouldBeTrue($"Forbidden dependencies found in: {offenders}");
    }
}

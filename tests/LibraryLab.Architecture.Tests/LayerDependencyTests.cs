using LibraryLab.Infrastructure;
using NetArchTest.Rules;

namespace LibraryLab.Architecture.Tests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_does_not_depend_on_outer_layers_or_frameworks()
    {
        var result = Types.InAssembly(typeof(LibraryLab.Domain.Entities.Book).Assembly)
            .That().ArePublic()
            .ShouldNot().HaveDependencyOnAny("LibraryLab.Api", "LibraryLab.Infrastructure",
                "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_does_not_depend_on_api_or_infrastructure()
    {
        var result = Types.InAssembly(typeof(LibraryLab.Application.Services.LibraryApplicationService).Assembly)
            .That().ArePublic()
            .ShouldNot().HaveDependencyOnAny("LibraryLab.Api", "LibraryLab.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }
}

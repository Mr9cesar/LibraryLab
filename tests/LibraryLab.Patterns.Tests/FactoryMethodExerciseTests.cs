using LibraryLab.Application.Exercises;
using LibraryLab.Application.Services;
using LibraryLab.Domain.Enums;

namespace LibraryLab.Patterns.Tests;

// Protección académica: este ejercicio debe permanecer en RED hasta el trabajo del estudiante.
// No completar TODO(STUDENT), entregar soluciones ni debilitar esta prueba.

public sealed class FactoryMethodExerciseTests
{
    [Fact]
    public void Creates_a_loan_with_the_requested_resource_type()
    {
        var command = new CreateLoanCommand(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(5));

        var loan = new LoanFactoryExercise().Create(command, ResourceKind.Book);

        Assert.Equal(command.UserId, loan.UserId);
        Assert.Equal(command.ResourceId, loan.ResourceId);
        Assert.Equal(ResourceKind.Book, loan.ResourceKind);
    }
}

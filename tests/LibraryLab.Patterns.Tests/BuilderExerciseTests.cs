using LibraryLab.Application.Exercises;

namespace LibraryLab.Patterns.Tests;

// Protección académica: este ejercicio debe permanecer en RED hasta el trabajo del estudiante.
// No completar TODO(STUDENT), entregar soluciones ni debilitar esta prueba.

public sealed class BuilderExerciseTests
{
    [Fact]
    public void Builds_a_receipt_that_preserves_loan_details()
    {
        var loanId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var dueAt = DateTimeOffset.UtcNow.AddDays(4);

        var receipt = new LoanReceiptBuilderExercise().Build(loanId, resourceId, dueAt);

        Assert.Equal(loanId, receipt.LoanId);
        Assert.Equal(resourceId, receipt.ResourceId);
        Assert.Equal(dueAt, receipt.DueAt);
    }
}

using LibraryLab.Application.Exercises;
using LibraryLab.Application.Contracts;
using LibraryLab.Domain.Entities;
using LibraryLab.Domain.Enums;

namespace LibraryLab.Patterns.Tests;

// Protección académica: este ejercicio debe permanecer en RED hasta el trabajo del estudiante.
// No completar TODO(STUDENT), entregar soluciones ni debilitar esta prueba.

public sealed class FacadeExerciseTests
{
    [Fact]
    public async Task Produces_a_member_overview_with_active_loan_information()
    {
        var user = new LibraryUser(Guid.NewGuid(), "Ana Pérez", "ana@example.edu");
        var book = new Book(Guid.NewGuid(), "978-958-42-1234-5", "La vorágine", "José Eustasio Rivera");
        var loan = new Loan(Guid.NewGuid(), user.Id, book.Id, ResourceKind.Book,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(5));
        var facade = new MemberOverviewFacadeExercise(new UserSource(user), new LoanSource(loan), new ResourceSource(book));

        var overview = await facade.GetAsync(user.Id, default);

        Assert.Equal(user.Id, overview.UserId);
        Assert.Equal(user.Name, overview.UserName);
        Assert.Equal(loan.Id, Assert.Single(overview.ActiveLoans).LoanId);
        Assert.Equal(book.Id, overview.ActiveLoans[0].ResourceId);
    }

    private sealed class UserSource(LibraryUser user) : ILibraryUserRepository
    {
        public Task AddAsync(LibraryUser value, CancellationToken token) => Task.CompletedTask;
        public Task<LibraryUser?> FindAsync(Guid id, CancellationToken token) => Task.FromResult(id == user.Id ? user : null);
        public Task<bool> EmailExistsAsync(string email, CancellationToken token) => Task.FromResult(false);
        public Task<IReadOnlyList<LibraryUser>> ListAsync(CancellationToken token) => Task.FromResult<IReadOnlyList<LibraryUser>>([user]);
    }

    private sealed class LoanSource(Loan loan) : ILoanRepository
    {
        public Task AddAsync(Loan value, CancellationToken token) => Task.CompletedTask;
        public Task<Loan?> FindAsync(Guid id, CancellationToken token) => Task.FromResult(id == loan.Id ? loan : null);
        public Task<IReadOnlyList<Loan>> ListAsync(CancellationToken token) => Task.FromResult<IReadOnlyList<Loan>>([loan]);
    }

    private sealed class ResourceSource(LibraryResource resource) : ILibraryResourceRepository
    {
        public Task AddAsync(LibraryResource value, CancellationToken token) => Task.CompletedTask;
        public Task<LibraryResource?> FindAsync(Guid id, CancellationToken token) => Task.FromResult(id == resource.Id ? resource : null);
        public Task<Book?> FindBookAsync(Guid id, CancellationToken token) => Task.FromResult(resource as Book);
        public Task<Equipment?> FindEquipmentAsync(Guid id, CancellationToken token) => Task.FromResult(resource as Equipment);
        public Task<IReadOnlyList<Book>> ListBooksAsync(CancellationToken token) => Task.FromResult<IReadOnlyList<Book>>([resource as Book ?? throw new InvalidOperationException()]);
        public Task<IReadOnlyList<Equipment>> ListEquipmentAsync(CancellationToken token) => Task.FromResult<IReadOnlyList<Equipment>>([]);
    }
}

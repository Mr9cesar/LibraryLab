using LibraryLab.Application.Contracts;
using LibraryLab.Application.Services;
using LibraryLab.Domain.Entities;
using LibraryLab.Domain.Enums;

namespace LibraryLab.Application.Tests;

public sealed class LoanWorkflowTests
{
    [Fact]
    public async Task Loan_workflow_reserves_and_releases_resource()
    {
        var user = new LibraryUser(Guid.NewGuid(), "Ana Pérez", "ana@example.edu");
        var book = new Book(Guid.NewGuid(), "978-958-42-1234-5", "La vorágine", "José Eustasio Rivera");
        var users = new TestUserRepository(user);
        var resources = new TestResourceRepository(book);
        var loans = new TestLoanRepository();
        var service = new LibraryApplicationService(users, resources, loans, new TestUnitOfWork());

        var created = await service.CreateLoanAsync(new(user.Id, book.Id, DateTimeOffset.UtcNow.AddDays(5)), default);

        Assert.Equal(ResourceKind.Book, created.ResourceKind);
        Assert.False(book.IsAvailable);
        var returned = await service.ReturnLoanAsync(created.Id, default);
        Assert.Equal(LoanStatus.Returned, returned.Status);
        Assert.True(book.IsAvailable);
    }

    [Fact]
    public async Task Suspended_user_cannot_borrow()
    {
        var user = new LibraryUser(Guid.NewGuid(), "Ana Pérez", "ana@example.edu");
        user.Suspend();
        var book = new Book(Guid.NewGuid(), "978-958-42-1234-5", "La vorágine", "José Eustasio Rivera");
        var service = new LibraryApplicationService(new TestUserRepository(user), new TestResourceRepository(book),
            new TestLoanRepository(), new TestUnitOfWork());

        await Assert.ThrowsAsync<ApplicationConflictException>(() =>
            service.CreateLoanAsync(new(user.Id, book.Id, DateTimeOffset.UtcNow.AddDays(5)), default));
        Assert.True(book.IsAvailable);
    }

    [Fact]
    public async Task Unavailable_resource_is_a_conflict()
    {
        var user = new LibraryUser(Guid.NewGuid(), "Ana Pérez", "ana@example.edu");
        var book = new Book(Guid.NewGuid(), "978-958-42-1234-5", "La vorágine", "José Eustasio Rivera");
        book.Reserve();
        var service = new LibraryApplicationService(new TestUserRepository(user), new TestResourceRepository(book),
            new TestLoanRepository(), new TestUnitOfWork());

        await Assert.ThrowsAsync<ApplicationConflictException>(() =>
            service.CreateLoanAsync(new(user.Id, book.Id, DateTimeOffset.UtcNow.AddDays(5)), default));
    }

    [Fact]
    public async Task Inactive_resource_is_a_conflict()
    {
        var user = new LibraryUser(Guid.NewGuid(), "Ana Pérez", "ana@example.edu");
        var equipment = new Equipment(Guid.NewGuid(), "EQ-102", "Portátil", EquipmentType.Laptop);
        equipment.SetStatus(ResourceStatus.Maintenance);
        var service = new LibraryApplicationService(new TestUserRepository(user), new TestResourceRepository(equipment),
            new TestLoanRepository(), new TestUnitOfWork());

        await Assert.ThrowsAsync<ApplicationConflictException>(() =>
            service.CreateLoanAsync(new(user.Id, equipment.Id, DateTimeOffset.UtcNow.AddDays(5)), default));
        Assert.True(equipment.IsAvailable);
    }

    [Fact]
    public async Task Repeated_return_is_a_conflict()
    {
        var user = new LibraryUser(Guid.NewGuid(), "Ana Pérez", "ana@example.edu");
        var book = new Book(Guid.NewGuid(), "978-958-42-1234-5", "La vorágine", "José Eustasio Rivera");
        var service = new LibraryApplicationService(new TestUserRepository(user), new TestResourceRepository(book),
            new TestLoanRepository(), new TestUnitOfWork());
        var created = await service.CreateLoanAsync(new(user.Id, book.Id, DateTimeOffset.UtcNow.AddDays(5)), default);
        await service.ReturnLoanAsync(created.Id, default);

        await Assert.ThrowsAsync<ApplicationConflictException>(() => service.ReturnLoanAsync(created.Id, default));
        Assert.True(book.IsAvailable);
    }

    private sealed class TestUserRepository(LibraryUser user) : ILibraryUserRepository
    {
        private readonly List<LibraryUser> _users = [user];
        public Task AddAsync(LibraryUser item, CancellationToken token) { _users.Add(item); return Task.CompletedTask; }
        public Task<LibraryUser?> FindAsync(Guid id, CancellationToken token) => Task.FromResult(_users.SingleOrDefault(x => x.Id == id));
        public Task<bool> EmailExistsAsync(string email, CancellationToken token) => Task.FromResult(_users.Any(x => x.Email == email));
        public Task<IReadOnlyList<LibraryUser>> ListAsync(CancellationToken token) => Task.FromResult<IReadOnlyList<LibraryUser>>(_users);
    }

    private sealed class TestResourceRepository(LibraryResource resource) : ILibraryResourceRepository
    {
        private readonly List<LibraryResource> _items = [resource];
        public Task AddAsync(LibraryResource item, CancellationToken token) { _items.Add(item); return Task.CompletedTask; }
        public Task<LibraryResource?> FindAsync(Guid id, CancellationToken token) => Task.FromResult(_items.SingleOrDefault(x => x.Id == id));
        public Task<Book?> FindBookAsync(Guid id, CancellationToken token) => Task.FromResult(_items.OfType<Book>().SingleOrDefault(x => x.Id == id));
        public Task<Equipment?> FindEquipmentAsync(Guid id, CancellationToken token) => Task.FromResult(_items.OfType<Equipment>().SingleOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<Book>> ListBooksAsync(CancellationToken token) => Task.FromResult<IReadOnlyList<Book>>(_items.OfType<Book>().ToArray());
        public Task<IReadOnlyList<Equipment>> ListEquipmentAsync(CancellationToken token) => Task.FromResult<IReadOnlyList<Equipment>>(_items.OfType<Equipment>().ToArray());
    }

    private sealed class TestLoanRepository : ILoanRepository
    {
        private readonly List<Loan> _items = [];
        public Task AddAsync(Loan item, CancellationToken token) { _items.Add(item); return Task.CompletedTask; }
        public Task<Loan?> FindAsync(Guid id, CancellationToken token) => Task.FromResult(_items.SingleOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<Loan>> ListAsync(CancellationToken token) => Task.FromResult<IReadOnlyList<Loan>>(_items);
    }

    private sealed class TestUnitOfWork : IUnitOfWork
    {
        public Task<IApplicationTransaction> BeginTransactionAsync(CancellationToken token) =>
            Task.FromResult<IApplicationTransaction>(new TestTransaction());
        public Task SaveChangesAsync(CancellationToken token) => Task.CompletedTask;
    }

    private sealed class TestTransaction : IApplicationTransaction
    {
        public Task CommitAsync(CancellationToken token) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

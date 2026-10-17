using LibraryLab.Application.Contracts;
using LibraryLab.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryLab.Infrastructure.Persistence;

public sealed class LibraryUserRepository(LibraryLabDbContext context) : ILibraryUserRepository
{
    public Task AddAsync(LibraryUser user, CancellationToken cancellationToken) =>
        context.Users.AddAsync(user, cancellationToken).AsTask();

    public Task<LibraryUser?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        context.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        context.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken);

    public async Task<IReadOnlyList<LibraryUser>> ListAsync(CancellationToken cancellationToken) =>
        await context.Users.AsNoTracking().OrderBy(user => user.Name).ToArrayAsync(cancellationToken);
}

public sealed class LibraryResourceRepository(LibraryLabDbContext context) : ILibraryResourceRepository
{
    public Task AddAsync(LibraryResource resource, CancellationToken cancellationToken) =>
        context.Resources.AddAsync(resource, cancellationToken).AsTask();

    public Task<LibraryResource?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        context.Resources.SingleOrDefaultAsync(resource => resource.Id == id, cancellationToken);

    public Task<Book?> FindBookAsync(Guid id, CancellationToken cancellationToken) =>
        context.Books.SingleOrDefaultAsync(book => book.Id == id, cancellationToken);

    public Task<Equipment?> FindEquipmentAsync(Guid id, CancellationToken cancellationToken) =>
        context.Equipment.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Book>> ListBooksAsync(CancellationToken cancellationToken) =>
        await context.Books.AsNoTracking().OrderBy(book => book.Name).ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<Equipment>> ListEquipmentAsync(CancellationToken cancellationToken) =>
        await context.Equipment.AsNoTracking().OrderBy(item => item.Name).ToArrayAsync(cancellationToken);
}

public sealed class LoanRepository(LibraryLabDbContext context) : ILoanRepository
{
    public Task AddAsync(Loan loan, CancellationToken cancellationToken) =>
        context.Loans.AddAsync(loan, cancellationToken).AsTask();

    public Task<Loan?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        context.Loans.SingleOrDefaultAsync(loan => loan.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Loan>> ListAsync(CancellationToken cancellationToken) =>
        await context.Loans.AsNoTracking().OrderByDescending(loan => loan.BorrowedAt).ToArrayAsync(cancellationToken);
}

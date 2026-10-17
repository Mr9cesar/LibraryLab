using LibraryLab.Domain.Entities;

namespace LibraryLab.Application.Contracts;

public interface ILoanRepository
{
    Task AddAsync(Loan loan, CancellationToken cancellationToken);
    Task<Loan?> FindAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Loan>> ListAsync(CancellationToken cancellationToken);
}

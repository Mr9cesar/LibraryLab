using LibraryLab.Domain.Entities;

namespace LibraryLab.Application.Contracts;

public interface ILibraryUserRepository
{
    Task AddAsync(LibraryUser user, CancellationToken cancellationToken);
    Task<LibraryUser?> FindAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<IReadOnlyList<LibraryUser>> ListAsync(CancellationToken cancellationToken);
}

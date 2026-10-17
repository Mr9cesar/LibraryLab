using LibraryLab.Domain.Entities;

namespace LibraryLab.Application.Contracts;

public interface ILibraryResourceRepository
{
    Task AddAsync(LibraryResource resource, CancellationToken cancellationToken);
    Task<LibraryResource?> FindAsync(Guid id, CancellationToken cancellationToken);
    Task<Book?> FindBookAsync(Guid id, CancellationToken cancellationToken);
    Task<Equipment?> FindEquipmentAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Book>> ListBooksAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Equipment>> ListEquipmentAsync(CancellationToken cancellationToken);
}

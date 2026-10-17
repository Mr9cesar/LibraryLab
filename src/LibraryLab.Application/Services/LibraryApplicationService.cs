using LibraryLab.Application.Contracts;
using LibraryLab.Domain.Entities;
using LibraryLab.Domain.Enums;
using LibraryLab.Domain.Exceptions;

namespace LibraryLab.Application.Services;

public sealed class LibraryApplicationService(
    ILibraryUserRepository users,
    ILibraryResourceRepository resources,
    ILoanRepository loans,
    IUnitOfWork unitOfWork)
{
    public async Task<UserView> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        if (await users.EmailExistsAsync(normalizedEmail, cancellationToken))
            throw new ApplicationConflictException("Ya existe un usuario con ese correo.");

        var user = new LibraryUser(Guid.NewGuid(), command.Name, command.Email);
        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task<IReadOnlyList<UserView>> ListUsersAsync(CancellationToken cancellationToken) =>
        (await users.ListAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<BookView> CreateBookAsync(CreateBookCommand command, CancellationToken cancellationToken)
    {
        var book = new Book(Guid.NewGuid(), command.Isbn, command.Title, command.Author);
        await resources.AddAsync(book, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(book);
    }

    public async Task<IReadOnlyList<BookView>> ListBooksAsync(CancellationToken cancellationToken) =>
        (await resources.ListBooksAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<BookView> GetBookAsync(Guid id, CancellationToken cancellationToken) =>
        Map(await resources.FindBookAsync(id, cancellationToken)
            ?? throw new ApplicationNotFoundException("No existe el libro solicitado."));

    public async Task<EquipmentView> CreateEquipmentAsync(CreateEquipmentCommand command, CancellationToken cancellationToken)
    {
        var equipment = new Equipment(Guid.NewGuid(), command.InventoryCode, command.Name, command.Type);
        await resources.AddAsync(equipment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(equipment);
    }

    public async Task<IReadOnlyList<EquipmentView>> ListEquipmentAsync(CancellationToken cancellationToken) =>
        (await resources.ListEquipmentAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<EquipmentView> GetEquipmentAsync(Guid id, CancellationToken cancellationToken) =>
        Map(await resources.FindEquipmentAsync(id, cancellationToken)
            ?? throw new ApplicationNotFoundException("No existe el equipo solicitado."));

    public async Task<LoanView> CreateLoanAsync(CreateLoanCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var user = await users.FindAsync(command.UserId, cancellationToken)
            ?? throw new ApplicationNotFoundException("No existe el usuario solicitado.");
        if (user.Status != LibraryUserStatus.Active)
            throw new ApplicationConflictException("El usuario no está habilitado para préstamos.");

        var resource = await resources.FindAsync(command.ResourceId, cancellationToken)
            ?? throw new ApplicationNotFoundException("No existe el recurso solicitado.");
        if (resource.Status != ResourceStatus.Active || !resource.IsAvailable)
            throw new ApplicationConflictException("El recurso no está habilitado o no está disponible para préstamo.");
        var now = DateTimeOffset.UtcNow;
        if (command.DueAt <= now) throw new DomainRuleException("La fecha límite debe ser futura.");
        resource.Reserve();
        var loan = new Loan(Guid.NewGuid(), user.Id, resource.Id, resource.Kind, now, command.DueAt);
        await loans.AddAsync(loan, cancellationToken);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException exception)
        {
            throw new ApplicationConflictException("El recurso fue prestado por otra operación. Consulta su disponibilidad.", exception);
        }

        await transaction.CommitAsync(cancellationToken);
        return Map(loan);
    }

    public async Task<LoanView> ReturnLoanAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var loan = await loans.FindAsync(id, cancellationToken)
            ?? throw new ApplicationNotFoundException("No existe el préstamo solicitado.");
        if (loan.Status != LoanStatus.Active)
            throw new ApplicationConflictException("El préstamo ya fue devuelto.");
        var resource = await resources.FindAsync(loan.ResourceId, cancellationToken)
            ?? throw new ApplicationConflictException("El recurso del préstamo no existe.");

        loan.Return(DateTimeOffset.UtcNow);
        resource.Release();
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException exception)
        {
            throw new ApplicationConflictException("El préstamo o el recurso cambió en otra operación. Consulta su estado.", exception);
        }

        await transaction.CommitAsync(cancellationToken);
        return Map(loan);
    }

    public async Task<IReadOnlyList<LoanView>> ListLoansAsync(CancellationToken cancellationToken) =>
        (await loans.ListAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<LoanView> GetLoanAsync(Guid id, CancellationToken cancellationToken) =>
        Map(await loans.FindAsync(id, cancellationToken)
            ?? throw new ApplicationNotFoundException("No existe el préstamo solicitado."));

    private static UserView Map(LibraryUser user) => new(user.Id, user.Name, user.Email, user.Status);
    private static BookView Map(Book book) => new(book.Id, book.Isbn, book.Name, book.Author, book.Status, book.IsAvailable);
    private static EquipmentView Map(Equipment equipment) => new(equipment.Id, equipment.InventoryCode,
        equipment.Name, equipment.Type, equipment.Status, equipment.IsAvailable);
    private static LoanView Map(Loan loan) => new(loan.Id, loan.UserId, loan.ResourceId, loan.ResourceKind,
        loan.BorrowedAt, loan.DueAt, loan.ReturnedAt, loan.Status,
        loan.Status == LoanStatus.Active && loan.DueAt < DateTimeOffset.UtcNow);
}

using LibraryLab.Domain.Enums;
using LibraryLab.Domain.Exceptions;

namespace LibraryLab.Domain.Entities;

public sealed class Loan
{
    private Loan() { }

    public Loan(Guid id, Guid userId, Guid resourceId, ResourceKind resourceKind,
        DateTimeOffset borrowedAt, DateTimeOffset dueAt)
    {
        if (id == Guid.Empty || userId == Guid.Empty || resourceId == Guid.Empty)
            throw new DomainRuleException("Los identificadores del préstamo son obligatorios.");
        if (dueAt <= borrowedAt) throw new DomainRuleException("La fecha límite debe ser posterior al préstamo.");
        Id = id;
        UserId = userId;
        ResourceId = resourceId;
        ResourceKind = resourceKind;
        BorrowedAt = borrowedAt.ToUniversalTime();
        DueAt = dueAt.ToUniversalTime();
        Status = LoanStatus.Active;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ResourceId { get; private set; }
    public ResourceKind ResourceKind { get; private set; }
    public DateTimeOffset BorrowedAt { get; private set; }
    public DateTimeOffset DueAt { get; private set; }
    public DateTimeOffset? ReturnedAt { get; private set; }
    public LoanStatus Status { get; private set; }
    public LibraryUser User { get; private set; } = null!;
    public LibraryResource Resource { get; private set; } = null!;

    public void Return(DateTimeOffset returnedAt)
    {
        if (Status != LoanStatus.Active) throw new DomainRuleException("El préstamo ya fue devuelto.");
        if (returnedAt < BorrowedAt) throw new DomainRuleException("La devolución no puede ser anterior al préstamo.");
        ReturnedAt = returnedAt.ToUniversalTime();
        Status = LoanStatus.Returned;
    }
}

using LibraryLab.Domain.Enums;

namespace LibraryLab.Application.Services;

public sealed record CreateUserCommand(string Name, string Email);
public sealed record CreateBookCommand(string Isbn, string Title, string Author);
public sealed record CreateEquipmentCommand(string InventoryCode, string Name, EquipmentType Type);
public sealed record CreateLoanCommand(Guid UserId, Guid ResourceId, DateTimeOffset DueAt);

public sealed record UserView(Guid Id, string Name, string Email, LibraryUserStatus Status);
public sealed record BookView(Guid Id, string Isbn, string Title, string Author, ResourceStatus Status, bool IsAvailable);
public sealed record EquipmentView(Guid Id, string InventoryCode, string Name, EquipmentType Type, ResourceStatus Status, bool IsAvailable);
public sealed record LoanView(Guid Id, Guid UserId, Guid ResourceId, ResourceKind ResourceKind, DateTimeOffset BorrowedAt,
    DateTimeOffset DueAt, DateTimeOffset? ReturnedAt, LoanStatus Status, bool IsOverdue);

public sealed class ApplicationNotFoundException(string message) : Exception(message);
public sealed class ApplicationConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);

using LibraryLab.Domain.Enums;
using LibraryLab.Domain.Exceptions;

namespace LibraryLab.Domain.Entities;

public sealed class LibraryUser
{
    private LibraryUser() { }

    public LibraryUser(Guid id, string name, string email)
    {
        if (id == Guid.Empty) throw new DomainRuleException("El identificador del usuario es obligatorio.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleException("El nombre del usuario es obligatorio.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
            throw new DomainRuleException("El correo electrónico no es válido.");

        Id = id;
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Status = LibraryUserStatus.Active;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public LibraryUserStatus Status { get; private set; }

    public void Suspend() => Status = LibraryUserStatus.Suspended;
    public void Activate() => Status = LibraryUserStatus.Active;
}

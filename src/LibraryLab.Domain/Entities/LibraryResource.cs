using LibraryLab.Domain.Enums;
using LibraryLab.Domain.Exceptions;

namespace LibraryLab.Domain.Entities;

public abstract class LibraryResource
{
    protected LibraryResource() { }

    protected LibraryResource(Guid id, string name, ResourceKind kind)
    {
        if (id == Guid.Empty) throw new DomainRuleException("El identificador del recurso es obligatorio.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainRuleException("El nombre del recurso es obligatorio.");

        Id = id;
        Name = name.Trim();
        Kind = kind;
        Status = ResourceStatus.Active;
        IsAvailable = true;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ResourceStatus Status { get; private set; }
    public bool IsAvailable { get; private set; }
    public long AvailabilityVersion { get; private set; }
    public ResourceKind Kind { get; private set; }

    public void Reserve()
    {
        if (Status != ResourceStatus.Active) throw new DomainRuleException("El recurso no está activo.");
        if (!IsAvailable) throw new DomainRuleException("El recurso no está disponible.");
        IsAvailable = false;
        AvailabilityVersion++;
    }

    public void Release()
    {
        if (IsAvailable) throw new DomainRuleException("El recurso ya está disponible.");
        IsAvailable = true;
        AvailabilityVersion++;
    }

    public void SetStatus(ResourceStatus status)
    {
        if (!Enum.IsDefined(status)) throw new DomainRuleException("El estado del recurso no es válido.");
        if (status != ResourceStatus.Active && !IsAvailable)
            throw new DomainRuleException("No se puede cambiar el estado de un recurso prestado.");
        Status = status;
    }
}

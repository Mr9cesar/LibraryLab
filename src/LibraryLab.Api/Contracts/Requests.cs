using System.ComponentModel.DataAnnotations;
using LibraryLab.Domain.Enums;

namespace LibraryLab.Api.Contracts;

public sealed class CreateUserRequest
{
    [Required, StringLength(160, MinimumLength = 2)] public string Name { get; init; } = string.Empty;
    [Required, EmailAddress, StringLength(254)] public string Email { get; init; } = string.Empty;
}

public sealed class CreateBookRequest
{
    [Required, StringLength(20)] public string Isbn { get; init; } = string.Empty;
    [Required, StringLength(200, MinimumLength = 1)] public string Title { get; init; } = string.Empty;
    [Required, StringLength(160, MinimumLength = 1)] public string Author { get; init; } = string.Empty;
}

public sealed class CreateEquipmentRequest
{
    [Required, StringLength(64)] public string InventoryCode { get; init; } = string.Empty;
    [Required, StringLength(200, MinimumLength = 1)] public string Name { get; init; } = string.Empty;
    [EnumDataType(typeof(EquipmentType))] public EquipmentType Type { get; init; }
}

public sealed class CreateLoanRequest : IValidatableObject
{
    public Guid UserId { get; init; }
    public Guid ResourceId { get; init; }
    public DateTimeOffset DueAt { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UserId == Guid.Empty)
            yield return new ValidationResult("El identificador del usuario es obligatorio.", [nameof(UserId)]);
        if (ResourceId == Guid.Empty)
            yield return new ValidationResult("El identificador del recurso es obligatorio.", [nameof(ResourceId)]);
        if (DueAt <= DateTimeOffset.UtcNow)
            yield return new ValidationResult("La fecha límite debe ser futura.", [nameof(DueAt)]);
    }
}

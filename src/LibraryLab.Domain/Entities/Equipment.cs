using LibraryLab.Domain.Enums;
using LibraryLab.Domain.Exceptions;

namespace LibraryLab.Domain.Entities;

public sealed class Equipment : LibraryResource
{
    private Equipment() { }

    public Equipment(Guid id, string inventoryCode, string name, EquipmentType type) : base(id, name, ResourceKind.Equipment)
    {
        if (string.IsNullOrWhiteSpace(inventoryCode))
            throw new DomainRuleException("El código de inventario es obligatorio.");
        if (!Enum.IsDefined(type)) throw new DomainRuleException("El tipo de equipo no es válido.");
        InventoryCode = inventoryCode.Trim();
        Type = type;
    }

    public string InventoryCode { get; private set; } = string.Empty;
    public EquipmentType Type { get; private set; }
}

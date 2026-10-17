using LibraryLab.Domain.Entities;
using LibraryLab.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibraryLab.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(LibraryLabDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);
        if (!await context.Users.AnyAsync(cancellationToken))
        {
            context.Users.Add(new LibraryUser(Guid.Parse("9e781be8-4f04-4de2-9d2f-ded867859e0a"),
                "Usuario de ejemplo", "demo@librarylab.local"));
        }
        if (!await context.Books.AnyAsync(cancellationToken))
        {
            context.Books.Add(new Book(Guid.Parse("1c9b29dd-011e-4edf-82b4-7de3557e3915"),
                "978-958-42-0000-1", "Cien años de soledad", "Gabriel García Márquez"));
        }
        if (!await context.Equipment.AnyAsync(cancellationToken))
        {
            context.Equipment.Add(new Equipment(Guid.Parse("a7e249c2-68b5-43c2-a4d0-884017f5bca3"),
                "EQ-DEMO-001", "Portátil de demostración", EquipmentType.Laptop));
        }
        await context.SaveChangesAsync(cancellationToken);
    }
}

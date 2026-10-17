using LibraryLab.Infrastructure.Persistence;
using LibraryLab.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryLab.Infrastructure.Tests;

public sealed class LibraryLabModelTests
{
    [Fact]
    public void MySql_model_maps_library_tables_and_resource_relationships()
    {
        var options = new DbContextOptionsBuilder<LibraryLabDbContext>()
            .UseMySql("Server=localhost;Database=library_lab;User=library_lab;Password=placeholder;",
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        using var context = new LibraryLabDbContext(options);
        var model = context.Model;

        Assert.NotNull(model.FindEntityType("LibraryLab.Domain.Entities.LibraryUser")?.GetTableName());
        Assert.Equal("Users", model.FindEntityType(typeof(LibraryLab.Domain.Entities.LibraryUser))?.GetTableName());
        Assert.Equal("Resources", model.FindEntityType(typeof(LibraryLab.Domain.Entities.LibraryResource))?.GetTableName());
        Assert.Equal("Books", model.FindEntityType(typeof(LibraryLab.Domain.Entities.Book))?.GetTableName());
        Assert.Equal("Equipment", model.FindEntityType(typeof(LibraryLab.Domain.Entities.Equipment))?.GetTableName());
        Assert.Equal("Loans", model.FindEntityType(typeof(LibraryLab.Domain.Entities.Loan))?.GetTableName());
    }

    [Fact]
    public void Loan_foreign_key_enforces_the_resource_kind()
    {
        var options = new DbContextOptionsBuilder<LibraryLabDbContext>()
            .UseMySql("Server=localhost;Database=library_lab;User=library_lab;Password=placeholder;",
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        using var context = new LibraryLabDbContext(options);
        var loan = context.Model.FindEntityType(typeof(Loan))!;
        var resourceForeignKey = Assert.Single(loan.GetForeignKeys()
            .Where(key => key.PrincipalEntityType.ClrType == typeof(LibraryResource)));

        Assert.Equal(new[] { "ResourceId", "ResourceKind" }, resourceForeignKey.Properties.Select(property => property.Name));
        Assert.Equal(new[] { "Id", "Kind" }, resourceForeignKey.PrincipalKey.Properties.Select(property => property.Name));
    }
}

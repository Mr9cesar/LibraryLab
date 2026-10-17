using LibraryLab.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryLab.Infrastructure.Persistence;

public sealed class LibraryLabDbContext(DbContextOptions<LibraryLabDbContext> options) : DbContext(options)
{
    public DbSet<LibraryUser> Users => Set<LibraryUser>();
    public DbSet<LibraryResource> Resources => Set<LibraryResource>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LibraryUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Id).HasColumnType("char(36)");
            entity.Property(user => user.Name).HasMaxLength(160).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(254).IsRequired();
            entity.Property(user => user.Status).HasConversion<int>();
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<LibraryResource>(entity =>
        {
            entity.UseTptMappingStrategy();
            entity.ToTable("Resources");
            entity.HasKey(resource => resource.Id);
            entity.Property(resource => resource.Id).HasColumnType("char(36)");
            entity.Property(resource => resource.Name).HasMaxLength(200).IsRequired();
            entity.Property(resource => resource.Status).HasConversion<int>();
            entity.Property(resource => resource.Kind).HasConversion<int>();
            entity.Property(resource => resource.AvailabilityVersion).IsConcurrencyToken();
            entity.HasAlternateKey(resource => new { resource.Id, resource.Kind });
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("Books");
            entity.Property(book => book.Isbn).HasMaxLength(20).IsRequired();
            entity.Property(book => book.Author).HasMaxLength(160).IsRequired();
            entity.HasIndex(book => book.Isbn).IsUnique();
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.ToTable("Equipment");
            entity.Property(item => item.InventoryCode).HasMaxLength(64).IsRequired();
            entity.Property(item => item.Type).HasConversion<int>();
            entity.HasIndex(item => item.InventoryCode).IsUnique();
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("Loans", table =>
            {
                table.HasCheckConstraint("CK_Loans_ReturnState",
                    "((Status = 1 AND ReturnedAt IS NULL) OR (Status = 2 AND ReturnedAt IS NOT NULL))");
                table.HasCheckConstraint("CK_Loans_ResourceKind", "ResourceKind IN (1, 2)");
                table.HasCheckConstraint("CK_Loans_DueAfterBorrowed", "DueAt > BorrowedAt");
            });
            entity.HasKey(loan => loan.Id);
            entity.Property(loan => loan.Id).HasColumnType("char(36)");
            entity.Property(loan => loan.UserId).HasColumnType("char(36)");
            entity.Property(loan => loan.ResourceId).HasColumnType("char(36)");
            entity.Property(loan => loan.Status).HasConversion<int>();
            entity.Property(loan => loan.ResourceKind).HasConversion<int>();
            entity.Property(loan => loan.BorrowedAt).HasPrecision(6);
            entity.Property(loan => loan.DueAt).HasPrecision(6);
            entity.Property(loan => loan.ReturnedAt).HasPrecision(6);
            entity.HasOne(loan => loan.User).WithMany().HasForeignKey(loan => loan.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(loan => loan.Resource).WithMany()
                .HasForeignKey(loan => new { loan.ResourceId, loan.ResourceKind })
                .HasPrincipalKey(resource => new { resource.Id, resource.Kind })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(loan => new { loan.ResourceId, loan.Status });
            entity.HasIndex(loan => new { loan.UserId, loan.Status });
        });
    }
}

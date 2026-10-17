using LibraryLab.Domain.Entities;
using LibraryLab.Domain.Enums;
using LibraryLab.Domain.Exceptions;

namespace LibraryLab.Domain.Tests;

public sealed class LibraryRulesTests
{
    [Fact]
    public void New_book_is_available_and_active()
    {
        var book = new Book(Guid.NewGuid(), "978-958-42-1234-5", "La vorágine", "José Eustasio Rivera");

        Assert.True(book.IsAvailable);
        Assert.Equal(ResourceStatus.Active, book.Status);
    }

    [Fact]
    public void Resource_can_be_reserved_and_released()
    {
        var item = new Equipment(Guid.NewGuid(), "EQ-101", "Proyector", EquipmentType.Projector);

        item.Reserve();
        Assert.False(item.IsAvailable);
        item.Release();
        Assert.True(item.IsAvailable);
    }

    [Fact]
    public void Resource_cannot_be_reserved_twice()
    {
        var book = new Book(Guid.NewGuid(), "978-958-42-1234-5", "La vorágine", "José Eustasio Rivera");
        book.Reserve();

        Assert.Throws<DomainRuleException>(book.Reserve);
    }

    [Fact]
    public void Inactive_resource_cannot_be_reserved()
    {
        var item = new Equipment(Guid.NewGuid(), "EQ-101", "Proyector", EquipmentType.Projector);
        item.SetStatus(ResourceStatus.Maintenance);

        Assert.Throws<DomainRuleException>(item.Reserve);
    }

    [Fact]
    public void Loan_requires_due_date_after_borrow_date()
    {
        var borrowedAt = DateTimeOffset.Parse("2026-10-02T10:00:00Z");

        Assert.Throws<DomainRuleException>(() => new Loan(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ResourceKind.Book, borrowedAt, borrowedAt));
    }

    [Fact]
    public void Returned_loan_cannot_be_returned_twice()
    {
        var borrowedAt = DateTimeOffset.Parse("2026-10-02T10:00:00Z");
        var loan = new Loan(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ResourceKind.Equipment,
            borrowedAt, borrowedAt.AddDays(3));

        loan.Return(borrowedAt.AddDays(1));

        Assert.Equal(LoanStatus.Returned, loan.Status);
        Assert.Throws<DomainRuleException>(() => loan.Return(borrowedAt.AddDays(2)));
    }

    [Fact]
    public void User_email_is_normalized()
    {
        var user = new LibraryUser(Guid.NewGuid(), "  Ana Pérez  ", " ANA@example.edu ");

        Assert.Equal("Ana Pérez", user.Name);
        Assert.Equal("ana@example.edu", user.Email);
        Assert.Equal(LibraryUserStatus.Active, user.Status);
    }
}

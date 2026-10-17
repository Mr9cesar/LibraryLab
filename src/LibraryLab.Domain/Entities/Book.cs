using LibraryLab.Domain.Exceptions;

namespace LibraryLab.Domain.Entities;

public sealed class Book : LibraryResource
{
    private Book() { }

    public Book(Guid id, string isbn, string title, string author) : base(id, title, LibraryLab.Domain.Enums.ResourceKind.Book)
    {
        if (string.IsNullOrWhiteSpace(isbn)) throw new DomainRuleException("El ISBN es obligatorio.");
        if (string.IsNullOrWhiteSpace(author)) throw new DomainRuleException("El autor es obligatorio.");
        Isbn = isbn.Trim();
        Author = author.Trim();
    }

    public string Isbn { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
}

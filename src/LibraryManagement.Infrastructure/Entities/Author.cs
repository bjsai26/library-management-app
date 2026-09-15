namespace LibraryManagement.Infrastructure.Entities;

/// <summary>A person who wrote one or more books in the catalogue.</summary>
public class Author : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string? Biography { get; set; }

    public string? Country { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}

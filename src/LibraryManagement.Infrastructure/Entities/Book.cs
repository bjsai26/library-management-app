namespace LibraryManagement.Infrastructure.Entities;

/// <summary>A title in the catalogue, linked to one author and one category.</summary>
public class Book : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Isbn { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Publisher { get; set; }

    public int PublishedYear { get; set; }

    public decimal Price { get; set; }

    public int CopiesAvailable { get; set; }

    public int AuthorId { get; set; }

    public Author Author { get; set; } = null!;

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;
}

namespace LibraryManagement.AppServices.DTOs.Book;

public class BookDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Isbn { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Publisher { get; set; }

    public int PublishedYear { get; set; }

    public decimal Price { get; set; }

    public int CopiesAvailable { get; set; }

    public int AuthorId { get; set; }

    public string AuthorName { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

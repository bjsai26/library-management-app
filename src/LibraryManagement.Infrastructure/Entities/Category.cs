namespace LibraryManagement.Infrastructure.Entities;

/// <summary>A shelving classification such as Fiction or History.</summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}

namespace LibraryManagement.AppServices.DTOs.Category;

public class CategoryDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int BookCount { get; set; }
}

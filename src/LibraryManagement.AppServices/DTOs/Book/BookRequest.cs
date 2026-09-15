using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.AppServices.DTOs.Book;

/// <summary>
/// Body for both POST and PUT, since the fields are identical. Implements
/// IValidatableObject for the one rule attributes alone cannot express.
/// </summary>
public class BookRequest : IValidatableObject
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(250, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 250 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "ISBN is required.")]
    [RegularExpression(@"^\d{10}$|^\d{13}$", ErrorMessage = "ISBN must be exactly 10 or 13 digits.")]
    public string Isbn { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    [StringLength(150, ErrorMessage = "Publisher cannot exceed 150 characters.")]
    public string? Publisher { get; set; }

    [Range(1450, 2100, ErrorMessage = "Published year must be between 1450 and 2100.")]
    public int PublishedYear { get; set; }

    [Range(0.0, 100000.0, ErrorMessage = "Price must be between 0 and 100000.")]
    public decimal Price { get; set; }

    [Range(0, 10000, ErrorMessage = "Copies available must be between 0 and 10000.")]
    public int CopiesAvailable { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A valid author must be selected.")]
    public int AuthorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A valid category must be selected.")]
    public int CategoryId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PublishedYear > DateTime.UtcNow.Year)
        {
            yield return new ValidationResult(
                "Published year cannot be in the future.",
                new[] { nameof(PublishedYear) });
        }
    }
}

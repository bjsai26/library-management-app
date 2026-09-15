using LibraryManagement.AppServices.DTOs.Book;
using LibraryManagement.AppServices.DTOs.Category;
using LibraryManagement.Infrastructure.Entities;

namespace LibraryManagement.AppServices.Mappings;

/// <summary>
/// Hand-written entity/DTO translation, kept explicit so the project takes no dependency
/// on a mapping library and every conversion is visible.
/// </summary>
public static class MappingExtensions
{
    public static CategoryDto ToDto(this Category entity, int bookCount = 0) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        BookCount = bookCount
    };

    /// <summary>Copies the request onto the entity, trimmed. Id and timestamps are left alone.</summary>
    public static void ApplyTo(this CategoryRequest request, Category entity)
    {
        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
    }

    public static BookDto ToDto(this Book entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Isbn = entity.Isbn,
        Description = entity.Description,
        Publisher = entity.Publisher,
        PublishedYear = entity.PublishedYear,
        Price = entity.Price,
        CopiesAvailable = entity.CopiesAvailable,
        AuthorId = entity.AuthorId,
        AuthorName = entity.Author?.FullName ?? string.Empty,
        CategoryId = entity.CategoryId,
        CategoryName = entity.Category?.Name ?? string.Empty,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    /// <summary>Copies the request onto the entity, trimmed. Id and timestamps are left alone.</summary>
    public static void ApplyTo(this BookRequest request, Book entity)
    {
        entity.Title = request.Title.Trim();
        entity.Isbn = request.Isbn.Trim();
        entity.Description = request.Description?.Trim();
        entity.Publisher = request.Publisher?.Trim();
        entity.PublishedYear = request.PublishedYear;
        entity.Price = request.Price;
        entity.CopiesAvailable = request.CopiesAvailable;
        entity.AuthorId = request.AuthorId;
        entity.CategoryId = request.CategoryId;
    }
}

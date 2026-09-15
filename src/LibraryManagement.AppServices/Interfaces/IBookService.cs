using LibraryManagement.AppServices.Common;
using LibraryManagement.AppServices.DTOs.Book;
using LibraryManagement.AppServices.DTOs.Common;

namespace LibraryManagement.AppServices.Interfaces;

/// <summary>Read and write access to the book catalogue.</summary>
public interface IBookService
{
    /// <summary>One page of books ordered by title. Search matches title or ISBN.</summary>
    Task<ServiceResult<PagedResult<BookDto>>> GetAllAsync(QueryParameters query, CancellationToken cancellationToken = default);

    /// <summary>
    /// A single book with its author and category names resolved. NotFound if the id is unknown.
    /// </summary>
    Task<ServiceResult<BookDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a book. Fails validation if the author or category does not exist, and conflicts
    /// if the ISBN is already in use.
    /// </summary>
    Task<ServiceResult<BookDto>> CreateAsync(BookRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Overwrites every field of an existing book. NotFound for an unknown id; conflicts if
    /// another book already holds the ISBN.
    /// </summary>
    Task<ServiceResult<BookDto>> UpdateAsync(int id, BookRequest request, CancellationToken cancellationToken = default);

    /// <summary>Permanently removes a book. NotFound if the id is unknown.</summary>
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

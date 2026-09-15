using LibraryManagement.AppServices.Common;
using LibraryManagement.AppServices.DTOs.Book;
using LibraryManagement.AppServices.DTOs.Common;
using LibraryManagement.AppServices.Interfaces;
using LibraryManagement.AppServices.Mappings;
using LibraryManagement.Infrastructure.Entities;
using LibraryManagement.Infrastructure.Repositories;

namespace LibraryManagement.AppServices.Services;

/// <inheritdoc cref="IBookService" />
public class BookService : IBookService
{
    private readonly IGenericRepository<Book> _books;
    private readonly IGenericRepository<Author> _authors;
    private readonly IGenericRepository<Category> _categories;

    public BookService(
        IGenericRepository<Book> books,
        IGenericRepository<Author> authors,
        IGenericRepository<Category> categories)
    {
        _books = books;
        _authors = authors;
        _categories = categories;
    }

    public async Task<ServiceResult<PagedResult<BookDto>>> GetAllAsync(
        QueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var search = query.Search?.Trim();

        var (books, totalCount) = await _books.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            filter: string.IsNullOrEmpty(search)
                ? null
                : b => b.Title.Contains(search) || b.Isbn.Contains(search),
            orderBy: b => b.Title,
            // Eager load so the response can show author and category names.
            includes: [b => b.Author, b => b.Category],
            cancellationToken: cancellationToken);

        var items = books.Select(b => b.ToDto()).ToList();

        return ServiceResult<PagedResult<BookDto>>.Success(
            new PagedResult<BookDto>(items, totalCount, query.PageNumber, query.PageSize));
    }

    public async Task<ServiceResult<BookDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _books.GetByIdAsync(id, [b => b.Author, b => b.Category], cancellationToken);

        return book is null
            ? ServiceResult<BookDto>.NotFound($"No book was found with id {id}.")
            : ServiceResult<BookDto>.Success(book.ToDto());
    }

    public async Task<ServiceResult<BookDto>> CreateAsync(
        BookRequest request,
        CancellationToken cancellationToken = default)
    {
        var failure = await ValidateAsync(request, null, cancellationToken);

        if (failure is not null)
        {
            return failure;
        }

        var book = new Book();
        request.ApplyTo(book);

        await _books.AddAsync(book, cancellationToken);
        await _books.SaveChangesAsync(cancellationToken);

        // Reloaded so the response carries the author and category names.
        var created = await _books.GetByIdAsync(book.Id, [b => b.Author, b => b.Category], cancellationToken);

        return ServiceResult<BookDto>.Success(created!.ToDto(), "Book created successfully.");
    }

    public async Task<ServiceResult<BookDto>> UpdateAsync(
        int id,
        BookRequest request,
        CancellationToken cancellationToken = default)
    {
        var book = await _books.GetByIdAsync(id, cancellationToken: cancellationToken);

        if (book is null)
        {
            return ServiceResult<BookDto>.NotFound($"No book was found with id {id}.");
        }

        var failure = await ValidateAsync(request, id, cancellationToken);

        if (failure is not null)
        {
            return failure;
        }

        request.ApplyTo(book);

        _books.Update(book);
        await _books.SaveChangesAsync(cancellationToken);

        var updated = await _books.GetByIdAsync(id, [b => b.Author, b => b.Category], cancellationToken);

        return ServiceResult<BookDto>.Success(updated!.ToDto(), "Book updated successfully.");
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _books.GetByIdAsync(id, cancellationToken: cancellationToken);

        if (book is null)
        {
            return ServiceResult.NotFound($"No book was found with id {id}.");
        }

        _books.Remove(book);
        await _books.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success("Book deleted successfully.");
    }

    /// <summary>
    /// Checks the rules that need the database: the referenced author and category must
    /// exist, and the ISBN must be unique. Returns null when everything is valid.
    /// </summary>
    private async Task<ServiceResult<BookDto>?> ValidateAsync(
        BookRequest request,
        int? excludeBookId,
        CancellationToken cancellationToken)
    {
        if (!await _authors.ExistsAsync(a => a.Id == request.AuthorId, cancellationToken))
        {
            return ServiceResult<BookDto>.Fail($"No author was found with id {request.AuthorId}.");
        }

        if (!await _categories.ExistsAsync(c => c.Id == request.CategoryId, cancellationToken))
        {
            return ServiceResult<BookDto>.Fail($"No category was found with id {request.CategoryId}.");
        }

        var isbn = request.Isbn.Trim();

        if (await _books.ExistsAsync(b => b.Isbn == isbn && (excludeBookId == null || b.Id != excludeBookId), cancellationToken))
        {
            return ServiceResult<BookDto>.Conflict($"A book with ISBN {isbn} already exists.");
        }

        return null;
    }
}

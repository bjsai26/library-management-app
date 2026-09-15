using LibraryManagement.AppServices.Common;
using LibraryManagement.AppServices.DTOs.Book;
using LibraryManagement.AppServices.DTOs.Common;

namespace LibraryManagement.AppServices.Interfaces;

public interface IBookService
{
    Task<ServiceResult<PagedResult<BookDto>>> GetAllAsync(QueryParameters query, CancellationToken cancellationToken = default);

    Task<ServiceResult<BookDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ServiceResult<BookDto>> CreateAsync(BookRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<BookDto>> UpdateAsync(int id, BookRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

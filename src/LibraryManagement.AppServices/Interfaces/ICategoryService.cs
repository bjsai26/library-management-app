using LibraryManagement.AppServices.Common;
using LibraryManagement.AppServices.DTOs.Category;
using LibraryManagement.AppServices.DTOs.Common;

namespace LibraryManagement.AppServices.Interfaces;

public interface ICategoryService
{
    Task<ServiceResult<PagedResult<CategoryDto>>> GetAllAsync(QueryParameters query, CancellationToken cancellationToken = default);

    Task<ServiceResult<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ServiceResult<CategoryDto>> CreateAsync(CategoryRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<CategoryDto>> UpdateAsync(int id, CategoryRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

using LibraryManagement.AppServices.Common;
using LibraryManagement.AppServices.DTOs.Category;
using LibraryManagement.AppServices.DTOs.Common;

namespace LibraryManagement.AppServices.Interfaces;

/// <summary>Read and write access to the categories that books are filed under.</summary>
public interface ICategoryService
{
    /// <summary>
    /// One page of categories ordered by name, each carrying its book count. Search matches the name.
    /// </summary>
    Task<ServiceResult<PagedResult<CategoryDto>>> GetAllAsync(QueryParameters query, CancellationToken cancellationToken = default);

    /// <summary>A single category with its book count. NotFound if the id is unknown.</summary>
    Task<ServiceResult<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Adds a category. Conflicts if the name is already taken.</summary>
    Task<ServiceResult<CategoryDto>> CreateAsync(CategoryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames or re-describes a category. NotFound for an unknown id; conflicts if another
    /// category already uses the name.
    /// </summary>
    Task<ServiceResult<CategoryDto>> UpdateAsync(int id, CategoryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a category, but only once it is empty — conflicts while any book still references it.
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

using LibraryManagement.AppServices.Common;
using LibraryManagement.AppServices.DTOs.Category;
using LibraryManagement.AppServices.DTOs.Common;
using LibraryManagement.AppServices.Interfaces;
using LibraryManagement.AppServices.Mappings;
using LibraryManagement.Infrastructure.Entities;
using LibraryManagement.Infrastructure.Repositories;

namespace LibraryManagement.AppServices.Services;

/// <inheritdoc cref="ICategoryService" />
public class CategoryService : ICategoryService
{
    private readonly IGenericRepository<Category> _categories;
    private readonly IGenericRepository<Book> _books;

    public CategoryService(IGenericRepository<Category> categories, IGenericRepository<Book> books)
    {
        _categories = categories;
        _books = books;
    }

    public async Task<ServiceResult<PagedResult<CategoryDto>>> GetAllAsync(
        QueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var search = query.Search?.Trim();

        var (categories, totalCount) = await _categories.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            filter: string.IsNullOrEmpty(search) ? null : c => c.Name.Contains(search),
            orderBy: c => c.Name,
            cancellationToken: cancellationToken);

        // One grouped count for the whole page, rather than a query per category.
        var categoryIds = categories.Select(c => c.Id).ToList();

        var bookCounts = categoryIds.Count == 0
            ? new Dictionary<int, int>()
            : await _books.CountByAsync(
                b => b.CategoryId,
                b => categoryIds.Contains(b.CategoryId),
                cancellationToken);

        var items = categories
            .Select(c => c.ToDto(bookCounts.GetValueOrDefault(c.Id)))
            .ToList();

        return ServiceResult<PagedResult<CategoryDto>>.Success(
            new PagedResult<CategoryDto>(items, totalCount, query.PageNumber, query.PageSize));
    }

    public async Task<ServiceResult<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(id, cancellationToken: cancellationToken);

        if (category is null)
        {
            return ServiceResult<CategoryDto>.NotFound($"No category was found with id {id}.");
        }

        var bookCount = await _books.CountAsync(b => b.CategoryId == id, cancellationToken);

        return ServiceResult<CategoryDto>.Success(category.ToDto(bookCount));
    }

    public async Task<ServiceResult<CategoryDto>> CreateAsync(
        CategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (await _categories.ExistsAsync(c => c.Name == name, cancellationToken))
        {
            return ServiceResult<CategoryDto>.Conflict($"A category named {name} already exists.");
        }

        var category = new Category();
        request.ApplyTo(category);

        await _categories.AddAsync(category, cancellationToken);
        await _categories.SaveChangesAsync(cancellationToken);

        return ServiceResult<CategoryDto>.Success(category.ToDto(), "Category created successfully.");
    }

    public async Task<ServiceResult<CategoryDto>> UpdateAsync(
        int id,
        CategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(id, cancellationToken: cancellationToken);

        if (category is null)
        {
            return ServiceResult<CategoryDto>.NotFound($"No category was found with id {id}.");
        }

        var name = request.Name.Trim();

        if (await _categories.ExistsAsync(c => c.Name == name && c.Id != id, cancellationToken))
        {
            return ServiceResult<CategoryDto>.Conflict($"Another category named {name} already exists.");
        }

        request.ApplyTo(category);

        _categories.Update(category);
        await _categories.SaveChangesAsync(cancellationToken);

        var bookCount = await _books.CountAsync(b => b.CategoryId == id, cancellationToken);

        return ServiceResult<CategoryDto>.Success(category.ToDto(bookCount), "Category updated successfully.");
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(id, cancellationToken: cancellationToken);

        if (category is null)
        {
            return ServiceResult.NotFound($"No category was found with id {id}.");
        }

        // Blocked rather than cascaded, so deleting a category never silently removes books.
        var bookCount = await _books.CountAsync(b => b.CategoryId == id, cancellationToken);

        if (bookCount > 0)
        {
            return ServiceResult.Conflict(
                $"This category cannot be deleted while {bookCount} book(s) belong to it.");
        }

        _categories.Remove(category);
        await _categories.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success("Category deleted successfully.");
    }
}

using System.Linq.Expressions;
using LibraryManagement.Infrastructure.Entities;

namespace LibraryManagement.Infrastructure.Repositories;

/// <summary>
/// The only data-access contract in the project. Services depend on this, which keeps
/// EF Core out of the application layer entirely.
/// </summary>
public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// One page of rows plus the total count, with optional filtering, ordering and
    /// eager loading of navigation properties.
    /// </summary>
    Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        Expression<Func<TEntity, object>>[]? includes = null,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(
        int id,
        Expression<Func<TEntity, object>>[]? includes = null,
        CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts rows grouped by <paramref name="keySelector" /> in a single GROUP BY query.
    /// Use this instead of calling <see cref="CountAsync" /> once per key.
    /// </summary>
    Task<Dictionary<TKey, int>> CountByAsync<TKey>(
        Expression<Func<TEntity, TKey>> keySelector,
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default) where TKey : notnull;

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    /// <summary>Commits everything tracked on the shared context.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

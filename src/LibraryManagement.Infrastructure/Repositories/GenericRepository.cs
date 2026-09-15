using System.Linq.Expressions;
using LibraryManagement.Infrastructure.Data;
using LibraryManagement.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

/// <inheritdoc cref="IGenericRepository{TEntity}" />
public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    private readonly AppDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        Expression<Func<TEntity, object>>[]? includes = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (includes is not null)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        // Counted before paging so the caller can work out the real page count.
        var totalCount = await query.CountAsync(cancellationToken);

        query = orderBy is null ? query.OrderBy(e => e.Id) : query.OrderBy(orderBy);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<TEntity?> GetByIdAsync(
        int id,
        Expression<Func<TEntity, object>>[]? includes = null,
        CancellationToken cancellationToken = default)
    {
        // Tracked, unlike GetPagedAsync: this is what the write paths load before saving.
        IQueryable<TEntity> query = _dbSet;

        if (includes is not null)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(filter, cancellationToken);

    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(filter, cancellationToken);

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
        => filter is null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(filter, cancellationToken);

    public async Task<Dictionary<TKey, int>> CountByAsync<TKey>(
        Expression<Func<TEntity, TKey>> keySelector,
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default) where TKey : notnull
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        // Grouped server-side, so one round trip covers every key.
        return await query
            .GroupBy(keySelector)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.Key, row => row.Count, cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => _dbSet.Update(entity);

    public void Remove(TEntity entity) => _dbSet.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}

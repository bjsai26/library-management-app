using LibraryManagement.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Data;

/// <summary>
/// EF Core context for the library database. Entity rules are configured inline below,
/// which keeps the whole schema readable in one place for a project this size.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(150);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(u => u.PasswordSalt).IsRequired().HasMaxLength(256);
            entity.Property(u => u.Role).HasConversion<int>();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.Property(a => a.FullName).IsRequired().HasMaxLength(150);
            entity.Property(a => a.Biography).HasMaxLength(2000);
            entity.Property(a => a.Country).HasMaxLength(100);
            entity.HasIndex(a => a.FullName);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(b => b.Title).IsRequired().HasMaxLength(250);
            entity.Property(b => b.Isbn).IsRequired().HasMaxLength(20);
            entity.Property(b => b.Description).HasMaxLength(2000);
            entity.Property(b => b.Publisher).HasMaxLength(150);
            entity.Property(b => b.Price).HasPrecision(18, 2);
            entity.HasIndex(b => b.Isbn).IsUnique();

            // Restrict, so an author or category with books cannot be deleted out from under them.
            entity.HasOne(b => b.Author)
                  .WithMany(a => a.Books)
                  .HasForeignKey(b => b.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Category)
                  .WithMany(c => c.Books)
                  .HasForeignKey(b => b.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        DataSeeder.Seed(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}

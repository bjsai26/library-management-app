using LibraryManagement.Infrastructure.Entities;
using LibraryManagement.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Data;

/// <summary>
/// Baseline data written by the initial migration, so the API is usable on first run.
/// </summary>
/// <remarks>
/// The password values are PBKDF2-SHA256 (100,000 iterations, 32-byte key) and match the
/// algorithm in PasswordHasher. Plain-text credentials are in README.md and are meant for
/// local evaluation only.
/// </remarks>
internal static class DataSeeder
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "System Administrator",
                Email = "admin@library.com",
                // Admin@123
                PasswordSalt = "Kx1acPCE4rG322kxCZ3YAw==",
                PasswordHash = "B+iU2v7qXvG7m37DA/icJY8RZCocE+Vy7r8WSQRLEvM=",
                Role = UserRole.Admin,
                CreatedAt = SeedDate
            },
            new User
            {
                Id = 2,
                FullName = "Sample Member",
                Email = "member@library.com",
                // Member@123
                PasswordSalt = "RXN4ofVaxQtlYCvaJJomCQ==",
                PasswordHash = "MrEG925IcMJD2Qbry1j2G7nHFGWP5/QjL3CR0S2lgls=",
                Role = UserRole.Member,
                CreatedAt = SeedDate
            });

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Fiction", Description = "Novels and short stories", CreatedAt = SeedDate },
            new Category { Id = 2, Name = "Science", Description = "Scientific literature and textbooks", CreatedAt = SeedDate },
            new Category { Id = 3, Name = "Technology", Description = "Software, hardware and engineering", CreatedAt = SeedDate });

        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, FullName = "George Orwell", Country = "United Kingdom", Biography = "English novelist and essayist.", CreatedAt = SeedDate },
            new Author { Id = 2, FullName = "Carl Sagan", Country = "United States", Biography = "Astronomer and science communicator.", CreatedAt = SeedDate },
            new Author { Id = 3, FullName = "Robert C. Martin", Country = "United States", Biography = "Author on software craftsmanship.", CreatedAt = SeedDate });

        modelBuilder.Entity<Book>().HasData(
            new Book
            {
                Id = 1,
                Title = "Nineteen Eighty-Four",
                Isbn = "9780451524935",
                Description = "A dystopian novel about totalitarian surveillance.",
                Publisher = "Secker and Warburg",
                PublishedYear = 1949,
                Price = 9.99m,
                CopiesAvailable = 5,
                AuthorId = 1,
                CategoryId = 1,
                CreatedAt = SeedDate
            },
            new Book
            {
                Id = 2,
                Title = "Cosmos",
                Isbn = "9780345539434",
                Description = "A tour of the universe and the history of science.",
                Publisher = "Random House",
                PublishedYear = 1980,
                Price = 14.50m,
                CopiesAvailable = 3,
                AuthorId = 2,
                CategoryId = 2,
                CreatedAt = SeedDate
            },
            new Book
            {
                Id = 3,
                Title = "Clean Code",
                Isbn = "9780132350884",
                Description = "A handbook of agile software craftsmanship.",
                Publisher = "Prentice Hall",
                PublishedYear = 2008,
                Price = 39.99m,
                CopiesAvailable = 4,
                AuthorId = 3,
                CategoryId = 3,
                CreatedAt = SeedDate
            });
    }
}

namespace LibraryManagement.Infrastructure.Entities;

/// <summary>
/// Fields shared by every table. CreatedAt and UpdatedAt are stamped automatically
/// by AppDbContext, so no service has to remember to set them.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

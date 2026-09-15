namespace LibraryManagement.AppServices.DTOs.Common;

/// <summary>
/// Query-string options shared by the list endpoints. The setters clamp the values, so a
/// caller cannot ask for page zero or an unbounded page.
/// </summary>
public class QueryParameters
{
    private const int MaxPageSize = 100;

    private int _pageNumber = 1;
    private int _pageSize = 10;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 10,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    /// <summary>Free-text filter. Each service decides which columns it matches against.</summary>
    public string? Search { get; set; }
}

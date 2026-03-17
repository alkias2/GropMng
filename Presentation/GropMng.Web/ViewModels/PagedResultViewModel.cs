namespace GropMng.Web.ViewModels;

/// <summary>
/// Represents a view model used by UI rendering and request/response composition.
/// Carries presentation-focused data without embedding business logic.
/// </summary>
public class PagedResultViewModel<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

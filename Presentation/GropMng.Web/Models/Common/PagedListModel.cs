namespace GropMng.Web.Models.Common
{
    /// <summary>
    /// Minimal reusable paged list response contract for listing view models.
    /// </summary>
    public class PagedListModel<TItem>
    {
        public List<TItem> Items { get; set; } = new();
        public PaginationModel Pagination { get; set; } = new();
    }
}

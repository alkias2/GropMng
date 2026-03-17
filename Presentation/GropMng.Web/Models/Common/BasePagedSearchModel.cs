namespace GropMng.Web.Models.Common
{
    /// <summary>
    /// Minimal reusable paging request contract for listing search models.
    /// </summary>
    public abstract class BasePagedSearchModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

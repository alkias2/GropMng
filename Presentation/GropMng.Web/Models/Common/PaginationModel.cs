using GropMng.Core;

namespace GropMng.Web.Models.Common
{
    /// <summary>
    /// Ultra-simple pagination model for views.
    /// Contains only essential properties needed for pager rendering.
    /// </summary>
    public class PaginationModel
    {
        /// <summary>
        /// Gets or sets the current page number (1-based for display)
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Gets or sets the page size (items per page)
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets the total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets the total number of items
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Gets a value indicating whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Gets a value indicating whether there is a next page
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Gets the previous page number (1-based)
        /// </summary>
        public int PreviousPage => CurrentPage - 1;

        /// <summary>
        /// Gets the next page number (1-based)
        /// </summary>
        public int NextPage => CurrentPage + 1;

        /// <summary>
        /// Factory method to create PaginationModel from IPagedList.
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="pagedList">IPagedList source</param>
        /// <param name="currentPage">Current page (1-based)</param>
        /// <returns>PaginationModel instance</returns>
        public static PaginationModel FromPagedList<T>(IPagedList<T> pagedList, int currentPage = 1)
        {
            if (pagedList == null)
                throw new ArgumentNullException(nameof(pagedList));

            return new PaginationModel
            {
                CurrentPage = currentPage,
                PageSize = pagedList.PageSize,
                TotalItems = pagedList.TotalCount,
                TotalPages = pagedList.TotalPages
            };
        }
    }
}

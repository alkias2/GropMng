using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using GropMng.Web.Models.Common;

namespace GropMng.Web.Extensions
{
    /// <summary>
    /// HTML Helper extensions for pagination.
    /// Provides simple, Bootstrap-compatible pager rendering.
    /// </summary>
    public static class HtmlPagerExtension
    {
        /// <summary>
        /// Generates a pager using query string parameter (?page=X)
        /// </summary>
        /// <param name="htmlHelper">HTML helper</param>
        /// <param name="pagination">Pagination model</param>
        /// <param name="actionName">Controller action name (optional)</param>
        /// <param name="controllerName">Controller name (optional)</param>
        /// <param name="routeValues">Additional route values (optional)</param>
        /// <returns>HTML content for pager</returns>
        public static IHtmlContent PagerQueryString(
            this IHtmlHelper htmlHelper,
            PaginationModel pagination,
            string actionName = null,
            string controllerName = null,
            object routeValues = null)
        {
            if (pagination == null)
                return new HtmlString(string.Empty);

            if (pagination.TotalPages <= 1)
                return new HtmlString(string.Empty);

            var html = new StringBuilder();

            // Wrapper nav with Bootstrap pagination class
            html.Append("<nav aria-label=\"Page navigation\">");
            html.Append("<ul class=\"pagination\">");

            // Previous button
            if (pagination.HasPreviousPage)
            {
                html.Append("<li class=\"page-item\">");
                html.Append($"<a class=\"page-link\" href=\"?page={pagination.PreviousPage}\" aria-label=\"Previous\">");
                html.Append("<span aria-hidden=\"true\"><i class=\"bx bx-chevron-left\"></i></span> Προηγούμενη");
                html.Append("</a>");
                html.Append("</li>");
            }
            else
            {
                html.Append("<li class=\"page-item disabled\">");
                html.Append("<a class=\"page-link\" href=\"#\" disabled>");
                html.Append("<span aria-hidden=\"true\"><i class=\"bx bx-chevron-left\"></i></span> Προηγούμενη");
                html.Append("</a>");
                html.Append("</li>");
            }

            // Page numbers
            // Show: first page, middle pages (around current), last page
            var startPage = Math.Max(1, pagination.CurrentPage - 2);
            var endPage = Math.Min(pagination.TotalPages, pagination.CurrentPage + 2);

            if (startPage > 1)
            {
                html.Append("<li class=\"page-item\">");
                html.Append($"<a class=\"page-link\" href=\"?page=1\">1</a>");
                html.Append("</li>");

                if (startPage > 2)
                {
                    html.Append("<li class=\"page-item disabled\">");
                    html.Append("<span class=\"page-link\">...</span>");
                    html.Append("</li>");
                }
            }

            for (int i = startPage; i <= endPage; i++)
            {
                if (i == pagination.CurrentPage)
                {
                    html.Append("<li class=\"page-item active\">");
                    html.Append($"<span class=\"page-link\">{i} <span class=\"sr-only\">(current)</span></span>");
                    html.Append("</li>");
                }
                else
                {
                    html.Append("<li class=\"page-item\">");
                    html.Append($"<a class=\"page-link\" href=\"?page={i}\">{i}</a>");
                    html.Append("</li>");
                }
            }

            if (endPage < pagination.TotalPages)
            {
                if (endPage < pagination.TotalPages - 1)
                {
                    html.Append("<li class=\"page-item disabled\">");
                    html.Append("<span class=\"page-link\">...</span>");
                    html.Append("</li>");
                }

                html.Append("<li class=\"page-item\">");
                html.Append($"<a class=\"page-link\" href=\"?page={pagination.TotalPages}\">{pagination.TotalPages}</a>");
                html.Append("</li>");
            }

            // Next button
            if (pagination.HasNextPage)
            {
                html.Append("<li class=\"page-item\">");
                html.Append($"<a class=\"page-link\" href=\"?page={pagination.NextPage}\" aria-label=\"Next\">");
                html.Append("Επόμενη <span aria-hidden=\"true\"><i class=\"bx bx-chevron-right\"></i></span>");
                html.Append("</a>");
                html.Append("</li>");
            }
            else
            {
                html.Append("<li class=\"page-item disabled\">");
                html.Append("<a class=\"page-link\" href=\"#\" disabled>");
                html.Append("Επόμενη <span aria-hidden=\"true\"><i class=\"bx bx-chevron-right\"></i></span>");
                html.Append("</a>");
                html.Append("</li>");
            }

            html.Append("</ul>");
            html.Append("</nav>");

            return new HtmlString(html.ToString());
        }

        /// <summary>
        /// Simplified overload - uses current URL and only modifies page parameter
        /// </summary>
        public static IHtmlContent Pager(
            this IHtmlHelper htmlHelper,
            PaginationModel pagination)
        {
            return PagerQueryString(htmlHelper, pagination);
        }
    }
}

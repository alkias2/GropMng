using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GropMng.Web.Infrastructure.Security;

/// <summary>
/// Enforces that the current request is authenticated and belongs to an administrator owner.
/// </summary>
public class AuthorizeAdminFilter : IAsyncAuthorizationFilter
{
    /// <inheritdoc />
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return Task.CompletedTask;
        }

        var isAdministrator = user.IsInRole("Administrator")
            || string.Equals(user.FindFirstValue(ClaimTypes.Role), "Administrator", StringComparison.OrdinalIgnoreCase);

        if (!isAdministrator)
            context.Result = new ForbidResult();

        return Task.CompletedTask;
    }
}

/// <summary>
/// Attribute that applies the administrator-only authorization filter.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class AuthorizeAdminAttribute : TypeFilterAttribute
{
    public AuthorizeAdminAttribute() : base(typeof(AuthorizeAdminFilter))
    {
    }
}

using GropMng.Core.Interfaces.Services.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GropMng.Web.Infrastructure.Security;

/// <summary>
/// Evaluates a permission system name against the current owner context.
/// </summary>
public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IPermissionService _permissionService;
    private readonly string _permissionSystemName;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionAuthorizationFilter"/> class.
    /// </summary>
    public PermissionAuthorizationFilter(IPermissionService permissionService, string permissionSystemName)
    {
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _permissionSystemName = string.IsNullOrWhiteSpace(permissionSystemName)
            ? throw new ArgumentException("Permission system name is required.", nameof(permissionSystemName))
            : permissionSystemName;
    }

    /// <inheritdoc />
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        var authorized = await _permissionService.AuthorizeAsync(_permissionSystemName, context.HttpContext.RequestAborted);
        if (!authorized)
            context.Result = new ForbidResult();
    }
}

/// <summary>
/// Attribute that applies a permission-based authorization filter to an action or controller.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class CheckPermissionAttribute : TypeFilterAttribute
{
    public CheckPermissionAttribute(string permissionSystemName) : base(typeof(PermissionAuthorizationFilter))
    {
        Arguments = new object[] { permissionSystemName };
    }
}

using System.Security.Claims;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace GropMng.Tests.Services;

/// <summary>
/// Covers the custom Milestone 5 admin and permission authorization filters.
/// </summary>
public class AdminAuthorizationFilterTests
{
    [Fact]
    public async Task AuthorizeAdminFilter_WhenUserIsNotAdministrator_ReturnsForbidResult()
    {
        // Arrange
        var filter = new AuthorizeAdminFilter();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Role, "RegisteredOwner") },
                authenticationType: "Cookies"))
        };

        var context = new AuthorizationFilterContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>());

        // Act
        await filter.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<ForbidResult>(context.Result);
    }

    [Fact]
    public async Task PermissionAuthorizationFilter_WhenPermissionIsMissing_ReturnsForbidResult()
    {
        // Arrange
        var permissionService = new Mock<IPermissionService>();
        permissionService
            .Setup(service => service.AuthorizeAsync("ManageSettings", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var filter = new PermissionAuthorizationFilter(permissionService.Object, "ManageSettings");
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Role, "Administrator") },
                authenticationType: "Cookies"))
        };

        var context = new AuthorizationFilterContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>());

        // Act
        await filter.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<ForbidResult>(context.Result);
    }

    [Fact]
    public void GropMngPermissionProvider_GetAllPermissions_ReturnsUniqueSystemNames()
    {
        // Arrange
        var permissions = GropMngPermissionProvider.GetAllPermissions();

        // Act
        var distinctCount = permissions.Select(permission => permission.SystemName).Distinct(StringComparer.OrdinalIgnoreCase).Count();

        // Assert
        Assert.NotEmpty(permissions);
        Assert.Equal(distinctCount, permissions.Count);
    }
}

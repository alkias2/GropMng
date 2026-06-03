using System.Security.Claims;
using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Services.Services.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GropMng.Tests.Services;

public class OwnerAuthenticationServiceInvalidationTests
{
    [Fact]
    public async Task SignOutAsync_WhenClaimsExist_UsesNormalizedOwnerIdAndEmailForCacheInvalidation()
    {
        var ownerId = Guid.NewGuid();
        var cacheManager = new Mock<IGropStaticCacheManager>();
        cacheManager
            .Setup(manager => manager.RemoveAsync(It.IsAny<GropCacheKey>(), It.IsAny<object[]>()))
            .Returns(Task.CompletedTask);

        var authenticationService = new Mock<IAuthenticationService>();
        authenticationService
            .Setup(service => service.SignOutAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(authenticationService.Object);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[]
                {
                    new Claim(CurrentOwnerProvider.OwnerIdClaimType, ownerId.ToString()),
                    new Claim(ClaimTypes.Email, "owner@gropmng.local")
                },
                authenticationType: "Cookies"))
        };

        var service = new OwnerAuthenticationService(
            new HttpContextAccessor { HttpContext = httpContext },
            new Mock<IRepository<Owner>>().Object,
            new OwnerPasswordService(),
            cacheManager.Object);

        await service.SignOutAsync();

        cacheManager.Verify(manager => manager.RemoveAsync(
            It.IsAny<GropCacheKey>(),
            It.Is<object[]>(parameters =>
                parameters.Length == 1
                && parameters[0] != null
                && string.Equals(parameters[0].ToString(), ownerId.ToString("N"), StringComparison.Ordinal))), Times.Once);

        cacheManager.Verify(manager => manager.RemoveAsync(
            It.IsAny<GropCacheKey>(),
            It.Is<object[]>(parameters =>
                parameters.Length == 1
                && parameters[0] != null
                && string.Equals(parameters[0].ToString(), "owner@gropmng.local", StringComparison.Ordinal))), Times.Once);
    }
}

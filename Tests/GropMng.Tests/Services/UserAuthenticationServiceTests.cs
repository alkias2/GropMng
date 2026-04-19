using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Domain.Security;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Services.Services.User;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace GropMng.Tests.Services;

/// <summary>
/// Covers the Milestone 3 custom owner authentication and permission services.
/// </summary>
public class UserAuthenticationServiceTests
{
    [Fact]
    public void HashPassword_WhenPasswordIsValid_CanBeVerifiedSuccessfully()
    {
        // Arrange
        var service = new OwnerPasswordService();

        // Act
        var result = service.HashPassword("StrongPass123!");

        // Assert
        Assert.NotEmpty(result.Hash);
        Assert.NotEmpty(result.Salt);
        Assert.True(service.VerifyPassword("StrongPass123!", result.Hash, result.Salt));
        Assert.False(service.VerifyPassword("WrongPass123!", result.Hash, result.Salt));
    }

    [Fact]
    public void VerifyPassword_WhenLegacySha256HashIsUsed_ReturnsTrueForMatchingPassword()
    {
        // Arrange
        const string password = "ChangeMe123!";
        var legacyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        var service = new OwnerPasswordService();

        // Act
        var isValid = service.VerifyPassword(password, legacyHash, string.Empty);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WhenPasswordMatchesCurrentRecord_ReturnsOwner()
    {
        // Arrange
        var ownerBusinessId = Guid.NewGuid();
        var passwordService = new OwnerPasswordService();
        var passwordResult = passwordService.HashPassword("StrongPass123!");
        var ownerRepository = new Mock<IRepository<Owner>>();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };

        ownerRepository
            .SetupGet(repository => repository.TableNoTracking)
            .Returns(new List<Owner>
            {
                new()
                {
                    OwnerId = ownerBusinessId,
                    FirstName = "System",
                    LastName = "Admin",
                    DisplayName = "System Admin",
                    Email = "admin@gropmng.local",
                    PasswordHash = passwordResult.Hash,
                    IsActive = true,
                    Passwords =
                    [
                        new OwnerPassword
                        {
                            PasswordHash = passwordResult.Hash,
                            PasswordSalt = passwordResult.Salt,
                            IsCurrent = true
                        }
                    ]
                }
            }.AsQueryable());

        var service = new OwnerAuthenticationService(httpContextAccessor, ownerRepository.Object, passwordService);

        // Act
        var owner = await service.ValidateCredentialsAsync("admin@gropmng.local", "StrongPass123!");

        // Assert
        Assert.NotNull(owner);
        Assert.Equal(ownerBusinessId, owner.OwnerId);
    }

    [Fact]
    public async Task AuthorizeAsync_WhenOwnerHasMatchingPermission_ReturnsTrue()
    {
        // Arrange
        var ownerBusinessId = Guid.NewGuid();
        var ownerRepository = new Mock<IRepository<Owner>>();
        var currentOwnerProvider = new Mock<ICurrentOwnerProvider>();

        currentOwnerProvider
            .Setup(provider => provider.GetCurrentOwnerIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerBusinessId);

        ownerRepository
            .Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Owner, bool>>>(),
                false,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Owner
            {
                OwnerId = ownerBusinessId,
                FirstName = "System",
                LastName = "Admin",
                DisplayName = "System Admin",
                Email = "admin@gropmng.local",
                PasswordHash = "irrelevant",
                IsActive = true,
                OwnerRoles =
                [
                    new OwnerRole
                    {
                        Name = "Administrator",
                        SystemName = "Administrator",
                        IsActive = true,
                        PermissionRecords =
                        [
                            new PermissionRecord
                            {
                                Name = "Manage Owners",
                                SystemName = "ManageOwners",
                                Category = "Owners"
                            }
                        ]
                    }
                ]
            });

        var service = new PermissionService(ownerRepository.Object, currentOwnerProvider.Object);

        // Act
        var isAuthorized = await service.AuthorizeAsync("ManageOwners");

        // Assert
        Assert.True(isAuthorized);
    }

    [Fact]
    public async Task GetCurrentOwnerIdAsync_WhenClaimExists_ReturnsAuthenticatedOwnerId()
    {
        // Arrange
        var ownerBusinessId = Guid.NewGuid();
        var ownerRepository = new Mock<IRepository<Owner>>();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim(CurrentOwnerProvider.OwnerIdClaimType, ownerBusinessId.ToString())
                        },
                        authenticationType: "Cookies"))
            }
        };

        var provider = new CurrentOwnerProvider(ownerRepository.Object, memoryCache, httpContextAccessor);

        // Act
        var result = await provider.GetCurrentOwnerIdAsync();

        // Assert
        Assert.Equal(ownerBusinessId, result);
        ownerRepository.Verify(
            repository => repository.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Owner, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

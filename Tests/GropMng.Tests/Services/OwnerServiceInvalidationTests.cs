using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Services.Services.User;
using Moq;

namespace GropMng.Tests.Services;

public class OwnerServiceInvalidationTests
{
    [Fact]
    public async Task AssignRolesAsync_WhenRolesUpdated_InvalidatesSecurityAndPermissionPrefixes()
    {
        var ownerId = Guid.NewGuid();
        var owner = new Owner
        {
            Id = 10,
            OwnerId = ownerId,
            FirstName = "Owner",
            LastName = "User",
            Email = "owner@gropmng.local",
            PasswordHash = "hash",
            IsActive = true
        };

        var ownerRepository = new Mock<IRepository<Owner>>();
        ownerRepository
            .Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Owner, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        ownerRepository
            .Setup(repository => repository.UpdateAsync(It.IsAny<Owner>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Owner updatedOwner, bool _, CancellationToken _) => updatedOwner);

        var ownerRoleRepository = new Mock<IRepository<OwnerRole>>();
        ownerRoleRepository
            .Setup(repository => repository.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<OwnerRole, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OwnerRole>
            {
                new() { SystemName = "Administrator", IsActive = true }
            });

        var cacheManager = new Mock<IGropStaticCacheManager>();
        cacheManager
            .Setup(manager => manager.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(Task.CompletedTask);

        var service = new OwnerService(ownerRepository.Object, ownerRoleRepository.Object, cacheManager.Object);

        await service.AssignRolesAsync(ownerId, new[] { "Administrator" });

        cacheManager.Verify(manager => manager.RemoveByPrefixAsync("Grop.auth.owner.byid.", It.IsAny<object[]>()), Times.Once);
        cacheManager.Verify(manager => manager.RemoveByPrefixAsync("Grop.auth.owner.byemail.", It.IsAny<object[]>()), Times.Once);
        cacheManager.Verify(manager => manager.RemoveByPrefixAsync("Grop.permissions.byowner.", It.IsAny<object[]>()), Times.Once);
    }
}

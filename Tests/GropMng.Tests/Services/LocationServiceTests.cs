using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Services.Services.Garden.Locations;
using Moq;

namespace GropMng.Tests.Services;

/// <summary>
/// Contains focused unit tests for <see cref="LocationService" />.
/// </summary>
public class LocationServiceTests
{
    #region AddGardenSpotAsync Tests

    /// <summary>
    /// Verifies that adding a garden spot assigns aggregate ownership data and audit values before persistence.
    /// </summary>
    [Fact]
    public async Task AddGardenSpotAsync_AssignsAggregateDataAndAuditFields()
    {
        // Arrange
        var ownerId = "owner-1";
        var locationRepository = new Mock<IRepository<Location>>();
        var gardenSpotRepository = new Mock<IRepository<GardenSpot>>();
        var location = new Location
        {
            Id = 10,
            OwnerId = ownerId,
            Name = "Backyard",
            City = "Athens"
        };

        locationRepository
            .Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Location, bool>>>(), false, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        gardenSpotRepository
            .Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GardenSpot, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        gardenSpotRepository
            .Setup(repository => repository.CreateAsync(It.IsAny<GardenSpot>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GardenSpot entity, bool _, CancellationToken _) => entity);

        var service = new LocationService(locationRepository.Object, gardenSpotRepository.Object);
        var gardenSpot = new GardenSpot
        {
            OwnerId = ownerId,
            Name = "  Sunny Corner  "
        };

        // Act
        var result = await service.AddGardenSpotAsync(location.Id, gardenSpot);

        // Assert
        Assert.Equal(location.Id, result.LocationId);
        Assert.Equal(ownerId, result.OwnerId);
        Assert.Equal("Sunny Corner", result.Name);
        Assert.NotEqual(default, result.CreatedAtUtc);
        Assert.Equal(result.CreatedAtUtc, result.UpdatedAtUtc);
    }

    #endregion
}
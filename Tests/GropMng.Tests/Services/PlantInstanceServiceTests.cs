using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Services.Services.Garden.Plants;
using Moq;

namespace GropMng.Tests.Services;

/// <summary>
/// Contains focused unit tests for <see cref="PlantInstanceService" />.
/// </summary>
public class PlantInstanceServiceTests
{
    #region AddWateringScheduleAsync Tests

    /// <summary>
    /// Verifies that adding a watering schedule stamps aggregate ownership and relationship keys before persistence.
    /// </summary>
    [Fact]
    public async Task AddWateringScheduleAsync_AssignsPlantInstanceAndOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var plantInstanceRepository = new Mock<IRepository<PlantInstance>>();
        var wateringScheduleRepository = new Mock<IRepository<WateringSchedule>>();

        plantInstanceRepository
            .Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PlantInstance, bool>>>(), false, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlantInstance
            {
                Id = 9,
                OwnerId = ownerId,
                PlantId = 1,
                GardenSpotId = 2
            });

        wateringScheduleRepository
            .Setup(repository => repository.CreateAsync(It.IsAny<WateringSchedule>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WateringSchedule entity, bool _, CancellationToken _) => entity);

        var service = new PlantInstanceService(
            plantInstanceRepository.Object,
            Mock.Of<IRepository<Plant>>(),
            Mock.Of<IRepository<GardenSpot>>(),
            Mock.Of<IRepository<Container>>(),
            Mock.Of<IRepository<SoilMix>>(),
            wateringScheduleRepository.Object,
            Mock.Of<IRepository<FertilizingSchedule>>(),
            Mock.Of<IRepository<PlantPhoto>>(),
            Mock.Of<IRepository<PlantNote>>(),
            Mock.Of<IRepository<GropMng.Core.Domain.Garden.Health.PlantDiseaseRecord>>(),
            Mock.Of<IRepository<GropMng.Core.Domain.Garden.Health.DiseasePhoto>>(),
            Mock.Of<IRepository<GropMng.Core.Domain.Garden.Health.Disease>>(),
            Mock.Of<IRepository<Fertilizer>>());

        var schedule = new WateringSchedule
        {
            OwnerId = ownerId,
            FrequencyDays = 3
        };

        // Act
        var result = await service.AddWateringScheduleAsync(9, schedule);

        // Assert
        Assert.Equal(9, result.PlantInstanceId);
        Assert.Equal(ownerId, result.OwnerId);
        Assert.NotEqual(default, result.CreatedAtUtc);
    }

    #endregion
}
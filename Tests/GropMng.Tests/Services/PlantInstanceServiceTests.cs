using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Care;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Services.Services.Garden.Plants;
using Moq;

namespace GropMng.Tests.Services;

/// <summary>
/// Contains focused unit tests for <see cref="PlantInstanceService" /> aggregate-root operations.
/// </summary>
public class PlantInstanceServiceTests
{
    [Fact]
    public async Task CreatePlantInstanceAsync_PersistsPlantBeforeContainerAssignment()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var plantInstanceRepository = new Mock<IRepository<PlantInstance>>();
        var plantRepository = new Mock<IRepository<Plant>>();
        var gardenSpotRepository = new Mock<IRepository<GardenSpot>>();
        var containerRepository = new Mock<IRepository<Container>>();

        var containers = new List<Container>
        {
            new() { Id = 153, OwnerId = ownerId, PlantInstanceId = null, ContainerType = GardenContainerType.Pot }
        };

        PlantInstance? createdEntity = null;
        var saveChangesCount = 0;

        plantInstanceRepository
            .Setup(repository => repository.CreateAsync(It.IsAny<PlantInstance>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlantInstance entity, bool _, CancellationToken _) =>
            {
                createdEntity = entity;
                return entity;
            });

        plantInstanceRepository
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken _) =>
            {
                saveChangesCount++;
                if (saveChangesCount == 1 && createdEntity is not null)
                    createdEntity.Id = 501;

                return Task.CompletedTask;
            });

        plantRepository
            .Setup(repository => repository.GetByIdAsync(22, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Plant { Id = 22, CommonName = "Aloe Vera", ScientificName = "Aloe vera" });

        gardenSpotRepository
            .Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GardenSpot, bool>>>(), false, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GardenSpot { Id = 10, OwnerId = ownerId, Name = "Balcony" });

        containerRepository
            .Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Container, bool>>>(), false, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Container, bool>> predicate, bool _, bool _, CancellationToken _) => containers.FirstOrDefault(predicate.Compile()));

        containerRepository
            .Setup(repository => repository.UpdateAsync(It.IsAny<Container>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Container entity, bool _, CancellationToken _) => entity);

        var service = new PlantInstanceService(
            plantInstanceRepository.Object,
            plantRepository.Object,
            gardenSpotRepository.Object,
            containerRepository.Object,
            Mock.Of<IRepository<SoilMix>>(),
            Mock.Of<IRepository<RepottingLog>>(),
            Mock.Of<IContainerService>(),
            Mock.Of<ISoilMixService>(),
            Mock.Of<IWateringService>(),
            Mock.Of<IFertilizingService>(),
            Mock.Of<IPlantPhotoService>(),
            Mock.Of<IPlantNoteService>());

        var plantInstance = new PlantInstance
        {
            OwnerId = ownerId,
            PlantId = 22,
            GardenSpotId = 10,
            ContainerId = 153,
            HealthStatus = PlantHealthStatus.Good,
            IsActive = true
        };

        // Act
        var created = await service.CreatePlantInstanceAsync(plantInstance);

        // Assert
        Assert.Equal(501, created.Id);
        Assert.Equal(501, containers[0].PlantInstanceId);
        Assert.Equal(2, saveChangesCount);
    }

    [Fact]
    public async Task UpdatePlantInstanceAsync_SyncsContainerAssignmentThroughContainerEntity()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var plantInstanceRepository = new Mock<IRepository<PlantInstance>>();
        var plantRepository = new Mock<IRepository<Plant>>();
        var gardenSpotRepository = new Mock<IRepository<GardenSpot>>();
        var containerRepository = new Mock<IRepository<Container>>();
        var soilMixRepository = new Mock<IRepository<SoilMix>>();

        var existingPlantInstance = new PlantInstance
        {
            Id = 15,
            OwnerId = ownerId,
            PlantId = 2,
            GardenSpotId = 7,
            SoilMixId = 3
        };

        var containers = new List<Container>
        {
            new() { Id = 20, OwnerId = ownerId, PlantInstanceId = 15, ContainerType = GardenContainerType.Pot },
            new() { Id = 30, OwnerId = ownerId, PlantInstanceId = null, ContainerType = GardenContainerType.RaisedBed }
        };

        plantInstanceRepository
            .Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PlantInstance, bool>>>(), false, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlantInstance);

        plantInstanceRepository
            .Setup(repository => repository.UpdateAsync(It.IsAny<PlantInstance>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlantInstance entity, bool _, CancellationToken _) => entity);

        plantInstanceRepository
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        plantRepository
            .Setup(repository => repository.GetByIdAsync(2, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Plant { Id = 2, CommonName = "Lavender", ScientificName = "Lavandula" });

        gardenSpotRepository
            .Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GardenSpot, bool>>>(), false, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GardenSpot { Id = 7, OwnerId = ownerId, Name = "Patio" });

        containerRepository
            .Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Container, bool>>>(), false, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Container, bool>> predicate, bool _, bool _, CancellationToken _) => containers.FirstOrDefault(predicate.Compile()));

        containerRepository
            .Setup(repository => repository.UpdateAsync(It.IsAny<Container>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Container entity, bool _, CancellationToken _) => entity);

        soilMixRepository
            .Setup(repository => repository.GetByIdAsync(3, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SoilMix { Id = 3, Name = "Balanced Mix" });

        var service = new PlantInstanceService(
            plantInstanceRepository.Object,
            plantRepository.Object,
            gardenSpotRepository.Object,
            containerRepository.Object,
            soilMixRepository.Object,
            Mock.Of<IRepository<RepottingLog>>(),
            Mock.Of<IContainerService>(),
            Mock.Of<ISoilMixService>(),
            Mock.Of<IWateringService>(),
            Mock.Of<IFertilizingService>(),
            Mock.Of<IPlantPhotoService>(),
            Mock.Of<IPlantNoteService>());

        var updatedPlantInstance = new PlantInstance
        {
            Id = 15,
            OwnerId = ownerId,
            PlantId = 2,
            GardenSpotId = 7,
            ContainerId = 30,
            SoilMixId = 3,
            HealthStatus = PlantHealthStatus.Good,
            IsActive = true
        };

        // Act
        await service.UpdatePlantInstanceAsync(updatedPlantInstance);

        // Assert
        Assert.Null(containers[0].PlantInstanceId);
        Assert.Equal(15, containers[1].PlantInstanceId);
    }

    [Fact]
    public async Task RepotPlantAsync_UsesLinkedContainerAsPreviousContainer()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var plantInstanceRepository = new Mock<IRepository<PlantInstance>>();
        var containerRepository = new Mock<IRepository<Container>>();
        var repottingLogRepository = new Mock<IRepository<RepottingLog>>();
        var soilMixRepository = new Mock<IRepository<SoilMix>>();

        var plantInstance = new PlantInstance
        {
            Id = 11,
            OwnerId = ownerId,
            PlantId = 5,
            GardenSpotId = 9,
            SoilMixId = 4,
            HealthStatus = PlantHealthStatus.Good,
            IsActive = true
        };

        var containers = new List<Container>
        {
            new() { Id = 101, OwnerId = ownerId, PlantInstanceId = 11, ContainerType = GardenContainerType.Pot },
            new() { Id = 202, OwnerId = ownerId, PlantInstanceId = null, ContainerType = GardenContainerType.WindowBox }
        };

        plantInstanceRepository
            .Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PlantInstance, bool>>>(), false, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantInstance);

        plantInstanceRepository
            .Setup(repository => repository.UpdateAsync(It.IsAny<PlantInstance>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlantInstance entity, bool _, CancellationToken _) => entity);

        plantInstanceRepository
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        containerRepository
            .Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Container, bool>>>(), false, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Container, bool>> predicate, bool _, bool _, CancellationToken _) => containers.FirstOrDefault(predicate.Compile()));

        containerRepository
            .Setup(repository => repository.UpdateAsync(It.IsAny<Container>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Container entity, bool _, CancellationToken _) => entity);

        soilMixRepository
            .Setup(repository => repository.GetByIdAsync(4, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SoilMix { Id = 4, Name = "Drainage Mix" });

        repottingLogRepository
            .Setup(repository => repository.CreateAsync(It.IsAny<RepottingLog>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RepottingLog entity, bool _, CancellationToken _) => entity);

        var service = new PlantInstanceService(
            plantInstanceRepository.Object,
            Mock.Of<IRepository<Plant>>(),
            Mock.Of<IRepository<GardenSpot>>(),
            containerRepository.Object,
            soilMixRepository.Object,
            repottingLogRepository.Object,
            Mock.Of<IContainerService>(),
            Mock.Of<ISoilMixService>(),
            Mock.Of<IWateringService>(),
            Mock.Of<IFertilizingService>(),
            Mock.Of<IPlantPhotoService>(),
            Mock.Of<IPlantNoteService>());

        var repottingLog = new RepottingLog
        {
            OwnerId = ownerId,
            NewContainerId = 202,
            NewSoilMixId = 4,
            RepottedAtUtc = DateTime.UtcNow
        };

        // Act
        var createdLog = await service.RepotPlantAsync(11, repottingLog);

        // Assert
        Assert.Equal(101, createdLog.PreviousContainerId);
        Assert.Null(containers[0].PlantInstanceId);
        Assert.Equal(11, containers[1].PlantInstanceId);
    }
}
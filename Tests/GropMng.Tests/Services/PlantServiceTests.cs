using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Services.Services.Garden.Plants;
using Moq;

namespace GropMng.Tests.Services;

/// <summary>
/// Contains focused unit tests for <see cref="PlantService" />.
/// </summary>
public class PlantServiceTests
{
    #region CreatePlantAsync Tests

    /// <summary>
    /// Verifies that duplicate scientific names are rejected before persistence.
    /// </summary>
    [Fact]
    public async Task CreatePlantAsync_WithDuplicateScientificName_ThrowsDomainException()
    {
        // Arrange
        var plantRepository = new Mock<IRepository<Plant>>();
        var plantInstanceRepository = new Mock<IRepository<PlantInstance>>();
        plantRepository
            .Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Plant, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new PlantService(plantRepository.Object, plantInstanceRepository.Object);
        var plant = new Plant
        {
            CommonName = "Tomato",
            ScientificName = "Solanum lycopersicum"
        };

        // Act
        var action = async () => await service.CreatePlantAsync(plant);

        // Assert
        await Assert.ThrowsAsync<DomainException>(action);
    }

    #endregion

    #region UpdatePlantAsync Tests

    /// <summary>
    /// Verifies that PictureId changes are persisted during plant updates.
    /// </summary>
    [Fact]
    public async Task UpdatePlantAsync_WithChangedPictureId_PersistsPictureId()
    {
        // Arrange
        var existingPlant = new Plant
        {
            Id = 12,
            CommonName = "Tomato",
            ScientificName = "Solanum lycopersicum",
            PictureId = 10
        };

        var plantRepository = new Mock<IRepository<Plant>>();
        var plantInstanceRepository = new Mock<IRepository<PlantInstance>>();

        plantRepository
            .Setup(repository => repository.GetByIdAsync(12, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlant);

        plantRepository
            .Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Plant, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        plantRepository
            .Setup(repository => repository.UpdateAsync(It.IsAny<Plant>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plant entity, bool _, CancellationToken _) => entity);

        var service = new PlantService(plantRepository.Object, plantInstanceRepository.Object);

        var updated = new Plant
        {
            Id = 12,
            CommonName = "Tomato",
            ScientificName = "Solanum lycopersicum",
            PictureId = 99
        };

        // Act
        var result = await service.UpdatePlantAsync(updated);

        // Assert
        Assert.Equal(99, result.PictureId);
        plantRepository.Verify(repository => repository.UpdateAsync(
            It.Is<Plant>(entity => entity != null && entity.PictureId == 99),
            true,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
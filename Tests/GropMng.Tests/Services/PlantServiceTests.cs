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
}
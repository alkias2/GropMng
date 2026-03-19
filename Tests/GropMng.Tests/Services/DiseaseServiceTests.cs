using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Services.Services.Garden.Health;
using Moq;

namespace GropMng.Tests.Services;

/// <summary>
/// Contains focused unit tests for <see cref="DiseaseService" />.
/// </summary>
public class DiseaseServiceTests
{
    #region AddRemedyLinkAsync Tests

    /// <summary>
    /// Verifies that adding a remedy link fails when the referenced pesticide does not exist.
    /// </summary>
    [Fact]
    public async Task AddRemedyLinkAsync_WhenPesticideDoesNotExist_ThrowsDomainException()
    {
        // Arrange
        var diseaseRepository = new Mock<IRepository<Disease>>();
        var remedyLinkRepository = new Mock<IRepository<DiseaseRemedyLink>>();
        var pesticideRepository = new Mock<IRepository<Pesticide>>();

        diseaseRepository
            .Setup(repository => repository.GetByIdAsync(15, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Disease { Id = 15, Name = "Powdery Mildew" });

        pesticideRepository
            .Setup(repository => repository.GetByIdAsync(5, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pesticide?)null);

        var service = new DiseaseService(diseaseRepository.Object, remedyLinkRepository.Object, pesticideRepository.Object);
        var remedyLink = new DiseaseRemedyLink
        {
            PesticideId = 5
        };

        // Act
        var action = async () => await service.AddRemedyLinkAsync(15, remedyLink);

        // Assert
        await Assert.ThrowsAsync<DomainException>(action);
    }

    #endregion
}
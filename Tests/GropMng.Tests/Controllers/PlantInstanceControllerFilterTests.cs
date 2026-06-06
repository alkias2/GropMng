using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core;
using GropMng.Core.Interfaces.Services.Garden.Care;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Core.Interfaces.Services.Garden.Locations;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Core.Interfaces.Services.Media;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Controllers;
using GropMng.Web.Models.Garden;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GropMng.Tests.Controllers;

public class PlantInstanceControllerFilterTests
{
    [Fact]
    public async Task List_WhenActiveOnlyFalse_PassesFalseToServiceAndReturnsFilterModelUnchecked()
    {
        var ownerId = Guid.NewGuid();

        var plantInstanceService = new Mock<IPlantInstanceService>();
        plantInstanceService
            .Setup(service => service.GetPlantInstancesAsync(
                ownerId,
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<PlantInstance>(Array.Empty<PlantInstance>(), 0, int.MaxValue));

        var plantPhotoService = new Mock<IPlantPhotoService>();
        plantPhotoService
            .Setup(service => service.GetMainPhotoAsync(It.IsAny<int>(), ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlantPhoto?)null);

        var plantService = new Mock<IPlantService>();
        plantService
            .Setup(service => service.GetPlantsAsync(null, null, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Plant>(Array.Empty<Plant>(), 0, int.MaxValue));

        var locationService = new Mock<ILocationService>();
        locationService
            .Setup(service => service.GetLocationsAsync(ownerId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Location>(Array.Empty<Location>(), 0, int.MaxValue));

        var currentOwnerProvider = new Mock<ICurrentOwnerProvider>();
        currentOwnerProvider
            .Setup(provider => provider.GetCurrentOwnerIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerId);

        var controller = new PlantInstanceController(
            plantInstanceService.Object,
            plantService.Object,
            locationService.Object,
            Mock.Of<IFertilizerService>(),
            Mock.Of<IDiseaseService>(),
            Mock.Of<IEnumLocalizationHelper>(),
            currentOwnerProvider.Object,
            Mock.Of<IPictureService>(),
            Mock.Of<IContainerService>(),
            Mock.Of<ISoilMixService>(),
            Mock.Of<IWateringService>(),
            Mock.Of<IFertilizingService>(),
            plantPhotoService.Object,
            Mock.Of<IPlantNoteService>(),
            Mock.Of<IPlantDiseaseService>(),
            Mock.Of<IRepottingLogService>());

        var result = await controller.List(null, null, null, false, default);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IList<PlantInstanceListRowModel>>(viewResult.Model);
        Assert.Empty(model);

        var filterModel = Assert.IsType<PlantInstanceListFilterModel>(controller.ViewBag.FilterModel);
        Assert.False(filterModel.ActiveOnly);

        plantInstanceService.Verify(service => service.GetPlantInstancesAsync(
            ownerId,
            null,
            null,
            null,
            false,
            0,
            int.MaxValue,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
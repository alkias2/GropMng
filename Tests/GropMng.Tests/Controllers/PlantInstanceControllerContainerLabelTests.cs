using GropMng.Core;
using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Domain.Garden.Plants;
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

public class PlantInstanceControllerContainerLabelTests
{
    [Fact]
    public async Task Edit_FormatsContainerLabelsWithDimensionsAndOccupancyState()
    {
        var ownerId = Guid.NewGuid();

        var occupiedPot = new Container
        {
            Id = 10,
            OwnerId = ownerId,
            PlantInstanceId = 7,
            ContainerType = GardenContainerType.Pot,
            Material = "Πήλινη",
            BaseCircumferenceCm = 54,
            RimCircumferenceCm = 70,
            HeightCm = 20,
            VolumeL = 6.2m,
            HasDrainageHole = true
        };

        var emptyBed = new Container
        {
            Id = 11,
            OwnerId = ownerId,
            PlantInstanceId = null,
            ContainerType = GardenContainerType.Bed,
            Material = "Παρτέρι",
            LengthCm = 80,
            WidthCm = 80,
            HasDrainageHole = false
        };

        var plantInstanceService = new Mock<IPlantInstanceService>();
        plantInstanceService
            .Setup(service => service.GetPlantInstanceByIdAsync(7, ownerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlantInstance
            {
                Id = 7,
                OwnerId = ownerId,
                PlantId = 3,
                GardenSpotId = 5,
                Container = occupiedPot,
                ContainerId = occupiedPot.Id,
                SoilMixId = null,
                DiseaseRecords = new List<PlantDiseaseRecord>(),
                IsActive = true
            });

        plantInstanceService
            .Setup(service => service.GetContainersAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Container> { occupiedPot, emptyBed });

        plantInstanceService
            .Setup(service => service.GetSoilMixesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<SoilMix>());

        var plantService = new Mock<IPlantService>();
        plantService
            .Setup(service => service.GetPlantsAsync(null, null, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Plant>(new[]
            {
                new Plant { Id = 3, CommonName = "Ελιά", ScientificName = "Olea europaea" }
            }, 1, int.MaxValue));

        var locationService = new Mock<ILocationService>();
        locationService
            .Setup(service => service.GetLocationsAsync(ownerId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Location>(new[]
            {
                new Location { Id = 5, OwnerId = ownerId, Name = "Garden A", City = "Athens" }
            }, 1, int.MaxValue));

        locationService
            .Setup(service => service.GetGardenSpotsAsync(5, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GardenSpot>
            {
                new() { Id = 5, OwnerId = ownerId, LocationId = 5, Name = "Spot 1" }
            });

        var currentOwnerProvider = new Mock<ICurrentOwnerProvider>();
        currentOwnerProvider
            .Setup(provider => provider.GetCurrentOwnerIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerId);

        var enumLocalizationHelper = new Mock<IEnumLocalizationHelper>();
        enumLocalizationHelper
            .Setup(helper => helper.GetLocalizedNameAsync(GardenContainerType.Pot, null))
            .ReturnsAsync("Πήλινη");
        enumLocalizationHelper
            .Setup(helper => helper.GetLocalizedNameAsync(GardenContainerType.Bed, null))
            .ReturnsAsync("Παρτέρι");

        var controller = new PlantInstanceController(
            plantInstanceService.Object,
            plantService.Object,
            locationService.Object,
            Mock.Of<IFertilizerService>(),
            Mock.Of<IDiseaseService>(),
            enumLocalizationHelper.Object,
            currentOwnerProvider.Object,
            Mock.Of<IPictureService>());

        var result = await controller.Edit(7, default);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PlantInstanceModel>(viewResult.Model);

        Assert.Equal("Πήλινη, 6.2 λίτρα, Β17, Χ22, Υ20, Κατειλημμένη", model.ContainerInfo);
        Assert.Contains(model.AvailableContainers, item => item.Value == "10" && item.Text == "Πήλινη, 6.2 λίτρα, Β17, Χ22, Υ20, Κατειλημμένη");
        Assert.Contains(model.AvailableContainers, item => item.Value == "11" && item.Text == "Παρτέρι, 80x80, Κενή");
    }
}

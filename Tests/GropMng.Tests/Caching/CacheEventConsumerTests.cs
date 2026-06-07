using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Services.Caching;
using GropMng.Services.Caching.Garden;
using Moq;

namespace GropMng.Tests.Caching;

public class CacheEventConsumerTests
{
    [Fact]
    public async Task PlantInstanceConsumer_OnUpdate_RemovesDashboardAndDependentPrefixes()
    {
        var cacheManager = new Mock<IGropStaticCacheManager>();
        cacheManager
            .Setup(manager => manager.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(Task.CompletedTask);

        var consumer = new PlantInstanceCacheEventConsumer(cacheManager.Object);

        await consumer.HandleEventAsync(new GropMng.Core.Events.EntityUpdatedEvent<PlantInstance>(new PlantInstance
        {
            OwnerId = Guid.NewGuid(),
            PlantId = 1,
            GardenSpotId = 2,
            HealthStatus = GropMng.Core.Domain.Garden.Enums.PlantHealthStatus.Good,
            IsActive = false
        }));

        cacheManager.Verify(manager => manager.RemoveByPrefixAsync(PlantCacheDefaults.PlantInstancePrefix, It.IsAny<object[]>()), Times.Once);
        cacheManager.Verify(manager => manager.RemoveByPrefixAsync(WateringCacheDefaults.SchedulePrefix, It.IsAny<object[]>()), Times.Once);
        cacheManager.Verify(manager => manager.RemoveByPrefixAsync(FertilizingCacheDefaults.SchedulePrefix, It.IsAny<object[]>()), Times.Once);
        cacheManager.Verify(manager => manager.RemoveByPrefixAsync(ActionSkipCacheDefaults.Prefix, It.IsAny<object[]>()), Times.Once);
        cacheManager.Verify(manager => manager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix, It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task ActionSkipConsumer_OnInsert_RemovesActionSkipAndDashboardPrefixes()
    {
        var cacheManager = new Mock<IGropStaticCacheManager>();
        cacheManager
            .Setup(manager => manager.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(Task.CompletedTask);

        var consumer = new ActionSkipCacheEventConsumer(cacheManager.Object);

        await consumer.HandleEventAsync(new GropMng.Core.Events.EntityInsertedEvent<ActionSkip>(new ActionSkip
        {
            OwnerId = Guid.NewGuid(),
            PlantInstanceId = 13,
            ActionType = ActionSkipType.Watering,
            ActiveUntilDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1)
        }));

        cacheManager.Verify(manager => manager.RemoveByPrefixAsync(ActionSkipCacheDefaults.Prefix, It.IsAny<object[]>()), Times.Once);
        cacheManager.Verify(manager => manager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix, It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task WateringScheduleConsumer_OnUpdate_RemovesWateringAndDashboardPrefixes()
    {
        var cacheManager = new Mock<IGropStaticCacheManager>();
        cacheManager
            .Setup(manager => manager.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(Task.CompletedTask);

        var consumer = new WateringScheduleCacheEventConsumer(cacheManager.Object);

        await consumer.HandleEventAsync(new GropMng.Core.Events.EntityUpdatedEvent<WateringSchedule>(new WateringSchedule
        {
            OwnerId = Guid.NewGuid(),
            PlantInstanceId = 13,
            Season = GropMng.Core.Domain.Garden.Enums.GardenSeason.Summer,
            FrequencyDays = 2
        }));

        cacheManager.Verify(manager => manager.RemoveByPrefixAsync(WateringCacheDefaults.SchedulePrefix, It.IsAny<object[]>()), Times.Once);
        cacheManager.Verify(manager => manager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix, It.IsAny<object[]>()), Times.Once);
    }
}
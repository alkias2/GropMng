using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Events;
using GropMng.Data.DbContext;
using GropMng.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GropMng.Tests.Caching;

public class EfRepositoryEventPublishingTests
{
    [Fact]
    public async Task SaveChangesAsync_WithDeferredUpdatedEntity_PublishesUpdatedEvent()
    {
        await using var context = CreateContext();
        var publisher = new Mock<IEventPublisher>();

        var repository = new EfRepository<PlantInstance>(context, publisher.Object);

        var plant = new Plant { CommonName = "Basil", ScientificName = "Ocimum basilicum" };
        var location = new Location { OwnerId = Guid.NewGuid(), Name = "Balcony", City = "Athens" };
        var spot = new GardenSpot { OwnerId = location.OwnerId, Location = location, Name = "East Corridor" };
        var instance = new PlantInstance
        {
            OwnerId = location.OwnerId,
            Plant = plant,
            GardenSpot = spot,
            HealthStatus = PlantHealthStatus.Good,
            IsActive = true
        };

        await repository.CreateAsync(instance, cancellationToken: default);

        publisher.Invocations.Clear();

        instance.IsActive = false;
        await repository.UpdateAsync(instance, saveNow: false, cancellationToken: default);

        publisher.Verify(p => p.PublishAsync(It.IsAny<EntityUpdatedEvent<PlantInstance>>(), It.IsAny<CancellationToken>()), Times.Never);

        await repository.SaveChangesAsync();

        publisher.Verify(p => p.PublishAsync(It.Is<EntityUpdatedEvent<PlantInstance>>(e => e.Entity == instance), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_WithDeferredInsertedEntity_PublishesInsertedEvent()
    {
        await using var context = CreateContext();
        var publisher = new Mock<IEventPublisher>();

        var repository = new EfRepository<ActionSkip>(context, publisher.Object);

        var actionSkip = new ActionSkip
        {
            OwnerId = Guid.NewGuid(),
            PlantInstanceId = 13,
            ActionType = ActionSkipType.Watering,
            ActiveUntilDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2)
        };

        await repository.CreateAsync(actionSkip, saveNow: false, cancellationToken: default);

        publisher.Verify(p => p.PublishAsync(It.IsAny<EntityInsertedEvent<ActionSkip>>(), It.IsAny<CancellationToken>()), Times.Never);

        await repository.SaveChangesAsync();

        publisher.Verify(p => p.PublishAsync(It.Is<EntityInsertedEvent<ActionSkip>>(e => e.Entity == actionSkip), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static GropContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<GropContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new GropContext(options);
    }
}
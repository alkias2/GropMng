using GropMng.Core;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Locations;

namespace GropMng.Services.Services.Garden.Locations;

/// <summary>
/// Provides aggregate-root operations for locations and their supporting garden spots.
/// </summary>
public class LocationService : ILocationService
{
    #region Fields

    private readonly IRepository<Location> _locationRepository;
    private readonly IRepository<GardenSpot> _gardenSpotRepository;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationService" /> class.
    /// </summary>
    /// <param name="locationRepository">The repository used to manage locations.</param>
    /// <param name="gardenSpotRepository">The repository used to manage garden spots.</param>
    public LocationService(IRepository<Location> locationRepository, IRepository<GardenSpot> gardenSpotRepository)
    {
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _gardenSpotRepository = gardenSpotRepository ?? throw new ArgumentNullException(nameof(gardenSpotRepository));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public Task<IPagedList<Location>> GetLocationsAsync(Guid ownerId, int pageIndex = 0, int pageSize = int.MaxValue, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);

        return _locationRepository.GetPagedAsync(
            query => query
                .Where(location => location.OwnerId == ownerId)
                .OrderBy(location => location.Name)
                .ThenBy(location => location.Id),
            pageIndex,
            pageSize,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Location?> GetLocationByIdAsync(int locationId, Guid ownerId, bool includeGardenSpots = false, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);

        var location = await _locationRepository.FirstOrDefaultAsync(
            entity => entity.Id == locationId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        if (location is null || !includeGardenSpots)
            return location;

        location.GardenSpots = (await _gardenSpotRepository.GetAllAsync(
            query => query
                .Where(spot => spot.LocationId == locationId && spot.OwnerId == ownerId)
                .OrderBy(spot => spot.Name)
                .ThenBy(spot => spot.Id),
            cancellationToken: cancellationToken)).ToList();

        return location;
    }

    /// <inheritdoc />
    public async Task<Location> CreateLocationAsync(Location location, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(location);
        ValidateOwnerId(location.OwnerId);
        ValidateRequired(location.Name, nameof(location.Name));
        ValidateRequired(location.City, nameof(location.City));

        StampForCreate(location);
        return await _locationRepository.CreateAsync(location, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Location> UpdateLocationAsync(Location location, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(location);
        ValidateOwnerId(location.OwnerId);
        ValidateRequired(location.Name, nameof(location.Name));
        ValidateRequired(location.City, nameof(location.City));

        var existingLocation = await EnsureLocationOwnedAsync(location.Id, location.OwnerId, cancellationToken);
        existingLocation.Name = location.Name.Trim();
        existingLocation.City = location.City.Trim();
        existingLocation.Country = string.IsNullOrWhiteSpace(location.Country) ? "Greece" : location.Country.Trim();
        existingLocation.Latitude = location.Latitude;
        existingLocation.Longitude = location.Longitude;
        existingLocation.ClimateZone = location.ClimateZone?.Trim();
        existingLocation.Notes = location.Notes?.Trim();
        StampForUpdate(existingLocation);

        return await _locationRepository.UpdateAsync(existingLocation, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteLocationAsync(int locationId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var location = await EnsureLocationOwnedAsync(locationId, ownerId, cancellationToken);
        await _locationRepository.DeleteAsync(location, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GardenSpot>> GetGardenSpotsAsync(int locationId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsureLocationOwnedAsync(locationId, ownerId, cancellationToken);

        return await _gardenSpotRepository.GetAllAsync(
            query => query
                .Where(spot => spot.LocationId == locationId && spot.OwnerId == ownerId)
                .OrderBy(spot => spot.Name)
                .ThenBy(spot => spot.Id),
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GardenSpot> AddGardenSpotAsync(int locationId, GardenSpot gardenSpot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(gardenSpot);
        ValidateRequired(gardenSpot.Name, nameof(gardenSpot.Name));

        var location = await EnsureLocationOwnedAsync(locationId, gardenSpot.OwnerId, cancellationToken);
        await EnsureGardenSpotNameIsUniqueAsync(locationId, gardenSpot.Name, null, cancellationToken);

        gardenSpot.LocationId = location.Id;
        gardenSpot.OwnerId = location.OwnerId;
        gardenSpot.Name = gardenSpot.Name.Trim();
        gardenSpot.Surroundings = gardenSpot.Surroundings?.Trim();
        gardenSpot.Notes = gardenSpot.Notes?.Trim();
        StampForCreate(gardenSpot);

        return await _gardenSpotRepository.CreateAsync(gardenSpot, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GardenSpot> UpdateGardenSpotAsync(int locationId, GardenSpot gardenSpot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(gardenSpot);
        ValidateRequired(gardenSpot.Name, nameof(gardenSpot.Name));

        var location = await EnsureLocationOwnedAsync(locationId, gardenSpot.OwnerId, cancellationToken);
        var existingGardenSpot = await EnsureGardenSpotOwnedAsync(location.Id, gardenSpot.Id, location.OwnerId, cancellationToken);
        await EnsureGardenSpotNameIsUniqueAsync(location.Id, gardenSpot.Name, gardenSpot.Id, cancellationToken);

        existingGardenSpot.Name = gardenSpot.Name.Trim();
        existingGardenSpot.Orientation = gardenSpot.Orientation;
        existingGardenSpot.CoverType = gardenSpot.CoverType;
        existingGardenSpot.SunHoursPerDay = gardenSpot.SunHoursPerDay;
        existingGardenSpot.Surroundings = gardenSpot.Surroundings?.Trim();
        existingGardenSpot.Notes = gardenSpot.Notes?.Trim();
        existingGardenSpot.PictureId = gardenSpot.PictureId;
        StampForUpdate(existingGardenSpot);

        return await _gardenSpotRepository.UpdateAsync(existingGardenSpot, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteGardenSpotAsync(int locationId, int gardenSpotId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsureLocationOwnedAsync(locationId, ownerId, cancellationToken);
        var gardenSpot = await EnsureGardenSpotOwnedAsync(locationId, gardenSpotId, ownerId, cancellationToken);
        await _gardenSpotRepository.DeleteAsync(gardenSpot, cancellationToken: cancellationToken);
    }

    #endregion

    #region Privates

    private async Task<Location> EnsureLocationOwnedAsync(int locationId, Guid ownerId, CancellationToken cancellationToken)
    {
        ValidateOwnerId(ownerId);

        var location = await _locationRepository.FirstOrDefaultAsync(
            entity => entity.Id == locationId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return location ?? throw new DomainException($"Location with id '{locationId}' was not found for owner '{ownerId}'.");
    }

    private async Task<GardenSpot> EnsureGardenSpotOwnedAsync(int locationId, int gardenSpotId, Guid ownerId, CancellationToken cancellationToken)
    {
        var gardenSpot = await _gardenSpotRepository.FirstOrDefaultAsync(
            entity => entity.Id == gardenSpotId && entity.LocationId == locationId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return gardenSpot ?? throw new DomainException($"Garden spot with id '{gardenSpotId}' was not found in location '{locationId}'.");
    }

    private async Task EnsureGardenSpotNameIsUniqueAsync(int locationId, string name, int? excludedGardenSpotId, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        var exists = await _gardenSpotRepository.AnyAsync(
            entity => entity.LocationId == locationId
                && entity.Name.ToLower() == normalizedName
                && (!excludedGardenSpotId.HasValue || entity.Id != excludedGardenSpotId.Value),
            cancellationToken: cancellationToken);

        if (exists)
            throw new DomainException($"A garden spot with the name '{name}' already exists in location '{locationId}'.");
    }

    private static void ValidateOwnerId(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
            throw new DomainException("OwnerId is required.");
    }

    private static void ValidateRequired(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{propertyName} is required.");
    }

    private static void StampForCreate(AuditableEntity entity)
    {
        var now = DateTime.UtcNow;
        entity.CreatedAtUtc = now;
        entity.UpdatedAtUtc = now;
        entity.IsDeleted = false;
        entity.DeletedAtUtc = null;
    }

    private static void StampForUpdate(AuditableEntity entity)
    {
        entity.UpdatedAtUtc = DateTime.UtcNow;
    }

    #endregion
}



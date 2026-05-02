using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Domain.Media;
using GropMng.Core.Interfaces.Services.Media;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GropMng.Web.Initialization.Seeders;

/// <summary>
/// Seeds one PlantPhoto per PlantInstance using the reference images from
/// _ReferenceFiles/2026-04-29/images/ (development seed data).
/// Images are named: 2026-04-29-{TempId}-{Name}.jpg
/// TempId maps 1:1 to plantInstanceIds by index (index 0 = TempId 1).
/// </summary>
internal sealed class PlantPhotoSeeder
{
    private static readonly Guid DemoOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateOnly TakenDate = new DateOnly(2026, 4, 29);

    private readonly GropContext _dbContext;
    private readonly IPictureService _pictureService;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<PlantPhotoSeeder> _logger;

    public PlantPhotoSeeder(
        GropContext dbContext,
        IPictureService pictureService,
        IHostEnvironment hostEnvironment,
        ILogger<PlantPhotoSeeder> logger)
    {
        _dbContext = dbContext;
        _pictureService = pictureService;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public async Task SeedAsync(IReadOnlyList<int> plantInstanceIds, CancellationToken cancellationToken = default)
    {
        // Idempotency: if all instances already have a photo, skip entirely
        var existing = await _dbContext.PlantPhotos
            .CountAsync(p => p.OwnerId == DemoOwnerBusinessId, cancellationToken);

        if (existing >= plantInstanceIds.Count)
            return;

        // Locate the reference images folder: solution root / _ReferenceFiles / 2026-04-29 / images
        // ContentRootPath = Presentation/GropMng.Web/ → go up 2 levels to reach solution root
        var imagesFolder = Path.GetFullPath(
            Path.Combine(_hostEnvironment.ContentRootPath, "..", "..", "_ReferenceFiles", "2026-04-29", "images"));

        if (!Directory.Exists(imagesFolder))
        {
            _logger.LogWarning(
                "PlantPhotoSeeder: seed images folder not found at '{Path}'. Skipping photo seeding.",
                imagesFolder);
            return;
        }

        var now = DateTime.UtcNow;
        var subfolder = $"images/plants/{DemoOwnerBusinessId}";

        for (var i = 0; i < plantInstanceIds.Count; i++)
        {
            var tempId = i + 1;
            var instanceId = plantInstanceIds[i];

            // Skip if this instance already has a photo (partial re-seed safety)
            var alreadyExists = await _dbContext.PlantPhotos
                .AnyAsync(p => p.OwnerId == DemoOwnerBusinessId && p.PlantInstanceId == instanceId, cancellationToken);

            if (alreadyExists)
                continue;

            // Find image file for this TempId: "2026-04-29-{tempId}-*"
            var pattern = $"2026-04-29-{tempId}-*";
            var files = Directory.GetFiles(imagesFolder, pattern);

            if (files.Length == 0)
            {
                _logger.LogWarning("PlantPhotoSeeder: no image found for TempId={TempId}. Skipping.", tempId);
                continue;
            }

            var imageFile = files[0];
            var seoFilename = Path.GetFileNameWithoutExtension(imageFile);

            byte[] bytes;
            try
            {
                bytes = await File.ReadAllBytesAsync(imageFile, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PlantPhotoSeeder: failed to read image for TempId={TempId}. Skipping.", tempId);
                continue;
            }

            // Persist via PictureService: validates, resizes, saves to disk + DB
            Picture picture;
            try
            {
                picture = await _pictureService.InsertPictureAsync(bytes, "image/jpeg", seoFilename, subfolder: subfolder);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PlantPhotoSeeder: failed to insert picture for TempId={TempId}. Skipping.", tempId);
                continue;
            }

            // Mark as not-new — seeded pictures are not orphan candidates
            picture.IsNew = false;
            await _pictureService.UpdatePictureAsync(picture);

            // Create the PlantPhoto record
            _dbContext.PlantPhotos.Add(new PlantPhoto
            {
                OwnerId = DemoOwnerBusinessId,
                PlantInstanceId = instanceId,
                PictureId = picture.Id,
                TakenDate = TakenDate,
                DisplayOrder = 0,
                Caption = null,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug(
                "PlantPhotoSeeder: seeded photo for TempId={TempId}, InstanceId={InstanceId}, PictureId={PictureId}",
                tempId, instanceId, picture.Id);
        }

        _logger.LogInformation("PlantPhotoSeeder: completed seeding plant photos.");
    }
}

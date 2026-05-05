using GropMng.Core.Interfaces.Services.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GropMng.Core.Interfaces.Services.User;

namespace GropMng.Web.Controllers;

/// <summary>
/// Handles async image uploads from owner-facing views (GardenSpot, PlantInstance, etc.).
/// Mirrors the Admin PictureController but requires only [Authorize] (owner access).
/// </summary>
[Authorize]
public class PictureController : Controller
{
    private readonly IPictureService _pictureService;
    private readonly ICurrentOwnerProvider _currentOwnerProvider;

    public PictureController(IPictureService pictureService, ICurrentOwnerProvider currentOwnerProvider)
    {
        _pictureService = pictureService;
        _currentOwnerProvider = currentOwnerProvider;
    }

    /// <summary>
    /// Accepts a single image file upload, saves it, and returns the new picture ID + preview URL.
    /// </summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AsyncUpload()
    {
        var file = Request.Form.Files.FirstOrDefault();
        if (file is null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded" });

        var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/gif", "image/webp"
        };

        if (!allowedTypes.Contains(file.ContentType))
            return Json(new { success = false, message = "Unsupported file type. Allowed: jpg, png, gif, webp." });

        const long maxFileSize = 10 * 1024 * 1024;
        if (file.Length > maxFileSize)
            return Json(new { success = false, message = "File too large. Maximum size is 10 MB." });

        try
        {
            var qqFileName = Request.Form.TryGetValue("qqfilename", out var qqVal)
                ? qqVal.ToString()
                : string.Empty;

            var seoName = await _pictureService.GetPictureSeNameAsync(
                Path.GetFileNameWithoutExtension(string.IsNullOrWhiteSpace(qqFileName) ? file.FileName : qqFileName));

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var binary = ms.ToArray();

            var validated = await _pictureService.ValidatePictureAsync(binary, file.ContentType, seoName);

            // Resolve the storage subfolder based on entity type + current owner
            var subfolder = await ResolveSubfolderAsync(Request.Form["entityType"].ToString());

            var picture = await _pictureService.InsertPictureAsync(validated, file.ContentType, seoName,
                subfolder: subfolder);

            var (previewUrl, _) = await _pictureService.GetPictureUrlAsync(picture, targetSize: 100);

            return Json(new
            {
                success   = true,
                pictureId = picture.Id,
                imageUrl  = previewUrl
            });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Maps a client-supplied entity type hint to a storage subfolder.
    /// The subfolder is constructed entirely server-side to prevent path injection.
    /// </summary>
    private async Task<string?> ResolveSubfolderAsync(string entityType)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync();
        return entityType?.ToLowerInvariant() switch
        {
            "gardenspot"    => $"images/GardenSpots/{ownerId:N}",
            "plantinstance" => $"images/Plants/{ownerId:N}",
            _               => null   // falls back to images/uploads
        };
    }
}

using GropMng.Core.Interfaces.Services.Media;
using GropMng.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Handles async image uploads from the Picture EditorTemplate.
/// The endpoint is consumed by the HTML5 file-picker widget embedded in Picture.cshtml.
/// </summary>
[Area("Admin")]
[AuthorizeAdmin]
public class PictureController : Controller
{
    private readonly IPictureService _pictureService;

    public PictureController(IPictureService pictureService)
    {
        _pictureService = pictureService;
    }

    /// <summary>
    /// Accepts a single image file upload, validates, resizes, saves to disk, and returns the new picture ID + preview URL.
    /// Called via Fetch API from Picture.cshtml.
    /// </summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AsyncUpload()
    {
        var file = Request.Form.Files.FirstOrDefault();
        if (file is null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded" });

        // Security: validate MIME type at server before passing to service
        var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/gif", "image/webp"
        };

        if (!allowedTypes.Contains(file.ContentType))
            return Json(new { success = false, message = "Unsupported file type. Allowed: jpg, png, gif, webp." });

        // Security: validate file size (max 10 MB before processing)
        const long maxFileSize = 10 * 1024 * 1024;
        if (file.Length > maxFileSize)
            return Json(new { success = false, message = "File too large. Maximum size is 10 MB." });

        try
        {
            // Use the qqfilename parameter if the client sends it (fine-uploader compat)
            var qqFileName = Request.Form.TryGetValue("qqfilename", out var qqVal)
                ? qqVal.ToString()
                : string.Empty;

            var seoName = await _pictureService.GetPictureSeNameAsync(
                Path.GetFileNameWithoutExtension(string.IsNullOrWhiteSpace(qqFileName) ? file.FileName : qqFileName));

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var binary = ms.ToArray();

            var validated = await _pictureService.ValidatePictureAsync(binary, file.ContentType, seoName);

            // Resolve storage subfolder based on the entity type passed from the upload widget
            var subfolder = ResolveSubfolder(Request.Form["entityType"].ToString());

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
    /// Maps an admin entity type hint to a storage subfolder.
    /// Constructed entirely server-side to prevent path injection.
    /// </summary>
    private static string? ResolveSubfolder(string entityType)
        => entityType?.ToLowerInvariant() switch
        {
            "plant" => "images/Plants",
            _       => null   // falls back to images/uploads
        };
}

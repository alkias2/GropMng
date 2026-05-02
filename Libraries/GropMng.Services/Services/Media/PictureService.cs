using GropMng.Core.Domain.Media;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Configuration;
using GropMng.Core.Interfaces.Services.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using SkiaSharp;
using System.Text.RegularExpressions;

namespace GropMng.Services.Services.Media;

/// <summary>
/// File system–backed implementation of <see cref="IPictureService"/>.
/// Images are stored under <c>wwwroot/images/</c>; thumbnails are cached on first request.
/// Uses SkiaSharp for resize and re-encode operations.
/// </summary>
public partial class PictureService : IPictureService
{
    // ── Allowed MIME types ──────────────────────────────────────────────────
    private static readonly HashSet<string> AllowedMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp"
    ];

    private static readonly Dictionary<string, string> MimeToExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = "jpg",
        ["image/png"]  = "png",
        ["image/gif"]  = "gif",
        ["image/webp"] = "webp"
    };

    private static readonly Dictionary<string, string> ExtensionToMime = new(StringComparer.OrdinalIgnoreCase)
    {
        ["jpg"]  = "image/jpeg",
        ["jpeg"] = "image/jpeg",
        ["png"]  = "image/png",
        ["gif"]  = "image/gif",
        ["webp"] = "image/webp"
    };

    // ── Dependencies ────────────────────────────────────────────────────────
    private readonly IRepository<Picture> _pictureRepository;
    private readonly ISettingService _settingService;
    private readonly IHostEnvironment _hostEnvironment;

    public PictureService(
        IRepository<Picture> pictureRepository,
        ISettingService settingService,
        IHostEnvironment hostEnvironment)
    {
        _pictureRepository = pictureRepository;
        _settingService = settingService;
        _hostEnvironment = hostEnvironment;
    }

    // ── URL / Path Helpers ──────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<string> GetPictureUrlAsync(int pictureId, int targetSize = 0,
        bool showDefaultPicture = true, PictureType defaultPictureType = PictureType.Entity)
    {
        if (pictureId <= 0)
            return showDefaultPicture ? await GetDefaultPictureUrlAsync(targetSize, defaultPictureType) : string.Empty;

        var picture = await GetPictureByIdAsync(pictureId);
        var (url, _) = await GetPictureUrlAsync(picture, targetSize, showDefaultPicture, defaultPictureType);
        return url;
    }

    /// <inheritdoc />
    public async Task<(string Url, Picture? Picture)> GetPictureUrlAsync(Picture? picture, int targetSize = 0,
        bool showDefaultPicture = true, PictureType defaultPictureType = PictureType.Entity)
    {
        if (picture is null)
        {
            var defaultUrl = showDefaultPicture
                ? await GetDefaultPictureUrlAsync(targetSize, defaultPictureType)
                : string.Empty;
            return (defaultUrl, null);
        }

        var url = targetSize > 0
            ? await GetOrCreateThumbnailUrlAsync(picture, targetSize)
            : BuildVirtualPath(picture);

        return (url, picture);
    }

    /// <inheritdoc />
    public async Task<string> GetDefaultPictureUrlAsync(int targetSize = 0,
        PictureType defaultPictureType = PictureType.Entity)
    {
        var settings = await _settingService.LoadAsync<MediaSettings>();
        return settings.DefaultPlantImageUrl;
    }

    /// <inheritdoc />
    public Task<string> GetPictureSeNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Task.FromResult(string.Empty);

        var seName = name.ToLowerInvariant().Trim();
        seName = NonAlphanumericRegex().Replace(seName, "-");
        seName = MultiDashRegex().Replace(seName, "-").Trim('-');
        return Task.FromResult(seName);
    }

    /// <inheritdoc />
    public Task<string> GetFileExtensionFromMimeTypeAsync(string mimeType)
    {
        if (MimeToExtension.TryGetValue(mimeType, out var ext))
            return Task.FromResult(ext);
        return Task.FromResult("jpg");
    }

    /// <inheritdoc />
    public string GetPictureContentTypeByFileExtension(string fileExtension)
    {
        var clean = fileExtension.TrimStart('.').ToLowerInvariant();
        return ExtensionToMime.GetValueOrDefault(clean, "image/jpeg");
    }

    // ── CRUD ────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<Picture?> GetPictureByIdAsync(int pictureId)
    {
        if (pictureId <= 0)
            return null;
        return await _pictureRepository.GetByIdAsync(pictureId);
    }

    /// <summary>
    /// Validates, resizes, and persists a picture uploaded via a web form.
    /// </summary>
    public async Task<Picture> InsertPictureAsync(IFormFile formFile, string defaultFileName = "")
    {
        ArgumentNullException.ThrowIfNull(formFile);

        if (!AllowedMimeTypes.Contains(formFile.ContentType.ToLowerInvariant()))
            throw new InvalidOperationException($"Unsupported image type: {formFile.ContentType}");

        using var ms = new MemoryStream();
        await formFile.CopyToAsync(ms);
        var binary = ms.ToArray();

        var fileName = string.IsNullOrWhiteSpace(formFile.FileName) ? defaultFileName : formFile.FileName;
        var seName = await GetPictureSeNameAsync(Path.GetFileNameWithoutExtension(fileName));

        var validated = await ValidatePictureAsync(binary, formFile.ContentType, fileName);
        return await PersistPictureAsync(validated, formFile.ContentType, seName, subfolder: null);
    }

    /// <inheritdoc />
    public async Task<Picture> InsertPictureAsync(byte[] pictureBinary, string mimeType, string seoFilename,
        string? altAttribute = null, string? titleAttribute = null, string? subfolder = null)
    {
        ArgumentNullException.ThrowIfNull(pictureBinary);

        if (!AllowedMimeTypes.Contains(mimeType.ToLowerInvariant()))
            throw new InvalidOperationException($"Unsupported image type: {mimeType}");

        var validated = await ValidatePictureAsync(pictureBinary, mimeType, seoFilename);
        var picture = await PersistPictureAsync(validated, mimeType, seoFilename, subfolder: subfolder);

        picture.AltAttribute   = altAttribute;
        picture.TitleAttribute = titleAttribute;
        return await _pictureRepository.UpdateAsync(picture);
    }

    /// <inheritdoc />
    public async Task<Picture> UpdatePictureAsync(Picture picture)
    {
        ArgumentNullException.ThrowIfNull(picture);
        return await _pictureRepository.UpdateAsync(picture);
    }

    /// <inheritdoc />
    public async Task DeletePictureAsync(Picture picture)
    {
        ArgumentNullException.ThrowIfNull(picture);

        // Delete original file
        DeleteFileIfExists(MapVirtualToPhysical(picture.VirtualPath));

        // Delete all cached thumbnails that match this picture
        DeleteThumbnailsForPicture(picture);

        await _pictureRepository.DeleteAsync(picture, softDelete: false);
    }

    // ── Validation & Processing ─────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<byte[]> ValidatePictureAsync(byte[] pictureBinary, string mimeType, string fileName)
    {
        ArgumentNullException.ThrowIfNull(pictureBinary);

        var settings = await _settingService.LoadAsync<MediaSettings>();
        return ResizeAndEncode(pictureBinary, mimeType, settings.MaximumImageSize, settings.DefaultImageQuality);
    }

    /// <inheritdoc />
    public async Task<byte[]> LoadPictureBinaryAsync(Picture picture)
    {
        ArgumentNullException.ThrowIfNull(picture);

        var path = MapVirtualToPhysical(picture.VirtualPath);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return [];

        return await File.ReadAllBytesAsync(path);
    }

    /// <inheritdoc />
    public async Task<Picture?> SetSeoFilenameAsync(int pictureId, string seoFilename)
    {
        var picture = await GetPictureByIdAsync(pictureId);
        if (picture is null)
            return null;

        picture.SeoFilename = seoFilename ?? string.Empty;
        return await _pictureRepository.UpdateAsync(picture);
    }

    // ── Private Helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Saves the validated binary to disk under <c>wwwroot/images/uploads/</c> and creates the DB record.
    /// </summary>
    private async Task<Picture> PersistPictureAsync(byte[] binary, string mimeType, string seoFilename,
        string? subfolder = null)
    {
        var ext = await GetFileExtensionFromMimeTypeAsync(mimeType);
        var fileName = $"{Guid.NewGuid():N}-{SanitizeFileName(seoFilename)}.{ext}";
        var relativeFolder = string.IsNullOrWhiteSpace(subfolder)
            ? Path.Combine("images", "uploads")
            : NormalizeSubfolder(subfolder);
        var physicalFolder = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", relativeFolder);

        Directory.CreateDirectory(physicalFolder);
        var physicalPath = Path.Combine(physicalFolder, fileName);
        await File.WriteAllBytesAsync(physicalPath, binary);

        var virtualPath = $"/{relativeFolder.Replace('\\', '/')}/{fileName}";

        var picture = new Picture
        {
            MimeType   = mimeType,
            SeoFilename = seoFilename,
            VirtualPath = virtualPath,
            IsNew      = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        return await _pictureRepository.CreateAsync(picture);
    }

    /// <summary>
    /// Returns the thumbnail URL, creating the thumbnail file on first request.
    /// Thumbnails are stored in a <c>thumbs/</c> subfolder co-located with the original image.
    /// </summary>
    private async Task<string> GetOrCreateThumbnailUrlAsync(Picture picture, int targetSize)
    {
        var ext = await GetFileExtensionFromMimeTypeAsync(picture.MimeType);
        var thumbFolder = GetThumbFolderForPicture(picture);
        var thumbFileName = $"{picture.Id}_{targetSize}.{ext}";
        var physicalThumbFolder = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", thumbFolder);
        var physicalThumbPath = Path.Combine(physicalThumbFolder, thumbFileName);

        if (!File.Exists(physicalThumbPath))
        {
            var original = await LoadPictureBinaryAsync(picture);
            if (original.Length == 0)
                return (await GetDefaultPictureUrlAsync()).ToString();

            var thumbBinary = ResizeAndEncode(original, picture.MimeType, targetSize, 90);
            Directory.CreateDirectory(physicalThumbFolder);
            await File.WriteAllBytesAsync(physicalThumbPath, thumbBinary);
        }

        return $"/{thumbFolder.Replace('\\', '/')}/{thumbFileName}";
    }

    /// <summary>
    /// Derives the thumbnails folder path (OS-relative, no leading separator) from the picture's VirtualPath.
    /// The thumb folder is the same directory as the original file, with a <c>thumbs</c> segment appended.
    /// Examples:
    ///   /images/GardenSpots/{ownerId}/file.jpg  →  images\GardenSpots\{ownerId}\thumbs
    ///   /images/Plants/{ownerId}/file.jpg        →  images\Plants\{ownerId}\thumbs
    ///   /images/Plants/file.jpg                  →  images\Plants\thumbs
    ///   /images/uploads/file.jpg                 →  images\thumbs  (legacy fallback)
    /// </summary>
    private static string GetThumbFolderForPicture(Picture picture)
    {
        if (string.IsNullOrEmpty(picture.VirtualPath))
            return Path.Combine("images", "thumbs");

        var virtualDir = Path.GetDirectoryName(picture.VirtualPath.TrimStart('/'));
        if (string.IsNullOrEmpty(virtualDir))
            return Path.Combine("images", "thumbs");

        // Normalise to OS directory separator then append thumbs
        return virtualDir.Replace('/', Path.DirectorySeparatorChar)
               + Path.DirectorySeparatorChar + "thumbs";
    }

    /// <summary>
    /// Resizes (if needed) and re-encodes the image binary. Returns the result as a byte array.
    /// </summary>
    private static byte[] ResizeAndEncode(byte[] binary, string mimeType, int maxSize, int quality)
    {
        using var inputStream = new SKMemoryStream(binary);
        using var original = SKBitmap.Decode(inputStream);

        if (original is null)
            return binary;

        // Determine new dimensions
        int newWidth  = original.Width;
        int newHeight = original.Height;

        if (maxSize > 0 && (original.Width > maxSize || original.Height > maxSize))
        {
            var ratio = Math.Min((double)maxSize / original.Width, (double)maxSize / original.Height);
            newWidth  = (int)(original.Width  * ratio);
            newHeight = (int)(original.Height * ratio);
        }

        SKBitmap bitmapToEncode;
        if (newWidth != original.Width || newHeight != original.Height)
        {
            bitmapToEncode = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
        }
        else
        {
            bitmapToEncode = original;
        }

        try
        {
            var format = mimeType.ToLowerInvariant() switch
            {
                "image/png"  => SKEncodedImageFormat.Png,
                "image/gif"  => SKEncodedImageFormat.Gif,
                "image/webp" => SKEncodedImageFormat.Webp,
                _            => SKEncodedImageFormat.Jpeg
            };

            using var image = SKImage.FromBitmap(bitmapToEncode);
            using var data  = image.Encode(format, quality);
            return data.ToArray();
        }
        finally
        {
            if (!ReferenceEquals(bitmapToEncode, original))
                bitmapToEncode.Dispose();
        }
    }

    private string BuildVirtualPath(Picture picture)
        => picture.VirtualPath ?? string.Empty;

    private string? MapVirtualToPhysical(string? virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath))
            return null;

        var relative = virtualPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", relative);
    }

    private void DeleteThumbnailsForPicture(Picture picture)
    {
        var relativeThumbFolder = GetThumbFolderForPicture(picture);
        var thumbFolder = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", relativeThumbFolder);
        if (!Directory.Exists(thumbFolder))
            return;

        foreach (var file in Directory.GetFiles(thumbFolder, $"{picture.Id}_*"))
            DeleteFileIfExists(file);
    }

    private static void DeleteFileIfExists(string? path)
    {
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
            File.Delete(path);
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "picture";

        var safe = NonAlphanumericRegex().Replace(name.ToLowerInvariant(), "-");
        safe = MultiDashRegex().Replace(safe, "-").Trim('-');
        return safe.Length > 50 ? safe[..50] : safe;
    }

    /// <summary>
    /// Normalizes a caller-supplied subfolder to a safe OS path, preventing path traversal.
    /// Only alphanumeric characters, hyphens, underscores, and forward slashes are allowed.
    /// </summary>
    private static string NormalizeSubfolder(string subfolder)
    {
        // Strip any dangerous sequences
        var clean = subfolder
            .Replace('\\', '/')
            .Replace("..", string.Empty)
            .Trim('/');

        // Allow only safe characters: letters, digits, hyphens, underscores, forward slashes
        clean = System.Text.RegularExpressions.Regex.Replace(clean, @"[^a-zA-Z0-9\-_/]", string.Empty);
        clean = System.Text.RegularExpressions.Regex.Replace(clean, @"/{2,}", "/");
        clean = clean.Trim('/');

        return string.IsNullOrEmpty(clean)
            ? Path.Combine("images", "uploads")
            : clean.Replace('/', Path.DirectorySeparatorChar);
    }

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultiDashRegex();
}

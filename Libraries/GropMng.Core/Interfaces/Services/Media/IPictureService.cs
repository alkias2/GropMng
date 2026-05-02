using GropMng.Core.Domain.Media;

namespace GropMng.Core.Interfaces.Services.Media;

public partial interface IPictureService
{
    // ── URL / Path helpers ────────────────────────────────────────────────

    /// <summary>Returns the public URL for a picture by id. Generates a thumbnail when targetSize > 0.</summary>
    Task<string> GetPictureUrlAsync(int pictureId, int targetSize = 0,
        bool showDefaultPicture = true, PictureType defaultPictureType = PictureType.Entity);

    /// <summary>Returns the public URL and the loaded Picture object.</summary>
    Task<(string Url, Picture? Picture)> GetPictureUrlAsync(Picture? picture, int targetSize = 0,
        bool showDefaultPicture = true, PictureType defaultPictureType = PictureType.Entity);

    /// <summary>Returns the URL of the default (no-image) picture.</summary>
    Task<string> GetDefaultPictureUrlAsync(int targetSize = 0,
        PictureType defaultPictureType = PictureType.Entity);

    /// <summary>Converts an arbitrary name into a URL-safe SEO filename.</summary>
    Task<string> GetPictureSeNameAsync(string name);

    /// <summary>Returns the file extension for the given MIME type (e.g. "image/jpeg" → "jpg").</summary>
    Task<string> GetFileExtensionFromMimeTypeAsync(string mimeType);

    /// <summary>Returns the content-type string for a given file extension.</summary>
    string GetPictureContentTypeByFileExtension(string fileExtension);

    // ── CRUD ──────────────────────────────────────────────────────────────

    Task<Picture?> GetPictureByIdAsync(int pictureId);

    /// <summary>Validates, resizes, and persists a picture from raw bytes.</summary>
    /// <summary>Validates, resizes, and persists a picture from raw bytes.</summary>
    /// <param name="subfolder">Optional subfolder relative to <c>wwwroot/</c>, e.g. <c>images/gardenspots/owner-guid</c>.
    /// When null, defaults to <c>images/uploads</c>.</param>
    Task<Picture> InsertPictureAsync(byte[] pictureBinary, string mimeType, string seoFilename,
        string? altAttribute = null, string? titleAttribute = null, string? subfolder = null);

    Task<Picture> UpdatePictureAsync(Picture picture);

    /// <summary>Deletes a picture record and its associated files (original + all cached thumbnails).</summary>
    Task DeletePictureAsync(Picture picture);

    // ── Validation & Processing ────────────────────────────────────────────

    /// <summary>Validates the image binary: resizes if larger than MaximumImageSize, re-encodes at DefaultImageQuality.</summary>
    Task<byte[]> ValidatePictureAsync(byte[] pictureBinary, string mimeType, string fileName);

    /// <summary>Reads the raw bytes of the picture from the file system.</summary>
    Task<byte[]> LoadPictureBinaryAsync(Picture picture);

    /// <summary>Updates the SEO filename for an existing picture.</summary>
    Task<Picture?> SetSeoFilenameAsync(int pictureId, string seoFilename);
}

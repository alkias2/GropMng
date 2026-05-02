namespace GropMng.Core.Domain.Media;

public partial class Picture : BaseEntity
{
    public string MimeType { get; set; } = string.Empty;

    public string SeoFilename { get; set; } = string.Empty;

    public string? AltAttribute { get; set; }

    public string? TitleAttribute { get; set; }

    public bool IsNew { get; set; } = true;

    public string? VirtualPath { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

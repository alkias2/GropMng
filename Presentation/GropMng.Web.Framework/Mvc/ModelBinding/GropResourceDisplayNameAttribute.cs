namespace GropMng.Web.Framework.Mvc.ModelBinding;

/// <summary>
/// Declares a localization resource key used as display name for a model property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class GropResourceDisplayNameAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GropResourceDisplayNameAttribute"/> class.
    /// </summary>
    /// <param name="resourceKey">The localization resource key.</param>
    public GropResourceDisplayNameAttribute(string resourceKey)
    {
        ResourceKey = resourceKey;
    }

    /// <summary>
    /// Gets the localization resource key.
    /// </summary>
    public string ResourceKey { get; }
}

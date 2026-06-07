using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GropMng.Web.Infrastructure.ModelBinding;

/// <summary>
/// Binds decimal values while accepting both comma and dot as decimal separators.
/// </summary>
public sealed class FlexibleDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
            throw new ArgumentNullException(nameof(bindingContext));

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
        var rawValue = valueProviderResult.FirstValue;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            if (bindingContext.ModelMetadata.IsReferenceOrNullableType)
                bindingContext.Result = ModelBindingResult.Success(null);

            return Task.CompletedTask;
        }

        var normalized = Normalize(rawValue);
        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            bindingContext.Result = ModelBindingResult.Success(parsed);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"The value '{rawValue}' is not valid.");
        return Task.CompletedTask;
    }

    private static string Normalize(string value)
    {
        var normalized = value.Trim()
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("\u00A0", string.Empty, StringComparison.Ordinal);

        var lastComma = normalized.LastIndexOf(',');
        var lastDot = normalized.LastIndexOf('.');

        if (lastComma >= 0 && lastDot >= 0)
        {
            var decimalSeparator = lastComma > lastDot ? ',' : '.';
            var thousandsSeparator = decimalSeparator == ',' ? "." : ",";

            normalized = normalized.Replace(thousandsSeparator, string.Empty, StringComparison.Ordinal);
            normalized = normalized.Replace(decimalSeparator, '.');
        }
        else
        {
            normalized = normalized.Replace(',', '.');
        }

        if (normalized.StartsWith(".", StringComparison.Ordinal))
            normalized = $"0{normalized}";

        if (normalized.StartsWith("-.", StringComparison.Ordinal))
            normalized = normalized.Replace("-.", "-0.", StringComparison.Ordinal);

        return normalized;
    }
}

/// <summary>
/// Provides <see cref="FlexibleDecimalModelBinder" /> for decimal model types.
/// </summary>
public sealed class FlexibleDecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var modelType = context.Metadata.UnderlyingOrModelType;
        return modelType == typeof(decimal) ? new FlexibleDecimalModelBinder() : null;
    }
}
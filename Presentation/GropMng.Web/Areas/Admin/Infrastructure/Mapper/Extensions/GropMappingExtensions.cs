using AutoMapper;
using GropMng.Core;

namespace GropMng.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

/// <summary>
/// Mapping helper extensions wrapping AutoMapper in explicit Grop conventions.
/// </summary>
public static class GropMappingExtensions
{
    public static TModel ToModel<TModel>(this BaseEntity entity, IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(mapper);

        return mapper.Map<TModel>(entity);
    }

    public static TModel ToModel<TEntity, TModel>(this TEntity entity, TModel model, IMapper mapper)
        where TEntity : BaseEntity
        where TModel : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(mapper);

        return mapper.Map(entity, model);
    }

    public static TEntity ToEntity<TEntity>(this object model, IMapper mapper)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(mapper);

        return mapper.Map<TEntity>(model);
    }

    public static TEntity ToEntity<TEntity>(this object model, TEntity entity, IMapper mapper)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(mapper);

        return mapper.Map(model, entity);
    }
}

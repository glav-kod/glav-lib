using FluentNHibernate.Mapping;
using GlavLib.Abstractions.DataTypes;
using GlavLib.Db.NhUserTypes;
using JetBrains.Annotations;
using NHibernate;

namespace GlavLib.Db.Extensions;

[PublicAPI]
public static class NhExtensions
{
    public static IdentityPart EnumObjectType<TEnumObject>(this IdentityPart identityPart)
        where TEnumObject : class, IEnumObject<TEnumObject>
    {
        return identityPart.CustomType<EnumObjectUserType<TEnumObject>>();
    }

    public static PropertyPart EnumObjectType<TEnumObject>(this PropertyPart identityPart)
        where TEnumObject : class, IEnumObject<TEnumObject>
    {
        return identityPart.CustomType<EnumObjectUserType<TEnumObject>>();
    }

    public static async Task SaveAndFlushAsync(this ISession session, object entity, CancellationToken cancellationToken)
    {
        await session.SaveAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
    }
}
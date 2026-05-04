using Domain.Constants;
using System.Collections.Concurrent;
using System.Reflection;

namespace Application.Common.Helper
{
    public static class AuditColumnMapper
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _cache = new();

        public static void Apply<TSource, TDest>(TSource source, TDest dest, AuditColumn columns) where TSource : class
            where TDest : class
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(dest);
            if (columns == AuditColumn.None)
            {
                return;
            }
            var sourceProps = GetAuditProperties(typeof(TSource));
            var destProps = GetAuditProperties(typeof(TDest));
            ApplyInternal(source, dest, columns, sourceProps, destProps);
        }

        public static void Apply<TSource, TDest>(List<TSource> sources, List<TDest> dests, AuditColumn columns)
            where TSource : class
            where TDest : class
        {
            ArgumentNullException.ThrowIfNull(sources);
            ArgumentNullException.ThrowIfNull(dests);
            if (columns == AuditColumn.None || sources.Count == 0)
            {
                return;
            }
            var sourceProps = GetAuditProperties(typeof(TSource));
            var destProps = GetAuditProperties(typeof(TDest));
            var count = Math.Min(sources.Count, dests.Count);
            for (var i = 0; i < count; i++)
            {
                ApplyInternal(sources[i], dests[i], columns, sourceProps, destProps);
            }
        }

        private static void ApplyInternal<TSource, TDest>(
            TSource source,
            TDest dest,
            AuditColumn columns,
            Dictionary<string, PropertyInfo> sourceProps,
            Dictionary<string, PropertyInfo> destProps)
        {
            if ((columns & AuditColumn.CreatedAt) != 0 &&
                sourceProps.TryGetValue(nameof(AuditColumn.CreatedAt), out var sCreated) &&
                destProps.TryGetValue(nameof(AuditColumn.CreatedAt), out var dCreated))
            {
                dCreated.SetValue(dest, sCreated.GetValue(source));
            }
            if ((columns & AuditColumn.UpdatedAt) != 0 &&
                sourceProps.TryGetValue(nameof(AuditColumn.UpdatedAt), out var sUpdated) &&
                destProps.TryGetValue(nameof(AuditColumn.UpdatedAt), out var dUpdated))
            {
                dUpdated.SetValue(dest, sUpdated.GetValue(source));
            }
            if ((columns & AuditColumn.DeletedAt) != 0 &&
                sourceProps.TryGetValue(nameof(AuditColumn.DeletedAt), out var sDeleted) &&
                destProps.TryGetValue(nameof(AuditColumn.DeletedAt), out var dDeleted))
            {
                dDeleted.SetValue(dest, sDeleted.GetValue(source));
            }
        }

        private static Dictionary<string, PropertyInfo> GetAuditProperties(Type type)
        {
            return _cache.GetOrAdd(
                type,
                t =>
                {
                    var auditNames = new HashSet<string> { "CreatedAt", "UpdatedAt", "DeletedAt" };
                    return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => auditNames.Contains(p.Name) && p.PropertyType == typeof(DateTimeOffset?))
                        .ToDictionary(p => p.Name);
                });
        }
    }
}

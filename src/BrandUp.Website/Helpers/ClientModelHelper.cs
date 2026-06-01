using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace BrandUp.Website.Helpers
{
    public static class ClientModelHelper
    {
        readonly static ConcurrentDictionary<Type, Lazy<List<ClientProperty>>> types = new();

        public static void CopyProperties(object sourceModel, IDictionary<string, object> destinationData)
        {
            ArgumentNullException.ThrowIfNull(sourceModel);
            ArgumentNullException.ThrowIfNull(destinationData);

            var clientProperties = types.GetOrAdd(sourceModel.GetType(), static t => new Lazy<List<ClientProperty>>(() => GetClientProperties(t))).Value;
            foreach (var clientProperty in clientProperties)
            {
                var value = clientProperty.Getter(sourceModel);
                if (value is Enum)
                    value = value.ToString();

                destinationData.Add(clientProperty.ClientName, value!);
            }
        }

        static List<ClientProperty> GetClientProperties(Type model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var clientProperties = new List<ClientProperty>();
            var sourceByClientName = new Dictionary<string, string>(StringComparer.Ordinal);

            var modelProperties = model.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var propertyInfo in modelProperties)
            {
                var attr = propertyInfo.GetCustomAttribute<ClientPropertyAttribute>();
                if (attr == null)
                    continue;

                var name = propertyInfo.Name;
                if (!string.IsNullOrEmpty(attr.Name))
                    name = attr.Name;

                var clientName = NormalizeName(name);
                if (!sourceByClientName.TryAdd(clientName, propertyInfo.Name))
                    throw new InvalidOperationException($"Client property name '{clientName}' on type '{model.FullName}' is defined by both '{sourceByClientName[clientName]}' and '{propertyInfo.Name}'.");

                clientProperties.Add(new ClientProperty
                {
                    Getter = BuildGetter(propertyInfo),
                    ClientName = clientName
                });
            }

            return clientProperties;
        }

        static Func<object, object?> BuildGetter(PropertyInfo property)
        {
            // Для доступных публичных свойств компилируем делегат (быстрее рефлексии),
            // иначе (приватные/internal-типы) откатываемся на PropertyInfo.GetValue.
            if (property.GetMethod is { IsPublic: true } && IsAccessible(property.DeclaringType))
            {
                var instance = Expression.Parameter(typeof(object), "instance");
                var body = Expression.Convert(Expression.Property(Expression.Convert(instance, property.DeclaringType!), property), typeof(object));
                return Expression.Lambda<Func<object, object?>>(body, instance).Compile();
            }

            return property.GetValue;
        }

        static bool IsAccessible(Type? type)
        {
            for (var current = type; current != null; current = current.DeclaringType)
            {
                if (current.IsNested ? !current.IsNestedPublic : !current.IsPublic)
                    return false;
            }

            return true;
        }

        static string NormalizeName(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            return value[..1].ToLowerInvariant() + value[1..];
        }

        class ClientProperty
        {
            public required Func<object, object?> Getter { get; init; }
            public required string ClientName { get; init; }
        }
    }
}
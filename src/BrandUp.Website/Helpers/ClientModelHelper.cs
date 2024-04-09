using System.Collections.Concurrent;
using System.Reflection;

namespace BrandUp.Website.Helpers
{
    public static class ClientModelHelper
    {
        readonly static ConcurrentDictionary<Type, List<ClientProperty>> types = new();

        public static void CopyProperties(object sourceModel, IDictionary<string, object> destinationData)
        {
            ArgumentNullException.ThrowIfNull(sourceModel);
            ArgumentNullException.ThrowIfNull(destinationData);

            var clientProperties = types.GetOrAdd(sourceModel.GetType(), GetClientProperties);
            foreach (var clientProperty in clientProperties)
            {
                var value = clientProperty.ModelProperty.GetValue(sourceModel);
                if (value is Enum)
                    value = value.ToString();

                destinationData.Add(clientProperty.ClientName, value);
            }
        }

        static List<ClientProperty> GetClientProperties(Type model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var clientProperties = new List<ClientProperty>();

            var modelProperties = model.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty);
            foreach (var propertyInfo in modelProperties)
            {
                var attr = propertyInfo.GetCustomAttribute<ClientPropertyAttribute>();
                if (attr == null)
                    continue;

                var name = propertyInfo.Name;
                if (!string.IsNullOrEmpty(attr.Name))
                    name = attr.Name;

                clientProperties.Add(new ClientProperty
                {
                    ModelProperty = propertyInfo,
                    ClientName = NormalizeName(name)
                });
            }

            return clientProperties;
        }

        static string NormalizeName(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            return value[..1].ToLower() + value[1..];
        }

        class ClientProperty
        {
            public PropertyInfo ModelProperty { get; set; }
            public string ClientName { get; set; }
        }
    }
}
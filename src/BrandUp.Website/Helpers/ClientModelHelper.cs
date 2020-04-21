using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Website.Helpers
{
    public static class ClientModelHelper
    {
        readonly static ConcurrentDictionary<Type, List<ClientProperty>> types = new ConcurrentDictionary<Type, List<ClientProperty>>();

        public static void CopyProperties(object sourceModel, IDictionary<string, object> destinationData)
        {
            if (sourceModel == null)
                throw new ArgumentNullException(nameof(sourceModel));
            if (destinationData == null)
                throw new ArgumentNullException(nameof(destinationData));

            var clientProperties = types.GetOrAdd(sourceModel.GetType(), GetClientProperties);
            foreach (var clientProperty in clientProperties)
            {
                var value = clientProperty.ModelProperty.GetValue(sourceModel);
                if (value is Enum)
                    value = value.ToString();

                destinationData.Add(clientProperty.ClientName, value);
            }
        }
        private static List<ClientProperty> GetClientProperties(Type model)
        {
            var clientProperties = new List<ClientProperty>();

            var modelProperties = model.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty);
            foreach (var p in modelProperties)
            {
                var attr = p.GetCustomAttribute<Pages.ClientModelAttribute>();
                if (attr == null)
                    continue;

                var name = p.Name;
                if (!string.IsNullOrEmpty(attr.Name))
                    name = attr.Name;

                name = name.Substring(0, 1).ToLower() + name.Substring(1);

                clientProperties.Add(new ClientProperty
                {
                    ModelProperty = p,
                    ClientName = name
                });
            }

            return clientProperties;
        }

        class ClientProperty
        {
            public PropertyInfo ModelProperty { get; set; }
            public string ClientName { get; set; }
        }
    }
}
using System.Collections.Generic;
using Xunit;

namespace BrandUp.Website.Helpers
{
    public class ClientModelHelperTest
    {
        [Fact]
        public void CheckProperty()
        {
            var model = new TestClientModel { Count = 1 };
            var properties = new Dictionary<string, object>();
            var propertyName = "count";

            ClientModelHelper.CopyProperties(model, properties);

            Assert.True(properties.ContainsKey(propertyName));
            Assert.Equal(1, properties[propertyName]);
        }

        class TestClientModel
        {
            [ClientProperty]
            public int Count { get; set; }
        }
    }
}
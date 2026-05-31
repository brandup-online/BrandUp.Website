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

        [Fact]
        public void OnlyAnnotatedProperties()
        {
            var model = new TestClientModel();
            var properties = new Dictionary<string, object>();

            ClientModelHelper.CopyProperties(model, properties);

            Assert.False(properties.ContainsKey("ignored"));
        }

        [Fact]
        public void Enum_IsConvertedToString()
        {
            var model = new TestClientModel { Color = TestColor.Green };
            var properties = new Dictionary<string, object>();

            ClientModelHelper.CopyProperties(model, properties);

            Assert.Equal("Green", properties["color"]);
        }

        [Fact]
        public void CustomName_IsUsedAndCamelCased()
        {
            var model = new TestClientModel();
            var properties = new Dictionary<string, object>();

            ClientModelHelper.CopyProperties(model, properties);

            Assert.True(properties.ContainsKey("customName"));
            Assert.False(properties.ContainsKey("renamed"));
        }

        [Fact]
        public void NullValue_IsCopied()
        {
            var model = new TestClientModel { Text = null };
            var properties = new Dictionary<string, object>();

            ClientModelHelper.CopyProperties(model, properties);

            Assert.True(properties.ContainsKey("text"));
            Assert.Null(properties["text"]);
        }

        enum TestColor { Red, Green }

        class TestClientModel
        {
            [ClientProperty]
            public int Count { get; set; }

            [ClientProperty]
            public TestColor Color { get; set; }

            [ClientProperty(Name = "CustomName")]
            public string? Renamed { get; set; }

            [ClientProperty]
            public string? Text { get; set; } = "value";

            public string Ignored { get; set; } = "x";
        }
    }
}
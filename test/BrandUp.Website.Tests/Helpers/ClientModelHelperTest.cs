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

        [Fact]
        public void Name_IsCamelCasedWithInvariantCulture()
        {
            // В культуре tr-TR "I".ToLower() даёт "ı"; нормализация имени должна быть инвариантной.
            var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");

                var model = new InvariantNameModel();
                var properties = new Dictionary<string, object>();

                ClientModelHelper.CopyProperties(model, properties);

                Assert.True(properties.ContainsKey("id"));
                Assert.False(properties.ContainsKey("ıd"));
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void DuplicateClientName_Throws()
        {
            var model = new DuplicateNameModel();
            var properties = new Dictionary<string, object>();

            var ex = Assert.Throws<InvalidOperationException>(() => ClientModelHelper.CopyProperties(model, properties));
            Assert.Contains("name", ex.Message);
        }

        [Fact]
        public void PublicProperty_IsCopiedViaCompiledGetter()
        {
            var model = new PublicClientModel { Number = 7 };
            var properties = new Dictionary<string, object>();

            ClientModelHelper.CopyProperties(model, properties);

            Assert.Equal(7, properties["number"]);
        }

        [Fact]
        public void NonPublicProperty_IsCopiedViaReflectionFallback()
        {
            var model = new NonPublicPropModel(42);
            var properties = new Dictionary<string, object>();

            ClientModelHelper.CopyProperties(model, properties);

            Assert.Equal(42, properties["secret"]);
        }

        enum TestColor { Red, Green }

        class InvariantNameModel
        {
            [ClientProperty]
            public int Id { get; set; }
        }

        class DuplicateNameModel
        {
            [ClientProperty]
            public string? Name { get; set; }

            [ClientProperty(Name = "name")]
            public string? Other { get; set; }
        }

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

        class NonPublicPropModel(int secret)
        {
            [ClientProperty]
            internal int Secret { get; } = secret;
        }
    }

    // Публичный тип верхнего уровня — для проверки скомпилированного геттера (не fallback).
    public class PublicClientModel
    {
        [ClientProperty]
        public int Number { get; set; }
    }
}
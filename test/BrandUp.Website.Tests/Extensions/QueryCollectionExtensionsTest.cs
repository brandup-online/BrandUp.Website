using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace BrandUp.Website.Extensions
{
    public class QueryCollectionExtensionsTest
    {
        [Fact]
        public void Query_EmptyStringValues_ReturnsNullWithoutThrow()
        {
            var query = new QueryCollection(new Dictionary<string, StringValues> { ["q"] = StringValues.Empty });

            var found = query.TryGetValue("q", out string? value);

            Assert.True(found);
            Assert.Null(value);
        }

        [Fact]
        public void Query_SingleValue_ReturnsValue()
        {
            var query = new QueryCollection(new Dictionary<string, StringValues> { ["q"] = "abc" });

            var found = query.TryGetValue("q", out string? value);

            Assert.True(found);
            Assert.Equal("abc", value);
        }

        [Fact]
        public void Query_Missing_ReturnsFalse()
        {
            var query = new QueryCollection(new Dictionary<string, StringValues>());

            var found = query.TryGetValue("q", out string? value);

            Assert.False(found);
            Assert.Null(value);
        }

        [Fact]
        public void Header_Missing_ReturnsFalse()
        {
            var headers = new HeaderDictionary();

            var found = headers.TryGetValue("x-test", out string? value);

            Assert.False(found);
            Assert.Null(value);
        }
    }
}

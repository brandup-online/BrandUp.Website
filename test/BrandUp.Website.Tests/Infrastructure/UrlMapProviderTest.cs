using BrandUp.Website.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BrandUp.Website.Infrastructure
{
    public class UrlMapProviderTest
    {
        static SubdomainUrlMapProvider CreateSubdomain(string host = "localhost")
            => new(Options.Create(new WebsiteOptions { Host = host, CookiesPrefix = "x" }));

        [Fact]
        public void Subdomain_Host_ReturnsNull()
        {
            Assert.Null(CreateSubdomain().ExtractName(new DefaultHttpContext(), "localhost"));
        }

        [Fact]
        public void Subdomain_Subdomain_ReturnsName()
        {
            Assert.Equal("msk", CreateSubdomain().ExtractName(new DefaultHttpContext(), "msk.localhost"));
        }

        [Fact]
        public void Subdomain_IsCaseInsensitive()
        {
            Assert.Equal("msk", CreateSubdomain().ExtractName(new DefaultHttpContext(), "MSK.LOCALHOST"));
        }

        [Theory]
        [InlineData("xlocalhost")]   // ends with host but is not a subdomain (no dot boundary)
        [InlineData("notlocalhost")]
        [InlineData("other.com")]
        public void Subdomain_NonSubdomainBoundary_ReturnsNull(string requestHost)
        {
            Assert.Null(CreateSubdomain().ExtractName(new DefaultHttpContext(), requestHost));
        }

        [Theory]
        [InlineData("/msk/catalog", "msk")]
        [InlineData("/msk", "msk")]
        [InlineData("/MSK/catalog", "msk")]
        public void Path_ExtractsFirstSegment(string path, string expected)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;

            Assert.Equal(expected, new PathUrlMapProvider().ExtractName(context, "localhost"));
        }
    }
}

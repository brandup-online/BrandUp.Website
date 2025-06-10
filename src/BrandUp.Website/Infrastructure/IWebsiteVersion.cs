using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace BrandUp.Website.Infrastructure
{
    public interface IWebsiteVersion
    {
        Version GetVersion();
    }

    internal class AssemblyWebsiteVersion : IWebsiteVersion
    {
        readonly Version version;

        public AssemblyWebsiteVersion(IHostEnvironment environment)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
                version = entryAssembly.GetName().Version;

            if (version == null || environment.IsDevelopment())
            {
                var rnd = new Random();
                version = new Version(rnd.Next(1, 255), rnd.Next(1, 255), rnd.Next(1, 255), rnd.Next(1, 255));
            }
        }

        public Version GetVersion()
        {
            return version;
        }
    }
}
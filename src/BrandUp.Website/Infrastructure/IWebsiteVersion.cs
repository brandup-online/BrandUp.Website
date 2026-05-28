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
            var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;

            if (assemblyVersion == null || environment.IsDevelopment())
            {
                var rnd = Random.Shared;
                version = new Version(rnd.Next(1, 255), rnd.Next(1, 255), rnd.Next(1, 255), rnd.Next(1, 255));
            }
            else
                version = assemblyVersion;
        }

        public Version GetVersion()
        {
            return version;
        }
    }
}
using System;

namespace BrandUp.Website.Pages
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ClientModelAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
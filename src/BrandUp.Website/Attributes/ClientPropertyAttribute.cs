using System;

namespace BrandUp.Website
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ClientPropertyAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
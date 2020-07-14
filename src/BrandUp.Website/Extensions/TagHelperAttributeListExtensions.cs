using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace BrandUp.Website
{
    public static class TagHelperAttributeListExtensions
    {
        public static TagHelperAttributeList AddCssClass(this TagHelperAttributeList attributeList, string cssClassName)
        {
            if (string.IsNullOrWhiteSpace(cssClassName))
                throw new ArgumentException("Css class name is required and not empty.", nameof(cssClassName));

            string cssClass;
            var hasCssClass = false;
            if (attributeList.TryGetAttribute("class", out TagHelperAttribute attribute))
            {
                cssClass = attribute.Value.ToString();
                hasCssClass = cssClass.Contains(cssClassName, StringComparison.InvariantCultureIgnoreCase);
            }
            else
                cssClass = string.Empty;

            if (!hasCssClass)
            {
                if (!string.IsNullOrEmpty(cssClass))
                    cssClass += " " + cssClassName;
                else
                    cssClass = cssClassName;
            }

            attributeList.SetAttribute("class", cssClass);

            return attributeList;
        }
    }
}

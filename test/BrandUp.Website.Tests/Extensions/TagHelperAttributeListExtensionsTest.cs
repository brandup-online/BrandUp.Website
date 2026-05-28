using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Website
{
    public class TagHelperAttributeListExtensionsTest
    {
        [Fact]
        public void AddCssClass_Empty_SetsClass()
        {
            var list = new TagHelperAttributeList();

            list.AddCssClass("applink");

            Assert.Equal("applink", list["class"].Value.ToString());
        }

        [Fact]
        public void AddCssClass_Existing_Appends()
        {
            var list = new TagHelperAttributeList { { "class", "item" } };

            list.AddCssClass("applink");

            Assert.Equal("item applink", list["class"].Value.ToString());
        }

        [Fact]
        public void AddCssClass_AlreadyPresent_DoesNotDuplicate()
        {
            var list = new TagHelperAttributeList { { "class", "item applink" } };

            list.AddCssClass("applink");

            Assert.Equal("item applink", list["class"].Value.ToString());
        }

        [Fact]
        public void AddCssClass_SubstringMatch_StillAppends()
        {
            // "applink" must not be considered present just because "applink-active" contains it
            var list = new TagHelperAttributeList { { "class", "applink-active" } };

            list.AddCssClass("applink");

            Assert.Equal("applink-active applink", list["class"].Value.ToString());
        }

        [Fact]
        public void AddCssClass_EmptyName_Throws()
        {
            var list = new TagHelperAttributeList();

            Assert.Throws<ArgumentException>(() => list.AddCssClass(" "));
        }
    }
}

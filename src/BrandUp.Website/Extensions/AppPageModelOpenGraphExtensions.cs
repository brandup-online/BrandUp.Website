using BrandUp.Website.Pages;

namespace BrandUp.Website
{
    public static class AppPageModelOpenGraphExtensions
    {
        /// <summary>
        /// Устанавливает Open Graph типа "website".
        /// Title по умолчанию берётся со страницы, Url всегда равен адресу текущей страницы.
        /// </summary>
        /// <param name="image">
        /// Путь к изображению. Если начинается с "~", резолвится через <see cref="IUrlHelperExtensions.ContentLink"/>
        /// в абсолютный URL; иначе трактуется как готовый абсолютный URL.
        /// </param>
        public static PageOpenGraph SetOpenGraphWebsite(this AppPageModel page, string image, string? title = null, string? description = null)
            => page.SetOpenGraphWebsite(ResolveImageUrl(page, image), title, description);

        /// <summary>
        /// Устанавливает Open Graph типа "website".
        /// Title по умолчанию берётся со страницы, Url всегда равен адресу текущей страницы.
        /// </summary>
        public static PageOpenGraph SetOpenGraphWebsite(this AppPageModel page, Uri image, string? title = null, string? description = null)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(image);

            var og = new PageOpenGraph(
                type: OpenGraphType.Website,
                image: image,
                title: title ?? page.Title,
                url: page.Link,
                description: description ?? page.Description);

            page.OpenGraph = og;
            return og;
        }

        /// <summary>
        /// Устанавливает Open Graph типа "article" вместе с namespace-свойствами article:*.
        /// Title и Url по умолчанию берутся со страницы.
        /// </summary>
        /// <param name="image">
        /// Путь к изображению. Если начинается с "~", резолвится через <see cref="IUrlHelperExtensions.ContentLink"/>
        /// в абсолютный URL; иначе трактуется как готовый абсолютный URL.
        /// </param>
        public static PageOpenGraph SetOpenGraphArticle(this AppPageModel page, string image, string? title = null, string? description = null,
            DateTimeOffset? publishedTime = null, DateTimeOffset? modifiedTime = null, string? section = null, string? author = null, string? tag = null)
            => page.SetOpenGraphArticle(ResolveImageUrl(page, image), title, description, publishedTime, modifiedTime, section, author, tag);

        /// <summary>
        /// Устанавливает Open Graph типа "article" вместе с namespace-свойствами article:*.
        /// Title и Url по умолчанию берутся со страницы.
        /// </summary>
        /// <remarks>
        /// article:author и article:tag по спецификации допускают несколько значений, но текущее
        /// хранилище <see cref="PageOpenGraph"/> (словарь по имени свойства) хранит только одно
        /// значение на свойство, поэтому здесь принимается по одному author/tag.
        /// </remarks>
        public static PageOpenGraph SetOpenGraphArticle(this AppPageModel page, Uri image, string? title = null, string? description = null,
            DateTimeOffset? publishedTime = null, DateTimeOffset? modifiedTime = null, string? section = null, string? author = null, string? tag = null)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(image);

            var og = new PageOpenGraph(
                type: OpenGraphType.Article,
                image: image,
                title: title ?? page.Title,
                url: page.Link,
                description: description ?? page.Description);

            if (publishedTime.HasValue)
                og.Set("article:published_time", publishedTime.Value.ToString("o"));
            if (modifiedTime.HasValue)
                og.Set("article:modified_time", modifiedTime.Value.ToString("o"));
            if (!string.IsNullOrEmpty(section))
                og.Set("article:section", section);
            if (!string.IsNullOrEmpty(author))
                og.Set("article:author", author);
            if (!string.IsNullOrEmpty(tag))
                og.Set("article:tag", tag);

            page.OpenGraph = og;
            return og;
        }

        private static Uri ResolveImageUrl(AppPageModel page, string image)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentException.ThrowIfNullOrEmpty(image);

            if (image.StartsWith('~'))
                return page.Url.ContentLink(image);

            return new Uri(image, UriKind.Absolute);
        }
    }
}

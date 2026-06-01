using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;

namespace BrandUp.Website.TagHelpers
{
	/// <summary>
	/// Встраивает содержимое скрипта прямо в страницу: &lt;script inline-src="~/dist/runtime.js"&gt;&lt;/script&gt;.
	/// Контент читается из wwwroot и кешируется до изменения файла.
	/// </summary>
	[HtmlTargetElement("script", Attributes = InlineAttributeName)]
	public class InlineScriptTagHelper(IWebHostEnvironment environment, IMemoryCache cache) : TagHelper
	{
		const string InlineAttributeName = "inline-src";
		const string CacheKeyPrefix = "BrandUp.Website:InlineScript:";

		readonly IWebHostEnvironment environment = environment ?? throw new ArgumentNullException(nameof(environment));
		readonly IMemoryCache cache = cache ?? throw new ArgumentNullException(nameof(cache));

		[HtmlAttributeName(InlineAttributeName)]
		public string? Src { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.Attributes.RemoveAll(InlineAttributeName);

			var relativePath = NormalizeRelativePath(Src);

			var content = ReadCached(relativePath);
			if (content == null)
			{
				// Файл ещё не собран — оставляем обычную внешнюю ссылку (root-относительную).
				output.Attributes.SetAttribute("src", "/" + relativePath);
				return;
			}

			output.Attributes.RemoveAll("src");
			output.TagMode = TagMode.StartTagAndEndTag;
			output.Content.SetHtmlContent(content);
		}

		static string NormalizeRelativePath(string? src)
			=> (src ?? string.Empty).TrimStart('~').Replace('\\', '/').TrimStart('/');

		string? ReadCached(string relativePath)
		{
			var cacheKey = CacheKeyPrefix + relativePath;
			if (cache.TryGetValue(cacheKey, out string? cached))
				return cached;

			var fileProvider = environment.WebRootFileProvider;
			var fileInfo = fileProvider.GetFileInfo(relativePath);
			if (!fileInfo.Exists)
				return null;

			return cache.GetOrCreate(cacheKey, entry =>
			{
				// Сбрасываем запись, как только файл изменится или будет удалён.
				entry.AddExpirationToken(fileProvider.Watch(relativePath));

				using var reader = new StreamReader(fileInfo.CreateReadStream());
				return reader.ReadToEnd();
			});
		}
	}
}

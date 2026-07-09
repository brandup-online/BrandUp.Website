using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;

namespace BrandUp.Website.TagHelpers
{
	/// <summary>
	/// Встраивает изображение прямо в страницу как data:-URI: &lt;img inline-src="~/dist/logo.svg" /&gt;.
	/// Работает для растровых форматов и SVG. Контент читается из wwwroot и кешируется до изменения файла.
	/// </summary>
	[HtmlTargetElement("img", Attributes = InlineAttributeName)]
	public class InlineImageTagHelper(IWebHostEnvironment environment, IMemoryCache cache) : TagHelper
	{
		const string InlineAttributeName = "inline-src";
		const string CacheKeyPrefix = "BrandUp.Website:InlineImage:";

		readonly IWebHostEnvironment environment = environment ?? throw new ArgumentNullException(nameof(environment));
		readonly IMemoryCache cache = cache ?? throw new ArgumentNullException(nameof(cache));

		[HtmlAttributeName(InlineAttributeName)]
		public string? Src { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.Attributes.RemoveAll(InlineAttributeName);

			var relativePath = NormalizeRelativePath(Src);

			var dataUri = ReadCached(relativePath);
			if (dataUri == null)
			{
				// Файл ещё не собран — оставляем обычную внешнюю ссылку (root-относительную).
				output.Attributes.SetAttribute("src", "/" + relativePath);
				return;
			}

			output.Attributes.SetAttribute("src", dataUri);
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

				return ReadDataUri(fileInfo, relativePath);
			});
		}

		static string ReadDataUri(IFileInfo file, string relativePath)
		{
			using var stream = file.CreateReadStream();
			using var ms = new MemoryStream();
			stream.CopyTo(ms);

			return $"data:{GetMimeType(relativePath)};base64,{Convert.ToBase64String(ms.ToArray())}";
		}

		static string GetMimeType(string relativePath) => Path.GetExtension(relativePath).ToLowerInvariant() switch
		{
			".png" => "image/png",
			".jpg" or ".jpeg" => "image/jpeg",
			".gif" => "image/gif",
			".webp" => "image/webp",
			".avif" => "image/avif",
			".ico" => "image/x-icon",
			".bmp" => "image/bmp",
			".svg" => "image/svg+xml",
			_ => "application/octet-stream",
		};
	}
}

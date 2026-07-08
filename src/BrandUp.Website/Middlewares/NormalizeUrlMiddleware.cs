using Microsoft.AspNetCore.Http;

namespace BrandUp.Website.Middlewares
{
	public class NormalizeUrlMiddleware(RequestDelegate next, string[] excludedPaths)
	{
		public async Task InvokeAsync(HttpContext context)
		{
			// Пути-исключения (например статические бандлы webpack /dist): регистр в именах чанков значим
			// (например uPlot) — приводить путь к нижнему регистру нельзя, иначе манифест static web assets не найдёт файл (404).
			var isExcluded = context.Request.Path.HasValue && IsExcluded(context.Request.Path.Value!);

			if (!isExcluded && context.Request.Method == "GET" && context.Request.Path.HasValue && context.Response.Headers.Location.Count == 0)
			{
				var path = context.Request.PathBase.Add(context.Request.Path).Value!;
				if (path.Length > 1)
				{
					// Инвариантная нормализация: убираем завершающий "/" и приводим к нижнему регистру.
					// Редирект только если результат реально отличается — это исключает зависимость от текущей
					// культуры и рассинхрон "есть ли верхний регистр" / "во что он приводится" (иначе возможен цикл 301).
					var normalizedPath = path.TrimEnd('/').ToLowerInvariant();

					if (!string.Equals(normalizedPath, path, StringComparison.Ordinal))
					{
						var urlBuilder = new UriBuilder(context.Request.Scheme, context.Request.Host.Value)
						{
							Path = normalizedPath
						};

						if (context.Request.QueryString.HasValue)
							urlBuilder.Query = context.Request.QueryString.Value;

						context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
						context.Response.Headers.Location = urlBuilder.ToString().Replace(urlBuilder.Host, context.Request.Host.Value);
						return;
					}
				}
			}

			await next(context);
		}

		bool IsExcluded(string path)
		{
			foreach (var excludedPath in excludedPaths)
			{
				if (path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}
	}
}

using System.Text.RegularExpressions;
using BrandUp.Website.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace BrandUp.Website.Middlewares
{
    public class MinifyHtmlMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var feature = new MinifyHtmlFeature(context);
            context.Features.Set<IMinifyHtmlFeature>(feature);

            await next(context);

            await feature.ProcessAsync();
        }

        class MinifyHtmlFeature(HttpContext context) : IMinifyHtmlFeature
        {
            Stream responseBody;
            Stream tempBody;

            // Replace all spaces between tags skipping PRE tags
            private readonly static Regex Regex1 = new Regex(@"(?<=\s)\s+(?![^<>]*</pre>)", RegexOptions.Compiled);
            // Replace all new lines between tags skipping PRE tags
            private readonly static Regex Regex2 = new Regex("\n(?![^<]*</pre>)", RegexOptions.Compiled);

            public bool AllowMinify { get; private set; }

            public void SetMinify()
            {
                AllowMinify = true;

                responseBody = context.Response.Body;

                tempBody = new MemoryStream();
                context.Response.Body = tempBody;
            }

            public async Task ProcessAsync()
            {
                if (!AllowMinify)
                    return;

                try
                {
                    tempBody.Seek(0, SeekOrigin.Begin);
                    context.Response.Body = responseBody;

                    if (!context.Response.Headers.TryGetValue("Content-type", out Microsoft.Extensions.Primitives.StringValues contentType) || !contentType[0].StartsWith("text/html", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await tempBody.CopyToAsync(responseBody, context.RequestAborted);
                        tempBody.Dispose();
                        return;
                    }

                    using var streamReader = new StreamReader(tempBody);
                    var html = await streamReader.ReadToEndAsync(context.RequestAborted);

                    html = Regex1.Replace(html, string.Empty);
                    html = Regex2.Replace(html, string.Empty);

                    await context.Response.WriteAsync(html, System.Text.Encoding.UTF8, context.RequestAborted);

                    await context.Response.Body.FlushAsync(context.RequestAborted);
                }
                catch (OperationCanceledException) { }
            }
        }
    }
}
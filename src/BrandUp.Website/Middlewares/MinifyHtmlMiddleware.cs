using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrandUp.Website.Middlewares
{
    public class MinifyHtmlMiddleware
    {
        // Replace all spaces between tags skipping PRE tags
        private readonly static Regex regex1 = new Regex(@"(?<=\s)\s+(?![^<>]*</pre>)", RegexOptions.Compiled);
        // Replace all new lines between tags skipping PRE tags
        private readonly static Regex regex2 = new Regex("\n(?![^<]*</pre>)", RegexOptions.Compiled);

        private readonly RequestDelegate next;

        public MinifyHtmlMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var responseBody = context.Response.Body;

            using var newResponseBody = new MemoryStream();
            context.Response.Body = newResponseBody;

            await next(context);

            newResponseBody.Seek(0, SeekOrigin.Begin);
            context.Response.Body = responseBody;

            if (!context.Response.Headers.TryGetValue("Content-type", out Microsoft.Extensions.Primitives.StringValues contentType) || !contentType[0].StartsWith("text/html", StringComparison.InvariantCultureIgnoreCase))
            {
                await newResponseBody.CopyToAsync(responseBody);
                return;
            }

            using var streamReader = new StreamReader(newResponseBody);
            var html = await streamReader.ReadToEndAsync();

            html = regex1.Replace(html, string.Empty);
            html = regex2.Replace(html, string.Empty);

            await context.Response.WriteAsync(html, context.RequestAborted);
        }
    }
}
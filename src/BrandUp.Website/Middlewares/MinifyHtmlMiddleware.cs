using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrandUp.Website.Middlewares
{
    public class MinifyHtmlMiddleware
    {
        private readonly RequestDelegate next;
        private readonly static Regex regex1;
        private readonly static Regex regex2;

        static MinifyHtmlMiddleware()
        {
            // Replace all spaces between tags skipping PRE tags
            regex1 = new Regex(@"(?<=\s)\s+(?![^<>]*</pre>)", RegexOptions.Compiled);

            // Replace all new lines between tags skipping PRE tags
            regex2 = new Regex("\n(?![^<]*</pre>)", RegexOptions.Compiled);
        }

        public MinifyHtmlMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var responseBody = context.Response.Body;

            using var newResponseBody = new MemoryStream();
            context.Response.Body = newResponseBody;

            await next(context);

            newResponseBody.Seek(0, SeekOrigin.Begin);
            context.Response.Body = responseBody;

            if (!context.Response.Headers.TryGetValue("Content-type", out Microsoft.Extensions.Primitives.StringValues contentType) || !contentType[0].StartsWith("text/html"))
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
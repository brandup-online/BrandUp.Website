using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace ExampleWebSite.Controllers
{
    public class SeoController : Controller
    {
        [HttpGet("robots.txt")]
        public IActionResult RobotsTxt()
        {
            var request = Request;
            var contentBuilder = new StringBuilder();

            contentBuilder.AppendLine("User-Agent: *");
            contentBuilder.AppendLine("Disallow: *?*");
            contentBuilder.AppendLine("Disallow: /error");
            contentBuilder.AppendLine("Disallow: /accessdenied");
            contentBuilder.AppendLine("Disallow: /signin");
            contentBuilder.AppendLine("Allow: *?v=*");

            contentBuilder.AppendLine(string.Empty);
            contentBuilder.AppendLine($"Host: https://{request.Host.ToUriComponent()}");
            contentBuilder.AppendLine($"Sitemap: https://{request.Host.ToUriComponent()}/sitemap.xml");

            return Content(contentBuilder.ToString(), "text/plain");
        }

        [HttpGet("sitemap.xml")]
        public IActionResult SitemapXml([FromServices] ApplicationPartManager applicationPartManager)
        {
            var request = Request;

            var viewsFeature = new Microsoft.AspNetCore.Mvc.Razor.Compilation.ViewsFeature();
            applicationPartManager.PopulateFeature(viewsFeature);

            var dateNow = DateTime.Now.ToString("s");
            var model = new SitemapModel { Urls = new List<SitemapUrl>() };

            foreach (var viewDescriptor in viewsFeature.ViewDescriptors)
            {
                var path = viewDescriptor.RelativePath;
                if (!path.StartsWith("/Pages/"))
                    continue;

                path = path.Substring("/Pages".Length).Replace(".cshtml", "").ToLower();

                var url = Url.Page(path, null, null, request.Scheme, request.Host.ToUriComponent());
                if (string.IsNullOrEmpty(url))
                    continue;

                if (url.Contains("error") || url.Contains("accessdenied") || url.Contains("signin") || url.Contains("tag"))
                    continue;

                model.Urls.Add(new SitemapUrl
                {
                    Location = url,
                    LastMod = dateNow,
                    ChangeFreq = "daily",
                    Priority = "0.6"
                });
            }

            model.Urls = model.Urls.OrderBy(it => it.Location).ToList();
            model.Urls[0].ChangeFreq = "daily";
            model.Urls[0].Priority = "1";

            return new XmlResult(model);
        }

        [XmlRoot("urlset")]
        public class SitemapModel
        {
            [XmlElement("url")]
            public List<SitemapUrl> Urls { get; set; }
        }

        public class SitemapUrl
        {
            [XmlElement("loc")]
            public string Location { get; set; }
            [XmlElement("lastmod")]
            public string LastMod { get; set; }
            [XmlElement("changefreq")]
            public string ChangeFreq { get; set; }
            [XmlElement("priority")]
            public string Priority { get; set; }
        }

        class XmlResult : ActionResult
        {
            readonly SitemapModel model;

            public XmlResult(SitemapModel model)
            {
                this.model = model;
            }

            public override void ExecuteResult(ActionContext context)
            {
                context.HttpContext.Response.ContentType = "text/xml";

                using var writer = XmlWriter.Create(context.HttpContext.Response.Body, new XmlWriterSettings
                {
                    CloseOutput = false,
                    Encoding = new UTF8Encoding(false)
                });

                writer.WriteStartDocument();
                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                foreach (var p in model.Urls)
                {
                    writer.WriteStartElement("url");

                    writer.WriteStartElement("loc");
                    writer.WriteRaw(p.Location);
                    writer.WriteEndElement();

                    //writer.WriteStartElement("lastmod");
                    //writer.WriteRaw(p.LastMod);
                    //writer.WriteEndElement();

                    writer.WriteStartElement("changefreq");
                    writer.WriteRaw(p.ChangeFreq);
                    writer.WriteEndElement();

                    writer.WriteStartElement("priority");
                    writer.WriteRaw(p.Priority.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
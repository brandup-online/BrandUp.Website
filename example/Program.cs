using System.IO.Compression;
using BrandUp.Website;
using ExampleWebSite.Infrastructure.HealthChecks;
using ExampleWebSite.Repositories;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebMarkupMin.AspNet.Common.Compressors;
using WebMarkupMin.AspNetCore8;

namespace ExampleWebSite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            var configuration = builder.Configuration;

            #region Configure

            var websiteOptions = builder.Configuration.GetSection("WebSite").Get<WebsiteOptions>();

            services
                .AddWebMarkupMin(options =>
                {
                    options.AllowMinificationInDevelopmentEnvironment = true;
                    options.AllowCompressionInDevelopmentEnvironment = true;
                    options.DefaultEncoding = System.Text.Encoding.UTF8;
                })
                .AddHtmlMinification(options =>
                {
                    var settings = options.MinificationSettings;
                    settings.RemoveRedundantAttributes = true;
                    settings.RemoveHttpProtocolFromAttributes = true;
                    settings.RemoveHttpsProtocolFromAttributes = true;
                })
                .AddHttpCompression(options =>
                {
                    options.CompressorFactories =
                    [
                        new BuiltInBrotliCompressorFactory(new BuiltInBrotliCompressionSettings { Level = CompressionLevel.Fastest }),
                        new DeflateCompressorFactory(new DeflateCompressionSettings { Level = CompressionLevel.Fastest }),
                        new GZipCompressorFactory(new GZipCompressionSettings { Level = CompressionLevel.Fastest })
                    ];
                });

            services.AddHttpContextAccessor();
            services.AddRazorPages();

            #region Web

            services
                .AddRequestDecompression()
                .AddResponseCompression(options =>
                {
                    options.EnableForHttps = true;
                    options.Providers.Add<BrotliCompressionProvider>();
                    options.Providers.Add<GzipCompressionProvider>();

                    options.MimeTypes = new string[] { "text/html", "text/xml", "text/json", "text/plain", "application/json", "application/xml", "application/javascript", "text/css" };
                })
                .Configure<BrotliCompressionProviderOptions>(options =>
                {
                    options.Level = System.IO.Compression.CompressionLevel.Fastest;
                })
                .Configure<GzipCompressionProviderOptions>(options =>
                {
                    options.Level = System.IO.Compression.CompressionLevel.Fastest;
                });

            services.AddResponseCaching();

            services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All);
            });

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "ExampleWebSite_af";
                options.Cookie.HttpOnly = true;
                //options.Cookie.Domain = websiteOptions.Host;
                options.Cookie.Path = "/";
            });

            services.AddRequestLocalization(options =>
            {
                var defaultCulture = new System.Globalization.CultureInfo("ru");
                var supportedCultures = new[] { defaultCulture };

                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.AppendTrailingSlash = false;
                options.LowercaseQueryStrings = true;
            });

            #endregion

            #region Authentication

            services
                .AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = "ExampleWebSite_id";
                    options.SlidingExpiration = true;
                    options.ReturnUrlParameter = "returnUrl";
                    options.LoginPath = "/signin";
                });

            #endregion

            services.AddSingleton<CityRepository>();

            services
                .AddWebsite(options => { options.MapConfiguration(configuration); })
                .AddWebsiteEvents<ExambleWebsiteEvents>()
                .AddPageEvents<Pages.PageEvents>()
                .AddUrlMapProvider<BrandUp.Website.Infrastructure.SubdomainUrlMapProvider>()
                .AddMultyWebsiteFrom<CityRepository>();

            services
                .AddHealthChecks()
                .AddCheck<RequestTimeHealthCheck>(nameof(RequestTimeHealthCheck), HealthStatus.Unhealthy, null, TimeSpan.FromSeconds(10));

            services.AddExceptionHandler(options => { options.ExceptionHandlingPath = "/error"; });
            services.AddProblemDetails(options => { });

            #endregion

            #region Build

            var app = builder.Build();

            app.UseRequestDecompression();

            //if (app.Environment.IsDevelopment())
            //    app.UseDeveloperExceptionPage();
            //else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { AllowCachingResponses = false });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseRequestLocalization();
            app.UseWebsite();
            app.UseResponseCompression();
            app.UseResponseCaching();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWebMarkupMin();

            app.MapRazorPages();
            app.MapControllers();

            app.UseStatusCodePagesWithReExecute("/notfound");

            //app.UseStatusCodePages(context =>
            //{
            //    switch (context.HttpContext.Response.StatusCode)
            //    {
            //        case 404:
            //            context.HttpContext.Response.RedirectPage("/notfound", replace: true);
            //            break;
            //    }

            //    return Task.CompletedTask;
            //});

            #endregion

            app.Run();
        }
    }

    public class ExambleWebsiteEvents : IWebsiteEvents
    {
        public Task StartAsync(StartWebsiteContext context)
        {
            return Task.CompletedTask;
        }

        public Task RenderBodyTag(OnRenderTagContext context)
        {
            return Task.CompletedTask;
        }

        public Task RenderHeadTag(OnRenderTagContext context)
        {
            return Task.CompletedTask;
        }
    }
}
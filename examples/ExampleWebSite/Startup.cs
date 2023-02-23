using BrandUp.Website;
using ExampleWebSite.Repositories;
using Microsoft.AspNetCore.ResponseCompression;

namespace ExampleWebSite
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            #region Web

            services
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

            services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.AppendTrailingSlash = false;
                options.LowercaseQueryStrings = true;
            });

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "ExampleWebSite_af";
                options.Cookie.HttpOnly = true;
                //options.Cookie.Domain = websiteOptions.Host;
                options.Cookie.Path = "/";
            });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var defaultCulture = new System.Globalization.CultureInfo("ru");
                var supportedCultures = new[] { defaultCulture };

                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            #endregion

            services.AddHttpContextAccessor();

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
                .AddWebsite(options =>
                {
                    options.MapConfiguration(Configuration);
                })
                .AddWebsiteEvents<ExambleWebsiteEvents>()
                .AddPageEvents<Pages.PageEvents>()
                .AddUrlMapProvider<BrandUp.Website.Infrastructure.SubdomainUrlMapProvider>()
                .AddMultyWebsiteFrom<CityRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseRequestLocalization();
            app.UseWebsite();
            app.UseResponseCompression();
            app.UseResponseCaching();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMinifyHtml();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
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

        public Task RenderPageTitle(RenderPageTitleContext context)
        {
            context.Title = context.PageModel.Title + " – BrandUp.Website";

            return Task.CompletedTask;
        }
    }
}
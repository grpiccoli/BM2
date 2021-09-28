using AspNetCore.IServiceCollection.AddIUrlHelper;
using BiblioMit.Authorization;
using BiblioMit.Blazor;
using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models;
using BiblioMit.Models.VM;
using BiblioMit.Services;
using BiblioMit.Services.Hubs;
using BiblioMit.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Globalization;
using WebEssentials.AspNetCore.Pwa;

[assembly: CLSCompliant(false)]
namespace BiblioMit
{
    public class Startup
    {
        private readonly string _os;
        private const string defaultCulture = "en";
        private readonly CultureInfo[] supportedCultures;
        public Startup(IConfiguration configuration)
        {
            supportedCultures = new[]
                {
                    new CultureInfo(defaultCulture),
                    new CultureInfo("es")
                };
            Configuration = configuration;
            _os = Environment.OSVersion.Platform.ToString();
        }
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString($"{_os}Connection"),
                    sqlServerOptions => sqlServerOptions.CommandTimeout(10000)));

            services.AddScoped<IPuppet, PuppetService>();
            services.AddHostedService<PlanktonBackground>();
            services.AddScoped<IPlanktonService, PlanktonService>();
            services.AddHostedService<SeedBackground>();
            services.AddScoped<ISeed, SeedService>();
            services.AddScoped<IUpdateJsons, UpdateJsons>();
            services.AddScoped<INodeService, NodeService>();
            services.AddScoped<IBannerService, BannerService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IImport, ImportService>();
            services.AddScoped<ITableToExcel, TableToExcelService>();

            services.AddAntiforgery(options =>
            {
                options.FormFieldName = "__RequestVerificationToken";
                options.HeaderName = "X-CSRF-TOKEN";
            });

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddScoped<IForum, ForumService>();
            services.AddScoped<IEntryHub, EntryHub>();
            services.AddScoped<IPost, PostService>();
            services.AddScoped<IApplicationUser, ApplicationUserService>();

            // Authorization handlers.
            services.AddScoped<IAuthorizationHandler, ContactIsOwnerAuthorizationHandler>();

            services.AddScoped<IAuthorizationHandler, ContactAdministratorsAuthorizationHandler>();

            services.AddScoped<IAuthorizationHandler, ContactManagerAuthorizationHandler>();

            services.AddScoped<IViewRenderService, ViewRenderService>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.Cookie.Name = "BiblioMit";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.LoginPath = "/Identity/Account/Login";
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
                options.SignIn.RequireConfirmedEmail = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultUI()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<SpanishIdentityErrorDescriber>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.LoginPath = new PathString("/login");
                    o.AccessDeniedPath = new PathString("/Account/AccessDenied");
                    o.LogoutPath = new PathString("/logout");
                });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                // Formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;
                // UI strings that we have localized.
                options.SupportedUICultures = supportedCultures;
            });

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddRazorPages()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization()
                .AddRazorRuntimeCompilation();
            services.AddServerSideBlazor();
            services.AddTransient<IEnvironmental, EnvironmentalService>();

            services.AddResponseCaching();

            services.AddMvc(options => {
                options.MaxIAsyncEnumerableBufferLimit = 10_000;
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddViewLocalization(
                LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization()
                .AddNewtonsoftJson(o => 
                    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore
                ).AddJsonOptions(o => 
                    o.JsonSerializerOptions.IgnoreNullValues = true);

            services.AddProgressiveWebApp(new PwaOptions {
                RegisterServiceWorker = false,
                RegisterWebmanifest = false,
                EnableCspNonce = true 
            });

            services.ConfigureNonBreakingSameSiteCookies();

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
            });

            //services.AddHttpsRedirection(options =>
            //    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect);

            services.Configure<AuthMessageSenderOptions>(Configuration);

            services.AddAuthorization(options =>
            {
                UserClaims.Banners.Enum2ListNames().ForEach(item =>
                    options.AddPolicy(item, policy => policy.RequireClaim(item, item))
                );
            });

            services.AddUrlHelper();

            services.Configure<FlowSettings>(o =>
            {
                var dev = true;
                var flowEnv = dev ? "Sandbox" : "Production";
                var preffix = dev ? "sandbox" : "www";
                o.ApiKey = Configuration[$"Flow:{flowEnv}:ApiKey"];
                o.SecretKey = Configuration[$"Flow:{flowEnv}:SecretKey"];
                o.Currency = "CLP";
                o.EndPoint = new Uri($"https://{preffix}.flow.cl/api");
            });
            services.AddScoped<IFlow, FlowService>();

            //services.AddCors();

            services.AddSignalR(options => {
                options.EnableDetailedErrors = true;
            });
            Libman.LoadJson();
            Bundler.LoadJson();
            WebCompiler.LoadJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (app == null || env == null) return;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts(hsts => hsts.MaxAge(365));
            }
            app.UseCookiePolicy();

            app.UseRedirectValidation(opts => {
                opts.AllowSameHostRedirectsToHttps();
                opts.AllowedDestinations("https://sandbox.flow.cl/app/web/pay.php");
            });

            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());

            FileExtensionContentTypeProvider provider = new();
            provider.Mappings[".webmanifest"] = "application/manifest+json";

            app.UseDefaultFiles();

            app.UseStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = provider,
                OnPrepareResponse = ctx =>
                {
                    const int durationInSecond = 60 * 60 * 24 * 365;
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=" + durationInSecond;
                }
            });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseRouting();

            app.UseResponseCaching();

            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
            new CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = TimeSpan.FromSeconds(60)
            };
                context.Response.Headers[HeaderNames.Vary] =
                    new string[] { "Accept-Encoding" };

                await next().ConfigureAwait(false);
            });

            app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);

            app.UseXfo(xfo => xfo.Deny());

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub().RequireAuthorization();
                endpoints.MapFallbackToController("Index", "Home");
                endpoints.MapRazorPages().RequireAuthorization();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                .RequireAuthorization();
                endpoints.MapHub<ChatHub>("/chatHub");
                endpoints.MapHub<EntryHub>("/entryHub").RequireAuthorization();
                endpoints.MapHub<ProgressHub>("/progressHub").RequireAuthorization();
            });
        }
    }
}

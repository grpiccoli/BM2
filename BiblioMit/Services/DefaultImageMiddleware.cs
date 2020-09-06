using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioMit.Services
{
    public class DefaultImageMiddleware
    {
        private readonly RequestDelegate _next;

        public static string DefaultImagePath { get; set; }

        public DefaultImageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context).ConfigureAwait(false);
            if (context?.Response.StatusCode == 404)
            {
                var contentType = context.Request.Headers["accept"].ToString().ToUpperInvariant();
                if (contentType.StartsWith("IMAGE", StringComparison.InvariantCulture))
                {
                    await SetDefaultImage(context).ConfigureAwait(false);
                }
            }
        }

        private static async Task SetDefaultImage(HttpContext context)
        {
            try
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), DefaultImagePath);

                using FileStream fs = File.OpenRead(path);
                byte[] bytes = new byte[fs.Length];
                await fs.ReadAsync(bytes.AsMemory(0, bytes.Length)).ConfigureAwait(false);
                //this header is use for browser cache, format like: "Mon, 15 May 2017 07:03:37 GMT". 
                context.Response.Headers.Append("Last-Modified", $"{File.GetLastWriteTimeUtc(path).ToString("ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture)} GMT");

                await context.Response.Body.WriteAsync(bytes.AsMemory(0, bytes.Length)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await context.Response.WriteAsync(ex.Message).ConfigureAwait(false);
                throw;
            }
        }
    }
    public static class DefaultImageMiddlewareExtensions
    {
        public static IApplicationBuilder UseDefaultImage(this IApplicationBuilder app, string defaultImagePath)
        {
            DefaultImageMiddleware.DefaultImagePath = defaultImagePath;

            return app.UseMiddleware<DefaultImageMiddleware>();
        }
    }
}

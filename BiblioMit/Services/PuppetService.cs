using PuppeteerSharp;
using System;
using System.Linq;
using System.Threading.Tasks;
//using PuppeteerSharp.Media;
//using AntiCaptchaNetCore;
//using System.IO;
using System.Collections.Generic;
//using Microsoft.AspNetCore.Hosting;

namespace BiblioMit.Services
{
    public class PuppetService : IPuppet
    {
        private readonly string _os;
        //private readonly IWebHostEnvironment _environment;
        public PuppetService(
            //IWebHostEnvironment environment
            )
        {
            //_environment = environment;
            _os = Environment.OSVersion.Platform.ToString();
        }
//        public async Task<string> GetCaptchaAsync(
//            Page page, string selector, string folder)
//        {
//            if (page == null) return null;
//            var rect = await page.EvaluateFunctionAsync<DOMRect>(
//@$"() => document.querySelector(""{selector}"").getBoundingClientRect().toJSON()")
//                .ConfigureAwait(false);
//            if (rect == null) return null;
//            var tmp = Path.Combine(Path.GetTempPath(), "tmp.png");
//            File.Delete(tmp);
//            await page.ScreenshotAsync(tmp,
//                new ScreenshotOptions
//                {
//                    Clip = new Clip
//                    {
//                        X = rect.Left,
//                        Y = rect.Top,
//                        Width = rect.Width,
//                        Height = rect.Height
//                    }
//                }).ConfigureAwait(false);
//            byte[] imageArray = await File.ReadAllBytesAsync(tmp).ConfigureAwait(false);
//            string base64image = Convert.ToBase64String(imageArray);
//            AntiCaptcha anticaptcha = AntiCaptchaClient.Get();
//            var solved = anticaptcha.GetAnswer(base64image);
//            var captcha = solved.solution.text;
//            if(folder != null)
//            {
//                var dest = Path.Combine(_environment.ContentRootPath, "Captcha", folder, $"{captcha}.png");
//                File.Copy(tmp, dest, true);
//            }
//            File.Delete(tmp);
//            return captcha;
//        }
        public static async Task<RevisionInfo> FetchAsync()
        {
            return await new BrowserFetcher()
                .DownloadAsync(BrowserFetcher.DefaultRevision)
                .ConfigureAwait(false);
        }
        public async Task<Browser> GetBrowserAsync()
        {
            await FetchAsync().ConfigureAwait(false);
            return await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                IgnoreHTTPSErrors = true,
                Args = GetArgs()
            }).ConfigureAwait(false);
        }
        public async Task<Page> GetPageAsync(string WebSocketEndpoint)
        {
            var browser = await Puppeteer
                .ConnectAsync(new ConnectOptions { BrowserWSEndpoint = WebSocketEndpoint })
                .ConfigureAwait(false);
            var pages = await browser.PagesAsync().ConfigureAwait(false);
            return pages[0];
        }
        public async Task<Page> ForceGetPageAsync(Uri uri, List<string> block)
        {
            var open = true;
            Page page = null;
            while (open)
            {
                try
                {
                    page = await GetPageAsync(uri, block)
                    .ConfigureAwait(false);
                    open = false;
                }
                catch (TimeoutException ex)
                {
                    Console.WriteLine(ex);
                    await page.Browser.CloseAsync().ConfigureAwait(false);
                }
            };
            return page;
        }
        public async Task<Page> GetPageAsync(Uri uri, List<string> block)
        {
            var browser = await GetBrowserAsync().ConfigureAwait(false);
            var pages = await browser.PagesAsync().ConfigureAwait(false);
            var page = pages[0];
            if(block != null)
            {
                await page.SetRequestInterceptionAsync(true).ConfigureAwait(false);
                page.Request += (sender, e) =>
                {
                    if (block.Any(b => e.Request.Url.Contains(b, StringComparison.InvariantCultureIgnoreCase)))
                        e.Request.AbortAsync();
                    else
                        e.Request.ContinueAsync();
                };
            }
            await page.GoToAsync(
                uri?.AbsoluteUri,
                new NavigationOptions
                {
                    WaitUntil = new WaitUntilNavigation[]
                    {
                        WaitUntilNavigation.Networkidle2
                    }
                }).ConfigureAwait(false);
            return page;
        }
        private string[] GetArgs()
        {
            var args = new List<string>
                {
                    "--disable-accelerated-2d-canvas",
                    "--disable-background-timer-throttling",
                    "--disable-backgrounding-occluded-windows",
                    "--disable-breakpad",
                    "--disable-component-extensions-with-background-pages",
                    "--disable-dev-shm-usage",
                    "--disable-extensions",
                    "--disable-features=TranslateUI,BlinkGenPropertyTrees",
                    "--disable-gpu",
                    "--disable-ipc-flooding-protection",
                    "--disable-renderer-backgrounding",
                    "--disable-setuid-sandbox",
                    "--enable-features=NetworkService,NetworkServiceInProcess",
                    "--force-color-profile=srgb",
                    "--hide-scrollbars",
                    "--metrics-recording-only",
                    "--mute-audio",
                    "--no-first-run",
                    "--no-sandbox",
                    "--no-zygote"
                };
            if (_os != "Win32NT") args.Add("--single-process");
            return args.ToArray();
        }
    }
    public class DOMRect
    {
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Top { get; set; }
        public decimal Right { get; set; }
        public decimal Bottom { get; set; }
        public decimal Left { get; set; }
    }
}

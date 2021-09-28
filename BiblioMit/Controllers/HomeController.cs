using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BiblioMit.Models;
using Microsoft.AspNetCore.Authorization;
using BiblioMit.Models.PostViewModels;
using BiblioMit.Models.ForumViewModels;
using System;
using BiblioMit.Models.HomeViewModels;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Globalization;
using BiblioMit.Services;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using BiblioMit.Models.VM;
using System.Threading.Tasks;
using BiblioMit.Extensions;
using BiblioMit.Data;
using Microsoft.EntityFrameworkCore;
using BiblioMit.Services.Interfaces;
using System.Text.RegularExpressions;

namespace BiblioMit.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPuppet _puppet;
        private readonly IBannerService _banner;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IPost _postService;
        private readonly INodeService _nodeService;

        public HomeController(
            IPuppet puppet,
            IBannerService banner,
            IStringLocalizer<HomeController> localizer,
            IPost postService,
            INodeService nodeService,
            ApplicationDbContext context
            )
        {
            _banner = banner;
            _context = context;
            _puppet = puppet;
            _localizer = localizer;
            _postService = postService;
            _nodeService = nodeService;
        }
        [HttpGet]
        public IActionResult Flowpaper(string n)
        {
            //var name = Request.Path.Value.Split("/").Last();
            var locale = _localizer["en_US"].Value;
            var model = n switch
            {
                "gallery" => new Flowpaper { Name = "colecci-n-virtual", Reload = 1516301843374, LocaleChain = locale },
                _ => new Flowpaper { Name = "MANUAL_DE_USO_BIBLIOMIT", Reload = 1512490982155, LocaleChain = locale }
            };
            return View("Flowpaper", model);
        }
        [HttpGet]
        public IActionResult GetBanner(string f)
        {
            var name = Regex.Replace(f, ".*/", "");

            var full = Path.Combine(Directory.GetCurrentDirectory(),
                                    "BannerImgs", name);

            return PhysicalFile(full, "image/jpg");
        }
        [HttpGet]
        public IActionResult Manual()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Survey()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Terms()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Responses()
        {
            var uri = new Uri("https://docs.google.com/forms/d/e/1FAIpQLSdtgpabkbTL8eXZ1PJuyNzEkyAtX_eIdX7_84cO6aAMHxUKyQ/viewanalytics");
            var page = await _puppet
                        .GetPageAsync(uri)
                        .ConfigureAwait(false);
            var model = await page.GetContentAsync().ConfigureAwait(false);
                return View("Responses", model);
        }
        [HttpGet]
        public IActionResult Analytics()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GetAnalyticsDataMonth()
        {
            using var service = GetService();
            var st = new DateTime(2018, 8, 28);
            var now = DateTime.Now;
            var cnt = new Dictionary<string,int>();

            foreach (var request in Enumerable
                .Range(0, 1 + (now.Year - st.Year) / 2)
                .Select(offset => {
                    var end = st.AddYears((offset + 1) * 2);
                    var enddate = end > now ? "today" :
                    end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    return new GetReportsRequest
                    {
                        ReportRequests = new[]
                        {
                                new ReportRequest
                                {
                                    ViewId = "180792983",
                                    Metrics = new[] { new Metric { Expression = "ga:entrances" } },
                                    Dimensions = new[]
                                    {
                                        new Dimension { Name = "ga:landingPagePath" },
                                        new Dimension { Name = "ga:date" }
                                    },
                                    DateRanges = new[] { new DateRange {
                                    StartDate = st.AddYears(offset*2).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                                    EndDate =  enddate } },
                                    OrderBys = new[] { new OrderBy { FieldName = "ga:date", SortOrder = "ASCENDING" } }
                                }
                    }
                    };
                }))
            {
                var batchRequest = service.Reports.BatchGet(request);
                var response = batchRequest.Execute();

                foreach (var r in response.Reports.First().Data.Rows)
                {
                    var date = r.Dimensions[1][0..^2].Insert(4, "-");
                    if (cnt.ContainsKey(date))
                    {
                        cnt[date] += int.Parse(r.Metrics.First().Values.First(), CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        cnt[date] = 0;
                    }
                }
            }

            //response.Reports.SelectMany(r => r.Data.Rows
            //.Select(r =>
            //{
            //    DateTime.TryParseExact(r.Dimensions[1], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d);
            //    return new
            //    {
            //        month = d.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            //        date = d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            //        views = int.Parse(r.Metrics.First().Values.First(), CultureInfo.InvariantCulture)
            //    };
            //}));;
            return Json(cnt.Select(x => new AmData
            { Date = x.Key, Value = x.Value }));
            //return freq switch
            //{
            //    "day" => Json(logins),
            //    "month" =>
            //        Json(logins.GroupBy(l => l.month).Select(g => new { date = g.Key, views = g.Sum(s => s.views) })),
            //    _ => Json(logins.Sum(l => l.views))
            //};
        }
        [HttpGet]
        public IActionResult GetAnalyticsData()
        {
            using var service = GetService();
            var st = new DateTime(2018, 8, 28);
            var now = DateTime.Now;
            var cnt = 0;

            foreach (var request in Enumerable
                .Range(0, 1 + (now.Year - st.Year) / 2)
                .Select(offset => {
                    var end = st.AddYears((offset + 1) * 2);
                    var enddate = end > now ? "today" : 
                    end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    return new GetReportsRequest
                    {
                        ReportRequests = new[]
                        {
                                new ReportRequest
                                {
                                    ViewId = "180792983",
                                    Metrics = new[] { new Metric { Expression = "ga:entrances" } },
                                    Dimensions = new[]
                                    {
                                        new Dimension { Name = "ga:landingPagePath" },
                                        new Dimension { Name = "ga:date" }
                                    },
                                    DateRanges = new[] { new DateRange {
                                    StartDate = st.AddYears(offset*2).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                                    EndDate =  enddate } },
                                    OrderBys = new[] { new OrderBy { FieldName = "ga:date", SortOrder = "ASCENDING" } }
                                }
                    }
                    };
                }))
            {
                var batchRequest = service.Reports.BatchGet(request);
                var response = batchRequest.Execute();

                foreach(var r in response.Reports.First().Data.Rows)
                {
                    cnt += int.Parse(r.Metrics.First().Values.First(), CultureInfo.InvariantCulture);
                }
            }

            //response.Reports.SelectMany(r => r.Data.Rows
            //.Select(r =>
            //{
            //    DateTime.TryParseExact(r.Dimensions[1], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d);
            //    return new
            //    {
            //        month = d.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            //        date = d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            //        views = int.Parse(r.Metrics.First().Values.First(), CultureInfo.InvariantCulture)
            //    };
            //}));


            return Json(cnt);
            //return freq switch
            //{
            //    "day" => Json(logins),
            //    "month" =>
            //        Json(logins.GroupBy(l => l.month).Select(g => new { date = g.Key, views = g.Sum(s => s.views) })),
            //    _ => Json(logins.Sum(l => l.views))
            //};
        }

        public static AnalyticsReportingService GetService()
        {
            var credential = GetCredential();

            return new AnalyticsReportingService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "BiblioMit",
            });
        }

        public static GoogleCredential GetCredential()
        {
            using var stream = new FileStream("BiblioMit-cb7f4de3a209.json", FileMode.Open, FileAccess.Read);
            return GoogleCredential.FromStream(stream)
.CreateScoped(AnalyticsReportingService.Scope.AnalyticsReadonly);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public string Translate(string text, string to)
        {
            var translated = _nodeService.Run("./wwwroot/js/translate.js", new string[] { text, to });
            return translated;
        }
		
        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Simac()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await _banner.GetCarouselAsync().ConfigureAwait(false);
            return View(model);
        }

        [HttpGet]
        public IActionResult Forum()
        {
            var model = BuildHomeIndexModel();
            return View(model);
        }
        [HttpGet]
        public IActionResult Results(string searchQuery)
        {
            var posts = _postService.GetFilteredPosts(searchQuery);
            var noResults = (!string.IsNullOrEmpty(searchQuery) && !posts.Any());
            var postListings = posts.Select(p => new PostListingModel
            {
                Id = p.Id,
                AuthorId = p.User.Id,
                AuthorName = p.User.UserName,
                AuthorRating = p.User.Rating,
                Title = p.Title,
                DatePosted = p.Created.ToString(CultureInfo.InvariantCulture),
                RepliesCount = p.Replies.Count(),
                Forum = BuildForumListing(p)
            });

            var model = new SearchResultModel
            {
                Posts = postListings,
                SearchQuery = searchQuery,
                EmptySearchResults = noResults
            };
            return View(model);
        }

        private static ForumListingModel BuildForumListing(Post p)
        {
            var forum = p.Forum;
            return new ForumListingModel
            {
                Id = forum.Id,
                ImageUrl = forum.ImageUrl,
                Name = forum.Title,
                Description = forum.Description
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(string searchQuery)
        {
            return RedirectToAction("Results", new { searchQuery });
        }

        private HomeIndexModel BuildHomeIndexModel()
        {
            return new HomeIndexModel
            {
                LatestPosts = _postService.GetLatestsPosts(5).Select(p => new PostListingModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    AuthorId = p.UserId,
                    AuthorName = p.User.Name,
                    AuthorRating = p.User.Rating,
                    DatePosted = p.Created.ToString(CultureInfo.InvariantCulture),
                    RepliesCount = p.Replies.Count(),
                    Forum = GetForumListingForPost(p)
                }),
                SearchQuery = string.Empty
            };
        }

        private static ForumListingModel GetForumListingForPost(Post post)
        {
            var forum = post.Forum;
            return new ForumListingModel
            {
                Name = forum.Title,
                Id = forum.Id,
                ImageUrl = forum.ImageUrl
            };
        }
        [HttpGet]
        public IActionResult About()
        {
            ViewData["Message"] = _localizer["About BiblioMit"];

            return View();
        }
        [HttpGet]
        public IActionResult Contact()
        {
            ViewData["Message"] = _localizer["Contact"];

            return View();
        }
        [HttpGet]

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult SetLanguage(string culture, Uri returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(culture))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { 
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        HttpOnly = true,
                        Path = "/",
                        Secure = true
                    }
                );
            }

            if(returnUrl == null)
            {
                returnUrl = new Uri("~/", UriKind.Relative);
            }

            return LocalRedirect(returnUrl.ToString());
        }
    }
    public class VisitCount
    {
        public string Date { get; set; }
        public int Views { get; set; }
    }
}

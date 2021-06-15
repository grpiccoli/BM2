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
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using BiblioMit.Extensions;

namespace BiblioMit.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IPuppet _puppet;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IPost _postService;
        private readonly INodeService _nodeService;

        public HomeController(
            IPuppet puppet,
            IStringLocalizer<HomeController> localizer,
            IPost postService,
            INodeService nodeService
            )
        {
            _puppet = puppet;
            _localizer = localizer;
            _postService = postService;
            _nodeService = nodeService;
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
        public IActionResult Index()
        {
            var model = new Collection<Banner>
            {
                new Banner
                {
                    Imgs = new Collection<Img>
                    {
                        new Img
                        {
                            FileName = "../images/backgrounds/87a36c4e7ac349ad932a71428ddb07a4_th.jpg",
                            Size = Size.xxl
                        }
                    },
                    Texts = new Collection<Text>
                    {
                        new Text
                        {
                            Title = "Place your ad here",
                            Subtitle = "Contact us",
                            Lang = Lang.eng,
                            Btns = new Collection<Btn>
                            {
                                new Btn
                                {
                                    Title = "jefedeproyectos@intemit.cl",
                                    Uri = new Uri("mail:jefedeproyectos@intemit.cl")
                                },
                                new Btn
                                {
                                    Title = "+56 65 253 1609",
                                    Uri = new Uri("tel:+56652531609")
                                }
                            }
                        },
                        new Text
                        {
                            Title = "Publicita tu empresa aquí",
                            Subtitle = "Contactanos",
                            Lang = Lang.esp,
                            Btns = new Collection<Btn>
                            {
                                new Btn
                                {
                                    Title = "jefedeproyectos@intemit.cl",
                                    Uri = new Uri("mail:jefedeproyectos@intemit.cl")
                                },
                                new Btn
                                {
                                    Title = "+56 65 253 1609",
                                    Uri = new Uri("tel:+56652531609")
                                }
                            }
                        }
                    }
                },
                new Banner
                {
                    Imgs = new Collection<Img>
                    {
                        new Img
                        {
                            FileName = "../images/backgrounds/mussels-shells-mytilus-watt-area-53131.min.jpg",
                            Size = Size.xxl
                        }
                    },
                    Texts = new Collection<Text>
                    {
                        new Text
                        {
                            Title = "Bibliomit",
                            Subtitle = "Electronic Library and Platform for Digital Management of Mytilus chilensis Resource.",
                            Lang = Lang.eng
                        },
                        new Text
                        {
                            Title = "Bibliomit",
                            Subtitle = "Plataforma y biblioteca electrónica para el manejo digital del recurso Mytilus chilensis",
                            Lang = Lang.esp
                        }
                    }
                }
            };
            model.Shuffle();

            var modelo = new Carousel();

            foreach(var item in model.Select((value, i) => new { i, value }))
            {
                var mask = "background-color: rgba(0, 0, 0, 0.6)";
                var active = item.i == 0 ? "active" : "";
                var lang = _localizer["eng"].Value;
                var text = item.value.Texts.First(t => t.Lang.ToString() == lang);
                modelo.Indicators += @$"<li data-mdb-target=""#introCarousel"" data-mdb-slide-to=""{item.i}"" class=""{active}""></li>";
                var btns = "";
                if (item.value.Rgbs != null && item.value.Rgbs.Any())
                {
                    if (item.value.Rgbs.Count > 1 && string.IsNullOrWhiteSpace(item.value.MaskAngle))
                    {
                        var rgbas = string.Join(",", item.value.Rgbs.Select(r => $"rgba({r.R}, {r.G}, {r.B}, 0.6)"));
                        mask = $"background: linear-gradient({item.value.MaskAngle}, {rgbas})";
                    }
                    else
                    {
                        var first = item.value.Rgbs.First();
                        mask = $"background-color: rgba({first.R}, {first.G}, {first.B}, 0.6)";
                    }
                }
                if(text.Btns != null && text.Btns.Any())
                {
                    btns = string.Join("", text.Btns.Select(b => @$"<a class=""btn btn-outline-light btn-lg m-2"" href=""{b.Uri}"";
role=""button"" rel=""nofollow"" target=""_blank"">{b.Title}</a>"));
                }
                mask = $@".banner-{item.i} .mask{{{mask};}}";
                modelo.Styles += string.Join(" ",item.value.Imgs.Select(i => $@"@media (max-width: {(int)i.Size}px){{ 
.banner-{item.i}{{
background-image: url('{i.FileName}') 
}}
}}"));
                modelo.Styles += mask;
                modelo.Items += @$"<div class=""carousel-item banner-{item.i} {active}"">
                     <div class=""mask"">
                           <div class=""d-flex justify-content-center align-items-center h-100"">
                                <div class=""text-white text-center"">
                                     <h1 class=""mb-3"">{text.Title}</h1>
                                     <h5 class=""mb-4"">{text.Subtitle}</h5>
                                        {btns}
                                </div>
                           </div>
                      </div>
                   </div>";
            }
            return View(modelo);
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
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
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
    public class Banner
    {
        public Collection<Img> Imgs { get; internal set; }
        public string MaskAngle { get; set; }
        public Collection<Text> Texts { get; internal set; }
        public Collection<Rgb> Rgbs { get; internal set; }
    }
    public class Text
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public Collection<Btn> Btns { get; internal set; }
        public Lang Lang { get; set; }
    }
    public class Btn
    {
        public string Title { get; set; }
        public Uri Uri { get; set; }
    }
    public class Rgb
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }
    public class Img
    {
        public Size Size { get; set; }
        public string FileName { get; set; }
    }
    public enum Lang
    {
        None,
        eng,
        esp
    }
    public enum Size
    {
        None,
        xs = 576,
        sm = 768,
        md = 992,
        lg = 1200,
        xl = 1400,
        xxl = 3800
    }
    public class Carousel
    {
        public string Indicators { get; set; }
        public string Items { get; set; }
        public string Styles { get; set; }
    }
}

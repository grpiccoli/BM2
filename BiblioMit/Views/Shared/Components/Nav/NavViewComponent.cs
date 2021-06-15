using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;

namespace BiblioMit.Views.Shared.Components.Nav
{
    public class NavViewComponent : ViewComponent
    {
        private readonly IStringLocalizer<NavViewComponent> Localizer;
        public NavViewComponent(IStringLocalizer<NavViewComponent> localizer)
        {
            Localizer = localizer;
        }
        public IViewComponentResult Invoke()
        {
            var links = new NavDDwnVM
            {
                Controller = "Home",
                Action = "Index",
                Logo = "fas fa-link",
                Title = Localizer["Links"].Value,
                Sections = new Collection<Section>
                {
                    new Section
                    {
                        Name = Localizer["Main"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Home",
                                Action = "Index",
                                Name = Localizer["Home"].Value,
                                Icon = "fas fa-home"
                            },
                            new Link
                            {
                                Controller = "Home",
                                Action = "About",
                                Name = Localizer["About us"].Value,
                                Icon = "fas fa-question"
                            },
                            new Link
                            {
                                Controller = "Home",
                                Action = "Contact",
                                Name = Localizer["Contact"].Value,
                                Icon = "fas fa-address-book"
                            }
                        }
                    },
                    new Section
                    {
                        Name = Localizer["Information"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Home",
                                Action = "Manual",
                                Name = Localizer["User Manual"].Value,
                                Icon = "far fa-question-circle"
                            },
                            new Link
                            {
                                Controller = "Home",
                                Action = "Privacy",
                                Name = Localizer["Privacy Policy"].Value,
                                Icon = "fas fa-shield-alt"
                            },
                            new Link
                            {
                                Controller = "Home",
                                Action = "Terms",
                                Name = Localizer["Terms & Conditions"].Value,
                                Icon = "fas fa-balance-scale"
                            }
                        }
                    }
                }
            };
            var producers = new NavDDwnVM
            {
                Controller = "Centres",
                Action = "Producers",
                Logo = "fas fa-map-marker-alt",
                Title = Localizer["Maps"].Value,
                Sections = new Collection<Section>
                {
                    new Section
                    {
                        Name = Localizer["Maps"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Ambiental",
                                Action = "Graph",
                                Name = Localizer["PSMBs"].Value,
                                Icon = "fas fa-water"
                            },
                            new Link
                            {
                                Controller = "Centres",
                                Action = "Producers",
                                Name = Localizer["Aquaculture Farms"].Value,
                                Icon = "fas fa-industry"
                            },
                            new Link
                            {
                                Controller = "Centres",
                                Action = "Research",
                                Name = Localizer["Research Centres"].Value,
                                Icon = "fas fa-microscope"
                            }
                        }
                    }
                }
            };
            var boletin = new NavDDwnVM
            {
                Controller = "Boletin",
                Action = "Index",
                Logo = "fas fa-chart-line",
                Title = Localizer["Reports"].Value,
                Sections = new Collection<Section>
                {
                    new Section
                    {
                        Name = Localizer["Publications"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Boletin",
                                Action = "Index",
                                Name = Localizer["Production / Environmental"].Value,
                                Icon = "fas fa-newspaper"
                            }
                        }
                    },
                    new Section
                    {
                        Name = Localizer["Website Analytics"],
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Home",
                                Action = "Analytics",
                                Name = Localizer["Web Analytics"].Value,
                                Icon = "fas fa-poll"
                            }
                        }
                    },
                    new Section
                    {
                        Name = Localizer["Sistemas"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Home",
                                Action = "Simac",
                                Name = "SIMAC",
                                Icon = "fab fa-centos"
                            }
                        }
                    }
                }
            };
            var publications = new NavDDwnVM
            {
                Controller = "Publications",
                Action = "Index",
                Logo = "fas fa-search",
                Title = Localizer["Search"].Value,
                Sections = new Collection<Section>
                {
                    new Section
                    {
                        Name = Localizer["Engines"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Publications",
                                Action = "Index",
                                Name = Localizer["Library"].Value,
                                Icon = "fas fa-book-reader"
                            },
                            new Link
                            {
                                Controller = "Contacts",
                                Action = "Index",
                                Name = Localizer["Contacts"].Value,
                                Icon = "far fa-address-book"
                            },
                            new Link
                            {
                                Controller = "Publications",
                                Action = "Agenda",
                                Name = Localizer["Funding"].Value,
                                Icon = "fas fa-hand-holding-usd"
                            },
                            //, "modal", "#modal-action"
                            new Link
                            {
                                Controller = "Home",
                                Action = "Search",
                                Name = Localizer["Website search"].Value,
                                Icon = "fas fa-search"
                            }
                        }
                    }
                }
            };
            var gallery = new NavDDwnVM
            {
                Controller = "Photos",
                Action = "Gallery",
                Logo = "fas fa-images",
                Title = Localizer["Images"].Value,
                Sections = new Collection<Section>
                {
                    new Section
                    {
                        Name = Localizer["Catalogue"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Photos",
                                Action = "Gallery",
                                Name = Localizer["Histopathology Gallery"].Value,
                                Icon = "fas fa-disease"
                            }
                        }
                    }
                }
            };
            var forum = new NavDDwnVM
            {
                Controller = "Home",
                Action = "Forum",
                Logo = "far fa-comment-dots",
                Title = Localizer["Networking"].Value,
                Sections = new Collection<Section>
                {
                    new Section
                    {
                        Name = Localizer["Forums"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Manage",
                                Action = "Index",
                                Name = Localizer["Profile"].Value,
                                Icon = "fas fa-user-edit"
                            },
                            new Link()
                            {
                                Controller = "Home",
                                Action = "Forum",
                                Name = Localizer["Forums"].Value,
                                Icon = "fas fa-comments"
                            }
                        }
                    },
                    new Section
                    {
                        Name = Localizer["Survey"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Home",
                                Action = "Survey",
                                Name = Localizer["Tell us what you think"].Value,
                                Icon = "fas fa-user-edit"
                            },
                            new Link()
                            {
                                Controller = "Home",
                                Action = "Responses",
                                Name = Localizer["Responses"].Value,
                                Icon = "fas fa-comments"
                            }
                        }
                    }
                }
            };
            var tools = new Collection<Section>();
            var claims = ((ClaimsIdentity)User.Identity).Claims;
            var admin = claims.Any(c => c.Value == "webmaster");

            if (User.Identity.IsAuthenticated)
            {
                var access = admin || User.IsInRole("Editor");

                if (admin || claims.Any(c => c.Value == "per" || c.Value == "sernapesca" || c.Value == "intemit"))
                {
                    var adminstrings = new Collection<Link>
                    {
                        new Link
                        {
                            Controller = "Columnas", 
                            Action = "Index", 
                            Name = Localizer["Input Format"].Value
                        }
                    };
                    if (admin || claims.Any(c => c.Value == "per" || c.Value == "sernapesca"))
                    {
                        adminstrings.Add(
                        new Link 
                        { 
                            Controller = "Entries", 
                            Action = "Index", 
                            Name = Localizer["Uploaded Files"].Value
                        }
                        );
                        if (User.IsInRole("Administrador"))
                        {
                            boletin.Sections.Add(
                                new Section
                                {
                                    Name = Localizer["Prod Uploads"].Value,
                                    Links = new Collection<Link>
                                    {
                                        new Link
                                        {
                                            Controller = "Entries",
                                            Action = "Create",
                                            Name = Localizer["Production"].Value
                                        }
                                    }
                                });
                        }
                    }
                    boletin.Sections.Add(new Section
                    {
                        Name = Localizer["Administration"].Value,
                        Links = adminstrings
                    });
                }

                if (admin || claims.Any(c => c.Value == "intemit"))
                {
                    boletin.Sections.Add(new Section
                    {
                        Name = Localizer["Env Uploads"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Entries",
                                Action = "CreateFito",
                                Name = Localizer["Environmental"].Value
                            }
                        }
                    });
                    boletin.Sections.Add(new Section
                    {
                        Name = Localizer["Graphs"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Ambiental",
                                Action = "Graph",
                                Name = Localizer["PSMB statistics"].Value
                            }
                            }
                    });
                    producers.Sections.Add(new Section
                    {
                        Name = Localizer["Reports"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Ambiental",
                                Action = "PullPlankton",
                                Name = Localizer["Upload Errors"].Value,
                                Icon = "fas fa-exclamation-triangle"
                            }
                        }
                    });
                }

                if (admin)
                {
                    forum.Sections.Add(new Section
                    {
                        Name = Localizer["Administration"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Fora",
                                Action = "Create",
                                Name = Localizer["Create Forum"].Value
                            }
                        }
                    });

                    tools.Add(new Section
                    {
                        Name = Localizer["Databases"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Companies",
                                Action = "Index",
                                Name = Localizer["Companies and Institutions"].Value
                            },
                            new Link
                            {
                                Controller = "Centres",
                                Action = "Index",
                                Name = Localizer["Centres"].Value
                            },
                            new Link
                            {
                                Controller = "Coordinates",
                                Action = "Index",
                                Name = Localizer["Coordinates"].Value
                            },
                            new Link
                            {
                                Controller = "Productions",
                                Action = "Index",
                                Name = Localizer["Productions"].Value
                            },
                            new Link
                            {
                                Controller = "Contacts",
                                Action = "Index",
                                Name = Localizer["Contacts"].Value
                            }
                        }
                    });

                    tools.Add(new Section
                    {
                        Name = Localizer["Users"].Value,
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "User",
                                Action = "Index",
                                Name = Localizer["Users and claims"].Value
                            },
                            new Link
                            {
                                Controller = "AppRole",
                                Action = "Index",
                                Name = Localizer["Roles"].Value
                            }
                        }
                    });
                }

                if (admin || claims.Any(c => c.Value == "mitilidb"))
                {
                    gallery.Sections.Add(new Section
                    {
                        Name = Localizer["Administration"].Value, 
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Samplings", 
                                Action = "Index", 
                                Name = Localizer["Samplings"].Value
                            },
                            new Link
                            {
                                Controller = "Individuals", 
                                Action = "Index", 
                                Name = Localizer["Subjects"].Value
                            },
                            new Link
                            {
                                Controller = "Softs", 
                                Action = "Index", 
                                Name = Localizer["Softs"].Value
                            }
                        }
                    });

                    gallery.Sections.Add(new Section
                    {
                        Name = Localizer["Images"].Value, 
                        Links = new Collection<Link>
                        {
                            new Link
                            {
                                Controller = "Photos",
                                Action = "Index",
                                Name = Localizer["Image gallery"].Value
                            }
                        }
                    });
                }
            }

            var model = new List<NavDDwnVM>
            {
                links, producers, boletin, publications, gallery, forum
            };

            if (admin)
            {
                var adminTools = new NavDDwnVM
                {
                    Controller = "User",
                    Action = "Index",
                    Logo = "fas fa-tools",
                    Title = Localizer["Administration"].Value,
                    Sections = tools
                };
                model.Add(adminTools);
            }

            return View(model);
        }
    }
    public class NavDDwnVM
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Logo { get; set; }
        public string Title { get; set; }
        public Collection<Section> Sections { get; internal set; } = new Collection<Section>();
    }
    public class Section
    {
        public string Name { get; set; }
        public Collection<Link> Links { get; internal set; } = new Collection<Link>();
    }
    public class Link
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
    }
}

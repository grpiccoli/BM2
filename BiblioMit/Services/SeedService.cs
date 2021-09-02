using BiblioMit.Extensions;
using BiblioMit.Data;
using BiblioMit.Models;
using BiblioMit.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Pluralize.NET.Core;
using BiblioMit.Models.Entities.Digest;
using BiblioMit.Models.Entities.Environmental;
using BiblioMit.Services.Interfaces;
using System.Globalization;
using BiblioMit.Models.Entities.Environmental.Plancton;
using System.Security.Claims;
using BiblioMit.Models.Entities.Ads;

namespace BiblioMit.Services
{
    public class SeedService : ISeed
    {
        private readonly IImport _import;
        private readonly IUpdateJsons _update;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly UserManager<ApplicationUser> _userManager;
        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _environment;
        private readonly string _os;
        private readonly string _conn;
        private readonly ApplicationDbContext _context;
        private readonly ILookupNormalizer _normalizer;
        public SeedService(
            ILogger<SeedService> logger,
            IImport import,
            IStringLocalizer<SeedService> localizer,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILookupNormalizer normalizer,
            IUpdateJsons update
            )
        {
            _update = update;
            _import = import;
            _logger = logger;
            _userManager = userManager;
            _localizer = localizer;
            Configuration = configuration;
            _environment = environment;
            _os = Environment.OSVersion.Platform.ToString();
            _conn = Configuration.GetConnectionString($"{_os}Connection");
            _context = context;
            _normalizer = normalizer;
        }
        public async Task Seed()
        {
            try
            {
                await Users().ConfigureAwait(false);
                await AddProcedures().ConfigureAwait(false);
                var adminId = _context.ApplicationUsers
                    .Where(u => u.Email == "contacto@epicsolutions.cl")
                    .SingleOrDefault().Id;

                var tsvPath = Path
                    .Combine(_environment.ContentRootPath, "Data", "Fora");
                if (!_context.Forums.Any())
                    await Insert<Forum>(tsvPath).ConfigureAwait(false);
                if (!_context.Posts.Any())
                {
                    await Insert<Post>(tsvPath).ConfigureAwait(false);
                    //Post
                    await _context.Posts.ForEachAsync(p => p.UserId = adminId).ConfigureAwait(false);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }

                tsvPath = Path
                    .Combine(_environment.ContentRootPath, "Data", "Centres");
                if (!_context.Localities.Any())
                    await Insert<Locality>(tsvPath).ConfigureAwait(false);
                if (!_context.AreaCodes.Any())
                    await Insert<AreaCode>(tsvPath).ConfigureAwait(false);
                if (!_context.AreaCodeProvinces.Any())
                    await Insert<AreaCodeProvince>(tsvPath).ConfigureAwait(false);
                if (!_context.CatchmentAreas.Any())
                    await Insert<CatchmentArea>(tsvPath).ConfigureAwait(false);
                if (!_context.Psmbs.Any())
                    await Insert<Psmb>(tsvPath).ConfigureAwait(false);
                if (!_context.Companies.Any())
                    await Insert<Company>(tsvPath).ConfigureAwait(false);
                if (!_context.Products.Any())
                    await Insert<Product>(tsvPath).ConfigureAwait(false);
                if (!_context.PlantProducts.Any())
                    await Insert<PlantProduct>(tsvPath).ConfigureAwait(false);
                if (!_context.Contacts.Any())
                {
                    await Insert<Contact>(tsvPath).ConfigureAwait(false);
                    //Contacts
                    await _context.Contacts.ForEachAsync(c => c.OwnerId = adminId).ConfigureAwait(false);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                if (!_context.Polygons.Any())
                    await Insert<Polygon>(tsvPath).ConfigureAwait(false);
                if (!_context.Coordinates.Any())
                    await Insert<Coordinate>(tsvPath).ConfigureAwait(false);

                tsvPath = Path
                    .Combine(_environment.ContentRootPath, "Data", "Histopathology");
                if (!_context.Samplings.Any())
                    await Insert<Sampling>(tsvPath).ConfigureAwait(false);
                if (!_context.Individuals.Any())
                    await Insert<Individual>(tsvPath).ConfigureAwait(false);
                if (!_context.Softs.Any())
                    await Insert<Soft>(tsvPath).ConfigureAwait(false);
                if (!_context.Photos.Any())
                    await Insert<Photo>(tsvPath).ConfigureAwait(false);
                
                tsvPath = Path
                    .Combine(_environment.ContentRootPath, "Data", "FileUploading");
                if (!_context.InputFiles.Any())
                    await Insert<InputFile>(tsvPath).ConfigureAwait(false);
                if (!_context.Registries.Any())
                    await Insert<Registry>(tsvPath).ConfigureAwait(false);
                if (!_context.Headers.Any())
                    await Insert<Header>(tsvPath).ConfigureAwait(false);

                tsvPath = Path
                    .Combine(_environment.ContentRootPath, "Data", "Digest");
                if (!_context.Origins.Any())
                    await Insert<Origin>(tsvPath).ConfigureAwait(false);
                if (!_context.SernapescaDeclarations.Any())
                    await Insert<SernapescaDeclaration>(tsvPath).ConfigureAwait(false);
                if (!_context.DeclarationDates.Any())
                    await Insert<DeclarationDate>(tsvPath).ConfigureAwait(false);
                if (!_context.Entries.Any())
                {
                    await Insert<Entry>(tsvPath).ConfigureAwait(false);
                    //Products
                    await _context.Entries.ForEachAsync(p => p.ApplicationUserId = adminId).ConfigureAwait(false);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }

                tsvPath = Path
                    .Combine(_environment.ContentRootPath, "Data", "Environmental");
                if (!_context.Analists.Any())
                    await Insert<Analist>(tsvPath).ConfigureAwait(false);
                if (!_context.Emails.Any())
                    await Insert<Email>(tsvPath).ConfigureAwait(false);
                if (!_context.GenusPhytoplanktons.Any())
                    await Insert<GenusPhytoplankton>(tsvPath).ConfigureAwait(false);
                if (!_context.Laboratories.Any())
                    await Insert<Laboratory>(tsvPath).ConfigureAwait(false);
                if (!_context.Phones.Any())
                    await Insert<Phone>(tsvPath).ConfigureAwait(false);
                if (!_context.PhylogeneticGroups.Any())
                    await Insert<PhylogeneticGroup>(tsvPath).ConfigureAwait(false);
                if (!_context.Phytoplanktons.Any())
                    await Insert<Phytoplankton>(tsvPath).ConfigureAwait(false);
                if (!_context.PlanktonAssays.Any())
                    await Insert<PlanktonAssay>(tsvPath).ConfigureAwait(false);
                if (!_context.PlanktonAssayEmails.Any())
                    await Insert<PlanktonAssayEmail>(tsvPath).ConfigureAwait(false);
                if (!_context.SamplingEntities.Any())
                    await Insert<SamplingEntity>(tsvPath).ConfigureAwait(false);
                if (!_context.SpeciesPhytoplanktons.Any())
                    await Insert<SpeciesPhytoplankton>(tsvPath).ConfigureAwait(false);
                if (!_context.Stations.Any())
                    await Insert<Station>(tsvPath).ConfigureAwait(false);
                if (!_context.PlanktonUsers.Any())
                    await Insert<PlanktonUser>(tsvPath).ConfigureAwait(false);

                tsvPath = Path
                    .Combine(_environment.ContentRootPath, "Data", "Ads");
                if (!_context.Banners.Any())
                    await Insert<Banner>(tsvPath).ConfigureAwait(false);
                if (!_context.Btns.Any())
                    await Insert<Btn>(tsvPath).ConfigureAwait(false);
                if (!_context.Captions.Any())
                    await Insert<Caption>(tsvPath).ConfigureAwait(false);
                if (!_context.Imgs.Any())
                    await Insert<Img>(tsvPath).ConfigureAwait(false);
                if (!_context.Rgbs.Any())
                    await Insert<Rgb>(tsvPath).ConfigureAwait(false);

                tsvPath = Path
                    .Combine(_environment.ContentRootPath, "Data", "Semaforo");
                if (!_context.Spawnings.Any())
                    await Insert<Spawning>(tsvPath).ConfigureAwait(false);
                if (!_context.ReproductiveStages.Any())
                    await Insert<ReproductiveStage>(tsvPath).ConfigureAwait(false);
                if (!_context.Species.Any())
                    await Insert<Specie>(tsvPath).ConfigureAwait(false);
                if (!_context.SpecieSeeds.Any())
                    await Insert<SpecieSeed>(tsvPath).ConfigureAwait(false);
                if (!_context.Seeds.Any())
                    await Insert<Seed>(tsvPath).ConfigureAwait(false);
                if (!_context.Tallas.Any())
                    await Insert<Talla>(tsvPath).ConfigureAwait(false);
                if (!_context.Larvaes.Any())
                    await Insert<Larvae>(tsvPath).ConfigureAwait(false);
                if (!_context.Larvas.Any())
                    await Insert<Larva>(tsvPath).ConfigureAwait(false);
                var path = "UNSYNC/bibliomit/DB";
                if(Directory.Exists(path))
                    await AddBulkFiles(path).ConfigureAwait(false);
                CultureInfo.CurrentUICulture = new CultureInfo("en");
                _update.SeedUpdate();
                CultureInfo.CurrentUICulture = new CultureInfo("es");
                _update.SeedUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer["There has been an error while seeding the database."]);
                throw;
            }
        }
        public async Task AddProcedures()
        {
            string query = "select * from sysobjects where type='P' and name='BulkInsert'";
            var sp = @"CREATE PROCEDURE BulkInsert(@TableName NVARCHAR(50), @Tsv NVARCHAR(100))
AS
BEGIN 
DECLARE @SQLSelectQuery NVARCHAR(MAX)=''
DECLARE @HasIdentity bit
SET @SQLSelectQuery = N'SELECT @HasIdentity=OBJECTPROPERTY(OBJECT_ID('''+@TableName+'''), ''TableHasIdentity'')'
  exec sp_executesql @SQLSelectQuery, N'@HasIdentity bit out', @HasIdentity out
IF @HasIdentity = 1
	BEGIN
    SET @SQLSelectQuery = 'SET IDENTITY_INSERT '+@TableName+' ON'
	exec(@SQLSelectQuery)
	END
SET @SQLSelectQuery = 'BULK INSERT ' + @TableName + ' FROM ' + QUOTENAME(@Tsv) + ' WITH (KEEPIDENTITY, DATAFILETYPE=''widechar'')'
  exec(@SQLSelectQuery)
IF @HasIdentity = 1
	BEGIN
    SET @SQLSelectQuery = 'SET IDENTITY_INSERT '+@TableName+' OFF'
	exec(@SQLSelectQuery)
	END
END";
            bool spExists = false;
            using SqlConnection connection = new(_conn);
            using SqlCommand command = new()
            {
                Connection = connection,
                CommandText = query
            };
            connection.Open();
            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    spExists = true;
                    break;
                }
            }
            if (!spExists)
            {
                command.CommandText = sp;
                using SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    spExists = true;
                    break;
                }
            }
            connection.Close();
        }
        public async Task Insert<TEntity>(string path) where TEntity : class
        {
            var type = typeof(TEntity);
            var name = new Pluralizer().Pluralize(type.Name);
            _context.Database.SetCommandTimeout(10000);
            var tsv = Path.Combine(path, $"{name}.tsv");
            var tmp = Path.Combine(Path.GetTempPath(), $"{name}.tsv");
            if (!File.Exists(tsv)) return;
            File.Copy(tsv, tmp, true);
            var dbo = $"dbo.{name}";
            await _context.Database
                .ExecuteSqlInterpolatedAsync($"BulkInsert {dbo}, {tmp}")
                .ConfigureAwait(false);
            File.Delete(tmp);
            Console.WriteLine($"{name} saved to Database");
            return;
        }
        public async Task Users()
        {
            if (!_context.ApplicationUserRoles.Any() && !_context.Users.Any())
            {
                if (!_context.ApplicationRoles.Any())
                {
                    var aprolls = RoleData.Administrator.Enum2ListNames().Select(r => new ApplicationRole
                    {
                        CreatedDate = DateTime.Now,
                        Name = r,
                        Description = "",
                        NormalizedName = _normalizer.NormalizeName(r)
                    });
                    foreach(var r in aprolls)
                        await _context.ApplicationRoles
                            .AddAsync(r)
                            .ConfigureAwait(false);

                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                var users = new List<UserInitializerVM>();
                var userPer = new UserInitializerVM(
                    //roles
                    new List<string> 
                    {
                        RoleData.Administrator.ToString()
                    }, 
                    //claims
                    new List<string> 
                    {
                        UserClaims.Digest.ToString()
                    })
                {
                    Name = "PER",
                    Email = "javier.aros@mejillondechile.cl",
                    Key = "Per@2018",
                    ImageUri = new Uri("~/images/ico/mejillondechile.svg", UriKind.Relative)
                };
                users.Add(userPer);
                var userMitilidb = new UserInitializerVM(
                    //roles
                    new List<string>
                    {
                        RoleData.Administrator.ToString()
                    },
                    //claims
                    new List<string>
                    {
                        UserClaims.Mitilidb.ToString()
                    })
                {
                    Name = "MytiliDB",
                    Email = "mytilidb@bibliomit.cl",
                    Key = "Sivisam@2016",
                    ImageUri = new Uri("~/images/ico/bibliomit.svg", UriKind.Relative)
                };
                users.Add(userMitilidb);
                var userWebmaster = new UserInitializerVM(
                    //roles
                    RoleData.Administrator.Enum2ListNames(),
                    //claims
                    UserClaims.Banners.Enum2ListNames())
                {
                    Name = "WebMaster",
                    Email = "contacto@epicsolutions.cl",
                    Key = "34#$erERdfDFcvCV",
                    ImageUri = new Uri("~/images/ico/bibliomit.svg", UriKind.Relative),
                    Rating = 10
                };
                users.Add(userWebmaster);
                var userSernapesca = new UserInitializerVM(
                    //roles
                    new List<string>
                    {
                        RoleData.Administrator.ToString()
                    },
                    //claims
                    new List<string>
                    {
                        UserClaims.Digest.ToString()
                    })
                {
                    Name = "Sernapesca",
                    Email = "sernapesca@bibliomit.cl",
                    Key = "Sernapesca@2018",
                    ImageUri = new Uri("~/images/ico/bibliomit.svg", UriKind.Relative)
                };
                users.Add(userSernapesca);
                var userIntemit = new UserInitializerVM(
                    //roles
                    new List<string>
                    {
                        RoleData.Administrator.ToString()
                    },
                    //claims
                    new List<string>
                    {
                        UserClaims.PSMB.ToString(),
                        UserClaims.Contacts.ToString(),
                        UserClaims.Forums.ToString(),
                        UserClaims.Banners.ToString()
                    })
                {
                    Name = "Intemit",
                    Email = "jefedeproyectos@intemit.cl",
                    Key = "Intemit@2018",
                    ImageUri = new Uri("~/images/ico/bibliomit.svg", UriKind.Relative)
                };
                users.Add(userIntemit);
                var hasher = new PasswordHasher<ApplicationUser>();
                foreach (var item in users)
                {
                    var user = new ApplicationUser
                    {
                        UserName = item.Name,
                        NormalizedUserName = _normalizer.NormalizeName(item.Name),
                        Email = item.Email,
                        NormalizedEmail = _normalizer.NormalizeEmail(item.Email),
                        EmailConfirmed = true,
                        LockoutEnabled = false,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ProfileImageUrl = item.ImageUri
                    };
                    IdentityResult userResult = await _userManager.CreateAsync(user, item.Key).ConfigureAwait(false);
                    if (!userResult.Succeeded) throw new InvalidOperationException($"user could not be added {user.Name}");

                    IdentityResult claimIdentityResult = await _userManager
                        .AddClaimsAsync(user, item.Claims.Select(c =>
                    new Claim(c, c)))
                        .ConfigureAwait(false);
                    if (!claimIdentityResult.Succeeded) throw new InvalidOperationException($"claims could not be added to user {user.Name}");

                    IdentityResult rolesIdentityResult = await _userManager
                        .AddToRolesAsync(user, item.Roles).ConfigureAwait(false);
                    if (!rolesIdentityResult.Succeeded) throw new InvalidOperationException($"roles could not be added to user {user.Name}");
                }
                await _context.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
        }
        public async Task<Task> AddBulkFiles(string path)
        {
            DirectoryInfo basePathInfo = Directory.GetParent(_environment.ContentRootPath).Parent;
            string pwd = Path.Combine(basePathInfo.FullName, path);
            try
            {
                return await _import.AddFilesAsync(pwd).ConfigureAwait(false);
            }
            catch(DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, _localizer["There has been an error while seeding the database."]);
                throw;
            }
        }
    }
}
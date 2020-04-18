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

namespace BiblioMit.Services
{
    public class SeedService : ISeed
    {
        private readonly IImport _import;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
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
            ILookupNormalizer normalizer
            )
        {
            _import = import;
            _logger = logger;
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
                    .Where(u => u.Email == "adminmit@bibliomit.cl")
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
                    .Combine(_environment.ContentRootPath, "Data", "Digest");
                if (!_context.InputFiles.Any())
                    await Insert<InputFile>(tsvPath).ConfigureAwait(false);
                if (!_context.Registries.Any())
                    await Insert<Registry>(tsvPath).ConfigureAwait(false);
                if (!_context.Headers.Any())
                    await Insert<Header>(tsvPath).ConfigureAwait(false);
                if (!_context.Origins.Any())
                    await Insert<Origin>(tsvPath).ConfigureAwait(false);
                //if (!_context.SernapescaDeclarations.Any())
                //    await Insert<SernapescaDeclaration>(tsvPath).ConfigureAwait(false);
                //if (!_context.SernapescaEntries.Any())
                //{
                //    await Insert<SernapescaEntry>(tsvPath).ConfigureAwait(false);
                //    //Products
                //    await _context.SernapescaEntries.ForEachAsync(p => p.ApplicationUserId = adminId).ConfigureAwait(false);
                //    await _context.SaveChangesAsync().ConfigureAwait(false);
                //}

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
                await AddBulkFiles().ConfigureAwait(false);
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
            using SqlConnection connection = new SqlConnection(_conn);
            using SqlCommand command = new SqlCommand
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
            return;
        }
        public async Task Users()
        {
            if (!_context.ApplicationUserRoles.Any() && !_context.Users.Any())
            {
                if (!_context.ApplicationRoles.Any())
                {
                    var aprolls = RoleData.AppRoles.Select(r => new ApplicationRole
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
                var userPer = new UserInitializerVM
                {
                    Name = "PER",
                    Email = "javier.aros@mejillondechile.cl",
                    Key = "per2018",
                    ImageUri = new Uri("~/images/ico/mejillondechile.svg", UriKind.Relative)
                };
                userPer.Roles.Add(RoleData.AppRoles.ElementAt(0));
                //userPer.Plataforma.Add(Plataforma.mytilidb);
                userPer.Claims.Add("per");
                users.Add(userPer);
                var userMitilidb = new UserInitializerVM
                {
                    Name = "MytiliDB",
                    Email = "mytilidb@bibliomit.cl",
                    Key = "sivisam2016",
                    ImageUri = new Uri("~/images/ico/bibliomit.svg", UriKind.Relative)
                };
                userMitilidb.Roles.Add(RoleData.AppRoles.ElementAt(0));
                //userMitilidb.Plataforma.Add(Plataforma.mytilidb);
                userMitilidb.Claims.Add("mitilidb");
                users.Add(userMitilidb);
                var userWebmaster = new UserInitializerVM
                {
                    Name = "WebMaster",
                    Email = "adminmit@bibliomit.cl",
                    Key = "34#$erERdfDFcvCV",
                    ImageUri = new Uri("~/images/ico/bibliomit.svg", UriKind.Relative),
                    Rating = 10
                };
                userWebmaster.Roles.AddRange(RoleData.AppRoles);
                //userWebmaster.Plataforma.AddRange(Plataforma.bibliomit.Enum2List());
                userWebmaster.Claims.AddRange(ClaimData.UserClaims);
                users.Add(userWebmaster);
                var userSernapesca = new UserInitializerVM
                {
                    Name = "Sernapesca",
                    Email = "sernapesca@bibliomit.cl",
                    Key = "sernapesca2018",
                    ImageUri = new Uri("~/images/ico/bibliomit.svg", UriKind.Relative)
                };
                userSernapesca.Roles.Add(RoleData.AppRoles.ElementAt(0));
                //userSernapesca.Plataforma.Add(Plataforma.boletin);
                userSernapesca.Claims.Add("sernapesca");
                users.Add(userSernapesca);
                var userIntemit = new UserInitializerVM
                {
                    Name = "Intemit",
                    Email = "intemit@bibliomit.cl",
                    Key = "intemit2018",
                    ImageUri = new Uri("~/images/ico/bibliomit.svg", UriKind.Relative)
                };
                userIntemit.Roles.Add(RoleData.AppRoles.ElementAt(0));
                //userIntemit.Plataforma.Add(Plataforma.psmb);
                userIntemit.Claims.Add("intemit");
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
                    user.PasswordHash = hasher.HashPassword(user, item.Key);

                    user.Claims.AddRangeOverride(item.Claims.Select(c => new IdentityUserClaim<string>
                    {
                        ClaimType = c,
                        ClaimValue = c
                    }).ToList());

                    foreach (var role in item.Roles)
                    {
                        var roller = await _context.Roles
                            .SingleOrDefaultAsync(r => r.Name == role)
                            .ConfigureAwait(false);
                        user.UserRoles.Add(new IdentityUserRole<string>
                        {
                            UserId = user.Id,
                            RoleId = roller.Id
                        });
                    }
                    await _context.Users.AddAsync(user)
                        .ConfigureAwait(false);
                }
                await _context.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
        }
        public async Task<Task> AddBulkFiles()
        {
            var basePathInfo = Directory.GetParent(_environment.ContentRootPath).Parent;
            var pwd = Path.Combine(basePathInfo.FullName, "UNSYNC/bibliomit/DB");
            var filesPath = Path.Combine(pwd, "plankton");
            if (!Directory.Exists(filesPath))
                throw new DirectoryNotFoundException(_localizer[$"directory {filesPath} not found"]);
            var files = Directory.GetDirectories(filesPath).SelectMany(d => Directory.GetFiles(d));
            return await _import.AddRangeAsync(pwd, files).ConfigureAwait(false);
        }
    }
}
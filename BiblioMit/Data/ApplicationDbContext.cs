using Microsoft.EntityFrameworkCore;
using BiblioMit.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BiblioMit.Models.Entities.Centres;
using BiblioMit.Models.Entities.Digest;
using BiblioMit.Models.Entities.Environmental;
using System.ComponentModel;

namespace BiblioMit.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            modelBuilder?.Entity<ApplicationUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();
                b.HasMany(e => e.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
            });
            modelBuilder.Entity<ApplicationRole>()
                .HasMany(e => e.Users)
                .WithOne()
                .HasForeignKey(e => e.RoleId)
                .IsRequired();

            modelBuilder.Entity<PlantProduct>(a => {
                a.HasKey(p => new { p.PlantId, p.ProductId });

                a.HasOne(md => md.Plant)
                    .WithMany(d => d.Products)
                    .HasForeignKey(md => md.PlantId);

                a.HasOne(md => md.Product)
                    .WithMany(d => d.Plants)
                    .HasForeignKey(md => md.ProductId);
            });
            //builder.Entity<PlataformaUser>()
            //    .HasKey(p => new { p.AppUserId, p.PlataformId });
            modelBuilder.Entity<PlanktonAssayEmail>(a => {
                a.HasKey(p => new { p.PlanktonAssayId, p.EmailId });

                a.HasOne(md => md.PlanktonAssay)
                    .WithMany(d => d.Emails)
                    .HasForeignKey(md => md.PlanktonAssayId);

                a.HasOne(md => md.Email)
                    .WithMany(d => d.PlanktonAssayEmails)
                    .HasForeignKey(md => md.EmailId);
            });
            modelBuilder.Entity<Phytoplankton>(a => {
                a.HasIndex(p => new { p.PlanktonAssayId, p.SpeciesId }).IsUnique();
                
                a.HasOne(md => md.PlanktonAssay)
                    .WithMany(d => d.Phytoplanktons)
                    .HasForeignKey(md => md.PlanktonAssayId);

                a.HasOne(md => md.Species)
                    .WithMany(d => d.Phytoplanktons)
                    .HasForeignKey(md => md.SpeciesId);
            });
            modelBuilder.Entity<Analist>()
                .HasIndex(p => p.NormalizedName).IsUnique();
            modelBuilder.Entity<SamplingEntity>()
                .HasIndex(p => p.NormalizedName).IsUnique();
            modelBuilder.Entity<Email>()
                .HasIndex(p => p.Address).IsUnique();
            modelBuilder.Entity<Laboratory>()
                .HasIndex(p => p.NormalizedName).IsUnique();
            modelBuilder.Entity<Station>()
                .HasIndex(p => p.NormalizedName).IsUnique();
            modelBuilder.Entity<PhylogeneticGroup>()
                .HasIndex(p => p.NormalizedName).IsUnique();
            modelBuilder.Entity<GenusPhytoplankton>()
                .HasIndex(p => p.NormalizedName).IsUnique();
            modelBuilder.Entity<SpeciesPhytoplankton>()
                .HasIndex(p => new { p.GenusId, p.NormalizedName }).IsUnique();
            modelBuilder.Entity<Phone>()
                .HasIndex(p => p.Number).IsUnique();
            //builder.Entity<Centre>()
            //    .HasMany(c => c.Abastecimientos)
            //    .WithOne()
            //    .HasForeignKey(a => a.CentreId)
            //    .IsRequired()
            //    .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AreaCodeProvince>(a => {
                a.HasKey(p => new { p.AreaCodeId, p.ProvinceId });

                a.HasOne(md => md.AreaCode)
                    .WithMany(d => d.AreaCodeProvinces)
                    .HasForeignKey(md => md.AreaCodeId);

                a.HasOne(md => md.Province)
                    .WithMany(d => d.AreaCodeProvinces)
                    .HasForeignKey(md => md.ProvinceId);
            });
            modelBuilder.Entity<Commune>()
                .HasOne(p => p.Province)
                .WithMany(p => p.Communes)
                .HasForeignKey(i => i.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Province>(r =>
            {
                r.HasOne(p => p.Region)
                .WithMany(p => p.Provinces)
                .HasForeignKey(i => i.RegionId)
                .OnDelete(DeleteBehavior.Restrict);
                r.HasMany(p => p.Communes)
                .WithOne(c => c.Province)
                .HasForeignKey(c => c.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Region>()
                .HasMany(r => r.Provinces)
                .WithOne(p => p.Region)
                .HasForeignKey(r => r.RegionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Polygon>(p => {
                p.HasOne(p => p.Psmb)
                .WithOne(p => p.Polygon)
                .HasForeignKey<Psmb>(i => i.PolygonId)
                .OnDelete(DeleteBehavior.Restrict);

                p.HasOne(p => p.CatchmentArea)
                .WithOne(p => p.Polygon)
                .HasForeignKey<CatchmentArea>(i => i.PolygonId)
                .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Psmb>(a => {
                a.HasIndex(p => new { p.Code, p.Discriminator }).IsUnique();
                a.HasIndex(p => p.Acronym).IsUnique();
                a.HasDiscriminator<PsmbType>("Discriminator")
                .HasValue<Craft>(PsmbType.Craft)
                .HasValue<Farm>(PsmbType.Farm)
                .HasValue<NaturalBed>(PsmbType.NaturalBed)
                .HasValue<Plant>(PsmbType.Plant)
                .HasValue<PsmbArea>(PsmbType.PsmbArea)
                .HasValue<ResearchCentre>(PsmbType.ResearchCentre);
            });
            modelBuilder.Entity<Locality>()
                .HasDiscriminator<LocalityType>("Discriminator")
                .HasValue<Region>(LocalityType.Region)
                .HasValue<Province>(LocalityType.Province)
                .HasValue<Commune>(LocalityType.Commune);
        }
        public DbSet<SupplyDeclaration> SupplyDeclarations { get; set; }
        public DbSet<HarvestDeclaration> HarvestDeclarations { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<ProductionDeclaration> ProductionDeclarations { get; set; }
        public DbSet<SeedDeclaration> SeedDeclarations { get; set; }
        public DbSet<IdentityUserClaim<string>> IdentityUserClaims { get; set; }
        public DbSet<IdentityUserRole<string>> IdentityUserRoles { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }
        public DbSet<Analist> Analists { get; set; }
        public DbSet<Analysis> Analyses { get; set; }
        public DbSet<AreaCode> AreaCodes { get; set; }
        public DbSet<AreaCodeProvince> AreaCodeProvinces { get; set; }
        public DbSet<Psmb> Psmbs { get; set; }
        public DbSet<PsmbArea> PsmbAreas { get; set; }
        public DbSet<Farm> Farms { get; set; }
        public DbSet<ResearchCentre> ResearchCentres { get; set; }
        public DbSet<NaturalBed> NaturalBeds { get; set; }
        public DbSet<Craft> Crafts { get; set; }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<PlantProduct> PlantProducts { get; set; }
        public DbSet<Registry> Registries { get; set; }
        public DbSet<Header> Headers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Commune> Communes { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Coordinate> Coordinates { get; set; }
        public DbSet<Census> Census { get; set; }
        public DbSet<CatchmentArea> CatchmentAreas { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<PlanktonAssayEmail> PlanktonAssayEmails { get; set; }
        public DbSet<PlanktonAssay> PlanktonAssays { get; set; }
        public DbSet<SamplingEntity> SamplingEntities { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<InputFile> InputFiles { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<PhylogeneticGroup> PhylogeneticGroups { get; set; }
        public DbSet<GenusPhytoplankton> GenusPhytoplanktons { get; set; }
        public DbSet<IdentityUserClaim<string>> IdentityUserClaim { get; set; }
        public DbSet<IdentityUserRole<string>> IdentityUserRole { get; set; }
        public DbSet<Individual> Individuals { get; set; }
        public DbSet<Laboratory> Laboratories { get; set; }
        public DbSet<Larva> Larvas { get; set; }
        public DbSet<Larvae> Larvaes { get; set; }
        public DbSet<Locality> Localities { get; set; }
        public DbSet<Origin> Origins { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Phytoplankton> Phytoplanktons { get; set; }
        public DbSet<SernapescaEntry> SernapescaEntries { get; set; }
        public DbSet<SernapescaDeclaration> SernapescaDeclarations { get; set; }
        //public DbSet<PlataformaUser> PlataformaUser { get; set; }
        //public DbSet<Platform> Platform { get; set; }
        public DbSet<Polygon> Polygons { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostReply> PostReplies { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<ReproductiveStage> ReproductiveStages { get; set; }
        public DbSet<Sampling> Samplings { get; set; }
        public DbSet<Seed> Seeds { get; set; }
        public DbSet<Soft> Softs { get; set; }
        public DbSet<Spawning> Spawnings { get; set; }
        public DbSet<Specie> Species { get; set; }
        public DbSet<SpecieSeed> SpecieSeeds { get; set; }
        public DbSet<SpeciesPhytoplankton> SpeciesPhytoplanktons { get; set; }
        public DbSet<Talla> Tallas { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Valve> Valves { get; set; }
        public override int SaveChanges() => base.SaveChanges();
    }
}

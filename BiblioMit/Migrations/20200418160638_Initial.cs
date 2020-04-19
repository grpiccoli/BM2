using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BiblioMit.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder?.CreateTable(
                name: "Analists",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NormalizedName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AreaCodes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    IPAddress = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Last = table.Column<string>(nullable: true),
                    Rating = table.Column<int>(nullable: false),
                    ProfileImageUrl = table.Column<string>(nullable: true),
                    MemberSince = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    BusinessName = table.Column<string>(nullable: true),
                    TradeName = table.Column<string>(nullable: true),
                    Acronym = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    NormalizedBusinessName = table.Column<string>(nullable: true),
                    NormalizedTradeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Forums",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    ImageUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InputFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Laboratories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NormalizedName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Laboratories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Origins",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Origins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Phones",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhylogeneticGroups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    NormalizedName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhylogeneticGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    NormalizedName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SamplingEntities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NormalizedName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SamplingEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Species",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Sp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Species", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NormalizedName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    ApplicationRoleId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetRoles_ApplicationRoleId",
                        column: x => x.ApplicationRoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    RoleAssigner = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    OutPut = table.Column<string>(nullable: true),
                    IP = table.Column<string>(nullable: true),
                    Updated = table.Column<int>(nullable: false),
                    Added = table.Column<int>(nullable: false),
                    Observations = table.Column<int>(nullable: false),
                    Success = table.Column<bool>(nullable: false),
                    Min = table.Column<DateTime>(nullable: false),
                    Max = table.Column<DateTime>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    DeclarationType = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entries_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    ForumId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Forums_ForumId",
                        column: x => x.ForumId,
                        principalTable: "Forums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Posts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Registries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Attribute = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    InputFileId = table.Column<int>(nullable: false),
                    Operation = table.Column<string>(nullable: true),
                    DecimalPlaces = table.Column<int>(nullable: true),
                    DecimalSeparator = table.Column<string>(nullable: true),
                    DeleteAfter2ndNegative = table.Column<bool>(nullable: true),
                    NormalizedAttribute = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Registries_InputFiles_InputFileId",
                        column: x => x.InputFileId,
                        principalTable: "InputFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GenusPhytoplanktons",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    NormalizedName = table.Column<string>(nullable: false),
                    GroupId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenusPhytoplanktons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GenusPhytoplanktons_PhylogeneticGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "PhylogeneticGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostReplies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    PostId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostReplies_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostReplies_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Headers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistryId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    NormalizedName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Headers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Headers_Registries_RegistryId",
                        column: x => x.RegistryId,
                        principalTable: "Registries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesPhytoplanktons",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    NormalizedName = table.Column<string>(nullable: true),
                    GenusId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesPhytoplanktons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeciesPhytoplanktons_GenusPhytoplanktons_GenusId",
                        column: x => x.GenusId,
                        principalTable: "GenusPhytoplanktons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanktonAssays",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    SamplingDate = table.Column<DateTime>(nullable: false),
                    StationId = table.Column<int>(nullable: true),
                    SamplingEntityId = table.Column<int>(nullable: true),
                    AssayStart = table.Column<DateTime>(nullable: true),
                    ReceptionDate = table.Column<DateTime>(nullable: true),
                    AssayEnd = table.Column<DateTime>(nullable: true),
                    LaboratoryId = table.Column<int>(nullable: true),
                    PhoneId = table.Column<int>(nullable: true),
                    PsmbId = table.Column<int>(nullable: false),
                    NoSamples = table.Column<int>(nullable: false),
                    DepartureDate = table.Column<DateTime>(nullable: true),
                    AnalistId = table.Column<int>(nullable: true),
                    Temperature = table.Column<double>(nullable: true),
                    Oxigen = table.Column<double>(nullable: true),
                    Ph = table.Column<double>(nullable: true),
                    Salinity = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanktonAssays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanktonAssays_Analists_AnalistId",
                        column: x => x.AnalistId,
                        principalTable: "Analists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanktonAssays_Laboratories_LaboratoryId",
                        column: x => x.LaboratoryId,
                        principalTable: "Laboratories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanktonAssays_Phones_PhoneId",
                        column: x => x.PhoneId,
                        principalTable: "Phones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanktonAssays_SamplingEntities_SamplingEntityId",
                        column: x => x.SamplingEntityId,
                        principalTable: "SamplingEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanktonAssays_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Phytoplanktons",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanktonAssayId = table.Column<int>(nullable: false),
                    SpeciesId = table.Column<int>(nullable: false),
                    EAR = table.Column<int>(nullable: true),
                    C = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phytoplanktons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Phytoplanktons_PlanktonAssays_PlanktonAssayId",
                        column: x => x.PlanktonAssayId,
                        principalTable: "PlanktonAssays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Phytoplanktons_SpeciesPhytoplanktons_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "SpeciesPhytoplanktons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanktonAssayEmails",
                columns: table => new
                {
                    EmailId = table.Column<int>(nullable: false),
                    PlanktonAssayId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanktonAssayEmails", x => new { x.PlanktonAssayId, x.EmailId });
                    table.ForeignKey(
                        name: "FK_PlanktonAssayEmails_Emails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "Emails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanktonAssayEmails_PlanktonAssays_PlanktonAssayId",
                        column: x => x.PlanktonAssayId,
                        principalTable: "PlanktonAssays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AreaCodeProvinces",
                columns: table => new
                {
                    ProvinceId = table.Column<int>(nullable: false),
                    AreaCodeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaCodeProvinces", x => new { x.AreaCodeId, x.ProvinceId });
                    table.ForeignKey(
                        name: "FK_AreaCodeProvinces_AreaCodes_AreaCodeId",
                        column: x => x.AreaCodeId,
                        principalTable: "AreaCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(nullable: true),
                    ConsessionOrResearchId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Last = table.Column<string>(nullable: true),
                    Phone = table.Column<long>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Position = table.Column<int>(nullable: false),
                    OpenHr = table.Column<DateTime>(nullable: false),
                    CloseHr = table.Column<DateTime>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Localities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Discriminator = table.Column<int>(nullable: false),
                    NormalizedName = table.Column<string>(nullable: true),
                    ProvinceId = table.Column<int>(nullable: true),
                    CatchmentAreaId = table.Column<int>(nullable: true),
                    RegionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Localities_Localities_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Localities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Localities_Localities_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Localities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Census",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<DateTime>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    LocationId = table.Column<int>(nullable: false),
                    LocalityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Census", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Census_Localities_LocalityId",
                        column: x => x.LocalityId,
                        principalTable: "Localities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Polygons",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocalityId = table.Column<int>(nullable: true),
                    PsmbId = table.Column<int>(nullable: true),
                    CatchmentAreaId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Polygons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Polygons_Localities_LocalityId",
                        column: x => x.LocalityId,
                        principalTable: "Localities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatchmentAreas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    PolygonId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatchmentAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatchmentAreas_Polygons_PolygonId",
                        column: x => x.PolygonId,
                        principalTable: "Polygons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Coordinates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Latitude = table.Column<double>(nullable: false),
                    Longitude = table.Column<double>(nullable: false),
                    PolygonId = table.Column<int>(nullable: false),
                    Order = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coordinates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coordinates_Polygons_PolygonId",
                        column: x => x.PolygonId,
                        principalTable: "Polygons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Psmbs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<int>(nullable: false),
                    CommuneId = table.Column<int>(nullable: true),
                    Discriminator = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    NormalizedName = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    CompanyId = table.Column<int>(nullable: true),
                    Acronym = table.Column<string>(nullable: true),
                    WaterBody = table.Column<int>(nullable: true),
                    PolygonId = table.Column<int>(nullable: true),
                    PsmbAreaId = table.Column<int>(nullable: true),
                    RnaInvoice = table.Column<int>(nullable: true),
                    Certifiable = table.Column<bool>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Psmbs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Psmbs_Psmbs_PsmbAreaId",
                        column: x => x.PsmbAreaId,
                        principalTable: "Psmbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Psmbs_Localities_CommuneId",
                        column: x => x.CommuneId,
                        principalTable: "Localities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Psmbs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Psmbs_Polygons_PolygonId",
                        column: x => x.PolygonId,
                        principalTable: "Polygons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Analyses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Analyses_Psmbs_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Psmbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Larvaes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Larvaes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Larvaes_Psmbs_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Psmbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantProducts",
                columns: table => new
                {
                    PlantId = table.Column<int>(nullable: false),
                    ProductId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantProducts", x => new { x.PlantId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_PlantProducts_Psmbs_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Psmbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlantProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Samplings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    CentreId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Salinity = table.Column<int>(nullable: true),
                    Temp = table.Column<double>(nullable: true),
                    O2 = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Samplings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Samplings_Psmbs_CentreId",
                        column: x => x.CentreId,
                        principalTable: "Psmbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seeds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    DateCuelga = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seeds_Psmbs_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Psmbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SernapescaDeclarations",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    DeclarationNumber = table.Column<int>(nullable: false),
                    CentreId = table.Column<int>(nullable: false),
                    Dato = table.Column<int>(nullable: false),
                    OriginId = table.Column<int>(nullable: true),
                    ProductionType = table.Column<int>(nullable: true),
                    ItemType = table.Column<int>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    Weight = table.Column<double>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    PlantId = table.Column<int>(nullable: true),
                    CraftId = table.Column<int>(nullable: true),
                    SupplyDeclaration_PlantId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SernapescaDeclarations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SernapescaDeclarations_Origins_OriginId",
                        column: x => x.OriginId,
                        principalTable: "Origins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SernapescaDeclarations_Psmbs_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Psmbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SernapescaDeclarations_Origins_OriginId1",
                        column: x => x.OriginId,
                        principalTable: "Origins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SernapescaDeclarations_Psmbs_CentreId",
                        column: x => x.CentreId,
                        principalTable: "Psmbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SernapescaDeclarations_Psmbs_CraftId",
                        column: x => x.CraftId,
                        principalTable: "Psmbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SernapescaDeclarations_Psmbs_SupplyDeclaration_PlantId",
                        column: x => x.SupplyDeclaration_PlantId,
                        principalTable: "Psmbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Spawnings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    MaleProportion = table.Column<int>(nullable: false),
                    FemaleProportion = table.Column<int>(nullable: false),
                    MaleIG = table.Column<double>(nullable: false),
                    FemaleIG = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spawnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spawnings_Psmbs_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Psmbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Larvas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LarvaeId = table.Column<int>(nullable: false),
                    SpecieId = table.Column<int>(nullable: false),
                    LarvaType = table.Column<int>(nullable: false),
                    Count = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Larvas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Larvas_Larvaes_LarvaeId",
                        column: x => x.LarvaeId,
                        principalTable: "Larvaes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Larvas_Species_SpecieId",
                        column: x => x.SpecieId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Individuals",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    SamplingId = table.Column<int>(nullable: false),
                    Sex = table.Column<int>(nullable: false),
                    Maturity = table.Column<int>(nullable: false),
                    Length = table.Column<int>(nullable: false),
                    Comment = table.Column<string>(nullable: true),
                    Number = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(nullable: true),
                    Depth = table.Column<int>(nullable: true),
                    ADG = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Individuals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Individuals_Samplings_SamplingId",
                        column: x => x.SamplingId,
                        principalTable: "Samplings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecieSeeds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecieId = table.Column<int>(nullable: false),
                    SeedId = table.Column<int>(nullable: false),
                    Capture = table.Column<int>(nullable: false),
                    Proportion = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecieSeeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecieSeeds_Seeds_SeedId",
                        column: x => x.SeedId,
                        principalTable: "Seeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpecieSeeds_Species_SpecieId",
                        column: x => x.SpecieId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReproductiveStages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpawningId = table.Column<int>(nullable: false),
                    Stage = table.Column<int>(nullable: false),
                    Proportion = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReproductiveStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReproductiveStages_Spawnings_SpawningId",
                        column: x => x.SpawningId,
                        principalTable: "Spawnings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Softs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndividualId = table.Column<int>(nullable: false),
                    SoftType = table.Column<int>(nullable: false),
                    Tissue = table.Column<int>(nullable: false),
                    Count = table.Column<int>(nullable: true),
                    Degree = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Softs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Softs_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Valves",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    IndividualId = table.Column<int>(nullable: false),
                    ValveType = table.Column<int>(nullable: false),
                    Species = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Valves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Valves_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tallas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecieSeedId = table.Column<int>(nullable: false),
                    Range = table.Column<int>(nullable: false),
                    Proportion = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tallas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tallas_SpecieSeeds_SpecieSeedId",
                        column: x => x.SpecieSeedId,
                        principalTable: "SpecieSeeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndividualId = table.Column<int>(nullable: false),
                    Key = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true),
                    Magnification = table.Column<int>(nullable: false),
                    SoftId = table.Column<int>(nullable: true),
                    ValveId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Photos_Softs_SoftId",
                        column: x => x.SoftId,
                        principalTable: "Softs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Photos_Valves_ValveId",
                        column: x => x.ValveId,
                        principalTable: "Valves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Analists_NormalizedName",
                table: "Analists",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Analyses_FarmId",
                table: "Analyses",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_AreaCodeProvinces_ProvinceId",
                table: "AreaCodeProvinces",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_ApplicationRoleId",
                table: "AspNetUserClaims",
                column: "ApplicationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CatchmentAreas_PolygonId",
                table: "CatchmentAreas",
                column: "PolygonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Census_LocalityId",
                table: "Census",
                column: "LocalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_ConsessionOrResearchId",
                table: "Contacts",
                column: "ConsessionOrResearchId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_OwnerId",
                table: "Contacts",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Coordinates_PolygonId",
                table: "Coordinates",
                column: "PolygonId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_Address",
                table: "Emails",
                column: "Address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entries_ApplicationUserId",
                table: "Entries",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GenusPhytoplanktons_GroupId",
                table: "GenusPhytoplanktons",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GenusPhytoplanktons_NormalizedName",
                table: "GenusPhytoplanktons",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Headers_RegistryId",
                table: "Headers",
                column: "RegistryId");

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_SamplingId",
                table: "Individuals",
                column: "SamplingId");

            migrationBuilder.CreateIndex(
                name: "IX_Laboratories_NormalizedName",
                table: "Laboratories",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Larvaes_FarmId",
                table: "Larvaes",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_Larvas_LarvaeId",
                table: "Larvas",
                column: "LarvaeId");

            migrationBuilder.CreateIndex(
                name: "IX_Larvas_SpecieId",
                table: "Larvas",
                column: "SpecieId");

            migrationBuilder.CreateIndex(
                name: "IX_Localities_CatchmentAreaId",
                table: "Localities",
                column: "CatchmentAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Localities_ProvinceId",
                table: "Localities",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_Localities_RegionId",
                table: "Localities",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Phones_Number",
                table: "Phones",
                column: "Number",
                unique: true,
                filter: "[Number] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_IndividualId",
                table: "Photos",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_SoftId",
                table: "Photos",
                column: "SoftId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_ValveId",
                table: "Photos",
                column: "ValveId");

            migrationBuilder.CreateIndex(
                name: "IX_PhylogeneticGroups_NormalizedName",
                table: "PhylogeneticGroups",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Phytoplanktons_SpeciesId",
                table: "Phytoplanktons",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_Phytoplanktons_PlanktonAssayId_SpeciesId",
                table: "Phytoplanktons",
                columns: new[] { "PlanktonAssayId", "SpeciesId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanktonAssayEmails_EmailId",
                table: "PlanktonAssayEmails",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanktonAssays_AnalistId",
                table: "PlanktonAssays",
                column: "AnalistId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanktonAssays_LaboratoryId",
                table: "PlanktonAssays",
                column: "LaboratoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanktonAssays_PhoneId",
                table: "PlanktonAssays",
                column: "PhoneId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanktonAssays_PsmbId",
                table: "PlanktonAssays",
                column: "PsmbId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanktonAssays_SamplingEntityId",
                table: "PlanktonAssays",
                column: "SamplingEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanktonAssays_StationId",
                table: "PlanktonAssays",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantProducts_ProductId",
                table: "PlantProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Polygons_LocalityId",
                table: "Polygons",
                column: "LocalityId");

            migrationBuilder.CreateIndex(
                name: "IX_PostReplies_PostId",
                table: "PostReplies",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostReplies_UserId",
                table: "PostReplies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_ForumId",
                table: "Posts",
                column: "ForumId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId",
                table: "Posts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Psmbs_PsmbAreaId",
                table: "Psmbs",
                column: "PsmbAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Psmbs_Acronym",
                table: "Psmbs",
                column: "Acronym",
                unique: true,
                filter: "[Acronym] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Psmbs_CommuneId",
                table: "Psmbs",
                column: "CommuneId");

            migrationBuilder.CreateIndex(
                name: "IX_Psmbs_CompanyId",
                table: "Psmbs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Psmbs_PolygonId",
                table: "Psmbs",
                column: "PolygonId",
                unique: true,
                filter: "[PolygonId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Psmbs_Code_Discriminator",
                table: "Psmbs",
                columns: new[] { "Code", "Discriminator" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registries_InputFileId",
                table: "Registries",
                column: "InputFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ReproductiveStages_SpawningId",
                table: "ReproductiveStages",
                column: "SpawningId");

            migrationBuilder.CreateIndex(
                name: "IX_SamplingEntities_NormalizedName",
                table: "SamplingEntities",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Samplings_CentreId",
                table: "Samplings",
                column: "CentreId");

            migrationBuilder.CreateIndex(
                name: "IX_Seeds_FarmId",
                table: "Seeds",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_SernapescaDeclarations_OriginId",
                table: "SernapescaDeclarations",
                column: "OriginId");

            migrationBuilder.CreateIndex(
                name: "IX_SernapescaDeclarations_PlantId",
                table: "SernapescaDeclarations",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_SernapescaDeclarations_OriginId1",
                table: "SernapescaDeclarations",
                column: "OriginId");

            migrationBuilder.CreateIndex(
                name: "IX_SernapescaDeclarations_CentreId",
                table: "SernapescaDeclarations",
                column: "CentreId");

            migrationBuilder.CreateIndex(
                name: "IX_SernapescaDeclarations_CraftId",
                table: "SernapescaDeclarations",
                column: "CraftId");

            migrationBuilder.CreateIndex(
                name: "IX_SernapescaDeclarations_SupplyDeclaration_PlantId",
                table: "SernapescaDeclarations",
                column: "SupplyDeclaration_PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_Softs_IndividualId",
                table: "Softs",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_Spawnings_FarmId",
                table: "Spawnings",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecieSeeds_SeedId",
                table: "SpecieSeeds",
                column: "SeedId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecieSeeds_SpecieId",
                table: "SpecieSeeds",
                column: "SpecieId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesPhytoplanktons_GenusId_NormalizedName",
                table: "SpeciesPhytoplanktons",
                columns: new[] { "GenusId", "NormalizedName" },
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_NormalizedName",
                table: "Stations",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tallas_SpecieSeedId",
                table: "Tallas",
                column: "SpecieSeedId");

            migrationBuilder.CreateIndex(
                name: "IX_Valves_IndividualId",
                table: "Valves",
                column: "IndividualId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanktonAssays_Psmbs_PsmbId",
                table: "PlanktonAssays",
                column: "PsmbId",
                principalTable: "Psmbs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AreaCodeProvinces_Localities_ProvinceId",
                table: "AreaCodeProvinces",
                column: "ProvinceId",
                principalTable: "Localities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Psmbs_ConsessionOrResearchId",
                table: "Contacts",
                column: "ConsessionOrResearchId",
                principalTable: "Psmbs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Localities_CatchmentAreas_CatchmentAreaId",
                table: "Localities",
                column: "CatchmentAreaId",
                principalTable: "CatchmentAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder?.DropForeignKey(
                name: "FK_Polygons_Localities_LocalityId",
                table: "Polygons");

            migrationBuilder.DropTable(
                name: "Analyses");

            migrationBuilder.DropTable(
                name: "AreaCodeProvinces");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Census");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "Coordinates");

            migrationBuilder.DropTable(
                name: "Entries");

            migrationBuilder.DropTable(
                name: "Headers");

            migrationBuilder.DropTable(
                name: "Larvas");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "Phytoplanktons");

            migrationBuilder.DropTable(
                name: "PlanktonAssayEmails");

            migrationBuilder.DropTable(
                name: "PlantProducts");

            migrationBuilder.DropTable(
                name: "PostReplies");

            migrationBuilder.DropTable(
                name: "ReproductiveStages");

            migrationBuilder.DropTable(
                name: "SernapescaDeclarations");

            migrationBuilder.DropTable(
                name: "Tallas");

            migrationBuilder.DropTable(
                name: "AreaCodes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Registries");

            migrationBuilder.DropTable(
                name: "Larvaes");

            migrationBuilder.DropTable(
                name: "Softs");

            migrationBuilder.DropTable(
                name: "Valves");

            migrationBuilder.DropTable(
                name: "SpeciesPhytoplanktons");

            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropTable(
                name: "PlanktonAssays");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Spawnings");

            migrationBuilder.DropTable(
                name: "Origins");

            migrationBuilder.DropTable(
                name: "SpecieSeeds");

            migrationBuilder.DropTable(
                name: "InputFiles");

            migrationBuilder.DropTable(
                name: "Individuals");

            migrationBuilder.DropTable(
                name: "GenusPhytoplanktons");

            migrationBuilder.DropTable(
                name: "Analists");

            migrationBuilder.DropTable(
                name: "Laboratories");

            migrationBuilder.DropTable(
                name: "Phones");

            migrationBuilder.DropTable(
                name: "SamplingEntities");

            migrationBuilder.DropTable(
                name: "Stations");

            migrationBuilder.DropTable(
                name: "Forums");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Seeds");

            migrationBuilder.DropTable(
                name: "Species");

            migrationBuilder.DropTable(
                name: "Samplings");

            migrationBuilder.DropTable(
                name: "PhylogeneticGroups");

            migrationBuilder.DropTable(
                name: "Psmbs");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Localities");

            migrationBuilder.DropTable(
                name: "CatchmentAreas");

            migrationBuilder.DropTable(
                name: "Polygons");
        }
    }
}

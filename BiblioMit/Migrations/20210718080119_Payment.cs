using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BiblioMit.Migrations
{
    public partial class Payment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Banners",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<int>(type: "int", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BannerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Banners_BannerId",
                        column: x => x.BannerId,
                        principalTable: "Banners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Banners_ApplicationUserId",
                table: "Banners",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BannerId",
                table: "Payments",
                column: "BannerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Banners_AspNetUsers_ApplicationUserId",
                table: "Banners",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Banners_AspNetUsers_ApplicationUserId",
                table: "Banners");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Banners_ApplicationUserId",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Banners");
        }
    }
}

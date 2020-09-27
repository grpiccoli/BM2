using Microsoft.EntityFrameworkCore.Migrations;

namespace BiblioMit.Migrations
{
    public partial class PlanktonUsers2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanktonAssays_PlanktonUser_PlanktonUserId",
                table: "PlanktonAssays");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanktonUser",
                table: "PlanktonUser");

            migrationBuilder.RenameTable(
                name: "PlanktonUser",
                newName: "PlanktonUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanktonUsers",
                table: "PlanktonUsers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanktonAssays_PlanktonUsers_PlanktonUserId",
                table: "PlanktonAssays",
                column: "PlanktonUserId",
                principalTable: "PlanktonUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanktonAssays_PlanktonUsers_PlanktonUserId",
                table: "PlanktonAssays");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanktonUsers",
                table: "PlanktonUsers");

            migrationBuilder.RenameTable(
                name: "PlanktonUsers",
                newName: "PlanktonUser");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanktonUser",
                table: "PlanktonUser",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanktonAssays_PlanktonUser_PlanktonUserId",
                table: "PlanktonAssays",
                column: "PlanktonUserId",
                principalTable: "PlanktonUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

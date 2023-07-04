using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apifirstproject1.Migrations
{
    public partial class v5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                table: "DataFiles");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Chat",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Chat");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "DataFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace TravelService.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersistedLocations",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Attributes = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    Term = table.Column<string>(nullable: true),
                    Lat = table.Column<double>(nullable: false),
                    Lng = table.Column<double>(nullable: false),
                    Address = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersistedLocations", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersistedLocations");
        }
    }
}

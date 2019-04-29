using Microsoft.EntityFrameworkCore.Migrations;

namespace TravelService.Migrations
{
    public partial class ResolvedLocationCoordinateRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Lng",
                table: "PersistedLocations",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Lat",
                table: "PersistedLocations",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Lng",
                table: "PersistedLocations",
                nullable: true,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<double>(
                name: "Lat",
                table: "PersistedLocations",
                nullable: true,
                oldClrType: typeof(double));
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MantenimientoIndustrial.Migrations
{
    /// <inheritdoc />
    public partial class AddFotoRutaToEquipo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoRuta",
                table: "Equipos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoRuta",
                table: "Equipos");
        }
    }
}

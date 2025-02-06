using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MantenimientoIndustrial.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEquiposFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "Equipos",
                newName: "Ubicacion");

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Equipos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Equipos");

            migrationBuilder.RenameColumn(
                name: "Ubicacion",
                table: "Equipos",
                newName: "Tipo");
        }
    }
}

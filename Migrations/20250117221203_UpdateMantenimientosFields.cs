using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MantenimientoIndustrial.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMantenimientosFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicial",
                table: "Mantenimientos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Frecuencia",
                table: "Mantenimientos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaInicial",
                table: "Mantenimientos");

            migrationBuilder.DropColumn(
                name: "Frecuencia",
                table: "Mantenimientos");
        }
    }
}

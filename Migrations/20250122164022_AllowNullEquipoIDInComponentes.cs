using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MantenimientoIndustrial.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullEquipoIDInComponentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Componentes_Equipos_EquipoID",
                table: "Componentes");

            migrationBuilder.AlterColumn<int>(
                name: "EquipoID",
                table: "Componentes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Componentes_Equipos_EquipoID",
                table: "Componentes",
                column: "EquipoID",
                principalTable: "Equipos",
                principalColumn: "EquipoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Componentes_Equipos_EquipoID",
                table: "Componentes");

            migrationBuilder.AlterColumn<int>(
                name: "EquipoID",
                table: "Componentes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Componentes_Equipos_EquipoID",
                table: "Componentes",
                column: "EquipoID",
                principalTable: "Equipos",
                principalColumn: "EquipoID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

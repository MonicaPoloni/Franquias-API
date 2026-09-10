using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Franquias.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAtivoUnidadeFranqueada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "UnidadesFranqueadas",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "UnidadesFranqueadas");
        }
    }
}

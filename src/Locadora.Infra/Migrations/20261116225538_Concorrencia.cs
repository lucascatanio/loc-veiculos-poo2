using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Locadora.Infra.Migrations
{
    /// <inheritdoc />
    public partial class Concorrencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Veiculos",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Locacoes",
                type: "rowversion",
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Locacoes");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace src.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClienteNomeForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing database data may contain values that don't match Clientes.Nome.
            // Since the FK columns are nullable, we null them out so the constraint can be added.
            migrationBuilder.Sql(@"
UPDATE m
SET Cliente = NULL
FROM dbo.Cliente_Movimentos m
LEFT JOIN dbo.Clientes c ON c.Nome = m.Cliente
WHERE m.Cliente IS NOT NULL AND c.Nome IS NULL;
");

            migrationBuilder.Sql(@"
UPDATE t
SET Unidade = NULL
FROM dbo.Cliente_Tags t
LEFT JOIN dbo.Clientes c ON c.Nome = t.Unidade
WHERE t.Unidade IS NOT NULL AND c.Nome IS NULL;
");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Clientes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Unidade",
                table: "Cliente_Tags",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cliente",
                table: "Cliente_Movimentos",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Clientes_Nome",
                table: "Clientes",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Tags_Unidade",
                table: "Cliente_Tags",
                column: "Unidade");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Movimentos_Cliente",
                table: "Cliente_Movimentos",
                column: "Cliente");

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_Movimentos_Clientes_Cliente",
                table: "Cliente_Movimentos",
                column: "Cliente",
                principalTable: "Clientes",
                principalColumn: "Nome",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_Tags_Clientes_Unidade",
                table: "Cliente_Tags",
                column: "Unidade",
                principalTable: "Clientes",
                principalColumn: "Nome",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_Movimentos_Clientes_Cliente",
                table: "Cliente_Movimentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_Tags_Clientes_Unidade",
                table: "Cliente_Tags");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Clientes_Nome",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_Tags_Unidade",
                table: "Cliente_Tags");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_Movimentos_Cliente",
                table: "Cliente_Movimentos");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Unidade",
                table: "Cliente_Tags",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cliente",
                table: "Cliente_Movimentos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}

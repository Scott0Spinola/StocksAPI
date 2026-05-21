using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace src.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClienteDocsNumeroKeysAndFks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NumeroGuia",
                table: "Cliente_Guias",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroDoc",
                table: "Cliente_Faturas",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroGuia",
                table: "Cliente_DetalheGuias",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroDoc",
                table: "Cliente_DetalheFaturas",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Cliente_Guias_NumeroGuia",
                table: "Cliente_Guias",
                column: "NumeroGuia");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Cliente_Faturas_NumeroDoc",
                table: "Cliente_Faturas",
                column: "NumeroDoc");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_DetalheGuias_NumeroGuia",
                table: "Cliente_DetalheGuias",
                column: "NumeroGuia");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_DetalheFaturas_NumeroDoc",
                table: "Cliente_DetalheFaturas",
                column: "NumeroDoc");

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_DetalheFaturas_Cliente_Faturas_NumeroDoc",
                table: "Cliente_DetalheFaturas",
                column: "NumeroDoc",
                principalTable: "Cliente_Faturas",
                principalColumn: "NumeroDoc",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_DetalheGuias_Cliente_Guias_NumeroGuia",
                table: "Cliente_DetalheGuias",
                column: "NumeroGuia",
                principalTable: "Cliente_Guias",
                principalColumn: "NumeroGuia",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_DetalheFaturas_Cliente_Faturas_NumeroDoc",
                table: "Cliente_DetalheFaturas");

            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_DetalheGuias_Cliente_Guias_NumeroGuia",
                table: "Cliente_DetalheGuias");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Cliente_Guias_NumeroGuia",
                table: "Cliente_Guias");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Cliente_Faturas_NumeroDoc",
                table: "Cliente_Faturas");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_DetalheGuias_NumeroGuia",
                table: "Cliente_DetalheGuias");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_DetalheFaturas_NumeroDoc",
                table: "Cliente_DetalheFaturas");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroGuia",
                table: "Cliente_Guias",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroDoc",
                table: "Cliente_Faturas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroGuia",
                table: "Cliente_DetalheGuias",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroDoc",
                table: "Cliente_DetalheFaturas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }
    }
}

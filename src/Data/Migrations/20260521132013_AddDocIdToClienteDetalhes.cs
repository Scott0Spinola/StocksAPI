using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace src.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDocIdToClienteDetalhes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DocId",
                table: "Cliente_DetalheGuias",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DocId",
                table: "Cliente_DetalheFaturas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE dg
SET dg.[DocId] = g.[Id]
FROM [Cliente_DetalheGuias] dg
INNER JOIN [Cliente_Guias] g ON g.[NumeroGuia] = dg.[NumeroGuia]
WHERE dg.[DocId] IS NULL;
");

            migrationBuilder.Sql(@"
UPDATE df
SET df.[DocId] = f.[Id]
FROM [Cliente_DetalheFaturas] df
INNER JOIN [Cliente_Faturas] f ON f.[NumeroDoc] = df.[NumeroDoc]
WHERE df.[DocId] IS NULL;
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM [Cliente_DetalheGuias] WHERE [DocId] IS NULL)
    THROW 50000, 'Cannot backfill Cliente_DetalheGuias.DocId because some rows have no matching Cliente_Guias by NumeroGuia.', 1;
IF EXISTS (SELECT 1 FROM [Cliente_DetalheFaturas] WHERE [DocId] IS NULL)
    THROW 50000, 'Cannot backfill Cliente_DetalheFaturas.DocId because some rows have no matching Cliente_Faturas by NumeroDoc.', 1;
");

            migrationBuilder.AlterColumn<Guid>(
                name: "DocId",
                table: "Cliente_DetalheGuias",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DocId",
                table: "Cliente_DetalheFaturas",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_DetalheGuias_DocId",
                table: "Cliente_DetalheGuias",
                column: "DocId");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_DetalheFaturas_DocId",
                table: "Cliente_DetalheFaturas",
                column: "DocId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_DetalheFaturas_Cliente_Faturas_DocId",
                table: "Cliente_DetalheFaturas",
                column: "DocId",
                principalTable: "Cliente_Faturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_DetalheGuias_Cliente_Guias_DocId",
                table: "Cliente_DetalheGuias",
                column: "DocId",
                principalTable: "Cliente_Guias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_DetalheFaturas_Cliente_Faturas_DocId",
                table: "Cliente_DetalheFaturas");

            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_DetalheGuias_Cliente_Guias_DocId",
                table: "Cliente_DetalheGuias");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_DetalheGuias_DocId",
                table: "Cliente_DetalheGuias");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_DetalheFaturas_DocId",
                table: "Cliente_DetalheFaturas");

            migrationBuilder.DropColumn(
                name: "DocId",
                table: "Cliente_DetalheGuias");

            migrationBuilder.DropColumn(
                name: "DocId",
                table: "Cliente_DetalheFaturas");
        }
    }
}

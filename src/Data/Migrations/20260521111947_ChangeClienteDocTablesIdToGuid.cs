using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace src.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeClienteDocTablesIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ConvertIntIdentityPkToGuidPk(migrationBuilder, "Cliente_Guias", "PK_Cliente_Guias");
            ConvertIntIdentityPkToGuidPk(migrationBuilder, "Cliente_Faturas", "PK_Cliente_Faturas");
            ConvertIntIdentityPkToGuidPk(migrationBuilder, "Cliente_DetalheGuias", "PK_Cliente_DetalheGuias");
            ConvertIntIdentityPkToGuidPk(migrationBuilder, "Cliente_DetalheFaturas", "PK_Cliente_DetalheFaturas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ConvertGuidPkToIntIdentityPk(migrationBuilder, "Cliente_Guias", "PK_Cliente_Guias");
            ConvertGuidPkToIntIdentityPk(migrationBuilder, "Cliente_Faturas", "PK_Cliente_Faturas");
            ConvertGuidPkToIntIdentityPk(migrationBuilder, "Cliente_DetalheGuias", "PK_Cliente_DetalheGuias");
            ConvertGuidPkToIntIdentityPk(migrationBuilder, "Cliente_DetalheFaturas", "PK_Cliente_DetalheFaturas");
        }

        private static void ConvertIntIdentityPkToGuidPk(MigrationBuilder migrationBuilder, string table, string pkName)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id_Temp",
                table: table,
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql($"UPDATE [{table}] SET [Id_Temp] = NEWID() WHERE [Id_Temp] IS NULL");

            migrationBuilder.DropPrimaryKey(
                name: pkName,
                table: table);

            migrationBuilder.DropColumn(
                name: "Id",
                table: table);

            migrationBuilder.RenameColumn(
                name: "Id_Temp",
                table: table,
                newName: "Id");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: table,
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.Sql($"ALTER TABLE [{table}] ADD CONSTRAINT [DF_{table}_Id] DEFAULT (NEWSEQUENTIALID()) FOR [Id]");

            migrationBuilder.AddPrimaryKey(
                name: pkName,
                table: table,
                column: "Id");
        }

        private static void ConvertGuidPkToIntIdentityPk(MigrationBuilder migrationBuilder, string table, string pkName)
        {
            migrationBuilder.Sql($"ALTER TABLE [{table}] ADD [Id_Temp] int IDENTITY(1,1) NOT NULL");

            migrationBuilder.DropPrimaryKey(
                name: pkName,
                table: table);

            migrationBuilder.DropColumn(
                name: "Id",
                table: table);

            migrationBuilder.RenameColumn(
                name: "Id_Temp",
                table: table,
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: pkName,
                table: table,
                column: "Id");
        }
    }
}

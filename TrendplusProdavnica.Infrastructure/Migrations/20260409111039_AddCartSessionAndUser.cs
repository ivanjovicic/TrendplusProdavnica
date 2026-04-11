using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrendplusProdavnica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCartSessionAndUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_product_variants_ProductId",
                schema: "catalog",
                table: "product_variants",
                newName: "ix_product_variant_product_id");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                schema: "sales",
                table: "carts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "sales",
                table: "carts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_carts_session_id",
                schema: "sales",
                table: "carts",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "ix_carts_user_id",
                schema: "sales",
                table: "carts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_carts_session_id",
                schema: "sales",
                table: "carts");

            migrationBuilder.DropIndex(
                name: "ix_carts_user_id",
                schema: "sales",
                table: "carts");

            migrationBuilder.DropColumn(
                name: "SessionId",
                schema: "sales",
                table: "carts");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "sales",
                table: "carts");

            migrationBuilder.RenameIndex(
                name: "ix_product_variant_product_id",
                schema: "catalog",
                table: "product_variants",
                newName: "IX_product_variants_ProductId");
        }
    }
}

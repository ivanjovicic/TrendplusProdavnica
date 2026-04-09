using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrendplusProdavnica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlpReadIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_products_BrandId",
                schema: "catalog",
                table: "products",
                newName: "ix_products_brand_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_primary_category_id",
                schema: "catalog",
                table: "products",
                column: "PrimaryCategoryId");

            migrationBuilder.CreateIndex(
                name: "ix_product_variants_price",
                schema: "catalog",
                table: "product_variants",
                column: "Price");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_primary_category_id",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_product_variants_price",
                schema: "catalog",
                table: "product_variants");

            migrationBuilder.RenameIndex(
                name: "ix_products_brand_id",
                schema: "catalog",
                table: "products",
                newName: "IX_products_BrandId");
        }
    }
}

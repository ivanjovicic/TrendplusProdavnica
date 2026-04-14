using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrendplusProdavnica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemCategorySnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CategoryIdSnapshot",
                schema: "sales",
                table: "order_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryNameSnapshot",
                schema: "sales",
                table: "order_items",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryIdSnapshot",
                schema: "sales",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "CategoryNameSnapshot",
                schema: "sales",
                table: "order_items");
        }
    }
}

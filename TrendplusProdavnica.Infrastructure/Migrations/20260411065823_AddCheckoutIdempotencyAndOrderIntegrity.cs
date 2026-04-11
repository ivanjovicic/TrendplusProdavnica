using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrendplusProdavnica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutIdempotencyAndOrderIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutIdempotencyKey",
                schema: "sales",
                table: "orders",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_orders_cart_id",
                schema: "sales",
                table: "orders",
                column: "CartId",
                unique: true,
                filter: "\"cart_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_orders_checkout_idempotency_key",
                schema: "sales",
                table: "orders",
                column: "CheckoutIdempotencyKey",
                unique: true,
                filter: "\"checkout_idempotency_key\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_orders_cart_id",
                schema: "sales",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ux_orders_checkout_idempotency_key",
                schema: "sales",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "CheckoutIdempotencyKey",
                schema: "sales",
                table: "orders");
        }
    }
}

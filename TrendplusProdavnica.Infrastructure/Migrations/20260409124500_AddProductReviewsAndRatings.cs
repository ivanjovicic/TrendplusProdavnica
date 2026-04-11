using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrendplusProdavnica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductReviewsAndRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_ratings",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false, defaultValue: 0m),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RatingCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OneStarCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TwoStarCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ThreeStarCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FourStarCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FiveStarCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastReviewAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_ratings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_ratings_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_reviews",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ExternalKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    AuthorName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    ReviewBody = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RatingValue = table.Column<decimal>(type: "numeric(2,1)", precision: 2, scale: 1, nullable: false),
                    IsVerifiedPurchase = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_reviews_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_reviews_product_status_published_at",
                schema: "catalog",
                table: "product_reviews",
                columns: new[] { "ProductId", "Status", "PublishedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "ux_product_ratings_product_id",
                schema: "catalog",
                table: "product_ratings",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_product_reviews_external_key",
                schema: "catalog",
                table: "product_reviews",
                column: "ExternalKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_ratings",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_reviews",
                schema: "catalog");
        }
    }
}

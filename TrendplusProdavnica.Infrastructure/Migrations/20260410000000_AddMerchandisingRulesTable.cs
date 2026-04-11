using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrendplusProdavnica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchandisingRulesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "merchandising_rules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    RuleType = table.Column<short>(type: "smallint", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    BrandId = table.Column<long>(type: "bigint", nullable: true),
                    ProductId = table.Column<long>(type: "bigint", nullable: true),
                    BoostScore = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    StartDateUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDateUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchandising_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_merchandising_rules_brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_merchandising_rules_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_merchandising_rules_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_merchandising_rules_active",
                table: "merchandising_rules",
                columns: new[] { "IsActive", "StartDateUtc", "EndDateUtc" });

            migrationBuilder.CreateIndex(
                name: "ix_merchandising_rules_brand",
                table: "merchandising_rules",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "ix_merchandising_rules_category",
                table: "merchandising_rules",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "ix_merchandising_rules_product",
                table: "merchandising_rules",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "ix_merchandising_rules_priority",
                table: "merchandising_rules",
                columns: new[] { "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "ix_merchandising_rules_rule_type",
                table: "merchandising_rules",
                column: "RuleType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "merchandising_rules");
        }
    }
}

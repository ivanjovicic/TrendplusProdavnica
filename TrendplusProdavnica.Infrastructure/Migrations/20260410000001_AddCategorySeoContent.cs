using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrendplusProdavnica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategorySeoContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category_seo_content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    MetaTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IntroTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IntroText = table.Column<string>(type: "text", nullable: true),
                    MainContent = table.Column<string>(type: "text", nullable: true),
                    Faq = table.Column<string>(type: "jsonb", nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_seo_content", x => x.Id);
                    table.ForeignKey(
                        name: "FK_category_seo_content_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_seo_content_CategoryId",
                table: "category_seo_content",
                column: "CategoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_category_seo_content_IsPublished_PublishedAtUtc",
                table: "category_seo_content",
                columns: new[] { "IsPublished", "PublishedAtUtc" },
                descending: new bool[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_seo_content");
        }
    }
}

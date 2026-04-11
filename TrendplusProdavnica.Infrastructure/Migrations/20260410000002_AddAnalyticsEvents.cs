using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrendplusProdavnica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.CreateTable(
                name: "analytics_events",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventType = table.Column<short>(type: "smallint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    SessionId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EventTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ReferrerUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    EventData = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytics_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_analytics_events_timestamp",
                schema: "analytics",
                table: "analytics_events",
                column: "EventTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_analytics_events_eventtype",
                schema: "analytics",
                table: "analytics_events",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_analytics_events_productid",
                schema: "analytics",
                table: "analytics_events",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_analytics_events_userid",
                schema: "analytics",
                table: "analytics_events",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_analytics_events_sessionid",
                schema: "analytics",
                table: "analytics_events",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_analytics_events_type_timestamp",
                schema: "analytics",
                table: "analytics_events",
                columns: new[] { "EventType", "EventTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_analytics_events_product_eventtype",
                schema: "analytics",
                table: "analytics_events",
                columns: new[] { "ProductId", "EventType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analytics_events",
                schema: "analytics");
        }
    }
}

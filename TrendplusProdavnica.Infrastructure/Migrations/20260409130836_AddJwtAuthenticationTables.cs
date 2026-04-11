using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrendplusProdavnica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJwtAuthenticationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "search_index_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastRetryAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDeadLettered = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeadLetteredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeadLetterReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_index_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_search_index_events_dlq",
                table: "search_index_events",
                columns: new[] { "IsDeadLettered", "DeadLetteredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_search_index_events_EventId",
                table: "search_index_events",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_search_index_events_pending",
                table: "search_index_events",
                columns: new[] { "IsProcessed", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "ix_search_index_events_product_id",
                table: "search_index_events",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "search_index_events");
        }
    }
}

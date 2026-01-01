using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DhSport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "site_log",
                columns: table => new
                {
                    log_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    prev_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    prev_url_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    curr_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    curr_url_ud = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    curr_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_site_log", x => x.log_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_site_log_curr_timestamp",
                table: "site_log",
                column: "curr_timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_site_log_session_id",
                table: "site_log",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_site_log_user_id",
                table: "site_log",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "site_log");
        }
    }
}

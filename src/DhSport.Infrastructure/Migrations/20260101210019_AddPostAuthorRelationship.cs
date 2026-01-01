using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DhSport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPostAuthorRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_post_author_id",
                table: "post",
                column: "author_id");

            migrationBuilder.AddForeignKey(
                name: "fk_post_users_author_id",
                table: "post",
                column: "author_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_post_users_author_id",
                table: "post");

            migrationBuilder.DropIndex(
                name: "ix_post_author_id",
                table: "post");
        }
    }
}

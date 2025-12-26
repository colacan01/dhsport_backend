using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DhSport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePostBoardCommentEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "post_user_id",
                table: "post",
                newName: "author_id");

            migrationBuilder.RenameColumn(
                name: "comment_user_id",
                table: "comment",
                newName: "author_id");

            migrationBuilder.AddColumn<int>(
                name: "display_order",
                table: "post_file",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<object>(
                name: "post_content",
                table: "post",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AddColumn<int>(
                name: "comment_cnt",
                table: "post",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_secret",
                table: "post",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "like_cnt",
                table: "post",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "post",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "view_cnt",
                table: "post",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "like_cnt",
                table: "comment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "display_order",
                table: "board",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "display_order",
                table: "post_file");

            migrationBuilder.DropColumn(
                name: "comment_cnt",
                table: "post");

            migrationBuilder.DropColumn(
                name: "is_secret",
                table: "post");

            migrationBuilder.DropColumn(
                name: "like_cnt",
                table: "post");

            migrationBuilder.DropColumn(
                name: "title",
                table: "post");

            migrationBuilder.DropColumn(
                name: "view_cnt",
                table: "post");

            migrationBuilder.DropColumn(
                name: "like_cnt",
                table: "comment");

            migrationBuilder.DropColumn(
                name: "display_order",
                table: "board");

            migrationBuilder.RenameColumn(
                name: "author_id",
                table: "post",
                newName: "post_user_id");

            migrationBuilder.RenameColumn(
                name: "author_id",
                table: "comment",
                newName: "comment_user_id");

            migrationBuilder.AlterColumn<string>(
                name: "post_content",
                table: "post",
                type: "jsonb",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(object),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}

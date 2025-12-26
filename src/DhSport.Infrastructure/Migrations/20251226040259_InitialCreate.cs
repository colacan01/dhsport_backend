using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DhSport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "board_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    board_type_nm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    board_type_desc = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "feature_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    feature_type_nm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    feature_type_desc = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feature_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "media_lib",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_file_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    media_file_nm = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    media_file_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    media_file_size = table.Column<float>(type: "real", nullable: false),
                    alt_text = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_lib", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "menu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_menu_id = table.Column<Guid>(type: "uuid", nullable: true),
                    menu_nm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    menu_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    menu_icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menu", x => x.id);
                    table.ForeignKey(
                        name: "fk_menu_menu_parent_menu_id",
                        column: x => x.parent_menu_id,
                        principalTable: "menu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notification_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    notification_content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "post_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_type_nm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    post_type_desc = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reservation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reservation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reservation_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reservation_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_nm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    customer_tel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    customer_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    reservation_note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    admin_note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reservation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_nm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role_desc = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "site_config",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    config_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    config_value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    config_desc = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_site_config", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    logon_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    passwd = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_nm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    profile_img = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "board",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    board_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    board_nm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    board_desc = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    board_slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    posts_per_page = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    allow_comment = table.Column<bool>(type: "boolean", nullable: false),
                    allow_anonymous = table.Column<bool>(type: "boolean", nullable: false),
                    require_approval = table.Column<bool>(type: "boolean", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board", x => x.id);
                    table.ForeignKey(
                        name: "fk_board_board_types_board_type_id",
                        column: x => x.board_type_id,
                        principalTable: "board_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_role_map",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_map", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_role_map_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_map_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    board_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_content = table.Column<string>(type: "jsonb", nullable: false),
                    post_slug = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    meta_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    meta_desc = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    meta_keywords = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_notice = table.Column<bool>(type: "boolean", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    publish_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_board_board_id",
                        column: x => x.board_id,
                        principalTable: "board",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_post_post_types_post_type_id",
                        column: x => x.post_type_id,
                        principalTable: "post_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "add_feature",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feature_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feature_content = table.Column<string>(type: "jsonb", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_add_feature", x => x.id);
                    table.ForeignKey(
                        name: "fk_add_feature_feature_types_feature_type_id",
                        column: x => x.feature_type_id,
                        principalTable: "feature_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_add_feature_post_post_id",
                        column: x => x.post_id,
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_comment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment_content = table.Column<string>(type: "jsonb", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comment", x => x.id);
                    table.ForeignKey(
                        name: "fk_comment_comment_parent_comment_id",
                        column: x => x.parent_comment_id,
                        principalTable: "comment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comment_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "like_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    like_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_like_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_like_log_post_post_id",
                        column: x => x.post_id,
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_file",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_file_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    post_file_nm = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    post_file_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    post_file_size = table.Column<float>(type: "real", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_file", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_file_post_post_id",
                        column: x => x.post_id,
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_revision",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_content = table.Column<string>(type: "jsonb", nullable: false),
                    revision_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    revision_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_revision", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_revision_post_post_id",
                        column: x => x.post_id,
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "read_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    read_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_read_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_read_log_post_post_id",
                        column: x => x.post_id,
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "related_post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    related_post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    create_dttm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_related_post", x => x.id);
                    table.ForeignKey(
                        name: "fk_related_post_post_post_id",
                        column: x => x.post_id,
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_related_post_post_related_post_id",
                        column: x => x.related_post_id,
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_add_feature_feature_type_id",
                table: "add_feature",
                column: "feature_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_add_feature_post_id",
                table: "add_feature",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_board_board_slug",
                table: "board",
                column: "board_slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_board_board_type_id",
                table: "board",
                column: "board_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_board_type_board_type_nm",
                table: "board_type",
                column: "board_type_nm",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_comment_parent_comment_id",
                table: "comment",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_comment_post_id",
                table: "comment",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_feature_type_feature_type_nm",
                table: "feature_type",
                column: "feature_type_nm",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_like_log_post_id_like_user_id",
                table: "like_log",
                columns: new[] { "post_id", "like_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_media_lib_create_dttm",
                table: "media_lib",
                column: "create_dttm");

            migrationBuilder.CreateIndex(
                name: "ix_media_lib_media_file_nm",
                table: "media_lib",
                column: "media_file_nm");

            migrationBuilder.CreateIndex(
                name: "ix_menu_display_order",
                table: "menu",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_menu_parent_menu_id",
                table: "menu",
                column: "parent_menu_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_create_dttm",
                table: "notification",
                column: "create_dttm");

            migrationBuilder.CreateIndex(
                name: "ix_notification_user_id",
                table: "notification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_user_id_is_read",
                table: "notification",
                columns: new[] { "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "ix_post_board_id",
                table: "post",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_is_published",
                table: "post",
                column: "is_published");

            migrationBuilder.CreateIndex(
                name: "ix_post_post_slug",
                table: "post",
                column: "post_slug");

            migrationBuilder.CreateIndex(
                name: "ix_post_post_type_id",
                table: "post",
                column: "post_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_publish_dttm",
                table: "post",
                column: "publish_dttm");

            migrationBuilder.CreateIndex(
                name: "ix_post_file_post_id",
                table: "post_file",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_revision_create_dttm",
                table: "post_revision",
                column: "create_dttm");

            migrationBuilder.CreateIndex(
                name: "ix_post_revision_post_id",
                table: "post_revision",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_type_post_type_nm",
                table: "post_type",
                column: "post_type_nm",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_read_log_create_dttm",
                table: "read_log",
                column: "create_dttm");

            migrationBuilder.CreateIndex(
                name: "ix_read_log_post_id",
                table: "read_log",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_related_post_post_id_related_post_id",
                table: "related_post",
                columns: new[] { "post_id", "related_post_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_related_post_related_post_id",
                table: "related_post",
                column: "related_post_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservation_reservation_dttm",
                table: "reservation",
                column: "reservation_dttm");

            migrationBuilder.CreateIndex(
                name: "ix_reservation_reservation_status",
                table: "reservation",
                column: "reservation_status");

            migrationBuilder.CreateIndex(
                name: "ix_reservation_user_id",
                table: "reservation",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_role_nm",
                table: "role",
                column: "role_nm",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_site_config_config_key",
                table: "site_config",
                column: "config_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_email",
                table: "user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_logon_id",
                table: "user",
                column: "logon_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_role_map_role_id",
                table: "user_role_map",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_map_user_id_role_id",
                table: "user_role_map",
                columns: new[] { "user_id", "role_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "add_feature");

            migrationBuilder.DropTable(
                name: "comment");

            migrationBuilder.DropTable(
                name: "like_log");

            migrationBuilder.DropTable(
                name: "media_lib");

            migrationBuilder.DropTable(
                name: "menu");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "post_file");

            migrationBuilder.DropTable(
                name: "post_revision");

            migrationBuilder.DropTable(
                name: "read_log");

            migrationBuilder.DropTable(
                name: "related_post");

            migrationBuilder.DropTable(
                name: "reservation");

            migrationBuilder.DropTable(
                name: "site_config");

            migrationBuilder.DropTable(
                name: "user_role_map");

            migrationBuilder.DropTable(
                name: "feature_type");

            migrationBuilder.DropTable(
                name: "post");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "board");

            migrationBuilder.DropTable(
                name: "post_type");

            migrationBuilder.DropTable(
                name: "board_type");
        }
    }
}

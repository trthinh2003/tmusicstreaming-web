using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TMusicStreaming.Migrations
{
    /// <inheritdoc />
    public partial class AddSomeSchemasForRecommendationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InteractionType",
                table: "UserInteractions");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "UserInteractions",
                newName: "PlayCount");

            migrationBuilder.AddColumn<double>(
                name: "InteractionScore",
                table: "UserInteractions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAddedToPlaylist",
                table: "UserInteractions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDownloaded",
                table: "UserInteractions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLiked",
                table: "UserInteractions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastInteractedAt",
                table: "UserInteractions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "AlbumId",
                table: "SongsDTO",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SongsDTO",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SongsDTO",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReleaseDate",
                table: "Songs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "UserSimilarities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId1 = table.Column<int>(type: "integer", nullable: false),
                    User1Id = table.Column<int>(type: "integer", nullable: false),
                    UserId2 = table.Column<int>(type: "integer", nullable: false),
                    User2Id = table.Column<int>(type: "integer", nullable: false),
                    SimilarityScore = table.Column<double>(type: "double precision", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSimilarities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSimilarities_Users_User1Id",
                        column: x => x.User1Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSimilarities_Users_User2Id",
                        column: x => x.User2Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserInteractions_UserId_SongId",
                table: "UserInteractions",
                columns: new[] { "UserId", "SongId" });

            migrationBuilder.CreateIndex(
                name: "IX_Songs_ReleaseDate",
                table: "Songs",
                column: "ReleaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserSimilarities_User1Id",
                table: "UserSimilarities",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSimilarities_User2Id",
                table: "UserSimilarities",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSimilarities_UserId1",
                table: "UserSimilarities",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserSimilarities_UserId1_UserId2",
                table: "UserSimilarities",
                columns: new[] { "UserId1", "UserId2" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSimilarities_UserId2",
                table: "UserSimilarities",
                column: "UserId2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSimilarities");

            migrationBuilder.DropIndex(
                name: "IX_UserInteractions_UserId_SongId",
                table: "UserInteractions");

            migrationBuilder.DropIndex(
                name: "IX_Songs_ReleaseDate",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "InteractionScore",
                table: "UserInteractions");

            migrationBuilder.DropColumn(
                name: "IsAddedToPlaylist",
                table: "UserInteractions");

            migrationBuilder.DropColumn(
                name: "IsDownloaded",
                table: "UserInteractions");

            migrationBuilder.DropColumn(
                name: "IsLiked",
                table: "UserInteractions");

            migrationBuilder.DropColumn(
                name: "LastInteractedAt",
                table: "UserInteractions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SongsDTO");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SongsDTO");

            migrationBuilder.RenameColumn(
                name: "PlayCount",
                table: "UserInteractions",
                newName: "Score");

            migrationBuilder.AddColumn<string>(
                name: "InteractionType",
                table: "UserInteractions",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AlbumId",
                table: "SongsDTO",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReleaseDate",
                table: "Songs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}

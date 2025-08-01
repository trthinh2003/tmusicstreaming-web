using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TMusicStreaming.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDisplayForSomeSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlaylistId",
                table: "SongsDTO",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isDisplay",
                table: "Songs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isDisplay",
                table: "Playlists",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isDisplay",
                table: "Albums",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "SongsDTO");

            migrationBuilder.DropColumn(
                name: "isDisplay",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "isDisplay",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "isDisplay",
                table: "Albums");
        }
    }
}

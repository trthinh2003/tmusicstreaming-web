using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TMusicStreaming.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionAndImageToPlaylistSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Playlists",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Playlists",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Playlists");
        }
    }
}

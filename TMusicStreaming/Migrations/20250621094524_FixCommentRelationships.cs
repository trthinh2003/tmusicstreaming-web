using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TMusicStreaming.Migrations
{
    /// <inheritdoc />
    public partial class FixCommentRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isDisplay",
                table: "SongsDTO",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isFavorite",
                table: "SongsDTO",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isLossless",
                table: "SongsDTO",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isPopular",
                table: "SongsDTO",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isDisplay",
                table: "SongsDTO");

            migrationBuilder.DropColumn(
                name: "isFavorite",
                table: "SongsDTO");

            migrationBuilder.DropColumn(
                name: "isLossless",
                table: "SongsDTO");

            migrationBuilder.DropColumn(
                name: "isPopular",
                table: "SongsDTO");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TMusicStreaming.Migrations
{
    /// <inheritdoc />
    public partial class FixUserSimilarity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSimilarities_Users_User1Id",
                table: "UserSimilarities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSimilarities_Users_User2Id",
                table: "UserSimilarities");

            migrationBuilder.DropIndex(
                name: "IX_UserSimilarities_User1Id",
                table: "UserSimilarities");

            migrationBuilder.DropIndex(
                name: "IX_UserSimilarities_User2Id",
                table: "UserSimilarities");

            migrationBuilder.DropColumn(
                name: "User1Id",
                table: "UserSimilarities");

            migrationBuilder.DropColumn(
                name: "User2Id",
                table: "UserSimilarities");

            migrationBuilder.AddColumn<int>(
                name: "playCount",
                table: "SongsDTO",
                type: "integer",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSimilarities_Users_UserId1",
                table: "UserSimilarities",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSimilarities_Users_UserId2",
                table: "UserSimilarities",
                column: "UserId2",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSimilarities_Users_UserId1",
                table: "UserSimilarities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSimilarities_Users_UserId2",
                table: "UserSimilarities");

            migrationBuilder.DropColumn(
                name: "playCount",
                table: "SongsDTO");

            migrationBuilder.AddColumn<int>(
                name: "User1Id",
                table: "UserSimilarities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "User2Id",
                table: "UserSimilarities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserSimilarities_User1Id",
                table: "UserSimilarities",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSimilarities_User2Id",
                table: "UserSimilarities",
                column: "User2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSimilarities_Users_User1Id",
                table: "UserSimilarities",
                column: "User1Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSimilarities_Users_User2Id",
                table: "UserSimilarities",
                column: "User2Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

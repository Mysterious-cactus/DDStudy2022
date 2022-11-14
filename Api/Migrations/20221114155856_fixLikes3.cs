using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class fixLikes3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddPrimaryKey(
                name: "PK_LikesPosts",
                table: "LikesPosts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LikesComments",
                table: "LikesComments",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LikesPosts",
                table: "LikesPosts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LikesComments",
                table: "LikesComments");
        }
    }
}

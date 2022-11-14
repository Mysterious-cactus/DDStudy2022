using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class fixLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LikesPosts",
                table: "LikesPosts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LikesComments",
                table: "LikesComments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "LikesPosts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "LikesComments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "LikesPosts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "LikesComments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_LikesPosts",
                table: "LikesPosts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LikesComments",
                table: "LikesComments",
                column: "Id");
        }
    }
}

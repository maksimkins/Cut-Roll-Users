using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cut_Roll_Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMovieModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS \"pgcrypto\";");
            migrationBuilder.AddColumn<DateTime>(
                name: "EmbeddingUpdatedAt",
                table: "movies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmbeddingVersion",
                table: "movies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasEmbedding",
                table: "movies",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP EXTENSION IF EXISTS \"pgcrypto\";");
            migrationBuilder.DropColumn(
                name: "EmbeddingUpdatedAt",
                table: "movies");

            migrationBuilder.DropColumn(
                name: "EmbeddingVersion",
                table: "movies");

            migrationBuilder.DropColumn(
                name: "HasEmbedding",
                table: "movies");
        }
    }
}

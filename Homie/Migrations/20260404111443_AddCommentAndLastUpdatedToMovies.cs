using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homie.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentAndLastUpdatedToMovies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "MoviesEF",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeasonEpisodeUpdated",
                table: "MoviesEF",
                type: "datetime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "MoviesEF");

            migrationBuilder.DropColumn(
                name: "LastSeasonEpisodeUpdated",
                table: "MoviesEF");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_cinema.Migrations
{
    /// <inheritdoc />
    public partial class AddFeaturedMovieFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackdropUrl",
                table: "Movies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Movies",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackdropUrl",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Movies");
        }
    }
}

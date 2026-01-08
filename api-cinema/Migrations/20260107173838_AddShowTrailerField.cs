using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_cinema.Migrations
{
    /// <inheritdoc />
    public partial class AddShowTrailerField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowTrailer",
                table: "Movies",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowTrailer",
                table: "Movies");
        }
    }
}

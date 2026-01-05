using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace api_cinema.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomsAndScreeningSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Capacity",
                table: "Theaters",
                newName: "RoomCount");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Theaters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Screenings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScreeningScheduleId",
                table: "Screenings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TheaterId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RoomNumber = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_Theaters_TheaterId",
                        column: x => x.TheaterId,
                        principalTable: "Theaters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MovieId = table.Column<int>(type: "integer", nullable: false),
                    TheaterId = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShowTimes = table.Column<string>(type: "text", nullable: false),
                    DaysOfWeek = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningSchedules_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScreeningSchedules_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScreeningSchedules_Theaters_TheaterId",
                        column: x => x.TheaterId,
                        principalTable: "Theaters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Screenings_RoomId",
                table: "Screenings",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Screenings_ScreeningScheduleId",
                table: "Screenings",
                column: "ScreeningScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_TheaterId",
                table: "Rooms",
                column: "TheaterId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningSchedules_MovieId",
                table: "ScreeningSchedules",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningSchedules_RoomId",
                table: "ScreeningSchedules",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningSchedules_TheaterId",
                table: "ScreeningSchedules",
                column: "TheaterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Screenings_Rooms_RoomId",
                table: "Screenings",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Screenings_ScreeningSchedules_ScreeningScheduleId",
                table: "Screenings",
                column: "ScreeningScheduleId",
                principalTable: "ScreeningSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Screenings_Rooms_RoomId",
                table: "Screenings");

            migrationBuilder.DropForeignKey(
                name: "FK_Screenings_ScreeningSchedules_ScreeningScheduleId",
                table: "Screenings");

            migrationBuilder.DropTable(
                name: "ScreeningSchedules");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Screenings_RoomId",
                table: "Screenings");

            migrationBuilder.DropIndex(
                name: "IX_Screenings_ScreeningScheduleId",
                table: "Screenings");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Theaters");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Screenings");

            migrationBuilder.DropColumn(
                name: "ScreeningScheduleId",
                table: "Screenings");

            migrationBuilder.RenameColumn(
                name: "RoomCount",
                table: "Theaters",
                newName: "Capacity");
        }
    }
}

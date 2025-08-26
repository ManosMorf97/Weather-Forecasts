using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherForecastApplication.Web.Migrations
{
    /// <inheritdoc />
    public partial class DbMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    City_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityName = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.City_Id);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Site_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Site_Name = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Site_Id);
                });

            migrationBuilder.CreateTable(
                name: "Timeslots",
                columns: table => new
                {
                    Timeslot_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timeslots", x => x.Timeslot_Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HashedPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "CitySites",
                columns: table => new
                {
                    City_Id = table.Column<int>(type: "int", nullable: false),
                    Site_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitySites", x => new { x.City_Id, x.Site_Id });
                    table.ForeignKey(
                        name: "FK_CitySites_Cities_City_Id",
                        column: x => x.City_Id,
                        principalTable: "Cities",
                        principalColumn: "City_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CitySites_Sites_Site_Id",
                        column: x => x.Site_Id,
                        principalTable: "Sites",
                        principalColumn: "Site_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Predictions",
                columns: table => new
                {
                    City_Id = table.Column<int>(type: "int", nullable: false),
                    Site_Id = table.Column<int>(type: "int", nullable: false),
                    Timeslot_Id = table.Column<int>(type: "int", nullable: false),
                    Weather = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Danger = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predictions", x => new { x.City_Id, x.Site_Id, x.Timeslot_Id });
                    table.ForeignKey(
                        name: "FK_Predictions_CitySites_City_Id_Site_Id",
                        columns: x => new { x.City_Id, x.Site_Id },
                        principalTable: "CitySites",
                        principalColumns: new[] { "City_Id", "Site_Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Predictions_Timeslots_Timeslot_Id",
                        column: x => x.Timeslot_Id,
                        principalTable: "Timeslots",
                        principalColumn: "Timeslot_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSiteCities",
                columns: table => new
                {
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    City_Id = table.Column<int>(type: "int", nullable: false),
                    Site_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSiteCities", x => new { x.Email, x.City_Id, x.Site_Id });
                    table.ForeignKey(
                        name: "FK_UserSiteCities_CitySites_City_Id_Site_Id",
                        columns: x => new { x.City_Id, x.Site_Id },
                        principalTable: "CitySites",
                        principalColumns: new[] { "City_Id", "Site_Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSiteCities_Users_Email",
                        column: x => x.Email,
                        principalTable: "Users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Site_Id = table.Column<int>(type: "int", nullable: false),
                    City_Id = table.Column<int>(type: "int", nullable: false),
                    Timeslot_Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => new { x.Site_Id, x.City_Id, x.Timeslot_Id });
                    table.ForeignKey(
                        name: "FK_Notifications_Predictions_City_Id_Site_Id_Timeslot_Id",
                        columns: x => new { x.City_Id, x.Site_Id, x.Timeslot_Id },
                        principalTable: "Predictions",
                        principalColumns: new[] { "City_Id", "Site_Id", "Timeslot_Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    City_Id = table.Column<int>(type: "int", nullable: false),
                    Site_Id = table.Column<int>(type: "int", nullable: false),
                    Timeslot_Id = table.Column<int>(type: "int", nullable: false),
                    Rating_Value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => new { x.Email, x.City_Id, x.Site_Id, x.Timeslot_Id });
                    table.ForeignKey(
                        name: "FK_Ratings_Predictions_City_Id_Site_Id_Timeslot_Id",
                        columns: x => new { x.City_Id, x.Site_Id, x.Timeslot_Id },
                        principalTable: "Predictions",
                        principalColumns: new[] { "City_Id", "Site_Id", "Timeslot_Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ratings_Users_Email",
                        column: x => x.Email,
                        principalTable: "Users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CityName",
                table: "Cities",
                column: "CityName",
                unique: true,
                filter: "[CityName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CitySites_Site_Id",
                table: "CitySites",
                column: "Site_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_City_Id_Site_Id_Timeslot_Id",
                table: "Notifications",
                columns: new[] { "City_Id", "Site_Id", "Timeslot_Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_Timeslot_Id",
                table: "Predictions",
                column: "Timeslot_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_City_Id_Site_Id_Timeslot_Id",
                table: "Ratings",
                columns: new[] { "City_Id", "Site_Id", "Timeslot_Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Site_Name",
                table: "Sites",
                column: "Site_Name",
                unique: true,
                filter: "[Site_Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSiteCities_City_Id_Site_Id",
                table: "UserSiteCities",
                columns: new[] { "City_Id", "Site_Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "UserSiteCities");

            migrationBuilder.DropTable(
                name: "Predictions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "CitySites");

            migrationBuilder.DropTable(
                name: "Timeslots");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Sites");
        }
    }
}

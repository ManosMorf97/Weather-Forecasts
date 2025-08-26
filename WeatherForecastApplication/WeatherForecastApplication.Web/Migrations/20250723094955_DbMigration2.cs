using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherForecastApplication.Web.Migrations
{
    /// <inheritdoc />
    public partial class DbMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sites_Site_Name",
                table: "Sites");

            migrationBuilder.DropIndex(
                name: "IX_Cities_CityName",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "HashedPassword",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CityName",
                table: "Cities");

            migrationBuilder.RenameColumn(
                name: "Site_Name",
                table: "Sites",
                newName: "Site_name");

            migrationBuilder.AddColumn<string>(
                name: "Hashed_password",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Site_name",
                table: "Sites",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateNotification",
                table: "Notifications",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "TimeNotification",
                table: "Notifications",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "City_name",
                table: "Cities",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    City_Id = table.Column<int>(type: "int", nullable: false),
                    Site_Id = table.Column<int>(type: "int", nullable: false),
                    Timeslot_Id = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifications", x => new { x.Email, x.City_Id, x.Site_Id, x.Timeslot_Id });
                    table.ForeignKey(
                        name: "FK_UserNotifications_Notifications_Site_Id_City_Id_Timeslot_Id",
                        columns: x => new { x.Site_Id, x.City_Id, x.Timeslot_Id },
                        principalTable: "Notifications",
                        principalColumns: new[] { "Site_Id", "City_Id", "Timeslot_Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserNotifications_Users_Email",
                        column: x => x.Email,
                        principalTable: "Users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Timeslots_Date_Time",
                table: "Timeslots",
                columns: new[] { "Date", "Time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Site_name",
                table: "Sites",
                column: "Site_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_City_name",
                table: "Cities",
                column: "City_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_Site_Id_City_Id_Timeslot_Id",
                table: "UserNotifications",
                columns: new[] { "Site_Id", "City_Id", "Timeslot_Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Timeslots_Date_Time",
                table: "Timeslots");

            migrationBuilder.DropIndex(
                name: "IX_Sites_Site_name",
                table: "Sites");

            migrationBuilder.DropIndex(
                name: "IX_Cities_City_name",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "Hashed_password",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateNotification",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TimeNotification",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "City_name",
                table: "Cities");

            migrationBuilder.RenameColumn(
                name: "Site_name",
                table: "Sites",
                newName: "Site_Name");

            migrationBuilder.AddColumn<string>(
                name: "HashedPassword",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Site_Name",
                table: "Sites",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "CityName",
                table: "Cities",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Site_Name",
                table: "Sites",
                column: "Site_Name",
                unique: true,
                filter: "[Site_Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CityName",
                table: "Cities",
                column: "CityName",
                unique: true,
                filter: "[CityName] IS NOT NULL");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventParkingReservationSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpPurposeForPasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "LoginOtps",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Login");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "LoginOtps");
        }
    }
}

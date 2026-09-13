using EventParkingReservationSystem.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventParkingReservationSystem.API.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260913183000_AddExternalEventVenues")]
public partial class AddExternalEventVenues : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "VenueId",
            table: "Events",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AddColumn<string>(
            name: "VenueMode",
            table: "Events",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            defaultValue: "OurProperty");

        migrationBuilder.AddColumn<string>(
            name: "ExternalVenueName",
            table: "Events",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ExternalVenueAddress",
            table: "Events",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "EventFavorites",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CustomerId = table.Column<int>(type: "int", nullable: false),
                EventId = table.Column<int>(type: "int", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventFavorites", x => x.Id);
                table.ForeignKey(
                    name: "FK_EventFavorites_Customers_CustomerId",
                    column: x => x.CustomerId,
                    principalTable: "Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EventFavorites_Events_EventId",
                    column: x => x.EventId,
                    principalTable: "Events",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_EventFavorites_CustomerId_EventId",
            table: "EventFavorites",
            columns: new[] { "CustomerId", "EventId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_EventFavorites_EventId",
            table: "EventFavorites",
            column: "EventId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "EventFavorites");

        migrationBuilder.DropColumn(name: "VenueMode", table: "Events");
        migrationBuilder.DropColumn(name: "ExternalVenueName", table: "Events");
        migrationBuilder.DropColumn(name: "ExternalVenueAddress", table: "Events");

        migrationBuilder.AlterColumn<int>(
            name: "VenueId",
            table: "Events",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);
    }
}

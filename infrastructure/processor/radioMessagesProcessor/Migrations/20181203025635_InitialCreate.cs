using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace radioMessagesProcessor.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RadioLocationMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Imei = table.Column<string>(nullable: true),
                    CollectionDateUTC = table.Column<DateTime>(nullable: false),
                    DeviceDate = table.Column<DateTime>(nullable: false),
                    RawEvent = table.Column<byte[]>(nullable: true),
                    DecodedEvent = table.Column<byte[]>(nullable: true),
                    RadioShapes = table.Column<byte[]>(nullable: true),
                    DecodedDateUTC = table.Column<DateTime>(nullable: false),
                    GpsLatitude = table.Column<double>(nullable: false),
                    GpsLongitude = table.Column<double>(nullable: false),
                    GpsAge = table.Column<long>(nullable: false),
                    GpsAccuracy = table.Column<double>(nullable: false),
                    GpsSpeed = table.Column<double>(nullable: false),
                    GpsBearing = table.Column<double>(nullable: true),
                    Rssi = table.Column<int>(nullable: false),
                    DecodedLatitude = table.Column<double>(nullable: false),
                    DecodedLongitude = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadioLocationMessages", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RadioLocationMessages");
        }
    }
}

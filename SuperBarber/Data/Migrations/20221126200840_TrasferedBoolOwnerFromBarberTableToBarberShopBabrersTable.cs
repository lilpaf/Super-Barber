using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperBarber.Data.Migrations
{
    public partial class TrasferedBoolOwnerFromBarberTableToBarberShopBabrersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Barbers");

            migrationBuilder.AddColumn<bool>(
                name: "IsOwner",
                table: "BarberShopBarbers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOwner",
                table: "BarberShopBarbers");

            migrationBuilder.AddColumn<bool>(
                name: "Owner",
                table: "Barbers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

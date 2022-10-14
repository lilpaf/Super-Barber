using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperBarber.Data.Migrations
{
    public partial class AddedWorkinHoursInTableBarberShop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "FinishHour",
                table: "BarberShops",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartHour",
                table: "BarberShops",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishHour",
                table: "BarberShops");

            migrationBuilder.DropColumn(
                name: "StartHour",
                table: "BarberShops");
        }
    }
}

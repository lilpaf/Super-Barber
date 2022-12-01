using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperBarber.Data.Migrations
{
    public partial class AddedIsPublicBoolInBarberShopTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "BarberShops",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "BarberShops");
        }
    }
}

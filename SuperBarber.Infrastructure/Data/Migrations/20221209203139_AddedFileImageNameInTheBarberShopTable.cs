using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperBarber.Infrastructure.Data.Migrations
{
    public partial class AddedFileImageNameInTheBarberShopTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "BarberShops");

            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "BarberShops",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "BarberShops");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "BarberShops",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

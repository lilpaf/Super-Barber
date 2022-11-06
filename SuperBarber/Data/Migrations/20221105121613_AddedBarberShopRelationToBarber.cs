using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperBarber.Data.Migrations
{
    public partial class AddedBarberShopRelationToBarber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Barbers_BarberShops_BarberShopId",
                table: "Barbers");

            migrationBuilder.AddForeignKey(
                name: "FK_Barbers_BarberShops_BarberShopId",
                table: "Barbers",
                column: "BarberShopId",
                principalTable: "BarberShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Barbers_BarberShops_BarberShopId",
                table: "Barbers");

            migrationBuilder.AddForeignKey(
                name: "FK_Barbers_BarberShops_BarberShopId",
                table: "Barbers",
                column: "BarberShopId",
                principalTable: "BarberShops",
                principalColumn: "Id");
        }
    }
}

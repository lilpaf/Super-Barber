using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperBarber.Infrastructure.Data.Migrations
{
    public partial class MadeBarberShopNullInBarberTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Barbers_BarberShops_BarberShopId",
                table: "Barbers");

            migrationBuilder.AlterColumn<int>(
                name: "BarberShopId",
                table: "Barbers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Barbers_BarberShops_BarberShopId",
                table: "Barbers",
                column: "BarberShopId",
                principalTable: "BarberShops",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Barbers_BarberShops_BarberShopId",
                table: "Barbers");

            migrationBuilder.AlterColumn<int>(
                name: "BarberShopId",
                table: "Barbers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Barbers_BarberShops_BarberShopId",
                table: "Barbers",
                column: "BarberShopId",
                principalTable: "BarberShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

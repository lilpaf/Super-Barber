using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperBarber.Infrastructure.Data.Migrations
{
    public partial class AddedBarberShopBarbersMappingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Barbers_BarberShops_BarberShopId",
                table: "Barbers");

            migrationBuilder.DropIndex(
                name: "IX_Barbers_BarberShopId",
                table: "Barbers");

            migrationBuilder.DropColumn(
                name: "BarberShopId",
                table: "Barbers");

            migrationBuilder.CreateTable(
                name: "BarberShopBarbers",
                columns: table => new
                {
                    BarberShopId = table.Column<int>(type: "int", nullable: false),
                    BarberId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarberShopBarbers", x => new { x.BarberId, x.BarberShopId });
                    table.ForeignKey(
                        name: "FK_BarberShopBarbers_Barbers_BarberId",
                        column: x => x.BarberId,
                        principalTable: "Barbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BarberShopBarbers_BarberShops_BarberShopId",
                        column: x => x.BarberShopId,
                        principalTable: "BarberShops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BarberShopBarbers_BarberShopId",
                table: "BarberShopBarbers",
                column: "BarberShopId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BarberShopBarbers");

            migrationBuilder.AddColumn<int>(
                name: "BarberShopId",
                table: "Barbers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Barbers_BarberShopId",
                table: "Barbers",
                column: "BarberShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Barbers_BarberShops_BarberShopId",
                table: "Barbers",
                column: "BarberShopId",
                principalTable: "BarberShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

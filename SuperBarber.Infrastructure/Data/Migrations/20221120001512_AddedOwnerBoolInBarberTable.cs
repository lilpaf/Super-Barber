using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperBarber.Infrastructure.Data.Migrations
{
    public partial class AddedOwnerBoolInBarberTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Owner",
                table: "Barbers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Barbers");
        }
    }
}

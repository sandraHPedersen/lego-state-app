using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedYellowGreen.Api.Persistence.Migrations
{
    public partial class AddProductionOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderNumber = table.Column<string>(type: "TEXT", nullable: false),
                    EquipmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    ScheduledStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScheduledEnd = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrders", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionOrders");
        }
    }
}

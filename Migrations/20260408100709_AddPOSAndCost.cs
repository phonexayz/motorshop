using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotorcycleRepairShop.Migrations
{
    /// <inheritdoc />
    public partial class AddPOSAndCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPOS",
                table: "RepairOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "RepairOrders",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "OrderDetails",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPOS",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "OrderDetails");
        }
    }
}

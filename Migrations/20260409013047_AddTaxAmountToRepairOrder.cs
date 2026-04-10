using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotorcycleRepairShop.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxAmountToRepairOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "RepairOrders",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "RepairOrders");
        }
    }
}

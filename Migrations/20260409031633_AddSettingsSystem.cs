using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MotorcycleRepairShop.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Group = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "Description", "DisplayName", "Group", "Key", "Value" },
                values: new object[,]
                {
                    { 1, null, "ຊື່ຮ້ານ", "Shop Info", "ShopName", "ຮ້ານສ້ອມແປງລົດຈັກ (MotorShop)" },
                    { 2, null, "ທີ່ຢູ່ຮ້ານ", "Shop Info", "ShopAddress", "123 ຖ.ສຸຂຸມວິທ ເມືອງວຽງຈັນ" },
                    { 3, null, "ເບີໂທລະສັບຮ້ານ", "Shop Info", "ShopPhone", "081-234-5678" },
                    { 4, null, "ອັດຕາພາສີ (%)", "Financial", "TaxRatePercentage", "10" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_Key",
                table: "AppSettings",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");
        }
    }
}

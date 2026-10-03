using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoHM.Api.Data.MigrationsBackup
{
    /// <inheritdoc />
    public partial class AddAccessory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessoryId",
                table: "OrderItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_AccessoryId",
                table: "OrderItems",
                column: "AccessoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Accessories_AccessoryId",
                table: "OrderItems",
                column: "AccessoryId",
                principalTable: "Accessories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Accessories_AccessoryId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_AccessoryId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "AccessoryId",
                table: "OrderItems");
        }
    }
}

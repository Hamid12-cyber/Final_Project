using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoHM.Api.Data.Migrations
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
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccessoryId",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_AccessoryId",
                table: "OrderItems",
                column: "AccessoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_AccessoryId",
                table: "CartItems",
                column: "AccessoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Accessories_AccessoryId",
                table: "CartItems",
                column: "AccessoryId",
                principalTable: "Accessories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Accessories_AccessoryId",
                table: "OrderItems",
                column: "AccessoryId",
                principalTable: "Accessories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Accessories_AccessoryId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Accessories_AccessoryId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_AccessoryId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_AccessoryId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "AccessoryId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "AccessoryId",
                table: "CartItems");
        }
    }
}

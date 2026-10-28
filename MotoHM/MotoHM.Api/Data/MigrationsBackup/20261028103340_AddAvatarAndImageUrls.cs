using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoHM.Api.Data.MigrationsBackup
{
    /// <inheritdoc />
    public partial class AddAvatarAndImageUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrlsJson",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrlsJson",
                table: "Motorcycles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrlsJson",
                table: "Accessories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ImageUrlsJson",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ImageUrlsJson",
                table: "Motorcycles");

            migrationBuilder.DropColumn(
                name: "ImageUrlsJson",
                table: "Accessories");
        }
    }
}

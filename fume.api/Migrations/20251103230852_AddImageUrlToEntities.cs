using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fume.api.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "SubCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProductImages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "categories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "SubCategories");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "categories");
        }
    }
}

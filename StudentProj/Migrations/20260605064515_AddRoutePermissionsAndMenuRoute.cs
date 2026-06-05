using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProj.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutePermissionsAndMenuRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MenuRoute",
                table: "Menus",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "RoutePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PathPattern = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    RequiredMenuName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequiredPrivilegeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutePermissions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoutePermissions");

            migrationBuilder.DropColumn(
                name: "MenuRoute",
                table: "Menus");
        }
    }
}

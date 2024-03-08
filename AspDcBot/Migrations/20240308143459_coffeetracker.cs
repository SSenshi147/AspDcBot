using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspDcBot.Migrations
{
    /// <inheritdoc />
    public partial class coffeetracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DrinkModels",
                columns: table => new
                {
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CoffeCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TeaCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrinkModels", x => x.UserId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrinkModels");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspDcBot.Migrations
{
    /// <inheritdoc />
    public partial class TrackingUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "UserId",
                table: "TrackedMessageModels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TrackedMessageModels");
        }
    }
}

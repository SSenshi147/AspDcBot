using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspDcBot.Migrations
{
    /// <inheritdoc />
    public partial class backgroundtasks_reminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReminderJobId",
                table: "JobDataModels",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderJobId",
                table: "JobDataModels");
        }
    }
}

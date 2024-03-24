using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspDcBot.Migrations
{
    /// <inheritdoc />
    public partial class betterReminderData1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_JobDataModels",
                table: "JobDataModels");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "JobDataModels",
                newName: "ChannelId");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "JobDataModels",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobDataModels",
                table: "JobDataModels",
                columns: new[] { "JobId", "ChannelId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_JobDataModels",
                table: "JobDataModels");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "JobDataModels");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "JobDataModels",
                newName: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobDataModels",
                table: "JobDataModels",
                column: "JobId");
        }
    }
}

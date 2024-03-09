using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspDcBot.Migrations
{
    /// <inheritdoc />
    public partial class caffeinehistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DrinkModels",
                table: "DrinkModels");

            migrationBuilder.RenameColumn(
                name: "TeaCount",
                table: "DrinkModels",
                newName: "TextChannelId");

            migrationBuilder.RenameColumn(
                name: "CoffeCount",
                table: "DrinkModels",
                newName: "MessageId");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "DrinkModels",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "DrinkModels",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Caffeine",
                table: "DrinkModels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DrinkModels",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_DrinkModels",
                table: "DrinkModels",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DrinkModels",
                table: "DrinkModels");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DrinkModels");

            migrationBuilder.DropColumn(
                name: "Caffeine",
                table: "DrinkModels");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DrinkModels");

            migrationBuilder.RenameColumn(
                name: "TextChannelId",
                table: "DrinkModels",
                newName: "TeaCount");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "DrinkModels",
                newName: "CoffeCount");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "DrinkModels",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DrinkModels",
                table: "DrinkModels",
                column: "UserId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestLocalBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddScreenshotUrlToQuest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScreenshotUrl",
                table: "Quests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScreenshotUrl",
                table: "Quests");
        }
    }
}

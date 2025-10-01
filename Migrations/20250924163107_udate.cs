using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestLocalBackend.Migrations
{
    /// <inheritdoc />
    public partial class udate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoSave",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoSave",
                table: "UserSettings");
        }
    }
}

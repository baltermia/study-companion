using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyCompanion.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddStateMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MinimalTelegramBotStates",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    MessageThreadId = table.Column<long>(type: "bigint", nullable: false),
                    StateGroupName = table.Column<string>(type: "text", nullable: false),
                    StateId = table.Column<int>(type: "integer", nullable: false),
                    StateData = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinimalTelegramBotStates", x => new { x.UserId, x.ChatId, x.MessageThreadId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MinimalTelegramBotStates");
        }
    }
}

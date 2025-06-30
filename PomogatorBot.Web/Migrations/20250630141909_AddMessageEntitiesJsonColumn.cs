using Microsoft.EntityFrameworkCore.Migrations;
using Telegram.Bot.Types;

#nullable disable

namespace PomogatorBot.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageEntitiesJsonColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<MessageEntity[]>(
                name: "message_entities",
                table: "broadcast_history",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "message_entities",
                table: "broadcast_history");
        }
    }
}

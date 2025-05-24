using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PomogatorBot.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAlias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "alias",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "alias",
                table: "users");
        }
    }
}

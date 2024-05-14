using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NBABets.API.Migrations
{
    /// <inheritdoc />
    public partial class AddedNameToBets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Bets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Bets");
        }
    }
}

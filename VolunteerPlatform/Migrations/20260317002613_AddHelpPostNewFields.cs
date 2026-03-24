using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolunteerPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddHelpPostNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ContactAfterAccept",
                table: "HelpPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ContactTelegram",
                table: "HelpPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactViber",
                table: "HelpPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HelpType",
                table: "HelpPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Urgency",
                table: "HelpPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactAfterAccept",
                table: "HelpPosts");

            migrationBuilder.DropColumn(
                name: "ContactTelegram",
                table: "HelpPosts");

            migrationBuilder.DropColumn(
                name: "ContactViber",
                table: "HelpPosts");

            migrationBuilder.DropColumn(
                name: "HelpType",
                table: "HelpPosts");

            migrationBuilder.DropColumn(
                name: "Urgency",
                table: "HelpPosts");
        }
    }
}

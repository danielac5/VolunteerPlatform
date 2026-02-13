using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolunteerPlatform.Migrations
{
    /// <inheritdoc />
    public partial class ExtendVerificationCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "EmailVerificationCodes");

            migrationBuilder.AddColumn<int>(
                name: "Attempts",
                table: "EmailVerificationCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CodeHash",
                table: "EmailVerificationCodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                table: "EmailVerificationCodes",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "Purpose",
                table: "EmailVerificationCodes",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attempts",
                table: "EmailVerificationCodes");

            migrationBuilder.DropColumn(
                name: "CodeHash",
                table: "EmailVerificationCodes");

            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                table: "EmailVerificationCodes");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "EmailVerificationCodes");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "EmailVerificationCodes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}

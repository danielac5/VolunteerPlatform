using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolunteerPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddHelpPostFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HelpPostFeedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HelpPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Safety = table.Column<int>(type: "int", nullable: false),
                    Communication = table.Column<int>(type: "int", nullable: false),
                    Reliability = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpPostFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HelpPostFeedbacks_HelpPostApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "HelpPostApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HelpPostFeedbacks_HelpPosts_HelpPostId",
                        column: x => x.HelpPostId,
                        principalTable: "HelpPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HelpPostFeedbacks_Users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HelpPostFeedbacks_Users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HelpPostFeedbacks_ApplicationId",
                table: "HelpPostFeedbacks",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpPostFeedbacks_FromUserId",
                table: "HelpPostFeedbacks",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpPostFeedbacks_HelpPostId_ApplicationId_FromUserId_ToUserId",
                table: "HelpPostFeedbacks",
                columns: new[] { "HelpPostId", "ApplicationId", "FromUserId", "ToUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HelpPostFeedbacks_ToUserId",
                table: "HelpPostFeedbacks",
                column: "ToUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HelpPostFeedbacks");
        }
    }
}
